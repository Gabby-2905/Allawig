using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ReplaceWithVersatileCity
{
    [MenuItem("Tools/Replace Map With Versatile City")]
    public static void Execute()
    {
        // ── 1. Open GameScene ──────────────────────────────────────────────
        var scene = EditorSceneManager.OpenScene(
            "Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        // ── 2. Remove existing CityMap (PolygonCity) ──────────────────────
        GameObject oldMap = GameObject.Find("CityMap");
        if (oldMap != null)
        {
            Object.DestroyImmediate(oldMap);
            Debug.Log("Removed CityMap (PolygonCity).");
        }

        // Also remove any leftover 1stMap just in case
        GameObject old1st = GameObject.Find("1stMap");
        if (old1st != null) Object.DestroyImmediate(old1st);

        // ── 3. Load and instantiate the Versatile Studio city prefab ──────
        string prefabPath = "Assets/Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/demo_city_by_versatile_studio.prefab";
        GameObject cityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (cityPrefab == null)
        {
            Debug.LogError("Could not find demo_city_by_versatile_studio.prefab at: " + prefabPath);
            return;
        }

        GameObject cityInstance = (GameObject)PrefabUtility.InstantiatePrefab(cityPrefab);
        cityInstance.name = "Map1_City";

        // The prefab's bounds center is at ~(770, 11, 683).
        // Offset it so the city center sits at world origin for easy navigation.
        // Ground is at Y=0 in the prefab, so keep Y=0.
        // Shift X and Z so the city center (770, 683) maps to (0, 0).
        cityInstance.transform.position = new Vector3(-770f, 0f, -683f);
        cityInstance.transform.rotation = Quaternion.identity;
        cityInstance.transform.localScale = Vector3.one;

        Debug.Log("Placed Map1_City (Versatile Studio) in GameScene.");

        // ── 4. Reposition Player at city center on the ground ─────────────
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            // City center is now at world (0, 0, 0) after the offset above.
            // Place player slightly above ground so CharacterController settles.
            player.transform.position = new Vector3(0f, 1f, 0f);
            Debug.Log("Player repositioned to city center.");
        }

        // ── 5. Save scene ──────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log("GameScene saved with Versatile Studio city map.");
    }
}
