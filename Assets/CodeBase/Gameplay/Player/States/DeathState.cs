using CodeBase.Services.InputService;
using UnityEngine;

namespace CodeBase.Gameplay.Player.States
{
    public class DeathState : IPlayerState
    {
        private readonly PlayerHealth _health;

        public DeathState(PlayerHealth health)
        {
            _health = health;
        }

        public void Enter()
        {
            Debug.Log("Player entered death state");
        }

        public void Exit()
        {
            Debug.Log("Player exited death state");
        }

        public PlayerStateType Tick(NetworkInputData input)
        {
            // В состоянии смерти игрок не реагирует на ввод
            // Можно добавить таймер или другую логику для автоматического респавна
            return PlayerStateType.Dead;
        }
    }
}