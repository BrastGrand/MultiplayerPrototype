using CodeBase.Services.InputService;

namespace CodeBase.Gameplay.Player.States
{
    public class DeathState : IPlayerState
    {
        private PlayerHealth _health;

        public DeathState(PlayerHealth health)
        {
            _health = health;
        }
        public void Enter() { }
        public void Tick(NetworkInputData input) { }
        public void Exit() { }

        public bool ShouldTransition(out PlayerStateType nextState)
        {
            nextState = PlayerStateType.Dead;
            return _health.CurrentHealth <= 0;
        }
    }
}