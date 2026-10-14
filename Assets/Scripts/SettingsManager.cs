using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

/// <summary>
/// Manages game settings: mouse sensitivity, music volume, effects volume.
/// Values are persisted via PlayerPrefs.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Sliders")]
    public Slider sensitivitySlider;
    public Slider musicVolumeSlider;
    public Slider effectsVolumeSlider;

    [Header("Value Labels")]
    public TextMeshProUGUI sensitivityLabel;
    public TextMeshProUGUI musicLabel;
    public TextMeshProUGUI effectsLabel;

    // PlayerPrefs keys
    private const string KEY_SENSITIVITY = "Sensitivity";
    private const string KEY_MUSIC       = "MusicVolume";
    private const string KEY_EFFECTS     = "EffectsVolume";

    // Defaults
    private const float DEFAULT_SENSITIVITY = 1.0f;
    private const float DEFAULT_MUSIC       = 0.8f;
    private const float DEFAULT_EFFECTS     = 0.8f;

    // Public accessors so other scripts can read current values
    public static float Sensitivity  => PlayerPrefs.GetFloat(KEY_SENSITIVITY, DEFAULT_SENSITIVITY);
    public static float MusicVolume  => PlayerPrefs.GetFloat(KEY_MUSIC,       DEFAULT_MUSIC);
    public static float EffectsVolume => PlayerPrefs.GetFloat(KEY_EFFECTS,    DEFAULT_EFFECTS);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void OnEnable()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        float sens    = PlayerPrefs.GetFloat(KEY_SENSITIVITY, DEFAULT_SENSITIVITY);
        float music   = PlayerPrefs.GetFloat(KEY_MUSIC,       DEFAULT_MUSIC);
        float effects = PlayerPrefs.GetFloat(KEY_EFFECTS,     DEFAULT_EFFECTS);

        if (sensitivitySlider  != null) { sensitivitySlider.value  = sens;    UpdateSensitivityLabel(sens); }
        if (musicVolumeSlider  != null) { musicVolumeSlider.value  = music;   UpdateMusicLabel(music); }
        if (effectsVolumeSlider != null) { effectsVolumeSlider.value = effects; UpdateEffectsLabel(effects); }
    }

    // ── Called by slider OnValueChanged events ─────────────────────────────

    public void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(KEY_SENSITIVITY, value);
        PlayerPrefs.Save();
        UpdateSensitivityLabel(value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(KEY_MUSIC, value);
        PlayerPrefs.Save();
        UpdateMusicLabel(value);
        // Apply to all AudioSources tagged as music at runtime
        ApplyMusicVolume(value);
    }

    public void OnEffectsVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(KEY_EFFECTS, value);
        PlayerPrefs.Save();
        UpdateEffectsLabel(value);
        ApplyEffectsVolume(value);
    }

    // ── Label helpers ───────────────────────────────────────────────────────

    private void UpdateSensitivityLabel(float v)
    {
        if (sensitivityLabel != null) sensitivityLabel.text = Mathf.RoundToInt(v * 100) + "%";
    }
    private void UpdateMusicLabel(float v)
    {
        if (musicLabel != null) musicLabel.text = Mathf.RoundToInt(v * 100) + "%";
    }
    private void UpdateEffectsLabel(float v)
    {
        if (effectsLabel != null) effectsLabel.text = Mathf.RoundToInt(v * 100) + "%";
    }

    // ── Volume application ──────────────────────────────────────────────────

    private void ApplyMusicVolume(float value)
    {
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var src in sources)
        {
            if (src.CompareTag("Music")) src.volume = value;
        }
    }

    private void ApplyEffectsVolume(float value)
    {
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var src in sources)
        {
            if (src.CompareTag("SFX")) src.volume = value;
        }
    }
}
