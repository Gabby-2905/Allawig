using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Boss health bar UI — shown at top of screen when boss is active.
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

    public void SetBoss(MrPebbleBoss boss)
    {
        _boss = boss;
        if (panel != null) panel.SetActive(true);
        if (bossNameText != null) bossNameText.text = "MR. PEBBLE";
        UpdateHealth(boss.currentHealth, boss.maxHealth);
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
