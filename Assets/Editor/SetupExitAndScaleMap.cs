using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// 1. Scales 1stMap up to 35,35,35
/// 2. Creates an exit zone at the far end of the map with a visual indicator
/// 3. Creates a Level2Scene and adds both scenes to Build Settings
/// </summary>
public class SetupExitAndScaleMap
{
    [MenuItem("Tools/Setup Exit Point + Scale Map")]
    public static void Execute()
    {
        // ── 1. Scale up the map ────────────────────────────────────────────
        GameObject map = GameObject.Find("1stMap");
        if (map != null)
        {
            Undo.RecordObject(map.transform, "Scale Up Map");
            map.transform.localScale = new Vector3(35f, 35f, 35f);
            Debug.Log("1stMap scaled to 35,35,35");
        }

        // ── 2. Build the Exit Zone ─────────────────────────────────────────
        // Remove any existing exit zone
        GameObject existing = GameObject.Find("ExitZone");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }

        // Root exit zone object
        GameObject exitZone = new GameObject("ExitZone");
        Undo.RegisterCreatedObjectUndo(exitZone, "Create ExitZone");

        // Place it at the far edge of the map (map spans ~-17 to +17 on X/Z at scale 35)
        exitZone.transform.position = new Vector3(0f, 0.5f, -14f);

        // Add BoxCollider trigger (3x2x3 walkable area)
        BoxCollider col = exitZone.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size      = new Vector3(3f, 2f, 3f);

        // Add ExitPoint script
        ExitPoint ep = exitZone.AddComponent<ExitPoint>();
        ep.nextSceneName = "Level2Scene";
        ep.holdDuration  = 2f;

        // ── Visual: glowing green platform ────────────────────────────────
        // Floor pad
        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = "ExitPad";
        Undo.RegisterCreatedObjectUndo(pad, "Create ExitPad");
        pad.transform.SetParent(exitZone.transform);
        pad.transform.localPosition = new Vector3(0f, -0.45f, 0f);
        pad.transform.localScale    = new Vector3(3f, 0.1f, 3f);
        // Remove collider from pad (parent trigger handles it)
        Object.DestroyImmediate(pad.GetComponent<Collider>());

        // Green emissive material
        Material padMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        padMat.color = new Color(0f, 0.9f, 0.4f, 1f);
        padMat.EnableKeyword("_EMISSION");
        padMat.SetColor("_EmissionColor", new Color(0f, 0.6f, 0.2f) * 2f);
        AssetDatabase.CreateAsset(padMat, "Assets/ExitPadMaterial.mat");
        pad.GetComponent<MeshRenderer>().material = padMat;

        // Arrow pillars (4 corners)
        Vector3[] corners = {
            new Vector3( 1.2f, 0f,  1.2f),
            new Vector3(-1.2f, 0f,  1.2f),
            new Vector3( 1.2f, 0f, -1.2f),
            new Vector3(-1.2f, 0f, -1.2f)
        };
        Material pillarMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        pillarMat.color = new Color(0f, 1f, 0.4f, 1f);
        pillarMat.EnableKeyword("_EMISSION");
        pillarMat.SetColor("_EmissionColor", new Color(0f, 0.8f, 0.3f) * 3f);
        AssetDatabase.CreateAsset(pillarMat, "Assets/ExitPillarMaterial.mat");

        foreach (var c in corners)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "ExitPillar";
            Undo.RegisterCreatedObjectUndo(pillar, "Create ExitPillar");
            pillar.transform.SetParent(exitZone.transform);
            pillar.transform.localPosition = c;
            pillar.transform.localScale    = new Vector3(0.12f, 0.8f, 0.12f);
            Object.DestroyImmediate(pillar.GetComponent<Collider>());
            pillar.GetComponent<MeshRenderer>().material = pillarMat;
        }

        // Point light above the zone
        GameObject lightGO = new GameObject("ExitLight");
        Undo.RegisterCreatedObjectUndo(lightGO, "Create ExitLight");
        lightGO.transform.SetParent(exitZone.transform);
        lightGO.transform.localPosition = new Vector3(0f, 2f, 0f);
        Light exitLight = lightGO.AddComponent<Light>();
        exitLight.type      = LightType.Point;
        exitLight.color     = new Color(0f, 1f, 0.4f);
        exitLight.intensity = 3f;
        exitLight.range     = 6f;
        ep.exitLight = exitLight;

        // ── World-space prompt label ───────────────────────────────────────
        GameObject labelGO = new GameObject("ExitLabel");
        Undo.RegisterCreatedObjectUndo(labelGO, "Create ExitLabel");
        labelGO.transform.SetParent(exitZone.transform);
        labelGO.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        labelGO.transform.localScale    = new Vector3(0.02f, 0.02f, 0.02f);

        // We'll use a Canvas in World Space for the label
        Canvas canvas = labelGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform rt = labelGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400f, 120f);

        // Background panel
        GameObject bg = new GameObject("Background");
        Undo.RegisterCreatedObjectUndo(bg, "Create ExitLabel BG");
        bg.transform.SetParent(labelGO.transform, false);
        UnityEngine.UI.Image bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0f, 0.1f, 0.05f, 0.85f);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Text
        GameObject textGO = new GameObject("PromptText");
        Undo.RegisterCreatedObjectUndo(textGO, "Create ExitLabel Text");
        textGO.transform.SetParent(labelGO.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = "STAND TO EXIT";
        tmp.fontSize  = 48f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = new Color(0f, 1f, 0.4f);
        tmp.fontStyle = FontStyles.Bold;
        RectTransform txtRt = textGO.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(8f, 8f);
        txtRt.offsetMax = new Vector2(-8f, -8f);

        // Wire prompt references
        ep.exitPromptUI = labelGO;
        ep.promptText   = tmp;

        // ── 3. Create Level2Scene ──────────────────────────────────────────
        string level2Path = "Assets/Scenes/Level2Scene.unity";
        if (!System.IO.File.Exists(level2Path))
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
            newScene.name = "Level2Scene";
            EditorSceneManager.SaveScene(newScene, level2Path);
            EditorSceneManager.CloseScene(newScene, true);
            Debug.Log("Created Level2Scene at " + level2Path);
        }

        // ── 4. Add both scenes to Build Settings ──────────────────────────
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        string gameScenePath  = "Assets/Scenes/GameScene.unity";
        string mainMenuPath   = "Assets/Scenes/MainMenuScene.unity";

        bool hasMain  = false, hasGame = false, hasLevel2 = false;
        foreach (var s in scenes)
        {
            if (s.path == mainMenuPath)   hasMain   = true;
            if (s.path == gameScenePath)  hasGame   = true;
            if (s.path == level2Path)     hasLevel2 = true;
        }

        if (!hasMain)   scenes.Insert(0, new EditorBuildSettingsScene(mainMenuPath,  true));
        if (!hasGame)   scenes.Add(new EditorBuildSettingsScene(gameScenePath,  true));
        if (!hasLevel2) scenes.Add(new EditorBuildSettingsScene(level2Path,     true));

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Build Settings updated with all scenes.");

        // ── Mark scene dirty & save ────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        Debug.Log("Exit zone created. Map scaled to 35. Scene saved.");
    }
}
