using CodeBase.Services.InputService;

namespace CodeBase.Gameplay.Player.States
{
    public class DeathState : IPlayerState
    {
        public DeathState(PlayerHealth health)
        {
        }

        public void Enter()
        {
        }

        public void Exit()
        {
        }

        public PlayerStateType Tick(NetworkInputData input)
        {
            return PlayerStateType.Dead;
        }
    }
}