using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FinalSceneFix
{
    [MenuItem("Tools/Final Scene Fix")]
    public static void Execute()
    {
        // ── Force scene view skybox ON ───────────────────────────────────────
        foreach (SceneView sv in SceneView.sceneViews)
        {
            sv.sceneViewState.showSkybox = true;
            sv.sceneLighting = true;
            sv.Repaint();
        }

        // ── Fix Gas Station completely ───────────────────────────────────────
        var gasStation = GameObject.Find("GasStation");
        if (gasStation == null) { Debug.LogError("GasStation not found"); return; }

        // Destroy and rebuild the entire exterior cleanly
        var exterior = gasStation.transform.Find("Exterior");
        if (exterior != null) GameObject.DestroyImmediate(exterior.gameObject);

        var newExterior = new GameObject("Exterior");
        newExterior.transform.SetParent(gasStation.transform, false);
        newExterior.transform.localPosition = Vector3.zero;

        // ── Station building (gas station main structure) ──
        var stationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Station_03.prefab");
        if (stationPrefab != null)
        {
            var s = (GameObject)PrefabUtility.InstantiatePrefab(stationPrefab, newExterior.transform);
            s.name = "StationBuilding";
            s.transform.localPosition = Vector3.zero;
        }

        // ── Convenience store (proper shop prefab with materials) ──
        // SM_Bld_Shop_01 is 5x3x5.5 - place two side by side for a wider store
        var shop1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_01.prefab");
        var shop2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_02.prefab");

        if (shop1Prefab != null)
        {
            var shop1 = (GameObject)PrefabUtility.InstantiatePrefab(shop1Prefab, newExterior.transform);
            shop1.name = "ConvenienceStore_A";
            shop1.transform.localPosition = new Vector3(11f, 0f, 0f);
        }
        if (shop2Prefab != null)
        {
            var shop2 = (GameObject)PrefabUtility.InstantiatePrefab(shop2Prefab, newExterior.transform);
            shop2.name = "ConvenienceStore_B";
            shop2.transform.localPosition = new Vector3(16f, 0f, 0f);
        }

        // ── Shop awning over entrance ──
        var coverPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_Cover_03.prefab");
        if (coverPrefab != null)
        {
            var cover = (GameObject)PrefabUtility.InstantiatePrefab(coverPrefab, newExterior.transform);
            cover.name = "StoreAwning";
            cover.transform.localPosition = new Vector3(11f, 3.05f, -2.8f);
        }

        // ── Gas pump canopy (flat roof over pumps) ──
        // Use SM_Bld_Shop_Cover_05 which is a flat square canopy
        var canopyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_Cover_05.prefab");
        if (canopyPrefab != null)
        {
            var canopy = (GameObject)PrefabUtility.InstantiatePrefab(canopyPrefab, newExterior.transform);
            canopy.name = "PumpCanopy";
            canopy.transform.localPosition = new Vector3(0f, 4.5f, -5f);
        }

        // ── Gas pumps (ATM props as stand-ins) ──
        var atmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_ATM_01.prefab");
        if (atmPrefab != null)
        {
            float[] pumpXs = { -3f, 0f, 3f };
            foreach (var px in pumpXs)
            {
                var pump = (GameObject)PrefabUtility.InstantiatePrefab(atmPrefab, newExterior.transform);
                pump.name = $"GasPump_{px}";
                pump.transform.localPosition = new Vector3(px, 0f, -5f);
            }
        }

        // ── Parking lot ground in front ──
        var barePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Road_Bare_01.prefab");
        if (barePrefab != null)
        {
            for (int xi = -1; xi <= 3; xi++)
            {
                for (int zi = -2; zi <= -1; zi++)
                {
                    var tile = (GameObject)PrefabUtility.InstantiatePrefab(barePrefab, newExterior.transform);
                    tile.name = $"Lot_{xi}_{zi}";
                    tile.transform.localPosition = new Vector3(xi * 5f + 2.5f, 0f, zi * 5f + 2.5f);
                }
            }
        }

        // ── Gas station sign ──
        var signPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Sign_Entrance_01.prefab");
        if (signPrefab != null)
        {
            var sign = (GameObject)PrefabUtility.InstantiatePrefab(signPrefab, newExterior.transform);
            sign.name = "GasSign";
            sign.transform.localPosition = new Vector3(-6f, 0f, -7f);
            sign.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        }

        // ── Wire up StoreInterior component ──
        var storeInterior = gasStation.GetComponent<StoreInterior>();
        if (storeInterior != null)
            storeInterior.exteriorRoot = newExterior;

        // ── Reposition gas station closer to road ──
        gasStation.transform.position = new Vector3(16f, 0f, 5f);

        // ── Save ──
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FinalSceneFix] Done! Gas station rebuilt with proper shop prefabs.");
    }
}
