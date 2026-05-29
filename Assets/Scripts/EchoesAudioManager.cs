using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Singleton runtime audio manager for Echoes of You.
/// Loads the AudioMixer from Resources and provides volume control
/// with proper logarithmic scaling (dB) for the UI sliders.
/// Persists volume settings via PlayerPrefs.
/// </summary>
public class EchoesAudioManager : MonoBehaviour
{
    public static EchoesAudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] AudioMixer audioMixer;

    // Exposed parameter names — must match the mixer's exposed parameters
    const string MasterParam = "MasterVolume";
    const string MusicParam = "MusicVolume";
    const string SFXParam = "SFXVolume";
    const string EchoParam = "EchoVolume";

    // PlayerPrefs keys
    const string MasterKey = "MasterVolume";
    const string MusicKey = "MusicVolume";
    const string SFXKey = "SfxVolume";
    const string EchoKey = "EchoVolume";

    // Default linear volumes (0-1 range, shown in UI)
    const float DefaultMaster = 0.84f;
    const float DefaultMusic = 0.60f;
    const float DefaultSFX = 0.72f;
    const float DefaultEcho = 0.70f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioMixer == null)
            audioMixer = Resources.Load<AudioMixer>("EchoesAudioMixer");
    }

    void Start()
    {
        ApplySavedVolumes();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Returns the AudioMixer reference for routing AudioSources to specific groups.
    /// </summary>
    public AudioMixer Mixer => audioMixer;

    /// <summary>
    /// Finds a mixer group by name (e.g. "Music", "SFX", "Echo", "Master").
    /// </summary>
    public AudioMixerGroup FindGroup(string groupName)
    {
        if (audioMixer == null) return null;
        AudioMixerGroup[] groups = audioMixer.FindMatchingGroups(groupName);
        return groups != null && groups.Length > 0 ? groups[0] : null;
    }

    // ═══════════════════════════════════════
    // VOLUME CONTROL (Linear 0–1 → dB)
    // ═══════════════════════════════════════

    /// <summary>
    /// Set master volume. value is linear 0-1.
    /// </summary>
    public void SetMasterVolume(float linear)
    {
        linear = Mathf.Clamp01(linear);
        SetMixerVolume(MasterParam, linear);
        PlayerPrefs.SetFloat(MasterKey, linear);
    }

    /// <summary>
    /// Set music volume. value is linear 0-1.
    /// </summary>
    public void SetMusicVolume(float linear)
    {
        linear = Mathf.Clamp01(linear);
        SetMixerVolume(MusicParam, linear);
        PlayerPrefs.SetFloat(MusicKey, linear);
    }

    /// <summary>
    /// Set SFX volume. value is linear 0-1.
    /// </summary>
    public void SetSFXVolume(float linear)
    {
        linear = Mathf.Clamp01(linear);
        SetMixerVolume(SFXParam, linear);
        PlayerPrefs.SetFloat(SFXKey, linear);
        SetEchoVolume(linear);
    }

    /// <summary>
    /// Set echo replay volume. value is linear 0-1.
    /// </summary>
    public void SetEchoVolume(float linear)
    {
        linear = Mathf.Clamp01(linear);
        SetMixerVolume(EchoParam, linear);
        PlayerPrefs.SetFloat(EchoKey, linear);
    }

    /// <summary>
    /// Get saved master volume (linear 0-1).
    /// </summary>
    public float GetMasterVolume() => PlayerPrefs.GetFloat(MasterKey, DefaultMaster);

    /// <summary>
    /// Get saved music volume (linear 0-1).
    /// </summary>
    public float GetMusicVolume() => PlayerPrefs.GetFloat(MusicKey, DefaultMusic);

    /// <summary>
    /// Get saved SFX volume (linear 0-1).
    /// </summary>
    public float GetSFXVolume() => PlayerPrefs.GetFloat(SFXKey, DefaultSFX);

    /// <summary>
    /// Get saved echo volume (linear 0-1).
    /// </summary>
    public float GetEchoVolume() => PlayerPrefs.GetFloat(EchoKey, DefaultEcho);

    /// <summary>
    /// Apply all saved volumes to the mixer. Call after scene load or settings change.
    /// </summary>
    public void ApplySavedVolumes()
    {
        SetMixerVolume(MasterParam, PlayerPrefs.GetFloat(MasterKey, DefaultMaster));
        SetMixerVolume(MusicParam, PlayerPrefs.GetFloat(MusicKey, DefaultMusic));
        SetMixerVolume(SFXParam, PlayerPrefs.GetFloat(SFXKey, DefaultSFX));
        SetMixerVolume(EchoParam, PlayerPrefs.GetFloat(EchoKey, DefaultEcho));
    }

    /// <summary>
    /// Reset all volumes to defaults.
    /// </summary>
    public void RestoreDefaults()
    {
        SetMasterVolume(DefaultMaster);
        SetMusicVolume(DefaultMusic);
        SetSFXVolume(DefaultSFX);
        SetEchoVolume(DefaultEcho);
        PlayerPrefs.Save();
    }

    // ═══════════════════════════════════════
    // INTERNAL — Logarithmic conversion
    // ═══════════════════════════════════════

    /// <summary>
    /// Converts a linear 0-1 value to decibels and sets it on the mixer.
    /// Uses logarithmic scaling: 0 → -80dB (silence), 1 → 0dB (full).
    /// </summary>
    void SetMixerVolume(string parameterName, float linearValue)
    {
        if (audioMixer == null) return;

        float dB;
        if (linearValue <= 0.0001f)
            dB = -80f; // Effectively silent
        else
            dB = Mathf.Log10(linearValue) * 20f;

        audioMixer.SetFloat(parameterName, dB);
    }

    /// <summary>
    /// Ensures the AudioManager singleton exists in the scene.
    /// Called by production builder and level runtime systems.
    /// </summary>
    public static EchoesAudioManager EnsureExists()
    {
        if (Instance != null) return Instance;

        EchoesAudioManager existing = FindAnyObjectByType<EchoesAudioManager>();
        if (existing != null) return existing;

        GameObject go = new GameObject("EchoesAudioManager");
        EchoesAudioManager mgr = go.AddComponent<EchoesAudioManager>();
        return mgr;
    }
}
