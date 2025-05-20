using CodeBase.Infrastructure.AssetManagement;
using Fusion;
using UnityEngine;

namespace CodeBase.Services.NetworkService
{
    public class PhotonNetworkSpawner : INetworkObjectSpawner
    {
        private readonly NetworkRunnerProvider _runnerProvider;
        private readonly IAssetProvider _assetProvider;

        public PhotonNetworkSpawner(NetworkRunnerProvider runnerProvider, IAssetProvider assetProvider)
        {
            _runnerProvider = runnerProvider;
            _assetProvider = assetProvider;
        }

        public async void SpawnPlayer(PlayerRef player, Vector3 position, Quaternion rotation)
        {
            if (!_runnerProvider.IsServer) return;

            var playerPrefab = await _assetProvider.Load<GameObject>(AssetLabels.PLAYER);
            _runnerProvider.Runner.SpawnAsync(playerPrefab.GetComponent<NetworkObject>(), position, rotation, player);
        }

        public async void SpawnNetworkObject(string prefabKey, Vector3 position)
        {
            if (!_runnerProvider.IsServer) return;

            var prefab = await _assetProvider.Load<GameObject>(prefabKey);
            _runnerProvider.Runner.Spawn(prefab.GetComponent<NetworkObject>(), position, Quaternion.identity);
        }
    }
}