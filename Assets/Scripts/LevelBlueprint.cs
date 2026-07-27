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
    // Legacy / Base
    StandardPlatform,
    BridgePlatform,
    RampPlatform,
    BarrierWall,
    PressurePlate,
    Door,
    LevelExit,
    PlayerStart,
    MovingPlatform,
    TutorialTrigger,
    PointLight,
    AmbientParticles,
    DistantArchitecture,
    LevelGoal,
    LevelRuntime,
    PuzzleSignal,
    PuzzleCondition,
    HazardField,
    ConflictTrap,
    MomentumRelay,
    MotorPlatform,
    
    // New Architectural Vocabulary
    ObservationChamber,
    TemporalBridge,
    PerspectiveAnchor,
    MemoryCorridor,
    ParadoxArena,
    ErosionVault,
    ResonanceChamber,
    LiminalThreshold,
    ChronologicalSpire,
    VoidGallery,

// School greybox vocabulary. Explicit values preserve the serialized
    // legacy blueprint values above while reserving the requested 31+ range.
    SchoolHall = 31,
    SchoolCorridor,
    SchoolClassroom,
    SchoolStairwell,
    SchoolLibrary,
    SchoolGym,
    SchoolLab,
    SchoolLyraClassroom,
    SchoolLiminalClassroom,
    TransitionSpace,
    SchoolEntrance,
    SchoolStaffRoom,
    SchoolCourtyard,

    // Phase 2 puzzle vocabulary. Explicit values preserve existing serialized
    // blueprint assets (SchoolCourtyard=43 implicit, so 44+ is safe).
    GhostBridge = 44
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
    
    [Header("Echo Limits")]
    public bool echoEnabled = true;
    public int maxEchoes = 1;
    public float maxRecordSeconds = 12f;

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

    [Header("Puzzle Constraints (Phase 2)")]
    [Tooltip("If true, the LevelExit only unlocks during a 5s window after an Echo recording completes.")]
    public bool recordFuture;
    [Tooltip("If true, an ambient Lyra ghost walks the level revealing hidden MemoryPlatforms. Player cannot record.")]
    public bool ambientEchoData;
    [Tooltip("If true, the EchoRecorder is locked and a pre-baked RecordFrame[] is fed to playback.")]
    public bool imposedEchoData;
    [Tooltip("If true, the camera mirrors around the XZ plane and player horizontal input is inverted.")]
    public bool inversionCamera;
    [Tooltip("Minimum synchronization window (seconds) for multi-actor puzzles. Enforced by PuzzleLevelValidator.")]
    public float timingFloor = 0.4f;

    [Header("Placed Modules")]
    public List<ModulePlacement> modules = new List<ModulePlacement>();
}
