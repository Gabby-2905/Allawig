using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

/// <summary>
/// Builds a post-apocalyptic urban map inspired by the reference image:
/// - Large central civic/government building (City Hall)
/// - Surrounding market stalls and shanty structures
/// - Scattered abandoned vehicles
/// - Trees throughout
/// - Background city buildings
/// - 2 lootable buildings with StoreInterior triggers
/// - Grass ground with road paths
/// - Warm hazy atmosphere
/// </summary>
public class BuildUrbanJunkyardMap : EditorWindow
{
    const string SYNTY_BLD  = "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/";
    const string SYNTY_ENV  = "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/";
    const string SYNTY_PROP = "Assets/SyntyStudios/PolygonCity/Prefabs/Props/";
    const string SYNTY_VEH  = "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/";
    const string CONT_PATH  = "Assets/Sayantan/Props/Large/Container_FREE/Prefabs/Clean/";

    [MenuItem("Tools/Build Urban Junkyard Map")]
    public static void Build()
    {
        // ── 1. Wipe scene ─────────────────────────────────────────────────
        var roots = new List<GameObject>();
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects(roots);
        foreach (var go in roots)
        {
            if (go.name == "Main Camera") continue;
            Undo.DestroyObjectImmediate(go);
        }

        // ── 2. Lighting ───────────────────────────────────────────────────
        SetupLighting();
        CreateDirectionalLight();

        // ── 3. Ground ─────────────────────────────────────────────────────
        BuildGround();

        // ── 4. Roads / paths ──────────────────────────────────────────────
        BuildRoads();

        // ── 5. Central City Hall (main focal building) ────────────────────
        BuildCityHall();

        // ── 6. Lootable Building 1 – Warehouse (left side) ────────────────
        BuildLootable1();

        // ── 7. Lootable Building 2 – Shop row (right side) ────────────────
        BuildLootable2();

        // ── 8. Market stalls / shanty structures around plaza ─────────────
        BuildMarketArea();

        // ── 9. Vehicles scattered around ─────────────────────────────────
        PlaceVehicles();

        // ── 10. Trees throughout ──────────────────────────────────────────
        PlaceTrees();

        // ── 11. Background city buildings ─────────────────────────────────
        PlaceBackgroundBuildings();

        // ── 12. Props (trash, boxes, containers) ──────────────────────────
        PlaceProps();

        // ── 13. Post-process ──────────────────────────────────────────────
        AddPostProcess();

        // ── 14. Frame & save ──────────────────────────────────────────────
        FrameScene();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("[BuildUrbanJunkyardMap] Done!");
    }

    // ─────────────────────────────────────────────────────────────────────
    static void SetupLighting()
    {
        // Warm hazy overcast sky matching reference image
        var skyboxMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/DaySkybox.mat");
        if (skyboxMat != null) RenderSettings.skybox = skyboxMat;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.72f, 0.68f, 0.52f);
        RenderSettings.ambientEquatorColor = new Color(0.55f, 0.50f, 0.38f);
        RenderSettings.ambientGroundColor  = new Color(0.22f, 0.20f, 0.15f);

        // Light haze fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.72f, 0.68f, 0.52f, 1f);
        RenderSettings.fogStartDistance = 60f;
        RenderSettings.fogEndDistance   = 200f;
    }

    static void CreateDirectionalLight()
    {
        var go = new GameObject("Directional Light");
        Undo.RegisterCreatedObjectUndo(go, "Dir Light");
        var l = go.AddComponent<Light>();
        l.type = LightType.Directional;
        l.color = new Color(1f, 0.95f, 0.78f);
        l.intensity = 1.2f;
        l.shadows = LightShadows.Soft;
        go.transform.rotation = Quaternion.Euler(50f, -20f, 0f);
    }

    // ─────────────────────────────────────────────────────────────────────
    static void BuildGround()
    {
        // Large grass ground – 120x120
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground_Grass";
        Undo.RegisterCreatedObjectUndo(ground, "Ground");
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(12f, 1f, 12f);
        var grassMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/TerrainGrass.mat");
        if (grassMat == null) grassMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/MuddyGround.mat");
        if (grassMat != null) ground.GetComponent<Renderer>().sharedMaterial = grassMat;

        // Central plaza / courtyard – cracked asphalt
        var plaza = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plaza.name = "Ground_Plaza";
        Undo.RegisterCreatedObjectUndo(plaza, "Plaza");
        plaza.transform.position = new Vector3(0f, 0.01f, 0f);
        plaza.transform.localScale = new Vector3(4f, 1f, 4f);
        var asphaltMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/CrackedAsphalt.mat");
        if (asphaltMat != null) plaza.GetComponent<Renderer>().sharedMaterial = asphaltMat;
    }

    // ─────────────────────────────────────────────────────────────────────
    static void BuildRoads()
    {
        var roadParent = new GameObject("Roads");
        Undo.RegisterCreatedObjectUndo(roadParent, "Roads");

        var roadPrefab = Load(SYNTY_ENV + "SM_Env_Road_01.prefab");
        var sidewalkPrefab = Load(SYNTY_ENV + "SM_Env_Sidewalk_01.prefab");
        if (roadPrefab == null) return;

        // Main road running North-South through center
        for (int i = -5; i <= 5; i++)
        {
            Place(roadPrefab, new Vector3(0f, 0.02f, i * 10f), Quaternion.identity, roadParent.transform);
        }
        // Cross road East-West
        for (int i = -5; i <= 5; i++)
        {
            Place(roadPrefab, new Vector3(i * 10f, 0.02f, 0f), Quaternion.Euler(0, 90, 0), roadParent.transform);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void BuildCityHall()
    {
        // Central government building – City Hall
        var root = new GameObject("CityHall_Central");
        Undo.RegisterCreatedObjectUndo(root, "CityHall");
        root.transform.position = new Vector3(0f, 0f, 8f);

        var prefab = Load(SYNTY_BLD + "SM_Bld_CityHall_01.prefab");
        if (prefab != null)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.Euler(0, 180, 0);
            Undo.RegisterCreatedObjectUndo(go, "CityHall Bld");
        }

        // Flanking office buildings (wings)
        var wingPrefab = Load(SYNTY_BLD + "SM_Bld_OfficeOld_Large_01.prefab");
        if (wingPrefab != null)
        {
            var leftWing = (GameObject)PrefabUtility.InstantiatePrefab(wingPrefab, root.transform);
            leftWing.transform.localPosition = new Vector3(-22f, 0f, 0f);
            leftWing.transform.localRotation = Quaternion.Euler(0, 180, 0);
            Undo.RegisterCreatedObjectUndo(leftWing, "Left Wing");

            var rightWing = (GameObject)PrefabUtility.InstantiatePrefab(wingPrefab, root.transform);
            rightWing.transform.localPosition = new Vector3(22f, 0f, 0f);
            rightWing.transform.localRotation = Quaternion.Euler(0, 180, 0);
            Undo.RegisterCreatedObjectUndo(rightWing, "Right Wing");
        }

        // Water tower on top-right
        var towerPrefab = Load(SYNTY_BLD + "SM_Prop_Water_Tower_01.prefab");
        if (towerPrefab != null)
        {
            var tower = (GameObject)PrefabUtility.InstantiatePrefab(towerPrefab, root.transform);
            tower.transform.localPosition = new Vector3(18f, 0f, 5f);
            Undo.RegisterCreatedObjectUndo(tower, "Water Tower");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void BuildLootable1()
    {
        // Lootable Building 1: Station/Depot on the LEFT side (like the "Ration Depot" in reference)
        var root = new GameObject("LootableBuilding_Depot");
        Undo.RegisterCreatedObjectUndo(root, "Lootable1");
        root.transform.position = new Vector3(-38f, 0f, 5f);

        var bldPrefab = Load(SYNTY_BLD + "SM_Bld_Station_01.prefab");
        if (bldPrefab != null)
        {
            var bld = (GameObject)PrefabUtility.InstantiatePrefab(bldPrefab, root.transform);
            bld.transform.localPosition = Vector3.zero;
            bld.transform.localRotation = Quaternion.Euler(0, 90, 0);
            Undo.RegisterCreatedObjectUndo(bld, "Depot Bld");
        }

        SetupLootTriggers(root, "Depot",
            new Vector3(8f, 1f, 0f),
            new Vector3(-8f, 1f, 0f),
            new Vector3(6f, 0.1f, 0f),
            new Vector3(-6f, 0.1f, 0f));
    }

    static void BuildLootable2()
    {
        // Lootable Building 2: Shop row on the RIGHT side (market stalls)
        var root = new GameObject("LootableBuilding_Market");
        Undo.RegisterCreatedObjectUndo(root, "Lootable2");
        root.transform.position = new Vector3(38f, 0f, -5f);

        var bldPrefab = Load(SYNTY_BLD + "SM_Bld_Shop_05.prefab");
        if (bldPrefab == null) bldPrefab = Load(SYNTY_BLD + "SM_Bld_Shop_04.prefab");
        if (bldPrefab != null)
        {
            var bld = (GameObject)PrefabUtility.InstantiatePrefab(bldPrefab, root.transform);
            bld.transform.localPosition = Vector3.zero;
            bld.transform.localRotation = Quaternion.Euler(0, -90, 0);
            Undo.RegisterCreatedObjectUndo(bld, "Market Bld");
        }

        // Add a second shop next to it
        var bld2Prefab = Load(SYNTY_BLD + "SM_Bld_Shop_06.prefab");
        if (bld2Prefab == null) bld2Prefab = Load(SYNTY_BLD + "SM_Bld_Shop_03.prefab");
        if (bld2Prefab != null)
        {
            var bld2 = (GameObject)PrefabUtility.InstantiatePrefab(bld2Prefab, root.transform);
            bld2.transform.localPosition = new Vector3(0f, 0f, 12f);
            bld2.transform.localRotation = Quaternion.Euler(0, -90, 0);
            Undo.RegisterCreatedObjectUndo(bld2, "Market Bld2");
        }

        SetupLootTriggers(root, "Market",
            new Vector3(-8f, 1f, 0f),
            new Vector3(8f, 1f, 0f),
            new Vector3(-6f, 0.1f, 0f),
            new Vector3(6f, 0.1f, 0f));
    }

    static void SetupLootTriggers(GameObject root, string label,
        Vector3 entranceLocal, Vector3 exitLocal,
        Vector3 enterSpawnLocal, Vector3 exitSpawnLocal)
    {
        var eTrig = new GameObject("EntranceTrigger");
        eTrig.transform.SetParent(root.transform);
        eTrig.transform.localPosition = entranceLocal;
        var ec = eTrig.AddComponent<BoxCollider>();
        ec.isTrigger = true; ec.size = new Vector3(4f, 2.5f, 2f);

        var xTrig = new GameObject("ExitTrigger");
        xTrig.transform.SetParent(root.transform);
        xTrig.transform.localPosition = exitLocal;
        var xc = xTrig.AddComponent<BoxCollider>();
        xc.isTrigger = true; xc.size = new Vector3(4f, 2.5f, 2f);

        var eSpawn = new GameObject("EnterSpawnPoint");
        eSpawn.transform.SetParent(root.transform);
        eSpawn.transform.localPosition = enterSpawnLocal;

        var xSpawn = new GameObject("ExitSpawnPoint");
        xSpawn.transform.SetParent(root.transform);
        xSpawn.transform.localPosition = exitSpawnLocal;

        // Interior loot
        var interior = new GameObject("InteriorRoot");
        interior.transform.SetParent(root.transform);
        interior.transform.localPosition = Vector3.zero;
        PlaceInteriorLoot(interior.transform);

        var exterior = new GameObject("ExteriorRoot");
        exterior.transform.SetParent(root.transform);

        var enterHint = new GameObject($"EnterHint_{label}");
        enterHint.transform.SetParent(root.transform);
        enterHint.transform.localPosition = entranceLocal + new Vector3(0, 1.5f, 0);
        enterHint.SetActive(false);

        var exitHint = new GameObject($"ExitHint_{label}");
        exitHint.transform.SetParent(root.transform);
        exitHint.transform.localPosition = exitLocal + new Vector3(0, 1.5f, 0);
        exitHint.SetActive(false);

        var si = root.AddComponent<StoreInterior>();
        si.exteriorRoot = exterior;
        si.interiorRoot = interior;
        si.enterSpawnPoint = eSpawn.transform;
        si.exitSpawnPoint  = xSpawn.transform;
        si.enterHint = enterHint;
        si.exitHint  = exitHint;

        var stE = eTrig.AddComponent<StoreTrigger>();
        stE.triggerType = StoreTrigger.TriggerType.Entrance;
        stE.storeInterior = si;

        var stX = xTrig.AddComponent<StoreTrigger>();
        stX.triggerType = StoreTrigger.TriggerType.Exit;
        stX.storeInterior = si;
    }

    static void PlaceInteriorLoot(Transform parent)
    {
        string[] paths = {
            SYNTY_PROP + "SM_Prop_CardboardBox_01.prefab",
            SYNTY_PROP + "SM_Prop_CardboardBox_02.prefab",
            SYNTY_PROP + "SM_Prop_Pallet_01.prefab",
            SYNTY_PROP + "SM_Prop_Skip_01.prefab",
        };
        Vector3[] positions = {
            new Vector3(-1.5f, 0f, -1f),
            new Vector3( 1.5f, 0f, -1f),
            new Vector3( 0f,   0f,  1.5f),
            new Vector3(-2f,   0f,  0.5f),
        };
        for (int i = 0; i < positions.Length; i++)
        {
            var p = Load(paths[i]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent);
            go.transform.localPosition = positions[i];
            go.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Undo.RegisterCreatedObjectUndo(go, "Loot");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void BuildMarketArea()
    {
        var parent = new GameObject("Market_Stalls");
        Undo.RegisterCreatedObjectUndo(parent, "Market");

        // Shop covers / awnings around the plaza (like market stalls in reference)
        string[] coverPaths = {
            SYNTY_BLD + "SM_Bld_Shop_Cover_01.prefab",
            SYNTY_BLD + "SM_Bld_Shop_Cover_02.prefab",
            SYNTY_BLD + "SM_Bld_Shop_Cover_03.prefab",
            SYNTY_BLD + "SM_Bld_Shop_Cover_04.prefab",
            SYNTY_BLD + "SM_Bld_Shop_Cover_05.prefab",
        };

        // Ring of market stalls around the central plaza
        (Vector3 pos, float rot)[] stallSpots = {
            (new Vector3(-18f, 0f, -12f),   0f),
            (new Vector3(-12f, 0f, -18f),  90f),
            (new Vector3(  0f, 0f, -20f),   0f),
            (new Vector3( 12f, 0f, -18f), -90f),
            (new Vector3( 18f, 0f, -12f), 180f),
            (new Vector3(-20f, 0f,   0f),  90f),
            (new Vector3( 20f, 0f,   0f), -90f),
            (new Vector3(-18f, 0f,  12f),   0f),
            (new Vector3( 18f, 0f,  12f), 180f),
            (new Vector3( -8f, 0f, -22f),  45f),
            (new Vector3(  8f, 0f, -22f), -45f),
        };

        for (int i = 0; i < stallSpots.Length; i++)
        {
            var p = Load(coverPaths[i % coverPaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = stallSpots[i].pos;
            go.transform.rotation = Quaternion.Euler(0, stallSpots[i].rot, 0);
            Undo.RegisterCreatedObjectUndo(go, "Stall");
        }

        // Small shops on south side
        string[] shopPaths = {
            SYNTY_BLD + "SM_Bld_Shop_01.prefab",
            SYNTY_BLD + "SM_Bld_Shop_02.prefab",
            SYNTY_BLD + "SM_Bld_Shop_03.prefab",
            SYNTY_BLD + "SM_Bld_Shop_04.prefab",
        };
        (Vector3 pos, float rot)[] shopSpots = {
            (new Vector3(-28f, 0f, -20f),   0f),
            (new Vector3(-16f, 0f, -28f),   0f),
            (new Vector3(  0f, 0f, -32f),   0f),
            (new Vector3( 16f, 0f, -28f),   0f),
        };
        for (int i = 0; i < shopSpots.Length; i++)
        {
            var p = Load(shopPaths[i % shopPaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = shopSpots[i].pos;
            go.transform.rotation = Quaternion.Euler(0, shopSpots[i].rot, 0);
            Undo.RegisterCreatedObjectUndo(go, "Shop");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void PlaceVehicles()
    {
        var parent = new GameObject("Vehicles");
        Undo.RegisterCreatedObjectUndo(parent, "Vehicles");

        string[] vehPaths = {
            SYNTY_VEH + "SM_Veh_Car_Sedan_01.prefab",
            SYNTY_VEH + "SM_Veh_Car_Small_01.prefab",
            SYNTY_VEH + "SM_Veh_Car_Van_01.prefab",
            SYNTY_VEH + "SM_Veh_Car_Muscle_01.prefab",
            SYNTY_VEH + "SM_Veh_Car_Medium_01.prefab",
            SYNTY_VEH + "SM_Veh_Car_Taxi_01.prefab",
            SYNTY_VEH + "SM_Veh_Car_Police_01.prefab",
            SYNTY_VEH + "SM_Veh_Car_Ambo_01.prefab",
        };

        (Vector3 pos, float rot)[] spots = {
            // Parking lot left of city hall
            (new Vector3(-14f, 0f,  18f),  10f),
            (new Vector3(-10f, 0f,  18f),  -5f),
            (new Vector3( -6f, 0f,  18f),   0f),
            // Parking lot right of city hall
            (new Vector3( 10f, 0f,  18f),   5f),
            (new Vector3( 14f, 0f,  18f), -10f),
            (new Vector3( 18f, 0f,  18f),   0f),
            // Scattered on roads
            (new Vector3(-30f, 0f,  -8f),  90f),
            (new Vector3( 30f, 0f,  10f), -90f),
            // Abandoned in plaza
            (new Vector3( -8f, 0f,  -8f),  35f),
            (new Vector3(  6f, 0f, -10f), -20f),
        };

        for (int i = 0; i < spots.Length; i++)
        {
            var p = Load(vehPaths[i % vehPaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = spots[i].pos;
            go.transform.rotation = Quaternion.Euler(0, spots[i].rot, 0);
            Undo.RegisterCreatedObjectUndo(go, "Vehicle");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void PlaceTrees()
    {
        var parent = new GameObject("Trees");
        Undo.RegisterCreatedObjectUndo(parent, "Trees");

        string[] treePaths = {
            SYNTY_ENV + "SM_Env_Tree_01.prefab",
            SYNTY_ENV + "SM_Env_Tree_02.prefab",
            SYNTY_ENV + "SM_Env_Tree_03.prefab",
        };

        // Trees lining the main road and scattered on grass
        Vector3[] treePositions = {
            // Along north road (flanking city hall)
            new Vector3(-8f,  0f, 30f), new Vector3(-4f,  0f, 30f),
            new Vector3( 4f,  0f, 30f), new Vector3( 8f,  0f, 30f),
            // Left side tree line
            new Vector3(-25f, 0f, 25f), new Vector3(-25f, 0f, 15f),
            new Vector3(-25f, 0f,  5f), new Vector3(-25f, 0f, -5f),
            new Vector3(-25f, 0f,-15f), new Vector3(-25f, 0f,-25f),
            // Right side tree line
            new Vector3( 25f, 0f, 25f), new Vector3( 25f, 0f, 15f),
            new Vector3( 25f, 0f,  5f), new Vector3( 25f, 0f, -5f),
            new Vector3( 25f, 0f,-15f), new Vector3( 25f, 0f,-25f),
            // South scattered
            new Vector3(-15f, 0f,-35f), new Vector3( 0f,  0f,-38f),
            new Vector3( 15f, 0f,-35f), new Vector3(-30f, 0f,-30f),
            new Vector3( 30f, 0f,-30f),
            // Background tree clusters
            new Vector3(-45f, 0f, 35f), new Vector3(-40f, 0f, 40f),
            new Vector3(-35f, 0f, 35f), new Vector3( 35f, 0f, 40f),
            new Vector3( 40f, 0f, 35f), new Vector3( 45f, 0f, 40f),
            new Vector3(-50f, 0f, 10f), new Vector3(-50f, 0f,-10f),
            new Vector3( 50f, 0f, 10f), new Vector3( 50f, 0f,-10f),
        };

        for (int i = 0; i < treePositions.Length; i++)
        {
            var p = Load(treePaths[i % treePaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = treePositions[i];
            go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            float s = Random.Range(0.9f, 1.3f);
            go.transform.localScale = Vector3.one * s;
            Undo.RegisterCreatedObjectUndo(go, "Tree");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void PlaceBackgroundBuildings()
    {
        var parent = new GameObject("Background_Buildings");
        Undo.RegisterCreatedObjectUndo(parent, "BG Buildings");

        // Apartment blocks on the right (like reference image top-right)
        string[] aptPaths = {
            SYNTY_BLD + "SM_Bld_Apartment_Stack_01.prefab",
            SYNTY_BLD + "SM_Bld_Apartment_Stack_02.prefab",
            SYNTY_BLD + "SM_Bld_Apartment_Stack_03.prefab",
            SYNTY_BLD + "SM_Bld_Apartment_01.prefab",
            SYNTY_BLD + "SM_Bld_Apartment_02.prefab",
            SYNTY_BLD + "SM_Bld_Apartment_03.prefab",
        };

        (Vector3 pos, float rot)[] bgSpots = {
            // Right cluster (like reference top-right)
            (new Vector3( 52f, 0f, 30f),   0f),
            (new Vector3( 62f, 0f, 30f),   0f),
            (new Vector3( 52f, 0f, 18f),   0f),
            (new Vector3( 62f, 0f, 18f),   0f),
            // Left background
            (new Vector3(-55f, 0f, 20f), 180f),
            (new Vector3(-55f, 0f,  5f), 180f),
            // North background
            (new Vector3(-20f, 0f, 55f),  90f),
            (new Vector3(  0f, 0f, 58f),   0f),
            (new Vector3( 20f, 0f, 55f), -90f),
        };

        for (int i = 0; i < bgSpots.Length; i++)
        {
            var p = Load(aptPaths[i % aptPaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = bgSpots[i].pos;
            go.transform.rotation = Quaternion.Euler(0, bgSpots[i].rot, 0);
            Undo.RegisterCreatedObjectUndo(go, "BG Bld");
        }

        // Office buildings on far edges
        string[] officePaths = {
            SYNTY_BLD + "SM_Bld_OfficeOld_Small_01.prefab",
            SYNTY_BLD + "SM_Bld_OfficeOld_Small_02.prefab",
            SYNTY_BLD + "SM_Bld_OfficeOld_Large_01.prefab",
        };
        (Vector3 pos, float rot)[] officeSpots = {
            (new Vector3(-50f, 0f,-30f),  45f),
            (new Vector3( 50f, 0f,-30f), -45f),
            (new Vector3(  0f, 0f,-55f), 180f),
        };
        for (int i = 0; i < officeSpots.Length; i++)
        {
            var p = Load(officePaths[i % officePaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = officeSpots[i].pos;
            go.transform.rotation = Quaternion.Euler(0, officeSpots[i].rot, 0);
            Undo.RegisterCreatedObjectUndo(go, "Office");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void PlaceProps()
    {
        var parent = new GameObject("Props");
        Undo.RegisterCreatedObjectUndo(parent, "Props");

        // Containers near depot
        string[] contPaths = {
            CONT_PATH + "PfCont40FBlue.prefab",
            CONT_PATH + "PfCont20FBeige.prefab",
            CONT_PATH + "PfCont40FBeige.prefab",
        };
        (Vector3 pos, float rot)[] contSpots = {
            (new Vector3(-42f, 0f, 15f),   0f),
            (new Vector3(-42f, 0f,  5f),   0f),
            (new Vector3(-42f, 0f, -5f),  90f),
        };
        for (int i = 0; i < contSpots.Length; i++)
        {
            var p = Load(contPaths[i % contPaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = contSpots[i].pos;
            go.transform.rotation = Quaternion.Euler(0, contSpots[i].rot, 0);
            Undo.RegisterCreatedObjectUndo(go, "Container");
        }

        // Trash / debris scattered around market
        string[] trashPaths = {
            SYNTY_PROP + "SM_Prop_TrashBag_01.prefab",
            SYNTY_PROP + "SM_Prop_TrashBag_02.prefab",
            SYNTY_PROP + "SM_Prop_CardboardBox_01.prefab",
            SYNTY_PROP + "SM_Prop_CardboardBox_03.prefab",
            SYNTY_PROP + "SM_Prop_Pallet_01.prefab",
            SYNTY_PROP + "SM_Prop_Skip_01.prefab",
            SYNTY_PROP + "SM_Prop_Trashbin_01.prefab",
        };
        Vector3[] trashPos = {
            new Vector3(-5f, 0f,-15f), new Vector3( 5f, 0f,-15f),
            new Vector3(-10f,0f,-10f), new Vector3(10f, 0f,-10f),
            new Vector3(-22f,0f, 10f), new Vector3( 22f,0f, 10f),
            new Vector3(-15f,0f,-22f), new Vector3(15f, 0f,-22f),
            new Vector3(  0f,0f,-25f), new Vector3(-8f, 0f, -5f),
        };
        for (int i = 0; i < trashPos.Length; i++)
        {
            var p = Load(trashPaths[i % trashPaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = trashPos[i];
            go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Undo.RegisterCreatedObjectUndo(go, "Trash");
        }

        // Light poles along main road
        var polePrefab = Load(SYNTY_PROP + "SM_Prop_LightPole_Base_01.prefab");
        if (polePrefab != null)
        {
            Vector3[] polePos = {
                new Vector3(-5f, 0f, 20f), new Vector3(5f, 0f, 20f),
                new Vector3(-5f, 0f,-20f), new Vector3(5f, 0f,-20f),
                new Vector3(-35f,0f,  0f), new Vector3(35f,0f,  0f),
            };
            foreach (var pos in polePos)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(polePrefab, parent.transform);
                go.transform.position = pos;
                Undo.RegisterCreatedObjectUndo(go, "Pole");
            }
        }

        // Barriers / barricades around city hall entrance
        var barrierPrefab = Load(SYNTY_PROP + "SM_Prop_Barrier_01.prefab");
        if (barrierPrefab != null)
        {
            Vector3[] barrierPos = {
                new Vector3(-6f, 0f, -2f), new Vector3(-3f, 0f, -2f),
                new Vector3( 3f, 0f, -2f), new Vector3( 6f, 0f, -2f),
            };
            foreach (var pos in barrierPos)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(barrierPrefab, parent.transform);
                go.transform.position = pos;
                Undo.RegisterCreatedObjectUndo(go, "Barrier");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void AddPostProcess()
    {
        var volGO = new GameObject("PostProcess_Volume");
        Undo.RegisterCreatedObjectUndo(volGO, "PostProcess");
        var vol = volGO.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 1f;

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();

        // Warm desaturated look matching reference image
        var ca = profile.Add<ColorAdjustments>(false);
        ca.postExposure.Override(0.1f);
        ca.contrast.Override(15f);
        ca.colorFilter.Override(new Color(1f, 0.96f, 0.82f));
        ca.saturation.Override(-12f);

        var vig = profile.Add<Vignette>(false);
        vig.intensity.Override(0.25f);
        vig.smoothness.Override(0.4f);

        var bloom = profile.Add<Bloom>(false);
        bloom.threshold.Override(1.0f);
        bloom.intensity.Override(0.3f);

        string path = "Assets/Settings/UrbanJunkyardPostProcess.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(profile, path);
        AssetDatabase.SaveAssets();
        vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
    }

    // ─────────────────────────────────────────────────────────────────────
    static void FrameScene()
    {
        SceneView sv = SceneView.lastActiveSceneView;
        if (sv == null && SceneView.sceneViews.Count > 0)
            sv = (SceneView)SceneView.sceneViews[0];
        if (sv == null) return;
        sv.pivot = new Vector3(0f, 0f, 0f);
        sv.rotation = Quaternion.Euler(45f, -30f, 0f);
        sv.size = 80f;
        sv.orthographic = false;
        sv.Repaint();
    }

    // ─────────────────────────────────────────────────────────────────────
    static GameObject Load(string path) => AssetDatabase.LoadAssetAtPath<GameObject>(path);

    static void Place(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)
    {
        if (prefab == null) return;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.transform.position = pos;
        go.transform.rotation = rot;
        Undo.RegisterCreatedObjectUndo(go, "Place");
    }
}
