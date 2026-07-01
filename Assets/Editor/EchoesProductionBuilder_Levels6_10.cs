using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static partial class EchoesProductionBuilder
{
    static void BuildLevel06()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_06";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(HexColor("485848"), 0.006f, HexColor("C8D0C8"));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 14f), 48f, 56f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 14f), 48f, 56f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, -2f), new Vector3(10f, 0.5f, 4f), env, _floorMat);
        GameObject mainChamberFloor = MakePlatform("MainChamberFloor", new Vector3(0f, 0f, 2f), new Vector3(10f, 0.5f, 4f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -1.5f));

        GameObject exitLedge = MakePlatform("ExitLedge", new Vector3(0f, 6f, 22f), new Vector3(6f, 0.5f, 6f), env, _floorMat);
        GameObject leftGravityFloor = MakePlatform("LeftGravityFloor", new Vector3(-6f, 4f, 11f), new Vector3(1f, 8f, 14f), env, _bridgeMat);

        CreateGravityZone("LeftWallGravity", new Vector3(-6f, 4f, 11f), new Vector3(1.2f, 8f, 14f), Vector3.left, 24f, 1, mech);

        PuzzleSignal blockSignal = CreatePuzzleSignal("Signal_Block", "Bloque Elevado", mech);
        CreateKineticBlock("PlatformBlock", new Vector3(0f, 0.25f, 11f), new Vector3(4f, 0.5f, 4f), new Vector3(-6f, 3.75f, 0f), new Vector3(1.2f, 4f, 4f), new Vector3(0f, 5.75f, 0f), mech, blockSignal, true, true, 3f);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 7.25f, 23.5f), mech, "Level_07");
        CreateSignalGoal(mech, "Entra en la zona de gravedad alterada en la pared izquierda y activa la plataforma.", "La gravedad es relativa en tu mente.", "Venciste a la perspectiva f├¡sica.", exit, blockSignal);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 10f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 5f, -11f));

        SpawnPuzzleIntent(mech, 0, 3, true, true, true, 18f, "Level 06: gravity alteration enabling horizontal wall-walking to raise central platform.");

        SpawnPointLight("Light_Wall", new Vector3(-4f, 6f, 11f), new Color(0.6f, 0.2f, 0.8f), 3f, 8f, env);
        SpawnPointLight("Light_Platform", new Vector3(0f, 3f, 11f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 9f, 23.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Usa la gravedad de la pared para activar el elevador del suelo.", "La mente no tiene arriba ni abajo.", "Elevaci├n completada.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 11f), 20f, 34f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.VerticalFall, 2f, 24f);

        // School dressing — Sage walls (Negociación)
        SpawnBarrierWall("SchoolWallL", new Vector3(-10f, 6f, 10f), new Vector3(0.5f, 12f, 28f), env, _wallSageMat);
        SpawnBarrierWall("SchoolWallR", new Vector3(8f, 6f, 10f), new Vector3(0.5f, 12f, 28f), env, _wallSageMat);
        TryInstantiateAssetByName("locker", env, new Vector3(-4f, 0.3f, -3f));
        TryInstantiateAssetByName("desk", env, new Vector3(-3f, 0.3f, -1f));
        TryInstantiateAssetByName("chairDesk", env, new Vector3(-2f, 0.3f, -1f));
        TryInstantiateAssetByName("pipe", env, new Vector3(-9.5f, 5f, 8f));
        TryInstantiateAssetByName("pipe", env, new Vector3(7.5f, 5f, 15f));

        SaveScene(scene, "Level_06");
    }

    static void BuildLevel07()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_07";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(HexColor("4A584A"), 0.005f, HexColor("CCD4CC"));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 16f), 28f, 38f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 16f), 28f, 38f, env);
        SpawnLevelLightingSettings(env, new Color(0.14f, 0.18f, 0.26f), 0.0055f);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -1f));

        MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.5f, 8f), env, _floorMat);
        MakePlatform("DecorativePlatA", new Vector3(-8f, 0f, 0f), new Vector3(2f, 0.5f, 2f), env, _floorMat);
        MakePlatform("DecorativePlatB", new Vector3(8f, 0f, 0f), new Vector3(2f, 0.5f, 2f), env, _floorMat);

        PressurePlate plateCyclic = CreatePlate("PlateCyclic", new Vector3(8f, 0.36f, 4f), mech);
        plateCyclic.autoReleaseTimer = 5f;
        CreateBridge(
            "CyclicBridge",
            new Vector3(0f, 0f, 14f),
            new Vector3(-12f, 0f, 0f),
            Vector3.zero,
            new Vector3(6f, 0.5f, 4f),
            plateCyclic,
            mech);

        PressurePlate plateGate = CreatePlate("PlateGate", new Vector3(-8f, 0.36f, 4f), mech);
        plateGate.autoReleaseTimer = 4f;
        DoorController timedGate = CreateDoor(
            "TimedGate",
            new Vector3(0f, 1.5f, 20f),
            new Vector3(8f, 3f, 0.5f),
            mech,
            new[] { plateGate });
        timedGate.latchOpen = false;

        MakePlatform("ExitPlatform", new Vector3(0f, 4f, 32f), new Vector3(10f, 0.5f, 8f), env, _floorMat);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 5.25f, 36f), mech, "Level_08");
        CreateLevelGoal(mech, "", "", "", exit, plateGate);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 1, 12f);
        SpawnGameplayCameraCustom(
            player.transform,
            new Vector3(-10f, 8f, -14f),
            60f,
            mech);

        SpawnPointLight("Light_CyclicPlate", new Vector3(8f, 3f, 4f), new Color(0.9f, 0.75f, 0.35f), 3f, 8f, env);
        SpawnPointLight("Light_GatePlate", new Vector3(-8f, 3f, 4f), new Color(0.3f, 0.75f, 1f), 3f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 7f, 36f), new Color(0.15f, 0.6f, 1f), 5f, 14f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "", "", "");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 18f), 24f, 36f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 36f);
        SpawnPuzzleIntent(mech, 2, 3, true, true, true, 20f,
            "PROTOTYPE A: anticipatory recording. Player must record before the condition exists. No hints, no path, no tutorial text.");

        // School dressing — Sage walls (Negociación)
        SpawnBarrierWall("SchoolWallL", new Vector3(-12f, 6f, 18f), new Vector3(0.5f, 12f, 40f), env, _wallSageMat);
        SpawnBarrierWall("SchoolWallR", new Vector3(12f, 6f, 18f), new Vector3(0.5f, 12f, 40f), env, _wallSageMat);
        TryInstantiateAssetByName("locker", env, new Vector3(-4f, 0.3f, -1f));
        TryInstantiateAssetByName("bookcaseOpen", env, new Vector3(4f, 0.3f, -1f));
        TryInstantiateAssetByName("desk", env, new Vector3(-3f, 0.3f, 2f));
        TryInstantiateAssetByName("chairDesk", env, new Vector3(-2f, 0.3f, 2f));
        TryInstantiateAssetByName("pipe", env, new Vector3(-11.5f, 5f, 12f));
        TryInstantiateAssetByName("pipe", env, new Vector3(11.5f, 5f, 24f));

        SaveScene(scene, "Level_07");
    }

    static void BuildLevel08()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_08";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(HexColor("485058"), 0.005f, HexColor("D0D4D8"));
        SpawnDirectionalLight();
        SpawnLevelLightingSettings(env, new Color(0.18f, 0.2f, 0.28f), 0.004f);
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 18f), 30f, 40f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 2f), new Vector3(10f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 36f), new Vector3(10f, 0.6f, 8f), env, _floorMat);

        GameObject sideGalleryL = MakePlatform("SideGalleryL", new Vector3(-8f, 4f, 10f), new Vector3(3f, 0.5f, 10f), env, _floorMat);
        GameObject sideGalleryR = MakePlatform("SideGalleryR", new Vector3(-8f, 4f, 23f), new Vector3(3f, 0.5f, 16f), env, _floorMat);

        SpawnBarrierWall("Barrier_Gallery_L", new Vector3(-10f, 6f, 18f), new Vector3(0.5f, 4f, 26f), env);
        SpawnBarrierWall("Barrier_Gallery_R", new Vector3(-6.2f, 6f, 18f), new Vector3(0.5f, 4f, 26f), env);
        SpawnBarrierWall("Barrier_Corridor_R", new Vector3(5.5f, 3f, 18f), new Vector3(0.5f, 6f, 26f), env);
        SpawnBarrierWall("Barrier_Corridor_L", new Vector3(-2.2f, 3f, 18f), new Vector3(0.5f, 6f, 26f), env);

        SpawnKillZone("VoidPit_L", new Vector3(-4.2f, -4f, 18f), new Vector3(3.2f, 8f, 26f), mech);
        SpawnKillZone("VoidPit_R", new Vector3(3f, -4f, 18f), new Vector3(4.5f, 8f, 26f), mech);

        PuzzleSignal bridge1Signal = CreatePuzzleSignal("Signal_Bridge1", "Puente 1 Alineado", mech);
        PuzzleSignal bridge2Signal = CreatePuzzleSignal("Signal_Bridge2", "Puente 2 Alineado", mech);
        PuzzleSignal bridge3Signal = CreatePuzzleSignal("Signal_Bridge3", "Puente 3 Alineado", mech);

        CreateKineticBlock("Bridge1", new Vector3(1.6f, -4f, 10f), new Vector3(3.4f, 0.5f, 6f), new Vector3(-9.6f, 8f, 0f), new Vector3(3f, 2f, 4f), new Vector3(0f, 4f, 0f), mech, bridge1Signal, true, true, 5f);
        CreateKineticBlock("Bridge2", new Vector3(1.6f, -4f, 18f), new Vector3(3.4f, 0.5f, 6f), new Vector3(-9.6f, 8f, 0f), new Vector3(3f, 2f, 4f), new Vector3(0f, 4f, 0f), mech, bridge2Signal, true, true, 5f);
        CreateKineticBlock("Bridge3", new Vector3(1.6f, -4f, 26f), new Vector3(3.4f, 0.5f, 6f), new Vector3(-9.6f, 8f, 0f), new Vector3(3f, 2f, 4f), new Vector3(0f, 4f, 0f), mech, bridge3Signal, true, true, 5f);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 38f), mech, "Level_09");
        
        PuzzleSignal runSignal = CreatePuzzleSignal("Signal_Run", "Corredor Cruzado", mech);
        CreateSignalGoal(mech, "Graba una proyecci├│n que active los tres puentes y corre a toda velocidad.", "La sombra corre adelante, el vac├¡o viene detr├ís.", "Sobreviviste a la fuga sincronizada.", exit, runSignal);

        GameObject player = SpawnPlayer(new Vector3(1.6f, 1.1f, 2f), true, 2, 9f);
        SpawnGameplayCamera(player.transform, new Vector3(-7f, 4f, -11f));

        SpawnPuzzleIntent(mech, 0, 4, true, true, true, 36f, "Level 08: high-speed chase requiring pre-recorded projection to raise bridges sequentially.");

        SpawnPointLight("Light_Gallery1", new Vector3(-8f, 6f, 10f), new Color(0.24f, 0.76f, 1f), 2f, 6f, env);
        SpawnPointLight("Light_Gallery2", new Vector3(-8f, 6f, 18f), new Color(0.24f, 0.76f, 1f), 2f, 6f, env);
        SpawnPointLight("Light_Gallery3", new Vector3(-8f, 6f, 26f), new Color(0.24f, 0.76f, 1f), 2f, 6f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 7f, 36f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Corre a trav├®s de los puentes activados por tu eco.", "La disoluci├│n espectral se acerca.", "Puentes cruzados con ├®xito.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 18f), 20f, 36f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Chase, 2f, 38f, enableChase: true);

        // School dressing — Sage walls (Culpa)
        SpawnBarrierWall("SchoolWallL", new Vector3(-12f, 6f, 18f), new Vector3(0.5f, 12f, 40f), env, _wallSageMat);
        SpawnBarrierWall("SchoolWallR", new Vector3(8f, 6f, 18f), new Vector3(0.5f, 12f, 40f), env, _wallSageMat);
        TryInstantiateAssetByName("locker", env, new Vector3(-4f, 0.3f, 0f));
        TryInstantiateAssetByName("desk", env, new Vector3(3f, 0.3f, 3f));
        TryInstantiateAssetByName("chairDesk", env, new Vector3(4f, 0.3f, 3f));
        TryInstantiateAssetByName("pipe", env, new Vector3(-11.5f, 5f, 14f));
        TryInstantiateAssetByName("pipe", env, new Vector3(7.5f, 5f, 26f));

        SaveScene(scene, "Level_08");
    }

    static void BuildLevel09()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_09";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(HexColor("4A5A62"), 0.005f, HexColor("D0D8DC"));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 12f), 28f, 36f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 12f), 28f, 36f, env);

        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.5f, 6f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -1.5f));

        GameObject chamberA = MakePlatform("ChamberA", new Vector3(-6f, 0f, 12f), new Vector3(6f, 0.5f, 14f), env, _floorMat);
        GameObject chamberB = MakePlatform("ChamberB", new Vector3(6f, 0f, 12f), new Vector3(6f, 0.5f, 14f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 24f), new Vector3(10f, 0.5f, 6f), env, _floorMat);

        PressurePlate startPlate = CreatePlateOnPlatform("StartPlate", chamberA, new Vector3(0f, 0f, -6f), mech, false);
        PressurePlate mirrorPlate = CreatePlateOnPlatform("MirrorPlate", chamberA, new Vector3(0f, 0f, 6f), mech, false);

        DoorController gateA = CreateDoor("GateA", new Vector3(-6f, 1.5f, 5f), new Vector3(4f, 3f, 0.5f), mech, new[] { startPlate });
        gateA.latchOpen = true;

        DoorController gateB = CreateDoor("GateB", new Vector3(6f, 1.5f, 19f), new Vector3(4f, 3f, 0.5f), mech, new[] { mirrorPlate });
        gateB.latchOpen = true;

        GameObject wireA = new GameObject("Wire_PlateA");
        wireA.transform.SetParent(mech, false);
        PuzzleWire wCompA = wireA.AddComponent<PuzzleWire>();
        PuzzleWire.Connection connA = new PuzzleWire.Connection();
        connA.door = gateA;
        connA.plates = new[] { startPlate };
        connA.logic = PuzzleWire.LogicMode.AND;
        connA.latchOpen = true;
        wCompA.connections = new[] { connA };

        GameObject wireB = new GameObject("Wire_PlateB");
        wireB.transform.SetParent(mech, false);
        PuzzleWire wCompB = wireB.AddComponent<PuzzleWire>();
        PuzzleWire.Connection connB = new PuzzleWire.Connection();
        connB.door = gateB;
        connB.plates = new[] { mirrorPlate };
        connB.logic = PuzzleWire.LogicMode.AND;
        connB.latchOpen = true;
        wCompB.connections = new[] { connB };

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 25f), mech, "Level_10");
        CreateLevelGoal(mech, "Sincroniza tus ecos para cruzar los portones interconectados por cables de energ├¡a.", "Las l├¡neas de energ├¡a modular se allinean en la memoria.", "Enlace de la simulaci├n completado.", exit, startPlate, mirrorPlate);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 12f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 4f, -11f));

        SpawnPuzzleIntent(mech, 2, 4, true, true, true, 24f, "Level 09: mirrored chambers utilizing visual puzzle wires to map connections between plates and doors.");

        SpawnPointLight("Light_StartPlate", new Vector3(-6f, 3f, 6f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_MirrorPlate", new Vector3(-6f, 3f, 18f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 8f, 24f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Sigue los cables de energ├¡a y activa las compuertas con tus ecos.", "El espejo refleja tus acciones pasadas.", "Simetr├¡a restaurada.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 12f), 20f, 25f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MovingCity, 2f, 24f);

        // School dressing — Teal walls (Culpa)
        SpawnBarrierWall("SchoolWallL", new Vector3(-12f, 6f, 12f), new Vector3(0.5f, 12f, 28f), env, _wallTealMat);
        SpawnBarrierWall("SchoolWallR", new Vector3(12f, 6f, 12f), new Vector3(0.5f, 12f, 28f), env, _wallTealMat);
        TryInstantiateAssetByName("locker", env, new Vector3(-4f, 0.3f, -2f));
        TryInstantiateAssetByName("locker", env, new Vector3(4f, 0.3f, -2f));
        TryInstantiateAssetByName("desk", env, new Vector3(-3f, 0.3f, 1f));
        TryInstantiateAssetByName("pipe", env, new Vector3(-11.5f, 5f, 8f));
        TryInstantiateAssetByName("pipe", env, new Vector3(11.5f, 5f, 16f));

        SaveScene(scene, "Level_09");
    }

    static void BuildLevel10()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_10";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(HexColor("584A50"), 0.004f, HexColor("D8D0D4"));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 20f), 34f, 50f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 20f), 34f, 50f, env);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -2f));

        MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(12f, 0.5f, 10f), env, _floorMat);
        MakePlatform("HiddenPlat_A", new Vector3(-2f, 0f, 14f), new Vector3(4f, 0.5f, 4f), env, _archMat);
        MakePlatform("HiddenPlat_B", new Vector3(3f, 1f, 20f), new Vector3(4f, 0.5f, 4f), env, _archMat);
        MakePlatform("HiddenPlat_C", new Vector3(-1f, 2f, 27f), new Vector3(4f, 0.5f, 4f), env, _archMat);
        MakePlatform("HiddenPlat_D", new Vector3(2f, 3f, 34f), new Vector3(5f, 0.5f, 5f), env, _archMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 4f, 42f), new Vector3(12f, 0.5f, 10f), env, _floorMat);

        GameObject lyraEchoRoot = new GameObject("LyraAmbientEcho_Waypoints");
        lyraEchoRoot.transform.SetParent(mech, false);
        Vector3[] lyraWaypoints =
        {
            new Vector3(0f, 0.1f, 4f),
            new Vector3(-2f, 0.1f, 14f),
            new Vector3(3f, 1.1f, 20f),
            new Vector3(-1f, 2.1f, 27f),
            new Vector3(2f, 3.1f, 34f),
            new Vector3(0f, 4.1f, 42f)
        };
        for (int i = 0; i < lyraWaypoints.Length; i++)
        {
            GameObject wp = new GameObject("Waypoint_" + i);
            wp.transform.SetParent(lyraEchoRoot.transform, false);
            wp.transform.position = lyraWaypoints[i];
        }

        GameObject voiceTrigger = new GameObject("LyraVoiceTrigger");
        voiceTrigger.transform.SetParent(mech, false);
        voiceTrigger.transform.position = new Vector3(0f, 2f, 27f);
        BoxCollider voiceCol = voiceTrigger.AddComponent<BoxCollider>();
        voiceCol.isTrigger = true;
        voiceCol.size = new Vector3(8f, 4f, 4f);

        PressurePlate plateExit = CreatePlateOnPlatform("PlateExit", exitPlat, new Vector3(0f, 0f, -2f), mech, false);
        DoorController exitGate = CreateDoor("ExitGate", new Vector3(0f, 5.5f, 44f), new Vector3(4f, 3f, 0.5f), mech, new[] { plateExit });
        exitGate.latchOpen = true;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 5.25f, 46f), mech, "Level_11");
        CreateLevelGoal(mech, "Activa la placa final para abrir el porton de salida.", "El eco de Lyra te guia al final.", "Porton abierto.", exit, plateExit);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 1, 14f);
        SpawnGameplayCameraCustom(
            player.transform,
            new Vector3(-6f, 12f, -16f),
            72f,
            mech);

        for (int i = 0; i < lyraWaypoints.Length - 1; i++)
        {
            SpawnPointLight(
                "LyraGlow_" + i,
                lyraWaypoints[i] + Vector3.up * 1.5f,
                new Color(0.85f, 0.7f, 0.45f),
                0.8f + i * 0.15f,
                5f,
                env);
        }
        SpawnPointLight("Light_Exit", new Vector3(0f, 7f, 46f), new Color(0.15f, 0.6f, 1f), 5f, 14f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Activa la placa final para abrir el porton de salida.", "El eco de Lyra te guia al final.", "Porton abierto.");
        SpawnAmbientLights(env, new Vector3(0f, 2f, 22f), 28f, 44f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MultiLayerTimeline, 2f, 44f);
        SpawnPuzzleIntent(mech, 1, 2, true, false, false, 30f,
            "PROTOTYPE B: Lyra ambient echo reveals hidden geometry. Player must follow Lyra to see the path. Narrative from mechanic. Zero text.");

        // School dressing — Rose walls (Depresión)
        SpawnBarrierWall("SchoolWallL", new Vector3(-10f, 6f, 21f), new Vector3(0.5f, 12f, 48f), env, _wallRoseMat);
        SpawnBarrierWall("SchoolWallR", new Vector3(10f, 6f, 21f), new Vector3(0.5f, 12f, 48f), env, _wallRoseMat);
        TryInstantiateAssetByName("locker", env, new Vector3(-5f, 0.3f, -1f));
        TryInstantiateAssetByName("bookcaseOpen", env, new Vector3(5f, 0.3f, -1f));
        TryInstantiateAssetByName("desk", env, new Vector3(-3f, 0.3f, 2f));
        TryInstantiateAssetByName("chairDesk", env, new Vector3(-2f, 0.3f, 2f));
        TryInstantiateAssetByName("pipe", env, new Vector3(-9.5f, 5f, 15f));
        TryInstantiateAssetByName("pipe", env, new Vector3(9.5f, 5f, 30f));

        SaveScene(scene, "Level_10");
    }
}
