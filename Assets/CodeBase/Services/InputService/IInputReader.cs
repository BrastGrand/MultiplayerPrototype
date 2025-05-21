using UnityEngine;

namespace CodeBase.Services.InputService
{
    public interface IInputReader
    {
        Vector2 MoveInput { get; }

        void Enable();
        void Disable();
    }
}