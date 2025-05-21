using Fusion;

namespace CodeBase.Services.Message
{
    public struct PlayerTakeDamageMessage : IMessage
    {
        public NetworkObject PlayerObject;
        public float Damage;
    }
}