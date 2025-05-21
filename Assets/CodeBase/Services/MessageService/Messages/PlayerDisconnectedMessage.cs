using Fusion;

namespace CodeBase.Services.Message
{
    public class PlayerDisconnectedMessage : IMessage
    {
        public PlayerRef PlayerRef;
    }
} 