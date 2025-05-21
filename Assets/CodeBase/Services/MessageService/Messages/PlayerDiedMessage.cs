using Fusion;

namespace CodeBase.Services.Message
{
    public struct PlayerDiedMessage : IMessage
    {
        public NetworkObject PlayerObject;
    }
}