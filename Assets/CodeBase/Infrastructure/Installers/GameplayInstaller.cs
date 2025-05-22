using System.Collections.Generic;
using CodeBase.Gameplay;
using CodeBase.Gameplay.Player;
using CodeBase.Services.DisconnectService;
using CodeBase.Services.Message;
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

            var messageService = Container.Resolve<IMessageService>();
            messageService.Publish(new SpawnPointsReadyMessage(provider));

            Container.Bind<IHealthService>().To<HealthService>().AsSingle();

            Container.Bind<IHostDisconnectHandler>().To<HostDisconnectHandler>().AsSingle();

            // Уведомляем о готовности игрового процесса
            var readyNotifier = Container.Resolve<IGameplayReadyNotifier>();
            readyNotifier.NotifyReady();
        }
    }
}