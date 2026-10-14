using UnityEngine;
using UnityEditor;

/// <summary>
/// Scales the 1stMap up to a large playable size (~300 units),
/// adjusts player speed, camera offset, and rain position.
/// </summary>
public class ScaleMapUp : MonoBehaviour
{
    [MenuItem("Tools/Scale 1stMap to Large Playable Size")]
    public static void Execute()
    {
        GameObject mapGO = GameObject.Find("1stMap");
        if (mapGO == null) { Debug.LogError("1stMap not found!"); return; }

        // Current scale is 100, bounds are ~98x94
        // Scale up to 300 units floor area: multiply scale by ~3
        // New scale = 300 (100 * 3)
        float newScale = 300f;
        mapGO.transform.localScale = Vector3.one * newScale;

        // Reposition so floor is at Y=0
        Bounds b = GetBounds(mapGO);
        mapGO.transform.position = new Vector3(
            -b.center.x,
            -b.min.y,
            -b.center.z
        );
        b = GetBounds(mapGO);

        Debug.Log($"Map scaled to {newScale}x. Floor area: {b.size.x:F0}x{b.size.z:F0} units. Height: {b.size.y:F0}");

        // ── Player: reposition + increase speed for larger map ─────────────
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0f, b.min.y + 0.3f, 0f);

            GamePlayerMovement gpm = player.GetComponent<GamePlayerMovement>();
            if (gpm != null)
            {
                gpm.walkSpeed = 8f;   // faster walk for big map
                gpm.runSpeed  = 18f;  // faster run
                EditorUtility.SetDirty(player);
            }
            Debug.Log("Player repositioned and speed updated.");
        }

        // ── Camera: increase offset for better overview ────────────────────
        GameObject camGO = GameObject.Find("Main Camera");
        if (camGO != null)
        {
            GameCameraFollow gcf = camGO.GetComponent<GameCameraFollow>();
            if (gcf != null)
            {
                gcf.offset = new Vector3(0f, 30f, -20f);
                EditorUtility.SetDirty(camGO);
            }
            // Also update camera position directly
            camGO.transform.position = new Vector3(0f, 30f, -20f);
            camGO.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
            EditorUtility.SetDirty(camGO);
            Debug.Log("Camera offset updated for large map.");
        }

        // ── Rain: reposition above map ─────────────────────────────────────
        GameObject rain = GameObject.Find("Rain");
        if (rain != null)
        {
            rain.transform.position = new Vector3(0f, b.max.y + 40f, 0f);

            // Scale rain emitter to cover the map
            ParticleSystem ps = rain.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var shape = ps.shape;
                shape.scale = new Vector3(b.size.x * 1.2f, 1f, b.size.z * 1.2f);
            }
            EditorUtility.SetDirty(rain);
        }

        // ── Containers: spread across the map ─────────────────────────────
        float surfaceY = b.min.y + 0.5f;
        float spread = Mathf.Min(b.size.x, b.size.z) * 0.35f;

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
        Debug.Log($"Repositioned {idx} containers across {spread:F0} unit spread.");

        // ── Save ───────────────────────────────────────────────────────────
        EditorUtility.SetDirty(mapGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"✅ Map scaled up! {b.size.x:F0}x{b.size.z:F0} units playable area.");
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
