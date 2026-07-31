using UnityEngine;

public class SimpleFollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1.8f, 0f);

    [Header("Orbit")]
    public float distance = 5.5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float zoomSpeed = 4f;

    [Header("Mouse / Angles")]
    public float yaw;
    public float pitch = 18f;
    public float minPitch = -30f;
    public float maxPitch = 75f;
    public float rotationSpeed = 120f;
    public float mouseSensitivity = 1f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.12f;
    public float rotationSmoothTime = 0.08f;

    [Header("Collision")]
    public LayerMask obstacleMask = -1;
    public float sphereCastRadius = 0.3f;

    Vector3 _velocity = Vector3.zero;
    Vector2 _smoothMouse;

    void Start()
    {
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        if (target != null) Snap();
    }

    void LateUpdate()
    {
        if (target == null) return;
        if (!Application.isPlaying) return;

        if (Cursor.lockState == CursorLockMode.Locked || true)
        {
            mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
            float rawX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float rawY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            float deadZone = 0.05f;
            if (System.Math.Abs(rawX) < deadZone) rawX = 0f;
            if (System.Math.Abs(rawY) < deadZone) rawY = 0f;
            yaw   += rawX * rotationSpeed * Time.unscaledDeltaTime;
            pitch -= rawY * rotationSpeed * Time.unscaledDeltaTime;
        }
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        Vector3 tp = target.position + targetOffset;
        Quaternion orbitRot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPos = tp - orbitRot * Vector3.forward * distance;

        if (obstacleMask != 0)
        {
            Vector3 dirToCam = (desiredPos - tp).normalized;
            float maxCheck = distance + sphereCastRadius;
            if (Physics.SphereCast(tp, sphereCastRadius, dirToCam, out RaycastHit hit, maxCheck, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                float safeDist = Mathf.Max(0.5f, hit.distance - sphereCastRadius);
                safeDist = Mathf.Min(safeDist, distance);
                desiredPos = tp + dirToCam * safeDist;
            }
        }

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _velocity, positionSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

        Quaternion lookAt = Quaternion.LookRotation(tp - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, Time.unscaledDeltaTime / rotationSmoothTime);
    }

    public void FocusOn(Vector3 worldPos, float duration) { }

    void Snap()
    {
        Vector3 tp = target.TransformPoint(targetOffset);
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = tp - rot * Vector3.forward * distance;
        transform.LookAt(tp);
    }
}