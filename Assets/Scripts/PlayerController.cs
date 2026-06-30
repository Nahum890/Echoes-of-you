using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterController))]
public partial class PlayerController : MonoBehaviour
{
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

    [Header("Movimiento")]
    public float moveSpeed = 8f;
    public float sprintMultiplier = 1.6f;
    public float acceleration = 36f;
    public float deceleration = 38f;
    public float rotationSharpness = 16f;
    [Range(0.1f, 1f)]
    public float airControlFactor = 0.72f;

    [Header("Salto / Gravedad")]
    public float jumpHeight = 3.2f;
    public float gravityStrength = 28f;
    public Vector3 defaultGravityDirection = Vector3.down;
    public float groundedStickForce = 5f;
    public float gravityBlendSpeed = 12f;
    public float fallGravityMultiplier = 2.0f;
    public bool alignToGroundNormal = true;

    [Header("Jump Assist")]
    public float jumpBufferTime = 0.2f;
    public float coyoteTime = 0.2f;

    [Header("Peso / Game Feel")]
    [SerializeField] float hardLandingSpeed = 13f;
    [SerializeField] float softLandingPause = 0.012f;
    [SerializeField] float hardLandingPause = 0.028f;
    [SerializeField] float footstepDistance = 1.75f;
    [SerializeField] float movementScrapeSpeed = 4.5f;

    [Header("Deteccion de suelo")]
    public Transform groundCheck;
    public float groundProbeRadius = 0.24f;
    public float groundProbeDistance = 0.38f;
    public LayerMask groundMask = (1 << 6); // Solo layer Ground — incluir Default causa que el SphereCast detecte al propio jugador

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
    float _sprintMomentumBonus;
    float _gravityScale = 1f;
    Vector3 _platformVelocity;

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
        _controller = GetComponent<CharacterController>();
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

        Vector3 initialForward = Vector3.ProjectOnPlane(transform.forward, _currentUp);
        if (initialForward.sqrMagnitude < 0.001f)
            initialForward = Vector3.ProjectOnPlane(transform.right, _currentUp);
        if (initialForward.sqrMagnitude < 0.001f)
            initialForward = Vector3.Cross(transform.right, _currentUp);

        _lastFacing = initialForward.normalized;
        transform.rotation = Quaternion.LookRotation(_lastFacing, _currentUp);
    }

    void OnEnable() => ForceUnlockAndReset();

    void Start() => ForceUnlockAndReset();

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
        _grounded = preMoveProbe.isGrounded || _controller.isGrounded;

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
            _grounded = lockedProbe.isGrounded || _controller.isGrounded;
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
                _grounded = postMoveProbe.isGrounded || _controller.isGrounded;
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
            GameFeelController.Instance?.PlayLanding(transform.position, movementUp, downwardSpeed);
            if (_lastHardLanding)
                TriggerAnimatorIfExists(AnimatorParamHardLanding);

            // Trigger visual and physical landing tilt on the active third-person camera or cinematic camera dynamics
            ThirdPersonCamera activeCam = ThirdPersonCamera.ResolveActive();
            if (activeCam != null)
                activeCam.PlayLandingTilt(downwardSpeed);
            else
            {
                CinematicCameraDynamics cinematicCam = FindAnyObjectByType<CinematicCameraDynamics>();
                if (cinematicCam != null)
                    cinematicCam.PlayLandingTilt(downwardSpeed);
            }
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

    public void SetSprintMomentumBonus(float bonus) => _sprintMomentumBonus = Mathf.Max(0f, bonus);

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
            cameraForward = Vector3.ProjectOnPlane(_cam.forward, movementUp).normalized;
            if (cameraForward.sqrMagnitude < 0.001f)
                cameraForward = Vector3.ProjectOnPlane(_cam.up, movementUp).normalized;

            cameraRight = Vector3.ProjectOnPlane(_cam.right, movementUp).normalized;
            if (cameraRight.sqrMagnitude < 0.001f)
                cameraRight = Vector3.Cross(movementUp, cameraForward).normalized;
        }

        Vector3 desiredDirection = cameraForward * input.z + cameraRight * input.x;
        desiredDirection = Vector3.ProjectOnPlane(desiredDirection, movementUp);
        if (desiredDirection.sqrMagnitude > 1f)
            desiredDirection.Normalize();

        float speed = moveSpeed * (1f + _sprintMomentumBonus);
        if (Input.GetKey(KeyCode.LeftShift))
            speed *= sprintMultiplier;
        Vector3 desiredVelocity = desiredDirection * speed;

        _planarVelocity = Vector3.ProjectOnPlane(_planarVelocity, movementUp);
        float sharpness = desiredVelocity.sqrMagnitude > 0.001f ? acceleration : deceleration;
        if (!_grounded)
            sharpness *= airControlFactor;
        if (_landingLockTimer > 0f)
        {
            _landingLockTimer -= Time.deltaTime;
            float retention = EchoesLocomotionSettings.Instance != null
                ? EchoesLocomotionSettings.Instance.landingVelocityRetention
                : 1f;
            
            // Rebuild landing impact: apply a physical stumble/slowdown depending on whether landing was hard
            float penaltyFactor = _lastHardLanding ? 0.15f : 0.65f;
            desiredVelocity = Vector3.Lerp(desiredVelocity * penaltyFactor, _planarVelocity, retention);
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

        float jumpSpeed = Mathf.Sqrt(2f * gravityStrength * jumpHeight);
        _verticalVelocity = movementUp * jumpSpeed;
        _grounded = false;
        _jumpedThisFrame = true;

        if (HasAnimatorParameter(AnimatorLegacyJump, AnimatorControllerParameterType.Trigger))
            _anim.SetTrigger(AnimatorLegacyJump);
        TriggerAnimatorIfExists("JumpStart");

        GameFeelController.Instance?.PlayJump(transform.position, movementUp);
    }

    void UpdateOrientation(GroundProbe probe, float deltaTime)
    {
        Vector3 desiredUp = -GravityDirection;
        if (alignToGroundNormal && probe.hit.collider != null)
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

    struct GroundProbe
    {
        public bool isGrounded;
        public RaycastHit hit;
    }
}
