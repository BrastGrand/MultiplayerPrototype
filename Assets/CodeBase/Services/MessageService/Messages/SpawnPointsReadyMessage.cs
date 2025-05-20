using CodeBase.Services.PlayerSpawnerService;

namespace CodeBase.Services.MessageService.Messages
{
    public readonly struct SpawnPointsReadyMessage : IMessage
    {
        public readonly ISpawnPointsProvider Provider;

        public SpawnPointsReadyMessage(ISpawnPointsProvider provider)
        {
            Provider = provider;
        }
    }
}