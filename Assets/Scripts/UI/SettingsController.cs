using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// SettingsController — singleton unificado para MainMenu + Pausa.
    /// Categorías vía EchoTabs: Vídeo / Audio / Controles / Accesibilidad / Gameplay.
    ///
    /// Todo control pasa por <see cref="EchoesSettings"/>, que es quien guarda y
    /// quien aplica. Antes cada ajuste hacía su propio PlayerPrefs + SendMessage y
    /// varios no llegaban a ningún consumidor:
    ///
    /// - sensibilidad: escribía "CameraSensitivity" y avisaba a ThirdPersonCamera
    ///   con un método que no existía; la cámara viva lee "MouseSensitivity".
    /// - tiempo extra de grabación: solo escribía la clave, nadie la leía.
    /// - niebla: escribía "FogDensity" y el bootstrap de nivel la pisaba con
    ///   "Echoes.GameFogDensity".
    /// - subtítulos, tamaño de subtítulo, fondo de subtítulo y reducir destellos:
    ///   se buscaban en el UXML y no se registraba callback ninguno.
    /// - "Borrar Todo" llamaba a UnityEditor.EditorUtility.DisplayDialog **sin**
    ///   guardas de compilación: no funcionaba fuera del editor y rompía la build.
    /// - "Restaurar Defecto" hacía PlayerPrefs.DeleteAll(), que además del ajuste
    ///   borraba el progreso de la partida.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        public static SettingsController Instance { get; private set; }

        public static SettingsController EnsureExists()
        {
            if (Instance != null) return Instance;
            var existing = FindAnyObjectByType<SettingsController>();
            if (existing != null)
            {
                Instance = existing;
                return Instance;
            }
            var go = new GameObject("SettingsController");
            Instance = go.AddComponent<SettingsController>();
            DontDestroyOnLoad(go);
            return Instance;
        }

        /// <summary>Se dispara al cerrar los ajustes (CERRAR TERMINAL / Aplicar cambios).
        /// Los anfitriones como PauseMenu restauran su UI previa con este evento.</summary>
        public static event System.Action SettingsClosed;

        [Header("Templates")]
        [SerializeField] VisualTreeAsset _settingsTemplate; // SettingsUI.uxml

        VisualElement _root;
        VisualElement _settingsContainer;

        // Tabs
        Button _tabVideo, _tabAudio, _tabControles, _tabAccesibilidad, _tabGameplay;
        VisualElement _panelVideo, _panelAudio, _panelControles, _panelAccesibilidad, _panelGameplay;

        // Vídeo
        DropdownField _resDropdown;
        Toggle _fullscreenToggle, _vsyncToggle;
        DropdownField _scaleDropdown;
        List<Resolution> _filteredResolutions;

        // Audio
        Slider _sldMaster, _sldMusic, _sldSfx, _sldEchoVoice;
        Label _lblMasterVal, _lblMusicVal, _lblSfxVal, _lblEchoVoiceVal;

        // Controles
        Button _btnSensLow, _btnSensMed, _btnSensHigh;
        Slider _sensitivitySlider;
        Label _lblCamSensVal;

        // Accesibilidad
        DropdownField _textSizeDropdown;
        Toggle _highContrastToggle, _subtitlesToggle, _subtitleBgToggle, _reduceFlashesToggle, _reduceMotionToggle;
        Slider _subtitleSizeSlider;

        // Gameplay
        Slider _extraRecordTimeSlider;
        Label _lblExtraRecordTimeVal;
        Slider _sldFog, _sldEcho;
        Label _lblFogVal, _lblEchoVal;

        // Botones
        Button _btnRestoreDefaults, _btnSettingsBack, _btnSettingsApply, _btnDeleteProgress;

        /// <summary>
        /// Mientras se rellenan los controles desde los valores guardados, los
        /// callbacks de cambio no deben aplicar nada: si no, LoadCurrentSettings
        /// dispara Apply* con valores a medio cargar.
        /// </summary>
        bool _loading;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ResolveTemplate();
        }

        void OnEnable()
        {
            // Recupera la referencia estática tras un domain reload: los objetos
            // DontDestroyOnLoad sobreviven pero los statics se limpian.
            if (Instance == null)
                Instance = this;
            ResolveTemplate();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void ResolveTemplate()
        {
            if (_settingsTemplate != null) return;
            _settingsTemplate = Resources.Load<VisualTreeAsset>("UI/SettingsUI")
                             ?? Resources.Load<VisualTreeAsset>("SettingsUI");
        }

        public void Setup(VisualElement root, VisualTreeAsset template)
        {
            _root = root;
            if (template != null) _settingsTemplate = template;
            ResolveTemplate();
            InitializeUI();
        }

        public void ShowInContainer(VisualElement container)
        {
            ResolveTemplate();
            if (container == null || _settingsTemplate == null) return;

            container.Clear();
            var panel = _settingsTemplate.CloneTree();
            // CloneTree() devuelve un TemplateContainer sin clases ni altura; el root
            // de ajustes que lleva dentro es position:absolute, así que el wrapper
            // colapsa a 0 de alto. Hay que estirarlo o no se ve ni se puede pulsar.
            panel.style.position = Position.Absolute;
            panel.style.left = 0;
            panel.style.top = 0;
            panel.style.right = 0;
            panel.style.bottom = 0;

            AttachStyleSheets(panel);

            container.Add(panel);
            _settingsContainer = panel;
            InitializeUIFromContainer(panel);
            ShowTab("Video");
        }

        void AttachStyleSheets(VisualElement panel)
        {
            var ssTheme = Resources.Load<StyleSheet>("UI/EchoesTheme");
            if (ssTheme != null && !panel.styleSheets.Contains(ssTheme))
                panel.styleSheets.Add(ssTheme);

            var ssSettings = Resources.Load<StyleSheet>("UI/SettingsUI");
            if (ssSettings != null && !panel.styleSheets.Contains(ssSettings))
                panel.styleSheets.Add(ssSettings);
        }

        void InitializeUI()
        {
            if (_root == null || _settingsTemplate == null) return;
            _settingsContainer = _settingsTemplate.CloneTree();
            AttachStyleSheets(_settingsContainer);
            _root.Add(_settingsContainer);
            InitializeUIFromContainer(_settingsContainer);
        }

        void InitializeUIFromContainer(VisualElement container)
        {
            // Cada contenedor es un CloneTree() nuevo: los VisualElement son otros,
            // así que no hace falta desregistrar nada del anterior.

            // ── Tabs ──
            _tabVideo         = container.Q<Button>("tabVideo");
            _tabAudio         = container.Q<Button>("tabAudio");
            _tabControles     = container.Q<Button>("tabControles");
            _tabAccesibilidad = container.Q<Button>("tabAccesibilidad");
            _tabGameplay      = container.Q<Button>("tabGameplay");

            _panelVideo         = container.Q("panelVideo");
            _panelAudio         = container.Q("panelAudio");
            _panelControles     = container.Q("panelControles");
            _panelAccesibilidad = container.Q("panelAccesibilidad");
            _panelGameplay      = container.Q("panelGameplay");

            if (_tabVideo != null)         _tabVideo.clicked         += () => ShowTab("Video");
            if (_tabAudio != null)         _tabAudio.clicked         += () => ShowTab("Audio");
            if (_tabControles != null)     _tabControles.clicked     += () => ShowTab("Controles");
            if (_tabAccesibilidad != null) _tabAccesibilidad.clicked += () => ShowTab("Accesibilidad");
            if (_tabGameplay != null)      _tabGameplay.clicked      += () => ShowTab("Gameplay");

            // ── VÍDEO ──
            _resDropdown      = container.Q<DropdownField>("ResolutionDropdown");
            _fullscreenToggle = container.Q<Toggle>("FullscreenToggle");
            _vsyncToggle      = container.Q<Toggle>("VsyncToggle");
            _scaleDropdown    = container.Q<DropdownField>("ScaleDropdown");

            if (_scaleDropdown != null)
            {
                _scaleDropdown.choices = new List<string>(GameSettings.UIScaleNames);
                _scaleDropdown.RegisterValueChangedCallback(evt => { if (!_loading) ApplyUIScale(evt.newValue); });
            }

            SetupResolutions();

            if (_fullscreenToggle != null)
                _fullscreenToggle.RegisterValueChangedCallback(evt => { if (!_loading) { EchoesSettings.Fullscreen = evt.newValue; ApplyVideo(); } });
            if (_vsyncToggle != null)
                _vsyncToggle.RegisterValueChangedCallback(evt => { if (!_loading) { EchoesSettings.VSync = evt.newValue; ApplyVideo(); } });
            if (_resDropdown != null)
                _resDropdown.RegisterValueChangedCallback(_ => { if (!_loading) ApplyVideo(); });

            // ── AUDIO ──
            _sldMaster    = container.Q<Slider>("sldMaster");
            _sldMusic     = container.Q<Slider>("sldMusic");
            _sldSfx       = container.Q<Slider>("sldSfx");
            _sldEchoVoice = container.Q<Slider>("sldEchoVoice");

            _lblMasterVal    = container.Q<Label>("lblMasterVal");
            _lblMusicVal     = container.Q<Label>("lblMusicVal");
            _lblSfxVal       = container.Q<Label>("lblSfxVal");
            _lblEchoVoiceVal = container.Q<Label>("lblEchoVoiceVal");

            if (_sldMaster != null)
                _sldMaster.RegisterValueChangedCallback(evt => { SetPercentLabel(_lblMasterVal, evt.newValue); if (!_loading) { EchoesSettings.MasterVolume = evt.newValue; EchoesSettings.ApplyAudio(); } });
            if (_sldMusic != null)
                _sldMusic.RegisterValueChangedCallback(evt => { SetPercentLabel(_lblMusicVal, evt.newValue); if (!_loading) { EchoesSettings.MusicVolume = evt.newValue; EchoesSettings.ApplyAudio(); } });
            if (_sldSfx != null)
                _sldSfx.RegisterValueChangedCallback(evt => { SetPercentLabel(_lblSfxVal, evt.newValue); if (!_loading) { EchoesSettings.SfxVolume = evt.newValue; EchoesSettings.ApplyAudio(); } });
            if (_sldEchoVoice != null)
                _sldEchoVoice.RegisterValueChangedCallback(evt => { SetPercentLabel(_lblEchoVoiceVal, evt.newValue); if (!_loading) { EchoesSettings.EchoVolume = evt.newValue; EchoesSettings.ApplyAudio(); } });

            // ── CONTROLES ──
            _btnSensLow        = container.Q<Button>("btnSensLow");
            _btnSensMed        = container.Q<Button>("btnSensMed");
            _btnSensHigh       = container.Q<Button>("btnSensHigh");
            _sensitivitySlider = container.Q<Slider>("SensitivitySlider");
            _lblCamSensVal     = container.Q<Label>("lblCamSensVal");

            if (_btnSensLow != null)  _btnSensLow.clicked  += () => SetSensitivityPreset(0.5f);
            if (_btnSensMed != null)  _btnSensMed.clicked  += () => SetSensitivityPreset(1.0f);
            if (_btnSensHigh != null) _btnSensHigh.clicked += () => SetSensitivityPreset(2.0f);

            if (_sensitivitySlider != null)
            {
                _sensitivitySlider.RegisterValueChangedCallback(evt =>
                {
                    SetText(_lblCamSensVal, evt.newValue.ToString("F1"));
                    UpdateSensitivityPresetUI(evt.newValue);
                    if (_loading) return;
                    EchoesSettings.Sensitivity = evt.newValue;
                    EchoesSettings.ApplySensitivity();
                });
            }

            // ── ACCESIBILIDAD ──
            _textSizeDropdown = container.Q<DropdownField>("TextSizeDropdown");
            if (_textSizeDropdown != null)
            {
                _textSizeDropdown.choices = new List<string>(GameSettings.UIScaleNames);
                _textSizeDropdown.RegisterValueChangedCallback(evt => { if (!_loading) ApplyUIScale(evt.newValue); });
            }

            _highContrastToggle  = container.Q<Toggle>("HighContrastToggle");
            _subtitlesToggle     = container.Q<Toggle>("SubtitlesToggle");
            _subtitleSizeSlider  = container.Q<Slider>("SubtitleSizeSlider");
            _subtitleBgToggle    = container.Q<Toggle>("SubtitleBgToggle");
            _reduceFlashesToggle = container.Q<Toggle>("ReduceFlashesToggle");
            _reduceMotionToggle  = container.Q<Toggle>("ReduceMotionToggle");

            if (_highContrastToggle != null)
                _highContrastToggle.RegisterValueChangedCallback(evt => { if (!_loading) { EchoesSettings.HighContrast = evt.newValue; EchoesSettings.ApplyAccessibility(); } });
            if (_reduceMotionToggle != null)
                _reduceMotionToggle.RegisterValueChangedCallback(evt => { if (!_loading) { EchoesSettings.ReduceMotion = evt.newValue; EchoesSettings.ApplyAccessibility(); } });
            // Estos cuatro se consultaban en el UXML y no se registraba nada:
            // eran controles muertos que se movían y no hacían nada.
            if (_subtitlesToggle != null)
                _subtitlesToggle.RegisterValueChangedCallback(evt => { if (!_loading) { EchoesSettings.Subtitles = evt.newValue; EchoesSettings.ApplyAccessibility(); } });
            if (_subtitleBgToggle != null)
                _subtitleBgToggle.RegisterValueChangedCallback(evt => { if (!_loading) { EchoesSettings.SubtitleBackground = evt.newValue; EchoesSettings.ApplyAccessibility(); } });
            if (_subtitleSizeSlider != null)
                _subtitleSizeSlider.RegisterValueChangedCallback(evt => { if (!_loading) { EchoesSettings.SubtitleSize = evt.newValue; EchoesSettings.ApplyAccessibility(); } });
            if (_reduceFlashesToggle != null)
                _reduceFlashesToggle.RegisterValueChangedCallback(evt => { if (!_loading) EchoesSettings.ReduceFlashes = evt.newValue; });

            // ── GAMEPLAY ──
            _extraRecordTimeSlider = container.Q<Slider>("ExtraRecordTimeSlider");
            _lblExtraRecordTimeVal = container.Q<Label>("lblExtraRecordTimeVal");
            if (_extraRecordTimeSlider != null)
            {
                _extraRecordTimeSlider.RegisterValueChangedCallback(evt =>
                {
                    SetText(_lblExtraRecordTimeVal, Mathf.RoundToInt(evt.newValue) + "%");
                    if (_loading) return;
                    EchoesSettings.ExtraRecordTimePercent = evt.newValue;
                    EchoesSettings.ApplyExtraRecordTime();
                });
            }

            _sldFog    = container.Q<Slider>("sldFog");
            _sldEcho   = container.Q<Slider>("sldEcho");
            _lblFogVal = container.Q<Label>("lblFogVal");
            _lblEchoVal = container.Q<Label>("lblEchoVal");

            if (_sldFog != null)
            {
                _sldFog.RegisterValueChangedCallback(evt =>
                {
                    SetText(_lblFogVal, evt.newValue.ToString("F3"));
                    if (_loading) return;
                    EchoesSettings.FogDensity = evt.newValue;
                    EchoesSettings.ApplyFog();
                });
            }
            if (_sldEcho != null)
            {
                _sldEcho.RegisterValueChangedCallback(evt =>
                {
                    SetPercentLabel(_lblEchoVal, evt.newValue);
                    if (_loading) return;
                    EchoesSettings.EchoOpacity = evt.newValue;
                    EchoesSettings.ApplyEchoOpacity();
                });
            }

            // ── BOTONES ──
            _btnRestoreDefaults = container.Q<Button>("btnRestoreDefaults");
            _btnSettingsBack    = container.Q<Button>("btnSettingsBack");
            _btnSettingsApply   = container.Q<Button>("btnSettingsApply");
            _btnDeleteProgress  = container.Q<Button>("btnDeleteProgress");

            if (_btnRestoreDefaults != null) _btnRestoreDefaults.clicked += RestoreFactoryDefaults;
            if (_btnSettingsBack != null)    _btnSettingsBack.clicked    += HideSettings;
            if (_btnSettingsApply != null)   _btnSettingsApply.clicked   += ApplyAll;
            if (_btnDeleteProgress != null)  _btnDeleteProgress.clicked  += ConfirmDeleteAllProgress;

            LoadCurrentSettings();
        }

        void ShowTab(string tabName)
        {
            _panelVideo?.AddToClassList("hidden");
            _panelAudio?.AddToClassList("hidden");
            _panelControles?.AddToClassList("hidden");
            _panelAccesibilidad?.AddToClassList("hidden");
            _panelGameplay?.AddToClassList("hidden");

            _tabVideo?.RemoveFromClassList("settings-tab--active");
            _tabAudio?.RemoveFromClassList("settings-tab--active");
            _tabControles?.RemoveFromClassList("settings-tab--active");
            _tabAccesibilidad?.RemoveFromClassList("settings-tab--active");
            _tabGameplay?.RemoveFromClassList("settings-tab--active");

            switch (tabName)
            {
                case "Video": _panelVideo?.RemoveFromClassList("hidden"); _tabVideo?.AddToClassList("settings-tab--active"); break;
                case "Audio": _panelAudio?.RemoveFromClassList("hidden"); _tabAudio?.AddToClassList("settings-tab--active"); break;
                case "Controles": _panelControles?.RemoveFromClassList("hidden"); _tabControles?.AddToClassList("settings-tab--active"); break;
                case "Accesibilidad": _panelAccesibilidad?.RemoveFromClassList("hidden"); _tabAccesibilidad?.AddToClassList("settings-tab--active"); break;
                case "Gameplay": _panelGameplay?.RemoveFromClassList("hidden"); _tabGameplay?.AddToClassList("settings-tab--active"); break;
            }
        }

        void SetupResolutions()
        {
            if (_resDropdown == null) return;

            Resolution[] resolutions = Screen.resolutions;
            _filteredResolutions = new List<Resolution>();
            var options = new List<string>();
            var seen = new HashSet<string>();

            int savedW = PlayerPrefs.GetInt(EchoesSettings.KeyResWidth, Screen.width);
            int savedH = PlayerPrefs.GetInt(EchoesSettings.KeyResHeight, Screen.height);
            int currentResIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                // Screen.resolutions repite la misma resolución una vez por refresco;
                // sin deduplicar, el desplegable sale con veinte entradas iguales.
                if (!seen.Add(option)) continue;

                _filteredResolutions.Add(resolutions[i]);
                options.Add(option);
                if (resolutions[i].width == savedW && resolutions[i].height == savedH)
                    currentResIndex = options.Count - 1;
            }

            _resDropdown.choices = options;
            if (options.Count > 0)
                _resDropdown.index = Mathf.Clamp(currentResIndex, 0, options.Count - 1);
        }

        /// <summary>
        /// Rellena los controles con lo guardado. <see cref="_loading"/> evita que
        /// cada asignación dispare su Apply* con el estado a medio construir.
        /// </summary>
        void LoadCurrentSettings()
        {
            _loading = true;
            try
            {
                SetSliderValue(_sldMaster, EchoesSettings.MasterVolume, _lblMasterVal);
                SetSliderValue(_sldMusic, EchoesSettings.MusicVolume, _lblMusicVal);
                SetSliderValue(_sldSfx, EchoesSettings.SfxVolume, _lblSfxVal);
                SetSliderValue(_sldEchoVoice, EchoesSettings.EchoVolume, _lblEchoVoiceVal);

                if (_fullscreenToggle != null) _fullscreenToggle.value = EchoesSettings.Fullscreen;
                if (_vsyncToggle != null) _vsyncToggle.value = EchoesSettings.VSync;
                if (_scaleDropdown != null) _scaleDropdown.value = GameSettings.UIScaleName;
                if (_textSizeDropdown != null) _textSizeDropdown.value = GameSettings.UIScaleName;

                float sens = EchoesSettings.Sensitivity;
                if (_sensitivitySlider != null) _sensitivitySlider.value = sens;
                SetText(_lblCamSensVal, sens.ToString("F1"));
                UpdateSensitivityPresetUI(sens);

                float fog = EchoesSettings.FogDensity;
                if (_sldFog != null) _sldFog.value = fog;
                SetText(_lblFogVal, fog.ToString("F3"));

                float echo = EchoesSettings.EchoOpacity;
                if (_sldEcho != null) _sldEcho.value = echo;
                SetPercentLabel(_lblEchoVal, echo);

                if (_highContrastToggle != null)  _highContrastToggle.value  = EchoesSettings.HighContrast;
                if (_reduceMotionToggle != null)  _reduceMotionToggle.value  = EchoesSettings.ReduceMotion;
                if (_reduceFlashesToggle != null) _reduceFlashesToggle.value = EchoesSettings.ReduceFlashes;
                if (_subtitlesToggle != null)     _subtitlesToggle.value     = EchoesSettings.Subtitles;
                if (_subtitleBgToggle != null)    _subtitleBgToggle.value    = EchoesSettings.SubtitleBackground;
                if (_subtitleSizeSlider != null)  _subtitleSizeSlider.value  = EchoesSettings.SubtitleSize;

                float extra = EchoesSettings.ExtraRecordTimePercent;
                if (_extraRecordTimeSlider != null) _extraRecordTimeSlider.value = extra;
                SetText(_lblExtraRecordTimeVal, Mathf.RoundToInt(extra) + "%");
            }
            finally
            {
                _loading = false;
            }
        }

        // ── Helpers de UI (todos tolerantes a null: antes un elemento ausente
        //    lanzaba NullReference y abortaba el resto del cableado) ──

        static void SetText(Label lbl, string text)
        {
            if (lbl != null) lbl.text = text;
        }

        static void SetPercentLabel(Label lbl, float val)
        {
            if (lbl != null) lbl.text = Mathf.RoundToInt(val * 100f) + "%";
        }

        static void SetSliderValue(Slider slider, float value, Label label)
        {
            if (slider != null) slider.value = value;
            SetPercentLabel(label, value);
        }

        void UpdateSensitivityPresetUI(float val)
        {
            _btnSensLow?.RemoveFromClassList("sensitivity-preset--active");
            _btnSensMed?.RemoveFromClassList("sensitivity-preset--active");
            _btnSensHigh?.RemoveFromClassList("sensitivity-preset--active");

            if (val <= 0.7f) _btnSensLow?.AddToClassList("sensitivity-preset--active");
            else if (val >= 1.6f) _btnSensHigh?.AddToClassList("sensitivity-preset--active");
            else _btnSensMed?.AddToClassList("sensitivity-preset--active");
        }

        void SetSensitivityPreset(float value)
        {
            if (_sensitivitySlider != null)
            {
                // El slider dispara su callback y ese aplica y guarda.
                _sensitivitySlider.value = value;
            }
            else
            {
                EchoesSettings.Sensitivity = value;
                EchoesSettings.ApplySensitivity();
            }
            SetText(_lblCamSensVal, value.ToString("F1"));
            UpdateSensitivityPresetUI(value);
        }

        void HideSettings()
        {
            EchoesSettings.Save();
            if (_settingsContainer != null)
                _settingsContainer.AddToClassList("hidden");
            SettingsClosed?.Invoke();
        }

        void ApplyAll()
        {
            // Los callbacks ya han guardado cada valor según se tocaba; esto
            // reaplica todo junto y persiste a disco.
            ApplyVideo();
            EchoesSettings.ApplyAll();
            EchoesSettings.Save();
            HideSettings();
        }

        void ApplyVideo()
        {
            if (_fullscreenToggle != null) EchoesSettings.Fullscreen = _fullscreenToggle.value;
            if (_vsyncToggle != null) EchoesSettings.VSync = _vsyncToggle.value;

            if (_resDropdown != null && _filteredResolutions != null &&
                _resDropdown.index >= 0 && _resDropdown.index < _filteredResolutions.Count)
            {
                Resolution res = _filteredResolutions[_resDropdown.index];
                EchoesSettings.SetResolution(res.width, res.height);
            }

            EchoesSettings.ApplyVideo();
        }

        void ApplyUIScale(string scale)
        {
            GameSettings.SetUIScaleByName(scale);
            if (_scaleDropdown != null && _scaleDropdown.value != scale) _scaleDropdown.value = scale;
            if (_textSizeDropdown != null && _textSizeDropdown.value != scale) _textSizeDropdown.value = scale;
        }

        void RestoreFactoryDefaults()
        {
            // Solo las claves de ajustes. La versión anterior hacía
            // PlayerPrefs.DeleteAll() y se llevaba por delante niveles
            // desbloqueados, ecos anclados y estadísticas.
            EchoesSettings.RestoreDefaults();
            GameSettings.SetUIScale(1.0f);
            LoadCurrentSettings();
            SetupResolutions();
        }

        /// <summary>
        /// Borrado de progreso con confirmación en el propio juego. Antes usaba
        /// <c>UnityEditor.EditorUtility.DisplayDialog</c> sin <c>#if UNITY_EDITOR</c>:
        /// no existe fuera del editor, así que el botón era inútil en build — y de
        /// hecho impedía compilar el jugador.
        /// </summary>
        void ConfirmDeleteAllProgress()
        {
            const string title = "Borrar Progreso Completo";
            const string body =
                "Esto eliminará TODOS los datos guardados:\n" +
                "· Ecos anclados y Recuerdos completados\n" +
                "· Niveles desbloqueados\n" +
                "· Todos los ajustes\n\n" +
                "Esta acción es IRREVERSIBLE.";

            if (ModalManager.Instance == null)
            {
                Debug.LogWarning("[SettingsController] Sin ModalManager no hay confirmación posible; no se borra nada.");
                return;
            }

            ModalManager.Instance.ShowModal(title, body,
                onConfirm: DeleteAllProgress,
                onCancel: () => { });
        }

        void DeleteAllProgress()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            GameProgress.EnsureInitialized();
            EchoesSettings.ApplyAll();
            LoadCurrentSettings();
            Debug.Log("[SettingsController] Progreso completo borrado por el usuario.");

            var mainMenu = FindAnyObjectByType<MainMenuController>();
            mainMenu?.SendMessage("RefreshLevelCards", SendMessageOptions.DontRequireReceiver);
        }
    }
}
