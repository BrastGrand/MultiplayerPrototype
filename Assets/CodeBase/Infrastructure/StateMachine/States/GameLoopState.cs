using CodeBase.Services.Log;
using CodeBase.Services.NetworkGameMode;
using CodeBase.Services.PlayerSpawnerService;
using Cysharp.Threading.Tasks;
using Fusion;

namespace CodeBase.Infrastructure.StateMachine
{
    public class GameLoopState : IState
    {
        private readonly IPlayerSpawner _playerSpawner;
        private readonly ILogService _log;
        private readonly IGameModeService _modeService;
        private bool _isInitialized;
        private bool _isExiting;

        public GameLoopState(
            IPlayerSpawner playerSpawner, 
            IGameModeService modeService,
            ILogService logService)
        {
            _playerSpawner = playerSpawner;
            _modeService = modeService;
            _log = logService;
            _playerSpawner.OnPlayerSpawned += OnPlayerSpawned;
        }

        private void OnPlayerSpawned(PlayerRef player)
        {
            _log.Log($"Player {player.PlayerId} spawned");
        }

        public async UniTask Enter()
        {
            if (_isInitialized)
            {
                _log.Log("GameLoopState already initialized, skipping");
                return;
            }

            if (_isExiting)
            {
                _log.Log("GameLoopState is exiting, skipping initialization");
                return;
            }

            _isInitialized = true;
            _log.Log($"Game started as {(_modeService.IsHost ? "Host" : "Client")}");
        }

        public async UniTask Exit()
        {
            if (!_isInitialized)
            {
                _log.Log("GameLoopState not initialized, skipping exit");
                return;
            }

            _isExiting = true;
            _log.Log("Game exit");
            _playerSpawner.OnPlayerSpawned -= OnPlayerSpawned;
            _isInitialized = false;
            _isExiting = false;
        }
    }
}