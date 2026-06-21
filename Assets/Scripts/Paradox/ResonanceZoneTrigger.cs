using UnityEngine;

/// <summary>
/// Disparador adjunto a cada zona de resonancia individual para notificar al ResonanceSystem.
/// </summary>
public class ResonanceZoneTrigger : MonoBehaviour
{
    private ResonanceSystem _system;
    private Collider _collider;

    void Awake()
    {
        _system = GetComponentInParent<ResonanceSystem>();
        _collider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_system != null)
            _system.RegisterOccupant(_collider, other);
    }

    void OnTriggerExit(Collider other)
    {
        if (_system != null)
            _system.UnregisterOccupant(_collider, other);
    }
}
