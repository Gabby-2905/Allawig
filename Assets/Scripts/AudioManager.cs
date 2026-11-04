using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central Audio Manager for BGM and Sound Effects.
/// Implements persistent singleton pattern, auto-loads audio clips from Resources,
/// handles music cross-fading (supporting Time.timeScale = 0), and responds to user volume settings.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AudioManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Music Tracks")]
    public AudioClip mainMenuMusic;
    public AudioClip level1Music;
    public AudioClip bossFightMusic;
    public AudioClip winMusic;
    public AudioClip loseMusic;

    [Header("Sound Effects")]
    public AudioClip bossVoiceline;
    public AudioClip hurtSfx;
    public AudioClip punchGruntSfx;
    public AudioClip coinCollectSfx;

    [Header("Volume Defaults")]
    [Range(0f, 1f)] public float defaultMusicVolume = 0.4f;
    [Range(0f, 1f)] public float defaultEffectsVolume = 0.8f;

    private AudioSource _musicSource;
    private AudioSource _sfxSource2D;
    private Coroutine _fadeCoroutine;

    public float CurrentMusicVolume
    {
        get => PlayerPrefs.GetFloat("Settings_MusicVol", defaultMusicVolume);
        set
        {
            PlayerPrefs.SetFloat("Settings_MusicVol", value);
            PlayerPrefs.Save();
            if (_musicSource != null)
                _musicSource.volume = value;
        }
    }

    public float CurrentEffectsVolume
    {
        get => PlayerPrefs.GetFloat("Settings_EffectsVol", defaultEffectsVolume);
        set
        {
            PlayerPrefs.SetFloat("Settings_EffectsVol", value);
            PlayerPrefs.Save();
            if (_sfxSource2D != null)
                _sfxSource2D.volume = value;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitAudioSources();
            LoadClipsIfNull();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Check current scene on initial launch
        CheckSceneMusic(SceneManager.GetActiveScene().name);
    }

    private void InitAudioSources()
    {
        if (_musicSource == null)
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = CurrentMusicVolume;
            try { gameObject.tag = "Music"; } catch { /* Ignore if tag missing */ }
        }

        if (_sfxSource2D == null)
        {
            _sfxSource2D = gameObject.AddComponent<AudioSource>();
            _sfxSource2D.playOnAwake = false;
            _sfxSource2D.loop = false;
            _sfxSource2D.volume = CurrentEffectsVolume;
        }
    }

    private void LoadClipsIfNull()
    {
        if (mainMenuMusic == null)  mainMenuMusic  = Resources.Load<AudioClip>("Music/MainMenuMusic");
        if (level1Music == null)    level1Music    = Resources.Load<AudioClip>("Music/Level1Music");
        if (bossFightMusic == null) bossFightMusic = Resources.Load<AudioClip>("Music/BossFightMusic");
        if (winMusic == null)        winMusic       = Resources.Load<AudioClip>("Music/WinMusic");
        if (loseMusic == null)       loseMusic      = Resources.Load<AudioClip>("Music/LoseMusic");

        if (bossVoiceline == null)  bossVoiceline  = Resources.Load<AudioClip>("Sfx/BossVoiceline1");
        if (hurtSfx == null)        hurtSfx        = Resources.Load<AudioClip>("Sfx/Hurtsfx");
        if (punchGruntSfx == null)  punchGruntSfx  = Resources.Load<AudioClip>("Sfx/PunchGruntSfx");
        if (coinCollectSfx == null) coinCollectSfx = Resources.Load<AudioClip>("Sfx/Coin_Collect_Audio");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckSceneMusic(scene.name);
    }

    private void CheckSceneMusic(string sceneName)
    {
        string s = sceneName.ToLower();
        if (s.Contains("mainmenu"))
        {
            // If the scene already has an active separate AudioSource playing MainMenuMusic, don't duplicate
            AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (var src in sources)
            {
                if (src != _musicSource && src.isPlaying && (src.clip == mainMenuMusic || (src.clip != null && src.clip.name.Contains("MainMenu"))))
                {
                    return;
                }
            }
            PlayMainMenuMusic();
        }
        else if (s.Contains("game") || s.Contains("level1") || s.Contains("gas"))
        {
            PlayLevel1Music();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  MUSIC PLAYBACK
    // ─────────────────────────────────────────────────────────────────────

    public void PlayLevel1Music(float fadeDuration = 0.4f)
    {
        if (level1Music == null) LoadClipsIfNull();
        PlayMusic(level1Music, true, fadeDuration);
    }

    public void PlayBossMusic(float fadeDuration = 0.4f)
    {
        if (bossFightMusic == null) LoadClipsIfNull();
        PlayMusic(bossFightMusic, true, fadeDuration);
    }

    public void PlayMainMenuMusic(float fadeDuration = 0.4f)
    {
        if (mainMenuMusic == null) LoadClipsIfNull();
        PlayMusic(mainMenuMusic, true, fadeDuration);
    }

    public void PlayWinMusic()
    {
        if (winMusic == null) LoadClipsIfNull();
        // Win music plays over or replaces BGM, non-looping
        PlayMusic(winMusic, false, 0.2f);
    }

    public void PlayLoseMusic()
    {
        if (loseMusic == null) LoadClipsIfNull();
        // Lose music plays over or replaces BGM, non-looping
        PlayMusic(loseMusic, false, 0.2f);
    }

    public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 0.4f)
    {
        if (clip == null) return;
        if (_musicSource == null) InitAudioSources();

        // If the same clip is already playing, simply ensure it is unpaused
        if (_musicSource.clip == clip && _musicSource.isPlaying)
        {
            _musicSource.loop = loop;
            return;
        }

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeToTrack(clip, loop, fadeDuration));
    }

    public void StopMusic(float fadeDuration = 0.4f)
    {
        if (_musicSource == null || !_musicSource.isPlaying) return;

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeOut(fadeDuration));
    }

    private IEnumerator FadeToTrack(AudioClip newClip, bool loop, float duration)
    {
        float targetVolume = CurrentMusicVolume;

        // Fade out current track
        if (_musicSource.isPlaying && duration > 0f)
        {
            float startVol = _musicSource.volume;
            float elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                _musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / (duration * 0.5f));
                yield return null;
            }
        }

        _musicSource.clip = newClip;
        _musicSource.loop = loop;
        _musicSource.Play();

        // Fade in new track
        if (duration > 0f)
        {
            float elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                _musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (duration * 0.5f));
                yield return null;
            }
        }

        _musicSource.volume = targetVolume;
        _fadeCoroutine = null;
    }

    private IEnumerator FadeOut(float duration)
    {
        float startVol = _musicSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }
        _musicSource.Stop();
        _musicSource.clip = null;
        _fadeCoroutine = null;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  SFX PLAYBACK
    // ─────────────────────────────────────────────────────────────────────

    public void PlayHurtSFX(Vector3 position = default)
    {
        if (hurtSfx == null) LoadClipsIfNull();
        PlaySFX(hurtSfx, 0.7f, position);
    }

    public void PlayPunchGruntSFX(Vector3 position = default)
    {
        if (punchGruntSfx == null) LoadClipsIfNull();
        PlaySFX(punchGruntSfx, 0.6f, position);
    }

    public void PlayBossVoiceline(Vector3 position = default)
    {
        if (bossVoiceline == null) LoadClipsIfNull();
        PlaySFX(bossVoiceline, 1.0f, position);
    }

    public void PlayCoinCollectSFX(Vector3 position = default)
    {
        if (coinCollectSfx == null) LoadClipsIfNull();
        PlaySFX(coinCollectSfx, 1.0f, position);
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1.0f, Vector3 position = default)
    {
        if (clip == null) return;
        if (_sfxSource2D == null) InitAudioSources();

        float finalVolume = volumeScale * CurrentEffectsVolume;

        if (position != default)
        {
            AudioSource.PlayClipAtPoint(clip, position, finalVolume);
        }
        else
        {
            _sfxSource2D.PlayOneShot(clip, finalVolume);
        }
    }
}
