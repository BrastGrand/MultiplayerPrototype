using Fusion;
using UnityEngine;

namespace CodeBase.Services.NetworkService
{
    public interface INetworkObjectSpawner
    {
        void SpawnPlayer(PlayerRef player, Vector3 position, Quaternion rotation);
        void SpawnNetworkObject(string prefabKey, Vector3 position);
    }
}