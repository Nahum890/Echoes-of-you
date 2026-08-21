using UnityEngine;

/// <summary>
/// A bridge that only solidifies (collides) and glows while a specific PuzzleSignal is active.
/// </summary>
public class GhostBridge : MonoBehaviour, IResettableLevelObject
{
    [SerializeField] PuzzleSignal signal;
    [SerializeField] GameObject visualMesh;
    [SerializeField] Collider bridgeCollider;
    [SerializeField] Color activeColor = new Color(0f, 0.9f, 1f, 0.85f);
    [SerializeField] Color inactiveColor = new Color(0f, 0.9f, 1f, 0.08f);

    Material _material;
    bool _wasActive;

    // Cache material property IDs
    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    // Los shaders del proyecto (Echoes/EchoLiminal) exponen _BaseColor, no _Color.
    // Escribimos en la propiedad que el material realmente tenga, en vez de
    // asumir el nombre del pipeline built-in.
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");

    static void ApplyTint(Material m, Color tint, Color emission)
    {
        if (m == null) return;
        if (m.HasProperty(BaseColorId)) m.SetColor(BaseColorId, tint);
        if (m.HasProperty(ColorId)) m.SetColor(ColorId, tint);
        if (m.HasProperty(EmissionColorId)) m.SetColor(EmissionColorId, emission);
        if (m.HasProperty(EmissiveColorId)) m.SetColor(EmissiveColorId, emission);
    }


    void Awake()
    {
        if (bridgeCollider == null)
            bridgeCollider = GetComponent<Collider>();
        if (visualMesh == null)
            visualMesh = gameObject;

        Renderer r = visualMesh.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            _material = r.material; // Create material instance
            _material.EnableKeyword("_EMISSION");
        }

        UpdateState(false);
    }

    void Start()
    {
        if (signal != null)
        {
            signal.SignalChanged += OnSignalChanged;
            UpdateState(signal.IsSatisfied);
        }
    }

    void OnDestroy()
    {
        if (signal != null)
            signal.SignalChanged -= OnSignalChanged;
    }

    void OnSignalChanged(PuzzleSignal sig, bool satisfied)
    {
        UpdateState(satisfied);
    }

    public void Configure(PuzzleSignal targetSignal, Color actCol, Color inactCol)
    {
        signal = targetSignal;
        activeColor = actCol;
        inactiveColor = inactCol;
        if (signal != null)
        {
            signal.SignalChanged -= OnSignalChanged;
            signal.SignalChanged += OnSignalChanged;
            UpdateState(signal.IsSatisfied);
        }
    }

    void UpdateState(bool active)
    {
        if (bridgeCollider != null)
            bridgeCollider.enabled = active;

        if (_material != null)
        {
            ApplyTint(_material,
                active ? activeColor : inactiveColor,
                active ? activeColor * 1.5f : inactiveColor * 0.1f);
        }

        if (active && !_wasActive)
        {
            GameFeelController.Instance?.PlayMechanicTick(transform.position, 0.8f);
        }
        _wasActive = active;
    }

    public void ResetLevelState()
    {
        UpdateState(signal != null ? signal.IsSatisfied : false);
    }
}
