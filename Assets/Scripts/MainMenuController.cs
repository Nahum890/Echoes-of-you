using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controlador del menú principal usando UI Toolkit.
/// Diseño "EXPEDIENTE DE RECUERDOS" — Escuela Liminal 2.0.
/// Requiere un UIDocument component en el mismo GameObject.
/// El sistema de hover icónico (PS2/VHS/CRT) es gestionado por MenuHoverSystem.cs,
/// que se añade automáticamente como componente requerido.
/// </summary>
[RequireComponent(typeof(UIDocument))]
[RequireComponent(typeof(MenuHoverSystem))]
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

    // Sistema de hover icónico (PS2/VHS/CRT) — gestionado por MenuHoverSystem.cs
    MenuHoverSystem _hoverSystem;

    // Panels
    VisualElement _settingsPanel;
    VisualElement _levelSelectPanel;

    // Hover Preview Panels
    VisualElement _panelNeuralArchives;
    VisualElement _panelStabilityMap;
    VisualElement _panelCalibrationPreview;
    VisualElement _panelDisconnectOffline;
    VisualElement _rightContentContainer;
    string _activePreviewPanelName = "panel-neural-archives";
    Coroutine _terminalLogCoroutine;

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

    // Evita registrar callbacks (clicks/hover) más de una vez si el componente se re-activa.
    bool _wired;

    void OnEnable()
    {
        _doc = GetComponent<UIDocument>();
        if (_doc == null || _doc.rootVisualElement == null) return;
        _root = _doc.rootVisualElement;

        // Inicializar el sistema de hover icónico
        // MenuHoverSystem se auto-configura desde su propio OnEnable,
        // pero necesita el UIDocument que ya tenemos.
        _hoverSystem = GetComponent<MenuHoverSystem>();
        // (MenuHoverSystem.OnEnable() se llama automáticamente por Unity)

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

        // Hover Preview Panels
        _rightContentContainer = _root.Q("right-content-container");
        _panelNeuralArchives = _root.Q("panel-neural-archives");
        _panelStabilityMap = _root.Q("panel-stability-map");
        _panelCalibrationPreview = _root.Q("panel-calibration-preview");
        _panelDisconnectOffline = _root.Q("panel-disconnect-offline");

        // Side nav buttons
        _btnNewGame = _root.Q<Button>("nav-newgame");
        _btnLevels = _root.Q<Button>("nav-levels");
        _btnSettings = _root.Q<Button>("nav-settings");
        _btnExit = _root.Q<Button>("nav-exit");

        // Setup hover behaviors + acciones de nav (una sola vez)
        if (!_wired)
        {
            SetupHoverCallbacks();
            SetupFocusNavigation();

            RegisterButtonClick("nav-newgame", StartNewGame);
            RegisterButtonClick("nav-levels", ShowStabilityMap);
            RegisterButtonClick("nav-settings", ShowSettings);
            RegisterButtonClick("nav-exit", QuitGame);
        }

        GameProgress.EnsureInitialized();

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

        // Registro de callbacks de settings (una sola vez)
        if (!_wired)
        {
            if (_btnLightLiminal != null) _btnLightLiminal.clicked += () => ApplyLightingPresetUi("liminal");
            if (_btnLightBruma != null) _btnLightBruma.clicked += () => ApplyLightingPresetUi("bruma");
            if (_btnLightClaridad != null) _btnLightClaridad.clicked += () => ApplyLightingPresetUi("claridad");
            if (_btnLightPenumbra != null) _btnLightPenumbra.clicked += () => ApplyLightingPresetUi("penumbra");

            RegisterButtonClick("btn-restore-defaults", RestoreFactoryDefaults);
            RegisterButtonClick("btn-settings-back", DiscardSettings);
            RegisterButtonClick("btn-settings-apply", ApplySettings);
            RegisterButtonClick("btn-levels-back", ShowStabilityMap);
            RegisterButtonClick("btn-reset-progress", OnResetProgressClicked);
            RegisterButtonClick("btn-reset-progress-confirm", ConfirmResetProgress);

            _wired = true;
        }

        RefreshDashboard();
        RefreshNeuralArchives();
        BindLevelMapButtons();

        InitializeSettings();
        ShowVoidIntro();

        // Start animated terminal log coroutine
        if (_terminalLogCoroutine != null) StopCoroutine(_terminalLogCoroutine);
        _terminalLogCoroutine = StartCoroutine(AnimateTerminalLogs());
    }

    void Start()
    {
        SetMenuCursor();

        GameProgress.RecordSessionStarted();

        // Debug auto-start
        if (autoStartGame)
        {
            Invoke(nameof(AutoStart), 0.5f);
        }
    }

    void OnDisable()
    {
        UnregisterButtonClick("nav-newgame", StartNewGame);
        UnregisterButtonClick("nav-levels", ShowStabilityMap);
        UnregisterButtonClick("nav-settings", ShowSettings);
        UnregisterButtonClick("nav-exit", QuitGame);

        if (_btnNewGame != null)
        {
            _btnNewGame.UnregisterCallback<MouseEnterEvent>(_ => {});
            _btnNewGame.UnregisterCallback<MouseLeaveEvent>(_ => {});
        }
        if (_btnLevels != null)
        {
            _btnLevels.UnregisterCallback<MouseEnterEvent>(_ => {});
            _btnLevels.UnregisterCallback<MouseLeaveEvent>(_ => {});
        }
        if (_btnSettings != null)
        {
            _btnSettings.UnregisterCallback<MouseEnterEvent>(_ => {});
            _btnSettings.UnregisterCallback<MouseLeaveEvent>(_ => {});
        }
        if (_btnExit != null)
        {
            _btnExit.UnregisterCallback<MouseEnterEvent>(_ => {});
            _btnExit.UnregisterCallback<MouseLeaveEvent>(_ => {});
        }
    }

    public static void SetGameplayCursor()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    public static void SetMenuCursor()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Descartar la intro ("presionar cualquier tecla para sintonizar") y mostrar el menú.
        // Sin esto el menú queda atascado en void-intro con main-content oculto.
        if (_voidIntro != null && !_voidIntro.ClassListContains("hidden") && Input.anyKeyDown)
        {
            DismissIntro();
        }
    }

    /// <summary>Oculta la intro y revela el menú principal (main-content).</summary>
    void DismissIntro()
    {
        _voidIntro?.AddToClassList("hidden");
        _mainContent?.RemoveFromClassList("hidden");
        _rightContentContainer?.RemoveFromClassList("hidden");

        SetActiveNav(_btnNewGame);
        ShowPreviewPanel("panel-neural-archives");
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

    void UnregisterButtonClick(string name, System.Action action)
    {
        var btn = _root.Q<Button>(name);
        if (btn != null)
        {
            btn.clicked -= action;
        }
    }

    void SetupFocusNavigation()
    {
        if (_btnNewGame != null)
        {
            _btnNewGame.RegisterCallback<FocusEvent>(_ => OnNavFocus(_btnNewGame, MainMenuCinematicWorld.MenuAmbience.Void, "Aula 104"));
        }
        if (_btnLevels != null)
        {
            _btnLevels.RegisterCallback<FocusEvent>(_ => OnNavFocus(_btnLevels, MainMenuCinematicWorld.MenuAmbience.Stability, "Archivos Escolares"));
        }
        if (_btnSettings != null)
        {
            _btnSettings.RegisterCallback<FocusEvent>(_ => OnNavFocus(_btnSettings, MainMenuCinematicWorld.MenuAmbience.System, "Ajustar Receptor"));
        }
        if (_btnExit != null)
        {
            _btnExit.RegisterCallback<FocusEvent>(_ => OnNavFocus(_btnExit, MainMenuCinematicWorld.MenuAmbience.Disconnect, "Salir del Recuerdo"));
        }
    }

    void OnNavFocus(Button btn, MainMenuCinematicWorld.MenuAmbience ambience, string title)
    {
        if (MainMenuCinematicWorld.Instance != null)
            MainMenuCinematicWorld.Instance.SetAmbience(ambience);

        if (_heroTitle != null)
            _heroTitle.text = title;

        SetActiveNav(btn);
        ShowPreviewPanel(GetPanelNameForButton(btn));
    }

    // --- Hover Background & Title Swap ---

    void SetupHoverCallbacks()
    {
        if (_btnNewGame != null)
        {
            _btnNewGame.RegisterCallback<MouseEnterEvent>(_ => OnNavHover(_btnNewGame, MainMenuCinematicWorld.MenuAmbience.Void, "Aula 104"));
            _btnNewGame.RegisterCallback<MouseLeaveEvent>(_ => OnNavHoverLeave(_btnNewGame));
        }
        if (_btnLevels != null)
        {
            _btnLevels.RegisterCallback<MouseEnterEvent>(_ => OnNavHover(_btnLevels, MainMenuCinematicWorld.MenuAmbience.Stability, "Archivos Escolares"));
            _btnLevels.RegisterCallback<MouseLeaveEvent>(_ => OnNavHoverLeave(_btnLevels));
        }
        if (_btnSettings != null)
        {
            _btnSettings.RegisterCallback<MouseEnterEvent>(_ => OnNavHover(_btnSettings, MainMenuCinematicWorld.MenuAmbience.System, "Ajustar Receptor"));
            _btnSettings.RegisterCallback<MouseLeaveEvent>(_ => OnNavHoverLeave(_btnSettings));
        }
        if (_btnExit != null)
        {
            _btnExit.RegisterCallback<MouseEnterEvent>(_ => OnNavHover(_btnExit, MainMenuCinematicWorld.MenuAmbience.Disconnect, "Salir del Recuerdo"));
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

        // Show the corresponding preview panel
        ShowPreviewPanel(GetPanelNameForButton(btn));
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

        // Return to the active nav's preview panel
        ShowPreviewPanel(GetPanelNameForButton(_activeNavButton));
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
        if (_activeNavButton == _btnNewGame) return "Aula 104";
        if (_activeNavButton == _btnLevels) return "Archivos Escolares";
        if (_activeNavButton == _btnSettings) return "Ajustar Receptor";
        if (_activeNavButton == _btnExit) return "Salir del Recuerdo";
        return "Recuerdo Aislado";
    }

    string GetPanelNameForButton(Button btn)
    {
        if (btn == _btnNewGame) return "panel-neural-archives";
        if (btn == _btnLevels) return "panel-stability-map";
        if (btn == _btnSettings) return "panel-calibration-preview";
        if (btn == _btnExit) return "panel-disconnect-offline";
        return "panel-neural-archives";
    }

    // --- Preview Panel Switching ---

    void ShowPreviewPanel(string panelName)
    {
        if (panelName == _activePreviewPanelName) return;
        _activePreviewPanelName = panelName;

        // Hide all preview panels
        _panelNeuralArchives?.RemoveFromClassList("preview-panel--visible");
        _panelStabilityMap?.RemoveFromClassList("preview-panel--visible");
        _panelCalibrationPreview?.RemoveFromClassList("preview-panel--visible");
        _panelDisconnectOffline?.RemoveFromClassList("preview-panel--visible");

        // Show the target panel
        var target = _root.Q(panelName);
        target?.AddToClassList("preview-panel--visible");

        if (panelName == "panel-calibration-preview")
        {
            RefreshCalibrationPreview();
        }
        else if (panelName == "panel-neural-archives")
        {
            RefreshNeuralArchives();
        }
    }

    // --- Panel Switching ---

    void ShowVoidIntro()
    {
        _settingsPanel?.AddToClassList("hidden");
        _levelSelectPanel?.AddToClassList("hidden");
        _rightContentContainer?.RemoveFromClassList("hidden");

        if (_voidIntro != null)
        {
            _mainContent?.AddToClassList("hidden");
            _voidIntro.RemoveFromClassList("hidden");
        }
        else
        {
            _mainContent?.RemoveFromClassList("hidden");
        }

        if (_menuBg != null)
        {
            _menuBg.style.opacity = 1f;
        }

        if (_heroTitle != null)
            _heroTitle.text = "Aula 104";

        SetActiveNav(_btnNewGame);
        _activePreviewPanelName = "";
        ShowPreviewPanel("panel-neural-archives");
        RefreshNeuralArchives();
    }

    void ShowStabilityMap()
    {
        _settingsPanel?.AddToClassList("hidden");
        _levelSelectPanel?.AddToClassList("hidden");
        _voidIntro?.AddToClassList("hidden");
        _mainContent?.RemoveFromClassList("hidden");
        _rightContentContainer?.RemoveFromClassList("hidden");

        if (_menuBg != null)
            _menuBg.style.opacity = 1f;

        if (_heroTitle != null)
            _heroTitle.text = "Archivos Escolares";

        SetActiveNav(_btnLevels);
        _activePreviewPanelName = ""; // reset para forzar refresco
        ShowPreviewPanel("panel-stability-map");
        RefreshDashboard();
    }

    void ShowSettings()
    {
        _settingsPanel?.AddToClassList("hidden");
        _levelSelectPanel?.AddToClassList("hidden");
        _voidIntro?.AddToClassList("hidden");
        _mainContent?.RemoveFromClassList("hidden");
        _rightContentContainer?.RemoveFromClassList("hidden");

        if (_heroTitle != null)
            _heroTitle.text = "Configuración";

        SetActiveNav(_btnSettings);
        _activePreviewPanelName = ""; // reset para forzar refresco
        ShowPreviewPanel("panel-calibration-preview");
        LoadCurrentSettingsIntoUI();
    }

    // --- Actions ---

    void StartNewGame()
    {
        LoadLevel(firstLevelScene);
    }

    void LoadLevel(string levelName)
    {
        if (string.IsNullOrWhiteSpace(levelName))
        {
            Debug.LogError("[MainMenuController] Cannot load an empty level name.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(levelName))
        {
            Debug.LogError($"[MainMenuController] Scene '{levelName}' is not in Build Settings or cannot be loaded.");
            return;
        }

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
                var allDocs = FindObjectsByType<UIDocument>(FindObjectsInactive.Exclude);
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
        var allRecorders = FindObjectsByType<EchoPlayback>(FindObjectsInactive.Exclude);
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
        // Regresa al panel principal (Neural Archives / "Acceder a Memoria")
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

    // --- Neural Archives (VOID Panel) Telemetry ---

    void RefreshNeuralArchives()
    {
        if (_root == null) return;

        int completed = GameProgress.GetCompletedCount();
        int total = GameProgress.TotalLevels;
        float stability = 0.20f + 0.80f * (total > 0 ? (float)completed / total : 0f);

        SetLabelText("lbl-archive-fragments", $"{completed}/{total}");
        SetLabelText("lbl-archive-echoes", GameProgress.GetTotalEchoesCreated().ToString());
        SetLabelText("lbl-archive-deaths", GameProgress.GetTotalDeathCount().ToString());
        SetLabelText("lbl-archive-time", GameProgress.FormatPlayTime(GameProgress.GetTotalPlayTimeSeconds()));
        SetLabelText("lbl-archive-stability-pct", $"{Mathf.RoundToInt(stability * 100f)}%");

        var stabilityBar = _root.Q("bar-archive-stability");
        if (stabilityBar != null)
            stabilityBar.style.width = Length.Percent(stability * 100f);
    }

    void RefreshCalibrationPreview()
    {
        if (_root == null) return;

        var audioMgr = EchoesAudioManager.EnsureExists();
        float master = audioMgr != null ? audioMgr.GetMasterVolume() : PlayerPrefs.GetFloat("MasterVolume", 0.84f);
        float music = audioMgr != null ? audioMgr.GetMusicVolume() : PlayerPrefs.GetFloat("MusicVolume", 0.6f);
        float sfx = audioMgr != null ? audioMgr.GetSFXVolume() : PlayerPrefs.GetFloat("SfxVolume", 0.72f);

        SetLabelText("lbl-preview-audio-master", $"Master: {Mathf.RoundToInt(master * 100f)}%");
        SetLabelText("lbl-preview-audio-music", $"Música: {Mathf.RoundToInt(music * 100f)}%");
        SetLabelText("lbl-preview-audio-sfx", $"SFX: {Mathf.RoundToInt(sfx * 100f)}%");

        // Get actual resolution
        string resText = $"{Screen.width} x {Screen.height}";
        SetLabelText("lbl-preview-video-res", $"Resolución: {resText}");
        SetLabelText("lbl-preview-video-fs", $"Pantalla Completa: {(Screen.fullScreen ? "SI" : "NO")}");
        SetLabelText("lbl-preview-video-scale", $"Escala UI: {PlayerPrefs.GetString("UIScale", "Normal")}");

        float sens = PlayerPrefs.GetFloat("CameraSensitivity", 1f);
        float echo = PlayerPrefs.GetFloat("EchoOpacity", 0.6f);
        SetLabelText("lbl-preview-neural-sens", $"Sensibilidad: {sens:F1}");
        SetLabelText("lbl-preview-neural-echo", $"Opacidad Eco: {Mathf.RoundToInt(echo * 100f)}%");
    }

    // --- Terminal Log Animation ---

    readonly string[] _diagnosticLines = new[]
    {
        "[RECUERDO] Sintonización de expediente completada.",
        "[OK] Vínculos de recuerdo establecidos.",
        "[OK] Estabilidad del aula sincronizada.",
        "[BÚSQUEDA] Comprobando integridad de pasillos...",
        "[OK] Nodos de pasillo respondieron: despejado.",
        "[SINC] Resonancia de fragmento calibrando...",
        "[OK] Puntos de anclaje de eco registrados.",
        "[AVISO] Inestabilidad en fragmento — sector 07.",
        "[OK] Memoria en recuperación.",
        "[SINC] Deriva temporal dentro del margen: 0.003ms.",
        "[OK] Integridad del expediente verificada.",
        "[BÚSQUEDA] Escaneando nodos de memoria profunda...",
        "[OK] Sin señales anómalas detectadas.",
        "[SISTEMA] Latido: 72 ppm — NOMINAL.",
        "[OK] Telemetría de sesión activa.",
        "[SINC] Resonancia del vacío: ESTABLE.",
        "Esperando comando de sintonización...",
    };

    IEnumerator AnimateTerminalLogs()
    {
        var log1 = _root?.Q<Label>("lbl-archive-log-1");
        var log2 = _root?.Q<Label>("lbl-archive-log-2");
        var log3 = _root?.Q<Label>("lbl-archive-log-3");
        if (log1 == null || log2 == null || log3 == null)
            yield break;

        int lineIndex = 3; // Start after the initial 3 lines that are hardcoded in UXML

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2.5f, 5.0f));

            // Shift lines up
            log1.text = log2.text;
            log2.text = log3.text;

            // Pick next line
            string nextLine = _diagnosticLines[lineIndex % _diagnosticLines.Length];
            lineIndex++;

            // Color the new line based on prefix
            if (nextLine.StartsWith("[WARN]"))
                log3.style.color = new StyleColor(new Color(1f, 0.76f, 0.36f, 1f)); // amber
            else if (nextLine.StartsWith("Waiting"))
                log3.style.color = new StyleColor(new Color(0.48f, 0.94f, 0.78f, 1f)); // #7af0c8
            else
                log3.style.color = new StyleColor(new Color(0.86f, 0.88f, 0.98f, 1f)); // default

            log3.text = nextLine;
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
            "Sincronización de recuerdos iniciando...",
            "Sincronización de recuerdos en curso...",
            "Sincronización de recuerdos completada.");

        SetBarStat("lbl-stat-coherence-val", "bar-stat-coherence-fill", "lbl-stat-coherence-desc",
            coherence, completedLevels, totalLevels,
            "Coherencia de memoria inestable.",
            "Señal de memoria intermitente.",
            "Coherencia de memoria estable.");

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
        SetLabelText("lbl-reset-hint", "Expediente borrado. Solo el primer recuerdo está disponible.");
        if (_heroTitle != null)
            _heroTitle.text = "Recuerdo Aislado";
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
