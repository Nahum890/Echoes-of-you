using UnityEngine;

/// <summary>
/// Marca una plataforma como parkour (salto requerido).
/// Añade feedback visual: emisión pulsante en el borde y
/// Gizmos en el Editor para validar distancias.
/// Colocar en plataformas que requieran salto para acceder.
/// </summary>
public class ParkourPlatformMarker : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] Color edgeGlowColor = new Color(0.32f, 0.52f, 0.72f, 1f); // Azul sutil
    [SerializeField] float glowIntensity = 0.4f;
    [SerializeField] float pulseSpeed = 1.8f;
    [SerializeField] bool createEdgeLight = true;
    [SerializeField] float edgeLightRange = 3f;

    [Header("Design Validation")]
    [SerializeField] float maxJumpHeight = 2.2f;
    [SerializeField] float minPlatformWidth = 3f;

    Renderer _renderer;
    MaterialPropertyBlock _propBlock;
    Light _edgeLight;
    float _pulsePhase;

    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _propBlock = new MaterialPropertyBlock();

        if (_renderer != null)
        {
            Material mat = _renderer.material;
            mat.EnableKeyword("_EMISSION");
        }

        if (createEdgeLight)
        {
            GameObject lightObj = new GameObject("ParkourEdgeLight");
            lightObj.transform.SetParent(transform, false);

            // Position light at top edge of platform
            float yOffset = transform.lossyScale.y * 0.5f + 0.3f;
            lightObj.transform.localPosition = new Vector3(0f, yOffset, 0f);

            _edgeLight = lightObj.AddComponent<Light>();
            _edgeLight.type = LightType.Point;
            _edgeLight.color = edgeGlowColor;
            _edgeLight.intensity = glowIntensity;
            _edgeLight.range = edgeLightRange;
            _edgeLight.shadows = LightShadows.None;
        }
    }

    void Update()
    {
        _pulsePhase += Time.deltaTime * pulseSpeed;
        float pulse = (Mathf.Sin(_pulsePhase) * 0.5f + 0.5f) * 0.4f + 0.6f;

        if (_renderer != null)
        {
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(EmissionColorId, edgeGlowColor * glowIntensity * pulse);
            _renderer.SetPropertyBlock(_propBlock);
        }

        if (_edgeLight != null)
            _edgeLight.intensity = glowIntensity * pulse;
    }

    void OnDrawGizmosSelected()
    {
        // Visualizar altura máxima de salto
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.3f);
        Vector3 center = transform.position + Vector3.up * (maxJumpHeight * 0.5f);
        Gizmos.DrawWireCube(center, new Vector3(minPlatformWidth, maxJumpHeight, minPlatformWidth));

        // Mostrar zona de aterrizaje segura
        Gizmos.color = new Color(0.3f, 1f, 0.5f, 0.2f);
        float topY = transform.position.y + transform.lossyScale.y * 0.5f;
        Gizmos.DrawWireCube(
            new Vector3(transform.position.x, topY + 0.1f, transform.position.z),
            new Vector3(minPlatformWidth, 0.2f, minPlatformWidth)
        );

        // Línea de altura máxima
        Gizmos.color = Color.yellow;
        Vector3 maxJumpPos = transform.position + Vector3.up * maxJumpHeight;
        Gizmos.DrawLine(transform.position, maxJumpPos);
        Gizmos.DrawSphere(maxJumpPos, 0.15f);
    }
}
