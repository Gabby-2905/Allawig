using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Randomly flashes a UI Image to simulate lightning, and plays thunder audio.
/// Attach to a full-screen UI Image (the lightning flash overlay).
/// </summary>
public class LightningEffect : MonoBehaviour
{
    [Header("Flash Settings")]
    [Tooltip("UI Image used as the white flash overlay")]
    public Image flashImage;

    [Tooltip("Minimum seconds between lightning strikes")]
    public float minInterval = 3f;

    [Tooltip("Maximum seconds between lightning strikes")]
    public float maxInterval = 10f;

    [Tooltip("How quickly the flash fades out")]
    public float fadeDuration = 0.15f;

    [Tooltip("Peak alpha of the flash (0-1)")]
    public float peakAlpha = 0.6f;

    [Header("Thunder Audio")]
    [Tooltip("AudioSource for thunder sounds")]
    public AudioSource thunderSource;

    [Tooltip("Thunder sound clips (one will be chosen at random)")]
    public AudioClip[] thunderClips;

    [Tooltip("Delay between flash and thunder (simulates distance)")]
    public float thunderDelay = 0.4f;

    private Coroutine _routine;

    void OnEnable()
    {
        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }
        _routine = StartCoroutine(LightningLoop());
    }

    void OnDisable()
    {
        if (_routine != null) StopCoroutine(_routine);
    }

    private IEnumerator LightningLoop()
    {
        while (true)
        {
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);

            // Sometimes do a double-flash
            int flashes = Random.value > 0.6f ? 2 : 1;
            for (int i = 0; i < flashes; i++)
            {
                yield return StartCoroutine(DoFlash());
                if (i < flashes - 1) yield return new WaitForSeconds(0.08f);
            }

            // Thunder after a short delay
            if (thunderSource != null && thunderClips != null && thunderClips.Length > 0)
            {
                yield return new WaitForSeconds(thunderDelay);
                AudioClip clip = thunderClips[Random.Range(0, thunderClips.Length)];
                thunderSource.volume = SettingsManager.EffectsVolume;
                thunderSource.PlayOneShot(clip);
            }
        }
    }

    private IEnumerator DoFlash()
    {
        if (flashImage == null) yield break;

        // Instant bright flash
        Color c = flashImage.color;
        c.a = peakAlpha;
        flashImage.color = c;

        // Fade out
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(peakAlpha, 0f, elapsed / fadeDuration);
            flashImage.color = c;
            yield return null;
        }

        c.a = 0f;
        flashImage.color = c;
    }
}
