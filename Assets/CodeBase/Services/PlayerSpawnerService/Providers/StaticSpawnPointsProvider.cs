using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeBase.Services.PlayerSpawnerService
{
    public class StaticSpawnPointsProvider : ISpawnPointsProvider
    {
        public List<Transform> SpawnPoints { get; }

        public Transform GetRandomPoint
        {
            get
            {
                if (SpawnPoints == null || SpawnPoints.Count == 0)
                {
                    Debug.LogError("[StaticSpawnPointsProvider] No spawn points available!");
                    return null;
                }
                
                return SpawnPoints[Random.Range(0, SpawnPoints.Count)];
            }
        }

        public StaticSpawnPointsProvider(IEnumerable<Transform> spawnPoints)
        {
            SpawnPoints = spawnPoints.ToList();
            Debug.Log($"[StaticSpawnPointsProvider] Initialized with {SpawnPoints.Count} spawn points");
        }
    }
}