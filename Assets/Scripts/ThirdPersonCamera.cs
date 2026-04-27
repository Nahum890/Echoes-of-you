using UnityEngine;

/// <summary>
/// Third-person camera with smooth follow, orbital mouse control,
/// support for arbitrary player up vectors, and auto-recenter
/// when the player stops moving the mouse.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 focusOffset = new Vector3(0f, 1.4f, 0f);
    public float distance = 7.2f;
    public float movementLead = 0.55f;

    [Header("Smoothing")]
    public float followDamping = 9f;
    public float rotationDamping = 10f;
    public float fovDamping = 8f;

    [Header("Orbit")]
    public float mouseSensitivity = 1.15f;
    public float minPitch = 10f;
    public float maxPitch = 24f;
    public bool lockCursorOnStart = true;

    [Header("Auto Recenter")]
    public bool enableRecenter = true;
    public float recenterDelay = 1.2f;
    public float recenterSpeed = 2.2f;

    [Header("Composition")]
    public bool clampYaw = true;
    public float maxYawOffset = 28f;
    public Vector3 authoredForward = Vector3.forward;
    public float baseFov = 58f;

    float _pitch = 12f;
    Vector3 _smoothedFocusPoint;
    Vector3 _orbitForward = Vector3.forward;
    CameraShake _cameraShake;
    Camera _camera;

    float _noInputTimer;
    Vector3 _lastFocusPoint;
    float _pulseTargetFov;
    float _pulseUntil;

    void Start()
    {
        _cameraShake = GetComponent<CameraShake>();
        _camera = GetComponent<Camera>();

        if (target != null)
        {
            Vector3 targetUp = target.up;
            _smoothedFocusPoint = GetFocusPoint();
            _orbitForward = ResolvePlanarForward(transform.forward, target.forward, target.right, targetUp);
            _lastFocusPoint = _smoothedFocusPoint;
        }

        if (_camera != null)
            _camera.fieldOfView = baseFov;

        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetUp = target.up;
        float yawDelta = Input.GetAxis("Mouse X") * mouseSensitivity;
        float pitchDelta = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Track mouse input for auto-recenter
        bool hasMouseInput = Mathf.Abs(yawDelta) > 0.01f || Mathf.Abs(pitchDelta) > 0.01f;
        if (hasMouseInput)
        {
            _noInputTimer = 0f;
        }
        else
        {
            _noInputTimer += Time.deltaTime;
        }

        _orbitForward = Quaternion.AngleAxis(yawDelta, targetUp) * _orbitForward;
        _orbitForward = ResolvePlanarForward(_orbitForward, transform.forward, target.right, targetUp);

        _pitch = Mathf.Clamp(_pitch - pitchDelta, minPitch, maxPitch);

        // Auto-recenter: smoothly return to behind the player after delay
        if (enableRecenter && _noInputTimer >= recenterDelay)
        {
            Vector3 authoredPlanar = ResolvePlanarForward(target.rotation * authoredForward, target.forward, target.right, targetUp);
            Vector3 behindPlayer = authoredPlanar.sqrMagnitude > 0.001f
                ? authoredPlanar
                : ResolvePlanarForward(target.forward, transform.forward, target.right, targetUp);
            float recenterBlend = DampingFactor(recenterSpeed, Time.deltaTime);
            _orbitForward = Vector3.Slerp(_orbitForward, behindPlayer, recenterBlend).normalized;
            _orbitForward = ResolvePlanarForward(_orbitForward, transform.forward, target.right, targetUp);
        }

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
        Vector3 lookDirection = Quaternion.AngleAxis(_pitch, orbitRight) * _orbitForward;
        lookDirection.Normalize();

        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection, targetUp);
        Vector3 desiredFocusPoint = GetFocusPoint();
        Vector3 focusVelocity = (desiredFocusPoint - _lastFocusPoint) / Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 planarLead = Vector3.ProjectOnPlane(focusVelocity, targetUp);
        if (planarLead.sqrMagnitude > 0.001f)
            desiredFocusPoint += Vector3.ClampMagnitude(planarLead, 2f) * movementLead * 0.08f;

        _smoothedFocusPoint = Vector3.Lerp(_smoothedFocusPoint, desiredFocusPoint, DampingFactor(followDamping, Time.deltaTime));
        _lastFocusPoint = desiredFocusPoint;

        Vector3 desiredPosition = _smoothedFocusPoint - lookDirection * distance;
        if (_cameraShake != null)
        {
            desiredPosition += desiredRotation * _cameraShake.PositionOffset;
            desiredRotation *= _cameraShake.RotationOffset;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, DampingFactor(followDamping, Time.deltaTime));
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, DampingFactor(rotationDamping, Time.deltaTime));

        if (_camera != null)
        {
            float targetFov = Time.unscaledTime < _pulseUntil ? _pulseTargetFov : baseFov;
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFov, DampingFactor(fovDamping, Time.deltaTime));
        }
    }

    public void RequestFovPulse(float temporaryFov, float holdSeconds = 0.25f)
    {
        _pulseTargetFov = temporaryFov;
        _pulseUntil = Time.unscaledTime + Mathf.Max(0.05f, holdSeconds);
    }

    Vector3 GetFocusPoint()
    {
        return target.position + target.rotation * focusOffset;
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
