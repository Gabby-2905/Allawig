using UnityEngine;
using UnityEditor;

/// <summary>
/// Sets the 1stMap to a balanced playable scale (~150 units),
/// verifies textures, player spawn, and camera.
/// </summary>
public class FinalizeMap : MonoBehaviour
{
    [MenuItem("Tools/Finalize Map Scale")]
    public static void Execute()
    {
        GameObject mapGO = GameObject.Find("1stMap");
        if (mapGO == null) { Debug.LogError("1stMap not found!"); return; }

        // Scale 150 = good balance: large enough to explore, small enough to see detail
        float newScale = 150f;
        mapGO.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        mapGO.transform.localScale = Vector3.one * newScale;
        mapGO.transform.position = Vector3.zero;

        // Reposition so floor is at Y=0
        Bounds b = GetBounds(mapGO);
        mapGO.transform.position = new Vector3(-b.center.x, -b.min.y, -b.center.z);
        b = GetBounds(mapGO);

        Debug.Log($"Map: {b.size.x:F0}x{b.size.z:F0} units, height: {b.size.y:F0}");

        // ── Player spawn at map center, on surface ─────────────────────────
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0f, b.min.y + 0.3f, 0f);
            GamePlayerMovement gpm = player.GetComponent<GamePlayerMovement>();
            if (gpm != null)
            {
                gpm.walkSpeed = 6f;
                gpm.runSpeed  = 14f;
            }
            EditorUtility.SetDirty(player);
        }

        // ── Camera offset scaled for map size ──────────────────────────────
        GameObject camGO = GameObject.Find("Main Camera");
        if (camGO != null)
        {
            GameCameraFollow gcf = camGO.GetComponent<GameCameraFollow>();
            if (gcf != null)
            {
                gcf.offset = new Vector3(0f, 20f, -15f);
                EditorUtility.SetDirty(camGO);
            }
            camGO.transform.position = new Vector3(0f, b.min.y + 20f, -15f);
            camGO.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
            EditorUtility.SetDirty(camGO);
        }

        // ── Rain above map ─────────────────────────────────────────────────
        GameObject rain = GameObject.Find("Rain");
        if (rain != null)
        {
            rain.transform.position = new Vector3(0f, b.max.y + 30f, 0f);
            ParticleSystem ps = rain.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var shape = ps.shape;
                shape.scale = new Vector3(b.size.x * 1.3f, 1f, b.size.z * 1.3f);
            }
            EditorUtility.SetDirty(rain);
        }

        // ── Containers spread across map ───────────────────────────────────
        float sy = b.min.y + 0.5f;
        float sp = Mathf.Min(b.size.x, b.size.z) * 0.3f;
        Vector3[] pos = {
            new Vector3( sp*.4f, sy,  sp*.4f), new Vector3(-sp*.4f, sy,  sp*.4f),
            new Vector3( sp*.4f, sy, -sp*.4f), new Vector3(-sp*.4f, sy, -sp*.4f),
            new Vector3( sp*.7f, sy,  0),      new Vector3(-sp*.7f, sy,  0),
            new Vector3( 0,      sy,  sp*.7f), new Vector3( 0,      sy, -sp*.7f),
            new Vector3( sp*.6f, sy,  sp*.6f), new Vector3(-sp*.6f, sy, -sp*.6f),
            new Vector3( sp*.8f, sy,  sp*.3f), new Vector3(-sp*.3f, sy,  sp*.8f),
            new Vector3( sp*.5f, sy, -sp*.7f), new Vector3(-sp*.7f, sy,  sp*.5f),
        };
        int idx = 0;
        foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            if (go.name.StartsWith("Container") && idx < pos.Length)
            { go.transform.position = pos[idx++]; EditorUtility.SetDirty(go); }

        // ── Verify material is assigned ────────────────────────────────────
        MeshRenderer mr = mapGO.GetComponent<MeshRenderer>();
        if (mr != null && (mr.sharedMaterial == null || mr.sharedMaterial.name == "Default-Material"))
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/TripoModels/post-apocalyptic_scrapyard_3d_model/Materials/post-apocalyptic_scrapyard_3d_model.mat");
            if (mat != null) { mr.sharedMaterial = mat; Debug.Log("Material reassigned."); }
        }

        EditorUtility.SetDirty(mapGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"✅ Map finalized at scale {newScale}. Area: {b.size.x:F0}x{b.size.z:F0} units.");
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
