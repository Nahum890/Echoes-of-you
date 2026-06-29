using UnityEngine;

public class DynamicTransformMotor : MonoBehaviour, IResettableLevelObject
{
    [SerializeField] Vector3 localPointA;
    [SerializeField] Vector3 localPointB;
    [SerializeField] Vector3 rotationDegreesPerSecond;
    [SerializeField] float cycleDuration = 4f;
    [SerializeField] float phaseOffset;
    [SerializeField] bool usePositionMotion = true;

    Quaternion _initialRotation;

    public void Configure(Vector3 pointA, Vector3 pointB, Vector3 rotationPerSecond, float duration, float phase, bool movePosition)
    {
        localPointA = pointA;
        localPointB = pointB;
        rotationDegreesPerSecond = rotationPerSecond;
        cycleDuration = duration;
        phaseOffset = phase;
        usePositionMotion = movePosition;
    }

    void Awake()
    {
        _initialRotation = transform.localRotation;
    }

    void Update()
    {
        if (usePositionMotion)
        {
            float t = Mathf.PingPong((Time.time + phaseOffset) / Mathf.Max(0.1f, cycleDuration), 1f);
            transform.localPosition = Vector3.Lerp(localPointA, localPointB, Smooth(t));
        }

        if (rotationDegreesPerSecond.sqrMagnitude > 0.001f)
            transform.Rotate(rotationDegreesPerSecond * Time.deltaTime, Space.Self);
    }

    public void ResetLevelState()
    {
        transform.localPosition = localPointA;
        transform.localRotation = _initialRotation;
    }

    static float Smooth(float t)
    {
        return t * t * (3f - 2f * t);
    }
}
