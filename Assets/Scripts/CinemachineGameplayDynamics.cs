using UnityEngine;
using Unity.Cinemachine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CinemachineCamera))]
public class CinemachineGameplayDynamics : MonoBehaviour
{
    [Header("Lens")]
    [SerializeField] float baseFov = 52f;
    [SerializeField] float speedFovBoost = 4f;
    [SerializeField] float fovDamping = 4f;

    [Header("Tilt")]
    [SerializeField] float tiltOnJump = 1.5f;
    [SerializeField] float dutchMax = 2f;
    [SerializeField] float tiltDamping = 2f;

    [Header("Landing")]
    [SerializeField] float landingOffsetDip = 0.08f;
    [SerializeField] float landingRecoverySpeed = 4f;

    [Header("Follow")]
    [SerializeField] Vector3 baseFollowOffset = new Vector3(-8f, 9f, -12f);
    [SerializeField] float movementInfluence = 0.12f;
    [SerializeField] float followDamping = 3.5f;

    CinemachineCamera _vcam;
    CinemachineFollow _follow;
    PlayerController _player;
    float _currentOffsetDip;
    float _targetOffsetDip;
    float _currentFov;
    float _jumpTiltTimer;

    void OnEnable()
    {
        if (_player == null) CacheReferences();
        if (_player != null)
        {
            _player.OnLanded += PlayLandingTilt;
            _player.OnJumped += OnJump;
        }
    }

    void OnDisable()
    {
        if (_player != null)
        {
            _player.OnLanded -= PlayLandingTilt;
            _player.OnJumped -= OnJump;
        }
    }

    void OnJump()
    {
        _jumpTiltTimer = 0.25f;
    }

    public void ApplyProfile(LevelCameraProfiles.Profile profile)
    {
        baseFov = profile.fov;
        followDamping = profile.followResponsiveness;
        speedFovBoost = profile.velocityFovBoost;
        tiltOnJump = profile.tiltOnJump;
        dutchMax = profile.dutchMax;
        baseFollowOffset = profile.followOffset;
    }

    void Start()
    {
        CacheReferences();
        _currentFov = baseFov;
    }

    void LateUpdate()
    {
        if (_vcam == null || !_vcam.IsValid) return;
        CacheReferences();

        float planarSpeed = _player != null
            ? Vector3.ProjectOnPlane(_player.Controller.velocity, Vector3.up).magnitude
            : 0f;
        float speed01 = Mathf.Clamp01(planarSpeed / 12f);
        bool grounded = _player != null && _player.IsGrounded;

        // --- FOV: slight boost while sprinting, only grounded ---
        float targetFov = baseFov + speed01 * speedFovBoost;
        _currentFov = Mathf.Lerp(_currentFov, targetFov, 1f - Mathf.Exp(-fovDamping * Time.deltaTime));
        var lens = _vcam.Lens;
        lens.FieldOfView = _currentFov;

        // --- Dutch: jump spike, fall tilt, slight sprint lean ---
        float targetDutch = 0f;
        if (_jumpTiltTimer > 0f)
        {
            _jumpTiltTimer -= Time.deltaTime;
            targetDutch += tiltOnJump * 0.8f;
        }
        if (!grounded && _jumpTiltTimer <= 0f)
        {
            float verticalSpeed = _player != null ? Mathf.Abs(_player.VerticalSpeed) : 0f;
            targetDutch += Mathf.Clamp01(verticalSpeed / 15f) * tiltOnJump * 0.2f;
        }
        else if (grounded && speed01 > 0.3f)
            targetDutch += speed01 * 0.6f * Mathf.Sin(Time.time * 0.8f);
        targetDutch = Mathf.Clamp(targetDutch, -dutchMax, dutchMax);
        lens.Dutch = Mathf.Lerp(lens.Dutch, targetDutch, 1f - Mathf.Exp(-tiltDamping * Time.deltaTime));
        _vcam.Lens = lens;

        // --- Landing dip ---
        _targetOffsetDip = Mathf.MoveTowards(_targetOffsetDip, 0f, Time.deltaTime * landingRecoverySpeed);
        float dipRate = _targetOffsetDip > _currentOffsetDip ? 0.5f : 0.18f;
        _currentOffsetDip = Mathf.Lerp(_currentOffsetDip, _targetOffsetDip, dipRate);

        // --- FollowOffset: movement influence + landing dip ---
        if (_follow != null)
        {
            Vector3 velocityOffset = speed01 > 0.01f
                ? Vector3.ProjectOnPlane(_player.Controller.velocity, Vector3.up) * (movementInfluence * 0.35f)
                : Vector3.zero;
            Vector3 targetOffset = baseFollowOffset + velocityOffset;
            targetOffset.y -= _currentOffsetDip;

            _follow.FollowOffset = Vector3.Lerp(
                _follow.FollowOffset,
                targetOffset,
                1f - Mathf.Exp(-followDamping * Time.deltaTime));
        }
    }

    public void PlayLandingTilt(float impactSpeed)
    {
        _targetOffsetDip = Mathf.Clamp(impactSpeed * landingOffsetDip * 1.2f, 0f, 3f);
    }

    void CacheReferences()
    {
        if (_vcam == null) _vcam = GetComponent<CinemachineCamera>();
        if (_vcam != null && _follow == null) _follow = _vcam.GetComponent<CinemachineFollow>();
        if (_player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerObj.TryGetComponent(out _player);
        }
    }
}