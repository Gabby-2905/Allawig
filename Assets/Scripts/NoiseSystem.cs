using UnityEngine;

/// <summary>
/// Singleton that tracks the player's current noise level (0-1).
/// NPCs query this to determine if they can hear the player.
/// </summary>
public class NoiseSystem : MonoBehaviour
{
    public static NoiseSystem Instance { get; private set; }

    [Header("Noise Levels (0 = silent, 1 = loud)")]
    [Tooltip("Noise emitted while standing still")]
    public float idleNoise = 0.05f;

    [Tooltip("Noise emitted while walking")]
    public float walkNoise = 0.4f;

    [Tooltip("Noise emitted while running/sprinting")]
    public float runNoise = 1.0f;

    [Tooltip("Noise emitted while crouching and still")]
    public float crouchIdleNoise = 0.0f;

    [Tooltip("Noise emitted while crouching and moving")]
    public float crouchWalkNoise = 0.15f;

    [Header("Noise Radius Multiplier")]
    [Tooltip("Base hearing radius of NPCs is multiplied by this noise level")]
    public float baseHearingRadius = 12f;

    // Current noise level this frame
    public float CurrentNoise { get; private set; }

    // Effective hearing radius based on current noise
    public float EffectiveHearingRadius => baseHearingRadius * CurrentNoise;

    private GamePlayerMovement _player;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _player = FindFirstObjectByType<GamePlayerMovement>();
    }

    void Update()
    {
        if (_player == null) { CurrentNoise = 0f; return; }

        bool isCrouching = _player.IsCrouching;
        bool isSprinting = _player.IsSprinting;
        bool isMoving    = _player.IsMoving;

        if (isCrouching)
            CurrentNoise = isMoving ? crouchWalkNoise : crouchIdleNoise;
        else if (isSprinting)
            CurrentNoise = runNoise;
        else if (isMoving)
            CurrentNoise = walkNoise;
        else
            CurrentNoise = idleNoise;
    }
}
