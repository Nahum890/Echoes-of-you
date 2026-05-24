using UnityEngine;

/// <summary>
/// Define la identidad de experiencia de un nivel: arquetipo, cámara y ritmo esperado.
/// Colócalo en la raíz de cada escena de nivel.
/// </summary>
public class LevelExperienceBlueprint : MonoBehaviour
{
    [Header("Arquetipo")]
    [SerializeField] LevelArchetype archetype = LevelArchetype.Standard;
    [SerializeField] EchoesCameraIdentity cameraIdentity = EchoesCameraIdentity.DynamicFollow;

    [Header("Ritmo (5 actos)")]
    [SerializeField] Transform movementSection;
    [SerializeField] Transform syncSection;
    [SerializeField] Transform escalationSection;
    [SerializeField] Transform ahaMoment;
    [SerializeField] Transform traversalClimax;

    [Header("Escape post-puzzle")]
    [SerializeField] bool requiresEscapeAfterPuzzle = true;
    [SerializeField] float escapeDurationSeconds = 18f;

    [Header("Chase (si aplica)")]
    [SerializeField] ChaseHazardMotor chaseHazard;

    public LevelArchetype Archetype => archetype;
    public EchoesCameraIdentity CameraIdentity => cameraIdentity;
    public bool RequiresEscape => requiresEscapeAfterPuzzle;
    public float EscapeDuration => escapeDurationSeconds;
    public ChaseHazardMotor ChaseHazard => chaseHazard;

    public static LevelExperienceBlueprint Active { get; private set; }

    void Awake()
    {
        Active = this;
    }

    void OnDestroy()
    {
        if (Active == this)
            Active = null;
    }
}
