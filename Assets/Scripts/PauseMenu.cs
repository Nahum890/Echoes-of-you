using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Echoes.UI
{
    /// <summary>
    /// PauseMenu — Rewrite Fase 3.
    /// Notebook 460px, stats, nav 4 botones.
    /// Escape → Reanudar (focus por defecto) en 1 tecla.
    /// Delegación: Settings → SettingsController (stub hasta Fase 5).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class PauseMenu : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] string hubSceneName = "MainMenu";
        [SerializeField] string mainMenuScene = "MainMenu";

        // Color constants for inline styling (bypass USS cascade issues)
        static readonly Color Amber = new Color(1f, 0.749f, 0f);       // #FFBF00
        static readonly Color Paper = new Color(0.957f, 0.949f, 0.933f); // #F4F2EE
        static readonly Color VoidBlack = new Color(0.039f, 0.039f, 0.051f); // #0A0A0D
        static readonly Color DarkBg = new Color(0.012f, 0.016f, 0.024f); // #030406
        static readonly Color Fluorescent = new Color(0.788f, 0.831f, 0.69f); // #C9D4B0
        static readonly Color Faded = new Color(0.353f, 0.29f, 0.18f);   // #5A4A2E
        static readonly Color DestructiveRed = new Color(0.698f, 0.227f, 0.227f); // #B23A3A
        static readonly Color BorderAmber = new Color(1f, 0.749f, 0f, 0.2f); // Amber 20%

        bool _paused;
        UIDocument _doc;
        VisualElement _root;
        VisualElement _pauseRoot;
        VisualElement _pauseNav;
        VisualElement _settingsPanel;

        Button _btnResume;
        Button _btnReiniciar;
        Button _btnSettings;
        Button _btnHub;

        void OnEnable()
        {
            InitializeUI();
        }

        void InitializeUI()
        {
            if (_paused) return; // ya inicializado

            _doc = GetComponent<UIDocument>();
            if (_doc == null || _doc.rootVisualElement == null) return;

            _root = _doc.rootVisualElement;

            // Query elements BEFORE loading stylesheets (needed for inline styles)
            _pauseRoot = _root.Q("pause-root");
            if (_pauseRoot == null) return;

            _pauseNav = _root.Q("pause-nav");
            _settingsPanel = _root.Q("pause-settings-panel");

            // Botones principales
            _btnResume    = _pauseRoot.Q<Button>("btn-resume");
            _btnReiniciar = _pauseRoot.Q<Button>("btn-reiniciar");
            _btnSettings  = _pauseRoot.Q<Button>("btn-settings");
            _btnHub       = _pauseRoot.Q<Button>("btn-hub");

            // Load stylesheets for editor play mode
            LoadStyleSheets();

            // IMMEDIATELY apply inline styles to override USS cascade
            ApplyInlineStyles();
            _settingsPanel?.AddToClassList("hidden");

            // Start hidden
            _pauseRoot.AddToClassList("hidden");
        }

        void Update()
        {
            if (!_paused && Input.GetKeyDown(KeyCode.Escape))
            {
                Pause();
            }
            else if (_paused && Input.GetKeyDown(KeyCode.Escape))
            {
                // Si settings abierto, cerrar y volver a nav
                if (_settingsPanel != null && !_settingsPanel.ClassListContains("hidden"))
                {
                    HideSettings();
                }
                else
                {
                    Resume();
                }
            }
        }

        void Pause()
        {
            _paused = true;
            Time.timeScale = 0f;
            UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None;
            UnityEngine.Cursor.visible = true;

            // Ocultar HUD gameplay
            var gameHUD = FindAnyObjectByType<GameHUD>();
            gameHUD?.SetVisible(false);

            InitializeUI();
            _pauseRoot?.RemoveFromClassList("hidden");
            _settingsPanel?.AddToClassList("hidden");
            _pauseNav?.RemoveFromClassList("hidden");
            RefreshStats();

            // Focus en Reanudar
            _btnResume?.Focus();
        }

        void Resume()
        {
            _paused = false;
            Time.timeScale = 1f;
            UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;

            // Mostrar HUD gameplay
            var gameHUD2 = FindAnyObjectByType<GameHUD>();
            gameHUD2?.SetVisible(true);

            _pauseRoot?.AddToClassList("hidden");
        }

        void ConfirmReiniciar()
        {
            ModalManager.Instance?.ShowModal(
                "Reiniciar Capítulo",
                "Se perderá el progreso del capítulo actual.",
                onConfirm: () =>
                {
                    Resume();
                    PostProcessingSetup.PrepareForSceneReload();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                },
                onCancel: () => { }
            );
        }

        void ConfirmHub()
        {
            ModalManager.Instance?.ShowModal(
                "Volver al Cuaderno",
                "¿Deseas salir al Cuaderno de Aiden? El progreso se guardará.",
                onConfirm: () =>
                {
                    UnpauseForMenu();
                    PostProcessingSetup.PrepareForSceneReload();
                    SceneManager.LoadScene(hubSceneName);
                },
                onCancel: () => { }
            );
        }

void ShowSettings()
        {
            _pauseNav?.AddToClassList("hidden");
            _settingsPanel?.RemoveFromClassList("hidden");
            SettingsController.Instance?.ShowInContainer(_settingsPanel);
        }

        void HideSettings()
        {
            _settingsPanel?.AddToClassList("hidden");
            _pauseNav?.RemoveFromClassList("hidden");
            _btnResume?.Focus();
        }

        void UnpauseForMenu()
        {
            _paused = false;
            Time.timeScale = 1f;
            UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            _pauseRoot?.AddToClassList("hidden");
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        void RefreshStats()
        {
            if (_doc == null || _doc.rootVisualElement == null) return;

            string sceneName = SceneManager.GetActiveScene().name;
            int levelIndex = GameProgress.GetSceneIndex(sceneName);
            bool isLevel = levelIndex >= 0;

            string fragmentLine = isLevel
                ? $"Recuerdo: {GameProgress.GetLevelDisplayName(sceneName)}"
                : $"Zona: {sceneName}";

            SetLabel("lbl-pause-fragment", fragmentLine);

            LevelRuntimeController runtime = LevelRuntimeController.Instance;
            float sessionTime = runtime != null ? runtime.SessionPlaySeconds : 0f;
            int sessionEchoes = runtime != null ? runtime.SessionEchoes : 0;
            int sessionDeaths = runtime != null ? runtime.SessionDeaths : 0;

            SetLabel("lbl-pause-time", $"Tiempo: {GameProgress.FormatPlayTime(sessionTime)}");
            SetLabel("lbl-pause-echoes", $"Ecos grabados: {sessionEchoes}");
            SetLabel("lbl-pause-deaths", isLevel
                ? $"Colapsos (aula): {GameProgress.GetSceneDeathCount(sceneName)} · sesión {sessionDeaths}"
                : $"Colapsos (sesión): {sessionDeaths}");

            int completed = GameProgress.GetCompletedCount();
            SetLabel("lbl-pause-total",
                $"Cuaderno de Aiden: {completed}/{GameProgress.TotalLevels} · {GameProgress.GetTotalEchoesCreated()} ecos · {GameProgress.FormatPlayTime(GameProgress.GetTotalPlayTimeSeconds())}");
        }

        void SetLabel(string elementName, string text)
        {
            var lbl = _doc.rootVisualElement.Q<Label>(elementName);
            if (lbl != null) lbl.text = text;
        }

        void LoadStyleSheets()
        {
            var paths = new[]
            {
                "Assets/UI/EchoesTheme.tss",
                "Assets/UI/PauseMenuUI.uss",
                "Assets/UI/Components/EchoButton.uss",
                "Assets/UI/Components/EchoPanel.uss",
                "Assets/UI/Components/EchoSlider.uss",
                "Assets/UI/Components/EchoToggle.uss",
                "Assets/UI/Components/EchoDropdown.uss",
                "Assets/UI/Components/EchoTabs.uss",
                "Assets/UI/Components/EchoModal.uss",
            };

            foreach (var path in paths)
            {
                StyleSheet ss = null;
#if UNITY_EDITOR
                ss = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
#else
                string resourcePath = path.Replace("Assets/Resources/", "").Replace(".uss", "").Replace(".tss", "");
                ss = Resources.Load<StyleSheet>(resourcePath);
#endif
                if (ss != null)
                {
                    _root.styleSheets.Add(ss);
                    Debug.Log("[PauseMenu] Loaded stylesheet: " + path);
                }
            }
        }

        void ApplyInlineStyles()
        {
            if (_pauseRoot == null || _pauseNav == null) return;

            // pause-root (fullscreen background)
            _pauseRoot.style.backgroundColor = new Color(0.039f, 0.039f, 0.051f, 0.92f); // #0A0A0D 92%
            _pauseRoot.style.color = Paper;
            _pauseRoot.style.fontSize = 30;

            // pause-nav (notebook panel)
            _pauseNav.style.backgroundColor = VoidBlack;
            _pauseNav.style.borderLeftWidth = 1; _pauseNav.style.borderRightWidth = 1;
            _pauseNav.style.borderTopWidth = 1; _pauseNav.style.borderBottomWidth = 1;
            _pauseNav.style.borderLeftColor = Amber; _pauseNav.style.borderRightColor = Amber;
            _pauseNav.style.borderTopColor = Amber; _pauseNav.style.borderBottomColor = Amber;
            _pauseNav.style.borderLeftWidth = 4;
            _pauseNav.style.borderLeftColor = Amber;
            _pauseNav.style.paddingTop = 48; _pauseNav.style.paddingBottom = 48;
            _pauseNav.style.paddingLeft = 36; _pauseNav.style.paddingRight = 36;
            _pauseNav.style.color = Paper;
            _pauseNav.style.fontSize = 30;

            // Header labels
            var header = _pauseNav.Q(className: "pause-notebook__header");
            if (header != null) {
                header.style.marginBottom = 48;
                header.style.paddingBottom = 36;
                header.style.borderBottomWidth = 1;
                header.style.borderBottomColor = new Color(0.169f, 0.29f, 0.29f); // Institutional Teal
            }

            var titleLabel = _pauseNav.Q<Label>(className: "pause-title");
            if (titleLabel != null) {
                titleLabel.style.fontSize = 32;
                titleLabel.style.color = Amber;
                titleLabel.style.unityFontDefinition = titleLabel.style.unityFontDefinition;
                titleLabel.style.letterSpacing = 1;
                titleLabel.style.marginBottom = 8;
            }

            var subtitleLabel = _pauseNav.Q<Label>(className: "pause-subtitle");
            if (subtitleLabel != null) {
                subtitleLabel.style.fontSize = 17;
                subtitleLabel.style.color = Fluorescent;
                subtitleLabel.style.letterSpacing = 2;
                // Text transform handled via USS class
            }

            // Stats
            var stats = _pauseNav.Q(className: "pause-stats");
            if (stats != null) {
                var statLabels = stats.Query<Label>(className: "pause-stat").ToList();
                foreach (var lbl in statLabels) {
                    lbl.style.color = Fluorescent;
                    lbl.style.fontSize = 17;
                }
            }

            // Buttons
            var buttons = new[] { _btnResume, _btnReiniciar, _btnSettings, _btnHub };
            foreach (var btn in buttons) {
                if (btn == null) continue;
                btn.style.backgroundColor = Amber;
                btn.style.color = VoidBlack;
                btn.style.fontSize = 16;
                btn.style.paddingTop = 14; btn.style.paddingBottom = 14;
                btn.style.paddingLeft = 20; btn.style.paddingRight = 20;
                btn.style.borderLeftWidth = 2;
                btn.style.borderLeftColor = Color.clear;
                btn.style.color = VoidBlack;
                btn.style.unityFontDefinition = btn.style.unityFontDefinition;
                btn.style.fontSize = 16;
                // Text transform handled via USS class
                btn.style.letterSpacing = 1;
                btn.style.marginBottom = 8;
            }

            // Destructive button (Reiniciar)
            if (_btnReiniciar != null) {
                _btnReiniciar.style.backgroundColor = Color.clear;
                _btnReiniciar.style.color = DestructiveRed;
                _btnReiniciar.style.borderLeftColor = Color.clear;
            }

            // Secondary button (Hub)
            if (_btnHub != null) {
                _btnHub.style.backgroundColor = new Color(0.11f, 0.141f, 0.188f); // Navy
                _btnHub.style.color = Fluorescent;
                _btnHub.style.borderLeftColor = Color.clear;
            }

            // Settings panel
            if (_settingsPanel != null) {
                _settingsPanel.style.backgroundColor = VoidBlack;
                _settingsPanel.style.borderLeftWidth = 1; _settingsPanel.style.borderRightWidth = 1;
                _settingsPanel.style.borderTopWidth = 1; _settingsPanel.style.borderBottomWidth = 1;
                _settingsPanel.style.borderLeftColor = new Color(0.788f, 0.831f, 0.69f, 0.2f);
                _settingsPanel.style.borderRightColor = new Color(0.788f, 0.831f, 0.69f, 0.2f);
                _settingsPanel.style.borderTopColor = new Color(0.788f, 0.831f, 0.69f, 0.2f);
                _settingsPanel.style.borderBottomColor = new Color(0.788f, 0.831f, 0.69f, 0.2f);
            }

            // Footer
            var footer = _pauseNav.Q<Label>(className: "pause-footer");
            if (footer != null) {
                footer.style.color = Faded;
                footer.style.fontSize = 17;
            }

            Debug.Log("[PauseMenu] Inline styles applied successfully.");
        }
    }
}