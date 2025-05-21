using CodeBase.Services.InputService;

namespace CodeBase.Gameplay.Player.States
{
    public class IdleState : IPlayerState
    {
        private readonly PlayerMovement _movement;

        public IdleState(PlayerMovement movement)
        {
            _movement = movement;
        }

        public void Enter() { }
        public void Tick(NetworkInputData input)
        {
            if (input.MoveInput.sqrMagnitude > 0.01f)
                _movement.Move(input.MoveInput);

        }

        public void Exit() { }

        public bool ShouldTransition(out PlayerStateType nextState)
        {
            nextState = PlayerStateType.Move;
            return _movement != null && _movement.HasMovementInput();
        }
    }
}