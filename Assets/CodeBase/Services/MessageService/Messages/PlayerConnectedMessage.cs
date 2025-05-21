using Fusion;

namespace CodeBase.Services.Message
{
    public class PlayerConnectedMessage : IMessage
    {
        public PlayerRef PlayerRef;
        public bool IsHost;
        public int PlayerId => PlayerRef.PlayerId;
    }
}