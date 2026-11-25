using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace KHato.Editor
{
    /// <summary>
    /// KHato (2074) – Etapa 2
    /// Creates the first-person Player hierarchy in SampleScene if it does not exist yet.
    /// Run once via the menu: KHato ▶ Setup Stage 2 Player
    ///
    /// What it creates:
    ///   Player (root)
    ///     └─ CharacterController  Height=7.5  Radius=0.95  Center=(0,3.75,0)  StepOffset=0.6
    ///     └─ FirstPersonController script
    ///     └─ MouseLook script
    ///     └─ CameraHolder (child)   localPosition=(0,7,0)
    ///          └─ Main Camera (reparented, local reset to origin)
    ///
    /// The script is MENU-ONLY: no [InitializeOnLoad] / [InitializeOnLoadMethod].
    /// It is idempotent: running it twice will not create duplicates.
    /// </summary>
    public static class Stage2PlayerSetup
    {
        // ── Approved parameters ───────────────────────────────────────────────
        private const float SpawnX     = 51.80f;
        private const float SpawnY     = 14.20f;   // top of FloorCollider
        private const float SpawnZ     = -42.00f;
        private const float SpawnYaw   = 180f;      // facing south toward PuertaMadera2

        private const float CCHeight   = 7.5f;
        private const float CCRadius   = 0.95f;
        private const float CCCenterY  = 3.75f;
        private const float CCStep     = 0.6f;

        private const float CamHolderY = 7.0f;     // local Y offset for eye-level

        // ─────────────────────────────────────────────────────────────────────

        [MenuItem("KHato/Setup Stage 2 Player")]
        public static void CreatePlayer()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "SampleScene")
            {
                Debug.LogWarning("[KHato] Please open Assets/Scenes/SampleScene.unity first.");
                return;
            }

            // ── 1. Guard: abort if Player already exists ──────────────────────
            var existing = GameObject.Find("Player");
            if (existing != null)
            {
                Debug.Log("[KHato] Player already exists in the scene. No changes made.");
                EditorGUIUtility.PingObject(existing);
                return;
            }

            // ── 2. Create Player root ─────────────────────────────────────────
            var player = new GameObject("Player");
            Undo.RegisterCreatedObjectUndo(player, "Create KHato Player");

            player.transform.position    = new Vector3(SpawnX, SpawnY, SpawnZ);
            player.transform.eulerAngles = new Vector3(0f, SpawnYaw, 0f);

            // ── 3. CharacterController ────────────────────────────────────────
            var cc         = Undo.AddComponent<CharacterController>(player);
            cc.height      = CCHeight;
            cc.radius      = CCRadius;
            cc.center      = new Vector3(0f, CCCenterY, 0f);
            cc.stepOffset  = CCStep;
            cc.slopeLimit  = 45f;
            cc.skinWidth   = 0.08f;
            cc.minMoveDistance = 0.001f;

            // ── 4. FirstPersonController script ───────────────────────────────
            Undo.AddComponent<FirstPersonController>(player);

            // ── 5. CameraHolder child ─────────────────────────────────────────
            var cameraHolder = new GameObject("CameraHolder");
            Undo.RegisterCreatedObjectUndo(cameraHolder, "Create CameraHolder");
            cameraHolder.transform.SetParent(player.transform, false);
            cameraHolder.transform.localPosition = new Vector3(0f, CamHolderY, 0f);
            cameraHolder.transform.localRotation = Quaternion.identity;

            // ── 6. MouseLook script (needs CameraHolder reference) ────────────
            var ml = Undo.AddComponent<MouseLook>(player);
            // Set the serialized field via SerializedObject so Undo works correctly
            var so = new SerializedObject(ml);
            so.FindProperty("cameraHolder").objectReferenceValue = cameraHolder.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            // ── 7. Reparent Main Camera ───────────────────────────────────────
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                Undo.SetTransformParent(mainCam.transform, cameraHolder.transform, "Reparent Main Camera");
                mainCam.transform.localPosition = Vector3.zero;
                mainCam.transform.localRotation = Quaternion.identity;
                mainCam.transform.localScale    = Vector3.one;
                Debug.Log($"[KHato] Main Camera reparented to CameraHolder. AudioListener preserved.");
            }
            else
            {
                Debug.LogWarning("[KHato] No Camera.main found. Reparenting skipped.");
            }

            // ── 8. Save scene ─────────────────────────────────────────────────
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"[KHato] Stage 2 Player created at ({SpawnX}, {SpawnY}, {SpawnZ}), " +
                      $"Yaw={SpawnYaw}°. CharacterController: H={CCHeight}, R={CCRadius}, " +
                      $"Center.Y={CCCenterY}, StepOffset={CCStep}.");

            Selection.activeGameObject = player;
            EditorGUIUtility.PingObject(player);
        }
    }
}
