using UnityEngine;
using Zenject;

namespace CodeBase.Services.PlayerSpawnerService
{
    [CreateAssetMenu(fileName = "SpawnPointsInstaller", menuName = "Installers/SpawnPointsInstaller")]
    public class SpawnPointsInstaller : ScriptableObjectInstaller<SpawnPointsInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ISpawnPointsProvider>().To<StaticSpawnPointsProvider>().AsSingle();
        }
    }
} 