using UnityEngine;

/// <summary>
/// El entorno se degrada con la presencia de ecos. Cada eco que pisa esta plataforma 
/// reduce su durabilidad. Al llegar a 0, la plataforma colapsa y no es transitable.
/// </summary>
public class ErosionSystem : MonoBehaviour, IResettableLevelObject
{
    [SerializeField] int maxDurability = 3;
    [SerializeField] Color healthyColor = new Color(0.35f, 0.4f, 0.45f, 1f);
    [SerializeField] Color warningColor = new Color(0.75f, 0.3f, 0.2f, 1f);
    [SerializeField] Color collapsedColor = new Color(0.2f, 0.1f, 0.1f, 0.05f);

    private int _currentDurability;
    private Collider _collider;
    private Renderer _renderer;
    private Material _material;

    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
        {
            _material = _renderer.material;
            _material.EnableKeyword("_EMISSION");
        }
        
        ResetLevelState();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_currentDurability <= 0) return;

        if (other.CompareTag("Echo"))
        {
            Degrade();
        }
    }

    private void Degrade()
    {
        _currentDurability--;
        UpdateVisuals();

        if (_currentDurability <= 0)
        {
            Collapse();
        }
        else
        {
            GameFeelController.Instance?.PlayMechanicTick(transform.position, 0.6f);
        }
    }

    private void UpdateVisuals()
    {
        if (_material == null) return;

        float t = 1f - ((float)_currentDurability / maxDurability);
        Color targetColor = Color.Lerp(healthyColor, warningColor, t);
        
        _material.SetColor(ColorId, targetColor);
        _material.SetColor(EmissionColorId, targetColor * (t * 1.5f));
    }

    private void Collapse()
    {
        if (_collider != null) _collider.enabled = false;
        if (_renderer != null) _renderer.enabled = false;

        // Visual and haptic feedback
        GameFeelController.Instance?.PlayCameraShake(0.35f);
        // Play explosion sound or particle if available
    }

    public void ResetLevelState()
    {
        _currentDurability = maxDurability;
        if (_collider != null) _collider.enabled = true;
        if (_renderer != null) _renderer.enabled = true;
        UpdateVisuals();
    }
}
