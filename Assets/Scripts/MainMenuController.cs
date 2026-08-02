using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Echoes.UI
{
    /// <summary>
    /// MainMenuController — Rewrite Fase 2.
    /// Vocabulario canónico: Cuaderno, Eco, Recuerdo, Capítulo, Grabar, Proyectar.
    /// Delegación: SettingsController, LevelSelectController, CreditsController.
    /// Navegación: NavigationManager + FocusManager.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] string firstLevelScene = "Level_01";

        // Color constants for inline styling (bypass USS cascade issues)
        static readonly Color Amber = new Color(1f, 0.749f, 0f);       // #FFBF00
        static readonly Color Paper = new Color(0.957f, 0.949f, 0.933f); // #F4F2EE
        static readonly Color VoidBlack = new Color(0.039f, 0.039f, 0.051f); // #0A0A0D
        static readonly Color DebugRed = new Color(1f, 0f, 0f, 1f);     // DEBUG visibility
        static readonly Color Fluorescent = new Color(0.788f, 0.831f, 0.69f); // #C9D4B0
        static readonly Color Faded = new Color(0.353f, 0.29f, 0.18f);   // #5A4A2E
        static readonly Color DarkBg = new Color(0.012f, 0.016f, 0.024f); // #030406
        static readonly Color RightBg = new Color(0.035f, 0.043f, 0.059f); // #090B0F
        static readonly Color MainBg = new Color(0.024f, 0.031f, 0.047f); // #06080C
        static readonly Color DestructiveRed = new Color(0.698f, 0.227f, 0.227f); // #B23A3A
        static readonly Color BorderAmber = new Color(1f, 0.749f, 0f, 0.2f); // Amber 20%

        UIDocument _doc;
        VisualElement _root;
        VisualElement _rootElement; // The "root" child element (name="root")
        VisualElement _voidIntro;
        VisualElement _mainContent;
        VisualElement _rightContainer;
        Label _heroTitle;

        Button _btnContinuar;
        Button _btnCapitulos;
        Button _btnConfigurar;
        Button _btnCreditos;
        Button _btnCerrar;

        VisualElement _panelCuadernoRecorrido;
        VisualElement _panelCapitulos;
        VisualElement _panelAjustes;
        VisualElement _panelCerrar;

        Label _lblEcosAnclados;
        Label _lblRecuerdos;
        VisualElement _levelCardGrid;

        bool _wired;

        void OnEnable()
        {
            _doc = GetComponent<UIDocument>();
            if (_doc == null || _doc.rootVisualElement == null) return;
            _root = _doc.rootVisualElement;

            // UI Elements — MUST query BEFORE LoadStyleSheets() so _rootElement is available
            _voidIntro = _root.Q("void-intro");
            _mainContent = _root.Q("main-content");
            _rightContainer = _root.Q("right-content-container");
            _heroTitle = _root.Q<Label>("heroTitle");
            _rootElement = _root.Q("root");

            // Load stylesheets for editor play mode (USS auto-load not reliable in editor)
            LoadStyleSheets();

            // IMMEDIATELY force styles on rootElement (critical — this covers the UIDocumentRootElement)
            if (_rootElement != null)
            {
                _rootElement.style.position = Position.Absolute;
                _rootElement.style.left = 0; _rootElement.style.top = 0; _rootElement.style.right = 0; _rootElement.style.bottom = 0;
                _rootElement.style.backgroundColor = DebugRed;
                _rootElement.style.color = Paper;
                _rootElement.style.fontSize = 30;
            }

            _btnContinuar  = _root.Q<Button>("nav-continuar");
            _btnCapitulos  = _root.Q<Button>("nav-capitulos");
            _btnConfigurar = _root.Q<Button>("nav-configurar");
            _btnCreditos   = _root.Q<Button>("nav-creditos");
            _btnCerrar     = _root.Q<Button>("nav-cerrar");

            _panelCuadernoRecorrido = _root.Q("panel-cuaderno-recorrido");
            _panelCapitulos         = _root.Q("panel-capitulos-cuaderno");
            _panelAjustes           = _root.Q("panel-ajustes-cuaderno");
            _panelCerrar            = _root.Q("panel-cerrar-cuaderno");

            _lblEcosAnclados = _root.Q<Label>("lblEcosAnclados");
            _lblRecuerdos    = _root.Q<Label>("lblRecuerdos");
            _levelCardGrid   = _root.Q("levelCardGrid");

            // Buttons en panel cerrar
            var btnConfirmExit = _root.Q<Button>("btn-confirm-exit");
            var btnCancelExit  = _root.Q<Button>("btn-cancel-exit");

            if (!_wired)
            {
                _btnContinuar.clicked  += OnContinuar;
                _btnCapitulos.clicked  += OnCapitulos;
                _btnConfigurar.clicked += OnConfigurar;
                _btnCreditos.clicked   += OnCreditos;
                _btnCerrar.clicked     += OnCerrar;

                btnConfirmExit.clicked += ConfirmExit;
                btnCancelExit.clicked  += CancelExit;

                _wired = true;
            }

            GameProgress.EnsureInitialized();
            RefreshFooterStats();
            BindLevelCards();
            ShowVoidIntro();
        }

        void Update()
        {
            if (!_voidIntro.ClassListContains("hidden") && Input.GetKeyDown(KeyCode.Space))
                DismissIntro();
        }

        void ShowVoidIntro()
        {
            _voidIntro.RemoveFromClassList("hidden");
            _mainContent.AddToClassList("hidden");
            _rightContainer.AddToClassList("hidden");
            _heroTitle.text = "Aula 104";
            SetActiveNav(_btnContinuar);
            ShowPanel(_panelCuadernoRecorrido);
        }

        void DismissIntro()
        {
            _voidIntro.AddToClassList("hidden");
            _mainContent.RemoveFromClassList("hidden");
            _rightContainer.RemoveFromClassList("hidden");
        }

        void OnContinuar()
        {
            var continueScene = GameProgress.GetContinueSceneName();
            if (!string.IsNullOrEmpty(continueScene))
                LoadLevel(continueScene);
            else
                LoadLevel(firstLevelScene);
        }

        void OnCapitulos()
        {
            SetActiveNav(_btnCapitulos);
            _heroTitle.text = "Capítulos del Cuaderno";
            ShowPanel(_panelCapitulos);
            RefreshLevelCards();
        }

        void OnConfigurar()
        {
            SetActiveNav(_btnConfigurar);
            _heroTitle.text = "Configurar Cuaderno";
            ShowPanel(_panelAjustes);
        }

        void OnCreditos()
        {
            SetActiveNav(_btnCreditos);
            _heroTitle.text = "Créditos";

            // Load credits scene directly — NavigationManager.Stack will be
            // rebuilt when CreditsController initializes after the scene loads.
            if (Application.CanStreamedLevelBeLoaded("CreditsScene"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("CreditsScene");
        }

        void OnCerrar()
        {
            SetActiveNav(_btnCerrar);
            _heroTitle.text = "Cerrar Cuaderno";
            ShowPanel(_panelCerrar);
        }

        void CancelExit() => ShowPanel(_panelCuadernoRecorrido);

        void ConfirmExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void LoadLevel(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;
            if (!Application.CanStreamedLevelBeLoaded(sceneName)) return;

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(sceneName);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        void SetActiveNav(Button active)
        {
            _btnContinuar.RemoveFromClassList("nav-item--active");
            _btnCapitulos.RemoveFromClassList("nav-item--active");
            _btnConfigurar.RemoveFromClassList("nav-item--active");
            _btnCreditos.RemoveFromClassList("nav-item--active");
            _btnCerrar.RemoveFromClassList("nav-item--active");
            active?.AddToClassList("nav-item--active");
        }

        void ShowPanel(VisualElement panel)
        {
            _panelCuadernoRecorrido.RemoveFromClassList("preview-panel--visible");
            _panelCapitulos.RemoveFromClassList("preview-panel--visible");
            _panelAjustes.RemoveFromClassList("preview-panel--visible");
            _panelCerrar.RemoveFromClassList("preview-panel--visible");
            panel?.AddToClassList("preview-panel--visible");
        }

        void RefreshFooterStats()
        {
            int completed = GameProgress.GetCompletedCount();
            int total = GameProgress.TotalLevels;
            _lblEcosAnclados.text = $"Ecos anclados: {completed}/{total}";
            _lblRecuerdos.text = $"Recuerdos completados: {completed}";
        }

        void BindLevelCards()
        {
            _levelCardGrid.Clear();
            for (int i = 1; i <= GameProgress.TotalLevels; i++)
            {
                string sceneName = $"Level_{i:D2}";
                var card = new Button { text = $"Capítulo {i:D2}" };
                card.AddToClassList("echo-card");

                bool unlocked = GameProgress.IsSceneUnlocked(sceneName);
                bool completed = GameProgress.IsSceneCompleted(sceneName);

                if (!unlocked) card.AddToClassList("echo-card--locked");
                else if (completed) card.AddToClassList("echo-card--completed");
                else if (sceneName == GameProgress.GetContinueSceneName()) card.AddToClassList("echo-card--current");
                else card.AddToClassList("echo-card--available");

                if (unlocked)
                {
                    string capturedScene = sceneName;
                    card.clicked += () => LoadLevel(capturedScene);
                }

                _levelCardGrid.Add(card);
            }
        }

        void RefreshLevelCards() => BindLevelCards();

        void LoadStyleSheets()
        {
            var paths = new[]
            {
                "Assets/UI/EchoesTheme.tss",
                "Assets/UI/MainMenuUI.uss",
                "Assets/UI/Components/EchoButton.uss",
                "Assets/UI/Components/EchoPanel.uss",
                "Assets/UI/Components/EchoCard.uss",
                "Assets/UI/Components/EchoTabs.uss",
                "Assets/UI/Components/EchoModal.uss",
                "Assets/UI/Components/EchoToast.uss",
                "Assets/UI/Components/EchoLoading.uss",
                "Assets/UI/Components/EchoSlider.uss",
                "Assets/UI/Components/EchoToggle.uss",
                "Assets/UI/Components/EchoDropdown.uss",
            };

            foreach (var path in paths)
            {
                StyleSheet ss = null;
#if UNITY_EDITOR
                ss = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
#else
                string resourcePath = path.Replace("Assets/Resources/", "").Replace(".uss", "");
                ss = Resources.Load<StyleSheet>(resourcePath);
#endif
                if (ss != null)
                {
                    _root.styleSheets.Add(ss);
                    Debug.Log($"[MainMenuController] Loaded stylesheet: {path}");
                }
            }

            // GARANTIZADO: aplicar estilos inline via C# por si falla el USS
            ApplyInlineStyles();
        }

        void ApplyInlineStyles()
        {
            // ROOT
            _root.style.position = Position.Absolute;
            _root.style.left = 0; _root.style.top = 0; _root.style.right = 0; _root.style.bottom = 0;
            _root.style.backgroundColor = VoidBlack;
            _root.style.color = Paper;
            _root.style.fontSize = 30;

            // ROOT ELEMENT (child "root") — CRITICAL: this covers the UIDocumentRootElement
            if (_rootElement != null)
            {
                _rootElement.style.position = Position.Absolute;
                _rootElement.style.left = 0; _rootElement.style.top = 0; _rootElement.style.right = 0; _rootElement.style.bottom = 0;
                _rootElement.style.backgroundColor = VoidBlack;
                _rootElement.style.color = Paper;
                _rootElement.style.fontSize = 30;
            }

            // VOID INTRO
            if (_voidIntro != null)
            {
                _voidIntro.style.backgroundColor = VoidBlack;
                _voidIntro.style.position = Position.Absolute;
                _voidIntro.style.left = 0; _voidIntro.style.top = 0; _voidIntro.style.right = 0; _voidIntro.style.bottom = 0;
                _voidIntro.style.justifyContent = Justify.Center;
                _voidIntro.style.alignItems = Align.Center;
                _voidIntro.style.color = Paper;
                _voidIntro.style.fontSize = 30;
            }

            // VOID INTRO LABEL
            var voidLabel = _voidIntro?.Q<Label>();
            if (voidLabel != null)
            {
                voidLabel.style.color = Paper;
                voidLabel.style.fontSize = 30;
                voidLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            }

            // MAIN CONTENT
            if (_mainContent != null)
            {
                _mainContent.style.backgroundColor = MainBg;
                _mainContent.style.position = Position.Absolute;
                _mainContent.style.left = 0; _mainContent.style.top = 0; _mainContent.style.right = 0; _mainContent.style.bottom = 0;
                _mainContent.style.flexDirection = FlexDirection.Row;
                _mainContent.style.color = Paper;
                _mainContent.style.fontSize = 30;
            }

            // SIDEBAR
            var sideNav = _rootElement?.Q("side-nav") ?? _root.Q("side-nav");
            if (sideNav != null)
            {
                sideNav.style.backgroundColor = DarkBg;
                sideNav.style.width = 420;
                sideNav.style.paddingTop = 48; sideNav.style.paddingBottom = 48;
                sideNav.style.paddingLeft = 32; sideNav.style.paddingRight = 32;
                sideNav.style.flexShrink = 0;
                sideNav.style.justifyContent = Justify.SpaceBetween;
                sideNav.style.borderRightWidth = 1;
                sideNav.style.borderRightColor = BorderAmber;
                sideNav.style.color = Paper;
                sideNav.style.fontSize = 30;
            }

            // HERO TITLE
            if (_heroTitle != null)
            {
                _heroTitle.style.fontSize = 42;
                _heroTitle.style.color = Fluorescent;
            }

            // BOTONES NAV
            var buttons = new[] { _btnContinuar, _btnCapitulos, _btnConfigurar, _btnCreditos, _btnCerrar };
            foreach (var btn in buttons)
            {
                if (btn == null) continue;
                btn.style.backgroundColor = Amber;
                btn.style.color = VoidBlack;
                btn.style.fontSize = 30;
                btn.style.paddingTop = 16; btn.style.paddingBottom = 16;
                btn.style.paddingLeft = 20; btn.style.paddingRight = 20;
                btn.style.borderLeftWidth = 2;
                btn.style.marginBottom = 4;
                btn.style.justifyContent = Justify.FlexStart;
            }

            // BUTTON "Cerrar" — destructive style
            if (_btnCerrar != null)
            {
                _btnCerrar.style.backgroundColor = new Color(0.698f, 0.227f, 0.227f); // #B23A3A
                _btnCerrar.style.color = Paper;
            }

            // RIGHT CONTAINER
            if (_rightContainer != null)
            {
                _rightContainer.style.backgroundColor = RightBg;
                _rightContainer.style.position = Position.Absolute;
                _rightContainer.style.left = 420;
                _rightContainer.style.top = 0; _rightContainer.style.right = 0; _rightContainer.style.bottom = 0;
                _rightContainer.style.color = Paper;
                _rightContainer.style.fontSize = 30;
            }

            // PANELS
            var panels = new[] { _panelCuadernoRecorrido, _panelCapitulos, _panelAjustes, _panelCerrar };
            foreach (var p in panels)
            {
                if (p == null) continue;
                p.style.position = Position.Absolute;
                p.style.left = 0; p.style.top = 0; p.style.right = 0; p.style.bottom = 0;
                p.style.paddingTop = 56; p.style.paddingBottom = 56;
                p.style.paddingLeft = 48; p.style.paddingRight = 48;
                p.style.flexDirection = FlexDirection.Column;
                p.style.backgroundColor = VoidBlack;
                p.style.color = Paper;
                p.style.fontSize = 30;
            }

            // PANEL TITLES
            var titles = _rootElement?.Query<Label>(className: "panel-title").ToList() 
                ?? _root.Query<Label>(className: "panel-title").ToList();
            foreach (var t in titles)
            {
                t.style.color = Amber;
                t.style.fontSize = 42;
                t.style.marginBottom = 32;
            }

            // FOOTER STATS
            if (_lblEcosAnclados != null)
            {
                _lblEcosAnclados.style.color = Faded;
                _lblEcosAnclados.style.fontSize = 17;
            }
            if (_lblRecuerdos != null)
            {
                _lblRecuerdos.style.color = Faded;
                _lblRecuerdos.style.fontSize = 17;
            }

            // LEVEL CARDS
            if (_levelCardGrid != null)
            {
                _levelCardGrid.style.flexGrow = 1;
            }

            Debug.Log("[MainMenuController] Inline styles applied successfully.");
        }
    }
}