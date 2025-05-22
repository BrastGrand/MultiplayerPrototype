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

        [Inject] private INetworkInputService _inputService;
        [Inject] private IMessageService _messageService;
        [Inject] private IAssetProvider _assetProvider;
        private FallDamageService _fallDamageService;
        private PlayerStateMachine _stateMachine;
        private PlayerHealth _health;
        private PlayerSettings _playerSettings;
        private DiContainer _container;

        public bool IsInitialized { get; private set; }

        public void SetDiContainer(DiContainer container)
        {
            _container = container;
        }

        public async Task Initialize()
        {
            Debug.Log($"NetworkPlayer Initialize. HasStateAuthority: {HasStateAuthority}, HasInputAuthority: {Object.HasInputAuthority}, Object: {Object.Id}");

            _playerSettings = await _assetProvider.Load<PlayerSettings>("PlayerSettings");
            Debug.Log("PlayerSettings loaded");

            var movement = GetComponent<PlayerMovement>();
            _health = GetComponent<PlayerHealth>();

            movement.Initialize(_playerSettings.MoveSpeed);
            _health.Initialize(_messageService, _playerSettings);
            Debug.Log("Movement and health initialized");

            _fallDamageService = new FallDamageService(_playerSettings.FallDamageThreshold, _messageService);

            var states = new Dictionary<PlayerStateType, States.IPlayerState>
            {
                { PlayerStateType.Idle, new IdleState(movement) },
                { PlayerStateType.Move, new MoveState(movement) },
                { PlayerStateType.Dead, new DeathState(_health) }
            };

            _stateMachine = new PlayerStateMachine(states);
            _stateMachine.Enter(PlayerStateType.Idle);
            Debug.Log("State machine initialized");

            _stateMachine.OnStateChanged += _playerAnimator.Play;
            _messageService.Subscribe<PlayerDiedMessage>(OnPlayerDied);
            _fallDamageService.Track(Object, _playerSettings.FallDamage);

            if (Object.HasInputAuthority)
            {
                Debug.Log("Input enabled for local player");

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
            Debug.Log("Player fully initialized");
        }

        public override void Spawned()
        {
            Debug.Log($"NetworkPlayer Spawned. HasStateAuthority: {HasStateAuthority}, HasInputAuthority: {Object.HasInputAuthority}, Object: {Object.Id}");

            if (Object.HasInputAuthority)
            {
                _cameraTargetBinder.Initialize();
            }

            if (!Object.HasStateAuthority)
            {
                Initialize();
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public async void RPC_RequestSpawnPlayer(PlayerRef player, Vector3 position, Quaternion rotation)
        {
            if (!HasStateAuthority) return;
            
            Debug.Log($"Server received spawn request from player {player}");
            try
            {
                var runner = Object.Runner;
                if (runner == null)
                {
                    Debug.LogError("Runner is null in RPC_RequestSpawnPlayer");
                    return;
                }

                var playerPrefab = await _assetProvider.Load<GameObject>(AssetLabels.PLAYER);
                if (playerPrefab == null)
                {
                    Debug.LogError($"Failed to load player prefab for {player}");
                    return;
                }

                var networkObject = playerPrefab.GetComponent<NetworkObject>();
                if (networkObject == null)
                {
                    Debug.LogError($"Player prefab does not have NetworkObject component for {player}");
                    return;
                }

                var spawnedObject = await runner.SpawnAsync(networkObject, position, rotation, player);
                if (spawnedObject == null)
                {
                    Debug.LogError($"Failed to spawn player for {player}");
                    return;
                }

                _container?.Inject(spawnedObject.gameObject);
                Debug.Log($"Server spawned player for {player}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in RPC_RequestSpawnPlayer: {e.Message}");
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!IsInitialized) return;

            if (GetInput(out NetworkInputData data))
            {
                _stateMachine.Tick(data);

                if (HasStateAuthority)
                {
                    _fallDamageService.Tick();
                }
            }
            else if (Object.HasInputAuthority)
            {
                var localInput = _inputService.GetInput();
                _stateMachine.Tick(localInput);
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

            if (_stateMachine != null && _playerAnimator != null)
            {
                _stateMachine.OnStateChanged -= _playerAnimator.Play;
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
            Unsubscribe();
        }
    }
}