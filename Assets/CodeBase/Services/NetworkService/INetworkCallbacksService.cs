using System.Threading.Tasks;
using Fusion;

namespace CodeBase.Services.NetworkService
{
    public interface INetworkCallbacksService
    {
        Task SceneLoadCompleted { get; }
        void RegisterPlayer(PlayerRef player);
    }
}