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

    [Header("Encuadre de puzzle")]
    [Tooltip("Al soltar un Eco la camara retrocede lo justo para que jugador y Eco quepan en pantalla.")]
    public bool framePuzzleTargets = true;
    [Tooltip("Distancia maxima a la que puede retroceder para encuadrar. Solo se aplica con Ecos activos.")]
    public float framingMaxDistance = 16f;
    [Tooltip("Cuanto se desplaza el punto de mira hacia el centro entre jugador y Eco. 0 = siempre al jugador.")]
    [Range(0f, 1f)] public float framingLookBias = 0.45f;
    public float framingSmoothTime = 0.35f;

    Vector3 _velocity = Vector3.zero;
    Vector2 _smoothMouse;
    bool _frozen;

    Camera _cam;
    EchoPlayback[] _echoes = System.Array.Empty<EchoPlayback>();
    float _nextEchoScan;
    float _framedDistance;
    float _framedDistanceVel;
    Vector3 _focusPoint;
    float _focusUntil;
    float _focusWeight;

    public bool Frozen
    {
        get => _frozen;
        set => _frozen = value;
    }

    void Start()
    {
        if (EchoesCameraAuthority.IsCinemachineActiveInScene())
        {
            enabled = false;
            return;
        }

        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        _cam = GetComponent<Camera>();
        _framedDistance = distance;
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

        if (Cursor.lockState == CursorLockMode.Locked || true)
        {
            mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
            float rawX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float rawY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            float deadZone = 0.05f;
            if (System.Math.Abs(rawX) < deadZone) rawX = 0f;
            if (System.Math.Abs(rawY) < deadZone) rawY = 0f;
            yaw   += rawX * rotationSpeed * Time.unscaledDeltaTime;
            pitch -= rawY * rotationSpeed * Time.unscaledDeltaTime;
        }
        yaw = Mathf.Repeat(yaw + 180f, 360f) - 180f;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        Vector3 tp = target.position + targetOffset;

        // --- encuadre de puzzle: jugador + Eco(s) + punto de interes ---
        // Solo puede ALEJAR la camara, nunca acercarla por debajo del zoom
        // que haya elegido el jugador.
        float shotDistance = distance;
        if (framePuzzleTargets)
        {
            RefreshEchoes();

            Vector3 centroid = tp;
            float spread = 0f;
            int count = 1;

            for (int i = 0; i < _echoes.Length; i++)
            {
                var e = _echoes[i];
                if (e == null || !e.isActiveAndEnabled) continue;
                centroid += e.transform.position + targetOffset;
                count++;
            }

            bool focusing = Time.unscaledTime < _focusUntil;
            if (focusing) { centroid += _focusPoint; count++; }

            if (count > 1)
            {
                centroid /= count;
                spread = Vector3.Distance(tp, centroid);
                for (int i = 0; i < _echoes.Length; i++)
                {
                    var e = _echoes[i];
                    if (e == null || !e.isActiveAndEnabled) continue;
                    spread = Mathf.Max(spread, Vector3.Distance(e.transform.position + targetOffset, centroid));
                }
                if (focusing) spread = Mathf.Max(spread, Vector3.Distance(_focusPoint, centroid));
            }

            float wanted = distance;
            if (spread > 0.01f && _cam != null)
            {
                float halfV = _cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
                float halfH = Mathf.Atan(Mathf.Tan(halfV) * Mathf.Max(0.1f, _cam.aspect));
                // margen para que nadie quede pegado al borde de la pantalla
                wanted = Mathf.Max(distance, spread / Mathf.Tan(halfH) + 2.5f);
            }
            wanted = Mathf.Min(wanted, framingMaxDistance);

            _framedDistance = Mathf.SmoothDamp(_framedDistance, wanted, ref _framedDistanceVel, framingSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            shotDistance = _framedDistance;

            float targetWeight = count > 1 ? framingLookBias : 0f;
            _focusWeight = Mathf.MoveTowards(_focusWeight, targetWeight, Time.unscaledDeltaTime / Mathf.Max(0.01f, framingSmoothTime));
            tp = Vector3.Lerp(tp, centroid, _focusWeight);
        }

        Quaternion orbitRot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPos = tp - orbitRot * Vector3.forward * shotDistance;

        if (obstacleMask != 0)
        {
            Vector3 dirToCam = (desiredPos - tp).normalized;
            float maxCheck = shotDistance + sphereCastRadius;
            if (Physics.SphereCast(tp, sphereCastRadius, dirToCam, out RaycastHit hit, maxCheck, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                float safeDist = Mathf.Max(0.5f, hit.distance - sphereCastRadius);
                safeDist = Mathf.Min(safeDist, shotDistance);
                desiredPos = tp + dirToCam * safeDist;
            }
        }

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _velocity, positionSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

        Quaternion lookAt = Quaternion.LookRotation(tp - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, Time.unscaledDeltaTime / rotationSmoothTime);
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

    void RefreshEchoes()
    {
        if (Time.unscaledTime < _nextEchoScan) return;
        _nextEchoScan = Time.unscaledTime + 0.25f;
        _echoes = FindObjectsByType<EchoPlayback>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    void Snap()
    {
        Vector3 tp = target.TransformPoint(targetOffset);
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = tp - rot * Vector3.forward * distance;
        transform.LookAt(tp);
    }
}