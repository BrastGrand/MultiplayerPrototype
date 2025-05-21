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
            
            // Устанавливаем через свойство, которое автоматически заполнит MoveX и MoveY
            data.MoveInput = _reader.MoveInput;
            
            return data;
        }

        public void Enable()
        {
            _reader.Enable();
            Debug.Log("[NetworkInputService] Input enabled");
        }

        public void Disable()
        {
            _reader.Disable();
            Debug.Log("[NetworkInputService] Input disabled");
        }
    }
}