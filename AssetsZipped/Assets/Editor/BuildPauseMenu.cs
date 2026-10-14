using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BuildPauseMenu
{
    [MenuItem("Tools/Build Pause Menu")]
    public static void Build()
    {
        // Remove existing
        var existingCanvas = GameObject.Find("PauseMenuCanvas");
        if (existingCanvas != null) Object.DestroyImmediate(existingCanvas);
        var existingManager = GameObject.Find("PauseMenuManager");
        if (existingManager != null) Object.DestroyImmediate(existingManager);

        // ---- Canvas ----
        var canvasGO = new GameObject("PauseMenuCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ---- Pause Panel ----
        var pausePanel = MakePanel(canvasGO.transform, "PausePanel", 460, 640);

        // Title box
        var titleBox = MakeImageBox(pausePanel.transform, "TitleBox", 240, 60, 0, -40, AnchorPreset.TopCenter);
        MakeText(titleBox.transform, "TitleText", "PAUSED", 32, FontStyles.Bold);

        // Buttons
        string[] labels = { "RESUME", "SETTINGS", "CONTROLS", "SAVE GAME", "MAIN MENU", "QUIT TO DESKTOP" };
        string[] names  = { "ResumeButton", "SettingsButton", "ControlsButton", "SaveGameButton", "MainMenuButton", "QuitToDesktopButton" };
        float startY = -130f;
        for (int i = 0; i < labels.Length; i++)
        {
            float fs = (i == labels.Length - 1) ? 18f : 24f;
            MakeButton(pausePanel.transform, names[i], labels[i], fs, 290, 55, 0, startY - i * 80f);
        }

        // ---- Settings Panel ----
        var settingsPanel = MakePanel(canvasGO.transform, "SettingsPanel", 460, 640);
        MakeSubPanelContent(settingsPanel.transform, "SETTINGS", new[] {
            "Volume: [slider placeholder]",
            "Graphics: [dropdown placeholder]",
            "Fullscreen: [toggle placeholder]"
        });

        // ---- Controls Panel ----
        var controlsPanel = MakePanel(canvasGO.transform, "ControlsPanel", 460, 640);
        MakeSubPanelContent(controlsPanel.transform, "CONTROLS", new[] {
            "WASD / Arrow Keys  -  Move",
            "Mouse  -  Look",
            "ESC  -  Pause / Unpause",
            "E  -  Interact",
            "Left Click  -  Attack"
        });

        // ---- PauseMenuManager ----
        var managerGO = new GameObject("PauseMenuManager");
        var pmm = managerGO.AddComponent<PauseMenuManager>();
        pmm.pauseMenuPanel  = pausePanel;
        pmm.settingsPanel   = settingsPanel;
        pmm.controlsPanel   = controlsPanel;

        // ---- Wire buttons ----
        WireBtn(pausePanel,    "ResumeButton",        pmm, "Resume");
        WireBtn(pausePanel,    "SettingsButton",      pmm, "OpenSettings");
        WireBtn(pausePanel,    "ControlsButton",      pmm, "OpenControls");
        WireBtn(pausePanel,    "SaveGameButton",      pmm, "SaveGame");
        WireBtn(pausePanel,    "MainMenuButton",      pmm, "GoToMainMenu");
        WireBtn(pausePanel,    "QuitToDesktopButton", pmm, "QuitToDesktop");
        WireBtn(settingsPanel, "BackButton",          pmm, "CloseSettings");
        WireBtn(controlsPanel, "BackButton",          pmm, "CloseControls");

        // Hide sub-panels; keep pause panel visible for now (will be hidden at runtime)
        pausePanel.SetActive(true);   // leave visible so we can screenshot
        settingsPanel.SetActive(false);
        controlsPanel.SetActive(false);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Debug.Log("[BuildPauseMenu] Done!");
    }

    // ---- Factory helpers ----

    enum AnchorPreset { Center, TopCenter, BottomCenter }

    static void ApplyAnchor(RectTransform rt, AnchorPreset preset, float w, float h, float x, float y)
    {
        switch (preset)
        {
            case AnchorPreset.TopCenter:
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                break;
            case AnchorPreset.BottomCenter:
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                break;
            default:
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                break;
        }
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, y);
    }

    static GameObject MakePanel(Transform parent, string name, float w, float h)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = Color.white;
        var ol = go.AddComponent<Outline>();
        ol.effectColor = Color.black;
        ol.effectDistance = new Vector2(2f, -2f);
        var rt = go.GetComponent<RectTransform>();
        ApplyAnchor(rt, AnchorPreset.Center, w, h, 0, 0);
        return go;
    }

    static GameObject MakeImageBox(Transform parent, string name, float w, float h, float x, float y, AnchorPreset anchor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = Color.white;
        var ol = go.AddComponent<Outline>();
        ol.effectColor = Color.black;
        ol.effectDistance = new Vector2(1.5f, -1.5f);
        var rt = go.GetComponent<RectTransform>();
        ApplyAnchor(rt, anchor, w, h, x, y);
        return go;
    }

    static TextMeshProUGUI MakeText(Transform parent, string name, string text, float fontSize, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return tmp;
    }

    static Button MakeButton(Transform parent, string name, string label, float fontSize, float w, float h, float x, float y)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = Color.white;
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor    = Color.white;
        cb.highlightedColor = new Color(0.85f, 0.85f, 0.85f);
        cb.pressedColor   = new Color(0.65f, 0.65f, 0.65f);
        cb.selectedColor  = Color.white;
        btn.colors = cb;
        var ol = go.AddComponent<Outline>();
        ol.effectColor = Color.black;
        ol.effectDistance = new Vector2(1.5f, -1.5f);
        var rt = go.GetComponent<RectTransform>();
        ApplyAnchor(rt, AnchorPreset.TopCenter, w, h, x, y);
        MakeText(go.transform, "Text", label, fontSize);
        return btn;
    }

    static void MakeSubPanelContent(Transform parent, string title, string[] lines)
    {
        var titleBox = MakeImageBox(parent, "TitleBox", 280, 60, 0, -40, AnchorPreset.TopCenter);
        MakeText(titleBox.transform, "TitleText", title, 32, FontStyles.Bold);

        for (int i = 0; i < lines.Length; i++)
        {
            var lineGO = new GameObject("Line_" + i);
            lineGO.transform.SetParent(parent, false);
            var tmp = lineGO.AddComponent<TextMeshProUGUI>();
            tmp.text = lines[i];
            tmp.fontSize = 20;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            var rt = lineGO.GetComponent<RectTransform>();
            ApplyAnchor(rt, AnchorPreset.TopCenter, 400, 40, 0, -130f - i * 50f);
        }

        MakeButton(parent, "BackButton", "BACK", 24f, 200, 55, 0, 40f);
        // Fix back button anchor to bottom
        var backRT = parent.Find("BackButton").GetComponent<RectTransform>();
        backRT.anchorMin = backRT.anchorMax = new Vector2(0.5f, 0f);
        backRT.pivot = new Vector2(0.5f, 0f);
        backRT.anchoredPosition = new Vector2(0f, 40f);
    }

    static void WireBtn(GameObject parent, string btnName, PauseMenuManager pmm, string methodName)
    {
        var t = parent.transform.Find(btnName);
        if (t == null) { Debug.LogWarning($"[BuildPauseMenu] Not found: {btnName}"); return; }
        var btn = t.GetComponent<Button>();
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        var method = typeof(PauseMenuManager).GetMethod(methodName);
        if (method == null) { Debug.LogWarning($"[BuildPauseMenu] Method not found: {methodName}"); return; }
        var action = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), pmm, method)
            as UnityEngine.Events.UnityAction;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action);
    }
}
