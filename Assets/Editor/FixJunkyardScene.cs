using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Fixes issues in the junkyard scene:
/// - Removes magenta/missing-material power poles
/// - Repositions the scrapyard FBX so it sits on the ground
/// - Positions the camera for a good overview
/// </summary>
public class FixJunkyardScene : EditorWindow
{
    [MenuItem("Tools/Fix Junkyard Scene")]
    public static void Fix()
    {
        // ── 1. Remove magenta power poles ─────────────────────────────────
        RemoveMagentaObjects();

        // ── 2. Fix scrapyard model scale/position ─────────────────────────
        FixScrapyardModel();

        // ── 3. Frame the scene nicely in SceneView ────────────────────────
        FrameScene();

        // ── 4. Save ───────────────────────────────────────────────────────
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("[FixJunkyardScene] Scene fixed successfully!");
    }

    static void RemoveMagentaObjects()
    {
        // Find and destroy all "Power_poles" objects (they have missing materials)
        var allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        var toDestroy = new List<GameObject>();

        foreach (var go in allObjects)
        {
            if (go.name.Contains("Power_poles") || go.name.Contains("power_pole") || go.name.Contains("Power_pole"))
            {
                toDestroy.Add(go);
                continue;
            }

            // Also check for any renderer with a magenta/missing material
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            bool hasMagenta = false;
            foreach (var r in renderers)
            {
                if (r.sharedMaterials != null)
                {
                    foreach (var mat in r.sharedMaterials)
                    {
                        if (mat == null)
                        {
                            hasMagenta = true;
                            break;
                        }
                        // Check if it's the default magenta error material
                        if (mat.name.Contains("Default-Material") && mat.shader != null && mat.shader.name == "Hidden/InternalErrorShader")
                        {
                            hasMagenta = true;
                            break;
                        }
                    }
                }
                if (hasMagenta) break;
            }

            if (hasMagenta && go.transform.parent != null && go.transform.parent.name == "Junkyard_Props")
            {
                toDestroy.Add(go);
            }
        }

        foreach (var go in toDestroy)
        {
            if (go != null)
            {
                Debug.Log($"[FixJunkyardScene] Removing: {go.name}");
                Undo.DestroyObjectImmediate(go);
            }
        }
    }

    static void FixScrapyardModel()
    {
        // Find the scrapyard FBX and make sure it's properly positioned
        var scrap = GameObject.Find("1stMap");
        if (scrap != null)
        {
            // Place it at center, scale it appropriately
            scrap.transform.position = new Vector3(0f, 0f, 0f);
            scrap.transform.localScale = Vector3.one * 0.3f;
            Debug.Log("[FixJunkyardScene] Adjusted scrapyard model.");
        }
    }

    static void FrameScene()
    {
        // Position the SceneView camera for a nice isometric-ish overview
        SceneView sv = SceneView.lastActiveSceneView;
        if (sv == null) sv = SceneView.sceneViews.Count > 0 ? (SceneView)SceneView.sceneViews[0] : null;
        if (sv == null) return;

        sv.pivot = new Vector3(0f, 0f, 0f);
        sv.rotation = Quaternion.Euler(35f, -45f, 0f);
        sv.size = 60f;
        sv.orthographic = false;
        sv.Repaint();
    }
}
