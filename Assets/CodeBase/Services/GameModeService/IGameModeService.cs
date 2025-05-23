namespace CodeBase.Services.NetworkGameMode
{
    public interface IGameModeService
    {
        bool IsHost { get; }
        bool IsClient { get; }
    }
}