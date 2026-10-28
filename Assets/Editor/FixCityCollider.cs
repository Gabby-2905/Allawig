using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixCityCollider
{
    public static void Execute()
    {
        // ── 1. Enable Read/Write on the city FBX ──────────────────────────
        string fbxPath = "Assets/Versatile Studio Assets/Demo City By Versatile Studio/Models/demo_city_by_versatile_studio.fbx";
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer != null)
        {
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
                Debug.Log("Enabled Read/Write on city FBX.");
            }
            else
            {
                Debug.Log("FBX already Read/Write enabled.");
            }
        }
        else
        {
            Debug.LogWarning("Could not find city FBX importer at: " + fbxPath);
        }

        // ── 2. Open GameScene and fix the MeshCollider ────────────────────
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        // Find the collider object (it's nested under Map1_City/STATIC_MODELS/...)
        // Search all MeshColliders in scene
        MeshCollider[] allColliders = Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None);
        Debug.Log($"Found {allColliders.Length} MeshColliders in scene.");
        foreach (var mc in allColliders)
        {
            Debug.Log($"  MeshCollider on '{mc.gameObject.name}' | convex={mc.convex} | mesh={(mc.sharedMesh != null ? mc.sharedMesh.name : "NULL")}");
        }

        // ── 3. Add a simple BoxCollider as ground fallback ─────────────────
        // Place a large flat box at the road surface Y to guarantee the player lands
        GameObject groundFallback = GameObject.Find("CityGroundFallback");
        if (groundFallback == null)
        {
            groundFallback = new GameObject("CityGroundFallback");
            EditorSceneManager.MoveGameObjectToScene(groundFallback, scene);
        }

        // Position at the city center, road surface Y
        // Collider bounds center: X=770, Z=699, min Y=0.69
        groundFallback.transform.position = new Vector3(770f, 0.5f, 699f);

        BoxCollider box = groundFallback.GetComponent<BoxCollider>();
        if (box == null) box = groundFallback.AddComponent<BoxCollider>();
        // Make it large enough to cover the whole city road area (3200 x 2050)
        box.size = new Vector3(3300f, 1f, 2200f);
        box.center = Vector3.zero;

        Debug.Log($"CityGroundFallback BoxCollider placed at {groundFallback.transform.position}, size={box.size}");

        // ── 4. Place player on the road surface ───────────────────────────
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Road surface Y = box top = 0.5 + 0.5 = 1.0
            player.transform.position = new Vector3(562f, 1.5f, 279f);

            if (cc != null) cc.enabled = true;
            Debug.Log($"Player placed at {player.transform.position}");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log("City collider fixed and scene saved.");
    }
}
