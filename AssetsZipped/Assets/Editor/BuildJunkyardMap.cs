using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Sector 9 - The Rusted Junkyard
/// Reference layout (top-down, Z=north):
///   NW (X=-10..-35, Z=10..35):  Scrap Processing Plant — factory building, chimneys, scrap piles
///   NE (X=10..35,  Z=10..35):  Vehicle Graveyard — dense car cluster + scrap mounds
///   Center (X=-10..10, Z=-10..10): Open dirt paths, scattered debris
///   SW (X=-10..-35, Z=-10..-35): Junk Market — stalls, huts, shacks
///   SE (X=10..35,  Z=-10..-35): Fortified Safehouse — container wall compound
/// </summary>
public class BuildJunkyardMap : MonoBehaviour
{
    [MenuItem("Tools/Build Junkyard Map")]
    public static void Execute()
    {
        ClearOldMap();

        const float Y    = 0f;
        const float HALF = 40f;

        BuildGround(Y, HALF);
        BuildPerimeterWalls(Y, HALF);
        BuildScrapProcessingPlant(Y);   // NW
        BuildVehicleGraveyard(Y);       // NE + center
        BuildJunkMarket(Y);             // SW
        BuildFortifiedSafehouse(Y);     // SE
        ScatterDebris(Y);
        SetAtmosphere();
        RepositionPlayer(Y);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[JunkyardMap] Done!");
    }

    // ── Clear ─────────────────────────────────────────────────────────────────
    static void ClearOldMap()
    {
        string[] names = { "Ground","Walls","Fence","LootableStore","Cars","StreetProps",
                           "ApocProps","ScrapPlant","VehicleGraveyard","JunkMarket",
                           "Safehouse","JunkDebris","Rain (1)" };
        foreach (var n in names)
        {
            var go = GameObject.Find(n);
            if (go != null) Undo.DestroyObjectImmediate(go);
        }
    }

    // ── Ground ────────────────────────────────────────────────────────────────
    static void BuildGround(float y, float half)
    {
        var root = new GameObject("Ground");
        Undo.RegisterCreatedObjectUndo(root, "Ground");

        var grass = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Grass_01.prefab");
        var bare  = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Road_Bare_01.prefab");

        for (int xi = 0; xi < 14; xi++)
        {
            float x = -32.5f + xi * 5f;
            for (int zi = 0; zi < 14; zi++)
            {
                float z = -32.5f + zi * 5f;
                // Dirt paths: cross through center + diagonal paths
                bool isDirt = (Mathf.Abs(x) < 10f) || (Mathf.Abs(z) < 10f) ||
                              (Mathf.Abs(x - z) < 8f) || (Mathf.Abs(x + z) < 8f);
                var prefab = isDirt ? bare : grass;
                if (prefab == null) continue;
                var tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
                tile.transform.position = new Vector3(x, y, z);
                tile.name = $"G_{xi}_{zi}";
            }
        }
    }

    // ── Perimeter walls ───────────────────────────────────────────────────────
    static void BuildPerimeterWalls(float y, float half)
    {
        var root = new GameObject("Walls");
        Undo.RegisterCreatedObjectUndo(root, "Walls");

        // Invisible collision walls
        float h = 6f, t = 1f, cy = y + h / 2f;
        MakeInvCube(root, new Vector3(0,     cy,  half), new Vector3(half*2, h, t), "Col_N");
        MakeInvCube(root, new Vector3(0,     cy, -half), new Vector3(half*2, h, t), "Col_S");
        MakeInvCube(root, new Vector3( half, cy,  0),    new Vector3(t, h, half*2), "Col_E");
        MakeInvCube(root, new Vector3(-half, cy,  0),    new Vector3(t, h, half*2), "Col_W");

        // Visual apartment facades
        var wall   = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Apartment_01.prefab");
        var corner = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Apartment_Corner_01.prefab");
        var roof   = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Apartment_Roof_01.prefab");
        var cRoof  = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Apartment_Roof_Corner_01.prefab");
        if (wall == null) return;

        const float S = 5f, FH = 3f, WP = 38.5f;
        float[] pos = new float[16];
        for (int i = 0; i < 16; i++) pos[i] = -37.5f + i * S;

        void PW(float wx, float wz, float ry, bool isCorner, string lbl)
        {
            var wp = isCorner ? corner : wall;
            var rp = isCorner ? cRoof  : roof;
            if (wp == null) return;
            var w = (GameObject)PrefabUtility.InstantiatePrefab(wp, root.transform);
            w.transform.SetPositionAndRotation(new Vector3(wx, y, wz), Quaternion.Euler(0, ry, 0));
            w.name = $"W_{lbl}";
            if (rp != null)
            {
                var r = (GameObject)PrefabUtility.InstantiatePrefab(rp, root.transform);
                r.transform.SetPositionAndRotation(new Vector3(wx, y + FH, wz), Quaternion.Euler(0, ry, 0));
                r.name = $"WR_{lbl}";
            }
        }

        // North (Z=+WP): NW corner rot=90, NE corner rot=180, straights rot=180
        for (int i = 0; i < 16; i++)
            PW(pos[i], WP, i==0 ? 90f : 180f, i==0||i==15, $"N{i}");
        // South (Z=-WP): SW corner rot=0, SE corner rot=270, straights rot=0
        for (int i = 0; i < 16; i++)
            PW(pos[i], -WP, i==15 ? 270f : 0f, i==0||i==15, $"S{i}");
        // East (X=+WP): rot=270, no corners
        for (int i = 1; i < 15; i++) PW( WP, pos[i], 270f, false, $"E{i}");
        // West (X=-WP): rot=90, no corners
        for (int i = 1; i < 15; i++) PW(-WP, pos[i],  90f, false, $"W{i}");
    }

    // ── NW: Scrap Processing Plant ────────────────────────────────────────────
    static void BuildScrapProcessingPlant(float y)
    {
        var root = new GameObject("ScrapPlant");
        Undo.RegisterCreatedObjectUndo(root, "ScrapPlant");

        // Main factory: station building
        var factory = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Station_01.prefab");
        if (factory != null) Inst(factory, root, new Vector3(-22f, y, 22f), Qy(0),   "Factory");

        // Secondary shed
        var shed = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Shop_01.prefab");
        if (shed != null)
        {
            Inst(shed, root, new Vector3(-30f, y, 18f), Qy(90),  "Shed_1");
            Inst(shed, root, new Vector3(-16f, y, 28f), Qy(180), "Shed_2");
        }

        // Chimneys (spires)
        var spire = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Spire_01.prefab");
        if (spire != null)
        {
            Inst(spire, root, new Vector3(-26f, y, 26f), Qy(0), "Chimney_1");
            Inst(spire, root, new Vector3(-21f, y, 28f), Qy(0), "Chimney_2");
        }

        // Scrap piles (rocks + barrels)
        var rock1  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_01.prefab");
        var rock2  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_02.prefab");
        var rock3  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_03.prefab");
        var barrel = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Barrel_01.prefab");
        var crate  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Crate_01.prefab");

        // NW scrap mound
        (float x, float z)[] mound = {(-32f,32f),(-30f,30f),(-28f,32f),(-32f,28f),(-30f,34f),(-34f,30f),(-28f,28f)};
        for (int i = 0; i < mound.Length; i++)
        {
            var p = i%3==0 ? rock1 : i%3==1 ? rock2 : rock3;
            Rnd(p, root, mound[i].x, y, mound[i].z, 1.5f, $"NWMound_{i}");
        }

        // Factory yard props
        Rnd(barrel, root, -16f, y, 20f, 1f, "FBarrel1");
        Rnd(barrel, root, -18f, y, 18f, 1f, "FBarrel2");
        Rnd(barrel, root, -14f, y, 22f, 1f, "FBarrel3");
        Rnd(crate,  root, -20f, y, 16f, 1f, "FCrate1");
        Rnd(crate,  root, -18f, y, 14f, 1f, "FCrate2");

        // Abandoned car near factory
        var van = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Van_01.prefab");
        if (van != null) Inst(van, root, new Vector3(-28f, y, 16f), Qy(35f), "FactoryCar");
    }

    // ── NE + Center: Vehicle Graveyard ────────────────────────────────────────
    static void BuildVehicleGraveyard(float y)
    {
        var root = new GameObject("VehicleGraveyard");
        Undo.RegisterCreatedObjectUndo(root, "VehicleGraveyard");

        string[] paths = {
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Sedan_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Small_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Medium_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Muscle_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Van_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Taxi_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Ambo_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Police_01.prefab",
        };

        // Cars spread across NE quadrant (X=8..32, Z=8..35) — vehicle graveyard
        (float x, float z, float r)[] cars = {
            (12f, 28f, 15f),  (18f, 30f, 60f),  (24f, 28f, 120f), (30f, 26f, 200f),
            (10f, 22f, 45f),  (16f, 24f, 90f),  (22f, 22f, 150f), (28f, 24f, 240f),
            (12f, 16f, 30f),  (18f, 18f, 75f),  (24f, 16f, 135f), (30f, 18f, 210f),
            (14f, 12f, 55f),  (20f, 14f, 100f), (26f, 12f, 170f), (32f, 14f, 280f),
            // A few spilling into center
            (8f,  8f,  20f),  (14f, 8f,  80f),  (20f, 8f,  160f), (26f, 8f,  300f),
        };

        for (int i = 0; i < cars.Length; i++)
        {
            var prefab = Load(paths[i % paths.Length]);
            if (prefab == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
            go.transform.position = new Vector3(cars[i].x, y, cars[i].z);
            go.transform.rotation = Qy(cars[i].r);
            go.name = $"Car_{i}";
        }

        // NE scrap mound (rocks)
        var r1 = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_01.prefab");
        var r2 = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_02.prefab");
        (float x, float z)[] nem = {(30f,32f),(32f,30f),(34f,32f),(30f,34f),(32f,34f),(28f,32f),(34f,28f)};
        for (int i = 0; i < nem.Length; i++)
            Rnd(i%2==0?r1:r2, root, nem[i].x, y, nem[i].z, 1.5f, $"NEMound_{i}");
    }

    // ── SW: Junk Market ───────────────────────────────────────────────────────
    static void BuildJunkMarket(float y)
    {
        var root = new GameObject("JunkMarket");
        Undo.RegisterCreatedObjectUndo(root, "JunkMarket");

        var v6  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Buildings/SM_Bld_Village_06.prefab"); // large
        var v1  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Buildings/SM_Bld_Village_01.prefab");
        var v2  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Buildings/SM_Bld_Village_02.prefab");
        var v3  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Buildings/SM_Bld_Village_03.prefab");
        var hut = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Buildings/SM_Bld_Hut_01.prefab");
        var s1  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Buildings/SM_Bld_Stall_01.prefab");
        var s2  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Buildings/SM_Bld_Stall_02.prefab");
        var s3  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Buildings/SM_Bld_Stall_03.prefab");

        // Main market building
        if (v6  != null) Inst(v6,  root, new Vector3(-20f, y, -15f), Qy(0),   "Market_Main");
        if (v1  != null) Inst(v1,  root, new Vector3(-28f, y, -12f), Qy(90),  "Market_Shack1");
        if (v2  != null) Inst(v2,  root, new Vector3(-28f, y, -20f), Qy(90),  "Market_Shack2");
        if (v3  != null) Inst(v3,  root, new Vector3(-14f, y, -22f), Qy(180), "Market_Shack3");
        if (hut != null) Inst(hut, root, new Vector3(-22f, y, -26f), Qy(270), "Market_Hut");

        // Stalls
        if (s1 != null) Inst(s1, root, new Vector3(-14f, y, -12f), Qy(0),   "Stall_1");
        if (s2 != null) Inst(s2, root, new Vector3(-14f, y, -16f), Qy(0),   "Stall_2");
        if (s3 != null) Inst(s3, root, new Vector3(-26f, y, -24f), Qy(180), "Stall_3");
        if (s1 != null) Inst(s1, root, new Vector3(-30f, y, -18f), Qy(90),  "Stall_4");

        // Market props
        var barrel = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Barrel_01.prefab");
        var crate  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Crate_01.prefab");
        var sack   = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Sack_01.prefab");
        var cart   = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Cart_01.prefab");
        var trash  = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_01.prefab");

        Rnd(barrel, root, -16f, y, -10f, 1f, "MB1"); Rnd(barrel, root, -24f, y, -10f, 1f, "MB2");
        Rnd(crate,  root, -18f, y, -24f, 1f, "MC1"); Rnd(crate,  root, -30f, y, -14f, 1f, "MC2");
        Rnd(sack,   root, -15f, y, -14f, 0.5f, "MS1"); Rnd(sack, root, -22f, y, -20f, 0.5f, "MS2");
        if (cart  != null) Inst(cart,  root, new Vector3(-18f, y, -17f), Qy(45),  "MCart");
        if (trash != null) { Rnd(trash, root, -12f, y, -18f, 1f, "MT1"); Rnd(trash, root, -26f, y, -22f, 1f, "MT2"); }

        // SW corner junk cars
        var c1 = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Sedan_01.prefab");
        var c2 = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Van_01.prefab");
        var r1 = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_01.prefab");
        if (c1 != null) Inst(c1, root, new Vector3(-30f, y, -28f), Qy(35),  "SW_Car1");
        if (c2 != null) Inst(c2, root, new Vector3(-34f, y, -22f), Qy(120), "SW_Car2");
        if (c1 != null) Inst(c1, root, new Vector3(-32f, y, -16f), Qy(200), "SW_Car3");
        Rnd(r1, root, -32f, y, -30f, 2f, "SW_Rock1"); Rnd(r1, root, -34f, y, -26f, 2f, "SW_Rock2");
    }

    // ── SE: Fortified Safehouse ───────────────────────────────────────────────
    static void BuildFortifiedSafehouse(float y)
    {
        var root = new GameObject("Safehouse");
        Undo.RegisterCreatedObjectUndo(root, "Safehouse");

        // Container walls: apartment stacks as containers
        var cont  = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Apartment_Stack_01.prefab");
        var wall  = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Apartment_01.prefab");

        // Interior safehouse building
        var bld = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/SM_Bld_Station_03.prefab");
        if (bld != null) Inst(bld, root, new Vector3(22f, y, -20f), Qy(0), "SH_Building");

        // Compound walls (center ~X=22, Z=-20, size ~20x18)
        var wp = cont ?? wall;
        if (wp != null)
        {
            // North wall of compound: Z=-11
            for (int i = 0; i < 4; i++) Inst(wp, root, new Vector3(12f+i*5f, y, -11f), Qy(0),   $"SH_N{i}");
            // South wall: Z=-31
            for (int i = 0; i < 4; i++) Inst(wp, root, new Vector3(12f+i*5f, y, -31f), Qy(180), $"SH_S{i}");
            // East wall: X=32
            for (int i = 0; i < 3; i++) Inst(wp, root, new Vector3(32f, y, -14f-i*5f), Qy(90),  $"SH_E{i}");
            // West wall with entrance gap: X=12, skip middle
            Inst(wp, root, new Vector3(12f, y, -14f), Qy(270), "SH_W0");
            Inst(wp, root, new Vector3(12f, y, -28f), Qy(270), "SH_W2");
        }

        // Gate barriers
        var barrier = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Barrier_01.prefab");
        if (barrier != null)
        {
            Inst(barrier, root, new Vector3(10f, y, -21f), Qy(90), "Gate_L");
            Inst(barrier, root, new Vector3(14f, y, -21f), Qy(90), "Gate_R");
        }

        // Interior props
        var barrel = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Barrel_01.prefab");
        var crate  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Crate_01.prefab");
        Rnd(barrel, root, 18f, y, -16f, 1f, "SH_B1"); Rnd(barrel, root, 28f, y, -28f, 1f, "SH_B2");
        Rnd(crate,  root, 26f, y, -14f, 1f, "SH_C1"); Rnd(crate,  root, 20f, y, -30f, 1f, "SH_C2");

        // SE corner junk
        var r1 = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_01.prefab");
        var r2 = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_02.prefab");
        var car = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Muscle_01.prefab");
        Rnd(r1, root, 30f, y, -34f, 2f, "SE_R1"); Rnd(r2, root, 34f, y, -30f, 2f, "SE_R2");
        if (car != null) Inst(car, root, new Vector3(34f, y, -24f), Qy(70), "SE_Car");
    }

    // ── Scatter debris ────────────────────────────────────────────────────────
    static void ScatterDebris(float y)
    {
        var root = new GameObject("JunkDebris");
        Undo.RegisterCreatedObjectUndo(root, "JunkDebris");

        var trash1 = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_01.prefab");
        var trash2 = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_02.prefab");
        var box    = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_01.prefab");
        var barrel = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Barrel_01.prefab");
        var crate  = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Crate_01.prefab");
        var dt1    = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_TreeDead_01.prefab");
        var dt2    = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_TreeDead_02.prefab");
        var skip   = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Skip_01.prefab");
        var pallet = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Pallet_01.prefab");
        var couch  = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Couch_01.prefab");
        var news   = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Newspaper_01.prefab");
        var bar2   = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Barrier_01.prefab");
        var rock   = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_01.prefab");

        // Dead trees around perimeter and open areas
        (float x, float z)[] trees = {
            (-5f,32f),(5f,34f),(-8f,-6f),(6f,-8f),(0f,-28f),(-6f,-32f),
            (36f,6f),(36f,-6f),(-36f,6f),(-36f,-6f),(2f,36f),(4f,-36f),
            (-10f,10f),(10f,-10f),(-10f,-10f),(10f,10f)
        };
        for (int i = 0; i < trees.Length; i++)
            Rnd(i%2==0?dt1:dt2, root, trees[i].x, y, trees[i].z, 2f, $"DT_{i}");

        // Trash along paths
        (float x, float z)[] trashPts = {
            (-4f,6f),(4f,-4f),(-6f,-8f),(5f,8f),(-3f,16f),(7f,-16f),(-7f,0f),(3f,-22f),(-5f,22f),(8f,3f)
        };
        for (int i = 0; i < trashPts.Length; i++)
            Rnd(i%2==0?trash1:trash2, root, trashPts[i].x, y, trashPts[i].z, 1.5f, $"Trash_{i}");

        // Boxes, barrels, rocks in center
        Rnd(box,    root, -8f, y, 5f,   1f, "Box1"); Rnd(box,    root, 5f, y, -5f,  1f, "Box2");
        Rnd(barrel, root, 0f,  y, 8f,   1f, "Bar1"); Rnd(barrel, root, -8f,y, -15f, 1f, "Bar2");
        Rnd(rock,   root, -10f,y, 5f,   2f, "Rk1");  Rnd(rock,   root, 8f, y, -10f, 2f, "Rk2");
        Rnd(crate,  root, -5f, y, -5f,  1f, "Cr1");  Rnd(crate,  root, 5f, y, 5f,   1f, "Cr2");

        // Skips, pallets, couch
        if (skip   != null) { Inst(skip,   root, new Vector3(-8f, y, 8f),  Qy(30),  "Skip1"); Inst(skip, root, new Vector3(8f, y, -8f), Qy(150), "Skip2"); }
        if (pallet != null) { Inst(pallet, root, new Vector3(-6f, y, -3f), Qy(20),  "Pal1");  Inst(pallet, root, new Vector3(6f, y, 3f), Qy(80), "Pal2"); }
        if (couch  != null)   Inst(couch,  root, new Vector3(-3f, y, -6f), Qy(45),  "Couch");
        if (bar2   != null) { Inst(bar2,   root, new Vector3(-7f, y, 0f),  Qy(90),  "Barr1"); Inst(bar2, root, new Vector3(7f, y, 0f), Qy(270), "Barr2"); }
        if (news   != null)
            for (int i = 0; i < 6; i++)
                Rnd(news, root, Random.Range(-12f,12f), y, Random.Range(-12f,12f), 0.5f, $"News_{i}");
    }

    // ── Atmosphere ────────────────────────────────────────────────────────────
    static void SetAtmosphere()
    {
        var lg = GameObject.Find("Directional Light");
        if (lg != null)
        {
            var l = lg.GetComponent<Light>();
            if (l != null) { l.color = new Color(0.78f,0.82f,0.90f,1f); l.intensity = 1.7f; lg.transform.rotation = Quaternion.Euler(48f,195f,0f); l.shadows = LightShadows.Soft; }
        }
        RenderSettings.ambientMode      = AmbientMode.Flat;
        RenderSettings.ambientLight     = new Color(0.42f,0.45f,0.52f,1f);
        RenderSettings.ambientIntensity = 1.3f;
        RenderSettings.skybox           = null;
        RenderSettings.fog              = true;
        RenderSettings.fogMode          = FogMode.Linear;
        RenderSettings.fogColor         = new Color(0.25f,0.27f,0.32f,1f);
        RenderSettings.fogStartDistance = 70f;
        RenderSettings.fogEndDistance   = 220f;

        var rg = GameObject.Find("Rain");
        if (rg != null)
        {
            var ps = rg.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                rg.transform.position = new Vector3(0f,25f,0f);
                var m = ps.main; m.startSpeed = new ParticleSystem.MinMaxCurve(18f,25f); m.startSize = new ParticleSystem.MinMaxCurve(0.04f,0.08f); m.startLifetime = new ParticleSystem.MinMaxCurve(1.2f,1.8f); m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.5f,0.6f,0.75f,0.6f),new Color(0.4f,0.5f,0.65f,0.4f)); m.maxParticles = 6000;
                var e = ps.emission; e.rateOverTime = 500f;
                var s = ps.shape; s.shapeType = ParticleSystemShapeType.Box; s.scale = new Vector3(90f,1f,90f);
            }
        }
    }

    static void RepositionPlayer(float y)
    {
        var p = GameObject.Find("Player");
        if (p != null) p.transform.position = new Vector3(0f, y+0.1f, 0f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    static GameObject Load(string path) => AssetDatabase.LoadAssetAtPath<GameObject>(path);
    static Quaternion Qy(float deg) => Quaternion.Euler(0, deg, 0);

    static void Inst(GameObject p, GameObject parent, Vector3 pos, Quaternion rot, string name)
    {
        if (p == null) return;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
        go.transform.SetPositionAndRotation(pos, rot);
        go.name = name;
    }

    static void Rnd(GameObject p, GameObject parent, float x, float y, float z, float spread, string name)
    {
        if (p == null) return;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent.transform);
        go.transform.position = new Vector3(x+Random.Range(-spread,spread), y, z+Random.Range(-spread,spread));
        go.transform.rotation = Qy(Random.Range(0f,360f));
        go.name = name;
    }

    static void MakeInvCube(GameObject parent, Vector3 pos, Vector3 scale, string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos; go.transform.localScale = scale;
        var mr = go.GetComponent<MeshRenderer>(); if (mr) mr.enabled = false;
        go.name = name;
    }
}
