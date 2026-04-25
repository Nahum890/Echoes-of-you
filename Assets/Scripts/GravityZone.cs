using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GravityZone : MonoBehaviour
{
    [SerializeField] Vector3 gravityDirection = Vector3.down;
    [SerializeField] Space directionSpace = Space.Self;
    [SerializeField] float gravityStrength = 24f;
    [SerializeField] int priority = 0;

    public int Priority => priority;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public Vector3 GetGravityVector()
    {
        Vector3 dir = gravityDirection.sqrMagnitude > 0.0001f ? gravityDirection.normalized : Vector3.down;
        if (directionSpace == Space.Self)
            dir = transform.TransformDirection(dir).normalized;

        return dir * Mathf.Max(0.01f, gravityStrength);
    }

    void OnTriggerEnter(Collider other)
    {
        TryRegister(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryRegister(other);
    }

    void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null)
            player.UnregisterGravityZone(this);
    }

    void TryRegister(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null)
            player.RegisterGravityZone(this);
    }

    void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
            return;

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.2f);
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
            Gizmos.DrawCube(box.center, box.size);

        Gizmos.matrix = oldMatrix;

        Vector3 start = transform.position;
        Vector3 gravity = GetGravityVector().normalized;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(start, start + gravity * 2f);
        Gizmos.DrawSphere(start + gravity * 2f, 0.15f);
    }
}
