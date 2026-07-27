using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grabación por mantener F/R: hasta 12s default / 20s max o al soltar. Genera un eco que repite el bucle.
/// Tecla Q ejecuta SoftReset de posición y slots de eco sin destruir el progreso del nivel.
/// Retardo de inicio de reproducción: 0.0s (instantáneo).
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class EchoRecorder : MonoBehaviour
{
    [Header("Prefab y límites")]
    public GameObject echoPrefab;
    [SerializeField] Transform echoSpawnRoot;
    [SerializeField] int maxEchoes = 3;
    [SerializeField] float maxRecordSeconds = 12f;
#pragma warning disable CS0414
    [SerializeField] float minRecordSeconds = 0.1f;
#pragma warning restore CS0414

    [Header("Future & Locking")]
    bool recordFuture = false;
    readonly List<RecordFrame> _pendingFutureEcho = new List<RecordFrame>();
    bool[] _slotLocked;

    [Header("HUD (opcional)")]
    [SerializeField] GameHUD hud;

    readonly List<RecordFrame> _frames = new List<RecordFrame>();
    readonly List<EchoPlayback> _echoes = new List<EchoPlayback>();

    PlayerController _playerController;
    Animator _anim;
    bool _recording;
    float _recordStartTime;
    float _lastRecordDuration;
    AudioClip _micClip;
    string _micDevice;
    int _micFrequency = 48000;
    int _micLastPosition;
    bool _micReady;
    float _micStartRealtime;
    float _nextRecordTime;
    float _recordInterval = 1f / 30f; // 30Hz recording

    // Estado Recording (ECHO_GRAMMAR Tabla 8.1): rim light echo-cyan #4FC3E8 solo en el jugador.
    static readonly Color RimCyan = new Color(0.31f, 0.765f, 0.91f, 1f);
    Light _recordingRimLight;
    MaterialPropertyBlock _rimBlock;
    readonly List<Renderer> _rimRenderers = new List<Renderer>();

    public bool IsRecording => _recording;
    public int EchoCount => _echoes.Count;
    public int MaxEchoes => maxEchoes;
    public float MaxRecordSeconds => maxRecordSeconds;
    public float RecordingElapsed => _recording ? Mathf.Min(Time.time - _recordStartTime, maxRecordSeconds) : 0f;
    public float LastClipDuration => _lastRecordDuration;
    public bool HasEchoes => _echoes.Count > 0;

    /// <summary>
    /// Configures the echo system mode from a LevelBlueprint.
    /// </summary>
    public void SetMode(EchoPlaybackMode mode, bool future, float degradation, bool lockSlots, int[] lockedIndices)
    {
        recordFuture = future;
        _slotLocked = new bool[maxEchoes];
        if (lockSlots && lockedIndices != null)
        {
            foreach (var i in lockedIndices)
                if (i >= 0 && i < maxEchoes)
                    _slotLocked[i] = true;
        }
    }

    public void LockSlot(int idx)
    {
        if (_slotLocked != null && idx >= 0 && idx < _slotLocked.Length)
            _slotLocked[idx] = true;
    }

    public void UnlockSlot(int idx)
    {
        if (_slotLocked != null && idx >= 0 && idx < _slotLocked.Length)
            _slotLocked[idx] = false;
    }

    /// <summary>
    /// Enables mirror recording mode: the player must replicate the imposed echo.
    /// </summary>
    public void EnableMirrorMode(EchoRecordingData data)
    {
        // Mirror mode: store reference data for sync comparison
        // The actual sync feedback is handled by EchoPlayback in Inversion/Mirror mode
        Debug.Log($"EchoRecorder: Mirror mode enabled with {data.frames.Count} reference frames.");
    }

    /// <summary>
    /// Triggers playback of a previously recorded future echo.
    /// </summary>
    public void TriggerFutureEcho()
    {
        if (_pendingFutureEcho.Count == 0) return;
        if (echoPrefab == null) return;

        var obj = Instantiate(echoPrefab, _pendingFutureEcho[0].position, _pendingFutureEcho[0].rotation);
        obj.layer = LayerMask.NameToLayer("Echo"); // Layer 9
        var playback = obj.GetComponent<EchoPlayback>();
        if (playback == null) playback = obj.AddComponent<EchoPlayback>();

        float duration = Mathf.Max(_pendingFutureEcho[_pendingFutureEcho.Count - 1].time, 0.05f);
        playback.BeginPlayback(_pendingFutureEcho, duration, null, EchoPlaybackMode.Future, 0f);
        _echoes.Add(playback);
        EchoCreated?.Invoke(_echoes.Count);

        _pendingFutureEcho.Clear();
        RefreshHud();
    }

    public static EchoRecorder Instance { get; private set; }

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

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
        // Echo system disabled for this level
        if (maxEchoes <= 0) return;

        bool hold = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.R);

        if (hold && !_recording)
            StartRecording();

        if (_recording && !hold)
            StopRecordingAndSpawn();

        if (_recording && Time.time - _recordStartTime >= maxRecordSeconds)
            StopRecordingAndSpawn();

        if (_recording)
            UpdateVoiceCaptureStatus();

        if (_anim != null && _anim.runtimeAnimatorController != null)
            _anim.SetBool("IsRecording", _recording);

        RefreshHud();
    }

    void FixedUpdate()
    {
        if (!_recording)
            return;

        if (Time.time >= _nextRecordTime)
        {
            float elapsed = Time.time - _recordStartTime;
            _frames.Add(new RecordFrame(elapsed, transform.position, transform.rotation));
            _nextRecordTime += _recordInterval;
        }
    }

    void StartRecording()
    {
        _recording = true;
        _recordStartTime = Time.time;
        _frames.Clear();
        _nextRecordTime = Time.time;

        StartVoiceCapture();

        if (_anim == null) _anim = GetComponentInChildren<Animator>();
        if (_anim != null && _anim.runtimeAnimatorController != null)
        {
            _anim.SetBool("IsRecording", true);
            SetAnimatorTriggerIfExists("RecordStart");
        }
        EnableRecordingRim();
        RecordingStarted?.Invoke();
        GameFeelController.Instance?.PlayRecordStart(transform.position, transform.up);
        GameStateController.Instance?.SetRecording(true, transform.position, transform.up);
        hud?.SetPrompt("Grabando eco", 1.2f);
        RefreshHud();
    }

    void StopRecordingAndSpawn()
    {
        if (!_recording)
            return;

        _recording = false;
        DisableRecordingRim();
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
            RecordingStopped?.Invoke(false);
            hud?.ShowToast("Grabacion muy corta", new Color(1f, 0.43f, 0.43f, 1f), 1.1f);
            hud?.SetEchoState("Error");
            GameFeelController.Instance?.PlaySoftError(transform.position);
            RefreshHud();
            return;
        }

        // Ensure first frame is at t=0 for stable spline interpolation
        if (_frames.Count > 0 && _frames[0].time > 0f)
        {
            _frames.Insert(0, new RecordFrame(0f, _frames[0].position, _frames[0].rotation));
        }

        if (echoPrefab == null)
        {
            Debug.LogError("EchoRecorder: asigna echoPrefab.");
            _frames.Clear();
            hud?.ShowToast("Falta el prefab del eco", new Color(1f, 0.43f, 0.43f, 1f), 1.2f);
            return;
        }

        // Future echo: store frames but don't spawn yet
        if (recordFuture)
        {
            _pendingFutureEcho.Clear();
            _pendingFutureEcho.AddRange(_frames);
            _frames.Clear();
            RecordingStopped?.Invoke(true);
            hud?.ShowToast("Eco futuro preparado", new Color(0.7f, 0.85f, 1f, 1f), 1.25f);
            RefreshHud();
            return;
        }

        TrimEchoesIfNeeded();

        Vector3 spawnPosition = _frames.Count > 0 ? _frames[0].position : echoSpawnRoot.position;
        Quaternion spawnRotation = _frames.Count > 0 ? _frames[0].rotation : echoSpawnRoot.rotation;
        GameObject instance = Instantiate(echoPrefab, spawnPosition, spawnRotation);
        instance.tag = "Echo";
        instance.layer = LayerMask.NameToLayer("Echo"); // Layer 9
        var playback = instance.GetComponent<EchoPlayback>();
        if (playback == null)
            playback = instance.AddComponent<EchoPlayback>();

        float duration = Mathf.Max(elapsed, 0.05f);
        playback.BeginPlayback(_frames, duration, voiceClip);
        _echoes.Add(playback);

        _frames.Clear();

        RecordingStopped?.Invoke(true);
        EchoCreated?.Invoke(_echoes.Count);
        GameProgress.RecordEchoCreated();
        hud?.ShowToast("Eco creado", new Color(0.48f, 0.94f, 0.78f, 1f), 1.25f);
        GameFeelController.Instance?.PlayEchoSpawn(instance.transform.position);

        RefreshHud();
    }

    void TrimEchoesIfNeeded()
    {
        while (_echoes.Count >= maxEchoes)
        {
            // Find first unlocked slot to trim
            int trimIndex = -1;
            for (int i = 0; i < _echoes.Count; i++)
            {
                if (_slotLocked == null || i >= _slotLocked.Length || !_slotLocked[i])
                {
                    trimIndex = i;
                    break;
                }
            }

            if (trimIndex < 0) break; // All slots locked, can't trim

            EchoPlayback oldest = _echoes[trimIndex];
            _echoes.RemoveAt(trimIndex);
            if (oldest != null)
                oldest.FadeOutAndDestroy(); // Residual 2.5s (AnalogGhost, alpha 0.3→0)
        }
    }

    public void ClearAllEchoes(bool showFeedback = true)
    {
        _recording = false;
        DisableRecordingRim();
        _frames.Clear();
        _lastRecordDuration = 0f;
        _playerController?.SetInputLocked(false);
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
        hud.SetEchoState(_recording ? "Grabando" : (_echoes.Count > 0 ? "Reproduciendo" : "Listo"));
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

    public void ForceUnlockPlayer()
    {
        _recording = false;
        DisableRecordingRim();
        _playerController?.SetInputLocked(false);
    }

    void EnableRecordingRim()
    {
        if (_recordingRimLight == null)
        {
            GameObject rimObject = new GameObject("RecordingRimLight");
            rimObject.transform.SetParent(transform, false);
            rimObject.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            _recordingRimLight = rimObject.AddComponent<Light>();
            _recordingRimLight.type = LightType.Point;
            _recordingRimLight.color = RimCyan;
            _recordingRimLight.intensity = 1.6f;
            _recordingRimLight.range = 3.5f;
            _recordingRimLight.shadows = LightShadows.None;
        }
        _recordingRimLight.enabled = true;

        // Emisión cyan sutil vía MaterialPropertyBlock — no instancia materiales
        // y se limpia al soltar la tecla. Solo afecta al jugador, nunca a los ecos.
        if (_rimBlock == null)
            _rimBlock = new MaterialPropertyBlock();
        _rimRenderers.Clear();
        GetComponentsInChildren(true, _rimRenderers);
        foreach (var rendererRef in _rimRenderers)
        {
            if (rendererRef == null || rendererRef.GetComponent<Light>() != null)
                continue;
            rendererRef.GetPropertyBlock(_rimBlock);
            _rimBlock.SetColor("_EmissionColor", RimCyan * 0.35f);
            rendererRef.SetPropertyBlock(_rimBlock);
        }
    }

    void DisableRecordingRim()
    {
        if (_recordingRimLight != null)
            _recordingRimLight.enabled = false;

        foreach (var rendererRef in _rimRenderers)
        {
            if (rendererRef != null)
                rendererRef.SetPropertyBlock(null);
        }
        _rimRenderers.Clear();
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
