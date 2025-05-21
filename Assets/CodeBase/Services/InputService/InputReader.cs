using System;
using UnityEngine;

namespace CodeBase.Services.InputService
{
    public class InputReader : IInputReader, IDisposable
    {
        private readonly InputControls _input = new InputControls();

        public Vector2 MoveInput => _input.Player.Move.ReadValue<Vector2>();

        public void Enable() => _input.Enable();
        public void Disable() => _input.Disable();
        public void Dispose() => _input.Dispose();
    }
}