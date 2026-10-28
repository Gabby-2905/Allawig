using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixPlayerSpawnOnCity
{
    public static void Execute()
    {
        var gameScene = EditorSceneManager.OpenScene(
            "Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        // ── Find the city map ──────────────────────────────────────────────
        GameObject cityMap = GameObject.Find("Map1_City");
        if (cityMap == null) { Debug.LogError("Map1_City not found!"); return; }

        // The prefab is offset by (-770, 0, -683) so the original scene's
        // center (562, 0, 279) maps to world (562-770, 0, 279-683) = (-208, 0, -404).
        // Ground in the original scene is at Y≈0, so world Y≈0 is the road surface.
        // Let's raycast downward from several candidate positions to find solid ground.

        Vector3[] candidates = new Vector3[]
        {
            new Vector3(0f,   50f, 0f),
            new Vector3(-208f, 50f, -404f),   // original scene center mapped to world
            new Vector3(-100f, 50f, -200f),
            new Vector3(50f,   50f, 50f),
            new Vector3(-300f, 50f, -300f),
        };

        Vector3 spawnPos = new Vector3(0f, 1f, 0f); // fallback
        bool found = false;

        foreach (var candidate in candidates)
        {
            RaycastHit hit;
            if (Physics.Raycast(candidate, Vector3.down, out hit, 200f))
            {
                spawnPos = hit.point + Vector3.up * 1f;
                Debug.Log($"Ground found at {hit.point} via raycast from {candidate}. Spawning player at {spawnPos}");
                found = true;
                break;
            }
        }

        if (!found)
        {
            // Fallback: use the known ground Y from the prefab bounds (min Y ≈ -1.5 in world)
            // Place player at the city's approximate center at ground level
            spawnPos = new Vector3(0f, 0.5f, 0f);
            Debug.Log($"No raycast hit — using fallback spawn: {spawnPos}");
        }

        // ── Reposition player ──────────────────────────────────────────────
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = spawnPos;
            Debug.Log($"Player moved to {spawnPos}");
        }

        // ── Fix ambient lighting ───────────────────────────────────────────
        RenderSettings.ambientMode         = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.55f, 0.65f, 0.85f);
        RenderSettings.ambientEquatorColor = new Color(0.45f, 0.45f, 0.45f);
        RenderSettings.ambientGroundColor  = new Color(0.2f,  0.2f,  0.2f);
        RenderSettings.ambientIntensity    = 1.3f;

        // Boost directional light
        GameObject dirLight = GameObject.Find("Directional Light");
        if (dirLight != null)
        {
            Light l = dirLight.GetComponent<Light>();
            if (l != null) l.intensity = 2.5f;
        }

        EditorSceneManager.MarkSceneDirty(gameScene);
        EditorSceneManager.SaveScene(gameScene, "Assets/Scenes/GameScene.unity");
        Debug.Log("Player spawn and lighting fixed.");
    }
}
