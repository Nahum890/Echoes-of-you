using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grabación por mantener R: hasta 6 s o al soltar. Genera un eco que repite el bucle.
/// Q limpia todos los ecos.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class EchoRecorder : MonoBehaviour
{
    [Header("Prefab y límites")]
    [SerializeField] GameObject echoPrefab;
    [SerializeField] Transform echoSpawnRoot;
    [SerializeField] int maxEchoes = 2;
    [SerializeField] float maxRecordSeconds = 6f;
    [SerializeField] float minRecordSeconds = 0.1f;

    [Header("Proyeccion")]
    [SerializeField] KeyCode projectionKey = KeyCode.F;
    [SerializeField] float projectionMoveSpeed = 7.5f;
    [SerializeField] float projectionSprintMultiplier = 1.45f;
    [SerializeField] float projectionHeightOffset = 0.05f;
    [SerializeField] float maxProjectionStepUp = 0.4f;

    [Header("HUD (opcional)")]
    [SerializeField] GameHUD hud;

    readonly List<RecordFrame> _frames = new List<RecordFrame>();
    readonly List<EchoPlayback> _echoes = new List<EchoPlayback>();

    PlayerController _playerController;
    Animator _anim;
    bool _recording;
    bool _projectionRecording;
    float _recordStartTime;
    float _lastRecordDuration;
    GameObject _projectionPilot;
    Animator _projectionAnim;
    Vector3 _projectionVelocity;
    AudioClip _micClip;
    string _micDevice;
    int _micFrequency = 48000;
    int _micLastPosition;
    bool _micReady;
    float _micStartRealtime;

    public bool IsRecording => _recording;
    public bool IsProjectionRecording => _recording && _projectionRecording;
    public int EchoCount => _echoes.Count;
    public int MaxEchoes => maxEchoes;
    public float MaxRecordSeconds => maxRecordSeconds;
    public float RecordingElapsed => _recording ? Mathf.Min(Time.time - _recordStartTime, maxRecordSeconds) : 0f;
    public float LastClipDuration => _lastRecordDuration;
    public bool HasEchoes => _echoes.Count > 0;

    /// <summary>Fired when an echo is created. Arg = current echo count.</summary>
    public event Action<int> EchoCreated;
    /// <summary>Fired when all echoes are cleared.</summary>
    public event Action EchoesCleared;
    /// <summary>Fired when recording starts.</summary>
    public event Action RecordingStarted;
    /// <summary>Fired when recording stops (even if too short).</summary>
    public event Action<bool> RecordingStopped;

    void Awake()
    {
        if (hud == null)
            hud = UnityEngine.Object.FindAnyObjectByType<GameHUD>();
        if (echoSpawnRoot == null)
            echoSpawnRoot = transform;
        _playerController = GetComponent<PlayerController>();
        _anim = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        ForceUnlockPlayer();
        RefreshHud();
    }

    void Update()
    {
        bool projectionHold = Input.GetKey(projectionKey);
        bool hold = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.R);

        if (projectionHold && !_recording)
            StartRecording(true);
        else if (hold && !_recording)
            StartRecording(false);

        if (_recording && ((_projectionRecording && !projectionHold) || (!_projectionRecording && !hold)))
            StopRecordingAndSpawn();

        if (_recording && Time.time - _recordStartTime >= maxRecordSeconds)
            StopRecordingAndSpawn();

        if (_projectionRecording)
            UpdateProjectionPilot();

        if (_recording)
            UpdateVoiceCaptureStatus();

        UpdateProjectionAnimator();

        if (_anim != null && _anim.runtimeAnimatorController != null)
            _anim.SetBool("IsRecording", _recording && !_projectionRecording);

        RefreshHud();
    }

    void FixedUpdate()
    {
        if (!_recording)
            return;

        float elapsed = Time.time - _recordStartTime;
        Transform source = _projectionRecording && _projectionPilot != null ? _projectionPilot.transform : transform;
        _frames.Add(new RecordFrame(elapsed, source.position, source.rotation));

        if (_frames.Count == 1)
        {
            // Duplicar primer fotograma en t=0 para interpolación estable al inicio
            _frames.Insert(0, new RecordFrame(0f, _frames[0].position, _frames[0].rotation));
        }
    }

    void StartRecording(bool projectionMode)
    {
        _recording = true;
        _projectionRecording = projectionMode;
        _recordStartTime = Time.time;
        _frames.Clear();

        StartVoiceCapture();

        if (_projectionRecording)
            CreateProjectionPilot();
        if (_anim == null) _anim = GetComponentInChildren<Animator>();
        if (_anim != null && _anim.runtimeAnimatorController != null && !_projectionRecording)
        {
            _anim.SetBool("IsRecording", true);
            SetAnimatorTriggerIfExists("RecordStart");
        }
        RecordingStarted?.Invoke();
        GameFeelController.Instance?.PlayRecordStart(transform.position, transform.up);
        GameStateController.Instance?.SetRecording(true, transform.position, transform.up);
        hud?.SetPrompt(_projectionRecording ? "Proyeccion activa" : "Grabando eco", 1.2f);
        RefreshHud();
    }

    void StopRecordingAndSpawn()
    {
        if (!_recording)
            return;

        _recording = false;
        bool wasProjection = _projectionRecording;
        _projectionRecording = false;
        float elapsed = Time.time - _recordStartTime;
        _lastRecordDuration = elapsed;
        GameStateController.Instance?.SetRecording(false, transform.position, transform.up);
        _playerController?.SetInputLocked(false);

        GameFeelController.Instance?.PlayRecordStop(transform.position);
        SetAnimatorTriggerIfExists("RecordStop");

        AudioClip voiceClip = StopVoiceCapture(elapsed);

        if (_frames.Count < 2)
        {
            _frames.Clear();
            DestroyProjectionPilot();
            RecordingStopped?.Invoke(false);
            hud?.ShowToast("Grabacion muy corta", new Color(1f, 0.43f, 0.43f, 1f), 1.1f);
            hud?.SetEchoState("Error");
            GameFeelController.Instance?.PlaySoftError(transform.position);
            RefreshHud();
            return;
        }

        if (echoPrefab == null)
        {
            Debug.LogError("EchoRecorder: asigna echoPrefab.");
            _frames.Clear();
            DestroyProjectionPilot();
            hud?.ShowToast("Falta el prefab del eco", new Color(1f, 0.43f, 0.43f, 1f), 1.2f);
            return;
        }

        TrimEchoesIfNeeded();

        Vector3 spawnPosition = _frames.Count > 0 ? _frames[0].position : echoSpawnRoot.position;
        Quaternion spawnRotation = _frames.Count > 0 ? _frames[0].rotation : echoSpawnRoot.rotation;
        GameObject instance = Instantiate(echoPrefab, spawnPosition, spawnRotation);
        instance.tag = "Echo";
        var playback = instance.GetComponent<EchoPlayback>();
        if (playback == null)
            playback = instance.AddComponent<EchoPlayback>();

        float duration = Mathf.Max(elapsed, 0.05f);
        playback.BeginPlayback(_frames, duration, voiceClip);
        _echoes.Add(playback);

        _frames.Clear();
        DestroyProjectionPilot();

        RecordingStopped?.Invoke(true);
        EchoCreated?.Invoke(_echoes.Count);
        GameProgress.RecordEchoCreated();
        hud?.ShowToast(wasProjection ? "Proyeccion convertida en eco" : "Eco creado", new Color(0.48f, 0.94f, 0.78f, 1f), 1.25f);
        GameFeelController.Instance?.PlayEchoSpawn(instance.transform.position);

        RefreshHud();
    }

    void TrimEchoesIfNeeded()
    {
        while (_echoes.Count >= maxEchoes)
        {
            EchoPlayback oldest = _echoes[0];
            _echoes.RemoveAt(0);
            if (oldest != null)
                oldest.FadeOutAndDestroy(0.65f);
        }
    }

    public void ClearAllEchoes(bool showFeedback = true)
    {
        _recording = false;
        _projectionRecording = false;
        _frames.Clear();
        _lastRecordDuration = 0f;
        _playerController?.SetInputLocked(false);
        DestroyProjectionPilot();
        GameStateController.Instance?.SetRecording(false, transform.position, transform.up);

        // Terminate microphone recording if it's currently running
        StopVoiceCapture(_lastRecordDuration);
        _micClip = null;

        for (int i = 0; i < _echoes.Count; i++)
        {
            if (_echoes[i] != null)
                _echoes[i].FadeOutAndDestroy(0.5f);
        }

        _echoes.Clear();
        EchoesCleared?.Invoke();
        if (showFeedback)
            hud?.ShowToast("Ecos limpiados", new Color(0.48f, 0.94f, 0.78f, 1f), 1f);
        RefreshHud();
    }

    void RefreshHud()
    {
        if (hud == null)
            return;

        hud.SetEchoCount(_echoes.Count, maxEchoes);
        hud.SetRecording(_recording, _recording ? RecordingElapsed / Mathf.Max(0.01f, maxRecordSeconds) : 0f);
        hud.SetEchoState(_recording ? (_projectionRecording ? "Proyectando" : "Grabando") : (_echoes.Count > 0 ? "Reproduciendo" : "Listo"));
    }

    void StartVoiceCapture()
    {
        _micClip = null;
        _micLastPosition = 0;
        _micReady = false;
        _micStartRealtime = Time.realtimeSinceStartup;
        _micDevice = Microphone.devices != null && Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        if (string.IsNullOrEmpty(_micDevice))
        {
            Debug.Log("EchoRecorder: no hay microfono disponible — el eco se creara sin voz.");
            return;
        }

        Microphone.GetDeviceCaps(_micDevice, out int minFrequency, out int maxFrequency);
        if (maxFrequency > 0)
        {
            int lowerBound = minFrequency > 0 ? minFrequency : 8000;
            _micFrequency = maxFrequency >= lowerBound ? Mathf.Clamp(48000, lowerBound, maxFrequency) : maxFrequency;
        }
        else
            _micFrequency = 48000;

        int captureSeconds = Mathf.Max(1, Mathf.CeilToInt(maxRecordSeconds) + 1);
        _micClip = Microphone.Start(_micDevice, false, captureSeconds, _micFrequency);
        if (_micClip == null)
        {
            Debug.Log($"EchoRecorder: no se pudo iniciar el microfono '{_micDevice}' — eco sin voz.");
            return;
        }

        Debug.Log($"EchoRecorder: capturando voz exacta del eco con '{_micDevice}' a {_micFrequency} Hz.");
    }

    void UpdateVoiceCaptureStatus()
    {
        if (_micClip == null || string.IsNullOrEmpty(_micDevice))
            return;

        if (!Microphone.IsRecording(_micDevice))
            return;

        int position = Microphone.GetPosition(_micDevice);
        if (position > 0)
        {
            _micLastPosition = position;
            _micReady = true;
        }
    }

    AudioClip StopVoiceCapture(float recordedSeconds)
    {
        if (_micClip == null || string.IsNullOrEmpty(_micDevice))
            return null;

        int microphonePosition = Microphone.IsRecording(_micDevice) ? Microphone.GetPosition(_micDevice) : _micLastPosition;
        if (microphonePosition > 0)
            _micLastPosition = microphonePosition;
        Microphone.End(_micDevice);

        int expectedSamples = Mathf.Clamp(
            Mathf.CeilToInt(Mathf.Max(0.01f, recordedSeconds) * _micClip.frequency),
            1,
            _micClip.samples);
        int sampleCount = _micLastPosition > 0 ? Mathf.Min(_micLastPosition, expectedSamples) : 0;
        int channels = Mathf.Max(1, _micClip.channels);

        if (!_micReady || sampleCount <= 0)
        {
            Debug.Log($"EchoRecorder: el microfono no entrego muestras validas — eco sin voz. Tiempo activo: {Time.realtimeSinceStartup - _micStartRealtime:0.00}s.");
            _micClip = null;
            _micDevice = null;
            return null;
        }

        float[] samples = new float[sampleCount * channels];
        _micClip.GetData(samples, 0);
        NormalizeVoiceSamples(samples);

        AudioClip voiceClip = AudioClip.Create("EchoVoice_ExactMicReplay", sampleCount, channels, _micClip.frequency, false);
        voiceClip.SetData(samples, 0);
        Debug.Log($"EchoRecorder: voz del eco capturada ({sampleCount / (float)_micClip.frequency:0.00}s, {channels} canales, {_micClip.frequency} Hz).");
        _micClip = null;
        _micDevice = null;
        return voiceClip;
    }

    static void NormalizeVoiceSamples(float[] samples)
    {
        if (samples == null || samples.Length == 0)
            return;

        float peak = 0f;
        for (int i = 0; i < samples.Length; i++)
            peak = Mathf.Max(peak, Mathf.Abs(samples[i]));

        if (peak < 0.0001f)
            return;

        float targetPeak = 0.82f;
        float gain = Mathf.Min(4f, targetPeak / peak);
        if (gain <= 1.02f)
            return;

        for (int i = 0; i < samples.Length; i++)
            samples[i] = Mathf.Clamp(samples[i] * gain, -1f, 1f);
    }

    void CreateProjectionPilot()
    {
        _playerController?.SetInputLocked(true);

        if (_projectionPilot != null)
            Destroy(_projectionPilot);

        _projectionPilot = new GameObject("EchoProjectionPilot");
        TryAssignProjectionTag(_projectionPilot);
        _projectionPilot.transform.SetPositionAndRotation(transform.position + Vector3.up * projectionHeightOffset, transform.rotation);
        EnsureProjectionPlateProbe();
        _projectionAnim = null;

        PlayerCharacterVisualSetup.EnsureOn(transform);
        Transform playerVisual = transform.Find("PlayerVisual");
        if (playerVisual != null)
        {
            GameObject visualClone = Instantiate(playerVisual.gameObject, _projectionPilot.transform);
            visualClone.name = "ProjectionVisual";
            visualClone.transform.localPosition = Vector3.zero;
            visualClone.transform.localRotation = Quaternion.identity;
            visualClone.transform.localScale = Vector3.one;

            foreach (PlayerProceduralAnimator procedural in visualClone.GetComponentsInChildren<PlayerProceduralAnimator>(true))
                Destroy(procedural);

            foreach (Collider col in visualClone.GetComponentsInChildren<Collider>(true))
                Destroy(col);

            ApplyProjectionGhostMaterials(visualClone);
            _projectionAnim = visualClone.GetComponentInChildren<Animator>(true);
            if (_projectionAnim != null)
            {
                _projectionAnim.applyRootMotion = false;
                EchoesAnimatorParams.SetBoolIfExists(_projectionAnim, "IsRecording", false);
                EchoesAnimatorParams.SetGrounded(_projectionAnim, true);
            }
        }
        else
        {
            GameObject missingVisual = new GameObject("ProjectionVisualMissingModel");
            missingVisual.transform.SetParent(_projectionPilot.transform, false);
        }

        _projectionVelocity = Vector3.zero;
    }

    static void TryAssignProjectionTag(GameObject target)
    {
        if (target == null)
            return;

        try
        {
            target.tag = "EchoProjection";
        }
        catch (UnityException)
        {
            Debug.LogWarning("EchoRecorder: crea el tag EchoProjection (Echoes → Production → Ensure Project Tags).");
        }
    }

    void EnsureProjectionPlateProbe()
    {
        if (_projectionPilot == null)
            return;

        CapsuleCollider probe = _projectionPilot.GetComponent<CapsuleCollider>();
        if (probe == null)
            probe = _projectionPilot.AddComponent<CapsuleCollider>();

        probe.isTrigger = true;
        probe.radius = 0.42f;
        probe.height = 1.55f;
        probe.center = new Vector3(0f, 0.85f, 0f);
    }

    static void ApplyProjectionGhostMaterials(GameObject root)
    {
        Color ghostColor = new Color(0.72f, 0.88f, 0.96f, 0.38f);
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rendererRef = renderers[i];
            if (rendererRef == null)
                continue;

            Material[] mats = rendererRef.materials;
            for (int m = 0; m < mats.Length; m++)
            {
                Material material = new Material(mats[m]);
                if (material.HasProperty("_Color"))
                    material.color = ghostColor;
                material.SetFloat("_Surface", 1f);
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                if (material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", new Color(0.35f, 0.82f, 1f) * 0.6f);
                }

                mats[m] = material;
            }

            rendererRef.materials = mats;
        }
    }

    void UpdateProjectionAnimator()
    {
        if (!_projectionRecording || _projectionAnim == null || _projectionAnim.runtimeAnimatorController == null)
            return;

        Vector3 localVelocity = _projectionPilot.transform.InverseTransformDirection(_projectionVelocity);
        EchoesAnimatorParams.SetLocomotion(_projectionAnim, _projectionVelocity.magnitude, localVelocity, true);
        EchoesAnimatorParams.SetBoolIfExists(_projectionAnim, "IsRecording", false);
    }

    void UpdateProjectionPilot()
    {
        if (_projectionPilot == null)
            return;

        _playerController?.SetInputLocked(true);

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v);
        if (input.sqrMagnitude > 1f)
            input.Normalize();

        Transform cam = Camera.main != null ? Camera.main.transform : transform;
        Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;
        forward.Normalize();

        Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up);
        if (right.sqrMagnitude < 0.001f)
            right = Vector3.right;
        right.Normalize();

        Vector3 desired = forward * input.z + right * input.x;
        float speed = projectionMoveSpeed * (Input.GetKey(KeyCode.LeftShift) ? projectionSprintMultiplier : 1f);
        _projectionVelocity = Vector3.MoveTowards(_projectionVelocity, desired * speed, 35f * Time.deltaTime);
        _projectionPilot.transform.position += _projectionVelocity * Time.deltaTime;
        SnapProjectionToWalkHeight();

        if (_projectionVelocity.sqrMagnitude > 0.05f)
            _projectionPilot.transform.rotation = Quaternion.Slerp(_projectionPilot.transform.rotation, Quaternion.LookRotation(_projectionVelocity.normalized, Vector3.up), 18f * Time.deltaTime);
    }

    void SnapProjectionToWalkHeight()
    {
        if (_projectionPilot == null)
            return;

        Vector3 pos = _projectionPilot.transform.position;
        float currentY = pos.y;
        Vector3 probeOrigin = pos + Vector3.up * 6f;

        if (!Physics.Raycast(probeOrigin, Vector3.down, out RaycastHit hit, 14f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return;

        if (hit.collider.GetComponentInParent<EchoKineticBody>() != null)
            return;

        float targetY = hit.point.y + projectionHeightOffset;
        if (targetY > currentY + maxProjectionStepUp)
            targetY = currentY;

        pos.y = targetY;
        _projectionPilot.transform.position = pos;
    }

    void DestroyProjectionPilot()
    {
        if (_projectionPilot != null)
            Destroy(_projectionPilot);
        _projectionPilot = null;
        _projectionVelocity = Vector3.zero;
    }

    public void ForceUnlockPlayer()
    {
        _recording = false;
        _projectionRecording = false;
        _playerController?.SetInputLocked(false);
        DestroyProjectionPilot();
    }

    void SetAnimatorTriggerIfExists(string parameterName)
    {
        if (_anim == null || _anim.runtimeAnimatorController == null)
            return;

        AnimatorControllerParameter[] parameters = _anim.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == AnimatorControllerParameterType.Trigger && parameters[i].name == parameterName)
            {
                _anim.SetTrigger(parameterName);
                return;
            }
        }
    }
}
