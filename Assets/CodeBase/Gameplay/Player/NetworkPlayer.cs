using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Gameplay.Camera;
using CodeBase.Gameplay.Player.Animations;
using CodeBase.Gameplay.Player.States;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.FallDamageService;
using CodeBase.Services.InputService;
using CodeBase.Services.Message;
using CodeBase.UI;
using Fusion;
using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay.Player
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private HealthUI _healthUI;
        [SerializeField] private PlayerAnimator _playerAnimator;
        [SerializeField] private CameraTargetBinder _cameraTargetBinder;

        // Синхронизируем состояние игрока по сети
        [Networked] 
        public PlayerStateType CurrentPlayerState { get; private set; } = PlayerStateType.Idle;
        
        // Локальная переменная для отслеживания изменений
        private PlayerStateType _previousPlayerState = PlayerStateType.Idle;

        private INetworkInputService _inputService;
        private IMessageService _messageService;
        private IAssetProvider _assetProvider;
        private FallDamageService _fallDamageService;
        private PlayerStateMachine _stateMachine;
        private PlayerHealth _health;
        private PlayerSettings _playerSettings;

        // Статические ссылки для автоинициализации на клиентах
        private static INetworkInputService _staticInputService;
        private static IMessageService _staticMessageService;
        private static IAssetProvider _staticAssetProvider;
        private static bool _staticServicesInitialized = false;

        public bool IsInitialized { get; private set; }

        // Статический метод для инициализации сервисов
        public static void InitializeStaticServices(
            INetworkInputService inputService,
            IMessageService messageService,
            IAssetProvider assetProvider)
        {
            _staticInputService = inputService;
            _staticMessageService = messageService;
            _staticAssetProvider = assetProvider;
            _staticServicesInitialized = true;
            Debug.Log("[NetworkPlayer] Static services initialized");
        }

        public void Initialize(
            INetworkInputService inputService,
            IMessageService messageService,
            IAssetProvider assetProvider)
        {
            if (IsInitialized)
            {
                Debug.Log($"[NetworkPlayer] Already initialized, skipping manual initialization. Object: {Object.Id}");
                return;
            }

            Debug.Log($"[NetworkPlayer] Manual Initialize called. HasStateAuthority: {HasStateAuthority}, HasInputAuthority: {Object.HasInputAuthority}, Object: {Object.Id}");

            _inputService = inputService;
            _messageService = messageService;
            _assetProvider = assetProvider;

            InitializeAsync();
        }

        private async void AutoInitialize()
        {
            Debug.Log($"[NetworkPlayer] AutoInitialize called. Static services ready: {_staticServicesInitialized}");

            if (!_staticServicesInitialized)
            {
                Debug.LogError("[NetworkPlayer] Static services not initialized! Cannot auto-initialize player.");
                return;
            }

            if (IsInitialized)
            {
                Debug.Log($"[NetworkPlayer] Already initialized, skipping auto-initialization. Object: {Object.Id}");
                return;
            }

            Debug.Log($"[NetworkPlayer] Auto-initializing player. HasStateAuthority: {HasStateAuthority}, HasInputAuthority: {Object.HasInputAuthority}, Object: {Object.Id}");

            _inputService = _staticInputService;
            _messageService = _staticMessageService;
            _assetProvider = _staticAssetProvider;

            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                Debug.Log($"[NetworkPlayer] Starting async initialization. Object: {Object.Id}");

                _playerSettings = await _assetProvider.Load<PlayerSettings>("PlayerSettings");
                Debug.Log($"[NetworkPlayer] PlayerSettings loaded. Object: {Object.Id}");

                var movement = GetComponent<PlayerMovement>();
                _health = GetComponent<PlayerHealth>();

                movement.Initialize(_playerSettings.MoveSpeed);
                _health.Initialize(_messageService, _playerSettings);

                _fallDamageService = new FallDamageService(_playerSettings.FallDamageThreshold, _messageService);

                var states = new Dictionary<PlayerStateType, States.IPlayerState>
                {
                    { PlayerStateType.Idle, new IdleState(movement) },
                    { PlayerStateType.Move, new MoveState(movement) },
                    { PlayerStateType.Dead, new DeathState() }
                };

                _stateMachine = new PlayerStateMachine(states);
                _stateMachine.Enter(PlayerStateType.Idle);

                // Подписываемся на изменения локального состояния для обновления сетевого состояния
                _stateMachine.OnStateChanged += OnLocalStateChanged;
                _messageService.Subscribe<PlayerDiedMessage>(OnPlayerDied);
                _fallDamageService.Track(Object, _playerSettings.FallDamage);

                if (Object.HasInputAuthority)
                {
                    Debug.Log($"[NetworkPlayer] Input enabled for local player. Object: {Object.Id}");
                    _inputService.Enable();

                    if (_healthUI != null)
                    {
                        _healthUI.Initialize(_playerSettings.MaxHp, _messageService);
                        _healthUI.gameObject.SetActive(true);
                    }
                }
                else
                {
                    _healthUI?.gameObject.SetActive(false);
                }

                IsInitialized = true;
                Debug.Log($"[NetworkPlayer] Player fully initialized. Object: {Object.Id}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkPlayer] Error initializing NetworkPlayer {Object.Id}: {e.Message}");
                throw;
            }
        }

        public override void Spawned()
        {
            Debug.Log($"[NetworkPlayer] Spawned. HasStateAuthority: {HasStateAuthority}, " +
                      $"HasInputAuthority: {Object.HasInputAuthority}, " +
                      $"Object: {Object.Id}, " +
                      $"Parent: {(transform.parent ? transform.parent.name : "None")}, " +
                      $"Scene: {gameObject.scene.name}");

            if (Object.HasInputAuthority)
            {
                _cameraTargetBinder.Initialize();
            }

            // Автоматическая инициализация для всех игроков на всех клиентах
            AutoInitialize();
        }

        private void Update()
        {
            if (!IsInitialized) return;

            if (_previousPlayerState != CurrentPlayerState)
            {
                OnPlayerStateChanged(_previousPlayerState, CurrentPlayerState);
                _previousPlayerState = CurrentPlayerState;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!IsInitialized) return;

            // FallDamageService должен тикать для всех авторитетных клиентов постоянно
            if ((HasStateAuthority || Object.HasInputAuthority) && _fallDamageService != null)
            {
                _fallDamageService.Tick();
            }

            // Только авторитетный клиент (хост или владелец персонажа) обновляет state machine
            if (HasStateAuthority || Object.HasInputAuthority)
            {
                if (GetInput(out NetworkInputData data))
                {
                    _stateMachine.Tick(data);
                }
                else if (Object.HasInputAuthority)
                {
                    // Для локального игрока без актуального ввода используем текущий локальный ввод
                    var localInput = _inputService.GetInput();
                    _stateMachine.Tick(localInput);
                }
                else
                {
                    // Пустой ввод для сервера без данных
                    var emptyInput = new NetworkInputData();
                    _stateMachine.Tick(emptyInput);
                }
            }
        }

        // Обработчик изменения сетевого состояния
        private void OnPlayerStateChanged(PlayerStateType oldState, PlayerStateType newState)
        {
            _playerAnimator.Play(newState);
        }

        // Метод вызывается локальной state machine при изменении состояния
        private void OnLocalStateChanged(PlayerStateType newState)
        {
            if (HasStateAuthority)
            {
                CurrentPlayerState = newState;
                Debug.Log($"[NetworkPlayer] Updated network state to {newState} for {Object.Id}");
            }
        }

        private void OnPlayerDied(PlayerDiedMessage msg)
        {
            if (msg.PlayerObject == Object)
                _stateMachine.Enter(PlayerStateType.Dead);
        }

        public void Respawn()
        {
            _health.ResetHealth();
            _stateMachine.Enter(PlayerStateType.Idle);
        }

        private void Unsubscribe()
        {
            if (_healthUI != null)
            {
                _healthUI.Clear();
            }

            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged -= OnLocalStateChanged;
            }

            if (_messageService != null)
            {
                _messageService.Unsubscribe<PlayerDiedMessage>(OnPlayerDied);
            }
        }

        private void OnDisable()
        {
            _healthUI?.Clear();
            _inputService?.Disable();
        }

        private void OnDestroy()
        {
            Debug.Log($"[NetworkPlayer] OnDestroy called. Instance: {GetHashCode()}, GameObject: {gameObject.name}");
            Unsubscribe();
        }
    }
}