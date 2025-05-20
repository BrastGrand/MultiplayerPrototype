using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.LogService;
using CodeBase.Services.MessageService;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.StateMachine
{
    public class GameBootstrapState : IState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly ILogService _logService;
        private readonly IAssetProvider _assetProvider;
        private readonly IMessageService _messageService;

        public GameBootstrapState(GameStateMachine gameStateMachine,
            IAssetProvider assetProvider,
            ILogService logService)
        {
            _gameStateMachine = gameStateMachine;
            _assetProvider = assetProvider;
            _logService = logService;
        }

        public async UniTask Enter()
        {
            _logService.Log("BootstrapState Enter");

            await InitServices();
            _gameStateMachine.Enter<MenuLoadingState>().Forget();
        }

        public async UniTask Exit()
        {
            _logService.Log("BootstrapState Exit");
        }

        private async UniTask InitServices()
        {
            await _assetProvider.InitializeAsync();
        }
    }
}