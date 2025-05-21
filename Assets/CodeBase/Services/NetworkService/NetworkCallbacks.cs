using CodeBase.Services.GameMode;
using CodeBase.Services.InputService;
using CodeBase.Services.Message;
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

        [Inject]
        public void Construct(IMessageService messageService, IGameModeService modeService, INetworkInputService inputService)
        {
            _messageService = messageService;
            _modeService = modeService;
            _inputService = inputService;
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[NetworkCallbacks] OnPlayerJoined: {player.PlayerId}");
            _messageService.Publish<PlayerConnectedMessage>(
                new PlayerConnectedMessage { PlayerRef = player, IsHost = _modeService.IsHost }
            );
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("[NetworkCallbacks] OnSceneLoadDone");
            var sceneName = runner.SceneManager?.MainRunnerScene.name ?? "Unknown";
            _messageService.Publish<SceneLoadedMessage>(new SceneLoadedMessage(sceneName));
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (_inputService == null)
            {
                Debug.LogError("[NetworkCallbacks] InputService is null in OnInput");
                return;
            }

            try
            {
                var data = _inputService.GetInput();
                
                // Подробное логирование для входных данных
                if (data.MoveInput.sqrMagnitude > 0.01f || data.Jump)
                {
                    Debug.Log($"[NetworkCallbacks] Sending input: Move=({data.MoveX:F2}, {data.MoveY:F2}), Jump={data.Jump}, Local Player: {runner.LocalPlayer.PlayerId}");
                }
                
                input.Set(data);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[NetworkCallbacks] Error in OnInput: {ex.Message}\n{ex.StackTrace}");
            }
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
            _messageService.Publish<PlayerDisconnectedMessage>(
                new PlayerDisconnectedMessage { PlayerRef = player }
            );
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
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
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