using UnityEngine;

public class SimpleFollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1.8f, 0f);

    [Header("Orbit")]
    public float distance = 5.5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float zoomSpeed = 4f;

    [Header("Mouse / Angles")]
    public float yaw;
    public float pitch = 18f;
    public float minPitch = -30f;
    public float maxPitch = 75f;
    public float rotationSpeed = 120f;
    public float mouseSensitivity = 1f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.12f;
    public float rotationSmoothTime = 0.08f;

    [Header("Collision")]
    public LayerMask obstacleMask = -1;
    public float sphereCastRadius = 0.3f;

    float _armLength;
    float _armVelocity;
    bool _frozen;
    Vector3 _focusPoint;
    float _focusUntil;
    public bool Frozen
    {
        get => _frozen;
        set => _frozen = value;
    }

    /// <summary>
    /// Relee la sensibilidad guardada. El menú de ajustes llamaba a esto por
    /// <c>SendMessage("ApplySavedSensitivity")</c> desde siempre, pero el método
    /// no existía en ninguna clase y además el menú escribía en otra clave
    /// (<c>CameraSensitivity</c>) distinta de la que se leía aquí: el slider de
    /// sensibilidad no movía nada. Ahora ambos lados pasan por EchoesSettings.
    /// </summary>
    public void ApplySavedSensitivity()
    {
        mouseSensitivity = EchoesSettings.Sensitivity;
    }

    void Start()
    {
        if (EchoesCameraAuthority.IsCinemachineActiveInScene())
        {
            enabled = false;
            return;
        }

        ApplySavedSensitivity();
        _armLength = distance;
        if (target != null) Snap();
    }

    void LateUpdate()
    {
        if (EchoesCameraAuthority.IsCinemachineActiveInScene())
        {
            enabled = false;
            return;
        }

        if (target == null) return;
        if (!Application.isPlaying) return;
        if (_frozen) return;

        if (Cursor.lockState == CursorLockMode.Locked && Time.timeScale > 0f)
        {
            // Legacy mouse axes report displacement since the previous frame.
            // Preserve the authored sensitivity at its original 60 Hz baseline.
            float degreesPerUnit = rotationSpeed * mouseSensitivity / 60f;
            yaw = Mathf.Repeat(yaw + Input.GetAxisRaw("Mouse X") * degreesPerUnit + 180f, 360f) - 180f;
            pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * degreesPerUnit, minPitch, maxPitch);
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, minDistance, maxDistance);
        }

        Vector3 pivot = target.position + targetOffset;
        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 arm = orbit * Vector3.back;
        float allowed = distance;
        if (obstacleMask != 0 && Physics.SphereCast(pivot, sphereCastRadius, arm, out RaycastHit hit,
                distance, obstacleMask, QueryTriggerInteraction.Ignore))
            allowed = Mathf.Min(distance, Mathf.Max(0.05f, hit.distance - 0.02f));

        if (allowed <= _armLength)
        {
            _armLength = allowed;
            _armVelocity = 0f;
        }
        else
            _armLength = Mathf.SmoothDamp(_armLength, allowed, ref _armVelocity,
                positionSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

        transform.position = pivot + arm * _armLength;
        // Brief puzzle cues may turn the view slightly without moving the pivot.
        if (Time.unscaledTime < _focusUntil)
        {
            Vector3 direction = _focusPoint - transform.position;
            if (direction.sqrMagnitude > 0.01f)
                orbit = Quaternion.RotateTowards(orbit, Quaternion.LookRotation(direction), 5f);
        }
        transform.rotation = orbit;
    }
    /// <summary>
    /// Incluye un punto del mundo en el encuadre durante unos segundos.
    /// Lo usan los beats de puzzle para enseñar el objetivo sin quitarle
    /// el control de la camara al jugador.
    /// </summary>
    public void FocusOn(Vector3 worldPos, float duration)
    {
        _focusPoint = worldPos;
        _focusUntil = Time.unscaledTime + Mathf.Max(0f, duration);
    }

    void Snap()
    {
        Vector3 tp = target.position + targetOffset;
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = tp - rot * Vector3.forward * distance;
        transform.LookAt(tp);
    }
}