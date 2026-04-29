using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public static class EchoesProductionBuilder
{
    const string SceneRoot = "Assets/Scenes";
    const string MaterialRoot = "Assets/Materials/Echoes";
    const string PrefabRoot = "Assets/Prefabs";
    const string EchoPrefabPath = "Assets/Prefabs/EchoPrefab.prefab";
    const string AnimatorControllerPath = "Assets/Prefabs/PlayerAnimController.controller";
    const string FenceStraightPath = "Assets/3D Models/Models/FBX format/fence-straight.fbx";
    const string PolesPath = "Assets/3D Models/Models/FBX format/poles.fbx";
    const string PipePath = "Assets/3D Models/Models/FBX format/pipe.fbx";
    const string SmokeDarkMaterialPath = "Assets/Materials/Echoes/Mat_LiminalFog.mat";
    const int GroundLayer = 6;

    static Material _floorMat;
    static Material _plateMat;
    static Material _bridgeMat;
    static Material _doorMat;
    static Material _goalMat;
    static Material _playerMat;
    static Material _echoMat;

    [MenuItem("Echoes of You/Production/Rebuild Menu Hub and Levels", false, 200)]
    public static void RebuildAll()
    {
        EnsureFolders();
        EnsureMaterials();
        EnsureAnimatorController();
        EnsureEchoPrefab();

        BuildMainMenu();
        BuildHub();
        BuildLevel01();
        BuildLevel02();
        BuildLevel03();
        BuildLevel04();
        BuildLevel05();
        BuildLevel06();
        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Echoes Production] Scenes rebuilt. Running validation...");
        LevelValidator.ValidateAllLevels();
    }

    static void BuildMainMenu()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        SetupAtmosphere(new Color(0.08f, 0.12f, 0.16f, 1f), 0.012f, new Color(0.13f, 0.18f, 0.23f, 1f));
        SpawnDirectionalLight();

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera cameraRef = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.transform.position = new Vector3(0f, 3.6f, -12f);
        cameraObject.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
        cameraRef.clearFlags = CameraClearFlags.SolidColor;
        cameraRef.backgroundColor = new Color(0.04f, 0.06f, 0.08f, 1f);

        MakePlatform("MenuFloor", new Vector3(0f, 0f, 6f), new Vector3(20f, 0.5f, 20f), null, _floorMat);
        MakeBackdrop("MenuBackdrop", Vector3.zero, 28f, 28f, 12f, null);
        MakePlatform("MenuMonolith", new Vector3(0f, 1.8f, 13f), new Vector3(3f, 3.6f, 3f), null, _goalMat);
        SpawnPointLight("MenuGlow", new Vector3(0f, 5f, 13f), new Color(0.16f, 0.85f, 1f, 1f), 6f, 12f, null);
        SpawnAmbientParticles(new Vector3(0f, 2f, 8f), new Vector3(18f, 8f, 18f));
        SpawnSmokeVolume("MenuSmokeLow", new Vector3(0f, 0.8f, 7f), new Vector3(22f, 3f, 22f), null, 120f);
        SpawnSmokeVolume("MenuSmokeFar", new Vector3(0f, 1.6f, 15f), new Vector3(24f, 5f, 10f), null, 65f);

        // --- CANVAS UI ---
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        MainMenu controller = canvasObj.AddComponent<MainMenu>();

        Font arial = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        UnityEngine.UI.DefaultControls.Resources uiRes = new UnityEngine.UI.DefaultControls.Resources();

        // Title text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Text titleTxt = titleObj.AddComponent<UnityEngine.UI.Text>();
        titleTxt.font = arial;
        titleTxt.text = "ECHOES OF YOU";
        titleTxt.fontSize = 64;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = new Color(0.75f, 0.8f, 0.9f, 0.9f); // Blanco frío tenue
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0, 300);
        titleRt.sizeDelta = new Vector2(800, 100);

        // Panels
        GameObject mainPnl = UnityEngine.UI.DefaultControls.CreatePanel(uiRes);
        mainPnl.transform.SetParent(canvasObj.transform, false);
        mainPnl.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0);
        
        GameObject lsPnl = UnityEngine.UI.DefaultControls.CreatePanel(uiRes);
        lsPnl.transform.SetParent(canvasObj.transform, false);
        lsPnl.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0.85f);
        
        GameObject setPnl = UnityEngine.UI.DefaultControls.CreatePanel(uiRes);
        setPnl.transform.SetParent(canvasObj.transform, false);
        setPnl.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0.85f);

        controller.mainPanel = mainPnl.AddComponent<CanvasGroup>();
        controller.levelSelectPanel = lsPnl.AddComponent<CanvasGroup>();
        controller.settingsPanel = setPnl.AddComponent<CanvasGroup>();

        // MAIN MENU BUTTONS (named so MainMenu.cs wires them at runtime)
        CreateMenuButton(mainPnl.transform, "PLAY", new Vector2(0, 50), "Btn_Play");
        CreateMenuButton(mainPnl.transform, "LEVEL SELECT", new Vector2(0, -30), "Btn_LevelSelect");
        CreateMenuButton(mainPnl.transform, "SETTINGS", new Vector2(0, -110), "Btn_Settings");
        CreateMenuButton(mainPnl.transform, "EXIT", new Vector2(0, -190), "Btn_Exit");

        // LEVEL SELECT BUTTONS
        GameObject lsTitle = new GameObject("LSTitle");
        lsTitle.transform.SetParent(lsPnl.transform, false);
        UnityEngine.UI.Text lstTxt = lsTitle.AddComponent<UnityEngine.UI.Text>();
        lstTxt.font = arial; lstTxt.text = "SELECT LEVEL"; lstTxt.fontSize = 50; lstTxt.alignment = TextAnchor.MiddleCenter; lstTxt.color = Color.white;
        lsTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 250);

        CreateMenuButton(lsPnl.transform, "Level 1: Aprendizaje", new Vector2(-220, 100), "Btn_Level01");
        CreateMenuButton(lsPnl.transform, "Level 2: Aplicacion", new Vector2(220, 100), "Btn_Level02");
        CreateMenuButton(lsPnl.transform, "Level 3: Timing", new Vector2(-220, 20), "Btn_Level03");
        CreateMenuButton(lsPnl.transform, "Level 4: Twist", new Vector2(220, 20), "Btn_Level04");
        CreateMenuButton(lsPnl.transform, "Level 5: Sacrificio", new Vector2(-220, -60), "Btn_Level05");
        CreateMenuButton(lsPnl.transform, "Level 6: Relevos", new Vector2(220, -60), "Btn_Level06");
        CreateMenuButton(lsPnl.transform, "Back", new Vector2(0, -250), "Btn_Back");

        // SETTINGS CONTROLS
        GameObject setTitle = new GameObject("SetTitle");
        setTitle.transform.SetParent(setPnl.transform, false);
        UnityEngine.UI.Text stTxt = setTitle.AddComponent<UnityEngine.UI.Text>();
        stTxt.font = arial; stTxt.text = "SETTINGS"; stTxt.fontSize = 50; stTxt.alignment = TextAnchor.MiddleCenter; stTxt.color = Color.white;
        setTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 250);

        GameObject resDrop = UnityEngine.UI.DefaultControls.CreateDropdown(uiRes);
        resDrop.name = "ResolutionDropdown";
        resDrop.transform.SetParent(setPnl.transform, false);
        resDrop.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
        
        GameObject fsTog = UnityEngine.UI.DefaultControls.CreateToggle(uiRes);
        fsTog.name = "FullscreenToggle";
        fsTog.transform.SetParent(setPnl.transform, false);
        fsTog.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20);
        fsTog.GetComponentInChildren<UnityEngine.UI.Text>().text = "Fullscreen";
        
        GameObject vsTog = UnityEngine.UI.DefaultControls.CreateToggle(uiRes);
        vsTog.name = "VsyncToggle";
        vsTog.transform.SetParent(setPnl.transform, false);
        vsTog.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -40);
        vsTog.GetComponentInChildren<UnityEngine.UI.Text>().text = "VSync";

        GameObject volSld = UnityEngine.UI.DefaultControls.CreateSlider(uiRes);
        volSld.name = "VolumeSlider";
        volSld.transform.SetParent(setPnl.transform, false);
        volSld.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);

        GameObject sensSld = UnityEngine.UI.DefaultControls.CreateSlider(uiRes);
        sensSld.name = "SensitivitySlider";
        sensSld.transform.SetParent(setPnl.transform, false);
        sensSld.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -160);

        CreateMenuButton(setPnl.transform, "Back", new Vector2(0, -250), "Btn_Back");

        GameObject tm = new GameObject("SceneTransitionManager");
        tm.AddComponent<SceneTransitionManager>();

        SaveScene(scene, "MainMenu");
    }

    static void CreateMenuButton(Transform parent, string text, Vector2 pos, string buttonName)
    {
        UnityEngine.UI.DefaultControls.Resources uiRes = new UnityEngine.UI.DefaultControls.Resources();
        GameObject btnObj = UnityEngine.UI.DefaultControls.CreateButton(uiRes);
        btnObj.name = buttonName;
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(350, 50);

        UnityEngine.UI.Image btnImg = btnObj.GetComponent<UnityEngine.UI.Image>();
        btnImg.color = new Color(0.06f, 0.08f, 0.12f, 0.75f);

        UnityEngine.UI.Button btn = btnObj.GetComponent<UnityEngine.UI.Button>();

        UnityEngine.UI.ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.06f, 0.08f, 0.12f, 0.75f);
        colors.highlightedColor = new Color(0.1f, 0.3f, 0.6f, 0.85f);
        colors.pressedColor = new Color(0.05f, 0.15f, 0.4f, 0.9f);
        colors.selectedColor = colors.highlightedColor;
        btn.colors = colors;

        UnityEngine.UI.Text txt = btnObj.GetComponentInChildren<UnityEngine.UI.Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = text;
        txt.fontSize = 20;
        txt.color = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    static void BuildHub()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_07";

        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.12f, 0.18f, 0.24f, 1f), 0.02f, new Color(0.12f, 0.16f, 0.2f, 1f));
        SpawnDirectionalLight();

        MakePlatform("HubFloor", new Vector3(0f, 0f, 0f), new Vector3(28f, 0.5f, 28f), env, _floorMat);
        MakePlatform("HubCore", new Vector3(0f, 0.6f, 0f), new Vector3(6f, 1.2f, 6f), env, _bridgeMat);
        MakeBackdrop("HubBackdrop", Vector3.zero, 36f, 36f, 12f, env);
        SpawnPointLight("HubCoreLight", new Vector3(0f, 4.5f, 0f), new Color(0.94f, 0.98f, 1f, 1f), 6f, 14f, env);
        SpawnSmokeVolume("HubSmoke", new Vector3(0f, 1f, 0f), new Vector3(34f, 4f, 34f), env, 150f);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, -3f), false, 0, 0f);
        SpawnGameplayCamera(player.transform);
        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);

        GameObject hubController = new GameObject("HubSceneController");
        hubController.transform.SetParent(mech, false);
        hubController.AddComponent<HubSceneController>();

        Vector3[] portalPositions =
        {
            new Vector3(-10f, 0f, 9f),
            new Vector3(0f, 0f, 12f),
            new Vector3(10f, 0f, 9f),
            new Vector3(10f, 0f, -9f),
            new Vector3(0f, 0f, -12f),
            new Vector3(-10f, 0f, -9f)
        };

        string[] sceneNames = { "Level_01", "Level_02", "Level_03", "Level_04", "Level_05", "Level_06" };
        string[] displayNames = { "Primer Rastro", "Camino Compartido", "Dos Decisiones", "Orden de Lectura", "Cadena Estable", "Nucleo" };
        string[] lines =
        {
            "Primero recuerdas.",
            "Luego pruebas.",
            "Dos decisiones se sostienen.",
            "El orden cambia el camino.",
            "La precision revela el patron.",
            "Todo converge al centro."
        };

        for (int i = 0; i < portalPositions.Length; i++)
            CreateHubPortal(portalPositions[i], sceneNames[i], displayNames[i], lines[i], mech, env);

        SaveScene(scene, "Level_07");
    }

    static void BuildLevel01()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_01";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");
        Transform tutorial = CreateRoot("--- TUTORIAL ---");

        SetupAtmosphere(new Color(0.05f, 0.05f, 0.08f, 1f), 0.045f, new Color(0.05f, 0.05f, 0.08f, 1f));
        SpawnDirectionalLight();
        MakeBackdrop("Backdrop", new Vector3(0f, 0f, 10f), 24f, 36f, 30f, env);

        MakePlatform("Floor_Main", new Vector3(0f, 0f, 14f), new Vector3(8f, 0.5f, 12f), env, _floorMat);
        MakePlatform("Floor_Pit", new Vector3(0f, -2f, 4f), new Vector3(8f, 0.5f, 8f), env, _floorMat);

        PressurePlate btn1 = CreatePlate("Button_1", new Vector3(0f, -1.64f, 4f), mech);
        
        DoorController door = CreateDoor("Door", new Vector3(0f, 1.75f, 18f), new Vector3(4f, 3.5f, 1f), mech, new[] { btn1 });
        door.latchOpen = false;
        
        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 22f), mech, "Level_02");
        CreateLevelGoal(mech, "Activa la memoria.", "Salida revelada.", "Primero recuerdas.", exit, btn1);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 14f), true, 1, 5f);
        SpawnGameplayCamera(player.transform);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 0.5f, 14f),
            new Vector3(0f, -1.5f, 4f)
        });
        SpawnPuzzleIntent(mech, 1, 1, true, true, false, 8f, "Reverse Role: Player at door, echo runs to pit");

        SpawnPointLight("Light_Button", new Vector3(0f, 0f, 4f), new Color(0.1f, 0.5f, 1f), 4f, 6f, env);
        SpawnPointLight("Light_Door", new Vector3(0f, 3f, 18f), new Color(0.6f, 0.1f, 0.1f), 3f, 5f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 3f, 22f), new Color(1f, 1f, 1f), 4f, 8f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnLevelRuntime(mech, "", "Usa el boton izquierdo del mouse para grabar.", "");

        SpawnAmbientLights(env, new Vector3(0f, 0f, 10f), 8f, 24f);
        SpawnSmokeVolume("L01_Fog", new Vector3(0f, 0.5f, 10f), new Vector3(10f, 3f, 26f), env, 40f);

        CreateTutorialTrigger("Tut_Record", new Vector3(0f, 1f, 12f), new Vector3(4f, 3f, 4f),
            "Manten E o R para grabar tu movimiento.", "Suelta para crear un eco.", 4f, tutorial);
        CreateTutorialTrigger("Tut_Pit", new Vector3(0f, -0.5f, 4f), new Vector3(6f, 3f, 6f),
            "El eco repetira tu camino. Usalo para pisar el boton.", "", 4f, tutorial);

        SaveScene(scene, "Level_01");
    }

    static void BuildLevel02()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_02";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");
        Transform tutorial = CreateRoot("--- TUTORIAL ---");

        SetupAtmosphere(new Color(0.05f, 0.05f, 0.08f, 1f), 0.045f, new Color(0.05f, 0.05f, 0.08f, 1f));
        SpawnDirectionalLight();
        MakeBackdrop("Backdrop", new Vector3(0f, 0f, 15f), 32f, 40f, 40f, env);

        // Ground Floor
        MakePlatform("Floor_Ground", new Vector3(0f, 0f, 6f), new Vector3(16f, 0.5f, 16f), env, _floorMat);
        // High Ledge
        MakePlatform("Floor_High", new Vector3(0f, 4f, 22f), new Vector3(8f, 0.5f, 8f), env, _floorMat);
        
        // Buttons on Ground Floor
        PressurePlate btnDecoy = CreatePlate("Button_DECOY", new Vector3(-5f, 0.36f, 6f), mech);
        PressurePlate btnReal = CreatePlate("Button_REAL", new Vector3(5f, 0.36f, 6f), mech);

        // Elevator (Bridge moving vertically)
        TimedMovingPlatform elevator = CreateBridge("Elevator", new Vector3(0f, 0f, 16f), new Vector3(0f, 4f, 0f), new Vector3(0f, 0f, 0f), new Vector3(4f, 0.5f, 4f), btnDecoy, mech);
        
        // Door on High Ledge blocking Exit
        DoorController exitDoor = CreateDoor("Door_Exit", new Vector3(0f, 5.75f, 19.5f), new Vector3(8f, 3.5f, 1f), mech, new[] { btnReal });
        exitDoor.latchOpen = false;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 5.25f, 24f), mech, "Level_03");
        CreateLevelGoal(mech, "Aprovecha el ascensor.", "Salida abierta.", "Un buen eco hace dos favores.", exit, btnReal);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 1, 10f);
        SpawnGameplayCamera(player.transform);

        SpawnPointLight("Light_Decoy", new Vector3(-5f, 2f, 6f), new Color(1f, 0.8f, 0.2f, 1f), 3f, 8f, env);
        SpawnPointLight("Light_Real", new Vector3(5f, 2f, 6f), new Color(0.1f, 0.5f, 1f), 3f, 8f, env);
        SpawnPointLight("Light_Elevator", new Vector3(0f, 6f, 16f), new Color(0.8f, 0.8f, 0.9f, 1f), 4f, 12f, env);
        SpawnPointLight("Light_Door", new Vector3(0f, 7f, 19.5f), new Color(0.6f, 0.1f, 0.1f), 2f, 5f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnLevelRuntime(mech, "", "El eco puede realizar multiples tareas antes de desaparecer.", "");
        SpawnAmbientLights(env, new Vector3(0f, 2f, 12f), 16f, 32f);
        SpawnSmokeVolume("L02_Fog", new Vector3(0f, 0.5f, 12f), new Vector3(18f, 3f, 32f), env, 30f);

        CreateTutorialTrigger("Tut_Elevator", new Vector3(-5f, 1f, 6f), new Vector3(4f, 3f, 4f),
            "Este boton baja el ascensor.", "Pero necesitas estar arriba para cruzar la puerta.", 4f, tutorial);

        SaveScene(scene, "Level_02");
    }

    static void BuildLevel03()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_03";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");
        Transform tutorial = CreateRoot("--- TUTORIAL ---");

        SetupAtmosphere(new Color(0.04f, 0.04f, 0.07f, 1f), 0.05f, new Color(0.04f, 0.04f, 0.07f, 1f));
        SpawnDirectionalLight();
        MakeBackdrop("Backdrop", new Vector3(0f, 0f, 8f), 32f, 40f, 40f, env);

        // U-Shape Upper Platforms
        MakePlatform("Floor_Start", new Vector3(-6f, 4f, 6f), new Vector3(6f, 0.5f, 18f), env, _floorMat);
        MakePlatform("Floor_Exit", new Vector3(6f, 4f, 6f), new Vector3(6f, 0.5f, 18f), env, _floorMat);
        
        // Deep Pit in the middle
        MakePlatform("Floor_Pit", new Vector3(0f, -6f, 10f), new Vector3(6f, 0.5f, 8f), env, _floorMat);

        // Button at bottom of pit
        PressurePlate btnPit = CreatePlate("Button_Pit", new Vector3(0f, -5.64f, 10f), mech);
        btnPit.autoReleaseTimer = 2f;

        // Drawbridge connecting the two upper arms across the void
        TimedMovingPlatform bridge = CreateBridge("VoidBridge", new Vector3(0f, 4f, 12f), new Vector3(0f, -8f, 0f), new Vector3(0f, 0f, 0f), new Vector3(6f, 0.5f, 4f), btnPit, mech);

        LevelExit exit = CreateLevelExit(new Vector3(6f, 5.25f, 12f), mech, "Level_04");
        CreateLevelGoal(mech, "Alguien debe caer.", "Puente extendido.", "El eco absorbe tu caida.", exit, btnPit);

        // A jumping ramp pointing into the void
        MakePlatform("Ramp_Jump", new Vector3(-3.5f, 4.2f, 10f), new Vector3(2f, 0.2f, 2f), env, _bridgeMat);

        GameObject player = SpawnPlayer(new Vector3(-6f, 5.1f, -1f), true, 1, 8f);
        SpawnGameplayCamera(player.transform);

        SpawnPointLight("Light_Pit", new Vector3(0f, -4f, 10f), new Color(0.8f, 0.15f, 0.15f, 1f), 4f, 12f, env);
        SpawnPointLight("Light_Bridge", new Vector3(0f, 6f, 12f), new Color(0.1f, 0.6f, 1f, 1f), 3f, 8f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnLevelRuntime(mech, "", "No puedes cruzar volando, pero puedes caer con estilo.", "");
        SpawnAmbientLights(env, new Vector3(0f, 4f, 6f), 16f, 24f);
        SpawnSmokeVolume("L03_PitFog", new Vector3(0f, -3f, 10f), new Vector3(8f, 8f, 12f), env, 30f);

        CreateTutorialTrigger("Tut_Jump", new Vector3(-6f, 5f, 4f), new Vector3(6f, 3f, 6f),
            "El boton esta muy abajo.", "Si saltas moriras... pero el eco sobrevivira lo suficiente.", 4f, tutorial);

        SaveScene(scene, "Level_03");
    }

    static void BuildLevel04()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_04";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");
        Transform tutorial = CreateRoot("--- TUTORIAL ---");

        SetupAtmosphere(new Color(0.04f, 0.04f, 0.07f, 1f), 0.05f, new Color(0.04f, 0.04f, 0.07f, 1f));
        SpawnDirectionalLight();
        MakeBackdrop("Backdrop", new Vector3(0f, 0f, 16f), 36f, 50f, 50f, env);

        // Center hub
        MakePlatform("Floor_Center", new Vector3(0f, 0f, 0f), new Vector3(6f, 0.5f, 6f), env, _floorMat);
        
        // Left Island (Requires 3.5m jump)
        MakePlatform("Floor_Left", new Vector3(-8f, 0f, 8f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        // Right Island (Requires 3.5m jump)
        MakePlatform("Floor_Right", new Vector3(8f, 0f, 8f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        // Exit Island (Requires 3.5m jump from center)
        MakePlatform("Floor_Exit", new Vector3(0f, 0f, 16f), new Vector3(6f, 0.5f, 8f), env, _floorMat);

        // Connecting jump pillars to make it slightly easier but risky
        MakePlatform("Pillar_L", new Vector3(-4f, -0.5f, 4f), new Vector3(2f, 1.5f, 2f), env, _bridgeMat);
        MakePlatform("Pillar_R", new Vector3(4f, -0.5f, 4f), new Vector3(2f, 1.5f, 2f), env, _bridgeMat);
        MakePlatform("Pillar_F", new Vector3(0f, -0.5f, 8f), new Vector3(2f, 1.5f, 2f), env, _bridgeMat);

        PressurePlate btnL = CreatePlate("Button_L", new Vector3(-8f, 0.36f, 8f), mech);
        PressurePlate btnR = CreatePlate("Button_R", new Vector3(8f, 0.36f, 8f), mech);

        DoorController door = CreateDoor("Door_Exit", new Vector3(0f, 1.75f, 13f), new Vector3(6f, 3.5f, 1f), mech, new[] { btnL, btnR });
        door.latchOpen = false;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 17f), mech, "Level_05");
        CreateLevelGoal(mech, "Ambos lados al mismo tiempo.", "Sincronizado.", "Coordina el salto.", exit, btnL, btnR);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, -1f), true, 1, 12f);
        SpawnGameplayCamera(player.transform);

        SpawnPointLight("Light_BtnL", new Vector3(-8f, 2f, 8f), new Color(0.1f, 0.5f, 1f), 4f, 8f, env);
        SpawnPointLight("Light_BtnR", new Vector3(8f, 2f, 8f), new Color(0.1f, 0.5f, 1f), 4f, 8f, env);
        SpawnPointLight("Light_Door", new Vector3(0f, 3f, 13f), new Color(0.6f, 0.1f, 0.1f), 3f, 6f, env);
        SpawnPointLight("Light_Center", new Vector3(0f, 4f, 0f), new Color(0.8f, 0.8f, 0.8f), 2f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnLevelRuntime(mech, "", "Calcula bien tus saltos y los del eco.", "");
        SpawnAmbientLights(env, new Vector3(0f, 0f, 8f), 20f, 24f);
        SpawnSmokeVolume("L04_Fog", new Vector3(0f, -2f, 8f), new Vector3(24f, 4f, 24f), env, 45f);

        SaveScene(scene, "Level_04");
    }

    static void BuildLevel05()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_05";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");
        Transform tutorial = CreateRoot("--- TUTORIAL ---");

        SetupAtmosphere(new Color(0.03f, 0.03f, 0.06f, 1f), 0.055f, new Color(0.03f, 0.03f, 0.06f, 1f));
        SpawnDirectionalLight();
        MakeBackdrop("Backdrop", new Vector3(0f, 0f, 12f), 28f, 40f, 40f, env);

        // Main area
        MakePlatform("Floor_Main", new Vector3(0f, 0f, 0f), new Vector3(14f, 0.5f, 14f), env, _floorMat);
        // Far platform with real button
        MakePlatform("Floor_Far", new Vector3(0f, 0f, 20f), new Vector3(8f, 0.5f, 8f), env, _floorMat);
        // Exit area
        MakePlatform("Floor_Exit", new Vector3(0f, 0f, 32f), new Vector3(8f, 0.5f, 6f), env, _floorMat);

        // TRAP button - near start, tempting. Opens barrier but CLOSES exit door!
        PressurePlate btnTrap = CreatePlate("Button_TRAP", new Vector3(-4f, 0.36f, 0f), mech);
        // REAL button - far away
        PressurePlate btnReal = CreatePlate("Button_REAL", new Vector3(0f, 0.36f, 20f), mech);

        // Barrier between main and far - opened by trap button
        DoorController barrier = CreateDoor("Barrier", new Vector3(0f, 1.75f, 10f), new Vector3(8f, 3.5f, 1f), mech, new[] { btnTrap });
        barrier.latchOpen = false;

        // Exit door - needs REAL button
        DoorController exitDoor = CreateDoor("Door_Exit", new Vector3(0f, 1.75f, 28f), new Vector3(8f, 3.5f, 1f), mech, new[] { btnReal });
        exitDoor.latchOpen = false;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 32f), mech, "Level_06");
        CreateLevelGoal(mech, "El camino facil es una trampa.", "Puerta real abierta.", "No todo boton es tu amigo.", exit, btnReal);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, -5f), true, 1, 12f);
        SpawnGameplayCamera(player.transform);

        SpawnPointLight("Light_Trap", new Vector3(-4f, 2f, 0f), new Color(1f, 0.3f, 0.1f, 1f), 3f, 6f, env);
        SpawnPointLight("Light_Real", new Vector3(0f, 2f, 20f), new Color(0.1f, 0.5f, 1f), 4f, 6f, env);
        SpawnPointLight("Light_Barrier", new Vector3(0f, 3f, 10f), new Color(0.6f, 0.1f, 0.1f), 2f, 5f, env);
        SpawnPointLight("Light_ExitDoor", new Vector3(0f, 3f, 28f), new Color(0.6f, 0.1f, 0.1f), 2f, 5f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnLevelRuntime(mech, "", "Cuidado con los atajos.", "");
        SpawnAmbientLights(env, new Vector3(0f, 0f, 14f), 14f, 36f);
        SpawnSmokeVolume("L05_Fog", new Vector3(0f, 0.5f, 14f), new Vector3(16f, 3f, 36f), env, 35f);

        SaveScene(scene, "Level_05");
    }

    static void BuildLevel06()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_06";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");
        Transform tutorial = CreateRoot("--- TUTORIAL ---");

        SetupAtmosphere(new Color(0.03f, 0.03f, 0.06f, 1f), 0.05f, new Color(0.03f, 0.03f, 0.06f, 1f));
        SpawnDirectionalLight();
        MakeBackdrop("Backdrop", new Vector3(0f, 0f, 20f), 28f, 60f, 60f, env);

        // Long corridor with 3 checkpoints
        MakePlatform("Floor_Main", new Vector3(0f, 0f, 18f), new Vector3(8f, 0.5f, 48f), env, _floorMat);

        PressurePlate btn1 = CreatePlate("Button_1", new Vector3(-2f, 0.36f, 4f), mech);
        PressurePlate btn2 = CreatePlate("Button_2", new Vector3(2f, 0.36f, 18f), mech);
        PressurePlate btn3 = CreatePlate("Button_3", new Vector3(-2f, 0.36f, 32f), mech);
        btn1.autoReleaseTimer = 5f;
        btn2.autoReleaseTimer = 5f;
        btn3.autoReleaseTimer = 5f;

        DoorController door1 = CreateDoor("Door_1", new Vector3(0f, 1.75f, 11f), new Vector3(8f, 3.5f, 1f), mech, new[] { btn1 });
        door1.latchOpen = false;
        DoorController door2 = CreateDoor("Door_2", new Vector3(0f, 1.75f, 25f), new Vector3(8f, 3.5f, 1f), mech, new[] { btn2 });
        door2.latchOpen = false;
        DoorController door3 = CreateDoor("Door_3", new Vector3(0f, 1.75f, 38f), new Vector3(8f, 3.5f, 1f), mech, new[] { btn3 });
        door3.latchOpen = false;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 42f), mech, "MainMenu");
        CreateLevelGoal(mech, "Tres puertas. Un eco. Piensa en relevos.", "Eres libre.", "La suma de tus ecos.", exit, btn3);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, -2f), true, 1, 8f);
        SpawnGameplayCamera(player.transform);

        SpawnPointLight("Light_Btn1", new Vector3(-2f, 2f, 4f), new Color(0.1f, 0.5f, 1f), 3f, 6f, env);
        SpawnPointLight("Light_Btn2", new Vector3(2f, 2f, 18f), new Color(0.1f, 0.5f, 1f), 3f, 6f, env);
        SpawnPointLight("Light_Btn3", new Vector3(-2f, 2f, 32f), new Color(0.1f, 0.5f, 1f), 3f, 6f, env);
        SpawnPointLight("Light_D1", new Vector3(0f, 3f, 11f), new Color(0.6f, 0.1f, 0.1f), 2f, 5f, env);
        SpawnPointLight("Light_D2", new Vector3(0f, 3f, 25f), new Color(0.6f, 0.1f, 0.1f), 2f, 5f, env);
        SpawnPointLight("Light_D3", new Vector3(0f, 3f, 38f), new Color(0.6f, 0.1f, 0.1f), 2f, 5f, env);
        SpawnPointLight("Corridor_A", new Vector3(3f, 3f, 7f), new Color(0.4f, 0.35f, 0.6f), 0.8f, 7f, env);
        SpawnPointLight("Corridor_B", new Vector3(-3f, 3f, 21f), new Color(0.4f, 0.35f, 0.6f), 0.8f, 7f, env);
        SpawnPointLight("Corridor_C", new Vector3(3f, 3f, 35f), new Color(0.4f, 0.35f, 0.6f), 0.8f, 7f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnLevelRuntime(mech, "", "Solo 1 eco. Los botones tienen temporizador. Turnense.", "");
        SpawnAmbientLights(env, new Vector3(0f, 0f, 18f), 8f, 48f);
        SpawnSmokeVolume("L06_Fog", new Vector3(0f, 0.5f, 18f), new Vector3(10f, 3f, 50f), env, 40f);

        SaveScene(scene, "Level_06");
    }

    static GameObject SpawnPlayer(Vector3 position, bool enableRecorder, int maxEchoes, float maxRecordSeconds)
    {
        Transform playerRoot = CreateRoot("--- PLAYER ---");
        GameObject player = new GameObject("Player");
        player.transform.SetParent(playerRoot, false);
        player.transform.position = position;
        player.tag = "Player";

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2.2f;
        controller.radius = 0.36f;
        controller.center = new Vector3(0f, 1.1f, 0f);
        controller.skinWidth = 0.08f;

        // FIXED: Unity requiere que al menos un objeto tenga Rigidbody para que OnTriggerEnter se dispare
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        PlayerController playerController = player.AddComponent<PlayerController>();
        playerController.moveSpeed = 6f;
        playerController.acceleration = 24f;
        playerController.deceleration = 28f;
        playerController.rotationSharpness = 14f;
        playerController.jumpHeight = 1.55f;
        playerController.gravityStrength = 26f;
        playerController.groundProbeRadius = 0.24f;
        playerController.groundProbeDistance = 0.38f;
        playerController.groundedStickForce = 5f;
        playerController.sprintMultiplier = 1f;
        playerController.groundMask = -1;

        if (enableRecorder)
        {
            EchoRecorder recorder = player.AddComponent<EchoRecorder>();
            SetSerializedValue(recorder, "echoPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(EchoPrefabPath));
            SetSerializedValue(recorder, "maxEchoes", maxEchoes);
            SetSerializedValue(recorder, "maxRecordSeconds", maxRecordSeconds);
        }

        CreateCharacterVisual(player.transform);

        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform, false);
        groundCheck.transform.localPosition = new Vector3(0f, -0.96f, 0f);

        GameObject focus = new GameObject("CameraFocus");
        focus.transform.SetParent(player.transform, false);
        focus.transform.localPosition = new Vector3(0f, 1.75f, 0.18f);

        return player;
    }

    static void CreateCharacterVisual(Transform player)
    {
        GameObject visualRoot = new GameObject("PlayerVisual");
        visualRoot.transform.SetParent(player, false);
        CreateCapsuleVisual(visualRoot.transform, false);
    }

    static void SpawnGameplayCamera(Transform player)
    {
        Transform cameraRoot = CreateRoot("--- CAMERA ---");

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(cameraRoot, false);
        cameraObject.transform.position = player.position + new Vector3(0f, 12f, -16f);
        Camera cameraRef = cameraObject.AddComponent<Camera>();
        cameraRef.clearFlags = CameraClearFlags.SolidColor;
        cameraRef.backgroundColor = new Color(0.02f, 0.03f, 0.05f, 1f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CameraShake>();
        cameraObject.AddComponent<AudioSource>();
        cameraObject.AddComponent<GameFeelController>();
        FixedPuzzleCameraController fixedCamera = cameraObject.AddComponent<FixedPuzzleCameraController>();

        CinemachineBrain brain = cameraObject.AddComponent<CinemachineBrain>();
        brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        brain.m_DefaultBlend.m_Time = 0.35f;

        Transform playerFocus = player.Find("CameraFocus");
        Transform goalFocus = FindPrimaryGoalFocus();

        GameObject targetGroupObject = new GameObject("GameplayCameraTargets");
        targetGroupObject.transform.SetParent(cameraRoot, false);
        CinemachineTargetGroup targetGroup = targetGroupObject.AddComponent<CinemachineTargetGroup>();
        targetGroup.m_Targets = new[]
        {
            new CinemachineTargetGroup.Target
            {
                target = playerFocus != null ? playerFocus : player,
                weight = 1.35f,
                radius = 0.6f
            },
            new CinemachineTargetGroup.Target
            {
                target = goalFocus != null ? goalFocus : player,
                weight = goalFocus != null ? 0.52f : 0f,
                radius = 1.4f
            }
        };

        GameObject eventFocus = new GameObject("CameraEventFocus");
        eventFocus.transform.SetParent(targetGroupObject.transform, false);
        eventFocus.transform.position = goalFocus != null ? goalFocus.position : player.position;

        GameObject vcamObj = new GameObject("PlayerVCam");
        vcamObj.transform.SetParent(cameraRoot, false);
        CinemachineVirtualCamera vcam = vcamObj.AddComponent<CinemachineVirtualCamera>();
        vcam.Priority = 20;
        vcam.Follow = player;
        vcam.LookAt = targetGroup.transform;
        vcam.m_Lens.FieldOfView = 55f;

        CinemachineTransposer transposer = vcam.AddCinemachineComponent<CinemachineTransposer>();
        transposer.m_FollowOffset = new Vector3(0f, 12f, -16f);
        transposer.m_XDamping = 0.28f;
        transposer.m_YDamping = 0.34f;
        transposer.m_ZDamping = 0.28f;
        transposer.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;

        CinemachineComposer composer = vcam.AddCinemachineComponent<CinemachineComposer>();
        composer.m_TrackedObjectOffset = new Vector3(0f, 0.2f, 0f);
        composer.m_HorizontalDamping = 0.22f;
        composer.m_VerticalDamping = 0.3f;
        composer.m_DeadZoneWidth = 0.02f;
        composer.m_DeadZoneHeight = 0.02f;
        composer.m_SoftZoneWidth = 0.18f;
        composer.m_SoftZoneHeight = 0.12f;
        composer.m_ScreenY = 0.58f;

        fixedCamera.virtualCamera = vcam;
        fixedCamera.targetGroup = targetGroup;
        fixedCamera.followTarget = player;
        fixedCamera.playerFocus = playerFocus != null ? playerFocus : player;
        fixedCamera.goalFocus = goalFocus;
        fixedCamera.eventFocus = eventFocus.transform;
        fixedCamera.baseFov = 46f;
        fixedCamera.playerWeight = 1.35f;
        fixedCamera.goalWeight = 0.52f;
        fixedCamera.goalWeight = 0.52f;
    }

    static Transform FindPrimaryGoalFocus()
    {
        LevelExit exit = Object.FindAnyObjectByType<LevelExit>();
        if (exit != null)
        {
            Transform goalFocus = exit.transform.parent != null ? exit.transform.parent.Find("GoalFocus") : null;
            if (goalFocus != null) return goalFocus;
            return exit.transform;
        }

        GameObject area = GameObject.Find("LevelExit_Area");
        if (area != null) return area.transform;

        return null;
    }

    static void SpawnGameplayHud(Transform parent)
    {
        GameObject hud = new GameObject("GameHUD");
        hud.transform.SetParent(parent, false);
        hud.AddComponent<GameHUD>();
    }

    static void SpawnPauseMenu(Transform parent)
    {
        GameObject pause = new GameObject("PauseMenu");
        pause.transform.SetParent(parent, false);
        pause.AddComponent<PauseMenu>();
    }

    static void SpawnAmbientLights(Transform parent, Vector3 center, float width, float depth)
    {
        Color warmDim = new Color(0.85f, 0.75f, 0.6f, 1f);
        Color coolDim = new Color(0.4f, 0.55f, 0.75f, 1f);
        float intensity = 0.6f;
        float range = 10f;
        float halfW = width * 0.4f;
        float halfD = depth * 0.4f;

        SpawnPointLight("Amb_FL", center + new Vector3(-halfW, 3f, -halfD), warmDim, intensity, range, parent);
        SpawnPointLight("Amb_FR", center + new Vector3(halfW, 3f, -halfD), coolDim, intensity, range, parent);
        SpawnPointLight("Amb_BL", center + new Vector3(-halfW, 3f, halfD), coolDim, intensity, range, parent);
        SpawnPointLight("Amb_BR", center + new Vector3(halfW, 3f, halfD), warmDim, intensity, range, parent);
        SpawnPointLight("Amb_Center", center + new Vector3(0f, 5f, 0f), new Color(0.5f, 0.6f, 0.8f, 1f), 0.4f, 14f, parent);
    }

    // Declara la intención de diseño del puzzle para validación
    static void SpawnPuzzleIntent(Transform parent, int buttons, int actions, bool movement, bool timing, bool multiStep, float echoDistance, string note)
    {
        GameObject intentObj = new GameObject("PuzzleIntent");
        intentObj.transform.SetParent(parent, false);
        PuzzleIntent intent = intentObj.AddComponent<PuzzleIntent>();
        intent.buttonCount = buttons;
        intent.requiredActions = Mathf.Max(2, actions, buttons + 1);
        intent.requiresMovement = movement || buttons > 1;
        intent.requiresTiming = timing || actions >= 2;
        intent.isMultiStep = multiStep || buttons > 1;
        intent.minimumEchoDistance = echoDistance;
        SetSerializedValue(intent, "designNote", note);
    }

    // Genera waypoints visuales para guiar la ruta del eco
    static void SpawnEchoPathHint(Transform parent, Vector3[] waypoints)
    {
        GameObject pathObj = new GameObject("EchoPathHint");
        pathObj.transform.SetParent(parent, false);
        EchoPathHint hint = pathObj.AddComponent<EchoPathHint>();
        hint.SetWaypoints(waypoints);
    }

    static void SpawnLevelRuntime(Transform parent, string objective, string intro, string completion)
    {
        GameObject runtime = new GameObject("LevelRuntimeController");
        runtime.transform.SetParent(parent, false);
        LevelRuntimeController controller = runtime.AddComponent<LevelRuntimeController>();
        SetSerializedValue(controller, "objectiveText", objective);
        SetSerializedValue(controller, "introLine", intro);
        SetSerializedValue(controller, "completionLine", completion);
    }

    static PressurePlate CreatePlate(string name, Vector3 position, Transform parent)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        BoxCollider colliderRef = root.AddComponent<BoxCollider>();
        colliderRef.size = new Vector3(2f, 0.12f, 2f);
        colliderRef.isTrigger = true;

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3D Models/kenney_prototype-kit/Models/FBX format/button-floor-square.fbx");
        GameObject visual;
        if (prefab != null)
        {
            visual = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, -0.06f, 0f);
            visual.transform.localScale = new Vector3(2f, 1f, 2f);
            Collider[] cols = visual.GetComponentsInChildren<Collider>();
            foreach (var c in cols) Object.DestroyImmediate(c);
            ApplyMaterialOverride(visual, _plateMat);
        }
        else
        {
            visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(2f, 0.12f, 2f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<MeshRenderer>().sharedMaterial = _plateMat;
            visual.AddComponent<KenneyTiling>();
        }

        // Punto de luz azul-violeta sobre cada botón
        SpawnPointLight(name + "_Glow", position + new Vector3(0f, 2f, 0f),
            new Color(0.24f, 0.56f, 0.74f, 1f), 1.8f, 6f, root.transform);

        return root.AddComponent<PressurePlate>();
    }

    static DoorController CreateDoor(string name, Vector3 position, Vector3 scale, Transform parent, PressurePlate[] plates)
    {
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = name;
        door.transform.SetParent(parent, false);
        door.transform.position = position;
        door.transform.localScale = scale;
        door.GetComponent<MeshRenderer>().sharedMaterial = _doorMat;
        door.AddComponent<KenneyTiling>();
        DoorController controller = door.AddComponent<DoorController>();
        controller.plates = plates;
        return controller;
    }

    static TimedMovingPlatform CreateBridge(string name, Vector3 anchorPosition, Vector3 inactiveLocal, Vector3 activeLocal, Vector3 scale, PressurePlate plate, Transform parent)
    {
        GameObject anchor = new GameObject(name + "_Anchor");
        anchor.transform.SetParent(parent, false);
        anchor.transform.position = anchorPosition;

        GameObject bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge.name = name;
        bridge.transform.SetParent(anchor.transform, false);
        bridge.transform.localPosition = inactiveLocal;
        bridge.transform.localScale = scale;
        bridge.layer = GroundLayer;
        bridge.GetComponent<MeshRenderer>().sharedMaterial = _bridgeMat;
        bridge.AddComponent<KenneyTiling>();

        TimedMovingPlatform platform = bridge.AddComponent<TimedMovingPlatform>();
        platform.plate = plate;
        platform.inactiveLocal = inactiveLocal;
        platform.activeLocal = activeLocal;
        platform.travelSpeed = 6f;
        return platform;
    }

    static LevelExit CreateLevelExit(Vector3 position, Transform parent, string nextSceneName, string completionToast = "")
    {
        GameObject exitRoot = new GameObject("LevelExit_Area");
        exitRoot.transform.SetParent(parent, false);
        exitRoot.transform.position = position;

        // Bigger collider for reliable detection
        BoxCollider col = exitRoot.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(3.5f, 4f, 2.5f);
        col.center = new Vector3(0f, 0.5f, 0f);

        // Rigidbody for reliable trigger events
        Rigidbody rb = exitRoot.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        GameObject goalFocus = new GameObject("GoalFocus");
        goalFocus.transform.SetParent(exitRoot.transform, false);
        goalFocus.transform.localPosition = new Vector3(0f, 2f, 0f);

        LevelExit exitComponent = exitRoot.AddComponent<LevelExit>();
        exitComponent.loadNextBuildIndex = false;
        exitComponent.nextSceneName = nextSceneName;
        if (!string.IsNullOrEmpty(completionToast))
            SetSerializedValue(exitComponent, "completionToast", completionToast);

        // --- PORTAL PILLARS ---
        GameObject leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftPillar.name = "LeftPillar";
        leftPillar.transform.SetParent(exitRoot.transform, false);
        leftPillar.transform.localPosition = new Vector3(-1.6f, 0.5f, 0f);
        leftPillar.transform.localScale = new Vector3(0.4f, 4.5f, 0.4f);
        leftPillar.GetComponent<MeshRenderer>().sharedMaterial = _bridgeMat;

        GameObject rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightPillar.name = "RightPillar";
        rightPillar.transform.SetParent(exitRoot.transform, false);
        rightPillar.transform.localPosition = new Vector3(1.6f, 0.5f, 0f);
        rightPillar.transform.localScale = new Vector3(0.4f, 4.5f, 0.4f);
        rightPillar.GetComponent<MeshRenderer>().sharedMaterial = _bridgeMat;

        GameObject topBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topBeam.name = "TopBeam";
        topBeam.transform.SetParent(exitRoot.transform, false);
        topBeam.transform.localPosition = new Vector3(0f, 2.8f, 0f);
        topBeam.transform.localScale = new Vector3(3.6f, 0.4f, 0.4f);
        topBeam.GetComponent<MeshRenderer>().sharedMaterial = _goalMat;

        // --- PORTAL SURFACE (glowing quad) ---
        GameObject portalSurface = GameObject.CreatePrimitive(PrimitiveType.Quad);
        portalSurface.name = "PortalSurface";
        portalSurface.transform.SetParent(exitRoot.transform, false);
        portalSurface.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        portalSurface.transform.localScale = new Vector3(2.8f, 4f, 1f);
        Object.DestroyImmediate(portalSurface.GetComponent<Collider>());

        Material portalMat = new Material(Shader.Find("Standard"));
        portalMat.color = new Color(0.25f, 0.45f, 0.9f, 0.12f);
        portalMat.SetFloat("_Mode", 3);
        portalMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        portalMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        portalMat.SetInt("_ZWrite", 0);
        portalMat.DisableKeyword("_ALPHATEST_ON");
        portalMat.EnableKeyword("_ALPHABLEND_ON");
        portalMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        portalMat.renderQueue = 3000;
        portalMat.EnableKeyword("_EMISSION");
        portalMat.SetColor("_EmissionColor", new Color(0.3f, 0.5f, 0.95f) * 2.2f);
        portalSurface.GetComponent<MeshRenderer>().sharedMaterial = portalMat;

        // --- SKY BEAM ---
        GameObject skyBeam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        skyBeam.name = "SkyBeam";
        skyBeam.transform.SetParent(exitRoot.transform, false);
        skyBeam.transform.localPosition = new Vector3(0f, 25f, 0f);
        skyBeam.transform.localScale = new Vector3(0.6f, 25f, 0.6f);
        Object.DestroyImmediate(skyBeam.GetComponent<Collider>());

        Material beamMat = new Material(Shader.Find("Standard"));
        beamMat.color = new Color(0.5f, 0.65f, 0.9f, 0.18f);
        beamMat.SetFloat("_Mode", 3);
        beamMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        beamMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        beamMat.SetInt("_ZWrite", 0);
        beamMat.DisableKeyword("_ALPHATEST_ON");
        beamMat.EnableKeyword("_ALPHABLEND_ON");
        beamMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        beamMat.renderQueue = 3000;
        beamMat.EnableKeyword("_EMISSION");
        beamMat.SetColor("_EmissionColor", new Color(0.4f, 0.55f, 0.85f) * 1.8f);
        skyBeam.GetComponent<MeshRenderer>().sharedMaterial = beamMat;

        // --- LIGHTS (portal glow) ---
        SpawnPointLight("ExitBeacon", position + new Vector3(0f, 5f, 0f),
            new Color(0.6f, 0.75f, 1f, 1f), 6f, 28f, exitRoot.transform);
        SpawnPointLight("ExitGlow", position + new Vector3(0f, 1.5f, 0f),
            new Color(0.4f, 0.6f, 0.95f, 1f), 4f, 14f, exitRoot.transform);
        SpawnPointLight("ExitRimL", position + new Vector3(-1.8f, 2f, 0f),
            new Color(0.3f, 0.5f, 1f, 1f), 2f, 6f, exitRoot.transform);
        SpawnPointLight("ExitRimR", position + new Vector3(1.8f, 2f, 0f),
            new Color(0.3f, 0.5f, 1f, 1f), 2f, 6f, exitRoot.transform);
        SpawnPointLight("ExitBase", position + new Vector3(0f, 0.3f, 0f),
            new Color(0.5f, 0.7f, 1f, 1f), 3f, 8f, exitRoot.transform);

        // --- PARTICLES (floating motes) ---
        GameObject motes = new GameObject("ExitParticles");
        motes.transform.SetParent(exitRoot.transform, false);
        motes.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        ParticleSystem ps = motes.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psr = motes.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = _goalMat;
        psr.renderMode = ParticleSystemRenderMode.Billboard;

        var psMain = ps.main;
        psMain.loop = true;
        psMain.playOnAwake = true;
        psMain.startLifetime = new ParticleSystem.MinMaxCurve(2f, 5f);
        psMain.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        psMain.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
        psMain.startColor = new ParticleSystem.MinMaxGradient(new Color(0.5f, 0.7f, 1f, 0.8f));
        psMain.maxParticles = 60;
        psMain.simulationSpace = ParticleSystemSimulationSpace.World;

        var psEmission = ps.emission;
        psEmission.rateOverTime = 15f;

        var psShape = ps.shape;
        psShape.shapeType = ParticleSystemShapeType.Box;
        psShape.scale = new Vector3(2.5f, 4f, 1.5f);

        var psVel = ps.velocityOverLifetime;
        psVel.enabled = true;
        psVel.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        psVel.y = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        psVel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var psCol = ps.colorOverLifetime;
        psCol.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.5f, 0.7f, 1f), 0f), new GradientColorKey(new Color(0.8f, 0.9f, 1f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.8f, 0.2f), new GradientAlphaKey(0.6f, 0.7f), new GradientAlphaKey(0f, 1f) });
        psCol.color = new ParticleSystem.MinMaxGradient(grad);

        return exitComponent;
    }

    static LevelGoal CreateLevelGoal(Transform parent, string objectiveText, string readyPrompt, string completionToast, LevelExit exit, params PressurePlate[] plates)
    {
        GameObject goalObject = new GameObject("LevelGoal");
        goalObject.transform.SetParent(parent, false);
        goalObject.transform.position = exit != null ? exit.transform.position : parent.position;

        for (int i = 0; i < plates.Length; i++)
            CreateGoalTrigger(goalObject.transform, plates[i], "Memoria " + (i + 1));

        LevelGoal goal = goalObject.AddComponent<LevelGoal>();
        SetSerializedValue(goal, "objectiveText", objectiveText);
        SetSerializedValue(goal, "readyPrompt", readyPrompt);
        SetSerializedValue(goal, "completionToast", completionToast);
        // CRITICAL: must be true so Awake() auto-discovers child GoalTriggers
        // SetSerializedValue cannot handle Object[] arrays, so manual array assignment fails
        SetSerializedValue(goal, "autoCollectChildTriggers", true);
        SetSerializedValue(goal, "requiredTriggerCount", plates.Length);
        // linkedExits fallback in LevelGoal.Awake uses FindObjectsOfType when null

        return goal;
    }

    static GoalTrigger CreateGoalTrigger(Transform parent, PressurePlate plate, string displayName)
    {
        GameObject triggerObject = new GameObject(displayName.Replace(" ", string.Empty) + "_Goal");
        triggerObject.transform.SetParent(parent, false);
        if (plate != null)
            triggerObject.transform.position = plate.transform.position + new Vector3(0f, 0.4f, 0f);

        GoalTrigger trigger = triggerObject.AddComponent<GoalTrigger>();
        SetSerializedValue(trigger, "displayName", displayName);
        SetSerializedValue(trigger, "pressurePlate", plate);
        SetSerializedValue(trigger, "usePlatePressedState", true);
        SetSerializedValue(trigger, "useDoorOpenState", false);
        SetSerializedValue(trigger, "accumulateOnce", true);
        return trigger;
    }

    static void CreateTutorialTrigger(string name, Vector3 position, Vector3 size, string message, string hint, float duration, Transform parent)
    {
        GameObject triggerObject = new GameObject(name);
        triggerObject.transform.SetParent(parent, false);
        triggerObject.transform.position = position;

        BoxCollider colliderRef = triggerObject.AddComponent<BoxCollider>();
        colliderRef.isTrigger = true;
        colliderRef.size = size;

        TutorialTrigger trigger = triggerObject.AddComponent<TutorialTrigger>();
        SetSerializedValue(trigger, "message", message);
        SetSerializedValue(trigger, "hint", hint);
        SetSerializedValue(trigger, "displayDuration", duration);
        SetSerializedValue(trigger, "oneShot", true);
    }

    static void CreateHubPortal(Vector3 position, string sceneName, string displayName, string memoryLine, Transform mechanics, Transform visuals)
    {
        MakePlatform(displayName + "_Pedestal", position, new Vector3(3f, 0.5f, 3f), visuals, _bridgeMat);

        GameObject portalRoot = new GameObject(displayName + "_Portal");
        portalRoot.transform.SetParent(mechanics, false);
        portalRoot.transform.position = position + new Vector3(0f, 0.5f, 0f);
        portalRoot.transform.rotation = Quaternion.LookRotation((Vector3.zero - position).normalized, Vector3.up);

        GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
        left.name = "LeftPillar";
        left.transform.SetParent(portalRoot.transform, false);
        left.transform.localPosition = new Vector3(-1.1f, 2.2f, 0f);
        left.transform.localScale = new Vector3(0.45f, 4.2f, 0.45f);
        left.GetComponent<MeshRenderer>().sharedMaterial = _bridgeMat;

        GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
        right.name = "RightPillar";
        right.transform.SetParent(portalRoot.transform, false);
        right.transform.localPosition = new Vector3(1.1f, 2.2f, 0f);
        right.transform.localScale = new Vector3(0.45f, 4.2f, 0.45f);
        right.GetComponent<MeshRenderer>().sharedMaterial = _bridgeMat;

        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
        top.name = "TopBeam";
        top.transform.SetParent(portalRoot.transform, false);
        top.transform.localPosition = new Vector3(0f, 4.1f, 0f);
        top.transform.localScale = new Vector3(2.8f, 0.35f, 0.45f);
        top.GetComponent<MeshRenderer>().sharedMaterial = _bridgeMat;

        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glow.name = "PortalGlow";
        glow.transform.SetParent(portalRoot.transform, false);
        glow.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        glow.transform.localScale = new Vector3(2f, 3.2f, 0.15f);
        glow.GetComponent<MeshRenderer>().sharedMaterial = _goalMat;
        Object.DestroyImmediate(glow.GetComponent<BoxCollider>());

        BoxCollider trigger = portalRoot.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 2.2f, 0f);
        trigger.size = new Vector3(2.8f, 4.2f, 2.5f);

        Light light = SpawnPointLight(displayName + "_Light", position + new Vector3(0f, 3.2f, 0f), new Color(0.16f, 0.85f, 1f, 1f), 3.5f, 8f, portalRoot.transform);

        HubPortal portal = portalRoot.AddComponent<HubPortal>();
        SetSerializedValue(portal, "sceneName", sceneName);
        SetSerializedValue(portal, "displayName", displayName);
        SetSerializedValue(portal, "memoryLine", memoryLine);
        SetSerializedValue(portal, "portalLight", light);
    }

    static GameObject MakePlatform(string name, Vector3 position, Vector3 scale, Transform parent, Material material)
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = name;
        if (parent != null)
            platform.transform.SetParent(parent, false);
        platform.transform.position = position;
        platform.transform.localScale = scale;
        platform.layer = GroundLayer;
        platform.isStatic = true;
        platform.GetComponent<MeshRenderer>().sharedMaterial = material;
        platform.AddComponent<KenneyTiling>();
        return platform;
    }

    static void MakeBackdrop(string prefix, Vector3 center, float width, float height, float depth, Transform parent)
    {
        MakePlatform(prefix + "_Back", center + new Vector3(0f, height * 0.5f, depth * 0.5f), new Vector3(width, height, 1f), parent, _bridgeMat);
        MakePlatform(prefix + "_Left", center + new Vector3(-width * 0.5f, height * 0.5f, 0f), new Vector3(0.5f, height, depth), parent, _bridgeMat);
        MakePlatform(prefix + "_Right", center + new Vector3(width * 0.5f, height * 0.5f, 0f), new Vector3(0.5f, height, depth), parent, _bridgeMat);
        MakePlatform(prefix + "_Ceiling", center + new Vector3(0f, height, depth * 0.1f), new Vector3(width, 0.5f, depth * 0.8f), parent, _bridgeMat);
    }

    static void SetupAtmosphere(Color originalFogColor, float originalFogDensity, Color originalAmbientColor)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.04f, 0.06f, 0.09f, 1f); // #0A0F18 aprox
        RenderSettings.fogDensity = 0.035f; // Progresiva: oculta bordes, no tapa gameplay
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.03f, 0.04f, 0.06f, 1f);
        RenderSettings.ambientEquatorColor = Color.black;
        RenderSettings.ambientGroundColor = Color.black;
        RenderSettings.skybox = null;

        // Ground Fog
        GameObject atmosphere = new GameObject("AtmosphereController");
        AtmosphereController atmoController = atmosphere.AddComponent<AtmosphereController>();
        SetSerializedValue(atmoController, "enableGroundFog", true);
        SetSerializedValue(atmoController, "maxFogParticles", 100);
    }

    static void SpawnDirectionalLight()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light lightRef = lightObject.AddComponent<Light>();
        lightRef.type = LightType.Directional;
        lightRef.color = new Color(0.45f, 0.5f, 0.6f, 1f); // Azulado frío
        lightRef.intensity = 0.25f; // Muy tenue — la iluminación la dan las point lights
        lightRef.shadows = LightShadows.Soft;
        lightRef.shadowStrength = 0.85f;
        lightObject.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
    }

    static Light SpawnPointLight(string name, Vector3 position, Color color, float intensity, float range, Transform parent)
    {
        GameObject lightObject = new GameObject(name);
        if (parent != null)
            lightObject.transform.SetParent(parent, false);
        lightObject.transform.position = position;

        Light lightRef = lightObject.AddComponent<Light>();
        lightRef.type = LightType.Point;
        lightRef.color = color;
        lightRef.intensity = intensity;
        lightRef.range = range;
        lightRef.shadows = LightShadows.None;
        return lightRef;
    }

    static void SpawnAmbientParticles(Vector3 position, Vector3 boxScale)
    {
        GameObject particleObject = new GameObject("EnvironmentParticles");
        particleObject.transform.position = position;
        ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
        ParticleSystemRenderer rendererRef = particleObject.GetComponent<ParticleSystemRenderer>();
        rendererRef.sharedMaterial = _goalMat;

        var main = particleSystem.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.maxParticles = 80;

        var emission = particleSystem.emission;
        emission.rateOverTime = 9f;

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = boxScale;
    }

    static void SpawnSmokeVolume(string name, Vector3 position, Vector3 boxScale, Transform parent, float rateOverTime)
    {
        GameObject particleObject = new GameObject(name);
        if (parent != null)
            particleObject.transform.SetParent(parent, false);
        particleObject.transform.position = position;

        ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
        ParticleSystemRenderer rendererRef = particleObject.GetComponent<ParticleSystemRenderer>();
        rendererRef.sharedMaterial = LoadSmokeMaterial();
        rendererRef.renderMode = ParticleSystemRenderMode.Billboard;
        rendererRef.sortMode = ParticleSystemSortMode.Distance;

        // Subtle, gentle mist instead of heavy smoke
        float adjustedRate = rateOverTime * 0.4f; // Much less dense

        var main = particleSystem.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 16f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.06f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 1.8f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.55f, 0.6f, 0.72f, 0.04f),  // Cool blue-gray, very transparent
            new Color(0.65f, 0.68f, 0.78f, 0.07f));
        main.maxParticles = Mathf.RoundToInt(adjustedRate * 2.5f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particleSystem.emission;
        emission.rateOverTime = adjustedRate;

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = boxScale;

        var velocity = particleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.01f, 0.04f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);

        var noise = particleSystem.noise;
        noise.enabled = true;
        noise.strength = 0.08f;
        noise.frequency = 0.15f;

        var color = particleSystem.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.6f, 0.65f, 0.75f), 0f),
                new GradientColorKey(new Color(0.55f, 0.6f, 0.7f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.06f, 0.2f),
                new GradientAlphaKey(0.05f, 0.75f),
                new GradientAlphaKey(0f, 1f)
            });
        color.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    static void SpawnLiminalDressing(string prefix, Vector3 center, float width, float depth, Transform parent)
    {
        float leftX = center.x - width * 0.5f + 1.15f;
        float rightX = center.x + width * 0.5f - 1.15f;
        float startZ = center.z - depth * 0.45f;
        float endZ = center.z + depth * 0.45f;

        for (float z = startZ; z <= endZ; z += 6f)
        {
            TrySpawnDecorPrefab(prefix + "_FenceL_" + z.ToString("0"), FenceStraightPath, new Vector3(leftX, 0f, z), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.25f, parent);
            TrySpawnDecorPrefab(prefix + "_FenceR_" + z.ToString("0"), FenceStraightPath, new Vector3(rightX, 0f, z), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 1.25f, parent);
        }

        for (float z = startZ + 3f; z <= endZ; z += 12f)
        {
            TrySpawnDecorPrefab(prefix + "_PolesL_" + z.ToString("0"), PolesPath, new Vector3(leftX + 1.2f, 0f, z), Quaternion.identity, Vector3.one * 1.05f, parent);
            TrySpawnDecorPrefab(prefix + "_PolesR_" + z.ToString("0"), PolesPath, new Vector3(rightX - 1.2f, 0f, z), Quaternion.identity, Vector3.one * 1.05f, parent);
            SpawnPointLight(prefix + "_LampL_" + z.ToString("0"), new Vector3(leftX + 1.2f, 4.2f, z), new Color(0.9f, 0.88f, 0.78f, 1f), 1.05f, 7.5f, parent);
            SpawnPointLight(prefix + "_LampR_" + z.ToString("0"), new Vector3(rightX - 1.2f, 4.2f, z), new Color(0.9f, 0.88f, 0.78f, 1f), 1.05f, 7.5f, parent);
        }

        TrySpawnDecorPrefab(prefix + "_PipeL", PipePath, new Vector3(leftX + 0.9f, 0f, endZ - 3f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.05f, parent);
        TrySpawnDecorPrefab(prefix + "_PipeR", PipePath, new Vector3(rightX - 0.9f, 0f, endZ - 3f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 1.05f, parent);
    }

    static void TrySpawnDecorPrefab(string name, string assetPath, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
            return;

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
            return;

        instance.name = name;
        if (parent != null)
            instance.transform.SetParent(parent, false);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale = scale;
    }

    static Material LoadSmokeMaterial()
    {
        Material smokeMaterial = AssetDatabase.LoadAssetAtPath<Material>(SmokeDarkMaterialPath);
        return smokeMaterial != null ? smokeMaterial : (_echoMat != null ? _echoMat : _goalMat);
    }

    static Transform CreateRoot(string name)
    {
        GameObject root = new GameObject(name);
        return root.transform;
    }

    static void EnsureFolders()
    {
        EnsureFolder(MaterialRoot);
        EnsureFolder(PrefabRoot);
        EnsureFolder(SceneRoot);
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    static void EnsureMaterials()
    {
        Texture2D gridDark = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/3D Models/kenney_prototype-kit/Models/Textures/variation-a.png");
        Texture2D gridLight = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/3D Models/kenney_prototype-kit/Models/Textures/variation-b.png");
        Texture2D gridNeutral = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/3D Models/kenney_prototype-kit/Models/Textures/variation-c.png");

        _floorMat = GetOrCreateMaterial("Mat_Floor", new Color(0.04f, 0.05f, 0.08f, 1f), false, gridDark);
        _plateMat = GetOrCreateEmissiveMaterial("Mat_Plate", new Color(0.1f, 0.4f, 1f, 1f), new Color(0.1f, 0.4f, 1f) * 1.5f, gridLight); // Azul brillante
        _bridgeMat = GetOrCreateMaterial("Mat_Bridge", new Color(0.02f, 0.03f, 0.05f, 1f), false, gridNeutral);
        _doorMat = GetOrCreateEmissiveMaterial("Mat_Door", new Color(0.45f, 0.08f, 0.1f, 1f), new Color(0.4f, 0.05f, 0.05f) * 0.8f, gridLight); // Rojo oscuro con glow
        _goalMat = GetOrCreateEmissiveMaterial("Mat_Exit", new Color(0.95f, 0.95f, 1f, 1f), new Color(1f, 1f, 1f) * 1.5f, gridLight); // Blanco
        _playerMat = GetOrCreateMaterial("Mat_Player", new Color(0.95f, 0.98f, 1f, 1f), false);
        _echoMat = GetOrCreateTransparentMaterial("Mat_Echo", new Color(0.65f, 0.15f, 1f, 0.35f), true); // Violeta
    }

    static Material GetOrCreateMaterial(string name, Color color, bool emissive = false, Texture2D tex = null)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            material.color = color;
            if (tex != null) material.mainTexture = tex;
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 0.8f);
            }
            return material;
        }

        material = new Material(Shader.Find("Standard"));
        material.color = color;
        if (tex != null) material.mainTexture = tex;
        if (emissive)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.8f);
        }

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    static Material GetOrCreateTransparentMaterial(string name, Color color, bool emissive)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            material.color = color;
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.4f);
            }
            return material;
        }

        material = new Material(Shader.Find("Standard"));
        material.color = color;
        material.SetFloat("_Mode", 3f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        if (emissive)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.4f);
        }

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    static Material GetOrCreateEmissiveMaterial(string name, Color color, Color emissionColor, Texture2D tex = null)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            material.color = color;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emissionColor);
            if (tex != null) material.mainTexture = tex;
            return material;
        }

        material = new Material(Shader.Find("Standard"));
        material.color = color;
        if (tex != null) material.mainTexture = tex;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emissionColor);

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    static void EnsureAnimatorController()
    {
        if (AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(AnimatorControllerPath) != null)
            return;

        UnityEditor.Animations.AnimatorController controller =
            UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(AnimatorControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("VelocityX", AnimatorControllerParameterType.Float);
        controller.AddParameter("VelocityZ", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);

        UnityEditor.Animations.AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimationClip idleClip = LoadClipFromFBX("Assets/3D Models/Animaciones/Locomotion/idle.fbx");
        AnimationClip walkClip = LoadClipFromFBX("Assets/3D Models/Animaciones/Locomotion/walking.fbx");
        AnimationClip runClip = LoadClipFromFBX("Assets/3D Models/Animaciones/Locomotion/running.fbx");
        AnimationClip jumpClip = LoadClipFromFBX("Assets/3D Models/Animaciones/Locomotion/jump.fbx");
        AnimationClip leftStrafeClip = LoadClipFromFBX("Assets/3D Models/Animaciones/Locomotion/left strafe walking.fbx");
        AnimationClip rightStrafeClip = LoadClipFromFBX("Assets/3D Models/Animaciones/Locomotion/right strafe walking.fbx");

        var moveState = stateMachine.AddState("Move");
        var blendTree = new UnityEditor.Animations.BlendTree
        {
            name = "Locomotion",
            blendType = UnityEditor.Animations.BlendTreeType.SimpleDirectional2D,
            blendParameter = "VelocityX",
            blendParameterY = "VelocityZ"
        };
        if (idleClip != null) blendTree.AddChild(idleClip, new Vector2(0f, 0f));
        if (walkClip != null) blendTree.AddChild(walkClip, new Vector2(0f, 2f));
        if (runClip != null) blendTree.AddChild(runClip, new Vector2(0f, 6f));
        if (leftStrafeClip != null) blendTree.AddChild(leftStrafeClip, new Vector2(-2f, 0f));
        if (rightStrafeClip != null) blendTree.AddChild(rightStrafeClip, new Vector2(2f, 0f));
        moveState.motion = blendTree;
        stateMachine.defaultState = moveState;

        if (jumpClip != null)
        {
            var jumpState = stateMachine.AddState("Jump");
            jumpState.motion = jumpClip;

            var anyToJump = stateMachine.AddAnyStateTransition(jumpState);
            anyToJump.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0f, "Jump");

            var jumpToMove = jumpState.AddTransition(moveState);
            jumpToMove.hasExitTime = true;
            jumpToMove.exitTime = 0.78f;
            jumpToMove.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0f, "Grounded");
        }

        AssetDatabase.AddObjectToAsset(blendTree, controller);
    }

    static void EnsureEchoPrefab()
    {
        GameObject root = new GameObject("EchoPrefab");
        root.tag = "Echo";

        Rigidbody body = root.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        CharacterController controller = root.AddComponent<CharacterController>();
        controller.height = 2.2f;
        controller.radius = 0.36f;
        controller.center = new Vector3(0f, 1.1f, 0f);
        controller.skinWidth = 0.08f;

        root.AddComponent<EchoPlayback>();

        GameObject visualRoot = new GameObject("Visual");
        visualRoot.transform.SetParent(root.transform, false);
        CreateCapsuleVisual(visualRoot.transform, true);

        PrefabUtility.SaveAsPrefabAsset(root, EchoPrefabPath);
        Object.DestroyImmediate(root);
    }

    static GameObject CreateCapsuleVisual(Transform parent, bool useEchoMaterial)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3D Models/lowpoly-character-freerigged-/source/LowPolyCharacterModel/FBX/LowPolyCharacter.fbx");
        GameObject visual;
        
        if (prefab != null)
        {
            GameObject scaler = new GameObject(useEchoMaterial ? "EchoScaler" : "PlayerScaler");
            scaler.transform.SetParent(parent, false);
            scaler.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

            visual = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            visual.name = "Model";
            visual.transform.SetParent(scaler.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            
            Collider[] colliders = visual.GetComponentsInChildren<Collider>();
            foreach (var col in colliders) Object.DestroyImmediate(col);

            SkinnedMeshRenderer[] renderers = visual.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var r in renderers)
            {
                // Si es Player, mantener materiales originales. Si es Eco, usar el material transparente violeta.
                if (useEchoMaterial)
                {
                    Material[] mats = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < mats.Length; i++)
                        mats[i] = _echoMat;
                    r.sharedMaterials = mats;
                }
            }

            Animator anim = visual.GetComponent<Animator>();
            if (anim == null) anim = visual.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(AnimatorControllerPath);
            anim.avatar = AssetDatabase.LoadAssetAtPath<Avatar>("Assets/3D Models/lowpoly-character-freerigged-/source/LowPolyCharacterModel/FBX/LowPolyCharacter.fbx");
        }
        else
        {
            visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = useEchoMaterial ? "EchoCapsule" : "PlayerCapsule";
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(0.8f, 1.05f, 0.8f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<MeshRenderer>().sharedMaterial = useEchoMaterial ? _echoMat : _playerMat;
        }

        return visual;
    }

    static void SetSerializedValue(Component component, string propertyName, object value)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            return;

        switch (value)
        {
            case int intValue:
                property.intValue = intValue;
                break;
            case float floatValue:
                property.floatValue = floatValue;
                break;
            case bool boolValue:
                property.boolValue = boolValue;
                break;
            case string stringValue:
                property.stringValue = stringValue;
                break;
            case Color colorValue:
                property.colorValue = colorValue;
                break;
            case Object objectValue:
                property.objectReferenceValue = objectValue;
                break;
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SaveScene(Scene scene, string sceneName)
    {
        EditorSceneManager.SaveScene(scene, $"{SceneRoot}/{sceneName}.unity");
    }

    static void UpdateBuildSettings()
    {
        string[] scenePaths =
        {
            $"{SceneRoot}/MainMenu.unity",
            $"{SceneRoot}/Level_07.unity",
            $"{SceneRoot}/Level_01.unity",
            $"{SceneRoot}/Level_02.unity",
            $"{SceneRoot}/Level_03.unity",
            $"{SceneRoot}/Level_04.unity",
            $"{SceneRoot}/Level_05.unity",
            $"{SceneRoot}/Level_06.unity"
        };

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        for (int i = 0; i < scenePaths.Length; i++)
            scenes.Add(new EditorBuildSettingsScene(scenePaths[i], true));

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static AnimationClip LoadClipFromFBX(string path)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        if (assets != null)
        {
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip && !clip.name.Contains("__preview__"))
                    return clip;
            }
        }
        return null;
    }

    static void ApplyMaterialOverride(GameObject obj, Material mat)
    {
        if (obj == null || mat == null) return;
        MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].sharedMaterial = mat;
        }
    }
}
