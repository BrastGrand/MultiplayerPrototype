using Fusion;

namespace CodeBase.Services.Message
{
    public class NetworkRunnerReadyMessage : IMessage
    {
        public NetworkRunner Runner { get; }

        public NetworkRunnerReadyMessage(NetworkRunner runner)
        {
            Runner = runner;
        }
    }
} 