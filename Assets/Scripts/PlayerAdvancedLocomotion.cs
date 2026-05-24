using UnityEngine;

/// <summary>
/// Parkour avanzado: slide, agarre, wall jump/run, dash aéreo y momentum de sprint.
/// Preserva velocidad; no detiene al jugador al aterrizar.
/// </summary>
[RequireComponent(typeof(PlayerController))]
[DefaultExecutionOrder(-40)]
public class PlayerAdvancedLocomotion : MonoBehaviour
{
    PlayerController _player;
    CharacterController _cc;
    EchoesLocomotionSettings _cfg;

    float _sprintMomentum;
    float _slideTimer;
    float _ledgeTimer;
    float _wallRunTimer;
    int _wallRunUses;
    float _airDashCooldown;
    Vector3 _wallNormal;
    bool _wallRunning;
    bool _sliding;
    bool _ledgeHanging;

    public bool IsSliding => _sliding;
    public bool IsWallRunning => _wallRunning;
    public bool IsLedgeHanging => _ledgeHanging;
    public float SprintMomentum01 => _sprintMomentum;

    void Awake()
    {
        _player = GetComponent<PlayerController>();
        _cc = GetComponent<CharacterController>();
        _cfg = EchoesLocomotionSettings.Instance;
    }

    void Update()
    {
        if (_player == null || _cfg == null)
            return;

        if (_player.IsInputLocked || !_player.IsAlive)
        {
            ResetStates();
            return;
        }

        float dt = Time.deltaTime;
        UpdateSprintMomentum(dt);
        TryAirDash();
        TrySlide(dt);
        TryLedgeGrab(dt);
        TryWallInteraction(dt);
    }

    void ResetStates()
    {
        _sliding = false;
        _ledgeHanging = false;
        _wallRunning = false;
    }

    void UpdateSprintMomentum(float dt)
    {
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        float speed = _player.PlanarSpeed;

        if (sprinting && _player.IsGrounded && speed >= _cfg.minSpeedForMomentum)
            _sprintMomentum = Mathf.Clamp01(_sprintMomentum + _cfg.sprintMomentumBuildRate * dt);
        else
            _sprintMomentum = Mathf.Clamp01(_sprintMomentum - _cfg.sprintMomentumDecayRate * dt);

        _player.SetSprintMomentumBonus(_sprintMomentum * _cfg.sprintMomentumMaxBonus);
    }

    void TrySlide(float dt)
    {
        if (_slideTimer > 0f)
        {
            _slideTimer -= dt;
            Vector3 vel = _player.PlanarVelocity;
            float speed = vel.magnitude;
            if (speed > 0.1f)
            {
                vel = Vector3.MoveTowards(vel, vel.normalized * (speed * _cfg.slideSpeedMultiplier), _cfg.slideFriction * dt);
                _player.SetPlanarVelocity(vel);
            }

            if (_slideTimer <= 0f || !_player.IsGrounded)
                _sliding = false;
            return;
        }

        if (!_player.IsGrounded)
            return;

        if (!Input.GetKeyDown(_cfg.slideKey))
            return;

        if (_player.PlanarSpeed < _cfg.slideMinSpeed)
            return;

        _sliding = true;
        _slideTimer = _cfg.slideDuration;
        Vector3 slideDir = _player.PlanarVelocity.normalized;
        _player.SetPlanarVelocity(slideDir * (_player.PlanarSpeed * _cfg.slideSpeedMultiplier));
        GameFeelController.Instance?.PlayMovementScrape(transform.position, transform.up, 0.9f);
        TriggerAnim("Slide");
    }

    void TryLedgeGrab(float dt)
    {
        if (_ledgeHanging)
        {
            _ledgeTimer -= dt;
            _player.SetInputLocked(true);
            _player.SetPlanarVelocity(Vector3.zero);
            _player.SetVerticalStick();

            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 up = _player.UpAxis;
                Vector3 climbForward = Vector3.ProjectOnPlane(transform.forward, up).normalized;
                _player.SetInputLocked(false);
                _player.AddVerticalImpulse(up, _cfg.ledgeClimbImpulse);
                _player.AddPlanarImpulse(climbForward * (_cfg.ledgeClimbImpulse * 0.55f));
                _ledgeHanging = false;
                TriggerAnim("LedgeClimb");
            }
            else if (_ledgeTimer <= 0f)
            {
                _ledgeHanging = false;
                _player.SetInputLocked(false);
            }

            return;
        }

        if (_player.IsGrounded || _player.VerticalSpeed > 0.5f)
            return;

        Vector3 origin = transform.position + _player.UpAxis * _cfg.ledgeProbeUp;
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, _player.UpAxis).normalized;
        if (forward.sqrMagnitude < 0.01f)
            return;

        Vector3 probe = origin + forward * _cfg.ledgeProbeForward;
        if (!Physics.Raycast(probe, -_player.UpAxis, out RaycastHit topHit, _cfg.ledgeProbeDown, _player.GroundMask, QueryTriggerInteraction.Ignore))
            return;

        if (!Physics.Raycast(origin, forward, _cfg.ledgeProbeForward + 0.2f, _player.GroundMask, QueryTriggerInteraction.Ignore))
        {
            _ledgeHanging = true;
            _ledgeTimer = _cfg.ledgeHangDuration;
            _player.SetInputLocked(true);
            transform.position = topHit.point - forward * 0.15f + _player.UpAxis * 0.05f;
            TriggerAnim("LedgeGrab");
        }
    }

    void TryWallInteraction(float dt)
    {
        if (_player.IsGrounded)
        {
            _wallRunUses = 0;
            _wallRunning = false;
            return;
        }

        Vector3 up = _player.UpAxis;
        Vector3[] dirs =
        {
            Vector3.ProjectOnPlane(transform.forward, up).normalized,
            Vector3.ProjectOnPlane(-transform.forward, up).normalized,
            Vector3.ProjectOnPlane(transform.right, up).normalized,
            Vector3.ProjectOnPlane(-transform.right, up).normalized
        };

        bool wallFound = false;
        for (int i = 0; i < dirs.Length; i++)
        {
            if (dirs[i].sqrMagnitude < 0.01f)
                continue;

            if (Physics.Raycast(transform.position + up * 0.5f, dirs[i], out RaycastHit hit, _cfg.wallProbeDistance, _player.GroundMask, QueryTriggerInteraction.Ignore))
            {
                _wallNormal = hit.normal;
                wallFound = true;
                break;
            }
        }

        if (!wallFound)
        {
            _wallRunning = false;
            return;
        }

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 jumpDir = (_wallNormal + up).normalized;
            _player.AddVerticalImpulse(up, _cfg.wallJumpImpulse);
            _player.SetPlanarVelocity(jumpDir * _cfg.wallJumpAwayForce);
            _wallRunning = false;
            GameFeelController.Instance?.PlayJump(transform.position, up);
            TriggerAnim("WallJump");
            return;
        }

        if (_wallRunUses < _cfg.wallRunMaxCount && _player.PlanarSpeed > 3f)
        {
            _wallRunning = true;
            _wallRunTimer += dt;
            Vector3 alongWall = Vector3.Cross(_wallNormal, up).normalized;
            if (Vector3.Dot(alongWall, _player.PlanarVelocity) < 0f)
                alongWall = -alongWall;

            _player.SetPlanarVelocity(Vector3.Lerp(_player.PlanarVelocity, alongWall * Mathf.Max(_player.PlanarSpeed, 6f), dt * 8f));
            _player.ApplyGravityScale(_cfg.wallRunGravityMultiplier);

            if (_wallRunTimer >= _cfg.wallRunDuration)
            {
                _wallRunTimer = 0f;
                _wallRunUses++;
                _wallRunning = false;
            }
        }
    }

    void TryAirDash()
    {
        if (_airDashCooldown > 0f)
            _airDashCooldown -= Time.deltaTime;

        if (_player.IsGrounded)
            return;

        if (!Input.GetKeyDown(_cfg.airDashKey))
            return;

        if (_airDashCooldown > 0f)
            return;

        int levelIndex = GameProgress.GetSceneIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        if (levelIndex < _cfg.airDashUnlockAtLevelIndex)
            return;

        Transform cam = Camera.main != null ? Camera.main.transform : transform;
        Vector3 dash = Vector3.ProjectOnPlane(cam.forward, _player.UpAxis);
        if (dash.sqrMagnitude < 0.01f)
            dash = Vector3.ProjectOnPlane(transform.forward, _player.UpAxis);
        dash.Normalize();

        _player.SetPlanarVelocity(dash * _cfg.airDashImpulse);
        _airDashCooldown = _cfg.airDashCooldown;
        GameFeelController.Instance?.PlayJump(transform.position, _player.UpAxis);
        TriggerAnim("AirDash");
    }

    void TriggerAnim(string triggerName)
    {
        Animator anim = GetComponentInChildren<Animator>();
        if (anim == null || anim.runtimeAnimatorController == null)
            return;

        AnimatorControllerParameter[] parameters = anim.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == AnimatorControllerParameterType.Trigger && parameters[i].name == triggerName)
            {
                anim.SetTrigger(triggerName);
                return;
            }
        }
    }
}
