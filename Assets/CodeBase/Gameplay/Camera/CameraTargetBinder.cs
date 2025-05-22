using UnityEngine;

namespace CodeBase.Gameplay.Camera
{
    public class CameraTargetBinder : MonoBehaviour
    {
        [SerializeField] private Transform _targetToFollow;

        private CameraFollow _cameraFollow;
        private bool _isInitialized;

        public void Initialize()
        {
            if(_isInitialized) return;

            UnityEngine.Camera camera = UnityEngine.Camera.main;
            if(camera == null) return;

            _cameraFollow = camera.GetComponent<CameraFollow>();
            _cameraFollow?.SetFollowTarget(_targetToFollow);
            _isInitialized = true;
        }
    }
}