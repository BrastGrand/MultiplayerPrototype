using System.Collections.Generic;
using System.Linq;
using CodeBase.Gameplay;
using CodeBase.Gameplay.Player;
using CodeBase.Services.GameMode;
using CodeBase.Services.Log;
using CodeBase.Services.Message;
using CodeBase.Services.NetworkService;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using Zenject;

namespace CodeBase.Services.PlayerSpawnerService
{
    public class PlayerSpawner : IPlayerSpawner
    {
        private readonly INetworkObjectSpawner _networkSpawner;
        private readonly IMessageService _messageService;
        private readonly ILogService _log;
        private readonly DiContainer _container;
        private readonly NetworkRunnerProvider _runnerProvider;

        private readonly HashSet<PlayerRef> _pendingPlayers = new();
        private bool _sceneLoaded;
        private bool _spawnPointsReady;
        private ISpawnPointsProvider _spawnPointsProvider;

        public event System.Action<PlayerRef> OnPlayerSpawned;

        [Inject]
        public PlayerSpawner(
            INetworkObjectSpawner networkSpawner,
            IMessageService messageService,
            IGameModeService modeService,
            ILogService log,
            DiContainer container,
            IGameplayReadyNotifier readyNotifier,
            NetworkRunnerProvider runnerProvider)
        {
            _networkSpawner = networkSpawner;
            _messageService = messageService;
            _log = log;
            _container = container;
            _runnerProvider = runnerProvider;

            _messageService.Subscribe<PlayerConnectedMessage>(OnPlayerConnected);
            _messageService.Subscribe<SceneLoadedMessage>(OnSceneLoaded);
            _messageService.Subscribe<SpawnPointsReadyMessage>(OnSpawnPointsReady);
        }

        private void OnPlayerConnected(PlayerConnectedMessage msg)
        {
            _log.Log($"[PlayerSpawner] PlayerConnected: {msg.PlayerRef.PlayerId}");
            _pendingPlayers.Add(msg.PlayerRef);
            TrySpawnAllPendingPlayers();
        }

        private void OnSceneLoaded(SceneLoadedMessage msg)
        {
            _log.Log($"[PlayerSpawner] SceneLoaded: {msg.SceneName}");
            _sceneLoaded = true;
            TrySpawnAllPendingPlayers();
        }

        private void OnSpawnPointsReady(SpawnPointsReadyMessage msg)
        {
            _log.Log("[PlayerSpawner] SpawnPointsReady");
            _spawnPointsReady = true;
            _spawnPointsProvider = msg.Provider;
            TrySpawnAllPendingPlayers();
        }

        private void TrySpawnAllPendingPlayers()
        {
            if (!_sceneLoaded || !_spawnPointsReady || _spawnPointsProvider == null)
            {
                _log.Log($"[PlayerSpawner] Not ready: SceneLoaded={_sceneLoaded}, SpawnPointsReady={_spawnPointsReady}, Provider={_spawnPointsProvider != null}");
                return;
            }

            foreach (var playerRef in _pendingPlayers.ToList())
            {
                if (_runnerProvider.Runner.GetPlayerObject(playerRef) == null)
                {
                    var spawnPoint = _spawnPointsProvider.SpawnPoints[Random.Range(0, _spawnPointsProvider.SpawnPoints.Count)];
                    _log.Log($"[PlayerSpawner] Spawning player {playerRef.PlayerId} at {spawnPoint.position}");
                    _networkSpawner.SpawnPlayer(playerRef, spawnPoint.position, spawnPoint.rotation, OnSpawnedPlayer);
                }
                else
                {
                    _log.Log($"[PlayerSpawner] Player {playerRef.PlayerId} already exists, skipping");
                }
                _pendingPlayers.Remove(playerRef);
            }
        }

        private async void OnSpawnedPlayer(NetworkPlayer player)
        {
            if (player == null)
            {
                _log.Log("[PlayerSpawner] Failed to spawn player");
                return;
            }

            try
            {
                _container.Inject(player);
                await player.Initialize();
                _log.Log($"[PlayerSpawner] Player {player.Object.InputAuthority.PlayerId} initialized and spawned");
                OnPlayerSpawned?.Invoke(player.Object.InputAuthority);
            }
            catch (System.Exception e)
            {
                _log.Log($"[PlayerSpawner] Error initializing player: {e.Message}\n{e.StackTrace}");
            }
        }

        public void Dispose()
        {
            _messageService.Unsubscribe<PlayerConnectedMessage>(OnPlayerConnected);
            _messageService.Unsubscribe<SceneLoadedMessage>(OnSceneLoaded);
            _messageService.Unsubscribe<SpawnPointsReadyMessage>(OnSpawnPointsReady);
        }
    }
}