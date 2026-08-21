using UnityEngine;

/// <summary>
/// Cable visible entre una placa y la puerta que abre.
///
/// ANTI_PATTERNS RULE-ANTI-007 prohibe que una placa accione una puerta sin un
/// cable explicito, pero PuzzleWire solo resuelve la logica: no dibuja nada.
/// Este componente cierra ese hueco y ademas cumple la regla de UX de que el
/// jugador vea "que cambio" sin depender de texto: el conducto se enciende
/// mientras la placa esta pisada y se apaga al soltarla.
///
/// El trazado no es una recta en el aire: baja al suelo, recorre el zocalo y
/// sube por el marco de la puerta, como una instalacion electrica de colegio.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class PuzzleCable : MonoBehaviour, IResettableLevelObject
{
    [SerializeField] PressurePlate source;
    [SerializeField] DoorController target;

    [Header("Color")]
    [SerializeField] Color inactiveColor = new Color(0.16f, 0.20f, 0.22f, 1f);
    [SerializeField] Color activeColor = new Color(0.31f, 0.75f, 0.73f, 1f);
    [SerializeField] float pulseSpeed = 2.2f;

    LineRenderer _line;
    Material _material;
    bool _wasPressed;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");

    /// <summary>Configuracion desde el pase de editor.</summary>
    public void Configure(PressurePlate plate, DoorController door, Color active)
    {
        source = plate;
        target = door;
        activeColor = active;
    }

    void Awake()
    {
        _line = GetComponent<LineRenderer>();
        if (_line != null && _line.sharedMaterial != null)
            _material = _line.material;   // instancia propia, no toca el asset
    }

    void Start()
    {
        Apply(false, true);
    }

    void Update()
    {
        bool pressed = source != null && source.IsPressed;
        if (pressed != _wasPressed)
        {
            Apply(pressed, false);
            _wasPressed = pressed;
        }
        else if (pressed)
        {
            // Pulso suave mientras esta activo: el cable "lleva corriente".
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            Tint(Color.Lerp(activeColor * 0.75f, activeColor, t));
        }
    }

    void Apply(bool pressed, bool force)
    {
        Tint(pressed ? activeColor : inactiveColor);
        if (_line != null)
            _line.widthMultiplier = pressed ? 0.09f : 0.06f;
        if (!force && pressed)
            GameFeelController.Instance?.PlayMechanicTick(transform.position, 0.5f);
    }

    void Tint(Color c)
    {
        if (_line != null)
        {
            _line.startColor = c;
            _line.endColor = c;
        }
        if (_material == null) return;

        // Mismo criterio que TemporalBridge: escribimos en la propiedad que el
        // material realmente expone (los shaders Echoes/* usan _BaseColor).
        if (_material.HasProperty(BaseColorId)) _material.SetColor(BaseColorId, c);
        if (_material.HasProperty(ColorId)) _material.SetColor(ColorId, c);
        if (_material.HasProperty(EmissionColorId)) _material.SetColor(EmissionColorId, c * 1.6f);
        if (_material.HasProperty(EmissiveColorId)) _material.SetColor(EmissiveColorId, c * 1.6f);
    }

    public void ResetLevelState()
    {
        _wasPressed = false;
        Apply(false, true);
    }
}
