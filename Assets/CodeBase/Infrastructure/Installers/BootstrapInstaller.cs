using CodeBase.Gameplay;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.SceneManagement;
using CodeBase.Infrastructure.StateMachine;
using CodeBase.Services.GameModeService;
using CodeBase.Services.LogService;
using CodeBase.Services.MessageService;
using CodeBase.Services.NetworkService;
using CodeBase.Services.PlayerSpawnerService;
using CodeBase.Services.UIService;
using Fusion;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    public class BootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindSceneLoader();
            BindGameStateMachine();
            BindGameBootstrapperFactory();

            Container.Bind<IGameplayReadyNotifier>().To<GameplayReadyNotifier>().AsSingle();
            Container.Bind<GameLoopStateFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<MessageService>().AsSingle();
            Container.BindInterfacesAndSelfTo<AssetProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<LogService>().AsSingle();
            Container.BindInterfacesAndSelfTo<UIFactory>().AsSingle();

            BindNetwork();
            BindSpawners();
        }

        private void BindGameBootstrapperFactory()
        {
            Container
                .BindFactory<GameBootstrapper, GameBootstrapper.Factory>()
                .FromComponentInNewPrefabResource(InfrastructureAssetPath.GAME_BOOTSTRAPPER);
        }
        private void BindSceneLoader() => Container.BindInterfacesAndSelfTo<SceneLoader>().AsSingle();
        private void BindGameStateMachine() => GameStateMachineInstaller.Install(Container);

        private void BindNetwork()
        {
            Container.Bind<NetworkCallbacks>()
                .AsSingle()
                .NonLazy();

            Container.Bind<INetworkRunnerCallbacks>()
                .To<NetworkCallbacks>()
                .FromResolve();

            Container.Bind<INetworkCallbacksService>()
                .To<NetworkCallbacks>()
                .FromResolve();

            Container.Bind<NetworkRunnerProvider>().AsSingle();
            Container.Bind<NetworkSceneLoadNotifier>().AsSingle().NonLazy();
            Container.Bind<IGameModeService>().To<GameModeService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PhotonNetworkService>().AsSingle().NonLazy();
            Container.Bind<INetworkObjectSpawner>()
                .To<PhotonNetworkSpawner>()
                .AsSingle();
        }

        private void BindSpawners()
        {
            Container.Bind<IPlayerSpawner>()
                .To<PlayerSpawner>()
                .AsSingle();
        }
    }
}