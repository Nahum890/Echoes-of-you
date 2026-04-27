using UnityEngine;

public class CameraWorldUpOverride : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 positionOffset = Vector3.zero;
    [SerializeField] float positionDamping = 10f;
    [SerializeField] float rotationDamping = 12f;

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + target.rotation * positionOffset;
        float positionBlend = 1f - Mathf.Exp(-positionDamping * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionBlend);

        Vector3 desiredUp = target.up;
        Vector3 desiredForward = Vector3.ProjectOnPlane(transform.forward, desiredUp);
        if (desiredForward.sqrMagnitude < 0.001f)
            desiredForward = Vector3.ProjectOnPlane(target.forward, desiredUp);
        if (desiredForward.sqrMagnitude < 0.001f)
            desiredForward = Vector3.Cross(target.right, desiredUp);

        Quaternion targetRotation = Quaternion.LookRotation(desiredForward.normalized, desiredUp);
        float rotationBlend = 1f - Mathf.Exp(-rotationDamping * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationBlend);
    }
}
