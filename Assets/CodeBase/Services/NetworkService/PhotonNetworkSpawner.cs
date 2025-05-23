using System;
using CodeBase.Gameplay.Player;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.Log;
using Fusion;
using UnityEngine;
using Zenject;

namespace CodeBase.Services.NetworkService
{
    public class PhotonNetworkSpawner : INetworkObjectSpawner
    {
        private NetworkRunnerProvider _runnerProvider;
        private IAssetProvider _assetProvider;
        private ILogService _log;
        private DiContainer _container;

        [Inject]
        public void Construct(
            NetworkRunnerProvider runnerProvider,
            IAssetProvider assetProvider,
            ILogService log,
            DiContainer container)
        {
            _runnerProvider = runnerProvider;
            _assetProvider = assetProvider;
            _log = log;
            _container = container;
        }

        public async void SpawnPlayer(PlayerRef player, Vector3 position, Quaternion rotation, Action<NetworkPlayer> onSpawnedPlayer)
        {
            _log.Log($"SpawnPlayer called for player {player}, IsServer: {_runnerProvider.Runner.IsServer}, " +
                     $"IsConnectedToServer: {_runnerProvider.Runner.IsConnectedToServer}, " +
                     $"IsRunning: {_runnerProvider.Runner.IsRunning}");

            if (_runnerProvider.Runner == null)
            {
                _log.LogError("Runner is null");
                onSpawnedPlayer?.Invoke(null);
                return;
            }

            // Для Host проверяем IsServer, для Client проверяем IsConnectedToServer
            var isConnected = _runnerProvider.Runner.IsServer || _runnerProvider.Runner.IsConnectedToServer;
            if (!isConnected)
            {
                _log.LogError($"Runner is not connected - IsServer: {_runnerProvider.Runner.IsServer}, IsConnectedToServer: {_runnerProvider.Runner.IsConnectedToServer}");
                onSpawnedPlayer?.Invoke(null);
                return;
            }

            if (!_runnerProvider.Runner.IsRunning)
            {
                _log.LogError("Runner is not running");
                onSpawnedPlayer?.Invoke(null);
                return;
            }

            if (!_runnerProvider.Runner.SceneManager?.MainRunnerScene.IsValid() ?? true)
            {
                _log.LogError("Runner scene is not valid");
                onSpawnedPlayer?.Invoke(null);
                return;
            }

            try
            {
                _log.Log("Loading player prefab from Addressables...");
                var playerPrefab = await _assetProvider.Load<GameObject>(AssetLabels.PLAYER);
                if (playerPrefab == null)
                {
                    _log.LogError("Failed to load player prefab from Addressables");
                    onSpawnedPlayer?.Invoke(null);
                    return;
                }
                _log.Log("Player prefab loaded successfully");

                var playerComponent = playerPrefab.GetComponent<NetworkPlayer>();
                if (playerComponent == null)
                {
                    _log.LogError("Player prefab doesn't have NetworkPlayer component");
                    onSpawnedPlayer?.Invoke(null);
                    return;
                }

                var spawnedPlayer = _runnerProvider.Runner.Spawn(playerComponent, position, rotation, player);
                
                _container.InjectGameObject(spawnedPlayer.gameObject);

                if (spawnedPlayer != null)
                {
                    _log.Log($"Player {player} spawned successfully. Parent: {(spawnedPlayer.transform.parent ? spawnedPlayer.transform.parent.name : "None")}");
                    onSpawnedPlayer?.Invoke(spawnedPlayer);
                }
                else
                {
                    _log.LogError($"Failed to spawn player {player}");
                    onSpawnedPlayer?.Invoke(null);
                }
            }
            catch (Exception e)
            {
                _log.LogError($"Error spawning player {player}: {e.Message}\n{e.StackTrace}");
                onSpawnedPlayer?.Invoke(null);
            }
        }

        public async void SpawnNetworkObject(string prefabKey, Vector3 position)
        {
            try
            {
                var prefab = await _assetProvider.Load<GameObject>(prefabKey);
                if (prefab == null)
                {
                    _log.LogError($"Failed to load prefab: {prefabKey}");
                    return;
                }

                var networkObject = prefab.GetComponent<NetworkObject>();
                if (networkObject == null)
                {
                    _log.LogError($"Prefab {prefabKey} does not have NetworkObject component");
                    return;
                }

                _runnerProvider.Runner.Spawn(networkObject, position, Quaternion.identity);
            }
            catch (Exception e)
            {
                _log.LogError($"Error spawning network object: {e.Message}");
            }
        }
    }
}