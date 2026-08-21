using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// MenuHoverSystem — Fase 6: Sistema de hover simplificado a 4 capas canónicas.
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

    [Tooltip("Avanzar diálogo / seleccionar opción")]
    [SerializeField] AudioClip dialogueAdvanceClip;

    [Tooltip("Interacción disponible (prompt)")]
    [SerializeField] AudioClip interactionAvailableClip;

    [Tooltip("Interacción denegada (error suave)")]
    [SerializeField] AudioClip interactionDeniedClip;

    [Tooltip("Cerrar pausa / menú")]
    [SerializeField] AudioClip pauseCloseClip;

    [Tooltip("Toast / notificación")]
    [SerializeField] AudioClip toastClip;

    [Header("Audio Settings")]
    [Range(0f, 1f)] [SerializeField] float hoverInVolume = 0.6f;
    [Range(0f, 1f)] [SerializeField] float hoverOutVolume = 0.3f;
    [Range(0f, 1f)] [SerializeField] float clickVolume = 0.8f;
    [Range(0f, 1f)] [SerializeField] float navMoveVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] float crtAmbientVolume = 0.08f;
    [Range(0f, 1f)] [SerializeField] float dialogueAdvanceVolume = 0.7f;
    [Range(0f, 1f)] [SerializeField] float interactionAvailableVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] float interactionDeniedVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] float pauseCloseVolume = 0.7f;
    [Range(0f, 1f)] [SerializeField] float toastVolume = 0.6f;

    [Header("Cursor")]
    [SerializeField] Texture2D menuCursorTexture;

    // ═══════════════════════════════════════════════════════════════
    // ESTADO INTERNO
    // ═══════════════════════════════════════════════════════════════

    VisualElement _root;
    AudioSource _uiAudioSource;
    AudioSource _crtAmbientSource;

    readonly Dictionary<Button, MenuButtonHoverState> _buttonStates = new();

    Button _btnContinue, _btnNewGame, _btnLevels, _btnChapters, _btnSettings, _btnCredits, _btnExit;

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
        _crtAmbientSource.playOnAwake = false;   // sin esto suena vacia antes de asignar el clip
        _crtAmbientSource.spatialBlend = 0f;
        _crtAmbientSource.loop = true;
        _crtAmbientSource.volume = 0f;
        _crtAmbientSource.priority = 128;

        // Auto-load missing clips from Resources/Audio/ui/
        if (hoverInClip == null) hoverInClip = Resources.Load<AudioClip>("Audio/ui/ui_dialogue_open");
        if (clickConfirmClip == null) clickConfirmClip = Resources.Load<AudioClip>("Audio/ui/ui_dialogue_advance");
        if (dialogueAdvanceClip == null) dialogueAdvanceClip = Resources.Load<AudioClip>("Audio/ui/ui_dialogue_advance");
        if (interactionAvailableClip == null) interactionAvailableClip = Resources.Load<AudioClip>("Audio/ui/ui_interaction_available");
        if (interactionDeniedClip == null) interactionDeniedClip = Resources.Load<AudioClip>("Audio/ui/ui_interaction_denied");
        if (pauseCloseClip == null) pauseCloseClip = Resources.Load<AudioClip>("Audio/ui/ui_pause_closemp3");
        if (toastClip == null) toastClip = Resources.Load<AudioClip>("Audio/ui/ui_toast");

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
        _btnContinue  = _root.Q<Button>("nav-continue");
        _btnNewGame   = _root.Q<Button>("nav-newgame");
        _btnLevels    = _root.Q<Button>("nav-levels");
        _btnChapters  = _root.Q<Button>("nav-chapters");
        _btnSettings  = _root.Q<Button>("nav-settings");
        _btnCredits   = _root.Q<Button>("nav-credits");
        _btnExit      = _root.Q<Button>("nav-exit");

        RegisterButton(_btnContinue, "CONTINUAR");
        RegisterButton(_btnNewGame,  "NUEVO EXPEDIENTE");
        RegisterButton(_btnLevels,   "ARCHIVOS");
        RegisterButton(_btnChapters, "SELECCIÓN DE CAPÍTULOS");
        RegisterButton(_btnSettings, "AJUSTES");
        RegisterButton(_btnCredits,  "CRÉDITOS");
        RegisterButton(_btnExit,     "SALIR");
    }

    // ═══════════════════════════════════════════════════════════════
    // REGISTRO Y SETUP DE BOTONES
    // ═══════════════════════════════════════════════════════════════

    void RegisterButton(Button btn, string label)
    {
        if (btn == null) return;

        // Añadir clase v2 al botón
        btn.AddToClassList("nav-item-v2");

        // Crear estado de hover para este botón
        var state = new MenuButtonHoverState(btn, this, label);
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
    // ══════════════════════════════════════════════════════════════

    void OnButtonMouseEnter(Button btn, MouseEnterEvent evt)
    {
        if (!_buttonStates.TryGetValue(btn, out var state)) return;

        // Determinar duración: si venimos de otro botón, la transición es más rápida
        float duration = (Time.unscaledTime - _lastHoverExitTime < 0.5f)
            ? 0.15f * 0.6f  // 90ms
            : 0.15f;        // 150ms (mouse)

        state.EnterHover(duration, isController: false);
        PlayHoverIn();
    }

    void OnButtonMouseLeave(Button btn, MouseLeaveEvent evt)
    {
        if (!_buttonStates.TryGetValue(btn, out var state)) return;

        state.ExitHover(0.15f);
        PlayHoverOut();
        _lastHoverExitTime = Time.unscaledTime;
    }

    void OnButtonClick(Button btn, ClickEvent evt)
    {
        if (!_buttonStates.TryGetValue(btn, out var state)) return;

        state.TriggerPress();
        PlayClick();
    }

    void OnButtonFocus(Button btn, FocusEvent evt)
    {
        if (!_buttonStates.TryGetValue(btn, out var state)) return;

        state.EnterHover(0.20f, isController: true); // 200ms controller
        PlayNavMove();
    }

    void OnButtonBlur(Button btn, BlurEvent evt)
    {
        if (!_buttonStates.TryGetValue(btn, out var state)) return;

        state.ExitHover(0.15f);
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
        _uiAudioSource.pitch = Random.Range(0.95f, 1.05f);
        _uiAudioSource.PlayOneShot(navMoveClip, navMoveVolume);
    }

    public void PlayDialogueAdvance()
    {
        if (dialogueAdvanceClip == null) return;
        _uiAudioSource.pitch = 1.05f;
        _uiAudioSource.PlayOneShot(dialogueAdvanceClip, dialogueAdvanceVolume);
    }

    public void PlayInteractionAvailable()
    {
        if (interactionAvailableClip == null) return;
        _uiAudioSource.pitch = 1.0f;
        _uiAudioSource.PlayOneShot(interactionAvailableClip, interactionAvailableVolume);
    }

    public void PlayInteractionDenied()
    {
        if (interactionDeniedClip == null) return;
        _uiAudioSource.pitch = 0.9f;
        _uiAudioSource.PlayOneShot(interactionDeniedClip, interactionDeniedVolume);
    }

    public void PlayPauseClose()
    {
        if (pauseCloseClip == null) return;
        _uiAudioSource.pitch = 1.0f;
        _uiAudioSource.PlayOneShot(pauseCloseClip, pauseCloseVolume);
    }

    public void PlayToast()
    {
        if (toastClip == null) return;
        _uiAudioSource.pitch = 1.0f;
        _uiAudioSource.PlayOneShot(toastClip, toastVolume);
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILIDADES
    // ═══════════════════════════════════════════════════════════════

    public Coroutine RunCoroutine(IEnumerator coroutine) => StartCoroutine(coroutine);
    public void StopRunningCoroutine(Coroutine coroutine) { if (coroutine != null) StopCoroutine(coroutine); }

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

/// <summary>
/// Estado de hover por botón — gestiona ghost layer + focus pip via CSS classes.
/// </summary>
public class MenuButtonHoverState
{
    readonly Button _btn;
    readonly MenuHoverSystem _system;
    readonly string _label;
    readonly VisualElement _ghostLayer;
    readonly VisualElement _focusPip;
    bool _isHovering;

    public MenuButtonHoverState(Button btn, MenuHoverSystem system, string label)
    {
        _btn = btn;
        _system = system;
        _label = !string.IsNullOrEmpty(btn.text) ? btn.text : label;

        // Crear ghost layer (1 solo) — usar Label para texto
        _ghostLayer = new Label(_label);
        _ghostLayer.AddToClassList("nav-item-ghost");
        _btn.Add(_ghostLayer);

        // Crear focus pip — usar Label para el símbolo ▸
        _focusPip = new Label("▸");
        _focusPip.AddToClassList("nav-item-focus-pip");
        _btn.Add(_focusPip);
    }

    public void EnterHover(float duration, bool isController)
    {
        if (_isHovering) return;
        _isHovering = true;

        // Ghost layer visible
        _ghostLayer.AddToClassList("nav-item-ghost--visible");

        // Focus pip si es controller
        if (isController)
            _focusPip.AddToClassList("nav-item-focus-pip--visible");
    }

    public void ExitHover(float duration)
    {
        if (!_isHovering) return;
        _isHovering = false;

        _ghostLayer.RemoveFromClassList("nav-item-ghost--visible");
        _focusPip.RemoveFromClassList("nav-item-focus-pip--visible");
    }

    public void TriggerPress()
    {
        // El estado :active se maneja por CSS
    }

    public void Cleanup()
    {
        _ghostLayer?.RemoveFromHierarchy();
        _focusPip?.RemoveFromHierarchy();
    }
}
