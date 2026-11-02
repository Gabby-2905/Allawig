
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject controlsPanel;

    [Header("Timer")]
    public GameTimer gameTimer;

    private bool isPaused = false;

    void Awake()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        isPaused = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Resume();
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Pause();
            }
        }
    }

    public void OnRetryLevel()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // PAUSE BUTTON
    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (gameTimer != null)
            gameTimer.PauseTimer();

        Debug.Log("Game Paused!");
    }

    // RESUME BUTTON
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

        Debug.Log("Game Resumed!");
    }

    // SETTINGS BUTTON
    public void OpenSettings()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    // CLOSE SETTINGS BUTTON
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    // CONTROLS BUTTON
    public void OpenControls()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }

    // CLOSE CONTROLS BUTTON
    public void CloseControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    // SAVE GAME BUTTON
    public void SaveGame()
    {
        PlayerPrefs.SetInt("SaveExists", 1);
        PlayerPrefs.Save();

        Debug.Log("Game Saved!");
    }

    // MAIN MENU BUTTON
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenuScene");
    }

    // QUIT BUTTON
    public void QuitToDesktop()
    {
        Time.timeScale = 1f;

        Debug.Log("Game Quit!");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}
