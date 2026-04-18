using UnityEngine;

/// <summary>
/// Puerta que se abre si todas las placas configuradas están presionadas a la vez (AND).
/// </summary>
public class DoorController : MonoBehaviour
{
    public PressurePlate[] plates;
    public Vector3 closedLocalPosition = Vector3.zero;
    public Vector3 openLocalPosition = new Vector3(0f, 8f, 0f);
    public float moveSpeed = 3f;

    Transform _t;
    Vector3 _targetLocal;

    void Awake()
    {
        _t = transform;
    }

    void Start()
    {
        if (plates == null)
            return;

        for (int i = 0; i < plates.Length; i++)
        {
            if (plates[i] != null)
                plates[i].PressedChanged += OnAnyPlateChanged;
        }

        RefreshTargetImmediate();
    }

    void OnDestroy()
    {
        if (plates == null)
            return;

        for (int i = 0; i < plates.Length; i++)
        {
            if (plates[i] != null)
                plates[i].PressedChanged -= OnAnyPlateChanged;
        }
    }

    void OnAnyPlateChanged(bool _)
    {
        RefreshTargetImmediate();
    }

    void RefreshTargetImmediate()
    {
        _targetLocal = AllPressed() ? openLocalPosition : closedLocalPosition;
    }

    bool AllPressed()
    {
        if (plates == null || plates.Length == 0)
            return false;

        for (int i = 0; i < plates.Length; i++)
        {
            if (plates[i] == null || !plates[i].IsPressed)
                return false;
        }

        return true;
    }

    void Update()
    {
        _t.localPosition = Vector3.MoveTowards(_t.localPosition, _targetLocal, moveSpeed * Time.deltaTime);
    }
}
