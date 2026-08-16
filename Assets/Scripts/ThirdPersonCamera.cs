using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Third-person camera with smooth follow, orbital mouse control,
/// support for arbitrary player up vectors.
/// Simplified for PS1 liminal feel - no auto-frame, no auto-recenter, no dynamic FOV.
/// Cinemachine v3 handles blends/narrative cuts; this is gameplay camera only.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 focusOffset = new Vector3(0f, 1.4f, 0f);
    public float distance = 4.1f;

    [Header("Smoothing")]
    public float followDamping = 25f;
    public float rotationDamping = 30f;
    public float fovDamping = 20f;

    [Header("Orbit")]
    public float mouseSensitivity = 1.15f;
    public float minPitch = 10f;
    public float maxPitch = 24f;
    public bool lockCursorOnStart = true;

    [Header("Composition")]
    public bool clampYaw = true;
    public float maxYawOffset = 28f;
    public Vector3 authoredForward = Vector3.forward;
    public float baseFov = 52f;
    public float nearClip = 0.3f;
    public float farClip = 300f;

    [Header("Camera Collision")]
    [Tooltip("Radio de la esfera para el cast de colisión de cámara.")]
    [SerializeField] float collisionRadius = 0.25f;
    [Tooltip("Capa a ignorar al hacer cast (player, echo, etc.). Por defecto detecta todo menos Ignore Raycast.")]
    [SerializeField] LayerMask collisionMask = ~0;
    [Tooltip("Distancia mínima a la que la cámara puede acercarse al focus point cuando choca.")]
    [SerializeField] float minCollisionDistance = 0.6f;
    [Tooltip("Cuánto suavizar el retroceso de colisión (mayor = más snappy).")]
    [SerializeField] float collisionDamping = 18f;

    float _pitch = 12f;
    Vector3 _smoothedFocusPoint;
    Vector3 _orbitForward = Vector3.forward;
    CameraShake _cameraShake;
    Camera _camera;

    Vector3 _lastFocusPoint;
    float _landingTiltAmount;

    public static ThirdPersonCamera ResolveActive()
    {
        Camera main = Camera.main;
        if (main != null && main.TryGetComponent(out ThirdPersonCamera cameraRef))
            return cameraRef;

        return FindAnyObjectByType<ThirdPersonCamera>();
    }

    void OnEnable()
    {
        if (GetComponent<SimpleFollowCamera>() != null && GetComponent<SimpleFollowCamera>().enabled)
        {
            enabled = false;
            return;
        }
        CheckAndDisableIfCinemachineActive();
    }

    public bool CheckAndDisableIfCinemachineActive()
    {
        if (IsCinemachineActiveInScene())
        {
            enabled = false;
            return true;
        }
        return false;
    }

    public bool IsCinemachineActiveInScene()
    {
        if (GetComponent<Unity.Cinemachine.CinemachineBrain>() != null && GetComponent<Unity.Cinemachine.CinemachineBrain>().enabled)
            return true;

        var brain = FindAnyObjectByType<Unity.Cinemachine.CinemachineBrain>();
        if (brain != null && brain.enabled)
            return true;

        var vcam = FindAnyObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (vcam != null && vcam.enabled && vcam.gameObject.activeInHierarchy)
            return true;

        System.Type vcamV2Type = System.Type.GetType("Cinemachine.CinemachineVirtualCamera, Cinemachine")
                              ?? System.Type.GetType("Cinemachine.CinemachineVirtualCamera");
        if (vcamV2Type != null)
        {
            var vcamObj = FindAnyObjectByType(vcamV2Type) as MonoBehaviour;
            if (vcamObj != null && vcamObj.enabled && vcamObj.gameObject.activeInHierarchy)
                return true;
        }

        return false;
    }

    void Start()
    {
        _cameraShake = GetComponent<CameraShake>();
        _camera = GetComponent<Camera>();

        if (GetComponent<SimpleFollowCamera>() != null && GetComponent<SimpleFollowCamera>().enabled)
        {
            enabled = false;
            return;
        }
        if (CheckAndDisableIfCinemachineActive())
            return;

        // Auto-resolve target if not assigned in inspector
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                target = playerObj.transform;
        }

        if (target != null)
        {
            Vector3 targetUp = target.up;
            _smoothedFocusPoint = GetFocusPoint();
            _orbitForward = ResolvePlanarForward(transform.forward, target.forward, target.right, targetUp);
            _lastFocusPoint = _smoothedFocusPoint;
        }

        if (_camera != null)
        {
            _camera.fieldOfView = baseFov;
            _camera.nearClipPlane = nearClip;
            _camera.farClipPlane = farClip;
        }

        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        if (CheckAndDisableIfCinemachineActive())
            return;
        // Re-resolve target if lost (destroyed, scene change, etc.)
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }

        if (target == null)
            return;

        Vector3 targetUp = target.up;
        float yawDelta = Input.GetAxis("Mouse X") * mouseSensitivity;
        float pitchDelta = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _orbitForward = Quaternion.AngleAxis(yawDelta, targetUp) * _orbitForward;
        _orbitForward = ResolvePlanarForward(_orbitForward, transform.forward, target.right, targetUp);

        // Decay the landing tilt amount smoothly over time
        _landingTiltAmount = Mathf.Lerp(_landingTiltAmount, 0f, DampingFactor(7.5f, Time.deltaTime));

        _pitch = Mathf.Clamp(_pitch - pitchDelta, minPitch, maxPitch);
        float finalPitch = Mathf.Clamp(_pitch + _landingTiltAmount, minPitch, maxPitch + 18f);

        if (clampYaw)
        {
            Vector3 authoredPlanar = ResolvePlanarForward(target.rotation * authoredForward, target.forward, target.right, targetUp);
            float signedAngle = Vector3.SignedAngle(authoredPlanar, _orbitForward, targetUp);
            float clampedAngle = Mathf.Clamp(signedAngle, -maxYawOffset, maxYawOffset);
            _orbitForward = Quaternion.AngleAxis(clampedAngle, targetUp) * authoredPlanar;
            _orbitForward = ResolvePlanarForward(_orbitForward, transform.forward, target.right, targetUp);
        }

        Vector3 orbitRight = Vector3.Cross(targetUp, _orbitForward).normalized;
        if (orbitRight.sqrMagnitude < 0.001f)
            orbitRight = Vector3.Cross(targetUp, target.right).normalized;
        Vector3 lookDirection = Quaternion.AngleAxis(finalPitch, orbitRight) * _orbitForward;
        lookDirection.Normalize();

        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection, targetUp);
        Vector3 desiredFocusPoint = GetFocusPoint();

        // Failsafe / Teleport snap
        if (Vector3.Distance(_smoothedFocusPoint, desiredFocusPoint) > 5f)
        {
            _smoothedFocusPoint = desiredFocusPoint;
            _lastFocusPoint = desiredFocusPoint;
        }
        else
        {
            _smoothedFocusPoint = Vector3.Lerp(_smoothedFocusPoint, desiredFocusPoint, DampingFactor(followDamping, Time.deltaTime));
        }
        _lastFocusPoint = desiredFocusPoint;

        Vector3 desiredPosition = _smoothedFocusPoint - lookDirection * distance;
        if (_cameraShake != null)
        {
            desiredPosition += desiredRotation * _cameraShake.PositionOffset;
            desiredRotation *= _cameraShake.RotationOffset;
        }

        // ─── Camera collision (prevent clipping through walls) ────────
        // SphereCast desde el focus point hacia la posición deseada de cámara;
        // si choca, empujamos desiredPosition al punto de impacto + radio.
        desiredPosition = ResolveCameraCollision(_smoothedFocusPoint, desiredPosition);

        // Apply PS1 noise manually (procedural)
        float time = Time.time * 12f;
        Vector3 posJitter = new Vector3(
            Mathf.PerlinNoise(time, 0f) - 0.5f,
            Mathf.PerlinNoise(0f, time) - 0.5f,
            Mathf.PerlinNoise(time, time) - 0.5f
        ) * 0.015f;
        Vector3 rotJitter = new Vector3(
            Mathf.PerlinNoise(time + 100f, 0f) - 0.5f,
            Mathf.PerlinNoise(0f, time + 100f) - 0.5f,
            0f
        ) * 0.12f;
        desiredPosition += desiredRotation * posJitter;
        desiredRotation *= Quaternion.Euler(rotJitter);

        transform.position = desiredPosition;
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, DampingFactor(rotationDamping, Time.deltaTime));

        if (_camera != null)
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, baseFov, DampingFactor(fovDamping, Time.deltaTime));
            _camera.nearClipPlane = nearClip;
            _camera.farClipPlane = farClip;
        }
    }

    public void RequestFovPulse(float temporaryFov, float holdSeconds = 0.25f)
    {
        // Disabled - no dynamic FOV in PS1 mode
    }

    public void RequestEventFocus(Vector3 worldPoint, float weight = 0.35f, float holdSeconds = 0.45f, float pulseFov = 60f)
    {
        // Disabled - Cinemachine handles narrative camera cuts
    }

    public void PlayLandingTilt(float impactSpeed)
    {
        // Tilts camera downwards on landing (impactSpeed drives intensity)
        _landingTiltAmount = Mathf.Clamp(impactSpeed * 0.72f, 0f, 15f);
    }

    Vector3 GetFocusPoint()
    {
        return target.position + target.rotation * focusOffset;
    }

    /// <summary>
    /// Evita que la cámara atraviese paredes/suelos: SphereCast desde el focus point
    /// (cabeza del player) hacia la posición deseada. Si hay obstáculo, empuja la
    /// cámara al punto de impacto + radio. Suavizado vía collisionDamping.
    /// </summary>
    Vector3 ResolveCameraCollision(Vector3 focusPoint, Vector3 desiredPosition)
    {
        Vector3 castDir = desiredPosition - focusPoint;
        float castDistance = castDir.magnitude;
        if (castDistance < 0.001f) return desiredPosition;
        castDir /= castDistance;

        // Excluir player/echo/pressureplate (layers 8/9/11) y triggers.
        LayerMask mask = collisionMask;
        mask &= ~(1 << 8);  // Player
        mask &= ~(1 << 9);  // Echo
        mask &= ~(1 << 11); // PressurePlate

        if (Physics.SphereCast(focusPoint, collisionRadius, castDir, out RaycastHit hit, castDistance + collisionRadius, mask, QueryTriggerInteraction.Ignore))
        {
            float safeDistance = Mathf.Max(minCollisionDistance, hit.distance - collisionRadius);
            Vector3 safePosition = focusPoint + castDir * safeDistance;
            // Suavizado: interpolar hacia la posición segura (evita popping brusco)
            return Vector3.Lerp(desiredPosition, safePosition, DampingFactor(collisionDamping, Time.deltaTime));
        }

        return desiredPosition;
    }

    static float DampingFactor(float sharpness, float deltaTime)
    {
        return 1f - Mathf.Exp(-Mathf.Max(0f, sharpness) * deltaTime);
    }

    static Vector3 ResolvePlanarForward(Vector3 primary, Vector3 fallbackForward, Vector3 fallbackRight, Vector3 up)
    {
        Vector3 planar = Vector3.ProjectOnPlane(primary, up);
        if (planar.sqrMagnitude < 0.001f)
            planar = Vector3.ProjectOnPlane(fallbackForward, up);
        if (planar.sqrMagnitude < 0.001f)
            planar = Vector3.Cross(fallbackRight, up);

        return planar.normalized;
    }
}