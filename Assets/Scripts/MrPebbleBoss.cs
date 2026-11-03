using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using TMPro;

/// <summary>
/// Mr Pebble Boss AI
/// ─────────────────────────────────────────────────────────────────────────
///  SKILL 1 – Bato Slam  (Heavy Attack)
///    • Raises fist (scale-up wind-up)
///    • During slam, boss rotates to 0 on X axis
///    • Red warning circle appears on the GROUND at the player's position
///    • After warning, slams down → shockwave damages player if still in radius
///    • After slam completes, returns to -90 on X for movement
///
///  SKILL 2 – Bato Charge  (Dash Attack)
///    • Smooth wind-up: faces player, leans forward
///    • Charges straight at high speed through the player's position
///    • If hits a WALL → stunned / vulnerable (takes 2× damage)
///    • If passes through PLAYER → deals damage once
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
public class MrPebbleBoss : MonoBehaviour
{
    // ── Stats ──────────────────────────────────────────────────────────────
    [Header("Boss Stats")]
    public float maxHealth      = 500f;
    public float currentHealth;
    public float detectionRange = 60f;
    public float attackRange    = 4f;
    public GameObject bossHealthBar;

    // ── Bato Slam ──────────────────────────────────────────────────────────
    [Header("Bato Slam")]
    public float slamDamage      = 35f;
    public float slamRadius      = 5f;
    public float slamCooldown    = 5f;
    public float slamWindupTime  = 1.2f;
    public float slamWarningTime = 0.8f;
    public GameObject warningIndicatorPrefab;

    // ── Bato Charge ────────────────────────────────────────────────────────
    [Header("Bato Charge")]
    public float chargeDamage       = 25f;
    public float chargeSpeed        = 18f;
    public float chargeWindupTime   = 0.6f;
    public float chargeCooldown     = 7f;
    public float chargeDistance     = 15f;
    public float vulnerableDuration = 3f;
    public float chargeHitRadius    = 1.8f;

    // ── Movement ───────────────────────────────────────────────────────────
    [Header("Movement")]
    public float walkSpeed  = 3.5f;
    public float chaseSpeed = 6f;
    [Header("Rotation")]
    public Vector3 rotationOffset = new Vector3(-90f, 0f, 0f);

    // ── Dialogue ───────────────────────────────────────────────────────────
    [Header("Dialogue")]
    public GameObject dialogueBubblePrefab;
    public float dialogueDisplayTime = 2.5f;

    // ── References ─────────────────────────────────────────────────────────
    [Header("References")]
    public BossHealthBar healthBar;
    public GameOverManager gameOverManager;

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
        AdjustHitboxForRotation();
    }

    void OnValidate()
    {
        AdjustHitboxForRotation();
    }

    /// <summary>
    /// Ensures the CapsuleCollider hitbox is ALWAYS upright in world space.
    /// Also makes collision slimmer (radius 0.55) so the player can approach and punch properly.
    /// </summary>
    public void AdjustHitboxForRotation()
    {
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col == null) return;

        col.radius = 0.55f;
        col.height = 4f;

        float pitch = transform.localEulerAngles.x;
        bool isPitched90 = Mathf.Abs(Mathf.DeltaAngle(pitch, -90f)) < 30f;

        if (isPitched90)
        {
            // When pitched -90 on X (moving/chasing), local Z is World UP
            col.direction = 2; // Z-axis
            col.center = new Vector3(0f, 0f, 2f);
        }
        else
        {
            // When upright at pitch 0 (during slam attack), local Y is World UP
            col.direction = 1; // Y-axis
            col.center = new Vector3(0f, 2f, 0f);
        }
    }

    void Start()
    {
        _agent.updateRotation = false;
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            _player         = playerGO.transform;
            _playerMovement = playerGO.GetComponent<GamePlayerMovement>();
        }

        if (gameOverManager == null)
            gameOverManager = FindFirstObjectByType<GameOverManager>();

        _agent.speed            = walkSpeed;
        _agent.stoppingDistance = attackRange * 0.8f;
        _agent.radius           = 0.55f;

        CreateWarningIndicator();
        CreateDialogueBubble();

        if (healthBar != null)
        {
            healthBar.SetBoss(this);
        }

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
                break;
        }
    }

    void LateUpdate()
    {
        if (_isDead) return;

        // Still locked at -90 on X when moving/chasing, 0 on X when slamming
        float targetPitch = (currentState == BossState.BatoSlam) ? 0f : -90f;
        transform.rotation = Quaternion.Euler(targetPitch, transform.eulerAngles.y, 0f);

        AdjustHitboxForRotation();
    }

    // ── Chase ──────────────────────────────────────────────────────────────
    void ChasePlayer(float dist)
    {
        _agent.speed = chaseSpeed;
        _agent.SetDestination(_player.position);
        FaceTarget(_player.position, 10f);

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

        // 0 rotation on X when slamming
        if (_player != null)
            FaceTarget(_player.position, 0f);
        else
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        ShowDialogue("TIKMAN MO ITO!");

        Vector3 origScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < slamWindupTime)
        {
            float t = elapsed / slamWindupTime;
            transform.localScale = Vector3.Lerp(origScale, origScale * 1.18f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 slamTarget = _player != null ? _player.position : transform.position + transform.forward * 3f;
        if (_warningCircle != null)
        {
            _warningCircle.transform.position   = new Vector3(slamTarget.x, slamTarget.y + 0.05f, slamTarget.z);
            _warningCircle.transform.localScale = new Vector3(slamRadius * 2f, 0.02f, slamRadius * 2f);
            _warningCircle.SetActive(true);
        }

        yield return new WaitForSeconds(slamWarningTime);

        elapsed = 0f;
        while (elapsed < 0.12f)
        {
            float t = elapsed / 0.12f;
            transform.localScale = Vector3.Lerp(origScale * 1.18f, origScale * 0.88f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = origScale;

        if (_warningCircle != null) _warningCircle.SetActive(false);

        Collider[] hits = Physics.OverlapSphere(slamTarget, slamRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                DamagePlayer(slamDamage);
                break;
            }
        }

        StartCoroutine(ShockwavePulse(slamTarget));

        yield return new WaitForSeconds(0.5f);

        // Slam finished - return to Chase and -90 on X
        currentState = BossState.Chase;
        _isActing    = false;
        transform.rotation = Quaternion.Euler(-90f, transform.eulerAngles.y, 0f);
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

        ShowDialogue("WALANG TAKAS!");

        Vector3 origScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < chargeWindupTime)
        {
            float t = elapsed / chargeWindupTime;
            if (_player != null)
                FaceTarget(_player.position, 15f);

            transform.localScale = Vector3.Lerp(
                origScale,
                new Vector3(origScale.x * 0.85f, origScale.y * 0.75f, origScale.z * 1.3f),
                t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 chargeDir = transform.forward;
        chargeDir.y = 0f;
        if (_player != null)
        {
            Vector3 toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > 0.001f)
                chargeDir = toPlayer.normalized;
        }
        if (chargeDir.sqrMagnitude > 0.001f)
        {
            float launchYaw = Mathf.Atan2(chargeDir.x, chargeDir.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(-90f, launchYaw, 0f);
        }

        _agent.enabled = false;

        float distToPlayer = _player != null ? Vector3.Distance(transform.position, _player.position) : 10f;
        float actualChargeDistance = Mathf.Max(chargeDistance, distToPlayer + 8f);

        float charged = 0f;
        bool hitWall = false;
        bool damagedPlayer = false;

        while (charged < actualChargeDistance && !hitWall)
        {
            float step = chargeSpeed * Time.deltaTime;
            transform.position += chargeDir * step;
            charged += step;

            transform.localScale = Vector3.Lerp(
                new Vector3(origScale.x * 0.85f, origScale.y * 0.75f, origScale.z * 1.3f),
                origScale,
                charged / actualChargeDistance);

            if (!damagedPlayer && _player != null)
            {
                Collider[] overlaps = Physics.OverlapSphere(transform.position + Vector3.up * 1f, chargeHitRadius);
                foreach (var col in overlaps)
                {
                    if (col.CompareTag("Player"))
                    {
                        DamagePlayer(chargeDamage);
                        damagedPlayer = true;
                        break;
                    }
                }
            }

            Vector3 rayOrigin = transform.position + Vector3.up * 1.2f + chargeDir * 0.8f;
            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, chargeDir, step + 0.4f);
            foreach (var hit in hits)
            {
                if (hit.collider.isTrigger) continue;
                if (hit.collider.CompareTag("Player")) continue;
                if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)) continue;

                if (hit.normal.y < 0.5f)
                {
                    hitWall = true;
                    break;
                }
            }

            yield return null;
        }

        transform.localScale = origScale;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(transform.position, out navHit, 5f, NavMesh.AllAreas))
        {
            transform.position = navHit.position;
        }

        _agent.enabled = true;
        while (!_agent.isOnNavMesh) yield return null;

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

    IEnumerator EnterVulnerable()
    {
        currentState = BossState.Vulnerable;
        ShowDialogue("Aray! Napalo ako sa pader...");
        Debug.Log("[MrPebble] VULNERABLE! Attack now!");

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

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        if (currentState == BossState.Vulnerable) amount *= 2f;

        currentHealth -= amount;
        currentHealth  = Mathf.Max(0f, currentHealth);

        if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);

        Debug.Log($"[MrPebble] Took {amount} dmg. HP: {currentHealth}/{maxHealth}" +
                  (currentState == BossState.Vulnerable ? " (VULNERABLE – 2× dmg!)" : ""));

        if (currentHealth <= 0f)
        {
            _isDead = true;
            if (bossHealthBar != null)
                bossHealthBar.SetActive(false);
            if (healthBar != null)
                healthBar.Hide();

            GamePlayerMovement playerMovement = FindFirstObjectByType<GamePlayerMovement>();
            if (playerMovement != null)
                playerMovement.hasGas = true;

            StartCoroutine(Die());
        }
    }

    IEnumerator Die()
    {
        _isDead      = true;
        currentState = BossState.Dead;
        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.enabled   = false;
        }

        if (_warningCircle != null)
            _warningCircle.SetActive(false);

        if (bossHealthBar != null)
            bossHealthBar.SetActive(false);
        if (healthBar != null)
            healthBar.Hide();

        ShowDialogue("May kailangan lang akong harapin na mas mataas na usapin.\nHindi pa rito nagtatapos ang laban natin!");
        Debug.Log("[MrPebble] DEFEATED!");

        float t = 0f;
        Vector3 startPos = transform.position;
        while (t < 1.5f)
        {
            transform.position = Vector3.Lerp(startPos, startPos - Vector3.up * 3f, t / 1.5f);
            t += Time.deltaTime;
            yield return null;
        }

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            ExitPoint.TeleportPlayer(playerGO.transform, new Vector3(0f, 1f, 0f));
            Debug.Log("[MrPebble] Player teleported back to (0, 1, 0)!");
        }

        Destroy(gameObject);
    }

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

    void FaceTarget(Vector3 target, float turnSpeed = 8f)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            float targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float currentYaw = transform.eulerAngles.y;
            float newYaw = turnSpeed <= 0f 
                ? targetYaw 
                : Mathf.MoveTowardsAngle(currentYaw, targetYaw, turnSpeed * Time.deltaTime * 60f);

            // Locked still at -90 on X when moving/chasing, 0 on X when slamming
            float targetPitch = (currentState == BossState.BatoSlam) ? 0f : -90f;
            transform.rotation = Quaternion.Euler(targetPitch, newYaw, 0f);
        }
    }

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
