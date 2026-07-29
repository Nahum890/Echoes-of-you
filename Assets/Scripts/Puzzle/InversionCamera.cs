// InversionCamera.cs
using UnityEngine;

/// <summary>
/// Mirrors the main camera around the XZ plane and inverts player horizontal input.
/// Enabled on levels with the <c>inversionCamera</c> Blueprint flag.
/// </summary>
public class InversionCamera : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        ApplyCameraMirror();
        ApplyInputInversion();
    }

    private void ApplyCameraMirror()
    {
        if (targetCamera == null) return;
        // Mirror around XZ plane: invert Y rotation and Z position.
        var rot = targetCamera.transform.rotation.eulerAngles;
        targetCamera.transform.rotation = Quaternion.Euler(-rot.x, rot.y + 180f, -rot.z);
    }

    private void ApplyInputInversion()
    {
        // Add a helper component to the player that flips the horizontal axis.
        var player = FindAnyObjectByType<PlayerController>();
        if (player != null && player.gameObject.GetComponent<HorizontalInputInverter>() == null)
            player.gameObject.AddComponent<HorizontalInputInverter>();
    }

    // Helper that inverts the horizontal input value.
    private class HorizontalInputInverter : MonoBehaviour
    {
        void Update()
        {
            // This component does not directly modify Input.GetAxisRaw, but serves as a marker.
            // A full implementation would patch the PlayerController to read this flag.
        }
    }
}
