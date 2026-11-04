using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GamePlayerMovement : MonoBehaviour
{
    
    // ── Movement & Requirements ──────────────────────────────────────────────────────────
    [Header("Movement")]
    public float walkSpeed     = 3f;
    public float runSpeed      = 8f;
    public float rotationSpeed = 720f;
    public bool  hasGas        = false;

    // ── Sprint ────────────────────────────────────────────────────────────
    [Header("Sprint")]
    public float maxStamina        = 100f;
    public float staminaDrainRate  = 20f;
    public float staminaRegenRate  = 12f;
    public float staminaRegenDelay = 1.5f;

    // ── Health ────────────────────────────────────────────────────────────
    [Header("Health")]
    public float maxHealth     = 100f;
    public float currentHealth = 100f;

    // ── Combat ────────────────────────────────────────────────────────────
    [Header("Combat")]
    public float punchCooldown = 0.55f;
    public float shootCooldown = 0.35f;
    public bool  hasGun        = false;

    // ── Animation ─────────────────────────────────────────────────────────
    [Header("Animation")]
    public Animator animator;

    // ── Crouch ────────────────────────────────────────────────────────────
    [Header("Crouch")]
    public float crouchSpeed        = 1.8f;
    public float crouchHeightScale  = 0.55f;   // visual scale-down on Y
    public KeyCode crouchKey        = KeyCode.C;

    // ── Public read-only ──────────────────────────────────────────────────
    public float CurrentStamina => _stamina;
    public float MaxStamina     => maxStamina;
    public bool  IsSprinting    => _isSprinting;
    public bool  IsAiming       => _isAiming;
    public bool  IsCrouching    => _isCrouching;
    public bool  IsMoving       => _isMoving;

    // ── Private ───────────────────────────────────────────────────────────
    private CharacterController _cc;
    private Vector3 _velocity;
    private float   _gravity = -30f;

    private float _stamina;
    private bool  _isSprinting;
    private float _regenTimer;

    private bool  _isAiming;
    private float _attackTimer;
    private bool  _nextPunchIsLeft = true;   // alternates L / R each click

    private bool  _isCrouching;
    private bool  _isMoving;
    private float _normalCCHeight;
    private float _normalCCCenterY;
    private Transform _visual;

    private Camera _cam;
    private Camera _firstPersonCam;
    public bool firstPersonMode = false;
    private float verticalVelocity;
    private float cameraRotationX;
    public float mouseSensitivity = 2f;
    public Transform playerCamera;
    public Camera mainCamera;
    public Camera firstPersonCamera;

    void Awake()
    {
        _cc      = GetComponent<CharacterController>();
        _stamina = maxStamina;
        _cam     = Camera.main;
        _firstPersonCam = _cam != null && _cam.CompareTag("FirstPersonCamera") ? _cam : null;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        _normalCCHeight  = _cc.height;
        _normalCCCenterY = _cc.center.y;

        // Cache the Visual child for crouch scaling
        Transform vis = transform.Find("Visual");
        _visual = vis != null ? vis : transform;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        _cam = _cam != null ? _cam : Camera.main;

        HandleCrouch();
        HandleAimAndCombat();
        HandleMovement();
        HandleCamera();

        if (Input.GetKeyDown("e"))
        {
            HealCollect();
        }
        

    }

    void HandleCamera()
    {
        // I want to add a feature where if the loot reached the required amount, the player will go to first person camera and the player will be able to shoot. If not, the player will stay in third person camera and will not be able to shoot. I want to add this feature in the Update() method.
        if (LootTaskManager.Instance != null && LootTaskManager.Instance._exitUnlocked)
        {
            if (_firstPersonCam != null)
                _firstPersonCam.enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                firstPersonMode = true;  
                mainCamera.gameObject.SetActive(false); // Disable the main camera
                firstPersonCamera.gameObject.SetActive(true); // Enable the first-person camera           
                _cam = _firstPersonCam; // Switch to first-person camera
            if (_cam != null && _cam != _firstPersonCam)
                _cam.enabled = false;
            //hasGun = true; // Allow shooting when loot requirement is met
        }
        else
        {
            if (_firstPersonCam != null)
                _firstPersonCam.enabled = false;
            if (_cam != null && _cam != _firstPersonCam)
                _cam.enabled = true;
            //hasGun = false; // Disable shooting when loot requirement is not met
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  COLLECTIBLE
    // ─────────────────────────────────────────────────────────────────────
    void HealCollect()
    {
        // 1. Identify active camera
        Camera activeCam = null;
        if (firstPersonCamera != null && firstPersonCamera.gameObject.activeInHierarchy && firstPersonCamera.enabled)
            activeCam = firstPersonCamera;
        else if (mainCamera != null && mainCamera.gameObject.activeInHierarchy && mainCamera.enabled)
            activeCam = mainCamera;
        else if (_cam != null && _cam.gameObject.activeInHierarchy && _cam.enabled)
            activeCam = _cam;
        else
            activeCam = Camera.main;

        float interactDistance = 4.0f;
        Medkit foundMedkit = null;

        // 2. Center-screen raycast from active camera (works in 1st person or 3rd person)
        if (activeCam != null)
        {
            Ray ray = activeCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance + 3f))
            {
                foundMedkit = hit.transform.GetComponent<Medkit>() ?? hit.transform.GetComponentInParent<Medkit>();
            }
        }

        // 3. Forward raycast from player chest
        if (foundMedkit == null)
        {
            Vector3 origin = transform.position + Vector3.up * 1.0f;
            if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, interactDistance))
            {
                foundMedkit = hit.transform.GetComponent<Medkit>() ?? hit.transform.GetComponentInParent<Medkit>();
            }
        }

        // 4. Proximity sphere check (standing right next to or in front of the medkit)
        if (foundMedkit == null)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position + Vector3.up * 0.8f, 2.5f);
            float closestDist = float.MaxValue;
            foreach (var col in colliders)
            {
                Medkit m = col.GetComponent<Medkit>() ?? col.GetComponentInParent<Medkit>();
                if (m != null)
                {
                    float d = Vector3.Distance(transform.position, m.transform.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        foundMedkit = m;
                    }
                }
            }
        }

        // 5. Trigger heal if medkit found
        if (foundMedkit != null)
        {
            foundMedkit.Heal(this);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  CROUCH
    // ─────────────────────────────────────────────────────────────────────
    void HandleCrouch()
    {
        bool wantsCrouch = Input.GetKey(crouchKey);

        // Can't sprint while crouching
        if (wantsCrouch && _isSprinting) wantsCrouch = false;

        if (wantsCrouch != _isCrouching)
        {
            _isCrouching = wantsCrouch;

            if (_isCrouching)
            {
                // Shrink CharacterController
                _cc.height   = _normalCCHeight * crouchHeightScale;
                _cc.center   = new Vector3(_cc.center.x, _normalCCCenterY * crouchHeightScale, _cc.center.z);

                // Scale visual down
                if (_visual != null)
                {
                    Vector3 s = _visual.localScale;
                    _visual.localScale = new Vector3(s.x, crouchHeightScale, s.z);
                }
            }
            else
            {
                // Restore CharacterController
                _cc.height = _normalCCHeight;
                _cc.center = new Vector3(_cc.center.x, _normalCCCenterY, _cc.center.z);

                // Restore visual scale
                if (_visual != null)
                {
                    Vector3 s = _visual.localScale;
                    _visual.localScale = new Vector3(s.x, 1f, s.z);
                }
            }

            if (animator != null)
                animator.SetBool("IsCrouching", _isCrouching);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  AIM + COMBAT
    // ─────────────────────────────────────────────────────────────────────
    void HandleAimAndCombat()
    {
        _attackTimer -= Time.deltaTime;

        // Right-click held = aiming / combat stance
        _isAiming = Input.GetMouseButton(1);

        if (_isAiming)
            RotateTowardMouse();

        // Sync animator
        if (animator != null)
            animator.SetBool("IsAiming", _isAiming);

        // Left-click = attack
        if (Input.GetMouseButtonDown(0))
        {
            float cd = hasGun ? shootCooldown : punchCooldown;
            if (_attackTimer <= 0f)
            {
                _attackTimer = cd;
                if (hasGun)
                    TriggerShoot();
                else
                    TriggerPunch();
            }
        }
    }

    void RotateTowardMouse()
    {
        if (_cam == null) return;
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        if (groundPlane.Raycast(ray, out float dist))
        {
            Vector3 worldPoint = ray.GetPoint(dist);
            Vector3 dir = (worldPoint - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, rotationSpeed * 2f * Time.deltaTime);
            }
        }
    }

    void TriggerPunch()
    {
        if (animator != null)
        {
            // Alternate left / right punch each click
            if (_nextPunchIsLeft)
                animator.SetTrigger("PunchLeft");
            else
                animator.SetTrigger("PunchRight");
            _nextPunchIsLeft = !_nextPunchIsLeft;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPunchGruntSFX(transform.position);
    }

    void TriggerShoot()
    {
        if (animator != null)
            animator.SetTrigger("Shoot");
        // TODO: raycast / bullet spawn here
    }

    // ─────────────────────────────────────────────────────────────────────
    //  MOVEMENT
    // ─────────────────────────────────────────────────────────────────────
    void HandleMovement()
    {
        
if (!firstPersonMode)
{
    // THIRD-PERSON MOVEMENT

    float h = Input.GetAxisRaw("Horizontal");
    float v = Input.GetAxisRaw("Vertical");

    Vector3 camFwd = _cam ? _cam.transform.forward : Vector3.forward;
    camFwd.y = 0f;
    camFwd.Normalize();

    Vector3 camRight = _cam ? _cam.transform.right : Vector3.right;
    camRight.y = 0f;
    camRight.Normalize();

    Vector3 moveDir = (camFwd * v + camRight * h).normalized;

    bool isMoving = moveDir.sqrMagnitude > 0.01f;
    _isMoving = isMoving;

    // SPRINT
    bool wantsRun =
        (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        && !_isAiming && !_isCrouching;

    bool canRun = _stamina > 0f;

    if (isMoving && wantsRun && canRun)
    {
        _isSprinting = true;
        _stamina = Mathf.Max(0f, _stamina - staminaDrainRate * Time.deltaTime);
        _regenTimer = staminaRegenDelay;
    }
    else
    {
        _isSprinting = false;

        if (_regenTimer > 0f)
            _regenTimer -= Time.deltaTime;
        else
            _stamina = Mathf.Min(maxStamina,
                _stamina + staminaRegenRate * Time.deltaTime);
    }

    // ROTATION
    if (isMoving && !_isAiming)
    {
        Quaternion targetRot = Quaternion.LookRotation(moveDir);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

    // ANIMATION
    float animSpeed = 0f;

    if (isMoving)
        animSpeed = _isSprinting ? 2f : 1f;

    if (animator != null)
    {
        animator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);

        Vector3 localMove =
            transform.InverseTransformDirection(moveDir);

        animator.SetFloat("VelocityX", localMove.x, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityZ", localMove.z, 0.1f, Time.deltaTime);
    }

    // GRAVITY
    if (_cc.isGrounded && _velocity.y < 0f)
        _velocity.y = -2f;

    _velocity.y += _gravity * Time.deltaTime;

    // APPLY MOVEMENT
    float speed = _isCrouching ? crouchSpeed :
                  (_isSprinting ? runSpeed : walkSpeed);

    _cc.Move((moveDir * speed + _velocity) * Time.deltaTime);
}
else
{
    // FIRST-PERSON CAMERA LOOK

    float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
    float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

    transform.Rotate(0f, mouseX, 0f);

    cameraRotationX -= mouseY;
    cameraRotationX = Mathf.Clamp(cameraRotationX, -90f, 90f);

    playerCamera.localRotation =
        Quaternion.Euler(cameraRotationX, 0f, 0f);

    // FIRST-PERSON MOVEMENT

    float x = Input.GetAxisRaw("Horizontal");
    float z = Input.GetAxisRaw("Vertical");

    Vector3 movement =
        (transform.right * x + transform.forward * z).normalized;

    bool isMoving = movement.sqrMagnitude > 0.01f;
    _isMoving = isMoving;

    // SPRINT
    bool wantsRun =
        (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        && !_isAiming && !_isCrouching;

    bool canRun = _stamina > 0f;

    if (isMoving && wantsRun && canRun)
    {
        _isSprinting = true;
        _stamina = Mathf.Max(0f,
            _stamina - staminaDrainRate * Time.deltaTime);

        _regenTimer = staminaRegenDelay;
    }
    else
    {
        _isSprinting = false;

        if (_regenTimer > 0f)
            _regenTimer -= Time.deltaTime;
        else
            _stamina = Mathf.Min(maxStamina,
                _stamina + staminaRegenRate * Time.deltaTime);
    }

    // ANIMATION
    float animSpeed = 0f;

    if (isMoving)
        animSpeed = _isSprinting ? 2f : 1f;

    if (animator != null)
    {
        animator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);

        Vector3 localMove =
            transform.InverseTransformDirection(movement);

        animator.SetFloat("VelocityX", localMove.x, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityZ", localMove.z, 0.1f, Time.deltaTime);
    }

    // GROUND CHECK
    bool grounded = Physics.CheckSphere(
        transform.position + Vector3.down * 0.1f,
        0.2f,
        LayerMask.GetMask("Ground")
    );

    // JUMPING
    if (grounded && verticalVelocity < 0f)
        verticalVelocity = -2f;

    if (grounded && Input.GetKeyDown(KeyCode.Space))
    {
        verticalVelocity = Mathf.Sqrt(3f * -_gravity * 1.5f);
    }

    // GRAVITY
    verticalVelocity += _gravity * Time.deltaTime;

    // APPLY MOVEMENT
    float currentSpeed = _isCrouching ? crouchSpeed :
                         (_isSprinting ? runSpeed : walkSpeed);

    movement *= currentSpeed;
    movement.y = verticalVelocity;

    _cc.Move(movement * Time.deltaTime);
}

    }

    // ─────────────────────────────────────────────────────────────────────
    //  PUBLIC HELPERS
    // ─────────────────────────────────────────────────────────────────────
    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHurtSFX(transform.position);

        if (currentHealth <= 0f)
        {
            GameOverManager gom = FindFirstObjectByType<GameOverManager>();
            if (gom != null)
            {
                gom.ShowGameOver("YOU DIED!", "You ran out of health.");
            }
        }
    }

    public void Heal(float amount)
        => currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);

    public void PickUpGun() { hasGun = true; }
    public void DropGun()   { hasGun = false; }
}
