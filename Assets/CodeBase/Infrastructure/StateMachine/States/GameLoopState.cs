using CodeBase.Services.GameMode;
using CodeBase.Services.Log;
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

        public UniTask Enter()
        {
            _log.Log($"Game started as {(_modeService.IsHost ? "Host" : "Client")}");
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            _log.Log("Game exit");
            _playerSpawner.OnPlayerSpawned -= OnPlayerSpawned;
            return UniTask.CompletedTask;
        }
    }
}