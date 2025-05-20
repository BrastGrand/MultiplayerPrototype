using System;
using CodeBase.Infrastructure.SceneManagement;
using CodeBase.Services.UIService;
using CodeBase.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.StateMachine
{
    public abstract class LoadingState : IState
    {
        protected readonly GameStateMachine StateMachine;
        private readonly ISceneLoader _sceneLoader;
        private IUIFactory _uiFactory;
        private LoadingScreen _loadingScreen;
        private const string _SCREEN_NAME = "LoadingScreen";

        protected virtual float MinLoadingTime => 1.5f;
        protected abstract string TargetScene { get; }
        protected abstract void OnLoaded();
        protected abstract void OnLoadFailed();

        protected LoadingState(GameStateMachine stateMachine, ISceneLoader sceneLoader, IUIFactory uiFactory)
        {
            StateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _uiFactory = uiFactory;
        }

        public async UniTask Enter()
        {
            _loadingScreen = await _uiFactory.CreateScreen<LoadingScreen>(_SCREEN_NAME);
            _loadingScreen.Show();

            try
            {
                var loadingTask = _sceneLoader.Load(TargetScene);
                var minTimeTask = UniTask.Delay(TimeSpan.FromSeconds(MinLoadingTime));
                await UniTask.WhenAll(loadingTask, minTimeTask);
                OnLoaded();
            }
            catch (Exception e)
            {
                await UniTask.Delay(2000);
                OnLoadFailed();
            }
        }

        public UniTask Exit()
        {
            Debug.Log("Loading state exit");
            _loadingScreen?.Hide(0f);
            _uiFactory.Dispose(_SCREEN_NAME);
            return default;
        }
    }
}