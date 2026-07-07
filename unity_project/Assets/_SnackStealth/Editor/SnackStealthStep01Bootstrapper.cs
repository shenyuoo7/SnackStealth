using System.IO;
using SnackStealth.Characters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SnackStealth.Editor
{
    public static class SnackStealthStep01Bootstrapper
    {
        private const string ScenePath = "Assets/_SnackStealth/Scenes/Classroom_Blockout.unity";
        private const string PlayerPrefabPath = "Assets/_SnackStealth/Prefabs/Characters/Player_ModularPlaceholder.prefab";

        [MenuItem("SnackStealth/Step 01/Build Classroom + Player")]
        public static void BuildStep01()
        {
            EnsureProjectFolders();
            AssetDatabase.Refresh();

            CreateMaterials(
                out Material floorMaterial,
                out Material wallMaterial,
                out Material deskMaterial,
                out Material chairMaterial,
                out Material boardMaterial,
                out Material bodyMaterial,
                out Material headMaterial,
                out Material backpackMaterial,
                out Material markerMaterial);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateLighting();
            CreateClassroom(floorMaterial, wallMaterial, deskMaterial, chairMaterial, boardMaterial);
            CreatePreviewCamera();
            CreatePlayerPrefabAndSceneInstance(bodyMaterial, headMaterial, backpackMaterial, markerMaterial);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"SnackStealth Step 01 generated: {ScenePath} and {PlayerPrefabPath}");
        }

        private static void EnsureProjectFolders()
        {
            string[] folders =
            {
                "Assets/ExternalAssets",
                "Assets/_SnackStealth/Art/Placeholders",
                "Assets/_SnackStealth/Materials",
                "Assets/_SnackStealth/Prefabs",
                "Assets/_SnackStealth/Prefabs/Characters",
                "Assets/_SnackStealth/Scenes",
                "Assets/_SnackStealth/Scripts",
                "Assets/_SnackStealth/Scripts/Characters"
            };

            foreach (string folder in folders)
            {
                EnsureAssetDirectory(folder);
            }
        }

        private static void EnsureAssetDirectory(string assetPath)
        {
            string relativePath = assetPath.StartsWith("Assets/")
                ? assetPath.Substring("Assets/".Length)
                : string.Empty;

            string fullPath = Path.Combine(Application.dataPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(fullPath);
        }

        private static void CreateMaterials(
            out Material floorMaterial,
            out Material wallMaterial,
            out Material deskMaterial,
            out Material chairMaterial,
            out Material boardMaterial,
            out Material bodyMaterial,
            out Material headMaterial,
            out Material backpackMaterial,
            out Material markerMaterial)
        {
            floorMaterial = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Blockout_Floor.mat", new Color(0.55f, 0.62f, 0.55f));
            wallMaterial = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Blockout_Wall.mat", new Color(0.74f, 0.78f, 0.72f));
            deskMaterial = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Blockout_Desk.mat", new Color(0.57f, 0.38f, 0.22f));
            chairMaterial = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Blockout_Chair.mat", new Color(0.22f, 0.34f, 0.55f));
            boardMaterial = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Blockout_Board.mat", new Color(0.08f, 0.27f, 0.17f));
            bodyMaterial = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Player_BodyPlaceholder.mat", new Color(0.22f, 0.39f, 0.72f));
            headMaterial = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Player_HeadPlaceholder.mat", new Color(0.86f, 0.70f, 0.55f));
            backpackMaterial = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Player_BackpackPlaceholder.mat", new Color(0.18f, 0.18f, 0.20f));
            markerMaterial = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Player_MarkerPlaceholder.mat", new Color(0.95f, 0.82f, 0.18f));
        }

        private static Material GetOrCreateMaterial(string assetPath, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader)
                {
                    name = Path.GetFileNameWithoutExtension(assetPath)
                };

                AssetDatabase.CreateAsset(material, assetPath);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateLighting()
        {
            GameObject lightObject = new GameObject("KeyLight_Directional");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            RenderSettings.ambientLight = new Color(0.48f, 0.50f, 0.52f);
        }

        private static void CreateClassroom(
            Material floorMaterial,
            Material wallMaterial,
            Material deskMaterial,
            Material chairMaterial,
            Material boardMaterial)
        {
            GameObject root = new GameObject("Classroom_Blockout");

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor_Plane";
            floor.transform.SetParent(root.transform);
            floor.transform.localScale = new Vector3(1.8f, 1f, 1.2f);
            AssignMaterial(floor, floorMaterial);

            CreateCube(root.transform, "Wall_Back", new Vector3(0f, 1.25f, 6f), new Vector3(18f, 2.5f, 0.25f), wallMaterial);
            CreateCube(root.transform, "Wall_Front_Low", new Vector3(0f, 0.75f, -6f), new Vector3(18f, 1.5f, 0.25f), wallMaterial);
            CreateCube(root.transform, "Wall_Left", new Vector3(-9f, 1.25f, 0f), new Vector3(0.25f, 2.5f, 12f), wallMaterial);
            CreateCube(root.transform, "Wall_Right", new Vector3(9f, 1.25f, 0f), new Vector3(0.25f, 2.5f, 12f), wallMaterial);

            CreateCube(root.transform, "Blackboard_Replaceable", new Vector3(0f, 1.7f, 5.82f), new Vector3(5.5f, 1.5f, 0.08f), boardMaterial);
            CreateCube(root.transform, "TeacherDesk_Blockout", new Vector3(0f, 0.45f, 3.9f), new Vector3(3.2f, 0.9f, 1.1f), deskMaterial);
            CreateCube(root.transform, "DoorMarker_Blockout", new Vector3(7.8f, 1.05f, -5.82f), new Vector3(1.2f, 2.1f, 0.1f), chairMaterial);

            int index = 1;
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    float x = -5.4f + column * 3.6f;
                    float z = 1.4f - row * 2.2f;
                    CreateStudentDeskSet(root.transform, index, new Vector3(x, 0f, z), deskMaterial, chairMaterial);
                    index++;
                }
            }
        }

        private static void CreateStudentDeskSet(Transform parent, int index, Vector3 basePosition, Material deskMaterial, Material chairMaterial)
        {
            GameObject setRoot = new GameObject($"StudentDeskSet_{index:00}_Replaceable");
            setRoot.transform.SetParent(parent);
            setRoot.transform.position = basePosition;

            CreateCube(setRoot.transform, "DeskTop", new Vector3(0f, 0.55f, 0f), new Vector3(1.25f, 0.18f, 0.85f), deskMaterial);
            CreateCube(setRoot.transform, "DeskBase", new Vector3(0f, 0.28f, 0f), new Vector3(1.05f, 0.55f, 0.55f), deskMaterial);
            CreateCube(setRoot.transform, "ChairSeat", new Vector3(0f, 0.32f, -0.95f), new Vector3(0.85f, 0.18f, 0.75f), chairMaterial);
            CreateCube(setRoot.transform, "ChairBack", new Vector3(0f, 0.85f, -1.28f), new Vector3(0.85f, 1.05f, 0.16f), chairMaterial);
        }

        private static void CreatePreviewCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.fieldOfView = 55f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 120f;
            cameraObject.transform.position = new Vector3(0f, 7.5f, -8.5f);
            cameraObject.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        }

        private static void CreatePlayerPrefabAndSceneInstance(
            Material bodyMaterial,
            Material headMaterial,
            Material backpackMaterial,
            Material markerMaterial)
        {
            GameObject playerRoot = new GameObject("PlayerRoot");
            playerRoot.transform.position = new Vector3(0f, 0.05f, -4.2f);

            CharacterController controller = playerRoot.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 0.95f, 0f);
            controller.height = 1.85f;
            controller.radius = 0.36f;
            controller.stepOffset = 0.25f;

            GameObject playerVisual = new GameObject("PlayerVisual");
            playerVisual.transform.SetParent(playerRoot.transform);
            playerVisual.transform.localPosition = Vector3.zero;
            playerVisual.transform.localRotation = Quaternion.identity;
            playerVisual.transform.localScale = Vector3.one;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(playerVisual.transform);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.56f, 0.72f, 0.42f);
            RemoveCollider(body);
            AssignMaterial(body, bodyMaterial);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Head";
            head.transform.SetParent(playerVisual.transform);
            head.transform.localPosition = new Vector3(0f, 1.68f, 0.02f);
            head.transform.localScale = new Vector3(0.48f, 0.42f, 0.46f);
            RemoveCollider(head);
            AssignMaterial(head, headMaterial);

            GameObject backpack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backpack.name = "Backpack";
            backpack.transform.SetParent(playerVisual.transform);
            backpack.transform.localPosition = new Vector3(0f, 0.96f, -0.38f);
            backpack.transform.localScale = new Vector3(0.52f, 0.78f, 0.18f);
            RemoveCollider(backpack);
            AssignMaterial(backpack, backpackMaterial);

            GameObject facingMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            facingMarker.name = "ForwardMarker";
            facingMarker.transform.SetParent(playerVisual.transform);
            facingMarker.transform.localPosition = new Vector3(0f, 1.08f, 0.36f);
            facingMarker.transform.localScale = new Vector3(0.18f, 0.18f, 0.08f);
            RemoveCollider(facingMarker);
            AssignMaterial(facingMarker, markerMaterial);

            GameObject headPoint = new GameObject("HeadPoint");
            headPoint.transform.SetParent(playerRoot.transform);
            headPoint.transform.localPosition = new Vector3(0f, 1.72f, 0.16f);

            GameObject cameraTarget = new GameObject("CameraTarget");
            cameraTarget.transform.SetParent(playerRoot.transform);
            cameraTarget.transform.localPosition = new Vector3(0f, 1.35f, 0f);

            CharacterModelSockets sockets = playerRoot.AddComponent<CharacterModelSockets>();
            sockets.Assign(playerVisual.transform, headPoint.transform, cameraTarget.transform);

            PrefabUtility.SaveAsPrefabAssetAndConnect(playerRoot, PlayerPrefabPath, InteractionMode.AutomatedAction);
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;
            AssignMaterial(cube, material);
            return cube;
        }

        private static void AssignMaterial(GameObject gameObject, Material material)
        {
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static void RemoveCollider(GameObject gameObject)
        {
            Collider collider = gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }
    }
}
