using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Boss health bar UI — shown at top of screen when boss is active.
/// Starts hidden by default until the player teleports to the boss arena.
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Image healthFill;
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI healthValueText;
    public TextMeshProUGUI stateText;
    public GameObject panel;

    private MrPebbleBoss _boss;
    private bool _hasBeenShown = false;

    void Awake()
    {
        if (!_hasBeenShown)
            Hide();
    }

    void Start()
    {
        if (!_hasBeenShown)
            Hide();
    }

    public void Show()
    {
        _hasBeenShown = true;
        gameObject.SetActive(true);
        if (panel != null)
            panel.SetActive(true);
    }

    public void Hide()
    {
        _hasBeenShown = false;
        if (panel != null && panel != gameObject)
            panel.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    public void SetBoss(MrPebbleBoss boss)
    {
        _boss = boss;
        if (bossNameText != null) bossNameText.text = "MR. PEBBLE";
        UpdateHealth(boss.currentHealth, boss.maxHealth);
        // Boss health bar remains hidden until player teleports to boss arena
    }

    public void UpdateHealth(float current, float max)
    {
        float ratio = Mathf.Clamp01(current / max);
        if (healthFill != null) healthFill.fillAmount = ratio;
        if (healthValueText != null)
            healthValueText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";

        // Color: green → yellow → red
        if (healthFill != null)
        {
            if (ratio > 0.5f)
                healthFill.color = Color.Lerp(new Color(1f, 0.8f, 0f), new Color(0.2f, 0.9f, 0.2f), (ratio - 0.5f) * 2f);
            else
                healthFill.color = Color.Lerp(new Color(0.9f, 0.1f, 0.1f), new Color(1f, 0.8f, 0f), ratio * 2f);
        }
    }

    void Update()
    {
        if (_boss == null || stateText == null) return;
        string stateLabel = _boss.currentState switch
        {
            MrPebbleBoss.BossState.BatoSlam   => "⚡ BATO SLAM!",
            MrPebbleBoss.BossState.BatoCharge => "💨 BATO CHARGE!",
            MrPebbleBoss.BossState.Vulnerable  => "⚠ VULNERABLE!",
            MrPebbleBoss.BossState.Dead        => "☠ DEFEATED",
            _                                  => ""
        };
        stateText.text = stateLabel;
        stateText.color = _boss.currentState == MrPebbleBoss.BossState.Vulnerable
            ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.4f, 0.1f);
    }
}
