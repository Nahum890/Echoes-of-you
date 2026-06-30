using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static partial class EchoesProductionBuilder
{
    static void BuildLevel11()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_11";

        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(HexColor("584A52"), 0.005f, HexColor("D4CCD0"));
        SpawnDirectionalLight();

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.5f, 10f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -2f));

        GameObject midPlat = MakePlatform("MidPlatform", new Vector3(0f, 0f, 20f), new Vector3(10f, 0.5f, 10f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 40f), new Vector3(10f, 0.5f, 10f), env, _floorMat);

        PressurePlate plate1 = CreatePlateOnPlatform("Plate1", startPlat, new Vector3(0f, 0f, 0f), mech, false);
        PressurePlate plate2 = CreatePlateOnPlatform("Plate2", midPlat, new Vector3(0f, 0f, 0f), mech, false);

        PuzzleSignal signal1 = CreatePuzzleSignal("Signal_Ghost1", "Puente Espectral 1", mech);
        signal1.Configure("Puente Espectral 1", false, false);
        CreateCondition("Cond_Ghost1", PuzzleCondition.ConditionType.AllPlatesSimultaneous, new[] { plate1 }, signal1, mech);
        CreateGhostBridge("GhostBridge1", new Vector3(0f, 0f, 10f), new Vector3(4f, 0.5f, 10f), mech, signal1);

        PuzzleSignal signal2 = CreatePuzzleSignal("Signal_Ghost2", "Puente Espectral 2", mech);
        signal2.Configure("Puente Espectral 2", false, false);
        CreateCondition("Cond_Ghost2", PuzzleCondition.ConditionType.AllPlatesSimultaneous, new[] { plate2 }, signal2, mech);
        CreateGhostBridge("GhostBridge2", new Vector3(0f, 0f, 30f), new Vector3(4f, 0.5f, 10f), mech, signal2);

        DoorController exitDoor = CreateDoor("ExitDoor", new Vector3(0f, 1.5f, 36f), new Vector3(6f, 3f, 0.5f), mech, new[] { plate1, plate2 });
        exitDoor.latchOpen = true;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 40f), mech, "Level_12");
        CreateLevelGoal(mech, "Solidifica los puentes fantasmas usando tus ecos para alcanzar y abrir la compuerta de salida.", "El archivo del puente espectral se alinea.", "Puente espectral cruzado.", exit, plate1, plate2);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, -3f), true, 3, 20f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 4f, -11f));

        SpawnPuzzleIntent(mech, 2, 4, true, true, true, 40f, "Level 11: introduction to GhostBridge requiring multiple recorded echoes to bridge gaps sequentially.");

        SpawnPointLight("Light_Plate1", new Vector3(0f, 3f, 0f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_Plate2", new Vector3(0f, 3f, 20f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 10f, 40f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Solidifica los puentes fantasmas usando tus ecos para alcanzar y abrir la compuerta de salida.", "El n├║cleo modular se alinea.", "Puente espectral cruzado.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 20f), 24f, 45f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 40f);

        // School dressing — Rose walls (Depresión)
        SpawnBarrierWall("SchoolWallL", new Vector3(-8f, 6f, 20f), new Vector3(0.5f, 12f, 46f), env, _wallRoseMat);
        SpawnBarrierWall("SchoolWallR", new Vector3(8f, 6f, 20f), new Vector3(0.5f, 12f, 46f), env, _wallRoseMat);
        TryInstantiateAssetByName("locker", env, new Vector3(-4f, 0.3f, -4f));
        TryInstantiateAssetByName("desk", env, new Vector3(-3f, 0.3f, -2f));
        TryInstantiateAssetByName("chairDesk", env, new Vector3(-2f, 0.3f, -2f));
        TryInstantiateAssetByName("pipe", env, new Vector3(-7.5f, 5f, 10f));
        TryInstantiateAssetByName("pipe", env, new Vector3(7.5f, 5f, 30f));

        SaveScene(scene, "Level_11");
    }

    static void BuildLevel12()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_12";

        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(HexColor("584850"), 0.005f, HexColor("D4D0D2"));
        SpawnDirectionalLight();

        GameObject leftFloor = MakePlatform("LeftFloor", new Vector3(-2.75f, 0f, 0f), new Vector3(8.5f, 0.5f, 14f), env, _floorMat);
        GameObject chamberFloor = MakePlatform("ChamberFloor", new Vector3(4.25f, 0f, 3.5f), new Vector3(5.5f, 0.5f, 7f), env, _floorMat);
        GameObject bottomFloor = MakePlatform("BottomFloor", new Vector3(4.25f, 0f, -6.25f), new Vector3(5.5f, 0.5f, 1.5f), env, _floorMat);
        GameObject midRightFloor = MakePlatform("MidRightFloor", new Vector3(4.25f, 0f, -1.25f), new Vector3(5.5f, 0.5f, 2.5f), env, _floorMat);
        GameObject elevatorRightSide = MakePlatform("ElevatorRightSide", new Vector3(5.75f, 0f, -4f), new Vector3(2.5f, 0.5f, 3f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -3f));

        GameObject highPlat = MakePlatform("HighPlatform", new Vector3(3f, 6f, -12f), new Vector3(6f, 0.5f, 6f), env, _floorMat);

        PressurePlate plate1 = CreatePlateOnPlatform("Plate1", leftFloor, new Vector3(-1.25f, 0f, 2f), mech, false);
        DoorController chamberGate = CreateDoor("ChamberGate", new Vector3(3f, 1.5f, 2f), new Vector3(0.5f, 3f, 4f), mech, new[] { plate1 });
        chamberGate.latchOpen = true;

        GameObject pushBlock = CreatePushableBlock("PushableBlock", new Vector3(5f, 1f, 2f), new Vector3(1.5f, 1.5f, 1.5f), mech, _floorMat);

        PressurePlate plate2 = CreatePlateOnPlatform("Plate2", leftFloor, new Vector3(-1.25f, 0f, -4f), mech, false);

        TimedMovingPlatform elevator = CreateBridge("Elevator", new Vector3(3f, 0f, -4f), Vector3.zero, new Vector3(0f, 6f, 0f), new Vector3(3f, 0.5f, 3f), plate2, mech);

        LevelExit exit = CreateLevelExit(new Vector3(3f, 7.25f, -12f), mech, "Level_13");
        CreateLevelGoal(mech, "Usa la inercia de tus ecos para abrir la c├ímara del bloque, y luego empujar el bloque sobre la placa del ascensor.", "La inercia cinem├ítica del bloque se registra.", "Ascenso completado.", exit, plate2);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 3, 18f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 4f, -11f));

        SpawnPuzzleIntent(mech, 2, 4, true, true, true, 18f, "Level 12: pushable block mechanic requiring player to open chamber, push block, and record echo pushing block onto elevator button.");

        SpawnPointLight("Light_Plate1", new Vector3(-4f, 3f, 2f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_Plate2", new Vector3(-4f, 3f, -4f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_Exit", new Vector3(3f, 10f, -12f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Usa la inercia de tus ecos para abrir la c├ímara del bloque, y luego empujar el bloque sobre la placa del ascensor.", "La inercia cinem├ítica del bloque se registra.", "Ascenso completado.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 0f), 20f, 20f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, -12f, 12f);

        // School dressing — Rose walls (Aceptación)
        SpawnBarrierWall("SchoolWallL", new Vector3(-10f, 6f, -2f), new Vector3(0.5f, 12f, 24f), env, _wallRoseMat);
        SpawnBarrierWall("SchoolWallR", new Vector3(10f, 6f, -2f), new Vector3(0.5f, 12f, 24f), env, _wallRoseMat);
        TryInstantiateAssetByName("locker", env, new Vector3(-6f, 0.3f, -2f));
        TryInstantiateAssetByName("desk", env, new Vector3(6f, 0.3f, -2f));
        TryInstantiateAssetByName("chairDesk", env, new Vector3(6f, 0.3f, -3f));
        TryInstantiateAssetByName("pipe", env, new Vector3(-9.5f, 5f, -4f));
        TryInstantiateAssetByName("pipe", env, new Vector3(9.5f, 5f, 4f));

        SaveScene(scene, "Level_12");
    }

    static void BuildLevel13()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_13";

        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(HexColor("584850"), 0.005f, HexColor("D4D0D2"));
        SpawnDirectionalLight();

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(8f, 0.5f, 8f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -2f));

        GameObject chamberA = MakePlatform("ChamberA", new Vector3(-12f, 0f, 12f), new Vector3(6f, 0.5f, 6f), env, _floorMat);
        GameObject chamberB = MakePlatform("ChamberB", new Vector3(12f, 0f, 12f), new Vector3(6f, 0.5f, 6f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 6f, 24f), new Vector3(8f, 0.5f, 8f), env, _floorMat);

        PressurePlate plateA = CreatePlateOnPlatform("PlateA", startPlat, new Vector3(-2f, 0f, -2f), mech, false);
        PressurePlate plateB = CreatePlateOnPlatform("PlateB", chamberA, new Vector3(0f, 0f, 1f), mech, false);
        PressurePlate plateC = CreatePlateOnPlatform("PlateC", chamberB, new Vector3(0f, 0f, 1f), mech, false);

        PressurePlate[] plates = new[] { plateA, plateB, plateC };
        Vector3[] positions = new[] {
            new Vector3(-12f, 0f, 4f),
            new Vector3(12f, 0f, 4f),
            new Vector3(0f, 6f, 16f)
        };
        MemoryPlatform memPlatform = CreateMemoryPlatform("MemoryPlatform", new Vector3(0f, 0f, 8f), new Vector3(4f, 0.5f, 4f), plates, positions, 5f, mech, _bridgeMat);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 7.25f, 26f), mech, "Level_14");
        CreateLevelGoal(mech, "Secuencia las coordenadas de la plataforma de memoria usando tus ecos para llegar a la salida.", "La memoria de la plataforma guarda las coordenadas.", "Ruta completada.", exit, plateC);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 3, 20f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 4f, -11f));

        SpawnPuzzleIntent(mech, 3, 5, true, true, true, 24f, "Level 13: MemoryPlatform introduction. Player routes platform through side chambers sequentially using three echoes.");

        SpawnPointLight("Light_PlateA", new Vector3(-2f, 3f, -2f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_PlateB", new Vector3(-12f, 3f, 13f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_PlateC", new Vector3(12f, 3f, 13f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 10f, 24f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        // School dressing — Rose walls (Verdad stage)
        SpawnBarrierWall("SchoolWallLeft", new Vector3(-15f, 6f, 12f), new Vector3(0.5f, 12f, 32f), env, _wallRoseMat);
        SpawnBarrierWall("SchoolWallRight", new Vector3(15f, 6f, 12f), new Vector3(0.5f, 12f, 32f), env, _wallRoseMat);
        TryInstantiateAssetByName("locker", env, new Vector3(-3f, 0.3f, 2f));
        TryInstantiateAssetByName("bookcaseOpen", env, new Vector3(3f, 0.3f, 2f));
        TryInstantiateAssetByName("desk", env, new Vector3(-10f, 0.3f, 11f));
        TryInstantiateAssetByName("chairDesk", env, new Vector3(-10f, 0.3f, 10f));
        TryInstantiateAssetByName("pipe", env, new Vector3(-14.5f, 5f, 10f));
        TryInstantiateAssetByName("pipe", env, new Vector3(14.5f, 5f, 15f));

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Secuencia las coordenadas de la plataforma de memoria usando tus ecos.", "Alineaci├│n de coordenadas espectrales activada.", "Ruta de la plataforma guardada.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 12f), 24f, 30f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MovingCity, 2f, 24f);

        SaveScene(scene, "Level_13");
    }

    static void BuildLevel14()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_14";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(HexColor("584852"), 0.004f, HexColor("D8D0D6"));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 14f), 26f, 36f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 14f), 26f, 36f, env);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -2f));

        MakePlatform("Floor_EchoSide", new Vector3(-4f, 0f, 8f), new Vector3(6f, 0.5f, 24f), env, _archMat);
        MakePlatform("Floor_PlayerSide", new Vector3(4f, 0f, 8f), new Vector3(6f, 0.5f, 24f), env, _floorMat);
        MakePlatform("SymmetryAxis", new Vector3(0f, 0.26f, 8f), new Vector3(0.15f, 0.1f, 24f), env, _goalMat);

        PressurePlate plateEcho = CreatePlate("Switch_EchoSide", new Vector3(-4f, 0.36f, 14f), mech);
        PressurePlate platePlayer = CreatePlate("Switch_PlayerSide", new Vector3(4f, 0.36f, 14f), mech);

        DoorController mirrorGate = CreateDoor(
            "MirrorGate",
            new Vector3(0f, 1.5f, 20f),
            new Vector3(10f, 3f, 0.5f),
            mech,
            new[] { plateEcho, platePlayer });
        mirrorGate.latchOpen = true;

        MakePlatform("ExitPlatform", new Vector3(0f, 0f, 26f), new Vector3(10f, 0.5f, 8f), env, _floorMat);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 30f), mech, "Level_15");
        CreateLevelGoal(mech, "", "", "", exit, plateEcho, platePlayer);

        GameObject preRecordedEchoMarker = new GameObject("PreRecordedEcho_Aiden_SETUP_REQUIRED");
        preRecordedEchoMarker.transform.SetParent(mech, false);
        preRecordedEchoMarker.transform.position = new Vector3(-4f, 1.1f, 2f);

        GameObject player = SpawnPlayer(new Vector3(4f, 1.1f, 2f), true, 0, 0f);
        SpawnGameplayCameraCustom(
            player.transform,
            new Vector3(0f, 5f, -16f),
            55f,
            mech);

        SpawnPointLight("Light_EchoSide", new Vector3(-4f, 4f, 12f), new Color(0.85f, 0.68f, 0.42f), 4f, 12f, env);
        SpawnPointLight("Light_PlayerSide", new Vector3(4f, 4f, 12f), new Color(0.3f, 0.65f, 1f), 4f, 12f, env);
        SpawnPointLight("Light_SwitchEcho", new Vector3(-4f, 3f, 14f), new Color(0.9f, 0.75f, 0.4f), 2f, 6f, env);
        SpawnPointLight("Light_SwitchPlayer", new Vector3(4f, 3f, 14f), new Color(0.25f, 0.7f, 1f), 2f, 6f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 4f, 30f), new Color(0.15f, 0.6f, 1f), 6f, 16f, env);

        // School dressing — Rose walls (Liberación stage)
        SpawnBarrierWall("SchoolWallLeft", new Vector3(-8f, 6f, 15f), new Vector3(0.5f, 12f, 32f), env, _wallRoseMat);
        SpawnBarrierWall("SchoolWallRight", new Vector3(8f, 6f, 15f), new Vector3(0.5f, 12f, 32f), env, _wallRoseMat);
        TryInstantiateAssetByName("locker", env, new Vector3(-6f, 0.3f, 4f));
        TryInstantiateAssetByName("locker", env, new Vector3(6f, 0.3f, 4f));
        TryInstantiateAssetByName("desk", env, new Vector3(6f, 0.3f, 12f));
        TryInstantiateAssetByName("chairDesk", env, new Vector3(6f, 0.3f, 10f));
        TryInstantiateAssetByName("pipe", env, new Vector3(-7.5f, 5f, 10f));
        TryInstantiateAssetByName("pipe", env, new Vector3(7.5f, 5f, 20f));

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "", "", "");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 14f), 18f, 28f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 30f);
        SpawnPuzzleIntent(mech, 2, 2, true, true, false, 14f,
            "PROTOTYPE C: inversion. Aiden follows the echo for the first time. No recording. Pre-recorded echo leads. Frontal camera on symmetry axis.");

        SaveScene(scene, "Level_14");
    }

    static void BuildLevel15()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_15";

        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(HexColor("584040"), 0.005f, HexColor("D8C8C8"));
        SpawnDirectionalLight();

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(8f, 0.5f, 10f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -2f));

        GameObject platform2 = MakePlatform("Platform2", new Vector3(0f, 0f, 20f), new Vector3(8f, 0.5f, 10f), env, _floorMat);
        GameObject platform3 = MakePlatform("Platform3", new Vector3(0f, 0f, 40f), new Vector3(8f, 0.5f, 12f), env, _floorMat);

        PressurePlate plate1 = CreatePlateOnPlatform("Plate1", startPlat, new Vector3(-3f, 0f, 3f), mech, false);
        DoorController gate1 = CreateDoor("Gate1", new Vector3(0f, 1.5f, 12f), new Vector3(6f, 3f, 0.5f), mech, new[] { plate1 });
        gate1.latchOpen = true;

        PressurePlate plate2 = CreatePlateOnPlatform("Plate2", platform2, new Vector3(3f, 0f, -2f), mech, false);
        DoorController gate2 = CreateDoor("Gate2", new Vector3(0f, 1.5f, 28f), new Vector3(6f, 3f, 0.5f), mech, new[] { plate2 });
        gate2.latchOpen = true;

        PressurePlate plate3 = CreatePlateOnPlatform("Plate3", platform2, new Vector3(-3f, 0f, 6f), mech, false);
        PuzzleSignal signalBridge15 = CreatePuzzleSignal("Signal_Bridge15", "Puente Final Alineado", mech);
        signalBridge15.Configure("Puente Final Alineado", false, false);
        CreateCondition("Cond_Bridge15", PuzzleCondition.ConditionType.AllPlatesSimultaneous, new[] { plate3 }, signalBridge15, mech);
        CreateGhostBridge("GhostBridge15", new Vector3(0f, 0f, 29.5f), new Vector3(4f, 0.5f, 9f), mech, signalBridge15);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 44f), mech, "MainMenu");
        CreateLevelGoal(mech, "┬íALERTA! El n├║cleo colapsa. Usa tus ecos al vuelo para abrir las compuertas y escapar.", "El colapso del n├║cleo es inminente.", "Sobreviviste a la simulaci├│n.", exit, plate1, plate2, plate3);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, -3f), true, 3, 24f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 4f, -11f));

        SpawnPuzzleIntent(mech, 3, 6, true, true, true, 44f, "Level 15: final chase escape sequence combining multiple fast plates, doors, and a ghost bridge.");

        SpawnPointLight("Light_Plate1", new Vector3(-3f, 3f, 3f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_Plate2", new Vector3(3f, 3f, 18f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_Plate3", new Vector3(-3f, 3f, 26f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 8f, 44f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        // School dressing — Rose walls (Integración stage)
        SpawnBarrierWall("SchoolWallLeft", new Vector3(-9f, 6f, 20f), new Vector3(0.5f, 12f, 56f), env, _wallRoseMat);
        SpawnBarrierWall("SchoolWallRight", new Vector3(9f, 6f, 20f), new Vector3(0.5f, 12f, 56f), env, _wallRoseMat);
        TryInstantiateAssetByName("locker", env, new Vector3(3f, 0.3f, 2f));
        TryInstantiateAssetByName("desk", env, new Vector3(-3f, 0.3f, 2f));
        TryInstantiateAssetByName("chairDesk", env, new Vector3(-2f, 0.3f, 2f));
        TryInstantiateAssetByName("bookcaseOpen", env, new Vector3(-3f, 0.3f, 20f));
        TryInstantiateAssetByName("pipe", env, new Vector3(-8.5f, 5f, 18f));
        TryInstantiateAssetByName("pipe", env, new Vector3(8.5f, 5f, 30f));

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "┬íHuye! El n├║cleo se colapsa. Usa tus ecos en movimiento para escapar de la disoluci├│n.", "COLAPSO CR├ìTICO. EVACUACI├ôN DE MEMORIA INICIADA.", "Simulaci├│n finalizada. Conexi├│n cerrada.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 20f), 24f, 45f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Chase, 2f, 44f, enableChase: true);

        SaveScene(scene, "Level_15");
    }
}
