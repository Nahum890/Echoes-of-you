using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Echoes.UI
{
    /// <summary>
    /// SettingsController — Singleton unificado para MainMenu + Pause.
    /// Categorías via EchoTabs: Video / Audio / Controles / Accesibilidad / Gameplay.
    /// Aplicación inmediata en runtime.
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

        /// <summary>Fired when the user closes settings (CERRAR TERMINAL / Aplicar cambios).
        /// Hosts like PauseMenu restore their previous UI state on this event.</summary>
        public static event System.Action SettingsClosed;

        [Header("Templates")]
        [SerializeField] VisualTreeAsset _settingsTemplate; // SettingsUI.uxml

        VisualElement _root;
        VisualElement _settingsContainer;

        // Tabs
        Button _tabVideo, _tabAudio, _tabControles, _tabAccesibilidad, _tabGameplay;
        VisualElement _panelVideo, _panelAudio, _panelControles, _panelAccesibilidad, _panelGameplay;

        // Video
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

        // Buttons
        Button _btnRestoreDefaults, _btnSettingsBack, _btnSettingsApply, _btnDeleteProgress;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_settingsTemplate == null)
            {
                _settingsTemplate = Resources.Load<VisualTreeAsset>("UI/SettingsUI");
                if (_settingsTemplate == null)
                    _settingsTemplate = Resources.Load<VisualTreeAsset>("SettingsUI");
            }
        }

        void OnEnable()
        {
            // Restore static ref after domain reload (DontDestroyOnLoad objects survive but statics are cleared)
            if (Instance == null)
                Instance = this;

            if (_settingsTemplate == null)
            {
                _settingsTemplate = Resources.Load<VisualTreeAsset>("UI/SettingsUI");
                if (_settingsTemplate == null)
                    _settingsTemplate = Resources.Load<VisualTreeAsset>("SettingsUI");
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Setup(VisualElement root, VisualTreeAsset template)
        {
            _root = root;
            _settingsTemplate = template;
            InitializeUI();
        }

        public void ShowInContainer(VisualElement container)
        {
            if (_settingsTemplate == null)
            {
                _settingsTemplate = Resources.Load<VisualTreeAsset>("UI/SettingsUI");
                if (_settingsTemplate == null)
                    _settingsTemplate = Resources.Load<VisualTreeAsset>("SettingsUI");
            }
            if (container == null || _settingsTemplate == null) return;

            container.Clear();
            var panel = _settingsTemplate.CloneTree();
            // CloneTree() returns a TemplateContainer wrapper with no classes and no height;
            // the settings root inside is position:absolute, so the wrapper collapses to 0 height.
            // Stretch the wrapper to fill the host panel or nothing renders and nothing is clickable.
            panel.style.position = Position.Absolute;
            panel.style.left = 0;
            panel.style.top = 0;
            panel.style.right = 0;
            panel.style.bottom = 0;

            var ssTheme = Resources.Load<StyleSheet>("UI/EchoesTheme");
            if (ssTheme != null && !panel.styleSheets.Contains(ssTheme))
                panel.styleSheets.Add(ssTheme);

            var ssSettings = Resources.Load<StyleSheet>("UI/SettingsUI");
            if (ssSettings != null && !panel.styleSheets.Contains(ssSettings))
                panel.styleSheets.Add(ssSettings);

            container.Add(panel);
            _settingsContainer = panel;
            InitializeUIFromContainer(panel);
            ShowTab("Video");
        }

        void InitializeUI()
        {
            if (_root == null || _settingsTemplate == null) return;
            _settingsContainer = _settingsTemplate.CloneTree();
            var ssTheme = Resources.Load<StyleSheet>("UI/EchoesTheme");
            if (ssTheme != null && !_settingsContainer.styleSheets.Contains(ssTheme))
                _settingsContainer.styleSheets.Add(ssTheme);
            var ssSettings = Resources.Load<StyleSheet>("UI/SettingsUI");
            if (ssSettings != null && !_settingsContainer.styleSheets.Contains(ssSettings))
                _settingsContainer.styleSheets.Add(ssSettings);
            _root.Add(_settingsContainer);
            InitializeUIFromContainer(_settingsContainer);
        }

        void InitializeUIFromContainer(VisualElement container)
        {
            // REMOVED: if (_initialized) return; — global flag prevents re-init in PauseMenu
            // Each container needs its own initialization since it's a fresh CloneTree()
            
            // Clear previous event subscriptions to avoid double-firing
            _tabVideo?.UnregisterCallback<ClickEvent>(_ => ShowTab("Video"));
            _tabAudio?.UnregisterCallback<ClickEvent>(_ => ShowTab("Audio"));
            _tabControles?.UnregisterCallback<ClickEvent>(_ => ShowTab("Controles"));
            _tabAccesibilidad?.UnregisterCallback<ClickEvent>(_ => ShowTab("Accesibilidad"));
            _tabGameplay?.UnregisterCallback<ClickEvent>(_ => ShowTab("Gameplay"));
            
            // Tabs
            _tabVideo         = container.Q<Button>("tabVideo");
            _tabAudio         = container.Q<Button>("tabAudio");
            _tabControles     = container.Q<Button>("tabControles");
            _tabAccesibilidad = container.Q<Button>("tabAccesibilidad");
            _tabGameplay      = container.Q<Button>("tabGameplay");

            _panelVideo       = container.Q("panelVideo");
            _panelAudio       = container.Q("panelAudio");
            _panelControles   = container.Q("panelControles");
            _panelAccesibilidad = container.Q("panelAccesibilidad");
            _panelGameplay    = container.Q("panelGameplay");

            // Unregister previous click handlers to avoid double-firing on re-init
            _tabVideo?.UnregisterCallback<ClickEvent>(OnTabVideoClicked);
            _tabAudio?.UnregisterCallback<ClickEvent>(OnTabAudioClicked);
            _tabControles?.UnregisterCallback<ClickEvent>(OnTabControlesClicked);
            _tabAccesibilidad?.UnregisterCallback<ClickEvent>(OnTabAccesibilidadClicked);
            _tabGameplay?.UnregisterCallback<ClickEvent>(OnTabGameplayClicked);

            if (_tabVideo != null) _tabVideo.RegisterCallback<ClickEvent>(OnTabVideoClicked);
            if (_tabAudio != null) _tabAudio.RegisterCallback<ClickEvent>(OnTabAudioClicked);
            if (_tabControles != null) _tabControles.RegisterCallback<ClickEvent>(OnTabControlesClicked);
            if (_tabAccesibilidad != null) _tabAccesibilidad.RegisterCallback<ClickEvent>(OnTabAccesibilidadClicked);
            if (_tabGameplay != null) _tabGameplay.RegisterCallback<ClickEvent>(OnTabGameplayClicked);

            // VIDEO
            _resDropdown = container.Q<DropdownField>("ResolutionDropdown");
            _fullscreenToggle = container.Q<Toggle>("FullscreenToggle");
            _vsyncToggle = container.Q<Toggle>("VsyncToggle");
            _scaleDropdown = container.Q<DropdownField>("ScaleDropdown");

            if (_scaleDropdown != null)
            {
                _scaleDropdown.choices = new List<string>(GameSettings.UIScaleNames);
                _scaleDropdown.value = GameSettings.UIScaleName;
                _scaleDropdown.RegisterValueChangedCallback(evt => ApplyUIScale(evt.newValue));
            }

            SetupResolutions();

            // AUDIO
            _sldMaster = container.Q<Slider>("sldMaster");
            _sldMusic = container.Q<Slider>("sldMusic");
            _sldSfx = container.Q<Slider>("sldSfx");
            _sldEchoVoice = container.Q<Slider>("sldEchoVoice");

            _lblMasterVal = container.Q<Label>("lblMasterVal");
            _lblMusicVal = container.Q<Label>("lblMusicVal");
            _lblSfxVal = container.Q<Label>("lblSfxVal");
            _lblEchoVoiceVal = container.Q<Label>("lblEchoVoiceVal");

            if (_sldMaster != null) _sldMaster.RegisterValueChangedCallback(evt => { UpdateLabel(_lblMasterVal, evt.newValue); ApplyAudio(); });
            if (_sldMusic != null) _sldMusic.RegisterValueChangedCallback(evt => { UpdateLabel(_lblMusicVal, evt.newValue); ApplyAudio(); });
            if (_sldSfx != null) _sldSfx.RegisterValueChangedCallback(evt => { UpdateLabel(_lblSfxVal, evt.newValue); ApplyAudio(); });
            if (_sldEchoVoice != null) _sldEchoVoice.RegisterValueChangedCallback(evt => { UpdateLabel(_lblEchoVoiceVal, evt.newValue); ApplyAudio(); });

            if (_fullscreenToggle != null) _fullscreenToggle.RegisterValueChangedCallback(evt => ApplyVideo());
            if (_vsyncToggle != null) _vsyncToggle.RegisterValueChangedCallback(evt => ApplyVideo());
            if (_resDropdown != null) _resDropdown.RegisterValueChangedCallback(evt => ApplyVideo());

            // CONTROLES
            _btnSensLow = container.Q<Button>("btnSensLow");
            _btnSensMed = container.Q<Button>("btnSensMed");
            _btnSensHigh = container.Q<Button>("btnSensHigh");
            _sensitivitySlider = container.Q<Slider>("SensitivitySlider");
            _lblCamSensVal = container.Q<Label>("lblCamSensVal");

            if (_btnSensLow != null) _btnSensLow.clicked += () => SetSensitivityPreset("Low", 0.5f);
            if (_btnSensMed != null) _btnSensMed.clicked += () => SetSensitivityPreset("Medium", 1.0f);
            if (_btnSensHigh != null) _btnSensHigh.clicked += () => SetSensitivityPreset("High", 2.0f);
            if (_sensitivitySlider != null)
            {
                _sensitivitySlider.RegisterValueChangedCallback(evt =>
                {
                    _lblCamSensVal.text = evt.newValue.ToString("F1");
                    PlayerPrefs.SetFloat("CameraSensitivity", evt.newValue);
                    ApplySensitivity();
                });
            }

            // ACCESIBILIDAD
            _textSizeDropdown = container.Q<DropdownField>("TextSizeDropdown");
            if (_textSizeDropdown != null)
            {
                _textSizeDropdown.choices = new List<string>(GameSettings.UIScaleNames);
                _textSizeDropdown.value = GameSettings.UIScaleName;
                _textSizeDropdown.RegisterValueChangedCallback(evt => ApplyUIScale(evt.newValue));
            }

            _highContrastToggle = container.Q<Toggle>("HighContrastToggle");
            _subtitlesToggle = container.Q<Toggle>("SubtitlesToggle");
            _subtitleSizeSlider = container.Q<Slider>("SubtitleSizeSlider");
            _subtitleBgToggle = container.Q<Toggle>("SubtitleBgToggle");
            _reduceFlashesToggle = container.Q<Toggle>("ReduceFlashesToggle");
            _reduceMotionToggle = container.Q<Toggle>("ReduceMotionToggle");

            if (_highContrastToggle != null)
            {
                _highContrastToggle.value = PlayerPrefs.GetInt("HighContrast", 0) == 1;
                _highContrastToggle.RegisterValueChangedCallback(evt => ApplyHighContrast(evt.newValue));
            }
            if (_reduceMotionToggle != null)
            {
                _reduceMotionToggle.value = PlayerPrefs.GetInt("ReduceMotion", 0) == 1;
                _reduceMotionToggle.RegisterValueChangedCallback(evt => ApplyReduceMotion(evt.newValue));
            }

            // GAMEPLAY
            _extraRecordTimeSlider = container.Q<Slider>("ExtraRecordTimeSlider");
            _lblExtraRecordTimeVal = container.Q<Label>("lblExtraRecordTimeVal");
            if (_extraRecordTimeSlider != null)
            {
                _extraRecordTimeSlider.value = PlayerPrefs.GetFloat("ExtraRecordTime", 0f);
                _lblExtraRecordTimeVal.text = Mathf.RoundToInt(_extraRecordTimeSlider.value) + "%";
                _extraRecordTimeSlider.RegisterValueChangedCallback(evt =>
                {
                    _lblExtraRecordTimeVal.text = Mathf.RoundToInt(evt.newValue) + "%";
                    PlayerPrefs.SetFloat("ExtraRecordTime", evt.newValue);
                    ApplyExtraRecordTime();
                });
            }

            _sldFog = container.Q<Slider>("sldFog");
            _sldEcho = container.Q<Slider>("sldEcho");
            _lblFogVal = container.Q<Label>("lblFogVal");
            _lblEchoVal = container.Q<Label>("lblEchoVal");

            if (_sldFog != null) _sldFog.RegisterValueChangedCallback(evt => { UpdateFogLabel(evt.newValue); ApplyFog(); });
            if (_sldEcho != null) _sldEcho.RegisterValueChangedCallback(evt => { UpdateLabel(_lblEchoVal, evt.newValue); ApplyEchoOpacity(); });

            // BUTTONS
            _btnRestoreDefaults = container.Q<Button>("btnRestoreDefaults");
            _btnSettingsBack = container.Q<Button>("btnSettingsBack");
            _btnSettingsApply = container.Q<Button>("btnSettingsApply");
            _btnDeleteProgress = container.Q<Button>("btnDeleteProgress");

            if (_btnRestoreDefaults != null) _btnRestoreDefaults.clicked += RestoreFactoryDefaults;
            if (_btnSettingsBack != null) _btnSettingsBack.clicked += HideSettings;
            if (_btnSettingsApply != null) _btnSettingsApply.clicked += ApplyAll;
            if (_btnDeleteProgress != null) _btnDeleteProgress.clicked += DeleteAllProgress;

            LoadCurrentSettings();
        }

        void ShowTab(string tabName)
        {
            // Panels use .settings-panel.hidden (display:none) and .settings-tab--active for the selected tab
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
            List<string> options = new List<string>();
            int currentResIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                _filteredResolutions.Add(resolutions[i]);
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);
                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                    currentResIndex = i;
            }

            _resDropdown.choices = options;
            if (options.Count > 0) _resDropdown.index = Mathf.Clamp(currentResIndex, 0, options.Count - 1);
        }

        void LoadCurrentSettings()
        {
            var audioMgr = EchoesAudioManager.EnsureExists();
            if (_sldMaster != null) _sldMaster.value = audioMgr != null ? audioMgr.GetMasterVolume() : PlayerPrefs.GetFloat("MasterVolume", 0.84f);
            if (_sldMusic != null) _sldMusic.value = audioMgr != null ? audioMgr.GetMusicVolume() : PlayerPrefs.GetFloat("MusicVolume", 0.6f);
            if (_sldSfx != null) _sldSfx.value = audioMgr != null ? audioMgr.GetSFXVolume() : PlayerPrefs.GetFloat("SfxVolume", 0.72f);
            if (_sldEchoVoice != null) _sldEchoVoice.value = audioMgr != null ? audioMgr.GetEchoVolume() : PlayerPrefs.GetFloat("EchoVolume", 0.7f);

            UpdateLabel(_lblMasterVal, _sldMaster.value);
            UpdateLabel(_lblMusicVal, _sldMusic.value);
            UpdateLabel(_lblSfxVal, _sldSfx.value);
            UpdateLabel(_lblEchoVoiceVal, _sldEchoVoice.value);

            if (_fullscreenToggle != null) _fullscreenToggle.value = Screen.fullScreen;
            if (_vsyncToggle != null) _vsyncToggle.value = QualitySettings.vSyncCount > 0;
            if (_scaleDropdown != null) _scaleDropdown.value = GameSettings.UIScaleName;
            if (_textSizeDropdown != null) _textSizeDropdown.value = GameSettings.UIScaleName;

            float sens = PlayerPrefs.GetFloat("CameraSensitivity", 1f);
            if (_sensitivitySlider != null) _sensitivitySlider.value = sens;
            _lblCamSensVal.text = sens.ToString("F1");
            UpdateSensitivityPresetUI(sens);

            float fog = PlayerPrefs.GetFloat("FogDensity", 0.035f);
            if (_sldFog != null) _sldFog.value = fog;
            UpdateFogLabel(fog);

            float echo = PlayerPrefs.GetFloat("EchoOpacity", 0.6f);
            if (_sldEcho != null) _sldEcho.value = echo;
            UpdateLabel(_lblEchoVal, echo);

            // Accesibilidad
            if (_highContrastToggle != null) _highContrastToggle.value = PlayerPrefs.GetInt("HighContrast", 0) == 1;
            if (_reduceMotionToggle != null) _reduceMotionToggle.value = PlayerPrefs.GetInt("ReduceMotion", 0) == 1;

            // Gameplay
            if (_extraRecordTimeSlider != null)
            {
                _extraRecordTimeSlider.value = PlayerPrefs.GetFloat("ExtraRecordTime", 0f);
                _lblExtraRecordTimeVal.text = Mathf.RoundToInt(_extraRecordTimeSlider.value) + "%";
            }
        }

        void UpdateLabel(Label lbl, float val)
        {
            if (lbl != null) lbl.text = Mathf.RoundToInt(val * 100f) + "%";
        }

        void UpdateFogLabel(float val)
        {
            if (_lblFogVal != null) _lblFogVal.text = val.ToString("F3");
        }

        void UpdateSensitivityPresetUI(float val)
        {
            _btnSensLow?.RemoveFromClassList("sensitivity-preset--active");
            _btnSensMed?.RemoveFromClassList("sensitivity-preset--active");
            _btnSensHigh?.RemoveFromClassList("sensitivity-preset--active");

            if (Mathf.Approximately(val, 0.5f)) _btnSensLow?.AddToClassList("sensitivity-preset--active");
            else if (Mathf.Approximately(val, 2.0f)) _btnSensHigh?.AddToClassList("sensitivity-preset--active");
            else _btnSensMed?.AddToClassList("sensitivity-preset--active");
        }

        void SetSensitivityPreset(string name, float value)
        {
            if (_sensitivitySlider != null) _sensitivitySlider.value = value;
            _lblCamSensVal.text = value.ToString("F1");
            UpdateSensitivityPresetUI(value);
        }

        void ShowSettings()
        {
            if (_settingsContainer != null)
                _settingsContainer.RemoveFromClassList("hidden");
        }

        void HideSettings()
        {
            if (_settingsContainer != null)
                _settingsContainer.AddToClassList("hidden");
            SettingsClosed?.Invoke();
        }

        void ApplyAll()
        {
            ApplyAudio();
            ApplyVideo();
            ApplySensitivity();
            ApplyFog();
            ApplyEchoOpacity();
            ApplyHighContrast(_highContrastToggle.value);
            ApplyReduceMotion(_reduceMotionToggle.value);
            ApplyExtraRecordTime();
            PlayerPrefs.Save();
            HideSettings();
        }

        void ApplyAudio()
        {
            var audioMgr = EchoesAudioManager.EnsureExists();
            float master = _sldMaster?.value ?? 0.84f;
            float music = _sldMusic?.value ?? 0.6f;
            float sfx = _sldSfx?.value ?? 0.72f;
            float echoVoice = _sldEchoVoice?.value ?? 0.7f;

            if (audioMgr != null)
            {
                audioMgr.SetMasterVolume(master);
                audioMgr.SetMusicVolume(music);
                audioMgr.SetSFXVolume(sfx);
                audioMgr.SetEchoVolume(echoVoice);
            }
            else
            {
                AudioListener.volume = master;
                PlayerPrefs.SetFloat("MasterVolume", master);
                PlayerPrefs.SetFloat("MusicVolume", music);
                PlayerPrefs.SetFloat("SfxVolume", sfx);
                PlayerPrefs.SetFloat("EchoVolume", echoVoice);
            }
        }

        void ApplyVideo()
        {
            if (_fullscreenToggle != null) Screen.fullScreen = _fullscreenToggle.value;
            if (_vsyncToggle != null) QualitySettings.vSyncCount = _vsyncToggle.value ? 1 : 0;
            if (_resDropdown != null && _filteredResolutions != null && _resDropdown.index >= 0 && _resDropdown.index < _filteredResolutions.Count)
            {
                Resolution res = _filteredResolutions[_resDropdown.index];
                Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            }
        }

        void ApplyUIScale(string scale)
        {
            GameSettings.SetUIScaleByName(scale);
            if (_scaleDropdown != null && _scaleDropdown.value != scale) _scaleDropdown.value = scale;
            if (_textSizeDropdown != null && _textSizeDropdown.value != scale) _textSizeDropdown.value = scale;
        }

        void ApplySensitivity()
        {
            float sens = _sensitivitySlider?.value ?? 1f;
            PlayerPrefs.SetFloat("CameraSensitivity", sens);
            var cam = FindAnyObjectByType<ThirdPersonCamera>();
            cam?.SendMessage("ApplySavedSensitivity", SendMessageOptions.DontRequireReceiver);
            UpdateSensitivityPresetUI(sens);
        }

        void ApplyFog()
        {
            float fog = _sldFog?.value ?? 0.035f;
            PlayerPrefs.SetFloat("FogDensity", fog);
            RenderSettings.fogDensity = fog;
            RenderSettings.fog = fog > 0f;
        }

        void ApplyEchoOpacity()
        {
            float echo = _sldEcho?.value ?? 0.6f;
            PlayerPrefs.SetFloat("EchoOpacity", echo);
            var allPlaybacks = FindObjectsByType<EchoPlayback>(FindObjectsInactive.Exclude);
            foreach (var playback in allPlaybacks) playback.SendMessage("ApplySavedEchoOpacity", SendMessageOptions.DontRequireReceiver);
        }

        void ApplyHighContrast(bool enabled)
        {
            PlayerPrefs.SetInt("HighContrast", enabled ? 1 : 0);
            var allDocs = FindObjectsByType<UIDocument>(FindObjectsInactive.Exclude);
            foreach (var doc in allDocs)
            {
                var root = doc.rootVisualElement;
                if (enabled) root.AddToClassList("high-contrast");
                else root.RemoveFromClassList("high-contrast");
            }
        }

        void ApplyReduceMotion(bool enabled)
        {
            PlayerPrefs.SetInt("ReduceMotion", enabled ? 1 : 0);
            var allDocs = FindObjectsByType<UIDocument>(FindObjectsInactive.Exclude);
            foreach (var doc in allDocs)
            {
                var root = doc.rootVisualElement;
                if (enabled) root.AddToClassList("reduce-motion");
                else root.RemoveFromClassList("reduce-motion");
            }
        }

        void ApplyExtraRecordTime()
        {
            float extra = _extraRecordTimeSlider?.value ?? 0f;
            PlayerPrefs.SetFloat("ExtraRecordTime", extra);
            // EchoRecorder picks this up in its maxRecordSeconds calculation
        }

        void RestoreFactoryDefaults()
        {
            if (_sldMaster != null) _sldMaster.value = 0.84f;
            if (_sldMusic != null) _sldMusic.value = 0.6f;
            if (_sldSfx != null) _sldSfx.value = 0.72f;
            if (_sldEchoVoice != null) _sldEchoVoice.value = 0.7f;
            UpdateLabel(_lblMasterVal, _sldMaster.value);
            UpdateLabel(_lblMusicVal, _sldMusic.value);
            UpdateLabel(_lblSfxVal, _sldSfx.value);
            UpdateLabel(_lblEchoVoiceVal, _sldEchoVoice.value);

            if (_fullscreenToggle != null) _fullscreenToggle.value = true;
            if (_vsyncToggle != null) _vsyncToggle.value = true;
            if (_scaleDropdown != null) _scaleDropdown.value = "Normal";
            ApplyUIScale("Normal");

            if (_sldFog != null) _sldFog.value = 0.035f;
            if (_sldEcho != null) _sldEcho.value = 0.6f;
            UpdateFogLabel(_sldFog.value);
            UpdateLabel(_lblEchoVal, _sldEcho.value);

            if (_sensitivitySlider != null) _sensitivitySlider.value = 1f;
            _lblCamSensVal.text = "1.0";
            UpdateSensitivityPresetUI(1f);

            if (_highContrastToggle != null) _highContrastToggle.value = false;
            if (_reduceMotionToggle != null) _reduceMotionToggle.value = false;
            ApplyHighContrast(false);
            ApplyReduceMotion(false);

            if (_extraRecordTimeSlider != null)
            {
                _extraRecordTimeSlider.value = 0f;
                _lblExtraRecordTimeVal.text = "0%";
            }

            if (_subtitlesToggle != null) _subtitlesToggle.value = false;
            if (_subtitleBgToggle != null) _subtitleBgToggle.value = false;
            if (_reduceFlashesToggle != null) _reduceFlashesToggle.value = false;

            PlayerPrefs.DeleteAll();
            LoadCurrentSettings();
        }

        void DeleteAllProgress()
        {
            // Confirmación doble para evitar borrado accidental
            if (UnityEditor.EditorUtility.DisplayDialog(
                "Borrar Progreso Completo",
                "Esto eliminará TODOS los datos guardados:\n- Ecos anclados y Recuerdos completados\n- Niveles desbloqueados\n- Todos los ajustes (video, audio, controles, accesibilidad, gameplay)\n\nEsta acción es IRREVERSIBLE.\n\n¿Estás seguro?",
                "Sí, borrar todo", "Cancelar"))
            {
                PlayerPrefs.DeleteAll();
                GameProgress.EnsureInitialized();
                LoadCurrentSettings();
                Debug.Log("[SettingsController] Progreso completo borrado por el usuario.");
                
                // Refresh level cards in main menu if present
                var mainMenu = FindAnyObjectByType<MainMenuController>();
                mainMenu?.SendMessage("RefreshLevelCards", SendMessageOptions.DontRequireReceiver);
            }
        }

        // Tab click handlers (named methods for proper unregistration on re-init)
        void OnTabVideoClicked(ClickEvent evt) => ShowTab("Video");
        void OnTabAudioClicked(ClickEvent evt) => ShowTab("Audio");
        void OnTabControlesClicked(ClickEvent evt) => ShowTab("Controles");
        void OnTabAccesibilidadClicked(ClickEvent evt) => ShowTab("Accesibilidad");
        void OnTabGameplayClicked(ClickEvent evt) => ShowTab("Gameplay");
    }
}