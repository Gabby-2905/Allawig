
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float wanderRadius = 15f;
    public float detectionRadius = 25f;
    public float punchDistance = 2f;
    public float retreatDistance = 8f;

    [Header("Speed")]
    public float wanderSpeed = 2f;
    public float chaseSpeed = 4f;
    public float retreatSpeed = 5f;

    [Header("Combat")]
    public float punchCooldown = 2f;
    public float punchDamage = 5f;

    [Header("References")]
    public Animator animator;

    private NavMeshAgent agent;
    private Transform player;
    private Vector3 startingPosition;

    private float nextPunchTime;

    private enum EnemyState
    {
        Wandering,
        Chasing,
        Retreating
    }

    private EnemyState currentState = EnemyState.Wandering;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.baseOffset = 0f;
        }

        if (animator == null)
            animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;

        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col == null)
            col = gameObject.AddComponent<CapsuleCollider>();

        col.isTrigger = true;
        col.center = new Vector3(0f, 1f, 0f);
        col.height = 1.6f;
        col.radius = 0.35f;
    }

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.baseOffset = 0f;

        if (animator == null)
            animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();

        startingPosition = transform.position;

        // Snap to NavMesh so NPCs don't float off-mesh
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(transform.position, out navHit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(navHit.position);
            startingPosition = navHit.position;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        Wander();
    }

    void Update()
    {
        // Update animator parameters based on current speed
        if (animator != null)
        {
            float speed = 0f;
            if (agent != null && !agent.isStopped)
            {
                speed = agent.velocity.magnitude;
                // If pathfinding but velocity hasn't caught up yet, use desired speed
                if (speed < 0.1f && agent.hasPath)
                    speed = agent.speed;
            }
            animator.SetFloat("Speed", speed);
            animator.SetBool("isMoving", speed > 0.1f);
        }

        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        switch (currentState)
        {
            case EnemyState.Wandering:
                if (distance <= detectionRadius)
                {
                    currentState = EnemyState.Chasing;
                }
                else if (!agent.pathPending &&
                         (!agent.hasPath || agent.remainingDistance <= 0.5f))
                {
                    Wander();
                }
                break;

            case EnemyState.Chasing:
                ChasePlayer(distance);
                break;

            case EnemyState.Retreating:
                CheckRetreat();
                break;
        }
    }

    // RANDOM WANDERING
    void Wander()
    {
        currentState = EnemyState.Wandering;

        agent.speed = wanderSpeed;
        agent.isStopped = false;

        Vector3 randomDirection =
            Random.insideUnitSphere * wanderRadius;

        randomDirection += startingPosition;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            randomDirection,
            out hit,
            wanderRadius,
            NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // CHASE PLAYER
    void ChasePlayer(float distance)
    {
        if (distance > detectionRadius)
        {
            currentState = EnemyState.Wandering;
            Wander();
            return;
        }

        agent.speed = chaseSpeed;
        agent.isStopped = false;

        if (distance <= punchDistance)
        {
            agent.isStopped = true;

            FacePlayer();

            if (Time.time >= nextPunchTime)
            {
                PunchPlayer();
            }
        }
        else
        {
            agent.SetDestination(player.position);
        }
    }

    // PUNCH PLAYER
    void PunchPlayer()
    {
        nextPunchTime = Time.time + punchCooldown;

        if (animator != null)
        {
            animator.SetTrigger("Punch");
            animator.SetTrigger("Attack");
        }

        GamePlayerMovement playerMovement = player != null ? player.GetComponent<GamePlayerMovement>() : null;
        if (playerMovement != null)
        {
            playerMovement.TakeDamage(punchDamage);
        }

        PlayerStun stun = player != null ? player.GetComponent<PlayerStun>() : null;

        if (stun != null)
        {
            stun.Stun(1f);
        }

        // Punch grunt SFX
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPunchGruntSFX(transform.position);

        // Delay retreat so attack animation visually plays before NPC runs away
        StartCoroutine(DelayedRetreat(0.45f));
    }

    IEnumerator DelayedRetreat(float delay)
    {
        yield return new WaitForSeconds(delay);
        RetreatFromPlayer();
    }

    // RUN 8 METERS AWAY
    void RetreatFromPlayer()
    {
        currentState = EnemyState.Retreating;

        agent.isStopped = false;
        agent.speed = retreatSpeed;

        Vector3 direction =
            (transform.position - player.position).normalized;

        direction.y = 0;

        Vector3 retreatPosition =
            player.position + direction * retreatDistance;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            retreatPosition,
            out hit,
            5f,
            NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // CHECK IF RETREAT IS FINISHED
    void CheckRetreat()
    {
        if (!agent.pathPending &&
            agent.hasPath &&
            agent.remainingDistance <= 0.5f)
        {
            agent.ResetPath();

            float distance = Vector3.Distance(
                transform.position,
                player.position
            );

            if (distance <= detectionRadius)
            {
                currentState = EnemyState.Chasing;
            }
            else
            {
                Wander();
            }
        }
    }

    // FACE THE PLAYER
    void FacePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion rotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotation,
                Time.deltaTime * 10f
            );
        }
    }

    // VISUALIZE DETECTION AND WANDER RADIUS
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            Application.isPlaying ? startingPosition : transform.position,
            wanderRadius
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRadius
        );
    }
}
