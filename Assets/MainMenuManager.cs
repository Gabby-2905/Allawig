using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject settingsPanel;

    [Header("Inventory Panel UI")]
    public Image[]            invSlotImages  = new Image[4];
    public TextMeshProUGUI[]  invSlotLabels  = new TextMeshProUGUI[4];
    public TextMeshProUGUI    invEmptyLabel;   // "No items saved" label

    // Keys used by the save system
    public const string SAVE_EXISTS_KEY = "SaveExists";
    public const string SAVE_SCENE_KEY  = "SavedScene";

    // ── Unity ──────────────────────────────────────────────────────────────

    void Start()
    {
        // Wire slider OnValueChanged at runtime so labels always update
        WireSliders();
    }

    // ── Slider wiring ──────────────────────────────────────────────────────

    private void WireSliders()
    {
        if (settingsPanel == null) return;
        SettingsManager sm = settingsPanel.GetComponent<SettingsManager>();
        if (sm == null) return;

        if (sm.sensitivitySlider != null)
        {
            sm.sensitivitySlider.onValueChanged.RemoveAllListeners();
            sm.sensitivitySlider.onValueChanged.AddListener(sm.OnSensitivityChanged);
        }
        if (sm.musicVolumeSlider != null)
        {
            sm.musicVolumeSlider.onValueChanged.RemoveAllListeners();
            sm.musicVolumeSlider.onValueChanged.AddListener(sm.OnMusicVolumeChanged);
        }
        if (sm.effectsVolumeSlider != null)
        {
            sm.effectsVolumeSlider.onValueChanged.RemoveAllListeners();
            sm.effectsVolumeSlider.onValueChanged.AddListener(sm.OnEffectsVolumeChanged);
        }
    }

    // ── Scene Buttons ──────────────────────────────────────────────────────

    /// <summary>Always starts fresh from GameScene (Level 1).</summary>
    public void StartNewGame()
    {
        // Clear previous save so the game starts fresh
        PlayerPrefs.DeleteKey(SAVE_EXISTS_KEY);
        PlayerPrefs.DeleteKey(SAVE_SCENE_KEY);
        PlayerPrefs.Save();

        // Clear saved inventory
        GameSaveManager.ClearInventory();

        SceneManager.LoadScene("GameScene");
    }

    /// <summary>Loads the last saved scene, or GameScene if no save exists.</summary>
    public void ContinueGame()
    {
        if (PlayerPrefs.HasKey(SAVE_EXISTS_KEY))
        {
            string savedScene = PlayerPrefs.GetString(SAVE_SCENE_KEY, "GameScene");
            SceneManager.LoadScene(savedScene);
        }
        else
        {
            Debug.Log("No save found — starting new game.");
            SceneManager.LoadScene("GameScene");
        }
    }

    // ── Panel Buttons ──────────────────────────────────────────────────────

    public void OpenInventory()
    {
        CloseAllPanels();
        if (inventoryPanel == null) return;

        // Populate inventory slots from saved data
        RefreshInventoryPanel();
        inventoryPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        CloseAllPanels();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            // Re-wire sliders in case the panel was just activated
            WireSliders();
            // Force load saved values into sliders/labels
            SettingsManager sm = settingsPanel.GetComponent<SettingsManager>();
            if (sm != null) sm.SendMessage("OnEnable", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void CloseAllPanels()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (settingsPanel  != null) settingsPanel.SetActive(false);
    }

    // ── Inventory helpers ──────────────────────────────────────────────────

    private void RefreshInventoryPanel()
    {
        bool anyItem = false;

        for (int i = 0; i < 4; i++)
        {
            var (name, color) = GameSaveManager.LoadInventorySlot(i);
            bool hasItem = !string.IsNullOrEmpty(name);
            if (hasItem) anyItem = true;

            if (i < invSlotImages.Length && invSlotImages[i] != null)
                invSlotImages[i].color = hasItem ? color : new Color(0.1f, 0.1f, 0.1f, 0.5f);

            if (i < invSlotLabels.Length && invSlotLabels[i] != null)
                invSlotLabels[i].text = hasItem ? name : "Empty";
        }

        if (invEmptyLabel != null)
            invEmptyLabel.gameObject.SetActive(!anyItem);
    }

    // ── Quit ──────────────────────────────────────────────────────────────

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
