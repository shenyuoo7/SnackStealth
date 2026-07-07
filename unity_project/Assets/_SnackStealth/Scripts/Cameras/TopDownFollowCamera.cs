using UnityEngine;

namespace SnackStealth.Cameras
{
    [DisallowMultipleComponent]
    public sealed class TopDownFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 7.5f, -7.5f);
        [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.12f;
        [SerializeField] private float lookAtHeight = 1.2f;

        private Vector3 velocity;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            velocity = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                positionSmoothTime);

            Vector3 lookPoint = target.position + Vector3.up * lookAtHeight;
            Vector3 lookDirection = lookPoint - transform.position;

            if (lookDirection.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            }
        }
    }
}
