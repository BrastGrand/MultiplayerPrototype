using System;
using System.Threading.Tasks;
using CodeBase.Gameplay;
using CodeBase.Gameplay.Player;
using CodeBase.Services.GameModeService;
using CodeBase.Services.LogService;
using CodeBase.Services.MessageService;
using CodeBase.Services.MessageService.Messages;
using CodeBase.Services.NetworkService;
using Cysharp.Threading.Tasks;
using Fusion;
using Zenject;
using Random = UnityEngine.Random;

namespace CodeBase.Services.PlayerSpawnerService
{
    public class PlayerSpawner : IPlayerSpawner
    {
        private readonly INetworkObjectSpawner _networkSpawner;
        private readonly IMessageService _messageService;
        private readonly IGameModeService _modeService;
        private readonly ILogService _log;
        private readonly INetworkCallbacksService _callbacksService;
        private readonly DiContainer _container;
        private readonly IGameplayReadyNotifier _readyNotifier;

        private readonly TaskCompletionSource<ISpawnPointsProvider> _spawnPointsTaskCompletionSource = new TaskCompletionSource<ISpawnPointsProvider>();

        public event Action<PlayerRef> OnPlayerSpawned;

        protected PlayerSpawner(
            INetworkObjectSpawner spawner,
            IMessageService messageService,
            IGameModeService modeService,
            ILogService logService,
            INetworkCallbacksService callbacksService,
            DiContainer container,
            IGameplayReadyNotifier readyNotifier)
        {
            _networkSpawner = spawner;
            _messageService = messageService;
            _modeService = modeService;
            _log = logService;
            _callbacksService = callbacksService;
            _container = container;
            _readyNotifier = readyNotifier;

            _messageService.Subscribe<SpawnPointsReadyMessage>(OnSpawnPointsReady);
            _messageService.Subscribe<PlayerConnectedMessage>(OnPlayerConnected);
        }

        private void OnSpawnPointsReady(SpawnPointsReadyMessage message)
        {
            _log.Log("Player Spawner OnSpawnPointsReady");

            if (!_spawnPointsTaskCompletionSource.Task.IsCompleted)
                _spawnPointsTaskCompletionSource.SetResult(message.Provider);
        }

        private void OnPlayerConnected(PlayerConnectedMessage message)
        {
            _log.Log("Player Spawner OnPlayerConnected");

            if (_modeService.IsHost)
            {
                _ = SpawnPlayerInternal(message.PlayerRef);
            }
        }

        private async UniTask SpawnPlayerInternal(PlayerRef playerRef)
        {
            _log.Log("Player Spawner SpawnPlayerInternal");

            await _readyNotifier.WaitUntilReady();

            //ждем загрузку сцены
            await _callbacksService.SceneLoadCompleted;

            //ждем доступность точек спавна
            var provider = await _spawnPointsTaskCompletionSource.Task;

            var spawnPoint = provider.SpawnPoints[Random.Range(0, provider.SpawnPoints.Count)];
            _networkSpawner.SpawnPlayer(playerRef, spawnPoint.position, spawnPoint.rotation, OnSpawnedPlayer);

            OnPlayerSpawned?.Invoke(playerRef);
        }

        private async void OnSpawnedPlayer(NetworkPlayer player)
        {
            if (player != null)
            {
                _container.InjectGameObject(player.gameObject);
                await UniTask.NextFrame();
                await _readyNotifier.WaitUntilReady();

                player.Initialize();
            }
        }

        public void Dispose()
        {
            _log.Log("Player Spawner Dispose");

            if (_modeService.IsHost)
            {
                _messageService.Unsubscribe<PlayerConnectedMessage>(OnPlayerConnected);
            }

            _messageService.Unsubscribe<SpawnPointsReadyMessage>(OnSpawnPointsReady);
        }
    }
}