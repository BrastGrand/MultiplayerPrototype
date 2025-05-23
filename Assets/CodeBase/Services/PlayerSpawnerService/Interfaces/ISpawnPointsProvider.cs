using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Services.PlayerSpawnerService
{
    public interface ISpawnPointsProvider
    {
        List<Transform> SpawnPoints { get; }
        Transform GetRandomPoint { get; }
    }
}