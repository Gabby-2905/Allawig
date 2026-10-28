using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Place this on a trigger collider in the scene.
/// When the player enters it, a short "hold to exit" prompt appears
/// and then the Level Complete screen is shown (or the next level is loaded).
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

    void Start()
    {
        if (exitPromptUI != null)
            exitPromptUI.SetActive(false);

        if (exitLight != null)
            exitLight.color = exitColor;

        UpdatePromptText(0f);
    }

    void Update()
    {
        if (!playerInside || loading) return;

        holdTimer += Time.deltaTime;
        UpdatePromptText(holdTimer);

        if (holdTimer >= holdDuration)
        {
            loading = true;
            StartCoroutine(TriggerExit());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        holdTimer    = 0f;

        if (exitPromptUI != null)
            exitPromptUI.SetActive(true);

        UpdatePromptText(0f);
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

    IEnumerator TriggerExit()
    {
        // Brief pause so the player sees the bar fill
        yield return new WaitForSeconds(0.3f);

        // Hide the prompt
        if (exitPromptUI != null)
            exitPromptUI.SetActive(false);

        if (showLevelCompleteUI && LevelCompleteManager.Instance != null)
        {
            // Show Level Complete screen
            LevelCompleteManager.Instance.nextSceneName = nextSceneName;
            LevelCompleteManager.Instance.Show(starsEarned, objectiveCurrent, objectiveTotal);
        }
        else
        {
            // Fallback: load next scene directly
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextSceneName);
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
