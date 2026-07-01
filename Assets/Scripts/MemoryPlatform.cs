using UnityEngine;

/// <summary>
/// A platform that moves between predefined positions triggered by plates, keeping its last target position (memory).
/// </summary>
public class MemoryPlatform : MonoBehaviour, IResettableLevelObject
{
    [Tooltip("Pressure plates that activate corresponding positions")]
    public PressurePlate[] plates;

    [Tooltip("Local positions corresponding to each plate index")]
    public Vector3[] localPositions;

    public float travelSpeed = 2.5f;

    Vector3 _startPosition;
    Vector3 _targetPosition;
    Transform _t;

    void Awake()
    {
        _t = transform;
        _startPosition = _t.localPosition;
        _targetPosition = _startPosition;

        if (GetComponent<MovingPlatformMomentum>() == null)
            gameObject.AddComponent<MovingPlatformMomentum>();
    }

    void Start()
    {
        if (plates != null)
        {
            for (int i = 0; i < plates.Length; i++)
            {
                if (plates[i] != null)
                {
                    int index = i;
                    plates[i].PressedChanged += (pressed) => OnPlatePressed(index, pressed);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (plates != null)
        {
            for (int i = 0; i < plates.Length; i++)
            {
                if (plates[i] != null)
                {
                    int index = i;
                    plates[i].PressedChanged -= (pressed) => OnPlatePressed(index, pressed);
                }
            }
        }
    }

    void OnPlatePressed(int index, bool pressed)
    {
        // Only update target when the button is pressed (preserves state on release)
        if (pressed && index < localPositions.Length)
        {
            _targetPosition = localPositions[index];
            GameFeelController.Instance?.PlayMechanicTick(transform.position, 0.75f);
        }
    }

    void Update()
    {
        _t.localPosition = Vector3.MoveTowards(_t.localPosition, _targetPosition, travelSpeed * Time.deltaTime);
    }

    public void ResetLevelState()
    {
        _targetPosition = _startPosition;
        if (_t != null)
            _t.localPosition = _startPosition;
    }
}
