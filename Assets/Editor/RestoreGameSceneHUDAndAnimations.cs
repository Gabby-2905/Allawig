using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Restores the full HUD (Timer, Inventory/Tab, Health, Sprint) and
/// creates + assigns a PlayerAnimator controller with Idle/Walk/Run/Punch clips.
/// </summary>
public class RestoreGameSceneHUDAndAnimations
{
    const string SCENE_PATH = "Assets/Scenes/GameScene.unity";
    const string ANIM_BASE  = "Assets/Kevin Iglesias/Human Animations/Animations/Male/";
    const string CTRL_PATH  = "Assets/Scripts/PlayerAnimator.controller";
    const string FONT_GUID  = "8f586378b4e144a9851e7b34d9b748ee"; // BebasNeue SDF

    [MenuItem("Tools/Restore GameScene HUD And Animations")]
    public static void Execute()
    {
        // ── 1. Open GameScene ─────────────────────────────────────────────
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        SceneManager.SetActiveScene(scene);

        // ── 2. Build / assign animator controller ─────────────────────────
        BuildAndAssignAnimator();

        // ── 3. Restore HUD ────────────────────────────────────────────────
        RestoreHUD(scene);

        // ── 4. Save ───────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        Debug.Log("[RestoreHUD] Done — GameScene saved.");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  ANIMATOR
    // ─────────────────────────────────────────────────────────────────────
    static void BuildAndAssignAnimator()
    {
        // Load clips
        AnimationClip idle   = LoadClip(ANIM_BASE + "Idles/HumanM@Idle01.fbx",                          "HumanM@Idle01");
        AnimationClip walk   = LoadClip(ANIM_BASE + "Movement/Walk/HumanM@Walk01_Forward.fbx",           "HumanM@Walk01_Forward");
        AnimationClip run    = LoadClip(ANIM_BASE + "Movement/Run/HumanM@Run01_Forward.fbx",             "HumanM@Run01_Forward");
        AnimationClip punchL = LoadClip(ANIM_BASE + "Combat/1H/HumanM@Attack1H01_L.fbx",                "HumanM@Attack1H01_L");
        AnimationClip punchR = LoadClip(ANIM_BASE + "Combat/1H/HumanM@Attack1H01_R.fbx",                "HumanM@Attack1H01_R");
        AnimationClip combatIdle = LoadClip(ANIM_BASE + "Combat/HumanM@CombatIdle01.fbx",               "HumanM@CombatIdle01");

        // Create controller
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(CTRL_PATH);

        // Parameters
        ctrl.AddParameter("Speed",      AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsAiming",   AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("PunchLeft",  AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("PunchRight", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("IsCrouching",AnimatorControllerParameterType.Bool);

        var root = ctrl.layers[0].stateMachine;

        // States
        var stIdle   = root.AddState("Idle");   stIdle.motion   = idle;
        var stWalk   = root.AddState("Walk");   stWalk.motion   = walk;
        var stRun    = root.AddState("Run");    stRun.motion    = run;
        var stAim    = root.AddState("CombatIdle"); stAim.motion = combatIdle;
        var stPunchL = root.AddState("PunchLeft");  stPunchL.motion = punchL;
        var stPunchR = root.AddState("PunchRight"); stPunchR.motion = punchR;

        root.defaultState = stIdle;

        // Idle → Walk (Speed > 0.1)
        AddTransition(stIdle, stWalk,  "Speed",  AnimatorConditionMode.Greater, 0.1f);
        // Walk → Idle (Speed < 0.1)
        AddTransition(stWalk, stIdle,  "Speed",  AnimatorConditionMode.Less,    0.1f);
        // Walk → Run (Speed > 1.5)
        AddTransition(stWalk, stRun,   "Speed",  AnimatorConditionMode.Greater, 1.5f);
        // Run → Walk (Speed < 1.5)
        AddTransition(stRun,  stWalk,  "Speed",  AnimatorConditionMode.Less,    1.5f);
        // Any → CombatIdle (IsAiming true)
        AddBoolTransitionFromAny(root, stAim,    "IsAiming", true);
        // CombatIdle → Idle (IsAiming false)
        AddBoolTransition(stAim, stIdle, "IsAiming", false);
        // CombatIdle → PunchLeft
        AddTriggerTransition(stAim,   stPunchL, "PunchLeft");
        AddTriggerTransition(stIdle,  stPunchL, "PunchLeft");
        AddTriggerTransition(stPunchL, stAim,   null); // auto-return
        // CombatIdle → PunchRight
        AddTriggerTransition(stAim,   stPunchR, "PunchRight");
        AddTriggerTransition(stIdle,  stPunchR, "PunchRight");
        AddTriggerTransition(stPunchR, stAim,   null);

        AssetDatabase.SaveAssets();
        Debug.Log("[RestoreHUD] PlayerAnimator controller created at " + CTRL_PATH);

        // Assign to Arnold child on Player
        var player = GameObject.FindWithTag("Player");
        if (player == null) { Debug.LogWarning("[RestoreHUD] Player not found."); return; }

        var anim = player.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.runtimeAnimatorController = ctrl;
            anim.applyRootMotion = false;
            var gpm = player.GetComponent<GamePlayerMovement>();
            if (gpm != null) gpm.animator = anim;
            Debug.Log("[RestoreHUD] Animator controller assigned to " + anim.gameObject.name);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  HUD
    // ─────────────────────────────────────────────────────────────────────
    static void RestoreHUD(Scene scene)
    {
        var player = GameObject.FindWithTag("Player");
        var gpm    = player != null ? player.GetComponent<GamePlayerMovement>() : null;

        // ── EventSystem ───────────────────────────────────────────────────
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            EditorSceneManager.MoveGameObjectToScene(es, scene);
        }

        // ── Main HUD Canvas ───────────────────────────────────────────────
        var hudGO = new GameObject("HUDCanvas");
        EditorSceneManager.MoveGameObjectToScene(hudGO, scene);
        var hudCanvas = hudGO.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 0;
        hudGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        hudGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        hudGO.AddComponent<GraphicRaycaster>();

        // ── Timer (top-center) ────────────────────────────────────────────
        var timerGO = MakePanel(hudGO.transform, "TimerPanel",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -10f), new Vector2(160f, 50f),
            new Color(0f, 0f, 0f, 0.55f));

        var timerText = MakeText(timerGO.transform, "TimerText",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            "3:00", 32, TextAlignmentOptions.Center);

        var timerComp = timerGO.AddComponent<GameTimer>();
        timerComp.totalTime = 180f;
        timerComp.timerText = timerText;

        // ── Health Bar (bottom-left) ───────────────────────────────────────
        var healthPanel = MakePanel(hudGO.transform, "HealthPanel",
            new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(10f, 10f), new Vector2(220f, 36f),
            new Color(0.05f, 0.05f, 0.05f, 0.8f));
        healthPanel.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f);

        var healthBg = MakePanel(healthPanel.transform, "HealthBarBg",
            Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-8f, -8f),
            new Color(0.2f, 0.05f, 0.05f, 1f));

        var healthFill = MakePanel(healthBg.transform, "HealthBarFill",
            Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-4f, -4f),
            new Color(0.85f, 0.15f, 0.15f, 1f));
        var healthFillImg = healthFill.GetComponent<Image>();
        healthFillImg.type = Image.Type.Filled;
        healthFillImg.fillMethod = Image.FillMethod.Horizontal;
        healthFillImg.fillAmount = 1f;

        var healthText = MakeText(healthPanel.transform, "HealthText",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            "100 / 100", 18, TextAlignmentOptions.Center);

        // ── Sprint Meter (bottom-left, above health) ──────────────────────
        var sprintPanel = MakePanel(hudGO.transform, "SprintPanel",
            new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(10f, 52f), new Vector2(220f, 28f),
            new Color(0.05f, 0.05f, 0.05f, 0.8f));
        sprintPanel.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f);

        var sprintBg = MakePanel(sprintPanel.transform, "SprintBg",
            Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-6f, -6f),
            new Color(0.05f, 0.15f, 0.05f, 1f));

        var sprintFill = MakePanel(sprintBg.transform, "SprintFill",
            Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-4f, -4f),
            new Color(0.18f, 0.82f, 0.34f, 1f));
        var sprintFillImg = sprintFill.GetComponent<Image>();
        sprintFillImg.type = Image.Type.Filled;
        sprintFillImg.fillMethod = Image.FillMethod.Horizontal;
        sprintFillImg.fillAmount = 1f;

        var sprintBorder = MakePanel(sprintPanel.transform, "SprintBorder",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(1f, 1f, 1f, 0.35f));
        sprintBorder.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f); // transparent bg

        var sprintLabel = MakeText(sprintPanel.transform, "SprintLabel",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            "STAMINA  100%", 14, TextAlignmentOptions.Center);

        var sprintCG = sprintPanel.AddComponent<CanvasGroup>();
        var sprintComp = sprintPanel.AddComponent<SprintMeterUI>();
        sprintComp.fillImage       = sprintFillImg;
        sprintComp.backgroundImage = sprintBg.GetComponent<Image>();
        sprintComp.borderImage     = sprintBorder.GetComponent<Image>();
        sprintComp.labelText       = sprintLabel;
        sprintComp.canvasGroup     = sprintCG;
        if (gpm != null) sprintComp.player = gpm;

        // ── GameHUD script ────────────────────────────────────────────────
        var hudComp = hudGO.AddComponent<GameHUD>();
        if (gpm != null) hudComp.player = gpm;
        hudComp.healthBarFill = healthFillImg;
        hudComp.healthText    = healthText;

        // ── Inventory Canvas (Tab) ─────────────────────────────────────────
        var invCanvasGO = new GameObject("InventoryCanvas");
        EditorSceneManager.MoveGameObjectToScene(invCanvasGO, scene);
        var invCanvas = invCanvasGO.AddComponent<Canvas>();
        invCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        invCanvas.sortingOrder = 5;
        invCanvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        invCanvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        invCanvasGO.AddComponent<GraphicRaycaster>();

        // Inventory root (hidden by default)
        var invRoot = MakePanel(invCanvasGO.transform, "InventoryRoot",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(500f, 400f),
            new Color(0.05f, 0.05f, 0.1f, 0.92f));
        invRoot.SetActive(false);

        MakeText(invRoot.transform, "InventoryTitle",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -10f), new Vector2(0f, -50f),
            "INVENTORY  [TAB]", 28, TextAlignmentOptions.Center);

        // 4 inventory slots
        var slots = new Image[4];
        var labels = new TextMeshProUGUI[4];
        for (int i = 0; i < 4; i++)
        {
            float xPos = -180f + i * 120f;
            var slot = MakePanel(invRoot.transform, $"Slot{i}",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(xPos, -30f), new Vector2(100f, 100f),
                new Color(0.1f, 0.1f, 0.1f, 0.85f));
            slots[i] = slot.GetComponent<Image>();
            labels[i] = MakeText(slot.transform, "Label",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                "", 16, TextAlignmentOptions.Center);
        }

        hudComp.inventorySlots  = slots;
        hudComp.inventoryLabels = labels;

        // InventoryUI script on the canvas
        var invUI = invCanvasGO.AddComponent<InventoryUI>();
        invUI.inventoryRoot = invRoot;

        // ── Pause Menu Canvas ─────────────────────────────────────────────
        var pauseCanvasGO = new GameObject("PauseCanvas");
        EditorSceneManager.MoveGameObjectToScene(pauseCanvasGO, scene);
        var pauseCanvas = pauseCanvasGO.AddComponent<Canvas>();
        pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pauseCanvas.sortingOrder = 10;
        pauseCanvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        pauseCanvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        pauseCanvasGO.AddComponent<GraphicRaycaster>();

        // Pause panel (hidden by default)
        var pausePanel = MakePanel(pauseCanvasGO.transform, "PausePanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(320f, 380f),
            new Color(0.05f, 0.05f, 0.1f, 0.95f));
        pausePanel.SetActive(false);

        MakeText(pausePanel.transform, "PauseTitle",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -15f), new Vector2(0f, -60f),
            "PAUSED", 36, TextAlignmentOptions.Center);

        string[] btnLabels = { "Resume", "Save Game", "Settings", "Controls", "Main Menu", "Quit" };
        for (int i = 0; i < btnLabels.Length; i++)
        {
            MakeButton(pausePanel.transform, btnLabels[i] + "Btn",
                new Vector2(0f, -80f - i * 52f), new Vector2(260f, 44f), btnLabels[i]);
        }

        // Settings panel (hidden)
        var settingsPanel = MakePanel(pauseCanvasGO.transform, "SettingsPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(320f, 280f),
            new Color(0.05f, 0.05f, 0.1f, 0.95f));
        settingsPanel.SetActive(false);
        MakeText(settingsPanel.transform, "Title",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -15f), new Vector2(0f, -50f),
            "SETTINGS", 30, TextAlignmentOptions.Center);
        MakeButton(settingsPanel.transform, "BackBtn",
            new Vector2(0f, -100f), new Vector2(220f, 44f), "Back");

        // Controls panel (hidden)
        var controlsPanel = MakePanel(pauseCanvasGO.transform, "ControlsPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(380f, 340f),
            new Color(0.05f, 0.05f, 0.1f, 0.95f));
        controlsPanel.SetActive(false);
        MakeText(controlsPanel.transform, "Title",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -15f), new Vector2(0f, -50f),
            "CONTROLS", 30, TextAlignmentOptions.Center);
        string[] controls = {
            "WASD  —  Move", "Shift  —  Sprint", "C  —  Crouch",
            "Left Click  —  Attack", "Right Click  —  Aim", "Tab  —  Inventory", "Esc  —  Pause"
        };
        for (int i = 0; i < controls.Length; i++)
            MakeText(controlsPanel.transform, $"ctrl{i}",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -65f - i * 30f), new Vector2(0f, -30f),
                controls[i], 18, TextAlignmentOptions.Center);
        MakeButton(controlsPanel.transform, "BackBtn",
            new Vector2(0f, -110f), new Vector2(220f, 44f), "Back");

        // PauseMenuManager
        var pmm = pauseCanvasGO.AddComponent<PauseMenuManager>();
        pmm.pauseMenuPanel = pausePanel;
        pmm.settingsPanel  = settingsPanel;
        pmm.controlsPanel  = controlsPanel;
        pmm.gameTimer      = timerComp;

        // ── GameOverManager ───────────────────────────────────────────────
        var gameOverGO = new GameObject("GameOverCanvas");
        EditorSceneManager.MoveGameObjectToScene(gameOverGO, scene);
        var gameOverCanvas = gameOverGO.AddComponent<Canvas>();
        gameOverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameOverCanvas.sortingOrder = 20;
        gameOverGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameOverGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        gameOverGO.AddComponent<GraphicRaycaster>();

        var gameOverPanel = MakePanel(gameOverGO.transform, "GameOverPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(400f, 300f),
            new Color(0.05f, 0.02f, 0.02f, 0.97f));
        gameOverPanel.SetActive(false);

        MakeText(gameOverPanel.transform, "GameOverTitle",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -20f), new Vector2(0f, -80f),
            "GAME OVER", 42, TextAlignmentOptions.Center);
        MakeButton(gameOverPanel.transform, "RetryBtn",
            new Vector2(0f, -30f), new Vector2(260f, 50f), "Try Again");
        MakeButton(gameOverPanel.transform, "MenuBtn",
            new Vector2(0f, -90f), new Vector2(260f, 50f), "Main Menu");

        var gom = gameOverGO.AddComponent<GameOverManager>();
        // Wire timer → game over via TriggerTimeUp
        gom.gameTimer = timerComp;
        gom.gameOverPanel = gameOverPanel;

        Debug.Log("[RestoreHUD] HUD, Inventory, Pause Menu, and Game Over canvas created.");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────────────
    static AnimationClip LoadClip(string path, string clipName)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var a in assets)
            if (a is AnimationClip c && c.name == clipName) return c;
        // fallback: first clip
        foreach (var a in assets)
            if (a is AnimationClip c2 && !c2.name.StartsWith("__")) return c2;
        Debug.LogWarning("[RestoreHUD] Clip not found: " + path);
        return null;
    }

    static AnimatorStateTransition AddTransition(AnimatorState from, AnimatorState to,
        string param, AnimatorConditionMode mode, float threshold)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = 0.15f;
        t.AddCondition(mode, threshold, param);
        return t;
    }

    static void AddBoolTransitionFromAny(AnimatorStateMachine sm, AnimatorState to,
        string param, bool value)
    {
        var t = sm.AddAnyStateTransition(to);
        t.hasExitTime = false;
        t.duration = 0.1f;
        t.canTransitionToSelf = false;
        t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, param);
    }

    static void AddBoolTransition(AnimatorState from, AnimatorState to,
        string param, bool value)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = 0.1f;
        t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, param);
    }

    static void AddTriggerTransition(AnimatorState from, AnimatorState to, string trigger)
    {
        var t = from.AddTransition(to);
        t.duration = 0.05f;
        if (trigger != null)
        {
            t.hasExitTime = false;
            t.AddCondition(AnimatorConditionMode.If, 0, trigger);
        }
        else
        {
            t.hasExitTime = true;
            t.exitTime = 0.9f;
        }
    }

    static GameObject MakePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin       = anchorMin;
        rt.anchorMax       = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta       = sizeDelta;
        rt.pivot           = new Vector2(0.5f, 0.5f);
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static TextMeshProUGUI MakeText(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 sizeDelta,
        string text, int fontSize, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = sizeDelta;
        rt.pivot            = new Vector2(0.5f, 0.5f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = align;
        tmp.color     = Color.white;

        // Try to assign BebasNeue font
        string fontPath = AssetDatabase.GUIDToAssetPath(FONT_GUID);
        if (!string.IsNullOrEmpty(fontPath))
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            if (font != null) tmp.font = font;
        }
        return tmp;
    }

    static GameObject MakeButton(Transform parent, string name,
        Vector2 anchoredPos, Vector2 sizeDelta, string label)
    {
        var go = MakePanel(parent, name,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            anchoredPos, sizeDelta,
            new Color(0.18f, 0.45f, 0.5f, 1f));
        go.AddComponent<Button>();
        MakeText(go.transform, "Label",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            label, 20, TextAlignmentOptions.Center);
        return go;
    }
}
