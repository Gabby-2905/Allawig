using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject settingsPanel;

    // --- Scene Buttons ---

    public void StartNewGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void ContinueGame()
    {
        if (PlayerPrefs.HasKey("SaveExists"))
            SceneManager.LoadScene("GameScene");
        else
            Debug.Log("No save found!");
    }

    // --- Panel Buttons ---

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
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // --- Quit ---

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
