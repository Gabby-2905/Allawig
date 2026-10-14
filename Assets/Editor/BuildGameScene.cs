using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class BuildGameScene
{
    const string CITY = "Assets/SyntyStudios/PolygonCity/Prefabs/";
    const string ENV  = CITY + "Environments/";
    const string BLD  = CITY + "Buildings/";
    const string VEH  = CITY + "Vehicles/";
    const string PROP = CITY + "Props/";
    const string CHAR = CITY + "Characters/FixedScaleCharacters/";

    [MenuItem("Tools/Build Game Scene")]
    public static void Execute()
    {
        var scene = EditorSceneManager.GetSceneByName("GameScene");
        if (!scene.isLoaded)
            scene = EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

        // Clear scene
        foreach (var go in scene.GetRootGameObjects())
            GameObject.DestroyImmediate(go);

        // ── LIGHTING ─────────────────────────────────────────────────────────
        GameObject dirLight = new GameObject("Directional Light");
        var light = dirLight.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(0.62f, 0.68f, 0.78f);
        light.intensity = 0.75f;
        dirLight.transform.rotation = Quaternion.Euler(48f, -25f, 0f);
        SceneManager.MoveGameObjectToScene(dirLight, scene);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.32f, 0.36f, 0.44f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.48f, 0.52f, 0.58f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 25f;
        RenderSettings.fogEndDistance = 65f;

        // ── MAP ROOT ─────────────────────────────────────────────────────────
        GameObject mapRoot = new GameObject("Map");
        SceneManager.MoveGameObjectToScene(mapRoot, scene);

        // ── GROUND BASE ──────────────────────────────────────────────────────
        // Tile grass 5x5 units, cover 50x50 area = 10x10 tiles
        for (int x = -5; x < 5; x++)
            for (int z = -5; z < 5; z++)
                PlacePrefab(ENV + "SM_Env_Grass_01.prefab", mapRoot.transform,
                    new Vector3(x * 5f + 2.5f, 0f, z * 5f + 2.5f), Quaternion.identity);

        // ── ROADS ────────────────────────────────────────────────────────────
        // Horizontal road: z = -2.5 to 2.5, x = -25 to 25 (5-unit tiles)
        // Road tile is 5x5. Use SM_Env_Road_01 (straight) and SM_Env_Road_YellowLines
        for (int x = -5; x < 5; x++)
        {
            // Horizontal road row (z=0 center)
            PlacePrefab(ENV + "SM_Env_Road_01.prefab", mapRoot.transform,
                new Vector3(x * 5f + 2.5f, 0f, 0f), Quaternion.identity);
            // Vertical road col (x=0 center)
            PlacePrefab(ENV + "SM_Env_Road_01.prefab", mapRoot.transform,
                new Vector3(0f, 0f, x * 5f + 2.5f), Quaternion.Euler(0f, 90f, 0f));
        }

        // Intersection center
        PlacePrefab(ENV + "SM_Env_Road_01.prefab", mapRoot.transform,
            new Vector3(0f, 0f, 0f), Quaternion.identity);

        // Yellow center lines on horizontal road
        for (int x = -5; x < 5; x++)
        {
            if (x == -1 || x == 0) continue; // skip intersection
            PlacePrefab(ENV + "SM_Env_Road_YellowLines_01.prefab", mapRoot.transform,
                new Vector3(x * 5f + 2.5f, 0f, 0f), Quaternion.identity);
        }
        // Yellow center lines on vertical road
        for (int z = -5; z < 5; z++)
        {
            if (z == -1 || z == 0) continue;
            PlacePrefab(ENV + "SM_Env_Road_YellowLines_01.prefab", mapRoot.transform,
                new Vector3(0f, 0f, z * 5f + 2.5f), Quaternion.Euler(0f, 90f, 0f));
        }

        // ── SIDEWALKS ────────────────────────────────────────────────────────
        // Around the intersection, 5-unit sidewalk tiles
        // NW quadrant south edge (z=5 to 25, x=-25 to -5)
        float[] swXPos = { -17.5f, -12.5f, -7.5f };
        float[] swZPos = {  17.5f,  12.5f,   7.5f };

        // South sidewalk of north blocks (z=5)
        for (int x = -3; x < 0; x++)
            PlacePrefab(ENV + "SM_Env_Sidewalk_Straight_01.prefab", mapRoot.transform,
                new Vector3(x * 5f + 2.5f, 0f, 5f), Quaternion.identity);
        for (int x = 0; x < 3; x++)
            PlacePrefab(ENV + "SM_Env_Sidewalk_Straight_01.prefab", mapRoot.transform,
                new Vector3(x * 5f + 2.5f, 0f, 5f), Quaternion.identity);

        // North sidewalk of south blocks (z=-5)
        for (int x = -3; x < 0; x++)
            PlacePrefab(ENV + "SM_Env_Sidewalk_Straight_01.prefab", mapRoot.transform,
                new Vector3(x * 5f + 2.5f, 0f, -5f), Quaternion.Euler(0f, 180f, 0f));
        for (int x = 0; x < 3; x++)
            PlacePrefab(ENV + "SM_Env_Sidewalk_Straight_01.prefab", mapRoot.transform,
                new Vector3(x * 5f + 2.5f, 0f, -5f), Quaternion.Euler(0f, 180f, 0f));

        // East sidewalk of west blocks (x=-5)
        for (int z = -3; z < 0; z++)
            PlacePrefab(ENV + "SM_Env_Sidewalk_Straight_01.prefab", mapRoot.transform,
                new Vector3(-5f, 0f, z * 5f + 2.5f), Quaternion.Euler(0f, 90f, 0f));
        for (int z = 0; z < 3; z++)
            PlacePrefab(ENV + "SM_Env_Sidewalk_Straight_01.prefab", mapRoot.transform,
                new Vector3(-5f, 0f, z * 5f + 2.5f), Quaternion.Euler(0f, 90f, 0f));

        // West sidewalk of east blocks (x=5)
        for (int z = -3; z < 0; z++)
            PlacePrefab(ENV + "SM_Env_Sidewalk_Straight_01.prefab", mapRoot.transform,
                new Vector3(5f, 0f, z * 5f + 2.5f), Quaternion.Euler(0f, 270f, 0f));
        for (int z = 0; z < 3; z++)
            PlacePrefab(ENV + "SM_Env_Sidewalk_Straight_01.prefab", mapRoot.transform,
                new Vector3(5f, 0f, z * 5f + 2.5f), Quaternion.Euler(0f, 270f, 0f));

        // Sidewalk corners
        PlacePrefab(ENV + "SM_Env_Sidewalk_Corner_01.prefab", mapRoot.transform,
            new Vector3(-5f, 0f, 5f), Quaternion.Euler(0f, 90f, 0f));
        PlacePrefab(ENV + "SM_Env_Sidewalk_Corner_01.prefab", mapRoot.transform,
            new Vector3(5f, 0f, 5f), Quaternion.identity);
        PlacePrefab(ENV + "SM_Env_Sidewalk_Corner_01.prefab", mapRoot.transform,
            new Vector3(-5f, 0f, -5f), Quaternion.Euler(0f, 180f, 0f));
        PlacePrefab(ENV + "SM_Env_Sidewalk_Corner_01.prefab", mapRoot.transform,
            new Vector3(5f, 0f, -5f), Quaternion.Euler(0f, 270f, 0f));

        // ── CONVENIENCE STORE (NE corner) ────────────────────────────────────
        // SM_Bld_Shop_03 is 10x5 wide — perfect for a convenience store front
        // Place it at NE block, facing south (toward road)
        GameObject store = PlacePrefab(BLD + "SM_Bld_Shop_03.prefab", mapRoot.transform,
            new Vector3(12f, 0f, 12f), Quaternion.Euler(0f, 180f, 0f));
        store.name = "ConvenienceStore";

        // Shop cover/awning over the front
        PlacePrefab(BLD + "SM_Bld_Shop_Cover_03.prefab", mapRoot.transform,
            new Vector3(12f, 3.05f, 7.8f), Quaternion.Euler(0f, 180f, 0f));

        // Shop sign
        PlacePrefab(PROP + "SM_Prop_Sign_Cafe_01.prefab", mapRoot.transform,
            new Vector3(12f, 4.2f, 7.5f), Quaternion.Euler(0f, 180f, 0f));

        // ATM next to store
        PlacePrefab(PROP + "SM_Prop_ATM_01.prefab", mapRoot.transform,
            new Vector3(8.5f, 0f, 8f), Quaternion.Euler(0f, 90f, 0f));

        // ── OTHER BUILDINGS (3 remaining corners) ────────────────────────────
        // SW corner — apartment
        PlacePrefab(BLD + "SM_Bld_Apartment_01.prefab", mapRoot.transform,
            new Vector3(-12f, 0f, 12f), Quaternion.Euler(0f, 90f, 0f));
        PlacePrefab(BLD + "SM_Bld_Apartment_Roof_01.prefab", mapRoot.transform,
            new Vector3(-12f, 3f, 12f), Quaternion.Euler(0f, 90f, 0f));

        // SE corner — office small
        PlacePrefab(BLD + "SM_Bld_OfficeOld_Small_01.prefab", mapRoot.transform,
            new Vector3(12f, 0f, -12f), Quaternion.Euler(0f, 0f, 0f));

        // NW corner — apartment stack
        PlacePrefab(BLD + "SM_Bld_Apartment_Stack_01.prefab", mapRoot.transform,
            new Vector3(-12f, 0f, -12f), Quaternion.Euler(0f, 270f, 0f));

        // ── VEHICLES ─────────────────────────────────────────────────────────
        // Parked sedan on horizontal road (west side)
        PlacePrefab(VEH + "SM_Veh_Car_Sedan_01.prefab", mapRoot.transform,
            new Vector3(-8f, 0f, 3.2f), Quaternion.Euler(0f, 90f, 0f));

        // Van parked on east side
        PlacePrefab(VEH + "SM_Veh_Car_Van_01.prefab", mapRoot.transform,
            new Vector3(8f, 0f, -3.2f), Quaternion.Euler(0f, 270f, 0f));

        // Muscle car on vertical road south
        PlacePrefab(VEH + "SM_Veh_Car_Muscle_01.prefab", mapRoot.transform,
            new Vector3(-3.2f, 0f, -8f), Quaternion.Euler(0f, 180f, 0f));

        // ── PROPS ────────────────────────────────────────────────────────────
        // Light poles at intersection corners
        PlacePrefab(PROP + "SM_Prop_LightPole_Base_01.prefab", mapRoot.transform,
            new Vector3(-5.5f, 0f, 5.5f), Quaternion.Euler(0f, 225f, 0f));
        PlacePrefab(PROP + "SM_Prop_LightPole_Base_01.prefab", mapRoot.transform,
            new Vector3(5.5f, 0f, 5.5f), Quaternion.Euler(0f, 315f, 0f));
        PlacePrefab(PROP + "SM_Prop_LightPole_Base_01.prefab", mapRoot.transform,
            new Vector3(-5.5f, 0f, -5.5f), Quaternion.Euler(0f, 135f, 0f));
        PlacePrefab(PROP + "SM_Prop_LightPole_Base_01.prefab", mapRoot.transform,
            new Vector3(5.5f, 0f, -5.5f), Quaternion.Euler(0f, 45f, 0f));

        // Trash cans near store
        PlacePrefab(PROP + "SM_Prop_Trashbin_01.prefab", mapRoot.transform,
            new Vector3(7.5f, 0f, 6.5f), Quaternion.Euler(0f, 45f, 0f));
        PlacePrefab(PROP + "SM_Prop_TrashBag_01.prefab", mapRoot.transform,
            new Vector3(7.8f, 0f, 7f), Quaternion.identity);

        // Traffic lights
        PlacePrefab(PROP + "SM_Prop_TrafficLight_01.prefab", mapRoot.transform,
            new Vector3(-5.5f, 0f, 5.5f), Quaternion.Euler(0f, 45f, 0f));
        PlacePrefab(PROP + "SM_Prop_TrafficLight_01.prefab", mapRoot.transform,
            new Vector3(5.5f, 0f, -5.5f), Quaternion.Euler(0f, 225f, 0f));

        // Trees on sidewalk corners
        PlacePrefab(ENV + "SM_Env_Tree_01.prefab", mapRoot.transform,
            new Vector3(-8f, 0f, 7f), Quaternion.identity);
        PlacePrefab(ENV + "SM_Env_Tree_02.prefab", mapRoot.transform,
            new Vector3(8f, 0f, 7f), Quaternion.identity);
        PlacePrefab(ENV + "SM_Env_Tree_01.prefab", mapRoot.transform,
            new Vector3(-8f, 0f, -7f), Quaternion.identity);

        // Bus stop
        PlacePrefab(PROP + "SM_Prop_BusStop_01.prefab", mapRoot.transform,
            new Vector3(-7f, 0f, 5.5f), Quaternion.Euler(0f, 90f, 0f));

        // Hotdog stand near store
        PlacePrefab(PROP + "SM_Prop_HotdogStand_01.prefab", mapRoot.transform,
            new Vector3(9f, 0f, 6.5f), Quaternion.Euler(0f, 180f, 0f));

        // Cardboard boxes near store entrance
        PlacePrefab(PROP + "SM_Prop_CardboardBox_01.prefab", mapRoot.transform,
            new Vector3(10f, 0f, 7.5f), Quaternion.Euler(0f, 30f, 0f));
        PlacePrefab(PROP + "SM_Prop_CardboardBox_02.prefab", mapRoot.transform,
            new Vector3(10.5f, 0.4f, 7.5f), Quaternion.Euler(0f, 15f, 0f));

        // Cones on road
        PlacePrefab(PROP + "SM_Prop_Cone_01.prefab", mapRoot.transform,
            new Vector3(-3f, 0f, 2f), Quaternion.identity);
        PlacePrefab(PROP + "SM_Prop_Cone_01.prefab", mapRoot.transform,
            new Vector3(-4f, 0f, 2f), Quaternion.identity);

        // ── PLAYER ───────────────────────────────────────────────────────────
        GameObject player = BuildPlayer(scene);

        // ── CAMERA ───────────────────────────────────────────────────────────
        GameObject camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.fieldOfView = 55f;
        cam.backgroundColor = new Color(0.4f, 0.45f, 0.5f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.AddComponent<AudioListener>();
        camGO.AddComponent<UniversalAdditionalCameraData>();
        var follow = camGO.AddComponent<GameCameraFollow>();
        follow.target = player.transform;
        follow.offset = new Vector3(0f, 13f, -9f);
        follow.smoothSpeed = 8f;
        camGO.transform.position = player.transform.position + new Vector3(0f, 13f, -9f);
        camGO.transform.LookAt(player.transform.position + Vector3.up);
        SceneManager.MoveGameObjectToScene(camGO, scene);

        // ── RAIN ─────────────────────────────────────────────────────────────
        BuildRain(scene, player.transform);

        // ── HUD ──────────────────────────────────────────────────────────────
        BuildHUD(scene, player);

        // ── EVENT SYSTEM ─────────────────────────────────────────────────────
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        SceneManager.MoveGameObjectToScene(es, scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[BuildGameScene] Done!");
    }

    // ── PLAYER ───────────────────────────────────────────────────────────────
    static GameObject BuildPlayer(Scene scene)
    {
        GameObject root = new GameObject("Player");
        SceneManager.MoveGameObjectToScene(root, scene);
        root.transform.position = new Vector3(0f, 0f, 0f);

        var cc = root.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        root.AddComponent<GamePlayerMovement>();

        // Use Male Hoodie as visual base (closest to casual street look)
        var charPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            CHAR + "Character_Male_Hoodie.prefab");
        if (charPrefab != null)
        {
            var charInst = (GameObject)PrefabUtility.InstantiatePrefab(charPrefab);
            charInst.transform.SetParent(root.transform, false);
            charInst.transform.localPosition = Vector3.zero;
            charInst.transform.localRotation = Quaternion.identity;
            charInst.name = "Visual";

            // Recolor to look like dirty casual clothes
            RecolorCharacter(charInst);
        }
        else
        {
            // Fallback: simple capsule
            var fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fallback.name = "Visual";
            fallback.transform.SetParent(root.transform, false);
            fallback.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            GameObject.DestroyImmediate(fallback.GetComponent<Collider>());
        }

        return root;
    }

    static void RecolorCharacter(GameObject charGO)
    {
        // Find all renderers and recolor materials to look like dirty casual wear
        var renderers = charGO.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                string matName = mats[i].name.ToLower();
                // Create instance to avoid modifying shared asset
                var matInst = new Material(mats[i]);
                if (matName.Contains("top") || matName.Contains("shirt") ||
                    matName.Contains("torso") || matName.Contains("body") ||
                    matName.Contains("hoodie") || matName.Contains("jacket"))
                {
                    matInst.color = new Color(0.38f, 0.30f, 0.20f); // dirty brown
                }
                else if (matName.Contains("pant") || matName.Contains("leg") ||
                         matName.Contains("short") || matName.Contains("bottom"))
                {
                    matInst.color = new Color(0.22f, 0.28f, 0.42f); // dark blue shorts
                }
                else if (matName.Contains("shoe") || matName.Contains("boot") ||
                         matName.Contains("foot"))
                {
                    matInst.color = new Color(0.18f, 0.15f, 0.12f); // dark slipper
                }
                mats[i] = matInst;
            }
            r.sharedMaterials = mats;
        }
    }

    // ── RAIN ─────────────────────────────────────────────────────────────────
    static void BuildRain(Scene scene, Transform playerTransform)
    {
        GameObject rainGO = new GameObject("Rain");
        SceneManager.MoveGameObjectToScene(rainGO, scene);
        var ps = rainGO.AddComponent<ParticleSystem>();
        rainGO.transform.position = new Vector3(0f, 15f, 0f);

        var main = ps.main;
        main.startLifetime = 1.4f;
        main.startSpeed = 20f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.07f);
        main.startColor = new Color(0.65f, 0.72f, 0.85f, 0.55f);
        main.maxParticles = 4000;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.6f;

        var emission = ps.emission;
        emission.rateOverTime = 800f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(70f, 1f, 70f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);
        vel.y = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.z = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);

        var renderer = rainGO.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale = 0.06f;
        renderer.lengthScale = 2.5f;

        Material rainMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        if (rainMat == null || rainMat.shader.name == "Hidden/InternalErrorShader")
            rainMat = new Material(Shader.Find("Particles/Standard Unlit"));
        rainMat.color = new Color(0.65f, 0.75f, 0.9f, 0.5f);
        renderer.material = rainMat;

        var follower = rainGO.AddComponent<RainFollower>();
        follower.target = playerTransform;
        follower.height = 15f;
    }

    // ── HUD ──────────────────────────────────────────────────────────────────
    static void BuildHUD(Scene scene, GameObject player)
    {
        GameObject canvasGO = new GameObject("HUDCanvas");
        SceneManager.MoveGameObjectToScene(canvasGO, scene);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Bottom bar background
        GameObject bottomBar = CreateUIObj("BottomBar", canvasGO.transform);
        SetAnchors(bottomBar, 0.25f, 0f, 0.75f, 0f, -10f, 6f, 10f, 140f);
        var bbImg = bottomBar.AddComponent<Image>();
        bbImg.color = new Color(0f, 0f, 0f, 0.70f);

        // 4 Inventory Slots
        var slotImages = new Image[4];
        var slotLabels = new TextMeshProUGUI[4];
        float slotSize = 72f;
        float slotSpacing = 10f;
        float totalW = 4 * slotSize + 3 * slotSpacing;
        float startX = -totalW / 2f + slotSize / 2f;

        for (int i = 0; i < 4; i++)
        {
            GameObject slot = CreateUIObj($"Slot_{i}", bottomBar.transform);
            var slotRT = slot.GetComponent<RectTransform>();
            slotRT.anchorMin = new Vector2(0.5f, 0f);
            slotRT.anchorMax = new Vector2(0.5f, 0f);
            slotRT.pivot = new Vector2(0.5f, 0f);
            slotRT.anchoredPosition = new Vector2(startX + i * (slotSize + slotSpacing), 48f);
            slotRT.sizeDelta = new Vector2(slotSize, slotSize);

            var slotImg = slot.AddComponent<Image>();
            slotImg.color = new Color(0.18f, 0.65f, 0.70f, 0.9f);
            slotImages[i] = slotImg;

            GameObject inner = CreateUIObj("Inner", slot.transform);
            SetAnchors(inner, 0f, 0f, 1f, 1f, 2f, 2f, -2f, -2f);
            inner.AddComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.95f);

            GameObject lbl = CreateUIObj("Label", slot.transform);
            SetAnchors(lbl, 0f, 0f, 1f, 1f, 2f, 2f, -2f, -2f);
            var lTMP = lbl.AddComponent<TextMeshProUGUI>();
            lTMP.text = "";
            lTMP.fontSize = 11;
            lTMP.color = Color.white;
            lTMP.alignment = TextAlignmentOptions.Center;
            slotLabels[i] = lTMP;
        }

        // HP Label
        GameObject hpLabel = CreateUIObj("HPLabel", bottomBar.transform);
        var hpLabelRT = hpLabel.GetComponent<RectTransform>();
        hpLabelRT.anchorMin = new Vector2(0.5f, 0f);
        hpLabelRT.anchorMax = new Vector2(0.5f, 0f);
        hpLabelRT.pivot = new Vector2(0.5f, 0f);
        hpLabelRT.anchoredPosition = new Vector2(-130f, 16f);
        hpLabelRT.sizeDelta = new Vector2(50f, 22f);
        var hpLabelTMP = hpLabel.AddComponent<TextMeshProUGUI>();
        hpLabelTMP.text = "HP";
        hpLabelTMP.fontSize = 14;
        hpLabelTMP.fontStyle = FontStyles.Bold;
        hpLabelTMP.color = new Color(0.9f, 0.25f, 0.25f, 1f);
        hpLabelTMP.alignment = TextAlignmentOptions.Right;

        // Health bar BG
        GameObject hpBG = CreateUIObj("HealthBarBG", bottomBar.transform);
        var hpBGRT = hpBG.GetComponent<RectTransform>();
        hpBGRT.anchorMin = new Vector2(0.5f, 0f);
        hpBGRT.anchorMax = new Vector2(0.5f, 0f);
        hpBGRT.pivot = new Vector2(0.5f, 0f);
        hpBGRT.anchoredPosition = new Vector2(20f, 16f);
        hpBGRT.sizeDelta = new Vector2(240f, 22f);
        hpBG.AddComponent<Image>().color = new Color(0.1f, 0.05f, 0.05f, 0.9f);

        // Health bar fill
        GameObject hpFill = CreateUIObj("HealthBarFill", hpBG.transform);
        SetAnchors(hpFill, 0f, 0f, 1f, 1f, 1f, 1f, -1f, -1f);
        var hpFillImg = hpFill.AddComponent<Image>();
        hpFillImg.color = new Color(0.85f, 0.15f, 0.15f, 1f);
        hpFillImg.type = Image.Type.Filled;
        hpFillImg.fillMethod = Image.FillMethod.Horizontal;
        hpFillImg.fillAmount = 1f;

        // HP text
        GameObject hpText = CreateUIObj("HPText", hpBG.transform);
        SetAnchors(hpText, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);
        var hpTMP = hpText.AddComponent<TextMeshProUGUI>();
        hpTMP.text = "100 / 100";
        hpTMP.fontSize = 11;
        hpTMP.color = Color.white;
        hpTMP.alignment = TextAlignmentOptions.Center;

        // Wire HUD
        var hud = canvasGO.AddComponent<GameHUD>();
        hud.player = player.GetComponent<GamePlayerMovement>();
        hud.healthBarFill = hpFillImg;
        hud.healthText = hpTMP;
        hud.inventorySlots = slotImages;
        hud.inventoryLabels = slotLabels;
    }

    // ── HELPERS ──────────────────────────────────────────────────────────────
    static GameObject PlacePrefab(string path, Transform parent, Vector3 pos, Quaternion rot)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning($"Prefab not found: {path}");
            return new GameObject("Missing_" + System.IO.Path.GetFileNameWithoutExtension(path));
        }
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        inst.transform.SetParent(parent, false);
        inst.transform.localPosition = pos;
        inst.transform.localRotation = rot;
        return inst;
    }

    static GameObject CreateUIObj(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void SetAnchors(GameObject go, float ax, float ay, float bx, float by,
                           float left, float bottom, float right, float top)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax, ay);
        rt.anchorMax = new Vector2(bx, by);
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(right, top);
    }
}
