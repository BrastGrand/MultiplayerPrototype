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

        public void Enter()
        {
            _movement.Stop();
        }

        public void Exit()
        {
        }

        public PlayerStateType Tick(NetworkInputData input)
        {
            if (input.MoveInput.sqrMagnitude > 0.01f)
            {
                return PlayerStateType.Move;
            }

            // Даже в состоянии покоя вызываем Move для поддержания физики (гравитация, падение)
            _movement.Move(input);

            return PlayerStateType.Idle;
        }
    }
}