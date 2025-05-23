using System;
using CodeBase.Infrastructure.Notifiers;
using CodeBase.Services.Log;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using Zenject;

namespace CodeBase.Services.NetworkService
{
    public class PhotonNetworkService : INetworkService
    {
        private NetworkRunnerProvider _runnerProvider;
        private ILogService _log;
        private NetworkCallbacks _callbacks;
        private NetworkSceneLoadNotifier _sceneLoadNotifier;

        [Inject]
        public void Construct(
            NetworkRunnerProvider runnerProvider,
            ILogService logService,
            NetworkCallbacks callbacks)
        {
            _runnerProvider = runnerProvider;
            _log = logService;
            _callbacks = callbacks;

            _sceneLoadNotifier = new NetworkSceneLoadNotifier(_callbacks, _runnerProvider);
        }

        private async UniTask<NetworkRunner> CreateRunner()
        {
            try
            {
                _log.Log("[PhotonNetworkService] Creating NetworkRunner...");
                var runner = new GameObject("NetworkRunner").AddComponent<NetworkRunner>();

                UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);

                runner.AddCallbacks(_callbacks);
                runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

                _runnerProvider.Initialize(runner);
                return runner;
            }
            catch (System.Exception e)
            {
                _log.LogError($"[PhotonNetworkService] Failed to create NetworkRunner: {e.Message}");
                return null;
            }
        }

        public async UniTask<StartGameResult> StartGame(GameMode mode)
        {
            _log.Log($"[PhotonNetworkService] Starting game in {mode} mode...");
            _log.Log($"[PhotonNetworkService] Current runner state - IsServer: {_runnerProvider.Runner.IsServer}," +
                     $" IsClient: {_runnerProvider.Runner.IsClient}, IsRunning: {_runnerProvider.Runner.IsRunning}," +
                     $" Scene: {_runnerProvider.Runner.SceneManager?.MainRunnerScene.name ?? "None"}");

            var startGameArgs = new StartGameArgs()
            {
                GameMode = mode,
                SessionName = "TestRoom",
                PlayerCount = 4
            };

            var result = await _runnerProvider.Runner.StartGame(startGameArgs);

            _log.Log($"[PhotonNetworkService] Game started with result: {result.Ok}," +
                     $" IsServer: {_runnerProvider.Runner.IsServer}, " +
                     $"IsClient: {_runnerProvider.Runner.IsClient}, " +
                     $"IsRunning: {_runnerProvider.Runner.IsRunning}, " +
                     $"Scene: {_runnerProvider.Runner.SceneManager?.MainRunnerScene.name ?? "None"}");

            if (result.Ok)
            {
                _log.Log($"[PhotonNetworkService] Game started successfully in {mode} mode");
            }
            else
            {
                _log.LogError($"[PhotonNetworkService] Failed to start game in {mode} mode: {result.ShutdownReason}");
            }

            return result;
        }

        public async UniTask StartHost()
        {
            try
            {
                _log.Log("[PhotonNetworkService] Starting host...");
                var runner = await CreateRunner();
                if (runner == null)
                {
                    _log.LogError("[PhotonNetworkService] Failed to create runner");
                    return;
                }

                var result = await StartGame(GameMode.Host);
                if (!result.Ok)
                {
                    _log.LogError($"[PhotonNetworkService] Failed to start host: {result.ShutdownReason}");
                    return;
                }

                _log.Log("[PhotonNetworkService] Host started successfully");
            }
            catch (System.Exception e)
            {
                _log.LogError($"[PhotonNetworkService] Error starting host: {e.Message}");
            }
        }

        public async UniTask StartClient()
        {
            try
            {
                _log.Log("Starting client...");
                var runner = await CreateRunner();
                var result = await StartGame(GameMode.Client);

                if (!result.Ok)
                {
                    _log.LogError($"Failed to start client: {result.ShutdownReason}");
                }
            }
            catch (Exception e)
            {
                _log.LogError($"Error starting client: {e.Message}");
            }
        }

        public void Disconnect()
        {
            if (_runnerProvider.Runner != null)
            {
                _runnerProvider.Runner.Shutdown();
            }
        }

        public void Dispose()
        {
            _sceneLoadNotifier?.Dispose();
        }
    }
}