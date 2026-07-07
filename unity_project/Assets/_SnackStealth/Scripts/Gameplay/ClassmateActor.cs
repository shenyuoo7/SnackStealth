using System.Collections;
using UnityEngine;

namespace SnackStealth.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class ClassmateActor : MonoBehaviour
    {
        private enum GazeState
        {
            Seated,
            Scanning,
            Warning,
            Tracking,
            Cooldown
        }

        [SerializeField] private Transform eyePoint;
        [SerializeField] private Transform targetRoot;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private Renderer[] alertEyeRenderers;
        [SerializeField] private GameStateController gameState;
        [SerializeField, Min(0f)] private float podiumViewDistance = 7f;
        [SerializeField, Range(1f, 180f)] private float podiumViewAngle = 55f;
        [SerializeField, Min(0.1f)] private float scanMinSeconds = 1.8f;
        [SerializeField, Min(0.1f)] private float scanMaxSeconds = 3.4f;
        [SerializeField, Min(0.1f)] private float warningSeconds = 0.9f;
        [SerializeField, Min(0.1f)] private float trackingSeconds = 0.9f;
        [SerializeField, Min(0.1f)] private float cooldownSeconds = 2f;
        [SerializeField, Min(0.1f)] private float groupTrackingCooldownSeconds = 2.5f;

        private GazeState gazeState = GazeState.Seated;
        private float gazeTimer;
        private float targetYaw;
        private static ClassmateActor activeTracker;
        private static float nextTrackingAllowedTime;

        public bool IsSeated { get; private set; } = true;

        public void Configure(
            Transform newEyePoint,
            Transform newTargetRoot,
            Transform newTargetPoint,
            GameStateController newGameState,
            Renderer[] newAlertEyeRenderers)
        {
            eyePoint = newEyePoint;
            targetRoot = newTargetRoot;
            targetPoint = newTargetPoint;
            gameState = newGameState;
            alertEyeRenderers = newAlertEyeRenderers;
            SetAlertEyes(false);
            EnterSeated();
        }

        public void EjectFromSeat(Transform podiumSpot, SeatStation fallbackSeat, GameplayEffectSpawner effects)
        {
            if (effects != null)
            {
                effects.PlaySeatKickEffect(transform.position);
            }

            StopAllCoroutines();

            if (podiumSpot != null)
            {
                IsSeated = false;
                StartCoroutine(FlyToPodium(podiumSpot));
                return;
            }

            if (fallbackSeat != null)
            {
                StartCoroutine(FlyToSeat(fallbackSeat));
                return;
            }

            EnterScanning();
        }

        private void Update()
        {
            if (IsSeated || gameState == null || !gameState.CanPlayerAct)
            {
                if (gazeState != GazeState.Seated && gazeState != GazeState.Scanning)
                {
                    SetAlertEyes(false);
                }

                return;
            }

            gazeTimer -= Time.deltaTime;

            switch (gazeState)
            {
                case GazeState.Scanning:
                    RotateTowardYaw(targetYaw, 105f);
                    if (gazeTimer <= 0f)
                    {
                        EnterWarning();
                    }
                    break;

                case GazeState.Warning:
                    RotateTowardPlayer(155f);
                    if (gazeTimer <= 0f)
                    {
                        EnterTracking();
                    }
                    break;

                case GazeState.Tracking:
                    RotateTowardPlayer(220f);
                    if (CanSeeTarget())
                    {
                        gameState.ReportPlayerSpotted("\u7f5a\u7ad9\u540c\u5b66");
                    }

                    if (gazeTimer <= 0f)
                    {
                        EnterCooldown();
                    }
                    break;

                case GazeState.Cooldown:
                    RotateTowardYaw(targetYaw, 130f);
                    if (gazeTimer <= 0f)
                    {
                        EnterScanning();
                    }
                    break;
            }
        }

        private IEnumerator FlyToPodium(Transform podiumSpot)
        {
            Vector3 start = transform.position;
            Quaternion startRotation = transform.rotation;
            Vector3 end = podiumSpot.position;
            Quaternion endRotation = podiumSpot.rotation;
            float timer = 0f;
            const float duration = 0.62f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);
                float arc = Mathf.Sin(t * Mathf.PI) * 2.05f;
                transform.position = Vector3.Lerp(start, end, EaseOutBack(t)) + Vector3.up * arc;
                transform.rotation = Quaternion.Slerp(startRotation, endRotation * Quaternion.Euler(0f, 720f * t, 0f), t);
                yield return null;
            }

            transform.SetPositionAndRotation(end, endRotation);
            EnterScanning();
        }

        private IEnumerator FlyToSeat(SeatStation seat)
        {
            IsSeated = false;
            SetAlertEyes(false);

            Vector3 start = transform.position;
            Quaternion startRotation = transform.rotation;
            Vector3 end = seat.SitPoint != null ? seat.SitPoint.position : transform.position;
            Quaternion endRotation = seat.SitPoint != null ? seat.SitPoint.rotation : transform.rotation;
            float timer = 0f;
            const float duration = 0.48f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);
                float arc = Mathf.Sin(t * Mathf.PI) * 0.9f;
                transform.position = Vector3.Lerp(start, end, t) + Vector3.up * arc;
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
                yield return null;
            }

            transform.SetPositionAndRotation(end, endRotation);
            EnterSeated();
        }

        private void EnterSeated()
        {
            ReleaseTrackingSlot();
            IsSeated = true;
            gazeState = GazeState.Seated;
            SetAlertEyes(false);
        }

        private void EnterScanning()
        {
            IsSeated = false;
            gazeState = GazeState.Scanning;
            targetYaw = Random.Range(-140f, 140f);
            gazeTimer = Random.Range(scanMinSeconds, scanMaxSeconds);
            SetAlertEyes(false);
        }

        private void EnterWarning()
        {
            gazeState = GazeState.Warning;
            gazeTimer = warningSeconds;
            SetAlertEyes(true);
        }

        private void EnterTracking()
        {
            if ((activeTracker != null && activeTracker != this) || Time.time < nextTrackingAllowedTime)
            {
                EnterCooldown();
                return;
            }

            activeTracker = this;
            gazeState = GazeState.Tracking;
            gazeTimer = trackingSeconds;
            SetAlertEyes(true);
        }

        private void EnterCooldown()
        {
            ReleaseTrackingSlot();
            gazeState = GazeState.Cooldown;
            targetYaw = Random.Range(-140f, 140f);
            gazeTimer = cooldownSeconds;
            SetAlertEyes(false);
        }

        private void OnDisable()
        {
            ReleaseTrackingSlot();
        }

        private void RotateTowardPlayer(float speed)
        {
            if (targetPoint == null)
            {
                return;
            }

            Vector3 direction = targetPoint.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);
        }

        private void RotateTowardYaw(float yaw, float speed)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, yaw, 0f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);
        }

        private void SetAlertEyes(bool visible)
        {
            if (alertEyeRenderers == null)
            {
                return;
            }

            foreach (Renderer eyeRenderer in alertEyeRenderers)
            {
                if (eyeRenderer != null)
                {
                    eyeRenderer.enabled = visible;
                }
            }
        }

        private void ReleaseTrackingSlot()
        {
            if (activeTracker != this)
            {
                return;
            }

            activeTracker = null;
            nextTrackingAllowedTime = Time.time + groupTrackingCooldownSeconds;
        }

        private bool CanSeeTarget()
        {
            if (eyePoint == null || targetRoot == null || targetPoint == null)
            {
                return false;
            }

            Vector3 eye = eyePoint.position;
            Vector3 toTarget = targetPoint.position - eye;
            if (toTarget.magnitude > podiumViewDistance)
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

            if (Vector3.Angle(flatForward.normalized, flatToTarget.normalized) > podiumViewAngle * 0.5f)
            {
                return false;
            }

            if (!Physics.Linecast(eye, targetPoint.position, out RaycastHit hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            return hit.transform == targetRoot || hit.transform.IsChildOf(targetRoot);
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
