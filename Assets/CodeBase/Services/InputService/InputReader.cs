using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Services.InputService
{
    public class InputReader : MonoBehaviour, IInputReader, IDisposable
    {
        private InputControls _controls;
        private Vector2 _moveInput;

        public Vector2 MoveInput => _moveInput;
        public bool Jump { get; private set; }

        private void Awake()
        {
            _controls = new InputControls();
            _controls.Player.Move.performed += OnMove;
            _controls.Player.Move.canceled += OnMove;
            _controls.Player.Jump.performed += OnJump;
            _controls.Player.Jump.canceled += OnJumpCanceled;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            Jump = context.ReadValueAsButton();
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            Jump = false;
        }

        public void Enable()
        {
            _controls?.Enable();
            Debug.Log("[InputReader] Controls enabled");
        }

        public void Disable()
        {
            _controls?.Disable();
            Debug.Log("[InputReader] Controls disabled");
        }

        private void OnEnable()
        {
            Enable();
        }

        private void OnDisable()
        {
            Disable();
        }

        private void OnDestroy()
        {
            if (_controls != null)
            {
                _controls.Player.Move.performed -= OnMove;
                _controls.Player.Move.canceled -= OnMove;
                _controls.Player.Jump.performed -= OnJump;
                _controls.Player.Jump.canceled -= OnJumpCanceled;
                _controls.Dispose();
            }
        }

        public void Dispose() => _controls.Dispose();
    }
}