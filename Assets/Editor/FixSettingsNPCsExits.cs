using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 1. Builds the Settings panel UI (sliders + labels matching the reference image)
/// 2. Adjusts the scene-view camera to show the gas station
/// 3. Re-adds 3 NPC Bandits with FSM near the gas station
/// 4. Re-adds 2 Exit Points in the scene
/// </summary>
public class FixSettingsNPCsExits
{
    [MenuItem("Tools/Fix Settings + NPCs + Exits")]
    public static void Run()
    {
        BuildSettingsPanel();
        RepositionSceneCamera();
        AddNPCs();
        AddExitPoints();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("[FixSettingsNPCsExits] All done and scene saved.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  1. SETTINGS PANEL
    // ─────────────────────────────────────────────────────────────────────────
    static void BuildSettingsPanel()
    {
        GameObject pauseCanvas = GameObject.Find("PauseCanvas");
        if (pauseCanvas == null) { Debug.LogError("PauseCanvas not found!"); return; }

        PauseMenuManager pmm = pauseCanvas.GetComponent<PauseMenuManager>();
        SettingsManager sm = Object.FindFirstObjectByType<SettingsManager>();

        Transform settingsPanel = pauseCanvas.transform.Find("SettingsPanel");
        if (settingsPanel == null) { Debug.LogError("SettingsPanel not found!"); return; }

        // ── Resize panel ──────────────────────────────────────────────────────
        RectTransform spRT = settingsPanel.GetComponent<RectTransform>();
        spRT.sizeDelta = new Vector2(480f, 380f);
        spRT.anchoredPosition = Vector2.zero;

        // Dark background
        Image spImg = settingsPanel.GetComponent<Image>();
        if (spImg != null) spImg.color = new Color(0.04f, 0.04f, 0.12f, 0.97f);

        // ── Clear old children except Title & BackBtn ─────────────────────────
        var toDestroy = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in settingsPanel)
        {
            string n = child.name;
            if (n != "Title" && n != "BackBtn")
                toDestroy.Add(child.gameObject);
        }
        foreach (var go in toDestroy) Object.DestroyImmediate(go);

        // ── Title ─────────────────────────────────────────────────────────────
        Transform titleT = settingsPanel.Find("Title");
        if (titleT != null)
        {
            RectTransform tRT = titleT.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 0.5f);
            tRT.anchorMax = new Vector2(0.5f, 0.5f);
            tRT.pivot = new Vector2(0.5f, 0.5f);
            tRT.anchoredPosition = new Vector2(0f, 165f);
            tRT.sizeDelta = new Vector2(400f, 55f);

            TextMeshProUGUI titleTMP = titleT.GetComponent<TextMeshProUGUI>();
            if (titleTMP != null)
            {
                titleTMP.text = "SETTINGS";
                titleTMP.fontSize = 36f;
                titleTMP.fontStyle = FontStyles.Bold;
                titleTMP.alignment = TextAlignmentOptions.Center;
                titleTMP.color = new Color(0.2f, 0.85f, 1f, 1f); // cyan like reference
            }
        }

        // ── Divider line under title ──────────────────────────────────────────
        CreateDivider(settingsPanel, new Vector2(0f, 135f), new Vector2(420f, 2f));

        // ── Three slider rows ─────────────────────────────────────────────────
        // Row positions (from top)
        float[] rowY = { 75f, 10f, -55f };
        string[] rowLabels = { "Mouse Sensitivity", "Music Volume", "Effects Volume" };
        string[] sliderNames = { "SensitivitySlider", "MusicSlider", "EffectsSlider" };
        string[] valueLabelNames = { "SensLabel", "MusicLabel", "EffectsLabel" };

        Slider sensitivitySlider = null, musicSlider = null, effectsSlider = null;
        TextMeshProUGUI sensLabel = null, musicLabel = null, effectsLabel = null;

        for (int i = 0; i < 3; i++)
        {
            // Row container
            GameObject row = new GameObject("Row_" + rowLabels[i]);
            row.transform.SetParent(settingsPanel, false);
            RectTransform rowRT = row.AddComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0.5f, 0.5f);
            rowRT.anchorMax = new Vector2(0.5f, 0.5f);
            rowRT.pivot = new Vector2(0.5f, 0.5f);
            rowRT.sizeDelta = new Vector2(440f, 40f);
            rowRT.anchoredPosition = new Vector2(0f, rowY[i]);

            // Label (left side)
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(row.transform, false);
            RectTransform labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0f, 0f);
            labelRT.anchorMax = new Vector2(0f, 1f);
            labelRT.pivot = new Vector2(0f, 0.5f);
            labelRT.anchoredPosition = new Vector2(0f, 0f);
            labelRT.sizeDelta = new Vector2(160f, 0f);
            TextMeshProUGUI labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text = rowLabels[i];
            labelTMP.fontSize = 16f;
            labelTMP.color = Color.white;
            labelTMP.alignment = TextAlignmentOptions.MidlineLeft;

            // Slider (middle)
            GameObject sliderGO = CreateSlider(row.transform, sliderNames[i],
                new Vector2(160f, 0f), new Vector2(220f, 0f), new Vector2(220f, 20f));
            Slider slider = sliderGO.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = i == 0 ? 1f : 0.8f;

            // Style the slider fill to cyan
            StyleSlider(slider);

            // Value label (right side)
            GameObject valGO = new GameObject(valueLabelNames[i]);
            valGO.transform.SetParent(row.transform, false);
            RectTransform valRT = valGO.AddComponent<RectTransform>();
            valRT.anchorMin = new Vector2(1f, 0f);
            valRT.anchorMax = new Vector2(1f, 1f);
            valRT.pivot = new Vector2(1f, 0.5f);
            valRT.anchoredPosition = new Vector2(0f, 0f);
            valRT.sizeDelta = new Vector2(55f, 0f);
            TextMeshProUGUI valTMP = valGO.AddComponent<TextMeshProUGUI>();
            valTMP.fontSize = 15f;
            valTMP.color = Color.white;
            valTMP.alignment = TextAlignmentOptions.MidlineRight;
            valTMP.text = i == 0 ? "100%" : "80%";

            // Store references
            if (i == 0) { sensitivitySlider = slider; sensLabel = valTMP; }
            else if (i == 1) { musicSlider = slider; musicLabel = valTMP; }
            else { effectsSlider = slider; effectsLabel = valTMP; }

            // Wire slider to update label live
            int capturedI = i;
            TextMeshProUGUI capturedLabel = valTMP;
            slider.onValueChanged.AddListener((v) =>
            {
                capturedLabel.text = Mathf.RoundToInt(v * 100) + "%";
            });
        }

        // ── Divider above Close button ────────────────────────────────────────
        CreateDivider(settingsPanel, new Vector2(0f, -100f), new Vector2(420f, 2f));

        // ── Close / Back button ───────────────────────────────────────────────
        Transform backBtn = settingsPanel.Find("BackBtn");
        if (backBtn != null)
        {
            RectTransform btnRT = backBtn.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.5f, 0.5f);
            btnRT.anchorMax = new Vector2(0.5f, 0.5f);
            btnRT.pivot = new Vector2(0.5f, 0.5f);
            btnRT.anchoredPosition = new Vector2(0f, -150f);
            btnRT.sizeDelta = new Vector2(180f, 44f);

            // Style button dark with white border look
            Image btnImg = backBtn.GetComponent<Image>();
            if (btnImg != null) btnImg.color = new Color(0.1f, 0.1f, 0.2f, 1f);

            Transform labelT = backBtn.Find("Label");
            if (labelT != null)
            {
                TextMeshProUGUI lTMP = labelT.GetComponent<TextMeshProUGUI>();
                if (lTMP != null)
                {
                    lTMP.text = "CLOSE";
                    lTMP.fontSize = 18f;
                    lTMP.fontStyle = FontStyles.Bold;
                    lTMP.color = Color.white;
                    lTMP.alignment = TextAlignmentOptions.Center;
                }
                RectTransform lRT = labelT.GetComponent<RectTransform>();
                if (lRT != null)
                {
                    lRT.anchorMin = Vector2.zero;
                    lRT.anchorMax = Vector2.one;
                    lRT.offsetMin = Vector2.zero;
                    lRT.offsetMax = Vector2.zero;
                }
            }

            // Wire Close button to PauseMenuManager.CloseSettings
            if (pmm != null)
            {
                Button btn = backBtn.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    for (int i = btn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                        UnityEventTools.RemovePersistentListener(btn.onClick, i);
                    var method = typeof(PauseMenuManager).GetMethod("CloseSettings");
                    if (method != null)
                    {
                        UnityAction action = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), pmm, method);
                        UnityEventTools.AddPersistentListener(btn.onClick, action);
                    }
                    Image img = backBtn.GetComponent<Image>();
                    if (img != null) btn.targetGraphic = img;
                    EditorUtility.SetDirty(btn);
                }
            }
        }

        // ── Wire SettingsManager if present ──────────────────────────────────
        if (sm != null)
        {
            sm.sensitivitySlider = sensitivitySlider;
            sm.musicVolumeSlider = musicSlider;
            sm.effectsVolumeSlider = effectsSlider;
            sm.sensitivityLabel = sensLabel;
            sm.musicLabel = musicLabel;
            sm.effectsLabel = effectsLabel;

            // Wire slider onValueChanged to SettingsManager
            if (sensitivitySlider != null)
            {
                sensitivitySlider.onValueChanged.RemoveAllListeners();
                sensitivitySlider.onValueChanged.AddListener(sm.OnSensitivityChanged);
                sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1f);
            }
            if (musicSlider != null)
            {
                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.onValueChanged.AddListener(sm.OnMusicVolumeChanged);
                musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            }
            if (effectsSlider != null)
            {
                effectsSlider.onValueChanged.RemoveAllListeners();
                effectsSlider.onValueChanged.AddListener(sm.OnEffectsVolumeChanged);
                effectsSlider.value = PlayerPrefs.GetFloat("EffectsVolume", 0.8f);
            }
            EditorUtility.SetDirty(sm);
        }

        EditorUtility.SetDirty(settingsPanel.gameObject);
        Debug.Log("[FixSettingsNPCsExits] Settings panel built.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HELPERS – UI
    // ─────────────────────────────────────────────────────────────────────────
    static void CreateDivider(Transform parent, Vector2 pos, Vector2 size)
    {
        GameObject div = new GameObject("Divider");
        div.transform.SetParent(parent, false);
        RectTransform rt = div.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image img = div.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.4f, 0.6f);
    }

    static GameObject CreateSlider(Transform parent, string name, Vector2 anchorPos, Vector2 anchorOffset, Vector2 size)
    {
        GameObject sliderGO = new GameObject(name);
        sliderGO.transform.SetParent(parent, false);
        RectTransform rt = sliderGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = anchorPos;
        rt.sizeDelta = size;

        Slider slider = sliderGO.AddComponent<Slider>();

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0.25f);
        bgRT.anchorMax = new Vector2(1f, 0.75f);
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.25f, 1f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform faRT = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0f, 0.25f);
        faRT.anchorMax = new Vector2(1f, 0.75f);
        faRT.offsetMin = new Vector2(5f, 0f);
        faRT.offsetMax = new Vector2(-15f, 0f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(0f, 1f);
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = new Vector2(10f, 0f);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.1f, 0.8f, 1f, 1f); // cyan fill

        // Handle Slide Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderGO.transform, false);
        RectTransform haRT = handleArea.AddComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero;
        haRT.anchorMax = Vector2.one;
        haRT.offsetMin = new Vector2(10f, 0f);
        haRT.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRT = handle.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20f, 20f);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        // Wire slider
        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;

        return sliderGO;
    }

    static void StyleSlider(Slider slider)
    {
        // Already styled in CreateSlider, but ensure handle color
        if (slider.targetGraphic != null)
            slider.targetGraphic.color = Color.white;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  2. REPOSITION SCENE-VIEW CAMERA
    // ─────────────────────────────────────────────────────────────────────────
    static void RepositionSceneCamera()
    {
        // Gas station is at ~(42, 0, 11). Position scene view to look at it.
        SceneView sv = SceneView.lastActiveSceneView;
        if (sv == null) sv = SceneView.sceneViews.Count > 0 ? (SceneView)SceneView.sceneViews[0] : null;
        if (sv == null) { Debug.LogWarning("No SceneView found."); return; }

        // Look at gas station from a nice angle
        sv.pivot = new Vector3(42f, 0f, 11f);
        sv.rotation = Quaternion.Euler(35f, 200f, 0f);
        sv.size = 25f;
        sv.Repaint();
        Debug.Log("[FixSettingsNPCsExits] Scene camera repositioned to gas station.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  3. ADD NPCs WITH FSM
    // ─────────────────────────────────────────────────────────────────────────
    static void AddNPCs()
    {
        // Remove any existing NPCs first
        var existing = Object.FindObjectsByType<NPCStateMachine>(FindObjectsSortMode.None);
        foreach (var e in existing) Object.DestroyImmediate(e.gameObject);

        // Gas station center ~(42, 0, 11)
        // Place 3 bandits patrolling around the gas station
        Vector3[] npcPositions = {
            new Vector3(35f, 1f, 8f),
            new Vector3(50f, 1f, 8f),
            new Vector3(42f, 1f, 20f),
        };

        // Patrol waypoints for each NPC (a small square around their start)
        Vector3[][] waypointSets = {
            new Vector3[] {
                new Vector3(32f, 1f, 5f),
                new Vector3(38f, 1f, 5f),
                new Vector3(38f, 1f, 14f),
                new Vector3(32f, 1f, 14f),
            },
            new Vector3[] {
                new Vector3(47f, 1f, 5f),
                new Vector3(55f, 1f, 5f),
                new Vector3(55f, 1f, 14f),
                new Vector3(47f, 1f, 14f),
            },
            new Vector3[] {
                new Vector3(38f, 1f, 17f),
                new Vector3(46f, 1f, 17f),
                new Vector3(46f, 1f, 24f),
                new Vector3(38f, 1f, 24f),
            },
        };

        string banditPrefabPath = "Assets/BasicBandit/Basic_BanditPrefab.prefab";
        GameObject banditPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(banditPrefabPath);

        // Create an NPCs parent object
        GameObject npcsParent = GameObject.Find("NPCs");
        if (npcsParent == null)
        {
            npcsParent = new GameObject("NPCs");
        }

        for (int i = 0; i < npcPositions.Length; i++)
        {
            GameObject npc;
            if (banditPrefab != null)
            {
                npc = (GameObject)PrefabUtility.InstantiatePrefab(banditPrefab);
                npc.name = "NPC_Bandit_" + (i + 1);
            }
            else
            {
                // Fallback: capsule
                npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                npc.name = "NPC_Bandit_" + (i + 1);
            }

            npc.transform.SetParent(npcsParent.transform);
            npc.transform.position = npcPositions[i];
            npc.transform.rotation = Quaternion.identity;

            // Add NavMeshAgent if missing
            UnityEngine.AI.NavMeshAgent agent = npc.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent == null) agent = npc.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.speed = 2f;
            agent.stoppingDistance = 0.5f;
            agent.radius = 0.3f;
            agent.height = 1.8f;

            // Add Animator if missing
            Animator anim = npc.GetComponent<Animator>();
            if (anim == null) anim = npc.AddComponent<Animator>();

            // Load the bandit controller if available
            RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/BasicBandit/NPCBanditController.controller");
            if (ctrl != null) anim.runtimeAnimatorController = ctrl;

            // Add NPCStateMachine
            NPCStateMachine fsm = npc.GetComponent<NPCStateMachine>();
            if (fsm == null) fsm = npc.AddComponent<NPCStateMachine>();

            // Create waypoint objects
            GameObject wpParent = new GameObject("Waypoints_" + (i + 1));
            wpParent.transform.SetParent(npcsParent.transform);
            Transform[] waypoints = new Transform[waypointSets[i].Length];
            for (int w = 0; w < waypointSets[i].Length; w++)
            {
                GameObject wp = new GameObject("WP_" + w);
                wp.transform.SetParent(wpParent.transform);
                wp.transform.position = waypointSets[i][w];
                waypoints[w] = wp.transform;
            }
            fsm.patrolWaypoints = waypoints;
            fsm.patrolSpeed = 2f;
            fsm.chaseSpeed = 5f;
            fsm.sightRange = 14f;
            fsm.sightAngle = 110f;
            fsm.attackRange = 2.2f;
            fsm.attackDamage = 10f;

            EditorUtility.SetDirty(npc);
            Debug.Log($"[FixSettingsNPCsExits] Added NPC_Bandit_{i + 1} at {npcPositions[i]}");
        }

        // Add NoiseSystem if missing
        if (NoiseSystem.Instance == null && Object.FindFirstObjectByType<NoiseSystem>() == null)
        {
            GameObject noiseSys = new GameObject("NoiseSystem");
            noiseSys.AddComponent<NoiseSystem>();
            Debug.Log("[FixSettingsNPCsExits] Added NoiseSystem.");
        }

        Debug.Log("[FixSettingsNPCsExits] NPCs added.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  4. ADD EXIT POINTS
    // ─────────────────────────────────────────────────────────────────────────
    static void AddExitPoints()
    {
        // Remove existing exit points
        var existing = Object.FindObjectsByType<ExitPoint>(FindObjectsSortMode.None);
        foreach (var e in existing) Object.DestroyImmediate(e.gameObject);

        // Exit point 1: near the gas station entrance
        // Exit point 2: on the opposite side of the map
        Vector3[] exitPositions = {
            new Vector3(42f, 1f, -5f),   // south of gas station
            new Vector3(-20f, 1f, -20f), // far corner
        };
        string[] exitScenes = { "Level2Scene", "Level2Scene" };
        Color exitColor = new Color(0f, 1f, 0.4f, 1f);

        GameObject exitsParent = GameObject.Find("ExitPoints");
        if (exitsParent == null) exitsParent = new GameObject("ExitPoints");

        for (int i = 0; i < exitPositions.Length; i++)
        {
            GameObject exitGO = new GameObject("ExitPoint_" + (i + 1));
            exitGO.transform.SetParent(exitsParent.transform);
            exitGO.transform.position = exitPositions[i];
            exitGO.transform.localScale = new Vector3(3f, 2f, 3f);

            // Trigger collider
            BoxCollider col = exitGO.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = Vector3.one;

            // Exit point script
            ExitPoint ep = exitGO.AddComponent<ExitPoint>();
            ep.nextSceneName = exitScenes[i];
            ep.holdDuration = 2f;

            // Visual: a glowing pillar (use a cylinder)
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "ExitPillar";
            pillar.transform.SetParent(exitGO.transform, false);
            pillar.transform.localPosition = Vector3.zero;
            pillar.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);
            Object.DestroyImmediate(pillar.GetComponent<CapsuleCollider>());

            // Green material
            Material exitMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/ExitPillarMaterial.mat");
            if (exitMat == null)
            {
                exitMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                exitMat.color = exitColor;
                AssetDatabase.CreateAsset(exitMat, "Assets/ExitPillarMaterial.mat");
            }
            pillar.GetComponent<MeshRenderer>().material = exitMat;

            // Point light
            GameObject lightGO = new GameObject("ExitLight");
            lightGO.transform.SetParent(exitGO.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 1f, 0f);
            Light lt = lightGO.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = exitColor;
            lt.intensity = 2f;
            lt.range = 6f;

            EditorUtility.SetDirty(exitGO);
            Debug.Log($"[FixSettingsNPCsExits] Added ExitPoint_{i + 1} at {exitPositions[i]}");
        }

        Debug.Log("[FixSettingsNPCsExits] Exit points added.");
    }
}
