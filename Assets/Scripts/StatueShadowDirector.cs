using UnityEngine;

/// <summary>
/// N03 liminal game feel: when a bound plate is pressed, the founder statue
/// rotates and the directional light tilts, casting a long shadow that
/// visually communicates the twist ("the echo can drown you if it stays too
/// long"). Lightweight, self-contained, no subsystem dependencies.
/// </summary>
public class StatueShadowDirector : MonoBehaviour
{
    [SerializeField] float statueYaw = 15f;
    [SerializeField] float sunTilt = 10f;
    [SerializeField] float blendSpeed = 2f;

    PressurePlate _plate;
    Quaternion _restStatateRot;
    Quaternion _targetStatateRot;
    Quaternion _restSunRot;
    Quaternion _targetSunRot;
    Light _sun;
    bool _active;
    bool _lastPressed;

    void Awake()
    {
        _restStatateRot = transform.rotation;
        _targetStatateRot = Quaternion.Euler(0f, statueYaw, 0f) * _restStatateRot;

        _sun = FindAnyObjectByType<Light>();
        if (_sun != null && _sun.type == LightType.Directional)
        {
            _restSunRot = _sun.transform.rotation;
            _targetSunRot = Quaternion.Euler(sunTilt, 0f, 0f) * _restSunRot;
        }
    }

    public void BindPlate(PressurePlate plate)
    {
        _plate = plate;
        if (_plate != null) _plate.PressedChanged += OnPlate;
    }

    void OnPlate(bool pressed)
    {
        _active = pressed;
        if (pressed && !_lastPressed)
            GameFeelController.Instance?.PlayMechanicTick(transform.position, 0.5f);
        _lastPressed = pressed;
    }

    void Update()
    {
        if (_sun == null) return;
        float k = Mathf.Clamp01(Time.deltaTime * blendSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation,
            _active ? _targetStatateRot : _restStatateRot, k);
        _sun.transform.rotation = Quaternion.Slerp(_sun.transform.rotation,
            _active ? _targetSunRot : _restSunRot, k);
    }

    void OnDestroy()
    {
        if (_plate != null) _plate.PressedChanged -= OnPlate;
    }
}
