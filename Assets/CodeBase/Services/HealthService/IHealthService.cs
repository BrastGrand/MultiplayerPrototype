using System;

namespace CodeBase.Gameplay.Player
{
    public interface IHealthService
    {
        float CurrentHealth { get; }
        void TakeDamage(float amount);
        void SetHealth(float amount);
        event Action<float> OnHealthChanged;
        event Action OnDeath;
    }
}