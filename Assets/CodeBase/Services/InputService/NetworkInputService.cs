namespace CodeBase.Services.InputService
{
    public class NetworkInputService : INetworkInputService
    {
        private readonly IInputReader _reader;

        public NetworkInputService(IInputReader reader)
        {
            _reader = reader;
        }

        public NetworkInputData GetInput()
        {
            return new NetworkInputData
            {
                MoveInput = _reader.MoveInput,
            };
        }

        public void Enable()
        {
            _reader.Enable();
        }
    }
}