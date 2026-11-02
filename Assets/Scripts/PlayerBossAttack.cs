using UnityEngine;

/// <summary>
/// Attach to the Player.
/// Detects when the player punches and deals damage to MrPebbleBoss
/// if the boss is within melee range and in front of the player.
///
/// Works by listening to the Animator's PunchLeft / PunchRight triggers
/// via Animation Events, OR by polling the animator state each frame.
/// </summary>
public class PlayerBossAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("Damage dealt per punch to the boss.")]
    public float punchDamage    = 20f;

    [Tooltip("How far in front of the player the punch reaches.")]
    public float punchRange     = 3.5f;

    [Tooltip("Half-angle of the punch cone (degrees). 60 = 120° arc in front.")]
    public float punchAngle     = 60f;

    [Tooltip("Cooldown between registering hits (prevents multi-hit per swing).")]
    public float hitCooldown    = 0.5f;

    // ── Private ────────────────────────────────────────────────────────────
    private MrPebbleBoss      _boss;
    private GamePlayerMovement _movement;
    private Animator           _animator;
    private float              _hitTimer = 0f;

    // Animator state hashes
    private static readonly int _hashPunchLeft  = Animator.StringToHash("PunchLeft");
    private static readonly int _hashPunchRight = Animator.StringToHash("PunchRight");

    void Start()
    {
        _movement = GetComponent<GamePlayerMovement>();
        _animator = GetComponentInChildren<Animator>();

        // Find boss in scene
        _boss = FindFirstObjectByType<MrPebbleBoss>();
        if (_boss == null)
            Debug.LogWarning("[PlayerBossAttack] No MrPebbleBoss found in scene.");
    }

    void Update()
    {
        if (_boss == null || _boss.currentState == MrPebbleBoss.BossState.Dead) return;

        _hitTimer -= Time.deltaTime;

        // Detect punch input (left-click while not aiming, or aiming)
        bool punchPressed = Input.GetMouseButtonDown(0);

        if (punchPressed && _hitTimer <= 0f)
        {
            TryHitBoss();
        }
    }

    /// <summary>
    /// Called by Animation Events on the punch animations (optional).
    /// Attach this as an Animation Event named "OnPunchHit" at the impact frame.
    /// </summary>
    public void OnPunchHit()
    {
        if (_boss == null) return;
        if (_hitTimer > 0f) return;
        TryHitBoss();
    }

    void TryHitBoss()
    {
        if (_boss == null) return;

        Vector3 toBoss = _boss.transform.position - transform.position;
        float   dist   = toBoss.magnitude;

        if (dist > punchRange) return;

        // Check if boss is within the punch cone (in front of player)
        float angle = Vector3.Angle(transform.forward, toBoss.normalized);
        if (angle > punchAngle) return;

        // Hit!
        _hitTimer = hitCooldown;
        _boss.TakeDamage(punchDamage);

        Debug.Log($"[PlayerBossAttack] Punched Mr Pebble for {punchDamage} dmg! " +
                  $"(dist={dist:F1}, angle={angle:F1}°)" +
                  (_boss.currentState == MrPebbleBoss.BossState.Vulnerable ? " [VULNERABLE – 2× dmg applied by boss]" : ""));
    }

    // ── Gizmos ─────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, punchRange);

        // Draw punch cone
        Vector3 leftDir  = Quaternion.Euler(0, -punchAngle, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0,  punchAngle, 0) * transform.forward;
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.6f);
        Gizmos.DrawRay(transform.position, leftDir  * punchRange);
        Gizmos.DrawRay(transform.position, rightDir * punchRange);
        Gizmos.DrawRay(transform.position, transform.forward * punchRange);
    }
}
