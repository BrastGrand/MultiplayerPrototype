using System.Collections.Generic;
using System.Linq;
using CodeBase.Gameplay;
using CodeBase.Gameplay.Player;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.InputService;
using CodeBase.Services.Log;
using CodeBase.Services.Message;
using CodeBase.Services.NetworkGameMode;
using CodeBase.Services.NetworkService;
using Fusion;
using Zenject;

namespace CodeBase.Services.PlayerSpawnerService
{
    public class PlayerSpawner : IPlayerSpawner
    {
        private readonly INetworkObjectSpawner _networkSpawner;
        private readonly IMessageService _messageService;
        private readonly ILogService _log;
        private readonly INetworkInputService _inputService;
        private readonly IAssetProvider _assetProvider;
        private readonly NetworkRunnerProvider _runnerProvider;
        private ISpawnPointsProvider _spawnPointsProvider;

        private readonly HashSet<PlayerRef> _pendingPlayers = new();
        private readonly HashSet<PlayerRef> _spawningPlayers = new();
        private bool _sceneLoaded;
        private bool _spawnPointsReady;
        private bool _isProviderReady;

        public event System.Action<PlayerRef> OnPlayerSpawned;

        [Inject]
        public PlayerSpawner(
            INetworkObjectSpawner networkSpawner,
            IMessageService messageService,
            IGameModeService modeService,
            ILogService log,
            INetworkInputService inputService,
            IAssetProvider assetProvider,
            IGameplayReadyNotifier readyNotifier,
            NetworkRunnerProvider runnerProvider,
            ISpawnPointsProvider spawnPointsProvider)
        {
            _networkSpawner = networkSpawner;
            _messageService = messageService;
            _log = log;
            _inputService = inputService;
            _assetProvider = assetProvider;
            _runnerProvider = runnerProvider;
            _spawnPointsProvider = spawnPointsProvider;

            _log.Log($"[PlayerSpawner] Constructor called. Instance: {GetHashCode()}");
            _log.Log($"[PlayerSpawner] Injected SpawnPointsProvider: {_spawnPointsProvider?.GetType()?.Name}, SpawnPoints: {_spawnPointsProvider?.SpawnPoints?.Count ?? -1}");

            _messageService.Subscribe<PlayerConnectedMessage>(OnPlayerConnected);
            _messageService.Subscribe<SceneLoadedMessage>(OnSceneLoaded);
            _messageService.Subscribe<SpawnPointsReadyMessage>(OnSpawnPointsReady);
            _messageService.Subscribe<PlayerDisconnectedMessage>(OnPlayerDisconnected);
            _messageService.Subscribe<NetworkRunnerReadyMessage>(OnNetworkRunnerReady);
        }

        private void OnPlayerConnected(PlayerConnectedMessage message)
        {
            _log.Log($"[PlayerSpawner] PlayerConnected: {message.PlayerRef.PlayerId}. Instance: {GetHashCode()}");

            if (!_pendingPlayers.Add(message.PlayerRef))
            {
                _log.Log($"[PlayerSpawner] Player {message.PlayerRef.PlayerId} already pending, skipping. Instance: {GetHashCode()}");
                return;
            }

            TrySpawnAllPendingPlayers();
        }

        private void OnPlayerDisconnected(PlayerDisconnectedMessage msg)
        {
            _log.Log($"[PlayerSpawner] PlayerDisconnected: {msg.PlayerRef.PlayerId}");
            _pendingPlayers.Remove(msg.PlayerRef);
            _spawningPlayers.Remove(msg.PlayerRef);
        }

        private void OnSceneLoaded(SceneLoadedMessage msg)
        {
            _log.Log($"[PlayerSpawner] SceneLoaded: {msg.SceneName}");

            // Игнорируем загрузку меню
            //TODO: В данный момент просто костыль
            if (msg.SceneName == "Menu")
            {
                _log.Log("[PlayerSpawner] Ignoring menu scene");
                return;
            }

            _sceneLoaded = true;
            TrySpawnAllPendingPlayers();
        }

        private void OnSpawnPointsReady(SpawnPointsReadyMessage msg)
        {
            _spawnPointsProvider = msg.Provider;
            _spawnPointsReady = true;
            _isProviderReady = _spawnPointsProvider != null && _spawnPointsProvider.SpawnPoints != null && _spawnPointsProvider.SpawnPoints.Count > 0;

            _log.Log($"[PlayerSpawner] State after SpawnPointsReady: _spawnPointsReady={_spawnPointsReady}," +
                     $" _isProviderReady={_isProviderReady}," +
                     $" SpawnPoints={_spawnPointsProvider?.SpawnPoints?.Count ?? -1}");

            TrySpawnAllPendingPlayers();
        }

        private void OnNetworkRunnerReady(NetworkRunnerReadyMessage msg)
        {
            _log.Log("[PlayerSpawner] NetworkRunner ready notification received");
            TrySpawnAllPendingPlayers();
        }

        private void TrySpawnAllPendingPlayers()
        {
            // КРИТИЧЕСКИ ВАЖНО: Только сервер должен спавнить игроков!
            if (_runnerProvider.Runner != null && !_runnerProvider.Runner.IsServer)
            {
                _log.Log($"[PlayerSpawner] Skipping spawn - not a server. IsServer: {_runnerProvider.Runner.IsServer}, IsClient: {_runnerProvider.Runner.IsClient}");
                return;
            }

            if (!IsReadyToSpawn())
            {
                _log.Log($"[PlayerSpawner] Not ready: SceneLoaded={_sceneLoaded}, SpawnPointsReady={_spawnPointsReady}, Provider={_isProviderReady}");
                return;
            }

            if (_runnerProvider.Runner == null)
            {
                _log.LogError("[PlayerSpawner] Runner is null");
                return;
            }

            if (!_runnerProvider.Runner.IsRunning)
            {
                _log.Log("[PlayerSpawner] Runner is not running yet");
                return;
            }

            // Для сервера проверяем только IsServer
            if (!_runnerProvider.Runner.IsServer)
            {
                _log.Log($"[PlayerSpawner] Not a server - IsServer: {_runnerProvider.Runner.IsServer}");
                return;
            }

            if (!_runnerProvider.Runner.SceneManager?.MainRunnerScene.IsValid() ?? true)
            {
                _log.Log("[PlayerSpawner] Runner scene is not valid yet");
                return;
            }

            var playersToSpawn = _pendingPlayers.ToList();
            foreach (var player in playersToSpawn)
            {
                if (_spawningPlayers.Contains(player))
                {
                    _log.Log($"[PlayerSpawner] Player {player.PlayerId} is already spawning, skipping");
                    continue;
                }

                var spawnPoint = _spawnPointsProvider.GetRandomPoint;
                if (spawnPoint == null)
                {
                    _log.LogError($"[PlayerSpawner] Failed to get spawn point for player {player.PlayerId}");
                    continue;
                }

                _log.Log($"[PlayerSpawner] Spawning player {player.PlayerId} at {spawnPoint.position}, " +
                         $"IsServer: {_runnerProvider.Runner.IsServer}, " +
                         $"IsClient: {_runnerProvider.Runner.IsClient}, " +
                         $"IsConnectedToServer: {_runnerProvider.Runner.IsConnectedToServer}");

                _pendingPlayers.Remove(player);
                _spawningPlayers.Add(player);
                _networkSpawner.SpawnPlayer(player, spawnPoint.position, spawnPoint.rotation, OnSpawnedPlayer);
            }
        }

        private bool IsReadyToSpawn()
        {
            var runnerNotNull = _runnerProvider.Runner != null;
            var isRunning = _runnerProvider.Runner?.IsRunning ?? false;
            var isServer = _runnerProvider.Runner?.IsServer ?? false;

            _log.Log($"[PlayerSpawner] Ready check: " +
                     $"SceneLoaded={_sceneLoaded}, " +
                     $"SpawnPointsReady={_spawnPointsReady}, " +
                     $"Provider={_isProviderReady}, " +
                     $"RunnerNotNull={runnerNotNull}, " +
                     $"IsRunning={isRunning}, " +
                     $"IsServer={isServer}");

            return _sceneLoaded && _spawnPointsReady && _isProviderReady && runnerNotNull && isRunning && isServer;
        }

        private void OnSpawnedPlayer(NetworkPlayer player)
        {
            if (player == null)
            {
                _log.LogError("[PlayerSpawner] Failed to spawn player");
                return;
            }

            var playerRef = player.Object.InputAuthority;
            _spawningPlayers.Remove(playerRef);

            try
            {
                player.Initialize(_inputService, _messageService, _assetProvider);

                _log.Log($"[PlayerSpawner] Player {playerRef.PlayerId} initialized successfully on server");
                OnPlayerSpawned?.Invoke(playerRef);
            }
            catch (System.Exception e)
            {
                _log.LogError($"[PlayerSpawner] Error initializing player {playerRef.PlayerId}: {e.Message}");

                // Возвращаем игрока в очередь на спавн
                _pendingPlayers.Add(playerRef);
            }
        }

        public void Dispose()
        {
            _messageService.Unsubscribe<PlayerConnectedMessage>(OnPlayerConnected);
            _messageService.Unsubscribe<SceneLoadedMessage>(OnSceneLoaded);
            _messageService.Unsubscribe<SpawnPointsReadyMessage>(OnSpawnPointsReady);
            _messageService.Unsubscribe<PlayerDisconnectedMessage>(OnPlayerDisconnected);
            _messageService.Unsubscribe<NetworkRunnerReadyMessage>(OnNetworkRunnerReady);
        }
    }
}