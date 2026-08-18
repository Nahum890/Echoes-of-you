using UnityEngine;
using UInput = UnityEngine.Input;

namespace Echoes
{
    /// <summary>
    /// InputActionMap — Wrapper unificado de input para la UI Toolkit de Echoes of You.
    ///
    /// NO usa el package com.unity.inputsystem (no instalado en el proyecto).
    /// Trabaja sobre UnityEngine.Input legacy (Input Manager) exponiendo una API
    /// consistente que la UI consume sin saber si el origen es teclado o gamepad.
    ///
    /// Acciones canónicas (plan F0.4):
    ///   - Navigate       (Value<Vector2>) : WASD / Arrows / LeftStick / D-Pad
    ///   - Submit         (Button)        : Enter / Space / Gamepad South (A)
    ///   - Cancel         (Button)        : Escape / Gamepad East  (B)
    ///   - Pause          (Button)        : Esc (UI) / Start        (gamepad)
    ///   - Record         (Button, Hold)  : R / Gamepad West (Y)
    ///   - Playback       (Button)        : E / Gamepad North (X)
    ///   - SoftReset      (Button, Hold)  : Backspace / Gamepad L3 (Left Stick press)
    ///
    /// Propiedades de polling (lerp del frame actual):
    ///   - NavigationValue   (Vector2, -1..1 en cada eje)
    ///   - SubmitPressed     (bool, true solo en el frame Down)
    ///   - CancelPressed
    ///   - PausePressed
    ///   - RecordHeld        (bool, true mientras se mantenga)
    ///   - PlaybackPressed
    ///   - SoftResetHeld
    ///
    /// Singleton con AutoLoad vía [DefaultExecutionOrder(-1000)] para que exista
    /// antes que cualquier controller. Persiste entre escenas.
    ///
    /// Gameplay legacy (PlayerController, EchoRecorder) no se migra y sigue usando
    /// UnityEngine.Input.GetKey legacy por su cuenta. La UI (NavigationManager, FocusManager,
    /// MenuHoverSystem simplificado en Fase 6) consume exclusivamente este wrapper.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class InputActionMap : MonoBehaviour
    {
        // ============================================================
        // SINGLETON
        // ============================================================
        public static InputActionMap Instance { get; private set; }

        // ============================================================
        // CONFIGURACIÓN PÚBLICA (editable en Inspector)
        // ============================================================
        [Header("Keyboard Bindings")]
        [SerializeField] KeyCode submitKey       = KeyCode.Return;
        [SerializeField] KeyCode submitAltKey   = KeyCode.Space;
        [SerializeField] KeyCode cancelKey      = KeyCode.Escape;
        [SerializeField] KeyCode pauseKey       = KeyCode.Escape;
        [SerializeField] KeyCode pauseAltKey   = KeyCode.JoystickButton7;  // Start
        [SerializeField] KeyCode recordKey     = KeyCode.R;
        [SerializeField] KeyCode playbackKey    = KeyCode.E;
        // SoftReset has been removed in 2.0 (hard reset via Hold T is canonical)

        [Header("Gamepad Bindings (Unity Input Manager)")]
        [Tooltip("Eje horizontal del Input Manager (menu lateral / D-Pad X).")]
        [SerializeField] string gamepadHorizontalAxis = "Horizontal";
        [Tooltip("Eje vertical del Input Manager (navegación vertical / D-Pad Y).")]
        [SerializeField] string gamepadVerticalAxis   = "Vertical";
        [Tooltip("Eje horizontal del Left Stick (si distinto al D-Pad).")]
        [SerializeField] string leftStickXAxis       = "ControllerLeftStickX";
        [Tooltip("Eje vertical del Left Stick (si distinto al D-Pad).")]
        [SerializeField] string leftStickYAxis       = "ControllerLeftStickY";

        [Tooltip("Gamepad buttons (KeyCode enum de Unity).")]
        [SerializeField] KeyCode gamepadSubmit      = KeyCode.JoystickButton0; // A / South
        [SerializeField] KeyCode gamepadCancel       = KeyCode.JoystickButton1; // B / East
        [SerializeField] KeyCode gamepadRecord      = KeyCode.JoystickButton3; // Y / West
        [SerializeField] KeyCode gamepadPlayback    = KeyCode.JoystickButton2; // X / North

        [Header("Deadzone (Gamepad axes)")]
        [Range(0.05f, 0.4f)] [SerializeField] float axisDeadzone = 0.18f;

        [Header("Repeat Rate (UI navigation held)")]
        [Tooltip("Segundos entre repeticiones cuando el eje se mantiene pulsado.")]
        [SerializeField] float initialRepeatDelay = 0.40f;
        [SerializeField] float repeatRate          = 0.12f;

        // ============================================================
        // ESTADO PRIVADO
        // ============================================================

        // Poll del frame actual (valores normalizados y singulares)
        Vector2 _navigateRaw;
        Vector2 _navigateDiscrete;   // -1/0/+1 por eje con repeat-rate
        bool _submitDown;
        bool _cancelDown;
        bool _pauseDown;
        bool _recordHeld;
        bool _playbackDown;

        // Repetición de navegación keyboard/gamepad (d-pad)
        Vector2Int _lastDir = Vector2Int.zero;
        float _nextRepeatAt = -1f;

        // Detección de gamepad conectado (para elegir timing hover en F6)
        bool _gamepadConnected;
        float _gamepadCheckAt = -1f;

        // ============================================================
        // CICLO DE VIDA
        // ============================================================

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            // Persistencia: este wrapper debe sobrevivir cargas de escena para no
            // perder bindings en MainMenu → Level → Pause round-trips.
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            PollNavigation();
            PollButtons();
            PollGamepadPresence();
        }

        // ============================================================
        // API PÚBLICA (que consume la UI)
        // ============================================================

        /// <summary>Vector2 de navegación, -1..1 en cada eje. Discrete-grid (con repeat rate para mantener pulsado).</summary>
        public Vector2 NavigationValue => _navigateDiscrete;

        /// <summary>True solo en el frame en el que se pulsó Submit (Enter/Space/Gamepad-South).</summary>
        public bool SubmitPressed   => _submitDown;

        /// <summary>True solo en el frame en el que se pulsó Cancel (Esc/Gamepad-East).</summary>
        public bool CancelPressed   => _cancelDown;

        /// <summary>True solo en el frame en el que se pulsó Pause (Esc/Start).</summary>
        public bool PausePressed    => _pauseDown;

        /// <summary>True mientras se mantenga Record pulsado (R/Gamepad-West).</summary>
        public bool RecordHeld      => _recordHeld;

        /// <summary>True solo en el frame en el que se pulsó Playback (E/Gamepad-North).</summary>
        public bool PlaybackPressed => _playbackDown;

        /// <summary>True mientras se mantenga SoftReset pulsado (Desactivado en 2.0).</summary>
        public bool SoftResetHeld   => false;

        /// <summary>True si el último input efectivo vino de gamepad (false = keyboard/mouse).</summary>
        public bool LastInputIsGamepad => _gamepadConnected && Time.time - _gamepadCheckAt < 0.5f;

        /// <summary>True si hay un gamepad conectado (refrescado cada ~1s).</summary>
        public bool GamepadConnected => _gamepadConnected;

        // ============================================================
        // POLL INTERNO
        // ============================================================

        void PollNavigation()
        {
            // 1. Keyboard arrows / WASD — pulso discreto
            int kbX = 0, kbY = 0;
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow))  kbX -= 1;
            if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow)) kbX += 1;
            if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow))  kbY -= 1;
            if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow))    kbY += 1;

            // 2. Gamepad: D-Pad legacy (Input Manager axis) + Left Stick
            int padX = 0, padY = 0;
            float padH = SafeAxis(gamepadHorizontalAxis);
            float padV = SafeAxis(gamepadVerticalAxis);
            if (Mathf.Abs(padH) > axisDeadzone) padX = padH > 0 ? 1 : -1;
            if (Mathf.Abs(padV) > axisDeadzone) padY = padV > 0 ? 1 : -1;

            // Si el D-Pad no responde, miramos el Left Stick ( bé came con el plan)
            if (padX == 0)
            {
                float lsX = SafeAxis(leftStickXAxis);
                if (Mathf.Abs(lsX) > axisDeadzone) padX = lsX > 0 ? 1 : -1;
            }
            if (padY == 0)
            {
                float lsY = SafeAxis(leftStickYAxis);
                if (Mathf.Abs(lsY) > axisDeadzone) padY = lsY > 0 ? 1 : -1;
            }

            int totalX = kbX != 0 ? kbX : padX;
            int totalY = kbY != 0 ? kbY : padY;

            // 3. Repeat-rate similar a Unity UI por defecto: si se mantiene la dirección,
            // disparamos evento de navegación a initialRepeatDelay, luego a repeatRate.
            float now = Time.unscaledTime;
            Vector2Int currentDir = new Vector2Int(totalX, totalY);
            bool push = false;

            if (currentDir != Vector2Int.zero)
            {
                if (currentDir != _lastDir)
                {
                    // Nueva dirección: pulso inmediato
                    push = true;
                    _nextRepeatAt = now + initialRepeatDelay;
                }
                else if (now >= _nextRepeatAt)
                {
                    // Repetición
                    push = true;
                    _nextRepeatAt = now + repeatRate;
                }
            }
            else
            {
                _nextRepeatAt = -1f;
            }

            _lastDir = currentDir;

            if (push)
            {
                _navigateDiscrete = new Vector2(currentDir.x, currentDir.y);
                _navigateRaw      = _navigateDiscrete;
            }
            else
            {
                _navigateDiscrete = Vector2.zero;
                // Raw siguesi es necesario para gamepad: se mantenga el valor real del eje
                // _navigateRaw se usa internamente solo para debug.
                _navigateRaw = new Vector2(SafeAxis(gamepadHorizontalAxis), SafeAxis(gamepadVerticalAxis));
            }
        }

        void PollButtons()
        {
            // Botones usemos GetKeyDown (.edge) y GetKey (held) del legacy.
            _submitDown     = UnityEngine.Input.GetKeyDown(submitKey) || UnityEngine.Input.GetKeyDown(submitAltKey) || UnityEngine.Input.GetKeyDown(gamepadSubmit);
            _cancelDown     = UnityEngine.Input.GetKeyDown(cancelKey) || UnityEngine.Input.GetKeyDown(gamepadCancel);
            _pauseDown      = UnityEngine.Input.GetKeyDown(pauseKey)   || UnityEngine.Input.GetKeyDown(pauseAltKey);
            _recordHeld     = UnityEngine.Input.GetKey(recordKey) || UnityEngine.Input.GetKey(gamepadRecord);
            _playbackDown   = UnityEngine.Input.GetKeyDown(playbackKey) || UnityEngine.Input.GetKeyDown(gamepadPlayback);
        }

        void PollGamepadPresence()
        {
            // Poll ligero (cada ~1s) para detectar joystick conectado.
            if (Time.unscaledTime - _gamepadCheckAt < 1f) return;
            _gamepadCheckAt = Time.unscaledTime;

            bool anyJoystick = false;
            string[] joys = UnityEngine.Input.GetJoystickNames();
            for (int i = 0; i < joys.Length; i++)
            {
                if (!string.IsNullOrEmpty(joys[i])) { anyJoystick = true; break; }
            }
            // También consideramos "conectado" si cualquier botón/eje gamepad tienes actividad.
            if (!anyJoystick)
            {
                if (Mathf.Abs(SafeAxis(gamepadHorizontalAxis)) > 0.01f ||
                    Mathf.Abs(SafeAxis(gamepadVerticalAxis))   > 0.01f ||
                    Mathf.Abs(SafeAxis(leftStickXAxis))          > 0.01f ||
                    Mathf.Abs(SafeAxis(leftStickYAxis))          > 0.01f)
                {
                    anyJoystick = true;
                }
            }
            _gamepadConnected = anyJoystick;
        }

        // ============================================================
        // UTILIDADES
        // ============================================================

        /// <summary>Lee un eje de Input Manager de forma segura (string vacío → 0).</summary>
        float SafeAxis(string axisName)
        {
            if (string.IsNullOrEmpty(axisName)) return 0f;
            try { return UnityEngine.Input.GetAxis(axisName); }
            catch { return 0f; } // Eje no definido en Input Manager
        }

        /// <summary>Forzar la detección de gamepad (ej. al conectar manualmente un mando).</summary>
        public void RefreshGamepadPresence() => _gamepadCheckAt = -1f;
    }
}
