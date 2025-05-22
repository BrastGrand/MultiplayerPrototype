using UnityEngine;

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
            var data = new NetworkInputData
            {
                Jump = _reader.Jump
            };
            
            data.MoveInput = _reader.MoveInput;
            
            return data;
        }

        public void Enable()
        {
            _reader.Enable();
        }

        public void Disable()
        {
            _reader.Disable();
        }
    }
}