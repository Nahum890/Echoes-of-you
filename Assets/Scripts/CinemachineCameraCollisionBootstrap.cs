using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Auto-configura CinemachineDeoccluder en todas las CinemachineCamera activas
/// cuando se detecta una CinemachineBrain en la escena. Esto evita que la cámara
/// gameplay atraviese paredes en niveles que usan Cinemachine (cuando ThirdPersonCamera
/// se deshabilita automáticamente por detección de brain).
///
/// Ejecuta una vez en el primer Update; luego se autodestruye para no consumir ciclo.
/// </summary>
[DefaultExecutionOrder(1000)]
public class CinemachineCameraCollisionBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (FindAnyObjectByType<CinemachineCameraCollisionBootstrap>() != null)
            return;

        if (FindAnyObjectByType<Unity.Cinemachine.CinemachineBrain>() == null)
            return;

        var go = new GameObject("CinemachineCameraCollisionBootstrap");
        go.AddComponent<CinemachineCameraCollisionBootstrap>();
        DontDestroyOnLoad(go);
    }

    void Start()
    {
        ConfigureAllVcams();
        // Tras configurar, podemos deshabilitar este componente. No destruimos el GO
        // para que persista entre escenas (no re-crea el AutoCreate).
        enabled = false;
    }

    void ConfigureAllVcams()
    {
        var vcams = FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include);
        if (vcams == null) return;

        LayerMask avoidWalls = ~(1 << 8); // Player
        avoidWalls &= ~(1 << 9);          // Echo
        avoidWalls &= ~(1 << 11);         // PressurePlate

        for (int i = 0; i < vcams.Length; i++)
        {
            CinemachineCamera vcam = vcams[i];
            if (vcam == null) continue;

            CinemachineDeoccluder deoccluder = vcam.GetComponent<CinemachineDeoccluder>();
            if (deoccluder == null)
            {
                deoccluder = vcam.gameObject.AddComponent<CinemachineDeoccluder>();
                Debug.Log($"[CinemachineDeoccluder] Auto-added to '{vcam.gameObject.name}'.");
            }

            // Configurar: los campos son públicos en Cinemachine 3.x (no sub-struct).
            deoccluder.CollideAgainst = avoidWalls;
            deoccluder.IgnoreTag = string.Empty;
            deoccluder.MinimumDistanceFromTarget = 0.3f;

            var oa = deoccluder.AvoidObstacles;
            oa.Enabled = true;
            oa.CameraRadius = 0.25f;
            oa.DistanceLimit = 0.6f;
            oa.Damping = 0.4f;
            oa.SmoothingTime = 0.2f;
            oa.Strategy = CinemachineDeoccluder.ObstacleAvoidance.ResolutionStrategy.PullCameraForward;
            deoccluder.AvoidObstacles = oa;
        }
    }
}
