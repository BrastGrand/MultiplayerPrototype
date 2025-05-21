using System.Threading.Tasks;
using CodeBase.Gameplay;
using CodeBase.Infrastructure.SceneManagement;
using CodeBase.Services.Message;
using CodeBase.Services.NetworkService;
using CodeBase.Services.UIService;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.StateMachine
{
    public class GameLoadingState : LoadingState
    {
        private readonly INetworkService _networkService;
        private readonly IGameplayReadyNotifier _readyNotifier;
        private readonly IMessageService _messageService;
        private readonly TaskCompletionSource<bool> _sceneLoadCompletionSource = new TaskCompletionSource<bool>();
        
        protected override string TargetScene => "Game";

        public GameLoadingState(
            GameStateMachine stateMachine,
            ISceneLoader sceneLoader,
            IUIFactory uiFactory,
            INetworkService networkService,
            IGameplayReadyNotifier readyNotifier,
            IMessageService messageService)
            : base(stateMachine, sceneLoader, uiFactory)
        {
            _networkService = networkService;
            _readyNotifier = readyNotifier;
            _messageService = messageService;
            
            // Подписываемся на сообщение о загрузке сцены
            _messageService.Subscribe<SceneLoadedMessage>(OnSceneLoaded);
        }

        private void OnSceneLoaded(SceneLoadedMessage message)
        {
            _sceneLoadCompletionSource.TrySetResult(true);
        }

        protected override async void OnLoaded()
        {
            await _readyNotifier.WaitUntilReady();
            await _sceneLoadCompletionSource.Task;
            await StateMachine.Enter<GameLoopState>();
        }

        protected override void OnLoadFailed()
        {
            _networkService.Disconnect();
            StateMachine.Enter<GameMenuState>();
        }
        
        public override async UniTask Exit()
        {
            await base.Exit();
            _messageService.Unsubscribe<SceneLoadedMessage>(OnSceneLoaded);
        }
    }
}