using CodeBase.Services.MessageService;
using CodeBase.Services.MessageService.Messages;
using CodeBase.Services.NetworkService;

namespace CodeBase.Gameplay
{
    public class NetworkSceneLoadNotifier
    {
        private readonly NetworkCallbacks _networkCallbacks;
        private readonly NetworkRunnerProvider _runnerProvider;
        private readonly IMessageService _messageService;

        public NetworkSceneLoadNotifier(NetworkCallbacks networkCallbacks, NetworkRunnerProvider runnerProvider, IMessageService messageService)
        {
            _networkCallbacks = networkCallbacks;
            _runnerProvider = runnerProvider;
            _messageService = messageService;

            _messageService.Subscribe<SceneLoadedMessage>(OnSceneLoaded);
        }

        private void OnSceneLoaded(SceneLoadedMessage message)
        {
            var runner = _runnerProvider.Runner;
            if (runner != null)
            {
                _networkCallbacks.NotifySceneLoadDone(runner);
            }
        }

        public void Dispose()
        {
            _messageService.Unsubscribe<SceneLoadedMessage>(OnSceneLoaded);
        }
    }
}