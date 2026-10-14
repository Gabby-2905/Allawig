using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class FixControlsPanelLayout
{
    [MenuItem("Tools/Fix Controls Panel Layout")]
    public static void Execute()
    {
        GameObject controlsPanel = null;
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go.name == "ControlsPanel") { controlsPanel = go; break; }
        }

        if (controlsPanel == null) { Debug.LogError("ControlsPanel not found."); return; }

        // Panel is 460px wide. Each row should be 400px wide, centered.
        float rowWidth  = 400f;
        float rowHeight = 56f;
        float iconSize  = 44f;
        float spacing   = 14f;
        float startY    = -130f;   // first row anchoredPosition.y
        float stepY     = -60f;    // gap between rows

        for (int i = 0; i < 5; i++)
        {
            Transform lineT = controlsPanel.transform.Find($"Line_{i}");
            if (lineT == null) continue;

            // ── Row RectTransform: centered, fixed size ────────────────────
            RectTransform rt = lineT.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 1f);
            rt.anchorMax        = new Vector2(0.5f, 1f);
            rt.pivot            = new Vector2(0.5f, 1f);
            rt.sizeDelta        = new Vector2(rowWidth, rowHeight);
            rt.anchoredPosition = new Vector2(0f, startY + i * stepY);

            // ── HorizontalLayoutGroup ──────────────────────────────────────
            HorizontalLayoutGroup hlg = lineT.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = lineT.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing                = spacing;
            hlg.childAlignment         = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth      = false;
            hlg.childControlHeight     = false;
            hlg.padding                = new RectOffset(10, 10, 0, 0);

            // ── Icon ───────────────────────────────────────────────────────
            Transform iconT = lineT.Find("Icon");
            if (iconT != null)
            {
                RectTransform iconRT = iconT.GetComponent<RectTransform>();
                iconRT.sizeDelta = new Vector2(iconSize, iconSize);
                // Reset anchors so HLG controls position
                iconRT.anchorMin = new Vector2(0, 0.5f);
                iconRT.anchorMax = new Vector2(0, 0.5f);
                iconRT.pivot     = new Vector2(0, 0.5f);
            }

            // ── Label ──────────────────────────────────────────────────────
            Transform labelT = lineT.Find("Label");
            if (labelT != null)
            {
                RectTransform labelRT = labelT.GetComponent<RectTransform>();
                // Width = row - icon - spacing - padding
                labelRT.sizeDelta = new Vector2(rowWidth - iconSize - spacing - 20f, rowHeight);
                labelRT.anchorMin = new Vector2(0, 0.5f);
                labelRT.anchorMax = new Vector2(0, 0.5f);
                labelRT.pivot     = new Vector2(0, 0.5f);

                TMP_Text tmp = labelT.GetComponent<TMP_Text>();
                if (tmp != null)
                {
                    tmp.alignment = TextAlignmentOptions.MidlineLeft;
                    tmp.fontSize  = 19f;
                    tmp.color     = Color.white;
                }
            }

            EditorUtility.SetDirty(lineT.gameObject);
        }

        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("Controls panel layout fixed.");
    }
}
