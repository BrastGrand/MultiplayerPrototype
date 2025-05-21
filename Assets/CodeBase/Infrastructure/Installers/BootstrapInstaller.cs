using CodeBase.Gameplay;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.SceneManagement;
using CodeBase.Infrastructure.StateMachine;
using CodeBase.Services.GameMode;
using CodeBase.Services.InputService;
using CodeBase.Services.Log;
using CodeBase.Services.Message;
using CodeBase.Services.NetworkService;
using CodeBase.Services.PlayerSpawnerService;
using CodeBase.Services.UIService;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    public class BootstrapInstaller : MonoInstaller
    {
        [SerializeField] private InputReader _inputReaderPrefab;
        [SerializeField] private GameObject _gameBootstrapperPrefab;
        
        public override void InstallBindings()
        {
            BindServices();
            BindNetwork();
            BindStateMachine();
            InstantiateGameBootstrapper();
        }

        private void InstantiateGameBootstrapper()
        {
            var bootstrapper = Container.InstantiatePrefab(_gameBootstrapperPrefab).GetComponent<GameBootstrapper>();
            Container.BindInstance(bootstrapper).AsSingle(); // если нужен доступ к нему через DI
        }

        private void BindServices()
        {
            Container.Bind<ILogService>().To<LogService>().AsSingle();
            Container.Bind<IMessageService>().To<MessageService>().AsSingle();
            Container.Bind<IGameModeService>().To<GameModeService>().AsSingle();
            Container.Bind<INetworkObjectSpawner>().To<PhotonNetworkSpawner>().AsSingle();
            Container.Bind<IGameplayReadyNotifier>().To<GameplayReadyNotifier>().AsSingle();
            Container.Bind<INetworkInputService>().To<NetworkInputService>().AsSingle();
            Container.Bind<IAssetProvider>().To<AssetProvider>().AsSingle();
            
            if (_inputReaderPrefab != null)
                Container.Bind<IInputReader>().FromInstance(_inputReaderPrefab).AsSingle();
            else
                Container.Bind<IInputReader>().FromComponentInNewPrefabResource("InputReader").AsSingle();
                
            Container.Bind<IUIFactory>().To<UIFactory>().AsSingle();
            Container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();
            Container.Bind<IPlayerSpawner>().To<PlayerSpawner>().AsSingle();
        }

        private void BindNetwork()
        {
            Container.Bind<INetworkService>().To<PhotonNetworkService>().AsSingle();
            Container.Bind<NetworkRunnerProvider>().AsSingle();
        }
        
        private void BindStateMachine()
        {
            Container.Bind<GameStateMachine>().AsSingle();
            Container.Bind<StatesFactory>().AsSingle();
        }
    }
}