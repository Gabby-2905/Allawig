using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Recreates the Player GameObject in GameScene with:
///   - CharacterController
///   - GamePlayerMovement
///   - Arnold FBX as child visual (with Animator)
///   - Main Camera wired to GameCameraFollow targeting the Player
/// </summary>
public class AddPlayerToGameScene
{
    [MenuItem("Tools/Add Player To GameScene")]
    public static void Execute()
    {
        // ── 1. Open GameScene ─────────────────────────────────────────────
        var scene = EditorSceneManager.OpenScene(
            "Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        if (!scene.IsValid())
        {
            Debug.LogError("[AddPlayer] Could not open GameScene.unity");
            return;
        }

        SceneManager.SetActiveScene(scene);

        // ── 2. Guard: don't add twice ─────────────────────────────────────
        var existing = GameObject.FindWithTag("Player");
        if (existing != null)
        {
            Debug.Log("[AddPlayer] Player already exists in GameScene.");
            return;
        }

        // ── 3. Create Player root ─────────────────────────────────────────
        var player = new GameObject("Player");
        player.tag = "Player";

        // CharacterController
        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.stepOffset = 0.3f;
        cc.slopeLimit = 45f;

        // GamePlayerMovement
        var gpm = player.AddComponent<GamePlayerMovement>();
        gpm.walkSpeed        = 4f;
        gpm.runSpeed         = 9f;
        gpm.rotationSpeed    = 720f;
        gpm.maxStamina       = 100f;
        gpm.staminaDrainRate = 20f;
        gpm.staminaRegenRate = 12f;
        gpm.staminaRegenDelay = 1.5f;
        gpm.maxHealth        = 100f;
        gpm.currentHealth    = 100f;
        gpm.punchCooldown    = 0.6f;
        gpm.shootCooldown    = 0.4f;
        gpm.hasGun           = false;
        gpm.crouchSpeed      = 1.8f;
        gpm.crouchHeightScale = 0.55f;

        // ── 4. Spawn Arnold FBX as child visual ───────────────────────────
        string arnoldPath = "Assets/Arnold/Arnold.fbx";
        var arnoldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(arnoldPath);
        GameObject arnoldGO = null;

        if (arnoldPrefab != null)
        {
            arnoldGO = (GameObject)PrefabUtility.InstantiatePrefab(arnoldPrefab, scene);
            arnoldGO.name = "Arnold";
            arnoldGO.transform.SetParent(player.transform, false);
            arnoldGO.transform.localPosition = Vector3.zero;
            arnoldGO.transform.localRotation = Quaternion.identity;

            // Disable root motion so GamePlayerMovement drives movement
            var anim = arnoldGO.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.applyRootMotion = false;
                gpm.animator = anim;
            }
        }
        else
        {
            Debug.LogWarning("[AddPlayer] Arnold.fbx not found at " + arnoldPath + ". Player created without visual.");
        }

        // ── 5. Position player on the terrain ────────────────────────────
        // Find terrain to place player on top of it
        var terrain = Object.FindFirstObjectByType<Terrain>();
        if (terrain != null)
        {
            Vector3 tc = terrain.transform.position;
            Vector3 ts = terrain.terrainData != null ? terrain.terrainData.size : new Vector3(500, 0, 500);
            float cx = tc.x + ts.x * 0.5f;
            float cz = tc.z + ts.z * 0.5f;
            float y  = terrain.SampleHeight(new Vector3(cx, 0, cz)) + tc.y + 1f;
            player.transform.position = new Vector3(cx, y, cz);
        }
        else
        {
            player.transform.position = new Vector3(0f, 1f, 0f);
        }

        // ── 6. Wire up Main Camera ────────────────────────────────────────
        var cam = Camera.main;
        if (cam != null)
        {
            // Remove old CameraFollow if present, add GameCameraFollow
            var oldFollow = cam.GetComponent<CameraFollow>();
            if (oldFollow != null) Object.DestroyImmediate(oldFollow);

            var camFollow = cam.GetComponent<GameCameraFollow>();
            if (camFollow == null) camFollow = cam.gameObject.AddComponent<GameCameraFollow>();
            camFollow.target = player.transform;
            camFollow.offset = new Vector3(0f, 13f, -9f);
            camFollow.smoothSpeed = 8f;
        }
        else
        {
            Debug.LogWarning("[AddPlayer] No Main Camera found in scene.");
        }

        // ── 7. Save ───────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log(saved
            ? "[AddPlayer] Player added and GameScene saved successfully."
            : "[AddPlayer] WARNING: GameScene save failed.");
    }
}
