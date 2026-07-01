using UnityEngine;

/// <summary>
/// Disintegrates any Echo that enters its trigger volume, unless the light beam from the 
/// emitter is blocked by an occluder (the player or a pushable object like a blackboard).
/// </summary>
public class EchoDisintegrationZone : MonoBehaviour, IResettableLevelObject
{
    [Header("Setup")]
    public Transform lightEmitter;
    public BoxCollider zoneCollider;
    public Renderer hazardRenderer;
    public LayerMask occlusionLayers;
    public Color activeColor = new Color(0.9f, 0.2f, 0.4f, 0.85f);
    public Color inactiveColor = new Color(0.9f, 0.2f, 0.4f, 0.1f);

    [Header("Debug Status")]
    public bool isBlocked = false;

    private Material _material;
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        if (zoneCollider == null)
            zoneCollider = GetComponent<BoxCollider>();
        
        if (zoneCollider != null)
            zoneCollider.isTrigger = true;

        if (hazardRenderer != null)
        {
            _material = hazardRenderer.material;
            _material.EnableKeyword("_EMISSION");
        }

        // Default: everything except triggers and echoes
        if (occlusionLayers == 0)
        {
            occlusionLayers = LayerMask.GetMask("Default", "Ground", "Player");
        }
    }

    void Update()
    {
        if (lightEmitter == null || zoneCollider == null)
            return;

        Vector3 start = lightEmitter.position;
        Vector3 target = zoneCollider.bounds.center;
        Vector3 dir = target - start;
        float dist = dir.magnitude;

        // Perform raycast to check if an object is casting a shadow over the zone
        isBlocked = Physics.Raycast(start, dir.normalized, out RaycastHit hit, dist, occlusionLayers, QueryTriggerInteraction.Ignore);

        // If blocked, the filter is inactive (shadowed / safe for Echo)
        if (isBlocked)
        {
            if (zoneCollider.enabled)
                zoneCollider.enabled = false;
            
            ApplyVisualState(false);
        }
        else
        {
            if (!zoneCollider.enabled)
                zoneCollider.enabled = true;
            
            ApplyVisualState(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isBlocked)
            return;

        if (other.CompareTag("Echo") || other.CompareTag("EchoProjection"))
        {
            var playback = other.GetComponent<EchoPlayback>();
            if (playback != null)
            {
                // Play sound and particle feedback
                GameFeelController.Instance?.PlayEchoFade(other.transform.position);
                GameFeelController.Instance?.PlayCameraShake(0.2f);
                playback.FadeOutAndDestroy(0.2f);
            }
        }
    }

    void ApplyVisualState(bool active)
    {
        if (_material == null) return;

        Color targetColor = active ? activeColor : inactiveColor;
        _material.SetColor(ColorId, targetColor);
        if (_material.HasProperty(EmissionColorId))
        {
            _material.SetColor(EmissionColorId, targetColor * (active ? 2.0f : 0.1f));
        }
    }

    public void ResetLevelState()
    {
        isBlocked = false;
        if (zoneCollider != null)
            zoneCollider.enabled = true;
        ApplyVisualState(true);
    }
}
