using CodeBase.Services.NetworkService;

namespace CodeBase.Services.GameMode
{
    public class GameModeService : IGameModeService
    {
        private readonly NetworkRunnerProvider _runnerProvider;

        public GameModeService(NetworkRunnerProvider runnerProvider)
        {
            _runnerProvider = runnerProvider;
        }

        public bool IsHost => _runnerProvider.Runner?.IsServer ?? false;
        public bool IsClient => !IsHost;
    }
}