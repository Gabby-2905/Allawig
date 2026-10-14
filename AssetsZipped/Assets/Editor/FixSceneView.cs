using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class FixSceneView
{
    static FixSceneView()
    {
        EditorApplication.delayCall += EnableSkybox;
    }

    [MenuItem("Tools/Enable Scene View Skybox")]
    public static void EnableSkybox()
    {
        foreach (SceneView sv in SceneView.sceneViews)
        {
            sv.sceneViewState.showSkybox = true;
            sv.Repaint();
        }

        SceneView.lastActiveSceneView?.Repaint();
    }

    [MenuItem("Tools/Fix Gas Station Store")]
    public static void FixGasStationStore()
    {
        // The convenience store box needs to be replaced with a proper shop prefab
        // Find and destroy the plain grey box
        var gasStation = GameObject.Find("GasStation");
        if (gasStation == null) { Debug.LogError("GasStation not found"); return; }

        var exterior = gasStation.transform.Find("Exterior");
        if (exterior == null) { Debug.LogError("Exterior not found"); return; }

        // Remove the old grey box convenience store if it exists
        var oldStore = exterior.Find("ConvenienceStore");
        if (oldStore != null) GameObject.DestroyImmediate(oldStore.gameObject);

        var oldAwning = exterior.Find("StoreAwning");
        if (oldAwning != null) GameObject.DestroyImmediate(oldAwning.gameObject);

        // Place SM_Bld_Shop_03 (10x3x5.5) as the convenience store
        // It has proper materials already
        var shopPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_03.prefab");
        if (shopPrefab != null)
        {
            var shop = (GameObject)PrefabUtility.InstantiatePrefab(shopPrefab, exterior);
            shop.name = "ConvenienceStore";
            // Place it next to the station building (station is at local 0,0,0, size ~10.78 x 5.74 x 5.91)
            shop.transform.localPosition = new Vector3(11f, 0f, 0f);
            shop.transform.localRotation = Quaternion.identity;
        }

        // Add a shop cover/awning over the entrance
        var coverPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_Cover_03.prefab");
        if (coverPrefab != null)
        {
            var cover = (GameObject)PrefabUtility.InstantiatePrefab(coverPrefab, exterior);
            cover.name = "StoreAwning";
            cover.transform.localPosition = new Vector3(11f, 3.08f, -2.8f);
            cover.transform.localRotation = Quaternion.identity;
        }

        // Add a gas station canopy (use SM_Bld_Shop_Cover_05 which is a flat roof cover)
        var canopyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_Cover_05.prefab");
        if (canopyPrefab != null)
        {
            var existingCanopy = exterior.Find("GasCanopy");
            if (existingCanopy == null)
            {
                var canopy = (GameObject)PrefabUtility.InstantiatePrefab(canopyPrefab, exterior);
                canopy.name = "GasCanopy";
                canopy.transform.localPosition = new Vector3(-2f, 4f, -4f);
                canopy.transform.localRotation = Quaternion.identity;
            }
        }

        // Add a sign for the gas station
        var signPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Sign_Entrance_01.prefab");
        if (signPrefab != null)
        {
            var existingSign = exterior.Find("GasSign");
            if (existingSign == null)
            {
                var sign = (GameObject)PrefabUtility.InstantiatePrefab(signPrefab, exterior);
                sign.name = "GasSign";
                sign.transform.localPosition = new Vector3(-5f, 0f, -6f);
                sign.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        // Add parking lot ground tiles in front of the gas station
        var roadBarePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Road_Bare_01.prefab");
        if (roadBarePrefab != null)
        {
            var existingLot = exterior.Find("ParkingLot_0");
            if (existingLot == null)
            {
                for (int xi = -1; xi <= 2; xi++)
                {
                    for (int zi = -2; zi <= -1; zi++)
                    {
                        var tile = (GameObject)PrefabUtility.InstantiatePrefab(roadBarePrefab, exterior);
                        tile.name = $"ParkingLot_{xi}_{zi}";
                        tile.transform.localPosition = new Vector3(xi * 5f + 2.5f, 0f, zi * 5f + 2.5f);
                    }
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FixSceneView] Gas station store fixed with proper prefab.");
    }
}
