using Fusion;

namespace CodeBase.Services.MessageService.Messages
{
    public struct PlayerTakeDamageMessage : IMessage
    {
        public NetworkObject PlayerObject;
        public float Damage;
    }
}