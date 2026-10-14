using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixSceneComplete
{
    [MenuItem("Tools/Fix Scene Complete")]
    public static void Execute()
    {
        // ── 1. Fix Skybox / Lighting ─────────────────────────────────────────
        // Set a procedural skybox so the sky isn't black
        var skyMat = new Material(Shader.Find("Skybox/Procedural"));
        skyMat.name = "ProceduralSky";
        skyMat.SetFloat("_SunSize", 0.04f);
        skyMat.SetFloat("_AtmosphereThickness", 1.0f);
        skyMat.SetColor("_SkyTint", new Color(0.5f, 0.5f, 0.5f));
        skyMat.SetColor("_GroundColor", new Color(0.37f, 0.35f, 0.35f));
        skyMat.SetFloat("_Exposure", 1.3f);
        RenderSettings.skybox = skyMat;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.0f;
        DynamicGI.UpdateEnvironment();

        // ── 2. Fix Directional Light ─────────────────────────────────────────
        var dirLight = GameObject.Find("Directional Light");
        if (dirLight != null)
        {
            var light = dirLight.GetComponent<Light>();
            if (light != null)
            {
                light.intensity = 1.2f;
                light.color = new Color(1f, 0.95f, 0.84f);
            }
            dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // ── 3. Destroy ALL old map children and rebuild ──────────────────────
        var mapGO = GameObject.Find("Map");
        if (mapGO == null) mapGO = new GameObject("Map");

        // Destroy all children of Map
        var children = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in mapGO.transform)
            children.Add(child.gameObject);
        foreach (var child in children)
            GameObject.DestroyImmediate(child);

        // ── 4. Load prefabs ──────────────────────────────────────────────────
        var roadPrefab     = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Road_01.prefab");
        var yellowPrefab   = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Road_YellowLines_01.prefab");
        var sidewalkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Sidewalk_Straight_01.prefab");
        var grassPrefab    = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SyntyStudios/PolygonCity/Prefabs/Environments/SM_Env_Grass_01.prefab");

        // The road prefab mesh is centered at (-2.5, 0, -2.5) in local space,
        // so to place a tile at world position (wx, 0, wz) we set localPosition = (wx+2.5, 0, wz+2.5)
        // Tile size = 5 units. We'll use bottom-left corner as the reference.
        // To place tile so its CENTER is at (cx, 0, cz): localPosition = (cx+2.5, 0, cz+2.5)
        // Actually easier: just place at corner and let the mesh fill the 5x5 square.

        // Road: 4 lanes wide (X: -10 to +10), 20 tiles long (Z: -50 to +50)
        // Sidewalk: 1 tile each side (X: -15 to -10, +10 to +15)
        // Grass: 4 tiles each side (X: -35 to -15, +15 to +35)

        float ts = 5f; // tile size

        // Road tiles: x from -2 to +1 (4 tiles wide), z from -5 to +4 (10 tiles long)
        for (int zi = -5; zi <= 4; zi++)
        {
            for (int xi = -2; xi <= 1; xi++)
            {
                // Bottom-left corner of tile in world space
                float wx = xi * ts;
                float wz = zi * ts;
                PlaceTile(roadPrefab, mapGO.transform, wx, wz, $"Road_{xi}_{zi}");
            }

            // Yellow center line at x=0
            PlaceTile(yellowPrefab, mapGO.transform, 0f, zi * ts, $"Yellow_{zi}", 0.002f);

            // Sidewalks
            PlaceTile(sidewalkPrefab, mapGO.transform, -3 * ts, zi * ts, $"SW_L_{zi}");
            PlaceTile(sidewalkPrefab, mapGO.transform,  2 * ts, zi * ts, $"SW_R_{zi}");

            // Grass (4 tiles each side)
            for (int gi = 0; gi < 5; gi++)
            {
                PlaceTile(grassPrefab, mapGO.transform, (-4 - gi) * ts, zi * ts, $"Grass_L_{gi}_{zi}");
                PlaceTile(grassPrefab, mapGO.transform,  (3 + gi) * ts, zi * ts, $"Grass_R_{gi}_{zi}");
            }
        }

        // ── 5. Move player to road center ────────────────────────────────────
        var playerGO = GameObject.Find("Player");
        if (playerGO != null)
        {
            var cc = playerGO.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerGO.transform.position = new Vector3(0f, 0.1f, 0f);
            if (cc != null) cc.enabled = true;
            try { playerGO.tag = "Player"; } catch { }
        }

        // ── 6. Reposition existing scene objects onto the road ───────────────
        // Move vehicles, props etc. to be near the road
        RepositionIfExists("Map/SM_Veh_Car_Sedan_01",   new Vector3(-5f, 0f, 5f));
        RepositionIfExists("Map/SM_Veh_Car_Van_01",     new Vector3(-5f, 0f, 15f));
        RepositionIfExists("Map/SM_Veh_Car_Muscle_01",  new Vector3( 5f, 0f, 10f));
        RepositionIfExists("Map/SM_Prop_LightPole_Base_01", new Vector3(12f, 0f, 0f));
        RepositionIfExists("Map/SM_Prop_Trashbin_01",   new Vector3(12f, 0f, 5f));
        RepositionIfExists("Map/SM_Prop_TrashBag_01",   new Vector3(12f, 0f, 6f));
        RepositionIfExists("Map/SM_Prop_TrafficLight_01", new Vector3(11f, 0f, -2f));
        RepositionIfExists("Map/SM_Env_Tree_01",        new Vector3(18f, 0f, 3f));
        RepositionIfExists("Map/SM_Env_Tree_02",        new Vector3(20f, 0f, 8f));
        RepositionIfExists("Map/SM_Prop_BusStop_01",    new Vector3(13f, 0f, -5f));
        RepositionIfExists("Map/SM_Prop_HotdogStand_01",new Vector3(13f, 0f, 2f));
        RepositionIfExists("Map/SM_Prop_CardboardBox_01",new Vector3(12f, 0f, 3f));
        RepositionIfExists("Map/SM_Prop_CardboardBox_02",new Vector3(12.5f, 0f, 3.5f));
        RepositionIfExists("Map/SM_Prop_Cone_01",       new Vector3(-8f, 0f, -3f));
        RepositionIfExists("Map/SM_Prop_ATM_01",        new Vector3(13f, 0f, -8f));
        RepositionIfExists("Map/SM_Bld_Shop_Cover_03",  new Vector3(13f, 3f, -10f));
        RepositionIfExists("Map/SM_Prop_Sign_Cafe_01",  new Vector3(14f, 0f, -10f));
        RepositionIfExists("Map/SM_Bld_Apartment_01",   new Vector3(20f, 0f, -10f));
        RepositionIfExists("Map/SM_Bld_Apartment_Roof_01", new Vector3(20f, 3f, -10f));
        RepositionIfExists("Map/SM_Bld_OfficeOld_Small_01", new Vector3(30f, 0f, -5f));
        RepositionIfExists("Map/SM_Bld_Apartment_Stack_01", new Vector3(25f, 0f, 5f));
        RepositionIfExists("Map/ConvenienceStore",      new Vector3(13f, 0f, -10f));

        // ── 7. Fix Gas Station position ──────────────────────────────────────
        var gasStation = GameObject.Find("GasStation");
        if (gasStation != null)
        {
            gasStation.transform.position = new Vector3(18f, 0f, 8f);
        }

        // ── 8. Save ──────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[FixSceneComplete] Done! Road rebuilt at origin, skybox fixed, player repositioned.");
    }

    static void PlaceTile(GameObject prefab, Transform parent, float wx, float wz, string name, float yOffset = 0f)
    {
        if (prefab == null) return;
        var tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        // The prefab mesh has its geometry starting at local (0,0,0) going to (5,0,5)
        // but the mesh center is at (-2.5, 0, -2.5) in local space.
        // So to place the tile's bottom-left corner at world (wx, 0, wz):
        // we set position = (wx + 2.5, yOffset, wz + 2.5)
        tile.transform.position = new Vector3(wx + 2.5f, yOffset, wz + 2.5f);
        tile.name = name;
    }

    static void RepositionIfExists(string path, Vector3 newPos)
    {
        var go = GameObject.Find(path);
        if (go != null) go.transform.position = newPos;
    }
}
