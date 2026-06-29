using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EchoConflictTrap : MonoBehaviour, IResettableLevelObject
{
    [SerializeField] DoorController[] doorsClosedByEcho;
    [SerializeField] DoorController[] doorsOpenedByEcho;
    [SerializeField] Transform[] warningObjects;
    [SerializeField] float warningSpinSpeed = 120f;
    [SerializeField] PuzzleSignal completionSignal;

    int _echoCount;

    public bool IsActive => _echoCount > 0;

    public void Configure(DoorController[] doorsToClose, DoorController[] doorsToOpen, Transform[] warnings, PuzzleSignal signal)
    {
        doorsClosedByEcho = doorsToClose;
        doorsOpenedByEcho = doorsToOpen;
        warningObjects = warnings;
        completionSignal = signal;
    }

    Collider _collider;
    BoxCollider _boxCollider;
    static readonly Collider[] _overlapBuffer = new Collider[32];
    bool _hadEcho;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        if (_collider != null)
        {
            _collider.isTrigger = true;
            _boxCollider = _collider as BoxCollider;
        }
    }

    void FixedUpdate()
    {
        if (_collider == null || !_collider.enabled || !gameObject.activeInHierarchy)
            return;

        int hitCount = 0;
        if (_boxCollider != null)
        {
            Vector3 center = transform.TransformPoint(_boxCollider.center);
            Vector3 halfExtents = Vector3.Scale(_boxCollider.size, transform.lossyScale) * 0.5f;
            hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, _overlapBuffer, transform.rotation);
        }
        else
        {
            Vector3 center = _collider.bounds.center;
            Vector3 extents = _collider.bounds.extents;
            hitCount = Physics.OverlapBoxNonAlloc(center, extents, _overlapBuffer, Quaternion.identity);
        }

        _echoCount = 0;
        for (int i = 0; i < hitCount; i++)
        {
            Collider c = _overlapBuffer[i];
            if (c == null) continue;

            if (c.CompareTag("Echo") || c.CompareTag("EchoProjection"))
            {
                _echoCount++;
            }
        }

        bool hasEcho = _echoCount > 0;
        if (_hadEcho && !hasEcho)
        {
            completionSignal?.MarkSatisfied();
        }

        _hadEcho = hasEcho;
    }

    void Update()
    {
        ApplyTrapState();

        if (!IsActive || warningObjects == null)
            return;

        for (int i = 0; i < warningObjects.Length; i++)
        {
            if (warningObjects[i] != null)
                warningObjects[i].Rotate(Vector3.up, warningSpinSpeed * Time.deltaTime, Space.World);
        }
    }

    public void ResetLevelState()
    {
        _echoCount = 0;
        _hadEcho = false;
        ApplyTrapState();
    }

    void ApplyTrapState()
    {
        if (doorsClosedByEcho != null)
        {
            for (int i = 0; i < doorsClosedByEcho.Length; i++)
            {
                if (doorsClosedByEcho[i] != null)
                    doorsClosedByEcho[i].SetOpenState(!IsActive);
            }
        }

        if (doorsOpenedByEcho != null)
        {
            for (int i = 0; i < doorsOpenedByEcho.Length; i++)
            {
                if (doorsOpenedByEcho[i] != null)
                    doorsOpenedByEcho[i].SetOpenState(IsActive);
            }
        }
    }
}
