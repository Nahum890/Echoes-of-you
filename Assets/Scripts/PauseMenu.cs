using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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
        Button _btnFooterMenu;

        void OnEnable() { InitializeUI(); SettingsController.SettingsClosed += OnSettingsClosed; }

        void OnDisable() { SettingsController.SettingsClosed -= OnSettingsClosed; }

        void OnSettingsClosed()
        {
            // Settings closed from inside (CERRAR TERMINAL / Aplicar cambios): restore pause nav
            _settingsPanel?.AddToClassList("hidden");
            _pauseNav?.RemoveFromClassList("hidden");
            _btnResume?.Focus();
        }

        void InitializeUI()
        {
            if (_paused) return;

            _doc = GetComponent<UIDocument>();
            if (_doc == null || _doc.rootVisualElement == null) return;
            _root = _doc.rootVisualElement;

            _pauseRoot = _root.Q("pause-root");
            if (_pauseRoot == null) return;

            _pauseNav = _root.Q("pause-nav");
            _settingsPanel = _root.Q("pause-settings-panel");

            _btnResume    = _pauseRoot.Q<Button>("btn-resume");
            _btnReiniciar = _pauseRoot.Q<Button>("btn-reiniciar");
            _btnSettings  = _pauseRoot.Q<Button>("btn-settings");
            _btnHub       = _pauseRoot.Q<Button>("btn-hub");
            _btnFooterMenu = _pauseRoot.Q<Button>("btn-footer-menu");

            // Wire button events just once
            if (_btnResume != null && !_btnResume.name.EndsWith("_wired"))
            {
                _btnResume.name += "_wired";
                _btnResume.clicked += Resume;
                _btnReiniciar.clicked += ConfirmReiniciar;
                _btnSettings.clicked += ShowSettings;
                _btnHub.clicked += ConfirmHub;
                if (_btnFooterMenu != null) _btnFooterMenu.clicked += ConfirmHub;
            }

            _settingsPanel?.AddToClassList("hidden");
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

            // Congelar cámara del jugador
            var cam = FindAnyObjectByType<SimpleFollowCamera>();
            if (cam != null) cam.Frozen = true;

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

            // Liberar cámara del jugador
            var cam = FindAnyObjectByType<SimpleFollowCamera>();
            if (cam != null) cam.Frozen = false;

            // Mostrar HUD gameplay
            var gameHUD2 = FindAnyObjectByType<GameHUD>();
            gameHUD2?.SetVisible(true);

            _pauseRoot?.AddToClassList("hidden");
        }

        void ConfirmReiniciar()
        {
            if (ModalManager.Instance == null)
            {
                Resume();
                PostProcessingSetup.PrepareForSceneReload();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
            ModalManager.Instance.ShowModal(
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
            if (ModalManager.Instance == null)
            {
                UnpauseForMenu();
                PostProcessingSetup.PrepareForSceneReload();
                SceneManager.LoadScene(hubSceneName);
                return;
            }
            ModalManager.Instance.ShowModal(
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

            var cam = FindAnyObjectByType<SimpleFollowCamera>();
            if (cam != null) cam.Frozen = false;

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
                ? $"Capítulo: {GameProgress.GetLevelDisplayName(sceneName)}"
                : $"Zona: {sceneName}";

            SetLabel("lbl-pause-fragment", fragmentLine);

            LevelRuntimeController runtime = LevelRuntimeController.Instance;
            float sessionTime = runtime != null ? runtime.SessionPlaySeconds : 0f;
            int sessionEchoes = runtime != null ? runtime.SessionEchoes : 0;
            int sessionDeaths = runtime != null ? runtime.SessionDeaths : 0;

            SetLabel("lbl-pause-time", $"Tiempo: {GameProgress.FormatPlayTime(sessionTime)}");
            SetLabel("lbl-pause-echoes", $"Ecos grabados: {sessionEchoes}");
            SetLabel("lbl-pause-deaths", isLevel
                ? $"Quiebres (aula): {GameProgress.GetSceneDeathCount(sceneName)} · sesión {sessionDeaths}"
                : $"Quiebres (sesión): {sessionDeaths}");

            int completed = GameProgress.GetCompletedCount();
            SetLabel("lbl-pause-total",
                $"Cuaderno de Aiden: {completed}/{GameProgress.TotalLevels} · {GameProgress.GetTotalEchoesCreated()} ecos · {GameProgress.FormatPlayTime(GameProgress.GetTotalPlayTimeSeconds())}");
        }

        void SetLabel(string elementName, string text)
        {
            var lbl = _doc.rootVisualElement.Q<Label>(elementName);
            if (lbl != null) lbl.text = text;
        }
    }
}
