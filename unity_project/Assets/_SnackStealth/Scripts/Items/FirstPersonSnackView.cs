using UnityEngine;

namespace SnackStealth.Items
{
    [DisallowMultipleComponent]
    public sealed class FirstPersonSnackView : MonoBehaviour
    {
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [SerializeField] private Transform snackVisual;
        [SerializeField, Min(0f)] private float eatingBobAmount = 0.055f;
        [SerializeField, Min(0f)] private float eatingBobSpeed = 10f;

        private Vector3 leftBaseLocalPosition;
        private Vector3 rightBaseLocalPosition;
        private Vector3 snackBaseLocalPosition;
        private bool isEating;

        public void Configure(Transform newLeftHand, Transform newRightHand, Transform newSnackVisual)
        {
            leftHand = newLeftHand;
            rightHand = newRightHand;
            snackVisual = newSnackVisual;
            CacheBasePositions();
        }

        public void SetEating(bool eating)
        {
            isEating = eating;
            if (snackVisual != null)
            {
                snackVisual.gameObject.SetActive(eating);
            }
        }

        private void Awake()
        {
            CacheBasePositions();
            SetEating(false);
        }

        private void LateUpdate()
        {
            if (leftHand == null || rightHand == null)
            {
                return;
            }

            float bob = isEating ? Mathf.Sin(Time.time * eatingBobSpeed) * eatingBobAmount : 0f;
            leftHand.localPosition = leftBaseLocalPosition + new Vector3(0f, bob * 0.6f, 0f);
            rightHand.localPosition = rightBaseLocalPosition + new Vector3(0f, -bob * 0.45f, bob);

            if (snackVisual != null)
            {
                snackVisual.localPosition = snackBaseLocalPosition + new Vector3(0f, bob, bob * 0.4f);
                snackVisual.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * eatingBobSpeed) * 8f, 0f);
            }
        }

        private void CacheBasePositions()
        {
            if (leftHand != null)
            {
                leftBaseLocalPosition = leftHand.localPosition;
            }

            if (rightHand != null)
            {
                rightBaseLocalPosition = rightHand.localPosition;
            }

            if (snackVisual != null)
            {
                snackBaseLocalPosition = snackVisual.localPosition;
            }
        }
    }
}
