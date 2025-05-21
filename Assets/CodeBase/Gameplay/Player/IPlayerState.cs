using CodeBase.Services.InputService;

namespace CodeBase.Gameplay.Player
{
    public interface IPlayerState
    {
        void Enter();
        void Tick(NetworkInputData input);
        void Exit();
        bool ShouldTransition(out PlayerStateType nextState);
    }
}