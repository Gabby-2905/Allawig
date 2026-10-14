using UnityEngine;

/// <summary>
/// Keeps the rain particle system centered above the player.
/// </summary>
public class RainFollower : MonoBehaviour
{
    public Transform target;
    public float height = 15f;

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = new Vector3(target.position.x, height, target.position.z);
    }
}
