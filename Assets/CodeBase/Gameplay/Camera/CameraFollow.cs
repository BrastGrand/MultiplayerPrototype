using UnityEngine;

namespace CodeBase.Gameplay.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTransform;
        private Transform _followTarget;
        public static CameraFollow Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SetFollowTarget(Transform target)
        {
            _followTarget = target;
        }

        private void LateUpdate()
        {
            if (_followTarget == null) return;
            _cameraTransform.position = _followTarget.position;
            _cameraTransform.rotation = _followTarget.transform.rotation;
        }
    }
}