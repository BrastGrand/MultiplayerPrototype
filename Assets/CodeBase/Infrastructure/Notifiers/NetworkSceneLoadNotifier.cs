using CodeBase.Services.NetworkService;
using Fusion;

namespace CodeBase.Infrastructure.Notifiers
{
    public class NetworkSceneLoadNotifier
    {
        private readonly NetworkCallbacks _networkCallbacks;
        private readonly NetworkRunnerProvider _runnerProvider;

        public NetworkSceneLoadNotifier(NetworkCallbacks networkCallbacks, NetworkRunnerProvider runnerProvider)
        {
            _networkCallbacks = networkCallbacks;
            _runnerProvider = runnerProvider;

            if (_runnerProvider.Runner != null)
            {
                _runnerProvider.Runner.AddCallbacks(_networkCallbacks);
            }
            else
            {
                _runnerProvider.OnRunnerInitialized += OnRunnerInitialized;
            }
        }

        private void OnRunnerInitialized(NetworkRunner runner)
        {
            runner.AddCallbacks(_networkCallbacks);
        }

        public void Dispose()
        {
            if (_runnerProvider.Runner != null)
            {
                _runnerProvider.Runner.RemoveCallbacks(_networkCallbacks);
            }
            _runnerProvider.OnRunnerInitialized -= OnRunnerInitialized;
        }
    }
}