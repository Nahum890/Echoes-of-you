using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Cámara adaptativa por identidad (A–E): FOV por velocidad, tilt en salto, drift de memoria, etc.
/// </summary>
[DisallowMultipleComponent]
public class CinematicCameraDynamics : MonoBehaviour
{
    [SerializeField] CinemachineCamera virtualCamera;
    [SerializeField] Transform followTarget;
    [SerializeField] Vector3 baseOffset = new Vector3(-5.5f, 3.2f, -9.5f);
    [SerializeField] float movementInfluence = 0.22f;
    [SerializeField] float noiseAmplitude = 0.35f;
    [SerializeField] float noiseFrequency = 0.18f;
    [SerializeField] float fovBase = 52f;
    [SerializeField] float fovSpeedBoost = 4f;
    [SerializeField] float dutchMax = 2.2f;
    [SerializeField] float followLerpSpeed = 3.5f;

    CinemachineFollow _transposer;
    PlayerController _playerController;
    EchoesCameraIdentity _identity = EchoesCameraIdentity.DynamicFollow;
    Vector3 _noiseSeed;
    Vector3 _delayedFollowPoint;
    float _currentFov;
    float _driftDelay;
    float _tiltOnJump;
    float _responsiveness = 3.5f;

    float _landingOffsetDip;
    float _pulseTargetFov = -1f;
    float _pulseUntil;

    public void PlayLandingTilt(float impactSpeed)
    {
        _landingOffsetDip = Mathf.Clamp(impactSpeed * 0.18f, 0f, 4f);
    }

    public void RequestFovPulse(float temporaryFov, float holdSeconds = 0.25f)
    {
        _pulseTargetFov = Mathf.Max(38f, temporaryFov);
        _pulseUntil = Time.unscaledTime + Mathf.Max(0.05f, holdSeconds);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureOnCamera()
    {
        Camera cameraRef = Camera.main;
        if (cameraRef == null || cameraRef.GetComponent<CinematicCameraDynamics>() != null)
            return;

        // No inyectar si ThirdPersonCamera ya controla la cámara.
        ThirdPersonCamera tpc = cameraRef.GetComponent<ThirdPersonCamera>();
        if (tpc != null && tpc.enabled)
            return;

        // No inyectar si SimpleFollowCamera controla la cámara (Cinemachine reemplazado).
        SimpleFollowCamera sfc = cameraRef.GetComponent<SimpleFollowCamera>();
        if (sfc != null && sfc.enabled)
            return;

        // No inyectar si CinemachineBrain ya controla la cámara (el usuario usa Cinemachine directamente).
        // CinematicCameraDynamics modifica el VirtualCamera cada frame; si Cinemachine ya gestiona
        // el transform de la cámara, ambos sistemas luchan entre sí causando jitter.
        Unity.Cinemachine.CinemachineBrain brain = cameraRef.GetComponent<Unity.Cinemachine.CinemachineBrain>();
        if (brain != null)
            return;

        cameraRef.gameObject.AddComponent<CinematicCameraDynamics>();
    }

    void Awake()
    {
        if (GetComponent<Unity.Cinemachine.CinemachineBrain>() != null)
        {
            Destroy(this);
            return;
        }

        _noiseSeed = new Vector3(Random.value * 10f, Random.value * 10f, Random.value * 10f);
        _currentFov = fovBase;
        CacheReferences();
    }

    public void ApplyProfile(LevelCameraProfiles.Profile profile)
    {
        _identity = profile.identity;
        baseOffset = profile.followOffset;
        fovBase = profile.fov;
        dutchMax = profile.dutchMax;
        noiseAmplitude = profile.noiseAmplitude;
        fovSpeedBoost = profile.velocityFovBoost;
        _tiltOnJump = profile.tiltOnJump;
        _driftDelay = profile.driftDelay;
        _responsiveness = profile.followResponsiveness;
        followLerpSpeed = profile.followResponsiveness;
        _currentFov = profile.fov;

        ApplyIdentityDefaults(profile.identity);

        CacheReferences();
        if (_transposer != null)
            _transposer.FollowOffset = baseOffset;
    }

    void ApplyIdentityDefaults(EchoesCameraIdentity identity)
    {
        switch (identity)
        {
            case EchoesCameraIdentity.WideLiminal:
                movementInfluence = 0.12f;
                noiseFrequency = 0.1f;
                followLerpSpeed = 2.2f;
                break;
            case EchoesCameraIdentity.DynamicFollow:
                movementInfluence = 0.28f;
                noiseFrequency = 0.22f;
                followLerpSpeed = 5.5f;
                break;
            case EchoesCameraIdentity.SideCinematic:
                movementInfluence = 0.08f;
                noiseFrequency = 0.14f;
                followLerpSpeed = 4f;
                break;
            case EchoesCameraIdentity.TopDescent:
                movementInfluence = 0.15f;
                noiseFrequency = 0.16f;
                followLerpSpeed = 3.8f;
                break;
            case EchoesCameraIdentity.Memory:
                movementInfluence = 0.18f;
                noiseFrequency = 0.24f;
                followLerpSpeed = 1.8f;
                break;
        }
    }

    void CacheReferences()
    {
        if (virtualCamera == null)
            virtualCamera = FindAnyObjectByType<CinemachineCamera>();

        if (virtualCamera != null && _transposer == null)
            _transposer = virtualCamera.GetComponent<CinemachineFollow>();

        if (followTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                followTarget = player.transform;
        }

        if (followTarget != null)
            followTarget.TryGetComponent(out _playerController);
    }

    void LateUpdate()
    {
        // Do NOT call CacheReferences() here every frame — FindAnyObjectByType is expensive
        // and may find the wrong camera if multiple VCams exist (e.g. echo's camera).
        if (_transposer == null || followTarget == null)
            return;

        Vector3 planarVelocity = Vector3.zero;
        if (_playerController != null)
            planarVelocity = Vector3.ProjectOnPlane(_playerController.Controller.velocity, Vector3.up);

        float speed01 = Mathf.Clamp01(planarVelocity.magnitude / 12f);
        Vector3 targetOffset = ResolveOffset(planarVelocity, speed01);
        float time = Time.time * noiseFrequency;
        Vector3 noise = BuildNoise(time);

        _landingOffsetDip = Mathf.MoveTowards(_landingOffsetDip, 0f, Time.deltaTime * 5f);
        targetOffset.y -= _landingOffsetDip;

        _transposer.FollowOffset = Vector3.Lerp(
            _transposer.FollowOffset,
            targetOffset + noise,
            Time.deltaTime * followLerpSpeed);

        if (_pulseTargetFov > 0f)
        {
            if (Time.unscaledTime >= _pulseUntil)
            {
                // Decay the landing pulse target FOV back to base FOV smoothly over time instead of jumping instantly
                _pulseTargetFov = Mathf.MoveTowards(_pulseTargetFov, fovBase, Time.deltaTime * 65f);
                if (Mathf.Approximately(_pulseTargetFov, fovBase))
                    _pulseTargetFov = -1f;
            }
        }
        float targetFovBase = _pulseTargetFov > 0f ? _pulseTargetFov : fovBase;

        // Only apply speed FOV boost while grounded — avoids jarring FOV spike on jump.
        bool isAirborne = _playerController != null && !_playerController.IsGrounded;
        float fovBoost = isAirborne ? 0f : speed01 * fovSpeedBoost;
        _currentFov = Mathf.Lerp(_currentFov, targetFovBase + fovBoost, Time.deltaTime * 4f);
        var lens = virtualCamera.Lens;
        lens.FieldOfView = _currentFov;

        float dutch = isAirborne ? 0f : speed01 * dutchMax;
        if (_playerController != null && !_playerController.IsGrounded)
            dutch += _tiltOnJump * 0.05f;  // minimal tilt, was 0.15f

        lens.Dutch = Mathf.Lerp(lens.Dutch, dutch * Mathf.Sin(Time.time * 0.7f), Time.deltaTime * 2f);
        virtualCamera.Lens = lens;

        ApplyMemoryDrift();
    }

    Vector3 ResolveOffset(Vector3 planarVelocity, float speed01)
    {
        switch (_identity)
        {
            case EchoesCameraIdentity.SideCinematic:
                return new Vector3(baseOffset.x, baseOffset.y + speed01 * 0.4f, baseOffset.z * 0.25f);
            case EchoesCameraIdentity.TopDescent:
                return new Vector3(baseOffset.x * 0.6f, baseOffset.y + speed01 * 1.2f, baseOffset.z * 0.7f);
            case EchoesCameraIdentity.WideLiminal:
                return baseOffset + planarVelocity * (movementInfluence * 0.35f);
            default:
                return baseOffset + planarVelocity * movementInfluence;
        }
    }

    Vector3 BuildNoise(float time)
    {
        float amp = noiseAmplitude;
        if (_identity == EchoesCameraIdentity.Memory)
            amp *= 1.35f;

        return new Vector3(
            Mathf.PerlinNoise(_noiseSeed.x, time) - 0.5f,
            Mathf.PerlinNoise(_noiseSeed.y, time + 2f) - 0.5f,
            Mathf.PerlinNoise(_noiseSeed.z, time + 4f) - 0.5f) * amp;
    }

    void ApplyMemoryDrift()
    {
        if (_identity != EchoesCameraIdentity.Memory || followTarget == null)
            return;

        if (_delayedFollowPoint == Vector3.zero)
            _delayedFollowPoint = followTarget.position;

        float delay = Mathf.Max(0.05f, _driftDelay);
        _delayedFollowPoint = Vector3.Lerp(_delayedFollowPoint, followTarget.position, Time.deltaTime / delay);
        Vector3 drift = (_delayedFollowPoint - followTarget.position) * 0.15f;
        _transposer.FollowOffset += drift;
    }
}
