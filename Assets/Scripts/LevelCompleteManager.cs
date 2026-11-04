using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the Level Complete screen.
/// Call Show() to display it (e.g. from ExitPoint).
/// </summary>
public class LevelCompleteManager : MonoBehaviour
{
    public static LevelCompleteManager Instance { get; private set; }

    [Header("Canvas")]
    public GameObject levelCompleteCanvas;

    [Header("Stars")]
    public Image starLeft;
    public Image starCenter;
    public Image starRight;

    [Header("Objective")]
    public TextMeshProUGUI objectiveText;

    [Header("Buttons")]
    public Button backToMainMenuButton;
    public Button nextSectorButton;

    [Header("Settings")]
    [Tooltip("Scene to load when Next Sector is pressed.")]
    public string nextSceneName = "";
    [Tooltip("Scene to load when Back to Main Menu is pressed.")]
    public string mainMenuSceneName = "MainMenuScene";

    [Header("Stars to show (1-3)")]
    [Range(1, 3)]
    public int starsEarned = 3;

    [Header("Objective Display")]
    public string objectiveLabel = "PRIMARY OBJECTIVE:";
    public int objectiveCurrent = 8;
    public int objectiveTotal = 8;

    // Star sprites (assigned by builder script)
    public Sprite starFilledSprite;
    public Sprite starEmptySprite;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (levelCompleteCanvas != null)
            levelCompleteCanvas.SetActive(false);
    }

    private void Start()
    {
        SetupButtons();
    }

    public void SetupButtons()
    {
        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.interactable = true;
            backToMainMenuButton.onClick.RemoveAllListeners();
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenu);
        }

        if (nextSectorButton != null)
        {
            nextSectorButton.interactable = true;
            nextSectorButton.onClick.RemoveAllListeners();
            nextSectorButton.onClick.AddListener(OnNextSector);
        }

        // Also search any other buttons inside levelCompleteCanvas
        if (levelCompleteCanvas != null)
        {
            Button[] buttons = levelCompleteCanvas.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                btn.interactable = true;
                string btnName = btn.name.ToLower();
                if (btnName.Contains("next") || btnName.Contains("sector") || btnName.Contains("continue"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(OnNextSector);
                }
                else if (btnName.Contains("menu") || btnName.Contains("main"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(OnBackToMainMenu);
                }
            }
        }
    }

    /// <summary>
    /// Show the Level Complete screen.
    /// </summary>
    public void Show()
    {
        if (levelCompleteCanvas == null) return;

        // Pause the game
        Time.timeScale = 0f;

        GameOverManager.EnsureEventSystemInputModule();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWinMusic();
        }

        // Update objective text
        if (objectiveText != null)
            objectiveText.text = $"{objectiveLabel}  {objectiveCurrent}/{objectiveTotal}";

        // Update stars
        UpdateStars();

        levelCompleteCanvas.SetActive(true);
        SetupButtons();

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Show with custom star count and objective values.
    /// </summary>
    public void Show(int stars, int current, int total)
    {
        starsEarned = Mathf.Clamp(stars, 1, 3);
        objectiveCurrent = current;
        objectiveTotal = total;
        Show();
    }

    private void UpdateStars()
    {
        bool leftFilled   = starsEarned >= 2;
        bool centerFilled = starsEarned >= 1;
        bool rightFilled  = starsEarned >= 3;

        SetStarFill(starLeft,   leftFilled);
        SetStarFill(starCenter, centerFilled);
        SetStarFill(starRight,  rightFilled);
    }

    private void SetStarFill(Image star, bool filled)
    {
        if (star == null) return;
        if (filled)
        {
            star.color = new Color(1f, 0.78f, 0.1f, 1f); // gold
        }
        else
        {
            star.color = new Color(0.4f, 0.3f, 0.05f, 1f); // dim gold
        }
    }

    private void OnBackToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnNextSector()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            SceneManager.LoadScene("Level2Scene");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
