using UnityEngine;
using UnityEditor;

/// <summary>
/// Raycasts downward from above the map center to find the actual
/// walkable ground surface, then places the player on it.
/// </summary>
public class FindMapSurface : MonoBehaviour
{
    [MenuItem("Tools/Find Map Surface & Place Player")]
    public static void Execute()
    {
        GameObject mapGO = GameObject.Find("1stMap");
        if (mapGO == null) { Debug.LogError("1stMap not found!"); return; }

        Bounds b = GetBounds(mapGO);
        float mapTopY = b.max.y + 5f;
        float mapCenterX = b.center.x;
        float mapCenterZ = b.center.z;

        // ── Raycast from above center to find ground surface ───────────────
        float groundY = FindGroundY(mapCenterX, mapTopY, mapCenterZ);

        // Try a few positions if center doesn't hit
        if (groundY < 0)
        {
            float[] offsets = { 0f, 5f, -5f, 10f, -10f, 15f, -15f };
            foreach (float ox in offsets)
            {
                foreach (float oz in offsets)
                {
                    groundY = FindGroundY(mapCenterX + ox, mapTopY, mapCenterZ + oz);
                    if (groundY >= 0)
                    {
                        Debug.Log($"Ground found at offset ({ox}, {oz}): Y={groundY:F2}");
                        break;
                    }
                }
                if (groundY >= 0) break;
            }
        }

        if (groundY < 0)
        {
            // Fallback: use the map's min Y + a small offset
            groundY = b.min.y + 1f;
            Debug.LogWarning($"Raycast failed, using fallback ground Y={groundY:F2}");
        }
        else
        {
            Debug.Log($"Ground surface found at Y={groundY:F2}");
        }

        // ── Place player on the ground surface ────────────────────────────
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(mapCenterX, groundY + 0.15f, mapCenterZ);
            EditorUtility.SetDirty(player);
            Debug.Log($"Player placed at Y={groundY + 0.15f:F2}");
        }

        // ── Reposition containers on the ground surface ───────────────────
        float sp = Mathf.Min(b.size.x, b.size.z) * 0.25f;
        Vector3[] positions = {
            new Vector3( sp*.4f, 0, sp*.4f),  new Vector3(-sp*.4f, 0, sp*.4f),
            new Vector3( sp*.4f, 0,-sp*.4f),  new Vector3(-sp*.4f, 0,-sp*.4f),
            new Vector3( sp*.7f, 0, 0),       new Vector3(-sp*.7f, 0, 0),
            new Vector3( 0,      0, sp*.7f),  new Vector3( 0,      0,-sp*.7f),
            new Vector3( sp*.6f, 0, sp*.6f),  new Vector3(-sp*.6f, 0,-sp*.6f),
            new Vector3( sp*.8f, 0, sp*.3f),  new Vector3(-sp*.3f, 0, sp*.8f),
            new Vector3( sp*.5f, 0,-sp*.7f),  new Vector3(-sp*.7f, 0, sp*.5f),
        };

        int idx = 0;
        foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (!go.name.StartsWith("Container") || idx >= positions.Length) continue;
            Vector3 p = positions[idx++];
            float cy = FindGroundY(p.x, mapTopY, p.z);
            if (cy < 0) cy = groundY;
            go.transform.position = new Vector3(p.x, cy + 0.3f, p.z);
            EditorUtility.SetDirty(go);
        }
        Debug.Log($"Placed {idx} containers on map surface.");

        // ── Save ───────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("✅ Player and containers placed on map surface.");
    }

    static float FindGroundY(float x, float fromY, float z)
    {
        Ray ray = new Ray(new Vector3(x, fromY, z), Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, fromY + 100f))
            return hit.point.y;
        return -1f;
    }

    static Bounds GetBounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.one * 10f);
        Bounds b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }
}
