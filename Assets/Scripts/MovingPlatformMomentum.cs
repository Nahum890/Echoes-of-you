using UnityEngine;

/// <summary>
/// Transfiere la velocidad de una plataforma en movimiento al jugador y ecos.
/// </summary>
public class MovingPlatformMomentum : MonoBehaviour
{
    [SerializeField] bool transferToPlayer = true;
    [SerializeField] bool transferToEcho = true;

    Vector3 _lastWorldPosition;
    Vector3 _platformVelocity;

    public Vector3 PlatformVelocity => _platformVelocity;

    void Awake()
    {
        _lastWorldPosition = transform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = transform.position - _lastWorldPosition;
        _platformVelocity = delta / Mathf.Max(Time.deltaTime, 0.0001f);
        _lastWorldPosition = transform.position;
    }

    void OnCollisionStay(Collision collision)
    {
        ApplyToBody(collision.collider);
    }

    void OnTriggerStay(Collider other)
    {
        ApplyToBody(other);
    }

    void ApplyToBody(Collider col)
    {
        if (col == null)
            return;

        if (col.CompareTag("Player") && transferToPlayer)
        {
            PlayerController player = col.GetComponent<PlayerController>();
            player?.AddPlatformVelocity(_platformVelocity);
        }
        else if (col.CompareTag("Echo") && transferToEcho)
        {
            CharacterController cc = col.GetComponent<CharacterController>();
            if (cc != null)
                cc.Move(_platformVelocity * Time.deltaTime);
        }
    }
}
