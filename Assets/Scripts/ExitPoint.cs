using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Place this on a trigger collider in the scene.
/// When the player enters it, it checks whether the player has gas.
/// If not, teleports the player to the boss arena.
/// If the player has gas (boss defeated), shows the Win / Level Complete panel.
/// </summary>
public class ExitPoint : MonoBehaviour
{
    [Header("Next Level")]
    [Tooltip("Exact name of the scene to load. Must be added to Build Settings.")]
    public string nextSceneName = "Level2Scene";

    [Header("Hold-to-Exit Settings")]
    [Tooltip("How long the player must stand in the zone before exiting (0 = instant).")]
    public float holdDuration = 2f;

    [Header("Level Complete Screen")]
    [Tooltip("If true, shows the Level Complete UI instead of loading the next scene directly.")]
    public bool showLevelCompleteUI = true;
    [Tooltip("Stars to award on the Level Complete screen (1-3).")]
    [Range(1, 3)]
    public int starsEarned = 2;
    [Tooltip("Current objective count shown on the Level Complete screen.")]
    public int objectiveCurrent = 8;
    [Tooltip("Total objective count shown on the Level Complete screen.")]
    public int objectiveTotal = 8;

    [Header("UI")]
    public GameObject exitPromptUI;          // optional world-space or screen-space panel
    public TextMeshProUGUI promptText;        // optional text label

    [Header("Visual")]
    public Light exitLight;                  // optional point light on the exit zone
    public Color exitColor = new Color(0f, 1f, 0.4f, 1f);

    // ── State ──────────────────────────────────────────────────────────────
    private bool playerInside = false;
    private float holdTimer   = 0f;
    private bool  loading     = false;
    private bool  _locked     = false;

    public GameObject winMenu;
    public GameObject bossHealthBar; // Reference to the boss health bar GameObject

    private GamePlayerMovement _playerMovement;

    void Start()
    {
        if (exitPromptUI != null)
            exitPromptUI.SetActive(false);

        if (exitLight != null)
            exitLight.color = exitColor;

        FindPlayerReference();
        UpdatePromptText(0f);
    }

    private void FindPlayerReference()
    {
        if (_playerMovement != null) return;

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            _playerMovement = playerGO.GetComponent<GamePlayerMovement>();
        }

        if (_playerMovement == null)
        {
            _playerMovement = FindFirstObjectByType<GamePlayerMovement>(FindObjectsInactive.Include);
        }
    }

    /// <summary>Safely teleports a player Transform even if CharacterController is active.</summary>
    public static void TeleportPlayer(Transform targetTransform, Vector3 destination)
    {
        if (targetTransform == null) return;
        CharacterController cc = targetTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        targetTransform.position = destination;
        if (cc != null) cc.enabled = true;
    }

    /// <summary>Lock or unlock this exit point.</summary>
    public void SetLocked(bool locked)
    {
        _locked = locked;
        if (exitLight != null)
            exitLight.color = locked ? new Color(1f, 0.3f, 0.1f, 1f) : exitColor;
        if (playerInside && promptText != null)
            UpdatePromptText(holdTimer);
    }

    void Update()
    {
        if (!playerInside || loading) return;

        if (_locked)
        {
            if (promptText != null)
            {
                int needed = LootTaskManager.Instance != null
                    ? (LootTaskManager.Instance.RequiredLoots - LootTaskManager.Instance.LootCount)
                    : 0;
                promptText.text = $"🔒 EXIT LOCKED\nCollect {needed} more loots!";
            }
            return;
        }

        holdTimer += Time.deltaTime;
        UpdatePromptText(holdTimer);

        FindPlayerReference();
        if (_playerMovement == null) return;

        // If player has not collected gas (boss not defeated yet), teleport to boss arena
        if (!_playerMovement.hasGas)
        {
            // Teleport to boss arena first
            TeleportPlayer(_playerMovement.transform, new Vector3(1036.271f, 2f, 391.0851f));
            playerInside = false;

            // Show the boss HP bar after teleporting
            // bossHealthBar field points to the panel GO; find the script separately
            if (bossHealthBar != null)
                bossHealthBar.SetActive(true);

            BossHealthBar bhbScript = FindFirstObjectByType<BossHealthBar>(FindObjectsInactive.Include);
            if (bhbScript != null)
            {
                bhbScript.gameObject.SetActive(true);
                bhbScript.Show();
            }

            // Play Boss BGM
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBossMusic();
            }
        }
        else // Player has gas (boss defeated), entering exit 1 triggers win!
        {
            ShowWin();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        holdTimer    = 0f;

        if (_playerMovement == null)
            _playerMovement = other.GetComponent<GamePlayerMovement>() ?? other.GetComponentInParent<GamePlayerMovement>();

        if (exitPromptUI != null)
            exitPromptUI.SetActive(true);

        UpdatePromptText(0f);
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;

        if (_playerMovement == null)
            _playerMovement = other.GetComponent<GamePlayerMovement>() ?? other.GetComponentInParent<GamePlayerMovement>();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        holdTimer    = 0f;

        if (exitPromptUI != null)
            exitPromptUI.SetActive(false);

        UpdatePromptText(0f);
    }

    void UpdatePromptText(float elapsed)
    {
        if (promptText == null) return;

        if (holdDuration <= 0f)
        {
            promptText.text = "EXIT";
        }
        else
        {
            float pct  = Mathf.Clamp01(elapsed / holdDuration);
            int   bars = Mathf.RoundToInt(pct * 10);
            string bar = "[" + new string('|', bars) + new string(' ', 10 - bars) + "]";
            promptText.text = $"STAND TO EXIT\n{bar}";
        }
    }

    public void ShowWin()
    {
        loading = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameOverManager.EnsureEventSystemInputModule();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWinMusic();
        }

        if (winMenu != null)
        {
            // Ensure parent Canvas is active
            Canvas parentCanvas = winMenu.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null)
                parentCanvas.gameObject.SetActive(true);

            winMenu.SetActive(true);

            // Wire buttons inside winMenu
            Button[] buttons = winMenu.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                btn.interactable = true;
                string btnName = btn.name.ToLower();
                if (btnName.Contains("next") || btnName.Contains("sector") || btnName.Contains("continue"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        Time.timeScale = 1f;
                        SceneManager.LoadScene(string.IsNullOrEmpty(nextSceneName) ? "Level2Scene" : nextSceneName);
                    });
                }
                else if (btnName.Contains("menu") || btnName.Contains("main"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        Time.timeScale = 1f;
                        SceneManager.LoadScene("MainMenuScene");
                    });
                }
            }
        }
        else if (LevelCompleteManager.Instance != null)
        {
            LevelCompleteManager.Instance.nextSceneName = nextSceneName;
            LevelCompleteManager.Instance.Show(starsEarned, objectiveCurrent, objectiveTotal);
        }
    }

    // ── Editor gizmo ───────────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        Gizmos.color = new Color(0f, 1f, 0.4f, 0.9f);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
