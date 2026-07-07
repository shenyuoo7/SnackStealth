using SnackStealth.Cameras;
using SnackStealth.Characters;
using SnackStealth.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SnackStealth.Editor
{
    public static class SnackStealthFirstPersonSetup
    {
        [MenuItem("SnackStealth/Step 01/Configure First Person + Jump")]
        public static void ConfigureFirstPersonAndJump()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("Exit Play Mode before configuring first-person controls.");
                return;
            }

            GameObject playerRoot = GameObject.Find("PlayerRoot");
            Camera mainCamera = Camera.main;

            if (playerRoot == null || mainCamera == null)
            {
                Debug.LogError("Could not find PlayerRoot and Main Camera in the open scene.");
                return;
            }

            CharacterModelSockets sockets = playerRoot.GetComponent<CharacterModelSockets>();
            if (sockets == null || !sockets.IsComplete)
            {
                Debug.LogError("PlayerRoot is missing complete CharacterModelSockets references.");
                return;
            }

            Undo.RecordObject(playerRoot.transform, "Configure first-person player");
            Undo.RecordObject(mainCamera.transform, "Configure first-person camera");

            TopDownFollowCamera topDownCamera = mainCamera.GetComponent<TopDownFollowCamera>();
            if (topDownCamera != null)
            {
                Undo.RecordObject(topDownCamera, "Disable top-down camera");
                topDownCamera.enabled = false;
                EditorUtility.SetDirty(topDownCamera);
            }

            FirstPersonCameraController firstPersonCamera = mainCamera.GetComponent<FirstPersonCameraController>();
            if (firstPersonCamera == null)
            {
                firstPersonCamera = Undo.AddComponent<FirstPersonCameraController>(mainCamera.gameObject);
            }

            firstPersonCamera.Configure(playerRoot.transform, sockets.HeadPoint, sockets.VisualRoot);
            EditorUtility.SetDirty(firstPersonCamera);

            PlayerMovementController movement = playerRoot.GetComponent<PlayerMovementController>();
            if (movement == null)
            {
                movement = Undo.AddComponent<PlayerMovementController>(playerRoot);
            }

            movement.ConfigureFirstPerson(mainCamera, sockets.VisualRoot, sockets.HeadPoint);
            EditorUtility.SetDirty(movement);

            mainCamera.transform.position = sockets.HeadPoint.position;
            mainCamera.transform.rotation = playerRoot.transform.rotation;

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("SnackStealth first-person camera and jump-ready movement are configured.");
        }
    }
}
