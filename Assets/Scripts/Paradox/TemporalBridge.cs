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
    private int _echoCount = 0;
    private bool _isActive = false;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        if (bridgeCollider == null)
            bridgeCollider = GetComponent<Collider>();
        if (visualMesh == null)
            visualMesh = gameObject;

        Renderer r = visualMesh.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            _material = r.material;
            _material.EnableKeyword("_EMISSION");
        }

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

        if (_material != null)
        {
            _material.SetColor(ColorId, active ? activeColor : inactiveColor);
            _material.SetColor(EmissionColorId, active ? activeColor * 1.8f : inactiveColor * 0.1f);
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
