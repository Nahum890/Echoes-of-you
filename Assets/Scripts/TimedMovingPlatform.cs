using UnityEngine;

/// <summary>
/// Plataforma que se mueve entre dos posiciones locales mientras la placa asociada esté presionada.
/// </summary>
public class TimedMovingPlatform : MonoBehaviour
{
    public PressurePlate plate;
    public Vector3 inactiveLocal = Vector3.zero;
    public Vector3 activeLocal = new Vector3(0f, 0f, 6f);
    public float travelSpeed = 2.5f;

    Transform _t;
    Vector3 _target;

    void Awake()
    {
        _t = transform;
    }

    void Start()
    {
        if (plate != null)
            plate.PressedChanged += OnPlate;

        RefreshTarget();
    }

    void OnDestroy()
    {
        if (plate != null)
            plate.PressedChanged -= OnPlate;
    }

    void OnPlate(bool _)
    {
        RefreshTarget();
    }

    void RefreshTarget()
    {
        _target = plate != null && plate.IsPressed ? activeLocal : inactiveLocal;
    }

    void Update()
    {
        _t.localPosition = Vector3.MoveTowards(_t.localPosition, _target, travelSpeed * Time.deltaTime);
    }
}
