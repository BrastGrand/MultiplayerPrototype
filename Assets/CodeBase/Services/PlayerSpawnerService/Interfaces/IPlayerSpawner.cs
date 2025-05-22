using System;
using Fusion;

namespace CodeBase.Services.PlayerSpawnerService
{
    public interface IPlayerSpawner : IDisposable
    {
        event Action<PlayerRef> OnPlayerSpawned;
    }
}