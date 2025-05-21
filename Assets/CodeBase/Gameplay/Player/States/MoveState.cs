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

        public void Enter() { }
        public void Tick(NetworkInputData input)
        {
            _movement.Move(input.MoveInput);
        }

        public void Exit() { }

        public bool ShouldTransition(out PlayerStateType nextState)
        {
            nextState = PlayerStateType.Idle;
            return _movement != null && !_movement.HasMovementInput();
        }
    }
}