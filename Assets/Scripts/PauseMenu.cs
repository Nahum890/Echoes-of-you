using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// PauseMenu — menú de pausa narrativo al 33% con diseño moderno.
    /// Botones: Reanudar, Reiniciar, Ajustes, Controles, Expediente, Salir.
    ///
    /// Por qué los botones no respondían: todos los UIDocument del juego comparten
    /// el mismo <c>EchoesPanelSettings</c>, así que están en **un solo panel** de UI
    /// Toolkit y el orden de picking lo decide <c>sortingOrder</c>. El menú de pausa
    /// se serializaba en las escenas con orden 10, mientras que
    /// <c>VN_DialogueController</c> se pone a 500 y <c>TutorialOverlayController</c>
    /// a 550. Comprobado en play mode: un <c>panel.Pick()</c> sobre la zona de los
    /// botones devolvía <c>vn-dialogue-root</c>, no el botón. El overlay de la novela
    /// visual, aunque no se viera, se comía cada clic.
    ///
    /// La solución es doble: el menú se sube por encima de esos overlays al pausar
    /// (<see cref="PauseSortingOrder"/>) y las capas que no están mostrando nada
    /// dejan de capturar el ratón.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class PauseMenu : MonoBehaviour
    {
        /// Por encima de VN (500) y del overlay de tutorial (550), y por debajo de
        /// la pantalla de carga (9999), que sí debe taparlo todo.
        public const int PauseSortingOrder = 900;
        /// El modal de confirmación tiene que quedar sobre el propio menú.
        public const int ModalSortingOrder = 950;

        [Header("Scenes")]
        [SerializeField] string hubSceneName = "MainMenu";

        bool _paused;
        UIDocument _doc;
        VisualElement _root;
        VisualElement _pauseRoot;
        VisualElement _pauseNav;
        VisualElement _settingsPanel;
        VisualElement _controlsPanel;
        VisualElement _expedientePanel;

        Button _btnResume;
        Button _btnReiniciar;
        Button _btnSettings;
        Button _btnControles;
        Button _btnExpediente;
        Button _btnHub;

        Button _btnCloseControls;
        Button _btnCloseExpediente;

        /// Se guarda el botón ya cableado, no un booleano global. Con el booleano,
        /// si InitializeUI corría una vez con el árbol a medio clonar, quedaba
        /// marcado como "cableado" y los botones no se enganchaban nunca.
        Button _wiredResume;

        public bool IsPaused => _paused;

        void OnEnable()
        {
            InitializeUI();
            SettingsController.SettingsClosed += OnSettingsClosed;
        }

        void OnDisable()
        {
            SettingsController.SettingsClosed -= OnSettingsClosed;
        }

        void OnSettingsClosed()
        {
            HideSettings();
        }

        void InitializeUI()
        {
            _doc = GetComponent<UIDocument>();
            if (_doc == null || _doc.rootVisualElement == null) return;
            _root = _doc.rootVisualElement;

            // Las escenas traen el PauseMenu serializado con sortingOrder 10, por
            // debajo de los overlays narrativos. Se corrige aquí y no solo desde
            // GameplayUIBootstrap para no depender del orden de arranque.
            if (_doc.sortingOrder < PauseSortingOrder)
                _doc.sortingOrder = PauseSortingOrder;

            _root.style.position = Position.Absolute;
            _root.style.left = 0;
            _root.style.top = 0;
            _root.style.right = 0;
            _root.style.bottom = 0;
            _root.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            _root.style.height = new StyleLength(new Length(100, LengthUnit.Percent));

            _pauseRoot = _root.Q("pause-root");
            if (_pauseRoot == null) return;

            _pauseNav        = _root.Q("pause-nav");
            _settingsPanel   = _root.Q("pause-settings-panel");
            _controlsPanel   = _root.Q("pause-controls-panel");
            _expedientePanel = _root.Q("pause-expediente-panel");

            _btnResume     = _pauseRoot.Q<Button>("btn-resume");
            _btnReiniciar  = _pauseRoot.Q<Button>("btn-reiniciar");
            _btnSettings   = _pauseRoot.Q<Button>("btn-settings");
            _btnControles  = _pauseRoot.Q<Button>("btn-controles");
            _btnExpediente = _pauseRoot.Q<Button>("btn-expediente");
            _btnHub        = _pauseRoot.Q<Button>("btn-hub");

            _btnCloseControls   = _pauseRoot.Q<Button>("btn-close-controls");
            _btnCloseExpediente = _pauseRoot.Q<Button>("btn-close-expediente");

            // Solo se cablea cuando el árbol está realmente clonado, y se re-cablea
            // si UIDocument lo ha vuelto a clonar (los Button serían otros objetos).
            if (_btnResume != null && !ReferenceEquals(_btnResume, _wiredResume))
            {
                _wiredResume = _btnResume;

                _btnResume.clicked     += Resume;
                if (_btnReiniciar != null)  _btnReiniciar.clicked  += ConfirmReiniciar;
                if (_btnSettings != null)   _btnSettings.clicked   += ShowSettings;
                if (_btnControles != null)  _btnControles.clicked  += ShowControls;
                if (_btnExpediente != null) _btnExpediente.clicked += ShowExpediente;
                if (_btnHub != null)        _btnHub.clicked        += ConfirmHub;

                if (_btnCloseControls != null)   _btnCloseControls.clicked   += HideControls;
                if (_btnCloseExpediente != null) _btnCloseExpediente.clicked += HideExpediente;
            }

            if (!_paused)
            {
                _settingsPanel?.AddToClassList("hidden");
                _controlsPanel?.AddToClassList("hidden");
                _expedientePanel?.AddToClassList("hidden");
                _pauseRoot.AddToClassList("hidden");
            }
        }

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            // Con un modal abierto, ESC lo cierra a él. Antes ESC llegaba a la vez
            // al modal y al menú, y reanudaba la partida por detrás del diálogo.
            if (ModalManager.Instance != null && ModalManager.Instance.IsModalOpen)
                return;

            if (!_paused)
            {
                Pause();
                return;
            }

            // Con un subpanel abierto, ESC vuelve a la navegación principal.
            if (IsVisible(_settingsPanel))        HideSettings();
            else if (IsVisible(_controlsPanel))   HideControls();
            else if (IsVisible(_expedientePanel)) HideExpediente();
            else                                  Resume();
        }

        static bool IsVisible(VisualElement element)
        {
            return element != null && !element.ClassListContains("hidden");
        }

        /// <summary>
        /// Sube el menú (y su modal) por encima de los overlays narrativos, y les
        /// quita el picking a las capas que no están mostrando nada. Sin esto los
        /// clics del menú se los quedaba el root a pantalla completa del VN.
        /// </summary>
        void EnsureTopmost()
        {
            if (_doc != null && _doc.sortingOrder < PauseSortingOrder)
                _doc.sortingOrder = PauseSortingOrder;

            if (ModalManager.Instance != null)
            {
                var modalDoc = ModalManager.Instance.GetComponent<UIDocument>();
                if (modalDoc != null && modalDoc.sortingOrder < ModalSortingOrder)
                    modalDoc.sortingOrder = ModalSortingOrder;
            }

            _pauseRoot?.BringToFront();
            ReleaseIdleOverlays();
        }

        /// <summary>
        /// Los overlays a pantalla completa que están ocultos siguen siendo
        /// pickables si su raíz quedó en display:Flex (pasa mientras el VN espera a
        /// que su panel se enganche, hasta 180 frames). Se les pone pickingMode
        /// Ignore para que no intercepten nada mientras no muestren contenido.
        /// </summary>
        static void ReleaseIdleOverlays()
        {
            foreach (var doc in FindObjectsByType<UIDocument>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (doc == null || doc.rootVisualElement == null) continue;
                if (doc.GetComponent<PauseMenu>() != null) continue;
                if (ModalManager.Instance != null && doc.gameObject == ModalManager.Instance.gameObject) continue;
                if (doc.sortingOrder < PauseSortingOrder) continue;

                for (int i = 0; i < doc.rootVisualElement.childCount; i++)
                {
                    VisualElement child = doc.rootVisualElement[i];
                    bool showing = child.resolvedStyle.display != DisplayStyle.None
                                && child.resolvedStyle.opacity > 0.01f;
                    child.pickingMode = showing ? PickingMode.Position : PickingMode.Ignore;
                }
            }
        }

        public void Pause()
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
            EnsureTopmost();

            _pauseRoot?.RemoveFromClassList("hidden");
            _pauseNav?.RemoveFromClassList("hidden");
            _settingsPanel?.AddToClassList("hidden");
            _controlsPanel?.AddToClassList("hidden");
            _expedientePanel?.AddToClassList("hidden");

            RefreshChapterHeader();
            RefreshExpedienteStats();

            _btnResume?.Focus();
        }

        public void Resume()
        {
            _paused = false;
            Time.timeScale = 1f;
            UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;

            var cam = FindAnyObjectByType<SimpleFollowCamera>();
            if (cam != null) cam.Frozen = false;

            var gameHUD2 = FindAnyObjectByType<GameHUD>();
            gameHUD2?.SetVisible(true);

            _pauseRoot?.AddToClassList("hidden");
        }

        void ConfirmReiniciar()
        {
            string currentScene = SceneManager.GetActiveScene().name;

            void DoRestart()
            {
                Resume();
                PostProcessingSetup.PrepareForSceneReload();
                LoadingScreenController.TransitionToScene(currentScene);
            }

            if (ModalManager.Instance == null)
            {
                DoRestart();
                return;
            }

            EnsureTopmost();
            ModalManager.Instance.ShowModal(
                "Reiniciar Capítulo",
                "Se reiniciará el nivel actual y se reestablecerán los ecos.",
                onConfirm: DoRestart,
                onCancel: () => { });
        }

        void ConfirmHub()
        {
            void DoExit()
            {
                UnpauseForMenu();
                PostProcessingSetup.PrepareForSceneReload();
                LoadingScreenController.TransitionToScene(hubSceneName);
            }

            if (ModalManager.Instance == null)
            {
                DoExit();
                return;
            }

            EnsureTopmost();
            ModalManager.Instance.ShowModal(
                "Volver al Menú Principal",
                "¿Deseas volver al menú principal? El progreso de los ecos se guardará.",
                onConfirm: DoExit,
                onCancel: () => { });
        }

        void ShowSettings()
        {
            _pauseNav?.AddToClassList("hidden");
            _controlsPanel?.AddToClassList("hidden");
            _expedientePanel?.AddToClassList("hidden");
            _settingsPanel?.RemoveFromClassList("hidden");
            SettingsController.EnsureExists()?.ShowInContainer(_settingsPanel);
        }

        void HideSettings()
        {
            _settingsPanel?.AddToClassList("hidden");
            _pauseNav?.RemoveFromClassList("hidden");
            _btnResume?.Focus();
        }

        void ShowControls()
        {
            _pauseNav?.AddToClassList("hidden");
            _settingsPanel?.AddToClassList("hidden");
            _expedientePanel?.AddToClassList("hidden");
            _controlsPanel?.RemoveFromClassList("hidden");
            _btnCloseControls?.Focus();
        }

        void HideControls()
        {
            _controlsPanel?.AddToClassList("hidden");
            _pauseNav?.RemoveFromClassList("hidden");
            _btnResume?.Focus();
        }

        void ShowExpediente()
        {
            _pauseNav?.AddToClassList("hidden");
            _settingsPanel?.AddToClassList("hidden");
            _controlsPanel?.AddToClassList("hidden");
            _expedientePanel?.RemoveFromClassList("hidden");
            RefreshExpedienteStats();
            _btnCloseExpediente?.Focus();
        }

        void HideExpediente()
        {
            _expedientePanel?.AddToClassList("hidden");
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

        void RefreshChapterHeader()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            int levelIndex = GameProgress.GetSceneIndex(sceneName);
            int displayLevelNum = levelIndex >= 0 ? levelIndex + 1 : 1;

            string chapterText;
            if (sceneName.StartsWith("Level_"))
            {
                string roman = GetRomanNumeral(displayLevelNum);
                string levelName = GameProgress.GetLevelDisplayName(sceneName);
                chapterText = $"Capítulo {roman}: {levelName}";
            }
            else
            {
                chapterText = $"Archivo: {sceneName.ToUpperInvariant()}";
            }

            SetLabel("lbl-pause-chapter", chapterText);

            string locationName = ResolveLocationName(displayLevelNum);
            SetLabel("lbl-pause-location", $"Checkpoint: {locationName}");
        }

        void RefreshExpedienteStats()
        {
            if (_doc == null || _doc.rootVisualElement == null) return;

            string sceneName = SceneManager.GetActiveScene().name;
            int levelIndex = GameProgress.GetSceneIndex(sceneName);
            int displayLevelNum = levelIndex >= 0 ? levelIndex + 1 : 1;

            // Etapa psicológica según nivel (1-4 Convicción, 5-8 Negación/Culpa,
            // 9-12 Desilusión, 13-15 Aceptación).
            string stageTitle = "ETAPA: CONVICCIÓN";
            string stageDesc = "Aiden se resiste a aceptar la alteración de los recuerdos. La memoria aún es una barrera.";
            if (displayLevelNum >= 13)
            {
                stageTitle = "ETAPA: ACEPTACIÓN";
                stageDesc = "Aiden comprende que el pasado no puede reescribirse, solo integrarse en la consciencia presente.";
            }
            else if (displayLevelNum >= 9)
            {
                stageTitle = "ETAPA: DESILUSIÓN / REALIZACIÓN";
                stageDesc = "Las fracturas temporales revelan que los ecos no son salvaciones, sino repeticiones del dolor.";
            }
            else if (displayLevelNum >= 5)
            {
                stageTitle = "ETAPA: NEGACIÓN / CULPA";
                stageDesc = "La presencia de Lyra se vuelve persistente en los recuerdos. Aiden intenta reparar lo irreparable.";
            }

            SetLabel("lbl-expediente-stage", stageTitle);
            SetLabel("lbl-expediente-stage-desc", stageDesc);

            LevelRuntimeController runtime = LevelRuntimeController.Instance;
            float sessionTime = runtime != null ? runtime.SessionPlaySeconds : 0f;
            int sessionEchoes = runtime != null ? runtime.SessionEchoes : 0;
            int sessionDeaths = runtime != null ? runtime.SessionDeaths : 0;

            SetLabel("lbl-expediente-time", GameProgress.FormatPlayTime(sessionTime));
            SetLabel("lbl-expediente-echoes", $"{sessionEchoes}");
            SetLabel("lbl-expediente-deaths", $"{sessionDeaths}");

            int completed = GameProgress.GetCompletedCount();
            SetLabel("lbl-expediente-total", $"{completed} / {GameProgress.TotalLevels} ECOS");
        }

        string ResolveLocationName(int levelNum)
        {
            switch (levelNum)
            {
                case 1: return "Pasillo de los Archivos";
                case 2: return "Aula de Clases 101";
                case 3: return "Biblioteca Histórica";
                case 4: return "Laboratorio de Ciencias";
                case 5: return "Patio Central";
                case 6: return "Auditorio Mayor";
                case 7: return "Sala de Música";
                case 8: return "Taller de Arte";
                case 9: return "Observatorio";
                case 10: return "Gimnasio Abandonado";
                case 11: return "Depósito Subterráneo";
                case 12: return "Azotea";
                case 13: return "Galería de Ecos";
                case 14: return "Umbral de la Memoria";
                case 15: return "El Núcleo del Pasado";
                default: return "Sector Institucional";
            }
        }

        string GetRomanNumeral(int number)
        {
            string[] roman = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII", "XIII", "XIV", "XV" };
            if (number >= 1 && number <= roman.Length) return roman[number - 1];
            return number.ToString();
        }

        void SetLabel(string elementName, string text)
        {
            var lbl = _doc?.rootVisualElement?.Q<Label>(elementName);
            if (lbl != null) lbl.text = text;
        }
    }
}
