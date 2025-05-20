using Cysharp.Threading.Tasks;

namespace CodeBase.Services.NetworkService
{
    public interface INetworkService
    {
        UniTask StartHost();
        UniTask StartClient();
        void Disconnect();
    }
}