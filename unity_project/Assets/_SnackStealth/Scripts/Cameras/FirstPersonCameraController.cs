using System.Collections.Generic;
using SnackStealth.Characters;
using UnityEngine;

namespace SnackStealth.Cameras
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class FirstPersonCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform headPoint;
        [SerializeField] private Transform localVisualRoot;

        [Header("Look")]
        [SerializeField, Min(0f)] private float mouseSensitivity = 2.2f;
        [SerializeField] private Vector2 pitchLimits = new Vector2(-65f, 70f);
        [SerializeField] private bool lockCursorOnPlay = true;

        [Header("Local View")]
        [SerializeField] private bool hideLocalVisual = true;

        private readonly List<Renderer> hiddenRenderers = new List<Renderer>();
        private float pitch;

        public void Configure(Transform newPlayerRoot, Transform newHeadPoint, Transform newLocalVisualRoot)
        {
            playerRoot = newPlayerRoot;
            headPoint = newHeadPoint;
            localVisualRoot = newLocalVisualRoot;
            hiddenRenderers.Clear();
        }

        private void Awake()
        {
            if (playerRoot != null && (headPoint == null || localVisualRoot == null)
                && playerRoot.TryGetComponent(out CharacterModelSockets sockets))
            {
                if (headPoint == null)
                {
                    headPoint = sockets.HeadPoint;
                }

                if (localVisualRoot == null)
                {
                    localVisualRoot = sockets.VisualRoot;
                }
            }
        }

        private void OnEnable()
        {
            SetLocalVisualVisible(!hideLocalVisual);

            if (lockCursorOnPlay)
            {
                LockCursor();
            }
        }

        private void OnDisable()
        {
            SetLocalVisualVisible(true);
            UnlockCursor();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnlockCursor();
                return;
            }

            if (lockCursorOnPlay && Input.GetMouseButtonDown(0))
            {
                LockCursor();
            }

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            float yawInput = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float pitchInput = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            if (playerRoot != null)
            {
                playerRoot.Rotate(Vector3.up, yawInput, Space.World);
            }

            pitch = Mathf.Clamp(pitch - pitchInput, pitchLimits.x, pitchLimits.y);
        }

        private void LateUpdate()
        {
            if (headPoint == null)
            {
                return;
            }

            transform.position = headPoint.position;

            float yaw = playerRoot != null ? playerRoot.eulerAngles.y : transform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void SetLocalVisualVisible(bool visible)
        {
            if (localVisualRoot == null)
            {
                return;
            }

            if (hiddenRenderers.Count == 0)
            {
                localVisualRoot.GetComponentsInChildren(true, hiddenRenderers);
            }

            foreach (Renderer renderer in hiddenRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = visible;
                }
            }
        }

        private static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
