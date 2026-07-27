using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EchoesBlueprintExporter
{
    private const string BlueprintRoot = "Assets/Data/Levels";

    [MenuItem("Echoes of You/Migration/Export Levels 1-5 to Blueprints", false, 301)]
    public static void ExportLevels()
    {
        EnsureFolderExists(BlueprintRoot);

        ExportLevel01();
        ExportLevel02();
        ExportLevel03();
        ExportLevel04();
        ExportLevel05();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Echoes Migration] Exported Levels 1-5 to blueprints successfully.");
    }

    private static void ExportLevel01()
    {
        LevelBlueprint bp = ScriptableObject.CreateInstance<LevelBlueprint>();
        bp.levelName = "Level_01";
        bp.nextLevel = "Level_02";
        bp.actNumber = 1;
        bp.archetype = LevelArchetype.Standard;
        bp.maxEchoes = 1;
        bp.maxRecordSeconds = 12f;

        bp.fogColor = new Color(0.04f, 0.04f, 0.05f, 1f); // void-black
        bp.fogDensity = 0.035f;
        bp.skyColor = new Color(0.06f, 0.08f, 0.12f, 1f);
        bp.ambientColor = new Color(0.11f, 0.14f, 0.19f, 1f);

        bp.narrativeIntroTitle = "Nivel 1 — Desorientación";
        bp.narrativeIntroDesc = "Un porche silencioso te da la bienvenida a un pasillo escolar sin fin. La memoria repite su estructura.";
        bp.narrativeIntroDuration = 6f;
        bp.puzzleObjectiveText = "Camina a través del umbral de la memoria.";
        bp.puzzleActiveText = "La repetición espacial oculta la salida.";
        bp.puzzleCompleteText = "Desorientación superada.";

        bp.pathHints = new[] {
            new Vector3(0f, 0.1f, -3f),
            new Vector3(0f, 0.1f, 10f),
            new Vector3(0f, 0.1f, 30f),
            new Vector3(0f, 0.1f, 52f)
        };

        bp.modules = new List<ModulePlacement>
        {
            // Zona A - Porche Entrada (z: -6 a 0)
            new ModulePlacement { name = "PorchHall", type = ModuleType.SchoolHall, position = new Vector3(0f, 0f, -3f), scale = new Vector3(8f, 3f, 6f) },
            new ModulePlacement { name = "PlayerStart", type = ModuleType.PlayerStart, position = new Vector3(0f, 0.1f, -5f) },
            
            // Zona B - Pasillo A (z: 0 a 20) — WallTealMat & Flicker
            new ModulePlacement { name = "CorridorA", type = ModuleType.SchoolCorridor, position = new Vector3(0f, 0f, 10f), scale = new Vector3(6f, 3f, 20f), customData = "flicker=true" },
            new ModulePlacement { name = "PlateA", type = ModuleType.PressurePlate, position = new Vector3(0f, 0.05f, 15f), scale = Vector3.one },
            
            // Zona C - Pasillo B (z: 20 a 40) — Idéntico geom, sin flicker
            new ModulePlacement { name = "CorridorB", type = ModuleType.SchoolCorridor, position = new Vector3(0f, 0f, 30f), scale = new Vector3(6f, 3f, 20f), customData = "flicker=false" },
            
            // Zona D - Umbral Final (z: 52)
            new ModulePlacement { name = "LiminalThreshold", type = ModuleType.TransitionSpace, position = new Vector3(0f, 0f, 48f), scale = new Vector3(8f, 3.5f, 8f) },
            new ModulePlacement { name = "ExitGate", type = ModuleType.Door, position = new Vector3(0f, 0f, 52f), scale = new Vector3(4f, 3f, 0.5f), targetSignals = new[] { "PlateA" } },
            new ModulePlacement { name = "LevelExit_Area", type = ModuleType.LevelExit, position = new Vector3(0f, 0.1f, 54f), scale = Vector3.one, customData = "Level_02" },
            
            // Runtime & Goals
            new ModulePlacement { name = "LevelGoal", type = ModuleType.LevelGoal, position = new Vector3(0f, 0.1f, 54f), targetSignals = new[] { "PlateA" }, customData = "Camina a través del pasillo y abre el umbral con tu eco.|La repetición espacial oculta la salida.|Desorientación superada." },
            new ModulePlacement { name = "LevelRuntime", type = ModuleType.LevelRuntime, customData = "Avanza por el pasillo.|El pasado se duplica.|Salida alcanzada." }
        };

        SaveBlueprint(bp, "Level_01_Blueprint");
    }

    private static void ExportLevel02()
    {
        LevelBlueprint bp = ScriptableObject.CreateInstance<LevelBlueprint>();
        bp.levelName = "Level_02";
        bp.nextLevel = "Level_03";
        bp.actNumber = 1;
        bp.archetype = LevelArchetype.Standard;
        bp.maxEchoes = 2;
        bp.maxRecordSeconds = 14f;

        bp.fogColor = new Color(0.04f, 0.05f, 0.08f, 1f);
        bp.fogDensity = 0.03f;
        bp.skyColor = new Color(0.04f, 0.05f, 0.08f, 1f);
        bp.ambientColor = new Color(0.1f, 0.12f, 0.16f, 1f);

        bp.narrativeIntroTitle = "Nivel 2 — Repetición";
        bp.narrativeIntroDesc = "Tres aulas consecutivas. Mismo espacio, pero la desorganización de la memoria altera sus mecanismos.";
        bp.narrativeIntroDuration = 6f;
        bp.puzzleObjectiveText = "Coordina las placas de presión en las aulas progresivas.";
        bp.puzzleActiveText = "La repetición exige mayor sincronización.";
        bp.puzzleCompleteText = "Repetición superada.";

        bp.pathHints = new[] {
            new Vector3(0f, 0.1f, 0f),
            new Vector3(0f, 0.1f, 16f),
            new Vector3(0f, 0.1f, 32f),
            new Vector3(0f, 0.1f, 48f)
        };

        bp.modules = new List<ModulePlacement>
        {
            // Hall Entrada (z: 0)
            new ModulePlacement { name = "EntranceHall", type = ModuleType.SchoolHall, position = new Vector3(0f, 0f, 0f), scale = new Vector3(10f, 3f, 8f) },
            new ModulePlacement { name = "PlayerStart", type = ModuleType.PlayerStart, position = new Vector3(0f, 0.1f, -2f) },

            // Aula 1 — Ordenada (z: 16) — 1 placa
            new ModulePlacement { name = "Classroom1", type = ModuleType.SchoolClassroom, position = new Vector3(0f, 0f, 16f), scale = new Vector3(10f, 3f, 12f), customData = "style=ordered" },
            new ModulePlacement { name = "Plate1", type = ModuleType.PressurePlate, position = new Vector3(-2f, 0.05f, 16f) },
            new ModulePlacement { name = "Door1", type = ModuleType.Door, position = new Vector3(0f, 0f, 22f), scale = new Vector3(4f, 3f, 0.5f), targetSignals = new[] { "Plate1" } },

            // Aula 2 — Corrida (z: 32) — 2 placas simultáneas
            new ModulePlacement { name = "Classroom2", type = ModuleType.SchoolClassroom, position = new Vector3(0f, 0f, 32f), scale = new Vector3(10f, 3f, 12f), customData = "style=disarray" },
            new ModulePlacement { name = "Plate2A", type = ModuleType.PressurePlate, position = new Vector3(-3f, 0.05f, 32f) },
            new ModulePlacement { name = "Plate2B", type = ModuleType.PressurePlate, position = new Vector3(3f, 0.05f, 32f) },
            new ModulePlacement { name = "Door2", type = ModuleType.Door, position = new Vector3(0f, 0f, 38f), scale = new Vector3(4f, 3f, 0.5f), targetSignals = new[] { "Plate2A", "Plate2B" } },

            // Aula 3 — Desordenada (z: 48) — Timing
            new ModulePlacement { name = "Classroom3", type = ModuleType.SchoolClassroom, position = new Vector3(0f, 0f, 48f), scale = new Vector3(10f, 3f, 12f), customData = "style=chaos" },
            new ModulePlacement { name = "Plate3", type = ModuleType.PressurePlate, position = new Vector3(0f, 0.05f, 48f) },
            new ModulePlacement { name = "ExitGate", type = ModuleType.Door, position = new Vector3(0f, 0f, 54f), scale = new Vector3(4f, 3f, 0.5f), targetSignals = new[] { "Plate3" } },
            new ModulePlacement { name = "LevelExit_Area", type = ModuleType.LevelExit, position = new Vector3(0f, 0.1f, 56f), customData = "Level_03" },

            // Goals
            new ModulePlacement { name = "LevelGoal", type = ModuleType.LevelGoal, position = new Vector3(0f, 0.1f, 56f), targetSignals = new[] { "Plate3" }, customData = "Coordina las tres aulas.|El ritmo de la memoria de Lyra se sostiene.|Repetición superada." },
            new ModulePlacement { name = "LevelRuntime", type = ModuleType.LevelRuntime, customData = "Avanza por las tres aulas.|Coordina tu pasado.|Aula final alcanzada." }
        };

        SaveBlueprint(bp, "Level_02_Blueprint");
    }

    private static void ExportLevel03()
    {
        LevelBlueprint bp = ScriptableObject.CreateInstance<LevelBlueprint>();
        bp.levelName = "Level_03";
        bp.nextLevel = "Level_04";
        bp.actNumber = 1;
        bp.archetype = LevelArchetype.Standard;
        bp.maxEchoes = 1;
        bp.maxRecordSeconds = 10f;

        bp.fogColor = new Color(0.02f, 0.02f, 0.04f, 1f);
        bp.fogDensity = 0.06f;
        bp.skyColor = new Color(0.02f, 0.02f, 0.04f, 1f);

        bp.narrativeIntroTitle = "Nivel 3 — La Paradoja del Conflicto";
        bp.narrativeIntroDesc = "Tu eco en la cámara izquierda desactiva la barrera roja, pero estar ahí activa una trampa que cierra la puerta final. Coordina los tiempos de salida de tu eco.";
        bp.narrativeIntroDuration = 10f;
        bp.puzzleObjectiveText = "Neutraliza el muro de energía sin activar la trampa de conflicto al salir.";
        bp.puzzleActiveText = "La paradoja temporal se activa.";
        bp.puzzleCompleteText = "Paradoja superada.";

        bp.pathHints = new[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-8f, 1.1f, 12f),
            new Vector3(0f, 1.1f, 12f),
            new Vector3(0f, 1.1f, 26f)
        };

        bp.modules = new List<ModulePlacement>
        {
            new ModulePlacement { name = "StartPlatform", type = ModuleType.StandardPlatform, position = new Vector3(0f, 0f, 0f), scale = new Vector3(10f, 0.5f, 8f) },
            new ModulePlacement { name = "Corridor", type = ModuleType.BridgePlatform, position = new Vector3(0f, 0f, 12f), scale = new Vector3(4f, 0.5f, 16f) },
            new ModulePlacement { name = "ExitPlatform", type = ModuleType.StandardPlatform, position = new Vector3(0f, 0f, 24f), scale = new Vector3(10f, 0.5f, 8f) },
            new ModulePlacement { name = "ControlChamber", type = ModuleType.StandardPlatform, position = new Vector3(-8f, 0f, 12f), scale = new Vector3(6f, 0.5f, 6f) },
            
            new ModulePlacement { name = "ControlPlate", type = ModuleType.PressurePlate, position = new Vector3(-8f, 0.33f, 12f) },
            new ModulePlacement { name = "Signal_Shield", type = ModuleType.PuzzleSignal, customData = "Energía Neutralizada|false|false" },
            new ModulePlacement { name = "Signal_Trap", type = ModuleType.PuzzleSignal, customData = "Trampa Paradoja|false|false" },
            
            new ModulePlacement { name = "Cond_Shield", type = ModuleType.PuzzleCondition, targetSignals = new[] { "ControlPlate", "Signal_Shield" }, customData = "AllPlatesSimultaneous" },
            new ModulePlacement { name = "Muro_Energia", type = ModuleType.HazardField, position = new Vector3(0f, 1.5f, 8f), scale = new Vector3(4f, 3f, 1.2f), targetSignals = new[] { "Signal_Shield" } },
            new ModulePlacement { name = "ExitGate", type = ModuleType.Door, position = new Vector3(0f, 1.5f, 18f), scale = new Vector3(4f, 3f, 0.5f) },
            new ModulePlacement { name = "ControlTrap", type = ModuleType.ConflictTrap, position = new Vector3(-8f, 1.5f, 12f), scale = new Vector3(5f, 3f, 5f), targetSignals = new[] { "ExitGate", "Signal_Trap" } },
            
            new ModulePlacement { name = "LevelExit_Area", type = ModuleType.LevelExit, position = new Vector3(0f, 1.25f, 26f), customData = "Level_04" },
            new ModulePlacement { name = "PlayerStart", type = ModuleType.PlayerStart, position = new Vector3(0f, 1.1f, 0f) },
            new ModulePlacement { name = "Tut_L03_Paradoja", type = ModuleType.TutorialTrigger, position = new Vector3(0f, 1f, 0f), scale = new Vector3(10f, 3f, 8f), customData = "Nivel 3 — La Paradoja del Conflicto|Tu eco en la cámara izquierda desactiva la barrera roja, pero estar ahí activa una trampa que cierra la puerta final. Graba un recorrido donde el eco pise la placa unos segundos, luego SALGA de la cámara izquierda. Cruza la barrera y espera a que el eco salga para cruzar la compuerta." },
            
            new ModulePlacement { name = "Light_ControlChamber", type = ModuleType.PointLight, position = new Vector3(-8f, 3f, 12f), customData = "3D7CFF,3,8" },
            new ModulePlacement { name = "Light_Hazard", type = ModuleType.PointLight, position = new Vector3(0f, 3f, 8f), customData = "FF2A14,3.5,10" },
            new ModulePlacement { name = "Light_Exit", type = ModuleType.PointLight, position = new Vector3(0f, 4f, 26f), customData = "2699FF,5,10" },
            new ModulePlacement { name = "DistantArch", type = ModuleType.DistantArchitecture, position = new Vector3(0f, 0f, 14f) },
            new ModulePlacement { name = "LevelGoal", type = ModuleType.LevelGoal, position = new Vector3(0f, 1.25f, 26f), targetSignals = new[] { "Signal_Shield", "Signal_Trap" }, customData = "Neutraliza el muro de energía sin activar la trampa de conflicto al salir.|La paradoja temporal se activa.|Paradoja superada." },
            new ModulePlacement { name = "LevelRuntime", type = ModuleType.LevelRuntime, customData = "Neutraliza la barrera de energía y evita la trampa de conflicto.|El eco es tu llave y tu prisión.|Acceso libre." }
        };

        SaveBlueprint(bp, "Level_03_Blueprint");
    }

    private static void ExportLevel04()
    {
        LevelBlueprint bp = ScriptableObject.CreateInstance<LevelBlueprint>();
        bp.levelName = "Level_04";
        bp.nextLevel = "Level_05";
        bp.actNumber = 1;
        bp.archetype = LevelArchetype.Standard;
        bp.maxEchoes = 2;
        bp.maxRecordSeconds = 10f;

        bp.fogColor = new Color(0.1f, 0.12f, 0.18f, 1f);
        bp.fogDensity = 0.01f;
        bp.skyColor = new Color(0.18f, 0.2f, 0.28f, 1f);

        bp.narrativeIntroTitle = "Nivel 4 — La Jaula de Presión";
        bp.narrativeIntroDesc = "Las placas deben ser pisadas en un orden específico (A -> B -> C). Coordina tus grabaciones secuenciales.";
        bp.narrativeIntroDuration = 10f;
        bp.puzzleObjectiveText = "Activa las placas en el orden exacto: Izquierda-Atrás, Derecha, Izquierda-Adelante.";
        bp.puzzleActiveText = "Las tres memorias deben sonar en armonía.";
        bp.puzzleCompleteText = "Sinfonía secuencial completada.";

        bp.pathHints = new[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-6f, 1.1f, 10f),
            new Vector3(6f, 1.1f, 15f),
            new Vector3(-6f, 1.1f, 20f)
        };

        bp.modules = new List<ModulePlacement>
        {
            new ModulePlacement { name = "StartPlatform", type = ModuleType.StandardPlatform, position = new Vector3(0f, 0f, 0f), scale = new Vector3(12f, 0.5f, 10f) },
            new ModulePlacement { name = "ExitPlatform", type = ModuleType.StandardPlatform, position = new Vector3(0f, 0f, 30f), scale = new Vector3(12f, 0.5f, 10f) },
            new ModulePlacement { name = "PlatePlatA", type = ModuleType.BridgePlatform, position = new Vector3(-6f, 0f, 10f), scale = new Vector3(3f, 0.5f, 3f) },
            new ModulePlacement { name = "PlatePlatB", type = ModuleType.BridgePlatform, position = new Vector3(6f, 0f, 15f), scale = new Vector3(3f, 0.5f, 3f) },
            new ModulePlacement { name = "PlatePlatC", type = ModuleType.BridgePlatform, position = new Vector3(-6f, 0f, 20f), scale = new Vector3(3f, 0.5f, 3f) },
            
            new ModulePlacement { name = "PlateA", type = ModuleType.PressurePlate, position = new Vector3(-6f, 0.33f, 10f) },
            new ModulePlacement { name = "PlateB", type = ModuleType.PressurePlate, position = new Vector3(6f, 0.33f, 15f) },
            new ModulePlacement { name = "PlateC", type = ModuleType.PressurePlate, position = new Vector3(-6f, 0.33f, 20f) },
            
            new ModulePlacement { name = "Rotating_Cross", type = ModuleType.MotorPlatform, position = new Vector3(0f, 0.25f, 15f), scale = new Vector3(10f, 0.35f, 1.2f), customData = "0,0,0|0,0,0|0,45,0|1|0" },
            new ModulePlacement { name = "ExitGate", type = ModuleType.Door, position = new Vector3(0f, 1.5f, 25f), scale = new Vector3(6f, 3f, 0.5f) },
            
            new ModulePlacement { name = "Signal_Sequence", type = ModuleType.PuzzleSignal, customData = "Secuencia Resuelta|false|false" },
            new ModulePlacement { name = "Condition_Sequential", type = ModuleType.PuzzleCondition, targetSignals = new[] { "PlateA", "PlateB", "PlateC", "Signal_Sequence", "ExitGate" }, customData = "SequentialOrder|Enlace secuencia|Secuencia correcta! Acceso concedido.|Secuencia rota! Intenta de nuevo." },
            
            new ModulePlacement { name = "LevelExit_Area", type = ModuleType.LevelExit, position = new Vector3(0f, 1.25f, 32f), customData = "Level_05" },
            new ModulePlacement { name = "PlayerStart", type = ModuleType.PlayerStart, position = new Vector3(0f, 1.1f, 0f) },
            new ModulePlacement { name = "Tut_L04_Secuencia", type = ModuleType.TutorialTrigger, position = new Vector3(0f, 1f, 0f), scale = new Vector3(10f, 3f, 8f), customData = "Nivel 4 — La Jaula de Presión|Las placas deben ser pisadas en un orden específico (A -> B -> C). Graba un recorrido donde pises la placa A (izquierda posterior) y luego la B (derecha). Como jugador, corre y pisa la placa C (izquierda anterior) justo cuando tu eco pise la B." },
            
            new ModulePlacement { name = "Light_PlateA", type = ModuleType.PointLight, position = new Vector3(-6f, 3f, 10f), customData = "4CC0FF,2.5,6" },
            new ModulePlacement { name = "Light_PlateB", type = ModuleType.PointLight, position = new Vector3(6f, 3f, 15f), customData = "4CC0FF,2.5,6" },
            new ModulePlacement { name = "Light_PlateC", type = ModuleType.PointLight, position = new Vector3(-6f, 3f, 20f), customData = "4CC0FF,2.5,6" },
            new ModulePlacement { name = "Light_Exit", type = ModuleType.PointLight, position = new Vector3(0f, 5f, 32f), customData = "2699FF,5,10" },
            
            new ModulePlacement { name = "DistantArch", type = ModuleType.DistantArchitecture, position = new Vector3(0f, 0f, 20f) },
            new ModulePlacement { name = "LevelGoal", type = ModuleType.LevelGoal, position = new Vector3(0f, 1.25f, 32f), targetSignals = new[] { "Signal_Sequence" }, customData = "Activa las placas en el orden exacto: Izquierda-Atrás, Derecha, Izquierda-Adelante.|Las tres memorias deben sonar en armonía.|Sinfonía secuencial completada." },
            new ModulePlacement { name = "LevelRuntime", type = ModuleType.LevelRuntime, customData = "Pisa las tres placas en la secuencia correcta (A -> B -> C).|La máquina requiere un orden exacto.|La secuencia ha sido grabada." }
        };

        SaveBlueprint(bp, "Level_04_Blueprint");
    }

    private static void ExportLevel05()
    {
        LevelBlueprint bp = ScriptObjectCreateInstance();
        bp.levelName = "Level_05";
        bp.nextLevel = "Level_06";
        bp.actNumber = 1;
        bp.archetype = LevelArchetype.Standard;
        bp.maxEchoes = 2;
        bp.maxRecordSeconds = 8f;

        bp.fogColor = new Color(0.015f, 0.02f, 0.03f, 1f);
        bp.fogDensity = 0.06f;
        bp.skyColor = new Color(0.015f, 0.02f, 0.03f, 1f);

        bp.narrativeIntroTitle = "Nivel 5 — La Cortina Inestable";
        bp.narrativeIntroDesc = "Sube a la cornisa izquierda de control y proyecta tu eco a través de la barrera roja. Usa el impulso cinético.";
        bp.narrativeIntroDuration = 10f;
        bp.puzzleObjectiveText = "Cruza la fractura neutralizando la barrera y usando el impulso cinético.";
        bp.puzzleActiveText = "La barrera cede temporalmente.";
        bp.puzzleCompleteText = "Salto de fe completado.";

        bp.pathHints = new[] {
            new Vector3(0f, 1.1f, -1f),
            new Vector3(-8f, 5.1f, 6f),
            new Vector3(0f, 1.1f, 8f),
            new Vector3(0f, 1.1f, 26f)
        };

        bp.modules = new List<ModulePlacement>
        {
            new ModulePlacement { name = "StartPlatform", type = ModuleType.StandardPlatform, position = new Vector3(0f, 0f, 0f), scale = new Vector3(8f, 0.5f, 6f) },
            new ModulePlacement { name = "ExitPlatform", type = ModuleType.StandardPlatform, position = new Vector3(0f, 0f, 26f), scale = new Vector3(8f, 0.5f, 6f) },
            new ModulePlacement { name = "ControlLedge", type = ModuleType.StandardPlatform, position = new Vector3(-8f, 4f, 6f), scale = new Vector3(4f, 0.5f, 4f) },
            new ModulePlacement { name = "ControlRamp", type = ModuleType.RampPlatform, position = new Vector3(-4.5f, 2f, 3f), scale = new Vector3(2f, 0.45f, 8f), rotation = new Vector3(22f, 0f, 0f) },
            new ModulePlacement { name = "Float_1", type = ModuleType.BridgePlatform, position = new Vector3(0f, 0f, 8f), scale = new Vector3(3f, 0.5f, 3f) },
            new ModulePlacement { name = "Float_2", type = ModuleType.BridgePlatform, position = new Vector3(0f, 0f, 18f), scale = new Vector3(3f, 0.5f, 3f) },
            
            new ModulePlacement { name = "Signal_Shield", type = ModuleType.PuzzleSignal, customData = "Energía Neutralizada|false|false" },
            new ModulePlacement { name = "Hazard_Curtain", type = ModuleType.HazardField, position = new Vector3(0f, 2f, 13f), scale = new Vector3(8f, 4f, 1.2f), targetSignals = new[] { "Signal_Shield" } },
            
            new ModulePlacement { name = "RelayTarget", type = ModuleType.PlayerStart, position = new Vector3(0f, 1f, 26f) }, // Spawn point used as target
            new ModulePlacement { name = "Boost_Float1", type = ModuleType.MomentumRelay, position = new Vector3(0f, 0f, 8f), scale = new Vector3(3f, 2f, 3f), targetSignals = new[] { "RelayTarget" }, customData = "14" },
            
            new ModulePlacement { name = "LevelExit_Area", type = ModuleType.LevelExit, position = new Vector3(0f, 1.25f, 28f), customData = "Level_06" },
            new ModulePlacement { name = "PlayerStart", type = ModuleType.PlayerStart, position = new Vector3(0f, 1.1f, -1f) },
            new ModulePlacement { name = "Tut_L05_Cortina", type = ModuleType.TutorialTrigger, position = new Vector3(0f, 1f, -1f), scale = new Vector3(8f, 3f, 4f), customData = "Nivel 5 — La Cortina Inestable|Sube a la cornisa izquierda de control y proyecta tu eco a través de la barrera roja. Luego, como jugador, corre y salta a la plataforma flotante central. Cuando el eco pase la barrera, esta se volverá azul y la plataforma te impulsará al final." },
            
            new ModulePlacement { name = "Light_ControlLedge", type = ModuleType.PointLight, position = new Vector3(-8f, 6f, 6f), customData = "59CCFF,3,8" },
            new ModulePlacement { name = "Light_Hazard", type = ModuleType.PointLight, position = new Vector3(0f, 4f, 13f), customData = "FF2A14,3.5,10" },
            new ModulePlacement { name = "Light_Exit", type = ModuleType.PointLight, position = new Vector3(0f, 5f, 28f), customData = "2699FF,5,10" },
            
            new ModulePlacement { name = "DistantArch", type = ModuleType.DistantArchitecture, position = new Vector3(0f, 0f, 16f) },
            new ModulePlacement { name = "LevelGoal", type = ModuleType.LevelGoal, position = new Vector3(0f, 1.25f, 28f), targetSignals = new[] { "Signal_Shield" }, customData = "Cruza la fractura neutralizando la barrera y usando el impulso cinético.|La barrera cede temporalmente.|Salto de fe completado." },
            new ModulePlacement { name = "LevelRuntime", type = ModuleType.LevelRuntime, customData = "Cruza la barrera usando el eco para neutralizarla y ganar impulso.|El eco es tu escudo y tu motor.|Cruce exitoso." }
        };

        SaveBlueprint(bp, "Level_05_Blueprint");
    }

    private static void ExportLevel06()
    {
        LevelBlueprint bp = ScriptableObject.CreateInstance<LevelBlueprint>();
        bp.levelName = "Level_06";
        bp.nextLevel = "Level_07";
        bp.actNumber = 3;
        bp.archetype = LevelArchetype.Standard;
        bp.maxEchoes = 1;
        bp.maxRecordSeconds = 12f;
        bp.degradationPerReplay = 0.02f;

        bp.fogColor = new Color(0.04f, 0.06f, 0.05f, 1f); // sage-green dark fog
        bp.fogDensity = 0.035f;
        bp.skyColor = new Color(0.06f, 0.08f, 0.07f, 1f);
        bp.ambientColor = new Color(0.1f, 0.14f, 0.12f, 1f);

        bp.narrativeIntroTitle = "Nivel 6 — Negación / Salto de Fe";
        bp.narrativeIntroDesc = "La biblioteca se fragmenta ante un abismo de 8 metros. El puente solo se materializa en el plano temporal del eco.";
        bp.narrativeIntroDuration = 6f;
        bp.puzzleObjectiveText = "Materializa el puente espectral mediante el eco para cruzar el abismo.";
        bp.puzzleActiveText = "El puente temporal sostiene la pisada del presente.";
        bp.puzzleCompleteText = "Salto de fe completado.";

        bp.pathHints = new[] {
            new Vector3(0f, 0.1f, 0f),
            new Vector3(0f, 0.1f, 12f),
            new Vector3(0f, 0.1f, 24f)
        };

        bp.modules = new List<ModulePlacement>
        {
            // Zona A - Entrada Biblioteca (z: 0)
            new ModulePlacement { name = "LibraryEntrance", type = ModuleType.SchoolLibrary, position = new Vector3(0f, 0f, 0f), scale = new Vector3(8f, 3.5f, 6f) },
            new ModulePlacement { name = "PlayerStart", type = ModuleType.PlayerStart, position = new Vector3(0f, 0.1f, -2f) },

            // Zona B - Pasillo Estanterías / Learning Zone (z: 10) — Abismo 2m seguro
            new ModulePlacement { name = "LibraryShelfZone", type = ModuleType.SchoolLibrary, position = new Vector3(0f, 0f, 10f), scale = new Vector3(8f, 3.5f, 6f) },
            new ModulePlacement { name = "SmallChasmBridge", type = ModuleType.TemporalBridge, position = new Vector3(0f, 0f, 13f), scale = new Vector3(3f, 0.5f, 2f) },

            // Zona C - Abismo Principal / Puzzle Real (z: 20) — Abismo 8m + TemporalBridge
            new ModulePlacement { name = "MainChasmBridge", type = ModuleType.TemporalBridge, position = new Vector3(0f, 0f, 22f), scale = new Vector3(4f, 0.5f, 8f) },
            new ModulePlacement { name = "BridgePlate", type = ModuleType.PressurePlate, position = new Vector3(4f, 0.05f, 18f) },

            // Zona D - Llegada / Espejo (z: 32)
            new ModulePlacement { name = "LibraryArrival", type = ModuleType.SchoolLibrary, position = new Vector3(0f, 0f, 32f), scale = new Vector3(8f, 3.5f, 6f) },
            new ModulePlacement { name = "ExitGate", type = ModuleType.Door, position = new Vector3(0f, 0f, 35f), scale = new Vector3(4f, 3f, 0.5f), targetSignals = new[] { "BridgePlate" } },
            new ModulePlacement { name = "LevelExit_Area", type = ModuleType.LevelExit, position = new Vector3(0f, 0.1f, 37f), customData = "Level_07" },

            // Goals
            new ModulePlacement { name = "LevelGoal", type = ModuleType.LevelGoal, position = new Vector3(0f, 0.1f, 37f), targetSignals = new[] { "BridgePlate" }, customData = "Cruza el abismo utilizando el puente espectral.|La estructura temporal responde.|Salto de fe completado." },
            new ModulePlacement { name = "LevelRuntime", type = ModuleType.LevelRuntime, customData = "Graba tu movimiento para proyectar el puente.|Confía en la memoria.|Abismo cruzado." }
        };

        SaveBlueprint(bp, "Level_06_Blueprint");
    }

    private static LevelBlueprint ScriptObjectCreateInstance()
    {
        return ScriptableObject.CreateInstance<LevelBlueprint>();
    }

    private static void SaveBlueprint(LevelBlueprint bp, string name)
    {
        string path = $"{BlueprintRoot}/{name}.asset";
        AssetDatabase.CreateAsset(bp, path);
        Debug.Log($"[Echoes Migration] Exported: {path}");
    }

    private static void EnsureFolderExists(string path)
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
}
