using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using TMPro;

/// <summary>
/// Mr Pebble Boss AI
/// ─────────────────────────────────────────────────────────────────────────
///  SKILL 1 – Bato Slam  (Heavy Attack)
///    • Raises fist (scale-up wind-up)
///    • Red warning circle appears on the GROUND at the player's position
///    • After warning, slams down → shockwave damages player if still in radius
///    • Tutorial: dodge or jump to avoid
///
///  SKILL 2 – Bato Charge  (Dash Attack)
///    • Short wind-up (leans forward, brief pause)
///    • Charges straight at player at high speed
///    • If hits a WALL → stunned / vulnerable (takes 2× damage)
///    • If hits PLAYER → deals damage, no stun
///    • Tutorial: side-dash and counter-attack during vulnerable window
///
///  DIALOGUE
///    Bato Slam  → "TIKMAN MO ITO!"
///    Bato Charge → "WALANG TAKAS!"
///    On death   → "May kailangan lang akong harapin na mas mataas na usapin.\nHindi pa rito nagtatapos ang laban natin!"
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
public class MrPebbleBoss : MonoBehaviour
{
    // ── Stats ──────────────────────────────────────────────────────────────
    [Header("Boss Stats")]
    public float maxHealth      = 500f;
    public float currentHealth;
    public float detectionRange = 60f;   // large – boss arena is far from spawn
    public float attackRange    = 4f;

    // ── Bato Slam ──────────────────────────────────────────────────────────
    [Header("Bato Slam")]
    public float slamDamage      = 35f;
    public float slamRadius      = 5f;
    public float slamCooldown    = 5f;
    public float slamWindupTime  = 1.2f;   // fist-raise duration
    public float slamWarningTime = 0.8f;   // red circle visible before impact
    public GameObject warningIndicatorPrefab; // optional prefab; auto-created if null

    // ── Bato Charge ────────────────────────────────────────────────────────
    [Header("Bato Charge")]
    public float chargeDamage       = 25f;
    public float chargeSpeed        = 18f;
    public float chargeWindupTime   = 0.6f;
    public float chargeCooldown     = 7f;
    public float chargeDistance     = 20f;
    public float vulnerableDuration = 3f;    // stun window after wall hit
    public float chargeHitRadius    = 1.5f;

    // ── Movement ───────────────────────────────────────────────────────────
    [Header("Movement")]
    public float walkSpeed  = 3.5f;
    public float chaseSpeed = 6f;
    [Header("Rotation")]
    public Vector3 rotationOffset = new Vector3(-180f, 0f, 0f);

    // ── Dialogue ───────────────────────────────────────────────────────────
    [Header("Dialogue")]
    public GameObject dialogueBubblePrefab;  // optional; auto-created if null
    public float dialogueDisplayTime = 2.5f;

    // ── References ─────────────────────────────────────────────────────────
    [Header("References")]
    public BossHealthBar healthBar;
    public GameOverManager gameOverManager;  // drag in Inspector or auto-found

    // ── State ──────────────────────────────────────────────────────────────
    public enum BossState { Idle, Chase, BatoSlam, BatoCharge, Vulnerable, Dead }
    public BossState currentState = BossState.Idle;

    // ── Private ────────────────────────────────────────────────────────────
    private NavMeshAgent          _agent;
    private Transform             _player;
    private GamePlayerMovement    _playerMovement;

    private float _slamTimer   = 0f;
    private float _chargeTimer = 0f;
    private bool  _isActing    = false;
    private bool  _isDead      = false;

    private GameObject       _warningCircle;
    private GameObject       _dialogueBubble;
    private TextMeshProUGUI  _dialogueText;

    // ── Unity ──────────────────────────────────────────────────────────────
    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        // Find player
        _agent.updateRotation = false;
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            _player         = playerGO.transform;
            _playerMovement = playerGO.GetComponent<GamePlayerMovement>();
        }

        // Find GameOverManager if not assigned
        if (gameOverManager == null)
            gameOverManager = FindFirstObjectByType<GameOverManager>();

        _agent.speed            = walkSpeed;
        _agent.stoppingDistance = attackRange * 0.8f;

        // Build runtime objects
        CreateWarningIndicator();
        CreateDialogueBubble();

        // Hook up health bar
        if (healthBar != null)
            healthBar.SetBoss(this);

        // Wait until on NavMesh before chasing
        StartCoroutine(InitWhenOnNavMesh());
    }

    IEnumerator InitWhenOnNavMesh()
    {
        while (!_agent.isOnNavMesh) yield return null;
        currentState = BossState.Chase;
    }

    void Update()
    {
        if (_isDead || _player == null) return;
        if (!_agent.isOnNavMesh)        return;
        if (_isActing)                  return;

        _slamTimer   -= Time.deltaTime;
        _chargeTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, _player.position);

        switch (currentState)
        {
            case BossState.Idle:
                if (dist <= detectionRange) currentState = BossState.Chase;
                break;

            case BossState.Chase:
                ChasePlayer(dist);
                break;

            case BossState.Vulnerable:
                // Handled by coroutine – boss stands still
                break;
        }
    }

    // ── Chase ──────────────────────────────────────────────────────────────
    void ChasePlayer(float dist)
    {
        _agent.speed = chaseSpeed;
        _agent.SetDestination(_player.position);

        // Prefer Slam when close; Charge when at medium range
        if (dist <= attackRange && _slamTimer <= 0f)
        {
            StartCoroutine(BatoSlam());
        }
        else if (dist > attackRange && dist <= chargeDistance && _chargeTimer <= 0f)
        {
            StartCoroutine(BatoCharge());
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  SKILL 1 – BATO SLAM
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator BatoSlam()
    {
        _isActing    = true;
        currentState = BossState.BatoSlam;
        _slamTimer   = slamCooldown;

        _agent.isStopped = true;
        _agent.velocity  = Vector3.zero;

        // Face the player
        FaceTarget(_player.position);

        // Dialogue
        ShowDialogue("TIKMAN MO ITO!");

        // ── Wind-up: scale up (raise fist visual) ──────────────────────────
        Vector3 origScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < slamWindupTime)
        {
            float t = elapsed / slamWindupTime;
            transform.localScale = Vector3.Lerp(origScale, origScale * 1.18f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ── Show red warning circle at PLAYER'S current position ───────────
        Vector3 slamTarget = _player.position;
        if (_warningCircle != null)
        {
            _warningCircle.transform.position   = new Vector3(slamTarget.x, slamTarget.y + 0.05f, slamTarget.z);
            _warningCircle.transform.localScale = new Vector3(slamRadius * 2f, 0.02f, slamRadius * 2f);
            _warningCircle.SetActive(true);
        }

        yield return new WaitForSeconds(slamWarningTime);

        // ── SLAM – snap scale back down fast ──────────────────────────────
        elapsed = 0f;
        while (elapsed < 0.12f)
        {
            float t = elapsed / 0.12f;
            transform.localScale = Vector3.Lerp(origScale * 1.18f, origScale * 0.88f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = origScale;

        // Hide warning
        if (_warningCircle != null) _warningCircle.SetActive(false);

        // ── Shockwave damage check ─────────────────────────────────────────
        Collider[] hits = Physics.OverlapSphere(slamTarget, slamRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                DamagePlayer(slamDamage);
                break;
            }
        }

        // ── Shockwave visual ring ──────────────────────────────────────────
        StartCoroutine(ShockwavePulse(slamTarget));

        yield return new WaitForSeconds(0.5f);

        //_agent.isStopped = false;
        currentState     = BossState.Chase;
        _isActing        = false;
    }

    IEnumerator ShockwavePulse(Vector3 origin)
    {
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "ShockwaveRing";
        Destroy(ring.GetComponent<Collider>());
        ring.transform.position   = new Vector3(origin.x, origin.y + 0.05f, origin.z);
        ring.transform.localScale = new Vector3(0.5f, 0.02f, 0.5f);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(1f, 0.3f, 0f, 0.7f);
        ring.GetComponent<Renderer>().material = mat;

        float t = 0f, dur = 0.5f;
        while (t < dur)
        {
            float s = Mathf.Lerp(0.5f, slamRadius * 2f, t / dur);
            float a = Mathf.Lerp(0.7f, 0f, t / dur);
            ring.transform.localScale = new Vector3(s, 0.02f, s);
            mat.color = new Color(1f, 0.3f, 0f, a);
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(ring);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  SKILL 2 – BATO CHARGE
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator BatoCharge()
    {
        _isActing    = true;
        currentState = BossState.BatoCharge;
        _chargeTimer = chargeCooldown;

        _agent.isStopped = true;
        _agent.velocity  = Vector3.zero;

        // Dialogue
        ShowDialogue("WALANG TAKAS!");

        // ── Wind-up: face player, lean forward (scale Z) ───────────────────
        FaceTarget(_player.position);
        Vector3 origScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < chargeWindupTime)
        {
            float t = elapsed / chargeWindupTime;
            // Lean forward: squish Y, stretch Z
            transform.localScale = Vector3.Lerp(
                origScale,
                new Vector3(origScale.x * 0.85f, origScale.y * 0.75f, origScale.z * 1.3f),
                t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ── Charge ─────────────────────────────────────────────────────────
        _agent.enabled = false;   // manual movement during charge

        Vector3 chargeDir  = transform.forward;
        float   charged    = 0f;
        bool    hitWall    = false;
        bool    hitPlayer  = false;

        while (charged < chargeDistance && !hitWall && !hitPlayer)
        {
            float step = chargeSpeed * Time.deltaTime;
            transform.position += chargeDir * step;
            charged += step;

            // Restore scale during charge
            transform.localScale = Vector3.Lerp(
                new Vector3(origScale.x * 0.85f, origScale.y * 0.75f, origScale.z * 1.3f),
                origScale,
                charged / chargeDistance);

            // Player hit check
            Collider[] overlaps = Physics.OverlapSphere(transform.position, chargeHitRadius);
            foreach (var col in overlaps)
            {
                if (col.CompareTag("Player"))
                {
                    DamagePlayer(chargeDamage);
                    hitPlayer = true;
                    break;
                }
            }

            // Wall hit check
            if (!hitPlayer)
            {
                RaycastHit wallHit;
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, chargeDir, out wallHit, step + 0.6f))
                {
                    if (!wallHit.collider.isTrigger && !wallHit.collider.CompareTag("Player"))
                    {
                        hitWall = true;
                        break;
                    }
                }
            }

            yield return null;
        }

        transform.localScale = origScale;

        // Re-enable NavMeshAgent
        _agent.enabled = true;
        while (!_agent.isOnNavMesh) yield return null;

        // ── Outcome ────────────────────────────────────────────────────────
        if (hitWall)
        {
            yield return StartCoroutine(EnterVulnerable());
        }
        else
        {
            yield return new WaitForSeconds(0.4f);
            _agent.isStopped = false;
            currentState     = BossState.Chase;
            _isActing        = false;
        }
    }

    // ── Vulnerable (stunned after wall hit) ────────────────────────────────
    IEnumerator EnterVulnerable()
    {
        currentState = BossState.Vulnerable;
        ShowDialogue("Aray! Napalo ako sa pader...");
        Debug.Log("[MrPebble] VULNERABLE! Attack now!");

        // Tint red to signal vulnerability
        var rend = GetComponent<Renderer>();
        Color origColor = Color.white;
        if (rend != null)
        {
            origColor = rend.material.color;
            rend.material.color = new Color(1f, 0.25f, 0.25f, 1f);
        }

        yield return new WaitForSeconds(vulnerableDuration);

        if (rend != null)
            rend.material.color = origColor;

        _agent.isStopped = false;
        currentState     = BossState.Chase;
        _isActing        = false;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  DAMAGE
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Deal damage to the player and trigger game-over if they die.</summary>
    void DamagePlayer(float amount)
    {
        if (_playerMovement == null) return;
        _playerMovement.TakeDamage(amount);
        Debug.Log($"[MrPebble] Dealt {amount} dmg to player. Player HP: {_playerMovement.currentHealth}");

        if (_playerMovement.currentHealth <= 0f)
            TriggerPlayerDeath();
    }

    void TriggerPlayerDeath()
    {
        Debug.Log("[MrPebble] Player has been defeated!");
        if (gameOverManager != null)
            gameOverManager.ShowGameOver("YOU DIED!", "Mr. Pebble has defeated you.");
    }

    /// <summary>Called by PlayerBossAttack when the player punches/hits the boss.</summary>
    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        // Double damage during vulnerable window
        if (currentState == BossState.Vulnerable) amount *= 2f;

        currentHealth -= amount;
        currentHealth  = Mathf.Max(0f, currentHealth);

        if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);

        Debug.Log($"[MrPebble] Took {amount} dmg. HP: {currentHealth}/{maxHealth}" +
                  (currentState == BossState.Vulnerable ? " (VULNERABLE – 2× dmg!)" : ""));

        if (currentHealth <= 0f)
        {
        GamePlayerMovement playerMovement = FindObjectOfType<GamePlayerMovement>();
        playerMovement.hasGas = true;
        playerMovement.transform.position = new Vector3(0f, 1f, 0f);
        }

        if (currentHealth <= 0f) StartCoroutine(Die()) ;
        
    }

    // ══════════════════════════════════════════════════════════════════════
    //  DEATH
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator Die()
    {
        _isDead      = true;
        currentState = BossState.Dead;
        _agent.isStopped = true;
        _agent.enabled   = false;

        ShowDialogue("May kailangan lang akong harapin na mas mataas na usapin.\nHindi pa rito nagtatapos ang laban natin!");
        Debug.Log("[MrPebble] DEFEATED!");

        // Sink into ground
        float t = 0f;
        Vector3 startPos = transform.position;
        while (t < 2f)
        {
            transform.position = Vector3.Lerp(startPos, startPos - Vector3.up * 3f, t / 2f);
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  DIALOGUE
    // ══════════════════════════════════════════════════════════════════════
    void ShowDialogue(string text)
    {
        if (_dialogueText != null)
        {
            StopCoroutine("HideDialogue");
            _dialogueText.text = text;
            _dialogueBubble.SetActive(true);
            StartCoroutine(HideDialogue());
        }
        Debug.Log($"[MrPebble] \"{text}\"");
    }

    IEnumerator HideDialogue()
    {
        yield return new WaitForSeconds(dialogueDisplayTime);
        if (_dialogueBubble != null) _dialogueBubble.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════════════════
    void FaceTarget(Vector3 target)
{
    Vector3 dir = target - transform.position;
    dir.y = 0f;

    if (dir.sqrMagnitude > 0.001f)
    {
        Quaternion targetRotation =
            Quaternion.LookRotation(dir) *
            Quaternion.Euler(rotationOffset);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * 8f
        );
    }
}

    // ── Runtime object creation ────────────────────────────────────────────
    void CreateWarningIndicator()
    {
        if (warningIndicatorPrefab != null)
        {
            _warningCircle = Instantiate(warningIndicatorPrefab);
        }
        else
        {
            _warningCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _warningCircle.name = "BatoSlamWarning";
            Destroy(_warningCircle.GetComponent<Collider>());
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f, 0f, 0f, 0.6f);
            _warningCircle.GetComponent<Renderer>().material = mat;
        }
        _warningCircle.SetActive(false);
    }

    void CreateDialogueBubble()
    {
        if (dialogueBubblePrefab != null)
        {
            _dialogueBubble = Instantiate(dialogueBubblePrefab, transform);
            _dialogueText   = _dialogueBubble.GetComponentInChildren<TextMeshProUGUI>();
            _dialogueBubble.SetActive(false);
            return;
        }

        // Auto-create a world-space canvas above the boss
        _dialogueBubble = new GameObject("MrPebbleDialogue");
        _dialogueBubble.transform.SetParent(transform);
        _dialogueBubble.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        _dialogueBubble.transform.localScale    = new Vector3(0.008f, 0.008f, 0.008f);

        var canvas = _dialogueBubble.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var bg    = new GameObject("BG");
        bg.transform.SetParent(_dialogueBubble.transform, false);
        var bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.78f);
        var bgRT  = bg.GetComponent<RectTransform>();
        bgRT.sizeDelta = new Vector2(520, 130);

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(bg.transform, false);
        _dialogueText = textGO.AddComponent<TextMeshProUGUI>();
        _dialogueText.fontSize  = 28;
        _dialogueText.color     = Color.white;
        _dialogueText.alignment = TextAlignmentOptions.Center;
        _dialogueText.enableWordWrapping = true;
        var tRT = textGO.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero;
        tRT.anchorMax = Vector2.one;
        tRT.offsetMin = new Vector2(10, 10);
        tRT.offsetMax = new Vector2(-10, -10);

        _dialogueBubble.SetActive(false);
    }

    // ── Gizmos ─────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, slamRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, chargeDistance);
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
