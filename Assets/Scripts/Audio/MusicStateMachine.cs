using System;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Simple cross-fade music state machine for Echoes of You.
/// Manages 2 music stems (A/B) for seamless transitions between states.
/// Reads clips from Resources/Audio/ via logical names.
/// </summary>
public class MusicStateMachine : MonoBehaviour
{
    public enum MusicState
    {
        Silence,
        Exploration,
        Tension,
        Puzzle,
        Memory,
        Dialogue,
        Ending,
        Credits,
        Menu
    }

    [Header("Config")]
    [SerializeField] float crossfadeDuration = 2.5f;
    [SerializeField] float defaultMusicVolume = 0.6f;

    [Header("Runtime (read-only)")]
    [SerializeField] MusicState _currentState = MusicState.Silence;
    [SerializeField] MusicState _targetState = MusicState.Silence;
    [SerializeField] float _crossfadeTimer;

    AudioSource _stemA;
    AudioSource _stemB;
    AudioSource _activeStem => _crossfadeTimer > 0f ? _stemB : _stemA;
    AudioSource _nextStem => _crossfadeTimer > 0f ? _stemA : _stemB;

    EchoesAudioManager _audioMgr;

    // Clip cache
    static readonly System.Collections.Generic.Dictionary<MusicState, AudioClip> _clipCache = new();

    public MusicState CurrentState => _currentState;
    public bool IsTransitioning => _crossfadeTimer > 0f;

    void Awake()
    {
        _audioMgr = EchoesAudioManager.EnsureExists();

        _stemA = gameObject.AddComponent<AudioSource>();
        _stemA.loop = true;
        _stemA.spatialBlend = 0f;
        _stemA.playOnAwake = false;
        if (_audioMgr != null)
            _stemA.outputAudioMixerGroup = _audioMgr.FindGroup("Music");

        _stemB = gameObject.AddComponent<AudioSource>();
        _stemB.loop = true;
        _stemB.spatialBlend = 0f;
        _stemB.playOnAwake = false;
        _stemB.volume = 0f;
        if (_audioMgr != null)
            _stemB.outputAudioMixerGroup = _audioMgr.FindGroup("Music");
    }

    void Update()
    {
        if (_crossfadeTimer > 0f)
        {
            _crossfadeTimer -= Time.unscaledDeltaTime;
            float t = 1f - Mathf.Clamp01(_crossfadeTimer / crossfadeDuration);
            float eased = EaseInOut(t);
            _stemA.volume = Mathf.Lerp(_stemA.volume, _crossfadeTimer > 0f ? 0f : defaultMusicVolume, eased);
            _stemB.volume = Mathf.Lerp(_stemB.volume, _crossfadeTimer > 0f ? defaultMusicVolume : 0f, eased);

            if (_crossfadeTimer <= 0f)
            {
                _currentState = _targetState;
                var tmp = _stemA; _stemA = _stemB; _stemB = tmp;
                _stemB.Stop();
                _stemB.clip = null;
                _stemB.volume = 0f;
            }
        }
    }

    static float EaseInOut(float t) => t * t * (3f - 2f * t);

    /// <summary>
    /// Request a music state change with crossfade.
    /// </summary>
    public void SetState(MusicState newState)
    {
        if (newState == _currentState && _crossfadeTimer <= 0f) return;
        if (newState == _targetState && _crossfadeTimer > 0f) return;

        _targetState = newState;
        _crossfadeTimer = crossfadeDuration;

        var clip = GetClipForState(newState);
        if (clip == null)
        {
            Debug.LogWarning($"[MusicStateMachine] No clip for state {newState}");
            _crossfadeTimer = 0f;
            return;
        }

        _nextStem.clip = clip;
        _nextStem.volume = 0f;
        _nextStem.Play();
    }

    /// <summary>
    /// Immediate cut (no crossfade). Used for scene loads.
    /// </summary>
    public void SetStateImmediate(MusicState newState)
    {
        _currentState = newState;
        _targetState = newState;
        _crossfadeTimer = 0f;

        var clip = GetClipForState(newState);
        if (clip == null) return;

        _stemA.clip = clip;
        _stemA.volume = defaultMusicVolume;
        _stemA.Play();
        _stemB.Stop();
        _stemB.clip = null;
        _stemB.volume = 0f;
    }

    AudioClip GetClipForState(MusicState state)
    {
        if (_clipCache.TryGetValue(state, out var cached))
            return cached;

        string resourcePath = state switch
        {
            MusicState.Exploration => "Audio/01-mus_exploration_dronewavduration-30-secondsseamless-loopno_081626",
            MusicState.Tension => "Audio/seamless-loopno-melodypromptcreate-a-dark_081626",
            MusicState.Puzzle => "Audio/mus_puzzle_texture",
            MusicState.Memory => "Audio/mus_memory_piano",
            MusicState.Dialogue => "Audio/mus_dialogue_bed",
            MusicState.Ending => "Audio/mus_ending_theme",
            MusicState.Credits => "Audio/mus_credits",
            MusicState.Menu => "Audio/mus_exploration_dronewavduration-30-secondsseamless-loopno_081626", // reuse exploration drone for menu
            _ => null
        };

        if (string.IsNullOrEmpty(resourcePath))
            return null;

        var clip = Resources.Load<AudioClip>(resourcePath);
        _clipCache[state] = clip;
        return clip;
    }

    /// <summary>
    /// Ensures the singleton exists in the scene.
    /// </summary>
    public static MusicStateMachine EnsureExists()
    {
        var existing = FindAnyObjectByType<MusicStateMachine>();
        if (existing != null) return existing;

        var go = new GameObject("MusicStateMachine");
        return go.AddComponent<MusicStateMachine>();
    }
}