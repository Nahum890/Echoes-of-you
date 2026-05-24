using Cinemachine;
using UnityEngine;

/// <summary>
/// Cámara adaptativa por identidad (A–E): FOV por velocidad, tilt en salto, drift de memoria, etc.
/// </summary>
[DisallowMultipleComponent]
public class CinematicCameraDynamics : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] Transform followTarget;
    [SerializeField] Vector3 baseOffset = new Vector3(-5.5f, 3.2f, -9.5f);
    [SerializeField] float movementInfluence = 0.22f;
    [SerializeField] float noiseAmplitude = 0.35f;
    [SerializeField] float noiseFrequency = 0.18f;
    [SerializeField] float fovBase = 52f;
    [SerializeField] float fovSpeedBoost = 4f;
    [SerializeField] float dutchMax = 2.2f;
    [SerializeField] float followLerpSpeed = 3.5f;

    CinemachineTransposer _transposer;
    PlayerController _playerController;
    EchoesCameraIdentity _identity = EchoesCameraIdentity.DynamicFollow;
    Vector3 _noiseSeed;
    Vector3 _delayedFollowPoint;
    float _currentFov;
    float _driftDelay;
    float _tiltOnJump;
    float _responsiveness = 3.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureOnCamera()
    {
        Camera cameraRef = Camera.main;
        if (cameraRef == null || cameraRef.GetComponent<CinematicCameraDynamics>() != null)
            return;

        cameraRef.gameObject.AddComponent<CinematicCameraDynamics>();
    }

    void Awake()
    {
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
            _transposer.m_FollowOffset = baseOffset;
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
            virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();

        if (virtualCamera != null && _transposer == null)
            _transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

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
        CacheReferences();
        if (_transposer == null || followTarget == null)
            return;

        Vector3 planarVelocity = Vector3.zero;
        if (_playerController != null)
            planarVelocity = Vector3.ProjectOnPlane(_playerController.Controller.velocity, Vector3.up);

        float speed01 = Mathf.Clamp01(planarVelocity.magnitude / 12f);
        Vector3 targetOffset = ResolveOffset(planarVelocity, speed01);
        float time = Time.time * noiseFrequency;
        Vector3 noise = BuildNoise(time);

        _transposer.m_FollowOffset = Vector3.Lerp(
            _transposer.m_FollowOffset,
            targetOffset + noise,
            Time.deltaTime * followLerpSpeed);

        _currentFov = Mathf.Lerp(_currentFov, fovBase + speed01 * fovSpeedBoost, Time.deltaTime * 4f);
        var lens = virtualCamera.m_Lens;
        lens.FieldOfView = _currentFov;

        float dutch = speed01 * dutchMax;
        if (_playerController != null && !_playerController.IsGrounded)
            dutch += _tiltOnJump * 0.15f;

        lens.Dutch = Mathf.Lerp(lens.Dutch, dutch * Mathf.Sin(Time.time * 0.7f), Time.deltaTime * 2f);
        virtualCamera.m_Lens = lens;

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
        _transposer.m_FollowOffset += drift;
    }
}
