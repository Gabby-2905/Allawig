using UnityEngine;
using UnityEditor;

/// <summary>
/// Finds the actual floor/road level of the map by raycasting at multiple
/// points and picking the lowest consistent hit — that's the ground floor.
/// </summary>
public class FindFloorLevel : MonoBehaviour
{
    [MenuItem("Tools/Find Floor Level & Place Player")]
    public static void Execute()
    {
        GameObject mapGO = GameObject.Find("1stMap");
        if (mapGO == null) { Debug.LogError("1stMap not found!"); return; }

        Bounds b = GetBounds(mapGO);
        float fromY = b.max.y + 10f;

        // Sample many points across the map and collect all hit Y values
        System.Collections.Generic.List<float> hitYValues = new System.Collections.Generic.List<float>();

        int samples = 20;
        float stepX = b.size.x / samples;
        float stepZ = b.size.z / samples;

        for (int xi = 0; xi <= samples; xi++)
        {
            for (int zi = 0; zi <= samples; zi++)
            {
                float x = b.min.x + xi * stepX;
                float z = b.min.z + zi * stepZ;

                Ray ray = new Ray(new Vector3(x, fromY, z), Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, fromY + 50f))
                {
                    hitYValues.Add(hit.point.y);
                }
            }
        }

        if (hitYValues.Count == 0)
        {
            Debug.LogError("No raycast hits found! Check MeshCollider.");
            return;
        }

        // Sort and find the most common low value (the ground floor)
        hitYValues.Sort();

        // The floor is the lowest 20% of hits (ignoring outliers below map)
        int floorSampleCount = Mathf.Max(1, hitYValues.Count / 5);
        float sumLow = 0f;
        for (int i = 0; i < floorSampleCount; i++)
            sumLow += hitYValues[i];
        float floorY = sumLow / floorSampleCount;

        // Also find the median for comparison
        float medianY = hitYValues[hitYValues.Count / 2];

        Debug.Log($"Hit count: {hitYValues.Count}, Min Y: {hitYValues[0]:F2}, Max Y: {hitYValues[hitYValues.Count-1]:F2}");
        Debug.Log($"Floor Y (avg of lowest 20%): {floorY:F2}, Median Y: {medianY:F2}");

        // Use the floor Y — this is the road/ground level
        float playerY = floorY + 0.2f;

        // Find a good spawn point on the road (near the perimeter road)
        // Try the circular road area — offset from center
        float roadOffset = b.size.x * 0.25f; // ~25% from center = road ring area
        float spawnX = roadOffset;
        float spawnZ = 0f;

        // Raycast at the road position
        Ray spawnRay = new Ray(new Vector3(spawnX, fromY, spawnZ), Vector3.down);
        RaycastHit spawnHit;
        if (Physics.Raycast(spawnRay, out spawnHit, fromY + 50f))
        {
            playerY = spawnHit.point.y + 0.2f;
            spawnX = spawnHit.point.x;
            spawnZ = spawnHit.point.z;
            Debug.Log($"Spawn point on road: ({spawnX:F1}, {playerY:F2}, {spawnZ:F1})");
        }

        // ── Place player ───────────────────────────────────────────────────
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(spawnX, playerY, spawnZ);
            EditorUtility.SetDirty(player);
            Debug.Log($"Player placed at ({spawnX:F1}, {playerY:F2}, {spawnZ:F1})");
        }

        // ── Place containers at floor level ───────────────────────────────
        float sp = b.size.x * 0.2f;
        Vector3[] offsets = {
            new Vector3( sp, 0, sp), new Vector3(-sp, 0, sp),
            new Vector3( sp, 0,-sp), new Vector3(-sp, 0,-sp),
            new Vector3(sp*1.5f, 0, 0), new Vector3(-sp*1.5f, 0, 0),
            new Vector3(0, 0, sp*1.5f), new Vector3(0, 0,-sp*1.5f),
            new Vector3(sp*1.2f, 0, sp*1.2f), new Vector3(-sp*1.2f, 0,-sp*1.2f),
            new Vector3(sp*1.8f, 0, sp*0.5f), new Vector3(-sp*0.5f, 0, sp*1.8f),
            new Vector3(sp*1.0f, 0,-sp*1.5f), new Vector3(-sp*1.5f, 0, sp*1.0f),
        };

        int idx = 0;
        foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (!go.name.StartsWith("Container") || idx >= offsets.Length) continue;
            Vector3 o = offsets[idx++];
            Ray cr = new Ray(new Vector3(o.x, fromY, o.z), Vector3.down);
            RaycastHit ch;
            float cy = Physics.Raycast(cr, out ch, fromY + 50f) ? ch.point.y + 0.3f : floorY + 0.3f;
            go.transform.position = new Vector3(o.x, cy, o.z);
            EditorUtility.SetDirty(go);
        }
        Debug.Log($"Placed {idx} containers.");

        // ── Update camera offset ───────────────────────────────────────────
        GameObject camGO = GameObject.Find("Main Camera");
        if (camGO != null)
        {
            GameCameraFollow gcf = camGO.GetComponent<GameCameraFollow>();
            if (gcf != null) { gcf.offset = new Vector3(0f, 20f, -15f); EditorUtility.SetDirty(camGO); }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log($"✅ Player placed on map floor at Y={playerY:F2}.");
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
