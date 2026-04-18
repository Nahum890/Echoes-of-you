using UnityEngine;

/// <summary>
/// Cámara tercera persona: sigue al objetivo con offset y yaw con ratón.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2.2f, -4.5f);
    public float followSmooth = 10f;
    public float mouseSensitivity = 2f;
    public float minPitch = -20f;
    public float maxPitch = 55f;

    float _yaw;
    float _pitch = 12f;

    void Start()
    {
        if (target != null)
        {
            _yaw = target.eulerAngles.y;
            Vector3 euler = transform.eulerAngles;
            _pitch = euler.x;
            if (_pitch > 180f) _pitch -= 360f;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        _yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 desired = target.position + rot * offset;
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
        transform.rotation = rot;
    }
}
