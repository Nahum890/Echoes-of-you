using UnityEngine;

/// <summary>
/// Autoridad de cámara: si hay una arquitectura Cinemachine activa en la escena,
/// las cámaras de gameplay manuales (SimpleFollowCamera, ThirdPersonCamera)
/// deben desactivarse y NO escribir transform. Solo hay una autoridad por escena.
/// </summary>
public static class EchoesCameraAuthority
{
    /// <summary>
    /// True si existe un CinemachineBrain o CinemachineCamera activo en la escena
    /// (v3 o v2 legacy). En ese caso la cámara manual debe ceder el control.
    /// </summary>
    public static bool IsCinemachineActiveInScene()
    {
        var brain = UnityEngine.Object.FindAnyObjectByType<Unity.Cinemachine.CinemachineBrain>();
        if (brain != null && brain.enabled)
            return true;

        var vcam = UnityEngine.Object.FindAnyObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (vcam != null && vcam.enabled && vcam.gameObject.activeInHierarchy)
            return true;

        System.Type vcamV2Type = System.Type.GetType("Cinemachine.CinemachineVirtualCamera, Cinemachine")
                              ?? System.Type.GetType("Cinemachine.CinemachineVirtualCamera");
        if (vcamV2Type != null)
        {
            var vcamObj = UnityEngine.Object.FindAnyObjectByType(vcamV2Type) as MonoBehaviour;
            if (vcamObj != null && vcamObj.enabled && vcamObj.gameObject.activeInHierarchy)
                return true;
        }

        return false;
    }
}