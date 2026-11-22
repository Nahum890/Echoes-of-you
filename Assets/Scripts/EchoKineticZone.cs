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

    Coroutine _relayFlight;
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

        bool relayPowered = role == EchoKineticRole.MomentumRelay && shieldField != null && shieldField.IsShielded;
        if (requireEcho && !echoPresent && !relayPowered)
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
                    if (shieldField != null && player != null)
                    {
                        if (_relayFlight == null && shieldField.IsShielded && !player.IsInputLocked)
                            _relayFlight = StartCoroutine(CarryToLanding(player));
                        break;
                    }
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

    System.Collections.IEnumerator CarryToLanding(PlayerController player)
    {
        Vector3 start = player.transform.position;
        Vector3 end = momentumRelayTarget.position;
        var landing = momentumRelayTarget.GetComponent<Collider>();
        if (landing != null) end.y = landing.bounds.max.y + 0.1f;
        float duration = Mathf.Max(0.25f, Vector3.Distance(start, end) / Mathf.Max(1f, momentumRelayForce));
        float elapsed = 0f;
        while (elapsed < duration && player != null && player.Controller.enabled && !player.IsInputLocked)
        {
            yield return null;
            if (player == null || !player.Controller.enabled || player.IsInputLocked) break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 next = Vector3.Lerp(start, end, t) + Vector3.up * (12f * t * (1f - t));
            if ((next - player.transform.position).sqrMagnitude > 9f) break;
            player.Controller.Move(next - player.transform.position);
        }
        _relayFlight = null;
    }

    void OnDisable()
    {
        if (_relayFlight != null) StopCoroutine(_relayFlight);
        _relayFlight = null;
    }

    public void ResetLevelState()
    {
        _echoCount = 0;
        _playerCount = 0;
    }
}
