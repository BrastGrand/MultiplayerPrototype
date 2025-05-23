using CodeBase.Infrastructure.StateMachine;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    [CreateAssetMenu(fileName = "GameStatesInstaller", menuName = "Installers/GameStatesInstaller")]
    public class GameStatesInstaller : ScriptableObjectInstaller<GameStatesInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<GameBootstrapState>().AsSingle();
            Container.Bind<GameMenuState>().AsSingle();
            Container.Bind<GameLoadingState>().AsSingle();
            Container.Bind<MenuLoadingState>().AsSingle();
            Container.Bind<GameLoopState>().AsSingle();
        }
    }
} 