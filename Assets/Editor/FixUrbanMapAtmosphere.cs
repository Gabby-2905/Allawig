using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

/// <summary>
/// Fixes the atmosphere (fog too heavy), improves lighting,
/// and adds more density to the urban junkyard map.
/// </summary>
public class FixUrbanMapAtmosphere : EditorWindow
{
    const string SYNTY_BLD  = "Assets/SyntyStudios/PolygonCity/Prefabs/Buildings/";
    const string SYNTY_ENV  = "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/";
    const string SYNTY_PROP = "Assets/SyntyStudios/PolygonCity/Prefabs/Props/";
    const string SYNTY_VEH  = "Assets/SyntyStudios/PolygonCity/Prefabs/Vehicles/";

    [MenuItem("Tools/Fix Urban Map Atmosphere")]
    public static void Fix()
    {
        // ── 1. Fix fog – push it much further out ─────────────────────────
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.68f, 0.65f, 0.50f, 1f);
        RenderSettings.fogStartDistance = 120f;   // was 60 – now much further
        RenderSettings.fogEndDistance   = 350f;   // was 200

        // ── 2. Fix ambient lighting – brighter, more contrast ─────────────
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.82f, 0.78f, 0.62f);
        RenderSettings.ambientEquatorColor = new Color(0.60f, 0.56f, 0.42f);
        RenderSettings.ambientGroundColor  = new Color(0.28f, 0.25f, 0.18f);

        // ── 3. Fix directional light ──────────────────────────────────────
        var dirLight = GameObject.Find("Directional Light");
        if (dirLight != null)
        {
            var l = dirLight.GetComponent<Light>();
            if (l != null)
            {
                l.color = new Color(1f, 0.96f, 0.80f);
                l.intensity = 1.4f;
                l.shadows = LightShadows.Soft;
                dirLight.transform.rotation = Quaternion.Euler(52f, -22f, 0f);
            }
        }

        // ── 4. Fix post-process – less color filter tint, more contrast ───
        FixPostProcess();

        // ── 5. Add more trees for density ─────────────────────────────────
        AddMoreTrees();

        // ── 6. Add more market stall props ────────────────────────────────
        AddMoreMarketProps();

        // ── 7. Add more vehicles ──────────────────────────────────────────
        AddMoreVehicles();

        // ── 8. Save ───────────────────────────────────────────────────────
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("[FixUrbanMapAtmosphere] Done!");
    }

    static void FixPostProcess()
    {
        var vol = GameObject.FindFirstObjectByType<Volume>();
        if (vol == null) return;

        var profile = vol.sharedProfile;
        if (profile == null) return;

        if (profile.TryGet<ColorAdjustments>(out var ca))
        {
            ca.postExposure.Override(0.3f);
            ca.contrast.Override(20f);
            ca.colorFilter.Override(new Color(1f, 0.97f, 0.88f)); // very subtle warm tint
            ca.saturation.Override(-8f);
        }
        if (profile.TryGet<Vignette>(out var vig))
        {
            vig.intensity.Override(0.22f);
        }
        if (profile.TryGet<Bloom>(out var bloom))
        {
            bloom.threshold.Override(1.1f);
            bloom.intensity.Override(0.2f);
        }

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
    }

    static void AddMoreTrees()
    {
        var treesParent = GameObject.Find("Trees");
        if (treesParent == null)
        {
            treesParent = new GameObject("Trees");
            Undo.RegisterCreatedObjectUndo(treesParent, "Trees");
        }

        string[] treePaths = {
            SYNTY_ENV + "SM_Env_Tree_01.prefab",
            SYNTY_ENV + "SM_Env_Tree_02.prefab",
            SYNTY_ENV + "SM_Env_Tree_03.prefab",
        };

        // Dense tree clusters matching reference image
        Vector3[] extraTrees = {
            // Dense cluster top-left (like reference)
            new Vector3(-42f, 0f, 42f), new Vector3(-38f, 0f, 46f),
            new Vector3(-46f, 0f, 46f), new Vector3(-34f, 0f, 42f),
            new Vector3(-42f, 0f, 38f), new Vector3(-38f, 0f, 38f),
            // Dense cluster top-right
            new Vector3( 42f, 0f, 42f), new Vector3( 38f, 0f, 46f),
            new Vector3( 46f, 0f, 46f), new Vector3( 34f, 0f, 42f),
            new Vector3( 42f, 0f, 38f), new Vector3( 38f, 0f, 38f),
            // Along left side of city hall
            new Vector3(-12f, 0f, 22f), new Vector3(-16f, 0f, 22f),
            new Vector3(-20f, 0f, 22f), new Vector3(-12f, 0f, 28f),
            // Along right side of city hall
            new Vector3( 12f, 0f, 22f), new Vector3( 16f, 0f, 22f),
            new Vector3( 20f, 0f, 22f), new Vector3( 12f, 0f, 28f),
            // South tree line
            new Vector3(-20f, 0f,-40f), new Vector3(-10f, 0f,-42f),
            new Vector3(  0f, 0f,-44f), new Vector3( 10f, 0f,-42f),
            new Vector3( 20f, 0f,-40f),
            // Scattered mid-field
            new Vector3(-32f, 0f, 10f), new Vector3(-32f, 0f,-10f),
            new Vector3( 32f, 0f, 10f), new Vector3( 32f, 0f,-10f),
            new Vector3(-18f, 0f, 35f), new Vector3( 18f, 0f, 35f),
        };

        for (int i = 0; i < extraTrees.Length; i++)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(treePaths[i % treePaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, treesParent.transform);
            go.transform.position = extraTrees[i];
            go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            go.transform.localScale = Vector3.one * Random.Range(0.85f, 1.25f);
            Undo.RegisterCreatedObjectUndo(go, "Tree");
        }
    }

    static void AddMoreMarketProps()
    {
        var propsParent = GameObject.Find("Props");
        if (propsParent == null)
        {
            propsParent = new GameObject("Props");
            Undo.RegisterCreatedObjectUndo(propsParent, "Props");
        }

        // More cardboard boxes, pallets, skips around market area
        string[] propPaths = {
            SYNTY_PROP + "SM_Prop_CardboardBox_01.prefab",
            SYNTY_PROP + "SM_Prop_CardboardBox_02.prefab",
            SYNTY_PROP + "SM_Prop_CardboardBox_03.prefab",
            SYNTY_PROP + "SM_Prop_CardboardBox_04.prefab",
            SYNTY_PROP + "SM_Prop_Pallet_01.prefab",
            SYNTY_PROP + "SM_Prop_Skip_01.prefab",
            SYNTY_PROP + "SM_Prop_Skip_02.prefab",
            SYNTY_PROP + "SM_Prop_TrashBag_01.prefab",
            SYNTY_PROP + "SM_Prop_TrashBag_02.prefab",
            SYNTY_PROP + "SM_Prop_TrashBag_03.prefab",
            SYNTY_PROP + "SM_Prop_Trashbin_01.prefab",
            SYNTY_PROP + "SM_Prop_Trashbin_02.prefab",
        };

        Vector3[] propPositions = {
            // Around market stalls
            new Vector3(-16f, 0f,-14f), new Vector3(-14f, 0f,-16f),
            new Vector3( 16f, 0f,-14f), new Vector3( 14f, 0f,-16f),
            new Vector3(-22f, 0f, -8f), new Vector3(-20f, 0f,-10f),
            new Vector3( 22f, 0f, -8f), new Vector3( 20f, 0f,-10f),
            new Vector3( -4f, 0f,-18f), new Vector3(  4f, 0f,-18f),
            new Vector3(-10f, 0f,-24f), new Vector3( 10f, 0f,-24f),
            // Near depot
            new Vector3(-36f, 0f, 12f), new Vector3(-34f, 0f, 10f),
            new Vector3(-36f, 0f, -2f), new Vector3(-34f, 0f, -4f),
            // Near market building
            new Vector3( 34f, 0f,  2f), new Vector3( 36f, 0f,  0f),
            new Vector3( 34f, 0f,-12f), new Vector3( 36f, 0f,-14f),
        };

        for (int i = 0; i < propPositions.Length; i++)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(propPaths[i % propPaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, propsParent.transform);
            go.transform.position = propPositions[i];
            go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Undo.RegisterCreatedObjectUndo(go, "Prop");
        }

        // Benches in plaza
        var benchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SYNTY_PROP + "SM_Prop_ParkBench_01.prefab");
        if (benchPrefab != null)
        {
            Vector3[] benchPos = {
                new Vector3(-6f, 0f, -5f), new Vector3( 6f, 0f, -5f),
                new Vector3(-6f, 0f,  5f), new Vector3( 6f, 0f,  5f),
            };
            float[] benchRots = { 0f, 0f, 180f, 180f };
            for (int i = 0; i < benchPos.Length; i++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(benchPrefab, propsParent.transform);
                go.transform.position = benchPos[i];
                go.transform.rotation = Quaternion.Euler(0, benchRots[i], 0);
                Undo.RegisterCreatedObjectUndo(go, "Bench");
            }
        }

        // Hydrants
        var hydrantPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SYNTY_PROP + "SM_Prop_Hydrant_01.prefab");
        if (hydrantPrefab != null)
        {
            Vector3[] hydrantPos = {
                new Vector3(-8f, 0f, 2f), new Vector3(8f, 0f, 2f),
                new Vector3(-8f, 0f,-2f), new Vector3(8f, 0f,-2f),
            };
            foreach (var pos in hydrantPos)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(hydrantPrefab, propsParent.transform);
                go.transform.position = pos;
                Undo.RegisterCreatedObjectUndo(go, "Hydrant");
            }
        }
    }

    static void AddMoreVehicles()
    {
        var vehParent = GameObject.Find("Vehicles");
        if (vehParent == null)
        {
            vehParent = new GameObject("Vehicles");
            Undo.RegisterCreatedObjectUndo(vehParent, "Vehicles");
        }

        string[] vehPaths = {
            SYNTY_VEH + "SM_Veh_Car_Sedan_01.prefab",
            SYNTY_VEH + "SM_Veh_Car_Small_01.prefab",
            SYNTY_VEH + "SM_Veh_Car_Van_01.prefab",
            SYNTY_VEH + "SM_Veh_Car_Muscle_01.prefab",
            SYNTY_VEH + "SM_Veh_Car_Medium_01.prefab",
        };

        (Vector3 pos, float rot)[] extraVehicles = {
            // More parking lot vehicles
            (new Vector3(-14f, 0f, 22f),   5f),
            (new Vector3(-18f, 0f, 22f),  -5f),
            (new Vector3( 22f, 0f, 22f),   0f),
            (new Vector3( 26f, 0f, 22f),  10f),
            // Street vehicles
            (new Vector3(-5f,  0f, 40f),   0f),
            (new Vector3( 5f,  0f, 40f),   0f),
            (new Vector3(-40f, 0f,  0f),  90f),
            (new Vector3( 40f, 0f,  0f), -90f),
        };

        for (int i = 0; i < extraVehicles.Length; i++)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(vehPaths[i % vehPaths.Length]);
            if (p == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, vehParent.transform);
            go.transform.position = extraVehicles[i].pos;
            go.transform.rotation = Quaternion.Euler(0, extraVehicles[i].rot, 0);
            Undo.RegisterCreatedObjectUndo(go, "Vehicle");
        }
    }
}
