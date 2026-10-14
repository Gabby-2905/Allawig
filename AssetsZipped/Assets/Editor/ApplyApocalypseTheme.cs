using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Applies a post-apocalyptic theme to the GameScene:
/// - Dark stormy lighting
/// - Heavy rain particle system
/// - Fog
/// - Dead trees replacing live ones
/// - Debris, trash bags, barrels, crates scattered around
/// - Knocked-over/damaged props
/// - Dark skybox
/// </summary>
public class ApplyApocalypseTheme : MonoBehaviour
{
    [MenuItem("Tools/Apply Apocalypse Theme")]
    public static void Execute()
    {
        SetLighting();
        SetFog();
        SetRain();
        ReplaceTreesWithDead();
        RemoveCleanProps();
        AddDebrisAndProps();
        AddAbandonedVehicles();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[ApocalypseTheme] Done!");
    }

    // ── Lighting: dim, cold, overcast ─────────────────────────────────────────
    static void SetLighting()
    {
        var light = GameObject.Find("Directional Light");
        if (light != null)
        {
            var l = light.GetComponent<Light>();
            if (l != null)
            {
                // Very dim, cold blue-grey overcast light
                l.color     = new Color(0.35f, 0.40f, 0.50f, 1f);
                l.intensity = 0.25f;
                // Angle: low, coming from side for dramatic shadows
                light.transform.rotation = Quaternion.Euler(35f, 200f, 0f);
            }
        }

        // Ambient light: very dark
        RenderSettings.ambientMode      = AmbientMode.Flat;
        RenderSettings.ambientLight     = new Color(0.08f, 0.09f, 0.12f, 1f);
        RenderSettings.ambientIntensity = 0.15f;

        // Skybox: use solid dark color (remove skybox material for pure dark sky)
        RenderSettings.skybox = null;

        Debug.Log("[ApocalypseTheme] Lighting set.");
    }

    // ── Fog: heavy dark fog ───────────────────────────────────────────────────
    static void SetFog()
    {
        RenderSettings.fog          = true;
        RenderSettings.fogMode      = FogMode.ExponentialSquared;
        RenderSettings.fogColor     = new Color(0.06f, 0.07f, 0.09f, 1f);
        RenderSettings.fogDensity   = 0.018f;
        Debug.Log("[ApocalypseTheme] Fog set.");
    }

    // ── Rain: boost particle count and speed ──────────────────────────────────
    static void SetRain()
    {
        var rainGO = GameObject.Find("Rain");
        if (rainGO == null) return;

        var ps = rainGO.GetComponent<ParticleSystem>();
        if (ps == null) return;

        // Move rain higher so it covers the whole map
        rainGO.transform.position = new Vector3(0f, 25f, 0f);

        var main = ps.main;
        main.startSpeed    = new ParticleSystem.MinMaxCurve(18f, 25f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.04f, 0.08f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 1.8f);
        main.startColor    = new ParticleSystem.MinMaxGradient(
            new Color(0.5f, 0.6f, 0.75f, 0.6f),
            new Color(0.4f, 0.5f, 0.65f, 0.4f));
        main.maxParticles  = 8000;

        var emission = ps.emission;
        emission.rateOverTime = 600f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale     = new Vector3(90f, 1f, 90f);

        Debug.Log("[ApocalypseTheme] Rain boosted.");
    }

    // ── Replace live trees with dead trees ───────────────────────────────────
    static void ReplaceTreesWithDead()
    {
        var deadTree1 = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_TreeDead_01.prefab");
        var deadTree2 = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_TreeDead_02.prefab");

        if (deadTree1 == null && deadTree2 == null)
        {
            Debug.LogWarning("[ApocalypseTheme] Dead tree prefabs not found.");
            return;
        }

        var propsRoot = GameObject.Find("StreetProps");
        if (propsRoot == null) return;

        // Find and destroy all live tree children
        var toDestroy = new List<GameObject>();
        foreach (Transform child in propsRoot.transform)
        {
            if (child.name.StartsWith("Tree_"))
                toDestroy.Add(child.gameObject);
        }

        foreach (var go in toDestroy)
        {
            var pos = go.transform.position;
            var name = go.name;
            Undo.DestroyObjectImmediate(go);

            // Replace with dead tree
            var deadPrefab = (name.Contains("_L_")) ? deadTree1 : (deadTree2 ?? deadTree1);
            if (deadPrefab == null) continue;

            var dead = (GameObject)PrefabUtility.InstantiatePrefab(deadPrefab, propsRoot.transform);
            dead.transform.position = pos;
            dead.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            dead.name = name + "_Dead";
        }

        Debug.Log("[ApocalypseTheme] Trees replaced with dead trees.");
    }

    // ── Remove clean/cheerful props ───────────────────────────────────────────
    static void RemoveCleanProps()
    {
        // Remove light poles (they'd be broken in apocalypse — we'll add broken ones)
        var propsRoot = GameObject.Find("StreetProps");
        if (propsRoot == null) return;

        var toRemove = new List<GameObject>();
        foreach (Transform child in propsRoot.transform)
        {
            if (child.name.StartsWith("Light_"))
                toRemove.Add(child.gameObject);
        }
        foreach (var go in toRemove)
            Undo.DestroyObjectImmediate(go);

        Debug.Log("[ApocalypseTheme] Clean props removed.");
    }

    // ── Add debris, trash, barrels, crates ───────────────────────────────────
    static void AddDebrisAndProps()
    {
        var root = new GameObject("ApocProps");
        Undo.RegisterCreatedObjectUndo(root, "ApocProps");

        float y = 0f;

        // Prefab paths
        var trashBag1  = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_01.prefab");
        var trashBag2  = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_02.prefab");
        var trashBag3  = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_03.prefab");
        var cardboard1 = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_01.prefab");
        var cardboard2 = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_02.prefab");
        var barrel     = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Barrel_01.prefab");
        var crate      = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Crate_01.prefab");
        var barrier    = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Barrier_01.prefab");
        var couch      = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Couch_01.prefab");
        var trashCan   = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashCan_01.prefab");
        var skip       = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Skip_01.prefab");
        var pallet     = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Pallet_01.prefab");
        var newspaper  = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Newspaper_01.prefab");
        var paper      = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Paper_01.prefab");
        var rock1      = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_01.prefab");
        var rock2      = Load("Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_02.prefab");

        // ── Left side debris (grass area X=-20 to -32) ──────────────────────
        // Trash bags cluster near store
        PlaceRandom(trashBag1, root, -18f, y, 8f,  2f, "Trash_01");
        PlaceRandom(trashBag2, root, -20f, y, 6f,  2f, "Trash_02");
        PlaceRandom(trashBag3, root, -19f, y, 10f, 2f, "Trash_03");
        PlaceRandom(trashBag1, root, -25f, y, -5f, 2f, "Trash_04");
        PlaceRandom(trashBag2, root, -26f, y, -8f, 2f, "Trash_05");
        PlaceRandom(trashBag1, root, -28f, y, 15f, 2f, "Trash_06");
        PlaceRandom(trashBag3, root, -22f, y, -15f,2f, "Trash_07");

        // Cardboard boxes
        PlaceRandom(cardboard1, root, -16f, y, 0f,  1f, "Box_01");
        PlaceRandom(cardboard2, root, -17f, y, -2f, 1f, "Box_02");
        PlaceRandom(cardboard1, root, -24f, y, 20f, 1f, "Box_03");
        PlaceRandom(cardboard2, root, -30f, y, -10f,1f, "Box_04");

        // Barrels
        if (barrel != null)
        {
            PlaceRandom(barrel, root, -20f, y, -20f, 1f, "Barrel_01");
            PlaceRandom(barrel, root, -25f, y, 10f,  1f, "Barrel_02");
            PlaceRandom(barrel, root, -28f, y, -5f,  1f, "Barrel_03");
        }

        // Crates
        if (crate != null)
        {
            PlaceRandom(crate, root, -22f, y, -12f, 1f, "Crate_01");
            PlaceRandom(crate, root, -18f, y, 18f,  1f, "Crate_02");
        }

        // Skip/dumpster
        if (skip != null)
            Inst(skip, root, new Vector3(-20f, y, -18f), Quaternion.Euler(0, 45f, 0), "Skip_01");

        // Couch (abandoned furniture)
        if (couch != null)
            Inst(couch, root, new Vector3(-26f, y, 5f), Quaternion.Euler(0, 110f, 0), "Couch_01");

        // Pallets
        if (pallet != null)
        {
            Inst(pallet, root, new Vector3(-19f, y, -5f), Quaternion.Euler(0, 30f, 0), "Pallet_01");
            Inst(pallet, root, new Vector3(-21f, y, -6f), Quaternion.Euler(0, 15f, 0), "Pallet_02");
        }

        // ── Right side debris (grass area X=+20 to +32) ─────────────────────
        PlaceRandom(trashBag1, root, 20f, y, 5f,   2f, "Trash_R01");
        PlaceRandom(trashBag2, root, 22f, y, -8f,  2f, "Trash_R02");
        PlaceRandom(trashBag3, root, 25f, y, 15f,  2f, "Trash_R03");
        PlaceRandom(trashBag1, root, 28f, y, -15f, 2f, "Trash_R04");
        PlaceRandom(trashBag2, root, 30f, y, 0f,   2f, "Trash_R05");

        if (barrel != null)
        {
            PlaceRandom(barrel, root, 22f, y, -20f, 1f, "Barrel_R01");
            PlaceRandom(barrel, root, 26f, y, 8f,   1f, "Barrel_R02");
        }

        if (crate != null)
            PlaceRandom(crate, root, 24f, y, -5f, 1f, "Crate_R01");

        // ── Road debris (scattered on sidewalk/road edges) ───────────────────
        // Barriers knocked over on road
        if (barrier != null)
        {
            Inst(barrier, root, new Vector3(-8f, y, -20f), Quaternion.Euler(0, 80f, 0),  "Barrier_01");
            Inst(barrier, root, new Vector3( 8f, y,  25f), Quaternion.Euler(0, 170f, 0), "Barrier_02");
            Inst(barrier, root, new Vector3(-6f, y,  30f), Quaternion.Euler(0, 45f, 0),  "Barrier_03");
            Inst(barrier, root, new Vector3( 7f, y, -28f), Quaternion.Euler(0, 200f, 0), "Barrier_04");
        }

        // Trash cans
        if (trashCan != null)
        {
            Inst(trashCan, root, new Vector3(-10f, y, -10f), Quaternion.Euler(0, 60f, 0),  "TrashCan_01");
            Inst(trashCan, root, new Vector3( 10f, y,  10f), Quaternion.Euler(0, 130f, 0), "TrashCan_02");
            Inst(trashCan, root, new Vector3(-10f, y,  20f), Quaternion.Euler(0, 200f, 0), "TrashCan_03");
        }

        // Newspapers/papers scattered
        if (newspaper != null)
        {
            for (int i = 0; i < 6; i++)
                PlaceRandom(newspaper, root, Random.Range(-15f, 15f), y, Random.Range(-30f, 30f), 0.5f, $"News_{i}");
        }
        if (paper != null)
        {
            for (int i = 0; i < 8; i++)
                PlaceRandom(paper, root, Random.Range(-12f, 12f), y, Random.Range(-30f, 30f), 0.5f, $"Paper_{i}");
        }

        // Rocks scattered on grass
        if (rock1 != null)
        {
            PlaceRandom(rock1, root, -25f, y, -25f, 1f, "Rock_01");
            PlaceRandom(rock1, root, -30f, y,  20f, 1f, "Rock_02");
            PlaceRandom(rock1, root,  25f, y, -20f, 1f, "Rock_03");
        }
        if (rock2 != null)
        {
            PlaceRandom(rock2, root, -28f, y, 12f, 1f, "Rock_04");
            PlaceRandom(rock2, root,  28f, y, 18f, 1f, "Rock_05");
        }

        Debug.Log("[ApocalypseTheme] Debris and props added.");
    }

    // ── Add more abandoned/wrecked vehicles ───────────────────────────────────
    static void AddAbandonedVehicles()
    {
        var carsRoot = GameObject.Find("Cars");
        if (carsRoot == null) return;

        float y = 0f;

        // Add a van and taxi as abandoned wrecks
        var van  = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Van_01.prefab");
        var taxi = Load("Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/SM_Veh_Car_Taxi_01.prefab");

        if (van != null)
        {
            // Van abandoned on left grass, slightly angled
            Inst(van, carsRoot, new Vector3(-22f, y, -10f), Quaternion.Euler(0, 35f, 0), "Van_Abandoned");
        }
        if (taxi != null)
        {
            // Taxi on right side, angled as if crashed
            Inst(taxi, carsRoot, new Vector3(20f, y, -18f), Quaternion.Euler(0, 155f, 0), "Taxi_Abandoned");
        }

        Debug.Log("[ApocalypseTheme] Abandoned vehicles added.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    static GameObject Load(string path) => AssetDatabase.LoadAssetAtPath<GameObject>(path);

    static void Inst(GameObject prefab, GameObject parent, Vector3 pos, Quaternion rot, string name)
    {
        if (prefab == null) return;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
        go.transform.SetPositionAndRotation(pos, rot);
        go.name = name;
    }

    static void PlaceRandom(GameObject prefab, GameObject parent, float x, float y, float z, float spread, string name)
    {
        if (prefab == null) return;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
        go.transform.position = new Vector3(
            x + Random.Range(-spread, spread),
            y,
            z + Random.Range(-spread, spread));
        go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        go.name = name;
    }
}
