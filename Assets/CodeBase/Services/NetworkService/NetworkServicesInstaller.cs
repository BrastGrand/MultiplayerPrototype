using CodeBase.Gameplay;
using CodeBase.Services.InputService;
using CodeBase.Services.Log;
using CodeBase.Services.Message;
using CodeBase.Services.NetworkGameMode;
using UnityEngine;
using Zenject;

namespace CodeBase.Services.NetworkService
{
    [CreateAssetMenu(fileName = "NetworkServicesInstaller", menuName = "Installers/NetworkServicesInstaller")]
    public class NetworkServicesInstaller : ScriptableObjectInstaller<NetworkServicesInstaller>
    {
        public override void InstallBindings()
        {
            // Регистрируем все необходимые сервисы
            Container.Bind<ILogService>().To<LogService>().AsSingle();
            Container.Bind<IMessageService>().To<MessageService>().AsSingle();
            Container.Bind<IGameModeService>().To<GameModeService>().AsSingle();
            Container.Bind<INetworkObjectSpawner>().To<PhotonNetworkSpawner>().AsSingle();
            Container.Bind<IGameplayReadyNotifier>().To<GameplayReadyNotifier>().AsSingle();
            Container.Bind<INetworkInputService>().To<NetworkInputService>().AsSingle();
        }
    }
} 