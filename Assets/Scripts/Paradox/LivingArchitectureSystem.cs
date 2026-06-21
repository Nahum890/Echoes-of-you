using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Arquitectura Viva: Hace que las paredes y plataformas oscilen entre dos posiciones o escalas 
/// en ciclos continuos deterministas, forzando al jugador a sincronizar sus movimientos y grabaciones.
/// </summary>
public class LivingArchitectureSystem : MonoBehaviour, IResettableLevelObject
{
    [System.Serializable]
    public class MovingWall
    {
        public Transform targetTransform;
        public Vector3 localOffset;
        public float duration = 4f;
        public float delay = 0f;
        public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [HideInInspector] public Vector3 initialLocalPosition;
    }

    [SerializeField] List<MovingWall> movingWalls = new List<MovingWall>();

    private float _timeAccumulator = 0f;
    private bool _running = true;

    void Awake()
    {
        foreach (var wall in movingWalls)
        {
            if (wall.targetTransform != null)
            {
                wall.initialLocalPosition = wall.targetTransform.localPosition;
            }
        }
    }

    void Update()
    {
        if (!_running) return;

        _timeAccumulator += Time.deltaTime;
        UpdateWallPositions();
    }

    private void UpdateWallPositions()
    {
        foreach (var wall in movingWalls)
        {
            if (wall.targetTransform == null) continue;

            float period = wall.duration * 2f;
            float timeInPeriod = (_timeAccumulator + wall.delay) % period;
            
            float t;
            if (timeInPeriod < wall.duration)
            {
                t = timeInPeriod / wall.duration;
            }
            else
            {
                t = 1f - ((timeInPeriod - wall.duration) / wall.duration);
            }

            float curveValue = wall.movementCurve.Evaluate(t);
            wall.targetTransform.localPosition = Vector3.Lerp(wall.initialLocalPosition, wall.initialLocalPosition + wall.localOffset, curveValue);
        }
    }

    public void ResetLevelState()
    {
        _timeAccumulator = 0f;
        UpdateWallPositions();
    }
}
