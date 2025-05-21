using CodeBase.Services.PlayerSpawnerService;

namespace CodeBase.Services.Message
{
    public readonly struct SpawnPointsReadyMessage : IMessage
    {
        public ISpawnPointsProvider Provider { get; }

        public SpawnPointsReadyMessage(ISpawnPointsProvider provider)
        {
            Provider = provider;
        }
    }
}