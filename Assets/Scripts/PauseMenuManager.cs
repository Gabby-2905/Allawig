using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the pause menu, settings subpanel, and controls subpanel.
/// Guarantees that every pause menu button is accessible, functional,
/// and that mouse cursor locking/unlocking is always properly maintained.
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject controlsPanel;

    [Header("Timer")]
    public GameTimer gameTimer;

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;

        if (pauseMenuPanel == null)
            pauseMenuPanel = GameObject.Find("PausePanel");

        EnsurePanels();

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        isPaused = false;
        Time.timeScale = 1f;
    }

    void Start()
    {
        EnsurePanels();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If settings or controls panel is open, return to pause panel
            if (isPaused && settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
                return;
            }

            if (isPaused && controlsPanel != null && controlsPanel.activeSelf)
            {
                CloseControls();
                return;
            }

            // Otherwise toggle pause
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  BUTTON ACTIONS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Resumes normal gameplay.
    /// </summary>
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (gameTimer != null)
            gameTimer.ResumeTimer();

        // Lock cursor back for first/third person gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[PauseMenu] Game Resumed!");
    }

    /// <summary>
    /// Pauses gameplay and opens the pause menu.
    /// </summary>
    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (gameTimer != null)
            gameTimer.PauseTimer();

        Debug.Log("[PauseMenu] Game Paused!");
    }

    /// <summary>
    /// Reloads the active level.
    /// </summary>
    public void OnRetryLevel()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Opens the settings subpanel.
    /// </summary>
    public void OpenSettings()
    {
        EnsurePanels();

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Closes settings and returns to the pause menu.
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    /// <summary>
    /// Opens the controls subpanel.
    /// </summary>
    public void OpenControls()
    {
        EnsurePanels();

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Closes controls and returns to the pause menu.
    /// </summary>
    public void CloseControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    /// <summary>
    /// Saves game state flag.
    /// </summary>
    public void SaveGame()
    {
        PlayerPrefs.SetInt("SaveExists", 1);
        PlayerPrefs.Save();
        Debug.Log("[PauseMenu] Game Saved!");
    }

    /// <summary>
    /// Loads MainMenuScene.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// Quits to main menu or exits desktop if standalone.
    /// </summary>
    public void QuitToDesktop()
    {
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[PauseMenu] Quitting Game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // In standalone, if MainMenuScene is available load it, otherwise quit
        if (Application.CanStreamedLevelBeLoaded("MainMenuScene"))
            SceneManager.LoadScene("MainMenuScene");
        else
            Application.Quit();
#endif
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PANEL AUTO-CREATION & SETUP
    // ─────────────────────────────────────────────────────────────────────────
    private void EnsurePanels()
    {
        Transform canvasTr = null;
        if (pauseMenuPanel != null)
            canvasTr = pauseMenuPanel.transform.parent;
        else
        {
            var pc = GameObject.Find("PauseCanvas");
            if (pc != null) canvasTr = pc.transform;
        }

        if (canvasTr == null) return;

        // 1. Settings Panel
        if (settingsPanel == null)
        {
            Transform existing = canvasTr.Find("SettingsPanel");
            if (existing != null)
            {
                settingsPanel = existing.gameObject;
            }
            else
            {
                settingsPanel = CreateSettingsPanel(canvasTr);
            }
        }

        // 2. Controls Panel
        if (controlsPanel == null)
        {
            Transform existing = canvasTr.Find("ControlsPanel");
            if (existing != null)
            {
                controlsPanel = existing.gameObject;
            }
            else
            {
                controlsPanel = CreateControlsPanel(canvasTr);
            }
        }
    }

    private GameObject CreateSettingsPanel(Transform parent)
    {
        GameObject panel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(500f, 440f);

        Image img = panel.GetComponent<Image>();
        img.color = new Color(0.08f, 0.08f, 0.12f, 0.96f);

        // Title
        CreateText(panel.transform, "Title", "SETTINGS", new Vector2(0, 160), new Vector2(400, 50), 32, TextAlignmentOptions.Center, Color.white);

        // Sensitivity Row
        CreateText(panel.transform, "SensLabel", "Mouse Sensitivity", new Vector2(-100, 90), new Vector2(250, 30), 20, TextAlignmentOptions.Left, Color.white);
        var sensSlider = CreateSlider(panel.transform, "SensSlider", new Vector2(40, 90), new Vector2(180, 24), 0.1f, 3.0f, PlayerPrefs.GetFloat("Sensitivity", 1f));
        var sensVal = CreateText(panel.transform, "SensVal", Mathf.RoundToInt(sensSlider.value * 100) + "%", new Vector2(180, 90), new Vector2(70, 30), 18, TextAlignmentOptions.Center, Color.yellow);
        sensSlider.onValueChanged.AddListener((v) => {
            PlayerPrefs.SetFloat("Sensitivity", v);
            sensVal.text = Mathf.RoundToInt(v * 100) + "%";
        });

        // Music Volume Row
        CreateText(panel.transform, "MusicLabel", "Music Volume", new Vector2(-100, 25), new Vector2(250, 30), 20, TextAlignmentOptions.Left, Color.white);
        var musicSlider = CreateSlider(panel.transform, "MusicSlider", new Vector2(40, 25), new Vector2(180, 24), 0f, 1f, PlayerPrefs.GetFloat("Settings_MusicVol", 0.8f));
        var musicVal = CreateText(panel.transform, "MusicVal", Mathf.RoundToInt(musicSlider.value * 100) + "%", new Vector2(180, 25), new Vector2(70, 30), 18, TextAlignmentOptions.Center, Color.yellow);
        musicSlider.onValueChanged.AddListener((v) => {
            PlayerPrefs.SetFloat("Settings_MusicVol", v);
            PlayerPrefs.SetFloat("MusicVolume", v);
            musicVal.text = Mathf.RoundToInt(v * 100) + "%";
            if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(v);
        });

        // Effects Volume Row
        CreateText(panel.transform, "SfxLabel", "Sound Effects", new Vector2(-100, -40), new Vector2(250, 30), 20, TextAlignmentOptions.Left, Color.white);
        var sfxSlider = CreateSlider(panel.transform, "SfxSlider", new Vector2(40, -40), new Vector2(180, 24), 0f, 1f, PlayerPrefs.GetFloat("Settings_EffectsVol", 1.0f));
        var sfxVal = CreateText(panel.transform, "SfxVal", Mathf.RoundToInt(sfxSlider.value * 100) + "%", new Vector2(180, -40), new Vector2(70, 30), 18, TextAlignmentOptions.Center, Color.yellow);
        sfxSlider.onValueChanged.AddListener((v) => {
            PlayerPrefs.SetFloat("Settings_EffectsVol", v);
            PlayerPrefs.SetFloat("EffectsVolume", v);
            sfxVal.text = Mathf.RoundToInt(v * 100) + "%";
            if (AudioManager.Instance != null) AudioManager.Instance.SetEffectsVolume(v);
        });

        // Back Button
        CreateButton(panel.transform, "BackBtn", "BACK", new Vector2(0, -145), new Vector2(220, 50), CloseSettings);

        panel.SetActive(false);
        return panel;
    }

    private GameObject CreateControlsPanel(Transform parent)
    {
        GameObject panel = new GameObject("ControlsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(520f, 500f);

        Image img = panel.GetComponent<Image>();
        img.color = new Color(0.08f, 0.08f, 0.12f, 0.96f);

        // Title
        CreateText(panel.transform, "Title", "CONTROLS", new Vector2(0, 200), new Vector2(400, 50), 32, TextAlignmentOptions.Center, Color.white);

        // Controls List Text
        string controlsInfo =
            "<color=#00E5FF>W, A, S, D</color>  —  Move\n" +
            "<color=#00E5FF>Left Shift</color>  —  Sprint\n" +
            "<color=#00E5FF>Space</color>  —  Jump\n" +
            "<color=#00E5FF>Left Ctrl / C</color>  —  Crouch\n" +
            "<color=#00E5FF>Left Click</color>  —  Punch / Attack\n" +
            "<color=#00E5FF>Right Click</color>  —  Aim\n" +
            "<color=#00E5FF>E</color>  —  Interact / Loot / Heal\n" +
            "<color=#00E5FF>Tab</color>  —  Open / Close Inventory\n" +
            "<color=#00E5FF>Esc</color>  —  Pause Menu";

        CreateText(panel.transform, "ControlsList", controlsInfo, new Vector2(0, 20), new Vector2(440, 280), 20, TextAlignmentOptions.Center, Color.white);

        // Back Button
        CreateButton(panel.transform, "BackBtn", "BACK", new Vector2(0, -185), new Vector2(220, 50), CloseControls);

        panel.SetActive(false);
        return panel;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  UI CREATION HELPERS
    // ─────────────────────────────────────────────────────────────────────────
    private TextMeshProUGUI CreateText(Transform parent, string name, string content, Vector2 pos, Vector2 size, float fontSize, TextAlignmentOptions align, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color;
        return tmp;
    }

    private Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.7f, 0.9f);

        Button btn = go.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.2f, 0.45f, 0.8f, 0.9f);
        cb.highlightedColor = new Color(0.3f, 0.6f, 1f, 1f);
        cb.pressedColor = new Color(0.15f, 0.35f, 0.65f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(action);

        CreateText(go.transform, "BtnText", label, Vector2.zero, size, 22, TextAlignmentOptions.Center, Color.white);

        return btn;
    }

    private Slider CreateSlider(Transform parent, string name, Vector2 pos, Vector2 size, float minVal, float maxVal, float curVal)
    {
        GameObject sliderGO = new GameObject(name, typeof(RectTransform), typeof(Slider));
        sliderGO.transform.SetParent(parent, false);

        RectTransform rt = sliderGO.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Slider slider = sliderGO.GetComponent<Slider>();
        slider.minValue = minVal;
        slider.maxValue = maxVal;
        slider.value = curVal;

        // Background
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bg.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.8f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0, 0.25f);
        faRT.anchorMax = new Vector2(1, 0.75f);
        faRT.sizeDelta = Vector2.zero;

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        fillRT.sizeDelta = Vector2.zero;
        fill.GetComponent<Image>().color = new Color(0.2f, 0.7f, 1f, 1f);

        slider.fillRect = fillRT;

        return slider;
    }
}
