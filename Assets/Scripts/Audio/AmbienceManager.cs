using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages ambient loops (room tone, hallway, ventilation, industrial, tape hiss, memory whisper).
/// Each layer has independent volume and can be toggled per scene/zone.
/// </summary>
public class AmbienceManager : MonoBehaviour
{
    [Header("Ambience Layers")]
    [SerializeField] AudioClip roomToneClip;
    [SerializeField] AudioClip hallwayClip;
    [SerializeField] AudioClip ventilationClip;
    [SerializeField] AudioClip industrialClip;
    [SerializeField] AudioClip distantClangClip;
    [SerializeField] AudioClip metalConcreteClip;
    [SerializeField] AudioClip fluorescentHumClip;
    [SerializeField] AudioClip tapeHissClip;
    [SerializeField] AudioClip memoryWhisperClip;

    [Header("Volumes (0-1)")]
    [SerializeField] float roomToneVolume = 0.15f;
    [SerializeField] float hallwayVolume = 0.12f;
    [SerializeField] float ventilationVolume = 0.08f;
    [SerializeField] float industrialVolume = 0.1f;
    [SerializeField] float distantClangVolume = 0.06f;
    [SerializeField] float metalConcreteVolume = 0.05f;
    [SerializeField] float fluorescentHumVolume = 0.07f;
    [SerializeField] float tapeHissVolume = 0.03f;
    [SerializeField] float memoryWhisperVolume = 0.1f;

    AudioSource _roomToneSource;
    AudioSource _hallwaySource;
    AudioSource _ventilationSource;
    AudioSource _industrialSource;
    AudioSource _distantClangSource;
    AudioSource _metalConcreteSource;
    AudioSource _fluorescentHumSource;
    AudioSource _tapeHissSource;
    AudioSource _memoryWhisperSource;

    EchoesAudioManager _audioMgr;

    public static AmbienceManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _audioMgr = EchoesAudioManager.EnsureExists();
    }

    void Start()
    {
        SetupAmbienceLayers();
    }

    void SetupAmbienceLayers()
    {
        var ambienceGroup = _audioMgr?.FindGroup("Ambience");
        var tapeHissGroup = _audioMgr?.FindGroup("TapeHiss");

        // Room Tone (loop)
        if (roomToneClip != null)
        {
            _roomToneSource = gameObject.AddComponent<AudioSource>();
            _roomToneSource.clip = roomToneClip;
            _roomToneSource.loop = true;
            _roomToneSource.volume = roomToneVolume;
            _roomToneSource.spatialBlend = 0f;
            if (ambienceGroup != null) _roomToneSource.outputAudioMixerGroup = ambienceGroup;
            _roomToneSource.Play();
        }

        // Hallway (loop)
        if (hallwayClip != null)
        {
            _hallwaySource = gameObject.AddComponent<AudioSource>();
            _hallwaySource.clip = hallwayClip;
            _hallwaySource.loop = true;
            _hallwaySource.volume = hallwayVolume;
            _hallwaySource.spatialBlend = 0f;
            if (ambienceGroup != null) _hallwaySource.outputAudioMixerGroup = ambienceGroup;
            _hallwaySource.Play();
        }

        // Ventilation (loop)
        if (ventilationClip != null)
        {
            _ventilationSource = gameObject.AddComponent<AudioSource>();
            _ventilationSource.clip = ventilationClip;
            _ventilationSource.loop = true;
            _ventilationSource.volume = ventilationVolume;
            _ventilationSource.spatialBlend = 0f;
            if (ambienceGroup != null) _ventilationSource.outputAudioMixerGroup = ambienceGroup;
            _ventilationSource.Play();
        }

        // Industrial (loop)
        if (industrialClip != null)
        {
            _industrialSource = gameObject.AddComponent<AudioSource>();
            _industrialSource.clip = industrialClip;
            _industrialSource.loop = true;
            _industrialSource.volume = industrialVolume;
            _industrialSource.spatialBlend = 0f;
            if (ambienceGroup != null) _industrialSource.outputAudioMixerGroup = ambienceGroup;
            _industrialSource.Play();
        }

        // Distant Clang (one-shot, triggered periodically)
        if (distantClangClip != null)
        {
            _distantClangSource = gameObject.AddComponent<AudioSource>();
            _distantClangSource.spatialBlend = 1f;
            _distantClangSource.minDistance = 10f;
            _distantClangSource.maxDistance = 50f;
            _distantClangSource.rolloffMode = AudioRolloffMode.Logarithmic;
            if (ambienceGroup != null) _distantClangSource.outputAudioMixerGroup = ambienceGroup;
            InvokeRepeating(nameof(PlayDistantClang), 15f, UnityEngine.Random.Range(20f, 40f));
        }

        // Metal Concrete (one-shot, for movement scrapes - triggered by GameFeelController)
        if (metalConcreteClip != null)
        {
            _metalConcreteSource = gameObject.AddComponent<AudioSource>();
            _metalConcreteSource.spatialBlend = 1f;
            if (ambienceGroup != null) _metalConcreteSource.outputAudioMixerGroup = ambienceGroup;
        }

        // Fluorescent Hum (loop)
        if (fluorescentHumClip != null)
        {
            _fluorescentHumSource = gameObject.AddComponent<AudioSource>();
            _fluorescentHumSource.clip = fluorescentHumClip;
            _fluorescentHumSource.loop = true;
            _fluorescentHumSource.volume = fluorescentHumVolume;
            _fluorescentHumSource.spatialBlend = 0f;
            if (ambienceGroup != null) _fluorescentHumSource.outputAudioMixerGroup = ambienceGroup;
            _fluorescentHumSource.Play();
        }

        // Tape Hiss (loop, on TapeHiss bus)
        if (tapeHissClip != null)
        {
            _tapeHissSource = gameObject.AddComponent<AudioSource>();
            _tapeHissSource.clip = tapeHissClip;
            _tapeHissSource.loop = true;
            _tapeHissSource.volume = tapeHissVolume;
            _tapeHissSource.spatialBlend = 0f;
            if (tapeHissGroup != null) _tapeHissSource.outputAudioMixerGroup = tapeHissGroup;
            _tapeHissSource.Play();
        }

        // Memory Whisper (3D, triggered by narrative events)
        if (memoryWhisperClip != null)
        {
            _memoryWhisperSource = gameObject.AddComponent<AudioSource>();
            _memoryWhisperSource.spatialBlend = 1f;
            _memoryWhisperSource.minDistance = 2f;
            _memoryWhisperSource.maxDistance = 15f;
            _memoryWhisperSource.rolloffMode = AudioRolloffMode.Logarithmic;
            if (ambienceGroup != null) _memoryWhisperSource.outputAudioMixerGroup = ambienceGroup;
        }
    }

    public void PlayDistantClang()
    {
        if (_distantClangSource != null && distantClangClip != null)
        {
            _distantClangSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            _distantClangSource.PlayOneShot(distantClangClip, distantClangVolume);
        }
    }

    public void PlayMetalConcrete(Vector3 position)
    {
        if (_metalConcreteSource != null && metalConcreteClip != null)
        {
            var go = new GameObject("MetalConcreteOneShot");
            go.transform.position = position;
            var src = go.AddComponent<AudioSource>();
            src.clip = metalConcreteClip;
            src.volume = metalConcreteVolume;
            src.spatialBlend = 1f;
            src.minDistance = 3f;
            src.maxDistance = 20f;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            if (_audioMgr != null)
                src.outputAudioMixerGroup = _audioMgr.FindGroup("Ambience");
            src.Play();
            Destroy(go, metalConcreteClip.length + 0.5f);
        }
    }

    public void PlayMemoryWhisper(Vector3 position)
    {
        if (memoryWhisperClip != null)
        {
            var go = new GameObject("MemoryWhisperOneShot");
            go.transform.position = position;
            var src = go.AddComponent<AudioSource>();
            src.clip = memoryWhisperClip;
            src.volume = memoryWhisperVolume;
            src.spatialBlend = 1f;
            src.minDistance = 2f;
            src.maxDistance = 15f;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            if (_audioMgr != null)
                src.outputAudioMixerGroup = _audioMgr.FindGroup("Ambience");
            src.Play();
            Destroy(go, memoryWhisperClip.length + 0.5f);
        }
    }

    public void SetLayerVolume(string layer, float volume)
    {
        volume = Mathf.Clamp01(volume);
        switch (layer.ToLower())
        {
            case "roomtone": case "room_tone": roomToneVolume = volume; if (_roomToneSource) _roomToneSource.volume = volume; break;
            case "hallway": hallwayVolume = volume; if (_hallwaySource) _hallwaySource.volume = volume; break;
            case "ventilation": ventilationVolume = volume; if (_ventilationSource) _ventilationSource.volume = volume; break;
            case "industrial": industrialVolume = volume; if (_industrialSource) _industrialSource.volume = volume; break;
            case "distantclang": case "distant_clang": distantClangVolume = volume; break;
            case "metalconcrete": case "metal_concrete": metalConcreteVolume = volume; break;
            case "fluorescenthum": case "fluorescent_hum": fluorescentHumVolume = volume; if (_fluorescentHumSource) _fluorescentHumSource.volume = volume; break;
            case "tapehiss": case "tape_hiss": tapeHissVolume = volume; if (_tapeHissSource) _tapeHissSource.volume = volume; break;
            case "memorywhisper": case "memory_whisper": memoryWhisperVolume = volume; break;
        }
    }

    public void StopAll()
    {
        _roomToneSource?.Stop();
        _hallwaySource?.Stop();
        _ventilationSource?.Stop();
        _industrialSource?.Stop();
        _fluorescentHumSource?.Stop();
        _tapeHissSource?.Stop();
        CancelInvoke(nameof(PlayDistantClang));
    }

    public static AmbienceManager EnsureExists()
    {
        var existing = FindAnyObjectByType<AmbienceManager>();
        if (existing != null) return existing;

        var go = new GameObject("AmbienceManager");
        return go.AddComponent<AmbienceManager>();
    }
}