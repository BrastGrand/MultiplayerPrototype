using Cysharp.Threading.Tasks;
using System;

namespace CodeBase.Services.NetworkService
{
    public interface INetworkService : IDisposable
    {
        UniTask StartHost();
        UniTask StartClient();
        void Disconnect();
    }
}