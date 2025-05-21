using CodeBase.Services.InputService;

namespace CodeBase.Gameplay.Player.States
{
    public class MoveState : IPlayerState
    {
        private readonly PlayerMovement _movement;

        public MoveState(PlayerMovement movement)
        {
            _movement = movement;
        }

        public void Enter()
        {
        }

        public void Exit()
        {
            _movement.Stop();
        }

        public PlayerStateType Tick(NetworkInputData input)
        {
            if (input.MoveInput.sqrMagnitude < 0.01f)
            {
                return PlayerStateType.Idle;
            }

            _movement.Move(input);
            return PlayerStateType.Move;
        }
    }
}