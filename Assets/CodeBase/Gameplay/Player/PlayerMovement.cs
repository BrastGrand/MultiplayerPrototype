using CodeBase.Services.InputService;
using Fusion;
using UnityEngine;

namespace CodeBase.Gameplay.Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private NetworkCharacterController _networkController;

        [Networked] private float NetworkRotation { get; set; }

        private readonly float _rotationSpeed = 60f;
        private float _moveSpeed = 5f;

        public void Initialize(float moveSpeed)
        {
            _moveSpeed = moveSpeed;
            _networkController.maxSpeed = moveSpeed;
        }

        public void Move(NetworkInputData input)
        {
            if (_networkController == null) return;

            var direction = new Vector2(input.MoveX, input.MoveY).normalized;
            float forwardInput = direction.y;
            float turnInput = direction.x;

            if (Mathf.Abs(turnInput) > 0.01f && (HasStateAuthority || Object.HasInputAuthority))
            {
                float rotationAmount = turnInput * _rotationSpeed * Runner.DeltaTime;
                NetworkRotation += rotationAmount;

                if (NetworkRotation >= 360f) NetworkRotation -= 360f;
                if (NetworkRotation < 0f) NetworkRotation += 360f;
            }

            transform.rotation = Quaternion.Euler(0, NetworkRotation, 0);

            Vector3 moveDirection = Vector3.zero;

            if (Mathf.Abs(forwardInput) > 0.01f)
            {
                moveDirection = transform.forward * forwardInput * _moveSpeed;
            }

            // Прыжок
            if (input.Jump && _networkController.Grounded)
            {
                _networkController.Jump();
            }

            // ВСЕГДА вызываем Move() для обеспечения работы гравитации
            _networkController.Move(moveDirection);
        }

        public void Stop()
        {
            if (_networkController != null)
            {
                // Вызываем Move() с нулевым направлением для поддержания гравитации
                _networkController.Move(Vector3.zero);
            }
        }

        // Принудительное обновление для поддержания гравитации в состоянии покоя
        public override void FixedUpdateNetwork()
        {
            if (_networkController == null) return;
            
            // Если нет актуального ввода, все равно поддерживаем физику
            if (!GetInput(out NetworkInputData input))
            {
                // Применяем гравитацию даже без ввода
                _networkController.Move(Vector3.zero);
            }
        }
    }
}