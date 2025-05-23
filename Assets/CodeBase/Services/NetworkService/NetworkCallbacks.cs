using CodeBase.Infrastructure.StateMachine;
using CodeBase.Services.InputService;
using CodeBase.Services.Message;
using CodeBase.Services.NetworkGameMode;
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
        private GameStateMachine _gameStateMachine;
        private NetworkRunnerProvider _runnerProvider;
        private bool _isInitialized;

        [Inject]
        public void Construct(
            IMessageService messageService,
            IGameModeService modeService,
            INetworkInputService inputService,
            GameStateMachine gameStateMachine,
            NetworkRunnerProvider runnerProvider)
        {
            _messageService = messageService;
            _modeService = modeService;
            _inputService = inputService;
            _gameStateMachine = gameStateMachine;
            _runnerProvider = runnerProvider;
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            _isInitialized = false;
        }

        private void ValidateDependencies()
        {
            if (!_isInitialized)
            {
                return;
            }

            var missingDependencies = new System.Collections.Generic.List<string>();

            if (_messageService == null) missingDependencies.Add("MessageService");
            if (_modeService == null) missingDependencies.Add("ModeService");
            if (_inputService == null) missingDependencies.Add("InputService");
            if (_gameStateMachine == null) missingDependencies.Add("GameStateMachine");
            if (_runnerProvider == null) missingDependencies.Add("RunnerProvider");

            if (missingDependencies.Count > 0)
            {
                Debug.LogError($"[NetworkCallbacks] Missing dependencies: {string.Join(", ", missingDependencies)}");
            }
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            ValidateDependencies();
            if (!_isInitialized) return;

            Debug.Log($"[NetworkCallbacks] OnPlayerJoined: {player.PlayerId}, " +
                      $"IsHost: {_modeService.IsHost}, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}, " +
                      $"Scene: {runner.SceneManager?.MainRunnerScene.name ?? "Unknown"}");

            _messageService.Publish(new PlayerConnectedMessage { PlayerRef = player, IsHost = _modeService.IsHost });
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            ValidateDependencies();
            var sceneName = runner.SceneManager?.MainRunnerScene.name ?? "Unknown";
            Debug.Log($"[NetworkCallbacks] OnSceneLoadStart: {sceneName}, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}");
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            ValidateDependencies();
            var sceneName = runner.SceneManager?.MainRunnerScene.name ?? "Unknown";

            Debug.Log($"[NetworkCallbacks] OnSceneLoadDone: {sceneName}, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}");

            _messageService.Publish(new SceneLoadedMessage(sceneName));
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            ValidateDependencies();
            if (_inputService == null)
            {
                Debug.LogWarning("[NetworkCallbacks] Input service is null");
                return;
            }

            var data = _inputService.GetInput();
            input.Set(data);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            ValidateDependencies();
            Debug.LogWarning($"[NetworkCallbacks] Input missing for player {player.PlayerId}, " +
                             $"IsServer: {runner.IsServer}, " +
                             $"IsClient: {runner.IsClient}");

            var emptyInput = new NetworkInputData();
            input.Set(emptyInput);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            ValidateDependencies();
            Debug.Log($"[NetworkCallbacks] OnPlayerLeft: {player.PlayerId}, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}");

            var playerObject = runner.GetPlayerObject(player);
            if (playerObject != null)
            {
                runner.Despawn(playerObject);
            }

            _messageService.Publish(new PlayerDisconnectedMessage { PlayerRef = player });
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log($"[NetworkCallbacks] OnConnectedToServer called," +
                      $" IsInitialized: {_isInitialized}, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}, " +
                      $"IsRunning: {runner.IsRunning}, " +
                      $"Scene: {runner.SceneManager?.MainRunnerScene.name ?? "None"}");

            if (!_isInitialized)
            {
                Debug.LogWarning("[NetworkCallbacks] OnConnectedToServer called before initialization");
                return;
            }

            ValidateDependencies();
            Debug.Log($"[NetworkCallbacks] OnConnectedToServer, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}, " +
                      $"Scene: {runner.SceneManager?.MainRunnerScene.name ?? "Unknown"}");

            _inputService?.Enable();
            _messageService.Publish(new NetworkRunnerReadyMessage(runner));
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            ValidateDependencies();
            Debug.Log($"[NetworkCallbacks] OnDisconnectedFromServer: {reason}, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}, " +
                      $"Scene: {runner.SceneManager?.MainRunnerScene.name ?? "Unknown"}");

            _inputService?.Disable();
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            ValidateDependencies();
            Debug.Log($"[NetworkCallbacks] OnShutdown: {shutdownReason}, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}, " +
                      $"Scene: {runner.SceneManager?.MainRunnerScene.name ?? "Unknown"}");

            switch (shutdownReason)
            {
                case ShutdownReason.DisconnectedByPluginLogic:
                case ShutdownReason.Ok:
                case ShutdownReason.IncompatibleConfiguration:
                case ShutdownReason.ServerInRoom:
                case ShutdownReason.GameClosed:
                case ShutdownReason.GameNotFound:
                case ShutdownReason.MaxCcuReached:
                case ShutdownReason.InvalidRegion:
                case ShutdownReason.GameIdAlreadyExists:
                case ShutdownReason.GameIsFull:
                case ShutdownReason.InvalidAuthentication:
                case ShutdownReason.CustomAuthenticationFailed:
                case ShutdownReason.AuthenticationTicketExpired:
                case ShutdownReason.PhotonCloudTimeout:
                    _messageService.Publish(new HostDisconnectMessage());
                    _gameStateMachine.Enter<GameMenuState>();
                    break;
                default:
                    Debug.LogWarning($"[NetworkCallbacks] Unhandled shutdown reason: {shutdownReason}");
                    break;
            }
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            ValidateDependencies();
            Debug.Log($"[NetworkCallbacks] OnConnectRequest from {request.RemoteAddress}, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}");
            request.Accept();
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.LogError(
                $"[NetworkCallbacks] OnConnectFailed: {reason}, IsServer: {runner.IsServer}, IsClient: {runner.IsClient}, IsRunning: {runner.IsRunning}, Scene: {runner.SceneManager?.MainRunnerScene.name ?? "None"}");
            _messageService.Publish(new NetworkRunnerFailedMessage(runner, reason));
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            ValidateDependencies();
            Debug.Log($"[NetworkCallbacks] OnHostMigration, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}, " +
                      $"Scene: {runner.SceneManager?.MainRunnerScene.name ?? "Unknown"}");
        }

        public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList)
        {
            ValidateDependencies();
            Debug.Log($"[NetworkCallbacks] OnSessionListUpdated: {sessionList.Count} sessions, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}");
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data)
        {
            ValidateDependencies();
            Debug.Log($"[NetworkCallbacks] OnCustomAuthenticationResponse, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}");
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data)
        {
            ValidateDependencies();
            Debug.Log($"[NetworkCallbacks] OnReliableDataReceived from player {player.PlayerId}, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}");
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            ValidateDependencies();
            Debug.Log($"[NetworkCallbacks] OnReliableDataProgress from player {player.PlayerId}: {progress}, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}");
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            ValidateDependencies();
            Debug.Log($"[NetworkCallbacks] OnUserSimulationMessage, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}");
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            ValidateDependencies();
            Debug.Log($"[NetworkCallbacks] OnObjectEnterAOI: {obj.Id} for player {player.PlayerId}, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}, " +
                      $"Scene: {runner.SceneManager?.MainRunnerScene.name ?? "Unknown"}");
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            ValidateDependencies();
            Debug.Log($"[NetworkCallbacks] OnObjectExitAOI: {obj.Id} for player {player.PlayerId}, " +
                      $"IsServer: {runner.IsServer}, " +
                      $"IsClient: {runner.IsClient}, " +
                      $"Scene: {runner.SceneManager?.MainRunnerScene.name ?? "Unknown"}");
        }
    }
}