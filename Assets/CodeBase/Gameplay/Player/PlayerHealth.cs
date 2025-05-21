using CodeBase.Services.MessageService;
using CodeBase.Services.MessageService.Messages;
using Fusion;
using UnityEngine;

namespace CodeBase.Gameplay.Player
{
    public class PlayerHealth : NetworkBehaviour
    {
        [Networked] public float CurrentHealth { get; private set; }

        private IMessageService _messageService;
        private PlayerSettings _playerSettings;

        public void Initialize(IMessageService messageService, PlayerSettings playerSettings)
        {
            _messageService = messageService;
            _playerSettings = playerSettings;

            CurrentHealth = _playerSettings.MaxHp;
            _messageService.Subscribe<PlayerTakeDamageMessage>(OnFallDamage);
        }

        public void Heal(float amount)
        {
            if (!HasStateAuthority) return;

            CurrentHealth = Mathf.Min(_playerSettings.MaxHp, CurrentHealth + amount);
            _messageService.Publish(new HealthChangedMessage { CurrentHp = CurrentHealth });
        }

        public void ResetHealth()
        {
            if (!HasStateAuthority) return;

            CurrentHealth = _playerSettings.MaxHp;
            _messageService.Publish(new HealthChangedMessage { CurrentHp = CurrentHealth });
        }

        private void OnFallDamage(PlayerTakeDamageMessage message)
        {
            if (message.PlayerObject == Object)
                TakeDamage(message.Damage);
        }

        private void TakeDamage(float amount)
        {
            if (!HasStateAuthority) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            _messageService.Publish(new HealthChangedMessage { CurrentHp = CurrentHealth });

            if (CurrentHealth <= 0)
                _messageService.Publish(new PlayerDiedMessage { PlayerObject = Object });
        }
    }
}