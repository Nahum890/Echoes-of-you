using UnityEngine;

/// <summary>
/// Alinea la placa al suelo bajo ella y amplía el trigger vertical para ecos en proyección (sin gravedad).
/// </summary>
[RequireComponent(typeof(PressurePlate))]
public class PressurePlateAlignment : MonoBehaviour
{
    public bool echoProjectionPlate = true;
    public float surfaceOffset = 0.08f;
    public float echoTriggerHeight = 1.65f;
    [SerializeField] LayerMask groundMask = 1 << 6;

    static readonly string[] EchoPlateNameHints = { "Eco", "eco", "Echo" };

    void Awake()
    {
        if (!echoProjectionPlate)
            echoProjectionPlate = IsEchoPlateName(gameObject.name);

        SnapToSurface();
        if (echoProjectionPlate)
            ExpandTriggerForProjection();
    }

    static bool IsEchoPlateName(string objectName)
    {
        for (int i = 0; i < EchoPlateNameHints.Length; i++)
        {
            if (objectName.Contains(EchoPlateNameHints[i]))
                return true;
        }

        return false;
    }

    public void SnapToSurface()
    {
        Vector3 origin = transform.position + Vector3.up * 12f;
        int mask = groundMask.value != 0 ? groundMask.value : Physics.DefaultRaycastLayers;

        RaycastHit hit;
        if (TryFindSurface(origin, mask, out hit) || TryFindSurface(origin, Physics.DefaultRaycastLayers, out hit))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + surfaceOffset;
            transform.position = pos;
        }
    }

    bool TryFindSurface(Vector3 origin, int mask, out RaycastHit result)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 24f, mask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider == null)
                continue;
            if (hits[i].collider.transform.IsChildOf(transform))
                continue;
            result = hits[i];
            return true;
        }

        result = default;
        return false;
    }

    public void ExpandTriggerForProjection()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
            return;

        float playerFeetY = 1.1f;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerFeetY = player.transform.position.y;

        float plateY = transform.position.y;
        float top = Mathf.Max(1.6f, playerFeetY + 0.35f);
        float bottom = plateY - 0.2f;
        float height = Mathf.Max(echoTriggerHeight, top - bottom);

        // XZ ampliados: la proyección del eco abarca un área más ancha que la placa
        // (antes 2.4f muy delgado → el eco en movimiento la atravesaba por los lados).
        box.size = new Vector3(Mathf.Max(box.size.x, 2.8f), height, Mathf.Max(box.size.z, 2.8f));
        box.center = new Vector3(0f, (height * 0.5f) - 0.1f, 0f);
    }
}
