using UnityEngine;

/// <summary>
/// Un puente que solo se solidifica y brilla cuando un Eco (pasado del jugador) 
/// está parado o caminando sobre él. El jugador en el presente lo ve como un vacío.
/// </summary>
public class TemporalBridge : MonoBehaviour, IResettableLevelObject
{
    [SerializeField] Collider bridgeCollider;
    [SerializeField] GameObject visualMesh;
    [SerializeField] Color activeColor = new Color(0f, 0.9f, 1f, 0.85f);
    [SerializeField] Color inactiveColor = new Color(0f, 0.9f, 1f, 0.08f);

    private Material _material;
    private Material[] _materials = System.Array.Empty<Material>();
    private int _echoCount = 0;
    private bool _isActive = false;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

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

        // El puente lo forma una fila de pupitres: hay que teñirlos todos,
        // no solo el primer renderer que aparezca.
        var renderers = visualMesh.GetComponentsInChildren<Renderer>(true);
        var mats = new System.Collections.Generic.List<Material>(renderers.Length);
        foreach (var rend in renderers)
        {
            foreach (var inst in rend.materials)
            {
                if (inst == null) continue;
                inst.EnableKeyword("_EMISSION");
                mats.Add(inst);
            }
        }
        _materials = mats.ToArray();
        if (_materials.Length > 0) _material = _materials[0];

        UpdateState(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Echo"))
        {
            _echoCount++;
            EvaluateState();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Echo"))
        {
            _echoCount = Mathf.Max(0, _echoCount - 1);
            EvaluateState();
        }
    }

    private void EvaluateState()
    {
        bool shouldBeActive = _echoCount > 0;
        if (shouldBeActive != _isActive)
        {
            UpdateState(shouldBeActive);
        }
    }

    private void UpdateState(bool active)
    {
        _isActive = active;
        if (bridgeCollider != null)
            bridgeCollider.enabled = active;

        if (_materials.Length > 0)
        {
            foreach (var m in _materials)
                ApplyTint(m,
                    active ? activeColor : inactiveColor,
                    active ? activeColor * 1.8f : inactiveColor * 0.1f);
        }

        if (active)
        {
            GameFeelController.Instance?.PlayMechanicTick(transform.position, 0.9f);
        }
    }

    public void ResetLevelState()
    {
        _echoCount = 0;
        UpdateState(false);
    }
}
