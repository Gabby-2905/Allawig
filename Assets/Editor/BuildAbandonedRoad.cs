using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BuildAbandonedRoad
{
    [MenuItem("Tools/Build Abandoned Road Scene")]
    public static void Execute()
    {
        // ── 1. Remove old Map and GasStation ────────────────────────────────
        var map = GameObject.Find("Map");
        if (map != null) GameObject.DestroyImmediate(map);
        var gas = GameObject.Find("GasStation");
        if (gas != null) GameObject.DestroyImmediate(gas);

        // ── 2. Shape the terrain ─────────────────────────────────────────────
        var terrainGO = GameObject.Find("AbandonedRoadTerrain");
        if (terrainGO == null) { Debug.LogError("Terrain not found!"); return; }
        var terrain = terrainGO.GetComponent<Terrain>();
        var td = terrain.terrainData;

        int res = td.heightmapResolution; // 257
        float[,] heights = new float[res, res];

        System.Random rng = new System.Random(42);

        for (int z = 0; z < res; z++)
        {
            for (int x = 0; x < res; x++)
            {
                float nx = x / (float)(res - 1);
                float nz = z / (float)(res - 1);

                // Base rolling hills using multiple sine waves
                float h = 0f;
                h += Mathf.Sin(nx * 3.5f) * 0.08f;
                h += Mathf.Sin(nz * 2.8f) * 0.07f;
                h += Mathf.Sin(nx * 7f + nz * 5f) * 0.03f;
                h += Mathf.Sin(nx * 1.5f + nz * 2f) * 0.12f;

                // Add random micro-noise for rough ground feel
                h += (float)(rng.NextDouble() - 0.5) * 0.015f;

                // Flatten a road corridor along Z axis (center X = 0.5, width ~0.08)
                float distFromCenter = Mathf.Abs(nx - 0.5f);
                float roadWidth = 0.08f;
                float roadBlend = Mathf.SmoothStep(roadWidth + 0.04f, roadWidth, distFromCenter);

                // Road sits at a fixed height with slight undulation
                float roadH = 0.18f + Mathf.Sin(nz * 4f) * 0.005f;
                h = Mathf.Lerp(h + 0.15f, roadH, roadBlend);

                heights[z, x] = Mathf.Clamp01(h);
            }
        }

        td.SetHeights(0, 0, heights);

        // ── 3. Create terrain materials ──────────────────────────────────────
        // Dry overgrown grass material
        var grassMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        grassMat.name = "TerrainGrass";
        grassMat.color = new Color(0.38f, 0.42f, 0.22f); // dry olive green
        grassMat.SetFloat("_Smoothness", 0.1f);
        AssetDatabase.CreateAsset(grassMat, "Assets/TerrainGrass.mat");

        // Cracked asphalt road material
        var roadMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        roadMat.name = "CrackedAsphalt";
        roadMat.color = new Color(0.22f, 0.22f, 0.22f); // dark grey
        roadMat.SetFloat("_Smoothness", 0.05f);
        AssetDatabase.CreateAsset(roadMat, "Assets/CrackedAsphalt.mat");

        AssetDatabase.SaveAssets();

        // Apply grass material to terrain
        terrain.materialTemplate = grassMat;

        // ── 4. Add road surface tiles along the center ───────────────────────
        var roadPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Road_Bare_01.prefab");
        var roadPatchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Road_Patch_01.prefab");

        var roadParent = new GameObject("Road");

        if (roadPrefab != null)
        {
            // Lay road tiles along Z from -120 to +120, 2 tiles wide
            for (int zi = -24; zi <= 24; zi++)
            {
                for (int xi = -1; xi <= 0; xi++)
                {
                    float wx = xi * 5f + 2.5f;
                    float wz = zi * 5f + 2.5f;
                    float wy = terrain.SampleHeight(new Vector3(wx, 0, wz));

                    var tile = (GameObject)PrefabUtility.InstantiatePrefab(roadPrefab, roadParent.transform);
                    tile.transform.position = new Vector3(wx, wy, wz);
                    tile.name = $"Road_{xi}_{zi}";
                }
            }
        }

        // Scatter some road patches for abandoned/cracked look
        if (roadPatchPrefab != null)
        {
            System.Random patchRng = new System.Random(99);
            for (int i = 0; i < 15; i++)
            {
                float wx = (float)(patchRng.NextDouble() - 0.5) * 8f;
                float wz = (float)(patchRng.NextDouble() * 2 - 1) * 100f;
                float wy = terrain.SampleHeight(new Vector3(wx, 0, wz));
                var patch = (GameObject)PrefabUtility.InstantiatePrefab(roadPatchPrefab, roadParent.transform);
                patch.transform.position = new Vector3(wx, wy + 0.01f, wz);
                patch.transform.rotation = Quaternion.Euler(0, (float)(patchRng.NextDouble() * 360), 0);
            }
        }

        // ── 5. Place abandoned props ─────────────────────────────────────────
        var propsParent = new GameObject("Props");

        // Dead trees along roadside
        string[] deadTreePaths = {
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_TreeDead_01.prefab",
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_TreeDead_02.prefab",
            "Assets/SyntyStudios/PolygonStarter/Prefabs/SM_Generic_TreeDead_01.prefab",
        };
        // Live trees for background
        string[] treePaths = {
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Tree_01.prefab",
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Tree_03.prefab",
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_TreePine_01.prefab",
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_TreePine_02.prefab",
        };
        // Rocks
        string[] rockPaths = {
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_01.prefab",
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_02.prefab",
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Rock_03.prefab",
            "Assets/SyntyStudios/PolygonStarter/Prefabs/SM_Generic_Small_Rocks_01.prefab",
        };
        // Bushes
        string[] bushPaths = {
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Bush_01.prefab",
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Bush_02.prefab",
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Environments/SM_Env_Plant_01.prefab",
        };
        // Abandoned props
        string[] abandonedPaths = {
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Cone_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_Barrier_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_TrashBag_01.prefab",
            "Assets/SyntyStudios/PolygonCity/Prefabs/Props/SM_Prop_CardboardBox_01.prefab",
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Barrel_01.prefab",
            "Assets/SyntyStudios/PolygonAdventure/Prefabs/Props/SM_Prop_Crate_01.prefab",
        };

        System.Random propRng = new System.Random(77);

        // Dead trees close to road edges
        PlaceProps(deadTreePaths, propsParent.transform, terrain, propRng,
            count: 30, xMin: 8f, xMax: 35f, zMin: -110f, zMax: 110f,
            scaleMin: 0.8f, scaleMax: 1.4f, bothSides: true);

        // Dense trees further out
        PlaceProps(treePaths, propsParent.transform, terrain, propRng,
            count: 80, xMin: 20f, xMax: 140f, zMin: -140f, zMax: 140f,
            scaleMin: 0.9f, scaleMax: 1.6f, bothSides: true);

        // Rocks scattered around
        PlaceProps(rockPaths, propsParent.transform, terrain, propRng,
            count: 50, xMin: 5f, xMax: 80f, zMin: -120f, zMax: 120f,
            scaleMin: 0.5f, scaleMax: 2.0f, bothSides: true);

        // Bushes encroaching on road
        PlaceProps(bushPaths, propsParent.transform, terrain, propRng,
            count: 60, xMin: 4f, xMax: 25f, zMin: -110f, zMax: 110f,
            scaleMin: 0.6f, scaleMax: 1.2f, bothSides: true);

        // Abandoned props on/near road
        PlaceProps(abandonedPaths, propsParent.transform, terrain, propRng,
            count: 20, xMin: 0f, xMax: 8f, zMin: -100f, zMax: 100f,
            scaleMin: 0.9f, scaleMax: 1.1f, bothSides: true);

        // ── 6. Overcast skybox ───────────────────────────────────────────────
        var skyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/DaySkybox.mat");
        if (skyMat == null)
        {
            skyMat = new Material(Shader.Find("Skybox/Procedural"));
            skyMat.name = "OvercastSky";
            AssetDatabase.CreateAsset(skyMat, "Assets/OvercastSky.mat");
        }
        skyMat.SetFloat("_SunSize", 0.02f);
        skyMat.SetFloat("_AtmosphereThickness", 1.8f);
        skyMat.SetColor("_SkyTint", new Color(0.45f, 0.48f, 0.52f));
        skyMat.SetColor("_GroundColor", new Color(0.3f, 0.3f, 0.28f));
        skyMat.SetFloat("_Exposure", 0.9f);
        RenderSettings.skybox = skyMat;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 0.8f;
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.55f, 0.55f, 0.52f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 60f;
        RenderSettings.fogEndDistance = 200f;
        DynamicGI.UpdateEnvironment();

        // ── 7. Directional light (overcast) ──────────────────────────────────
        var dirLight = GameObject.Find("Directional Light");
        if (dirLight != null)
        {
            var light = dirLight.GetComponent<Light>();
            if (light != null)
            {
                light.intensity = 0.7f;
                light.color = new Color(0.85f, 0.88f, 0.92f);
                light.shadows = LightShadows.Soft;
            }
            dirLight.transform.rotation = Quaternion.Euler(45f, 160f, 0f);
        }

        // ── 8. Reposition player on road ─────────────────────────────────────
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            float roadY = terrain.SampleHeight(Vector3.zero);
            player.transform.position = new Vector3(0f, roadY + 0.1f, 0f);
            if (cc != null) cc.enabled = true;
            try { player.tag = "Player"; } catch { }
        }

        // ── 9. Fix camera ─────────────────────────────────────────────────────
        var camGO = GameObject.Find("Main Camera");
        if (camGO != null)
        {
            var cam = camGO.GetComponent<Camera>();
            if (cam != null) cam.clearFlags = CameraClearFlags.Skybox;
        }

        // ── 10. Save ──────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[BuildAbandonedRoad] Done! Abandoned road terrain built.");
    }

    static void PlaceProps(string[] prefabPaths, Transform parent, Terrain terrain,
        System.Random rng, int count, float xMin, float xMax,
        float zMin, float zMax, float scaleMin, float scaleMax, bool bothSides)
    {
        // Load valid prefabs
        var prefabs = new System.Collections.Generic.List<GameObject>();
        foreach (var path in prefabPaths)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (p != null) prefabs.Add(p);
        }
        if (prefabs.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            var prefab = prefabs[rng.Next(prefabs.Count)];
            float side = bothSides ? (rng.Next(2) == 0 ? 1f : -1f) : 1f;
            float x = side * ((float)(rng.NextDouble() * (xMax - xMin)) + xMin);
            float z = (float)(rng.NextDouble() * (zMax - zMin)) + zMin;
            float y = terrain.SampleHeight(new Vector3(x, 0, z));
            float scale = (float)(rng.NextDouble() * (scaleMax - scaleMin)) + scaleMin;
            float rot = (float)(rng.NextDouble() * 360f);

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = new Vector3(x, y, z);
            go.transform.rotation = Quaternion.Euler(0, rot, 0);
            go.transform.localScale = Vector3.one * scale;
        }
    }
}
