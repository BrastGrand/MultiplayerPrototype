using Fusion;
using UnityEngine;
using Zenject;

namespace CodeBase.Services.NetworkService
{
    [CreateAssetMenu(fileName = "NetworkCallbacksInstaller", menuName = "Installers/NetworkCallbacksInstaller")]
    public class NetworkCallbacksInstaller : ScriptableObjectInstaller<NetworkCallbacksInstaller>
    {
        [SerializeField] private NetworkCallbacks _networkCallbacksPrefab;

        public override void InstallBindings()
        {
            Container.Bind<NetworkCallbacks>()
                .FromComponentInNewPrefab(_networkCallbacksPrefab)
                .AsSingle()
                .NonLazy();

            Container.Bind<INetworkRunnerCallbacks>()
                .To<NetworkCallbacks>()
                .FromResolve();
        }
    }
} 