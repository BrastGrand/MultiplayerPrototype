using UnityEngine;

namespace CodeBase.Gameplay.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTransform;
        private Transform _followTarget;
        private int _currentPriority = 0;
        public static CameraFollow Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SetFollowTarget(Transform target, int priority = 0)
        {
            if (priority < _currentPriority)
            {
                return;
            }

            _followTarget = target;
            _currentPriority = priority;
        }

        private void LateUpdate()
        {
            if (_followTarget == null || _cameraTransform == null)
                return;

            _cameraTransform.position = _followTarget.position;
            _cameraTransform.rotation = _followTarget.transform.rotation;
        }
    }
}