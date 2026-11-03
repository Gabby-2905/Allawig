using UnityEngine;

/// <summary>
/// Place this script on a Medkit GameObject (with a Collider).
/// When interacted with (via 'E' key), it restores player health,
/// plays a sound effect, and destroys itself after the sound finishes playing.
/// </summary>
public class Medkit : MonoBehaviour
{
    [Header("Heal Settings")]
    [Tooltip("Amount of health restored when used.")]
    public float healAmount = 50f;

    [Header("Audio")]
    [Tooltip("AudioSource component. If null, will try to get one on this GameObject.")]
    public AudioSource audioSource;
    [Tooltip("Sound clip played when the medkit is collected.")]
    public AudioClip healSoundClip;

    private bool _collected = false;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Call to collect the medkit and heal the player.
    /// </summary>
    public void Heal(GamePlayerMovement player = null)
    {
        if (_collected) return;
        _collected = true;

        if (player == null)
            player = FindFirstObjectByType<GamePlayerMovement>();

        if (player != null)
        {
            player.Heal(healAmount);
            Debug.Log($"[Medkit] Restored {healAmount} HP! Current HP: {player.currentHealth}/{player.maxHealth}");
        }

        // Play audio if available
        float soundDelay = 0f;
        if (audioSource != null && healSoundClip != null)
        {
            audioSource.PlayOneShot(healSoundClip);
            soundDelay = healSoundClip.length;
        }
        else if (healSoundClip != null)
        {
            AudioSource.PlayClipAtPoint(healSoundClip, transform.position);
            soundDelay = healSoundClip.length;
        }
        else if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
            soundDelay = audioSource.clip.length;
        }

        // Hide visuals and disable colliders so player cannot interact again while sound is playing
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        // Destroy after the sound finishes playing (matching Collectible.cs)
        Destroy(gameObject, soundDelay);
    }
}
