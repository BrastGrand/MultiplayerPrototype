using UnityEngine;

namespace CodeBase.Gameplay.Camera
{
    public class CameraTargetBinder : MonoBehaviour
    {
        private CameraFollow _cameraFollow;

        [SerializeField] private Transform _targetToFollow;

        /*
         //Возникли проблемы с инъекцией, поэтому временно отказался от этого решения и добавил синглтон в CameraFollow
        [Inject]
        public void Construct(CameraFollow cameraFollow)
        {
            _cameraFollow = cameraFollow;
        }
        */

        private void Start()
        {
            CameraFollow.Instance.SetFollowTarget(_targetToFollow);
            //_cameraFollow.SetFollowTarget(_targetToFollow);
        }
    }
}