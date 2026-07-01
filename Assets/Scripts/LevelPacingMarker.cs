using UnityEngine;

/// <summary>
/// Marcador de sección de ritmo (movimiento, sync, escalada, aha, clímax).
/// Sin texto: solo guía por geometría y cámara.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LevelPacingMarker : MonoBehaviour
{
    public enum Section
    {
        Movement,
        Synchronization,
        Escalation,
        AhaMoment,
        TraversalClimax
    }

    [SerializeField] Section section = Section.Movement;
    [SerializeField] bool adjustCameraOnEnter;
    [SerializeField] EchoesCameraIdentity cameraHint = EchoesCameraIdentity.DynamicFollow;

    static Section _currentSection = Section.Movement;

    public static Section CurrentSection => _currentSection;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _currentSection = section;

        if (adjustCameraOnEnter)
        {
            CinematicCameraDynamics dynamics = FindAnyObjectByType<CinematicCameraDynamics>();
            if (dynamics != null && LevelCameraProfiles.TryGet(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, out LevelCameraProfiles.Profile profile))
            {
                profile.identity = cameraHint;
                dynamics.ApplyProfile(profile);
            }
        }
    }
}
