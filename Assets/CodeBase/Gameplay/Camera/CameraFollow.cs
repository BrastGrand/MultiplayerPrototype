using UnityEngine;

namespace CodeBase.Gameplay.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        private Transform _followTarget;

        public void SetFollowTarget(Transform target)
        {
            _followTarget = target;
        }

        private void LateUpdate()
        {
            if (_followTarget == null)
                return;

            transform.position = _followTarget.position;
            transform.rotation = _followTarget.transform.rotation;
        }
    }
}