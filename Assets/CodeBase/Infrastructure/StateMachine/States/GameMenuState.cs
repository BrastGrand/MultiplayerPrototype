using System;
using CodeBase.Services.NetworkService;
using CodeBase.Services.UIService;
using CodeBase.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.StateMachine
{
    public class GameMenuState : IState, IDisposable
    {
        private readonly GameStateMachine _stateMachine;
        private readonly INetworkService _networkService;
        private readonly IUIFactory _uiFactory;

        private const string _SCREEN_NAME = "MenuScreen";
        private MenuScreen _menuScreen;

        public GameMenuState(GameStateMachine stateMachine, INetworkService networkService, IUIFactory uiFactory)
        {
            _stateMachine = stateMachine;
            _networkService = networkService;
            _uiFactory = uiFactory;
        }

        public async UniTask Enter()
        {
            _menuScreen = await _uiFactory.CreateScreen<MenuScreen>(_SCREEN_NAME);
            _menuScreen.OnHostClicked += OnHostClicked;
            _menuScreen.OnJoinClicked += OnJoinClicked;
            _menuScreen.OnQuitClicked += OnQuitClicked;
            _menuScreen.Show();
        }

        public UniTask Exit()
        {
            _menuScreen?.Hide();
            _uiFactory.CloseScreen<MenuScreen>();
            return UniTask.CompletedTask;
        }

        private async void OnHostClicked()
        {
            _menuScreen.SetStatus("Creating game...");
            _menuScreen.SetButtonsInteractable(false);

            try
            {
                await StartHost();
            }
            catch (Exception e)
            {
                _menuScreen.SetStatus($"Error: {e.Message}", true);
                _menuScreen.SetButtonsInteractable(true);
            }
        }

        private async void OnJoinClicked()
        {
            _menuScreen.SetStatus("Joining game...");
            _menuScreen.SetButtonsInteractable(false);

            try
            {
                await StartClient();
            }
            catch (Exception e)
            {
                _menuScreen.SetStatus($"Error: {e.Message}", true);
                _menuScreen.SetButtonsInteractable(true);
            }
        }

        private void OnQuitClicked()
        {
            Application.Quit();
        }

        private async UniTask StartHost()
        {
            await _networkService.StartHost();
            await _stateMachine.Enter<GameLoadingState>();
            _menuScreen?.Hide(0);
        }

        private async UniTask StartClient()
        {
            await _networkService.StartClient();
            await _stateMachine.Enter<GameLoadingState>();
            _menuScreen?.Hide(0);
        }

        public void Dispose() => Exit();
    }
}