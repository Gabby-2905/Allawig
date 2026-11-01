using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEditor.SceneManagement;

public class SetupLootTask
{
    [MenuItem("Tools/Setup Loot Task (GameScene)")]
    public static void Execute()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/BebasNeue-Regular SDF.asset");

        // ── 1. LootTaskManager GameObject ─────────────────────────────────
        var existing = GameObject.Find("LootTaskManager");
        if (existing != null) Undo.DestroyObjectImmediate(existing);

        var lootMgrGO = new GameObject("LootTaskManager");
        Undo.RegisterCreatedObjectUndo(lootMgrGO, "Create LootTaskManager");
        var lootMgr = lootMgrGO.AddComponent<LootTaskManager>();
        lootMgr.requiredLoots = 10;

        // Auto-assign exit points
        var exitPoints = Object.FindObjectsByType<ExitPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        lootMgr.exitPoints = exitPoints;
        Debug.Log($"[SetupLootTask] Found {exitPoints.Length} exit points.");

        // ── 2. Make ExitPoints visible ─────────────────────────────────────
        foreach (var ep in exitPoints)
        {
            // Add a visible cylinder marker if no renderer exists
            var renderers = ep.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                // Create a glowing cylinder
                var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cylinder.name = "ExitMarker";
                cylinder.transform.SetParent(ep.transform, false);
                cylinder.transform.localPosition = Vector3.zero;
                cylinder.transform.localScale    = new Vector3(0.8f, 1f, 0.8f);

                // Green emissive material
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0f, 1f, 0.4f, 1f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0f, 0.8f, 0.3f, 1f) * 2f);
                cylinder.GetComponent<Renderer>().material = mat;

                // Remove collider from visual (exit has its own trigger)
                var col = cylinder.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);

                // Add a point light
                var lightGO = new GameObject("ExitLight");
                lightGO.transform.SetParent(ep.transform, false);
                lightGO.transform.localPosition = new Vector3(0, 2f, 0);
                var light = lightGO.AddComponent<Light>();
                light.type      = LightType.Point;
                light.color     = new Color(0f, 1f, 0.4f, 1f);
                light.intensity = 3f;
                light.range     = 8f;
                ep.exitLight    = light;
                EditorUtility.SetDirty(ep);

                // Add a floating label
                var labelGO = new GameObject("ExitLabel");
                labelGO.transform.SetParent(ep.transform, false);
                labelGO.transform.localPosition = new Vector3(0, 2.5f, 0);
                labelGO.transform.localScale    = new Vector3(0.02f, 0.02f, 0.02f);
                var canvas = labelGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                labelGO.AddComponent<CanvasScaler>();
                var labelText = new GameObject("Text");
                labelText.transform.SetParent(labelGO.transform, false);
                var tmp = labelText.AddComponent<TextMeshProUGUI>();
                tmp.text      = "EXIT";
                if (font != null) tmp.font = font;
                tmp.fontSize  = 80;
                tmp.color     = new Color(0f, 1f, 0.4f, 1f);
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                var rt = labelText.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(400, 100);
                rt.anchoredPosition = Vector2.zero;

                Debug.Log($"[SetupLootTask] Added visual to: {ep.name}");
            }
        }

        // ── 3. Add Loot Counter to HUDCanvas ──────────────────────────────
        var hudCanvas = GameObject.Find("HUDCanvas");
        if (hudCanvas != null)
        {
            // Remove old task panel if exists
            var oldPanel = hudCanvas.transform.Find("LootTaskPanel");
            if (oldPanel != null) Undo.DestroyObjectImmediate(oldPanel.gameObject);

            // Task panel
            var panelGO = new GameObject("LootTaskPanel");
            panelGO.transform.SetParent(hudCanvas.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.65f);
            var panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 1f);
            panelRT.anchorMax = new Vector2(0.5f, 1f);
            panelRT.pivot     = new Vector2(0.5f, 1f);
            panelRT.sizeDelta = new Vector2(420, 70);
            panelRT.anchoredPosition = new Vector2(0, -10);

            // Loot counter text
            var counterGO = new GameObject("LootCounterText");
            counterGO.transform.SetParent(panelGO.transform, false);
            var counterTMP = counterGO.AddComponent<TextMeshProUGUI>();
            counterTMP.text      = "LOOTS: 0 / 10";
            if (font != null) counterTMP.font = font;
            counterTMP.fontSize  = 26;
            counterTMP.color     = new Color(1f, 0.85f, 0.2f, 1f);
            counterTMP.fontStyle = FontStyles.Bold;
            counterTMP.alignment = TextAlignmentOptions.Center;
            var counterRT = counterGO.GetComponent<RectTransform>();
            counterRT.anchorMin = new Vector2(0, 0.5f);
            counterRT.anchorMax = new Vector2(1f, 1f);
            counterRT.offsetMin = new Vector2(8, 0);
            counterRT.offsetMax = new Vector2(-8, 0);

            // Status text
            var statusGO = new GameObject("TaskStatusText");
            statusGO.transform.SetParent(panelGO.transform, false);
            var statusTMP = statusGO.AddComponent<TextMeshProUGUI>();
            statusTMP.text      = "🔒 EXIT LOCKED  (10 more loots needed)";
            if (font != null) statusTMP.font = font;
            statusTMP.fontSize  = 16;
            statusTMP.color     = new Color(1f, 0.4f, 0.1f, 1f);
            statusTMP.fontStyle = FontStyles.Normal;
            statusTMP.alignment = TextAlignmentOptions.Center;
            var statusRT = statusGO.GetComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0, 0f);
            statusRT.anchorMax = new Vector2(1f, 0.5f);
            statusRT.offsetMin = new Vector2(8, 0);
            statusRT.offsetMax = new Vector2(-8, 0);

            // Assign to LootTaskManager
            lootMgr.lootCounterText = counterTMP;
            lootMgr.taskStatusText  = statusTMP;
            lootMgr.taskPanel       = panelGO;

            EditorUtility.SetDirty(lootMgrGO);
            Debug.Log("[SetupLootTask] Loot counter HUD added.");
        }
        else
        {
            Debug.LogWarning("[SetupLootTask] HUDCanvas not found!");
        }

        // ── 4. Save ────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[SetupLootTask] Done! Loot task system set up in GameScene.");
    }
}
