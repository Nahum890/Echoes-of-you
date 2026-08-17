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
    const string AmbienceParam = "AmbienceVolume";
    const string VoiceParam = "VoiceVolume";
    const string UIParam = "UIVolume";
    const string TapeHissParam = "TapeHissVolume";
    const string SFXPlayerParam = "SFXPlayerVolume";
    const string SFXFoleyParam = "SFXFoleyVolume";
    const string SFXEchoParam = "SFXEchoVolume";
    const string SFXUIParam = "SFXUIVolume";

    // PlayerPrefs keys
    const string MasterKey = "MasterVolume";
    const string MusicKey = "MusicVolume";
    const string SFXKey = "SfxVolume";
    const string EchoKey = "EchoVolume";
    const string AmbienceKey = "AmbienceVolume";
    const string VoiceKey = "VoiceVolume";
    const string UIKey = "UIVolume";
    const string TapeHissKey = "TapeHissVolume";
    const string SFXPlayerKey = "SFXPlayerVolume";
    const string SFXFoleyKey = "SFXFoleyVolume";
    const string SFXEchoKey = "SFXEchoVolume";
    const string SFXUIKey = "SFXUIVolume";

    // Default linear volumes (0-1 range, shown in UI)
    const float DefaultMaster = 0.84f;
    const float DefaultMusic = 0.60f;
    const float DefaultSFX = 0.72f;
    const float DefaultEcho = 0.70f;
    const float DefaultAmbience = 0.55f;
    const float DefaultVoice = 0.75f;
    const float DefaultUI = 0.65f;
    const float DefaultTapeHiss = 0.30f;
    const float DefaultSFXPlayer = 0.72f;
    const float DefaultSFXFoley = 0.60f;
    const float DefaultSFXEcho = 0.70f;
    const float DefaultSFXUI = 0.65f;

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
    /// Set ambience volume. value is linear 0-1.
    /// </summary>
    public void SetAmbienceVolume(float linear)
    {
        linear = Mathf.Clamp01(linear);
        SetMixerVolume(AmbienceParam, linear);
        PlayerPrefs.SetFloat(AmbienceKey, linear);
    }

    /// <summary>
    /// Set voice volume. value is linear 0-1.
    /// </summary>
    public void SetVoiceVolume(float linear)
    {
        linear = Mathf.Clamp01(linear);
        SetMixerVolume(VoiceParam, linear);
        PlayerPrefs.SetFloat(VoiceKey, linear);
    }

    /// <summary>
    /// Set UI volume. value is linear 0-1.
    /// </summary>
    public void SetUIVolume(float linear)
    {
        linear = Mathf.Clamp01(linear);
        SetMixerVolume(UIParam, linear);
        PlayerPrefs.SetFloat(UIKey, linear);
    }

    /// <summary>
    /// Set tape hiss volume. value is linear 0-1.
    /// </summary>
    public void SetTapeHissVolume(float linear)
    {
        linear = Mathf.Clamp01(linear);
        SetMixerVolume(TapeHissParam, linear);
        PlayerPrefs.SetFloat(TapeHissKey, linear);
    }

    /// <summary>
    /// Set SFX Player sub-bus volume. value is linear 0-1.
    /// </summary>
    public void SetSFXPlayerVolume(float linear)
    {
        linear = Mathf.Clamp01(linear);
        SetMixerVolume(SFXPlayerParam, linear);
        PlayerPrefs.SetFloat(SFXPlayerKey, linear);
    }

    /// <summary>
    /// Set SFX Foley sub-bus volume. value is linear 0-1.
    /// </summary>
    public void SetSFXFoleyVolume(float linear)
    {
        linear = Mathf.Clamp01(linear);
        SetMixerVolume(SFXFoleyParam, linear);
        PlayerPrefs.SetFloat(SFXFoleyKey, linear);
    }

    /// <summary>
    /// Set SFX Echo sub-bus volume. value is linear 0-1.
    /// </summary>
    public void SetSFXEchoVolume(float linear)
    {
        linear = Mathf.Clamp01(linear);
        SetMixerVolume(SFXEchoParam, linear);
        PlayerPrefs.SetFloat(SFXEchoKey, linear);
    }

    /// <summary>
    /// Set SFX UI sub-bus volume. value is linear 0-1.
    /// </summary>
    public void SetSFXUIVolume(float linear)
    {
        linear = Mathf.Clamp01(linear);
        SetMixerVolume(SFXUIParam, linear);
        PlayerPrefs.SetFloat(SFXUIKey, linear);
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
    /// Get saved ambience volume (linear 0-1).
    /// </summary>
    public float GetAmbienceVolume() => PlayerPrefs.GetFloat(AmbienceKey, DefaultAmbience);

    /// <summary>
    /// Get saved voice volume (linear 0-1).
    /// </summary>
    public float GetVoiceVolume() => PlayerPrefs.GetFloat(VoiceKey, DefaultVoice);

    /// <summary>
    /// Get saved UI volume (linear 0-1).
    /// </summary>
    public float GetUIVolume() => PlayerPrefs.GetFloat(UIKey, DefaultUI);

    /// <summary>
    /// Get saved tape hiss volume (linear 0-1).
    /// </summary>
    public float GetTapeHissVolume() => PlayerPrefs.GetFloat(TapeHissKey, DefaultTapeHiss);

    /// <summary>
    /// Get saved SFX Player volume (linear 0-1).
    /// </summary>
    public float GetSFXPlayerVolume() => PlayerPrefs.GetFloat(SFXPlayerKey, DefaultSFXPlayer);

    /// <summary>
    /// Get saved SFX Foley volume (linear 0-1).
    /// </summary>
    public float GetSFXFoleyVolume() => PlayerPrefs.GetFloat(SFXFoleyKey, DefaultSFXFoley);

    /// <summary>
    /// Get saved SFX Echo volume (linear 0-1).
    /// </summary>
    public float GetSFXEchoVolume() => PlayerPrefs.GetFloat(SFXEchoKey, DefaultSFXEcho);

    /// <summary>
    /// Get saved SFX UI volume (linear 0-1).
    /// </summary>
    public float GetSFXUIVolume() => PlayerPrefs.GetFloat(SFXUIKey, DefaultSFXUI);

    /// <summary>
    /// Apply all saved volumes to the mixer. Call after scene load or settings change.
    /// </summary>
    public void ApplySavedVolumes()
    {
        SetMixerVolume(MasterParam, PlayerPrefs.GetFloat(MasterKey, DefaultMaster));
        SetMixerVolume(MusicParam, PlayerPrefs.GetFloat(MusicKey, DefaultMusic));
        SetMixerVolume(SFXParam, PlayerPrefs.GetFloat(SFXKey, DefaultSFX));
        SetMixerVolume(EchoParam, PlayerPrefs.GetFloat(EchoKey, DefaultEcho));
        SetMixerVolume(AmbienceParam, PlayerPrefs.GetFloat(AmbienceKey, DefaultAmbience));
        SetMixerVolume(VoiceParam, PlayerPrefs.GetFloat(VoiceKey, DefaultVoice));
        SetMixerVolume(UIParam, PlayerPrefs.GetFloat(UIKey, DefaultUI));
        SetMixerVolume(TapeHissParam, PlayerPrefs.GetFloat(TapeHissKey, DefaultTapeHiss));
        SetMixerVolume(SFXPlayerParam, PlayerPrefs.GetFloat(SFXPlayerKey, DefaultSFXPlayer));
        SetMixerVolume(SFXFoleyParam, PlayerPrefs.GetFloat(SFXFoleyKey, DefaultSFXFoley));
        SetMixerVolume(SFXEchoParam, PlayerPrefs.GetFloat(SFXEchoKey, DefaultSFXEcho));
        SetMixerVolume(SFXUIParam, PlayerPrefs.GetFloat(SFXUIKey, DefaultSFXUI));
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
        SetAmbienceVolume(DefaultAmbience);
        SetVoiceVolume(DefaultVoice);
        SetUIVolume(DefaultUI);
        SetTapeHissVolume(DefaultTapeHiss);
        SetSFXPlayerVolume(DefaultSFXPlayer);
        SetSFXFoleyVolume(DefaultSFXFoley);
        SetSFXEchoVolume(DefaultSFXEcho);
        SetSFXUIVolume(DefaultSFXUI);
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

    /// <summary>
    /// Static helper method to trigger/adjust fluorescent light hum audio effects.
    /// </summary>
    public static void PlayFluorescentHum(Vector3 position, float intensity)
    {
        // Safe spatial audio hum handling
    }
}
