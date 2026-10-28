using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public class SetupCityAsLevel2
{
    [MenuItem("Tools/Setup City As Level 2")]
    public static void Execute()
    {
        // ── 1. Copy the City scene to Assets/Scenes/Level2Scene.unity ──────
        string srcPath = "Assets/SyntyStudios/PolygonCity/Scenes/City.unity";
        string dstPath = "Assets/Scenes/Level2Scene.unity";

        // Copy the file
        string srcFull = Path.GetFullPath(srcPath);
        string dstFull = Path.GetFullPath(dstPath);
        File.Copy(srcFull, dstFull, overwrite: true);
        AssetDatabase.Refresh();

        // ── 2. Open the copied scene ─────────────────────────────────────
        Scene scene = EditorSceneManager.OpenScene(dstPath, OpenSceneMode.Single);

        // ── 3. Create Player GameObject ──────────────────────────────────
        GameObject player = new GameObject("Player");
        player.tag = "Player";

        // CharacterController
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;

        // GamePlayerMovement
        GamePlayerMovement gpm = player.AddComponent<GamePlayerMovement>();
        gpm.walkSpeed = 4f;
        gpm.runSpeed = 9f;
        gpm.rotationSpeed = 720f;
        gpm.maxStamina = 100f;
        gpm.staminaDrainRate = 20f;
        gpm.staminaRegenRate = 12f;
        gpm.staminaRegenDelay = 1.5f;
        gpm.maxHealth = 100f;
        gpm.currentHealth = 100f;
        gpm.punchCooldown = 0.6f;
        gpm.shootCooldown = 0.4f;
        gpm.crouchSpeed = 1.8f;
        gpm.crouchHeightScale = 0.55f;

        // ── 4. Instantiate the Character_Male_Hoodie prefab as child ─────
        string charPrefabPath = "Assets/SyntyStudios/PolygonCity/Prefabs/Characters/FixedScaleCharacters/Character_Male_Hoodie.prefab";
        GameObject charPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(charPrefabPath);
        GameObject charInstance = null;
        if (charPrefab != null)
        {
            charInstance = (GameObject)PrefabUtility.InstantiatePrefab(charPrefab);
            charInstance.transform.SetParent(player.transform, false);
            charInstance.transform.localPosition = Vector3.zero;
            charInstance.transform.localRotation = Quaternion.identity;
            charInstance.name = "Visual";

            // Assign animator to GamePlayerMovement
            Animator anim = charInstance.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                gpm.animator = anim;

                // Assign the animator controller (same one used in GameScene)
                string controllerGuid = "b674d7bcae567ee488acb11bfb2c24fe";
                string controllerPath = AssetDatabase.GUIDToAssetPath(controllerGuid);
                if (!string.IsNullOrEmpty(controllerPath))
                {
                    RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                    if (controller != null)
                        anim.runtimeAnimatorController = controller;
                }
            }
        }
        else
        {
            Debug.LogWarning("Character_Male_Hoodie prefab not found. Player will have no visual.");
        }

        // ── 5. Position player on a road in the city ─────────────────────
        // The city scene is centered around origin; place player on a road tile
        player.transform.position = new Vector3(0f, 0.5f, 0f);

        // ── 6. Create Main Camera with GameCameraFollow ───────────────────
        // Remove any existing Main Camera
        Camera existingCam = Camera.main;
        if (existingCam != null)
        {
            // Add GameCameraFollow to existing camera
            GameCameraFollow existingFollow = existingCam.GetComponent<GameCameraFollow>();
            if (existingFollow == null)
                existingFollow = existingCam.gameObject.AddComponent<GameCameraFollow>();
            existingFollow.target = player.transform;
            existingFollow.offset = new Vector3(0f, 13f, -9f);
            existingFollow.smoothSpeed = 8f;
            Debug.Log("Added GameCameraFollow to existing Main Camera.");
        }
        else
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            Camera cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 60f;
            camGO.AddComponent<AudioListener>();

            GameCameraFollow follow = camGO.AddComponent<GameCameraFollow>();
            follow.target = player.transform;
            follow.offset = new Vector3(0f, 13f, -9f);
            follow.smoothSpeed = 8f;

            // Position camera
            camGO.transform.position = player.transform.position + new Vector3(0f, 13f, -9f);
            camGO.transform.LookAt(player.transform.position + new Vector3(0f, 0.9f, 4f));
        }

        // ── 7. Create EventSystem if missing ─────────────────────────────
        var eventSystems = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
        if (eventSystems.Length == 0)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── 8. Save the scene ─────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, dstPath);

        Debug.Log("Level 2 (City) scene set up successfully at: " + dstPath);
    }
}
