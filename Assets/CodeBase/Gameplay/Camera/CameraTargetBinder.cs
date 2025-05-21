using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay.Camera
{
    public class CameraTargetBinder : MonoBehaviour
    {
        private CameraFollow _cameraFollow;

        [SerializeField] private Transform _targetToFollow;
        [SerializeField] private int _priority = 0;

        [Inject(Optional = true)]
        public void Construct(CameraFollow cameraFollow)
        {
            _cameraFollow = cameraFollow;
        }

        private void Start()
        {
            // Проверяем наличие камеры перед использованием
            if (_cameraFollow != null)
            {
                _cameraFollow.SetFollowTarget(_targetToFollow, _priority);
                Debug.Log($"CameraTargetBinder: Set camera target to {_targetToFollow.name} via injection");
            }
            else if (CameraFollow.Instance != null)
            {
                // Запасной вариант - использование синглтона
                CameraFollow.Instance.SetFollowTarget(_targetToFollow, _priority);
                Debug.Log($"CameraTargetBinder: Set camera target to {_targetToFollow.name} via singleton");
            }
            else
            {
                Debug.LogWarning("CameraTargetBinder: No camera follow component found!");
            }
        }
    }
}