using Fusion;

namespace CodeBase.Services.MessageService.Messages
{
    public class PlayerConnectedMessage : IMessage
    {
        public PlayerRef PlayerRef;
        public bool IsHost;
        public int PlayerId => PlayerRef.PlayerId;
    }
}