using CodeBase.Services.InputService;

namespace CodeBase.Gameplay.Player.States
{
    public interface IPlayerState
    {
        void Enter();
        void Exit();
        PlayerStateType Tick(NetworkInputData input);
    }
} 