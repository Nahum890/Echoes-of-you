using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EchoKineticBody : MonoBehaviour, IResettableLevelObject
{
    [SerializeField] Transform controlledTransform;
    [SerializeField] Vector3 inactiveLocalPosition;
    [SerializeField] Vector3 activeLocalPosition;
    [SerializeField] Vector3 inactiveLocalEuler;
    [SerializeField] Vector3 activeLocalEuler;
    [SerializeField] float travelSpeed = 3f;
    [SerializeField] float rotationSpeed = 160f;
    [SerializeField] bool acceptPlayer = true;
    [SerializeField] bool acceptEcho = true;
    [SerializeField] bool requireEcho;
    [SerializeField] bool holdToMove = true;
    [SerializeField] PuzzleSignal completionSignal;

    int _playerCount;
    int _echoCount;
    bool _latched;

    public void Configure(
        Transform target,
        Vector3 inactivePosition,
        Vector3 activePosition,
        Vector3 inactiveEulerAngles,
        Vector3 activeEulerAngles,
        float speed,
        bool playerCanMove,
        bool echoCanMove,
        bool echoRequired,
        bool mustHold,
        PuzzleSignal signal)
    {
        controlledTransform = target;
        inactiveLocalPosition = inactivePosition;
        activeLocalPosition = activePosition;
        inactiveLocalEuler = inactiveEulerAngles;
        activeLocalEuler = activeEulerAngles;
        travelSpeed = speed;
        acceptPlayer = playerCanMove;
        acceptEcho = echoCanMove;
        requireEcho = echoRequired;
        holdToMove = mustHold;
        completionSignal = signal;
    }

    Collider _collider;
    BoxCollider _boxCollider;
    static readonly Collider[] _overlapBuffer = new Collider[32];

    void Awake()
    {
        _collider = GetComponent<Collider>();
        if (_collider != null)
        {
            _collider.isTrigger = true;
            _boxCollider = _collider as BoxCollider;
        }

        if (controlledTransform == null)
            controlledTransform = transform;
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
            halfExtents.y += 0.2f;
            center.y += 0.1f;
            hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, _overlapBuffer, transform.rotation);
        }
        else
        {
            Vector3 center = _collider.bounds.center;
            Vector3 extents = _collider.bounds.extents + new Vector3(0.1f, 0.2f, 0.1f);
            hitCount = Physics.OverlapBoxNonAlloc(center, extents, _overlapBuffer, Quaternion.identity);
        }

        _playerCount = 0;
        _echoCount = 0;

        for (int i = 0; i < hitCount; i++)
        {
            Collider c = _overlapBuffer[i];
            if (c == null) continue;

            if (c.CompareTag("Player"))
            {
                _playerCount++;
            }
            else if (c.CompareTag("Echo") || c.CompareTag("EchoProjection"))
            {
                _echoCount++;
            }
        }
    }

    void Start()
    {
        ResetLevelState();
    }

    void Update()
    {
        bool actorPresent = (acceptPlayer && _playerCount > 0) || (acceptEcho && _echoCount > 0);
        bool echoRequirementMet = !requireEcho || _echoCount > 0;
        bool active = actorPresent && echoRequirementMet;

        if (active && !holdToMove)
            _latched = true;

        Vector3 targetPosition = active || _latched ? activeLocalPosition : inactiveLocalPosition;
        Quaternion targetRotation = Quaternion.Euler(active || _latched ? activeLocalEuler : inactiveLocalEuler);

        controlledTransform.localPosition = Vector3.MoveTowards(controlledTransform.localPosition, targetPosition, travelSpeed * Time.deltaTime);
        controlledTransform.localRotation = Quaternion.RotateTowards(controlledTransform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (completionSignal != null && Vector3.Distance(controlledTransform.localPosition, activeLocalPosition) <= 0.08f)
            completionSignal.MarkSatisfied();
    }

    public void ResetLevelState()
    {
        _playerCount = 0;
        _echoCount = 0;
        _latched = false;

        if (controlledTransform == null)
            controlledTransform = transform;

        controlledTransform.localPosition = inactiveLocalPosition;
        controlledTransform.localRotation = Quaternion.Euler(inactiveLocalEuler);
    }
}
