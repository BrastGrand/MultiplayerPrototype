using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Services.MessageService;
using CodeBase.Services.MessageService.Messages;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace CodeBase.Services.NetworkService
{
    public class NetworkCallbacks : INetworkRunnerCallbacks, INetworkCallbacksService
    {
        private readonly IMessageService _messageService;
        private readonly HashSet<PlayerRef> _pendingPlayers = new HashSet<PlayerRef>();
        private readonly TaskCompletionSource<bool> _sceneLoadTaskCompleteSource;

        public Task SceneLoadCompleted => _sceneLoadTaskCompleteSource.Task;

        public NetworkCallbacks(IMessageService messageService)
        {
            _messageService = messageService;
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
            Debug.Log($"Publishing PlayerConnected after scene load. {runner.IsServer}");

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

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
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

        public void OnInput(NetworkRunner runner, NetworkInput input)
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