using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Displays a stamina bar that drains left-to-right as the player sprints.
/// Fades out when stamina is full and the player is not sprinting.
public class SprintMeterUI : MonoBehaviour
{
    [Header("References")]
    public GamePlayerMovement player;
    public Image  fillImage;
    public Image  backgroundImage;
    public Image  borderImage;
    public TextMeshProUGUI labelText;
    public CanvasGroup canvasGroup;

    [Header("Colors")]
    public Color fullColor  = new Color(0.18f, 0.82f, 0.34f, 1f);  // green
    public Color midColor   = new Color(0.95f, 0.75f, 0.10f, 1f);  // yellow
    public Color lowColor   = new Color(0.95f, 0.35f, 0.10f, 1f);  // orange-red
    public Color emptyColor = new Color(0.75f, 0.08f, 0.08f, 1f);  // red
    [Range(0f, 1f)] public float lowThreshold = 0.25f;
    [Range(0f, 1f)] public float midThreshold = 0.60f;

    [Header("Pulse when empty")]
    public float pulseSpeed = 4f;

    [Header("Fade")]
    public float fadeInSpeed  = 6f;
    public float fadeOutSpeed = 2f;
    public float fadeOutDelay = 2f;   // seconds after stamina is full before fading

    private float _fadeTimer;
    private bool  _wasSprinting;

    void Awake()
    {
        if (player == null)
            player = FindAnyObjectByType<GamePlayerMovement>();
    }

    void Update()
    {
        if (player == null || canvasGroup == null) return;

        float ratio = Mathf.Clamp01(player.CurrentStamina / player.MaxStamina);

        // ── Fill amount (drains left → right, so fillAmount decreases) ──
        if (fillImage != null)
        {
            fillImage.fillAmount = ratio;
            fillImage.color      = GetBarColor(ratio);
        }

        // ── Pulse border when empty ──
        if (borderImage != null)
        {
            if (ratio <= 0.01f)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                borderImage.color = Color.Lerp(emptyColor, Color.white, pulse * 0.5f);
            }
            else
            {
                borderImage.color = new Color(1f, 1f, 1f, 0.35f);
            }
        }

        // ── Label ──
        if (labelText != null)
            labelText.text = $"STAMINA  {Mathf.RoundToInt(ratio * 100)}%";

        // ── Fade logic ──
        bool sprinting = player.IsSprinting;
        bool staminaFull = ratio >= 0.999f;

        if (sprinting || !staminaFull)
        {
            _fadeTimer = fadeOutDelay;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, fadeInSpeed * Time.deltaTime);
        }
        else
        {
            _fadeTimer -= Time.deltaTime;
            if (_fadeTimer <= 0f)
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeOutSpeed * Time.deltaTime);
        }

        _wasSprinting = sprinting;
    }

    Color GetBarColor(float ratio)
    {
        if (ratio <= lowThreshold)
            return Color.Lerp(emptyColor, lowColor, ratio / lowThreshold);
        else if (ratio <= midThreshold)
            return Color.Lerp(lowColor, midColor, (ratio - lowThreshold) / (midThreshold - lowThreshold));
        else
            return Color.Lerp(midColor, fullColor, (ratio - midThreshold) / (1f - midThreshold));
    }
}
