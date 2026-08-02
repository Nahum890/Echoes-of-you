using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterController))]
public partial class PlayerController : MonoBehaviour
{
    public System.Action<float> OnLanded;
    public System.Action OnJumped;
    const string AnimatorParamSpeed = "Speed";
    const string AnimatorParamIsGrounded = "IsGrounded";
    const string AnimatorParamIsRecording = "IsRecording";
    const string AnimatorLegacyGrounded = "Grounded";
    const string AnimatorLegacyFalling = "Falling";
    const string AnimatorLegacyJump = "Jump";
    const string AnimatorParamVerticalSpeed = "VerticalSpeed";
    const string AnimatorParamTurn = "Turn";
    const string AnimatorParamHardLanding = "HardLanding";
    const string AnimatorParamStartRun = "StartRun";
    const string AnimatorParamStopRun = "StopRun";
    const string AnimatorParamDeath = "Death";
    const string AnimatorParamRespawn = "Respawn";

    [Header("Movimiento PS1 Modern Liminal")]
    public float acceleration = 8f;
    public float deceleration = 12f;
    public float maxSpeed = 4.5f;
    public float sprintMultiplier = 1.35f; // solo speed, no momentum
    public float turnSpeed = 8f;
    public float airControl = 0.15f;
    public float coyoteTime = 0.12f;
    public float jumpBuffer = 0.1f;
    public float gravityScale = 2.5f;
    public float landingVelocityRetention = 0.0f; // FRENO TOTAL al aterrizar

    [Header("Salto / Gravedad (Legacy - usar gravityScale arriba)")]
    public float jumpForce = 5.5f;
    public float gravityStrength = 18.0f;
    public Vector3 defaultGravityDirection = Vector3.down;
    public float groundedStickForce = 2f;
    public float gravityBlendSpeed = 12f;
    public float fallGravityMultiplier = 2.0f;
    public float maxFallSpeed = 25.0f;
    public bool alignToGroundNormal = false;

    /// <summary>Theoretical max jump height calculated from jumpForce and gravity (v²/2g).</summary>
    public float JumpHeight => (jumpForce * jumpForce) / (2f * gravityStrength);

    [Header("Jump Assist")]
    public float jumpBufferTime = 0.10f;

    [Header("Peso / Game Feel")]
    [SerializeField] float hardLandingSpeed = 13f;
    [SerializeField] float softLandingPause = 0.012f;
    [SerializeField] float hardLandingPause = 0.028f;
    [SerializeField] float footstepDistance = 1.75f;
    [SerializeField] float movementScrapeSpeed = 4.5f;

    [Header("Deteccion de suelo")]
    public Transform groundCheck;
    public float groundProbeRadius = 0.25f;
    public float groundProbeDistance = 0.6f;
    public LayerMask groundMask = (1 << 6); // Solo layer Ground — incluir Default causa que el SphereCast detecte al propio jugador

    [Header("Spawn Safety")]
    public float spawnValidationRadius = 1f;
    public LayerMask groundCheckMask = 1 << 6;

    [Header("Failsafe")]
    public float voidHeight = -15f;

    CharacterController _controller;
    Transform _cam;
    Animator _anim;
    Rigidbody _rb;
    EchoRecorder _echoRecorder;
    Transform _visualRoot;
    Transform _modelRoot;

    readonly List<GravityZone> _gravityZones = new List<GravityZone>();

    Vector3 _planarVelocity;
    Vector3 _verticalVelocity;
    Vector3 _currentGravity;
    Vector3 _targetGravity;
    Vector3 _currentUp = Vector3.up;
    Vector3 _lastFacing = Vector3.forward;
    bool _grounded;
    bool _wasGrounded;
    bool _isDead;
    bool _jumpedThisFrame;
    bool _isFalling;
    bool _wasMoving;
    bool _lastHardLanding;
    bool _inputLocked;
    float _landingLockTimer;
    float _distanceSinceFootstep;
    float _lastPlanarSpeed;
    float _turnAmount;
    float _gravityScale = 1f;
    Vector3 _platformVelocity;
    float _notGroundedTimer;

    public enum PlayerAnimationState
    {
        Idle = 0,
        Run = 1,
        Jump = 2,
        Recording = 3,
        Falling = 4,
        Landing = 5,
        Death = 6,
        Respawn = 7
    }

    // Jump buffer & coyote
    float _jumpBufferTimer;
    float _coyoteTimer;

    public Vector3 UpAxis => _currentUp;
    public Vector3 GravityDirection => _currentGravity.sqrMagnitude > 0.0001f ? _currentGravity.normalized : Vector3.down;
    public Vector3 GravityVector => _currentGravity;
    public bool IsGrounded => _grounded;
    public bool IsAlive => !_isDead;
    public bool IsInputLocked => _inputLocked;
    public LayerMask GroundMask => groundMask;
    public float PlanarSpeed => Vector3.ProjectOnPlane(_controller != null ? _controller.velocity : _planarVelocity, _currentUp).magnitude;
    public Vector3 PlanarVelocity => _planarVelocity;
    public float VerticalSpeed => Vector3.Dot(_verticalVelocity, _currentUp);
    public float TurnAmount => _turnAmount;
    public bool LastLandingWasHard => _lastHardLanding;
    public PlayerAnimationState CurrentAnimationState { get; private set; }
    public CharacterController Controller => _controller;

    void Awake()
    {
        gameObject.tag = "Player";
        gameObject.layer = LayerMask.NameToLayer("Player"); // Layer 8
        _controller = GetComponent<CharacterController>();
        if (_controller != null)
        {
            _controller.stepOffset = 0.30f;
            _controller.slopeLimit = 45.0f;
            _controller.radius = 0.35f;
            _controller.height = 1.80f;
            _controller.center = new Vector3(0f, 0.90f, 0f);
            _controller.skinWidth = 0.08f;
        }

        TryGetComponent(out _rb);
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        EnsureGroundCheck();
        PlayerCharacterVisualSetup.EnsureOn(transform);
        EnsureVisualAnimator();
        EnsureOptionalComponent("PlayerLocomotionAnimator");
        EnsureOptionalComponent("PlayerAdvancedLocomotion");
        PlayerAnimationRuntimeBootstrap.ApplyToHierarchy(gameObject);
        EnsureCameraFocus();
        RefreshCameraReference();

        _anim = GetComponentInChildren<Animator>();
        _echoRecorder = GetComponent<EchoRecorder>();
        _targetGravity = SafeGravity(defaultGravityDirection, gravityStrength);
        _currentGravity = _targetGravity;
        _currentUp = -_currentGravity.normalized;
        _gravityScale = gravityScale; // PS1 heavier gravity (2.5x)

        Vector3 initialForward = Vector3.ProjectOnPlane(transform.forward, _currentUp);
        if (initialForward.sqrMagnitude < 0.001f)
            initialForward = Vector3.ProjectOnPlane(transform.right, _currentUp);
        if (initialForward.sqrMagnitude < 0.001f)
            initialForward = Vector3.Cross(transform.right, _currentUp);

        _lastFacing = initialForward.normalized;
        transform.rotation = Quaternion.LookRotation(_lastFacing, _currentUp);
    }

    void OnEnable() => ForceUnlockAndReset();

    void Start()
    {
        ForceUnlockAndReset();
        StartCoroutine(ValidateAndFixSpawn());
    }

    System.Collections.IEnumerator ValidateAndFixSpawn()
    {
        yield return new WaitForEndOfFrame(); // Esperar a que la geometría se genere
        
        LayerMask maskToUse = groundMask != 0 ? groundMask : groundCheckMask;

        // 1. Buscar suelo bajo el player
        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f, maskToUse))
        {
            float dist = hit.distance;
            if (dist > 0.5f && dist < 5f)
            {
                // Spawn en el suelo
                _controller.enabled = false;
                transform.position = hit.point + Vector3.up * 1.0f;
                _controller.enabled = true;
                _planarVelocity = Vector3.zero;
                _verticalVelocity = Vector3.zero;
                yield break;
            }
        }
        
        // 2. Buscar posición segura en radio
        for (float r = 1f; r < 15f; r += 1f)
        {
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI * 2f / 12f;
                Vector3 testPos = transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * r;
                if (Physics.Raycast(testPos + Vector3.up * 3f, Vector3.down, out RaycastHit sphereHit, 10f, maskToUse))
                {
                    if (sphereHit.distance > 0.5f && sphereHit.distance < 4f && !Physics.CheckSphere(sphereHit.point + Vector3.up, 0.6f))
                    {
                        _controller.enabled = false;
                        transform.position = sphereHit.point + Vector3.up * 1.0f;
                        _controller.enabled = true;
                        _planarVelocity = Vector3.zero;
                        _verticalVelocity = Vector3.zero;
                        yield break;
                    }
                }
            }
        }
        
        // 3. Fallback: buscar LevelExit
        var exit = FindAnyObjectByType<LevelExit>();
        if (exit != null)
        {
            _controller.enabled = false;
            transform.position = exit.transform.position + Vector3.up * 2f;
            _controller.enabled = true;
            _planarVelocity = Vector3.zero;
            _verticalVelocity = Vector3.zero;
        }
    }

    public void ValidateSpawnPosition()
    {
        StartCoroutine(ValidateAndFixSpawn());
    }

    public bool IsInsideWall()
    {
        return IsInsideWall(transform.position);
    }

    public bool IsInsideWall(Vector3 checkPos)
    {
        bool wasEnabled = _controller != null && _controller.enabled;
        if (_controller != null) _controller.enabled = false;
        bool inside = Physics.CheckSphere(checkPos, 0.5f, ~0, QueryTriggerInteraction.Ignore);
        if (_controller != null) _controller.enabled = wasEnabled;
        return inside;
    }

    void Update()
    {
        if (_isDead)
            return;

        if (_cam == null || (Camera.main != null && _cam != Camera.main.transform))
            RefreshCameraReference();

        if (transform.position.y < voidHeight)
        {
            Die();
            return;
        }

        EnsureGroundCheck();
        UpdateTargetGravity();
        BlendGravity(Time.deltaTime);

        _wasGrounded = _grounded;
        GroundProbe preMoveProbe = ProbeGround(_currentUp);
        // Usar sólo el SphereCast probe como fuente de verdad para grounding.
        // _controller.isGrounded (Unity built-in) da falsos positivos en esquinas/paredes
        // y cancela el salto en el frame siguiente (softlock). El probe incluye
        // validación de ángulo de normal (BUG 5), por lo que es más confiable.
        _grounded = preMoveProbe.isGrounded;

        // Coyote time: start timer when leaving ground (not from jumping)
        if (_wasGrounded && !_grounded && !_jumpedThisFrame)
            _coyoteTimer = coyoteTime;
        if (_coyoteTimer > 0f)
            _coyoteTimer -= Time.deltaTime;

        // Jump buffer: count down
        if (_jumpBufferTimer > 0f)
            _jumpBufferTimer -= Time.deltaTime;

        float downwardSpeed = Mathf.Max(0f, Vector3.Dot(_verticalVelocity, GravityDirection));
        if (_grounded && downwardSpeed > 0f)
            _verticalVelocity = GravityDirection * groundedStickForce;

        Vector3 movementUp = ResolveMovementUp(preMoveProbe);

        if (_inputLocked)
        {
            _planarVelocity = Vector3.zero;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
            if (_grounded)
                _verticalVelocity = GravityDirection * groundedStickForce;

            _controller.Move(_verticalVelocity * Time.deltaTime);
            GroundProbe lockedProbe = ProbeGround(movementUp);
            _grounded = lockedProbe.isGrounded;
            UpdateOrientation(lockedProbe, Time.deltaTime);
            UpdateMovementFeedback(movementUp, Time.deltaTime);
            UpdateAnimator();
            _jumpedThisFrame = false;
            return;
        }

        HandleMovementInput(movementUp);
        HandleJumpInput(movementUp);

        if (!_grounded || _jumpedThisFrame)
        {
            float gravMul = _gravityScale;
            float downSpeed = Vector3.Dot(_verticalVelocity, GravityDirection);
            if (downSpeed > 0f && !_jumpedThisFrame)
                gravMul *= fallGravityMultiplier;
            _verticalVelocity += _currentGravity * gravMul * Time.deltaTime;
            // Terminal fall velocity cap (spec: -25 m/s)
            float currentDownSpeed = Vector3.Dot(_verticalVelocity, GravityDirection);
            if (currentDownSpeed > maxFallSpeed)
            {
                _verticalVelocity = GravityDirection * maxFallSpeed;
            }
        }

        _gravityScale = 1f;
        _isFalling = !_grounded && Vector3.Dot(_verticalVelocity, GravityDirection) > 0.5f;



        Vector3 motion = (_planarVelocity + _verticalVelocity + _platformVelocity) * Time.deltaTime;
        _platformVelocity = Vector3.zero;
        _controller.Move(motion);

        GroundProbe postMoveProbe = ProbeGround(movementUp);
        // No re-groundear en el frame del salto o mientras nos movemos hacia arriba (evita cancelar el salto instantáneamente)
        if (!_jumpedThisFrame)
        {
            bool movingUp = Vector3.Dot(_verticalVelocity, GravityDirection) < -0.1f;
            if (!movingUp)
            {
                _grounded = postMoveProbe.isGrounded;
                if (_grounded)
                    _verticalVelocity = GravityDirection * groundedStickForce;
            }
            else
            {
                _grounded = false;
            }
        }

        if (!_wasGrounded && _grounded)
        {
            _lastHardLanding = downwardSpeed >= hardLandingSpeed;
            _landingLockTimer = _lastHardLanding ? hardLandingPause : softLandingPause;
            OnLanded?.Invoke(downwardSpeed);
            GameFeelController.Instance?.PlayLanding(transform.position, movementUp, downwardSpeed);
            if (_lastHardLanding)
                TriggerAnimatorIfExists(AnimatorParamHardLanding);
        }

        UpdateOrientation(postMoveProbe, Time.deltaTime);
        UpdateMovementFeedback(movementUp, Time.deltaTime);
        UpdateAnimator();

        _jumpedThisFrame = false;
    }

    public void SetInputLocked(bool locked)
    {
        _inputLocked = locked;
        if (!locked)
            return;

        _planarVelocity = Vector3.zero;
        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f;
    }

    public void SetPlanarVelocity(Vector3 velocity) => _planarVelocity = velocity;

    public void AddPlanarImpulse(Vector3 impulse) => _planarVelocity += impulse;

    public void AddVerticalImpulse(Vector3 upAxis, float speed) => _verticalVelocity = upAxis * speed;

    public void SetVerticalStick() => _verticalVelocity = GravityDirection * groundedStickForce;

    public void AddPlatformVelocity(Vector3 velocity) => _platformVelocity += velocity;

    public void SetSprintMomentumBonus(float bonus) { } // No-op for PS1 mode

    public void ApplyGravityScale(float scale) => _gravityScale = Mathf.Clamp(scale, 0.05f, 2f);

    public void ForceUnlockAndReset()
    {
        _inputLocked = false;
        _isDead = false;
        _landingLockTimer = 0f;
        if (_controller != null)
            _controller.enabled = true;
    }
    public void Teleport(Vector3 worldPosition, Quaternion worldRotation)
    {
        _controller.enabled = false;
        transform.SetPositionAndRotation(worldPosition, worldRotation);
        _planarVelocity = Vector3.zero;
        _verticalVelocity = Vector3.zero;
        _currentUp = transform.up;
        _currentGravity = -_currentUp * gravityStrength;
        _targetGravity = _currentGravity;
        _controller.enabled = true;
    }

    void HandleMovementInput(Vector3 movementUp)
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0f, v);
        if (input.sqrMagnitude > 1f)
            input.Normalize();

        Vector3 cameraForward = Vector3.ProjectOnPlane(transform.forward, movementUp).normalized;
        Vector3 cameraRight = Vector3.Cross(movementUp, cameraForward).normalized;

        if (_cam != null)
        {
            // Use camera yaw only for movement direction (ignore pitch) to avoid zigzag when camera looks down
            Vector3 flatForward = _cam.forward;
            flatForward.y = 0f;
            cameraForward = flatForward.sqrMagnitude > 0.001f ? flatForward.normalized : Vector3.forward;
            Vector3 flatRight = _cam.right;
            flatRight.y = 0f;
            cameraRight = flatRight.sqrMagnitude > 0.001f ? flatRight.normalized : Vector3.right;
        }

        Vector3 desiredDirection = cameraForward * input.z + cameraRight * input.x;
        desiredDirection = Vector3.ProjectOnPlane(desiredDirection, movementUp);
        if (desiredDirection.sqrMagnitude > 1f)
            desiredDirection.Normalize();

        float currentMaxSpeed = maxSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            currentMaxSpeed *= sprintMultiplier;
        Vector3 desiredVelocity = desiredDirection * currentMaxSpeed;

        _planarVelocity = Vector3.ProjectOnPlane(_planarVelocity, movementUp);
        float sharpness = desiredVelocity.sqrMagnitude > 0.001f ? acceleration : deceleration;
        if (!_grounded)
            sharpness *= airControl;
        
        // Landing: FRENO TOTAL (landingVelocityRetention = 0.0f)
        if (_landingLockTimer > 0f)
        {
            _landingLockTimer -= Time.deltaTime;
            float retention = landingVelocityRetention; // 0.0 = full stop
            // Apply retention to current planar velocity
            _planarVelocity *= retention;
            desiredVelocity *= retention;
        }
        _planarVelocity = DampVector(_planarVelocity, desiredVelocity, sharpness, Time.deltaTime);
    }

    void HandleJumpInput(Vector3 movementUp)
    {
        // Variable jump height: cut upward velocity if button released
        if (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.Space))
        {
            if (Vector3.Dot(_verticalVelocity, movementUp) > 0f)
            {
                _verticalVelocity -= movementUp * (Vector3.Dot(_verticalVelocity, movementUp) * 0.5f);
            }
        }

        // Buffer the jump input
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            _jumpBufferTimer = jumpBufferTime;

        // Can jump if: (grounded OR coyote active) AND (buffer active)
        bool canJump = (_grounded || _coyoteTimer > 0f) && _jumpBufferTimer > 0f;

        if (!canJump)
            return;

        // Consume both timers
        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f;

        // Inherit platform velocity when jumping off it
        if (_platformVelocity.sqrMagnitude > 0.01f)
        {
            _planarVelocity += Vector3.ProjectOnPlane(_platformVelocity, movementUp);
        }

        float jumpSpeed = jumpForce;
        _verticalVelocity = movementUp * jumpSpeed;
        _grounded = false;
        _jumpedThisFrame = true;
        OnJumped?.Invoke();

        if (HasAnimatorParameter(AnimatorLegacyJump, AnimatorControllerParameterType.Trigger))
            _anim.SetTrigger(AnimatorLegacyJump);
        TriggerAnimatorIfExists("JumpStart");

        GameFeelController.Instance?.PlayJump(transform.position, movementUp);
    }

    void UpdateOrientation(GroundProbe probe, float deltaTime)
    {
        Vector3 desiredUp = -GravityDirection;
        if (alignToGroundNormal && probe.isGrounded && probe.hit.collider != null)
            desiredUp = probe.hit.normal;

        float upBlend = DampingFactor(gravityBlendSpeed, deltaTime);
        _currentUp = Vector3.Slerp(_currentUp, desiredUp.normalized, upBlend).normalized;

        Vector3 facing = Vector3.ProjectOnPlane(_planarVelocity, _currentUp);
        if (facing.sqrMagnitude > 0.001f)
            _lastFacing = facing.normalized;
        else
            _lastFacing = Vector3.ProjectOnPlane(transform.forward, _currentUp).normalized;

        if (_lastFacing.sqrMagnitude < 0.001f)
            _lastFacing = Vector3.Cross(transform.right, _currentUp).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(_lastFacing, _currentUp);
        Vector3 oldForward = Vector3.ProjectOnPlane(transform.forward, _currentUp).normalized;
        float rotationSharpness = turnSpeed; // PS1: turn speed directly as sharpness
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, DampingFactor(rotationSharpness, deltaTime));
        Vector3 newForward = Vector3.ProjectOnPlane(transform.forward, _currentUp).normalized;
        if (oldForward.sqrMagnitude > 0.001f && newForward.sqrMagnitude > 0.001f)
            _turnAmount = Mathf.MoveTowards(_turnAmount, Vector3.SignedAngle(oldForward, newForward, _currentUp) / Mathf.Max(deltaTime, 0.0001f), 720f * deltaTime);
    }

    void Die()
    {
        _isDead = true;
        _controller.enabled = false;
        TriggerAnimatorIfExists(AnimatorParamDeath);
        
        if (LevelRuntimeController.Instance != null)
        {
            LevelRuntimeController.Instance.HandlePlayerDeath(transform.position, 1.2f);
        }
        else
        {
            GameFeelController.Instance?.PlayPlayerDeath(transform.position);
            StartCoroutine(FallbackRestart());
        }
    }

    System.Collections.IEnumerator FallbackRestart()
    {
        yield return new WaitForSecondsRealtime(1.2f);
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    static float DampingFactor(float sharpness, float deltaTime)
    {
        return 1f - Mathf.Exp(-Mathf.Max(0f, sharpness) * deltaTime);
    }

    static Vector3 DampVector(Vector3 current, Vector3 target, float sharpness, float deltaTime)
    {
        return Vector3.Lerp(current, target, DampingFactor(sharpness, deltaTime));
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y < 0.5f)
        { // Pared vertical (inclinación > 60°)
            Vector3 slideDir = Vector3.ProjectOnPlane(_planarVelocity, hit.normal);
            _planarVelocity = slideDir; // Slide suave en lugar de parar
        }

        // Cancel velocity hacia paredes inclinadas > 45°
        float dotUp = Vector3.Dot(hit.normal, _currentUp);
        if (dotUp < 0.7071f)
        { // > 45°
            float normalDot = Vector3.Dot(_planarVelocity, hit.normal);
            if (normalDot < 0f)
                _planarVelocity -= hit.normal * normalDot;
        }
    }

    struct GroundProbe
    {
        public bool isGrounded;
        public RaycastHit hit;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        CharacterController cc = _controller != null ? _controller : GetComponent<CharacterController>();
        if (cc == null) return;

        // CharacterController bounds (cyan-blue wireframe)
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position + cc.center, cc.radius);
        Gizmos.DrawWireSphere(transform.position + cc.center + Vector3.up * (cc.height * 0.5f), cc.radius);
        Gizmos.DrawWireSphere(transform.position + cc.center - Vector3.up * (cc.height * 0.5f), cc.radius);

        // GroundCheck probe (green if grounded, red if not)
        if (groundCheck != null)
        {
            Gizmos.color = _grounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundProbeRadius);
            Gizmos.DrawRay(groundCheck.position, -_currentUp * groundProbeDistance);

            // SphereCast visualization
            if (Physics.SphereCast(
                groundCheck.position + _currentUp * groundProbeRadius,
                groundProbeRadius,
                -_currentUp,
                out RaycastHit gizmoHit,
                groundProbeDistance + groundProbeRadius,
                groundCheckMask != 0 ? groundCheckMask : Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(gizmoHit.point, 0.08f);
            }
        }

        // Player velocity (yellow forward)
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, _planarVelocity);
    }
#endif
}
