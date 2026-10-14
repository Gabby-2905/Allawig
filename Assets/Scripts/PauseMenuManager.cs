using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject controlsPanel;

    [Header("Timer (optional)")]
    public GameTimer gameTimer;

    private bool isPaused = false;

    void Awake()
    {
        // Always ensure all panels start hidden when the scene loads
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel   != null) settingsPanel.SetActive(false);
        if (controlsPanel   != null) controlsPanel.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (gameTimer != null) gameTimer.PauseTimer();
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel  != null) settingsPanel.SetActive(false);
        if (controlsPanel  != null) controlsPanel.SetActive(false);
        if (gameTimer != null) gameTimer.ResumeTimer();
    }

    public void OpenSettings()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void OpenControls()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void SaveGame()
    {
        PlayerPrefs.SetInt("SaveExists", 1);
        PlayerPrefs.Save();
        Debug.Log("Game Saved!");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenuScene");
    }

    public void QuitToDesktop()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
