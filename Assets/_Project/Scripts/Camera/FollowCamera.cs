using UnityEngine;

namespace Game.CameraControl
{
    public class FollowCamera : MonoBehaviour
    {
        [Tooltip("Local-space offset from the target (behind and above it).")]
        [SerializeField] private Vector3 offset = new(0f, 8f, -8f);

        [Tooltip("How quickly the camera catches up to the target position (higher = snappier).")]
        [SerializeField] private float followSpeed = 4f;

        [Tooltip("Fixed camera look angle (pitch/yaw/roll) — the camera only translates, it never rotates.")]
        [SerializeField] private Vector3 fixedEulerAngles = new(50f, 0f, 0f);

        private Transform _target;

        public void Initialize(Transform target)
        {
            _target = target;
            transform.eulerAngles = fixedEulerAngles;
            transform.position = _target.position + offset; // snap immediately, no smoothing on the first frame
        }

        private void LateUpdate()
        {
            if (_target == null)
                return;

            FollowTick(Time.deltaTime);
        }
        
        public void FollowTick(float deltaTime)
        {
            Vector3 desired = _target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, followSpeed * deltaTime);
        }
    }
}
