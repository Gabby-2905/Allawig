using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

/// <summary>
/// Completely rebuilds Level2Scene as a post-apocalyptic junkyard map.
/// Uses only assets confirmed to have valid materials.
/// </summary>
public class RebuildJunkyardClean : EditorWindow
{
    [MenuItem("Tools/Rebuild Junkyard Clean")]
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

        // ── 2. Lighting & atmosphere ──────────────────────────────────────
        SetupLighting();
        SetupDirectionalLight();

        // ── 3. Ground ─────────────────────────────────────────────────────
        BuildGround();

        // ── 4. Boundary fences ────────────────────────────────────────────
        BuildFences();

        // ── 5. Lootable Building 1 – Abandoned Warehouse ──────────────────
        BuildWarehouse();

        // ── 6. Lootable Building 2 – Ruined Shop ──────────────────────────
        BuildShop();

        // ── 7. Junkyard scatter ───────────────────────────────────────────
        ScatterProps();

        // ── 8. Post-process ───────────────────────────────────────────────
        AddPostProcess();

        // ── 9. Frame scene view ───────────────────────────────────────────
        FrameScene();

        // ── 10. Save ──────────────────────────────────────────────────────
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("[RebuildJunkyardClean] Done!");
    }

    // ─────────────────────────────────────────────────────────────────────
    static void SetupLighting()
    {
        var skyboxMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/DaySkybox.mat");
        if (skyboxMat != null) RenderSettings.skybox = skyboxMat;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.58f, 0.40f, 0.22f);
        RenderSettings.ambientEquatorColor = new Color(0.38f, 0.30f, 0.18f);
        RenderSettings.ambientGroundColor  = new Color(0.14f, 0.11f, 0.08f);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.50f, 0.36f, 0.20f, 1f);
        RenderSettings.fogStartDistance = 35f;
        RenderSettings.fogEndDistance   = 130f;
    }

    static void SetupDirectionalLight()
    {
        var go = new GameObject("Directional Light");
        Undo.RegisterCreatedObjectUndo(go, "Dir Light");
        var l = go.AddComponent<Light>();
        l.type = LightType.Directional;
        l.color = new Color(1f, 0.75f, 0.40f);
        l.intensity = 1.15f;
        l.shadows = LightShadows.Soft;
        go.transform.rotation = Quaternion.Euler(44f, -28f, 0f);
    }

    // ─────────────────────────────────────────────────────────────────────
    static void BuildGround()
    {
        // Main ground – cracked asphalt
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "JunkyardGround";
        Undo.RegisterCreatedObjectUndo(ground, "Ground");
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(7f, 1f, 7f); // 70x70 units

        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/CrackedAsphalt.mat");
        if (mat != null) ground.GetComponent<Renderer>().sharedMaterial = mat;

        // Dirt patch overlay
        var dirt = GameObject.CreatePrimitive(PrimitiveType.Plane);
        dirt.name = "DirtPatch";
        Undo.RegisterCreatedObjectUndo(dirt, "Dirt");
        dirt.transform.position = new Vector3(8f, 0.01f, 6f);
        dirt.transform.localScale = new Vector3(2.5f, 1f, 2.5f);
        var dirtMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/MuddyGround.mat");
        if (dirtMat != null) dirt.GetComponent<Renderer>().sharedMaterial = dirtMat;
    }

    // ─────────────────────────────────────────────────────────────────────
    static void BuildFences()
    {
        var parent = new GameObject("Boundary_Fences");
        Undo.RegisterCreatedObjectUndo(parent, "Fences");

        // Use SyntyStudios fence – confirmed good materials
        var fencePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Fence_01.prefab");
        var fenceEndPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Fence_End_01.prefab");

        if (fencePrefab == null) return;

        float half = 34f;
        float step = 4f;

        // North / South rows
        for (float x = -half; x <= half; x += step)
        {
            Place(fencePrefab, new Vector3(x, 0f,  half), Quaternion.identity, parent.transform);
            Place(fencePrefab, new Vector3(x, 0f, -half), Quaternion.identity, parent.transform);
        }
        // East / West columns
        for (float z = -half + step; z < half; z += step)
        {
            Place(fencePrefab, new Vector3( half, 0f, z), Quaternion.Euler(0, 90, 0), parent.transform);
            Place(fencePrefab, new Vector3(-half, 0f, z), Quaternion.Euler(0, 90, 0), parent.transform);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void BuildWarehouse()
    {
        // Building 1: SM_Bld_Station_01 – industrial warehouse look, good materials
        var root = new GameObject("LootableBuilding_Warehouse");
        Undo.RegisterCreatedObjectUndo(root, "Warehouse");
        root.transform.position = new Vector3(-16f, 0f, 10f);

        var bldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Station_01.prefab");
        if (bldPrefab == null)
            bldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_OfficeOld_Large_01.prefab");

        if (bldPrefab != null)
        {
            var bld = (GameObject)PrefabUtility.InstantiatePrefab(bldPrefab, root.transform);
            bld.transform.localPosition = Vector3.zero;
            bld.transform.localRotation = Quaternion.Euler(0, 180, 0);
            Undo.RegisterCreatedObjectUndo(bld, "Warehouse Bld");
        }

        SetupLootBuilding(root, "Warehouse", new Vector3(0f, 1f, -8f), new Vector3(0f, 1f, 8f));
    }

    static void BuildShop()
    {
        // Building 2: SM_Bld_Shop_03 – small shop, good materials
        var root = new GameObject("LootableBuilding_Shop");
        Undo.RegisterCreatedObjectUndo(root, "Shop");
        root.transform.position = new Vector3(16f, 0f, -10f);

        var bldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_03.prefab");
        if (bldPrefab == null)
            bldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_04.prefab");

        if (bldPrefab != null)
        {
            var bld = (GameObject)PrefabUtility.InstantiatePrefab(bldPrefab, root.transform);
            bld.transform.localPosition = Vector3.zero;
            bld.transform.localRotation = Quaternion.identity;
            Undo.RegisterCreatedObjectUndo(bld, "Shop Bld");
        }

        SetupLootBuilding(root, "Shop", new Vector3(0f, 1f, -5f), new Vector3(0f, 1f, 5f));
    }

    static void SetupLootBuilding(GameObject root, string label,
        Vector3 entranceLocalPos, Vector3 exitLocalPos)
    {
        // Entrance trigger
        var eTrig = new GameObject("EntranceTrigger");
        eTrig.transform.SetParent(root.transform);
        eTrig.transform.localPosition = entranceLocalPos;
        var ec = eTrig.AddComponent<BoxCollider>();
        ec.isTrigger = true; ec.size = new Vector3(4f, 2.5f, 2f);

        // Exit trigger
        var xTrig = new GameObject("ExitTrigger");
        xTrig.transform.SetParent(root.transform);
        xTrig.transform.localPosition = exitLocalPos;
        var xc = xTrig.AddComponent<BoxCollider>();
        xc.isTrigger = true; xc.size = new Vector3(4f, 2.5f, 2f);

        // Spawn points
        var eSpawn = new GameObject("EnterSpawnPoint");
        eSpawn.transform.SetParent(root.transform);
        eSpawn.transform.localPosition = entranceLocalPos + new Vector3(0, -0.9f, 1.5f);

        var xSpawn = new GameObject("ExitSpawnPoint");
        xSpawn.transform.SetParent(root.transform);
        xSpawn.transform.localPosition = exitLocalPos + new Vector3(0, -0.9f, -1.5f);

        // Interior loot props
        var interior = new GameObject("InteriorRoot");
        interior.transform.SetParent(root.transform);
        interior.transform.localPosition = Vector3.zero;
        PlaceInteriorLoot(interior.transform);

        var exterior = new GameObject("ExteriorRoot");
        exterior.transform.SetParent(root.transform);

        // Hint objects
        var enterHint = new GameObject($"EnterHint_{label}");
        enterHint.transform.SetParent(root.transform);
        enterHint.transform.localPosition = entranceLocalPos + new Vector3(0, 1.5f, 0);
        enterHint.SetActive(false);

        var exitHint = new GameObject($"ExitHint_{label}");
        exitHint.transform.SetParent(root.transform);
        exitHint.transform.localPosition = exitLocalPos + new Vector3(0, 1.5f, 0);
        exitHint.SetActive(false);

        // StoreInterior component
        var si = root.AddComponent<StoreInterior>();
        si.exteriorRoot = exterior;
        si.interiorRoot = interior;
        si.enterSpawnPoint = eSpawn.transform;
        si.exitSpawnPoint  = xSpawn.transform;
        si.enterHint = enterHint;
        si.exitHint  = exitHint;

        // StoreTrigger components
        var stE = eTrig.AddComponent<StoreTrigger>();
        stE.triggerType = StoreTrigger.TriggerType.Entrance;
        stE.storeInterior = si;

        var stX = xTrig.AddComponent<StoreTrigger>();
        stX.triggerType = StoreTrigger.TriggerType.Exit;
        stX.storeInterior = si;

        // Scatter debris around building
        ScatterDebrisAround(root.transform.position, 8f, 14f, 6);
    }

    static void PlaceInteriorLoot(Transform parent)
    {
        string[] paths = {
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_02.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Pallet_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Skip_01.prefab",
        };
        Vector3[] positions = {
            new Vector3(-1.5f, 0f, -1f),
            new Vector3( 1.5f, 0f, -1f),
            new Vector3( 0f,   0f,  1.5f),
            new Vector3(-2f,   0f,  0.5f),
        };
        for (int i = 0; i < positions.Length; i++)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i % paths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent);
            go.transform.localPosition = positions[i];
            go.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Undo.RegisterCreatedObjectUndo(go, "Interior Loot");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void ScatterProps()
    {
        var parent = new GameObject("Junkyard_Props");
        Undo.RegisterCreatedObjectUndo(parent, "Junkyard Props");

        // ── Rusty cars (OlegWER – confirmed good materials) ───────────────
        string[] carPaths = {
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_ivy_05.prefab",
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_ivy_06.prefab",
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_ivy_07.prefab",
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_ivy_08.prefab",
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_ivy_09.prefab",
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_asset_03.prefab",
            "Assets/OlegWER/RustyCarsFree/Prefabs/SM_asset_04.prefab",
        };
        (Vector3 pos, float rot)[] cars = {
            (new Vector3(-26f, 0f,  4f),  15f),
            (new Vector3(-20f, 0f, -8f), -30f),
            (new Vector3(  4f, 0f, 24f),  45f),
            (new Vector3( 26f, 0f,  6f),   0f),
            (new Vector3( 14f, 0f,-24f), -15f),
            (new Vector3(-10f, 0f,-26f),  60f),
            (new Vector3( 28f, 0f,-16f), -45f),
        };
        for (int i = 0; i < cars.Length; i++)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(carPaths[i % carPaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = cars[i].pos;
            go.transform.rotation = Quaternion.Euler(0, cars[i].rot, 0);
            Undo.RegisterCreatedObjectUndo(go, "Car");
        }

        // ── Shipping containers (Sayantan – confirmed good) ───────────────
        string[] contPaths = {
            "Assets/Sayantan/Props/Large/Container_FREE/Prefabs/Clean/PfCont40FBlue.prefab",
            "Assets/Sayantan/Props/Large/Container_FREE/Prefabs/Clean/PfCont20FBeige.prefab",
            "Assets/Sayantan/Props/Large/Container_FREE/Prefabs/Clean/PfCont40FBeige.prefab",
            "Assets/Sayantan/Props/Large/Container_FREE/Prefabs/Clean/PfCont20FBlue.prefab",
        };
        (Vector3 pos, float rot)[] conts = {
            (new Vector3(-28f, 0f,-14f),   0f),
            (new Vector3(-22f, 0f,-14f),   0f),
            (new Vector3(-28f, 0f, -4f),  90f),
            (new Vector3( 24f, 0f, 20f),   0f),
        };
        for (int i = 0; i < conts.Length; i++)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(contPaths[i % contPaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = conts[i].pos;
            go.transform.rotation = Quaternion.Euler(0, conts[i].rot, 0);
            Undo.RegisterCreatedObjectUndo(go, "Container");
        }

        // ── Trash bags & bins (SyntyStudios – confirmed good) ─────────────
        string[] trashPaths = {
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_02.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_03.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Trashbin_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_03.prefab",
        };
        Vector3[] trashPos = {
            new Vector3( -5f, 0f, 18f),
            new Vector3( -8f, 0f, 20f),
            new Vector3(  8f, 0f, 15f),
            new Vector3( 12f, 0f, 18f),
            new Vector3(-14f, 0f,  4f),
            new Vector3(-11f, 0f, -4f),
            new Vector3(  4f, 0f,-18f),
            new Vector3( 10f, 0f,-20f),
            new Vector3(-20f, 0f, 20f),
        };
        for (int i = 0; i < trashPos.Length; i++)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(trashPaths[i % trashPaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = trashPos[i];
            go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Undo.RegisterCreatedObjectUndo(go, "Trash");
        }

        // ── Rocks (Flooded_Grounds – confirmed good) ──────────────────────
        string[] rockPaths = {
            "Assets/Flooded_Grounds/Prefabs/Nature/Rocks/Rock_A.prefab",
            "Assets/Flooded_Grounds/Prefabs/Nature/Rocks/Rock_B.prefab",
            "Assets/Flooded_Grounds/Prefabs/Nature/Rocks/CobbleRock_A.prefab",
            "Assets/Flooded_Grounds/Prefabs/Nature/Rocks/CobbleRock_B.prefab",
        };
        (Vector3 pos, float scale)[] rocks = {
            (new Vector3(-24f, 0f, 24f), 1.4f),
            (new Vector3( 22f, 0f,-28f), 1.8f),
            (new Vector3(-30f, 0f,  8f), 1.2f),
            (new Vector3( 30f, 0f, -4f), 1.6f),
            (new Vector3(  0f, 0f,-30f), 1.0f),
            (new Vector3(-18f, 0f,-20f), 1.3f),
        };
        for (int i = 0; i < rocks.Length; i++)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(rockPaths[i % rockPaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = rocks[i].pos;
            go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            go.transform.localScale = Vector3.one * rocks[i].scale;
            Undo.RegisterCreatedObjectUndo(go, "Rock");
        }

        // ── Background ruined buildings (SyntyStudios – confirmed good) ───
        string[] bgPaths = {
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Apartment_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Apartment_02.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_OfficeOld_Small_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_OfficeOld_Small_02.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Apartment_03.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Station_02.prefab",
        };
        (Vector3 pos, float rot)[] bgBlds = {
            (new Vector3(-32f, 0f,-32f),  45f),
            (new Vector3( 32f, 0f,-32f), -45f),
            (new Vector3(-32f, 0f, 32f), 135f),
            (new Vector3( 32f, 0f, 32f),-135f),
            (new Vector3(  0f, 0f,-33f),   0f),
            (new Vector3(  0f, 0f, 33f), 180f),
        };
        for (int i = 0; i < bgBlds.Length; i++)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(bgPaths[i % bgPaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = bgBlds[i].pos;
            go.transform.rotation = Quaternion.Euler(0, bgBlds[i].rot, 0);
            Undo.RegisterCreatedObjectUndo(go, "BG Building");
        }

        // ── Skips / dumpsters ─────────────────────────────────────────────
        var skipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Skip_01.prefab");
        var skip2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Skip_02.prefab");
        Vector3[] skipPos = {
            new Vector3( 2f, 0f, 10f),
            new Vector3(-6f, 0f,-12f),
            new Vector3(18f, 0f,  4f),
        };
        for (int i = 0; i < skipPos.Length; i++)
        {
            var p = (i % 2 == 0) ? skipPrefab : skip2Prefab;
            if (p == null) p = skipPrefab;
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = skipPos[i];
            go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Undo.RegisterCreatedObjectUndo(go, "Skip");
        }

        // ── Pallets ───────────────────────────────────────────────────────
        var palletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Pallet_01.prefab");
        if (palletPrefab != null)
        {
            Vector3[] palletPos = {
                new Vector3(-18f, 0f, 14f),
                new Vector3(-16f, 0f, 14f),
                new Vector3( 20f, 0f,-14f),
            };
            foreach (var pos in palletPos)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(palletPrefab, parent.transform);
                go.transform.position = pos;
                go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                Undo.RegisterCreatedObjectUndo(go, "Pallet");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void ScatterDebrisAround(Vector3 center, float minR, float maxR, int count)
    {
        var parent = new GameObject("Debris_Scatter");
        Undo.RegisterCreatedObjectUndo(parent, "Debris");

        string[] paths = {
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_01.prefab",
            "Assets/Flooded_Grounds/Prefabs/Nature/Rocks/CobbleRock_A.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_02.prefab",
        };
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float r = Random.Range(minR, maxR);
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i % paths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Undo.RegisterCreatedObjectUndo(go, "Debris");
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

        var ca = profile.Add<ColorAdjustments>(false);
        ca.postExposure.Override(0.15f);
        ca.contrast.Override(22f);
        ca.colorFilter.Override(new Color(1f, 0.84f, 0.58f));
        ca.saturation.Override(-18f);

        var vig = profile.Add<Vignette>(false);
        vig.intensity.Override(0.32f);
        vig.smoothness.Override(0.5f);
        vig.color.Override(new Color(0.18f, 0.09f, 0.04f));

        var bloom = profile.Add<Bloom>(false);
        bloom.threshold.Override(0.95f);
        bloom.intensity.Override(0.35f);

        string path = "Assets/Settings/JunkyardPostProcess.asset";
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

        sv.pivot = new Vector3(0f, 2f, 0f);
        sv.rotation = Quaternion.Euler(38f, -42f, 0f);
        sv.size = 55f;
        sv.orthographic = false;
        sv.Repaint();
    }

    // ─────────────────────────────────────────────────────────────────────
    static void Place(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)
    {
        if (prefab == null) return;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.transform.position = pos;
        go.transform.rotation = rot;
        Undo.RegisterCreatedObjectUndo(go, "Place");
    }
}
