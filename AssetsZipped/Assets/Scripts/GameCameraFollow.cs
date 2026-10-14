using UnityEngine;

public class GameCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 13f, -9f);
    public float smoothSpeed = 8f;

    void Start()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        // Look at a point slightly in front of and above the player
        transform.LookAt(target.position + new Vector3(0f, 0.9f, 4f));
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + new Vector3(0f, 0.9f, 4f));
    }
}
