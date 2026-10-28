using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Copies the exact contents of "map1 city.unity" into GameScene,
/// replacing the current Map1_City prefab instance.
/// Skips cameras, directional lights, event systems from the source scene
/// since GameScene already has those.
/// </summary>
public class CopyMap1CityToGameScene
{
    [MenuItem("Tools/Copy Map1 City To GameScene")]
    public static void Execute()
    {
        // ── 1. Open GameScene ──────────────────────────────────────────────
        var gameScene = EditorSceneManager.OpenScene(
            "Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        // ── 2. Remove existing Map1_City (prefab instance) ────────────────
        GameObject existing = GameObject.Find("Map1_City");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
            Debug.Log("Removed existing Map1_City.");
        }

        // Also remove any leftover old maps
        foreach (string name in new[] { "CityMap", "1stMap" })
        {
            GameObject old = GameObject.Find(name);
            if (old != null) Object.DestroyImmediate(old);
        }

        // ── 3. Open map1 city scene additively ────────────────────────────
        var mapScene = EditorSceneManager.OpenScene(
            "Assets/Versatile Studio Assets/Demo City By Versatile Studio/Scenes/map1 city.unity",
            OpenSceneMode.Additive);

        // ── 4. Create a parent container in GameScene ──────────────────────
        SceneManager.SetActiveScene(gameScene);
        GameObject mapParent = new GameObject("Map1_City");
        EditorSceneManager.MoveGameObjectToScene(mapParent, gameScene);

        // ── 5. Move all relevant root objects from map scene into GameScene ─
        int movedCount = 0;
        foreach (var go in mapScene.GetRootGameObjects())
        {
            // Skip objects that GameScene already has
            bool isCamera    = go.GetComponent<Camera>() != null;
            bool isEventSys  = go.GetComponent<UnityEngine.EventSystems.EventSystem>() != null;
            bool isAudioList = go.GetComponent<AudioListener>() != null;

            // Keep directional lights from the map scene (they set the mood)
            // but skip if GameScene already has one
            bool isDirectionalLight = false;
            Light l = go.GetComponent<Light>();
            if (l != null && l.type == LightType.Directional)
                isDirectionalLight = true;

            if (isCamera || isEventSys || isAudioList || isDirectionalLight)
            {
                Debug.Log($"Skipping '{go.name}' (camera/eventsys/audiolistener/dirlight)");
                continue;
            }

            EditorSceneManager.MoveGameObjectToScene(go, gameScene);
            go.transform.SetParent(mapParent.transform, true);
            movedCount++;
        }

        Debug.Log($"Moved {movedCount} objects from map1 city into GameScene.");

        // ── 6. Close the map scene ─────────────────────────────────────────
        EditorSceneManager.CloseScene(mapScene, true);

        // ── 7. Find the road surface and place player on it ───────────────
        // The map1 city scene has its content at world origin (no offset needed).
        // Raycast from above the known road area to find ground Y.
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            // Try several positions on the main road area
            Vector3[] spawnCandidates = new Vector3[]
            {
                new Vector3(562f, 100f, 279f),   // original scene center
                new Vector3(500f, 100f, 250f),
                new Vector3(600f, 100f, 300f),
                new Vector3(550f, 100f, 200f),
                new Vector3(400f, 100f, 200f),
            };

            Vector3 spawnPos = new Vector3(562f, 1f, 279f);
            foreach (var candidate in spawnCandidates)
            {
                RaycastHit hit;
                if (Physics.Raycast(candidate, Vector3.down, out hit, 200f))
                {
                    spawnPos = hit.point + Vector3.up * 1f;
                    Debug.Log($"Ground hit at {hit.point} from {candidate}. Spawning at {spawnPos}");
                    break;
                }
            }

            player.transform.position = spawnPos;
            Debug.Log($"Player placed at {player.transform.position}");
        }

        // ── 8. Fix lighting ────────────────────────────────────────────────
        RenderSettings.ambientMode      = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.2f;

        // Boost the GameScene directional light
        GameObject dirLight = GameObject.Find("Directional Light");
        if (dirLight != null)
        {
            Light dl = dirLight.GetComponent<Light>();
            if (dl != null) { dl.intensity = 2f; dl.color = new Color(1f, 0.95f, 0.85f); }
        }

        // ── 9. Save ────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(gameScene);
        EditorSceneManager.SaveScene(gameScene, "Assets/Scenes/GameScene.unity");
        Debug.Log("GameScene saved with exact map1 city content.");
    }
}
