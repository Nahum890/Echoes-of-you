import re

path = r"c:\Users\lol xdd\OneDrive\Documentos\Colegio\Echoes of you\Assets\Editor\EchoesProductionBuilder.cs"

with open(path, "r", encoding="utf-8") as f:
    content = f.read()

# Define the C# implementations for all 10 levels
levels_code = """
    static void BuildLevel07()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_07";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.08f, 0.1f, 0.16f, 1f), 0.012f, new Color(0.16f, 0.18f, 0.24f, 1f));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 16f), 28f, 38f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 16f), 28f, 38f, env);
        SpawnLevelLightingSettings(env, new Color(0.14f, 0.18f, 0.26f), 0.0055f);

        // Start and Exit platforms
        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(12f, 0.5f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 4f, 36f), new Vector3(12f, 0.5f, 8f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -1f));

        // Sub-system 1: Elevator plate and elevator platform
        PressurePlate plateElevator = CreatePlate("PlateElevator", new Vector3(-4f, 0.36f, 2f), mech);
        plateElevator.autoReleaseTimer = 1f;
        TimedMovingPlatform elevator = CreateBridge("Elevator", new Vector3(-8f, 0f, 12f), Vector3.zero, new Vector3(0f, 4f, 0f), new Vector3(3f, 0.5f, 3f), plateElevator, mech);

        // Sub-system 2: Sliding rotator platform
        PressurePlate plateRotator = CreatePlate("PlateRotator", new Vector3(4f, 0.36f, 2f), mech);
        plateRotator.autoReleaseTimer = 1f;
        TimedMovingPlatform slidingBridge = CreateBridge("SlidingBridge", new Vector3(0f, 4f, 18f), new Vector3(-8f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(12f, 0.5f, 3f), plateRotator, mech);

        // Sub-system 3: High lateral catwalks and exit Hazard Field
        GameObject sideCatwalk = MakePlatform("SideCatwalk", new Vector3(8f, 4f, 24f), new Vector3(3f, 0.5f, 14f), env, _bridgeMat);
        PuzzleSignal shieldSignal = CreatePuzzleSignal("Signal_Shield", "Escudo Núcleo", mech);
        CreateHazardField("ExitHazard", new Vector3(0f, 5.5f, 30f), new Vector3(8f, 3f, 1.2f), mech, shieldSignal);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 5.25f, 38f), mech, "Level_08");
        CreateLevelGoal(mech, "Sincroniza tres ecos para activar el ascensor, deslizar el puente y neutralizar la barrera final.", "El núcleo del motor responde a las tres memorias.", "Sinfonía del Núcleo completada.", exit, plateElevator, plateRotator);

        // Player starts with max 3 echoes and 16 seconds of recording time
        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 3, 16f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 5.5f, -12f));

        CreateTutorialTrigger("Tut_L07_Sinfonia", new Vector3(0f, 1f, 0f), new Vector3(12f, 3f, 8f),
            "Nivel 7 — El Núcleo Cíclico",
            "Esta es una sinfonía de tres ecos: 1. Un eco pisa la placa izquierda para subir el ascensor. 2. Otro eco sube por el ascensor y camina por la cornisa derecha para meterse dentro de la barrera roja. 3. Un tercer eco pisa la placa derecha para alinear el puente central. Corre a través del puente y cruza la barrera azul.",
            12f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-8f, 1.1f, 12f),
            new Vector3(8f, 5.1f, 24f),
            new Vector3(0f, 5.1f, 36f)
        });
        SpawnPuzzleIntent(mech, 3, 5, true, true, true, 28f, "Level 07: grand core symphony coordinating elevator, sliding platform, and hazard shielding.");

        SpawnPointLight("Light_Elevator", new Vector3(-8f, 3f, 12f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_Rotator", new Vector3(0f, 6f, 18f), new Color(0.9f, 0.7f, 0.4f), 3f, 10f, env);
        SpawnPointLight("Light_Hazard", new Vector3(0f, 6f, 30f), new Color(1f, 0.16f, 0.08f), 3.5f, 10f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 7f, 38f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Sincroniza tres ecos en el ascensor, puente y barrera.", "El motor está en fase armónica.", "Motor encendido. Camino libre.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 18f), 24f, 36f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 36f);

        SaveScene(scene, "Level_07");
    }

    static void BuildLevel01()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_01";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.12f, 0.14f, 0.2f, 1f), 0.008f, new Color(0.2f, 0.22f, 0.28f, 1f));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 12f), 22f, 34f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 12f), 22f, 34f, env);

        // Start and Exit platforms at high altitude (y = 4)
        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 4f, 0f), new Vector3(8f, 0.5f, 6f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 4f, 22f), new Vector3(8f, 0.5f, 6f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 4f, -1f));

        // Lateral walls to prevent falling off the start platform
        SpawnBarrierWall("BarrierStart_L", new Vector3(-3.8f, 5.5f, 0f), new Vector3(0.4f, 3f, 6f), env);
        SpawnBarrierWall("BarrierStart_R", new Vector3(3.8f, 5.5f, 0f), new Vector3(0.4f, 3f, 6f), env);
        SpawnBarrierWall("BarrierExit_L", new Vector3(-3.8f, 5.5f, 22f), new Vector3(0.4f, 3f, 6f), env);
        SpawnBarrierWall("BarrierExit_R", new Vector3(3.8f, 5.5f, 22f), new Vector3(0.4f, 3f, 6f), env);

        // Lower mirror channel on the left (y = 0)
        GameObject lowerChannelL = MakePlatform("LowerChannel_L", new Vector3(-6f, 0f, 11f), new Vector3(3f, 0.5f, 16f), env, _bridgeMat);
        // Visual decoration on the right lower channel to create architectural symmetry
        MakePlatform("LowerChannel_R", new Vector3(6f, 0f, 11f), new Vector3(3f, 0.5f, 16f), env, _bridgeMat);

        // Kinetic bridge connecting the high start and exit platforms across the central void
        // The bridge moves from y = 0 up to y = 4 when an echo is inside the control zone in the lower channel
        PuzzleSignal bridgeSignal = CreatePuzzleSignal("Signal_TemporalBridge", "Puente Temporal", mech);
        CreateKineticBlock("Bridge_Central", new Vector3(0f, 0.25f, 11f), new Vector3(4f, 0.5f, 16f), new Vector3(-6f, -0.25f, 0f), new Vector3(3f, 2f, 4f), new Vector3(0f, 3.75f, 0f), mech, bridgeSignal, true, true, 4f);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 5.25f, 23.5f), mech, "Level_02");
        CreateSignalGoal(mech, "Activa la placa inferior proyectando un eco y cruza antes de que baje el puente.", "El puente se alinea temporalmente.", "Cruzaste la fractura simétrica.", exit, bridgeSignal);

        // Player starts on the high platform
        GameObject player = SpawnPlayer(new Vector3(0f, 5.1f, 0f), true, 1, 8f);
        SpawnGameplayCamera(player.transform, new Vector3(-6f, 7.4f, -10f));

        CreateTutorialTrigger("Tut_L01_Simetria", new Vector3(0f, 5f, 0f), new Vector3(8f, 3f, 6f),
            "Nivel 1 — Despertar Simétrico",
            "Mantén F para proyectar tu eco hacia la zona azul en el canal inferior izquierdo. Suelta F, luego cruza rápido por el puente central.",
            8f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 5f, 0f),
            new Vector3(-6f, 1f, 11f),
            new Vector3(0f, 5f, 22f)
        });
        SpawnPuzzleIntent(mech, 1, 2, true, true, true, 8f, "Level 01: introduction to projection and timed vertical traversal using lower channels.");

        SpawnPointLight("Light_LowerPlate", new Vector3(-6f, 3f, 11f), new Color(0.25f, 0.75f, 1f), 4f, 10f, env);
        SpawnPointLight("Light_Bridge", new Vector3(0f, 6f, 11f), new Color(0.9f, 0.7f, 0.4f), 3f, 12f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 7f, 23.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Proyecta tu eco a la placa inferior y cruza el puente.", "El puente obecece a tu pasado.", "El primer enlace se ha completado.");
        SpawnAmbientLights(env, new Vector3(0f, 2f, 11f), 18f, 28f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 22f);

        SaveScene(scene, "Level_01");
    }

    static void BuildLevel02()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_02";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.04f, 0.05f, 0.08f, 1f), 0.043f, new Color(0.04f, 0.05f, 0.08f, 1f));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 12f), 34f, 36f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 12f), 34f, 36f, env);

        // Low starting platform (y = 0)
        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(8f, 0.5f, 8f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -1.5f));

        // Two vertical elevators moving continuously out of phase (y = 0 to 6)
        CreateMotorPlatform("LeftElevator", new Vector3(-6f, 0f, 10f), new Vector3(3f, 0.5f, 3f), Vector3.zero, new Vector3(0f, 6f, 0f), Vector3.zero, 5f, 0f, env, _bridgeMat);
        CreateMotorPlatform("RightElevator", new Vector3(6f, 0f, 10f), new Vector3(3f, 0.5f, 3f), Vector3.zero, new Vector3(0f, 6f, 0f), Vector3.zero, 5f, 2.5f, env, _bridgeMat);

        // High left control ledge with a pressure plate
        GameObject leftLedge = MakePlatform("LeftLedge", new Vector3(-6f, 6f, 18f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        PressurePlate plate = CreatePlateOnPlatform("LeftPlate", leftLedge, Vector3.zero, mech, false);
        plate.autoReleaseTimer = 1.5f;

        // High central exit ledge with exit door
        GameObject exitLedge = MakePlatform("ExitLedge", new Vector3(0f, 6f, 26f), new Vector3(6f, 0.5f, 6f), env, _floorMat);
        DoorController door = CreateDoor("ExitDoor", new Vector3(0f, 7.5f, 23f), new Vector3(4f, 3f, 0.5f), mech, new[] { plate });
        door.latchOpen = false;

        LevelExit exit = CreateLevelExit(new Vector3(0f, 7.25f, 27.5f), mech, "Level_03");
        CreateLevelGoal(mech, "Usa tu eco para presionar la placa en la cornisa izquierda mientras subes por la derecha.", "La puerta de salida se abre al sincronizar.", "Sincronía vertical alcanzada.", exit, plate);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 1, 12f);
        SpawnGameplayCamera(player.transform, new Vector3(-7f, 4.2f, -11f));

        CreateTutorialTrigger("Tut_L02_Contrapeso", new Vector3(0f, 1f, 0f), new Vector3(8f, 3f, 8f),
            "Nivel 2 — El Eco Contrapeso",
            "Sube por el ascensor izquierdo y graba un recorrido yendo a la cornisa izquierda para presionar la placa. Luego, como jugador, sube por el ascensor derecho mientras el eco abre la puerta.",
            9f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-6f, 7.1f, 18f),
            new Vector3(6f, 7.1f, 10f),
            new Vector3(0f, 7.1f, 26f)
        });
        SpawnPuzzleIntent(mech, 1, 3, true, true, true, 12f, "Level 02: vertical traversal synchronizing out-of-phase elevators using recording.");

        SpawnPointLight("Light_LeftLedge", new Vector3(-6f, 8f, 18f), new Color(0.24f, 0.56f, 0.74f), 3f, 8f, env);
        SpawnPointLight("Light_RightLift", new Vector3(6f, 4f, 10f), new Color(0.35f, 0.75f, 1f), 2.5f, 10f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 9f, 27.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Sube por un ascensor para presionar la placa con un eco, luego cruza por el otro.", "Tu eco es tu propio contrapeso temporal.", "La puerta se ha desbloqueado.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 13f), 18f, 32f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MovingCity, 2f, 26f);

        SaveScene(scene, "Level_02");
    }

    static void BuildLevel03()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_03";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.02f, 0.02f, 0.04f, 1f), 0.06f, new Color(0.02f, 0.02f, 0.04f, 1f));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 14f), 32f, 40f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 14f), 32f, 40f, env);

        // Start and Exit platforms
        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.5f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 24f), new Vector3(10f, 0.5f, 8f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -1f));

        // Central corridor connecting them
        GameObject corridor = MakePlatform("Corridor", new Vector3(0f, 0f, 12f), new Vector3(4f, 0.5f, 16f), env, _bridgeMat);

        // Left control chamber where the echo shields the hazard but triggers the conflict trap
        GameObject controlChamber = MakePlatform("ControlChamber", new Vector3(-8f, 0f, 12f), new Vector3(6f, 0.5f, 6f), env, _floorMat);

        // Hazard Field blocking the central corridor
        PuzzleSignal shieldSignal = CreatePuzzleSignal("Signal_Shield", "Energía Neutralizada", mech);
        CreateHazardField("Muro_Energia", new Vector3(0f, 1.5f, 8f), new Vector3(4f, 3f, 1.2f), mech, shieldSignal);

        // Exit gate blocking the final approach
        DoorController exitDoor = CreateDoor("ExitGate", new Vector3(0f, 1.5f, 18f), new Vector3(4f, 3f, 0.5f), mech, new PressurePlate[0]);
        exitDoor.latchOpen = false;

        // Conflict trap inside the control chamber
        PuzzleSignal trapSignal = CreatePuzzleSignal("Signal_Trap", "Trampa Paradoja", mech);
        CreateConflictTrap("ControlTrap", new Vector3(-8f, 1.5f, 12f), new Vector3(5f, 3f, 5f), mech, new[] { exitDoor }, null, trapSignal);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 26f), mech, "Level_04");
        CreateSignalGoal(mech, "Neutraliza el muro de energía sin activar permanentemente la trampa de conflicto.", "El espacio se reordena.", "Sobreviviste a la paradoja.", exit, shieldSignal, trapSignal);

        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 1, 10f);
        SpawnGameplayCamera(player.transform, new Vector3(-6f, 4.5f, -10f));

        CreateTutorialTrigger("Tut_L03_Paradoja", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 3 — La Paradoja del Conflicto",
            "Tu eco en la cámara izquierda neutraliza el muro rojo, pero bloquea la puerta de salida. Graba un recorrido donde el eco entre, espere 3 segundos y luego SALGA de la cámara. Cruza el muro rojo cuando se vuelva azul, y espera a que el eco salga para cruzar la puerta.",
            10f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-8f, 1.1f, 12f),
            new Vector3(0f, 1.1f, 12f),
            new Vector3(0f, 1.1f, 24f)
        });
        SpawnPuzzleIntent(mech, 0, 3, true, true, true, 16f, "Level 03: introduction to conflict traps and hazard fields requiring precise time-delayed routing.");

        SpawnPointLight("Light_ControlChamber", new Vector3(-8f, 3f, 12f), new Color(0.24f, 0.76f, 1f), 3f, 8f, env);
        SpawnPointLight("Light_Hazard", new Vector3(0f, 3f, 8f), new Color(1f, 0.16f, 0.08f), 3.5f, 10f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 4f, 26f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Neutraliza el muro rojo y evita que la trampa cierre la puerta.", "El eco es tu llave y tu jaula.", "Camino liberado.");
        SpawnAmbientLights(env, new Vector3(0f, 2f, 12f), 18f, 28f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 24f);

        SaveScene(scene, "Level_03");
    }

    static void BuildLevel04()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_04";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.1f, 0.12f, 0.18f, 1f), 0.01f, new Color(0.18f, 0.2f, 0.28f, 1f));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 20f), 32f, 44f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 20f), 32f, 44f, env);

        // Start and Exit platforms
        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(12f, 0.5f, 10f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 30f), new Vector3(12f, 0.5f, 10f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -1f));

        // Three distinct plates arranged in a wide triangle
        GameObject platePlatA = MakePlatform("PlatePlatA", new Vector3(-6f, 0f, 10f), new Vector3(3f, 0.5f, 3f), env, _bridgeMat);
        GameObject platePlatB = MakePlatform("PlatePlatB", new Vector3(6f, 0f, 15f), new Vector3(3f, 0.5f, 3f), env, _bridgeMat);
        GameObject platePlatC = MakePlatform("PlatePlatC", new Vector3(-6f, 0f, 20f), new Vector3(3f, 0.5f, 3f), env, _bridgeMat);

        PressurePlate plateA = CreatePlateOnPlatform("PlateA", platePlatA, Vector3.zero, mech, false);
        PressurePlate plateB = CreatePlateOnPlatform("PlateB", platePlatB, Vector3.zero, mech, false);
        PressurePlate plateC = CreatePlateOnPlatform("PlateC", platePlatC, Vector3.zero, mech, false);

        // Central rotating spine to cross between platforms
        CreateMotorPlatform("Rotating_Cross", new Vector3(0f, 0.25f, 15f), new Vector3(10f, 0.35f, 1.2f), Vector3.zero, Vector3.zero, new Vector3(0f, 45f, 0f), 1f, 0f, env, _doorMat);

        // Exit door opened by sequential condition
        DoorController exitDoor = CreateDoor("ExitGate", new Vector3(0f, 1.5f, 25f), new Vector3(6f, 3f, 0.5f), mech, new PressurePlate[0]);
        exitDoor.latchOpen = true;

        // PuzzleCondition setup
        GameObject condObj = new GameObject("Condition_Sequential");
        condObj.transform.SetParent(mech);
        PuzzleCondition condition = condObj.AddComponent<PuzzleCondition>();
        condition.type = PuzzleCondition.ConditionType.SequentialOrder;
        condition.plates = new[] { plateA, plateB, plateC };
        condition.progressMessage = "Enlace secuencia";
        condition.successMessage = "Secuencia correcta! Acceso concedido.";
        condition.failMessage = "Secuencia rota! Intenta de nuevo.";
        condition.doorsToOpen = new[] { exitDoor };

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 32f), mech, "Level_05");
        
        // Connect PuzzleCondition satisfaction to LevelGoal via a PuzzleSignal
        PuzzleSignal sequenceSignal = CreatePuzzleSignal("Signal_Sequence", "Secuencia Resuelta", mech);
        condition.targetSignal = sequenceSignal;

        CreateSignalGoal(mech, "Activa las placas en el orden exacto: Izquierda-Atrás, Derecha, Izquierda-Adelante.", "Las tres memorias deben sonar en armonía.", "Sinfonía secuencial completada.", exit, sequenceSignal);

        // Player starts with max 2 echoes and 10 seconds of recording time
        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 10f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 4f, -11f));

        CreateTutorialTrigger("Tut_L04_Secuencia", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 8f),
            "Nivel 4 — La Jaula de Presión",
            "Las placas deben ser pisadas en un orden específico (A -> B -> C). Graba un recorrido donde pises la placa A (izquierda posterior) y luego la B (derecha). Como jugador, corre y pisa la placa C (izquierda anterior) justo cuando tu eco pise la B.",
            10f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-6f, 1.1f, 10f),
            new Vector3(6f, 1.1f, 15f),
            new Vector3(-6f, 1.1f, 20f)
        });
        SpawnPuzzleIntent(mech, 3, 3, true, true, true, 20f, "Level 04: advanced sequential coordination using PuzzleCondition.");

        SpawnPointLight("Light_PlateA", new Vector3(-6f, 3f, 10f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_PlateB", new Vector3(6f, 3f, 15f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_PlateC", new Vector3(-6f, 3f, 20f), new Color(0.3f, 0.75f, 1f), 2.5f, 6f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 5f, 32f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Pisa las tres placas en la secuencia correcta (A -> B -> C).", "La máquina requiere un orden exacto.", "La secuencia ha sido grabada.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 15f), 24f, 36f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 32f);

        SaveScene(scene, "Level_04");
    }

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

        // Start and Exit platforms separated by a void gap
        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(8f, 0.5f, 6f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 26f), new Vector3(8f, 0.5f, 6f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -1.5f));

        // High control ledge for the echo on the left (y = 4)
        GameObject controlLedge = MakePlatform("ControlLedge", new Vector3(-8f, 4f, 6f), new Vector3(4f, 0.5f, 4f), env, _floorMat);
        // Connect the start platform to the control ledge with a ramp the echo can walk up
        GameObject ramp = MakePlatform("ControlRamp", new Vector3(-4.5f, 2f, 3f), new Vector3(2f, 0.45f, 8f), env, _bridgeMat);
        ramp.transform.rotation = Quaternion.Euler(22f, 0f, 0f);

        // Floating jump platforms
        GameObject float1 = MakePlatform("Float_1", new Vector3(0f, 0f, 8f), new Vector3(3f, 0.5f, 3f), env, _bridgeMat);
        GameObject float2 = MakePlatform("Float_2", new Vector3(0f, 0f, 18f), new Vector3(3f, 0.5f, 3f), env, _bridgeMat);

        // A lethal Hazard Field blocks the center of the gap (z = 13)
        PuzzleSignal shieldSignal = CreatePuzzleSignal("Signal_Shield", "Energía Neutralizada", mech);
        CreateHazardField("Hazard_Curtain", new Vector3(0f, 2f, 13f), new Vector3(8f, 4f, 1.2f), mech, shieldSignal);

        // Target object at the exit platform for the momentum relay to boost toward
        GameObject relayTarget = new GameObject("RelayTarget");
        relayTarget.transform.SetParent(exitPlat.transform, false);
        relayTarget.transform.localPosition = new Vector3(0f, 1f, 0f);

        // Momentum relay placed on Float_1
        CreateMomentumRelay("Boost_Float1", new Vector3(0f, 0f, 8f), new Vector3(3f, 2f, 3f), relayTarget.transform, 14f, mech);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 28f), mech, "Level_06");
        CreateSignalGoal(mech, "Cruza la fractura neutralizando la barrera y usando el impulso cinético.", "La barrera cede temporalmente.", "Salto de fe completado.", exit, shieldSignal);

        // Player starts with max 2 echoes and 8 seconds of recording time
        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, -1f), true, 2, 8f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 5f, -12f));

        CreateTutorialTrigger("Tut_L05_Cortina", new Vector3(0f, 1f, 0f), new Vector3(8f, 3f, 6f),
            "Nivel 5 — La Cortina Inestable",
            "Sube a la cornisa izquierda de control y proyecta tu eco a través de la barrera roja (hasta el otro lado). Luego, como jugador, corre y salta a la plataforma flotante central. Cuando el eco pase por la barrera, esta se volverá azul y segura, y la plataforma te impulsará con fuerza hacia el final.",
            10f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, -1f),
            new Vector3(-8f, 5.1f, 6f),
            new Vector3(0f, 1.1f, 8f),
            new Vector3(0f, 1.1f, 26f)
        });
        SpawnPuzzleIntent(mech, 1, 3, true, true, true, 24f, "Level 05: combining hazard shielding with echo-activated momentum relays.");

        SpawnPointLight("Light_ControlLedge", new Vector3(-8f, 6f, 6f), new Color(0.35f, 0.8f, 1f), 3f, 8f, env);
        SpawnPointLight("Light_Hazard", new Vector3(0f, 4f, 13f), new Color(1f, 0.16f, 0.08f), 3.5f, 10f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 5f, 28f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Cruza la barrera usando el eco para neutralizarla y ganar impulso.", "El eco es tu escudo y tu motor.", "Cruce exitoso.");
        SpawnAmbientLights(env, new Vector3(0f, 2f, 13f), 20f, 34f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 28f);

        SaveScene(scene, "Level_05");
    }

    static EchoKineticZone CreateMomentumRelay(string name, Vector3 position, Vector3 size, Transform target, float force, Transform parent)
    {
        GameObject zone = new GameObject(name);
        zone.transform.SetParent(parent, false);
        zone.transform.position = position;

        BoxCollider col = zone.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;

        EchoKineticZone kZone = zone.AddComponent<EchoKineticZone>();
        SetSerializedValue(kZone, "role", EchoKineticRole.MomentumRelay);
        SetSerializedValue(kZone, "momentumRelayTarget", target);
        SetSerializedValue(kZone, "momentumRelayForce", force);
        SetSerializedValue(kZone, "requireEcho", true);
        SetSerializedValue(kZone, "acceptPlayer", true);

        // Visual indicator (glowing zone)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(zone.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(size.x, 0.08f, size.z);
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        visual.GetComponent<MeshRenderer>().sharedMaterial = _echoMat;

        SpawnPointLight(name + "_Glow", position + Vector3.up * 1.5f, new Color(0.16f, 0.85f, 1f), 2.5f, Mathf.Max(size.x, size.z) + 4f, zone.transform);

        return kZone;
    }

    static void BuildLevel06()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_06";
        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(new Color(0.01f, 0.01f, 0.02f, 1f), 0.07f, new Color(0.01f, 0.01f, 0.02f, 1f));
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 14f), 48f, 56f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 14f), 48f, 56f, env);

        // Start platform (y = 0)
        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(10f, 0.5f, 8f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -1.5f));

        // High exit ledge (y = 6)
        GameObject exitLedge = MakePlatform("ExitLedge", new Vector3(0f, 6f, 22f), new Vector3(6f, 0.5f, 6f), env, _floorMat);

        // Left wall structure that provides a physical wall-walking path
        GameObject leftWall = MakePlatform("LeftWall", new Vector3(-6f, 4f, 11f), new Vector3(1f, 8f, 14f), env, _bridgeMat);

        // Gravity zone on the left wall to flip gravity 90 degrees to the left (Vector3.left)
        CreateGravityZone("LeftWallGravity", new Vector3(-6f, 4f, 11f), new Vector3(1.2f, 8f, 14f), Vector3.left, 24f, 1, mech);

        // Kinetic platform block that lifts the player from y = 0 to y = 6
        PuzzleSignal blockSignal = CreatePuzzleSignal("Signal_Block", "Bloque Elevado", mech);
        CreateKineticBlock("PlatformBlock", new Vector3(0f, 0.25f, 11f), new Vector3(4f, 0.5f, 4f), new Vector3(-6f, 3.75f, 0f), new Vector3(1.2f, 4f, 4f), new Vector3(0f, 5.75f, 0f), mech, blockSignal, true, true, 3f);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 7.25f, 23.5f), mech, "Level_07");
        CreateSignalGoal(mech, "Entra en la zona de gravedad alterada en la pared izquierda y activa la plataforma.", "La gravedad es relativa en tu mente.", "Venciste a la perspectiva física.", exit, blockSignal);

        // Player starts on the floor with max 2 echoes
        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 2, 10f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 5f, -11f));

        CreateTutorialTrigger("Tut_L06_Gravedad", new Vector3(0f, 1f, 0f), new Vector3(8f, 3f, 6f),
            "Nivel 6 — El Laberinto Modular",
            "La zona violeta en la pared izquierda cambia tu gravedad al tocarla. Salta a la pared, camina por ella y graba un recorrido donde te pares en la zona azul de control. Como jugador, quédate parado sobre la plataforma flotante central en el suelo, y el eco te elevará.",
            10f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-6f, 4.1f, 11f),
            new Vector3(0f, 6.1f, 22f)
        });
        SpawnPuzzleIntent(mech, 0, 3, true, true, true, 18f, "Level 06: gravity alteration enabling horizontal wall-walking to raise central platform.");

        SpawnPointLight("Light_Wall", new Vector3(-4f, 6f, 11f), new Color(0.6f, 0.2f, 0.8f), 3f, 8f, env);
        SpawnPointLight("Light_Platform", new Vector3(0f, 3f, 11f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_Exit", new Vector3(0f, 9f, 23.5f), new Color(0.15f, 0.6f, 1f), 5f, 10f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Usa la gravedad de la pared para activar el elevador del suelo.", "La mente no tiene arriba ni abajo.", "Elevación completada.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 11f), 20f, 34f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.VerticalFall, 2f, 24f);

        SaveScene(scene, "Level_06");
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

        // Visual indicator (translucent violet box)
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
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 18f), 30f, 40f, env);

        // Start and Exit platforms
        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 2f), new Vector3(10f, 0.6f, 8f), env, _floorMat);
        GameObject exitPlat = MakePlatform("ExitPlatform", new Vector3(0f, 0f, 36f), new Vector3(10f, 0.6f, 8f), env, _floorMat);

        // High lateral gallery for the echo projection (y = 4)
        GameObject sideGallery = MakePlatform("SideGallery", new Vector3(-8f, 4f, 18f), new Vector3(3f, 0.5f, 26f), env, _floorMat);

        // Barrier walls and lethal void pits
        SpawnBarrierWall("Barrier_Gallery_L", new Vector3(-10f, 6f, 18f), new Vector3(0.5f, 4f, 26f), env);
        SpawnBarrierWall("Barrier_Gallery_R", new Vector3(-6.2f, 6f, 18f), new Vector3(0.5f, 4f, 26f), env);
        SpawnBarrierWall("Barrier_Corridor_R", new Vector3(5.5f, 3f, 18f), new Vector3(0.5f, 6f, 26f), env);
        SpawnBarrierWall("Barrier_Corridor_L", new Vector3(-2.2f, 3f, 18f), new Vector3(0.5f, 6f, 26f), env);

        SpawnKillZone("VoidPit_L", new Vector3(-4.2f, -4f, 18f), new Vector3(3.2f, 8f, 26f), mech);
        SpawnKillZone("VoidPit_R", new Vector3(3f, -4f, 18f), new Vector3(4.5f, 8f, 26f), mech);

        // Three sequential moving bridges over the pits, controlled by echo zones in the gallery
        PuzzleSignal bridge1Signal = CreatePuzzleSignal("Signal_Bridge1", "Puente 1 Alineado", mech);
        PuzzleSignal bridge2Signal = CreatePuzzleSignal("Signal_Bridge2", "Puente 2 Alineado", mech);
        PuzzleSignal bridge3Signal = CreatePuzzleSignal("Signal_Bridge3", "Puente 3 Alineado", mech);

        CreateKineticBlock("Bridge1", new Vector3(1.6f, -4f, 10f), new Vector3(3.4f, 0.5f, 6f), new Vector3(-9.6f, 8f, 0f), new Vector3(3f, 2f, 4f), new Vector3(0f, 4f, 0f), mech, bridge1Signal, true, true, 5f);
        CreateKineticBlock("Bridge2", new Vector3(1.6f, -4f, 18f), new Vector3(3.4f, 0.5f, 6f), new Vector3(-9.6f, 8f, 0f), new Vector3(3f, 2f, 4f), new Vector3(0f, 4f, 0f), mech, bridge2Signal, true, true, 5f);
        CreateKineticBlock("Bridge3", new Vector3(1.6f, -4f, 26f), new Vector3(3.4f, 0.5f, 6f), new Vector3(-9.6f, 8f, 0f), new Vector3(3f, 2f, 4f), new Vector3(0f, 4f, 0f), mech, bridge3Signal, true, true, 5f);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 1.25f, 38f), mech, "Level_09");
        
        PuzzleSignal runSignal = CreatePuzzleSignal("Signal_Run", "Corredor Cruzado", mech);
        CreateSignalGoal(mech, "Graba una proyección que active los tres puentes y corre a toda velocidad.", "La sombra corre adelante, el vacío viene detrás.", "Sobreviviste a la fuga sincronizada.", exit, runSignal);

        // Player starts with max 2 echoes and 9 seconds of recording time
        GameObject player = SpawnPlayer(new Vector3(1.6f, 1.1f, 2f), true, 2, 9f);
        SpawnGameplayCamera(player.transform, new Vector3(-7f, 4f, -11f));

        CreateTutorialTrigger("Tut_L08_Fuga", new Vector3(1.6f, 1f, 2f), new Vector3(6f, 3f, 4f),
            "Nivel 8 — La Fuga Sincronizada",
            "Una entidad oscura te perseguirá y no podrás detenerte. Mantén F y proyecta un recorrido que corra por la galería izquierda presionando las zonas 1, 2 y 3 consecutivamente. Suelta F, cruza el foso central corriendo y confía en tu eco.",
            10f, mech);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Graba la proyección en la galería y corre sin parar por el centro.", "Corre. Confía en tu pasado.", "Escape completado.");
        SpawnAmbientLights(env, new Vector3(0f, 4f, 18f), 22f, 34f);
        
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Chase, 2f, 38f, enableChase: true);
        
        GameObject relayTrigger = new GameObject("RunSignalTrigger");
        relayTrigger.transform.SetParent(mech, false);
        relayTrigger.transform.position = new Vector3(1.6f, 1.25f, 35f);
        BoxCollider triggerCol = relayTrigger.AddComponent<BoxCollider>();
        triggerCol.isTrigger = true;
        triggerCol.size = new Vector3(3f, 3f, 3f);
        EchoKineticZone rZone = relayTrigger.AddComponent<EchoKineticZone>();
        SetSerializedValue(rZone, "role", EchoKineticRole.TimingActivator);
        SetSerializedValue(rZone, "completionSignal", runSignal);
        SetSerializedValue(rZone, "requireEcho", false);
        SetSerializedValue(rZone, "acceptPlayer", true);

        SaveScene(scene, "Level_08");
    }

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

        // Two identical mirrored chambers (Left Chamber A and Right Chamber B)
        GameObject chamberA = MakePlatform("ChamberA", new Vector3(-5f, 0f, 14f), new Vector3(8f, 0.5f, 18f), env, _floorMat);
        GameObject chamberB = MakePlatform("ChamberB", new Vector3(5f, 0f, 14f), new Vector3(8f, 0.5f, 18f), env, _floorMat);

        // Transparent divider partition
        GameObject wall = MakePlatform("MirrorPartition", new Vector3(0f, 4f, 14f), new Vector3(0.15f, 8f, 18f), env, _bridgeMat);

        // Gate AB connecting Chamber A and Chamber B at z = 4
        DoorController gateAB = CreateDoor("GateAB", new Vector3(0f, 1.5f, 4f), new Vector3(3f, 3f, 0.5f), mech, new PressurePlate[0]);
        gateAB.latchOpen = false;

        // Plate 1 in Chamber A to open Gate AB
        PressurePlate startPlate = CreatePlateOnPlatform("StartPlate", chamberA, new Vector3(0f, 0f, -6f), mech, false);
        startPlate.autoReleaseTimer = 0.5f;

        // Wire Connection: startPlate opens gateAB
        GameObject wireObj = new GameObject("Wire_GateAB");
        wireObj.transform.SetParent(mech);
        PuzzleWire wire = wireObj.AddComponent<PuzzleWire>();
        wire.connections = new PuzzleWire.Connection[1];
        wire.connections[0] = new PuzzleWire.Connection {
            door = gateAB,
            plates = new[] { startPlate },
            logic = PuzzleWire.LogicMode.AND,
            latchOpen = false
        };

        // Plate 2 in Chamber A (mirror plate)
        PressurePlate mirrorPlate = CreatePlateOnPlatform("MirrorPlate", chamberA, new Vector3(0f, 0f, 6f), mech, false);
        mirrorPlate.autoReleaseTimer = 0.5f;

        // Exit gate in Chamber B, opened by mirrorPlate in Chamber A
        DoorController exitDoor = CreateDoor("ExitGate", new Vector3(5f, 1.5f, 20f), new Vector3(4f, 3f, 0.5f), mech, new PressurePlate[0]);
        exitDoor.latchOpen = false;

        // Wire Connection: mirrorPlate opens exitDoor
        GameObject wireExitObj = new GameObject("Wire_ExitDoor");
        wireExitObj.transform.SetParent(mech);
        PuzzleWire wireExit = wireExitObj.AddComponent<PuzzleWire>();
        wireExit.connections = new PuzzleWire.Connection[1];
        wireExit.connections[0] = new PuzzleWire.Connection {
            door = exitDoor,
            plates = new[] { mirrorPlate },
            logic = PuzzleWire.LogicMode.AND,
            latchOpen = false
        };

        // An elevator in Chamber B
        CreateMotorPlatform("ExitLift", new Vector3(5f, 0f, 20f), new Vector3(3f, 0.5f, 3f), Vector3.zero, new Vector3(0f, 4f, 0f), Vector3.zero, 5f, 0f, env, _bridgeMat);

        LevelExit exit = CreateLevelExit(new Vector3(5f, 5.25f, 22f), mech, "Level_10");
        CreateLevelGoal(mech, "Usa un eco con retardo temporal en la placa inicial para cruzar, luego sincronízate frente al espejo.", "El espejismo temporal se alinea.", "Espejo de memoria superado.", exit, startPlate, mirrorPlate);

        // Player starts in Chamber A (Left)
        GameObject player = SpawnPlayer(new Vector3(-5f, 1.1f, 4f), true, 2, 10f);
        SpawnGameplayCamera(player.transform, new Vector3(-7f, 4.5f, -10f));

        CreateTutorialTrigger("Tut_L09_Espejo", new Vector3(-5f, 1f, 4f), new Vector3(6f, 3f, 4f),
            "Nivel 9 — El Espejismo de la Memoria",
            "Para cruzar a la sala derecha, debes hacer que tu eco mantenga presionada la placa trasera durante 4 segundos antes de caminar hacia adelante. Graba ese recorrido, corre a través del portón central que abre tu eco, y pisa la placa del espejo en sincronía con tu pasado.",
            12f, mech);

        SpawnEchoPathHint(mech, new Vector3[] {
            new Vector3(-5f, 1.1f, 4f),
            new Vector3(-5f, 1.1f, 8f),
            new Vector3(5f, 1.1f, 8f),
            new Vector3(5f, 1.1f, 20f)
        });
        SpawnPuzzleIntent(mech, 2, 4, true, true, true, 22f, "Level 09: mirrored chambers requiring time-delayed temporal echoes and cross-boundary plate activations.");

        SpawnPointLight("Light_StartPlate", new Vector3(-5f, 3f, 8f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_MirrorPlate", new Vector3(-5f, 3f, 20f), new Color(0.24f, 0.76f, 1f), 2.5f, 8f, env);
        SpawnPointLight("Light_ExitDoor", new Vector3(5f, 3f, 20f), new Color(0.15f, 0.6f, 1f), 3f, 8f, env);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Cruza al lado derecho usando el eco y ábrete paso coordinadamente.", "Tu pasado es la llave al otro lado del espejo.", "El reflejo se ha completado.");
        SpawnAmbientLights(env, new Vector3(0f, 2f, 14f), 20f, 32f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.Standard, 2f, 32f);

        SaveScene(scene, "Level_09");
    }

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

        // Start platform at y = 0
        GameObject startPlat = MakePlatform("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(14f, 0.5f, 10f), env, _floorMat);
        SpawnIntroDressing(env, new Vector3(0f, 0f, -1.5f));

        // High middle tier at y = 6
        GameObject midTier = MakePlatform("MidTier", new Vector3(0f, 6f, 22f), new Vector3(10f, 0.5f, 10f), env, _floorMat);

        // Lower double gates at z = 6
        PressurePlate plateA = CreatePlateOnPlatform("PlateA", startPlat, new Vector3(-4f, 0f, 2f), mech, false);
        PressurePlate plateB = CreatePlateOnPlatform("PlateB", startPlat, new Vector3(4f, 0f, 2f), mech, false);
        DoorController lowerGate = CreateDoor("LowerGate", new Vector3(0f, 1.5f, 6f), new Vector3(6f, 3f, 0.5f), mech, new[] { plateA, plateB });
        lowerGate.latchOpen = true;

        // Timed elevator platform inside the gate at z = 12, rising to y = 6
        PressurePlate plateElevator = CreatePlate("PlateElevator", new Vector3(0f, 0.36f, 8f), mech);
        plateElevator.autoReleaseTimer = 1.5f;
        TimedMovingPlatform elevator = CreateBridge("MainElevator", new Vector3(0f, 0f, 14f), Vector3.zero, new Vector3(0f, 6f, 0f), new Vector3(4f, 0.5f, 4f), plateElevator, mech);

        // High lateral catwalk for echo shielding (y = 6, x = 6, z = 22)
        GameObject sideCatwalk = MakePlatform("SideCatwalk", new Vector3(6f, 6f, 22f), new Vector3(3f, 0.5f, 10f), env, _bridgeMat);

        // Lethal Hazard Field blocking the exit approach at y = 6, z = 28
        PuzzleSignal shieldSignal = CreatePuzzleSignal("Signal_Shield", "Puerta de Energía Final", mech);
        CreateHazardField("FinalHazard", new Vector3(0f, 7.5f, 28f), new Vector3(8f, 3f, 1.2f), mech, shieldSignal);

        LevelExit exit = CreateLevelExit(new Vector3(0f, 7.25f, 32f), mech, "MainMenu");
        CreateLevelGoal(mech, "Usa todos los ecos en secuencia para abrir la compuerta doble, subir el ascensor y neutralizar la barrera final.", "El archivo del núcleo modular se alinea.", "Bienvenido al archivo de memoria.", exit, plateA, plateB, plateElevator);

        // Player starts with max 3 echoes and 18 seconds of recording time
        GameObject player = SpawnPlayer(new Vector3(0f, 1.1f, 0f), true, 3, 18f);
        SpawnGameplayCamera(player.transform, new Vector3(-8f, 4f, -11f));

        CreateTutorialTrigger("Tut_L10_Catedral", new Vector3(0f, 1f, 0f), new Vector3(10f, 3f, 6f),
            "Nivel 10 — El Eco Final",
            "Esta es la catedral de la memoria. Sincroniza dos ecos en las placas A y B para abrir el portón inicial. Luego, usa tu tercer eco para pisar la placa del ascensor, subir y neutralizar el muro rojo final.",
            12f, mech);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, "Activa los portones, el ascensor y la barrera final con tus ecos.", "El núcleo final espera la armonía completa.", "Enlace del núcleo restaurado.");
        SpawnAmbientLights(env, new Vector3(0f, 3f, 18f), 24f, 40f);
        SpawnExperienceSystems(mech, env, exit, LevelArchetype.MultiLayerTimeline, 2f, 32f);

        SaveScene(scene, "Level_10");
    }
"""

# Find start index
start_idx = content.find("static void BuildLevel07()")
if start_idx == -1:
    start_idx = content.find("    static void BuildLevel07()")

# Find end index after BuildLevel10 exit
end_idx = content.find('SaveScene(scene, "Level_10");\r\n    }')
if end_idx == -1:
    end_idx = content.find('SaveScene(scene, "Level_10");\n    }')

if start_idx != -1 and end_idx != -1:
    end_idx += len('SaveScene(scene, "Level_10");\n    }')
    content = content[:start_idx] + levels_code.strip() + "\n" + content[end_idx:]
    print("Indentation-aware boundary replacement succeeded!")
else:
    print(f"Could not find boundaries! start_idx={start_idx}, end_idx={end_idx}")

with open(path, "w", encoding="utf-8") as f:
    f.write(content)

print("EchoesProductionBuilder.cs updated successfully.")
