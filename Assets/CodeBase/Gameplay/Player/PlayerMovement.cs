using Fusion;
using UnityEngine;

namespace CodeBase.Gameplay.Player
{
    public class PlayerMovement : NetworkBehaviour, IPlayerMovement
    {
        [SerializeField] private CharacterController _controller;
        [SerializeField] private float _moveSpeed = 1f;
        private float _rotationSpeed = 120f;
        private float _gravity = -9.81f;

        private Vector2 _lastMoveInput = Vector2.zero;

        private Vector3 _velocity;

        public void Initialize(float moveSpeed)
        {
            _moveSpeed = moveSpeed;
        }

        public void Move(Vector2 direction)
        {
            _lastMoveInput = direction;

            float forwardInput = direction.y; // W/S
            float turnInput = direction.x;    // A/D

            // Поворот по оси Y
            if (Mathf.Abs(turnInput) > 0.01f)
            {
                float rotationAmount = turnInput * _rotationSpeed * Runner.DeltaTime;
                _controller.transform.Rotate(0f, rotationAmount, 0f);
            }

            // Движение вперёд/назад
            if (Mathf.Abs(forwardInput) > 0.01f)
            {
                Vector3 moveDirection = _controller.transform.forward * forwardInput;
                _controller.Move(moveDirection * _moveSpeed * Runner.DeltaTime);
            }

            bool isGrounded = _controller.isGrounded;

            if (isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            _velocity.y += _gravity * Runner.DeltaTime;
            _controller.Move(_velocity * Runner.DeltaTime);
        }

        public bool HasMovementInput()
        {
            return _lastMoveInput.sqrMagnitude > 0.01f;
        }
    }
}