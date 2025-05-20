using CodeBase.Gameplay;
using CodeBase.Infrastructure.SceneManagement;
using CodeBase.Services.NetworkService;
using CodeBase.Services.UIService;

namespace CodeBase.Infrastructure.StateMachine
{
    public class GameLoadingState : LoadingState
    {
        private readonly INetworkService _networkService;
        private readonly IGameplayReadyNotifier _readyNotifier;
        private readonly INetworkCallbacksService _networkCallbacks;
        protected override string TargetScene => "Game";

        public GameLoadingState(
            GameStateMachine stateMachine,
            ISceneLoader sceneLoader,
            IUIFactory uiFactory,
            INetworkService networkService,
            IGameplayReadyNotifier readyNotifier,
            INetworkCallbacksService networkCallbacks)
            : base(stateMachine, sceneLoader, uiFactory)
        {
            _networkService = networkService;
            _readyNotifier = readyNotifier;
            _networkCallbacks = networkCallbacks;
        }

        protected override async void OnLoaded()
        {
            await _readyNotifier.WaitUntilReady();
            await _networkCallbacks.SceneLoadCompleted;
            await StateMachine.Enter<GameLoopState>();
        }

        protected override void OnLoadFailed()
        {
            _networkService.Disconnect();
            StateMachine.Enter<GameMenuState>();
        }
    }
}