using UnityEngine;

/// <summary>
/// Alinea la placa al suelo bajo ella y amplía el trigger vertical para ecos en proyección (sin gravedad).
/// </summary>
[RequireComponent(typeof(PressurePlate))]
public class PressurePlateAlignment : MonoBehaviour
{
    public bool echoProjectionPlate = true;
    [SerializeField] float surfaceOffset = 0.08f;
    [SerializeField] float echoTriggerHeight = 1.65f;
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

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 24f, mask, QueryTriggerInteraction.Ignore))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + surfaceOffset;
            transform.position = pos;
            return;
        }

        if (Physics.Raycast(origin, Vector3.down, out hit, 24f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + surfaceOffset;
            transform.position = pos;
        }
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
        float top = Mathf.Max(playerFeetY + 0.35f, plateY + 0.15f);
        float bottom = plateY - 0.05f;
        float height = Mathf.Max(echoTriggerHeight, top - bottom);

        box.size = new Vector3(Mathf.Max(box.size.x, 2.4f), height, Mathf.Max(box.size.z, 2.4f));
        box.center = new Vector3(0f, (height * 0.5f) - 0.04f, 0f);
    }
}
