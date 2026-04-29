using System;
using UnityEngine;

/// <summary>
/// Placa de presión: detecta Player y Echo. Cambia de color cuando está activa.
/// Incluye indicador visual con luz propia para ser fácilmente visible.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour, IResettableLevelObject
{
    [SerializeField] string playerTag = "Player";
    [SerializeField] string echoTag = "Echo";

    [Header("Visual Feedback")]
    [SerializeField] Color inactiveColor = new Color(0.06f, 0.35f, 0.85f, 1f);    // Azul eléctrico tenue
    [SerializeField] Color activeColor = new Color(0.1f, 0.6f, 1f, 1f);            // Azul eléctrico brillante
    [SerializeField] Color emissionInactive = new Color(0.02f, 0.15f, 0.5f, 1f);   // Glow azul suave
    [SerializeField] Color emissionActive = new Color(0.1f, 0.5f, 1f, 1f);         // Glow azul fuerte
    [SerializeField] float pulseSpeed = 2.5f;
    [SerializeField] bool createIndicatorLight = true;
    [SerializeField] float lightIntensity = 3f;
    [SerializeField] float lightRange = 5f;

    [Header("Behavior")]
    public float autoReleaseTimer = 0f;

    bool _pressed;
    float _timeUntilRelease;
    Renderer _renderer;
    MaterialPropertyBlock _propBlock;
    Light _indicatorLight;
    float _pulsePhase;

    // Cache material property IDs
    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    public bool IsPressed => _pressed;

    public event Action<bool> PressedChanged;

    void Awake()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;

        _renderer = GetComponentInChildren<Renderer>();
        _propBlock = new MaterialPropertyBlock();

        // Create indicator light if needed
        if (createIndicatorLight)
        {
            GameObject lightObj = new GameObject("PlateLight");
            lightObj.transform.SetParent(transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            _indicatorLight = lightObj.AddComponent<Light>();
            _indicatorLight.type = LightType.Point;
            _indicatorLight.color = inactiveColor;
            _indicatorLight.intensity = lightIntensity * 0.5f;
            _indicatorLight.range = lightRange;
            _indicatorLight.shadows = LightShadows.None;
        }

        // Enable emission on material
        if (_renderer != null)
        {
            Material mat = _renderer.material;
            mat.EnableKeyword("_EMISSION");
            // Set initial color
            UpdateVisuals(false);
        }
    }

    void SetPressed(bool value)
    {
        if (_pressed == value)
            return;

        _pressed = value;
        PressedChanged?.Invoke(_pressed);
        UpdateVisuals(_pressed);

        if (_pressed)
        {
            GameFeelController.Instance?.PlayPlatePress(transform.position);
            Debug.Log($"[QA PressurePlate] {gameObject.name} ACTIVADO.");
        }
        else
        {
            Debug.Log($"[QA PressurePlate] {gameObject.name} DESACTIVADO.");
        }
    }

    void Update()
    {
        if (_renderer == null) return;

        // Pulse effect when inactive (attract attention)
        if (!_pressed)
        {
            _pulsePhase += Time.deltaTime * pulseSpeed;
            float pulse = (Mathf.Sin(_pulsePhase) * 0.5f + 0.5f) * 0.3f + 0.7f;

            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_propBlock);
            Color emColor = emissionInactive * pulse;
            _propBlock.SetColor(EmissionColorId, emColor);
            _renderer.SetPropertyBlock(_propBlock);

            if (_indicatorLight != null)
                _indicatorLight.intensity = lightIntensity * 0.3f + lightIntensity * 0.2f * pulse;
        }
    }

    void UpdateVisuals(bool pressed)
    {
        if (_renderer != null)
        {
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(ColorId, pressed ? activeColor : inactiveColor);
            _propBlock.SetColor(EmissionColorId, pressed ? emissionActive : emissionInactive);
            _renderer.SetPropertyBlock(_propBlock);
        }

        if (_indicatorLight != null)
        {
            _indicatorLight.color = pressed ? activeColor : inactiveColor;
            _indicatorLight.intensity = pressed ? lightIntensity : lightIntensity * 0.5f;
        }
    }

    void FixedUpdate()
    {
        var box = GetComponent<BoxCollider>();
        Vector3 center = transform.TransformPoint(box.center);
        Vector3 halfExtents = Vector3.Scale(box.size, transform.lossyScale) * 0.5f;

        // Inflate detection slightly upward
        halfExtents.y += 0.2f;
        center.y += 0.1f;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
        
        bool foundActor = false;
        foreach (var c in hits)
        {
            if (c.CompareTag(playerTag) || c.CompareTag(echoTag))
            {
                foundActor = true;
                break;
            }
        }

        if (foundActor)
        {
            _timeUntilRelease = Time.time + autoReleaseTimer;
            SetPressed(true);
        }
        else
        {
            if (autoReleaseTimer > 0f)
            {
                if (Time.time >= _timeUntilRelease)
                    SetPressed(false);
            }
            else
            {
                SetPressed(false);
            }
        }
    }

    public void ResetLevelState()
    {
        SetPressed(false);
        _pulsePhase = 0f;
        UpdateVisuals(false);
    }
}
