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
            await _menuScreen.Show();
        }

        public async UniTask Exit()
        {
            if (_menuScreen != null)
            {
                await _menuScreen.Hide();
                _uiFactory.CloseScreen<MenuScreen>();
            }
        }

        private void OnHostClicked()
        {
            _menuScreen.SetStatus("Creating game...");
            _menuScreen.SetButtonsInteractable(false);

            try
            {
                StartHost();
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

        private async void StartHost()
        {
            if (_menuScreen != null)
            {
                _menuScreen.SetStatus("Creating game...");
                _menuScreen.SetButtonsInteractable(false);
            }

            await _networkService.StartHost();
            
            // Сохраняем ссылку на экран перед уничтожением
            var screen = _menuScreen;
            _menuScreen = null;
            
            if (screen != null)
            {
                await screen.Hide();
                _uiFactory.CloseScreen<MenuScreen>();
            }
            
            await _stateMachine.Enter<GameLoadingState>();
        }

        private async UniTask StartClient()
        {
            if (_menuScreen != null)
            {
                _menuScreen.SetStatus("Joining game...");
                _menuScreen.SetButtonsInteractable(false);
            }

            await _networkService.StartClient();
            
            // Сохраняем ссылку на экран перед уничтожением
            var screen = _menuScreen;
            _menuScreen = null;
            
            if (screen != null)
            {
                await screen.Hide();
                _uiFactory.CloseScreen<MenuScreen>();
            }
            
            await _stateMachine.Enter<GameLoadingState>();
        }

        public void Dispose() => Exit().Forget();
    }
}