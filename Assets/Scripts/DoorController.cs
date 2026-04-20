using UnityEngine;

/// <summary>
/// Múltiples placas requeridas: cuando todas están presionadas, la puerta desaparece.
/// </summary>
public class DoorController : MonoBehaviour
{
    public PressurePlate[] plates;

    Collider _col;
    Renderer[] _renderers;

    void Start()
    {
        _col = GetComponent<Collider>();
        _renderers = GetComponentsInChildren<Renderer>();

        if (plates != null)
        {
            foreach (var p in plates)
            {
                if (p != null) p.PressedChanged += OnPlateChanged;
            }
        }
        UpdateState();
    }

    void OnDestroy()
    {
        if (plates != null)
        {
            foreach (var p in plates)
            {
                if (p != null) p.PressedChanged -= OnPlateChanged;
            }
        }
    }

    void OnPlateChanged(bool obj)
    {
        UpdateState();
    }

    void UpdateState()
    {
        bool allPressed = true;
        if (plates == null || plates.Length == 0) allPressed = false;
        else
        {
            foreach (var p in plates)
            {
                if (p != null && !p.IsPressed)
                {
                    allPressed = false;
                    break;
                }
            }
        }

        // Si se abre (todas presionadas), desaparece
        if (_col != null) _col.enabled = !allPressed;
        if (_renderers != null)
        {
            foreach (var r in _renderers)
            {
                if (r != null) r.enabled = !allPressed;
            }
        }
    }
}
