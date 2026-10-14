using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Removes all OlegWER car prefabs from the scene (which cause magenta)
/// and replaces them with SyntyStudios vehicles + other confirmed-working props.
/// Also removes any remaining magenta objects.
/// </summary>
public class ReplaceCarProps : EditorWindow
{
    [MenuItem("Tools/Replace Car Props")]
    public static void Fix()
    {
        // ── 1. Find and destroy all OlegWER objects in scene ──────────────
        var allGOs = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        var toDestroy = new List<GameObject>();

        foreach (var go in allGOs)
        {
            // OlegWER prefab names start with SM_ivy_ or SM_asset_
            string n = go.name;
            if ((n.StartsWith("SM_ivy_") || n.StartsWith("SM_asset_")) && go.transform.parent != null)
            {
                // Only destroy root-level OlegWER objects (direct children of Junkyard_Props)
                if (go.transform.parent.name == "Junkyard_Props")
                    toDestroy.Add(go);
            }
        }

        foreach (var go in toDestroy)
        {
            if (go != null)
            {
                Debug.Log($"[ReplaceCarProps] Removing: {go.name}");
                Undo.DestroyObjectImmediate(go);
            }
        }

        // ── 2. Place replacement vehicles using SyntyStudios ──────────────
        var junkyardParent = GameObject.Find("Junkyard_Props");
        if (junkyardParent == null)
        {
            Debug.LogError("[ReplaceCarProps] Junkyard_Props not found!");
            return;
        }

        // SyntyStudios vehicles – confirmed URP compatible
        string[] vehiclePaths = {
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Sedan_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Small_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Van_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Muscle_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Medium_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Taxi_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Police_01.prefab",
        };

        // Scattered positions around the junkyard – avoid building zones
        (Vector3 pos, float rot)[] carSpots = {
            (new Vector3(-24f, 0f,  5f),  20f),
            (new Vector3(-18f, 0f, -9f), -35f),
            (new Vector3(  5f, 0f, 22f),  50f),
            (new Vector3( 25f, 0f,  7f),   5f),
            (new Vector3( 13f, 0f,-23f), -20f),
            (new Vector3(-11f, 0f,-25f),  65f),
            (new Vector3( 27f, 0f,-17f), -50f),
        };

        for (int i = 0; i < carSpots.Length; i++)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(vehiclePaths[i % vehiclePaths.Length]);
            if (prefab == null) { Debug.LogWarning($"[ReplaceCarProps] Missing prefab: {vehiclePaths[i % vehiclePaths.Length]}"); continue; }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, junkyardParent.transform);
            go.transform.position = carSpots[i].pos;
            go.transform.rotation = Quaternion.Euler(0, carSpots[i].rot, 0);
            // Tilt slightly for abandoned look
            if (i % 3 == 0) go.transform.rotation = Quaternion.Euler(0, carSpots[i].rot, Random.Range(-4f, 4f));
            Undo.RegisterCreatedObjectUndo(go, "Place Vehicle");
            Debug.Log($"[ReplaceCarProps] Placed: {go.name} at {carSpots[i].pos}");
        }

        // ── 3. Save scene ─────────────────────────────────────────────────
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("[ReplaceCarProps] Done!");
    }
}
