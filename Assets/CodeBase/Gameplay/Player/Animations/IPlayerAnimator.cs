using CodeBase.Gameplay.Player.States;

namespace CodeBase.Gameplay.Player.Animations
{
    public interface IPlayerAnimator
    {
        void Play(PlayerStateType state);
    }
}