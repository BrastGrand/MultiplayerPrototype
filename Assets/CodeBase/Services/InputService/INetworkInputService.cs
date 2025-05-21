namespace CodeBase.Services.InputService
{
    public interface INetworkInputService
    {
        NetworkInputData GetInput();
        void Enable();
    }
}