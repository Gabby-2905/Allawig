using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Tracks how many loot containers the player has looted.
/// Locks all ExitPoints until the required loot count is reached.
/// Shows a HUD counter and a locked/unlocked status message.
/// </summary>
public class LootTaskManager : MonoBehaviour
{
    public static LootTaskManager Instance { get; private set; }

    [Header("Task Settings")]
    public int requiredLoots = 10;

    [Header("HUD References")]
    public TextMeshProUGUI lootCounterText;   // e.g. "LOOTS: 3 / 10"
    public TextMeshProUGUI taskStatusText;    // e.g. "EXIT LOCKED" / "EXIT UNLOCKED!"
    public GameObject taskPanel;              // optional panel background

    [Header("Exit Points")]
    public ExitPoint[] exitPoints;            // drag all ExitPoints here

    // ── State ──────────────────────────────────────────────────────────────
    private int _lootCount = 0;
    private bool _exitUnlocked = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Auto-find exit points if not assigned
        if (exitPoints == null || exitPoints.Length == 0)
            exitPoints = FindObjectsByType<ExitPoint>(FindObjectsSortMode.None);

        LockExits();
        RefreshHUD();
    }

    /// <summary>Called by LootContainer when the player loots it.</summary>
    public void RegisterLoot()
    {
        if (_exitUnlocked) return; // already done

        _lootCount++;
        Debug.Log($"[LootTask] Loots: {_lootCount} / {requiredLoots}");

        if (_lootCount >= requiredLoots)
        {
            _exitUnlocked = true;
            UnlockExits();
        }

        RefreshHUD();
    }

    void LockExits()
    {
        if (exitPoints == null) return;
        foreach (var ep in exitPoints)
            if (ep != null) ep.SetLocked(true);
    }

    void UnlockExits()
    {
        if (exitPoints == null) return;
        foreach (var ep in exitPoints)
            if (ep != null) ep.SetLocked(false);

        if (taskStatusText != null)
        {
            taskStatusText.text = "✓ EXIT UNLOCKED!";
            taskStatusText.color = new Color(0.2f, 1f, 0.4f, 1f);
        }
    }

    void RefreshHUD()
    {
        if (lootCounterText != null)
            lootCounterText.text = $"LOOTS: {_lootCount} / {requiredLoots}";

        if (taskStatusText != null && !_exitUnlocked)
        {
            int remaining = requiredLoots - _lootCount;
            taskStatusText.text = $"🔒 EXIT LOCKED  ({remaining} more loots needed)";
            taskStatusText.color = new Color(1f, 0.4f, 0.1f, 1f);
        }
    }

    public int LootCount => _lootCount;
    public bool ExitUnlocked => _exitUnlocked;
    public int RequiredLoots => requiredLoots;
}
