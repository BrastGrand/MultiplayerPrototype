using Cysharp.Threading.Tasks;

namespace CodeBase.Gameplay
{
    public interface IGameplayReadyNotifier
    {
        UniTask WaitUntilReady();
        void NotifyReady();
    }
}