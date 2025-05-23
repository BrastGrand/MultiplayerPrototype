using System.Threading.Tasks;
using CodeBase.Gameplay;
using CodeBase.Infrastructure.SceneManagement;
using CodeBase.Services.Log;
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
        private readonly GameStateMachine _stateMachine;
        private readonly ILogService _log;
        private bool _isSceneLoaded;
        
        protected override string TargetScene => "GameScene";

        public GameLoadingState(
            GameStateMachine stateMachine,
            ISceneLoader sceneLoader,
            IUIFactory uiFactory,
            INetworkService networkService,
            IGameplayReadyNotifier readyNotifier,
            IMessageService messageService,
            ILogService log)
            : base(stateMachine, sceneLoader, uiFactory)
        {
            _networkService = networkService;
            _readyNotifier = readyNotifier;
            _messageService = messageService;
            _stateMachine = stateMachine;
            _log = log;
            
            _messageService.Subscribe<SceneLoadedMessage>(OnSceneLoaded);
        }

        private void OnSceneLoaded(SceneLoadedMessage message)
        {
            if (_isSceneLoaded)
            {
                _log.Log("Scene already loaded, ignoring duplicate message");
                return;
            }

            _isSceneLoaded = true;
            _sceneLoadCompletionSource.TrySetResult(true);
        }

        protected override async void OnLoaded()
        {
            try
            {
                if (!_isSceneLoaded)
                {
                    await _sceneLoadCompletionSource.Task;
                }
                await _readyNotifier.WaitUntilReady();
                await _stateMachine.Enter<GameLoopState>();
            }
            catch (System.Exception e)
            {
                _log.Log($"Error during game loading: {e.Message}");
                OnLoadFailed();
            }
        }

        protected override void OnLoadFailed()
        {
            _log.Log("Game loading failed, disconnecting and returning to menu");
            _networkService.Disconnect();
            StateMachine.Enter<GameMenuState>();
        }
        
        public override async UniTask Exit()
        {
            _log.Log("Game loading state exit");
            await base.Exit();
            _messageService.Unsubscribe<SceneLoadedMessage>(OnSceneLoaded);
            _isSceneLoaded = false;
        }
    }
}