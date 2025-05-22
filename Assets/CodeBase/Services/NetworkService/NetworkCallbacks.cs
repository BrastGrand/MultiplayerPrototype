using CodeBase.Infrastructure.StateMachine;
using CodeBase.Services.GameMode;
using CodeBase.Services.InputService;
using CodeBase.Services.Message;
using CodeBase.Services.PlayerSpawnerService;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using Zenject;

namespace CodeBase.Services.NetworkService
{
    public class NetworkCallbacks : MonoBehaviour, INetworkRunnerCallbacks
    {
        private IMessageService _messageService;
        private IGameModeService _modeService;
        private INetworkInputService _inputService;
        private PlayerSpawner _playerSpawner;
        private GameStateMachine _gameStateMachine;

        [Inject]
        public void Construct(
            IMessageService messageService,
            IGameModeService modeService,
            INetworkInputService inputService,
            GameStateMachine gameStateMachine)
        {
            _messageService = messageService;
            _modeService = modeService;
            _inputService = inputService;
            _gameStateMachine = gameStateMachine;
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[NetworkCallbacks] OnPlayerJoined: {player.PlayerId}");
            _messageService.Publish(
                new PlayerConnectedMessage { PlayerRef = player, IsHost = _modeService.IsHost }
            );
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("[NetworkCallbacks] OnSceneLoadDone");
            var sceneName = runner.SceneManager?.MainRunnerScene.name ?? "Unknown";
            _messageService.Publish(new SceneLoadedMessage(sceneName));
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (_inputService == null)
            {
                return;
            }

            var data = _inputService.GetInput();
            input.Set(data);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            Debug.LogWarning($"[NetworkCallbacks] Input missing for player {player.PlayerId}");
            
            // Предоставляем пустой ввод для непредвиденных случаев
            var emptyInput = new NetworkInputData();
            input.Set(emptyInput);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[NetworkCallbacks] OnPlayerLeft: {player.PlayerId}");

            var playerObject = runner.GetPlayerObject(player);
            if (playerObject != null)
            {
                runner.Despawn(playerObject);
            }

            _messageService.Publish(new PlayerDisconnectedMessage { PlayerRef = player });
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("[NetworkCallbacks] OnConnectedToServer");
            // Активируем ввод, когда клиент подключается к серверу
            _inputService?.Enable();
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.Log($"[NetworkCallbacks] OnDisconnectedFromServer: {reason}");
            // Отключаем ввод при разрыве соединения
            _inputService?.Disable();
        }

        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (shutdownReason == ShutdownReason.DisconnectedByPluginLogic)
            {
                _messageService.Publish(new HostDisconnectMessage());
                _gameStateMachine.Enter<GameMenuState>();
            }
        }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    }
}