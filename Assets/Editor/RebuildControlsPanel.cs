using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Rebuilds each Line_N in ControlsPanel to show:
///   [Icon 60x60]  [Text label]
/// side by side in a horizontal layout.
/// </summary>
public static class RebuildControlsPanel
{
    // Map: line index -> (icon asset path, label text)
    private static readonly (string icon, string label)[] Lines = new[]
    {
        ("Assets/UI/ControlIcons/icon_wasd.png",      "WASD / Arrow Keys  —  Move"),
        ("Assets/UI/ControlIcons/icon_mouse.png",     "Mouse  —  Look / Aim"),
        ("Assets/UI/ControlIcons/icon_esc.png",       "ESC  —  Pause / Unpause"),
        ("Assets/UI/ControlIcons/icon_e.png",         "E  —  Interact"),
        ("Assets/UI/ControlIcons/icon_leftclick.png", "Left Click  —  Attack"),
    };

    [MenuItem("Tools/Rebuild Controls Panel Icons")]
    public static void Execute()
    {
        // Find ControlsPanel
        GameObject controlsPanel = null;
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go.name == "ControlsPanel") { controlsPanel = go; break; }
        }

        if (controlsPanel == null)
        {
            Debug.LogError("ControlsPanel not found in scene.");
            return;
        }

        for (int i = 0; i < Lines.Length; i++)
        {
            string lineName = $"Line_{i}";
            Transform lineT = controlsPanel.transform.Find(lineName);
            if (lineT == null)
            {
                Debug.LogWarning($"{lineName} not found, skipping.");
                continue;
            }

            GameObject lineGO = lineT.gameObject;

            // ── Load sprite ────────────────────────────────────────────────
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Lines[i].icon);
            if (sprite == null)
            {
                // Force import as sprite
                TextureImporter ti = AssetImporter.GetAtPath(Lines[i].icon) as TextureImporter;
                if (ti != null)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    ti.spriteImportMode = SpriteImportMode.Single;
                    AssetDatabase.ImportAsset(Lines[i].icon, ImportAssetOptions.ForceUpdate);
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Lines[i].icon);
                }
            }

            // ── Widen the line row ─────────────────────────────────────────
            RectTransform lineRT = lineGO.GetComponent<RectTransform>();
            lineRT.sizeDelta = new Vector2(500f, 60f);

            // ── Remove old TMP text from the line root ─────────────────────
            TMP_Text oldTmp = lineGO.GetComponent<TMP_Text>();
            if (oldTmp != null) Object.DestroyImmediate(oldTmp);
            CanvasRenderer oldCR = lineGO.GetComponent<CanvasRenderer>();
            if (oldCR != null) Object.DestroyImmediate(oldCR);

            // ── Add HorizontalLayoutGroup to the row ──────────────────────
            HorizontalLayoutGroup hlg = lineGO.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = lineGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing            = 16f;
            hlg.childAlignment     = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth  = false;
            hlg.childControlHeight = false;
            hlg.padding = new RectOffset(8, 8, 0, 0);

            // ── Clear existing children ────────────────────────────────────
            for (int c = lineGO.transform.childCount - 1; c >= 0; c--)
                Object.DestroyImmediate(lineGO.transform.GetChild(c).gameObject);

            // ── Icon child ────────────────────────────────────────────────
            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(lineGO.transform, false);
            RectTransform iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(50f, 50f);
            Image iconImg = iconGO.AddComponent<Image>();
            iconImg.preserveAspect = true;
            if (sprite != null)
                iconImg.sprite = sprite;
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;

            // ── Text child ────────────────────────────────────────────────
            GameObject textGO = new GameObject("Label");
            textGO.transform.SetParent(lineGO.transform, false);
            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.sizeDelta = new Vector2(420f, 50f);
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = Lines[i].label;
            tmp.fontSize  = 20f;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;

            EditorUtility.SetDirty(lineGO);
            Debug.Log($"Rebuilt {lineName}: {Lines[i].label}");
        }

        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("ControlsPanel rebuilt and scene saved.");
    }
}
