using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Services.PlayerSpawnerService
{
    /// <summary>
    /// В случае, если точки спавна могут меняться динамически
    /// </summary>
    public class DynamicSpawnPointsProvider : ISpawnPointsProvider
    {
        private List<Transform> _spawnPoints = new List<Transform>();
        public IReadOnlyList<Transform> SpawnPoints => _spawnPoints;

        public void UpdateSpawnPoints(IEnumerable<Transform> newPoints)
        {
            _spawnPoints = new List<Transform>();
        }
    }
}