#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor-only component: dibuja Gizmos de ayuda para validar composición visual.
/// Añadir a un GameObject vacío en cada nivel para visualizar:
/// - Zona de spawn y dirección de lectura
/// - Conexiones placa→puerta (visibilidad)
/// - Distancias de salto entre plataformas
/// - Zona de vacío peligroso
/// </summary>
public class LevelVisualGuide : MonoBehaviour
{
    [Header("Layout Validation")]
    [SerializeField] Transform playerSpawn;
    [SerializeField] Transform levelExit;
    [SerializeField] PressurePlate[] plates;
    [SerializeField] DoorController[] doors;

    [Header("Reading Direction")]
    [SerializeField] Vector3 readingDirection = Vector3.forward;
    [SerializeField] float readingAngle = 60f; // grados del cono de lectura

    [Header("Design Rules")]
    [SerializeField] float voidHeight = -15f;

    void OnDrawGizmos()
    {
        // Dirección de lectura principal (+Z)
        if (playerSpawn != null)
        {
            Gizmos.color = new Color(1f, 0.83f, 0.42f, 0.6f);
            Vector3 start = playerSpawn.position;
            Vector3 end = start + readingDirection.normalized * 40f;
            Gizmos.DrawLine(start, end);

            // Cono de visibilidad
            Gizmos.color = new Color(1f, 0.83f, 0.42f, 0.08f);
            float dist = 30f;
            float halfAngle = readingAngle * 0.5f * Mathf.Deg2Rad;
            float radius = dist * Mathf.Tan(halfAngle);
            Vector3 coneCenter = start + readingDirection.normalized * dist;
            DrawWireCircle(coneCenter, radius, readingDirection.normalized);
        }

        // Conexiones placa → puerta
        if (plates != null && doors != null)
        {
            foreach (var plate in plates)
            {
                if (plate == null) continue;
                foreach (var door in doors)
                {
                    if (door == null) continue;
                    Gizmos.color = plate.IsPressed
                        ? new Color(0f, 1f, 0.75f, 0.8f)
                        : new Color(0.18f, 0.75f, 0.55f, 0.4f);
                    Gizmos.DrawLine(plate.transform.position + Vector3.up, door.transform.position + Vector3.up);
                }
            }
        }

        // Exit beacon
        if (levelExit != null)
        {
            Gizmos.color = new Color(1f, 0.83f, 0.42f, 0.5f);
            Gizmos.DrawWireSphere(levelExit.position, 2f);
            Gizmos.DrawLine(levelExit.position, levelExit.position + Vector3.up * 5f);
        }

        // Void plane
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.08f);
        Gizmos.DrawCube(new Vector3(0f, voidHeight, 0f), new Vector3(100f, 0.1f, 100f));
    }

    static void DrawWireCircle(Vector3 center, float radius, Vector3 normal)
    {
        Vector3 right = Vector3.Cross(normal, Vector3.up).normalized;
        if (right.sqrMagnitude < 0.001f)
            right = Vector3.Cross(normal, Vector3.right).normalized;
        Vector3 forward = Vector3.Cross(right, normal).normalized;

        int segments = 24;
        Vector3 prev = center + right * radius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 point = center + (right * Mathf.Cos(angle) + forward * Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prev, point);
            prev = point;
        }
    }
}
#endif
