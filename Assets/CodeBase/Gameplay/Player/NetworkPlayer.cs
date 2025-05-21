using System.Collections.Generic;
using CodeBase.Gameplay.Player.Animations;
using CodeBase.Gameplay.Player.States;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.FallDamageService;
using CodeBase.Services.InputService;
using CodeBase.Services.MessageService;
using CodeBase.Services.MessageService.Messages;
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

        private INetworkInputService _inputService;
        private IMessageService _messageService;
        private IAssetProvider _assetProvider;
        private FallDamageService _fallDamageService;
        private PlayerStateMachine _stateMachine;
        private PlayerHealth _health;
        private PlayerSettings _playerSettings;

        private bool _isInitialized;

        [Inject]
        public void Construct(
            INetworkInputService inputService,
            IMessageService messageService,
            IAssetProvider assetProvider)
        {
            _inputService = inputService;
            _messageService = messageService;
            _assetProvider = assetProvider;
        }

        public async void Initialize()
        {
            _playerSettings = await _assetProvider.Load<PlayerSettings>("PlayerSettings");

            var movement = GetComponent<PlayerMovement>();
            _health = GetComponent<PlayerHealth>();

            movement.Initialize(_playerSettings.MoveSpeed);
            _health.Initialize(_messageService, _playerSettings);

            _fallDamageService = new FallDamageService(_playerSettings.FallDamageThreshold, _messageService);

            var states = new Dictionary<PlayerStateType, IPlayerState>
            {
                { PlayerStateType.Idle, new IdleState(movement) },
                { PlayerStateType.Move, new MoveState(movement) },
                { PlayerStateType.Dead, new DeathState(_health) }
            };

            _stateMachine = new PlayerStateMachine(states);
            _stateMachine.Enter(PlayerStateType.Idle);

            if(_playerAnimator != null)
                _stateMachine.OnStateChanged += _playerAnimator.Play;

            _messageService.Subscribe<PlayerDiedMessage>(OnPlayerDied);
            _fallDamageService.Track(Object, _playerSettings.FallDamage);

            if (_healthUI != null)
            {
                _healthUI.Initialize(_playerSettings.MaxHp, _messageService);
                _healthUI.gameObject.SetActive(true);
            }

            _inputService.Enable();
            _isInitialized = true;
        }


        public override void Spawned()
        {
            if(!HasStateAuthority) return;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || !_isInitialized) return;

            var input = _inputService.GetInput();
            _stateMachine.Tick(input);
            _fallDamageService.Tick();
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
            _healthUI?.Clear();

            if (_playerAnimator != null)
            {
                _stateMachine.OnStateChanged -= _playerAnimator.Play;
            }

            _messageService.Unsubscribe<PlayerDiedMessage>(OnPlayerDied);
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}