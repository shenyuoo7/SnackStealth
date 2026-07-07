using System.Collections.Generic;
using System.IO;
using System.Linq;
using SnackStealth.AI;
using SnackStealth.Cameras;
using SnackStealth.Characters;
using SnackStealth.Detection;
using SnackStealth.Gameplay;
using SnackStealth.Items;
using SnackStealth.Navigation;
using SnackStealth.Player;
using SnackStealth.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace SnackStealth.Editor
{
    public static class SnackStealthGameplaySetup
    {
        private const string ClassroomScenePath = "Assets/_SnackStealth/Scenes/Classroom_Blockout.unity";
        private const string NavMeshDataPath = "Assets/_SnackStealth/Navigation/Classroom_NavMesh.asset";
        private static readonly string[] SnackNames =
        {
            "\u5de7\u514b\u529b\u68d2",
            "\u6d77\u82d4\u8106\u7247",
            "\u66f2\u5947\u997c\u5e72",
            "\u8292\u679c\u5e72",
            "\u8ff7\u4f60\u4e09\u660e\u6cbb",
            "\u8f6f\u7cd6"
        };

        [MenuItem("SnackStealth/Step 03/Build Quiz Snack Gameplay")]
        public static void BuildQuizSnackGameplay()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("Exit Play Mode before building gameplay.");
                return;
            }

            EnsureFolders();
            AssetDatabase.Refresh();

            GameObject playerRoot = GameObject.Find("PlayerRoot");
            Camera mainCamera = Camera.main;
            if (playerRoot == null || mainCamera == null)
            {
                if (!TryOpenClassroomScene(out playerRoot, out mainCamera))
                {
                    Debug.LogError("Open Classroom_Blockout first. PlayerRoot and Main Camera are required.");
                    return;
                }
            }

            CharacterModelSockets playerSockets = playerRoot.GetComponent<CharacterModelSockets>();
            if (playerSockets == null || !playerSockets.IsComplete)
            {
                Debug.LogError("PlayerRoot needs complete CharacterModelSockets references.");
                return;
            }

            Materials materials = CreateMaterials();

            GameObject systems = CreateGameSystems(materials, out GameStateController gameState, out GameplayEffectSpawner effects);
            Transform[] podiumSpots = CreatePodiumSpots();
            ConfigureFirstPersonRig(mainCamera, materials, out FirstPersonSnackView snackView);
            PlayerSeatInteraction seatInteraction = ConfigurePlayer(playerRoot, playerSockets, mainCamera, gameState, effects, podiumSpots, snackView);

            List<SeatStation> seats = CreateSeatGameplay(playerRoot.transform, playerSockets.HeadPoint, gameState, effects, materials, out SeatStation startingSeat);
            seatInteraction.SetInitialSeat(startingSeat);
            Transform[] patrolPoints = CreatePatrolPoints();
            GameObject teacherRoot = CreateTeacher(materials, playerRoot.transform, playerSockets.HeadPoint, gameState, patrolPoints);

            CreateQuizDressing(materials);
            SnackStealthVisualUpgrade.ApplyToOpenScene();
            NavMeshData navMeshData = BuildClassroomNavMesh();
            CreateNavMeshLoader(navMeshData);
            CreateHud(gameState, seatInteraction);
            EnsureActiveSceneInBuildSettings();

            systems.transform.SetAsFirstSibling();
            Selection.activeGameObject = teacherRoot;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"SnackStealth quiz gameplay generated: {seats.Count} seats, finite desk snacks, classmates, teacher, HUD, and NavMesh.");
        }

        [MenuItem("SnackStealth/Step 02/Build Playable Stealth Prototype")]
        public static void BuildPlayableStealthPrototype()
        {
            BuildQuizSnackGameplay();
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                "Assets/_SnackStealth/Navigation",
                "Assets/_SnackStealth/Materials"
            };

            foreach (string folder in folders)
            {
                string relative = folder.StartsWith("Assets/") ? folder.Substring("Assets/".Length) : folder;
                Directory.CreateDirectory(Path.Combine(Application.dataPath, relative.Replace('/', Path.DirectorySeparatorChar)));
            }
        }

        private static bool TryOpenClassroomScene(out GameObject playerRoot, out Camera mainCamera)
        {
            playerRoot = null;
            mainCamera = null;

            if (!File.Exists(Path.Combine(Application.dataPath, "_SnackStealth/Scenes/Classroom_Blockout.unity")))
            {
                return false;
            }

            EditorSceneManager.OpenScene(ClassroomScenePath, OpenSceneMode.Single);
            playerRoot = GameObject.Find("PlayerRoot");
            mainCamera = Camera.main;
            return playerRoot != null && mainCamera != null;
        }

        private static void EnsureActiveSceneInBuildSettings()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(activeScene.path))
            {
                return;
            }

            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            EditorBuildSettingsScene existing = scenes.FirstOrDefault(scene => scene.path == activeScene.path);
            if (existing == null)
            {
                scenes.Insert(0, new EditorBuildSettingsScene(activeScene.path, true));
            }
            else
            {
                existing.enabled = true;
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static GameObject CreateGameSystems(Materials materials, out GameStateController gameState, out GameplayEffectSpawner effects)
        {
            DestroyIfExists("GameSystems");

            GameObject systems = new GameObject("GameSystems");
            gameState = systems.AddComponent<GameStateController>();
            effects = systems.AddComponent<GameplayEffectSpawner>();
            effects.Configure(gameState, materials.CaughtEffect, materials.EatEffect, materials.KickEffect, materials.VictoryEffect);
            EditorUtility.SetDirty(gameState);
            EditorUtility.SetDirty(effects);
            return systems;
        }

        private static PlayerSeatInteraction ConfigurePlayer(
            GameObject playerRoot,
            CharacterModelSockets sockets,
            Camera mainCamera,
            GameStateController gameState,
            GameplayEffectSpawner effects,
            Transform[] podiumSpots,
            FirstPersonSnackView snackView)
        {
            TopDownFollowCamera topDown = mainCamera.GetComponent<TopDownFollowCamera>();
            if (topDown != null)
            {
                topDown.enabled = false;
                EditorUtility.SetDirty(topDown);
            }

            FirstPersonCameraController olderFirstPerson = mainCamera.GetComponent<FirstPersonCameraController>();
            if (olderFirstPerson != null)
            {
                olderFirstPerson.enabled = false;
                EditorUtility.SetDirty(olderFirstPerson);
            }

            PlayerMovementController movement = playerRoot.GetComponent<PlayerMovementController>();
            if (movement == null)
            {
                movement = playerRoot.AddComponent<PlayerMovementController>();
            }

            movement.ConfigureFirstPerson(mainCamera, sockets.VisualRoot, sockets.HeadPoint);
            movement.ConfigureGameState(gameState);
            EditorUtility.SetDirty(movement);

            PlayerSeatInteraction seatInteraction = playerRoot.GetComponent<PlayerSeatInteraction>();
            if (seatInteraction == null)
            {
                seatInteraction = playerRoot.AddComponent<PlayerSeatInteraction>();
            }

            seatInteraction.Configure(gameState, effects, podiumSpots);
            EditorUtility.SetDirty(seatInteraction);

            PlayerSnackController snackController = playerRoot.GetComponent<PlayerSnackController>();
            if (snackController == null)
            {
                snackController = playerRoot.AddComponent<PlayerSnackController>();
            }

            snackController.Configure(gameState, seatInteraction, effects, snackView);
            EditorUtility.SetDirty(snackController);

            return seatInteraction;
        }

        private static Transform[] CreatePodiumSpots()
        {
            DestroyIfExists("PodiumStandSpots");

            GameObject root = new GameObject("PodiumStandSpots");
            Transform[] spots = new Transform[12];
            for (int i = 0; i < spots.Length; i++)
            {
                GameObject spot = new GameObject($"PodiumSpot_{i + 1:00}");
                spot.transform.SetParent(root.transform);
                float x = -4.8f + (i % 6) * 1.9f;
                float z = 4.68f - (i / 6) * 0.85f;
                spot.transform.SetPositionAndRotation(new Vector3(x, 0.05f, z), Quaternion.Euler(0f, 180f, 0f));
                spots[i] = spot.transform;
            }

            return spots;
        }

        private static void ConfigureFirstPersonRig(Camera mainCamera, Materials materials, out FirstPersonSnackView snackView)
        {
            DestroyIfExists("FirstPersonSnackRig");

            GameObject rig = new GameObject("FirstPersonSnackRig");
            rig.transform.SetParent(mainCamera.transform);
            rig.transform.localPosition = Vector3.zero;
            rig.transform.localRotation = Quaternion.identity;

            GameObject leftHand = CreateCube(rig.transform, "LeftHand_Replaceable", new Vector3(-0.26f, -0.26f, 0.46f), new Vector3(0.16f, 0.12f, 0.32f), materials.Hand);
            GameObject rightHand = CreateCube(rig.transform, "RightHand_Replaceable", new Vector3(0.26f, -0.27f, 0.46f), new Vector3(0.16f, 0.12f, 0.32f), materials.Hand);
            GameObject snack = CreateCube(rig.transform, "HeldSnack_Replaceable", new Vector3(0f, -0.2f, 0.58f), new Vector3(0.22f, 0.08f, 0.18f), materials.Snack);

            snackView = rig.AddComponent<FirstPersonSnackView>();
            snackView.Configure(leftHand.transform, rightHand.transform, snack.transform);
            EditorUtility.SetDirty(snackView);
        }

        private static List<SeatStation> CreateSeatGameplay(
            Transform playerRoot,
            Transform playerHeadPoint,
            GameStateController gameState,
            GameplayEffectSpawner effects,
            Materials materials,
            out SeatStation startingSeat)
        {
            DestroyIfExists("ClassmatesRoot");

            GameObject classmatesRoot = new GameObject("ClassmatesRoot");
            List<SeatStation> seats = new List<SeatStation>();
            GameObject[] deskSets = FindObjectsByNamePrefix("StudentDeskSet_")
                .OrderBy(go => go.name)
                .ToArray();

            int startDeskIndex = FindNearestDeskIndex(deskSets, playerRoot.position);
            startingSeat = null;

            for (int i = 0; i < deskSets.Length; i++)
            {
                GameObject deskSet = deskSets[i];
                RemoveGeneratedChildren(deskSet.transform);

                bool isStartingDesk = i == startDeskIndex;
                Transform sitPoint = CreateMarker(deskSet.transform, "SeatPoint", new Vector3(0f, 0.05f, -1.02f), Quaternion.Euler(0f, 0f, 0f));
                Transform snackPoint = CreateMarker(deskSet.transform, "DeskSnackPoint", new Vector3(0f, 0.32f, 0.18f), Quaternion.identity);

                GameObject snackVisual = CreateCube(deskSet.transform, "HiddenSnack_Replaceable", snackPoint.localPosition, new Vector3(0.38f, 0.12f, 0.24f), materials.Snack);
                SnackStash stash = snackVisual.AddComponent<SnackStash>();
                stash.Configure(isStartingDesk ? "\u7a7a\u5305\u88c5\u888b" : SnackNames[i % SnackNames.Length], isStartingDesk ? 0f : 20f);

                CreateCube(deskSet.transform, "QuizPaper_Replaceable", new Vector3(0f, 0.665f, 0.02f), new Vector3(0.52f, 0.018f, 0.36f), materials.Paper);
                CreateCube(deskSet.transform, "Pencil_Replaceable", new Vector3(0.32f, 0.69f, 0.1f), new Vector3(0.04f, 0.025f, 0.34f), materials.Pencil);

                GameObject classmate = isStartingDesk ? null : CreateClassmate(i + 1, sitPoint, classmatesRoot.transform, playerRoot, playerHeadPoint, gameState, materials);

                SeatStation seat = deskSet.GetComponent<SeatStation>();
                if (seat == null)
                {
                    seat = deskSet.AddComponent<SeatStation>();
                }

                seat.Configure(sitPoint, snackPoint, stash, classmate != null ? classmate.GetComponent<ClassmateActor>() : null);
                EditorUtility.SetDirty(seat);
                seats.Add(seat);

                if (isStartingDesk)
                {
                    startingSeat = seat;
                }
            }

            return seats;
        }

        private static GameObject CreateClassmate(
            int index,
            Transform sitPoint,
            Transform parent,
            Transform playerRoot,
            Transform playerHeadPoint,
            GameStateController gameState,
            Materials materials)
        {
            GameObject root = new GameObject($"Classmate_{index:00}_Root");
            root.transform.SetParent(parent);
            root.transform.SetPositionAndRotation(sitPoint.position, sitPoint.rotation);

            GameObject visual = new GameObject("ClassmateVisual");
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body_Replaceable";
            body.transform.SetParent(visual.transform);
            body.transform.localPosition = new Vector3(0f, 0.72f, 0f);
            body.transform.localScale = new Vector3(0.42f, 0.48f, 0.34f);
            DestroyCollider(body);
            AssignMaterial(body, materials.ClassmateBody);

            GameObject head = CreateCube(visual.transform, "Head_Replaceable", new Vector3(0f, 1.28f, 0.04f), new Vector3(0.34f, 0.32f, 0.32f), materials.ClassmateHead);
            GameObject hand = CreateCube(visual.transform, "WritingHand_Replaceable", new Vector3(0.24f, 0.78f, 0.28f), new Vector3(0.12f, 0.08f, 0.28f), materials.Hand);
            GameObject leftEye = CreateCube(visual.transform, "AlertEye_Left_VisibleWhenWatching", new Vector3(-0.085f, 1.33f, 0.214f), new Vector3(0.055f, 0.045f, 0.018f), materials.AlertEye);
            GameObject rightEye = CreateCube(visual.transform, "AlertEye_Right_VisibleWhenWatching", new Vector3(0.085f, 1.33f, 0.214f), new Vector3(0.055f, 0.045f, 0.018f), materials.AlertEye);
            Renderer[] alertEyes =
            {
                leftEye.GetComponent<Renderer>(),
                rightEye.GetComponent<Renderer>()
            };

            GameObject eyePoint = new GameObject("ClassmateEyePoint");
            eyePoint.transform.SetParent(root.transform);
            eyePoint.transform.localPosition = new Vector3(0f, 1.26f, 0.18f);

            ClassmateActor actor = root.AddComponent<ClassmateActor>();
            actor.Configure(eyePoint.transform, playerRoot, playerHeadPoint, gameState, alertEyes);
            EditorUtility.SetDirty(actor);
            return root;
        }

        private static Transform[] CreatePatrolPoints()
        {
            DestroyIfExists("TeacherPatrolPoints");

            GameObject root = new GameObject("TeacherPatrolPoints");
            Vector3[] positions =
            {
                new Vector3(-6.7f, 0.05f, 3.45f),
                new Vector3(6.7f, 0.05f, 3.45f),
                new Vector3(6.7f, 0.05f, -3.55f),
                new Vector3(-6.7f, 0.05f, -3.55f)
            };

            Transform[] points = new Transform[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject point = new GameObject($"PatrolPoint_{i + 1:00}");
                point.transform.SetParent(root.transform);
                point.transform.position = positions[i];
                points[i] = point.transform;
            }

            return points;
        }

        private static GameObject CreateTeacher(Materials materials, Transform playerRoot, Transform playerHeadPoint, GameStateController gameState, Transform[] patrolPoints)
        {
            DestroyIfExists("TeacherRoot");

            GameObject teacherRoot = new GameObject("TeacherRoot");
            teacherRoot.transform.position = new Vector3(-6.7f, 0.05f, 3.45f);
            teacherRoot.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            NavMeshAgent agent = teacherRoot.AddComponent<NavMeshAgent>();
            agent.speed = 2.05f;
            agent.angularSpeed = 260f;
            agent.acceleration = 10f;
            agent.radius = 0.34f;
            agent.height = 1.85f;
            agent.stoppingDistance = 0.05f;

            TeacherPatrolAgent patrolAgent = teacherRoot.AddComponent<TeacherPatrolAgent>();
            patrolAgent.Configure(patrolPoints);

            GameObject visualRoot = new GameObject("TeacherVisual");
            visualRoot.transform.SetParent(teacherRoot.transform);
            visualRoot.transform.localPosition = Vector3.zero;
            visualRoot.transform.localRotation = Quaternion.identity;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body_Replaceable";
            body.transform.SetParent(visualRoot.transform);
            body.transform.localPosition = new Vector3(0f, 0.92f, 0f);
            body.transform.localScale = new Vector3(0.62f, 0.78f, 0.46f);
            DestroyCollider(body);
            AssignMaterial(body, materials.TeacherBody);

            CreateCube(visualRoot.transform, "Head_Replaceable", new Vector3(0f, 1.76f, 0.03f), new Vector3(0.5f, 0.44f, 0.46f), materials.TeacherHead);
            CreateCube(visualRoot.transform, "QuizClipboard_Replaceable", new Vector3(0.38f, 1.05f, 0.28f), new Vector3(0.12f, 0.55f, 0.36f), materials.TeacherBook);

            Transform eyePoint = CreateMarker(teacherRoot.transform, "TeacherEyePoint", new Vector3(0f, 1.68f, 0.32f), Quaternion.identity);
            CreateMarker(teacherRoot.transform, "TeacherHeadPoint", new Vector3(0f, 1.72f, 0.16f), Quaternion.identity);

            TeacherVisionSensor visionSensor = teacherRoot.AddComponent<TeacherVisionSensor>();
            visionSensor.Configure(eyePoint, playerRoot, playerHeadPoint, gameState, "\u8001\u5e08");

            GameObject cone = new GameObject("VisionCone_DeveloperVisual");
            cone.transform.SetParent(teacherRoot.transform);
            cone.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            cone.transform.localRotation = Quaternion.identity;
            cone.AddComponent<MeshFilter>();
            MeshRenderer coneRenderer = cone.AddComponent<MeshRenderer>();
            coneRenderer.sharedMaterial = materials.VisionCone;
            VisionConeVisualizer coneVisualizer = cone.AddComponent<VisionConeVisualizer>();
            coneVisualizer.Configure(visionSensor);

            return teacherRoot;
        }

        private static void CreateQuizDressing(Materials materials)
        {
            GameObject blackboard = GameObject.Find("Blackboard_Replaceable");
            if (blackboard == null)
            {
                return;
            }

            Transform oldText = blackboard.transform.Find("QuizText_Replaceable");
            if (oldText != null)
            {
                Object.DestroyImmediate(oldText.gameObject);
            }

            GameObject oldWorldText = GameObject.Find("QuizText_Replaceable");
            if (oldWorldText != null)
            {
                Object.DestroyImmediate(oldWorldText);
            }

            GameObject textObject = new GameObject("QuizText_Replaceable");
            textObject.transform.SetParent(GameObject.Find("Classroom_Blockout")?.transform);
            textObject.transform.SetPositionAndRotation(new Vector3(0f, 1.74f, 5.66f), Quaternion.Euler(0f, 180f, 0f));
            textObject.transform.localScale = Vector3.one * 0.025f;

            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = "\u968f\u5802\u6d4b\u9a8c";
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.55f;
            textMesh.fontSize = 48;
            textMesh.color = new Color(0.9f, 0.94f, 0.86f);
        }

        private static NavMeshData BuildClassroomNavMesh()
        {
            GameObject classroomRoot = GameObject.Find("Classroom_Blockout");
            List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
            List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
            int includedLayers = Physics.DefaultRaycastLayers;

            if (classroomRoot != null)
            {
                NavMeshBuilder.CollectSources(classroomRoot.transform, includedLayers, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);
            }
            else
            {
                Bounds sceneBounds = new Bounds(Vector3.zero, new Vector3(24f, 6f, 18f));
                NavMeshBuilder.CollectSources(sceneBounds, includedLayers, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);
            }

            NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);
            settings.agentRadius = 0.34f;
            settings.agentHeight = 1.85f;
            settings.agentClimb = 0.35f;
            settings.agentSlope = 45f;

            Bounds bounds = new Bounds(Vector3.zero, new Vector3(24f, 6f, 18f));
            NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);

            AssetDatabase.DeleteAsset(NavMeshDataPath);
            AssetDatabase.CreateAsset(navMeshData, NavMeshDataPath);
            return AssetDatabase.LoadAssetAtPath<NavMeshData>(NavMeshDataPath);
        }

        private static void CreateNavMeshLoader(NavMeshData navMeshData)
        {
            DestroyIfExists("RuntimeNavMesh");

            GameObject navMeshRoot = new GameObject("RuntimeNavMesh");
            NavMeshDataLoader loader = navMeshRoot.AddComponent<NavMeshDataLoader>();
            loader.Configure(navMeshData);
            EditorUtility.SetDirty(loader);
        }

        private static void CreateHud(GameStateController gameState, PlayerSeatInteraction seatInteraction)
        {
            DestroyIfExists("SnackStealthHUD");

            GameObject hudObject = new GameObject("SnackStealthHUD");
            SnackStealthHud hud = hudObject.AddComponent<SnackStealthHud>();
            hud.Configure(gameState, seatInteraction);
            EditorUtility.SetDirty(hud);
        }

        private static Materials CreateMaterials()
        {
            return new Materials
            {
                TeacherBody = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Teacher_BodyPlaceholder.mat", new Color(0.55f, 0.12f, 0.12f)),
                TeacherHead = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Teacher_HeadPlaceholder.mat", new Color(0.88f, 0.72f, 0.55f)),
                TeacherBook = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Teacher_BookPlaceholder.mat", new Color(0.12f, 0.12f, 0.14f)),
                ClassmateBody = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Classmate_BodyPlaceholder.mat", new Color(0.18f, 0.34f, 0.68f)),
                ClassmateHead = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Classmate_HeadPlaceholder.mat", new Color(0.82f, 0.66f, 0.50f)),
                AlertEye = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Classmate_AlertEyes.mat", new Color(1f, 0f, 0.02f)),
                Hand = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_FirstPerson_HandsPlaceholder.mat", new Color(0.90f, 0.72f, 0.56f)),
                Snack = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_SnackPlaceholder.mat", new Color(0.95f, 0.46f, 0.12f)),
                Paper = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_QuizPaperPlaceholder.mat", new Color(0.96f, 0.96f, 0.90f)),
                Pencil = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_PencilPlaceholder.mat", new Color(0.95f, 0.74f, 0.16f)),
                EatEffect = GetOrCreateTransparentMaterial("Assets/_SnackStealth/Materials/M_EatEffect.mat", new Color(1f, 0.85f, 0.15f, 0.62f)),
                KickEffect = GetOrCreateTransparentMaterial("Assets/_SnackStealth/Materials/M_KickEffect.mat", new Color(0.35f, 0.65f, 1f, 0.62f)),
                CaughtEffect = GetOrCreateTransparentMaterial("Assets/_SnackStealth/Materials/M_CaughtEffect.mat", new Color(1f, 0.1f, 0.05f, 0.62f)),
                VictoryEffect = GetOrCreateTransparentMaterial("Assets/_SnackStealth/Materials/M_VictoryEffect.mat", new Color(0.35f, 1f, 0.35f, 0.62f)),
                VisionCone = GetOrCreateTransparentMaterial("Assets/_SnackStealth/Materials/M_Teacher_VisionCone.mat", new Color(1f, 0.8f, 0.1f, 0.22f))
            };
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

            SetMaterialColor(material, color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material GetOrCreateTransparentMaterial(string assetPath, Color color)
        {
            Material material = GetOrCreateMaterial(assetPath, color);
            material.renderQueue = (int)RenderQueue.Transparent;

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_SrcBlend"))
            {
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            }

            if (material.HasProperty("_DstBlend"))
            {
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            }

            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
            SetMaterialColor(material, color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private static Transform CreateMarker(Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
        {
            GameObject marker = new GameObject(name);
            marker.transform.SetParent(parent);
            marker.transform.localPosition = localPosition;
            marker.transform.localRotation = localRotation;
            return marker.transform;
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;
            DestroyCollider(cube);
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

        private static void DestroyCollider(GameObject gameObject)
        {
            Collider collider = gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }

        private static void DestroyIfExists(string objectName)
        {
            GameObject existing = GameObject.Find(objectName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }

        private static GameObject[] FindObjectsByNamePrefix(string prefix)
        {
            return Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.name.StartsWith(prefix))
                .ToArray();
        }

        private static int FindNearestDeskIndex(GameObject[] deskSets, Vector3 position)
        {
            int nearestIndex = 0;
            float nearestSqrDistance = float.PositiveInfinity;

            for (int i = 0; i < deskSets.Length; i++)
            {
                if (deskSets[i] == null)
                {
                    continue;
                }

                float sqrDistance = (deskSets[i].transform.position - position).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        private static void RemoveGeneratedChildren(Transform parent)
        {
            string[] generatedPrefixes =
            {
                "SeatPoint",
                "DeskSnackPoint",
                "HiddenSnack_",
                "QuizPaper_",
                "Pencil_"
            };

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (generatedPrefixes.Any(prefix => child.name.StartsWith(prefix)))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private sealed class Materials
        {
            public Material TeacherBody;
            public Material TeacherHead;
            public Material TeacherBook;
            public Material ClassmateBody;
            public Material ClassmateHead;
            public Material AlertEye;
            public Material Hand;
            public Material Snack;
            public Material Paper;
            public Material Pencil;
            public Material EatEffect;
            public Material KickEffect;
            public Material CaughtEffect;
            public Material VictoryEffect;
            public Material VisionCone;
        }
    }
}
