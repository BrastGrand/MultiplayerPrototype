using System;
using Fusion;

namespace CodeBase.Services.PlayerSpawnerService
{
    public interface IPlayerSpawner : IDisposable
    {
        /// <summary>
        /// Событие, вызываемое после спавна игрока
        /// </summary>
        event Action<PlayerRef> OnPlayerSpawned;
    }
}