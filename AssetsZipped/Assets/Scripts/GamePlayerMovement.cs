using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GamePlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Animation")]
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;
    private float gravity = -20f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        // Auto-find animator on child Visual object if not assigned
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Block movement while paused
        if (Time.timeScale == 0f) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Move relative to camera-forward projected on XZ plane
        Camera cam = Camera.main;
        Vector3 camForward = cam ? cam.transform.forward : Vector3.forward;
        camForward.y = 0f;
        camForward.Normalize();
        Vector3 camRight = cam ? cam.transform.right : Vector3.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 move = (camForward * v + camRight * h).normalized;

        if (move.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Drive animator Speed (0 = idle, 1 = running)
        if (animator != null)
            animator.SetFloat("Speed", move.magnitude, 0.1f, Time.deltaTime);

        // Gravity
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;

        controller.Move((move * moveSpeed + velocity) * Time.deltaTime);
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
    }
}
