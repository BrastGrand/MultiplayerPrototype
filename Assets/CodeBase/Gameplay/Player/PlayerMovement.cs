using CodeBase.Services.InputService;
using Fusion;
using UnityEngine;

namespace CodeBase.Gameplay.Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpForce = 5f;
        [SerializeField] private CharacterController _characterController;

        private const float _GRAVITY = -9.81f;
        private float _rotationSpeed = 120f;
        private Vector3 _velocity;

        private bool _isGrounded;

        public void Initialize(float moveSpeed)
        {
            _moveSpeed = moveSpeed;
        }

        public void Move(NetworkInputData input)
        {
            if (!_characterController) return;

            var direction = new Vector2(input.MoveX, input.MoveY);

            float forwardInput = direction.y; // W/S
            float turnInput = direction.x;    // A/D

            // Поворот по оси Y
            if (Mathf.Abs(turnInput) > 0.01f)
            {
                float rotationAmount = turnInput * _rotationSpeed * Runner.DeltaTime;
                _characterController.transform.Rotate(0f, rotationAmount, 0f);
            }

            // Движение вперёд/назад
            if (Mathf.Abs(forwardInput) > 0.01f)
            {
                Vector3 moveDirection = _characterController.transform.forward * forwardInput;
                _characterController.Move(moveDirection * _moveSpeed * Runner.DeltaTime);
            }

            bool isGrounded = _characterController.isGrounded;

            if (isGrounded)
            {
                _velocity.y = -0.5f;

                if (input.Jump)
                {
                    _velocity.y = _jumpForce;
                }
            }
            else
            {
                _velocity.y += _GRAVITY * Runner.DeltaTime;
            }

            _characterController.Move(_velocity * Runner.DeltaTime);
        }

        public void Stop()
        {

        }
    }
}