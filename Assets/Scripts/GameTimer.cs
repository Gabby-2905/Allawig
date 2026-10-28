using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Counts down from a set duration. When it reaches zero, triggers the
/// GameOverManager to show the lose screen.
/// </summary>
public class GameTimer : MonoBehaviour
{
    [Header("Settings")]
    public float totalTime = 180f;   // 1 minute 30 seconds

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public Image timerBarFill;      // optional fill bar

    [Header("Warning")]
    public float warningThreshold = 20f;   // flash red below this
    public Color normalColor  = new Color(1f, 1f, 1f, 1f);
    public Color warningColor = new Color(1f, 0.25f, 0.15f, 1f);

    // ── State ──────────────────────────────────────────────────────────────
    private float timeRemaining;
    private bool  isRunning = false;
    private bool  isWarning = false;

    // ── Events ─────────────────────────────────────────────────────────────
    public System.Action OnTimeUp;

    // ── Public accessors ───────────────────────────────────────────────────
    public float TimeRemaining => timeRemaining;
    public bool  IsRunning     => isRunning;

    void Start()
    {
        timeRemaining = totalTime;
        isRunning = true;
        UpdateDisplay();
    }

    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            UpdateDisplay();
            OnTimeUp?.Invoke();
            return;
        }

        UpdateDisplay();

        // Warning flash when low
        if (timeRemaining <= warningThreshold && !isWarning)
        {
            isWarning = true;
            StartCoroutine(FlashWarning());
        }
    }

    void UpdateDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        if (timerText != null)
            timerText.text = $"{minutes}:{seconds:D2}";

        if (timerBarFill != null)
            timerBarFill.fillAmount = timeRemaining / totalTime;
    }

    IEnumerator FlashWarning()
    {
        while (isRunning && timeRemaining <= warningThreshold)
        {
            if (timerText != null)
                timerText.color = warningColor;
            yield return new WaitForSeconds(0.4f);
            if (timerText != null)
                timerText.color = normalColor;
            yield return new WaitForSeconds(0.4f);
        }
        if (timerText != null)
            timerText.color = normalColor;
    }

    public void PauseTimer()  => isRunning = false;
    public void ResumeTimer() => isRunning = true;
    public void ResetTimer()
    {
        timeRemaining = totalTime;
        isRunning = true;
        isWarning = false;
        UpdateDisplay();
    }
}
