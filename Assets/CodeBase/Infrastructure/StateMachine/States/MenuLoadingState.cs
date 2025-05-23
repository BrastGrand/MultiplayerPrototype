using CodeBase.Infrastructure.SceneManagement;
using CodeBase.Services.UIService;
using UnityEngine;

namespace CodeBase.Infrastructure.StateMachine
{
    public class MenuLoadingState : LoadingState
    {
        protected override string TargetScene => "MenuScene";

        public MenuLoadingState(
            GameStateMachine stateMachine,
            ISceneLoader sceneLoader,
            IUIFactory uiFactory)
            : base(stateMachine, sceneLoader, uiFactory) { }

        protected override void OnLoaded()
        {
            StateMachine.Enter<GameMenuState>();
        }

        protected override void OnLoadFailed()
        {
            Application.Quit();
        }
    }
}