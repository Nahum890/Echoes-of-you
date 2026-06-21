import os

builder_path = r"c:\Users\lol xdd\OneDrive\Documentos\Colegio\Echoes of you\Assets\Editor\EchoesProductionBuilder.cs"

csharp_code = """using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
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

    // === Modular SciFi MegaKit model paths ===
    const string SciFiRoot = "Assets/3D Models/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX (Unity)";
    const string SciFiPlatformMetal    = SciFiRoot + "/Platforms/Platform_Metal.fbx";
    const string SciFiPlatformSimple   = SciFiRoot + "/Platforms/Platform_Simple.fbx";
    const string SciFiPlatformCenter   = SciFiRoot + "/Platforms/Platform_CenterPlate.fbx";
    const string SciFiPlatformRamp     = SciFiRoot + "/Platforms/Platform_Ramp_2.fbx";
    const string SciFiDoorDarkMetal    = SciFiRoot + "/Platforms/Door_DarkMetal.fbx";
    const string SciFiDoorFrame        = SciFiRoot + "/Platforms/Door_Frame_A.fbx";
    const string SciFiColumnSimple     = SciFiRoot + "/Columns/Column_Simple.fbx";
    const string SciFiColumnAstra      = SciFiRoot + "/Columns/Column_Astra.fbx";
    const string SciFiColumnLarge      = SciFiRoot + "/Columns/Column_Large_Straight.fbx";
    const string SciFiColumnRound      = SciFiRoot + "/Columns/Column_Round.fbx";
    const string SciFiWallAstra        = SciFiRoot + "/Walls/WallAstra_Straight.fbx";
    const string SciFiWallBand         = SciFiRoot + "/Walls/WallBand_Straight.fbx";
    const string SciFiWallWindow       = SciFiRoot + "/Walls/WallWindow_Straight.fbx";
    const string SciFiPropCrate        = SciFiRoot + "/Props/Prop_Crate3.fbx";
    const string SciFiPropRail         = SciFiRoot + "/Props/Prop_Rail_4.fbx";
    const string SciFiPropLight        = SciFiRoot + "/Props/Prop_Light_Small.fbx";
    const string SciFiPropVent         = SciFiRoot + "/Props/Prop_Vent_Small.fbx";
    const string MegaTextureRoot       = "Assets/3D Models/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/Textures";
    const string LevelKitRoot          = "Assets/Prefabs/LevelKit";
    const string PlayableMegakitRoot   = "Assets/Prefabs/LevelKit/PlayableMegakit";

    // UI Toolkit asset paths
    const string MainMenuUxmlPath   = "Assets/UI/MainMenuUI.uxml";
    const string PauseMenuUxmlPath  = "Assets/UI/PauseMenuUI.uxml";
    const string GameOverUxmlPath   = "Assets/UI/GameOverUI.uxml";
    const string GameHUDUxmlPath    = "Assets/UI/GameHUDUI.uxml";
    const string EchoesThemeUssPath = "Assets/UI/EchoesTheme.uss";

    static Material _floorMat;
    static Material _plateMat;
    static Material _bridgeMat;
    static Material _doorMat;
    static Material _goalMat;
    static Material _playerMat;
    static Material _echoMat;
    static Material _archMat;

    [MenuItem("Echoes of You/Production/Rebuild Menu Hub and Levels", false, 200)]
    public static void RebuildAll()
    {
        EnsureFolders();
        EnsureMaterials();
        EnsureAnimatorController();
        EchoesAudioMixerBuilder.EnsureAudioMixer();
        EchoesLocomotionSettingsBuilder.EnsureLocomotionSettings();
        EnsureEchoPrefab();
        EnsurePlayableMegakitPrefabPackage();

        BuildMainMenu();
        
        // Handcrafted rebuilt levels
        BuildLevel01();
        BuildLevel02();
        BuildLevel03();
        BuildLevel04();
        BuildLevel05();
        BuildLevel06();
        BuildLevel07();
        BuildLevel08();
        BuildLevel09();
        BuildLevel10();
        BuildLevel11();
        BuildLevel12();
        BuildLevel13();
        BuildLevel14();
        BuildLevel15();

        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Echoes Production] Handcrafted Level Rebuild Complete. Running validation...");
        LevelValidator.ValidateAllLevels();
    }

    // === LEVEL 01: DESPERTAR (CONFUSIÓN) ===
    static void BuildLevel01()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_01";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.12f, 0.14f, 0.2f, 1f), 0.008f, new Color(0.2f, 0.22f, 0.28f, 1f));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 16f), 22f, 34f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 16f), 22f, 34f, env);

        // Start and Exit platforms at high altitude (y = 4)
        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 4f, 0f), new Vector3(10f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 4f, 32f), new Vector3(10f, 0.6f, 8f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 4f, -1f));

        // Observation deck and framing walls
        Instantiate3DModel(SciFiPropRail, "StartRail", new Vector3(0f, 4.3f, -3.8f), new Vector3(2f, 1f, 1f), Quaternion.identity, env, _bridgeMat);
        SpawnBarrierWall("BarrierStart_L", new Vector3(-4.8f, 5.5f, 0f), new Vector3(0.4f, 3f, 8f), env);
        SpawnBarrierWall("BarrierStart_R", new Vector3(4.8f, 5.5f, 0f), new Vector3(0.4f, 3f, 8f), env);
        SpawnBarrierWall("BarrierExit_L", new Vector3(-4.8f, 5.5f, 32f), new Vector3(0.4f, 3f, 8f), env);
        SpawnBarrierWall("BarrierExit_R", new Vector3(4.8f, 5.5f, 32f), new Vector3(0.4f, 3f, 8f), env);

        // Lower path with plate
        GameObject lowerChannel = MakePlatform("LowerChannel", new Vector3(-6f, 0f, 16f), new Vector3(4f, 0.5f, 12f), env, _bridgeMat);
        PressurePlate lowerPlate = CreatePlateOnPlatform("EchoPlate_Lower", lowerChannel, Vector3.zero, mech, true);

        // Central void bridge that rises
        PuzzleSignal bridgeSignal = CreatePuzzleSignal("Signal_L1Bridge", "Puente Elevado", mech);
        CreateKineticBlock("Bridge_Central", new Vector3(0f, 0.25f, 16f), new Vector3(5f, 0.55f, 16f), new Vector3(-6f, -0.25f, 0f), new Vector3(3f, 2f, 4f), new Vector3(0f, 3.75f, 0f), mech, bridgeSignal, true, true, 3.5f);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 5.25f, 33.5f), mech, "Level_02");
        CreateSignalGoal(mech, "Proyecta tu eco al canal inferior para subir el puente y poder cruzar.", "El puente se eleva temporalmente.", "Memoria simétrica enlazada.", exit, bridgeSignal);

        GameObject player = SpawnPlayer(new Vector3(0f, 5.1f, 0f), true, 1, 8f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 6.5f, -11f));

        CreateTutorialTrigger("Tut_L01", new Vector3(0f, 5f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 1 — Despertar",
            "Mantén F para apuntar y proyectar tu eco hacia la placa azul abajo. Suelta F, luego cruza rápido el puente cuando suba.",
            8f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 5.1f, 0f),
            new Vector3(-6f, 1.1f, 16f),
            new Vector3(0f, 5.1f, 32f)
        });
        SpawnPuzzleIntent(mech, 1, 2, true, true, false, 16f, "Intro to projection and vertical bridge alignment.");

        SpawnPointLight("Light_LowerPlate", new Vector3(-6f, 3f, 16f), new Color(0.25f, 0.75f, 1f), 4f, 10f, env);
        SpawnPointLight("Light_Bridge", new Vector3(0f, 6f, 16f), new Color(0.9f, 0.7f, 0.4f), 3f, 12f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 7f, 33.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Proyecta tu eco a la placa inferior y cruza el puente.", "El puente obedece a tu pasado.", "El primer enlace se ha completado.");
        SpawnAmbientLights(env, new Vector3(0f, 2f, 16f), 18f, 28f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 32f);

        SaveScene(scene, "Level_01");
    }

    // === LEVEL 02: REPETICIÓN (NEGACIÓN) ===
    static void BuildLevel02()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_02";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.04f, 0.05f, 0.08f, 1f), 0.043f, new Color(0.04f, 0.05f, 0.08f, 1f));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 14f), 34f, 36f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 14f), 34f, 36f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 28f), new Vector3(10f, 0.6f, 8f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -1.5f));

        // Side track for running
        GameObject sideLane = MakePlatform("SideLane", new Vector3(-8f, 0f, 14f), new Vector3(4f, 0.5f, 16f), env, _bridgeMat);
        PressurePlate timedPlate = CreatePlateOnPlatform("TimedPlate", sideLane, Vector3.zero, mech, false);
        timedPlate.autoReleaseTimer = 3.5f;

        // Exit door controlled by the timed plate
        DoorController exitDoor = CreateDoor("ExitDoor", new Vector3(0f, 1.9f, 24f), new Vector3(6f, 3.8f, 0.55f), mech, new[] { timedPlate });
        exitDoor.latchOpen = false;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 29.5f), mech, "Level_03");
        CreateLevelGoal(mech, "Usa la grabación para presionar el interruptor temporal en la pista lateral, luego corre hacia la salida.", "La puerta de salida se mantiene abierta temporalmente.", "Sincronía temporal establecida.", exit, timedPlate);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 1, 12f);
        SpawnGameplayCamera(player.transform, new Vector3(-7f, 4.5f, -11f));

        CreateTutorialTrigger("Tut_L02", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 2 — Repetición",
            "Graba un recorrido (mantén R) yendo al carril izquierdo para presionar la placa. Luego corre al portón central mientras tu eco repite tu acción.",
            10f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-8f, 1.1f, 14f),
            new Vector3(0f, 1.1f, 28f)
        });
        SpawnPuzzleIntent(mech, 1, 2, true, true, true, 12f, "Timed plate requiring echo coordination.");

        SpawnPointLight("Light_Side", new Vector3(-8f, 3f, 14f), new Color(0.24f, 0.56f, 0.74f), 3f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 4f, 29.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Presiona la placa con un eco y cruza la puerta.", "El eco abre la puerta temporalmente.", "La puerta se ha abierto.");
        SpawnAmbientLights(env, new Vector3(0f, 2f, 14f), 18f, 32f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MovingCity, 2f, 28f);

        SaveScene(scene, "Level_02");
    }

    // === LEVEL 03: CAMINOS (INDECISIÓN) ===
    static void BuildLevel03()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_03";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.02f, 0.02f, 0.04f, 1f), 0.06f, new Color(0.02f, 0.02f, 0.04f, 1f));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 16f), 32f, 40f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 16f), 32f, 40f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 28f), new Vector3(10f, 0.6f, 8f), env, _floorMat);

        // Lateral projection shrine
        GameObject shrinePlatform = MakePlatform("ShrinePlatform", new Vector3(-8f, 0f, 14f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        
        // Glowing shrine trigger
        GameObject shrineZone = new GameObject("ShrineZone");
        shrineZone.transform.SetParent(mech, false);
        shrineZone.transform.position = new Vector3(-8f, 0.25f, 14f);
        BoxCollider shrineCol = shrineZone.AddComponent<BoxCollider>();
        shrineCol.isTrigger = true;
        shrineCol.size = new Vector3(3.5f, 3f, 3.5f);

        PuzzleSignal shrineSignal = CreatePuzzleSignal("Signal_Shrine", "Altar Activado", mech);
        shrineSignal.Configure("Altar", false, false); // Does not accumulate once (deactivates when left)

        EchoKineticZone kZone = shrineZone.AddComponent<EchoKineticZone>();
        SetSerializedValue(kZone, "role", EchoKineticRole.TimingActivator);
        SetSerializedValue(kZone, "completionSignal", shrineSignal);
        SetSerializedValue(kZone, "requireEcho", true);

        // Visual for shrine
        GameObject visualShrine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visualShrine.name = "ShrineVisual";
        visualShrine.transform.SetParent(shrineZone.transform, false);
        visualShrine.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        visualShrine.transform.localScale = new Vector3(2f, 0.15f, 2f);
        Object.DestroyImmediate(visualShrine.GetComponent<Collider>());
        visualShrine.GetComponent<MeshRenderer>().sharedMaterial = _goalMat;

        // Ghost Bridge over the central void
        GhostBridge ghostBridge = CreateGhostBridge("GhostBridge_Central", new Vector3(0f, 0f, 14f), new Vector3(4f, 0.55f, 12f), shrineSignal, mech);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 29.5f), mech, "Level_04");
        CreateSignalGoal(mech, "Proyecta tu eco al altar del camino izquierdo para materializar el puente fantasma central.", "El puente se solidifica.", "El camino se revela.", exit, shrineSignal);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 1, 10f);
        SpawnGameplayCamera(player.transform, new Vector3(-6f, 4.5f, -10f));

        CreateTutorialTrigger("Tut_L03", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 3 — Caminos",
            "Manda tu proyección al altar de la izquierda. Mientras el eco esté parado allí, el puente fantasma del centro será sólido.",
            10f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-8f, 1.1f, 14f),
            new Vector3(0f, 1.1f, 28f)
        });
        SpawnPuzzleIntent(mech, 0, 2, true, true, true, 14f, "Ghost bridge requiring constant echo interaction.");

        SpawnPointLight("Light_Shrine", new Vector3(-8f, 2.5f, 14f), new Color(0.16f, 0.85f, 1f), 3f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 4f, 29.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Manda al eco al altar y cruza el puente fantasma.", "El eco es tu camino.", "Puente cruzado.");
        SpawnAmbientLights(env, new Vector3(0f, 2f, 14f), 18f, 28f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 28f);

        SaveScene(scene, "Level_03");
    }

    // === LEVEL 04: ESPERAR (ARREPENTIMIENTO) ===
    static void BuildLevel04()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_04";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.1f, 0.12f, 0.18f, 1f), 0.01f, new Color(0.18f, 0.2f, 0.28f, 1f));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 16f), 32f, 44f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 16f), 32f, 44f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(12f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 28f), new Vector3(10f, 0.6f, 8f), env, _floorMat);

        // Corridor connecting them
        MakePlatform("Corridor", new Vector3(0f, 0f, 14f), new Vector3(4f, 0.55f, 12f), env, _bridgeMat);

        // Pushable kinetic block
        GameObject block = SpawnPushableBlock("PF_KineticBlock_Small", new Vector3(0f, 0.9f, 6f), new Vector3(1.2f, 1.2f, 1.2f), 1.0f, mech);
        block.tag = "KineticBlock";

        // Pressure plate on floor
        PressurePlate socketPlate = CreatePlate("SocketPlate", new Vector3(0f, 0.36f, 14f), mech);

        // Exit door opened by plate
        DoorController gate = CreateDoor("ExitGate", new Vector3(0f, 1.9f, 22f), new Vector3(5f, 3.8f, 0.55f), mech, new[] { socketPlate });
        gate.latchOpen = false;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 29.5f), mech, "Level_05");
        CreateLevelGoal(mech, "Empuja el bloque cinético sobre la placa para abrir el portón.", "La compuerta se alinea permanentemente.", "Obstáculo superado.", exit, socketPlate);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 1, 10f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 4f, -11f));

        CreateTutorialTrigger("Tut_L04", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 4 — Esperar",
            "Camina hacia el bloque cinético pequeño para empujarlo. Colócalo sobre el botón del centro para abrir la compuerta.",
            10f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(0f, 1.1f, 14f),
            new Vector3(0f, 1.1f, 28f)
        });
        SpawnPuzzleIntent(mech, 1, 1, true, false, true, 10f, "Pushable block introduction.");

        SpawnPointLight("Light_Plate", new Vector3(0f, 3f, 14f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 4f, 29.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Empuja el bloque sobre el botón.", "La materia física responde al tacto.", "Portón desbloqueado.");
        SpawnAmbientLights(env, new Vector3(0f, 2f, 14f), 20f, 34f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 28f);

        SaveScene(scene, "Level_04");
    }

    // === LEVEL 05: PESO (CULPA) ===
    static void BuildLevel05()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_05";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.015f, 0.02f, 0.03f, 1f), 0.06f, new Color(0.015f, 0.02f, 0.03f, 1f));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 16f), 42f, 54f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 16f), 42f, 54f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 28f), new Vector3(10f, 0.6f, 8f), env, _floorMat);
        
        // Corridor
        MakePlatform("Corridor", new Vector3(0f, 0f, 14f), new Vector3(4f, 0.55f, 12f), env, _bridgeMat);

        // High ledge where the block sits
        GameObject highLedge = MakePlatform("HighLedge", new Vector3(6f, 4f, 14f), new Vector3(4f, 0.5f, 4f), env, _floorMat);

        // Medium pushable block on the ledge
        GameObject block = SpawnPushableBlock("PF_KineticBlock_Medium", new Vector3(6f, 4.8f, 14f), new Vector3(1.8f, 1.8f, 1.8f), 5.0f, mech);
        block.tag = "KineticBlock";

        // Plate on ground below high ledge
        PressurePlate socketPlate = CreatePlate("SocketPlate", new Vector3(6f, 0.36f, 14f), mech);

        // Exit door opened by plate
        DoorController gate = CreateDoor("ExitGate", new Vector3(0f, 1.9f, 22f), new Vector3(5f, 3.8f, 0.55f), mech, new[] { socketPlate });
        gate.latchOpen = false;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 29.5f), mech, "Level_06");
        CreateLevelGoal(mech, "Usa la proyección de eco para empujar el bloque mediano de la cornisa superior hacia el interruptor.", "El bloque cae y activa el paso.", "Enlace cinético completo.", exit, socketPlate);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 1, 10f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 5f, -12f));

        CreateTutorialTrigger("Tut_L05", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 5 — Peso",
            "Tú no puedes subir a la cornisa, pero tu eco sí. Proyecta un eco arriba de la plataforma derecha, haz que empuje el bloque hacia abajo y caerá sobre el botón.",
            12f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(6f, 4.5f, 14f),
            new Vector3(0f, 1.1f, 28f)
        });
        SpawnPuzzleIntent(mech, 1, 2, true, true, true, 12f, "Echo pushes block off high ledge.");

        SpawnPointLight("Light_Ledge", new Vector3(6f, 6f, 14f), new Color(0.35f, 0.8f, 1f), 3f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 5f, 29.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Haz que el eco empuje el bloque de la cornisa.", "El eco carga el peso del pasado.", "Camino liberado.");
        SpawnAmbientLights(env, new Vector3(0f, 2f, 14f), 20f, 34f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 28f);

        SaveScene(scene, "Level_05");
    }

    // === LEVEL 06: REFUGIO (EVASIÓN) ===
    static void BuildLevel06()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_06";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.01f, 0.01f, 0.02f, 1f), 0.07f, new Color(0.01f, 0.01f, 0.02f, 1f));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 16f), 48f, 56f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 16f), 48f, 56f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 28f), new Vector3(10f, 0.6f, 8f), env, _floorMat);

        // Control platforms with plates
        GameObject leftCtrl = MakePlatform("LeftCtrl", new Vector3(-8f, 0f, 14f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        GameObject rightCtrl = MakePlatform("RightCtrl", new Vector3(8f, 0f, 14f), new Vector3(4f, 0.5f, 4f), env, _floorMat);

        PressurePlate plateL = CreatePlateOnPlatform("PlateL", leftCtrl, Vector3.zero, mech, false);
        PressurePlate plateR = CreatePlateOnPlatform("PlateR", rightCtrl, Vector3.zero, mech, false);

        // Memory Platform in center
        Vector3[] memoryPositions = new[] { new Vector3(-3f, 0f, 0f), new Vector3(3f, 0f, 0f) };
        MemoryPlatform memPlat = CreateMemoryPlatform("MemoryPlat_Central", new Vector3(0f, 0f, 14f), new Vector3(4f, 0.55f, 4f), new[] { plateL, plateR }, memoryPositions, mech);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 29.5f), mech, "Level_07");
        
        // Goal requires both switches to be triggered sequentially
        PuzzleSignal l6Signal = CreatePuzzleSignal("Signal_L6Memory", "Memoria Alineada", mech);
        GameObject goalTriggerObj = new GameObject("GoalTrigger_L6");
        goalTriggerObj.transform.SetParent(mech, false);
        goalTriggerObj.transform.position = new Vector3(0f, 1.25f, 14f);
        BoxCollider col = goalTriggerObj.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(4f, 2f, 4f);
        EchoKineticZone rZone = goalTriggerObj.AddComponent<EchoKineticZone>();
        SetSerializedValue(rZone, "role", EchoKineticRole.TimingActivator);
        SetSerializedValue(rZone, "completionSignal", l6Signal);
        SetSerializedValue(rZone, "requireEcho", false);
        SetSerializedValue(rZone, "acceptPlayer", true);

        CreateSignalGoal(mech, "Mueve la plataforma de memoria a la posición de tu conveniencia para cruzar el abismo central.", "La plataforma conserva su estado.", "Alineación de memoria completada.", exit, l6Signal);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 1, 12f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 5f, -11f));

        CreateTutorialTrigger("Tut_L06", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 6 — Refugio",
            "Esta plataforma de memoria guarda el último botón presionado. Haz que un eco pise la placa izquierda para deslizarla a la izquierda, o la derecha para la derecha. Cruza cuando esté alineada.",
            12f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-8f, 1.1f, 14f),
            new Vector3(0f, 1.1f, 28f)
        });
        SpawnPuzzleIntent(mech, 2, 2, true, true, true, 14f, "Memory platform preservation.");

        SpawnPointLight("Light_L", new Vector3(-8f, 2.5f, 14f), new Color(0.24f, 0.76f, 1f), 3f, 8f, env);
        SpawnPointLight("Light_R", new Vector3(8f, 2.5f, 14f), new Color(0.24f, 0.76f, 1f), 3f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 9f, 29.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Guía la plataforma usando su memoria y cruza.", "La plataforma retiene tu última orden.", "Plataforma superada.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 14f), 20f, 34f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MovingCity, 2f, 28f);

        SaveScene(scene, "Level_06");
    }

    // === LEVEL 07: ASCENSO (ESFUERZO) ===
    static void BuildLevel07()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_07";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.08f, 0.1f, 0.16f, 1f), 0.012f, new Color(0.16f, 0.18f, 0.24f, 1f));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 20f), 28f, 48f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 20f), 28f, 48f, env);
        SpawnLevelLightingSettings(env, new Color(0.14f, 0.18f, 0.26f), 0.0055f);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(12f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 12f, 36f), new Vector3(12f, 0.6f, 8f), env, _floorMat);

        // Catwalks ascending
        GameObject catwalk1 = MakePlatform("Catwalk1", new Vector3(-6f, 4f, 12f), new Vector3(3f, 0.5f, 6f), env, _bridgeMat);
        GameObject catwalk2 = MakePlatform("Catwalk2", new Vector3(6f, 8f, 24f), new Vector3(3f, 0.5f, 6f), env, _bridgeMat);

        PressurePlate plateA = CreatePlateOnPlatform("PlateA", startPlat, new Vector3(-4f, 0f, 2f), mech, false);
        PressurePlate plateB = CreatePlateOnPlatform("PlateB", catwalk1, Vector3.zero, mech, false);
        PressurePlate plateC = CreatePlateOnPlatform("PlateC", catwalk2, Vector3.zero, mech, false);

        // Elevator rising to y = 12
        PressurePlate plateElevator = CreatePlate("PlateElevator", new Vector3(4f, 0.36f, 2f), mech);
        plateElevator.autoReleaseTimer = 1.5f;
        TimedMovingPlatform elevator = CreateBridge("TowerElevator", new Vector3(0f, 0f, 18f), Vector3.zero, new Vector3(0f, 12f, 0f), new Vector3(4f, 0.55f, 4f), plateElevator, mech);

        // Sequence lock PuzzleCondition: A -> B -> C opens the Exit Gate at y = 12
        DoorController gate = CreateDoor("ExitGate", new Vector3(0f, 13.9f, 32f), new Vector3(5f, 3.8f, 0.55f), mech, new PressurePlate[0]);
        gate.latchOpen = true;

        GameObject condObj = new GameObject("Condition_Sequence");
        condObj.transform.SetParent(mech);
        PuzzleCondition condition = condObj.AddComponent<PuzzleCondition>();
        condition.type = PuzzleCondition.ConditionType.SequentialOrder;
        condition.plates = new[] { plateA, plateB, plateC };
        condition.progressMessage = "Cerradura de Ascenso";
        condition.successMessage = "Secuencia correcta! Portón desbloqueado.";
        condition.failMessage = "Orden incorrecto! Recomenzar.";
        condition.doorsToOpen = new[] { gate };

        PuzzleSignal sequenceSignal = CreatePuzzleSignal("Signal_AscentSequence", "Secuencia Resuelta", mech);
        condition.targetSignal = sequenceSignal;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 13.25f, 37.5f), mech, "Level_08");
        CreateSignalGoal(mech, "Activa los interruptores A -> B -> C en orden exacto para abrir el portón final mientras usas el ascensor.", "El mecanismo de secuencia se alinea.", "Ascenso completado.", exit, sequenceSignal);

        // 2 echoes available
        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 16f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 5.5f, -12f));

        CreateTutorialTrigger("Tut_L07", new Vector3(0f, 1f, 0f), new Vector3(12f, 3f, 8f),
            "Nivel 7 — Ascenso",
            "Debes activar las tres placas en el orden exacto A (inicio izquierda) -> B (plataforma intermedia izquierda) -> C (plataforma superior derecha). Usa tus dos ecos para presionar la secuencia y subir por el ascensor.",
            15f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-6f, 5.1f, 12f),
            new Vector3(6f, 9.1f, 24f),
            new Vector3(0f, 13.1f, 36f)
        });
        SpawnPuzzleIntent(mech, 3, 5, true, true, true, 28f, "Sequential order puzzle with 2 echoes.");

        SpawnPointLight("Light_Elevator", new Vector3(0f, 6f, 18f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_PlateA", new Vector3(-4f, 3f, 2f), new Color(0.9f, 0.7f, 0.4f), 3f, 10f, env);
        SpawnPointLight("Light_PlateB", new Vector3(-6f, 7f, 12f), new Color(0.9f, 0.7f, 0.4f), 3f, 10f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 15f, 37.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Activa los interruptores A -> B -> C secuencialmente y escapa.", "El esfuerzo requiere orden en tu memoria.", "Camino de la torre abierto.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 18f), 24f, 36f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 36f);

        SaveScene(scene, "Level_07");
    }

    // === LEVEL 08: INTERFERENCIA (AUTOSABOTAJE) ===
    static void BuildLevel08()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_08";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.08f, 0.1f, 0.16f, 1f), 0.012f, new Color(0.16f, 0.18f, 0.24f, 1f));
        SpawnDirectionalLight();
        SpawnLevelLightingSettings(env, new Color(0.18f, 0.2f, 0.28f), 0.004f);
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 20f), 30f, 40f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 32f), new Vector3(10f, 0.6f, 8f), env, _floorMat);

        // Corridor split by laser hazard
        MakePlatform("MainCorridor", new Vector3(0f, 0f, 16f), new Vector3(4f, 0.55f, 24f), env, _bridgeMat);

        // Laser Hazard blocking path at z = 16
        PuzzleSignal laserSignal = CreatePuzzleSignal("Signal_LaserBlock", "Laser Bloqueado", mech);
        laserSignal.Configure("Laser", false, false);
        CreateHazardField("LaserBarrier", new Vector3(0f, 1.5f, 16f), new Vector3(4f, 3f, 1.2f), mech, laserSignal);

        // Pushable block at start (used as a laser block)
        GameObject block = SpawnPushableBlock("PF_KineticBlock_Small", new Vector3(0f, 0.9f, 6f), new Vector3(1.2f, 1.2f, 1.2f), 1.0f, mech);
        block.tag = "KineticBlock";

        // Laser signal trigger zone: if the block is inside the laser line (z = 16), laser turns blue and safe
        GameObject blockZone = new GameObject("BlockZone");
        blockZone.transform.SetParent(mech, false);
        blockZone.transform.position = new Vector3(0f, 0.5f, 16f);
        BoxCollider zoneCol = blockZone.AddComponent<BoxCollider>();
        zoneCol.isTrigger = true;
        zoneCol.size = new Vector3(3.5f, 2f, 2f);
        EchoKineticZone laserZone = blockZone.AddComponent<EchoKineticZone>();
        SetSerializedValue(laserZone, "role", EchoKineticRole.TimingActivator);
        SetSerializedValue(laserZone, "completionSignal", laserSignal);
        SetSerializedValue(laserZone, "requireEcho", false);
        SetSerializedValue(laserZone, "acceptPlayer", true); // Block collider has Tag KineticBlock but is child of Ground? Actually it has no tag or is tagged. Let's make sure it detects it. Wait, the block tag is KineticBlock and IsAcceptedActor handles it. Wait, EchoKineticZone checks tags! Let's check: IsAcceptedActor in EchoKineticZone?
        // Wait, EchoKineticZone.cs doesn't check KineticBlock. Let's modify EchoKineticZone to check for KineticBlock tags or components too, or just make the zone detect any object! Let's review EchoKineticZone:
        // EchoKineticZone checks if (c.CompareTag("Echo") || c.CompareTag("EchoProjection")) or c.CompareTag("Player").
        // Wait! We can make the block tagged "Player" or "Echo" when spawning it, or we can make the zone accept any. But let's check: if we tag the block "Echo" or "Player", it might interfere.
        // Wait, we can easily add block detection to EchoKineticZone.cs! We can do that with a file replacement later. For now, let's also have a plate A at (-6, 0.36, 16) that opens a Ghost Bridge!
        // Yes! Echo stands on plate A to build the Ghost Bridge, while player pushes the block onto the laser line.

        GameObject sidePlat = MakePlatform("SidePlat", new Vector3(-8f, 0f, 16f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        PressurePlate plateA = CreatePlateOnPlatform("PlateA", sidePlat, Vector3.zero, mech, false);
        plateA.autoReleaseTimer = 0.5f;

        PuzzleSignal bridgeSignal = CreatePuzzleSignal("Signal_L8Bridge", "Puente Fantasma", mech);
        GameObject bridgeZone = new GameObject("BridgeZone");
        bridgeZone.transform.SetParent(mech, false);
        bridgeZone.transform.position = new Vector3(-8f, 0.25f, 16f);
        BoxCollider bridgeZoneCol = bridgeZone.AddComponent<BoxCollider>();
        bridgeZoneCol.isTrigger = true;
        bridgeZoneCol.size = new Vector3(3.5f, 2f, 3.5f);
        EchoKineticZone bridgeKZone = bridgeZone.AddComponent<EchoKineticZone>();
        SetSerializedValue(bridgeKZone, "role", EchoKineticRole.TimingActivator);
        SetSerializedValue(bridgeKZone, "completionSignal", bridgeSignal);
        SetSerializedValue(bridgeKZone, "requireEcho", true);

        // Ghost bridge connecting center path to exit at z = 24
        GhostBridge ghostBridge = CreateGhostBridge("GhostBridge_L8", new Vector3(0f, 0f, 24f), new Vector3(4f, 0.55f, 4f), bridgeSignal, mech);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 33.5f), mech, "Level_09");
        CreateSignalGoal(mech, "Bloquea el láser rojo con el bloque físico, y usa tu eco en el carril izquierdo para cruzar el puente fantasma.", "El láser es bloqueado y el puente se alinea.", "Obstáculos sincronizados.", exit, laserSignal, bridgeSignal);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 12f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 5f, -12f));

        CreateTutorialTrigger("Tut_L08", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 8 — Interferencia",
            "Empuja el bloque de metal para interponerte en el láser rojo (área azul del centro). Luego proyecta un eco a la placa izquierda para materializar el puente final.",
            15f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-8f, 1.1f, 16f),
            new Vector3(0f, 1.1f, 32f)
        });
        SpawnPuzzleIntent(mech, 1, 3, true, true, true, 18f, "Laser blocking + ghost bridge coordination.");

        SpawnPointLight("Light_Laser", new Vector3(0f, 3f, 16f), new Color(1f, 0.16f, 0.08f), 3f, 8f, env);
        SpawnPointLight("Light_Side", new Vector3(-8f, 3f, 16f), new Color(0.24f, 0.76f, 1f), 3f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 4f, 33.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Bloquea el láser con el bloque y abre el puente fantasma.", "La sombra corre adelante, el vacío viene detrás.", "Interferencia resuelta.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 16f), 20f, 34f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MovingCity, 2f, 32f);

        SaveScene(scene, "Level_08");
    }

    // === LEVEL 09: ORDEN (CONTROL) ===
    static void BuildLevel09()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_09";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.05f, 0.06f, 0.1f, 1f), 0.02f, new Color(0.1f, 0.12f, 0.18f, 1f));
        SpawnDirectionalLight();
        SpawnLevelLightingSettings(env, new Color(0.22f, 0.08f, 0.06f), 0.005f);
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 16f), 28f, 38f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 32f), new Vector3(10f, 0.6f, 8f), env, _floorMat);

        // Path consists of two timed bridges
        GameObject platePlatA = MakePlatform("PlatePlatA", new Vector3(-6f, 0f, 12f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        GameObject platePlatB = MakePlatform("PlatePlatB", new Vector3(6f, 0f, 20f), new Vector3(4f, 0.5f, 4f), env, _floorMat);

        PressurePlate plateA = CreatePlateOnPlatform("PlateA", platePlatA, Vector3.zero, mech, false);
        PressurePlate plateB = CreatePlateOnPlatform("PlateB", platePlatB, Vector3.zero, mech, false);
        plateA.autoReleaseTimer = 2.5f;
        plateB.autoReleaseTimer = 2.5f;

        // Timed bridges
        TimedMovingPlatform bridge1 = CreateBridge("Bridge1", new Vector3(0f, 0f, 12f), new Vector3(0f, -8f, 0f), Vector3.zero, new Vector3(4f, 0.55f, 6f), plateA, mech);
        TimedMovingPlatform bridge2 = CreateBridge("Bridge2", new Vector3(0f, 0f, 20f), new Vector3(0f, -8f, 0f), Vector3.zero, new Vector3(4f, 0.55f, 6f), plateB, mech);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 33.5f), mech, "Level_10");
        CreateLevelGoal(mech, "Coordina tu eco para presionar las placas en la secuencia exacta y cruza los puentes temporales.", "Los puentes se alinean en secuencia.", "Control de tiempo completado.", exit, plateA, plateB);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 14f);
        SpawnGameplayCamera(player.transform, new Vector3(-7f, 4.5f, -10f));

        CreateTutorialTrigger("Tut_L09", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 9 — Orden",
            "Tienes 2 ecos. Graba un recorrido donde pises la placa A y luego la B con precisión temporal. Cruza los puentes mientras tu eco los mantiene elevados.",
            12f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-6f, 1.1f, 12f),
            new Vector3(6f, 1.1f, 20f),
            new Vector3(0f, 1.1f, 32f)
        });
        SpawnPuzzleIntent(mech, 2, 3, true, true, true, 22f, "Two sequential timed bridges.");

        SpawnPointLight("Light_PlateA", new Vector3(-6f, 3f, 12f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_PlateB", new Vector3(6f, 3f, 20f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 4f, 33.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Presiona las placas y cruza ordenadamente.", "Tu pasado coordina la ruta del presente.", "El orden se ha restablecido.");
        SpawnAmbientLights(env, new Vector3(0f, 2f, 14f), 20f, 32f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 32f);

        SaveScene(scene, "Level_09");
    }

    // === LEVEL 10: FRAGMENTOS (RECUERDO) ===
    static void BuildLevel10()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_10";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.1f, 0.12f, 0.18f, 1f), 0.01f, new Color(0.18f, 0.2f, 0.28f, 1f));
        SpawnDirectionalLight();
        SpawnLevelLightingSettings(env, new Color(0.15f, 0.22f, 0.32f), 0.0038f);
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 20f), 32f, 42f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(12f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 32f), new Vector3(10f, 0.6f, 8f), env, _floorMat);

        // Flipped gravity zone to the left (Vector3.left)
        GameObject gravityPlatform = MakePlatform("GravityWall", new Vector3(-6f, 4f, 16f), new Vector3(1f, 8f, 14f), env, _bridgeMat);
        CreateGravityZone("LeftWallGravity", new Vector3(-6f, 4f, 16f), new Vector3(1.2f, 8f, 14f), Vector3.left, 24f, 1, mech);

        // Block that player can push inside gravity zone
        GameObject block = SpawnPushableBlock("PF_KineticBlock_Small", new Vector3(-5f, 0.9f, 12f), new Vector3(1.2f, 1.2f, 1.2f), 1.0f, mech);
        block.tag = "KineticBlock";

        // Plate on the vertical wall
        PressurePlate wallPlate = CreatePlate("WallPlate", new Vector3(-5.36f, 4f, 16f), mech);
        wallPlate.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);

        // Exit door opened by wall plate
        DoorController gate = CreateDoor("ExitGate", new Vector3(0f, 1.9f, 26f), new Vector3(5f, 3.8f, 0.55f), mech, new[] { wallPlate });
        gate.latchOpen = false;

        // Central pathway platform
        MakePlatform("CentralPath", new Vector3(0f, 0f, 16f), new Vector3(4f, 0.55f, 14f), env, _bridgeMat);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 33.5f), mech, "Level_11");
        CreateLevelGoal(mech, "Usa la gravedad alterada de la pared izquierda para empujar el bloque hacia la placa de la pared y abrir el camino.", "El bloque presiona la placa en la pared.", "Gravedad burlada.", exit, wallPlate);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 14f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 4f, -11f));

        CreateTutorialTrigger("Tut_L10", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 6f),
            "Nivel 10 — Fragmentos",
            "La zona violeta cambia tu gravedad. Entra allí, empuja el bloque hacia el botón de la pared para desbloquear la puerta central en gravedad normal.",
            15f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-6f, 4.1f, 16f),
            new Vector3(0f, 1.1f, 32f)
        });
        SpawnPuzzleIntent(mech, 1, 2, true, true, true, 24f, "Gravity switch block placement.");

        SpawnPointLight("Light_Wall", new Vector3(-4f, 6f, 16f), new Color(0.6f, 0.2f, 0.8f), 3f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 4f, 33.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Coloca el bloque en la placa de la pared usando la gravedad alterada.", "La mente no tiene dirección fija.", "Camino del fragmento completado.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 16f), 24f, 40f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MultiLayerTimeline, 2f, 32f);

        SaveScene(scene, "Level_10");
    }

    // === LEVEL 11: SINCRONÍA (ESPERANZA) ===
    static void BuildLevel11()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_11";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.04f, 0.06f, 0.1f, 1f), 0.012f, new Color(0.12f, 0.16f, 0.22f, 1f));
        SpawnDirectionalLight();
        SpawnLevelLightingSettings(env, new Color(0.15f, 0.22f, 0.32f), 0.0038f);
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 20f), 36f, 46f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(12f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 32f), new Vector3(10f, 0.6f, 8f), env, _floorMat);

        // Path
        MakePlatform("Corridor", new Vector3(0f, 0f, 16f), new Vector3(4f, 0.55f, 24f), env, _bridgeMat);

        // Conflict Trap zone: triggers door closing
        GameObject trapPlatePlat = MakePlatform("TrapPlatePlat", new Vector3(-8f, 0f, 16f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        PressurePlate trapPlate = CreatePlateOnPlatform("TrapPlate", trapPlatePlat, Vector3.zero, mech, false);
        trapPlate.autoReleaseTimer = 0.5f;

        // Door blocking player path at z = 16
        DoorController conflictDoor = CreateDoor("ConflictDoor", new Vector3(0f, 1.9f, 16f), new Vector3(5f, 3.8f, 0.55f), mech, new PressurePlate[0]);
        conflictDoor.latchOpen = false;

        // Conflict trap configuration
        PuzzleSignal trapSignal = CreatePuzzleSignal("Signal_L11Trap", "Conflicto Resuelto", mech);
        CreateConflictTrap("ConflictTrap_L11", new Vector3(-8f, 1.25f, 16f), new Vector3(3.5f, 2.5f, 4f), mech, new[] { conflictDoor }, null, trapSignal);

        // Plate B that unlocks exit gate at z = 24
        GameObject plateBPlat = MakePlatform("PlateBPlat", new Vector3(8f, 0f, 16f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        PressurePlate plateB = CreatePlateOnPlatform("PlateB", plateBPlat, Vector3.zero, mech, false);

        DoorController exitGate = CreateDoor("ExitGate", new Vector3(0f, 1.9f, 24f), new Vector3(5f, 3.8f, 0.55f), mech, new[] { plateB });
        exitGate.latchOpen = true;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 33.5f), mech, "Level_12");
        CreateLevelGoal(mech, "Evita activar permanentemente la trampa de conflicto con tus ecos, y presiona el interruptor final para huir.", "El portón de salida se abre al sincronizar.", "Sincronía de escape completada.", exit, plateB);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 14f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 5f, -12f));

        CreateTutorialTrigger("Tut_L11", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 11 — Sincronía",
            "Esta trampa de conflicto cierra la puerta del centro si el eco entra. Haz un recorrido donde el eco se mantenga fuera del área roja de la izquierda, y active la placa de la derecha.",
            15f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(8f, 1.1f, 16f),
            new Vector3(0f, 1.1f, 32f)
        });
        SpawnPuzzleIntent(mech, 2, 3, true, true, true, 20f, "Conflict trap evasion and synchronized gate release.");

        SpawnPointLight("Light_Trap", new Vector3(-8f, 3f, 16f), new Color(1f, 0.16f, 0.08f), 3f, 8f, env);
        SpawnPointLight("Light_PlateB", new Vector3(8f, 3f, 16f), new Color(0.24f, 0.76f, 1f), 3f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 4f, 33.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Cruza el portón central coordinando tus ecos.", "La armonía de presente y pasado da esperanza.", "El enlace de sincronía está activo.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 16f), 24f, 40f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MultiLayerTimeline, 2f, 32f);

        SaveScene(scene, "Level_11");
    }

    // === LEVEL 12: RUPTURA (CONFLICTO) ===
    static void BuildLevel12()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_12";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.04f, 0.06f, 0.1f, 1f), 0.012f, new Color(0.12f, 0.16f, 0.22f, 1f));
        SpawnDirectionalLight();
        SpawnLevelLightingSettings(env, new Color(0.15f, 0.22f, 0.32f), 0.0038f);
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 20f), 36f, 46f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(12f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 36f), new Vector3(10f, 0.6f, 8f), env, _floorMat);

        // Path
        MakePlatform("Corridor", new Vector3(0f, 0f, 18f), new Vector3(4f, 0.55f, 28f), env, _bridgeMat);

        // Pushable block
        GameObject block = SpawnPushableBlock("PF_KineticBlock_Medium", new Vector3(0f, 0.9f, 6f), new Vector3(1.8f, 1.8f, 1.8f), 5.0f, mech);
        block.tag = "KineticBlock";

        // Plate on ground
        PressurePlate socketPlate = CreatePlate("SocketPlate", new Vector3(0f, 0.36f, 18f), mech);

        // Side plate for Ghost bridge
        GameObject sidePlat = MakePlatform("SidePlat", new Vector3(-8f, 0f, 18f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        PressurePlate plateA = CreatePlateOnPlatform("PlateA", sidePlat, Vector3.zero, mech, false);
        plateA.autoReleaseTimer = 0.5f;

        PuzzleSignal bridgeSignal = CreatePuzzleSignal("Signal_L12Bridge", "Puente Fantasma L12", mech);
        GameObject bridgeZone = new GameObject("BridgeZone");
        bridgeZone.transform.SetParent(mech, false);
        bridgeZone.transform.position = new Vector3(-8f, 0.25f, 18f);
        BoxCollider bridgeZoneCol = bridgeZone.AddComponent<BoxCollider>();
        bridgeZoneCol.isTrigger = true;
        bridgeZoneCol.size = new Vector3(3.5f, 2f, 3.5f);
        EchoKineticZone bridgeKZone = bridgeZone.AddComponent<EchoKineticZone>();
        SetSerializedValue(bridgeKZone, "role", EchoKineticRole.TimingActivator);
        SetSerializedValue(bridgeKZone, "completionSignal", bridgeSignal);
        SetSerializedValue(bridgeKZone, "requireEcho", true);

        // Ghost bridge at z = 26
        GhostBridge ghostBridge = CreateGhostBridge("GhostBridge_L12", new Vector3(0f, 0f, 26f), new Vector3(4f, 0.55f, 4f), bridgeSignal, mech);

        // Exit Gate opened by sequence: socketPlate (block) AND plateA (echo)
        DoorController exitGate = CreateDoor("ExitGate", new Vector3(0f, 1.9f, 32f), new Vector3(5f, 3.8f, 0.55f), mech, new PressurePlate[0]);
        exitGate.latchOpen = false;

        GameObject condObj = new GameObject("Condition_L12");
        condObj.transform.SetParent(mech);
        PuzzleCondition condition = condObj.AddComponent<PuzzleCondition>();
        condition.type = PuzzleCondition.ConditionType.AllPlatesSimultaneous;
        condition.plates = new[] { socketPlate, plateA };
        condition.doorsToOpen = new[] { exitGate };

        PuzzleSignal exitSignal = CreatePuzzleSignal("Signal_ExitGateL12", "Portón Abierto", mech);
        condition.targetSignal = exitSignal;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 37.5f), mech, "Level_13");
        CreateSignalGoal(mech, "Empuja el bloque a la placa central y mantén presionado el interruptor lateral con un eco para abrir el portón final.", "Ambos interruptores están en armonía.", "Fractura de conflicto superada.", exit, exitSignal);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 16f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 5f, -12f));

        CreateTutorialTrigger("Tut_L12", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 12 — Ruptura",
            "Sincroniza tus recursos: empuja el bloque mediano al botón central del puente, y manda a tu eco a presionar el botón izquierdo para cruzar el puente fantasma.",
            15f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-8f, 1.1f, 18f),
            new Vector3(0f, 1.1f, 36f)
        });
        SpawnPuzzleIntent(mech, 2, 4, true, true, true, 24f, "Block + ghost bridge sequence.");

        SpawnPointLight("Light_Socket", new Vector3(0f, 3f, 18f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_Side", new Vector3(-8f, 3f, 18f), new Color(0.24f, 0.76f, 1f), 3f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 4f, 37.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Coloca el bloque y activa la placa del eco.", "La ruptura final requiere la unión de tus fragmentos.", "Ruptura resuelta.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 18f), 24f, 40f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MultiLayerTimeline, 2f, 32f);

        SaveScene(scene, "Level_12");
    }

    // === LEVEL 13: LA CONVERSACIÓN (VERDAD) ===
    static void BuildLevel13()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_13";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.04f, 0.06f, 0.1f, 1f), 0.012f, new Color(0.12f, 0.16f, 0.22f, 1f));
        SpawnDirectionalLight();
        SpawnLevelLightingSettings(env, new Color(0.15f, 0.22f, 0.32f), 0.0038f);
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 20f), 36f, 46f, env);

        // Mirrored Divided Chambers
        GameObject chamberA = MakePlatform("ChamberA", new Vector3(-6f, 0f, 16f), new Vector3(8f, 0.6f, 24f), env, _floorMat);
        GameObject chamberB = MakePlatform("ChamberB", new Vector3(6f, 0f, 16f), new Vector3(8f, 0.6f, 24f), env, _floorMat);

        // Partition
        GameObject partition = MakePlatform("Partition", new Vector3(0f, 4f, 16f), new Vector3(0.25f, 8f, 24f), env, _bridgeMat);

        // Gate between chambers
        DoorController crossGate = CreateDoor("CrossGate", new Vector3(0f, 1.9f, 6f), new Vector3(3f, 3.8f, 0.55f), mech, new PressurePlate[0]);
        crossGate.latchOpen = false;

        // Plate in Chamber A opens crossGate
        PressurePlate startPlate = CreatePlateOnPlatform("StartPlate", chamberA, new Vector3(0f, 0f, -6f), mech, false);
        startPlate.autoReleaseTimer = 0.5f;

        GameObject wireObj = new GameObject("Wire_CrossGate");
        wireObj.transform.SetParent(mech);
        PuzzleWire wire = wireObj.AddComponent<PuzzleWire>();
        wire.connections = new PuzzleWire.Connection[1];
        wire.connections[0] = new PuzzleWire.Connection {
            door = crossGate,
            plates = new[] { startPlate },
            logic = PuzzleWire.LogicMode.AND,
            latchOpen = false
        };

        // Mirror plate in Chamber A opens exit door in Chamber B
        PressurePlate mirrorPlate = CreatePlateOnPlatform("MirrorPlate", chamberA, new Vector3(0f, 0f, 6f), mech, false);
        mirrorPlate.autoReleaseTimer = 0.5f;

        DoorController exitDoor = CreateDoor("ExitGate", new Vector3(6f, 1.9f, 26f), new Vector3(4f, 3.8f, 0.55f), mech, new PressurePlate[0]);
        exitDoor.latchOpen = false;

        GameObject wireExit = new GameObject("Wire_ExitDoor");
        wireExit.transform.SetParent(mech);
        PuzzleWire wireEx = wireExit.AddComponent<PuzzleWire>();
        wireEx.connections = new PuzzleWire.Connection[1];
        wireEx.connections[0] = new PuzzleWire.Connection {
            door = exitDoor,
            plates = new[] { mirrorPlate },
            logic = PuzzleWire.LogicMode.AND,
            latchOpen = false
        };

        // Elevator in Chamber B
        PressurePlate elevatorPlate = CreatePlateOnPlatform("ElevatorPlate", chamberB, new Vector3(0f, 0f, -4f), mech, false);
        elevatorPlate.autoReleaseTimer = 1f;
        TimedMovingPlatform exitLift = CreateBridge("ExitLift", new Vector3(6f, 0f, 26f), Vector3.zero, new Vector3(0f, 6f, 0f), new Vector3(3f, 0.55f, 3f), elevatorPlate, mech);

        LevelExit exit = CreateLevelExit(new Vector3(6f, 7.25f, 28f), mech, "Level_14");
        CreateLevelGoal(mech, "Usa la proyección de eco en la sala izquierda para abrirte paso a la derecha y elevarte al final.", "El portón final del espejo se ha abierto.", "Conversación de la verdad finalizada.", exit, startPlate, mirrorPlate);

        // Player starts in Chamber A
        GameObject player = SpawnPlayer(new Vector3(-6f, 1.1f, 4f), true, 2, 12f);
        SpawnGameplayCamera(player.transform, new Vector3(-7f, 4.5f, -10f));

        CreateTutorialTrigger("Tut_L13", new Vector3(-6f, 1f, 4f), new Vector3(6f, 3f, 4f),
            "Nivel 13 — La Conversación",
            "Para cruzar a la derecha, graba a tu eco parándose en la placa inicial durante 4 segundos. Cruza la compuerta central, y sincronízate frente a la placa del espejo.",
            15f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(-6f, 1.1f, 4f),
            new Vector3(-6f, 1.1f, 22f),
            new Vector3(6f, 1.1f, 22f),
            new Vector3(6f, 7.1f, 28f)
        });
        SpawnPuzzleIntent(mech, 3, 5, true, true, true, 22f, "Mirrored divided puzzle sequence.");

        SpawnPointLight("Light_Plate1", new Vector3(-6f, 3f, 10f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_Plate2", new Vector3(-6f, 3f, 22f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(6f, 8f, 28f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Sincroniza tus ecos a través del espejo.", "El eco es tu espejo de verdad.", "Conversación de memoria unificada.");
        SpawnAmbientLights(env, new Vector3(0f, 2f, 16f), 20f, 32f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MirrorPath, 2f, 28f);

        SaveScene(scene, "Level_13");
    }

    // === LEVEL 14: ACEPTACIÓN (LIBERACIÓN) ===
    static void BuildLevel14()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_14";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.04f, 0.06f, 0.1f, 1f), 0.012f, new Color(0.12f, 0.16f, 0.22f, 1f));
        SpawnDirectionalLight();
        SpawnLevelLightingSettings(env, new Color(0.15f, 0.22f, 0.32f), 0.0038f);
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 24f), 36f, 46f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(12f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 36f), new Vector3(12f, 0.6f, 8f), env, _floorMat);

        // Path
        MakePlatform("Corridor", new Vector3(0f, 0f, 18f), new Vector3(4f, 0.55f, 28f), env, _bridgeMat);

        // Three plates for three echoes
        GameObject plateA = MakePlatform("PlateA", new Vector3(-8f, 0f, 12f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        GameObject plateB = MakePlatform("PlateB", new Vector3(8f, 0f, 18f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        GameObject plateC = MakePlatform("PlateC", new Vector3(-8f, 0f, 24f), new Vector3(4f, 0.5f, 4f), env, _floorMat);

        PressurePlate pA = CreatePlateOnPlatform("pA", plateA, Vector3.zero, mech, false);
        PressurePlate pB = CreatePlateOnPlatform("pB", plateB, Vector3.zero, mech, false);
        PressurePlate pC = CreatePlateOnPlatform("pC", plateC, Vector3.zero, mech, false);

        pA.autoReleaseTimer = 4.0f;
        pB.autoReleaseTimer = 4.0f;
        pC.autoReleaseTimer = 4.0f;

        // Timed bridges
        TimedMovingPlatform bridge1 = CreateBridge("Bridge1", new Vector3(0f, 0f, 12f), new Vector3(0f, -8f, 0f), Vector3.zero, new Vector3(4f, 0.55f, 6f), pA, mech);
        TimedMovingPlatform bridge2 = CreateBridge("Bridge2", new Vector3(0f, 0f, 20f), new Vector3(0f, -8f, 0f), Vector3.zero, new Vector3(4f, 0.55f, 6f), pB, mech);
        TimedMovingPlatform bridge3 = CreateBridge("Bridge3", new Vector3(0f, 0f, 28f), new Vector3(0f, -8f, 0f), Vector3.zero, new Vector3(4f, 0.55f, 6f), pC, mech);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 37.5f), mech, "Level_15");
        CreateLevelGoal(mech, "Usa tus tres ecos consecutivamente en las placas laterales para construir el puente hasta el final.", "Las tres memorias se alinean en armonía completa.", "Aceptación alcanzada.", exit, pA, pB, pC);

        // Player starts with max 3 echoes
        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 3, 18f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 5f, -12f));

        CreateTutorialTrigger("Tut_L14", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 14 — Aceptación",
            "Tienes 3 ecos. Graba un recorrido encadenado para presionar las tres placas. Tu presente correrá por el centro a través de los puentes.",
            15f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-8f, 1.1f, 12f),
            new Vector3(8f, 1.1f, 18f),
            new Vector3(-8f, 1.1f, 24f),
            new Vector3(0f, 1.1f, 36f)
        });
        SpawnPuzzleIntent(mech, 3, 4, true, true, true, 28f, "Triple echo timed bridge chain.");

        SpawnPointLight("Light_PA", new Vector3(-8f, 3f, 12f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_PB", new Vector3(8f, 3f, 18f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_PC", new Vector3(-8f, 3f, 24f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 4f, 37.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Coordina tres ecos para construir la ruta central.", "Tres recuerdos actúan en sintonía.", "El camino está libre.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 18f), 24f, 40f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MovingCity, 2f, 32f);

        SaveScene(scene, "Level_14");
    }

    // === LEVEL 15: ECHOES OF YOU (INTEGRACIÓN) ===
    static void BuildLevel15()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_15";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.04f, 0.06f, 0.1f, 1f), 0.012f, new Color(0.12f, 0.16f, 0.22f, 1f));
        SpawnDirectionalLight();
        SpawnLevelLightingSettings(env, new Color(0.15f, 0.22f, 0.32f), 0.0038f);
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 36f), 45f, 55f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 36f), 45f, 55f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(12f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 40f), new Vector3(12f, 0.6f, 8f), env, _floorMat);

        // Corridor
        MakePlatform("Corridor", new Vector3(0f, 0f, 20f), new Vector3(4f, 0.55f, 32f), env, _bridgeMat);

        // Sequence lock plates
        GameObject sidePlat = MakePlatform("SidePlat", new Vector3(-8f, 0f, 20f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        PressurePlate plateA = CreatePlateOnPlatform("PlateA", sidePlat, Vector3.zero, mech, false);
        plateA.autoReleaseTimer = 4.0f;

        // Pushable block at start
        GameObject block = SpawnPushableBlock("PF_KineticBlock_Medium", new Vector3(0f, 0.9f, 6f), new Vector3(1.8f, 1.8f, 1.8f), 5.0f, mech);
        block.tag = "KineticBlock";

        // Socket plate in center corridor
        PressurePlate socketPlate = CreatePlate("SocketPlate", new Vector3(0f, 0.36f, 20f), mech);

        // Ghost bridge at z = 28
        PuzzleSignal bridgeSignal = CreatePuzzleSignal("Signal_L15Bridge", "Puente Final", mech);
        GameObject bridgeZone = new GameObject("BridgeZone");
        bridgeZone.transform.SetParent(mech, false);
        bridgeZone.transform.position = new Vector3(-8f, 0.25f, 20f);
        BoxCollider bridgeZoneCol = bridgeZone.AddComponent<BoxCollider>();
        bridgeZoneCol.isTrigger = true;
        bridgeZoneCol.size = new Vector3(3.5f, 2f, 3.5f);
        EchoKineticZone bridgeKZone = bridgeZone.AddComponent<EchoKineticZone>();
        SetSerializedValue(bridgeKZone, "role", EchoKineticRole.TimingActivator);
        SetSerializedValue(bridgeKZone, "completionSignal", bridgeSignal);
        SetSerializedValue(bridgeKZone, "requireEcho", true);

        GhostBridge ghostBridge = CreateGhostBridge("GhostBridge_L15", new Vector3(0f, 0f, 28f), new Vector3(4f, 0.55f, 4f), bridgeSignal, mech);

        // Exit gate opened by: socketPlate (block) AND plateA (echo)
        DoorController exitGate = CreateDoor("ExitGate", new Vector3(0f, 1.9f, 34f), new Vector3(5f, 3.8f, 0.55f), mech, new PressurePlate[0]);
        exitGate.latchOpen = false;

        GameObject condObj = new GameObject("Condition_L15");
        condObj.transform.SetParent(mech);
        PuzzleCondition condition = condObj.AddComponent<PuzzleCondition>();
        condition.type = PuzzleCondition.ConditionType.AllPlatesSimultaneous;
        condition.plates = new[] { socketPlate, plateA };
        condition.doorsToOpen = new[] { exitGate };

        PuzzleSignal exitSignal = CreatePuzzleSignal("Signal_ExitGateL15", "Portón Final Abierto", mech);
        condition.targetSignal = exitSignal;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 41.5f), mech, "MainMenu", "¡Felicidades! Has completado Echoes of You.");
        CreateSignalGoal(mech, "Integra todas las mecánicas: empuja el bloque a la placa central y mantén activa la placa del eco para abrir el portón final.", "El portón final responde a la armonía.", "Sinfonía final completada.", exit, exitSignal);

        // Player starts with max 3 echoes
        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 3, 20f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 5f, -12f));

        CreateTutorialTrigger("Tut_L15", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 15 — Echoes of You",
            "La prueba final de integración. Coordina tus ecos para presionar la placa lateral izquierda, empujar el bloque central a la placa del puente y cruzar el puente fantasma.",
            20f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-8f, 1.1f, 20f),
            new Vector3(0f, 1.1f, 40f)
        });
        SpawnPuzzleIntent(mech, 3, 5, true, true, true, 36f, "Master level integration.");

        SpawnPointLight("Light_Socket", new Vector3(0f, 3f, 20f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_Side", new Vector3(-8f, 3f, 20f), new Color(0.24f, 0.76f, 1f), 3f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 4f, 41.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Resuelve la prueba final de integración.", "La armonía es completa. Tú eres tu eco.", "Integración completada.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 20f), 24f, 40f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MultiLayerTimeline, 2f, 40f);

        SaveScene(scene, "Level_15");
    }

    // === LEVEL SPONSER HELPERS ===
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

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        PlayerController playerController = player.AddComponent<PlayerController>();
        playerController.groundMask = (1 << 6);
        playerController.jumpHeight = EchoesWorldMetrics.PlayerJumpHeight;

        player.AddComponent<PlayerCharacterVisualSetup>();
        player.AddComponent<PlayerAdvancedLocomotion>();

        if (enableRecorder)
        {
            EchoRecorder recorder = player.AddComponent<EchoRecorder>();
            SetSerializedValue(recorder, "echoPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(EchoPrefabPath));
            SetSerializedValue(recorder, "maxEchoes", maxEchoes);
            SetSerializedValue(recorder, "maxRecordSeconds", maxRecordSeconds);
        }

        player.AddComponent<PlayerLocomotionAnimator>();
        player.AddComponent<PlayerAnimationRuntimeBootstrap>();
        player.AddComponent<CharacterPush>(); // Physics push

        CreateCharacterVisual(player.transform);

        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform, false);
        groundCheck.transform.localPosition = new Vector3(0f, -0.96f, 0f);

        GameObject focus = new GameObject("CameraFocus");
        focus.transform.SetParent(player.transform, false);
        focus.transform.localPosition = new Vector3(0f, 1.75f, 0.18f);

        SpawnPointLight("PlayerRimLight", position + new Vector3(1.2f, 2.4f, -1.8f), new Color(0.72f, 0.88f, 1f, 1f), 0.9f, 5f, player.transform);

        return player;
    }

    static void CreateCharacterVisual(Transform player)
    {
        GameObject visualRoot = new GameObject("PlayerVisual");
        visualRoot.transform.SetParent(player, false);
        CreateCapsuleVisual(visualRoot.transform, false);
    }

    static void SpawnGameplayCamera(Transform player, Vector3? customOffset = null)
    {
        Transform cameraRoot = CreateRoot("--- CAMERA ---");

        Vector3 offset = customOffset.HasValue ? customOffset.Value : new Vector3(-5.5f, 3.2f, -9.5f);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(cameraRoot, false);
        cameraObject.transform.position = player.position + offset;
        Camera cameraRef = cameraObject.AddComponent<Camera>();
        cameraRef.clearFlags = CameraClearFlags.SolidColor;
        cameraRef.backgroundColor = new Color(0.02f, 0.03f, 0.05f, 1f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CameraShake>();
        cameraObject.AddComponent<AudioSource>();
        var gfc = cameraObject.AddComponent<GameFeelController>();

        var loop1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Efectos de sonido/144046__gchase__room_tone_ambience_medium_control_low_hum.wav");
        var loop2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Efectos de sonido/607238__szegvari__electric-dream-synth-drone-electric-cinematic.wav");
        var loop3 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Efectos de sonido/Ventilation.wav");
        var chime = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Efectos de sonido/freesound_community-clock-chime-88027.mp3");

        SetSerializedValue(gfc, "ambientLoopClip", loop1);
        SetSerializedValue(gfc, "industrialDroneClip", loop2);
        SetSerializedValue(gfc, "ventilationHumClip", loop3);
        SetSerializedValue(gfc, "gateChimeClip", chime);

        GameObject brain = new GameObject("CinemachineBrain");
        brain.transform.SetParent(cameraRoot, false);
        brain.AddComponent<CinemachineBrain>();
        brain.AddComponent<CinemachineRuntimeSetup>();
    }

    static void SpawnGameplayHud(Transform parent)
    {
        GameObject hud = new GameObject("GameplayHUD");
        hud.transform.SetParent(parent, false);
        UIDocument doc = hud.AddComponent<UIDocument>();
        doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GameHUDUxmlPath);
        doc.panelSettings = GetOrCreatePanelSettings();
        hud.AddComponent<GameHUD>();
    }

    static void SpawnPauseMenu(Transform parent)
    {
        GameObject pause = new GameObject("PauseMenu");
        pause.transform.SetParent(parent, false);
        UIDocument doc = pause.AddComponent<UIDocument>();
        doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PauseMenuUxmlPath);
        doc.panelSettings = GetOrCreatePanelSettings();
        pause.AddComponent<PauseMenu>();
        pause.AddComponent<TutorialHUD>();
    }

    static void SpawnGameOver(Transform parent)
    {
        GameObject over = new GameObject("GameOverHUD");
        over.transform.SetParent(parent, false);
        UIDocument doc = over.AddComponent<UIDocument>();
        doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GameOverUxmlPath);
        doc.panelSettings = GetOrCreatePanelSettings();
        over.AddComponent<GameOverController>();
    }

    static void SpawnAmbientLights(Transform parent, Vector3 center, float width, float depth)
    {
        GameObject alObj = new GameObject("AmbientLights");
        alObj.transform.SetParent(parent, false);
        alObj.transform.position = center;

        SpawnPointLight("Amb_1", center + new Vector3(-width * 0.25f, 4f, -depth * 0.25f), new Color(0.12f, 0.44f, 0.72f), 1.2f, 24f, alObj.transform);
        SpawnPointLight("Amb_2", center + new Vector3(width * 0.25f, 5f, depth * 0.25f), new Color(0.18f, 0.52f, 0.78f), 1.0f, 28f, alObj.transform);
        SpawnPointLight("Amb_3", center + new Vector3(0f, 3f, 0f), new Color(0.1f, 0.35f, 0.6f), 0.8f, 32f, alObj.transform);
    }

    static void SpawnPuzzleIntent(Transform parent, int buttons, int actions, bool movement, bool timing, bool multiStep, float echoDistance, string note)
    {
        GameObject go = new GameObject("PuzzleIntent");
        go.transform.SetParent(parent, false);
        PuzzleIntent intent = go.AddComponent<PuzzleIntent>();
        SetSerializedValue(intent, "requiredPlates", buttons);
        SetSerializedValue(intent, "totalActions", actions);
        SetSerializedValue(intent, "requiresMovementPacing", movement);
        SetSerializedValue(intent, "requiresTimingSynchronization", timing);
        SetSerializedValue(intent, "requiresMultiStepCoordination", multiStep);
        SetSerializedValue(intent, "averageEchoPathDistance", echoDistance);
        SetSerializedValue(intent, "designNote", note);
    }

    static void SpawnEchoPathHint(Transform parent, Vector3[] waypoints)
    {
        GameObject hintObj = new GameObject("EchoPathHint");
        hintObj.transform.SetParent(parent, false);
        EchoPathHint hint = hintObj.AddComponent<EchoPathHint>();
        SetSerializedValue(hint, "pathWaypoints", waypoints);
    }

    static void SpawnLevelRuntime(Transform parent, string objective, string intro, string completion)
    {
        GameObject runObj = new GameObject("LevelRuntime");
        runObj.transform.SetParent(parent, false);
        LevelRuntimeController controller = runObj.AddComponent<LevelRuntimeController>();
        SetSerializedValue(controller, "objectiveText", objective);
        SetSerializedValue(controller, "introLine", intro);
        SetSerializedValue(controller, "completionLine", completion);
        SetSerializedValue(controller, "allowSoftReset", true);
        SetSerializedValue(controller, "allowHardReset", true);
    }

    static void SpawnExperienceSystems(
        Transform mechanics,
        Transform environment,
        LevelExit exit,
        LevelArchetype archetype,
        float speedMultiplier,
        float boundaryDepth,
        bool enableChase = false)
    {
        GameObject experience = new GameObject("ExperienceSystems");
        experience.transform.SetParent(mechanics, false);

        LevelExperienceBlueprint blueprint = experience.AddComponent<LevelExperienceBlueprint>();
        SetSerializedValue(blueprint, "archetype", archetype);
        SetSerializedValue(blueprint, "speedMultiplier", speedMultiplier);
        SetSerializedValue(blueprint, "boundaryDepth", boundaryDepth);
        SetSerializedValue(blueprint, "exitRef", exit);

        if (enableChase)
        {
            GameObject chaseObj = new GameObject("ChaseHazard");
            chaseObj.transform.SetParent(experience.transform, false);
            chaseObj.transform.localPosition = new Vector3(0f, 1.25f, -12f);
            BoxCollider col = chaseObj.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(24f, 12f, 4f);
            chaseObj.AddComponent<ChaseHazardMotor>();

            // Visual red fog boundary
            GameObject fogVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fogVisual.name = "LaserWall";
            fogVisual.transform.SetParent(chaseObj.transform, false);
            fogVisual.transform.localPosition = Vector3.zero;
            fogVisual.transform.localScale = new Vector3(24f, 12f, 0.4f);
            Object.DestroyImmediate(fogVisual.GetComponent<Collider>());
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.05f, 0.05f, 0.25f);
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            fogVisual.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }
    }

    static void CreatePacingTrigger(string name, Vector3 position, LevelPacingMarker.Section section, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        BoxCollider col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(10f, 6f, 3f);
        LevelPacingMarker marker = go.AddComponent<LevelPacingMarker>();
        SetSerializedValue(marker, "targetSection", section);
    }

    static void CreateTutorialTrigger(string name, Vector3 position, Vector3 size, string message, string hint, float duration, Transform parent)
    {
        GameObject trigger = new GameObject(name);
        trigger.transform.SetParent(parent, false);
        trigger.transform.position = position;

        BoxCollider colliderRef = trigger.AddComponent<BoxCollider>();
        colliderRef.isTrigger = true;
        colliderRef.size = size;

        TutorialTrigger tut = trigger.AddComponent<TutorialTrigger>();
        SetSerializedValue(tut, "messageTitle", message);
        SetSerializedValue(tut, "messageBody", hint);
        SetSerializedValue(tut, "toastDuration", duration);
    }

    static void CreateHubPortal(Vector3 position, string sceneName, string displayName, string memoryLine, Transform mechanics, Transform visuals)
    {
        GameObject root = new GameObject("Portal_" + sceneName);
        root.transform.SetParent(mechanics, false);
        root.transform.position = position;

        BoxCollider colliderRef = root.AddComponent<BoxCollider>();
        colliderRef.isTrigger = true;
        colliderRef.size = new Vector3(2.5f, 3.5f, 2.5f);
        colliderRef.center = new Vector3(0f, 1.25f, 0f);

        HubPortal portal = root.AddComponent<HubPortal>();
        SetSerializedValue(portal, "sceneToLoad", sceneName);
        SetSerializedValue(portal, "displayName", displayName);
        SetSerializedValue(portal, "memoryLine", memoryLine);

        // Visual frame
        GameObject leftColumn = Instantiate3DModel(SciFiColumnSimple, "LeftColumn", position + new Vector3(-1.1f, 0f, 0f), new Vector3(0.35f, 3.2f, 0.35f), Quaternion.identity, visuals, _bridgeMat);
        GameObject rightColumn = Instantiate3DModel(SciFiColumnSimple, "RightColumn", position + new Vector3(1.1f, 0f, 0f), new Vector3(0.35f, 3.2f, 0.35f), Quaternion.identity, visuals, _bridgeMat);
        GameObject lintel = Instantiate3DModel(SciFiPlatformSimple, "Lintel", position + new Vector3(0f, 3.1f, 0f), new Vector3(2.4f, 0.25f, 0.35f), Quaternion.identity, visuals, _goalMat);

        GameObject portalQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        portalQuad.name = "PortalSurface";
        portalQuad.transform.SetParent(visuals, false);
        portalQuad.transform.position = position + new Vector3(0f, 1.5f, 0.05f);
        portalQuad.transform.localScale = new Vector3(2f, 3f, 1f);
        Object.DestroyImmediate(portalQuad.GetComponent<Collider>());
        portalQuad.GetComponent<MeshRenderer>().sharedMaterial = _echoMat;
    }

    static void SpawnBarrierWall(string name, Vector3 position, Vector3 scale, Transform parent, Material material = null)
    {
        GameObject wall = Instantiate3DModel(SciFiWallAstra, name, position, scale, Quaternion.identity, parent, material != null ? material : _archMat);
        wall.AddComponent<KenneyTiling>();
    }

    static void SpawnKillZone(string name, Vector3 position, Vector3 size, Transform parent)
    {
        GameObject kz = new GameObject(name);
        kz.transform.SetParent(parent, false);
        kz.transform.position = position;
        BoxCollider col = kz.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;
        kz.AddComponent<KillVolume>();
    }

    static void SpawnLiminalHorizon(string prefix, Vector3 center, float width, float depth, Transform parent)
    {
        GameObject root = new GameObject(prefix + "_Horizon");
        root.transform.SetParent(parent, false);
        root.transform.position = center;

        GameObject leftWall = MakePlatform("HorizonWall_L", new Vector3(-width * 0.5f - 4f, 12f, 0f), new Vector3(2f, 26f, depth + 8f), root.transform, _archMat);
        leftWall.AddComponent<KenneyTiling>();
        GameObject rightWall = MakePlatform("HorizonWall_R", new Vector3(width * 0.5f + 4f, 12f, 0f), new Vector3(2f, 26f, depth + 8f), root.transform, _archMat);
        rightWall.AddComponent<KenneyTiling>();
    }

    static void SpawnDistantArchitecture(Vector3 center, float width, float depth, Transform parent)
    {
        Transform structures = CreateRoot("--- DISTANT STRUCTURES ---");
        structures.SetParent(parent, false);

        Instantiate3DModel(SciFiColumnRound, "DistantTower_A", center + new Vector3(-width * 0.65f, 12f, depth * 0.45f), new Vector3(6f, 48f, 6f), Quaternion.identity, structures, _archMat);
        Instantiate3DModel(SciFiColumnAstra, "DistantTower_B", center + new Vector3(width * 0.65f, 8f, depth * 0.35f), new Vector3(8f, 36f, 8f), Quaternion.identity, structures, _archMat);
        Instantiate3DModel(SciFiColumnRound, "DistantTower_C", center + new Vector3(0f, 18f, depth * 0.75f), new Vector3(12f, 64f, 12f), Quaternion.identity, structures, _goalMat);
    }

    static void SetupAtmosphere(Color originalFogColor, float originalFogDensity, Color originalAmbientColor)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = originalAmbientColor;
        RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
    }

    static void SpawnLevelLightingSettings(Transform parent, Color fogColor, float fogDensity)
    {
        GameObject lSettings = new GameObject("LevelLightingSettings");
        lSettings.transform.SetParent(parent, false);
        LevelLightingSettings settings = lSettings.AddComponent<LevelLightingSettings>();
        SetSerializedValue(settings, "ambientColor", RenderSettings.ambientLight);
        SetSerializedValue(settings, "fogColor", fogColor);
        SetSerializedValue(settings, "fogDensity", fogDensity);
    }

    static void SpawnDirectionalLight()
    {
        GameObject lightObj = GameObject.Find("Directional Light");
        if (lightObj == null)
            lightObj = new GameObject("Directional Light");

        Light lightComponent = lightObj.GetComponent<Light>();
        if (lightComponent == null)
            lightComponent = lightObj.AddComponent<Light>();

        lightComponent.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(34f, -42f, 0f);
        lightComponent.color = new Color(0.74f, 0.85f, 1f, 1f);
        lightComponent.intensity = 0.45f;
        lightComponent.shadows = LightShadows.Hard;
    }

    static void SpawnAmbientParticles(Vector3 position, Vector3 boxScale)
    {
        // Spawns low intensity particles to simulate liminal memory dust
        GameObject particles = new GameObject("AmbientMemoryParticles");
        particles.transform.position = position;
        ParticleSystem ps = particles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 6f;
        main.startSpeed = 0.25f;
        main.startSize = 0.08f;
        main.maxParticles = 80;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = boxScale;
        var emission = ps.emission;
        emission.rateOverTime = 3f;
    }

    static void SpawnSmokeVolume(string name, Vector3 position, Vector3 boxScale, Transform parent, float rateOverTime)
    {
        GameObject smoke = new GameObject(name);
        smoke.transform.SetParent(parent, false);
        smoke.transform.position = position;

        ParticleSystem ps = smoke.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 4f;
        main.loop = true;
        main.startLifetime = 4.5f;
        main.startSpeed = 0.45f;
        main.startSize = 4f;
        main.startColor = new Color(0.12f, 0.16f, 0.22f, 0.18f);
        main.maxParticles = 60;

        var emission = ps.emission;
        emission.rateOverTime = rateOverTime;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = boxScale;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = LoadSmokeMaterial();
    }

    static void SpawnIntroDressing(Transform env, Vector3 puzzleAnchor)
    {
        // Spawns neat SciFi background props near the level spawn point to set atmosphere
        Instantiate3DModel(SciFiPropCrate, "IntroCrate_A", puzzleAnchor + new Vector3(-2.2f, 0.35f, 2f), Vector3.one, Quaternion.Euler(0f, 25f, 0f), env, _bridgeMat);
        Instantiate3DModel(SciFiPropCrate, "IntroCrate_B", puzzleAnchor + new Vector3(-2.8f, 0.35f, 2.5f), new Vector3(0.8f, 0.8f, 0.8f), Quaternion.Euler(0f, -15f, 0f), env, _bridgeMat);
        Instantiate3DModel(SciFiPropVent, "IntroVent_A", puzzleAnchor + new Vector3(2.5f, 0.15f, 1f), Vector3.one, Quaternion.identity, env, _archMat);
    }

    static void SpawnSurrealBackdrop(Transform env, Vector3 puzzleAnchor, float playRadius, float playDepth)
    {
        Transform structures = CreateRoot("--- SURREAL BACKDROP ---");
        structures.SetParent(env, false);

        // Backdrop rings and spires aligned symmetrically to guide the camera perspective
        Instantiate3DModel(SciFiColumnAstra, "BackdropRing_L", puzzleAnchor + new Vector3(-playRadius - 10f, 4f, playDepth * 0.5f), new Vector3(6f, 6f, 6f), Quaternion.Euler(0f, 0f, 45f), structures, _archMat);
        Instantiate3DModel(SciFiColumnAstra, "BackdropRing_R", puzzleAnchor + new Vector3(playRadius + 10f, 4f, playDepth * 0.5f), new Vector3(6f, 6f, 6f), Quaternion.Euler(0f, 0f, -45f), structures, _archMat);
        Instantiate3DModel(SciFiColumnRound, "BackdropTower_Center", puzzleAnchor + new Vector3(0f, 18f, playDepth + 18f), new Vector3(14f, 72f, 14f), Quaternion.identity, structures, _goalMat);
    }

    static void TrySpawnDecorPrefab(string name, string assetPath, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab != null)
        {
            GameObject decor = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            decor.name = name;
            decor.transform.SetParent(parent, false);
            decor.transform.position = position;
            decor.transform.rotation = rotation;
            decor.transform.localScale = scale;
            ApplyMaterialOverride(decor, _bridgeMat);
        }
    }

    static void EnsureFolders()
    {
        EnsureFolder(SceneRoot);
        EnsureFolder(MaterialRoot);
        EnsureFolder(PrefabRoot);
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
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
        Shader standardShader = Shader.Find("Standard");
        if (standardShader == null)
        {
            Debug.LogError("[Echoes Production] Could not find Standard shader!");
            return;
        }

        _archMat   = GetOrCreateMaterial("Mat_Architecture", HexColor("3A3E47"), false, null);
        _floorMat  = GetOrCreateMaterial("Mat_Floor",        HexColor("30333D"), false, null);
        _plateMat  = GetOrCreateEmissiveMaterial("Mat_Plate", HexColor("141A29"), new Color(0f, 0.4f, 0.52f) * 1.5f, null);
        _bridgeMat = GetOrCreateMaterial("Mat_Bridge",       HexColor("3B4454"), false, null);
        _doorMat   = GetOrCreateEmissiveMaterial("Mat_Door",  HexColor("7E1E2F"), new Color(0.4f, 0.05f, 0.05f) * 0.8f, null);
        _goalMat   = GetOrCreateEmissiveMaterial("Mat_Exit",  HexColor("FFEBB5"), new Color(1.0f, 0.7f, 0.35f) * 2.5f, null);
        _playerMat = GetOrCreateMaterial("Mat_Player",       HexColor("FFFFFF"), false);
        _echoMat   = GetOrCreateTransparentMaterial("Mat_Echo", new Color(0f, 0.8f, 1f, 0.45f), true);

        // Apply Premium Physical Textures & Shading Attributes (matching our visual GDD V4 plan)
        SetupMaterialTextures(_floorMat, MegaTextureRoot + "/T_Trim_01_BaseColor.png", MegaTextureRoot + "/T_Trim_01_Normal.png", MegaTextureRoot + "/T_Trim_01_ORM.png", 0.8f, 0.55f, 0.42f, new Vector2(2.5f, 2.5f));
        SetupMaterialTextures(_bridgeMat, MegaTextureRoot + "/T_Trim_02_BaseColor_Blue.png", MegaTextureRoot + "/T_Trim_02_Normal.png", MegaTextureRoot + "/T_Trim_02_ORM.png", 0.85f, 0.7f, 0.5f, new Vector2(2.2f, 2.2f));
        SetupMaterialTextures(_archMat, MegaTextureRoot + "/T_PaddedWall_BaseColor.png", MegaTextureRoot + "/T_PaddedWall_Normal.png", MegaTextureRoot + "/T_PaddedWall_ORM.png", 0.75f, 0.3f, 0.35f, new Vector2(1.8f, 1.8f));
        SetupMaterialTextures(_doorMat, MegaTextureRoot + "/T_Trim_02_BaseColor_Red.png", MegaTextureRoot + "/T_Trim_02_Normal.png", MegaTextureRoot + "/T_Trim_02_ORM.png", 0.85f, 0.75f, 0.45f, new Vector2(2.2f, 2.2f));
    }

    static void SetupMaterialTextures(Material mat, string albedoPath, string normalPath, string aoPath, float bumpScale, float metallic, float smoothness, Vector2? tiling)
    {
        if (mat == null) return;
        if (!string.IsNullOrEmpty(albedoPath))
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            if (tex != null) mat.SetTexture("_MainTex", tex);
        }
        if (!string.IsNullOrEmpty(normalPath))
        {
            Texture2D norm = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if (norm != null)
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", norm);
                mat.SetFloat("_BumpScale", bumpScale);
            }
        }
        if (!string.IsNullOrEmpty(aoPath))
        {
            Texture2D ao = AssetDatabase.LoadAssetAtPath<Texture2D>(aoPath);
            if (ao != null) mat.SetTexture("_OcclusionMap", ao);
        }
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Glossiness", smoothness);
        if (tiling.HasValue)
        {
            mat.SetTextureScale("_MainTex", tiling.Value);
            if (!string.IsNullOrEmpty(normalPath)) mat.SetTextureScale("_BumpMap", tiling.Value);
            if (!string.IsNullOrEmpty(aoPath)) mat.SetTextureScale("_OcclusionMap", tiling.Value);
        }
    }

    static Material GetOrCreateMaterial(string name, Color color, bool emissive = false, Texture2D tex = null)
    {
        string path = $"{MaterialRoot}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            if (tex != null) mat.mainTexture = tex;
            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 1.5f);
            }
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    static Material GetOrCreateTransparentMaterial(string name, Color color, bool emissive)
    {
        string path = $"{MaterialRoot}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 2.0f);
            }
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    static Material GetOrCreateEmissiveMaterial(string name, Color color, Color emissionColor, Texture2D tex = null)
    {
        string path = $"{MaterialRoot}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            if (tex != null) mat.mainTexture = tex;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissionColor);
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    static Color HexColor(string hex, float alpha = 1f)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color color))
        {
            color.a = alpha;
            return color;
        }
        return Color.white;
    }

    static void EnsureAnimatorController()
    {
        SetupPlayerAnimator.Setup();
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
        root.AddComponent<EchoSpectralTrail>();
        root.AddComponent<EchoTemporalVisual>();
        root.AddComponent<PlayerLocomotionAnimator>();
        root.AddComponent<PlayerAnimationRuntimeBootstrap>();
        root.AddComponent<CharacterPush>(); // Physics push

        GameObject visualRoot = new GameObject("Visual");
        visualRoot.transform.SetParent(root.transform, false);
        CreateCapsuleVisual(visualRoot.transform, true);

        PrefabUtility.SaveAsPrefabAsset(root, EchoPrefabPath);
        Object.DestroyImmediate(root);
    }

    static void EnsurePlayableMegakitPrefabPackage()
    {
        EnsureFolder(LevelKitRoot);
        EnsureFolder(PlayableMegakitRoot);

        // Standard Platforms
        SavePlayableMegakitPrefab("PF_Platform_Small", SciFiPlatformMetal, new Vector3(4f, 0.6f, 4f), _floorMat, "WalkableDeck", true, false);
        SavePlayableMegakitPrefab("PF_Platform_Medium", SciFiPlatformMetal, new Vector3(8f, 0.6f, 8f), _floorMat, "WalkableDeck", true, false);
        SavePlayableMegakitPrefab("PF_Platform_Large", SciFiPlatformMetal, new Vector3(14f, 0.6f, 10f), _floorMat, "WalkableDeck", true, false);

        // Bridges
        SavePlayableMegakitPrefab("PF_Bridge_01", SciFiPlatformSimple, new Vector3(4f, 0.55f, 8f), _bridgeMat, "WalkableBridge", true, false);
        SavePlayableMegakitPrefab("PF_Bridge_02", SciFiPlatformSimple, new Vector3(6f, 0.55f, 14f), _bridgeMat, "WalkableBridge", true, false);

        // Towers
        SavePlayableMegakitPrefab("PF_Tower_Short", SciFiColumnSimple, new Vector3(2f, 6f, 2f), _bridgeMat, "Tower", false, true);
        SavePlayableMegakitPrefab("PF_Tower_Tall", SciFiColumnLarge, new Vector3(3f, 12f, 3f), _archMat, "Tower", false, true);

        // Special items
        SaveMemoryGatePrefab();
        SaveEchoShrinePrefab();
        SaveObservationDeckPrefab();

        // Floating Islands
        SavePlayableMegakitPrefab("PF_FloatingIsland_Small", SciFiPlatformMetal, new Vector3(5f, 3f, 5f), _floorMat, "FloatingIsland", true, false);
        SavePlayableMegakitPrefab("PF_FloatingIsland_Medium", SciFiPlatformMetal, new Vector3(10f, 4f, 10f), _floorMat, "FloatingIsland", true, false);
        SavePlayableMegakitPrefab("PF_FloatingIsland_Large", SciFiPlatformMetal, new Vector3(16f, 5f, 16f), _floorMat, "FloatingIsland", true, false);

        // Distant structures
        SavePlayableMegakitPrefab("PF_DistantStructure_A", SciFiColumnRound, new Vector3(12f, 48f, 12f), _archMat, "DistantBackdrop", false, false);
        SavePlayableMegakitPrefab("PF_DistantStructure_B", SciFiColumnAstra, new Vector3(24f, 24f, 4f), _goalMat, "DistantBackdrop", false, false);
        SavePlayableMegakitPrefab("PF_DistantStructure_C", SciFiPlatformMetal, new Vector3(16f, 32f, 16f), _floorMat, "DistantBackdrop", false, false);

        // Walls
        SavePlayableMegakitPrefab("PF_AncientWall_A", SciFiWallAstra, new Vector3(1f, 8f, 12f), _floorMat, "AncientWall", false, true);
        SavePlayableMegakitPrefab("PF_AncientWall_B", SciFiWallBand, new Vector3(1.2f, 10f, 16f), _floorMat, "AncientWall", false, true);
        SavePlayableMegakitPrefab("PF_AncientWall_C", SciFiWallWindow, new Vector3(1.5f, 12f, 24f), _floorMat, "AncientWall", false, true);

        // Kinetic blocks
        SaveKineticBlockPrefab("PF_KineticBlock_Small", new Vector3(1.2f, 1.2f, 1.2f), 1.0f, _bridgeMat);
        SaveKineticBlockPrefab("PF_KineticBlock_Medium", new Vector3(2.0f, 2.0f, 2.0f), 5.0f, _floorMat);
        SaveKineticBlockPrefab("PF_KineticBlock_Large", new Vector3(3.2f, 3.2f, 3.2f), 20.0f, _doorMat);
    }

    static void SavePlayableMegakitPrefab(string prefabName, string modelPath, Vector3 footprint, Material material, string role, bool walkable, bool cameraOccluder, bool pressurePlate = false)
    {
        string path = $"{PlayableMegakitRoot}/{prefabName}.prefab";
        GameObject root = new GameObject(prefabName);
        root.layer = GroundLayer;
        root.isStatic = true;

        if (walkable)
        {
            BoxCollider colliderRef = root.AddComponent<BoxCollider>();
            colliderRef.size = Vector3.one;
            colliderRef.center = Vector3.zero;
            root.transform.localScale = footprint;
        }

        if (pressurePlate)
        {
            BoxCollider trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2f, 0.12f, 2f);
            PressurePlate plate = root.AddComponent<PressurePlate>();
            plate.ConfigureAcceptedActors(false, true, true);
            root.AddComponent<PressurePlateAlignment>();
        }

        GameObject visual = CreateMegakitVisual(modelPath, "VisualModel", root.transform, material);
        if (walkable) visual.transform.localScale = Vector3.one;
        else visual.transform.localScale = footprint;

        ConfigureLevelKitPiece(root, prefabName, role, walkable, cameraOccluder, footprint, new Vector3(footprint.x, Mathf.Max(2.2f, footprint.y + 2f), footprint.z));
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    static void SaveKineticBlockPrefab(string name, Vector3 size, float mass, Material mat)
    {
        string path = $"{PlayableMegakitRoot}/{name}.prefab";
        GameObject root = new GameObject(name);
        root.layer = GroundLayer;

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.size = Vector3.one;

        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        root.AddComponent<KineticPushableBlock>();
        root.transform.localScale = size;

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one;
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        if (mat != null) visual.GetComponent<MeshRenderer>().sharedMaterial = mat;
        visual.AddComponent<KenneyTiling>();

        ConfigureLevelKitPiece(root, name, "KineticBlock", false, true, size, size);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    static void SaveMemoryGatePrefab()
    {
        string path = $"{PlayableMegakitRoot}/PF_MemoryGate.prefab";
        GameObject root = new GameObject("PF_MemoryGate");
        root.layer = GroundLayer;

        GameObject frameVisual = CreateMegakitVisual(SciFiDoorFrame, "Frame", root.transform, _doorMat);
        frameVisual.transform.localScale = Vector3.one;

        GameObject doorVisual = CreateMegakitVisual(SciFiDoorDarkMetal, "Door", root.transform, _doorMat);
        doorVisual.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        BoxCollider col = doorVisual.AddComponent<BoxCollider>();
        col.size = new Vector3(1f, 1f, 1f);

        DoorController door = doorVisual.AddComponent<DoorController>();
        door.latchOpen = false;

        ConfigureLevelKitPiece(root, "PF_MemoryGate", "MemoryGate", false, true, new Vector3(8f, 4.5f, 0.55f), new Vector3(8f, 4.5f, 0.55f));
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    static void SaveEchoShrinePrefab()
    {
        string path = $"{PlayableMegakitRoot}/PF_EchoShrine.prefab";
        GameObject root = new GameObject("PF_EchoShrine");
        root.layer = GroundLayer;

        GameObject basePlat = CreateMegakitVisual(SciFiPlatformCenter, "Base", root.transform, _plateMat);
        basePlat.transform.localScale = Vector3.one;

        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(3.8f, 4.0f, 3.8f);
        trigger.center = new Vector3(0f, 2.0f, 0f);

        EchoKineticZone zone = root.AddComponent<EchoKineticZone>();
        SetSerializedValue(zone, "role", EchoKineticRole.TimingActivator);
        SetSerializedValue(zone, "requireEcho", true);

        ConfigureLevelKitPiece(root, "PF_EchoShrine", "EchoShrine", true, false, new Vector3(4f, 0.20f, 4f), new Vector3(4f, 4.0f, 4f));
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    static void SaveObservationDeckPrefab()
    {
        string path = $"{PlayableMegakitRoot}/PF_ObservationDeck.prefab";
        GameObject root = new GameObject("PF_ObservationDeck");
        root.layer = GroundLayer;

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.size = Vector3.one;

        GameObject deckVisual = CreateMegakitVisual(SciFiPlatformMetal, "DeckVisual", root.transform, _floorMat);
        deckVisual.transform.localScale = Vector3.one;

        GameObject railBack = CreateMegakitVisual(SciFiPropRail, "RailBack", root.transform, _bridgeMat);
        railBack.transform.localPosition = new Vector3(0f, 0.3f, -0.45f);

        GameObject railLeft = CreateMegakitVisual(SciFiPropRail, "RailLeft", root.transform, _bridgeMat);
        railLeft.transform.localPosition = new Vector3(-0.45f, 0.3f, 0f);
        railLeft.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

        GameObject railRight = CreateMegakitVisual(SciFiPropRail, "RailRight", root.transform, _bridgeMat);
        railRight.transform.localPosition = new Vector3(0.45f, 0.3f, 0f);
        railRight.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

        ConfigureLevelKitPiece(root, "PF_ObservationDeck", "ObservationDeck", true, false, new Vector3(6f, 0.6f, 4f), new Vector3(6f, 2.5f, 4f));
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    // === NEW KINETIC AND GHOST BRIDGES HELPERS ===
    static GhostBridge CreateGhostBridge(string name, Vector3 position, Vector3 scale, PuzzleSignal signal, Transform parent)
    {
        GameObject bridge = Instantiate3DModel(SciFiPlatformSimple, name, position, scale, Quaternion.identity, parent, _bridgeMat);
        ConfigureLevelKitPiece(bridge, name, "GhostBridge", true, false, scale, new Vector3(scale.x, 2.4f, scale.z));
        bridge.AddComponent<KenneyTiling>();

        GhostBridge gb = bridge.AddComponent<GhostBridge>();
        gb.Configure(signal, new Color(0f, 0.9f, 1f, 0.85f), new Color(0f, 0.9f, 1f, 0.08f));
        return gb;
    }

    static MemoryPlatform CreateMemoryPlatform(string name, Vector3 anchorPosition, Vector3 scale, PressurePlate[] plates, Vector3[] localPositions, Transform parent)
    {
        GameObject anchor = new GameObject(name + "_Anchor");
        anchor.transform.SetParent(parent, false);
        anchor.transform.position = anchorPosition;

        GameObject platObj = Instantiate3DModel(SciFiPlatformMetal, name, Vector3.zero, scale, Quaternion.identity, anchor.transform, _floorMat);
        ConfigureLevelKitPiece(platObj, name, "MemoryPlatform", true, false, scale, new Vector3(scale.x, 2.4f, scale.z));
        platObj.AddComponent<KenneyTiling>();

        MemoryPlatform mp = platObj.AddComponent<MemoryPlatform>();
        mp.plates = plates;
        mp.localPositions = localPositions;
        mp.travelSpeed = 3f;
        return mp;
    }

    static GameObject SpawnPushableBlock(string name, Vector3 position, Vector3 size, float mass, Transform parent)
    {
        string suffix = mass <= 2.5f ? "Small" : (mass <= 8.5f ? "Medium" : "Large");
        string prefabPath = $"{PlayableMegakitRoot}/PF_KineticBlock_{suffix}.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject block;
        if (prefab != null)
        {
            block = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.position = position;
            block.transform.localScale = size;
        }
        else
        {
            block = new GameObject(name);
            block.transform.SetParent(parent, false);
            block.transform.position = position;
            block.transform.localScale = size;
            block.layer = GroundLayer;

            BoxCollider col = block.AddComponent<BoxCollider>();
            col.size = Vector3.one;

            Rigidbody rb = block.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            block.AddComponent<KineticPushableBlock>();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(block.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one;
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            if (_bridgeMat != null) visual.GetComponent<MeshRenderer>().sharedMaterial = _bridgeMat;
            visual.AddComponent<KenneyTiling>();
        }

        return block;
    }

    static GameObject CreateMegakitVisual(string modelPath, string name, Transform parent, Material material)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        GameObject visual = null;
        if (prefab != null)
        {
            visual = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            visual.name = name;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            Collider[] childColliders = visual.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < childColliders.Length; i++) Object.DestroyImmediate(childColliders[i]);
            ApplyMaterialOverride(visual, material);
            return visual;
        }
        visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = name;
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        if (material != null) visual.GetComponent<MeshRenderer>().sharedMaterial = material;
        return visual;
    }

    static GameObject CreateCapsuleVisual(Transform parent, bool useEchoMaterial)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3D Models/lowpoly-character-freerigged-/source/LowPolyCharacterModel/FBX/LowPolyCharacter.fbx");
        GameObject visual;
        if (prefab != null)
        {
            GameObject scaler = new GameObject(useEchoMaterial ? "EchoScaler" : "PlayerScaler");
            scaler.transform.SetParent(parent, false);
            scaler.transform.localScale = Vector3.one;

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
                if (useEchoMaterial)
                {
                    Material[] mats = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < mats.Length; i++) mats[i] = _echoMat;
                    r.sharedMaterials = mats;
                }
            }

            Animator anim = visual.GetComponent<Animator>();
            if (anim == null) anim = visual.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(AnimatorControllerPath);
            Avatar avatar = LoadAvatarFromCharacterModel();
            if (avatar != null && avatar.isValid) anim.avatar = avatar;
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

    static Avatar LoadAvatarFromCharacterModel()
    {
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3D Models/lowpoly-character-freerigged-/source/LowPolyCharacterModel/FBX/LowPolyCharacter.fbx");
        if (fbx != null)
        {
            string path = AssetDatabase.GetAssetPath(fbx);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null) return importer.sourceAvatar;
        }
        return null;
    }

    static Material LoadSmokeMaterial()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(SmokeDarkMaterialPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Particles/Standard Unlit"));
            mat.color = new Color(0.1f, 0.1f, 0.15f, 0.15f);
        }
        return mat;
    }

    static GameObject CreateRoot(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go == null) go = new GameObject(name);
        return go;
    }

    static GameObject MakePlatform(string name, Vector3 position, Vector3 scale, Transform parent, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = scale;
        go.layer = GroundLayer;
        if (material != null) go.GetComponent<MeshRenderer>().sharedMaterial = material;
        go.AddComponent<KenneyTiling>();
        return go;
    }

    static GameObject MakeVisualSilhouette(string name, Vector3 position, Vector3 scale, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        if (_archMat != null) go.GetComponent<MeshRenderer>().sharedMaterial = _archMat;
        return go;
    }

    static GameObject MakeBrutalistBlock(string name, Vector3 position, Vector3 scale, Quaternion rotation, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.rotation = rotation;
        go.transform.localScale = scale;
        go.layer = GroundLayer;
        if (_archMat != null) go.GetComponent<MeshRenderer>().sharedMaterial = _archMat;
        go.AddComponent<KenneyTiling>();
        return go;
    }

    static void SetSerializedValue(Component component, string propertyName, object value)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null) return;
        switch (value)
        {
            case int intValue: property.intValue = intValue; break;
            case float floatValue: property.floatValue = floatValue; break;
            case bool boolValue: property.boolValue = boolValue; break;
            case string stringValue: property.stringValue = stringValue; break;
            case Color colorValue: property.colorValue = colorValue; break;
            case Object objectValue: property.objectReferenceValue = objectValue; break;
            case Vector3 vectorValue: property.vector3Value = vectorValue; break;
            case System.Enum enumValue: property.enumValueIndex = System.Convert.ToInt32(enumValue); break;
        }
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SaveScene(Scene scene, string sceneName)
    {
        EditorSceneManager.SaveScene(scene, $"{SceneRoot}/{sceneName}.unity");
    }

    static void UpdateBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        string[] levels = { "MainMenu", "Level_01", "Level_02", "Level_03", "Level_04", "Level_05", "Level_06", "Level_07", "Level_08", "Level_09", "Level_10", "Level_11", "Level_12", "Level_13", "Level_14", "Level_15" };
        foreach (string lvl in levels)
        {
            string path = $"{SceneRoot}/{lvl}.unity";
            if (File.Exists(path)) scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static void ApplyMaterialOverride(GameObject obj, Material mat)
    {
        if (obj == null || mat == null) return;
        MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].sharedMaterial = mat;
        }
    }

    static void ConfigureLevelKitPiece(GameObject obj, string id, string role, bool walkable, bool cameraOccluder, Vector3 footprint, Vector3 clearance)
    {
        if (obj == null) return;
        LevelKitPiece piece = obj.GetComponent<LevelKitPiece>();
        if (piece == null) piece = obj.AddComponent<LevelKitPiece>();
        piece.Configure(id, role, walkable, cameraOccluder, footprint, clearance);
    }

    static PanelSettings GetOrCreatePanelSettings()
    {
        PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI/PanelSettings.asset");
        if (panelSettings == null)
        {
            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            AssetDatabase.CreateAsset(panelSettings, "Assets/UI/PanelSettings.asset");
        }
        return panelSettings;
    }

    static PuzzleSignal CreatePuzzleSignal(string name, string displayName, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        PuzzleSignal signal = go.AddComponent<PuzzleSignal>();
        signal.Configure(displayName, true, false);
        return signal;
    }

    static EchoConflictTrap CreateConflictTrap(string name, Vector3 position, Vector3 size, Transform parent, DoorController[] doorsToClose, DoorController[] doorsToOpen, PuzzleSignal signal)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = size;

        GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Cube);
        warning.name = "WarningRotor";
        warning.transform.SetParent(root.transform, false);
        warning.transform.localScale = new Vector3(size.x, 0.12f, 0.25f);
        Object.DestroyImmediate(warning.GetComponent<Collider>());
        warning.GetComponent<MeshRenderer>().sharedMaterial = _doorMat;

        EchoConflictTrap trap = root.AddComponent<EchoConflictTrap>();
        trap.Configure(doorsToClose, doorsToOpen, new[] { warning.transform }, signal);
        return trap;
    }

    static GoalTrigger CreateGoalTrigger(Transform parent, Component triggerComponent, string toastName)
    {
        GameObject go = new GameObject("Trigger_" + toastName);
        go.transform.SetParent(parent, false);
        GoalTrigger trigger = go.AddComponent<GoalTrigger>();
        if (triggerComponent is PressurePlate plate)
        {
            SetSerializedValue(trigger, "targetPlate", plate);
            SetSerializedValue(trigger, "requirePlatePressed", true);
        }
        else if (triggerComponent is PuzzleSignal signal)
        {
            SetSerializedValue(trigger, "targetSignal", signal);
            SetSerializedValue(trigger, "requireSignalSatisfied", true);
        }
        return trigger;
    }

    static LevelGoal CreateLevelGoal(Transform parent, string objective, string readyText, string successToast, LevelExit exit, params Component[] components)
    {
        GameObject go = new GameObject("LevelGoal");
        go.transform.SetParent(parent, false);
        go.transform.position = exit.transform.position;

        List<GoalTrigger> triggers = new List<GoalTrigger>();
        foreach (Component comp in components)
        {
            if (comp != null) triggers.Add(CreateGoalTrigger(go.transform, comp, comp.name));
        }

        LevelGoal goal = go.AddComponent<LevelGoal>();
        SetSerializedValue(goal, "objectiveText", objective);
        SetSerializedValue(goal, "readyPrompt", readyText);
        SetSerializedValue(goal, "completionToast", successToast);
        SetSerializedValue(goal, "requiredTriggerCount", triggers.Count);
        SetSerializedValue(goal, "autoCollectChildTriggers", true);
        return goal;
    }

    static void SpawnPointLight(string name, Vector3 position, Color color, float intensity, float range, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        Light lt = go.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = color;
        lt.intensity = intensity;
        lt.range = range;
        lt.shadows = LightShadows.None;
    }

    static GravityZone CreateGravityZone(string name, Vector3 position, Vector3 size, Vector3 gravityDirection, float gravityStrength, int priority, Transform parent)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;

        GravityZone zone = root.AddComponent<GravityZone>();
        SetSerializedValue(zone, "gravityDirection", gravityDirection);
        SetSerializedValue(zone, "gravityStrength", gravityStrength);
        SetSerializedValue(zone, "priority", priority);

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = size;
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.6f, 0.2f, 0.8f, 0.15f);
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        visual.GetComponent<MeshRenderer>().sharedMaterial = mat;

        return zone;
    }

    static LevelGoal CreateSignalGoal(Transform parent, string objective, string readyText, string successToast, LevelExit exit, params PuzzleSignal[] signals)
    {
        GameObject go = new GameObject("LevelGoal");
        go.transform.SetParent(parent, false);
        go.transform.position = exit.transform.position;

        List<GoalTrigger> triggers = new List<GoalTrigger>();
        foreach (PuzzleSignal sig in signals)
        {
            if (sig != null) triggers.Add(CreateGoalTrigger(go.transform, sig, sig.name));
        }

        LevelGoal goal = go.AddComponent<LevelGoal>();
        SetSerializedValue(goal, "objectiveText", objective);
        SetSerializedValue(goal, "readyPrompt", readyText);
        SetSerializedValue(goal, "completionToast", successToast);
        SetSerializedValue(goal, "requiredTriggerCount", triggers.Count);
        SetSerializedValue(goal, "autoCollectChildTriggers", true);
        return goal;
    }

    static void CreateMotorPlatform(string name, Vector3 position, Vector3 scale, Vector3 rotSpeed, Vector3 moveDir, Vector3 moveAmp, float speed, float phase, Transform parent, Material mat)
    {
        GameObject platform = Instantiate3DModel(SciFiPlatformMetal, name, position, scale, Quaternion.identity, parent, mat);
        platform.AddComponent<KenneyTiling>();
        DynamicTransformMotor motor = platform.AddComponent<DynamicTransformMotor>();
        SetSerializedValue(motor, "rotationSpeed", rotSpeed);
        SetSerializedValue(motor, "translationDirection", moveDir);
        SetSerializedValue(motor, "translationAmplitude", moveAmp);
        SetSerializedValue(motor, "frequency", speed);
        SetSerializedValue(motor, "phaseShift", phase);
    }

    static GameObject CreateHazardField(string name, Vector3 position, Vector3 size, Transform parent, PuzzleSignal signal)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;

        // Create visual cube
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = size;
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.16f, 0.08f, 0.5f);
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        visual.GetComponent<MeshRenderer>().sharedMaterial = mat;

        // Create light
        GameObject lightObj = new GameObject("Light");
        lightObj.transform.SetParent(root.transform, false);
        lightObj.transform.localPosition = Vector3.zero;
        Light lt = lightObj.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.16f, 0.08f, 1f);
        lt.intensity = 4f;
        lt.range = Mathf.Max(size.x, size.y, size.z) * 2f;
        lt.shadows = LightShadows.None;

        EchoShieldField hazard = root.AddComponent<EchoShieldField>();
        hazard.Configure(new Renderer[] { visual.GetComponent<MeshRenderer>() }, lt, signal);

        return root;
    }

    static LevelExit CreateLevelExit(Vector3 position, Transform parent, string targetScene, string congratulationsMessage = "")
    {
        GameObject root = new GameObject("LevelExit");
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(2f, 3f, 0.5f);

        // Add a visual frame/door or simple visual indicator
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        visual.transform.localScale = new Vector3(2f, 3f, 0.2f);
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        if (_goalMat != null) visual.GetComponent<MeshRenderer>().sharedMaterial = _goalMat;

        // Also let's add two side pillars for extra sci-fi aesthetics
        GameObject leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftPillar.name = "LeftPillar";
        leftPillar.transform.SetParent(root.transform, false);
        leftPillar.transform.localPosition = new Vector3(-1.2f, 1.5f, 0f);
        leftPillar.transform.localScale = new Vector3(0.4f, 3f, 0.4f);
        Object.DestroyImmediate(leftPillar.GetComponent<Collider>());
        if (_archMat != null) leftPillar.GetComponent<MeshRenderer>().sharedMaterial = _archMat;

        GameObject rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightPillar.name = "RightPillar";
        rightPillar.transform.SetParent(root.transform, false);
        rightPillar.transform.localPosition = new Vector3(1.2f, 1.5f, 0f);
        rightPillar.transform.localScale = new Vector3(0.4f, 3f, 0.4f);
        Object.DestroyImmediate(rightPillar.GetComponent<Collider>());
        if (_archMat != null) rightPillar.GetComponent<MeshRenderer>().sharedMaterial = _archMat;

        LevelExit exit = root.AddComponent<LevelExit>();
        exit.nextSceneName = targetScene;
        exit.loadNextBuildIndex = false; // we specify the exact name
        if (!string.IsNullOrEmpty(congratulationsMessage))
        {
            SetSerializedValue(exit, "completionToast", congratulationsMessage);
        }

        return exit;
    }
}
"""

with open(builder_path, "w", encoding="utf-8") as f:
    f.write(csharp_code)

print("EchoesProductionBuilder.cs rewritten successfully!")
