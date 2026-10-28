using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Replaces the 1stMap object in GameScene with all the city environment
/// objects from the PolygonCity City scene, grouped under a "CityMap" parent.
/// </summary>
public class ReplaceMapWithCity
{
    [MenuItem("Tools/Replace 1stMap With City Map")]
    public static void Execute()
    {
        // ── 1. Open GameScene ──────────────────────────────────────────────
        var gameScene = EditorSceneManager.OpenScene(
            "Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        // ── 2. Delete 1stMap ───────────────────────────────────────────────
        GameObject oldMap = GameObject.Find("1stMap");
        if (oldMap != null)
        {
            Object.DestroyImmediate(oldMap);
            Debug.Log("Removed 1stMap.");
        }
        else
        {
            Debug.LogWarning("1stMap not found — may already be removed.");
        }

        // ── 3. Load City scene additively to copy its objects ──────────────
        var cityScene = EditorSceneManager.OpenScene(
            "Assets/SyntyStudios/PolygonCity/Scenes/City.unity",
            OpenSceneMode.Additive);

        // ── 4. Collect all root objects from City scene ────────────────────
        List<GameObject> cityRoots = new List<GameObject>();
        foreach (var go in cityScene.GetRootGameObjects())
        {
            // Skip cameras, lights, event systems — keep only environment
            bool isCamera     = go.GetComponent<Camera>() != null;
            bool isLight      = go.GetComponent<Light>() != null;
            bool isEventSys   = go.GetComponent<UnityEngine.EventSystems.EventSystem>() != null;
            bool isAudioList  = go.GetComponent<AudioListener>() != null;

            if (!isCamera && !isLight && !isEventSys)
                cityRoots.Add(go);
        }

        // ── 5. Create a CityMap parent in GameScene ────────────────────────
        SceneManager.SetActiveScene(gameScene);
        GameObject cityParent = new GameObject("CityMap");
        cityParent.transform.position = Vector3.zero;
        cityParent.transform.rotation = Quaternion.identity;
        cityParent.transform.localScale = Vector3.one;
        EditorSceneManager.MoveGameObjectToScene(cityParent, gameScene);

        // ── 6. Move city objects into GameScene under CityMap ──────────────
        foreach (var go in cityRoots)
        {
            EditorSceneManager.MoveGameObjectToScene(go, gameScene);
            go.transform.SetParent(cityParent.transform, true);
        }

        // ── 7. Close City scene ────────────────────────────────────────────
        EditorSceneManager.CloseScene(cityScene, true);

        // ── 8. Reposition Player on a road in the city ────────────────────
        // The city is centered at origin; place player on a sidewalk
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0f, 0.5f, 0f);
            Debug.Log("Player repositioned to city origin.");
        }

        // ── 9. Save GameScene ──────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(gameScene);
        EditorSceneManager.SaveScene(gameScene, "Assets/Scenes/GameScene.unity");
        Debug.Log("GameScene saved with CityMap replacing 1stMap.");
    }
}
