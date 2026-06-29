using UnityEngine;

/// <summary>
/// Attached to pushable kinetic blocks to handle physics state resetting upon level soft-reset/reloads.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class KineticPushableBlock : MonoBehaviour, IResettableLevelObject
{
    Vector3 _startPosition;
    Quaternion _startRotation;
    Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    public void ResetLevelState()
    {
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        transform.position = _startPosition;
        transform.rotation = _startRotation;
    }
}
