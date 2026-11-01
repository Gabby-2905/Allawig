using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int lootValue = 1;
    public AudioSource audioSource;
    public AudioClip lootCollectClip;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Add loot value to player's inventory or score
            // Example: PlayerInventory.AddLoot(lootValue);
            audioSource.PlayOneShot(lootCollectClip);
            LootTaskManager.Instance.AddLoot(lootValue); // Assuming you have a GameManager to handle loot
            Destroy(gameObject, lootCollectClip.length); // Destroy after the sound plays
        }
    }

}

