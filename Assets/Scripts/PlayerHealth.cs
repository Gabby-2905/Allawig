using UnityEngine;

/// <summary>
/// Simpleng halimbawa lang. Palitan o i-merge sa sarili mong player health script.
/// Ang mahalaga: may public TakeDamage(int) at Tag na "Player" ang player.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public float invincibleTime = 0.8f;
    int health;
    float lastHitTime = -10f;

    void Awake() { health = maxHealth; }

    public void TakeDamage(int amount)
    {
        if (Time.time - lastHitTime < invincibleTime) return; // i-frames
        lastHitTime = Time.time;
        health -= amount;
        Debug.Log($"Player HP: {health}");
        if (health <= 0) Debug.Log("Player died!");
    }
}
