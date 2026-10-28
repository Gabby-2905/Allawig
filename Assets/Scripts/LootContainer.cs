using UnityEngine;
using TMPro;

/// <summary>
/// Attach to any loot crate/box in the world.
/// Player walks up and presses E to loot it.
/// Shows a prompt UI hint when nearby.
/// </summary>
public class LootContainer : MonoBehaviour
{
    [Header("Loot Settings")]
    public string containerName = "Supply Crate";
    public bool isLooted = false;

    [Header("UI")]
    public GameObject interactHint;   // "Press E to Loot" world-space label (optional)

    [Header("Visual")]
    public GameObject lootedVisual;   // Optional: swap to open-box mesh when looted
    public GameObject unlootedVisual; // Optional: closed-box mesh

    [Header("Audio")]
    public AudioClip lootSound;

    private bool playerNearby = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (interactHint != null) interactHint.SetActive(false);
        if (lootedVisual != null) lootedVisual.SetActive(false);
    }

    void Update()
    {
        if (playerNearby && !isLooted && Input.GetKeyDown(KeyCode.E))
        {
            Loot();
        }
    }

    void Loot()
    {
        isLooted = true;
        Debug.Log($"[LootContainer] Looted: {containerName}");

        if (interactHint != null) interactHint.SetActive(false);

        // Swap visuals
        if (lootedVisual != null)   lootedVisual.SetActive(true);
        if (unlootedVisual != null) unlootedVisual.SetActive(false);

        // Play sound
        if (audioSource != null && lootSound != null)
            audioSource.PlayOneShot(lootSound);

        // Log loot notification
        Debug.Log($"[LootContainer] {containerName} has been looted!");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        playerNearby = true;
        if (!isLooted && interactHint != null)
            interactHint.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        playerNearby = false;
        if (interactHint != null)
            interactHint.SetActive(false);
    }

    bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player")
            || other.name == "Player"
            || other.GetComponent<GamePlayerMovement>() != null;
    }
}
