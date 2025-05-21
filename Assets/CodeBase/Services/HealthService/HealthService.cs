using System;
using UnityEngine;

namespace CodeBase.Gameplay.Player
{
    public class HealthService : IHealthService
    {
        private readonly float _maxHealth;
        public float CurrentHealth { get; private set; }

        public event Action<float> OnHealthChanged;
        public event Action OnDeath;

        public HealthService(float maxHealth)
        {
            _maxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (CurrentHealth <= 0) return;

            CurrentHealth -= amount;
            OnHealthChanged?.Invoke(CurrentHealth);

            if (CurrentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }

        public void SetHealth(float amount)
        {
            CurrentHealth = Mathf.Clamp(amount, 0, _maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth);
        }
    }
}