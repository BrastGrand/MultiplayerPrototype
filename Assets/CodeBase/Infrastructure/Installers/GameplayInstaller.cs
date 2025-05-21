using System.Collections.Generic;
using CodeBase.Gameplay.Camera;
using CodeBase.Gameplay.Player;
using CodeBase.Services.DisconnectService;
using CodeBase.Services.InputService;
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

            Container.Bind<IInputReader>().To<InputReader>().AsSingle();
            Container.Bind<INetworkInputService>().To<NetworkInputService>().AsSingle();

            Container.Bind<IHealthService>().To<HealthService>().AsSingle();

            Container.Bind<IHostDisconnectHandler>().To<HostDisconnectHandler>().AsSingle();

            Container.Bind<CameraFollow>().FromInstance(_cameraFollow).AsSingle();
        }
    }
}