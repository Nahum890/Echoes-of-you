using UnityEngine;

/// <summary>
/// Ensures core audio managers exist in every scene.
/// Add to a bootstrapper GameObject or call from LevelEnvironmentBootstrap.
/// </summary>
public class AudioManagersBootstrap : MonoBehaviour
{
    [Header("Auto-setup")]
    [SerializeField] bool setupOnAwake = true;

    void Awake()
    {
        if (!setupOnAwake) return;

        // Core managers (DontDestroyOnLoad)
        EchoesAudioManager.EnsureExists();
        MusicStateMachine.EnsureExists();
        AmbienceManager.EnsureExists();
        TransitionManager.EnsureExists();

        // Scene-specific: GameFeelController is already in Level_01 on Main Camera
        // MenuHoverSystem is in MainMenu
    }

    /// <summary>
    /// Call from level start to set initial music state.
    /// </summary>
    public void SetLevelMusic(MusicStateMachine.MusicState state, bool immediate = true)
    {
        var music = MusicStateMachine.EnsureExists();
        if (immediate)
            music.SetStateImmediate(state);
        else
            music.SetState(state);
    }

    /// <summary>
    /// Call to enable/disable ambience layers per zone.
    /// </summary>
    public void SetAmbienceZone(string zonePreset)
    {
        var amb = AmbienceManager.Instance;
        if (amb == null) return;

        switch (zonePreset.ToLower())
        {
            case "hallway":
                amb.SetLayerVolume("hallway", 0.15f);
                amb.SetLayerVolume("ventilation", 0.1f);
                amb.SetLayerVolume("roomtone", 0.05f);
                break;
            case "classroom":
                amb.SetLayerVolume("roomtone", 0.2f);
                amb.SetLayerVolume("ventilation", 0.08f);
                amb.SetLayerVolume("fluorescenthum", 0.1f);
                break;
            case "industrial":
                amb.SetLayerVolume("industrial", 0.15f);
                amb.SetLayerVolume("distantclang", 0.1f);
                amb.SetLayerVolume("tapehiss", 0.05f);
                break;
            case "memory":
                amb.SetLayerVolume("memorywhisper", 0.15f);
                amb.SetLayerVolume("tapehiss", 0.08f);
                break;
            case "silence":
                amb.StopAll();
                break;
            default:
                // Default balanced mix
                amb.SetLayerVolume("roomtone", 0.15f);
                amb.SetLayerVolume("ventilation", 0.08f);
                amb.SetLayerVolume("industrial", 0.05f);
                break;
        }
    }
}