using UnityEngine;

/// <summary>
/// Tras resolver el puzzle, el jugador debe escapar de una secuencia en colapso
/// antes de que la salida final se desbloquee por completo.
/// </summary>
public class LevelEscapeSequence : MonoBehaviour
{
    [SerializeField] LevelGoal goal;
    [SerializeField] LevelExit[] exits;
    [SerializeField] Transform escapeRouteEnd;
    [SerializeField] float collapseDelay = 0.8f;
    [SerializeField] float escapeTimeLimit = 20f;
    [SerializeField] ChaseHazardMotor hazard;

    bool _escapeActive;
    float _escapeTimer;
    bool _escapeComplete;

    public bool IsEscapeComplete => _escapeComplete;

    bool _exitsTemporarilyLocked = true;

    void Awake()
    {
        if (goal == null)
            goal = LevelGoal.Instance;
        if (exits == null || exits.Length == 0)
            exits = FindObjectsByType<LevelExit>(FindObjectsSortMode.None);

        LockExits(true);
    }

    void OnEnable()
    {
        if (goal != null)
        {
            // Re-evaluar si el goal ya estaba listo al cargar
            if (goal.IsReady)
                BeginEscape();
        }
    }

    void Update()
    {
        if (!_escapeActive)
        {
            if (goal != null && goal.IsReady && !_escapeActive)
                BeginEscape();
            return;
        }

        _escapeTimer -= Time.deltaTime;
        if (_escapeTimer <= 0f)
            CompleteEscape();

        if (escapeRouteEnd != null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null && Vector3.Distance(player.transform.position, escapeRouteEnd.position) < 2.5f)
                CompleteEscape();
        }
    }

    void BeginEscape()
    {
        LevelExperienceBlueprint blueprint = LevelExperienceBlueprint.Active;
        if (blueprint != null && !blueprint.RequiresEscape)
        {
            CompleteEscape();
            return;
        }

        if (_escapeActive)
            return;

        _escapeActive = true;
        _escapeTimer = blueprint != null ? blueprint.EscapeDuration : escapeTimeLimit;
        LockExits(true);

        if (hazard == null && blueprint != null)
            hazard = blueprint.ChaseHazard;

        hazard?.Activate();

        Invoke(nameof(StartCollapseFeedback), collapseDelay);
        GameHUD hud = FindAnyObjectByType<GameHUD>();
        hud?.ShowToast("¡Escapa antes del colapso!", new Color(1f, 0.55f, 0.35f, 1f), 2.2f);
    }

    void StartCollapseFeedback()
    {
        GameFeelController.Instance?.PlaySoftError(transform.position);
    }

    void CompleteEscape()
    {
        if (!_exitsTemporarilyLocked)
            return;

        _escapeActive = false;
        _escapeComplete = true;
        _exitsTemporarilyLocked = false;
        LockExits(false);

        GameHUD hud = FindAnyObjectByType<GameHUD>();
        hud?.ShowToast("Salida desbloqueada", new Color(0.5f, 0.95f, 0.75f, 1f), 1.4f);
    }

    void LockExits(bool locked)
    {
        if (exits == null)
            return;

        for (int i = 0; i < exits.Length; i++)
        {
            if (exits[i] != null)
                exits[i].SetUnlocked(!locked && (goal == null || goal.IsReady));
        }
    }
}
