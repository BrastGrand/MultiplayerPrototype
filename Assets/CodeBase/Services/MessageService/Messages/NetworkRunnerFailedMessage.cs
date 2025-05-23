using Fusion;
using Fusion.Sockets;

namespace CodeBase.Services.Message
{
    public class NetworkRunnerFailedMessage : IMessage
    {
        public NetworkRunner Runner { get; }
        public NetConnectFailedReason Reason { get; }

        public NetworkRunnerFailedMessage(NetworkRunner runner, NetConnectFailedReason reason)
        {
            Runner = runner;
            Reason = reason;
        }
    }
} 