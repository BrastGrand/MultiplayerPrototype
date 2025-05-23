namespace CodeBase.Services.Message
{
    public class GameStartedMessage : IMessage
    {
        public Fusion.GameMode Mode { get; }

        public GameStartedMessage(Fusion.GameMode mode)
        {
            Mode = mode;
        }
    }
} 