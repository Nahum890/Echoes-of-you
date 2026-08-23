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

    // En Start, no en Awake: LevelEnvironmentBootstrap escala la geometria del nivel
    // (x2) despues de los Awake. Alinear y dimensionar antes de eso hacia que
    // ExpandTriggerForProjection leyese un lossyScale a medias y dejase el trigger
    // al doble de lo pedido.
    void Start()
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
        //
        // OJO: box.size es LOCAL. Las medidas de arriba estan pensadas en metros de
        // mundo, y las placas llegan a runtime con lossyScale 4 (escala de la escena
        // x2 sobre un transform que ya venia a x2). Asignarlas tal cual convertia una
        // placa de 2.8 m en un trigger de 11.2 x 6.6 x 11.2 m que detectaba al eco
        // desde media sala. Convertimos de mundo a local antes de asignar.
        Vector3 escala = transform.lossyScale;
        float sx = Mathf.Abs(escala.x) > 0.0001f ? Mathf.Abs(escala.x) : 1f;
        float sy = Mathf.Abs(escala.y) > 0.0001f ? Mathf.Abs(escala.y) : 1f;
        float sz = Mathf.Abs(escala.z) > 0.0001f ? Mathf.Abs(escala.z) : 1f;

        // NO se ensancha en XZ. Antes se forzaba a 2.8 m como minimo, asi que la
        // zona de deteccion sobresalia medio metro por cada lado de la placa que
        // se ve: te bajabas de ella y seguia contando como pisada. La respuesta
        // tiene que coincidir con lo que el jugador ve.
        // La altura si se amplia: el eco en proyeccion no cae por gravedad y
        // puede quedar unos centimetros por encima.
        float anchoMundo = box.size.x * sx;
        float fondoMundo = box.size.z * sz;

        box.size = new Vector3(anchoMundo / sx, height / sy, fondoMundo / sz);
        box.center = new Vector3(0f, ((height * 0.5f) - 0.1f) / sy, 0f);
    }
}
