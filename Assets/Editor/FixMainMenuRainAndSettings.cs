using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Editor script that:
/// 1. Moves the rain particle system inside the Canvas so it renders on top of the background.
/// 2. Adds a lightning flash overlay + LightningEffect component.
/// 3. Builds a full Settings panel with Sensitivity, Music Volume, and Effects Volume sliders.
/// 4. Wires the Settings button to open the panel.
/// </summary>
public class FixMainMenuRainAndSettings : MonoBehaviour
{
    [MenuItem("Tools/Fix Main Menu - Rain + Lightning + Settings")]
    public static void Execute()
    {
        // ── Find scene objects ─────────────────────────────────────────────
        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) { Debug.LogError("Canvas not found!"); return; }

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        GameObject particleGO = GameObject.Find("Particle System");

        // ── 1. Fix Rain Particle System ────────────────────────────────────
        FixRainParticleSystem(canvasGO, canvas, particleGO);

        // ── 2. Add Lightning Flash Overlay ─────────────────────────────────
        GameObject lightningFlash = AddLightningFlash(canvasGO);

        // ── 3. Build Settings Panel ────────────────────────────────────────
        GameObject settingsPanel = BuildSettingsPanel(canvasGO);

        // ── 4. Wire Settings Button ────────────────────────────────────────
        WireSettingsButton(canvasGO, settingsPanel);

        // ── 5. Wire MainMenuManager ────────────────────────────────────────
        MainMenuManager mmm = canvasGO.GetComponent<MainMenuManager>();
        if (mmm != null)
        {
            mmm.settingsPanel = settingsPanel;
            EditorUtility.SetDirty(canvasGO);
        }

        // ── Save ───────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("✅ Main Menu fixed: Rain on top, Lightning added, Settings panel built.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Rain Fix
    // ─────────────────────────────────────────────────────────────────────────
    static void FixRainParticleSystem(GameObject canvasGO, Canvas canvas, GameObject particleGO)
    {
        // Create a new rain particle system inside the Canvas
        // First, check if one already exists inside canvas
        Transform existingRain = canvasGO.transform.Find("RainParticles");
        if (existingRain != null)
        {
            Debug.Log("RainParticles already exists inside Canvas.");
            return;
        }

        // Create a new GameObject inside the Canvas for rain
        GameObject rainGO = new GameObject("RainParticles");
        rainGO.transform.SetParent(canvasGO.transform, false);

        // Position it just above the background (low sibling index = behind other UI)
        rainGO.transform.SetSiblingIndex(1); // right after Background

        // Add RectTransform and stretch to fill
        RectTransform rt = rainGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Add ParticleSystem
        ParticleSystem ps = rainGO.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psr = rainGO.GetComponent<ParticleSystemRenderer>();

        // Configure the particle system for rain
        var main = ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(800f, 1200f);
        main.startSize = new ParticleSystem.MinMaxCurve(1f, 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.7f, 0.85f, 1f, 0.5f),
            new Color(0.6f, 0.75f, 0.95f, 0.3f));
        main.maxParticles = 1000;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        // Emission
        var emission = ps.emission;
        emission.rateOverTime = 300f;

        // Shape: box spanning the screen
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(2000f, 1f, 1f);
        shape.position = new Vector3(0f, 600f, 0f);
        shape.rotation = new Vector3(0f, 0f, 0f);

        // Velocity over lifetime — rain falls downward
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.y = new ParticleSystem.MinMaxCurve(-1200f, -900f);
        vel.x = new ParticleSystem.MinMaxCurve(-50f, -20f); // slight angle

        // Renderer — use the default particle material, render in UI space
        psr.renderMode = ParticleSystemRenderMode.Stretch;
        psr.velocityScale = 0.05f;
        psr.lengthScale = 3f;
        psr.sortingOrder = 1; // above background

        // Use a simple additive/transparent material
        Material rainMat = new Material(Shader.Find("Particles/Standard Unlit"));
        if (rainMat.shader == null || rainMat.shader.name == "Hidden/InternalErrorShader")
        {
            rainMat = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        }
        if (rainMat.shader == null || rainMat.shader.name == "Hidden/InternalErrorShader")
        {
            rainMat = new Material(Shader.Find("Sprites/Default"));
        }
        rainMat.color = new Color(0.7f, 0.85f, 1f, 0.4f);
        psr.material = rainMat;

        // Disable the old world-space particle system if it exists
        if (particleGO != null)
        {
            particleGO.SetActive(false);
            Debug.Log("Disabled old world-space Particle System.");
        }

        Debug.Log("Rain particle system created inside Canvas.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Lightning Flash
    // ─────────────────────────────────────────────────────────────────────────
    static GameObject AddLightningFlash(GameObject canvasGO)
    {
        // Remove existing if any
        Transform existing = canvasGO.transform.Find("LightningFlash");
        if (existing != null) DestroyImmediate(existing.gameObject);

        // Create full-screen overlay image
        GameObject flashGO = new GameObject("LightningFlash");
        flashGO.transform.SetParent(canvasGO.transform, false);
        flashGO.transform.SetSiblingIndex(2); // above rain, below UI panels

        RectTransform rt = flashGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = flashGO.AddComponent<Image>();
        img.color = new Color(0.9f, 0.95f, 1f, 0f); // starts invisible, slight blue-white tint

        // Add AudioSource for thunder
        AudioSource thunderAudio = flashGO.AddComponent<AudioSource>();
        thunderAudio.playOnAwake = false;
        thunderAudio.spatialBlend = 0f; // 2D sound
        thunderAudio.volume = 0.7f;

        // Add LightningEffect component
        LightningEffect le = flashGO.AddComponent<LightningEffect>();
        le.flashImage = img;
        le.thunderSource = thunderAudio;
        le.minInterval = 4f;
        le.maxInterval = 12f;
        le.fadeDuration = 0.18f;
        le.peakAlpha = 0.55f;
        le.thunderDelay = 0.5f;

        Debug.Log("Lightning flash overlay added.");
        return flashGO;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Settings Panel
    // ─────────────────────────────────────────────────────────────────────────
    static GameObject BuildSettingsPanel(GameObject canvasGO)
    {
        // Remove existing settings panel if any
        Transform existing = canvasGO.transform.Find("SettingsPanel");
        if (existing != null) DestroyImmediate(existing.gameObject);

        // ── Panel root ─────────────────────────────────────────────────────
        GameObject panel = new GameObject("SettingsPanel");
        panel.transform.SetParent(canvasGO.transform, false);

        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(700f, 520f);
        panelRT.anchoredPosition = Vector2.zero;

        // Dark semi-transparent background
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.08f, 0.93f);

        // ── Title ──────────────────────────────────────────────────────────
        GameObject titleGO = CreateTMPText(panel, "SettingsTitle", "SETTINGS",
            new Vector2(0f, 200f), new Vector2(600f, 70f), 42, FontStyles.Bold);
        TextMeshProUGUI titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        titleTMP.color = new Color(0.2f, 0.8f, 0.9f, 1f);
        titleTMP.alignment = TextAlignmentOptions.Center;

        // ── Divider ────────────────────────────────────────────────────────
        CreateDivider(panel, new Vector2(0f, 165f), new Vector2(580f, 2f));

        // ── Sensitivity Row ────────────────────────────────────────────────
        GameObject sensLabel = CreateTMPText(panel, "SensLabel", "Mouse Sensitivity",
            new Vector2(-120f, 100f), new Vector2(280f, 45f), 22, FontStyles.Normal);
        sensLabel.GetComponent<TextMeshProUGUI>().color = Color.white;
        sensLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        GameObject sensSliderGO = CreateSlider(panel, "SensSlider",
            new Vector2(130f, 100f), new Vector2(280f, 30f), 0.1f, 3f, 1f);

        GameObject sensValueLabel = CreateTMPText(panel, "SensValue", "100%",
            new Vector2(310f, 100f), new Vector2(80f, 45f), 20, FontStyles.Normal);
        sensValueLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.8f, 0.9f, 1f);
        sensValueLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // ── Music Volume Row ───────────────────────────────────────────────
        GameObject musicLabel = CreateTMPText(panel, "MusicLabel", "Music Volume",
            new Vector2(-120f, 20f), new Vector2(280f, 45f), 22, FontStyles.Normal);
        musicLabel.GetComponent<TextMeshProUGUI>().color = Color.white;
        musicLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        GameObject musicSliderGO = CreateSlider(panel, "MusicSlider",
            new Vector2(130f, 20f), new Vector2(280f, 30f), 0f, 1f, 0.8f);

        GameObject musicValueLabel = CreateTMPText(panel, "MusicValue", "80%",
            new Vector2(310f, 20f), new Vector2(80f, 45f), 20, FontStyles.Normal);
        musicValueLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.8f, 0.9f, 1f);
        musicValueLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // ── Effects Volume Row ─────────────────────────────────────────────
        GameObject fxLabel = CreateTMPText(panel, "FXLabel", "Effects Volume",
            new Vector2(-120f, -60f), new Vector2(280f, 45f), 22, FontStyles.Normal);
        fxLabel.GetComponent<TextMeshProUGUI>().color = Color.white;
        fxLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        GameObject fxSliderGO = CreateSlider(panel, "FXSlider",
            new Vector2(130f, -60f), new Vector2(280f, 30f), 0f, 1f, 0.8f);

        GameObject fxValueLabel = CreateTMPText(panel, "FXValue", "80%",
            new Vector2(310f, -60f), new Vector2(80f, 45f), 20, FontStyles.Normal);
        fxValueLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.8f, 0.9f, 1f);
        fxValueLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // ── Divider ────────────────────────────────────────────────────────
        CreateDivider(panel, new Vector2(0f, -115f), new Vector2(580f, 2f));

        // ── Close Button ───────────────────────────────────────────────────
        GameObject closeBtn = CreateButton(panel, "CloseBtn", "CLOSE",
            new Vector2(0f, -175f), new Vector2(200f, 55f));

        // ── Add SettingsManager ────────────────────────────────────────────
        SettingsManager sm = panel.AddComponent<SettingsManager>();
        sm.sensitivitySlider  = sensSliderGO.GetComponent<Slider>();
        sm.musicVolumeSlider  = musicSliderGO.GetComponent<Slider>();
        sm.effectsVolumeSlider = fxSliderGO.GetComponent<Slider>();
        sm.sensitivityLabel   = sensValueLabel.GetComponent<TextMeshProUGUI>();
        sm.musicLabel         = musicValueLabel.GetComponent<TextMeshProUGUI>();
        sm.effectsLabel       = fxValueLabel.GetComponent<TextMeshProUGUI>();

        // Wire slider events to SettingsManager
        Slider sensSl = sensSliderGO.GetComponent<Slider>();
        sensSl.onValueChanged.AddListener(sm.OnSensitivityChanged);

        Slider musicSl = musicSliderGO.GetComponent<Slider>();
        musicSl.onValueChanged.AddListener(sm.OnMusicVolumeChanged);

        Slider fxSl = fxSliderGO.GetComponent<Slider>();
        fxSl.onValueChanged.AddListener(sm.OnEffectsVolumeChanged);

        // Wire close button to MainMenuManager.CloseAllPanels
        MainMenuManager mmm = canvasGO.GetComponent<MainMenuManager>();
        if (mmm != null)
        {
            Button closeBtnComp = closeBtn.GetComponent<Button>();
            closeBtnComp.onClick.AddListener(mmm.CloseAllPanels);
        }

        // Start hidden
        panel.SetActive(false);

        Debug.Log("Settings panel built.");
        return panel;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Wire Settings Button
    // ─────────────────────────────────────────────────────────────────────────
    static void WireSettingsButton(GameObject canvasGO, GameObject settingsPanel)
    {
        // Find the SETTINGS button
        Transform settingsBtnT = canvasGO.transform.Find("ButtonPanel/SETTINGS_Btn");
        if (settingsBtnT == null)
        {
            Debug.LogWarning("SETTINGS_Btn not found.");
            return;
        }

        Button btn = settingsBtnT.GetComponent<Button>();
        if (btn == null) { Debug.LogWarning("No Button component on SETTINGS_Btn."); return; }

        // Clear existing listeners and add OpenSettings
        btn.onClick.RemoveAllListeners();
        MainMenuManager mmm = canvasGO.GetComponent<MainMenuManager>();
        if (mmm != null)
        {
            btn.onClick.AddListener(mmm.OpenSettings);
            Debug.Log("Settings button wired to MainMenuManager.OpenSettings.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UI Helpers
    // ─────────────────────────────────────────────────────────────────────────

    static GameObject CreateTMPText(GameObject parent, string name, string text,
        Vector2 anchoredPos, Vector2 size, float fontSize, FontStyles style)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return go;
    }

    static GameObject CreateSlider(GameObject parent, string name,
        Vector2 anchoredPos, Vector2 size, float minVal, float maxVal, float defaultVal)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Slider slider = go.AddComponent<Slider>();
        slider.minValue = minVal;
        slider.maxValue = maxVal;
        slider.value = defaultVal;
        slider.wholeNumbers = false;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        RectTransform bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0.25f);
        bgRT.anchorMax = new Vector2(1f, 0.75f);
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRT.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRT.offsetMin = new Vector2(5f, 0f);
        fillAreaRT.offsetMax = new Vector2(-15f, 0f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0f, 0f);
        fillRT.anchorMax = new Vector2(0f, 1f);
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = new Vector2(10f, 0f);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.75f, 0.85f, 1f);

        // Handle Slide Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(go.transform, false);
        RectTransform handleAreaRT = handleArea.AddComponent<RectTransform>();
        handleAreaRT.anchorMin = new Vector2(0f, 0f);
        handleAreaRT.anchorMax = new Vector2(1f, 1f);
        handleAreaRT.offsetMin = new Vector2(10f, 0f);
        handleAreaRT.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRT = handle.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20f, 20f);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(0.9f, 0.95f, 1f, 1f);

        // Wire slider references
        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.value = defaultVal;

        return go;
    }

    static void CreateDivider(GameObject parent, Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject("Divider");
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.8f, 0.9f, 0.4f);
    }

    static GameObject CreateButton(GameObject parent, string name, string label,
        Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        RectTransform labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(0.2f, 0.8f, 0.9f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;

        return go;
    }
}
