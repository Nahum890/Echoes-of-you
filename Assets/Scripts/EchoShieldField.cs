using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EchoShieldField : MonoBehaviour, IResettableLevelObject
{
    [SerializeField] Renderer[] hazardRenderers;
    [SerializeField] Light hazardLight;
    [SerializeField] Color activeColor = new Color(1f, 0.16f, 0.08f, 1f);
    [SerializeField] Color shieldedColor = new Color(0.25f, 0.8f, 1f, 1f);
    [SerializeField] float activeIntensity = 4f;
    [SerializeField] float shieldedIntensity = 1.3f;
    [SerializeField] PuzzleSignal completionSignal;
    [Tooltip("Distancia en metros desde la que un eco neutraliza la cortina.")]
    [SerializeField, Min(0f)] float echoReach = 1.25f;

    [SerializeField] PressurePlate controlPlate;
    int _echoCount;
    MaterialPropertyBlock _block;
    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    public bool IsShielded => _echoCount > 0;

    public void Configure(Renderer[] renderers, Light lightRef, PuzzleSignal signal)
    {
        hazardRenderers = renderers;
        hazardLight = lightRef;
        completionSignal = signal;
    }

    Collider _collider;
    BoxCollider _boxCollider;
    static readonly Collider[] _overlapBuffer = new Collider[32];

    void Awake()
    {
        _collider = GetComponent<Collider>();
        if (_collider != null)
        {
            _collider.isTrigger = true;
            _boxCollider = _collider as BoxCollider;
        }
        _block = new MaterialPropertyBlock();

        if (hazardRenderers == null || hazardRenderers.Length == 0)
            hazardRenderers = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        ApplyVisuals();
    }

    void FixedUpdate()
    {
        if (_collider == null || !_collider.enabled || !gameObject.activeInHierarchy)
            return;

        int hitCount = 0;
        if (_boxCollider != null)
        {
            Vector3 center = transform.TransformPoint(_boxCollider.center);
            Vector3 halfExtents = Vector3.Scale(_boxCollider.size, transform.lossyScale) * 0.5f;
            hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents + Vector3.one * echoReach, _overlapBuffer, transform.rotation);
        }
        else
        {
            Vector3 center = _collider.bounds.center;
            Vector3 extents = _collider.bounds.extents;
            hitCount = Physics.OverlapBoxNonAlloc(center, extents, _overlapBuffer, Quaternion.identity);
        }

        int previousEchoCount = _echoCount;
        _echoCount = controlPlate != null && controlPlate.IsPressed ? 1 : 0;
        Collider playerCollider = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider c = _overlapBuffer[i];
            if (c == null) continue;

            if (c.CompareTag("Echo") || c.CompareTag("EchoProjection"))
            {
                _echoCount++;
            }
            else if (c.CompareTag("Player"))
            {
                playerCollider = c;
            }
        }

        if (_echoCount > 0)
        {
            completionSignal?.MarkSatisfied();
        }

        if (_echoCount != previousEchoCount)
        {
            ApplyVisuals();
        }

        if (playerCollider != null && !IsShielded)
        {
            if (OverlapsPlayerBody(playerCollider))
            {
                LevelRuntimeController.Instance?.HandlePlayerDeath(playerCollider.transform.position, 0.8f);
            }
        }
    }

    bool OverlapsPlayerBody(Collider playerCollider)
    {
        return Physics.ComputePenetration(_collider, transform.position, transform.rotation,
            playerCollider, playerCollider.transform.position, playerCollider.transform.rotation, out _, out _);
    }

    public void ResetLevelState()
    {
        _echoCount = 0;
        ApplyVisuals();
    }

    void ApplyVisuals()
    {
        Color color = IsShielded ? shieldedColor : activeColor;
        Color emission = color * (IsShielded ? 1.1f : 2.2f);

        if (hazardRenderers != null)
        {
            for (int i = 0; i < hazardRenderers.Length; i++)
            {
                Renderer rendererRef = hazardRenderers[i];
                if (rendererRef == null)
                    continue;

                rendererRef.GetPropertyBlock(_block);
                _block.SetColor(ColorId, color);
                _block.SetColor(Shader.PropertyToID("_BaseColor"), color);
                _block.SetColor(Shader.PropertyToID("_EmissiveColor"), emission);
                _block.SetColor(EmissionColorId, emission);
                rendererRef.SetPropertyBlock(_block);
            }
        }

        if (hazardLight != null)
        {
            hazardLight.color = color;
            hazardLight.intensity = IsShielded ? shieldedIntensity : activeIntensity;
        }
    }
}
