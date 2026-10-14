using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// Rebuilds the GameScene map using Polygon City Pack assets.
/// 80x80 unit enclosed city block: road center, sidewalks, grass, solid border walls, store, cars.
/// </summary>
public class RebuildCityMap : MonoBehaviour
{
    [MenuItem("Tools/Rebuild City Map")]
    public static void Execute()
    {
        RemoveOldEnvironment();

        const float groundY = 0f;
        const float mapHalf = 40f;

        BuildGround(groundY);
        BuildBorderWalls(mapHalf, groundY);
        BuildLootableStore(groundY);
        PlaceCars(groundY);
        PlaceStreetProps(groundY);
        RepositionPlayer(groundY);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[RebuildCityMap] Done!");
    }

    static void RemoveOldEnvironment()
    {
        string[] names = { "Road", "Props", "AbandonedRoadTerrain", "CityMap",
                           "Ground", "Walls", "LootableStore", "Cars", "StreetProps", "MapEnvironment" };
        foreach (var n in names)
        {
            var go = GameObject.Find(n);
            if (go != null) { Undo.DestroyObjectImmediate(go); Debug.Log($"Removed: {n}"); }
        }
    }

    // ── Ground: 14×14 interior tiles ─────────────────────────────────────────
    static void BuildGround(float y)
    {
        var root = new GameObject("Ground");
        Undo.RegisterCreatedObjectUndo(root, "Ground");

        var roadPrefab     = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Road_01.prefab");
        var sidewalkPrefab = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Sidewalk_01.prefab");
        var grassPrefab    = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Grass_01.prefab");

        for (int xi = 0; xi < 14; xi++)
        {
            float x = -32.5f + xi * 5f;
            for (int zi = 0; zi < 14; zi++)
            {
                float z = -32.5f + zi * 5f;
                bool isRoad     = (x >= -7.5f && x <= 7.5f);
                bool isSidewalk = !isRoad && (Mathf.Abs(x) <= 17.5f);
                var prefab = isRoad ? roadPrefab : (isSidewalk ? sidewalkPrefab : grassPrefab);
                if (prefab == null) continue;
                var tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
                tile.transform.position = new Vector3(x, y, z);
                tile.name = $"T_{xi}_{zi}";
            }
        }
        Debug.Log("[RebuildCityMap] Ground built.");
    }

    // ── Border Walls ──────────────────────────────────────────────────────────
    // Uses SM_Env_Bridge_Wall_01 (5.0 wide × 3.96 tall × 1.29 deep) as wall panels.
    // Stacked 2 high for ~8 units total height.
    // Wall panels placed at the map edge (X or Z = ±40), facing inward.
    // The panel pivot is at its center, depth=1.29, so inner face at 40 - 0.645 = 39.355
    // We place panels at X/Z = ±39.355 so inner face is flush with ±38.71 (close to ground edge ±35)
    // Actually: place at ±38 so inner face is at ±38 - 0.645 = ±37.355 ≈ ±37.5 (ground tile edge)
    // 
    // For a clean look: place wall panel centers at ±38.5 (inner face at ±37.855, outer at ±39.145)
    // This keeps walls tight against the ground tiles.
    //
    // 16 panels per side at X/Z = -37.5 to +37.5 step 5
    static void BuildBorderWalls(float half, float y)
    {
        var root = new GameObject("Walls");
        Undo.RegisterCreatedObjectUndo(root, "Walls");

        // Bridge wall panel: 5.0 wide × 3.96 tall × 1.29 deep
        var wallPanel = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Bridge_Wall_01.prefab");

        // Apartment stack for extra height/visual variety on top
        var aptStack  = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Apartment_Stack_01.prefab");
        var aptPrefab = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Apartment_01.prefab");

        if (wallPanel == null && aptPrefab == null)
        {
            BuildPrimitiveWalls(root, half, y);
            Debug.Log("[RebuildCityMap] Walls built (primitive fallback).");
            return;
        }

        const float tileSize  = 5f;
        const float panelH    = 3.96f;  // bridge wall height
        const int   panelRows = 2;      // stack 2 rows = ~8 units tall
        // Panel depth = 1.29, so pivot offset from inner face = 0.645
        // Place panel pivot at 38.5 → inner face at 37.855 (just inside ground edge at 35+2.5=37.5)
        const float wallPivot = 38.5f;

        // 16 tile positions along each wall
        float[] pos = new float[16];
        for (int i = 0; i < 16; i++) pos[i] = -37.5f + i * tileSize;

        void PlacePanel(float wx, float wz, float rotY, string label)
        {
            if (wallPanel == null) return;
            for (int row = 0; row < panelRows; row++)
            {
                var w = (GameObject)PrefabUtility.InstantiatePrefab(wallPanel, root.transform);
                w.transform.SetPositionAndRotation(
                    new Vector3(wx, y + row * panelH, wz),
                    Quaternion.Euler(0, rotY, 0));
                w.name = $"WallPanel_{label}_R{row}";
            }
        }

        // North wall: panels at Z=+wallPivot, facing south (rot 180)
        for (int i = 0; i < 16; i++)
            PlacePanel(pos[i], wallPivot, 180f, $"N{i}");

        // South wall: panels at Z=-wallPivot, facing north (rot 0)
        for (int i = 0; i < 16; i++)
            PlacePanel(pos[i], -wallPivot, 0f, $"S{i}");

        // East wall: panels at X=+wallPivot, facing west (rot 270)
        for (int i = 0; i < 16; i++)
            PlacePanel(wallPivot, pos[i], 270f, $"E{i}");

        // West wall: panels at X=-wallPivot, facing east (rot 90)
        for (int i = 0; i < 16; i++)
            PlacePanel(-wallPivot, pos[i], 90f, $"W{i}");

        // ── Solid invisible collision cubes at the true map edge ──────────
        float wallH = panelH * panelRows;
        float wallCY = y + wallH / 2f;
        MakeInvisibleCube(root, new Vector3(0,      wallCY,  half), new Vector3(half*2, wallH, 2f), "Col_N");
        MakeInvisibleCube(root, new Vector3(0,      wallCY, -half), new Vector3(half*2, wallH, 2f), "Col_S");
        MakeInvisibleCube(root, new Vector3( half,  wallCY,  0),    new Vector3(2f, wallH, half*2), "Col_E");
        MakeInvisibleCube(root, new Vector3(-half,  wallCY,  0),    new Vector3(2f, wallH, half*2), "Col_W");

        Debug.Log("[RebuildCityMap] Walls built.");
    }

    static void BuildPrimitiveWalls(GameObject root, float half, float y)
    {
        float h = 8f, t = 2f, cy = y + h / 2f;
        MakeInvisibleCube(root, new Vector3(0,     cy,  half), new Vector3(half*2+t*2, h, t), "N");
        MakeInvisibleCube(root, new Vector3(0,     cy, -half), new Vector3(half*2+t*2, h, t), "S");
        MakeInvisibleCube(root, new Vector3( half, cy,  0),    new Vector3(t, h, half*2), "E");
        MakeInvisibleCube(root, new Vector3(-half, cy,  0),    new Vector3(t, h, half*2), "W");
    }

    static void MakeInvisibleCube(GameObject parent, Vector3 pos, Vector3 scale, string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetParent(parent.transform);
        go.transform.position   = pos;
        go.transform.localScale = scale;
        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;
        go.name = name;
    }

    // ── Lootable Store ────────────────────────────────────────────────────────
    static void BuildLootableStore(float y)
    {
        var root = new GameObject("LootableStore");
        Undo.RegisterCreatedObjectUndo(root, "LootableStore");

        var shopPrefab = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_03.prefab")
                      ?? Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_01.prefab");

        if (shopPrefab != null)
        {
            var shop = (GameObject)PrefabUtility.InstantiatePrefab(shopPrefab, root.transform);
            shop.transform.SetPositionAndRotation(new Vector3(-22f, y, 5f), Quaternion.Euler(0, 90f, 0));
            shop.name = "ShopBuilding";
        }

        var trigGO = new GameObject("StoreTrigger_Entrance");
        trigGO.transform.SetParent(root.transform);
        trigGO.transform.position = new Vector3(-15f, y + 1f, 5f);
        var col = trigGO.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(3f, 2f, 4f);
        var trig = trigGO.AddComponent<StoreTrigger>();
        trig.triggerType = StoreTrigger.TriggerType.Entrance;

        Debug.Log("[RebuildCityMap] Store placed.");
    }

    // ── Cars ──────────────────────────────────────────────────────────────────
    static void PlaceCars(float y)
    {
        var root = new GameObject("Cars");
        Undo.RegisterCreatedObjectUndo(root, "Cars");

        string[] paths = {
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Sedan_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Small_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Medium_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Muscle_01.prefab",
        };

        var p0 = Load(paths[0]);
        if (p0 != null) Inst(p0, root, new Vector3(-11f, y, 12f), Quaternion.Euler(0, 0, 0), "Car_Left");

        float[] zPos = { 15f, 5f, -5f };
        for (int i = 0; i < 3; i++)
        {
            var p = Load(paths[i + 1]);
            if (p != null) Inst(p, root, new Vector3(12f, y, zPos[i]), Quaternion.Euler(0, 180f, 0), $"Car_Right_{i+1}");
        }

        Debug.Log("[RebuildCityMap] Cars placed.");
    }

    // ── Street Props ──────────────────────────────────────────────────────────
    static void PlaceStreetProps(float y)
    {
        var root = new GameObject("StreetProps");
        Undo.RegisterCreatedObjectUndo(root, "StreetProps");

        var lightPrefab = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_LightPole_Base_01.prefab");
        if (lightPrefab != null)
        {
            float[] lz = { -25f, -10f, 5f, 20f };
            foreach (float z in lz)
            {
                Inst(lightPrefab, root, new Vector3(-9f, y, z), Quaternion.Euler(0,  90f, 0), $"Light_L_{z}");
                Inst(lightPrefab, root, new Vector3( 9f, y, z), Quaternion.Euler(0, 270f, 0), $"Light_R_{z}");
            }
        }

        var tree1 = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Tree_01.prefab");
        if (tree1 != null)
        {
            float[] tz = { -28f, -18f, -8f, 2f, 12f, 22f };
            foreach (float z in tz)
                Inst(tree1, root, new Vector3(-27f, y, z), Quaternion.identity, $"Tree_L_{z}");
        }

        var tree2 = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Tree_02.prefab") ?? tree1;
        if (tree2 != null)
        {
            float[] tz2 = { -25f, -15f, -5f, 5f, 15f, 25f };
            foreach (float z in tz2)
                Inst(tree2, root, new Vector3(27f, y, z), Quaternion.identity, $"Tree_R_{z}");
        }

        Debug.Log("[RebuildCityMap] Props placed.");
    }

    static void RepositionPlayer(float y)
    {
        var player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0f, y + 0.1f, 0f);
            Debug.Log("[RebuildCityMap] Player repositioned.");
        }
    }

    static GameObject Load(string path) => AssetDatabase.LoadAssetAtPath<GameObject>(path);

    static void Inst(GameObject prefab, GameObject parent, Vector3 pos, Quaternion rot, string name)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
        go.transform.SetPositionAndRotation(pos, rot);
        go.name = name;
    }
}
