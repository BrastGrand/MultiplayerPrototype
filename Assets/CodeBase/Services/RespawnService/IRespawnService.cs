using Fusion;

namespace CodeBase.Gameplay.Player
{
    public interface IRespawnService
    {
        void Respawn(NetworkObject player);
    }
}