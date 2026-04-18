using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grabación por mantener R: hasta 10 s o al soltar. Genera un eco que repite el bucle.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class EchoRecorder : MonoBehaviour
{
    [Header("Prefab y límites")]
    [SerializeField] GameObject echoPrefab;
    [SerializeField] Transform echoSpawnRoot;
    [SerializeField] int maxEchoes = 3;
    [SerializeField] float maxRecordSeconds = 10f;
    [SerializeField] float minRecordSeconds = 0.35f;

    [Header("HUD (opcional)")]
    [SerializeField] GameHUD hud;

    readonly List<RecordFrame> _frames = new List<RecordFrame>();
    readonly List<EchoPlayback> _echoes = new List<EchoPlayback>();

    bool _recording;
    float _recordStartTime;
    float _lastRecordDuration;

    public bool IsRecording => _recording;
    public int EchoCount => _echoes.Count;
    public int MaxEchoes => maxEchoes;
    public float MaxRecordSeconds => maxRecordSeconds;
    public float RecordingElapsed => _recording ? Mathf.Min(Time.time - _recordStartTime, maxRecordSeconds) : 0f;
    public float LastClipDuration => _lastRecordDuration;

    void Awake()
    {
        if (hud == null)
            hud = FindFirstObjectByType<GameHUD>();
        if (echoSpawnRoot == null)
            echoSpawnRoot = transform;
    }

    void OnEnable()
    {
        RefreshHud();
    }

    void Update()
    {
        bool hold = Input.GetKey(KeyCode.R);

        if (hold && !_recording)
            StartRecording();

        if (!hold && _recording)
            StopRecordingAndSpawn();

        if (_recording && Time.time - _recordStartTime >= maxRecordSeconds)
            StopRecordingAndSpawn();

        RefreshHud();
    }

    void FixedUpdate()
    {
        if (!_recording)
            return;

        float elapsed = Time.time - _recordStartTime;
        _frames.Add(new RecordFrame(elapsed, transform.position, transform.rotation));

        if (_frames.Count == 1)
        {
            // Duplicar primer fotograma en t=0 para interpolación estable al inicio
            _frames.Insert(0, new RecordFrame(0f, _frames[0].position, _frames[0].rotation));
        }
    }

    void StartRecording()
    {
        _recording = true;
        _recordStartTime = Time.time;
        _frames.Clear();
    }

    void StopRecordingAndSpawn()
    {
        if (!_recording)
            return;

        _recording = false;
        float elapsed = Time.time - _recordStartTime;
        _lastRecordDuration = elapsed;

        if (elapsed < minRecordSeconds || _frames.Count < 2)
        {
            _frames.Clear();
            RefreshHud();
            return;
        }

        if (echoPrefab == null)
        {
            Debug.LogError("EchoRecorder: asigna echoPrefab.");
            _frames.Clear();
            return;
        }

        TrimEchoesIfNeeded();

        GameObject instance = Instantiate(echoPrefab, echoSpawnRoot.position, echoSpawnRoot.rotation);
        var playback = instance.GetComponent<EchoPlayback>();
        if (playback == null)
            playback = instance.AddComponent<EchoPlayback>();

        float duration = Mathf.Max(elapsed, 0.05f);
        playback.BeginPlayback(_frames, duration);
        _echoes.Add(playback);

        _frames.Clear();
        RefreshHud();
    }

    void TrimEchoesIfNeeded()
    {
        while (_echoes.Count >= maxEchoes)
        {
            EchoPlayback oldest = _echoes[0];
            _echoes.RemoveAt(0);
            if (oldest != null)
                Destroy(oldest.gameObject);
        }
    }

    public void ClearAllEchoes()
    {
        for (int i = 0; i < _echoes.Count; i++)
        {
            if (_echoes[i] != null)
                Destroy(_echoes[i].gameObject);
        }

        _echoes.Clear();
        RefreshHud();
    }

    void RefreshHud()
    {
        if (hud == null)
            return;

        hud.SetEchoCount(_echoes.Count, maxEchoes);
        hud.SetRecording(_recording, _recording ? RecordingElapsed / Mathf.Max(0.01f, maxRecordSeconds) : 0f);
    }
}
