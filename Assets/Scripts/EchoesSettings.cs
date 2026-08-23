using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Capa central de ajustes de *Echoes of You*.
///
/// Existe porque el menú de ajustes escribía en PlayerPrefs claves que **nadie
/// leía**, y nada volvía a aplicarlas al arrancar el juego. Dos ejemplos reales
/// que arregla este archivo:
///
/// - La sensibilidad se guardaba en <c>"CameraSensitivity"</c> y se notificaba con
///   <c>SendMessage("ApplySavedSensitivity")</c> a <c>ThirdPersonCamera</c>. Pero la
///   cámara viva es <see cref="SimpleFollowCamera"/>, que lee
///   <c>"MouseSensitivity"</c>, y el método <c>ApplySavedSensitivity</c> no existía
///   en ninguna clase: el SendMessage no llegaba a nada.
/// - La niebla se guardaba en <c>"FogDensity"</c> y se aplicaba a
///   <c>RenderSettings.fogDensity</c>, pero <c>LevelEnvironmentBootstrap</c> la
///   sobrescribe en cada nivel con <c>EchoesPresentationSettings.GameFogDensity</c>
///   (clave <c>"Echoes.GameFogDensity"</c>). El slider se pisaba solo.
///
/// Todo ajuste pasa ahora por aquí: una propiedad por ajuste, un
/// <c>Apply…</c> que lo lleva al juego, y un bootstrap que lo restaura al
/// arrancar y en cada carga de escena.
/// </summary>
public static class EchoesSettings
{
    // ── Claves ────────────────────────────────────────────────────────────
    // Se mantienen los nombres que ya leen otros sistemas; donde había dos
    // claves para lo mismo se escriben ambas para no romper al que lea la vieja.

    public const string KeyMasterVolume = "MasterVolume";
    public const string KeyMusicVolume  = "MusicVolume";
    public const string KeySfxVolume    = "SfxVolume";
    public const string KeyEchoVolume   = "EchoVolume";

    /// La que lee SimpleFollowCamera.
    public const string KeySensitivity       = "MouseSensitivity";
    /// Alias histórico que escribe MainMenuController.
    public const string KeySensitivityLegacy = "CameraSensitivity";

    public const string KeyEchoOpacity     = "EchoOpacity";
    public const string KeyExtraRecordTime = "ExtraRecordTime";
    /// Alias histórico de niebla; la canónica es Echoes.GameFogDensity.
    public const string KeyFogLegacy = "FogDensity";

    public const string KeyFullscreen  = "Video.Fullscreen";
    public const string KeyVSync       = "Video.VSync";
    public const string KeyResWidth    = "Video.ResWidth";
    public const string KeyResHeight   = "Video.ResHeight";

    public const string KeyHighContrast  = "HighContrast";
    public const string KeyReduceMotion  = "ReduceMotion";
    public const string KeyReduceFlashes = "ReduceFlashes";
    public const string KeySubtitles     = "Subtitles";
    public const string KeySubtitleSize  = "SubtitleSize";
    public const string KeySubtitleBg    = "SubtitleBg";

    // ── Valores por defecto ───────────────────────────────────────────────

    public const float DefaultMasterVolume = 0.84f;
    public const float DefaultMusicVolume  = 0.60f;
    public const float DefaultSfxVolume    = 0.72f;
    public const float DefaultEchoVolume   = 0.70f;
    public const float DefaultSensitivity  = 1.0f;
    public const float DefaultEchoOpacity  = 0.45f;
    public const float DefaultExtraRecord  = 0f;
    public const float DefaultSubtitleSize = 1.0f;

    /// Todas las claves que posee este sistema. Sirve para restaurar valores de
    /// fábrica **sin** borrar el progreso de la partida, que es lo que hacía el
    /// <c>PlayerPrefs.DeleteAll()</c> del botón "restaurar".
    public static readonly string[] AllKeys =
    {
        KeyMasterVolume, KeyMusicVolume, KeySfxVolume, KeyEchoVolume,
        KeySensitivity, KeySensitivityLegacy,
        KeyEchoOpacity, KeyExtraRecordTime, KeyFogLegacy,
        KeyFullscreen, KeyVSync, KeyResWidth, KeyResHeight,
        KeyHighContrast, KeyReduceMotion, KeyReduceFlashes,
        KeySubtitles, KeySubtitleSize, KeySubtitleBg,
        "UIScaleFactor", "UIScale",
        "Echoes.GameFogDensity",
    };

    // ── Audio ─────────────────────────────────────────────────────────────

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(KeyMasterVolume, DefaultMasterVolume);
        set => PlayerPrefs.SetFloat(KeyMasterVolume, Mathf.Clamp01(value));
    }

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(KeyMusicVolume, DefaultMusicVolume);
        set => PlayerPrefs.SetFloat(KeyMusicVolume, Mathf.Clamp01(value));
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(KeySfxVolume, DefaultSfxVolume);
        set => PlayerPrefs.SetFloat(KeySfxVolume, Mathf.Clamp01(value));
    }

    public static float EchoVolume
    {
        get => PlayerPrefs.GetFloat(KeyEchoVolume, DefaultEchoVolume);
        set => PlayerPrefs.SetFloat(KeyEchoVolume, Mathf.Clamp01(value));
    }

    public static void ApplyAudio()
    {
        var mgr = EchoesAudioManager.EnsureExists();
        if (mgr == null)
        {
            AudioListener.volume = MasterVolume;
            return;
        }

        mgr.SetMasterVolume(MasterVolume);
        mgr.SetMusicVolume(MusicVolume);
        mgr.SetSFXVolume(SfxVolume);
        mgr.SetEchoVolume(EchoVolume);
        // El bus de voz alimenta la voz grabada del eco: sigue al slider del eco
        // para que bajarlo silencie de verdad lo que repite el eco.
        mgr.SetVoiceVolume(EchoVolume);
        AudioListener.volume = 1f;
    }

    // ── Controles ─────────────────────────────────────────────────────────

    public static float Sensitivity
    {
        get
        {
            if (PlayerPrefs.HasKey(KeySensitivity))
                return PlayerPrefs.GetFloat(KeySensitivity, DefaultSensitivity);
            // Partidas guardadas con el menú antiguo.
            return PlayerPrefs.GetFloat(KeySensitivityLegacy, DefaultSensitivity);
        }
        set
        {
            float v = Mathf.Clamp(value, 0.1f, 3f);
            PlayerPrefs.SetFloat(KeySensitivity, v);
            PlayerPrefs.SetFloat(KeySensitivityLegacy, v);
        }
    }

    public static void ApplySensitivity()
    {
        float sens = Sensitivity;

        foreach (var cam in Object.FindObjectsByType<SimpleFollowCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            cam.ApplySavedSensitivity();

        foreach (var cam in Object.FindObjectsByType<ThirdPersonCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            cam.ApplySavedSensitivity();

        GameSettings.SetSensitivity(sens);
    }

    // ── Gameplay ──────────────────────────────────────────────────────────

    public static float EchoOpacity
    {
        get => PlayerPrefs.GetFloat(KeyEchoOpacity, DefaultEchoOpacity);
        set => PlayerPrefs.SetFloat(KeyEchoOpacity, Mathf.Clamp01(value));
    }

    public static void ApplyEchoOpacity()
    {
        foreach (var pb in Object.FindObjectsByType<EchoPlayback>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            pb.ApplySavedEchoOpacity();
    }

    /// Porcentaje extra de duración de grabación (0-100). Lo lee EchoRecorder.
    public static float ExtraRecordTimePercent
    {
        get => Mathf.Clamp(PlayerPrefs.GetFloat(KeyExtraRecordTime, DefaultExtraRecord), 0f, 100f);
        set => PlayerPrefs.SetFloat(KeyExtraRecordTime, Mathf.Clamp(value, 0f, 100f));
    }

    /// Multiplicador listo para usar: 0% → 1.0, 100% → 2.0.
    public static float ExtraRecordTimeMultiplier => 1f + ExtraRecordTimePercent * 0.01f;

    public static void ApplyExtraRecordTime()
    {
        foreach (var rec in Object.FindObjectsByType<EchoRecorder>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            rec.ApplySavedExtraRecordTime();
    }

    /// <summary>
    /// Densidad de niebla del juego. Escribe la clave canónica
    /// (<c>Echoes.GameFogDensity</c>, la que consume LevelEnvironmentBootstrap)
    /// y la histórica, para que el slider no lo pise el bootstrap del nivel.
    /// </summary>
    public static float FogDensity
    {
        get => EchoesPresentationSettings.GameFogDensity;
        set
        {
            float v = Mathf.Clamp(value, 0f, 0.02f);
            PlayerPrefs.SetFloat("Echoes.GameFogDensity", v);
            PlayerPrefs.SetFloat(KeyFogLegacy, v);
        }
    }

    public static void ApplyFog()
    {
        float fog = FogDensity;
        RenderSettings.fog = fog > 0.0001f;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = fog;
    }

    // ── Vídeo ─────────────────────────────────────────────────────────────

    public static bool Fullscreen
    {
        get => PlayerPrefs.GetInt(KeyFullscreen, Screen.fullScreen ? 1 : 0) == 1;
        set => PlayerPrefs.SetInt(KeyFullscreen, value ? 1 : 0);
    }

    public static bool VSync
    {
        get => PlayerPrefs.GetInt(KeyVSync, 1) == 1;
        set => PlayerPrefs.SetInt(KeyVSync, value ? 1 : 0);
    }

    public static void SetResolution(int width, int height)
    {
        PlayerPrefs.SetInt(KeyResWidth, width);
        PlayerPrefs.SetInt(KeyResHeight, height);
    }

    public static void ApplyVideo()
    {
        QualitySettings.vSyncCount = VSync ? 1 : 0;

        int w = PlayerPrefs.GetInt(KeyResWidth, 0);
        int h = PlayerPrefs.GetInt(KeyResHeight, 0);
        bool fs = Fullscreen;

        // Sin resolución guardada solo se toca el modo ventana: cambiar de
        // resolución al vuelo en el editor no hace nada y en build molesta.
        if (w > 0 && h > 0 && (w != Screen.width || h != Screen.height || fs != Screen.fullScreen))
            Screen.SetResolution(w, h, fs);
        else if (fs != Screen.fullScreen)
            Screen.fullScreen = fs;
    }

    // ── Accesibilidad ─────────────────────────────────────────────────────

    public static bool HighContrast
    {
        get => PlayerPrefs.GetInt(KeyHighContrast, 0) == 1;
        set => PlayerPrefs.SetInt(KeyHighContrast, value ? 1 : 0);
    }

    public static bool ReduceMotion
    {
        get => PlayerPrefs.GetInt(KeyReduceMotion, 0) == 1;
        set => PlayerPrefs.SetInt(KeyReduceMotion, value ? 1 : 0);
    }

    /// Consumido por <see cref="LightFlicker"/>: sin parpadeos de fluorescente.
    public static bool ReduceFlashes
    {
        get => PlayerPrefs.GetInt(KeyReduceFlashes, 0) == 1;
        set => PlayerPrefs.SetInt(KeyReduceFlashes, value ? 1 : 0);
    }

    public static bool Subtitles
    {
        get => PlayerPrefs.GetInt(KeySubtitles, 1) == 1;
        set => PlayerPrefs.SetInt(KeySubtitles, value ? 1 : 0);
    }

    public static bool SubtitleBackground
    {
        get => PlayerPrefs.GetInt(KeySubtitleBg, 0) == 1;
        set => PlayerPrefs.SetInt(KeySubtitleBg, value ? 1 : 0);
    }

    public static float SubtitleSize
    {
        get => Mathf.Clamp(PlayerPrefs.GetFloat(KeySubtitleSize, DefaultSubtitleSize), 0.7f, 1.8f);
        set => PlayerPrefs.SetFloat(KeySubtitleSize, Mathf.Clamp(value, 0.7f, 1.8f));
    }

    /// <summary>
    /// Aplica las clases de accesibilidad a todos los paneles UI Toolkit vivos.
    /// Las clases existen en EchoesDesignTokens.uss (.high-contrast, .reduce-motion).
    /// </summary>
    public static void ApplyAccessibility()
    {
        bool contrast = HighContrast;
        bool motion = ReduceMotion;
        bool subs = Subtitles;
        bool subBg = SubtitleBackground;
        float subSize = SubtitleSize;

        foreach (var doc in Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            VisualElement root = doc != null ? doc.rootVisualElement : null;
            if (root == null) continue;

            SetClass(root, "high-contrast", contrast);
            SetClass(root, "reduce-motion", motion);
            SetClass(root, "subtitles-off", !subs);
            SetClass(root, "subtitle-bg", subBg);

            SetClass(root, "subtitle-size-small", subSize < 0.9f);
            SetClass(root, "subtitle-size-large", subSize > 1.15f);
        }
    }

    static void SetClass(VisualElement element, string className, bool on)
    {
        if (on) element.AddToClassList(className);
        else element.RemoveFromClassList(className);
    }

    // ── Aplicación global ─────────────────────────────────────────────────

    /// <summary>Lleva al juego **todos** los ajustes guardados.</summary>
    public static void ApplyAll()
    {
        ApplyAudio();
        ApplyVideo();
        ApplySensitivity();
        ApplyFog();
        ApplyEchoOpacity();
        ApplyExtraRecordTime();
        ApplyAccessibility();
        GameSettings.ApplyCurrentUIScale();
    }

    /// <summary>
    /// Devuelve los ajustes a fábrica **sin tocar el progreso de la partida**.
    /// El botón "restaurar" del menú llamaba a <c>PlayerPrefs.DeleteAll()</c>, que
    /// también borraba niveles desbloqueados y ecos anclados.
    /// </summary>
    public static void RestoreDefaults()
    {
        foreach (string key in AllKeys)
            PlayerPrefs.DeleteKey(key);

        PlayerPrefs.Save();
        ApplyAll();
    }

    public static void Save() => PlayerPrefs.Save();

    // ── Bootstrap ─────────────────────────────────────────────────────────

    static bool _hooked;

    /// <summary>
    /// Restaura los ajustes al arrancar y tras cada carga de escena. Sin esto, el
    /// menú aplicaba los cambios en caliente pero el juego volvía a los valores
    /// por defecto en el siguiente arranque: es la razón principal por la que los
    /// ajustes "no funcionaban de verdad".
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        ApplyVideo();
        ApplyAudio();

        if (_hooked) return;
        _hooked = true;
        SceneManager.sceneLoaded += (_, __) => ApplyAfterSceneLoad();
    }

    static void ApplyAfterSceneLoad()
    {
        // Un frame de margen: los bootstraps de nivel (luces, niebla, HUD) corren
        // en sus propios Awake/Start y pisarían lo que se aplique aquí.
        var runner = SettingsRunner.Ensure();
        if (runner != null) runner.ApplyNextFrame();
        else ApplyAll();
    }

    /// Helper MonoBehaviour: hace falta un objeto vivo para poder esperar un frame.
    class SettingsRunner : MonoBehaviour
    {
        static SettingsRunner _instance;

        public static SettingsRunner Ensure()
        {
            if (_instance != null) return _instance;
            if (!Application.isPlaying) return null;

            var go = new GameObject("EchoesSettingsRunner") { hideFlags = HideFlags.HideAndDontSave };
            _instance = go.AddComponent<SettingsRunner>();
            DontDestroyOnLoad(go);
            return _instance;
        }

        public void ApplyNextFrame()
        {
            StopAllCoroutines();
            StartCoroutine(ApplyRoutine());
        }

        System.Collections.IEnumerator ApplyRoutine()
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            ApplyAll();
        }
    }
}
