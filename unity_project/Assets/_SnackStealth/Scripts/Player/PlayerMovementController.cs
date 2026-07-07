using SnackStealth.Characters;
using SnackStealth.Cameras;
using SnackStealth.Gameplay;
using UnityEngine;

namespace SnackStealth.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera movementCamera;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform headPoint;
        [SerializeField] private GameStateController gameState;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 3.6f;
        [SerializeField, Min(0f)] private float acceleration = 18f;
        [SerializeField, Min(0f)] private float rotationSpeed = 720f;
        [SerializeField, Min(0f)] private float gravity = 18f;
        [SerializeField, Min(0f)] private float jumpHeight = 1.15f;

        [Header("First Person")]
        [SerializeField] private bool firstPersonMode = true;
        [SerializeField, Min(0f)] private float mouseSensitivity = 2.2f;
        [SerializeField] private Vector2 pitchLimits = new Vector2(-65f, 70f);
        [SerializeField] private bool hideLocalVisual = true;
        [SerializeField] private bool lockCursorOnPlay = true;

        private CharacterController characterController;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float cameraPitch;
        private Renderer[] visualRenderers;
        private bool movementEnabled = true;

        public void Configure(Camera newMovementCamera, Transform newVisualRoot)
        {
            movementCamera = newMovementCamera;
            visualRoot = newVisualRoot;
        }

        public void ConfigureFirstPerson(Camera newMovementCamera, Transform newVisualRoot, Transform newHeadPoint)
        {
            movementCamera = newMovementCamera;
            visualRoot = newVisualRoot;
            headPoint = newHeadPoint;
            firstPersonMode = true;
            hideLocalVisual = true;
        }

        public void ConfigureGameState(GameStateController newGameState)
        {
            gameState = newGameState;
        }

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;
            if (!enabled)
            {
                horizontalVelocity = Vector3.zero;
            }
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (movementCamera == null)
            {
                movementCamera = Camera.main;
            }

            if (visualRoot == null && TryGetComponent(out CharacterModelSockets sockets))
            {
                visualRoot = sockets.VisualRoot;
                headPoint = sockets.HeadPoint;
            }
            else if (headPoint == null && TryGetComponent(out CharacterModelSockets modelSockets))
            {
                headPoint = modelSockets.HeadPoint;
            }

            if (firstPersonMode)
            {
                DisableTopDownCamera();
            }
        }

        private void OnEnable()
        {
            if (!firstPersonMode)
            {
                return;
            }

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
            bool canAct = gameState == null || gameState.CanPlayerAct;

            if (firstPersonMode && canAct)
            {
                HandleFirstPersonLook();
            }

            if (!canAct)
            {
                horizontalVelocity = Vector3.zero;
                return;
            }

            Vector2 input = ReadMovementInput();
            if (!movementEnabled)
            {
                input = Vector2.zero;
            }

            Vector3 desiredDirection = GetMovementDirection(input);
            Vector3 desiredVelocity = desiredDirection * moveSpeed;

            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                desiredVelocity,
                acceleration * Time.deltaTime);

            ApplyGravity();
            ApplyJump();

            Vector3 motion = horizontalVelocity + Vector3.up * verticalVelocity;
            characterController.Move(motion * Time.deltaTime);

            RotateVisual(desiredDirection);
        }

        private void LateUpdate()
        {
            if (!firstPersonMode || movementCamera == null || headPoint == null)
            {
                return;
            }

            movementCamera.transform.SetPositionAndRotation(
                headPoint.position,
                Quaternion.Euler(cameraPitch, transform.eulerAngles.y, 0f));

            if (hideLocalVisual)
            {
                SetLocalVisualVisible(false);
            }
        }

        private static Vector2 ReadMovementInput()
        {
            Vector2 input = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical"));

            return Vector2.ClampMagnitude(input, 1f);
        }

        private Vector3 GetMovementDirection(Vector2 input)
        {
            if (input.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            if (firstPersonMode)
            {
                Vector3 playerForward = transform.forward;
                Vector3 playerRight = transform.right;
                playerForward.y = 0f;
                playerRight.y = 0f;
                playerForward.Normalize();
                playerRight.Normalize();
                return (playerRight * input.x + playerForward * input.y).normalized;
            }

            Transform cameraTransform = movementCamera != null ? movementCamera.transform : null;

            Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            return (right * input.x + forward * input.y).normalized;
        }

        private void HandleFirstPersonLook()
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

            transform.Rotate(Vector3.up, yawInput, Space.World);
            cameraPitch = Mathf.Clamp(cameraPitch - pitchInput, pitchLimits.x, pitchLimits.y);
        }

        private void ApplyGravity()
        {
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
                return;
            }

            verticalVelocity -= gravity * Time.deltaTime;
        }

        private void ApplyJump()
        {
            if (!characterController.isGrounded || !Input.GetButtonDown("Jump"))
            {
                return;
            }

            verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
        }

        private void RotateVisual(Vector3 direction)
        {
            if (visualRoot == null || direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            if (firstPersonMode)
            {
                visualRoot.rotation = transform.rotation;
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            visualRoot.rotation = Quaternion.RotateTowards(
                visualRoot.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }

        private void DisableTopDownCamera()
        {
            if (movementCamera == null)
            {
                return;
            }

            TopDownFollowCamera topDownFollowCamera = movementCamera.GetComponent<TopDownFollowCamera>();
            if (topDownFollowCamera != null)
            {
                topDownFollowCamera.enabled = false;
            }

            FirstPersonCameraController firstPersonCameraController = movementCamera.GetComponent<FirstPersonCameraController>();
            if (firstPersonCameraController != null)
            {
                firstPersonCameraController.enabled = false;
            }
        }

        private void SetLocalVisualVisible(bool visible)
        {
            if (visualRoot == null)
            {
                return;
            }

            visualRenderers ??= visualRoot.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in visualRenderers)
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
