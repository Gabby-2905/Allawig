using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Polishes the Level2Scene urban map:
/// - Fixes all overlapping objects with precise positions
/// - Spreads buildings, vehicles, trees, props properly
/// - Adds LootContainer components to crates/boxes
/// - Adds world-space loot hint labels
/// - Places extra loot crates around the map
/// </summary>
public class PolishUrbanMap : EditorWindow
{
    const string SYNTY_BLD  = "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/";
    const string SYNTY_ENV  = "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/";
    const string SYNTY_PROP = "Assets/SyntyStudios/PolygonCity/Prefabs/Props/";
    const string SYNTY_VEH  = "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/";
    const string CONT_PATH  = "Assets/Sayantan/Props/Large/Container_FREE/Prefabs/Clean/";

    [MenuItem("Tools/Polish Urban Map")]
    public static void Polish()
    {
        // ── 1. Fix building positions ─────────────────────────────────────
        FixBuildingPositions();

        // ── 2. Fix vehicle positions ──────────────────────────────────────
        FixVehiclePositions();

        // ── 3. Fix tree positions ─────────────────────────────────────────
        FixTreePositions();

        // ── 4. Fix prop positions ─────────────────────────────────────────
        FixPropPositions();

        // ── 5. Fix market stall positions ────────────────────────────────
        FixMarketStalls();

        // ── 6. Add LootContainer components to existing boxes ─────────────
        AddLootContainersToExisting();

        // ── 7. Place dedicated loot crates around the map ─────────────────
        PlaceLootCrates();

        // ── 8. Save ───────────────────────────────────────────────────────
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("[PolishUrbanMap] Done!");
    }

    // ─────────────────────────────────────────────────────────────────────
    static void FixBuildingPositions()
    {
        // City Hall – centered, facing south
        SetTransform("CityHall_Central", new Vector3(0f, 0f, 12f), Quaternion.Euler(0, 180, 0));

        // Left wing office – clear of city hall
        var leftWing = FindChildInParent("CityHall_Central", "SM_Bld_OfficeOld_Large_01");
        if (leftWing != null) leftWing.transform.localPosition = new Vector3(-24f, 0f, 0f);

        // Right wing – we only have one OfficeOld_Large, so find by index
        var cityHall = GameObject.Find("CityHall_Central");
        if (cityHall != null)
        {
            var offices = cityHall.GetComponentsInChildren<Transform>(true);
            int officeCount = 0;
            foreach (var t in offices)
            {
                if (t.name == "SM_Bld_OfficeOld_Large_01" && t.parent == cityHall.transform)
                {
                    if (officeCount == 0) t.localPosition = new Vector3(-24f, 0f, 0f);
                    else                  t.localPosition = new Vector3( 24f, 0f, 0f);
                    officeCount++;
                }
            }
        }

        // Lootable Depot – left side, clear of everything
        SetTransform("LootableBuilding_Depot", new Vector3(-42f, 0f, 8f), Quaternion.Euler(0, 90, 0));

        // Lootable Market – right side
        SetTransform("LootableBuilding_Market", new Vector3(42f, 0f, -8f), Quaternion.Euler(0, -90, 0));

        // Background buildings – push further out
        var bgParent = GameObject.Find("Background_Buildings");
        if (bgParent != null)
        {
            (string name, Vector3 pos, float rot)[] bgFixes = {
                ("SM_Bld_Apartment_Stack_01", new Vector3( 58f, 0f, 32f),   0f),
                ("SM_Bld_Apartment_Stack_02", new Vector3( 68f, 0f, 32f),   0f),
                ("SM_Bld_Apartment_Stack_03", new Vector3( 58f, 0f, 20f),   0f),
                ("SM_Bld_Apartment_01",       new Vector3( 68f, 0f, 20f),   0f),
                ("SM_Bld_Apartment_02",       new Vector3(-60f, 0f, 22f), 180f),
                ("SM_Bld_Apartment_03",       new Vector3(-60f, 0f,  8f), 180f),
                ("SM_Bld_OfficeOld_Small_01", new Vector3(-55f, 0f,-32f),  45f),
                ("SM_Bld_OfficeOld_Small_02", new Vector3( 55f, 0f,-32f), -45f),
                ("SM_Bld_OfficeOld_Large_01", new Vector3(  0f, 0f,-60f), 180f),
            };
            foreach (var (bname, pos, rot) in bgFixes)
            {
                var t = bgParent.transform.Find(bname);
                if (t != null)
                {
                    t.position = pos;
                    t.rotation = Quaternion.Euler(0, rot, 0);
                }
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void FixVehiclePositions()
    {
        var vehParent = GameObject.Find("Vehicles");
        if (vehParent == null) return;

        // Precise non-overlapping vehicle positions
        // Each car is ~4.5 wide x 9 long – spacing them 6 units apart in rows
        (string name, Vector3 pos, float rot)[] vehFixes = {
            // Parking row LEFT of city hall (facing north)
            ("SM_Veh_Car_Sedan_01",  new Vector3(-20f, 0f, 24f),   0f),
            ("SM_Veh_Car_Small_01",  new Vector3(-14f, 0f, 24f),   0f),
            ("SM_Veh_Car_Van_01",    new Vector3( -8f, 0f, 24f),   0f),
            // Parking row RIGHT of city hall
            ("SM_Veh_Car_Muscle_01", new Vector3(  8f, 0f, 24f),   0f),
            ("SM_Veh_Car_Medium_01", new Vector3( 14f, 0f, 24f),   0f),
            ("SM_Veh_Car_Taxi_01",   new Vector3( 20f, 0f, 24f),   0f),
            // Abandoned on road / plaza
            ("SM_Veh_Car_Police_01", new Vector3(-10f, 0f,  -6f),  25f),
            ("SM_Veh_Car_Ambo_01",   new Vector3( 10f, 0f, -10f), -15f),
        };

        foreach (var (vname, pos, rot) in vehFixes)
        {
            var t = vehParent.transform.Find(vname);
            if (t != null)
            {
                t.position = pos;
                t.rotation = Quaternion.Euler(0, rot, 0);
            }
        }

        // Extra vehicles added by FixUrbanMapAtmosphere – reposition them too
        var allVeh = vehParent.GetComponentsInChildren<Transform>(false);
        // Additional vehicles placed later – find by position and spread
        int extraIdx = 0;
        (Vector3 pos, float rot)[] extraSlots = {
            (new Vector3(-20f, 0f, 30f),   0f),
            (new Vector3(-14f, 0f, 30f),   0f),
            (new Vector3( 20f, 0f, 30f),   0f),
            (new Vector3( 26f, 0f, 30f),   0f),
            (new Vector3( -5f, 0f, 44f),   0f),
            (new Vector3(  5f, 0f, 44f),   0f),
            (new Vector3(-44f, 0f,  0f),  90f),
            (new Vector3( 44f, 0f,  0f), -90f),
        };

        // Find duplicate vehicle names (the extras)
        var seen = new HashSet<string>();
        foreach (Transform child in vehParent.transform)
        {
            if (seen.Contains(child.name))
            {
                // This is a duplicate – reposition it
                if (extraIdx < extraSlots.Length)
                {
                    child.position = extraSlots[extraIdx].pos;
                    child.rotation = Quaternion.Euler(0, extraSlots[extraIdx].rot, 0);
                    extraIdx++;
                }
            }
            else
            {
                seen.Add(child.name);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void FixTreePositions()
    {
        var treesParent = GameObject.Find("Trees");
        if (treesParent == null) return;

        // Reassign ALL trees to clean non-overlapping positions
        var allTrees = new List<Transform>();
        foreach (Transform child in treesParent.transform)
            allTrees.Add(child);

        // Clean grid of tree positions – no overlaps with buildings
        Vector3[] treeGrid = {
            // Left avenue (flanking depot road)
            new Vector3(-30f, 0f, 32f), new Vector3(-30f, 0f, 22f),
            new Vector3(-30f, 0f, 12f), new Vector3(-30f, 0f,  2f),
            new Vector3(-30f, 0f,-8f),  new Vector3(-30f, 0f,-18f),
            new Vector3(-30f, 0f,-28f),
            // Right avenue
            new Vector3( 30f, 0f, 32f), new Vector3( 30f, 0f, 22f),
            new Vector3( 30f, 0f, 12f), new Vector3( 30f, 0f,  2f),
            new Vector3( 30f, 0f,-8f),  new Vector3( 30f, 0f,-18f),
            new Vector3( 30f, 0f,-28f),
            // North of city hall
            new Vector3(-10f, 0f, 36f), new Vector3(-5f,  0f, 38f),
            new Vector3(  0f, 0f, 40f), new Vector3( 5f,  0f, 38f),
            new Vector3( 10f, 0f, 36f),
            // South tree line
            new Vector3(-22f, 0f,-38f), new Vector3(-12f, 0f,-42f),
            new Vector3(  0f, 0f,-44f), new Vector3( 12f, 0f,-42f),
            new Vector3( 22f, 0f,-38f),
            // Corner clusters
            new Vector3(-44f, 0f, 44f), new Vector3(-40f, 0f, 48f),
            new Vector3(-48f, 0f, 40f), new Vector3(-44f, 0f, 36f),
            new Vector3( 44f, 0f, 44f), new Vector3( 40f, 0f, 48f),
            new Vector3( 48f, 0f, 40f), new Vector3( 44f, 0f, 36f),
            new Vector3(-44f, 0f,-44f), new Vector3( 44f, 0f,-44f),
            // Mid-field scattered
            new Vector3(-18f, 0f, 18f), new Vector3( 18f, 0f, 18f),
            new Vector3(-18f, 0f,-18f), new Vector3( 18f, 0f,-18f),
        };

        for (int i = 0; i < allTrees.Count; i++)
        {
            if (i < treeGrid.Length)
            {
                allTrees[i].position = treeGrid[i];
                allTrees[i].rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                allTrees[i].localScale = Vector3.one * Random.Range(0.9f, 1.2f);
            }
            else
            {
                // Extra trees – place in outer ring
                float angle = (i - treeGrid.Length) * 22.5f * Mathf.Deg2Rad;
                float r = 52f;
                allTrees[i].position = new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
                allTrees[i].rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void FixPropPositions()
    {
        var propsParent = GameObject.Find("Props");
        if (propsParent == null) return;

        // Containers near depot – neat stack
        SetChildPos(propsParent, "PfCont40FBlue",  new Vector3(-50f, 0f, 18f), 0f);
        SetChildPos(propsParent, "PfCont20FBeige", new Vector3(-50f, 0f, 10f), 0f);
        SetChildPos(propsParent, "PfCont40FBeige", new Vector3(-50f, 0f,  2f), 0f);

        // Light poles – along main road
        RepositionByName(propsParent, "SM_Prop_LightPole_Base_01", new Vector3[] {
            new Vector3(-5f, 0f, 22f), new Vector3(5f, 0f, 22f),
            new Vector3(-5f, 0f,-22f), new Vector3(5f, 0f,-22f),
            new Vector3(-38f,0f,  0f), new Vector3(38f,0f,  0f),
        });

        // Barriers – in front of city hall entrance
        RepositionByName(propsParent, "SM_Prop_Barrier_01", new Vector3[] {
            new Vector3(-8f, 0f, -1f), new Vector3(-4f, 0f, -1f),
            new Vector3( 4f, 0f, -1f), new Vector3( 8f, 0f, -1f),
        });

        // Benches – in plaza
        RepositionByName(propsParent, "SM_Prop_ParkBench_01", new Vector3[] {
            new Vector3(-7f, 0f, -4f), new Vector3(7f, 0f, -4f),
            new Vector3(-7f, 0f,  4f), new Vector3(7f, 0f,  4f),
        });

        // Hydrants – on sidewalk corners
        RepositionByName(propsParent, "SM_Prop_Hydrant_01", new Vector3[] {
            new Vector3(-9f, 0f, 2f), new Vector3(9f, 0f, 2f),
            new Vector3(-9f, 0f,-2f), new Vector3(9f, 0f,-2f),
        });

        // Trash / debris – spread naturally around market area
        Vector3[] trashPositions = {
            new Vector3(-16f, 0f,-14f), new Vector3(-14f, 0f,-17f),
            new Vector3( 16f, 0f,-14f), new Vector3( 14f, 0f,-17f),
            new Vector3(-22f, 0f, -8f), new Vector3(-20f, 0f,-11f),
            new Vector3( 22f, 0f, -8f), new Vector3( 20f, 0f,-11f),
            new Vector3( -4f, 0f,-20f), new Vector3(  4f, 0f,-20f),
            new Vector3(-10f, 0f,-26f), new Vector3( 10f, 0f,-26f),
            new Vector3(-36f, 0f, 14f), new Vector3(-34f, 0f, 11f),
            new Vector3(-36f, 0f, -2f), new Vector3(-34f, 0f, -5f),
            new Vector3( 36f, 0f,  4f), new Vector3( 36f, 0f, -2f),
            new Vector3( 36f, 0f,-14f), new Vector3( 36f, 0f,-18f),
        };

        string[] trashNames = {
            "SM_Prop_TrashBag_01", "SM_Prop_TrashBag_02", "SM_Prop_TrashBag_03",
            "SM_Prop_CardboardBox_01", "SM_Prop_CardboardBox_02",
            "SM_Prop_CardboardBox_03", "SM_Prop_CardboardBox_04",
            "SM_Prop_Pallet_01", "SM_Prop_Skip_01", "SM_Prop_Skip_02",
            "SM_Prop_Trashbin_01", "SM_Prop_Trashbin_02",
        };

        int trashIdx = 0;
        foreach (Transform child in propsParent.transform)
        {
            bool isTrash = false;
            foreach (var tn in trashNames)
                if (child.name == tn) { isTrash = true; break; }

            if (isTrash && trashIdx < trashPositions.Length)
            {
                child.position = trashPositions[trashIdx];
                child.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                trashIdx++;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void FixMarketStalls()
    {
        var stallParent = GameObject.Find("Market_Stalls");
        if (stallParent == null) return;

        // Reposition covers in a clean ring around the plaza
        (string name, Vector3 pos, float rot)[] stallFixes = {
            ("SM_Bld_Shop_Cover_01", new Vector3(-20f, 0f,-12f),   0f),
            ("SM_Bld_Shop_Cover_02", new Vector3(-12f, 0f,-20f),  90f),
            ("SM_Bld_Shop_Cover_03", new Vector3(  0f, 0f,-22f),   0f),
            ("SM_Bld_Shop_Cover_04", new Vector3( 12f, 0f,-20f), -90f),
            ("SM_Bld_Shop_Cover_05", new Vector3( 20f, 0f,-12f), 180f),
        };
        foreach (var (sname, pos, rot) in stallFixes)
        {
            var t = stallParent.transform.Find(sname);
            if (t != null) { t.position = pos; t.rotation = Quaternion.Euler(0, rot, 0); }
        }

        // Reposition shops on south side – evenly spaced
        (string name, Vector3 pos, float rot)[] shopFixes = {
            ("SM_Bld_Shop_01", new Vector3(-30f, 0f,-30f),   0f),
            ("SM_Bld_Shop_02", new Vector3(-16f, 0f,-32f),   0f),
            ("SM_Bld_Shop_03", new Vector3(  0f, 0f,-34f),   0f),
            ("SM_Bld_Shop_04", new Vector3( 16f, 0f,-32f),   0f),
        };
        foreach (var (sname, pos, rot) in shopFixes)
        {
            var t = stallParent.transform.Find(sname);
            if (t != null) { t.position = pos; t.rotation = Quaternion.Euler(0, rot, 0); }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void AddLootContainersToExisting()
    {
        // Add LootContainer to all interior loot props in both buildings
        AddLootToBuilding("LootableBuilding_Depot",  "Depot Supply Cache");
        AddLootToBuilding("LootableBuilding_Market", "Market Stash");
    }

    static void AddLootToBuilding(string buildingName, string lootLabel)
    {
        var building = GameObject.Find(buildingName);
        if (building == null) return;

        var interior = building.transform.Find("InteriorRoot");
        if (interior == null) return;

        string[] lootNames = { "Ammo Box", "Med Kit", "Food Supplies", "Weapon Parts" };
        int idx = 0;

        foreach (Transform child in interior)
        {
            // Add BoxCollider trigger if not present
            var col = child.GetComponent<BoxCollider>();
            if (col == null) col = child.gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1.5f, 1.5f, 1.5f);

            // Add LootContainer if not present
            var lc = child.GetComponent<LootContainer>();
            if (lc == null) lc = child.gameObject.AddComponent<LootContainer>();
            lc.containerName = $"{lootLabel} - {lootNames[idx % lootNames.Length]}";
            lc.isLooted = false;

            // Create a simple world-space hint label
            if (lc.interactHint == null)
            {
                var hint = CreateLootHint(child, "[E] Loot");
                lc.interactHint = hint;
            }

            idx++;
            EditorUtility.SetDirty(child.gameObject);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void PlaceLootCrates()
    {
        // Create a dedicated LootCrates parent
        var existing = GameObject.Find("LootCrates");
        if (existing != null) Undo.DestroyObjectImmediate(existing);

        var parent = new GameObject("LootCrates");
        Undo.RegisterCreatedObjectUndo(parent, "LootCrates");

        string[] cratePrefabPaths = {
            SYNTY_PROP + "SM_Prop_CardboardBox_01.prefab",
            SYNTY_PROP + "SM_Prop_CardboardBox_02.prefab",
            SYNTY_PROP + "SM_Prop_CardboardBox_03.prefab",
            SYNTY_PROP + "SM_Prop_CardboardBox_04.prefab",
            SYNTY_PROP + "SM_Prop_Skip_01.prefab",
            SYNTY_PROP + "SM_Prop_Skip_02.prefab",
        };

        string[] lootNames = {
            "Ammo Cache", "Med Supplies", "Food Rations", "Weapon Parts",
            "Fuel Canister", "Tool Kit", "Survival Gear", "Scrap Metal",
            "First Aid Kit", "Explosives", "Water Supply", "Radio Parts",
        };

        // Strategic loot positions around the map – spread out for gameplay
        (Vector3 pos, float rot, int prefabIdx)[] lootSpots = {
            // Near city hall entrance – contested loot
            (new Vector3( -3f, 0f,  -3f),  30f, 0),
            (new Vector3(  3f, 0f,  -3f), -20f, 1),
            // Inside market stall area
            (new Vector3(-18f, 0f, -10f),  45f, 2),
            (new Vector3( 18f, 0f, -10f), -45f, 3),
            (new Vector3(  0f, 0f, -20f),   0f, 4),
            // Near depot (left side)
            (new Vector3(-46f, 0f,  14f),  15f, 0),
            (new Vector3(-46f, 0f,   6f), -10f, 1),
            (new Vector3(-46f, 0f,  -2f),  20f, 5),
            // Near market building (right side)
            (new Vector3( 46f, 0f,   2f),   0f, 2),
            (new Vector3( 46f, 0f,  -6f),  30f, 3),
            // Scattered on roads / alleys
            (new Vector3(-24f, 0f,   0f),  60f, 4),
            (new Vector3( 24f, 0f,   0f), -60f, 5),
            (new Vector3(  0f, 0f,  32f),  10f, 0),
            (new Vector3(-12f, 0f, -30f),  25f, 1),
            (new Vector3( 12f, 0f, -30f), -25f, 2),
            // Behind shops
            (new Vector3(-28f, 0f, -28f),  90f, 3),
            (new Vector3( 28f, 0f, -28f), -90f, 4),
        };

        for (int i = 0; i < lootSpots.Length; i++)
        {
            var (pos, rot, prefabIdx) = lootSpots[i];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(cratePrefabPaths[prefabIdx]);
            if (prefab == null) continue;

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, rot, 0);
            go.name = $"LootCrate_{lootNames[i % lootNames.Length].Replace(" ", "_")}";
            Undo.RegisterCreatedObjectUndo(go, "LootCrate");

            // Add trigger collider
            var col = go.GetComponent<BoxCollider>();
            if (col == null) col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1.5f, 1.5f, 1.5f);

            // Add LootContainer
            var lc = go.AddComponent<LootContainer>();
            lc.containerName = lootNames[i % lootNames.Length];
            lc.isLooted = false;

            // Add hint label
            var hint = CreateLootHint(go.transform, "[E] Loot");
            lc.interactHint = hint;

            EditorUtility.SetDirty(go);
        }

        Debug.Log($"[PolishUrbanMap] Placed {lootSpots.Length} loot crates.");
    }

    // ─────────────────────────────────────────────────────────────────────
    static GameObject CreateLootHint(Transform parent, string text)
    {
        var hint = new GameObject("LootHint");
        hint.transform.SetParent(parent);
        hint.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        hint.transform.localRotation = Quaternion.identity;
        hint.SetActive(false);

        // Add a simple quad as visual placeholder (will show in game as billboard)
        // In a real game you'd use TextMeshPro world-space canvas
        // For now just a named empty that the LootContainer script toggles
        return hint;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Helpers
    static void SetTransform(string name, Vector3 pos, Quaternion rot)
    {
        var go = GameObject.Find(name);
        if (go != null) { go.transform.position = pos; go.transform.rotation = rot; }
    }

    static Transform FindChildInParent(string parentName, string childName)
    {
        var parent = GameObject.Find(parentName);
        if (parent == null) return null;
        return parent.transform.Find(childName);
    }

    static void SetChildPos(GameObject parent, string childName, Vector3 pos, float rotY)
    {
        var t = parent.transform.Find(childName);
        if (t != null) { t.position = pos; t.rotation = Quaternion.Euler(0, rotY, 0); }
    }

    static void RepositionByName(GameObject parent, string childName, Vector3[] positions)
    {
        int idx = 0;
        foreach (Transform child in parent.transform)
        {
            if (child.name == childName && idx < positions.Length)
            {
                child.position = positions[idx];
                idx++;
            }
        }
    }
}
