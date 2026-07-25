using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tipo de paradojas temporales soportadas por el sistema.
/// </summary>
public enum ParadoxType
{
    None,
    Erosion,
    Resonance,
    LivingArchitecture,
    SelfContradiction,
    Perspective,
    QuantumVisibility,
    MemoryReconfiguration
}

/// <summary>
/// Tipos de módulos arquitectónicos del vocabulario y legacy.
/// </summary>
public enum ModuleType
{
    // Legacy / Base (0-20)
    StandardPlatform = 0,
    BridgePlatform = 1,
    RampPlatform = 2,
    BarrierWall = 3,
    PressurePlate = 4,
    Door = 5,
    LevelExit = 6,
    PlayerStart = 7,
    MovingPlatform = 8,
    TutorialTrigger = 9,
    PointLight = 10,
    AmbientParticles = 11,
    DistantArchitecture = 12,
    LevelGoal = 13,
    LevelRuntime = 14,
    PuzzleSignal = 15,
    PuzzleCondition = 16,
    HazardField = 17,
    ConflictTrap = 18,
    MomentumRelay = 19,
    MotorPlatform = 20,

    // Phase 3 (21-30)
    ObservationChamber = 21,
    TemporalBridge = 22,
    PerspectiveAnchor = 23,
    MemoryCorridor = 24,
    ParadoxArena = 25,
    ErosionVault = 26,
    ResonanceChamber = 27,
    LiminalThreshold = 28,
    ChronologicalSpire = 29,
    VoidGallery = 30,

    // VOCABULARIO ARQUITECTÓNICO ESCOLAR (31-46) — USADOS EN BLUEPRINTS
    SchoolHall = 31,
    SchoolCorridor = 32,
    SchoolClassroom = 33,
    SchoolStairwell = 34,
    SchoolBathroom = 35,
    SchoolStaffRoom = 36,
    SchoolLibrary = 37,
    SchoolCourtyard = 38,
    SchoolGym = 39,
    SchoolLab = 40,
    SchoolMaintenanceCorridor = 41,
    SchoolEmergencyCorridor = 42,
    SchoolLyraClassroom = 43,
    SchoolOffice = 44,
    SchoolLiminalClassroom = 45,
    TransitionSpace = 46
}

/// <summary>
/// Configuración de posicionamiento e interconexión de un módulo en la escena.
/// </summary>
[System.Serializable]
public struct ModulePlacement
{
    public string name;
    public ModuleType type;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    
    [Tooltip("Valores extra de configuración en formato key=value o texto libre.")]
    public string customData;
    
    [Tooltip("Nombres de señales que este objeto emite o a las que reacciona.")]
    public string[] targetSignals;
}

/// <summary>
/// Asset de configuración que describe un nivel completo de forma declarativa.
/// </summary>
[CreateAssetMenu(fileName = "NewLevelBlueprint", menuName = "Echoes of You/Level Blueprint", order = 1)]
public class LevelBlueprint : ScriptableObject
{
    [Header("Basic Information")]
    public string levelName = "Level_XX";
    public string nextLevel = "Level_YY";
    public int actNumber = 1;
    public LevelArchetype archetype = LevelArchetype.Standard;
    
    [Header("Echo Limits & Advanced Modes")]
    public bool echoEnabled = true;
    public int maxEchoes = 1;
    public float maxRecordSeconds = 12f;
    public EchoPlaybackMode echoMode = EchoPlaybackMode.Standard;
    public bool recordFuture = false;
    public float degradationPerReplay = 0.0f;
    public EchoRecordingData imposedEchoData;
    public EchoRecordingData ambientEchoData;
    public bool lockEchoSlots = false;
    public int[] lockedSlotIndices;
    public CameraProfile cameraProfile;
    public LightingProfile lightingProfile;

    [Header("Paradox Systems")]
    public ParadoxType[] activeParadoxes;

    [Header("Atmosphere & Lighting")]
    public Color fogColor = new Color(0.12f, 0.14f, 0.2f, 1f);
    public float fogDensity = 0.008f;
    public Color skyColor = new Color(0.2f, 0.22f, 0.28f, 1f);
    public Color ambientColor = new Color(0.06f, 0.08f, 0.12f, 1f);
    public Vector3 directionalLightRotation = new Vector3(50f, -30f, 0f);
    public Color directionalLightColor = Color.white;
    public float directionalLightIntensity = 1f;

    [Header("Narrative Texts")]
    public string narrativeIntroTitle = "Nivel — Título";
    [TextArea(3, 5)]
    public string narrativeIntroDesc = "Descripción larga del nivel y su significado.";
    public float narrativeIntroDuration = 10f;
    public string puzzleObjectiveText = "Proyecta tu eco.";
    public string puzzleActiveText = "El eco es tu llave del pasado.";
    public string puzzleCompleteText = "Enlace completado.";

    [Header("Visual Guides & Pacing")]
    public Vector3[] pathHints;

    [Header("Placed Modules")]
    public List<ModulePlacement> modules = new List<ModulePlacement>();
}
