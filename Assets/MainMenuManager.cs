using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject settingsPanel;

    // Key used by the save system to record that a save exists
    public const string SAVE_EXISTS_KEY  = "SaveExists";
    public const string SAVE_SCENE_KEY   = "SavedScene";

    // ── Scene Buttons ──────────────────────────────────────────────────────

    /// <summary>Wipes any existing save and starts fresh from GameScene.</summary>
    public void StartNewGame()
    {
        // Clear previous save data so the game starts fresh
        PlayerPrefs.DeleteKey(SAVE_EXISTS_KEY);
        PlayerPrefs.DeleteKey(SAVE_SCENE_KEY);
        PlayerPrefs.Save();

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
            // No save found — flash a message or just start new
            Debug.Log("No save found — starting new game.");
            SceneManager.LoadScene("GameScene");
        }
    }

    // ── Panel Buttons ──────────────────────────────────────────────────────

    public void OpenInventory()
    {
        CloseAllPanels();
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        CloseAllPanels();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseAllPanels()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (settingsPanel != null)  settingsPanel.SetActive(false);
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
