using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Sistema completo de hover icónico para Echoes of You.
/// Implementa 14 capas de efectos: escala, movimiento, ghosting, aberración cromática,
/// distorsión analógica, blur simulado, vibración, audio, partículas, 
/// cambio de fondo, iluminación, ruido, profundidad y eco visual.
///
/// Inspirado en menús PS2, cintas VHS, monitores CRT y grabaciones deterioradas.
/// Estilo: elegante, minimalista, nostálgico. Sin terror. Sin glitch exagerado.
///
/// Cada transición dura entre 150 y 300ms con curvas de easing diseñadas.
/// </summary>
public class MenuHoverSystem : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // CONFIGURACIÓN
    // ═══════════════════════════════════════════════════════════════

    [Header("UI Document")]
    [SerializeField] UIDocument uiDocument;

    [Header("Audio Clips")]
    [Tooltip("Clic analógico suave al entrar en hover")]
    [SerializeField] AudioClip hoverInClip;

    [Tooltip("Tono descendente al salir del hover")]
    [SerializeField] AudioClip hoverOutClip;

    [Tooltip("Click de confirmación + tono cálido")]
    [SerializeField] AudioClip clickConfirmClip;

    [Tooltip("Tick seco para navegación con controller")]
    [SerializeField] AudioClip navMoveClip;

    [Tooltip("Zumbido CRT de fondo — loop")]
    [SerializeField] AudioClip crtAmbientClip;

    [Header("Audio Settings")]
    [Range(0f, 1f)] [SerializeField] float hoverInVolume = 0.6f;
    [Range(0f, 1f)] [SerializeField] float hoverOutVolume = 0.3f;
    [Range(0f, 1f)] [SerializeField] float clickVolume = 0.8f;
    [Range(0f, 1f)] [SerializeField] float navMoveVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] float crtAmbientVolume = 0.08f;

    [Header("Hover Timing")]
    [Tooltip("Duración de transición hover para mouse (segundos)")]
    [SerializeField] float hoverEnterDurationMouse = 0.20f;

    [Tooltip("Duración de transición hover para controller (segundos)")]
    [SerializeField] float hoverEnterDurationController = 0.25f;

    [Tooltip("Duración de transición hover leave")]
    [SerializeField] float hoverLeaveDuration = 0.15f;

    [Header("Effect Intensities")]
    [Range(0f, 1f)] [SerializeField] float noiseBaseOpacity = 0.06f;
    [Range(0f, 1f)] [SerializeField] float ghostLayerAOpacity = 0.18f;
    [Range(0f, 1f)] [SerializeField] float ghostLayerBOpacity = 0.09f;
    [Range(0f, 2f)] [SerializeField] float particleSpeed = 1f;

    [Header("Cursor")]
    [SerializeField] Texture2D menuCursorTexture;

    // ═══════════════════════════════════════════════════════════════
    // ESTADO INTERNO
    // ═══════════════════════════════════════════════════════════════

    VisualElement _root;
    AudioSource _uiAudioSource;
    AudioSource _crtAmbientSource;

    // Registro de botones y sus estados
    readonly Dictionary<Button, MenuButtonHoverState> _buttonStates = new();

    // Botones del menú principal
    Button _btnNewGame, _btnLevels, _btnSettings, _btnExit;

    // Panel tint overlay (temperatura de color)
    VisualElement _panelTintOverlay;
    VisualElement _rightPanelCrtBlink;

    // Gradient del side nav
    VisualElement _sideNavGradientOverlay;

    // Cursor anterior (para restaurar al salir del menú)
    Texture2D _previousCursor;

#pragma warning disable CS0414
    bool _isControllerNavigation = false;
#pragma warning restore CS0414
#pragma warning disable CS0414
    bool _wasPreviouslyHovering = false;
#pragma warning restore CS0414
    float _lastHoverExitTime = -1f;

    // ═══════════════════════════════════════════════════════════════
    // INICIALIZACIÓN
    // ═══════════════════════════════════════════════════════════════

    void Awake()
    {
        // Crear AudioSource para UI (2D, sin posición)
        _uiAudioSource = gameObject.AddComponent<AudioSource>();
        _uiAudioSource.spatialBlend = 0f;
        _uiAudioSource.playOnAwake = false;
        _uiAudioSource.priority = 64;

        // AudioSource para el CRT ambient hum
        _crtAmbientSource = gameObject.AddComponent<AudioSource>();
        _crtAmbientSource.spatialBlend = 0f;
        _crtAmbientSource.loop = true;
        _crtAmbientSource.volume = 0f;
        _crtAmbientSource.priority = 128;

        if (crtAmbientClip != null)
        {
            _crtAmbientSource.clip = crtAmbientClip;
            _crtAmbientSource.Play();
            StartCoroutine(FadeAudioSource(_crtAmbientSource, 0f, crtAmbientVolume, 2.0f));
        }
    }

    void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        _root = uiDocument.rootVisualElement;
        if (_root == null) return;

        InitializeButtons();
        InitializeOverlays();

        // Registrar cursor custom
        if (menuCursorTexture != null)
            UnityEngine.Cursor.SetCursor(menuCursorTexture, new Vector2(8, 8), CursorMode.Auto);
    }

    void OnDisable()
    {
        // Restaurar cursor por defecto
        UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        // Limpiar todos los estados
        foreach (var kvp in _buttonStates)
            kvp.Value.Cleanup();
        _buttonStates.Clear();
    }

    void InitializeButtons()
    {
        _btnNewGame  = _root.Q<Button>("nav-newgame");
        _btnLevels   = _root.Q<Button>("nav-levels");
        _btnSettings = _root.Q<Button>("nav-settings");
        _btnExit     = _root.Q<Button>("nav-exit");

        RegisterButton(_btnNewGame,  "INICIAR RECUERDO",  PanelTintType.Warm);
        RegisterButton(_btnLevels,   "ARCHIVOS",          PanelTintType.Neutral);
        RegisterButton(_btnSettings, "CALIBRAR",          PanelTintType.Cool);
        RegisterButton(_btnExit,     "DESCONECTAR",       PanelTintType.Cold);
    }

    void InitializeOverlays()
    {
        // Panel tint overlay — temperatura de color del panel derecho
        _panelTintOverlay = _root.Q("panel-tint-overlay");

        // CRT blink overlay
        _rightPanelCrtBlink = _root.Q("right-panel-crt-blink");

        // Side nav gradient
        _sideNavGradientOverlay = _root.Q("side-nav-gradient-overlay");
    }

    // ═══════════════════════════════════════════════════════════════
    // REGISTRO Y SETUP DE BOTONES
    // ═══════════════════════════════════════════════════════════════

    void RegisterButton(Button btn, string label, PanelTintType tintType)
    {
        if (btn == null) return;

        // Añadir clase v2 al botón (reemplaza nav-item clásico)
        btn.AddToClassList("nav-item-v2");

        // Crear estado de hover para este botón
        var state = new MenuButtonHoverState(btn, this, label, tintType);
        _buttonStates[btn] = state;

        // Registrar eventos de mouse
        btn.RegisterCallback<MouseEnterEvent>(evt => OnButtonMouseEnter(btn, evt));
        btn.RegisterCallback<MouseLeaveEvent>(evt => OnButtonMouseLeave(btn, evt));
        btn.RegisterCallback<ClickEvent>(evt => OnButtonClick(btn, evt));

        // Registrar eventos de teclado/controller (focus)
        btn.RegisterCallback<FocusEvent>(evt => OnButtonFocus(btn, evt));
        btn.RegisterCallback<BlurEvent>(evt => OnButtonBlur(btn, evt));
    }

    // ═══════════════════════════════════════════════════════════════
    // CALLBACKS DE EVENTOS
    // ═══════════════════════════════════════════════════════════════

    void OnButtonMouseEnter(Button btn, MouseEnterEvent evt)
    {
        if (!_buttonStates.TryGetValue(btn, out var state)) return;

        _isControllerNavigation = false;

        // Determinar duración: si venimos de otro botón, la transición es más rápida
        float duration = (Time.unscaledTime - _lastHoverExitTime < 0.5f)
            ? hoverEnterDurationMouse * 0.6f
            : hoverEnterDurationMouse;

        state.EnterHover(duration, isController: false);
        PlayHoverIn();
        TriggerPanelEffects(btn);
        _wasPreviouslyHovering = true;
    }

    void OnButtonMouseLeave(Button btn, MouseLeaveEvent evt)
    {
        if (!_buttonStates.TryGetValue(btn, out var state)) return;

        state.ExitHover(hoverLeaveDuration);
        PlayHoverOut();
        _lastHoverExitTime = Time.unscaledTime;
    }

    void OnButtonClick(Button btn, ClickEvent evt)
    {
        if (!_buttonStates.TryGetValue(btn, out var state)) return;

        state.TriggerPress();
        PlayClick();
        StartCoroutine(CRTBlinkOnClick());
    }

    void OnButtonFocus(Button btn, FocusEvent evt)
    {
        if (!_buttonStates.TryGetValue(btn, out var state)) return;

        _isControllerNavigation = true;
        state.EnterHover(hoverEnterDurationController, isController: true);
        PlayNavMove();
        TriggerPanelEffects(btn);
    }

    void OnButtonBlur(Button btn, BlurEvent evt)
    {
        if (!_buttonStates.TryGetValue(btn, out var state)) return;

        state.ExitHover(hoverLeaveDuration);
    }

    // ═══════════════════════════════════════════════════════════════
    // EFECTOS DE PANEL (FONDO Y TEMPERATURA DE COLOR)
    // ═══════════════════════════════════════════════════════════════

    void TriggerPanelEffects(Button btn)
    {
        if (!_buttonStates.TryGetValue(btn, out var state)) return;

        // CRT blink — parpadeo del panel derecho
        StartCoroutine(CRTBlinkSequence());

        // Temperatura de color
        UpdatePanelTint(state.TintType);

        // Gradiente del side nav
        UpdateSideNavGradient(btn);
    }

    void UpdatePanelTint(PanelTintType tint)
    {
        if (_panelTintOverlay == null) return;

        _panelTintOverlay.RemoveFromClassList("panel-tint-warm");
        _panelTintOverlay.RemoveFromClassList("panel-tint-neutral");
        _panelTintOverlay.RemoveFromClassList("panel-tint-cool");
        _panelTintOverlay.RemoveFromClassList("panel-tint-cold");

        string tintClass = tint switch
        {
            PanelTintType.Warm    => "panel-tint-warm",
            PanelTintType.Neutral => "panel-tint-neutral",
            PanelTintType.Cool    => "panel-tint-cool",
            PanelTintType.Cold    => "panel-tint-cold",
            _                     => "panel-tint-neutral"
        };

        _panelTintOverlay.AddToClassList(tintClass);
    }

    void UpdateSideNavGradient(Button btn)
    {
        if (_sideNavGradientOverlay == null) return;

        _sideNavGradientOverlay.RemoveFromClassList("side-nav-gradient-newgame");
        _sideNavGradientOverlay.RemoveFromClassList("side-nav-gradient-levels");
        _sideNavGradientOverlay.RemoveFromClassList("side-nav-gradient-settings");
        _sideNavGradientOverlay.RemoveFromClassList("side-nav-gradient-exit");

        string gradClass =
            btn == _btnNewGame  ? "side-nav-gradient-newgame"  :
            btn == _btnLevels   ? "side-nav-gradient-levels"   :
            btn == _btnSettings ? "side-nav-gradient-settings" :
            btn == _btnExit     ? "side-nav-gradient-exit"     :
                                  "side-nav-gradient-newgame";

        _sideNavGradientOverlay.AddToClassList(gradClass);
    }

    // ═══════════════════════════════════════════════════════════════
    // CORRUTINAS DE EFECTOS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Parpadeo CRT al cambiar de ambience.</summary>
    IEnumerator CRTBlinkSequence()
    {
        if (_rightPanelCrtBlink == null) yield break;

        _rightPanelCrtBlink.AddToClassList("right-panel-crt-blink--flash");
        yield return new WaitForSecondsRealtime(0.05f);
        _rightPanelCrtBlink.RemoveFromClassList("right-panel-crt-blink--flash");
    }

    /// <summary>Parpadeo CRT más intenso al hacer click.</summary>
    IEnumerator CRTBlinkOnClick()
    {
        if (_rightPanelCrtBlink == null) yield break;

        _rightPanelCrtBlink.AddToClassList("right-panel-crt-blink--flash");
        yield return new WaitForSecondsRealtime(0.06f);
        _rightPanelCrtBlink.RemoveFromClassList("right-panel-crt-blink--flash");
        yield return new WaitForSecondsRealtime(0.04f);
        _rightPanelCrtBlink.AddToClassList("right-panel-crt-blink--flash");
        yield return new WaitForSecondsRealtime(0.03f);
        _rightPanelCrtBlink.RemoveFromClassList("right-panel-crt-blink--flash");
    }

    // ═══════════════════════════════════════════════════════════════
    // AUDIO
    // ═══════════════════════════════════════════════════════════════

    void PlayHoverIn()
    {
        if (hoverInClip == null) return;
        _uiAudioSource.pitch = 1.0f;
        _uiAudioSource.PlayOneShot(hoverInClip, hoverInVolume);
    }

    void PlayHoverOut()
    {
        if (hoverOutClip == null) return;
        _uiAudioSource.pitch = 0.85f;
        _uiAudioSource.PlayOneShot(hoverOutClip, hoverOutVolume);
    }

    void PlayClick()
    {
        if (clickConfirmClip == null) return;
        _uiAudioSource.pitch = 1.1f;
        _uiAudioSource.PlayOneShot(clickConfirmClip, clickVolume);
    }

    void PlayNavMove()
    {
        if (navMoveClip == null) return;
        // Pitch ligeramente aleatorio para variedad orgánica
        _uiAudioSource.pitch = Random.Range(0.95f, 1.05f);
        _uiAudioSource.PlayOneShot(navMoveClip, navMoveVolume);
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILIDADES
    // ═══════════════════════════════════════════════════════════════

    public Coroutine RunCoroutine(IEnumerator coroutine) => StartCoroutine(coroutine);
    public void StopRunningCoroutine(Coroutine coroutine) { if (coroutine != null) StopCoroutine(coroutine); }

    public float NoiseBaseOpacity => noiseBaseOpacity;
    public float GhostLayerAOpacity => ghostLayerAOpacity;
    public float GhostLayerBOpacity => ghostLayerBOpacity;
    public float ParticleSpeed => particleSpeed;

    static IEnumerator FadeAudioSource(AudioSource source, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        source.volume = to;
    }
}

/// <summary>Tipos de temperatura de color para el panel derecho.</summary>
public enum PanelTintType
{
    Warm,    // nav-newgame — ámbar
    Neutral, // nav-levels — sage
    Cool,    // nav-settings — azul frío
    Cold     // nav-exit — muy frío
}
