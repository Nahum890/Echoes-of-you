using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controlador del menú principal usando UI Toolkit.
/// Diseño "ESTADO DEL SISTEMA COGNITIVO" con side nav.
/// Requiere un UIDocument component en el mismo GameObject.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] string firstLevelScene = "Level_01";

    [Header("Debug")]
    [Tooltip("Si es true, carga Level_01 automáticamente sin esperar input")]
    [SerializeField] bool autoStartGame = false;



    UIDocument _doc;
    VisualElement _root;
    VisualElement _menuBg;
    Label _heroTitle;
    VisualElement _voidIntro;
    VisualElement _mainContent;

    // Panels
    VisualElement _settingsPanel;
    VisualElement _levelSelectPanel;

    // Main Menu Buttons
    Button _btnNewGame;
    Button _btnLevels;
    Button _btnSettings;
    Button _btnExit;
    Button _activeNavButton;

    // Settings Controls - Audio
    Slider _sldMaster;
    Slider _sldMusic;
    Slider _sldSfx;
    Label _lblMasterVal;
    Label _lblMusicVal;
    Label _lblSfxVal;

    // Settings Controls - Visuals
    DropdownField _resDropdown;
    Toggle _fullscreenToggle;
    Toggle _vsyncToggle;
    DropdownField _scaleDropdown;

    // Settings Controls - Neural
    Button _btnSensLow;
    Button _btnSensMed;
    Button _btnSensHigh;
    Label _lblSensVal;
    Slider _sensitivitySlider;
    Label _lblCamSensVal;

    // Fog Density & Echo Opacity Settings
    Slider _sldFog;
    Label _lblFogVal;
    Slider _sldEcho;
    Label _lblEchoVal;

    Slider _sldGameFog;
    Slider _sldGameSun;
    Slider _sldGameLights;
    Slider _sldGameAmbient;
    Slider _sldMenuText;
    Label _lblGameFogVal;
    Label _lblGameSunVal;
    Label _lblGameLightsVal;
    Label _lblGameAmbientVal;
    Label _lblMenuTextVal;

    Button _btnLightLiminal;
    Button _btnLightBruma;
    Button _btnLightClaridad;
    Button _btnLightPenumbra;
    string _activeLightingPresetId = "liminal";

    List<Resolution> _filteredResolutions;

    void OnEnable()
    {
        _doc = GetComponent<UIDocument>();
        if (_doc == null || _doc.rootVisualElement == null) return;
        _root = _doc.rootVisualElement;

        ApplySavedUIScale();
        ApplySavedMenuTextScale();

        // Background & Hero
        _menuBg = _root.Q("menu-bg");
        _heroTitle = _root.Q<Label>("hero-title");
        _voidIntro = _root.Q("void-intro");
        _mainContent = _root.Q("main-content");

        // Panels
        _settingsPanel = _root.Q("settings-panel");
        _levelSelectPanel = _root.Q("level-select-panel");

        // Side nav buttons
        _btnNewGame = _root.Q<Button>("nav-newgame");
        _btnLevels = _root.Q<Button>("nav-levels");
        _btnSettings = _root.Q<Button>("nav-settings");
        _btnExit = _root.Q<Button>("nav-exit");

        // Setup hover behaviors
        SetupHoverCallbacks();

        // Bind panel actions
        RegisterButtonClick("nav-newgame", StartNewGame);
        RegisterButtonClick("nav-levels", ShowStabilityMap);
        RegisterButtonClick("nav-settings", ShowSettings);
        RegisterButtonClick("nav-exit", QuitGame);

        GameProgress.EnsureInitialized();
        GameProgress.RecordSessionStarted();

        // Settings panel bindings
        _sldMaster = _root.Q<Slider>("sld-master");
        _sldMusic = _root.Q<Slider>("sld-music");
        _sldSfx = _root.Q<Slider>("sld-sfx");

        _lblMasterVal = _root.Q<Label>("lbl-master-val");
        _lblMusicVal = _root.Q<Label>("lbl-music-val");
        _lblSfxVal = _root.Q<Label>("lbl-sfx-val");

        _resDropdown = _root.Q<DropdownField>("ResolutionDropdown");
        _fullscreenToggle = _root.Q<Toggle>("FullscreenToggle");
        _vsyncToggle = _root.Q<Toggle>("VsyncToggle");
        _scaleDropdown = _root.Q<DropdownField>("ScaleDropdown");

        _btnSensLow = _root.Q<Button>("btn-sens-low");
        _btnSensMed = _root.Q<Button>("btn-sens-med");
        _btnSensHigh = _root.Q<Button>("btn-sens-high");
        _lblSensVal = _root.Q<Label>("lbl-sens-val");

        _sensitivitySlider = _root.Q<Slider>("SensitivitySlider");
        _lblCamSensVal = _root.Q<Label>("lbl-cam-sens-val");

        _sldFog = _root.Q<Slider>("sld-fog");
        _lblFogVal = _root.Q<Label>("lbl-fog-val");
        _sldEcho = _root.Q<Slider>("sld-echo");
        _lblEchoVal = _root.Q<Label>("lbl-echo-val");
        _sldGameFog = _root.Q<Slider>("sld-game-fog");
        _sldGameSun = _root.Q<Slider>("sld-game-sun");
        _sldGameLights = _root.Q<Slider>("sld-game-lights");
        _sldGameAmbient = _root.Q<Slider>("sld-game-ambient");
        _sldMenuText = _root.Q<Slider>("sld-menu-text");
        _lblGameFogVal = _root.Q<Label>("lbl-game-fog-val");
        _lblGameSunVal = _root.Q<Label>("lbl-game-sun-val");
        _lblGameLightsVal = _root.Q<Label>("lbl-game-lights-val");
        _lblGameAmbientVal = _root.Q<Label>("lbl-game-ambient-val");
        _lblMenuTextVal = _root.Q<Label>("lbl-menu-text-val");

        _btnLightLiminal = _root.Q<Button>("btn-light-liminal");
        _btnLightBruma = _root.Q<Button>("btn-light-bruma");
        _btnLightClaridad = _root.Q<Button>("btn-light-claridad");
        _btnLightPenumbra = _root.Q<Button>("btn-light-penumbra");
        if (_btnLightLiminal != null) _btnLightLiminal.clicked += () => ApplyLightingPresetUi("liminal");
        if (_btnLightBruma != null) _btnLightBruma.clicked += () => ApplyLightingPresetUi("bruma");
        if (_btnLightClaridad != null) _btnLightClaridad.clicked += () => ApplyLightingPresetUi("claridad");
        if (_btnLightPenumbra != null) _btnLightPenumbra.clicked += () => ApplyLightingPresetUi("penumbra");

        // Bind settings buttons
        RegisterButtonClick("btn-restore-defaults", RestoreFactoryDefaults);
        RegisterButtonClick("btn-settings-back", DiscardSettings);
        RegisterButtonClick("btn-settings-apply", ApplySettings);
        RegisterButtonClick("btn-levels-back", ShowStabilityMap);
        RegisterButtonClick("btn-reset-progress", OnResetProgressClicked);
        RegisterButtonClick("btn-reset-progress-confirm", ConfirmResetProgress);

        RefreshDashboard();
        BindLevelMapButtons();



        InitializeSettings();
        ShowVoidIntro();
    }

    void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        Time.timeScale = 1f;

        // Debug auto-start
        if (autoStartGame)
        {
            Invoke(nameof(AutoStart), 0.5f);
        }
    }

    void AutoStart()
    {
        LoadLevel(firstLevelScene);
    }

    void RegisterButtonClick(string name, System.Action action)
    {
        var btn = _root.Q<Button>(name);
        if (btn != null)
        {
            btn.clicked += action;
        }
        else
        {
            var el = _root.Q(name);
            if (el != null)
                el.RegisterCallback<ClickEvent>(_ => action());
        }
    }

    // --- Hover Background & Title Swap ---

    void SetupHoverCallbacks()
    {
        if (_btnNewGame != null)
        {
            _btnNewGame.RegisterCallback<MouseEnterEvent>(_ => OnNavHover(_btnNewGame, MainMenuCinematicWorld.MenuAmbience.Void, "NEURAL SYNC"));
            _btnNewGame.RegisterCallback<MouseLeaveEvent>(_ => OnNavHoverLeave(_btnNewGame));
        }
        if (_btnLevels != null)
        {
            _btnLevels.RegisterCallback<MouseEnterEvent>(_ => OnNavHover(_btnLevels, MainMenuCinematicWorld.MenuAmbience.Stability, "STABILITY"));
            _btnLevels.RegisterCallback<MouseLeaveEvent>(_ => OnNavHoverLeave(_btnLevels));
        }
        if (_btnSettings != null)
        {
            _btnSettings.RegisterCallback<MouseEnterEvent>(_ => OnNavHover(_btnSettings, MainMenuCinematicWorld.MenuAmbience.System, "CALIBRATION"));
            _btnSettings.RegisterCallback<MouseLeaveEvent>(_ => OnNavHoverLeave(_btnSettings));
        }
        if (_btnExit != null)
        {
            _btnExit.RegisterCallback<MouseEnterEvent>(_ => OnNavHover(_btnExit, MainMenuCinematicWorld.MenuAmbience.Disconnect, "VOID MAP"));
            _btnExit.RegisterCallback<MouseLeaveEvent>(_ => OnNavHoverLeave(_btnExit));
        }
    }

    void OnNavHover(Button btn, MainMenuCinematicWorld.MenuAmbience ambience, string title)
    {
        if (MainMenuCinematicWorld.Instance != null)
        {
            MainMenuCinematicWorld.Instance.SetAmbience(ambience);
        }

        if (_heroTitle != null)
        {
            _heroTitle.text = title;
        }

        btn.AddToClassList("nav-item--active");
    }

    void OnNavHoverLeave(Button btn)
    {
        if (MainMenuCinematicWorld.Instance != null)
        {
            MainMenuCinematicWorld.Instance.SetAmbience(GetActiveNavAmbience());
        }

        if (_heroTitle != null)
        {
            _heroTitle.text = GetActiveHeroTitle();
        }

        // Keep active selection styling only on the active nav button
        if (btn != _activeNavButton)
        {
            btn.RemoveFromClassList("nav-item--active");
        }
    }

    void SetActiveNav(Button activeBtn)
    {
        _activeNavButton = activeBtn;
        _btnNewGame?.RemoveFromClassList("nav-item--active");
        _btnLevels?.RemoveFromClassList("nav-item--active");
        _btnSettings?.RemoveFromClassList("nav-item--active");
        _btnExit?.RemoveFromClassList("nav-item--active");
        activeBtn?.AddToClassList("nav-item--active");

        if (MainMenuCinematicWorld.Instance != null)
        {
            MainMenuCinematicWorld.Instance.SetAmbience(GetActiveNavAmbience());
        }
    }

    MainMenuCinematicWorld.MenuAmbience GetActiveNavAmbience()
    {
        if (_activeNavButton == _btnNewGame) return MainMenuCinematicWorld.MenuAmbience.Void;
        if (_activeNavButton == _btnLevels) return MainMenuCinematicWorld.MenuAmbience.Stability;
        if (_activeNavButton == _btnSettings) return MainMenuCinematicWorld.MenuAmbience.System;
        if (_activeNavButton == _btnExit) return MainMenuCinematicWorld.MenuAmbience.Disconnect;
        return MainMenuCinematicWorld.MenuAmbience.Void;
    }

    string GetActiveHeroTitle()
    {
        if (_activeNavButton == _btnNewGame) return "VOID";
        if (_activeNavButton == _btnLevels) return "STABILITY";
        if (_activeNavButton == _btnSettings) return "CALIBRATION";
        if (_activeNavButton == _btnExit) return "VOID MAP";
        return "ISOLATED";
    }

    // --- Panel Switching ---

    void ShowVoidIntro()
    {
        _settingsPanel?.AddToClassList("hidden");
        _levelSelectPanel?.AddToClassList("hidden");
        _mainContent?.AddToClassList("hidden");
        _voidIntro?.RemoveFromClassList("hidden");

        if (_menuBg != null)
        {
            _menuBg.style.opacity = 0f;
        }

        if (_heroTitle != null)
            _heroTitle.text = "VOID";

        SetActiveNav(_btnNewGame);
    }

    void ShowStabilityMap()
    {
        _settingsPanel?.AddToClassList("hidden");
        _levelSelectPanel?.AddToClassList("hidden");
        _voidIntro?.AddToClassList("hidden");
        _mainContent?.RemoveFromClassList("hidden");

        if (_menuBg != null)
        {
            _menuBg.style.opacity = 0f;
        }

        if (_heroTitle != null)
            _heroTitle.text = "STABILITY";

        SetActiveNav(_btnLevels);
        RefreshDashboard();
    }

    void ShowSettings()
    {
        _levelSelectPanel?.AddToClassList("hidden");
        _voidIntro?.AddToClassList("hidden");
        _mainContent?.AddToClassList("hidden");
        _settingsPanel?.RemoveFromClassList("hidden");
        SetActiveNav(_btnSettings);
        LoadCurrentSettingsIntoUI();
    }

    // --- Actions ---

    void StartNewGame()
    {
        LoadLevel(firstLevelScene);
    }

    void LoadLevel(string levelName)
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(levelName);
        }
        else
        {
            PostProcessingSetup.PrepareForSceneReload();
            UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
        }
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- Settings & Calibration logic ---

    void InitializeSettings()
    {
        // UI Scale dropdown setup
        if (_scaleDropdown != null)
        {
            _scaleDropdown.choices = new List<string> { "Normal", "Large", "Extra Large" };
            _scaleDropdown.value = PlayerPrefs.GetString("UIScale", "Normal");
        }

        // Neural presets
        if (_btnSensLow != null) _btnSensLow.clicked += () => SelectSensitivityPreset("Low", 0.5f);
        if (_btnSensMed != null) _btnSensMed.clicked += () => SelectSensitivityPreset("Medium", 1.0f);
        if (_btnSensHigh != null) _btnSensHigh.clicked += () => SelectSensitivityPreset("High", 2.0f);

        // Sliders change updates labels
        if (_sldMaster != null) _sldMaster.RegisterValueChangedCallback(evt => UpdateLabel(_lblMasterVal, evt.newValue));
        if (_sldMusic != null) _sldMusic.RegisterValueChangedCallback(evt => UpdateLabel(_lblMusicVal, evt.newValue));
        if (_sldSfx != null) _sldSfx.RegisterValueChangedCallback(evt => UpdateLabel(_lblSfxVal, evt.newValue));
        if (_sensitivitySlider != null) _sensitivitySlider.RegisterValueChangedCallback(evt => UpdateSensitivityLabel(evt.newValue));
        if (_sldFog != null) _sldFog.RegisterValueChangedCallback(evt => UpdateFogLabel(evt.newValue));
        if (_sldEcho != null) _sldEcho.RegisterValueChangedCallback(evt => UpdateLabel(_lblEchoVal, evt.newValue));
        if (_sldGameFog != null) _sldGameFog.RegisterValueChangedCallback(evt => UpdateGameFogLabel(evt.newValue));
        if (_sldGameSun != null) _sldGameSun.RegisterValueChangedCallback(evt => UpdateGameSunLabel(evt.newValue));
        if (_sldGameLights != null) _sldGameLights.RegisterValueChangedCallback(evt => UpdateGameLightsLabel(evt.newValue));
        if (_sldGameAmbient != null) _sldGameAmbient.RegisterValueChangedCallback(evt => UpdateGameAmbientLabel(evt.newValue));
        if (_sldMenuText != null) _sldMenuText.RegisterValueChangedCallback(evt => UpdateMenuTextLabel(evt.newValue));

        // Resolutions
        if (_resDropdown != null)
        {
            Resolution[] resolutions = Screen.resolutions;
            _filteredResolutions = new List<Resolution>();
            List<string> options = new List<string>();
            int currentResIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                _filteredResolutions.Add(resolutions[i]);
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                    currentResIndex = i;
            }

            _resDropdown.choices = options;
            if (options.Count > 0)
            {
                _resDropdown.index = Mathf.Clamp(currentResIndex, 0, options.Count - 1);
            }
        }

        LoadCurrentSettingsIntoUI();
    }

    void LoadCurrentSettingsIntoUI()
    {
        var audioMgr = EchoesAudioManager.EnsureExists();
        if (_sldMaster != null) _sldMaster.value = audioMgr != null ? audioMgr.GetMasterVolume() : PlayerPrefs.GetFloat("MasterVolume", 0.84f);
        if (_sldMusic != null) _sldMusic.value = audioMgr != null ? audioMgr.GetMusicVolume() : PlayerPrefs.GetFloat("MusicVolume", 0.6f);
        if (_sldSfx != null) _sldSfx.value = audioMgr != null ? audioMgr.GetSFXVolume() : PlayerPrefs.GetFloat("SfxVolume", 0.72f);

        if (_lblMasterVal != null) UpdateLabel(_lblMasterVal, _sldMaster.value);
        if (_lblMusicVal != null) UpdateLabel(_lblMusicVal, _sldMusic.value);
        if (_lblSfxVal != null) UpdateLabel(_lblSfxVal, _sldSfx.value);

        if (_fullscreenToggle != null) _fullscreenToggle.value = Screen.fullScreen;
        if (_vsyncToggle != null) _vsyncToggle.value = QualitySettings.vSyncCount > 0;

        if (_scaleDropdown != null) _scaleDropdown.value = PlayerPrefs.GetString("UIScale", "Normal");

        float currentSens = PlayerPrefs.GetFloat("CameraSensitivity", 1f);
        if (_sensitivitySlider != null) _sensitivitySlider.value = currentSens;
        UpdateSensitivityLabel(currentSens);

        if (_sldFog != null) _sldFog.value = PlayerPrefs.GetFloat("FogDensity", RenderSettings.fog ? RenderSettings.fogDensity : 0.035f);
        if (_sldEcho != null) _sldEcho.value = PlayerPrefs.GetFloat("EchoOpacity", 0.6f);

        if (_lblFogVal != null) UpdateFogLabel(_sldFog.value);
        if (_lblEchoVal != null) UpdateLabel(_lblEchoVal, _sldEcho.value);

        if (_sldGameFog != null) _sldGameFog.value = EchoesPresentationSettings.GameFogDensity;
        if (_sldGameSun != null) _sldGameSun.value = EchoesPresentationSettings.GameSunIntensity;
        if (_sldGameLights != null) _sldGameLights.value = EchoesPresentationSettings.GamePointLightMultiplier;
        if (_sldGameAmbient != null) _sldGameAmbient.value = EchoesPresentationSettings.GameAmbientMultiplier;
        if (_sldMenuText != null) _sldMenuText.value = EchoesPresentationSettings.MenuTextScale;
        UpdateGameFogLabel(_sldGameFog != null ? _sldGameFog.value : EchoesPresentationSettings.DefaultGameFogDensity);
        UpdateGameSunLabel(_sldGameSun != null ? _sldGameSun.value : EchoesPresentationSettings.DefaultGameSunIntensity);
        UpdateGameLightsLabel(_sldGameLights != null ? _sldGameLights.value : EchoesPresentationSettings.DefaultGamePointLightMul);
        UpdateGameAmbientLabel(_sldGameAmbient != null ? _sldGameAmbient.value : EchoesPresentationSettings.DefaultGameAmbientMul);
        UpdateMenuTextLabel(_sldMenuText != null ? _sldMenuText.value : EchoesPresentationSettings.DefaultMenuTextScale);

        // Preset button highlights based on sensitivity value
        if (Mathf.Approximately(currentSens, 0.5f)) SelectSensitivityPresetUI("Low");
        else if (Mathf.Approximately(currentSens, 2.0f)) SelectSensitivityPresetUI("High");
        else SelectSensitivityPresetUI("Medium");
    }

    void UpdateLabel(Label lbl, float val)
    {
        if (lbl != null) lbl.text = Mathf.RoundToInt(val * 100f) + "%";
    }

    void UpdateFogLabel(float val)
    {
        if (_lblFogVal != null) _lblFogVal.text = val.ToString("F3");
    }

    void UpdateSensitivityLabel(float val)
    {
        if (_lblCamSensVal != null) _lblCamSensVal.text = val.ToString("F1");
    }

    void SelectSensitivityPreset(string name, float value)
    {
        if (_sensitivitySlider != null) _sensitivitySlider.value = value;
        if (_lblSensVal != null) _lblSensVal.text = name;
        SelectSensitivityPresetUI(name);
    }

    void SelectSensitivityPresetUI(string activePreset)
    {
        _btnSensLow?.RemoveFromClassList("preset-button--active");
        _btnSensMed?.RemoveFromClassList("preset-button--active");
        _btnSensHigh?.RemoveFromClassList("preset-button--active");

        if (activePreset == "Low") _btnSensLow?.AddToClassList("preset-button--active");
        else if (activePreset == "High") _btnSensHigh?.AddToClassList("preset-button--active");
        else _btnSensMed?.AddToClassList("preset-button--active");

        if (_lblSensVal != null) _lblSensVal.text = activePreset;
    }

    void ApplySettings()
    {
        // 1. Audio
        float master = _sldMaster != null ? _sldMaster.value : 0.84f;
        float music = _sldMusic != null ? _sldMusic.value : 0.6f;
        float sfx = _sldSfx != null ? _sldSfx.value : 0.72f;

        var audioMgr = EchoesAudioManager.EnsureExists();
        if (audioMgr != null)
        {
            audioMgr.SetMasterVolume(master);
            audioMgr.SetMusicVolume(music);
            audioMgr.SetSFXVolume(sfx);
        }
        else
        {
            AudioListener.volume = master;
            PlayerPrefs.SetFloat("MasterVolume", master);
            PlayerPrefs.SetFloat("MusicVolume", music);
            PlayerPrefs.SetFloat("SfxVolume", sfx);
        }

        // Broadcast audio settings
        var levelCtrl = FindAnyObjectByType<LevelRuntimeController>();
        if (levelCtrl != null) levelCtrl.SendMessage("ApplySavedAudioSettings", SendMessageOptions.DontRequireReceiver);

        // 2. Visuals
        if (_fullscreenToggle != null) Screen.fullScreen = _fullscreenToggle.value;
        if (_vsyncToggle != null) QualitySettings.vSyncCount = _vsyncToggle.value ? 1 : 0;

        if (_resDropdown != null && _filteredResolutions != null && _resDropdown.index >= 0 && _resDropdown.index < _filteredResolutions.Count)
        {
            Resolution res = _filteredResolutions[_resDropdown.index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        }

        // 3. UI Scale
        if (_scaleDropdown != null)
        {
            string oldScale = PlayerPrefs.GetString("UIScale", "Normal");
            string newScale = _scaleDropdown.value;
            PlayerPrefs.SetString("UIScale", newScale);

            if (oldScale != newScale)
            {
                ApplySavedUIScale();
                // Broadcast live UI Scale update to any other open UI document
                var allDocs = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
                foreach (var doc in allDocs)
                {
                    if (doc != _doc && doc.gameObject != gameObject)
                    {
                        doc.gameObject.SendMessage("ApplySavedUIScale", SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

        // 4. Sensitivity
        float sens = _sensitivitySlider != null ? _sensitivitySlider.value : 1.0f;
        PlayerPrefs.SetFloat("CameraSensitivity", sens);

        // Broadcast camera sensitivity
        var cam = FindAnyObjectByType<ThirdPersonCamera>();
        if (cam != null) cam.SendMessage("ApplySavedSensitivity", SendMessageOptions.DontRequireReceiver);

        // 5. Fog Density
        float fogDensity = _sldFog != null ? _sldFog.value : 0.035f;
        PlayerPrefs.SetFloat("FogDensity", fogDensity);
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fog = fogDensity > 0f;

        // 6. Echo Opacity
        float echoOpacity = _sldEcho != null ? _sldEcho.value : 0.6f;
        PlayerPrefs.SetFloat("EchoOpacity", echoOpacity);

        // Broadcast echo opacity
        var allRecorders = FindObjectsByType<EchoPlayback>(FindObjectsSortMode.None);
        foreach (var playback in allRecorders)
        {
            playback.SendMessage("ApplySavedEchoOpacity", SendMessageOptions.DontRequireReceiver);
        }

        float gameFog = _sldGameFog != null ? _sldGameFog.value : EchoesPresentationSettings.DefaultGameFogDensity;
        float gameSun = _sldGameSun != null ? _sldGameSun.value : EchoesPresentationSettings.DefaultGameSunIntensity;
        float gameLights = _sldGameLights != null ? _sldGameLights.value : EchoesPresentationSettings.DefaultGamePointLightMul;
        float gameAmbient = _sldGameAmbient != null ? _sldGameAmbient.value : EchoesPresentationSettings.DefaultGameAmbientMul;
        float menuText = _sldMenuText != null ? _sldMenuText.value : EchoesPresentationSettings.DefaultMenuTextScale;
        EchoesPresentationSettings.SaveLighting(gameFog, gameSun, gameLights, gameAmbient);
        EchoesPresentationSettings.Save(
            EchoesPresentationSettings.CharacterVisualScale,
            EchoesPresentationSettings.AnimationPlaybackSpeed,
            EchoesPresentationSettings.ProceduralMotionEnabled,
            menuText);
        ApplySavedMenuTextScale();

        PlayerPrefs.Save();
        ShowStabilityMap();
    }

    void DiscardSettings()
    {
        if (_mainContent != null && !_mainContent.ClassListContains("hidden"))
            ShowStabilityMap();
        else
            ShowVoidIntro();
    }

    void ApplyLightingPresetUi(string presetId)
    {
        _activeLightingPresetId = presetId;
        EchoesPresentationSettings.ApplyLightingPreset(presetId);

        if (EchoesPresentationSettings.TryGetLightingPreset(presetId, out float fog, out float sun, out float point, out float ambient))
        {
            if (_sldGameFog != null) _sldGameFog.value = fog;
            if (_sldGameSun != null) _sldGameSun.value = sun;
            if (_sldGameLights != null) _sldGameLights.value = point;
            if (_sldGameAmbient != null) _sldGameAmbient.value = ambient;
            UpdateGameFogLabel(fog);
            UpdateGameSunLabel(sun);
            UpdateGameLightsLabel(point);
            UpdateGameAmbientLabel(ambient);
        }

        SetLightingPresetButtonActive(presetId);
    }

    void SetLightingPresetButtonActive(string presetId)
    {
        _btnLightLiminal?.RemoveFromClassList("preset-button--active");
        _btnLightBruma?.RemoveFromClassList("preset-button--active");
        _btnLightClaridad?.RemoveFromClassList("preset-button--active");
        _btnLightPenumbra?.RemoveFromClassList("preset-button--active");

        switch (presetId)
        {
            case "bruma": _btnLightBruma?.AddToClassList("preset-button--active"); break;
            case "claridad": _btnLightClaridad?.AddToClassList("preset-button--active"); break;
            case "penumbra": _btnLightPenumbra?.AddToClassList("preset-button--active"); break;
            default: _btnLightLiminal?.AddToClassList("preset-button--active"); break;
        }
    }

    void RestoreFactoryDefaults()
    {
        if (_sldMaster != null) _sldMaster.value = 0.84f;
        if (_sldMusic != null) _sldMusic.value = 0.60f;
        if (_sldSfx != null) _sldSfx.value = 0.72f;

        if (_fullscreenToggle != null) _fullscreenToggle.value = true;
        if (_vsyncToggle != null) _vsyncToggle.value = true;

        if (_scaleDropdown != null) _scaleDropdown.value = "Normal";

        if (_sldFog != null) _sldFog.value = 0.035f;
        if (_sldEcho != null) _sldEcho.value = 0.60f;

        SelectSensitivityPreset("Medium", 1.0f);

        if (_sldGameFog != null) _sldGameFog.value = EchoesPresentationSettings.DefaultGameFogDensity;
        if (_sldGameSun != null) _sldGameSun.value = EchoesPresentationSettings.DefaultGameSunIntensity;
        if (_sldGameLights != null) _sldGameLights.value = EchoesPresentationSettings.DefaultGamePointLightMul;
        if (_sldGameAmbient != null) _sldGameAmbient.value = EchoesPresentationSettings.DefaultGameAmbientMul;
        if (_sldMenuText != null) _sldMenuText.value = EchoesPresentationSettings.DefaultMenuTextScale;

        ApplyLightingPresetUi("liminal");
    }

    void UpdateGameFogLabel(float value)
    {
        if (_lblGameFogVal != null)
            _lblGameFogVal.text = value.ToString("F4");
    }

    void UpdateGameSunLabel(float value)
    {
        if (_lblGameSunVal != null)
            _lblGameSunVal.text = value.ToString("F2");
    }

    void UpdateGameLightsLabel(float value)
    {
        if (_lblGameLightsVal != null)
            _lblGameLightsVal.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    void UpdateGameAmbientLabel(float value)
    {
        if (_lblGameAmbientVal != null)
            _lblGameAmbientVal.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    void UpdateMenuTextLabel(float value)
    {
        if (_lblMenuTextVal != null)
            _lblMenuTextVal.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    void ApplySavedMenuTextScale()
    {
        if (_root == null)
            return;

        float scale = EchoesPresentationSettings.MenuTextScale;
        _root.RemoveFromClassList("scale-large");
        _root.RemoveFromClassList("scale-xl");

        if (scale >= 1.45f)
            _root.AddToClassList("scale-xl");
        else if (scale >= 1.12f)
            _root.AddToClassList("scale-large");
    }

    // Apply scaling styles to root UXML element
    public void ApplySavedUIScale()
    {
        if (_root == null) return;

        string scale = PlayerPrefs.GetString("UIScale", "Normal");
        _root.RemoveFromClassList("scale-large");
        _root.RemoveFromClassList("scale-xl");

        if (scale == "Large")
        {
            _root.AddToClassList("scale-large");
        }
        else if (scale == "Extra Large")
        {
            _root.AddToClassList("scale-xl");
        }
    }

    void RefreshDashboard()
    {
        if (_root == null)
            return;

        int completedLevels = GameProgress.GetCompletedCount();
        int totalLevels = GameProgress.TotalLevels;
        float completionRatio = totalLevels > 0 ? (float)completedLevels / totalLevels : 0f;

        float stability = 0.20f + 0.80f * completionRatio;
        float coherence = 0.10f + 0.90f * completionRatio;
        float progress = completionRatio;

        SetBarStat("lbl-stat-stability-val", "bar-stat-stability-fill", "lbl-stat-stability-desc",
            stability, completedLevels, totalLevels,
            "Neural cohesion initializing...",
            "Neural alignment in progress...",
            "Neural cohesion fully synchronized.");

        SetBarStat("lbl-stat-coherence-val", "bar-stat-coherence-fill", "lbl-stat-coherence-desc",
            coherence, completedLevels, totalLevels,
            "Fragment resonance is currently unstable.",
            "Intermittent signal synchronization.",
            "Fragment resonance stable.");

        SetBarStat("lbl-stat-progress-val", "bar-stat-progress-fill", "lbl-stat-progress-desc",
            progress, completedLevels, totalLevels,
            "Deep memory nodes still inaccessible.",
            "Memory fragments beginning to re-align.",
            "All memory nodes restored.");

        SetLabelText("lbl-telemetry-fragments", $"{completedLevels}/{totalLevels}");
        SetLabelText("lbl-telemetry-echoes", GameProgress.GetTotalEchoesCreated().ToString());
        SetLabelText("lbl-telemetry-deaths", GameProgress.GetTotalDeathCount().ToString());
        SetLabelText("lbl-telemetry-time", GameProgress.FormatPlayTime(GameProgress.GetTotalPlayTimeSeconds()));

        int integrity = GameProgress.GetIntegrityPercent();
        SetLabelText("lbl-user-integrity", $"Integridad: {integrity}%");
        SetLabelText("lbl-user-rank", GameProgress.GetArchivistRank());
        SetLabelText("lbl-user-sessions", $"Sesiones: {GameProgress.GetSessionCount()}");
        SetLabelText("lbl-protocol-desc", GameProgress.GetActiveProtocolMessage(completedLevels, totalLevels));

        string continueScene = GameProgress.GetContinueSceneName();
        int continueIndex = GameProgress.GetSceneIndex(continueScene);
        string continueName = continueIndex >= 0 ? GameProgress.GetLevelDisplayName(continueScene) : "—";
        string lastFragmentLine = continueIndex >= 0
            ? $"{continueName} · Nivel {continueIndex + 1:D2}"
            : continueName;
        SetLabelText("lbl-last-fragment", lastFragmentLine);

        if (completedLevels >= totalLevels && totalLevels > 0)
            SetLabelText("lbl-continue-hint", "VOID reinicia · elige cualquier fragmento en el mapa.");
        else if (completedLevels == 0)
            SetLabelText("lbl-continue-hint", "VOID inicia el primer fragmento.");
        else
            SetLabelText("lbl-continue-hint", $"Siguiente fragmento sugerido: {continueName}.");

        SetLabelText("lbl-map-progress", completedLevels == 1
            ? "1 nodo restaurado"
            : $"{completedLevels} nodos restaurados");

        UpdateLevelMapLabels();
    }

    void SetBarStat(string valueName, string barName, string descName, float value,
        int completed, int total, string descEmpty, string descMid, string descFull)
    {
        var lblVal = _root.Q<Label>(valueName);
        var barFill = _root.Q(barName);
        var lblDesc = _root.Q<Label>(descName);

        if (lblVal != null)
            lblVal.text = value.ToString("F2");
        if (barFill != null)
            barFill.style.width = Length.Percent(value * 100f);
        if (lblDesc != null)
        {
            if (completed == 0)
                lblDesc.text = descEmpty;
            else if (completed >= total)
                lblDesc.text = descFull;
            else
                lblDesc.text = descMid;
        }
    }

    void SetLabelText(string name, string text)
    {
        var lbl = _root.Q<Label>(name);
        if (lbl != null)
            lbl.text = text;
    }

    void UpdateLevelMapLabels()
    {
        for (int i = 1; i <= GameProgress.TotalLevels; i++)
        {
            string sceneName = $"Level_{i:D2}";
            var lbl = _root.Q<Label>($"lbl-level-{i:D2}");
            if (lbl == null)
                continue;

            if (!GameProgress.IsSceneUnlocked(sceneName))
            {
                lbl.text = "BLOQUEADO";
                continue;
            }

            if (GameProgress.IsSceneCompleted(sceneName))
            {
                int deaths = GameProgress.GetSceneDeathCount(sceneName);
                lbl.text = deaths > 0 ? $"COMPLETO · {deaths} colapsos" : "COMPLETO";
            }
            else if (sceneName == GameProgress.GetContinueSceneName())
            {
                lbl.text = "EN CURSO";
            }
        }
    }

    readonly System.Collections.Generic.Dictionary<string, System.Action> _levelClickHandlers = new();

    bool _resetArmed;

    void OnResetProgressClicked()
    {
        if (!_resetArmed)
        {
            _resetArmed = true;
            SetLabelText("lbl-reset-hint", "Pulsa REINICIAR ARCHIVO otra vez para confirmar.");
            return;
        }

        ConfirmResetProgress();
    }

    void ConfirmResetProgress()
    {
        _resetArmed = false;
        GameProgress.ResetProgress();
        SetLabelText("lbl-reset-hint", "Archivo reiniciado. Solo el fragmento 01 está activo.");
        if (_heroTitle != null)
            _heroTitle.text = "ISOLATED";
        RefreshDashboard();
        BindLevelMapButtons();
    }

    void BindLevelMapButtons()
    {
        for (int i = 1; i <= GameProgress.TotalLevels; i++)
        {
            string sceneName = $"Level_{i:D2}";
            string btnName = $"btn-level-{i:D2}";
            var btn = _root.Q<Button>(btnName);
            if (btn == null)
                continue;

            if (_levelClickHandlers.TryGetValue(btnName, out System.Action existing))
                btn.clicked -= existing;

            bool isUnlocked = GameProgress.IsSceneUnlocked(sceneName);
            bool isCompleted = GameProgress.IsSceneCompleted(sceneName);

            btn.RemoveFromClassList("level-button--locked");
            btn.RemoveFromClassList("level-button--completed");

            if (!isUnlocked)
            {
                btn.AddToClassList("level-button--locked");
                btn.SetEnabled(false);
                continue;
            }

            btn.SetEnabled(true);
            if (isCompleted)
                btn.AddToClassList("level-button--completed");

            System.Action handler = () => LoadLevel(sceneName);
            _levelClickHandlers[btnName] = handler;
            btn.clicked += handler;
        }
    }
}
