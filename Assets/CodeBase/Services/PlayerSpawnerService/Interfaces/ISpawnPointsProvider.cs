using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Services.PlayerSpawnerService
{
    public interface ISpawnPointsProvider
    {
        IReadOnlyList<Transform> SpawnPoints { get; }
    }
}