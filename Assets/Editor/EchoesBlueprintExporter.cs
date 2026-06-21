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

        bp.fogColor = new Color(0.12f, 0.14f, 0.2f, 1f);
        bp.fogDensity = 0.008f;
        bp.skyColor = new Color(0.2f, 0.22f, 0.28f, 1f);

        bp.narrativeIntroTitle = "Nivel 1 — Despertar Simétrico";
        bp.narrativeIntroDesc = "Baja por la rampa al nivel inferior. Graba un eco parado sobre la placa para abrir la compuerta superior. Regresa rápido y crúzala mientras tu eco mantiene el portón abierto.";
        bp.narrativeIntroDuration = 10f;
        bp.puzzleObjectiveText = "Activa la placa inferior proyectando un eco y cruza antes de que baje la compuerta.";
        bp.puzzleActiveText = "El eco del portal responde en la simulación.";
        bp.puzzleCompleteText = "Fractura simétrica superada.";

        bp.pathHints = new[] {
            new Vector3(0f, 7.1f, -1f),
            new Vector3(0f, 1.1f, 18f),
            new Vector3(0f, 7.1f, -6f)
        };

        bp.modules = new List<ModulePlacement>
        {
            new ModulePlacement { name = "StartPlatform", type = ModuleType.StandardPlatform, position = new Vector3(0f, 6f, 0f), scale = new Vector3(8f, 0.5f, 8f) },
            new ModulePlacement { name = "BarrierStart_L", type = ModuleType.BarrierWall, position = new Vector3(-3.8f, 7.5f, 0f), scale = new Vector3(0.4f, 3f, 8f) },
            new ModulePlacement { name = "BarrierStart_R", type = ModuleType.BarrierWall, position = new Vector3(3.8f, 7.5f, 0f), scale = new Vector3(0.4f, 3f, 8f) },
            new ModulePlacement { name = "RampDown", type = ModuleType.RampPlatform, position = new Vector3(0f, 3f, 8f), scale = new Vector3(4f, 0.5f, 10f), rotation = new Vector3(22f, 0f, 0f) },
            new ModulePlacement { name = "LowerPlatform", type = ModuleType.StandardPlatform, position = new Vector3(0f, 0f, 18f), scale = new Vector3(10f, 0.5f, 10f) },
            new ModulePlacement { name = "Plate1", type = ModuleType.PressurePlate, position = new Vector3(0f, 0.33f, 18f), scale = Vector3.one },
            new ModulePlacement { name = "ExitGate", type = ModuleType.Door, position = new Vector3(0f, 7.5f, -3.8f), scale = new Vector3(4f, 3f, 0.5f), targetSignals = new[] { "Plate1" } },
            new ModulePlacement { name = "ExitPlatform", type = ModuleType.StandardPlatform, position = new Vector3(0f, 6f, -7f), scale = new Vector3(6f, 0.5f, 6f) },
            new ModulePlacement { name = "LevelExit_Area", type = ModuleType.LevelExit, position = new Vector3(0f, 7.25f, -6f), scale = Vector3.one, customData = "Level_02" },
            new ModulePlacement { name = "PlayerStart", type = ModuleType.PlayerStart, position = new Vector3(0f, 7.1f, -1f) },
            new ModulePlacement { name = "Tut_L01_Simetria", type = ModuleType.TutorialTrigger, position = new Vector3(0f, 7f, -1f), scale = new Vector3(8f, 3f, 4f), customData = "Nivel 1 — Despertar Simétrico|Baja por la rampa al nivel inferior. Graba un eco parado sobre la placa para abrir la compuerta superior. Regresa rápido y crúzala mientras tu eco mantiene el portón abierto." },
            new ModulePlacement { name = "Light_LowerPlate", type = ModuleType.PointLight, position = new Vector3(0f, 3f, 18f), customData = "40C0FF,4,10" },
            new ModulePlacement { name = "Light_Exit", type = ModuleType.PointLight, position = new Vector3(0f, 9f, -6f), customData = "2699FF,5,10" },
            new ModulePlacement { name = "DistantArch", type = ModuleType.DistantArchitecture, position = new Vector3(0f, 0f, 12f) },
            new ModulePlacement { name = "AmbientParticles", type = ModuleType.AmbientParticles, position = new Vector3(0f, 2f, 8f), scale = new Vector3(18f, 8f, 18f) },
            new ModulePlacement { name = "LevelGoal", type = ModuleType.LevelGoal, position = new Vector3(0f, 7.25f, -6f), targetSignals = new[] { "Plate1" }, customData = "Activa la placa inferior proyectando un eco y cruza antes de que baje la compuerta.|El eco del portal responde en la simulación.|Fractura simétrica superada." },
            new ModulePlacement { name = "LevelRuntime", type = ModuleType.LevelRuntime, customData = "Proyecta tu eco a la placa inferior y cruza la compuerta.|El eco es tu llave del pasado.|El primer enlace se ha completado." }
        };

        SaveBlueprint(bp, "Level_01_Blueprint");
    }

    private static void ExportLevel02()
    {
        LevelBlueprint bp = ScriptableObject.CreateInstance<LevelBlueprint>();
        bp.levelName = "Level_02";
        bp.nextLevel = "Level_03";
        bp.actNumber = 1;
        bp.archetype = LevelArchetype.MovingCity;
        bp.maxEchoes = 2;
        bp.maxRecordSeconds = 14f;

        bp.fogColor = new Color(0.04f, 0.05f, 0.08f, 1f);
        bp.fogDensity = 0.043f;
        bp.skyColor = new Color(0.04f, 0.05f, 0.08f, 1f);

        bp.narrativeIntroTitle = "Nivel 2 — Plataformas Cruzadas";
        bp.narrativeIntroDesc = "Sincroniza tus ecos en las placas cruzadas para elevar las plataformas y cruzar el puente superior.";
        bp.narrativeIntroDuration = 10f;
        bp.puzzleObjectiveText = "Sincroniza tus ecos en las placas cruzadas para elevar las plataformas y cruzar el puente.";
        bp.puzzleActiveText = "El contrapeso de la memoria está listo.";
        bp.puzzleCompleteText = "Enlace completado.";

        bp.pathHints = new[] {
            new Vector3(0f, 1.1f, 0f),
            new Vector3(-6f, 7.1f, 12f)
        };

        bp.modules = new List<ModulePlacement>
        {
            new ModulePlacement { name = "StartPlatform", type = ModuleType.StandardPlatform, position = new Vector3(0f, 0f, 0f), scale = new Vector3(14f, 0.5f, 8f) },
            new ModulePlacement { name = "LeftTower", type = ModuleType.StandardPlatform, position = new Vector3(-6f, 6f, 12f), scale = new Vector3(4f, 0.5f, 4f) },
            new ModulePlacement { name = "RightTower", type = ModuleType.StandardPlatform, position = new Vector3(6f, 6f, 12f), scale = new Vector3(4f, 0.5f, 4f) },
            new ModulePlacement { name = "PlateA", type = ModuleType.PressurePlate, position = new Vector3(-4f, 0.33f, 0f) },
            new ModulePlacement { name = "PlateB", type = ModuleType.PressurePlate, position = new Vector3(4f, 0.33f, 0f) },
            new ModulePlacement { name = "PlateC", type = ModuleType.PressurePlate, position = new Vector3(6f, 6.33f, 12f) },
            new ModulePlacement { name = "ElevLeft", type = ModuleType.MovingPlatform, position = new Vector3(-6f, 0f, 6f), scale = new Vector3(3f, 0.5f, 3f), targetSignals = new[] { "PlateB" }, customData = "0,0,0|0,6,0|6" },
            new ModulePlacement { name = "ElevRight", type = ModuleType.MovingPlatform, position = new Vector3(6f, 0f, 6f), scale = new Vector3(3f, 0.5f, 3f), targetSignals = new[] { "PlateA" }, customData = "0,0,0|0,6,0|6" },
            new ModulePlacement { name = "BridgeDoor", type = ModuleType.Door, position = new Vector3(0f, 7.5f, 12f), scale = new Vector3(4f, 3f, 0.5f), targetSignals = new[] { "PlateC" } },
            new ModulePlacement { name = "BridgeHigh", type = ModuleType.BridgePlatform, position = new Vector3(0f, 6f, 12f), scale = new Vector3(8f, 0.5f, 3f) },
            new ModulePlacement { name = "LevelExit_Area", type = ModuleType.LevelExit, position = new Vector3(-6f, 7.25f, 12f), customData = "Level_03" },
            new ModulePlacement { name = "PlayerStart", type = ModuleType.PlayerStart, position = new Vector3(0f, 1.1f, 0f) },
            new ModulePlacement { name = "DistantArch", type = ModuleType.DistantArchitecture, position = new Vector3(0f, 0f, 12f) },
            new ModulePlacement { name = "LevelGoal", type = ModuleType.LevelGoal, position = new Vector3(-6f, 7.25f, 12f), targetSignals = new[] { "PlateC" }, customData = "Sincroniza tus ecos en las placas cruzadas para elevar las plataformas y cruzar el puente superior.|El contrapeso de la memoria está listo.|Enlace completado." },
            new ModulePlacement { name = "LevelRuntime", type = ModuleType.LevelRuntime, customData = "Sincroniza tus ecos en las placas cruzadas.|El contrapeso de la memoria está listo.|Enlace completado." }
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
