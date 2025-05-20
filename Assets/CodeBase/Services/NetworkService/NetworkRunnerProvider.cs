using System;
using Fusion;

namespace CodeBase.Services.NetworkService
{
    public class NetworkRunnerProvider : IDisposable
    {
        private NetworkRunner _runner;

        public NetworkRunner Runner => _runner;
        public bool IsServer => _runner?.IsServer ?? false;

        public void Initialize(NetworkRunner runner)
        {
            _runner = runner;
        }

        public void Dispose() => _runner = null;
    }
}