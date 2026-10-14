using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

/// <summary>
/// Builds a post-apocalyptic junkyard map in Level2Scene.
/// </summary>
public class BuildJunkyardLevel2 : EditorWindow
{
    [MenuItem("Tools/Build Junkyard Level 2")]
    public static void Build()
    {
        // ── 1. Clear existing scene objects (except Main Camera) ──────────
        var roots = new List<GameObject>();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        scene.GetRootGameObjects(roots);
        foreach (var go in roots)
        {
            if (go.name == "Main Camera") continue;
            Undo.DestroyObjectImmediate(go);
        }

        // ── 2. Lighting / Skybox ──────────────────────────────────────────
        SetupLighting();

        // ── 3. Ground plane ───────────────────────────────────────────────
        BuildGround();

        // ── 4. Boundary walls / fences ────────────────────────────────────
        BuildBoundaryFences();

        // ── 5. Lootable Building 1 – Abandoned Warehouse ──────────────────
        BuildLootableBuilding1();

        // ── 6. Lootable Building 2 – Ruined Shop ──────────────────────────
        BuildLootableBuilding2();

        // ── 7. Junkyard scatter (cars, containers, debris) ────────────────
        ScatterJunkyardProps();

        // ── 8. Post-process volume ────────────────────────────────────────
        AddPostProcessVolume();

        // ── 9. Directional light (apocalyptic orange sun) ─────────────────
        SetupDirectionalLight();

        // ── 10. Save scene ────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("[BuildJunkyardLevel2] Junkyard map built successfully!");
    }

    // ─────────────────────────────────────────────────────────────────────
    static void SetupLighting()
    {
        // Use the apocalyptic day skybox if available
        var skyboxMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/DaySkybox.mat");
        if (skyboxMat != null)
            RenderSettings.skybox = skyboxMat;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor    = new Color(0.55f, 0.38f, 0.20f); // warm orange sky
        RenderSettings.ambientEquatorColor = new Color(0.35f, 0.28f, 0.18f);
        RenderSettings.ambientGroundColor  = new Color(0.12f, 0.10f, 0.08f);

        // Fog – thick dusty haze
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.52f, 0.38f, 0.22f, 1f);
        RenderSettings.fogStartDistance = 30f;
        RenderSettings.fogEndDistance   = 120f;
    }

    // ─────────────────────────────────────────────────────────────────────
    static void SetupDirectionalLight()
    {
        var lightGO = new GameObject("Directional Light");
        Undo.RegisterCreatedObjectUndo(lightGO, "Create Dir Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.72f, 0.38f);   // warm orange
        light.intensity = 1.1f;
        light.shadows = LightShadows.Soft;
        lightGO.transform.rotation = Quaternion.Euler(42f, -30f, 0f);
    }

    // ─────────────────────────────────────────────────────────────────────
    static void BuildGround()
    {
        // Large cracked asphalt ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "JunkyardGround";
        Undo.RegisterCreatedObjectUndo(ground, "Create Ground");
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(8f, 1f, 8f); // 80x80 units

        // Apply cracked asphalt material
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/CrackedAsphalt.mat");
        if (mat == null) mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/MuddyGround.mat");
        if (mat != null)
            ground.GetComponent<Renderer>().sharedMaterial = mat;

        // Dirt patches
        var dirt = GameObject.CreatePrimitive(PrimitiveType.Plane);
        dirt.name = "DirtPatch";
        Undo.RegisterCreatedObjectUndo(dirt, "Create Dirt");
        dirt.transform.position = new Vector3(10f, 0.01f, 8f);
        dirt.transform.localScale = new Vector3(3f, 1f, 3f);
        var dirtMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/MuddyGround.mat");
        if (dirtMat != null)
            dirt.GetComponent<Renderer>().sharedMaterial = dirtMat;
    }

    // ─────────────────────────────────────────────────────────────────────
    static void BuildBoundaryFences()
    {
        var fenceParent = new GameObject("Boundary_Fences");
        Undo.RegisterCreatedObjectUndo(fenceParent, "Create Fences");

        // Use POLYGON city pack fence prefabs
        string fencePath = "Assets/POLYGON city pack/Prefabs/Props/fence c prefab.prefab";
        string wallPath1 = "Assets/POLYGON city pack/Prefabs/Props/Wall 1 prefab.prefab";
        string wallPath2 = "Assets/POLYGON city pack/Prefabs/Props/Wall 2 prefab.prefab";

        var fencePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fencePath);
        var wall1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(wallPath1);
        var wall2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(wallPath2);

        // Fallback to SyntyStudios fence
        if (fencePrefab == null)
            fencePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Fence_01.prefab");

        // Place fences along the 4 sides of the 80x80 map
        float mapHalf = 38f;
        float spacing = 6f;

        // North & South walls
        for (float x = -mapHalf; x <= mapHalf; x += spacing)
        {
            PlaceProp(fencePrefab ?? wall1Prefab, new Vector3(x, 0f, mapHalf), Quaternion.identity, fenceParent.transform);
            PlaceProp(fencePrefab ?? wall1Prefab, new Vector3(x, 0f, -mapHalf), Quaternion.identity, fenceParent.transform);
        }
        // East & West walls
        for (float z = -mapHalf; z <= mapHalf; z += spacing)
        {
            PlaceProp(fencePrefab ?? wall2Prefab, new Vector3(mapHalf, 0f, z), Quaternion.Euler(0, 90, 0), fenceParent.transform);
            PlaceProp(fencePrefab ?? wall2Prefab, new Vector3(-mapHalf, 0f, z), Quaternion.Euler(0, 90, 0), fenceParent.transform);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void BuildLootableBuilding1()
    {
        // Building 1: Abandoned Warehouse (Car Repair shop – industrial look)
        var root = new GameObject("LootableBuilding_Warehouse");
        Undo.RegisterCreatedObjectUndo(root, "Create Building 1");
        root.transform.position = new Vector3(-18f, 0f, 12f);

        // Main building prefab
        var bldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/POLYGON city pack/Prefabs/Buildings/Car_repair_prefab.prefab");
        if (bldPrefab == null)
            bldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/POLYGON city pack/Prefabs/Buildings/Building_E_prefab.prefab");

        if (bldPrefab != null)
        {
            var bld = (GameObject)PrefabUtility.InstantiatePrefab(bldPrefab, root.transform);
            bld.transform.localPosition = Vector3.zero;
            bld.transform.localRotation = Quaternion.Euler(0, 180, 0);
            Undo.RegisterCreatedObjectUndo(bld, "Place Building 1");
        }

        // Entrance trigger zone
        var entranceTrigger = new GameObject("EntranceTrigger");
        entranceTrigger.transform.SetParent(root.transform);
        entranceTrigger.transform.localPosition = new Vector3(0f, 1f, -12f);
        var ec = entranceTrigger.AddComponent<BoxCollider>();
        ec.isTrigger = true;
        ec.size = new Vector3(4f, 2f, 2f);

        // Exit trigger zone
        var exitTrigger = new GameObject("ExitTrigger");
        exitTrigger.transform.SetParent(root.transform);
        exitTrigger.transform.localPosition = new Vector3(0f, 1f, 12f);
        var exC = exitTrigger.AddComponent<BoxCollider>();
        exC.isTrigger = true;
        exC.size = new Vector3(4f, 2f, 2f);

        // Spawn points
        var enterSpawn = new GameObject("EnterSpawnPoint");
        enterSpawn.transform.SetParent(root.transform);
        enterSpawn.transform.localPosition = new Vector3(0f, 0.1f, -10f);

        var exitSpawn = new GameObject("ExitSpawnPoint");
        exitSpawn.transform.SetParent(root.transform);
        exitSpawn.transform.localPosition = new Vector3(0f, 0.1f, 10f);

        // Interior root (placeholder)
        var interiorRoot = new GameObject("InteriorRoot");
        interiorRoot.transform.SetParent(root.transform);
        interiorRoot.transform.localPosition = Vector3.zero;
        PlaceInteriorProps(interiorRoot.transform, "Warehouse");

        // Exterior root
        var exteriorRoot = new GameObject("ExteriorRoot");
        exteriorRoot.transform.SetParent(root.transform);

        // StoreInterior component
        var si = root.AddComponent<StoreInterior>();
        si.exteriorRoot = exteriorRoot;
        si.interiorRoot = interiorRoot;
        si.enterSpawnPoint = enterSpawn.transform;
        si.exitSpawnPoint = exitSpawn.transform;

        // StoreTrigger on entrance
        var stEntrance = entranceTrigger.AddComponent<StoreTrigger>();
        stEntrance.triggerType = StoreTrigger.TriggerType.Entrance;
        stEntrance.storeInterior = si;

        // StoreTrigger on exit
        var stExit = exitTrigger.AddComponent<StoreTrigger>();
        stExit.triggerType = StoreTrigger.TriggerType.Exit;
        stExit.storeInterior = si;

        // "Press E" hint UI
        AddInteractionHint(root.transform, new Vector3(0f, 3f, -12f), "Warehouse", si);

        // Scatter some debris around the building
        ScatterDebrisAround(root.transform.position, 14f, 20f, 8);
    }

    // ─────────────────────────────────────────────────────────────────────
    static void BuildLootableBuilding2()
    {
        // Building 2: Ruined Shop / Convenience Store
        var root = new GameObject("LootableBuilding_Shop");
        Undo.RegisterCreatedObjectUndo(root, "Create Building 2");
        root.transform.position = new Vector3(18f, 0f, -10f);

        var bldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/POLYGON city pack/Prefabs/Buildings/Shop_A_prefab.prefab");
        if (bldPrefab == null)
            bldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/POLYGON city pack/Prefabs/Buildings/Building_M_prefab.prefab");

        if (bldPrefab != null)
        {
            var bld = (GameObject)PrefabUtility.InstantiatePrefab(bldPrefab, root.transform);
            bld.transform.localPosition = Vector3.zero;
            bld.transform.localRotation = Quaternion.Euler(0, 0, 0);
            Undo.RegisterCreatedObjectUndo(bld, "Place Building 2");
        }

        // Entrance trigger
        var entranceTrigger = new GameObject("EntranceTrigger");
        entranceTrigger.transform.SetParent(root.transform);
        entranceTrigger.transform.localPosition = new Vector3(0f, 1f, -6f);
        var ec = entranceTrigger.AddComponent<BoxCollider>();
        ec.isTrigger = true;
        ec.size = new Vector3(4f, 2f, 2f);

        // Exit trigger
        var exitTrigger = new GameObject("ExitTrigger");
        exitTrigger.transform.SetParent(root.transform);
        exitTrigger.transform.localPosition = new Vector3(0f, 1f, 6f);
        var exC = exitTrigger.AddComponent<BoxCollider>();
        exC.isTrigger = true;
        exC.size = new Vector3(4f, 2f, 2f);

        // Spawn points
        var enterSpawn = new GameObject("EnterSpawnPoint");
        enterSpawn.transform.SetParent(root.transform);
        enterSpawn.transform.localPosition = new Vector3(0f, 0.1f, -4f);

        var exitSpawn = new GameObject("ExitSpawnPoint");
        exitSpawn.transform.SetParent(root.transform);
        exitSpawn.transform.localPosition = new Vector3(0f, 0.1f, 4f);

        // Interior
        var interiorRoot = new GameObject("InteriorRoot");
        interiorRoot.transform.SetParent(root.transform);
        interiorRoot.transform.localPosition = Vector3.zero;
        PlaceInteriorProps(interiorRoot.transform, "Shop");

        var exteriorRoot = new GameObject("ExteriorRoot");
        exteriorRoot.transform.SetParent(root.transform);

        var si = root.AddComponent<StoreInterior>();
        si.exteriorRoot = exteriorRoot;
        si.interiorRoot = interiorRoot;
        si.enterSpawnPoint = enterSpawn.transform;
        si.exitSpawnPoint = exitSpawn.transform;

        var stEntrance = entranceTrigger.AddComponent<StoreTrigger>();
        stEntrance.triggerType = StoreTrigger.TriggerType.Entrance;
        stEntrance.storeInterior = si;

        var stExit = exitTrigger.AddComponent<StoreTrigger>();
        stExit.triggerType = StoreTrigger.TriggerType.Exit;
        stExit.storeInterior = si;

        AddInteractionHint(root.transform, new Vector3(0f, 3f, -6f), "Shop", si);

        ScatterDebrisAround(root.transform.position, 10f, 16f, 6);
    }

    // ─────────────────────────────────────────────────────────────────────
    static void PlaceInteriorProps(Transform parent, string buildingType)
    {
        // Cardboard boxes / shelves as loot containers
        string[] boxPaths = new string[]
        {
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_02.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Pallet_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Skip_01.prefab",
        };

        Vector3[] positions = buildingType == "Warehouse"
            ? new Vector3[] { new Vector3(-2f, 0f, -2f), new Vector3(2f, 0f, -2f), new Vector3(0f, 0f, 2f), new Vector3(-3f, 0f, 1f) }
            : new Vector3[] { new Vector3(-1.5f, 0f, -1f), new Vector3(1.5f, 0f, -1f), new Vector3(0f, 0f, 1.5f) };

        for (int i = 0; i < positions.Length; i++)
        {
            string path = boxPaths[i % boxPaths.Length];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                go.transform.localPosition = positions[i];
                go.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                Undo.RegisterCreatedObjectUndo(go, "Place Interior Prop");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void AddInteractionHint(Transform parent, Vector3 localPos, string label, StoreInterior si)
    {
        var hint = new GameObject($"EnterHint_{label}");
        hint.transform.SetParent(parent);
        hint.transform.localPosition = localPos;
        // The StoreInterior will toggle this
        si.enterHint = hint;
        hint.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────
    static void ScatterJunkyardProps()
    {
        var junkyardParent = new GameObject("Junkyard_Props");
        Undo.RegisterCreatedObjectUndo(junkyardParent, "Create Junkyard Props");

        // ── Rusty Cars ────────────────────────────────────────────────────
        string[] carPaths = new string[]
        {
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_ivy_05.prefab",  // rusty car with ivy
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_ivy_06.prefab",
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_ivy_07.prefab",
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_ivy_08.prefab",
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_ivy_09.prefab",
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_asset_03.prefab",
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_asset_04.prefab",
        };

        Vector3[] carPositions = new Vector3[]
        {
            new Vector3(-28f, 0f, 5f),
            new Vector3(-22f, 0f, -8f),
            new Vector3(5f, 0f, 25f),
            new Vector3(28f, 0f, 8f),
            new Vector3(15f, 0f, -25f),
            new Vector3(-10f, 0f, -28f),
            new Vector3(30f, 0f, -18f),
        };

        float[] carRotations = new float[] { 15f, -30f, 45f, 0f, -15f, 60f, -45f };

        for (int i = 0; i < carPositions.Length; i++)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(carPaths[i % carPaths.Length]);
            if (prefab != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, junkyardParent.transform);
                go.transform.position = carPositions[i];
                go.transform.rotation = Quaternion.Euler(0, carRotations[i], 0);
                Undo.RegisterCreatedObjectUndo(go, "Place Car");
            }
        }

        // ── Shipping Containers ───────────────────────────────────────────
        string[] containerPaths = new string[]
        {
            "Assets/Sayantan/Props/Large/Container_FREE/Prefabs/Clean/PfCont40FBlue.prefab",
            "Assets/Sayantan/Props/Large/Container_FREE/Prefabs/Clean/PfCont20FBeige.prefab",
            "Assets/Sayantan/Props/Large/Container_FREE/Prefabs/Clean/PfCont40FBeige.prefab",
            "Assets/Sayantan/Props/Large/Container_FREE/Prefabs/Clean/PfCont20FBlue.prefab",
        };

        Vector3[] containerPositions = new Vector3[]
        {
            new Vector3(-30f, 0f, -15f),
            new Vector3(-24f, 0f, -15f),
            new Vector3(-30f, 0f, -5f),
            new Vector3(25f, 0f, 20f),
        };

        float[] containerRots = new float[] { 0f, 0f, 90f, 0f };

        for (int i = 0; i < containerPositions.Length; i++)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(containerPaths[i % containerPaths.Length]);
            if (prefab != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, junkyardParent.transform);
                go.transform.position = containerPositions[i];
                go.transform.rotation = Quaternion.Euler(0, containerRots[i], 0);
                Undo.RegisterCreatedObjectUndo(go, "Place Container");
            }
        }

        // ── Scrapyard 3D Model ────────────────────────────────────────────
        var scrapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TripoModels/post-apocalyptic_scrapyard_3d_model/1stMap.fbx");
        if (scrapPrefab != null)
        {
            var scrap = (GameObject)PrefabUtility.InstantiatePrefab(scrapPrefab, junkyardParent.transform);
            scrap.transform.position = new Vector3(0f, 0f, 0f);
            scrap.transform.localScale = Vector3.one * 0.5f;
            Undo.RegisterCreatedObjectUndo(scrap, "Place Scrapyard Model");
        }

        // ── Trash / Debris ────────────────────────────────────────────────
        string[] trashPaths = new string[]
        {
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_02.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_03.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Trashbin_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_03.prefab",
            "Assets/POLYGON city pack/Prefabs/Props/trashBag prefab.prefab",
            "Assets/POLYGON city pack/Prefabs/Props/trashcan prefab.prefab",
            "Assets/POLYGON city pack/Prefabs/Props/Big_trash_bin prefab.prefab",
        };

        Vector3[] trashPositions = new Vector3[]
        {
            new Vector3(-5f, 0f, 18f),
            new Vector3(-8f, 0f, 20f),
            new Vector3(8f, 0f, 15f),
            new Vector3(12f, 0f, 18f),
            new Vector3(-15f, 0f, 5f),
            new Vector3(-12f, 0f, -5f),
            new Vector3(5f, 0f, -18f),
            new Vector3(10f, 0f, -20f),
            new Vector3(-20f, 0f, 20f),
        };

        for (int i = 0; i < trashPositions.Length; i++)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(trashPaths[i % trashPaths.Length]);
            if (prefab != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, junkyardParent.transform);
                go.transform.position = trashPositions[i];
                go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                Undo.RegisterCreatedObjectUndo(go, "Place Trash");
            }
        }

        // ── Rocks / Rubble ────────────────────────────────────────────────
        string[] rockPaths = new string[]
        {
            "Assets/Flooded_Grounds/Prefabs/Nature/Rocks/Rock_A.prefab",
            "Assets/Flooded_Grounds/Prefabs/Nature/Rocks/Rock_B.prefab",
            "Assets/Flooded_Grounds/Prefabs/Nature/Rocks/CobbleRock_A.prefab",
            "Assets/Flooded_Grounds/Prefabs/Nature/Rocks/CobbleRock_B.prefab",
            "Assets/POLYGON city pack/Prefabs/Props/Stone 1 Prefab.prefab",
            "Assets/POLYGON city pack/Prefabs/Props/Stone 2 Prefab.prefab",
        };

        Vector3[] rockPositions = new Vector3[]
        {
            new Vector3(-25f, 0f, 25f),
            new Vector3(22f, 0f, -28f),
            new Vector3(-32f, 0f, 10f),
            new Vector3(32f, 0f, -5f),
            new Vector3(0f, 0f, -32f),
            new Vector3(-18f, 0f, -22f),
        };

        for (int i = 0; i < rockPositions.Length; i++)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(rockPaths[i % rockPaths.Length]);
            if (prefab != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, junkyardParent.transform);
                go.transform.position = rockPositions[i];
                go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                float s = Random.Range(0.8f, 2.0f);
                go.transform.localScale = Vector3.one * s;
                Undo.RegisterCreatedObjectUndo(go, "Place Rock");
            }
        }

        // ── Power poles / pipes ───────────────────────────────────────────
        var polePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/POLYGON city pack/Prefabs/Props/Power_poles prefab.prefab");
        if (polePrefab != null)
        {
            Vector3[] polePositions = new Vector3[]
            {
                new Vector3(-20f, 0f, 0f),
                new Vector3(-10f, 0f, 0f),
                new Vector3(0f, 0f, 0f),
                new Vector3(10f, 0f, 0f),
            };
            foreach (var pos in polePositions)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(polePrefab, junkyardParent.transform);
                go.transform.position = pos;
                Undo.RegisterCreatedObjectUndo(go, "Place Pole");
            }
        }

        // ── Background buildings (distant, for atmosphere) ────────────────
        PlaceBackgroundBuildings(junkyardParent.transform);
    }

    // ─────────────────────────────────────────────────────────────────────
    static void PlaceBackgroundBuildings(Transform parent)
    {
        // Distant ruined buildings on the edges for atmosphere
        string[] bgBldPaths = new string[]
        {
            "Assets/POLYGON city pack/Prefabs/Buildings/Building_E_prefab.prefab",
            "Assets/POLYGON city pack/Prefabs/Buildings/Building_F_prefab.prefab",
            "Assets/POLYGON city pack/Prefabs/Buildings/Building_M_prefab.prefab",
            "Assets/POLYGON city pack/Prefabs/Buildings/Building_T_prefab.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_02.prefab",
        };

        Vector3[] bgPositions = new Vector3[]
        {
            new Vector3(-35f, 0f, -35f),
            new Vector3(35f, 0f, -35f),
            new Vector3(-35f, 0f, 35f),
            new Vector3(35f, 0f, 35f),
            new Vector3(0f, 0f, -38f),
            new Vector3(0f, 0f, 38f),
        };

        float[] bgRots = new float[] { 45f, -45f, 135f, -135f, 0f, 180f };

        for (int i = 0; i < bgPositions.Length; i++)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(bgBldPaths[i % bgBldPaths.Length]);
            if (prefab != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                go.transform.position = bgPositions[i];
                go.transform.rotation = Quaternion.Euler(0, bgRots[i], 0);
                Undo.RegisterCreatedObjectUndo(go, "Place BG Building");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void ScatterDebrisAround(Vector3 center, float minRadius, float maxRadius, int count)
    {
        var debrisParent = new GameObject("Debris_Scatter");
        Undo.RegisterCreatedObjectUndo(debrisParent, "Create Debris");

        string[] debrisPaths = new string[]
        {
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_01.prefab",
            "Assets/POLYGON city pack/Prefabs/Props/trashBag prefab.prefab",
            "Assets/Flooded_Grounds/Prefabs/Nature/Rocks/CobbleRock_A.prefab",
        };

        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(minRadius, maxRadius);
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(debrisPaths[i % debrisPaths.Length]);
            if (prefab != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, debrisParent.transform);
                go.transform.position = pos;
                go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                Undo.RegisterCreatedObjectUndo(go, "Place Debris");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void AddPostProcessVolume()
    {
        var volGO = new GameObject("PostProcess_Volume");
        Undo.RegisterCreatedObjectUndo(volGO, "Create PostProcess Volume");

        var vol = volGO.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 1f;

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();

        // Color Adjustments – desaturated, orange tint
        var ca = profile.Add<ColorAdjustments>(false);
        ca.postExposure.Override(0.2f);
        ca.contrast.Override(25f);
        ca.colorFilter.Override(new Color(1f, 0.82f, 0.55f));
        ca.saturation.Override(-20f);

        // Vignette
        var vig = profile.Add<Vignette>(false);
        vig.intensity.Override(0.35f);
        vig.smoothness.Override(0.5f);
        vig.color.Override(new Color(0.2f, 0.1f, 0.05f));

        // Bloom – subtle glow
        var bloom = profile.Add<Bloom>(false);
        bloom.threshold.Override(0.9f);
        bloom.intensity.Override(0.4f);

        // Save profile as asset
        string profilePath = "Assets/Settings/JunkyardPostProcess.asset";
        AssetDatabase.CreateAsset(profile, profilePath);
        AssetDatabase.SaveAssets();

        vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
    }

    // ─────────────────────────────────────────────────────────────────────
    static void PlaceProp(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null) return;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.transform.position = position;
        go.transform.rotation = rotation;
        Undo.RegisterCreatedObjectUndo(go, "Place Prop");
    }
}
