using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject gameOverPanel;

    [Header("UI")]
    public TextMeshProUGUI reasonText;
    public TextMeshProUGUI subtitleText;

    [Header("References")]
    public GameTimer gameTimer;
    public PauseMenuManager pauseMenuManager;
    public CanvasGroup gameOverCanvasGroup;

    // Cached button RectTransforms for direct hit-testing fallback
    private RectTransform _retryRect;
    private RectTransform _menuRect;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(this); // Destroy duplicate component, not the whole GO
            return;
        }
    }

    void Start()
    {
        Time.timeScale = 1f;

        if (gameOverPanel != null)
        {
            SetupButtons();
            gameOverPanel.SetActive(false);
        }

        if (gameTimer != null)
            gameTimer.OnTimeUp += TriggerTimeUp;
    }

    void OnDestroy()
    {
        if (gameTimer != null)
            gameTimer.OnTimeUp -= TriggerTimeUp;

        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (gameOverPanel == null || !gameOverPanel.activeInHierarchy) return;

        // Force cursor unlocked every frame while Game Over is showing
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // ── Keyboard shortcuts ──────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.R))
        {
            RetryLevel();
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
        {
            GoToMainMenu();
            return;
        }

        // ── Direct mouse-click fallback (works even when InputSystem module  ──
        // ── has broken action references)                                    ──
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 screenPos = Input.mousePosition;

            if (_retryRect != null && RectTransformUtility.RectangleContainsScreenPoint(
                    _retryRect, screenPos, null))
            {
                RetryLevel();
                return;
            }

            if (_menuRect != null && RectTransformUtility.RectangleContainsScreenPoint(
                    _menuRect, screenPos, null))
            {
                GoToMainMenu();
                return;
            }
        }
    }

    public static void EnsureEventSystemInputModule()
    {
        EventSystem es = EventSystem.current;
        if (es == null)
            es = FindFirstObjectByType<EventSystem>();

        if (es == null) return;

        // Remove legacy StandaloneInputModule if present
        StandaloneInputModule standalone = es.GetComponent<StandaloneInputModule>();
        if (standalone != null)
            Destroy(standalone);

        // Add / retrieve InputSystemUIInputModule
        InputSystemUIInputModule isModule = es.GetComponent<InputSystemUIInputModule>();
        if (isModule == null)
            isModule = es.gameObject.AddComponent<InputSystemUIInputModule>();

        // Try to assign default actions so pointer / click work
        try { isModule.AssignDefaultActions(); }
        catch { /* SDK version may not have this overload — ignore */ }
    }

    public void SetupButtons()
    {
        if (gameOverPanel == null) return;

        _retryRect = null;
        _menuRect  = null;

        Button[] buttons = gameOverPanel.GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            btn.interactable = true;
            string btnName = btn.name.ToLower();

            if (btnName.Contains("retry") || btnName.Contains("restart"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(RetryLevel);
                _retryRect = btn.GetComponent<RectTransform>();
            }
            else if (btnName.Contains("menu") || btnName.Contains("main"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(GoToMainMenu);
                _menuRect = btn.GetComponent<RectTransform>();
            }
            else if (btnName.Contains("quit") || btnName.Contains("exit"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(QuitGame);
            }
        }
    }

    public void TriggerTimeUp()
    {
        ShowGameOver("TIME'S UP!", "You ran out of time.");
    }

    public void ShowGameOver(string reason, string subtitle = "")
    {
        // Pause the game
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        EnsureEventSystemInputModule();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLoseMusic();
        }

        // Pause timer
        if (gameTimer != null)
            gameTimer.PauseTimer();

        // Disable pause menu so it can't steal input
        if (pauseMenuManager != null)
            pauseMenuManager.enabled = false;

        // Show Game Over panel
        if (gameOverPanel != null)
        {
            Canvas parentCanvas = gameOverPanel.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null)
                parentCanvas.gameObject.SetActive(true);

            gameOverPanel.SetActive(true);
            SetupButtons(); // wire buttons & cache RectTransforms
        }

        // Make sure CanvasGroup is interactive
        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha          = 1f;
            gameOverCanvasGroup.interactable   = true;
            gameOverCanvasGroup.blocksRaycasts = true;
        }

        // Update text
        if (reasonText   != null) reasonText.text   = reason;
        if (subtitleText != null) subtitleText.text  = subtitle;

        Debug.Log("[GameOverManager] GAME OVER SCREEN ACTIVATED!");
    }

    // ── Button actions ──────────────────────────────────────────────────────

    public void RetryLevel()
    {
        Debug.Log("[GameOverManager] RetryLevel called");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Debug.Log("[GameOverManager] GoToMainMenu called");
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        SceneManager.LoadScene("MainMenuScene");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("[GameOverManager] Game Quit!");
    }
}
