using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.0f;
    public float acceleration = 24f;
    public float deceleration = 28f;
    public float rotationSharpness = 14f;
    [Range(0.1f, 1f)]
    public float airControlFactor = 0.65f;

    [Header("Salto / Gravedad")]
    public float jumpHeight = 1.55f;
    public float gravityStrength = 26f;
    public Vector3 defaultGravityDirection = Vector3.down;
    public float groundedStickForce = 5f;
    public float gravityBlendSpeed = 9f;
    public float fallGravityMultiplier = 1.4f;
    public bool alignToGroundNormal = true;

    [Header("Jump Assist")]
    public float jumpBufferTime = 0.15f;
    public float coyoteTime = 0.12f;

    [Header("Deteccion de suelo")]
    public Transform groundCheck;
    public float groundProbeRadius = 0.24f;
    public float groundProbeDistance = 0.38f;
    public LayerMask groundMask = (1 << 0) | (1 << 6);

    [Header("Failsafe")]
    public float voidHeight = -15f;

    CharacterController _controller;
    Transform _cam;
    Animator _anim;
    Rigidbody _rb;

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

    // Jump buffer & coyote
    float _jumpBufferTimer;
    float _coyoteTimer;

    public Vector3 UpAxis => _currentUp;
    public Vector3 GravityDirection => _currentGravity.sqrMagnitude > 0.0001f ? _currentGravity.normalized : Vector3.down;
    public Vector3 GravityVector => _currentGravity;
    public bool IsGrounded => _grounded;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        TryGetComponent(out _rb);
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        EnsureGroundCheck();
        EnsureVisualAnimator();
        RefreshCameraReference();

        _anim = GetComponentInChildren<Animator>();
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
        HandleMovementInput(movementUp);
        HandleJumpInput(movementUp);

        if (!_grounded || _jumpedThisFrame)
        {
            float gravMul = 1f;
            float downSpeed = Vector3.Dot(_verticalVelocity, GravityDirection);
            if (downSpeed > 0f && !_jumpedThisFrame)
                gravMul = fallGravityMultiplier;
            _verticalVelocity += _currentGravity * gravMul * Time.deltaTime;
        }

        _isFalling = !_grounded && Vector3.Dot(_verticalVelocity, GravityDirection) > 0.5f;

        Vector3 motion = (_planarVelocity + _verticalVelocity) * Time.deltaTime;
        _controller.Move(motion);

        GroundProbe postMoveProbe = ProbeGround(movementUp);
        _grounded = postMoveProbe.isGrounded || _controller.isGrounded;

        if (_grounded && !_jumpedThisFrame)
            _verticalVelocity = GravityDirection * groundedStickForce;

        if (!_wasGrounded && _grounded)
        {
            GameFeelController.Instance?.PlayLanding(transform.position, movementUp, downwardSpeed);
        }

        UpdateOrientation(postMoveProbe, Time.deltaTime);
        UpdateAnimator();

        _jumpedThisFrame = false;
    }

    public void RegisterGravityZone(GravityZone zone)
    {
        if (zone == null || _gravityZones.Contains(zone))
            return;

        _gravityZones.Add(zone);
    }

    public void UnregisterGravityZone(GravityZone zone)
    {
        if (zone == null)
            return;

        _gravityZones.Remove(zone);
    }

    public void ForceGravity(Vector3 worldDirection, float strength = -1f, bool playFeedback = true)
    {
        Vector3 nextGravity = SafeGravity(worldDirection, strength > 0f ? strength : gravityStrength);
        if (playFeedback && Vector3.Angle(_targetGravity, nextGravity) > 8f)
            GameFeelController.Instance?.PlayGravityShift(transform.position, -nextGravity.normalized);

        _targetGravity = nextGravity;
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

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);
        Vector3 desiredVelocity = desiredDirection * speed;

        _planarVelocity = Vector3.ProjectOnPlane(_planarVelocity, movementUp);
        float sharpness = desiredVelocity.sqrMagnitude > 0.001f ? acceleration : deceleration;
        if (!_grounded)
            sharpness *= airControlFactor;
        _planarVelocity = DampVector(_planarVelocity, desiredVelocity, sharpness, Time.deltaTime);
    }

    void HandleJumpInput(Vector3 movementUp)
    {
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

        float jumpSpeed = Mathf.Sqrt(2f * gravityStrength * jumpHeight);
        _verticalVelocity = movementUp * jumpSpeed;
        _grounded = false;
        _jumpedThisFrame = true;

        if (_anim != null)
            _anim.SetTrigger("Jump");

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
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, DampingFactor(rotationSharpness, deltaTime));
    }

    void UpdateAnimator()
    {
        if (_anim == null)
            return;

        Vector3 flatVelocity = Vector3.ProjectOnPlane(_controller.velocity, _currentUp);
        _anim.SetFloat("Speed", flatVelocity.magnitude);
        
        Vector3 localVelocity = transform.InverseTransformDirection(_controller.velocity);
        _anim.SetFloat("VelocityX", localVelocity.x);
        _anim.SetFloat("VelocityZ", localVelocity.z);
        
        _anim.SetBool("Grounded", _grounded);
        _anim.SetBool("Falling", _isFalling);
    }

    void UpdateTargetGravity()
    {
        GravityZone strongestZone = null;
        int highestPriority = int.MinValue;

        for (int i = _gravityZones.Count - 1; i >= 0; i--)
        {
            GravityZone zone = _gravityZones[i];
            if (zone == null)
            {
                _gravityZones.RemoveAt(i);
                continue;
            }

            if (zone.Priority >= highestPriority)
            {
                highestPriority = zone.Priority;
                strongestZone = zone;
            }
        }

        Vector3 nextGravity = strongestZone != null
            ? strongestZone.GetGravityVector()
            : SafeGravity(defaultGravityDirection, gravityStrength);

        if (Vector3.Angle(_targetGravity, nextGravity) > 8f || Mathf.Abs(_targetGravity.magnitude - nextGravity.magnitude) > 0.5f)
            GameFeelController.Instance?.PlayGravityShift(transform.position, -nextGravity.normalized);

        _targetGravity = nextGravity;
    }

    void BlendGravity(float deltaTime)
    {
        Vector3 currentDirection = _currentGravity.sqrMagnitude > 0.0001f ? _currentGravity.normalized : Vector3.down;
        Vector3 targetDirection = _targetGravity.normalized;
        float blend = DampingFactor(gravityBlendSpeed, deltaTime);
        Vector3 blendedDirection = Vector3.Slerp(currentDirection, targetDirection, blend).normalized;
        float blendedStrength = Mathf.Lerp(_currentGravity.magnitude, _targetGravity.magnitude, blend);
        _currentGravity = blendedDirection * blendedStrength;
    }

    Vector3 ResolveMovementUp(GroundProbe probe)
    {
        if (alignToGroundNormal && probe.hit.collider != null)
            return probe.hit.normal;

        return _currentUp;
    }

    GroundProbe ProbeGround(Vector3 probeUp)
    {
        Vector3 normalizedUp = probeUp.sqrMagnitude > 0.001f ? probeUp.normalized : transform.up;
        Vector3 origin = groundCheck != null
            ? groundCheck.position + normalizedUp * groundProbeRadius
            : transform.position + normalizedUp * groundProbeRadius;

        RaycastHit hit;
        bool grounded = Physics.SphereCast(
            origin,
            groundProbeRadius,
            -normalizedUp,
            out hit,
            groundProbeDistance + groundProbeRadius,
            groundMask,
            QueryTriggerInteraction.Ignore);

        if (grounded)
            grounded = hit.distance <= groundProbeDistance + 0.02f;

        return new GroundProbe
        {
            isGrounded = grounded,
            hit = hit
        };
    }

    void EnsureGroundCheck()
    {
        if (groundCheck == null)
        {
            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform, false);
            groundCheck = gc.transform;
        }

        float localYOffset = -((_controller.height * 0.5f) - _controller.radius + 0.02f);
        groundCheck.localPosition = new Vector3(0f, localYOffset, 0f);
        groundCheck.localRotation = Quaternion.identity;
    }

    void RefreshCameraReference()
    {
        if (Camera.main != null)
            _cam = Camera.main.transform;
    }

    void EnsureVisualAnimator()
    {
        Transform visualRoot = transform.Find("PlayerVisual");
        if (visualRoot == null)
        {
            GameObject root = new GameObject("PlayerVisual");
            root.transform.SetParent(transform, false);
            visualRoot = root.transform;
        }

        if (visualRoot.childCount == 0)
            CreateFallbackVisual(visualRoot);

        Transform modelRoot = visualRoot.GetChild(0);
        Animator visualAnimator = modelRoot.GetComponent<Animator>();
        if (visualAnimator == null)
            visualAnimator = modelRoot.GetComponentInChildren<Animator>(true);
        if (visualAnimator != null)
        {
            visualAnimator.applyRootMotion = false;
            visualAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        SkinnedMeshRenderer[] renderers = modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].updateWhenOffscreen = true;
    }

    void CreateFallbackVisual(Transform visualRoot)
    {
        // Try loading the Poly Style character model
        #if UNITY_EDITOR
        GameObject polyChar = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3D Models/Models/FBX format/character-oobi.fbx");
        if (polyChar != null)
        {
            GameObject instance = Instantiate(polyChar, visualRoot);
            instance.name = "PlayerModel";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * 1.2f;

            Collider[] cols = instance.GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
                Destroy(cols[i]);
            return;
        }
        #endif

        // Fallback capsule if no model found
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = "FallbackCapsule";
        capsule.transform.SetParent(visualRoot, false);
        capsule.transform.localPosition = new Vector3(0f, 1.05f, 0f);
        capsule.transform.localRotation = Quaternion.identity;
        capsule.transform.localScale = new Vector3(0.8f, 1.05f, 0.8f);

        Collider capsuleCollider = capsule.GetComponent<Collider>();
        if (capsuleCollider != null)
            Destroy(capsuleCollider);

        Renderer rendererRef = capsule.GetComponent<Renderer>();
        if (rendererRef != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.87f, 0.91f, 0.96f, 1f);
            rendererRef.sharedMaterial = material;
        }
    }

    void Die()
    {
        _isDead = true;
        _controller.enabled = false;
        Invoke(nameof(RestartScene), 1.2f);
    }

    void RestartScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void OnGUI()
    {
        if (!_isDead)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 50;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.red;
        style.fontStyle = FontStyle.Bold;

        Rect bounds = new Rect(0, 0, Screen.width, Screen.height);
        var shadowStyle = new GUIStyle(style);
        shadowStyle.normal.textColor = Color.black;

        Rect shadowRect = new Rect(2, 2, Screen.width, Screen.height);
        GUI.Label(shadowRect, "HAS CAIDO AL VACIO", shadowStyle);
        GUI.Label(bounds, "HAS CAIDO AL VACIO", style);

        style.fontSize = 20;
        style.normal.textColor = Color.white;
        shadowStyle.fontSize = 20;
        shadowStyle.normal.textColor = Color.black;

        Rect hintRect = new Rect(0, Screen.height * 0.5f + 40, Screen.width, 50);
        Rect hintShadowRect = new Rect(2, Screen.height * 0.5f + 42, Screen.width, 50);
        GUI.Label(hintShadowRect, "Reiniciando...", shadowStyle);
        GUI.Label(hintRect, "Reiniciando...", style);
    }

    static Vector3 SafeGravity(Vector3 direction, float strength)
    {
        Vector3 fallback = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.down;
        return fallback * Mathf.Max(0.01f, strength);
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
