using UnityEngine;

/// <summary>
/// Zona letal: mata al jugador al entrar (vacío, energía, láser, etc.).
/// </summary>
[RequireComponent(typeof(Collider))]
public class KillVolume : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";
    [SerializeField] float deathDelay = 0f;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        Kill(other.transform.position);
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        Kill(other.transform.position);
    }

    void Kill(Vector3 position)
    {
        LevelRuntimeController controller = LevelRuntimeController.Instance;
        if (controller == null)
            return;

        controller.HandlePlayerDeath(position, deathDelay);
    }
}
