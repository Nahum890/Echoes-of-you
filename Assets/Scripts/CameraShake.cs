using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] Vector3 maxPositionShake = new Vector3(0.18f, 0.18f, 0.08f);
    [SerializeField] Vector3 maxRotationShake = new Vector3(2f, 2f, 3f);
    [SerializeField] float traumaDecay = 1.8f;
    [SerializeField] float frequency = 24f;

    float _trauma;
    float _seed;

    public Vector3 PositionOffset { get; private set; }
    public Quaternion RotationOffset { get; private set; } = Quaternion.identity;

    void Awake()
    {
        _seed = Random.value * 100f;
    }

    public void AddShake(float amount)
    {
        _trauma = Mathf.Clamp01(_trauma + Mathf.Abs(amount));
    }

    void Update()
    {
        if (_trauma <= 0f)
        {
            PositionOffset = Vector3.zero;
            RotationOffset = Quaternion.identity;
            return;
        }

        float intensity = _trauma * _trauma;
        float time = Time.unscaledTime * frequency;

        PositionOffset = new Vector3(
            SampleNoise(0f, time) * maxPositionShake.x,
            SampleNoise(17f, time) * maxPositionShake.y,
            SampleNoise(29f, time) * maxPositionShake.z) * intensity;

        Vector3 euler = new Vector3(
            SampleNoise(43f, time) * maxRotationShake.x,
            SampleNoise(59f, time) * maxRotationShake.y,
            SampleNoise(71f, time) * maxRotationShake.z) * intensity;

        RotationOffset = Quaternion.Euler(euler);
        _trauma = Mathf.MoveTowards(_trauma, 0f, traumaDecay * Time.unscaledDeltaTime);
    }

    float SampleNoise(float offset, float time)
    {
        return (Mathf.PerlinNoise(_seed + offset, time) - 0.5f) * 2f;
    }
}
