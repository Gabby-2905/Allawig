using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FinalizeUrbanMap : EditorWindow
{
    [MenuItem("Tools/Finalize Urban Map")]
    public static void Finalize()
    {
        // ── Reposition camera to match reference image angle ──────────────
        // Reference shows: slightly elevated, looking slightly down-forward
        // from the south-west, city hall centered
        SceneView sv = SceneView.lastActiveSceneView;
        if (sv == null && SceneView.sceneViews.Count > 0)
            sv = (SceneView)SceneView.sceneViews[0];
        if (sv != null)
        {
            sv.pivot = new Vector3(2f, 0f, 5f);       // slightly north of center
            sv.rotation = Quaternion.Euler(40f, -35f, 0f); // elevated south-west view
            sv.size = 72f;
            sv.orthographic = false;
            sv.sceneLighting = true;
            sv.Repaint();
        }

        // ── Ensure scene lighting is on ───────────────────────────────────
        // (already set but make sure)
        RenderSettings.fog = true;
        RenderSettings.fogStartDistance = 130f;
        RenderSettings.fogEndDistance   = 380f;

        // ── Save ──────────────────────────────────────────────────────────
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("[FinalizeUrbanMap] Done!");
    }
}
