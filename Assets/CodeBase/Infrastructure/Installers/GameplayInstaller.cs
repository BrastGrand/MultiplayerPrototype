using System.Collections.Generic;
using CodeBase.Gameplay;
using CodeBase.Gameplay.Camera;
using CodeBase.Gameplay.Player;
using CodeBase.Services.DisconnectService;
using CodeBase.Services.InputService;
using CodeBase.Services.Message;
using CodeBase.Services.PlayerSpawnerService;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    public class GameplayInstaller : MonoInstaller
    {
        [SerializeField] private CameraFollow _cameraFollow;
        [SerializeField] private List<Transform> _spawnPoints;

        public override void InstallBindings()
        {
            Debug.Log("GameplayInstaller");
            var provider = new StaticSpawnPointsProvider(_spawnPoints);

            Container.Bind<ISpawnPointsProvider>().FromInstance(provider).AsSingle();
            Container.Bind<StaticSpawnPointsProvider>().FromInstance(provider).AsSingle();

            // Временный хак для отправки SpawnPointsReadyMessage
            var messageService = Container.Resolve<IMessageService>();
            Debug.Log("[GameplayInstaller] Manually publishing SpawnPointsReadyMessage"); 
            messageService.Publish<SpawnPointsReadyMessage>(new SpawnPointsReadyMessage(provider));

            Container.Bind<IHealthService>().To<HealthService>().AsSingle();

            Container.Bind<IHostDisconnectHandler>().To<HostDisconnectHandler>().AsSingle();

            Container.Bind<CameraFollow>().FromInstance(_cameraFollow).AsSingle();
            
            // Уведомляем о готовности игрового процесса
            var readyNotifier = Container.Resolve<IGameplayReadyNotifier>();
            Debug.Log("[GameplayInstaller] Notifying GameplayReadyNotifier");
            readyNotifier.NotifyReady();
        }
    }
}