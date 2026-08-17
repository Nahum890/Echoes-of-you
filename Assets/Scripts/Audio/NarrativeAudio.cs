using UnityEngine;

/// <summary>
/// Helper for narrative audio: voice lines, memory discovery, dialogue events.
/// Call static methods from VN_DialogueController, LevelIntroTrigger, etc.
/// </summary>
public static class NarrativeAudio
{
    // Voice clips (Resources/Audio/VN/voice/) — 0 KB placeholders currently
    static AudioClip _lyra01 => Resources.Load<AudioClip>("Audio/VN/voice/lyra_line_001");
    static AudioClip _lyra02 => Resources.Load<AudioClip>("Audio/VN/voice/lyra_line_002");
    static AudioClip _aiden01 => Resources.Load<AudioClip>("Audio/VN/voice/aiden_line_001");
    static AudioClip _aiden02 => Resources.Load<AudioClip>("Audio/VN/voice/aiden_line_002");

    // Narrative SFX
    static AudioClip _memoryDiscovery => Resources.Load<AudioClip>("Audio/narrative/sfx_memory_discovery");
    static AudioClip _memoryWhisper => Resources.Load<AudioClip>("Audio/narrative/amb_memory_whisper");

    static AudioSource _voiceSource;
    static AudioSource _sfxSource;
    static EchoesAudioManager _audioMgr;

    static void EnsureSources()
    {
        if (_voiceSource != null) return;

        var go = new GameObject("NarrativeAudio");
        UnityEngine.Object.DontDestroyOnLoad(go);

        _audioMgr = EchoesAudioManager.EnsureExists();
        var voiceGroup = _audioMgr?.FindGroup("Voice");
        var ambienceGroup = _audioMgr?.FindGroup("Ambience");

        _voiceSource = go.AddComponent<AudioSource>();
        _voiceSource.spatialBlend = 0f;
        _voiceSource.playOnAwake = false;
        _voiceSource.priority = 48;
        if (voiceGroup != null) _voiceSource.outputAudioMixerGroup = voiceGroup;

        _sfxSource = go.AddComponent<AudioSource>();
        _sfxSource.spatialBlend = 1f;
        _sfxSource.playOnAwake = false;
        _sfxSource.priority = 64;
        if (ambienceGroup != null) _sfxSource.outputAudioMixerGroup = ambienceGroup;
    }

    /// <summary>
    /// Play Lyra voice line (0 = line 001, 1 = line 002).
    /// </summary>
    public static void PlayLyraLine(int index = 0)
    {
        EnsureSources();
        var clip = index == 0 ? _lyra01 : _lyra02;
        if (clip != null && clip.length > 0.1f) // skip 0 KB placeholders
        {
            _voiceSource.pitch = 1f;
            _voiceSource.PlayOneShot(clip, 0.85f);
        }
    }

    /// <summary>
    /// Play Aiden voice line (0 = line 001, 1 = line 002).
    /// </summary>
    public static void PlayAidenLine(int index = 0)
    {
        EnsureSources();
        var clip = index == 0 ? _aiden01 : _aiden02;
        if (clip != null && clip.length > 0.1f)
        {
            _voiceSource.pitch = 1f;
            _voiceSource.PlayOneShot(clip, 0.85f);
        }
    }

    /// <summary>
    /// Play memory discovery sting at world position (3D).
    /// </summary>
    public static void PlayMemoryDiscovery(Vector3 position)
    {
        if (_memoryDiscovery == null) return;
        var go = new GameObject("MemoryDiscoverySFX");
        go.transform.position = position;
        var src = go.AddComponent<AudioSource>();
        src.clip = _memoryDiscovery;
        src.volume = 0.6f;
        src.spatialBlend = 1f;
        src.minDistance = 3f;
        src.maxDistance = 20f;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        if (_audioMgr != null)
            src.outputAudioMixerGroup = _audioMgr.FindGroup("Ambience");
        src.Play();
        UnityEngine.Object.Destroy(go, _memoryDiscovery.length + 0.5f);
    }

    /// <summary>
    /// Play memory whisper at world position (3D, subtle).
    /// </summary>
    public static void PlayMemoryWhisper(Vector3 position)
    {
        if (_memoryWhisper == null) return;
        var go = new GameObject("MemoryWhisperSFX");
        go.transform.position = position;
        var src = go.AddComponent<AudioSource>();
        src.clip = _memoryWhisper;
        src.volume = 0.4f;
        src.spatialBlend = 1f;
        src.minDistance = 2f;
        src.maxDistance = 15f;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        if (_audioMgr != null)
            src.outputAudioMixerGroup = _audioMgr.FindGroup("Ambience");
        src.Play();
        UnityEngine.Object.Destroy(go, _memoryWhisper.length + 0.5f);
    }

    /// <summary>
    /// Play room tone loop (2D, on Ambience bus).
    /// </summary>
    public static void PlayRoomTone(bool play = true)
    {
        EnsureSources();
        // Room tone is handled by AmbienceManager, this is a placeholder for direct control
    }
}