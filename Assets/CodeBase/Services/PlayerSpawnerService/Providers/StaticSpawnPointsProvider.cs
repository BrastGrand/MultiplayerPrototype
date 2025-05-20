using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeBase.Services.PlayerSpawnerService
{
    public class StaticSpawnPointsProvider : ISpawnPointsProvider
    {
        public IReadOnlyList<Transform> SpawnPoints { get; }

        public StaticSpawnPointsProvider(IEnumerable<Transform> spawnPoints)
        {
            SpawnPoints = spawnPoints.ToList().AsReadOnly();
        }
    }
}