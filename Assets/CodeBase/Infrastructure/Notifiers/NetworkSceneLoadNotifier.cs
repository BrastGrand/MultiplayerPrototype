using CodeBase.Services.Message;
using CodeBase.Services.NetworkService;
using Fusion;
using UnityEngine;

namespace CodeBase.Infrastructure.Notifiers
{
    public class NetworkSceneLoadNotifier
    {
        private readonly NetworkCallbacks _networkCallbacks;
        private readonly NetworkRunnerProvider _runnerProvider;

        public NetworkSceneLoadNotifier(NetworkCallbacks networkCallbacks, NetworkRunnerProvider runnerProvider)
        {
            Debug.Log("NetworkSceneLoadNotifier: Constructor");
            _networkCallbacks = networkCallbacks;
            _runnerProvider = runnerProvider;

            if (_runnerProvider.Runner != null)
            {
                Debug.Log("NetworkSceneLoadNotifier: Runner already exists, adding callbacks");
                _runnerProvider.Runner.AddCallbacks(_networkCallbacks);
            }
            else
            {
                Debug.Log("NetworkSceneLoadNotifier: Runner is null, subscribing to OnRunnerInitialized");
                _runnerProvider.OnRunnerInitialized += OnRunnerInitialized;
            }
        }

        private void OnRunnerInitialized(NetworkRunner runner)
        {
            Debug.Log($"NetworkSceneLoadNotifier: OnRunnerInitialized - Runner: {runner != null}, IsServer: {runner?.IsServer}");
            runner.AddCallbacks(_networkCallbacks);
        }

        public void Dispose()
        {
            Debug.Log("NetworkSceneLoadNotifier: Dispose");
            if (_runnerProvider.Runner != null)
            {
                _runnerProvider.Runner.RemoveCallbacks(_networkCallbacks);
            }
            _runnerProvider.OnRunnerInitialized -= OnRunnerInitialized;
        }
    }
}