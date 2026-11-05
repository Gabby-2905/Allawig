using UnityEngine;

/// <summary>
/// Collectible loot item. When collected, adds to LootTaskManager counter,
/// stores the item in InventoryUI, plays collect audio, and destroys itself.
/// </summary>
public class Collectible : MonoBehaviour
{
    [Header("Loot Info")]
    public string itemName = "Ration Pack";
    public int lootValue = 1;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip lootCollectClip;

    private bool _collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_collected) return;

        if (other.gameObject.CompareTag("Player") || other.GetComponent<GamePlayerMovement>() != null)
        {
            _collected = true;

            // 1. Play sound
            float soundDuration = 0.3f;
            if (audioSource != null && lootCollectClip != null)
            {
                audioSource.PlayOneShot(lootCollectClip);
                soundDuration = lootCollectClip.length;
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoinCollectSFX(transform.position);
                soundDuration = 0.5f;
            }

            // 2. Add to LootTaskManager
            if (LootTaskManager.Instance != null)
            {
                LootTaskManager.Instance.AddLoot(lootValue);
            }

            // 3. Add to player Inventory
            if (InventoryUI.Instance != null)
            {
                string nameToStore = string.IsNullOrEmpty(itemName) ? "Rations" : itemName;
                InventoryUI.Instance.AddItem(nameToStore, lootValue);
            }

            // 4. Hide visuals immediately so player cannot trigger again while sound plays
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = false;
            foreach (var c in GetComponentsInChildren<Collider>())
                c.enabled = false;

            Destroy(gameObject, soundDuration);
        }
    }
}
