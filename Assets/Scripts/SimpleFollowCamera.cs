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
    // Estaba en 0.45: con un Eco activo la camara se iba casi a medio camino
    // entre los dos y el jugador dejaba de estar centrado, que se percibe como
    // \"la camara no me sigue\". 0.18 deja ver al Eco sin descentrar al jugador.
    [Range(0f, 1f)] public float framingLookBias = 0.18f;
    public float framingSmoothTime = 0.35f;

    // Distancia con la que arranca el nivel. Sirve de referencia para que la
    // rueda del raton siga funcionando cuando el encuadre automatico esta
    // mandando: ver el calculo de shotDistance en LateUpdate.
    float _baseDistance = -1f;

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
        _cam = GetComponent<Camera>();
        _framedDistance = distance;
        _baseDistance = Mathf.Max(0.01f, distance);
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

            // El encuadre propone una distancia, pero la rueda del raton tiene
            // que seguir mandando. Antes esto era shotDistance = _framedDistance
            // a secas: con un Eco en pantalla el encuadre ganaba siempre y el
            // zoom del jugador no hacia absolutamente nada.
            //
            // Ahora el zoom se aplica como FACTOR sobre lo que pide el encuadre:
            // sin tocar la rueda el factor es 1 y se ve el encuadre completo;
            // al acercar, se acerca de forma proporcional.
            if (_baseDistance <= 0f) _baseDistance = Mathf.Max(0.01f, distance);
            float zoomFactor = distance / _baseDistance;
            shotDistance = Mathf.Clamp(_framedDistance * zoomFactor, minDistance, framingMaxDistance);

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