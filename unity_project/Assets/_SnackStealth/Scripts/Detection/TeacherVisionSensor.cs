using SnackStealth.Gameplay;
using UnityEngine;

namespace SnackStealth.Detection
{
    [DisallowMultipleComponent]
    public sealed class TeacherVisionSensor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform eyePoint;
        [SerializeField] private Transform targetRoot;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private GameStateController gameState;
        [SerializeField] private string spotterName = "teacher";

        [Header("Vision")]
        [SerializeField, Min(0f)] private float viewDistance = 6.5f;
        [SerializeField, Range(1f, 180f)] private float horizontalAngle = 70f;
        [SerializeField, Min(0f)] private float verticalTolerance = 2.2f;

        public float ViewDistance => viewDistance;
        public float HorizontalAngle => horizontalAngle;
        public bool CanSeeTarget { get; private set; }

        public void Configure(Transform newEyePoint, Transform newTargetRoot, Transform newTargetPoint, GameStateController newGameState, string newSpotterName = "teacher")
        {
            eyePoint = newEyePoint;
            targetRoot = newTargetRoot;
            targetPoint = newTargetPoint;
            gameState = newGameState;
            spotterName = newSpotterName;
        }

        private void Update()
        {
            CanSeeTarget = HasLineOfSightToTarget();

            if (CanSeeTarget && gameState != null)
            {
                gameState.ReportPlayerSpotted(spotterName);
            }
        }

        private bool HasLineOfSightToTarget()
        {
            if (eyePoint == null || targetRoot == null || targetPoint == null)
            {
                return false;
            }

            Vector3 eye = eyePoint.position;
            Vector3 target = targetPoint.position;
            Vector3 toTarget = target - eye;

            if (toTarget.magnitude > viewDistance)
            {
                return false;
            }

            if (Mathf.Abs(toTarget.y) > verticalTolerance)
            {
                return false;
            }

            Vector3 flatForward = transform.forward;
            Vector3 flatToTarget = toTarget;
            flatForward.y = 0f;
            flatToTarget.y = 0f;

            if (flatToTarget.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            float angle = Vector3.Angle(flatForward.normalized, flatToTarget.normalized);
            if (angle > horizontalAngle * 0.5f)
            {
                return false;
            }

            if (!Physics.Linecast(eye, target, out RaycastHit hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            return hit.transform == targetRoot || hit.transform.IsChildOf(targetRoot);
        }

        private void OnDrawGizmosSelected()
        {
            Transform source = eyePoint != null ? eyePoint : transform;
            Gizmos.color = CanSeeTarget ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(source.position, viewDistance);

            Vector3 left = Quaternion.Euler(0f, -horizontalAngle * 0.5f, 0f) * transform.forward;
            Vector3 right = Quaternion.Euler(0f, horizontalAngle * 0.5f, 0f) * transform.forward;
            Gizmos.DrawLine(source.position, source.position + left * viewDistance);
            Gizmos.DrawLine(source.position, source.position + right * viewDistance);
        }
    }
}
