using System.Collections.Generic;
using CodeBase.Gameplay.Player;
using CodeBase.Services.DisconnectService;
using CodeBase.Services.PlayerSpawnerService;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    public class GameplayInstaller : MonoInstaller
    {
        [SerializeField] private List<Transform> _spawnPoints;

        public override void InstallBindings()
        {
            var provider = new StaticSpawnPointsProvider(_spawnPoints);

            Container.Bind<ISpawnPointsProvider>().FromInstance(provider).AsSingle();
            Container.Bind<StaticSpawnPointsProvider>().FromInstance(provider).AsSingle();

            Container.Bind<IHealthService>().To<HealthService>().AsSingle();
            Container.Bind<IHostDisconnectHandler>().To<HostDisconnectHandler>().AsSingle();
            Container.Bind<IRespawnService>().To<RespawnService>().AsSingle().NonLazy();
        }
    }
}