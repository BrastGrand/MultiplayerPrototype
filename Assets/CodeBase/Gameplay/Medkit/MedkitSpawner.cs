using CodeBase.Services.NetworkService;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using Zenject;
using System.Collections.Generic;

namespace CodeBase.Gameplay.Medkit
{
    public class MedkitSpawner : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkPrefabRef _medkitPrefab;
        [SerializeField] private Transform[] _spawnPoints;

        private NetworkRunnerProvider _runnerProvider;
        private NetworkRunner _runner;
        private bool _medkitsSpawned = false;

        [Inject]
        public void Construct(NetworkRunnerProvider runnerProvider)
        {
            _runnerProvider = runnerProvider;

            Debug.Log($"[MedkitSpawner] Construct called. Runner ready: {runnerProvider.Runner != null}");

            if (runnerProvider.Runner != null)
            {
                _runner = runnerProvider.Runner;
                
                if (_runner.IsServer)
                {
                    // Подписываемся на события подключения игроков
                    _runner.AddCallbacks(this);
                    Debug.Log("[MedkitSpawner] Server ready, subscribed to player join events");
                }
                else
                {
                    Debug.Log("[MedkitSpawner] Client - waiting for medkits from server");
                }
            }
            else
            {
                Debug.Log("[MedkitSpawner] Runner not ready yet");
            }
        }

        // Вызывается когда игрок подключается
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            // Если это сервер и медкиты ещё не заспавнены
            if ((runner.IsServer) && !_medkitsSpawned)
            {
                Debug.Log($"[MedkitSpawner] Player {player} joined, spawning medkits now");
                Invoke(nameof(DelayedSpawnMedkits), 2f); // 2 секунды задержки для стабильности
            }
        }

        private void DelayedSpawnMedkits()
        {
            if (_medkitsSpawned) return; // Проверяем что не заспавнили уже
            
            SpawnMedkits();
        }

        private void SpawnMedkits()
        {
            if (_runner == null || !(_runner.IsServer))
            {
                return;
            }

            if (_medkitsSpawned)
            {
                return;
            }

            _medkitsSpawned = true;

            foreach (var spawnPoint in _spawnPoints)
            {
                SpawnMedkit(spawnPoint);
            }
        }

        private async void SpawnMedkit(Transform spawnPoint)
        {
            if (_runner == null) return;

            if (!(_runner.IsServer))
            {
                return;
            }

            if (!_medkitPrefab.IsValid)
            {
                return;
            }

            try
            {
                _runner.Spawn(_medkitPrefab, spawnPoint.position, spawnPoint.rotation);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MedkitSpawner] Exception while spawning medkit: {e.Message}");
            }
        }

        private void OnDestroy()
        {
            if (_runner != null)
            {
                _runner.RemoveCallbacks(this);
            }
            CancelInvoke();
        }

        // Остальные методы INetworkRunnerCallbacks (пустые, нам нужен только OnPlayerJoined)
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    }
}