using System;
using CodeBase.Gameplay.Player;
using CodeBase.Infrastructure.AssetManagement;
using Fusion;
using UnityEngine;

namespace CodeBase.Services.NetworkService
{
    public class PhotonNetworkSpawner : INetworkObjectSpawner
    {
        private readonly NetworkRunnerProvider _runnerProvider;
        private readonly IAssetProvider _assetProvider;

        public PhotonNetworkSpawner(NetworkRunnerProvider runnerProvider, IAssetProvider assetProvider)
        {
            _runnerProvider = runnerProvider;
            _assetProvider = assetProvider;
        }

        public async void SpawnPlayer(PlayerRef player, Vector3 position, Quaternion rotation, Action<NetworkPlayer> onSpawnedPlayer)
        {
            Debug.Log($"SpawnPlayer called for player {player}, IsServer: {_runnerProvider.Runner?.IsServer}");

            if (_runnerProvider.Runner == null)
            {
                Debug.LogError("Runner is not initialized");
                onSpawnedPlayer?.Invoke(null);
                return;
            }

            if (_runnerProvider.Runner.IsServer)
            {
                // Сервер может спавнить напрямую
                try
                {
                    Debug.Log("Server spawning player directly");
                    var playerPrefab = await _assetProvider.Load<GameObject>(AssetLabels.PLAYER);
                    if (playerPrefab == null)
                    {
                        Debug.LogError("Failed to load player prefab");
                        onSpawnedPlayer?.Invoke(null);
                        return;
                    }

                    Debug.Log("Player prefab loaded successfully");
                    var networkObject = playerPrefab.GetComponent<NetworkObject>();
                    if (networkObject == null)
                    {
                        Debug.LogError("Player prefab does not have NetworkObject component");
                        onSpawnedPlayer?.Invoke(null);
                        return;
                    }

                    Debug.Log($"Attempting to spawn player at position {position}");
                    var spawnedObject = await _runnerProvider.Runner.SpawnAsync(networkObject, position, rotation, player);
                    if (spawnedObject == null)
                    {
                        Debug.LogError("Failed to spawn player - spawnedObject is null");
                        onSpawnedPlayer?.Invoke(null);
                        return;
                    }

                    Debug.Log("Player object spawned successfully");
                    var networkPlayer = spawnedObject.GetComponent<NetworkPlayer>();
                    if (networkPlayer == null)
                    {
                        Debug.LogError("Spawned object does not have NetworkPlayer component");
                        onSpawnedPlayer?.Invoke(null);
                        return;
                    }

                    Debug.Log($"Successfully spawned player for {player}");
                    onSpawnedPlayer?.Invoke(networkPlayer);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error spawning player: {e.Message}\n{e.StackTrace}");
                    onSpawnedPlayer?.Invoke(null);
                }
            }
            else
            {
                // Клиент должен отправить запрос на сервер через RPC
                Debug.Log("Client sending RPC request to spawn player");

                // Найти любой существующий сетевой объект для отправки RPC
                // Ищем сетевые объекты на сцене
                NetworkObject[] networkObjects = GameObject.FindObjectsOfType<NetworkObject>();
                if (networkObjects != null && networkObjects.Length > 0)
                {
                    // Берем первый объект с состоянием сервера для отправки RPC
                    foreach (var obj in networkObjects)
                    {
                        if (obj.HasStateAuthority)
                        {
                            var networkPlayer = obj.GetComponent<NetworkPlayer>();
                            if (networkPlayer != null)
                            {
                                Debug.Log($"Found server object to send RPC: {obj.Id}");
                                networkPlayer.RPC_RequestSpawnPlayer(player, position, rotation);
                                // В этом случае обратный вызов будет вызван позже, когда игрок появится
                                return;
                            }
                        }
                    }
                }

                Debug.LogError("No server network objects found to send spawn request");
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
                    Debug.LogError($"Failed to load prefab: {prefabKey}");
                    return;
                }

                var networkObject = prefab.GetComponent<NetworkObject>();
                if (networkObject == null)
                {
                    Debug.LogError($"Prefab {prefabKey} does not have NetworkObject component");
                    return;
                }

                _runnerProvider.Runner.Spawn(networkObject, position, Quaternion.identity);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error spawning network object: {e.Message}");
            }
        }
    }
}