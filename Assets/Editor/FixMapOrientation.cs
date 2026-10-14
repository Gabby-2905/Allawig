using UnityEngine;
using UnityEditor;

/// <summary>
/// Fixes the 1stMap orientation: rotates it flat, rescales to a large playable area,
/// and repositions the player on top of it.
/// </summary>
public class FixMapOrientation : MonoBehaviour
{
    [MenuItem("Tools/Fix 1stMap Orientation & Scale")]
    public static void Execute()
    {
        GameObject mapGO = GameObject.Find("1stMap");
        if (mapGO == null) { Debug.LogError("1stMap not found in scene!"); return; }

        // ── Step 1: Rotate -90 on X so the map lies flat (Z-up → Y-up) ────
        mapGO.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        mapGO.transform.localScale = Vector3.one;
        mapGO.transform.position = Vector3.zero;

        // ── Step 2: Measure bounds after rotation ──────────────────────────
        Bounds b = GetBounds(mapGO);
        Debug.Log($"After rotation - bounds size: {b.size}, center: {b.center}");

        // ── Step 3: Scale to large playable map ────────────────────────────
        // Target the larger horizontal dimension to be ~200 units
        float naturalFloorSize = Mathf.Max(b.size.x, b.size.z);
        float targetSize = 200f;
        float scale = (naturalFloorSize > 0.01f) ? (targetSize / naturalFloorSize) : 20f;
        scale = Mathf.Clamp(scale, 5f, 100f);

        mapGO.transform.localScale = Vector3.one * scale;

        // ── Step 4: Reposition so floor sits at Y=0 ────────────────────────
        b = GetBounds(mapGO);
        mapGO.transform.position = new Vector3(
            -b.center.x,
            -b.min.y,
            -b.center.z
        );

        b = GetBounds(mapGO);
        Debug.Log($"Final bounds: size={b.size}, min={b.min}, max={b.max}");

        // ── Step 5: Reposition Player on the map surface ───────────────────
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0f, b.min.y + 0.2f, 0f);
            EditorUtility.SetDirty(player);
            Debug.Log($"Player placed at Y={b.min.y + 0.2f:F2}");
        }

        // ── Step 6: Reposition Rain above the map ─────────────────────────
        GameObject rain = GameObject.Find("Rain");
        if (rain != null)
        {
            rain.transform.position = new Vector3(0f, b.max.y + 25f, 0f);
            EditorUtility.SetDirty(rain);
        }

        // ── Step 7: Spread containers across the map ──────────────────────
        float surfaceY = b.min.y + 0.5f;
        float spread = Mathf.Min(b.size.x, b.size.z) * 0.3f;

        Vector3[] positions = new Vector3[]
        {
            new Vector3( spread * 0.4f,  surfaceY,  spread * 0.4f),
            new Vector3(-spread * 0.4f,  surfaceY,  spread * 0.4f),
            new Vector3( spread * 0.4f,  surfaceY, -spread * 0.4f),
            new Vector3(-spread * 0.4f,  surfaceY, -spread * 0.4f),
            new Vector3( spread * 0.7f,  surfaceY,  0f),
            new Vector3(-spread * 0.7f,  surfaceY,  0f),
            new Vector3( 0f,             surfaceY,  spread * 0.7f),
            new Vector3( 0f,             surfaceY, -spread * 0.7f),
            new Vector3( spread * 0.6f,  surfaceY,  spread * 0.6f),
            new Vector3(-spread * 0.6f,  surfaceY, -spread * 0.6f),
            new Vector3( spread * 0.8f,  surfaceY,  spread * 0.3f),
            new Vector3(-spread * 0.3f,  surfaceY,  spread * 0.8f),
            new Vector3( spread * 0.5f,  surfaceY, -spread * 0.7f),
            new Vector3(-spread * 0.7f,  surfaceY,  spread * 0.5f),
        };

        int idx = 0;
        foreach (GameObject go in UnityEngine.SceneManagement.SceneManager
            .GetActiveScene().GetRootGameObjects())
        {
            if (go.name.StartsWith("Container") && idx < positions.Length)
            {
                go.transform.position = positions[idx++];
                EditorUtility.SetDirty(go);
            }
        }
        Debug.Log($"Repositioned {idx} containers.");

        // ── Save ───────────────────────────────────────────────────────────
        EditorUtility.SetDirty(mapGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"✅ 1stMap fixed! Scale={scale:F1}x, Floor area: {b.size.x:F0}x{b.size.z:F0} units.");
    }

    static Bounds GetBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one * 10f);
        Bounds b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }
}
