using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixGasStationLayout
{
    [MenuItem("Tools/Fix Gas Station Layout")]
    public static void Execute()
    {
        var gasStation = GameObject.Find("GasStation");
        if (gasStation == null) { Debug.LogError("GasStation not found"); return; }

        var exterior = gasStation.transform.Find("Exterior");
        if (exterior == null) { Debug.LogError("Exterior not found"); return; }

        // ── Fix shop rotations so they face the road (negative Z direction) ──
        // The station building faces Z+ (toward z=5 area), road is at z=0 side
        // Shops should face Z- (toward the road/parking lot)

        var storeA = exterior.Find("ConvenienceStore_A");
        var storeB = exterior.Find("ConvenienceStore_B");
        var canopy  = exterior.Find("PumpCanopy");

        // Rotate shops 180° so their fronts face the road (negative Z)
        if (storeA != null)
        {
            storeA.localRotation = Quaternion.Euler(0f, 180f, 0f);
            // After 180° rotation, offset X so it still lines up
            storeA.localPosition = new Vector3(16f, 0f, 5.5f);
        }
        if (storeB != null)
        {
            storeB.localRotation = Quaternion.Euler(0f, 180f, 0f);
            storeB.localPosition = new Vector3(21f, 0f, 5.5f);
        }

        // Remove the floating canopy (it looks bad)
        if (canopy != null) GameObject.DestroyImmediate(canopy.gameObject);

        // Remove the pump canopy wires - find and remove SM_Bld_Shop_Cover_05
        var toRemove = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in exterior)
        {
            if (child.name.Contains("PumpCanopy") || child.name.Contains("GasCanopy"))
                toRemove.Add(child);
        }
        foreach (var t in toRemove) GameObject.DestroyImmediate(t.gameObject);

        // ── Reposition gas station so it sits right next to the sidewalk ──
        // Road is at X: -10 to +10, sidewalk at X: +10 to +15
        // Gas station lot starts at X: +15
        gasStation.transform.position = new Vector3(15f, 0f, 0f);

        // ── Fix parking lot tiles to be in front of the store ──
        var lotTiles = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in exterior)
        {
            if (child.name.StartsWith("Lot_"))
                lotTiles.Add(child);
        }
        foreach (var t in lotTiles) GameObject.DestroyImmediate(t.gameObject);

        // Rebuild parking lot: 4 wide x 2 deep in front of station
        var barePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Road_Bare_01.prefab");
        if (barePrefab != null)
        {
            for (int xi = 0; xi <= 3; xi++)
            {
                for (int zi = -2; zi <= -1; zi++)
                {
                    var tile = (GameObject)PrefabUtility.InstantiatePrefab(barePrefab, exterior);
                    tile.name = $"Lot_{xi}_{zi}";
                    tile.transform.localPosition = new Vector3(xi * 5f + 2.5f, 0f, zi * 5f + 2.5f);
                }
            }
        }

        // ── Fix gas pumps position ──
        foreach (Transform child in exterior)
        {
            if (child.name.StartsWith("GasPump_"))
            {
                // Pumps in the parking lot area
                float px = float.Parse(child.name.Replace("GasPump_", ""));
                child.localPosition = new Vector3(px + 7f, 0f, -4f);
            }
        }

        // ── Force scene view skybox ──
        foreach (SceneView sv in SceneView.sceneViews)
        {
            sv.sceneViewState.showSkybox = true;
            sv.sceneLighting = true;
            sv.Repaint();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FixGasStationLayout] Done!");
    }
}
