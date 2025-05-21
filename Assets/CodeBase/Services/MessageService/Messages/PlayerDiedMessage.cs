using Fusion;

namespace CodeBase.Services.MessageService.Messages
{
    public struct PlayerDiedMessage : IMessage
    {
        public NetworkObject PlayerObject;
    }
}