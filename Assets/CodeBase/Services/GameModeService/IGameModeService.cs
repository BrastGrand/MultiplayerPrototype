namespace CodeBase.Services.GameMode
{
    public interface IGameModeService
    {
        bool IsHost { get; }
        bool IsClient { get; }
    }
}