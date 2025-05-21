using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Services.InputService;
using CodeBase.Services.MessageService;
using CodeBase.Services.MessageService.Messages;
using Fusion;
using Fusion.Sockets;

namespace CodeBase.Services.NetworkService
{
    public class NetworkCallbacks : INetworkRunnerCallbacks, INetworkCallbacksService
    {
        private readonly IMessageService _messageService;
        private readonly INetworkInputService _inputService;
        private readonly HashSet<PlayerRef> _pendingPlayers = new HashSet<PlayerRef>();
        private readonly TaskCompletionSource<bool> _sceneLoadTaskCompleteSource;

        public Task SceneLoadCompleted => _sceneLoadTaskCompleteSource.Task;

        public NetworkCallbacks(IMessageService messageService, INetworkInputService inputService)
        {
            _messageService = messageService;
            _inputService = inputService;
            _sceneLoadTaskCompleteSource = new TaskCompletionSource<bool>();
        }

        public void NotifySceneLoadDone(NetworkRunner runner)
        {
            OnSceneLoadDone(runner);
        }

        public void RegisterPlayer(PlayerRef player)
        {
            if (!_pendingPlayers.Contains(player))
                _pendingPlayers.Add(player);
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                _pendingPlayers.Add(player);
            }
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {

            if (runner.IsServer)
            {
                foreach (var player in _pendingPlayers)
                {
                    _messageService.Publish(new PlayerConnectedMessage
                    {
                        PlayerRef = player,
                        IsHost = true
                    });
                }

                _pendingPlayers.Clear();
            }

            if (!_sceneLoadTaskCompleteSource.Task.IsCompleted)
                _sceneLoadTaskCompleteSource.SetResult(true);
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (runner.GameMode == GameMode.Client)
            {
                _messageService.Publish(new HostDisconnectMessage());
            }
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            if (runner.GameMode == GameMode.Client)
            {
                _messageService.Publish(new HostDisconnectMessage());
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = _inputService.GetInput();
            input.Set(data);
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }
    }
}