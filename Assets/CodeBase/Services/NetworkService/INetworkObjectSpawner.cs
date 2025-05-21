using System;
using System.Threading.Tasks;
using CodeBase.Gameplay.Player;
using Fusion;
using UnityEngine;

namespace CodeBase.Services.NetworkService
{
    public interface INetworkObjectSpawner
    {
        void SpawnPlayer(PlayerRef player, Vector3 position, Quaternion rotation, Action<NetworkPlayer> onSpawnedPlayer);
        void SpawnNetworkObject(string prefabKey, Vector3 position);
    }
}