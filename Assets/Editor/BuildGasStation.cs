using UnityEngine;
using UnityEditor;
using TMPro;

public class BuildGasStation : MonoBehaviour
{
    [MenuItem("Tools/Build Gas Station Scene")]
    public static void Execute()
    {
        // ── 1. Move player to road area ──────────────────────────────────────
        var playerGO = GameObject.Find("Player");
        if (playerGO != null)
        {
            var cc = playerGO.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerGO.transform.position = new Vector3(0f, 0f, 0f);
            if (cc != null) cc.enabled = true;
        }

        // ── 2. Rebuild road grid around origin ───────────────────────────────
        // Remove old road/sidewalk/grass tiles that are far away
        var mapGO = GameObject.Find("Map");
        if (mapGO == null)
        {
            mapGO = new GameObject("Map");
        }

        // Clear old environment tiles (road, sidewalk, grass, yellow lines)
        string[] envNames = {
            "SM_Env_Road_01", "SM_Env_Road_YellowLines_01",
            "SM_Env_Sidewalk_Straight_01", "SM_Env_Sidewalk_Corner_01",
            "SM_Env_Grass_01"
        };
        foreach (var n in envNames)
        {
            var found = GameObject.Find("Map/" + n);
            if (found != null) GameObject.DestroyImmediate(found);
            // Also check direct children
            found = GameObject.Find(n);
            if (found != null && found.transform.parent == mapGO.transform)
                GameObject.DestroyImmediate(found);
        }

        // Road prefab: 5x5 tile
        string roadPrefabPath = "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Road_01.prefab";
        string yellowLinesPrefabPath = "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Road_YellowLines_01.prefab";
        string sidewalkPrefabPath = "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Sidewalk_Straight_01.prefab";
        string grassPrefabPath = "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Grass_01.prefab";

        var roadPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(roadPrefabPath);
        var yellowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(yellowLinesPrefabPath);
        var sidewalkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sidewalkPrefabPath);
        var grassPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(grassPrefabPath);

        // Lay a 10-tile-wide road along Z axis (X: -25 to +25, Z: -25 to +25)
        // Road runs along Z, 2 lanes wide (X: -5 to +5)
        // Sidewalks at X: -10 to -5 and X: +5 to +10
        // Grass beyond sidewalks

        float tileSize = 5f;
        int roadLanesHalf = 2;   // 2 tiles each side = 10 units wide road
        int sidewalkTiles = 1;   // 1 tile sidewalk each side
        int grassTiles = 3;      // 3 tiles grass each side
        int lengthTiles = 10;    // 10 tiles long (50 units)

        for (int z = -lengthTiles / 2; z < lengthTiles / 2; z++)
        {
            float zPos = z * tileSize + tileSize * 0.5f;

            // Road lanes (X: -10 to +10)
            for (int x = -roadLanesHalf; x < roadLanesHalf; x++)
            {
                float xPos = x * tileSize + tileSize * 0.5f;
                if (roadPrefab != null)
                {
                    var tile = (GameObject)PrefabUtility.InstantiatePrefab(roadPrefab, mapGO.transform);
                    tile.transform.position = new Vector3(xPos, 0f, zPos);
                    tile.name = $"Road_{x}_{z}";
                }
            }

            // Yellow center lines
            if (yellowPrefab != null)
            {
                var yl = (GameObject)PrefabUtility.InstantiatePrefab(yellowPrefab, mapGO.transform);
                yl.transform.position = new Vector3(0f, 0.001f, zPos);
                yl.name = $"YellowLines_{z}";
            }

            // Sidewalks
            for (int side = -1; side <= 1; side += 2)
            {
                for (int sw = 0; sw < sidewalkTiles; sw++)
                {
                    float swX = side * (roadLanesHalf * tileSize + sw * tileSize + tileSize * 0.5f);
                    if (sidewalkPrefab != null)
                    {
                        var swTile = (GameObject)PrefabUtility.InstantiatePrefab(sidewalkPrefab, mapGO.transform);
                        swTile.transform.position = new Vector3(swX, 0f, zPos);
                        swTile.name = $"Sidewalk_{side}_{sw}_{z}";
                    }
                }

                // Grass
                for (int g = 0; g < grassTiles; g++)
                {
                    float gX = side * (roadLanesHalf * tileSize + sidewalkTiles * tileSize + g * tileSize + tileSize * 0.5f);
                    if (grassPrefab != null)
                    {
                        var gTile = (GameObject)PrefabUtility.InstantiatePrefab(grassPrefab, mapGO.transform);
                        gTile.transform.position = new Vector3(gX, 0f, zPos);
                        gTile.name = $"Grass_{side}_{g}_{z}";
                    }
                }
            }
        }

        // ── 3. Build Gas Station ─────────────────────────────────────────────
        // Place gas station on the right side of the road (positive X)
        // Station building: SM_Bld_Station_03 (~10.78 x 5.74 x 5.91)
        // Position: X=20, Z=5 (on the grass/lot area)

        var existingStation = GameObject.Find("GasStation");
        if (existingStation != null) GameObject.DestroyImmediate(existingStation);

        var gasStationRoot = new GameObject("GasStation");
        gasStationRoot.transform.position = new Vector3(20f, 0f, 5f);

        // Exterior root (shown when outside)
        var exteriorRoot = new GameObject("Exterior");
        exteriorRoot.transform.SetParent(gasStationRoot.transform, false);

        // ── Gas station canopy / station building ──
        string stationPrefabPath = "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Station_03.prefab";
        var stationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(stationPrefabPath);
        if (stationPrefab != null)
        {
            var stationBld = (GameObject)PrefabUtility.InstantiatePrefab(stationPrefab, exteriorRoot.transform);
            stationBld.transform.localPosition = Vector3.zero;
            stationBld.name = "StationBuilding";
        }

        // ── Convenience store (shop) attached to station ──
        // SM_Bld_Shop_03 is 10x3x5.5 - good for a convenience store
        string shopPrefabPath = "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_03.prefab";
        var shopPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(shopPrefabPath);
        GameObject shopBld = null;
        if (shopPrefab != null)
        {
            shopBld = (GameObject)PrefabUtility.InstantiatePrefab(shopPrefab, exteriorRoot.transform);
            // Place shop to the right of the station building
            shopBld.transform.localPosition = new Vector3(10f, 0f, 0f);
            shopBld.name = "ConvenienceStore";
        }

        // ── Shop cover/awning ──
        string coverPrefabPath = "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_Cover_03.prefab";
        var coverPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(coverPrefabPath);
        if (coverPrefab != null)
        {
            var cover = (GameObject)PrefabUtility.InstantiatePrefab(coverPrefab, exteriorRoot.transform);
            cover.transform.localPosition = new Vector3(10f, 3.08f, -2.5f);
            cover.name = "StoreAwning";
        }

        // ── Gas pumps (use ATM props as stand-ins, or barriers) ──
        string atmPrefabPath = "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_ATM_01.prefab";
        var atmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(atmPrefabPath);
        if (atmPrefab != null)
        {
            // Two pump islands
            float[] pumpX = { -3f, 3f };
            float[] pumpZ = { -3f, 3f };
            foreach (var px in pumpX)
            {
                var pump = (GameObject)PrefabUtility.InstantiatePrefab(atmPrefab, exteriorRoot.transform);
                pump.transform.localPosition = new Vector3(px, 0f, -4f);
                pump.name = $"GasPump_{px}";
            }
        }

        // ── Interior root (shown when inside) ──
        var interiorRoot = new GameObject("Interior");
        interiorRoot.transform.SetParent(gasStationRoot.transform, false);
        interiorRoot.SetActive(false);

        // Interior floor
        var floorGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floorGO.name = "InteriorFloor";
        floorGO.transform.SetParent(interiorRoot.transform, false);
        floorGO.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        floorGO.transform.localScale = new Vector3(10f, 0.1f, 8f);
        var floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        floorMat.color = new Color(0.85f, 0.82f, 0.78f);
        floorGO.GetComponent<Renderer>().material = floorMat;

        // Interior walls
        CreateInteriorWall(interiorRoot.transform, "WallBack",   new Vector3(0f, 1.5f, 4f),   new Vector3(10f, 3f, 0.2f), new Color(0.9f, 0.88f, 0.85f));
        CreateInteriorWall(interiorRoot.transform, "WallLeft",   new Vector3(-5f, 1.5f, 0f),  new Vector3(0.2f, 3f, 8f),  new Color(0.9f, 0.88f, 0.85f));
        CreateInteriorWall(interiorRoot.transform, "WallRight",  new Vector3(5f, 1.5f, 0f),   new Vector3(0.2f, 3f, 8f),  new Color(0.9f, 0.88f, 0.85f));
        CreateInteriorWall(interiorRoot.transform, "Ceiling",    new Vector3(0f, 3f, 0f),      new Vector3(10f, 0.1f, 8f), new Color(0.95f, 0.95f, 0.95f));

        // Interior shelves using shop interior props
        string shelf1Path = "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_ShopInterior_Shelf_01.prefab";
        string shelf2Path = "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_ShopInterior_Shelf_02.prefab";
        string counterPath = "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_ShopInterior_Desk_01.prefab";
        string displayPath = "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_ShopInterior_Display_01.prefab";

        var shelf1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(shelf1Path);
        var shelf2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(shelf2Path);
        var counterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(counterPath);
        var displayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(displayPath);

        // Shelves along back wall
        if (shelf1Prefab != null)
        {
            for (int i = 0; i < 3; i++)
            {
                var shelf = (GameObject)PrefabUtility.InstantiatePrefab(shelf1Prefab, interiorRoot.transform);
                shelf.transform.localPosition = new Vector3(-3f + i * 2.5f, 0f, 3f);
                shelf.name = $"Shelf_{i}";
            }
        }

        // Display in middle
        if (displayPrefab != null)
        {
            var disp = (GameObject)PrefabUtility.InstantiatePrefab(displayPrefab, interiorRoot.transform);
            disp.transform.localPosition = new Vector3(0f, 0f, 0f);
            disp.name = "Display";
        }

        // Counter near entrance
        if (counterPrefab != null)
        {
            var counter = (GameObject)PrefabUtility.InstantiatePrefab(counterPrefab, interiorRoot.transform);
            counter.transform.localPosition = new Vector3(3f, 0f, -2f);
            counter.name = "Counter";
        }

        // ── Entrance trigger (outside the store door) ──
        var entranceTriggerGO = new GameObject("EntranceTrigger");
        entranceTriggerGO.transform.SetParent(gasStationRoot.transform, false);
        entranceTriggerGO.transform.localPosition = new Vector3(10f, 0.5f, -3f);
        var entranceBox = entranceTriggerGO.AddComponent<BoxCollider>();
        entranceBox.isTrigger = true;
        entranceBox.size = new Vector3(3f, 2f, 2f);
        var entranceTrigger = entranceTriggerGO.AddComponent<StoreTrigger>();
        entranceTrigger.triggerType = StoreTrigger.TriggerType.Entrance;

        // ── Exit trigger (inside the store near door) ──
        var exitTriggerGO = new GameObject("ExitTrigger");
        exitTriggerGO.transform.SetParent(gasStationRoot.transform, false);
        exitTriggerGO.transform.localPosition = new Vector3(0f, 0.5f, -3f);
        var exitBox = exitTriggerGO.AddComponent<BoxCollider>();
        exitBox.isTrigger = true;
        exitBox.size = new Vector3(3f, 2f, 2f);
        var exitTrigger = exitTriggerGO.AddComponent<StoreTrigger>();
        exitTrigger.triggerType = StoreTrigger.TriggerType.Exit;

        // ── Spawn points ──
        var enterSpawn = new GameObject("EnterSpawnPoint");
        enterSpawn.transform.SetParent(gasStationRoot.transform, false);
        enterSpawn.transform.localPosition = new Vector3(0f, 0.1f, -2f);
        enterSpawn.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        var exitSpawn = new GameObject("ExitSpawnPoint");
        exitSpawn.transform.SetParent(gasStationRoot.transform, false);
        exitSpawn.transform.localPosition = new Vector3(10f, 0.1f, -4f);
        exitSpawn.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        // ── StoreInterior controller ──
        var storeInterior = gasStationRoot.AddComponent<StoreInterior>();
        storeInterior.exteriorRoot = exteriorRoot;
        storeInterior.interiorRoot = interiorRoot;
        storeInterior.enterSpawnPoint = enterSpawn.transform;
        storeInterior.exitSpawnPoint = exitSpawn.transform;

        // Wire triggers
        entranceTrigger.storeInterior = storeInterior;
        exitTrigger.storeInterior = storeInterior;

        // ── 4. Create HUD hints ──────────────────────────────────────────────
        CreateInteractionHint(gasStationRoot, storeInterior, "EnterHint", "[E] Enter Store", true);
        CreateInteractionHint(gasStationRoot, storeInterior, "ExitHint", "[E] Exit Store", false);

        // ── 5. Tag player ────────────────────────────────────────────────────
        if (playerGO != null)
        {
            try { playerGO.tag = "Player"; } catch { }
        }

        // ── 6. Save scene ────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[BuildGasStation] Done! Gas station with convenience store built. Press E near the door to enter/exit.");
    }

    static void CreateInteriorWall(Transform parent, string name, Vector3 localPos, Vector3 scale, Color color)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.localPosition = localPos;
        wall.transform.localScale = scale;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        wall.GetComponent<Renderer>().material = mat;
    }

    static void CreateInteractionHint(GameObject stationRoot, StoreInterior storeInterior, string goName, string text, bool isEnter)
    {
        // Find or create HUD canvas
        var hudCanvas = GameObject.Find("HUDCanvas");
        if (hudCanvas == null) return;

        var hintGO = new GameObject(goName);
        hintGO.transform.SetParent(hudCanvas.transform, false);

        var rect = hintGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.15f);
        rect.anchorMax = new Vector2(0.5f, 0.15f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(300f, 50f);

        // Background
        var bg = new GameObject("BG");
        bg.transform.SetParent(hintGO.transform, false);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.6f);

        // Text
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(hintGO.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        hintGO.SetActive(false);

        if (isEnter)
            storeInterior.enterHint = hintGO;
        else
            storeInterior.exitHint = hintGO;
    }
}
