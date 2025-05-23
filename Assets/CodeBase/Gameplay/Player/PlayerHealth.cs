using CodeBase.Services.Message;
using Fusion;
using UnityEngine;

namespace CodeBase.Gameplay.Player
{
    public class PlayerHealth : NetworkBehaviour
    {
        [Networked] public float CurrentHealth { get; private set; }

        private IMessageService _messageService;
        private PlayerSettings _playerSettings;
        private bool _isInitialized;

        public void Initialize(IMessageService messageService, PlayerSettings playerSettings)
        {
            _messageService = messageService;
            _playerSettings = playerSettings;

            // Инициализируем здоровье только на сервере/хосте
            if (HasStateAuthority)
            {
                CurrentHealth = _playerSettings.MaxHp;
            }

            _messageService.Subscribe<PlayerTakeDamageMessage>(OnFallDamage);
            _isInitialized = true;

            Debug.Log($"[PlayerHealth] Initialized on {(HasStateAuthority ? "Server" : "Client")}. " +
                     $"Object: {Object.Id}, Initial Health: {CurrentHealth}");
        }

        public void Heal(float amount)
        {
            if (!HasStateAuthority) return;

            float newHealth = Mathf.Min(_playerSettings.MaxHp, CurrentHealth + amount);
            CurrentHealth = newHealth;

            Debug.Log($"[PlayerHealth] Healed on server. Object: {Object.Id}, New Health: {CurrentHealth}");
            
            // Уведомляем всех клиентов об изменении здоровья
            RPC_UpdateHealthUI(newHealth);
        }

        public void ResetHealth()
        {
            if (!HasStateAuthority) return;

            CurrentHealth = _playerSettings.MaxHp;
            
            Debug.Log($"[PlayerHealth] Health reset on server. Object: {Object.Id}, Health: {CurrentHealth}");
            
            // Уведомляем всех клиентов
            RPC_UpdateHealthUI(CurrentHealth);
        }

        private void OnFallDamage(PlayerTakeDamageMessage message)
        {
            if (message.PlayerObject == Object)
            {
                Debug.Log($"[PlayerHealth] Fall damage received. Object: {Object.Id}, Damage: {message.Damage}");
                TakeDamage(message.Damage);
            }
        }

        private void TakeDamage(float amount)
        {
            if (!HasStateAuthority) return;

            float newHealth = Mathf.Max(0, CurrentHealth - amount);
            CurrentHealth = newHealth;

            Debug.Log($"[PlayerHealth] Damage taken on server. Object: {Object.Id}, " +
                     $"Damage: {amount}, New Health: {CurrentHealth}");

            // Уведомляем всех клиентов об изменении здоровья
            RPC_UpdateHealthUI(newHealth);

            if (CurrentHealth <= 0)
            {
                Debug.Log($"[PlayerHealth] Player died. Object: {Object.Id}");
                _messageService.Publish(new PlayerDiedMessage { PlayerObject = Object });
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_UpdateHealthUI(float newHealth)
        {
            if (!_isInitialized) return;

            Debug.Log($"[PlayerHealth] RPC_UpdateHealthUI received. Object: {Object.Id}, " +
                     $"Health: {newHealth}, HasInputAuth: {Object.HasInputAuthority}");

            // Публикуем сообщение для обновления UI только для игрока с InputAuthority
            if (Object.HasInputAuthority)
            {
                _messageService.Publish(new HealthChangedMessage { CurrentHp = newHealth });
                Debug.Log($"[PlayerHealth] Health UI updated for local player. Health: {newHealth}");
            }
        }

        // Метод для синхронизации здоровья при спавне
        public override void Spawned()
        {
            if (HasStateAuthority && _isInitialized)
            {
                // Отправляем текущее здоровье всем клиентам
                RPC_UpdateHealthUI(CurrentHealth);
            }
        }
    }
}