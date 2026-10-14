using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixGasStationFinal
{
    [MenuItem("Tools/Fix Gas Station Final")]
    public static void Execute()
    {
        var gasStation = GameObject.Find("GasStation");
        if (gasStation == null) { Debug.LogError("GasStation not found"); return; }

        var exterior = gasStation.transform.Find("Exterior");
        if (exterior == null) { Debug.LogError("Exterior not found"); return; }

        // ── Remove ALL children and rebuild cleanly ──────────────────────────
        var toDestroy = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in exterior)
            toDestroy.Add(child.gameObject);
        foreach (var go in toDestroy)
            GameObject.DestroyImmediate(go);

        // ── Load prefabs ─────────────────────────────────────────────────────
        var stationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Station_03.prefab");
        var shop1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_01.prefab");
        var shop2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_02.prefab");
        var coverPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_Cover_03.prefab");
        var barePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Road_Bare_01.prefab");
        var atmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_ATM_01.prefab");
        var signPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Sign_Entrance_01.prefab");

        // ── Layout plan (all local positions, gas station root at world 15,0,0):
        // Station building: faces -Z (toward road), centered at local (5, 0, 3)
        // Convenience store: directly attached right side, local (16, 0, 3), faces -Z
        // Parking lot: in front (local z = -2 to -12)
        // Gas pumps: in parking lot

        // Station building - SM_Bld_Station_03 is ~10.78 wide x 5.91 deep
        if (stationPrefab != null)
        {
            var s = (GameObject)PrefabUtility.InstantiatePrefab(stationPrefab, exterior);
            s.name = "StationBuilding";
            s.transform.localPosition = new Vector3(0f, 0f, 0f);
            s.transform.localRotation = Quaternion.identity;
        }

        // Convenience store A - SM_Bld_Shop_01 is ~5 wide x 5.5 deep, faces -Z when rotated 180
        if (shop1Prefab != null)
        {
            var s = (GameObject)PrefabUtility.InstantiatePrefab(shop1Prefab, exterior);
            s.name = "ConvStore_A";
            // Place directly to the right of station, same Z alignment
            // Station spans X: ~10.6 to 21.4 (width 10.78), so right edge at ~21.4
            // Shop is 5 wide, place at X=21.4 rotated 180 so front faces -Z
            s.transform.localPosition = new Vector3(11f, 0f, 5.5f);
            s.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        // Convenience store B - SM_Bld_Shop_02 is ~5 wide x 5.4 deep
        if (shop2Prefab != null)
        {
            var s = (GameObject)PrefabUtility.InstantiatePrefab(shop2Prefab, exterior);
            s.name = "ConvStore_B";
            s.transform.localPosition = new Vector3(16f, 0f, 5.5f);
            s.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        // Awning over store entrance
        if (coverPrefab != null)
        {
            var c = (GameObject)PrefabUtility.InstantiatePrefab(coverPrefab, exterior);
            c.name = "StoreAwning";
            c.transform.localPosition = new Vector3(13.5f, 3.05f, 2.5f);
            c.transform.localRotation = Quaternion.identity;
        }

        // Parking lot ground (4 wide x 3 deep in front of buildings)
        if (barePrefab != null)
        {
            for (int xi = -1; xi <= 4; xi++)
            {
                for (int zi = -3; zi <= -1; zi++)
                {
                    var t = (GameObject)PrefabUtility.InstantiatePrefab(barePrefab, exterior);
                    t.name = $"Lot_{xi}_{zi}";
                    t.transform.localPosition = new Vector3(xi * 5f + 2.5f, 0f, zi * 5f + 2.5f);
                }
            }
        }

        // Gas pumps in the parking lot
        if (atmPrefab != null)
        {
            float[] pumpXs = { 2f, 7f, 12f };
            foreach (var px in pumpXs)
            {
                var p = (GameObject)PrefabUtility.InstantiatePrefab(atmPrefab, exterior);
                p.name = $"GasPump_{px}";
                p.transform.localPosition = new Vector3(px, 0f, -5f);
            }
        }

        // Sign at entrance
        if (signPrefab != null)
        {
            var sg = (GameObject)PrefabUtility.InstantiatePrefab(signPrefab, exterior);
            sg.name = "GasSign";
            sg.transform.localPosition = new Vector3(-3f, 0f, -8f);
            sg.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        }

        // ── Position gas station: right side of road, sidewalk ends at X=15 ──
        gasStation.transform.position = new Vector3(15f, 0f, -5f);

        // ── Update StoreInterior spawn points ────────────────────────────────
        var storeInterior = gasStation.GetComponent<StoreInterior>();
        if (storeInterior != null)
        {
            storeInterior.exteriorRoot = exterior.gameObject;

            // Update entrance trigger position
            var entranceTrigger = gasStation.transform.Find("EntranceTrigger");
            if (entranceTrigger != null)
                entranceTrigger.localPosition = new Vector3(13.5f, 0.5f, 2f);

            // Update enter spawn point
            var enterSpawn = gasStation.transform.Find("EnterSpawnPoint");
            if (enterSpawn != null)
            {
                enterSpawn.localPosition = new Vector3(13.5f, 0.1f, 3f);
                enterSpawn.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }

            // Update exit spawn point
            var exitSpawn = gasStation.transform.Find("ExitSpawnPoint");
            if (exitSpawn != null)
            {
                exitSpawn.localPosition = new Vector3(13.5f, 0.1f, 1f);
                exitSpawn.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }

        // ── Force scene view skybox ──────────────────────────────────────────
        foreach (SceneView sv in SceneView.sceneViews)
        {
            sv.sceneViewState.showSkybox = true;
            sv.sceneLighting = true;
            sv.Repaint();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FixGasStationFinal] Done! Clean gas station layout built.");
    }
}
