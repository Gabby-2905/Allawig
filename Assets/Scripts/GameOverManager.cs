using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Handles the Game Over screen shown when the timer runs out.
/// Hooks into GameTimer.OnTimeUp automatically.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject gameOverPanel;

    [Header("UI")]
    public TextMeshProUGUI reasonText;   // "TIME'S UP!" or custom message
    public TextMeshProUGUI subtitleText; // optional subtitle

    [Header("Timer Reference")]
    public GameTimer gameTimer;

    [Header("Pause Menu Reference")]
    public PauseMenuManager pauseMenuManager;

    void Start()
    {
        // Hide panel on start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Hook into timer
        if (gameTimer != null)
            gameTimer.OnTimeUp += TriggerTimeUp;
    }

    void OnDestroy()
    {
        if (gameTimer != null)
            gameTimer.OnTimeUp -= TriggerTimeUp;
    }

    // ── Called when timer hits zero ────────────────────────────────────────
    public void TriggerTimeUp()
    {
        ShowGameOver("TIME'S UP!", "You ran out of time.");
    }

    public void ShowGameOver(string reason, string subtitle = "")
    {
        // Pause the game
        Time.timeScale = 0f;

        // Pause the timer
        if (gameTimer != null) gameTimer.PauseTimer();

        // Disable pause menu if open
        if (pauseMenuManager != null) pauseMenuManager.enabled = false;

        // Show panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (reasonText   != null) reasonText.text   = reason;
        if (subtitleText != null) subtitleText.text  = subtitle;
    }

    // ── Button callbacks ───────────────────────────────────────────────────

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
