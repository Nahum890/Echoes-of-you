using UnityEngine;

/// <summary>
/// Asegura rig y animator en el jugador. Los parámetros los maneja PlayerController.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-10)]
public class PlayerLocomotionAnimator : MonoBehaviour
{
    void Awake()
    {
        if (GetComponent<EchoPlayback>() != null)
            return;

        PlayerAnimationRuntimeBootstrap.ApplyToHierarchy(gameObject);
    }
}
