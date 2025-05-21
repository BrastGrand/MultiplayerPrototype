using CodeBase.Services.NetworkService;

namespace CodeBase.Services.InputService
{
    public interface INetworkInputService
    {
        NetworkInputData GetInput();
        void Enable();
        void Disable();
    }
}