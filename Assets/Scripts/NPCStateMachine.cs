using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Finite State Machine for NPC Bandits.
/// States: Patrol → Investigate → Chase → Attack → ReturnToPatrol
/// Detection uses both sight (line-of-sight + range) and hearing (NoiseSystem).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NPCStateMachine : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────
    //  ENUMS
    // ─────────────────────────────────────────────────────────────────────
    public enum NPCState
    {
        Patrol,
        Investigate,
        Chase,
        Attack,
        ReturnToPatrol
    }

    // ─────────────────────────────────────────────────────────────────────
    //  INSPECTOR
    // ─────────────────────────────────────────────────────────────────────
    [Header("State")]
    public NPCState currentState = NPCState.Patrol;

    [Header("Patrol")]
    [Tooltip("Waypoints the NPC walks between. If empty, NPC wanders randomly.")]
    public Transform[] patrolWaypoints;
    public float patrolSpeed    = 2f;
    public float waypointRadius = 1.5f;

    [Header("Detection – Sight")]
    public float sightRange  = 14f;
    public float sightAngle  = 110f;   // full cone angle in degrees
    public float attackRange = 2.2f;

    [Header("Detection – Hearing")]
    [Tooltip("NPC hears the player when EffectiveHearingRadius >= distance to player")]
    public float hearingMultiplier = 1.0f;

    [Header("Chase / Attack")]
    public float chaseSpeed   = 5f;
    public float attackCooldown = 1.4f;
    public float attackDamage   = 10f;

    [Header("Investigate")]
    public float investigateDuration = 4f;

    [Header("Return")]
    public float returnSpeed = 3f;

    [Header("Debug")]
    public bool showGizmos = true;

    // ─────────────────────────────────────────────────────────────────────
    //  PRIVATE
    // ─────────────────────────────────────────────────────────────────────
    private NavMeshAgent    _agent;
    private Animator        _animator;
    private Transform       _player;
    private GamePlayerMovement _playerMovement;

    private int   _waypointIndex   = 0;
    private float _stateTimer      = 0f;
    private float _attackTimer     = 0f;
    private Vector3 _lastHeardPos  = Vector3.zero;
    private bool  _hasLastHeardPos = false;

    // Animator parameter hashes
    private static readonly int HashSpeed    = Animator.StringToHash("Speed");
    private static readonly int HashWalk     = Animator.StringToHash("Walk");
    private static readonly int HashAttack   = Animator.StringToHash("Attack");

    // ─────────────────────────────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        _agent    = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            _player         = playerGO.transform;
            _playerMovement = playerGO.GetComponent<GamePlayerMovement>();
        }

        _agent.speed = patrolSpeed;
        EnterState(currentState);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        _attackTimer -= Time.deltaTime;
        _stateTimer  -= Time.deltaTime;

        switch (currentState)
        {
            case NPCState.Patrol:         UpdatePatrol();         break;
            case NPCState.Investigate:    UpdateInvestigate();    break;
            case NPCState.Chase:          UpdateChase();          break;
            case NPCState.Attack:         UpdateAttack();         break;
            case NPCState.ReturnToPatrol: UpdateReturnToPatrol(); break;
        }

        UpdateAnimator();
    }

    // ─────────────────────────────────────────────────────────────────────
    //  STATE MACHINE – ENTER
    // ─────────────────────────────────────────────────────────────────────
    void EnterState(NPCState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case NPCState.Patrol:
                _agent.speed = patrolSpeed;
                _agent.isStopped = false;
                SetNextWaypoint();
                break;

            case NPCState.Investigate:
                _agent.speed = patrolSpeed;
                _agent.isStopped = false;
                _stateTimer = investigateDuration;
                if (_hasLastHeardPos)
                    _agent.SetDestination(_lastHeardPos);
                break;

            case NPCState.Chase:
                _agent.speed = chaseSpeed;
                _agent.isStopped = false;
                break;

            case NPCState.Attack:
                _agent.isStopped = true;
                _agent.velocity  = Vector3.zero;
                break;

            case NPCState.ReturnToPatrol:
                _agent.speed = returnSpeed;
                _agent.isStopped = false;
                SetNextWaypoint();
                break;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  STATE MACHINE – UPDATE
    // ─────────────────────────────────────────────────────────────────────
    void UpdatePatrol()
    {
        // Check for player detection first
        if (CanSeePlayer())   { EnterState(NPCState.Chase);       return; }
        if (CanHearPlayer())  { EnterState(NPCState.Investigate); return; }

        // Advance waypoint
        if (!_agent.pathPending && _agent.remainingDistance < waypointRadius)
            SetNextWaypoint();
    }

    void UpdateInvestigate()
    {
        if (CanSeePlayer()) { EnterState(NPCState.Chase); return; }

        // Arrived at heard position – look around then return
        if (!_agent.pathPending && _agent.remainingDistance < waypointRadius)
        {
            if (_stateTimer <= 0f)
                EnterState(NPCState.ReturnToPatrol);
        }

        // If we hear the player again, update destination
        if (CanHearPlayer())
        {
            _stateTimer = investigateDuration;
            _agent.SetDestination(_lastHeardPos);
        }
    }

    void UpdateChase()
    {
        if (_player == null) { EnterState(NPCState.ReturnToPatrol); return; }

        float dist = Vector3.Distance(transform.position, _player.position);

        // Lost sight and hearing
        if (!CanSeePlayer() && !CanHearPlayer())
        {
            _hasLastHeardPos = true;
            _lastHeardPos    = _player.position;
            EnterState(NPCState.Investigate);
            return;
        }

        // Close enough to attack
        if (dist <= attackRange)
        {
            EnterState(NPCState.Attack);
            return;
        }

        _agent.SetDestination(_player.position);
    }

    void UpdateAttack()
    {
        if (_player == null) { EnterState(NPCState.ReturnToPatrol); return; }

        float dist = Vector3.Distance(transform.position, _player.position);

        // Face the player
        Vector3 dir = (_player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, Quaternion.LookRotation(dir), 360f * Time.deltaTime);

        // Player moved away
        if (dist > attackRange * 1.5f)
        {
            EnterState(NPCState.Chase);
            return;
        }

        // Lost sight
        if (!CanSeePlayer())
        {
            EnterState(NPCState.Investigate);
            return;
        }

        // Perform attack
        if (_attackTimer <= 0f)
        {
            _attackTimer = attackCooldown;
            PerformAttack();
        }
    }

    void UpdateReturnToPatrol()
    {
        if (CanSeePlayer()) { EnterState(NPCState.Chase);       return; }
        if (CanHearPlayer()) { EnterState(NPCState.Investigate); return; }

        if (!_agent.pathPending && _agent.remainingDistance < waypointRadius)
            EnterState(NPCState.Patrol);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  DETECTION
    // ─────────────────────────────────────────────────────────────────────
    bool CanSeePlayer()
    {
        if (_player == null) return false;

        Vector3 toPlayer = _player.position - transform.position;
        float dist = toPlayer.magnitude;

        if (dist > sightRange) return false;

        // Cone check
        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > sightAngle * 0.5f) return false;

        // Line-of-sight raycast
        Vector3 eyePos = transform.position + Vector3.up * 1.6f;
        Vector3 playerChest = _player.position + Vector3.up * 1.0f;
        if (Physics.Linecast(eyePos, playerChest, out RaycastHit hit))
        {
            if (!hit.transform.IsChildOf(_player) && hit.transform != _player)
                return false;
        }

        return true;
    }

    bool CanHearPlayer()
    {
        if (_player == null || NoiseSystem.Instance == null) return false;

        float dist = Vector3.Distance(transform.position, _player.position);
        float hearRadius = NoiseSystem.Instance.EffectiveHearingRadius * hearingMultiplier;

        if (dist <= hearRadius)
        {
            _hasLastHeardPos = true;
            _lastHeardPos    = _player.position;
            return true;
        }
        return false;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  ATTACK
    // ─────────────────────────────────────────────────────────────────────
    void PerformAttack()
    {
        if (_animator != null)
            _animator.SetTrigger(HashAttack);

        if (_playerMovement != null)
            _playerMovement.TakeDamage(attackDamage);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  PATROL WAYPOINTS
    // ─────────────────────────────────────────────────────────────────────
    void SetNextWaypoint()
    {
        if (patrolWaypoints != null && patrolWaypoints.Length > 0)
        {
            _agent.SetDestination(patrolWaypoints[_waypointIndex].position);
            _waypointIndex = (_waypointIndex + 1) % patrolWaypoints.Length;
        }
        else
        {
            // Random wander within a radius
            Vector3 randomDir = Random.insideUnitSphere * 8f;
            randomDir += transform.position;
            randomDir.y = transform.position.y;
            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, 8f, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  ANIMATOR
    // ─────────────────────────────────────────────────────────────────────
    void UpdateAnimator()
    {
        if (_animator == null) return;

        float speed = _agent.velocity.magnitude;

        // Use Speed float if available, else Walk bool
        bool hasSpeed = false;
        foreach (var p in _animator.parameters)
        {
            if (p.nameHash == HashSpeed) { hasSpeed = true; break; }
        }

        if (hasSpeed)
        {
            _animator.SetFloat(HashSpeed, speed, 0.1f, Time.deltaTime);
        }
        else
        {
            bool hasWalk = false;
            foreach (var p in _animator.parameters)
                if (p.nameHash == HashWalk) { hasWalk = true; break; }

            if (hasWalk)
                _animator.SetBool(HashWalk, speed > 0.1f);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  GIZMOS
    // ─────────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // Sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Hearing radius (dynamic)
        if (NoiseSystem.Instance != null)
        {
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position,
                NoiseSystem.Instance.EffectiveHearingRadius * hearingMultiplier);
        }

        // Sight cone
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Vector3 leftBound  = Quaternion.Euler(0, -sightAngle * 0.5f, 0) * transform.forward * sightRange;
        Vector3 rightBound = Quaternion.Euler(0,  sightAngle * 0.5f, 0) * transform.forward * sightRange;
        Gizmos.DrawLine(transform.position, transform.position + leftBound);
        Gizmos.DrawLine(transform.position, transform.position + rightBound);

        // State label color
        switch (currentState)
        {
            case NPCState.Patrol:         Gizmos.color = Color.green;  break;
            case NPCState.Investigate:    Gizmos.color = Color.cyan;   break;
            case NPCState.Chase:          Gizmos.color = Color.yellow; break;
            case NPCState.Attack:         Gizmos.color = Color.red;    break;
            case NPCState.ReturnToPatrol: Gizmos.color = Color.blue;   break;
        }
        Gizmos.DrawSphere(transform.position + Vector3.up * 2.5f, 0.2f);
    }
}
