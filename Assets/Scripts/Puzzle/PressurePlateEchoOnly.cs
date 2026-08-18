// PressurePlateEchoOnly.cs
using UnityEngine;

/// <summary>
/// Variant of PressurePlate that only accepts echo actors.
/// Configures a standard PressurePlate to act as an EchoOnly plate and physically
/// rejects the player from stepping on or activating it.
/// </summary>
[RequireComponent(typeof(PressurePlate))]
public class PressurePlateEchoOnly : MonoBehaviour
{
    [SerializeField] float repulsionForce = 4.0f;

    PressurePlate _plate;
    GameObject _barrierObj;

    void Awake()
    {
        Initialize();
    }

    void Reset()
    {
        Initialize();
    }

    void OnValidate()
    {
        _plate = GetComponent<PressurePlate>();
        if (_plate != null)
        {
            _plate.ConfigureAcceptedActors(false, true, true);
        }
    }

    public void Initialize()
    {
        _plate = GetComponent<PressurePlate>();
        if (_plate != null)
        {
            // Strictly reject Player, accept only Echo and EchoProjection
            _plate.ConfigureAcceptedActors(false, true, true);
        }

        CreatePlayerBarrier();
    }

    void CreatePlayerBarrier()
    {
        // Create a solid barrier that physically blocks the Player.
        // Echos automatically ignore colliders tagged "PlayerOnlyBarrier" via EchoPlayback.Awake().
        Transform existing = transform.Find("EchoOnly_PlayerBarrier");
        if (existing != null)
            _barrierObj = existing.gameObject;
        else
        {
            _barrierObj = new GameObject("EchoOnly_PlayerBarrier");
            _barrierObj.transform.SetParent(transform, false);
            _barrierObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            
            BoxCollider barrierCol = _barrierObj.AddComponent<BoxCollider>();
            barrierCol.size = new Vector3(1.8f, 1.0f, 1.8f);
            barrierCol.isTrigger = false;
            _barrierObj.tag = "PlayerOnlyBarrier";
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other == null)
            return;

        if (other.CompareTag("Player"))
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                Vector3 away = other.transform.position - transform.position;
                away.y = 0f;
                if (away.sqrMagnitude < 0.001f)
                    away = Vector3.back;
                cc.Move(away.normalized * (repulsionForce * Time.deltaTime));
            }
        }
    }
}

