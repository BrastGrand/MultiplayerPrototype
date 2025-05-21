using CodeBase.Gameplay;
using CodeBase.Infrastructure.Notifiers;
using CodeBase.Services.GameMode;
using CodeBase.Services.InputService;
using CodeBase.Services.Log;
using CodeBase.Services.Message;
using CodeBase.Services.PlayerSpawnerService;
using Cysharp.Threading.Tasks;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace CodeBase.Services.NetworkService
{
    public class PhotonNetworkService : INetworkService
    {
        private NetworkRunnerProvider _runnerProvider;
        private IMessageService _messageService;
        private INetworkInputService _inputService;
        private ILogService _log;
        private IGameModeService _modeService;
        private INetworkObjectSpawner _networkSpawner;
        private IGameplayReadyNotifier _readyNotifier;
        private ISpawnPointsProvider _spawnPointsProvider;
        private NetworkCallbacks _callbacks;
        private DiContainer _container;

        [Inject]
        public void Construct(
            NetworkRunnerProvider runnerProvider,
            IMessageService messageService,
            INetworkInputService inputService,
            ILogService logService,
            IGameModeService modeService,
            INetworkObjectSpawner networkSpawner,
            IGameplayReadyNotifier readyNotifier,
            ISpawnPointsProvider spawnPointsProvider,
            DiContainer container)
        {
            _runnerProvider = runnerProvider;
            _messageService = messageService;
            _inputService = inputService;
            _log = logService;
            _modeService = modeService;
            _networkSpawner = networkSpawner;
            _readyNotifier = readyNotifier;
            _spawnPointsProvider = spawnPointsProvider;
            _container = container;
        }

        public async UniTask StartHost()
        {
            _callbacks = _container.InstantiateComponent<NetworkCallbacks>(new GameObject("NetworkCallbacks"));
            _callbacks.Construct(_messageService, _modeService, _inputService);

            var runner = new GameObject("NetworkRunner").AddComponent<NetworkRunner>();
            _runnerProvider.Initialize(runner);

            var sceneManager = runner.AddComponent<NetworkSceneManagerDefault>();
            var sceneLoadNotifier = _container.Instantiate<NetworkSceneLoadNotifier>();

            // Create the NetworkSceneInfo from the current scene
            var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            var sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid) {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            }

            var result = await runner.StartGame(new StartGameArgs
            {
                GameMode = Fusion.GameMode.Host,
                SessionName = "TestRoom",
                Scene = scene,
                SceneManager = sceneManager,
                PlayerCount = 4
            });

            if (result.Ok)
            {
                _log.Log("Host started successfully");
                // Ждем, пока runner действительно станет сервером
                await UniTask.WaitUntil(() => runner.IsServer);
                _log.Log($"Runner is now server: {runner.IsServer}");
            }
            else
            {
                _log.Log($"Failed to start host: {result.ShutdownReason}");
            }
        }

        public async UniTask StartClient()
        {
            _callbacks = _container.InstantiateComponent<NetworkCallbacks>(new GameObject("NetworkCallbacks"));
            _callbacks.Construct(_messageService, _modeService, _inputService);

            var runner = new GameObject("NetworkRunner").AddComponent<NetworkRunner>();
            _runnerProvider.Initialize(runner);

            var sceneManager = runner.AddComponent<NetworkSceneManagerDefault>();
            var sceneLoadNotifier = _container.Instantiate<NetworkSceneLoadNotifier>();

            // Create the NetworkSceneInfo from the current scene
            var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            var sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid) {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            }

            var result = await runner.StartGame(new StartGameArgs
            {
                GameMode = Fusion.GameMode.Client,
                SessionName = "TestRoom",
                Scene = scene,
                SceneManager = sceneManager
            });

            if (result.Ok)
            {
                _log.Log("Client started successfully");
            }
            else
            {
                _log.Log($"Failed to start client: {result.ShutdownReason}");
            }
        }

        public void Disconnect()
        {
            _runnerProvider
                .Runner
                .Shutdown()
                .Dispose();
        }
    }
}