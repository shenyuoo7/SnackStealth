using System.IO;
using System.Linq;
using SnackStealth.Characters;
using SnackStealth.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SnackStealth.Editor
{
    public static class SnackStealthVisualUpgrade
    {
        private const string KenneyFurnitureFbxRoot = "Assets/ExternalAssets/Kenney/FurnitureKit/Models/FBX format/";

        [MenuItem("SnackStealth/Step 04/Upgrade Low Poly Visuals")]
        public static void UpgradeLowPolyVisuals()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("Exit Play Mode before upgrading visuals.");
                return;
            }

            ApplyToOpenScene();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void ApplyToOpenScene()
        {
            VisualMaterials materials = CreateMaterials();
            UpgradeClassroom(materials);
            UpgradeDeskSets(materials);
            UpgradePlayer(materials);
            UpgradeTeacher(materials);
            UpgradeClassmates(materials);
        }

        private static void UpgradeClassroom(VisualMaterials materials)
        {
            GameObject classroomRoot = GameObject.Find("Classroom_Blockout");
            if (classroomRoot == null)
            {
                return;
            }

            DestroyChildIfExists(classroomRoot.transform, "VisualDressup_Replaceable");

            GameObject dressup = new GameObject("VisualDressup_Replaceable");
            dressup.transform.SetParent(classroomRoot.transform);
            dressup.transform.localPosition = Vector3.zero;
            dressup.transform.localRotation = Quaternion.identity;
            ApplyBaseSurfaceMaterials(materials);
            ConfigureLightingAndCamera();

            for (int i = -4; i <= 4; i++)
            {
                CreateCube(dressup.transform, $"FloorTileLine_X_{i + 5:00}_Replaceable", new Vector3(i * 2f, 0.014f, 0f), new Vector3(0.018f, 0.018f, 11.7f), materials.TileLine);
            }

            for (int i = -3; i <= 3; i++)
            {
                CreateCube(dressup.transform, $"FloorTileLine_Z_{i + 4:00}_Replaceable", new Vector3(0f, 0.016f, i * 1.8f), new Vector3(17.6f, 0.018f, 0.018f), materials.TileLine);
            }

            CreateCube(dressup.transform, "BackWallTrim_Top_Replaceable", new Vector3(0f, 2.52f, 5.82f), new Vector3(17.7f, 0.12f, 0.08f), materials.WallTrim);
            CreateCube(dressup.transform, "BackWallTrim_Bottom_Replaceable", new Vector3(0f, 0.18f, 5.82f), new Vector3(17.7f, 0.14f, 0.08f), materials.WallTrim);
            CreateCube(dressup.transform, "FrontWallTrim_Bottom_Replaceable", new Vector3(0f, 0.18f, -5.82f), new Vector3(17.7f, 0.14f, 0.08f), materials.WallTrim);

            for (int i = 0; i < 3; i++)
            {
                float z = -3.4f + i * 2.6f;
                CreateCube(dressup.transform, $"WindowPane_Left_{i + 1:00}_Replaceable", new Vector3(-8.83f, 1.6f, z), new Vector3(0.05f, 0.82f, 1.35f), materials.WindowGlass);
                CreateCube(dressup.transform, $"WindowFrame_LeftTop_{i + 1:00}_Replaceable", new Vector3(-8.80f, 2.08f, z), new Vector3(0.08f, 0.08f, 1.52f), materials.WindowFrame);
                CreateCube(dressup.transform, $"WindowFrame_LeftBottom_{i + 1:00}_Replaceable", new Vector3(-8.80f, 1.12f, z), new Vector3(0.08f, 0.08f, 1.52f), materials.WindowFrame);
                CreateCube(dressup.transform, $"WindowFrame_LeftFront_{i + 1:00}_Replaceable", new Vector3(-8.80f, 1.6f, z - 0.76f), new Vector3(0.08f, 0.95f, 0.08f), materials.WindowFrame);
                CreateCube(dressup.transform, $"WindowFrame_LeftBack_{i + 1:00}_Replaceable", new Vector3(-8.80f, 1.6f, z + 0.76f), new Vector3(0.08f, 0.95f, 0.08f), materials.WindowFrame);
                CreateCube(dressup.transform, $"WindowPane_Right_{i + 1:00}_Replaceable", new Vector3(8.83f, 1.6f, z), new Vector3(0.05f, 0.82f, 1.35f), materials.WindowGlass);
                CreateCube(dressup.transform, $"WindowFrame_RightTop_{i + 1:00}_Replaceable", new Vector3(8.80f, 2.08f, z), new Vector3(0.08f, 0.08f, 1.52f), materials.WindowFrame);
                CreateCube(dressup.transform, $"WindowFrame_RightBottom_{i + 1:00}_Replaceable", new Vector3(8.80f, 1.12f, z), new Vector3(0.08f, 0.08f, 1.52f), materials.WindowFrame);
                CreateCube(dressup.transform, $"WindowFrame_RightFront_{i + 1:00}_Replaceable", new Vector3(8.80f, 1.6f, z - 0.76f), new Vector3(0.08f, 0.95f, 0.08f), materials.WindowFrame);
                CreateCube(dressup.transform, $"WindowFrame_RightBack_{i + 1:00}_Replaceable", new Vector3(8.80f, 1.6f, z + 0.76f), new Vector3(0.08f, 0.95f, 0.08f), materials.WindowFrame);
            }

            CreateCube(dressup.transform, "QuizPoster_Left_Replaceable", new Vector3(-7.15f, 1.65f, 5.67f), new Vector3(0.72f, 0.92f, 0.035f), materials.Poster);
            CreateCube(dressup.transform, "QuizPoster_Right_Replaceable", new Vector3(7.15f, 1.65f, 5.67f), new Vector3(0.72f, 0.92f, 0.035f), materials.PosterAccent);

            CreateCylinder(dressup.transform, "Clock_Replaceable", new Vector3(6.1f, 2.12f, 5.68f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.34f, 0.035f, 0.34f), materials.Clock);
            CreateExternalModelFitted(dressup.transform, "Bookcase_Left_Kenney_Replaceable", "bookcaseOpen.fbx", new Vector3(-8.32f, 0.82f, 3.72f), Quaternion.Euler(0f, 90f, 0f), new Vector3(0.85f, 1.65f, 1.05f));
            CreateExternalModelFitted(dressup.transform, "Bookcase_Right_Kenney_Replaceable", "bookcaseClosedWide.fbx", new Vector3(8.28f, 0.82f, 2.3f), Quaternion.Euler(0f, -90f, 0f), new Vector3(0.85f, 1.65f, 1.45f));
            CreateExternalModelFitted(dressup.transform, "Plant_FrontCorner_Kenney_Replaceable", "pottedPlant.fbx", new Vector3(-7.65f, 0.58f, -5.08f), Quaternion.identity, new Vector3(0.58f, 1.05f, 0.58f));
            CreateExternalModelFitted(dressup.transform, "Doorway_Kenney_Replaceable", "doorwayOpen.fbx", new Vector3(7.8f, 1.08f, -5.84f), Quaternion.Euler(0f, 180f, 0f), new Vector3(1.42f, 2.15f, 0.18f));

            GameObject teacherDesk = GameObject.Find("TeacherDesk_Blockout");
            if (teacherDesk != null)
            {
                Object.DestroyImmediate(teacherDesk);
            }

            GameObject teacherDeskRoot = new GameObject("TeacherDesk_Replaceable");
            teacherDeskRoot.transform.SetParent(classroomRoot.transform);
            teacherDeskRoot.transform.localPosition = new Vector3(0f, 0f, 3.92f);
                CreateCube(teacherDeskRoot.transform, "TeacherDeskTop_Replaceable", new Vector3(0f, 0.68f, 0f), new Vector3(3.35f, 0.16f, 1.12f), materials.Desk, true);
                CreateCube(teacherDeskRoot.transform, "TeacherDeskFront_Replaceable", new Vector3(0f, 0.38f, 0.36f), new Vector3(3.18f, 0.55f, 0.12f), materials.DeskDark, true);
                CreateCube(teacherDeskRoot.transform, "TeacherDeskDrawer_Left_Replaceable", new Vector3(-1.1f, 0.42f, -0.35f), new Vector3(0.82f, 0.48f, 0.14f), materials.DeskLight);
                CreateCube(teacherDeskRoot.transform, "TeacherDeskDrawer_Right_Replaceable", new Vector3(1.1f, 0.42f, -0.35f), new Vector3(0.82f, 0.48f, 0.14f), materials.DeskLight);
                CreateExternalModelFitted(teacherDeskRoot.transform, "Books_Kenney_Replaceable", "books.fbx", new Vector3(-0.86f, 0.82f, -0.12f), Quaternion.Euler(0f, -18f, 0f), new Vector3(0.44f, 0.18f, 0.34f));
                CreateExternalModelFitted(teacherDeskRoot.transform, "DeskLamp_Kenney_Replaceable", "lampRoundTable.fbx", new Vector3(1.05f, 1.02f, 0.06f), Quaternion.Euler(0f, 25f, 0f), new Vector3(0.36f, 0.54f, 0.36f));

            Transform blackboard = GameObject.Find("Blackboard_Replaceable")?.transform;
            if (blackboard != null)
            {
                DestroyChildIfExists(blackboard, "BlackboardDressup_Replaceable");
                CreateCube(dressup.transform, "BlackboardFrame_Top_Replaceable", new Vector3(0f, 2.47f, 5.74f), new Vector3(5.75f, 0.08f, 0.08f), materials.BoardFrame);
                CreateCube(dressup.transform, "BlackboardFrame_Bottom_Replaceable", new Vector3(0f, 0.93f, 5.74f), new Vector3(5.75f, 0.08f, 0.08f), materials.BoardFrame);
                CreateCube(dressup.transform, "BlackboardFrame_Left_Replaceable", new Vector3(-2.82f, 1.7f, 5.74f), new Vector3(0.08f, 1.58f, 0.08f), materials.BoardFrame);
                CreateCube(dressup.transform, "BlackboardFrame_Right_Replaceable", new Vector3(2.82f, 1.7f, 5.74f), new Vector3(0.08f, 1.58f, 0.08f), materials.BoardFrame);
                CreateCube(dressup.transform, "ChalkLine_01_Replaceable", new Vector3(-1.25f, 1.92f, 5.68f), new Vector3(1.18f, 0.025f, 0.025f), materials.Chalk);
                CreateCube(dressup.transform, "ChalkLine_02_Replaceable", new Vector3(1.1f, 1.52f, 5.68f), new Vector3(1.55f, 0.025f, 0.025f), materials.Chalk);
            }
        }

        private static void UpgradeDeskSets(VisualMaterials materials)
        {
            foreach (GameObject deskSet in FindObjectsByNamePrefix("StudentDeskSet_"))
            {
                DestroyChildrenByPrefix(
                    deskSet.transform,
                    "DeskTop",
                    "DeskBase",
                    "DeskLeg_",
                    "DeskSide_",
                    "DeskBackPanel_",
                    "DeskApron_",
                    "DeskLip_",
                    "DeskCollider_",
                    "ChairSeat",
                    "ChairBack",
                    "ChairLeg_",
                    "ChairFrame_",
                    "ChairFoot_",
                    "ChairCollider_",
                    "KenneyFurnitureVisual_");

                if (TryBuildExternalDeskSet(deskSet.transform))
                {
                    continue;
                }

                CreateCube(deskSet.transform, "DeskTop_Replaceable", new Vector3(0f, 0.56f, 0f), new Vector3(1.36f, 0.12f, 0.9f), materials.Desk, true);
                CreateCube(deskSet.transform, "DeskLip_Front_Replaceable", new Vector3(0f, 0.63f, -0.49f), new Vector3(1.4f, 0.08f, 0.08f), materials.DeskLight);
                CreateCube(deskSet.transform, "DeskBackPanel_Replaceable", new Vector3(0f, 0.34f, 0.38f), new Vector3(1.18f, 0.42f, 0.08f), materials.DeskDark, true);
                CreateCube(deskSet.transform, "DeskSide_Left_Replaceable", new Vector3(-0.62f, 0.34f, 0.02f), new Vector3(0.08f, 0.38f, 0.7f), materials.DeskDark);
                CreateCube(deskSet.transform, "DeskSide_Right_Replaceable", new Vector3(0.62f, 0.34f, 0.02f), new Vector3(0.08f, 0.38f, 0.7f), materials.DeskDark);

                CreateCube(deskSet.transform, "DeskLeg_FL_Replaceable", new Vector3(-0.54f, 0.26f, -0.34f), new Vector3(0.08f, 0.52f, 0.08f), materials.Metal);
                CreateCube(deskSet.transform, "DeskLeg_FR_Replaceable", new Vector3(0.54f, 0.26f, -0.34f), new Vector3(0.08f, 0.52f, 0.08f), materials.Metal);
                CreateCube(deskSet.transform, "DeskLeg_BL_Replaceable", new Vector3(-0.54f, 0.26f, 0.34f), new Vector3(0.08f, 0.52f, 0.08f), materials.Metal);
                CreateCube(deskSet.transform, "DeskLeg_BR_Replaceable", new Vector3(0.54f, 0.26f, 0.34f), new Vector3(0.08f, 0.52f, 0.08f), materials.Metal);

                CreateCube(deskSet.transform, "ChairSeat_Replaceable", new Vector3(0f, 0.31f, -1.02f), new Vector3(0.88f, 0.16f, 0.72f), materials.Chair, true);
                CreateCube(deskSet.transform, "ChairBack_Replaceable", new Vector3(0f, 0.86f, -1.86f), new Vector3(0.9f, 0.92f, 0.14f), materials.Chair, true);
                CreateCube(deskSet.transform, "ChairFrame_Left_Replaceable", new Vector3(-0.48f, 0.62f, -1.42f), Quaternion.Euler(-16f, 0f, 0f), new Vector3(0.07f, 0.9f, 0.07f), materials.Metal);
                CreateCube(deskSet.transform, "ChairFrame_Right_Replaceable", new Vector3(0.48f, 0.62f, -1.42f), Quaternion.Euler(-16f, 0f, 0f), new Vector3(0.07f, 0.9f, 0.07f), materials.Metal);
                CreateCube(deskSet.transform, "ChairLeg_FL_Replaceable", new Vector3(-0.36f, 0.17f, -0.76f), new Vector3(0.07f, 0.34f, 0.07f), materials.Metal);
                CreateCube(deskSet.transform, "ChairLeg_FR_Replaceable", new Vector3(0.36f, 0.17f, -0.76f), new Vector3(0.07f, 0.34f, 0.07f), materials.Metal);
                CreateCube(deskSet.transform, "ChairLeg_BL_Replaceable", new Vector3(-0.36f, 0.17f, -1.26f), new Vector3(0.07f, 0.34f, 0.07f), materials.Metal);
                CreateCube(deskSet.transform, "ChairLeg_BR_Replaceable", new Vector3(0.36f, 0.17f, -1.26f), new Vector3(0.07f, 0.34f, 0.07f), materials.Metal);
            }
        }

        private static void UpgradePlayer(VisualMaterials materials)
        {
            Transform playerVisual = GameObject.Find("PlayerRoot")?.transform.Find("PlayerVisual");
            if (playerVisual == null)
            {
                return;
            }

            DestroyAllChildren(playerVisual);
            CreateStudentBody(playerVisual, "Player", materials.PlayerUniform, materials.Skin, materials.HairDark, materials.Backpack, materials.PlayerAccent, false, out _);
        }

        private static void UpgradeTeacher(VisualMaterials materials)
        {
            Transform teacherRoot = GameObject.Find("TeacherRoot")?.transform;
            Transform teacherVisual = teacherRoot?.Find("TeacherVisual");
            if (teacherVisual == null)
            {
                return;
            }

            DestroyAllChildren(teacherVisual);

            CreateCube(teacherVisual, "TeacherTorso_Replaceable", new Vector3(0f, 0.96f, 0f), new Vector3(0.58f, 0.82f, 0.38f), materials.TeacherJacket);
            CreateCube(teacherVisual, "TeacherShirt_Replaceable", new Vector3(0f, 1.05f, 0.205f), new Vector3(0.32f, 0.58f, 0.035f), materials.TeacherShirt);
            CreateCube(teacherVisual, "TeacherHead_Replaceable", new Vector3(0f, 1.66f, 0.02f), new Vector3(0.46f, 0.42f, 0.42f), materials.Skin);
            CreateCube(teacherVisual, "TeacherHair_Replaceable", new Vector3(0f, 1.91f, -0.02f), new Vector3(0.5f, 0.16f, 0.46f), materials.HairDark);
            CreateCube(teacherVisual, "TeacherGlasses_Replaceable", new Vector3(0f, 1.68f, 0.242f), new Vector3(0.42f, 0.045f, 0.03f), materials.Metal);
            CreateCube(teacherVisual, "TeacherArm_Left_Replaceable", new Vector3(-0.42f, 1.03f, 0.06f), Quaternion.Euler(0f, 0f, -12f), new Vector3(0.13f, 0.64f, 0.16f), materials.TeacherJacket);
            CreateCube(teacherVisual, "TeacherArm_Right_Replaceable", new Vector3(0.43f, 1.04f, 0.12f), Quaternion.Euler(22f, 0f, 16f), new Vector3(0.13f, 0.62f, 0.16f), materials.TeacherJacket);
            CreateCube(teacherVisual, "TeacherHand_Left_Replaceable", new Vector3(-0.49f, 0.68f, 0.08f), new Vector3(0.14f, 0.12f, 0.14f), materials.Skin);
            CreateCube(teacherVisual, "TeacherHand_Right_Replaceable", new Vector3(0.55f, 0.72f, 0.2f), new Vector3(0.14f, 0.12f, 0.14f), materials.Skin);
            CreateCube(teacherVisual, "Clipboard_Replaceable", new Vector3(0.42f, 0.96f, 0.34f), Quaternion.Euler(15f, -8f, -8f), new Vector3(0.42f, 0.55f, 0.05f), materials.Clipboard);
            CreateCube(teacherVisual, "RedPen_Replaceable", new Vector3(0.63f, 0.86f, 0.39f), Quaternion.Euler(0f, 0f, 28f), new Vector3(0.045f, 0.42f, 0.045f), materials.AlertEye);
            CreateCube(teacherVisual, "TeacherLeg_Left_Replaceable", new Vector3(-0.16f, 0.38f, 0f), new Vector3(0.18f, 0.62f, 0.2f), materials.TeacherPants);
            CreateCube(teacherVisual, "TeacherLeg_Right_Replaceable", new Vector3(0.16f, 0.38f, 0f), new Vector3(0.18f, 0.62f, 0.2f), materials.TeacherPants);
        }

        private static void UpgradeClassmates(VisualMaterials materials)
        {
            Transform playerRoot = GameObject.Find("PlayerRoot")?.transform;
            CharacterModelSockets playerSockets = playerRoot != null ? playerRoot.GetComponent<CharacterModelSockets>() : null;
            GameStateController gameState = Object.FindFirstObjectByType<GameStateController>();
            GameObject[] classmates = FindObjectsByNamePrefix("Classmate_").OrderBy(go => go.name).ToArray();

            for (int i = 0; i < classmates.Length; i++)
            {
                GameObject classmateRoot = classmates[i];
                Transform visual = classmateRoot.transform.Find("ClassmateVisual");
                if (visual == null)
                {
                    visual = new GameObject("ClassmateVisual").transform;
                    visual.SetParent(classmateRoot.transform);
                    visual.localPosition = Vector3.zero;
                    visual.localRotation = Quaternion.identity;
                }

                DestroyAllChildren(visual);

                Material uniform = i % 3 == 0 ? materials.ClassmateUniformA : i % 3 == 1 ? materials.ClassmateUniformB : materials.ClassmateUniformC;
                Material hair = i % 2 == 0 ? materials.HairDark : materials.HairWarm;
                CreateStudentBody(visual, "Classmate", uniform, materials.Skin, hair, materials.BackpackMuted, materials.Pencil, true, out Renderer[] alertEyes);

                Transform eyePoint = classmateRoot.transform.Find("ClassmateEyePoint");
                if (eyePoint == null)
                {
                    eyePoint = new GameObject("ClassmateEyePoint").transform;
                    eyePoint.SetParent(classmateRoot.transform);
                }

                eyePoint.localPosition = new Vector3(0f, 1.3f, 0.22f);

                ClassmateActor actor = classmateRoot.GetComponent<ClassmateActor>();
                if (actor != null && playerRoot != null && playerSockets != null)
                {
                    actor.Configure(eyePoint, playerRoot, playerSockets.HeadPoint, gameState, alertEyes);
                    EditorUtility.SetDirty(actor);
                }
            }
        }

        private static void CreateStudentBody(
            Transform parent,
            string prefix,
            Material uniform,
            Material skin,
            Material hair,
            Material backpack,
            Material accent,
            bool addAlertEyes,
            out Renderer[] alertEyes)
        {
            CreateCube(parent, $"{prefix}Torso_Replaceable", new Vector3(0f, 0.94f, 0f), new Vector3(0.48f, 0.66f, 0.32f), uniform);
            CreateCube(parent, $"{prefix}Collar_Replaceable", new Vector3(0f, 1.26f, 0.18f), new Vector3(0.32f, 0.08f, 0.04f), accent);
            CreateCube(parent, $"{prefix}Head_Replaceable", new Vector3(0f, 1.48f, 0.04f), new Vector3(0.38f, 0.34f, 0.36f), skin);
            CreateCube(parent, $"{prefix}Hair_Replaceable", new Vector3(0f, 1.68f, 0.02f), new Vector3(0.42f, 0.13f, 0.38f), hair);
            CreateCube(parent, $"{prefix}Arm_Left_Replaceable", new Vector3(-0.34f, 0.96f, 0.08f), Quaternion.Euler(8f, 0f, -9f), new Vector3(0.12f, 0.46f, 0.14f), uniform);
            CreateCube(parent, $"{prefix}Arm_Right_Replaceable", new Vector3(0.34f, 0.96f, 0.13f), Quaternion.Euler(16f, 0f, 12f), new Vector3(0.12f, 0.46f, 0.14f), uniform);
            CreateCube(parent, $"{prefix}Hand_Left_Replaceable", new Vector3(-0.39f, 0.7f, 0.16f), new Vector3(0.12f, 0.1f, 0.12f), skin);
            CreateCube(parent, $"{prefix}Hand_Right_Replaceable", new Vector3(0.39f, 0.72f, 0.25f), new Vector3(0.12f, 0.1f, 0.12f), skin);
            CreateCube(parent, $"{prefix}Leg_Left_Replaceable", new Vector3(-0.13f, 0.44f, 0f), new Vector3(0.16f, 0.46f, 0.18f), uniform);
            CreateCube(parent, $"{prefix}Leg_Right_Replaceable", new Vector3(0.13f, 0.44f, 0f), new Vector3(0.16f, 0.46f, 0.18f), uniform);
            CreateCube(parent, $"{prefix}Shoe_Left_Replaceable", new Vector3(-0.13f, 0.15f, 0.06f), new Vector3(0.18f, 0.1f, 0.28f), materialsOrDefaultBlack());
            CreateCube(parent, $"{prefix}Shoe_Right_Replaceable", new Vector3(0.13f, 0.15f, 0.06f), new Vector3(0.18f, 0.1f, 0.28f), materialsOrDefaultBlack());
            CreateCube(parent, $"{prefix}Backpack_Replaceable", new Vector3(0f, 0.98f, -0.28f), new Vector3(0.46f, 0.58f, 0.16f), backpack);
            CreateCube(parent, $"{prefix}BackpackStrap_Left_Replaceable", new Vector3(-0.18f, 1.05f, 0.19f), new Vector3(0.055f, 0.48f, 0.04f), backpack);
            CreateCube(parent, $"{prefix}BackpackStrap_Right_Replaceable", new Vector3(0.18f, 1.05f, 0.19f), new Vector3(0.055f, 0.48f, 0.04f), backpack);

            Renderer[] eyes = new Renderer[2];
            CreateCube(parent, $"{prefix}Eye_Left_Replaceable", new Vector3(-0.085f, 1.51f, 0.229f), new Vector3(0.05f, 0.035f, 0.018f), materialsOrDefaultBlack());
            CreateCube(parent, $"{prefix}Eye_Right_Replaceable", new Vector3(0.085f, 1.51f, 0.229f), new Vector3(0.05f, 0.035f, 0.018f), materialsOrDefaultBlack());

            if (addAlertEyes)
            {
                GameObject leftAlert = CreateCube(parent, $"{prefix}AlertEye_Left_VisibleWhenWatching", new Vector3(-0.085f, 1.512f, 0.244f), new Vector3(0.07f, 0.052f, 0.014f), VisualMaterials.SharedAlertEye);
                GameObject rightAlert = CreateCube(parent, $"{prefix}AlertEye_Right_VisibleWhenWatching", new Vector3(0.085f, 1.512f, 0.244f), new Vector3(0.07f, 0.052f, 0.014f), VisualMaterials.SharedAlertEye);
                eyes[0] = leftAlert.GetComponent<Renderer>();
                eyes[1] = rightAlert.GetComponent<Renderer>();
                eyes[0].enabled = false;
                eyes[1].enabled = false;
            }

            alertEyes = eyes;
        }

        private static Material materialsOrDefaultBlack()
        {
            return VisualMaterials.SharedDark;
        }

        private static bool TryBuildExternalDeskSet(Transform deskSet)
        {
            GameObject visual = new GameObject("KenneyFurnitureVisual_Replaceable");
            visual.transform.SetParent(deskSet);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            bool createdDesk = CreateExternalModelFitted(
                visual.transform,
                "Desk_Kenney_Replaceable",
                "desk.fbx",
                new Vector3(0f, 0.38f, 0.04f),
                Quaternion.identity,
                new Vector3(1.36f, 0.72f, 0.9f)) != null;

            bool createdChair = CreateExternalModelFitted(
                visual.transform,
                "Chair_Kenney_Replaceable",
                "chair.fbx",
                new Vector3(0f, 0.48f, -1.02f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(0.88f, 0.96f, 0.78f)) != null;

            if (!createdDesk && !createdChair)
            {
                Object.DestroyImmediate(visual);
                return false;
            }

            CreateBoxColliderOnly(deskSet, "DeskCollider_NavBlocker", new Vector3(0f, 0.36f, 0.04f), new Vector3(1.42f, 0.72f, 0.96f));
            CreateBoxColliderOnly(deskSet, "ChairCollider_NavBlocker", new Vector3(0f, 0.5f, -1.2f), new Vector3(1.02f, 1f, 0.92f));
            return true;
        }

        private static VisualMaterials CreateMaterials()
        {
            VisualMaterials.SharedDark = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_Dark.mat", new Color(0.08f, 0.09f, 0.1f));
            VisualMaterials.SharedAlertEye = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Classmate_AlertEyes.mat", new Color(1f, 0f, 0.02f));

            return new VisualMaterials
            {
                Floor = GetOrCreateProceduralMaterial("Assets/_SnackStealth/Materials/M_Real_FloorTile.mat", "Assets/_SnackStealth/Materials/GeneratedTextures/T_FloorTile.png", ProceduralTextureKind.FloorTile, new Color(0.78f, 0.82f, 0.78f), 0.22f),
                Wall = GetOrCreateProceduralMaterial("Assets/_SnackStealth/Materials/M_Real_WallPlaster.mat", "Assets/_SnackStealth/Materials/GeneratedTextures/T_WallPlaster.png", ProceduralTextureKind.WallPlaster, new Color(0.86f, 0.80f, 0.72f), 0.36f),
                Blackboard = GetOrCreateProceduralMaterial("Assets/_SnackStealth/Materials/M_Real_Blackboard.mat", "Assets/_SnackStealth/Materials/GeneratedTextures/T_BlackboardDust.png", ProceduralTextureKind.Blackboard, new Color(0.08f, 0.24f, 0.16f), 0.18f),
                Desk = GetOrCreateProceduralMaterial("Assets/_SnackStealth/Materials/M_Visual_DeskWarm.mat", "Assets/_SnackStealth/Materials/GeneratedTextures/T_DeskWood.png", ProceduralTextureKind.DeskWood, new Color(0.62f, 0.42f, 0.25f), 0.28f),
                DeskDark = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_DeskDark.mat", new Color(0.39f, 0.25f, 0.15f)),
                DeskLight = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_DeskLight.mat", new Color(0.78f, 0.55f, 0.34f)),
                Chair = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_ChairBlue.mat", new Color(0.2f, 0.38f, 0.72f)),
                Metal = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_Metal.mat", new Color(0.22f, 0.25f, 0.28f)),
                TileLine = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_FloorTileLine.mat", new Color(0.68f, 0.78f, 0.75f)),
                WallTrim = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_WallTrim.mat", new Color(0.86f, 0.91f, 0.88f)),
                WindowGlass = GetOrCreateTransparentMaterial("Assets/_SnackStealth/Materials/M_Visual_WindowGlass.mat", new Color(0.58f, 0.82f, 0.95f, 0.42f)),
                WindowFrame = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_WindowFrame.mat", new Color(0.9f, 0.92f, 0.9f)),
                Poster = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_Poster.mat", new Color(0.95f, 0.7f, 0.24f)),
                PosterAccent = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_PosterAccent.mat", new Color(0.35f, 0.64f, 0.55f)),
                Clock = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_Clock.mat", new Color(0.92f, 0.9f, 0.82f)),
                BoardFrame = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_BoardFrame.mat", new Color(0.28f, 0.18f, 0.1f)),
                Chalk = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_Chalk.mat", new Color(0.92f, 0.94f, 0.88f)),
                PlayerUniform = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_PlayerUniform.mat", new Color(0.18f, 0.35f, 0.72f)),
                PlayerAccent = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_PlayerAccent.mat", new Color(0.95f, 0.77f, 0.16f)),
                ClassmateUniformA = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_ClassmateUniformA.mat", new Color(0.19f, 0.42f, 0.76f)),
                ClassmateUniformB = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_ClassmateUniformB.mat", new Color(0.25f, 0.54f, 0.43f)),
                ClassmateUniformC = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_ClassmateUniformC.mat", new Color(0.52f, 0.36f, 0.64f)),
                TeacherJacket = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_TeacherJacket.mat", new Color(0.48f, 0.12f, 0.16f)),
                TeacherShirt = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_TeacherShirt.mat", new Color(0.94f, 0.88f, 0.76f)),
                TeacherPants = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_TeacherPants.mat", new Color(0.12f, 0.16f, 0.22f)),
                Clipboard = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_Clipboard.mat", new Color(0.78f, 0.65f, 0.48f)),
                Skin = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_Skin.mat", new Color(0.88f, 0.7f, 0.55f)),
                HairDark = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_HairDark.mat", new Color(0.11f, 0.08f, 0.06f)),
                HairWarm = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_HairWarm.mat", new Color(0.31f, 0.17f, 0.08f)),
                Backpack = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_Backpack.mat", new Color(0.16f, 0.16f, 0.2f)),
                BackpackMuted = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_Visual_BackpackMuted.mat", new Color(0.18f, 0.24f, 0.3f)),
                Pencil = GetOrCreateMaterial("Assets/_SnackStealth/Materials/M_PencilPlaceholder.mat", new Color(0.95f, 0.74f, 0.16f)),
                AlertEye = VisualMaterials.SharedAlertEye
            };
        }

        private static Material GetOrCreateMaterial(string assetPath, Color color)
        {
            EnsureParentFolder(assetPath);
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
            material.renderQueue = 3000;

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
            SetMaterialColor(material, color);
            return material;
        }

        private static Material GetOrCreateProceduralMaterial(string materialPath, string texturePath, ProceduralTextureKind textureKind, Color tint, float smoothness)
        {
            Texture2D texture = GetOrCreateProceduralTexture(texturePath, textureKind);
            Material material = GetOrCreateMaterial(materialPath, Color.white);

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }

            SetMaterialColor(material, tint);

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Texture2D GetOrCreateProceduralTexture(string assetPath, ProceduralTextureKind kind)
        {
            EnsureParentFolder(assetPath);

            if (!File.Exists(assetPath))
            {
                Texture2D generated = GenerateTexture(kind, 512);
                File.WriteAllBytes(assetPath, generated.EncodeToPNG());
                AssetDatabase.ImportAsset(assetPath);
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture != null)
            {
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.wrapMode = TextureWrapMode.Repeat;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.mipmapEnabled = true;
                    importer.SaveAndReimport();
                }
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        private static Texture2D GenerateTexture(ProceduralTextureKind kind, int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size;
                    float v = y / (float)size;
                    Color color = kind switch
                    {
                        ProceduralTextureKind.DeskWood => GenerateWoodPixel(u, v),
                        ProceduralTextureKind.FloorTile => GenerateFloorTilePixel(u, v),
                        ProceduralTextureKind.Blackboard => GenerateBlackboardPixel(u, v),
                        _ => GenerateWallPixel(u, v)
                    };

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        private static Color GenerateWoodPixel(float u, float v)
        {
            float grain = Mathf.PerlinNoise(u * 28f, v * 5f) * 0.18f + Mathf.Sin((u * 18f + Mathf.PerlinNoise(u * 3f, v * 3f) * 2f) * Mathf.PI) * 0.045f;
            return Color.Lerp(new Color(0.42f, 0.25f, 0.13f), new Color(0.78f, 0.52f, 0.29f), Mathf.Clamp01(0.48f + grain));
        }

        private static Color GenerateFloorTilePixel(float u, float v)
        {
            float gridU = Mathf.Abs((u * 6f) % 1f - 0.5f);
            float gridV = Mathf.Abs((v * 4f) % 1f - 0.5f);
            float grout = gridU > 0.478f || gridV > 0.472f ? 0.18f : 0f;
            float noise = Mathf.PerlinNoise(u * 18f, v * 18f) * 0.12f;
            Color tile = Color.Lerp(new Color(0.62f, 0.72f, 0.70f), new Color(0.84f, 0.88f, 0.82f), noise + 0.45f);
            return Color.Lerp(tile, new Color(0.38f, 0.46f, 0.45f), grout);
        }

        private static Color GenerateWallPixel(float u, float v)
        {
            float broad = Mathf.PerlinNoise(u * 6f, v * 6f) * 0.13f;
            float fine = Mathf.PerlinNoise(u * 58f, v * 58f) * 0.05f;
            return Color.Lerp(new Color(0.68f, 0.58f, 0.48f), new Color(0.95f, 0.88f, 0.78f), 0.52f + broad + fine);
        }

        private static Color GenerateBlackboardPixel(float u, float v)
        {
            float dust = Mathf.PerlinNoise(u * 36f, v * 36f) * 0.16f + Mathf.PerlinNoise(u * 7f, v * 7f) * 0.14f;
            return Color.Lerp(new Color(0.025f, 0.13f, 0.09f), new Color(0.18f, 0.34f, 0.24f), dust + 0.2f);
        }

        private static void ApplyBaseSurfaceMaterials(VisualMaterials materials)
        {
            AssignMaterial(GameObject.Find("Floor_Plane"), materials.Floor);
            AssignMaterial(GameObject.Find("Wall_Back"), materials.Wall);
            AssignMaterial(GameObject.Find("Wall_Front_Low"), materials.Wall);
            AssignMaterial(GameObject.Find("Wall_Left"), materials.Wall);
            AssignMaterial(GameObject.Find("Wall_Right"), materials.Wall);
            AssignMaterial(GameObject.Find("Blackboard_Replaceable"), materials.Blackboard);
        }

        private static void ConfigureLightingAndCamera()
        {
            RenderSettings.ambientLight = new Color(0.62f, 0.65f, 0.68f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.72f, 0.78f, 0.82f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;

            Light keyLight = GameObject.Find("KeyLight_Directional")?.GetComponent<Light>();
            if (keyLight != null)
            {
                keyLight.intensity = 1.35f;
                keyLight.color = new Color(1f, 0.94f, 0.86f);
                keyLight.transform.rotation = Quaternion.Euler(46f, -30f, 0f);
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.fieldOfView = 68f;
                mainCamera.nearClipPlane = 0.035f;
                mainCamera.farClipPlane = 95f;
                mainCamera.backgroundColor = new Color(0.68f, 0.76f, 0.82f);
            }
        }

        private static GameObject CreateExternalModelFitted(
            Transform parent,
            string name,
            string fileName,
            Vector3 localCenter,
            Quaternion localRotation,
            Vector3 targetSize)
        {
            GameObject instance = CreateExternalModel(parent, name, fileName, Vector3.zero, localRotation, Vector3.one);
            if (instance == null)
            {
                return null;
            }

            if (!TryGetRendererBounds(instance, out Bounds bounds) || bounds.size.sqrMagnitude <= 0.0001f)
            {
                instance.transform.localPosition = localCenter;
                return instance;
            }

            float scale = Mathf.Min(
                targetSize.x / Mathf.Max(bounds.size.x, 0.001f),
                targetSize.y / Mathf.Max(bounds.size.y, 0.001f),
                targetSize.z / Mathf.Max(bounds.size.z, 0.001f));

            instance.transform.localScale = Vector3.one * scale;

            if (TryGetRendererBounds(instance, out Bounds scaledBounds))
            {
                Vector3 targetWorldCenter = parent.TransformPoint(localCenter);
                instance.transform.position += targetWorldCenter - scaledBounds.center;
            }

            return instance;
        }

        private static GameObject CreateExternalModel(
            Transform parent,
            string name,
            string fileName,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(KenneyFurnitureFbxRoot + fileName);
            if (prefab == null)
            {
                return null;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                return null;
            }

            instance.name = name;
            instance.transform.SetParent(parent);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = localScale;
            RemoveCollidersInChildren(instance);
            return instance;
        }

        private static bool TryGetRendererBounds(GameObject root, out Bounds bounds)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            bounds = default;
            bool hasBounds = false;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        private static void RemoveCollidersInChildren(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                if (collider != null)
                {
                    Object.DestroyImmediate(collider);
                }
            }
        }

        private static void CreateBoxColliderOnly(Transform parent, string name, Vector3 center, Vector3 size)
        {
            GameObject blocker = new GameObject(name);
            blocker.transform.SetParent(parent);
            blocker.transform.localPosition = Vector3.zero;
            blocker.transform.localRotation = Quaternion.identity;

            BoxCollider collider = blocker.AddComponent<BoxCollider>();
            collider.center = center;
            collider.size = size;
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

        private static void EnsureParentFolder(string assetPath)
        {
            string parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent) || AssetDatabase.IsValidFolder(parent))
            {
                return;
            }

            string[] parts = parent.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material, bool keepCollider = false)
        {
            return CreateCube(parent, name, localPosition, Quaternion.identity, localScale, material, keepCollider);
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material, bool keepCollider = false)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = localRotation;
            cube.transform.localScale = localScale;
            AssignMaterial(cube, material);

            if (!keepCollider)
            {
                RemoveCollider(cube);
            }

            return cube;
        }

        private static GameObject CreateCylinder(Transform parent, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent);
            cylinder.transform.localPosition = localPosition;
            cylinder.transform.localRotation = localRotation;
            cylinder.transform.localScale = localScale;
            AssignMaterial(cylinder, material);
            RemoveCollider(cylinder);
            return cylinder;
        }

        private static void AssignMaterial(GameObject gameObject, Material material)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
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

        private static void DestroyChildIfExists(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void DestroyAllChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static void DestroyChildrenByPrefix(Transform parent, params string[] prefixes)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (prefixes.Any(prefix => child.name.StartsWith(prefix)))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static GameObject[] FindObjectsByNamePrefix(string prefix)
        {
            return Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.name.StartsWith(prefix))
                .ToArray();
        }

        private enum ProceduralTextureKind
        {
            DeskWood,
            FloorTile,
            WallPlaster,
            Blackboard
        }

        private sealed class VisualMaterials
        {
            public static Material SharedDark;
            public static Material SharedAlertEye;
            public Material Floor;
            public Material Wall;
            public Material Blackboard;
            public Material Desk;
            public Material DeskDark;
            public Material DeskLight;
            public Material Chair;
            public Material Metal;
            public Material TileLine;
            public Material WallTrim;
            public Material WindowGlass;
            public Material WindowFrame;
            public Material Poster;
            public Material PosterAccent;
            public Material Clock;
            public Material BoardFrame;
            public Material Chalk;
            public Material PlayerUniform;
            public Material PlayerAccent;
            public Material ClassmateUniformA;
            public Material ClassmateUniformB;
            public Material ClassmateUniformC;
            public Material TeacherJacket;
            public Material TeacherShirt;
            public Material TeacherPants;
            public Material Clipboard;
            public Material Skin;
            public Material HairDark;
            public Material HairWarm;
            public Material Backpack;
            public Material BackpackMuted;
            public Material Pencil;
            public Material AlertEye;
        }
    }
}
