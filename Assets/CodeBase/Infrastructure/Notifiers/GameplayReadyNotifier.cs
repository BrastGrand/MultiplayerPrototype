using Cysharp.Threading.Tasks;

namespace CodeBase.Gameplay
{
    public class GameplayReadyNotifier : IGameplayReadyNotifier
    {
        private readonly UniTaskCompletionSource _taskCompletionSource = new UniTaskCompletionSource();

        public void NotifyReady()
        {
            if (_taskCompletionSource.Task.Status.IsCompleted() == false)
                _taskCompletionSource.TrySetResult();
        }

        public UniTask WaitUntilReady() => _taskCompletionSource.Task;
    }
}