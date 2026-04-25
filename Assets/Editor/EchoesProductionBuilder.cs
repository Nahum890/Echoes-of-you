using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        Debug.Log("[Echoes Production] Scenes rebuilt.");
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

        GameObject menu = new GameObject("MainMenu");
        menu.AddComponent<MainMenu>();

        SaveScene(scene, "MainMenu");
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

        SetupAtmosphere(new Color(0.11f, 0.17f, 0.22f, 1f), 0.022f, new Color(0.12f, 0.16f, 0.2f, 1f));
        SpawnDirectionalLight();
        MakeBackdrop("Backdrop", new Vector3(0f, 0f, 16f), 18f, 42f, 10f, env);

        MakePlatform("Platform_Start", new Vector3(0f, 0f, 0f), new Vector3(8f, 0.5f, 8f), env, _floorMat);
        MakePlatform("Walk_A", new Vector3(0f, 0f, 6f), new Vector3(3f, 0.5f, 4f), env, _floorMat);
        MakePlatform("Platform_Plate", new Vector3(0f, 0f, 11f), new Vector3(6f, 0.5f, 6f), env, _floorMat);
        MakePlatform("Walk_B", new Vector3(0f, 0f, 16f), new Vector3(3f, 0.5f, 4f), env, _floorMat);
        MakePlatform("Platform_Gate", new Vector3(0f, 0f, 22f), new Vector3(6f, 0.5f, 4f), env, _floorMat);
        MakePlatform("Platform_Exit", new Vector3(0f, 0f, 31f), new Vector3(8f, 0.5f, 8f), env, _floorMat);

        PressurePlate plate = CreatePlate("PressurePlate_A", new Vector3(0f, 0.36f, 11f), mech);
        CreateDoor("MemoryGate_A", new Vector3(0f, 1.75f, 19f), new Vector3(4f, 3.5f, 0.5f), mech, new[] { plate });
        CreateLevelExit(new Vector3(0f, 1.25f, 34f), mech, "Level_07");
        SpawnPointLight("GoalLight", new Vector3(0f, 3.2f, 31f), new Color(1f, 0.83f, 0.42f, 1f), 4f, 10f, env);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 6f);
        SpawnGameplayCamera(player.transform);
        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnLevelRuntime(mech, "Deja un eco sosteniendo la placa.", "Cada decision deja una forma de ti.", "Primero recuerdas.");

        CreateTutorialTrigger("Hint_Move", new Vector3(0f, 1.2f, 1f), new Vector3(4f, 3f, 4f), "WASD para moverte", "La salida ya esta delante.", 3.2f, tutorial);
        CreateTutorialTrigger("Hint_Record", new Vector3(0f, 1.2f, 10.5f), new Vector3(4f, 3f, 4f), "Mantén R sobre la placa", "Suelta para dejar un eco.", 3.5f, tutorial);

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

        SetupAtmosphere(new Color(0.11f, 0.17f, 0.22f, 1f), 0.024f, new Color(0.12f, 0.16f, 0.2f, 1f));
        SpawnDirectionalLight();
        MakeBackdrop("Backdrop", new Vector3(0f, 0f, 12f), 24f, 34f, 10f, env);

        MakePlatform("Platform_Start", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.5f, 8f), env, _floorMat);
        MakePlatform("Walk_Left", new Vector3(-3.5f, 0f, 5.5f), new Vector3(2f, 0.5f, 5f), env, _floorMat);
        MakePlatform("Platform_Plate", new Vector3(-6f, 0f, 11f), new Vector3(5f, 0.5f, 5f), env, _floorMat);
        MakePlatform("Platform_Mid", new Vector3(0f, 0f, 13.5f), new Vector3(6f, 0.5f, 4f), env, _floorMat);
        MakePlatform("Platform_End", new Vector3(0f, 0f, 21f), new Vector3(10f, 0.5f, 8f), env, _floorMat);

        PressurePlate plate = CreatePlate("PressurePlate_B", new Vector3(-6f, 0.36f, 11f), mech);
        CreateBridge("Bridge_B", new Vector3(0f, 0f, 9.5f), new Vector3(0f, -4f, 0f), Vector3.zero, new Vector3(3f, 0.5f, 11f), plate, mech);
        CreateDoor("MemoryGate_B", new Vector3(0f, 1.75f, 16.5f), new Vector3(3f, 3.5f, 0.5f), mech, new[] { plate });
        CreateLevelExit(new Vector3(0f, 1.25f, 24f), mech, "Level_07");
        SpawnPointLight("GoalLight", new Vector3(0f, 3.2f, 21f), new Color(1f, 0.83f, 0.42f, 1f), 4f, 10f, env);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 6f);
        SpawnGameplayCamera(player.transform);
        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnLevelRuntime(mech, "Usa un eco para sostener el puente.", "Aprender de tus acciones te permite avanzar.", "Luego pruebas.");

        CreateTutorialTrigger("Hint_Bridge", new Vector3(-5.8f, 1.2f, 10.8f), new Vector3(4f, 3f, 4f), "La placa eleva el puente", "Deja tu eco aqui y vuelve al centro.", 3.5f, tutorial);

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

        SetupAtmosphere(new Color(0.11f, 0.17f, 0.22f, 1f), 0.026f, new Color(0.12f, 0.16f, 0.2f, 1f));
        SpawnDirectionalLight();
        MakeBackdrop("Backdrop", new Vector3(0f, 0f, 10f), 30f, 30f, 10f, env);

        MakePlatform("Platform_Start", new Vector3(0f, 0f, 0f), new Vector3(8f, 0.5f, 8f), env, _floorMat);
        MakePlatform("Walk_Left", new Vector3(-4f, 0f, 4.5f), new Vector3(2f, 0.5f, 5f), env, _floorMat);
        MakePlatform("Platform_Left", new Vector3(-8f, 0f, 9f), new Vector3(5f, 0.5f, 5f), env, _floorMat);
        MakePlatform("Walk_Right", new Vector3(4f, 0f, 4.5f), new Vector3(2f, 0.5f, 5f), env, _floorMat);
        MakePlatform("Platform_Right", new Vector3(8f, 0f, 9f), new Vector3(5f, 0.5f, 5f), env, _floorMat);
        MakePlatform("Walk_Forward", new Vector3(0f, 0f, 9f), new Vector3(3f, 0.5f, 6f), env, _floorMat);
        MakePlatform("Platform_Exit", new Vector3(0f, 0f, 18f), new Vector3(10f, 0.5f, 8f), env, _floorMat);
        MakePlatform("Platform_Final", new Vector3(0f, 0f, 25f), new Vector3(8f, 0.5f, 6f), env, _floorMat);

        PressurePlate leftPlate = CreatePlate("PressurePlate_Left", new Vector3(-8f, 0.36f, 9f), mech);
        PressurePlate rightPlate = CreatePlate("PressurePlate_Right", new Vector3(8f, 0.36f, 9f), mech);
        CreateDoor("MemoryGate_C", new Vector3(0f, 1.75f, 13f), new Vector3(4f, 3.5f, 0.5f), mech, new[] { leftPlate, rightPlate });
        CreateBridge("Bridge_C", new Vector3(0f, 0f, 21f), new Vector3(0f, -3.5f, 0f), Vector3.zero, new Vector3(3f, 0.5f, 6f), leftPlate, mech);
        CreateLevelExit(new Vector3(0f, 1.25f, 27.5f), mech, "Level_07");

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 6f);
        SpawnGameplayCamera(player.transform);
        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnLevelRuntime(mech, "Sostén ambas placas con tus ecos.", "Tus decisiones pueden sostenerse entre si.", "Dos decisiones se sostienen.");

        CreateTutorialTrigger("Hint_FirstEcho", new Vector3(-6f, 1.2f, 6f), new Vector3(4f, 3f, 4f), "Primero deja un eco en una placa", "Luego repite en la opuesta.", 3.2f, tutorial);

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

        SetupAtmosphere(new Color(0.11f, 0.17f, 0.22f, 1f), 0.028f, new Color(0.12f, 0.16f, 0.2f, 1f));
        SpawnDirectionalLight();
        MakeBackdrop("Backdrop", new Vector3(0f, 0f, 12f), 30f, 38f, 10f, env);

        MakePlatform("Platform_Start", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.5f, 8f), env, _floorMat);
        MakePlatform("Walk_Left", new Vector3(-4f, 0f, 5f), new Vector3(2f, 0.5f, 4f), env, _floorMat);
        MakePlatform("Platform_A", new Vector3(-8f, 0f, 10f), new Vector3(6f, 0.5f, 6f), env, _floorMat);
        MakePlatform("Walk_A_Forward", new Vector3(-6f, 0f, 15f), new Vector3(2f, 0.5f, 4f), env, _floorMat);
        MakePlatform("Platform_B", new Vector3(0f, 0f, 22f), new Vector3(10f, 0.5f, 8f), env, _floorMat);
        MakePlatform("Walk_Right", new Vector3(4f, 0f, 5.5f), new Vector3(2f, 0.5f, 5f), env, _floorMat);
        MakePlatform("Platform_Exit", new Vector3(8f, 0f, 12f), new Vector3(6f, 0.5f, 8f), env, _floorMat);
        MakePlatform("Link_Final", new Vector3(4f, 0f, 16f), new Vector3(2f, 0.5f, 4f), env, _floorMat);

        PressurePlate plateA = CreatePlate("PressurePlate_A", new Vector3(-8f, 0.36f, 10f), mech);
        PressurePlate plateB = CreatePlate("PressurePlate_B", new Vector3(0f, 0.36f, 22f), mech);
        CreateDoor("MemoryGate_A", new Vector3(-4f, 1.75f, 17f), new Vector3(4f, 3.5f, 0.5f), mech, new[] { plateA });
        CreateDoor("MemoryGate_B", new Vector3(5.5f, 1.75f, 8f), new Vector3(0.5f, 3.5f, 4f), mech, new[] { plateB });
        CreateBridge("Bridge_D", new Vector3(4f, 0f, 16f), new Vector3(0f, -3.5f, 0f), Vector3.zero, new Vector3(2f, 0.5f, 4f), plateA, mech);
        CreateLevelExit(new Vector3(8f, 1.25f, 15f), mech, "Level_07");

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 6f);
        SpawnGameplayCamera(player.transform);
        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnLevelRuntime(mech, "Graba la ruta larga y usa la corta.", "Tus decisiones no valen solo por lo que hacen, sino por cuando llegan.", "El orden cambia el camino.");

        CreateTutorialTrigger("Hint_Order", new Vector3(1f, 1.2f, 2f), new Vector3(5f, 3f, 4f), "La salida corta depende de otra accion lejana", "No sigas al eco: aprovechalo.", 3.6f, tutorial);

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

        SetupAtmosphere(new Color(0.11f, 0.17f, 0.22f, 1f), 0.03f, new Color(0.12f, 0.16f, 0.2f, 1f));
        SpawnDirectionalLight();
        MakeBackdrop("Backdrop", new Vector3(0f, 0f, 14f), 30f, 42f, 10f, env);

        MakePlatform("Platform_Start", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.5f, 8f), env, _floorMat);
        MakePlatform("Walk_Left", new Vector3(-4f, 0f, 5f), new Vector3(2f, 0.5f, 4f), env, _floorMat);
        MakePlatform("Platform_A", new Vector3(-8f, 0f, 10f), new Vector3(6f, 0.5f, 6f), env, _floorMat);
        MakePlatform("Platform_B", new Vector3(0f, 0f, 18f), new Vector3(8f, 0.5f, 8f), env, _floorMat);
        MakePlatform("Walk_Exit", new Vector3(4f, 0f, 22f), new Vector3(2f, 0.5f, 4f), env, _floorMat);
        MakePlatform("Platform_Exit", new Vector3(8f, 0f, 26f), new Vector3(8f, 0.5f, 8f), env, _floorMat);
        MakePlatform("Platform_Top", new Vector3(0f, 0f, 27f), new Vector3(4f, 0.5f, 4f), env, _floorMat);

        PressurePlate plateA = CreatePlate("PressurePlate_A", new Vector3(-8f, 0.36f, 10f), mech);
        PressurePlate plateB = CreatePlate("PressurePlate_B", new Vector3(0f, 0.36f, 18f), mech);
        CreateBridge("Bridge_C", new Vector3(0f, 0f, 9f), new Vector3(0f, -4f, 0f), Vector3.zero, new Vector3(3f, 0.5f, 12f), plateA, mech);
        CreateDoor("MemoryGate_D", new Vector3(5.5f, 1.75f, 22f), new Vector3(0.5f, 3.5f, 4f), mech, new[] { plateB });
        CreateBridge("Bridge_E", new Vector3(4f, 0f, 26f), new Vector3(0f, -3.5f, 0f), new Vector3(0f, 0f, 0f), new Vector3(4f, 0.5f, 2f), plateB, mech);
        CreateLevelExit(new Vector3(0f, 1.25f, 29f), mech, "Level_07");

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 6f);
        SpawnGameplayCamera(player.transform);
        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnLevelRuntime(mech, "Usa un eco para grabar el siguiente.", "Aprender de tus acciones tambien es encadenarlas.", "La precision revela el patron.");

        CreateTutorialTrigger("Hint_Chain", new Vector3(-4f, 1.2f, 7f), new Vector3(5f, 3f, 4f), "Primero sostiene el puente", "Despues deja el segundo eco en la placa central.", 3.8f, tutorial);

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

        SetupAtmosphere(new Color(0.12f, 0.18f, 0.24f, 1f), 0.032f, new Color(0.13f, 0.18f, 0.22f, 1f));
        SpawnDirectionalLight();
        MakeBackdrop("Backdrop", Vector3.zero, 34f, 40f, 12f, env);

        MakePlatform("Platform_Start", new Vector3(0f, 0f, -14f), new Vector3(10f, 0.5f, 8f), env, _floorMat);
        MakePlatform("Walk_Left", new Vector3(-4f, 0f, -10f), new Vector3(2f, 0.5f, 4f), env, _floorMat);
        MakePlatform("Platform_A", new Vector3(-8f, 0f, -6f), new Vector3(6f, 0.5f, 6f), env, _floorMat);
        MakePlatform("Walk_Upper", new Vector3(-4f, 0f, 2f), new Vector3(2f, 0.5f, 4f), env, _floorMat);
        MakePlatform("Platform_B", new Vector3(-4f, 0f, 12f), new Vector3(6f, 0.5f, 6f), env, _floorMat);
        MakePlatform("Platform_Right", new Vector3(8f, 0f, -2f), new Vector3(6f, 0.5f, 6f), env, _floorMat);
        MakePlatform("Platform_Core", new Vector3(0f, 0f, 0f), new Vector3(8f, 0.5f, 8f), env, _goalMat);
        MakePlatform("Core_Approach", new Vector3(4f, 0f, 0f), new Vector3(2f, 0.5f, 4f), env, _floorMat);

        PressurePlate plateA = CreatePlate("PressurePlate_A", new Vector3(-8f, 0.36f, -6f), mech);
        PressurePlate plateB = CreatePlate("PressurePlate_B", new Vector3(-4f, 0.36f, 12f), mech);
        PressurePlate plateC = CreatePlate("PressurePlate_C", new Vector3(8f, 0.36f, -2f), mech);
        CreateBridge("Bridge_Upper", new Vector3(-4f, 0f, 4f), new Vector3(0f, -4f, 0f), Vector3.zero, new Vector3(3f, 0.5f, 12f), plateA, mech);
        CreateBridge("Bridge_Core", new Vector3(0f, 0f, -7f), new Vector3(0f, -4f, 0f), Vector3.zero, new Vector3(3f, 0.5f, 10f), plateB, mech);
        CreateDoor("MemoryGate_Final", new Vector3(4f, 1.75f, 0f), new Vector3(0.5f, 3.5f, 4f), mech, new[] { plateC });
        CreateLevelExit(new Vector3(0f, 1.25f, 0f), mech, "MainMenu", "Tu identidad vuelve al centro.");
        SpawnPointLight("CoreLight", new Vector3(0f, 4.5f, 0f), new Color(0.94f, 0.98f, 1f, 1f), 6f, 14f, env);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, -14f), true, 2, 6f);
        SpawnGameplayCamera(player.transform);
        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnLevelRuntime(mech, "Abre el camino final al nucleo.", "Tus decisiones construyen quien eres.", "Eres la suma de lo que elegiste conservar.");

        CreateTutorialTrigger("Hint_Final", new Vector3(-3f, 1.2f, -11f), new Vector3(5f, 3f, 4f), "La meta esta visible desde el inicio", "Solo necesitas sostener el espacio correcto.", 3.8f, tutorial);

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
        cameraObject.transform.position = player.position + new Vector3(0f, 4.5f, -7f);
        cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CameraShake>();
        cameraObject.AddComponent<AudioSource>();
        cameraObject.AddComponent<GameFeelController>();

        ThirdPersonCamera cameraRig = cameraObject.AddComponent<ThirdPersonCamera>();
        cameraRig.target = player.Find("CameraFocus");
        cameraRig.distance = 6.4f;
        cameraRig.focusOffset = new Vector3(0f, 0f, 0f);
        cameraRig.baseFov = 56f;
        cameraRig.mouseSensitivity = 1.15f;
        cameraRig.minPitch = 12f;
        cameraRig.maxPitch = 26f;
        cameraRig.clampYaw = true;
        cameraRig.maxYawOffset = 24f;
        cameraRig.movementLead = 0.7f;
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

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(2f, 0.12f, 2f);
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        visual.GetComponent<MeshRenderer>().sharedMaterial = _plateMat;

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

        TimedMovingPlatform platform = bridge.AddComponent<TimedMovingPlatform>();
        platform.plate = plate;
        platform.inactiveLocal = inactiveLocal;
        platform.activeLocal = activeLocal;
        platform.travelSpeed = 6f;
        return platform;
    }

    static LevelExit CreateLevelExit(Vector3 position, Transform parent, string nextSceneName, string completionToast = "")
    {
        GameObject exit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exit.name = "LevelExit";
        exit.transform.SetParent(parent, false);
        exit.transform.position = position;
        exit.transform.localScale = new Vector3(2.5f, 2.5f, 0.8f);
        exit.GetComponent<MeshRenderer>().sharedMaterial = _goalMat;

        LevelExit exitComponent = exit.AddComponent<LevelExit>();
        exitComponent.loadNextBuildIndex = false;
        exitComponent.nextSceneName = nextSceneName;
        if (!string.IsNullOrEmpty(completionToast))
            SetSerializedValue(exitComponent, "completionToast", completionToast);

        return exitComponent;
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

    static void MakePlatform(string name, Vector3 position, Vector3 scale, Transform parent, Material material)
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
    }

    static void MakeBackdrop(string prefix, Vector3 center, float width, float depth, float height, Transform parent)
    {
        MakePlatform(prefix + "_Back", center + new Vector3(0f, height * 0.5f, depth * 0.5f), new Vector3(width, height, 0.5f), parent, _bridgeMat);
        MakePlatform(prefix + "_Left", center + new Vector3(-width * 0.5f, height * 0.5f, 0f), new Vector3(0.5f, height, depth), parent, _bridgeMat);
        MakePlatform(prefix + "_Right", center + new Vector3(width * 0.5f, height * 0.5f, 0f), new Vector3(0.5f, height, depth), parent, _bridgeMat);
        MakePlatform(prefix + "_Ceiling", center + new Vector3(0f, height, depth * 0.1f), new Vector3(width, 0.5f, depth * 0.8f), parent, _bridgeMat);
        SpawnSmokeVolume(prefix + "_SmokeLow", center + new Vector3(0f, 0.8f, depth * 0.1f), new Vector3(width - 2f, 4f, depth - 4f), parent, 115f);
        SpawnLiminalDressing(prefix, center, width, depth, parent);
    }

    static void SetupAtmosphere(Color fogColor, float fogDensity, Color ambientColor)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        RenderSettings.skybox = null;
    }

    static void SpawnDirectionalLight()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light lightRef = lightObject.AddComponent<Light>();
        lightRef.type = LightType.Directional;
        lightRef.color = new Color(0.74f, 0.78f, 0.72f, 1f);
        lightRef.intensity = 0.72f;
        lightRef.shadows = LightShadows.Soft;
        lightObject.transform.rotation = Quaternion.Euler(22f, -32f, 0f);
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

        var main = particleSystem.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 12f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.15f);
        main.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.8f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.72f, 0.74f, 0.7f, 0.14f));
        main.maxParticles = Mathf.RoundToInt(rateOverTime * 1.6f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particleSystem.emission;
        emission.rateOverTime = rateOverTime;

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = boxScale;

        var velocity = particleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);

        var noise = particleSystem.noise;
        noise.enabled = true;
        noise.strength = 0.16f;
        noise.frequency = 0.24f;

        var color = particleSystem.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(0.68f, 0.7f, 0.66f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.35f, 0.15f),
                new GradientAlphaKey(0.3f, 0.7f),
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
        _floorMat = GetOrCreateMaterial("Mat_Floor", new Color(0.18f, 0.22f, 0.28f, 1f));
        _plateMat = GetOrCreateMaterial("Mat_Plate", new Color(0.18f, 0.72f, 0.58f, 1f), true);
        _bridgeMat = GetOrCreateMaterial("Mat_Bridge", new Color(0.10f, 0.14f, 0.18f, 1f));
        _doorMat = GetOrCreateMaterial("Mat_Door", new Color(0.42f, 0.22f, 0.3f, 1f), true);
        _goalMat = GetOrCreateMaterial("Mat_Exit", new Color(1f, 0.83f, 0.42f, 1f), true);
        _playerMat = GetOrCreateMaterial("Mat_Player", new Color(0.95f, 0.98f, 1f, 1f), true);
        _echoMat = GetOrCreateTransparentMaterial("Mat_Echo", new Color(0.38f, 0.96f, 1f, 0.28f), true);
    }

    static Material GetOrCreateMaterial(string name, Color color, bool emissive = false)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
            return material;

        material = new Material(Shader.Find("Standard"));
        material.color = color;
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
            return material;

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
        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/3D Models/Animaciones/Locomotion/idle.fbx");
        AnimationClip walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/3D Models/Animaciones/Locomotion/walking.fbx");
        AnimationClip runClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/3D Models/Animaciones/Locomotion/running.fbx");
        AnimationClip jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/3D Models/Animaciones/Locomotion/jump.fbx");
        AnimationClip leftStrafeClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/3D Models/Animaciones/Locomotion/left strafe walking.fbx");
        AnimationClip rightStrafeClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/3D Models/Animaciones/Locomotion/right strafe walking.fbx");

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
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = useEchoMaterial ? "EchoCapsule" : "PlayerCapsule";
        capsule.transform.SetParent(parent, false);
        capsule.transform.localPosition = new Vector3(0f, 1.05f, 0f);
        capsule.transform.localRotation = Quaternion.identity;
        capsule.transform.localScale = new Vector3(0.8f, 1.05f, 0.8f);
        Object.DestroyImmediate(capsule.GetComponent<Collider>());
        capsule.GetComponent<MeshRenderer>().sharedMaterial = useEchoMaterial ? _echoMat : _playerMat;
        return capsule;
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
}
