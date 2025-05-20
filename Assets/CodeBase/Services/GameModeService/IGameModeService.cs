namespace CodeBase.Services.GameModeService
{
    public interface IGameModeService
    {
        bool IsHost { get; }
        bool IsClient { get; }
    }
}