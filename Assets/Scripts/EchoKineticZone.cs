using UnityEngine;

/// <summary>
/// Zona que reacciona a la presencia del eco (y opcionalmente del jugador) según su rol cinético.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EchoKineticZone : MonoBehaviour, IResettableLevelObject
{
    [SerializeField] EchoKineticRole role = EchoKineticRole.TimingActivator;
    [SerializeField] EchoKineticBody kineticBody;
    [SerializeField] EchoShieldField shieldField;
    [SerializeField] TimedMovingPlatform platform;
    [SerializeField] DynamicTransformMotor geometryMotor;
    [SerializeField] PuzzleSignal completionSignal;
    [SerializeField] Transform momentumRelayTarget;
    [SerializeField] float momentumRelayForce = 9f;
    [SerializeField] bool requireEcho = true;
    [SerializeField] bool acceptPlayer;

    int _echoCount;
    int _playerCount;

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
            // Inflate slightly to catch actors resting exactly on or slightly intersecting
            halfExtents.y += 0.2f;
            center.y += 0.1f;
            hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, _overlapBuffer, transform.rotation);
        }
        else
        {
            // General fallback using AABB bounds
            Vector3 center = _collider.bounds.center;
            Vector3 extents = _collider.bounds.extents + new Vector3(0.1f, 0.2f, 0.1f);
            hitCount = Physics.OverlapBoxNonAlloc(center, extents, _overlapBuffer, Quaternion.identity);
        }

        _echoCount = 0;
        _playerCount = 0;
        Collider playerCollider = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider c = _overlapBuffer[i];
            if (c == null) continue;

            if (c.CompareTag("Echo") || c.CompareTag("EchoProjection"))
            {
                _echoCount++;
            }
            else if (acceptPlayer && c.CompareTag("Player"))
            {
                _playerCount++;
                playerCollider = c;
            }
        }

        bool echoPresent = _echoCount > 0;
        bool playerPresent = _playerCount > 0;

        if (requireEcho && !echoPresent)
            return;

        if (!requireEcho && !echoPresent && !playerPresent)
            return;

        Collider targetCollider = playerCollider != null ? playerCollider : (hitCount > 0 ? _overlapBuffer[0] : null);
        if (targetCollider != null)
        {
            ApplyRole(targetCollider);
        }
    }

    void ApplyRole(Collider other)
    {
        bool echoPresent = _echoCount > 0;
        switch (role)
        {
            case EchoKineticRole.TemporaryShield:
                break;
            case EchoKineticRole.PlatformActivator:
                if (platform != null && echoPresent)
                    platform.SendMessage("ForceActive", SendMessageOptions.DontRequireReceiver);
                break;
            case EchoKineticRole.MomentumRelay:
                if (momentumRelayTarget != null && other.CompareTag("Player"))
                {
                    PlayerController player = other.GetComponent<PlayerController>();
                    Vector3 dir = (momentumRelayTarget.position - other.transform.position);
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.01f)
                        player?.AddPlanarImpulse(dir.normalized * momentumRelayForce * Time.deltaTime * 30f);
                }
                break;
            case EchoKineticRole.HazardBait:
                completionSignal?.MarkSatisfied();
                break;
            case EchoKineticRole.PathOpener:
            case EchoKineticRole.GeometryModifier:
            case EchoKineticRole.SynchronizedTraversal:
            case EchoKineticRole.TimingActivator:
                completionSignal?.MarkSatisfied();
                break;
        }
    }

    public void ResetLevelState()
    {
        _echoCount = 0;
        _playerCount = 0;
    }
}
