using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ConnectGasStation
{
    [MenuItem("Tools/Connect Gas Station")]
    public static void Execute()
    {
        var gasStation = GameObject.Find("GasStation");
        if (gasStation == null) { Debug.LogError("GasStation not found"); return; }

        var exterior = gasStation.transform.Find("Exterior");
        if (exterior == null) { Debug.LogError("Exterior not found"); return; }

        // ── Check actual bounds of station building ──────────────────────────
        var stationBld = exterior.Find("StationBuilding");
        if (stationBld == null) { Debug.LogError("StationBuilding not found"); return; }

        // Station world bounds: min X=10.6, max X=21.4 (width ~10.78)
        // Gas station root is at world X=15, so local max X = 21.4-15 = 6.4
        // So right edge of station in local space is at X ≈ 6.4

        // ── Move convenience stores to butt up against station right side ──
        var storeA = exterior.Find("ConvStore_A");
        var storeB = exterior.Find("ConvStore_B");

        // SM_Bld_Shop_01 when rotated 180°: its mesh goes from local X=0 to X=5
        // but after 180° rotation the pivot flips, so we need to account for that
        // Place store A so its left edge aligns with station right edge
        // Station right edge in local space ≈ X=6.4
        // Store A: 5 units wide, rotated 180°, so place at X=6.4+5=11.4 (pivot at right)
        if (storeA != null)
        {
            storeA.localPosition = new Vector3(6.5f, 0f, 5.5f);
            storeA.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        // Store B: place right next to store A
        if (storeB != null)
        {
            storeB.localPosition = new Vector3(11.5f, 0f, 5.5f);
            storeB.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        // ── Move awning to be over store entrance ──
        var awning = exterior.Find("StoreAwning");
        if (awning != null)
        {
            awning.localPosition = new Vector3(9f, 3.05f, 2.8f);
            awning.localRotation = Quaternion.identity;
        }

        // ── Reposition parking lot tiles to fill the gap ──
        var lotTiles = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in exterior)
            if (child.name.StartsWith("Lot_")) lotTiles.Add(child);
        foreach (var t in lotTiles) GameObject.DestroyImmediate(t.gameObject);

        var barePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Road_Bare_01.prefab");
        if (barePrefab != null)
        {
            // Lot in front of both buildings: X from -2 to +4 (local), Z from -2 to -1
            for (int xi = -1; xi <= 3; xi++)
            {
                for (int zi = -2; zi <= -1; zi++)
                {
                    var t = (GameObject)PrefabUtility.InstantiatePrefab(barePrefab, exterior);
                    t.name = $"Lot_{xi}_{zi}";
                    t.transform.localPosition = new Vector3(xi * 5f + 2.5f, 0f, zi * 5f + 2.5f);
                }
            }
        }

        // ── Reposition gas pumps in the lot ──
        foreach (Transform child in exterior)
        {
            if (!child.name.StartsWith("GasPump_")) continue;
            var name = child.name.Replace("GasPump_", "");
            if (name == "2") child.localPosition = new Vector3(0f, 0f, -5f);
            else if (name == "7") child.localPosition = new Vector3(5f, 0f, -5f);
            else if (name == "12") child.localPosition = new Vector3(10f, 0f, -5f);
        }

        // ── Move gas station root so it sits right on the sidewalk edge ──
        // Sidewalk right edge is at world X = 15 (road X: -10 to +10, sidewalk +10 to +15)
        // Station building left edge in local space ≈ X = -4.4 (10.6 - 15)
        // We want station left edge at world X = 15, so root at X = 15 + 4.4 = 19.4
        gasStation.transform.position = new Vector3(19f, 0f, -2f);

        // ── Force scene view skybox ──────────────────────────────────────────
        foreach (SceneView sv in SceneView.sceneViews)
        {
            sv.sceneViewState.showSkybox = true;
            sv.sceneLighting = true;
            sv.Repaint();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[ConnectGasStation] Done! Buildings connected.");
    }
}
