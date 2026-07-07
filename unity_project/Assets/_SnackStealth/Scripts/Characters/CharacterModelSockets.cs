using UnityEngine;

namespace SnackStealth.Characters
{
    [DisallowMultipleComponent]
    public sealed class CharacterModelSockets : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform headPoint;
        [SerializeField] private Transform cameraTarget;

        public Transform VisualRoot => visualRoot;
        public Transform HeadPoint => headPoint;
        public Transform CameraTarget => cameraTarget;

        public bool IsComplete => visualRoot != null && headPoint != null && cameraTarget != null;

        public void Assign(Transform newVisualRoot, Transform newHeadPoint, Transform newCameraTarget)
        {
            visualRoot = newVisualRoot;
            headPoint = newHeadPoint;
            cameraTarget = newCameraTarget;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (visualRoot != null && visualRoot == transform)
            {
                Debug.LogWarning($"{nameof(CharacterModelSockets)} should reference a child visual root, not the root object.", this);
            }
        }
#endif
    }
}
