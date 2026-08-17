using UnityEngine;

/// <summary>
/// Handles level transition audio (fade in/out stings) and global audio events.
/// Subscribes to GameStateController.StateChanged for automatic transitions.
/// </summary>
public class TransitionManager : MonoBehaviour
{
    [Header("Transition Clips")]
    [SerializeField] AudioClip transitionInClip;
    [SerializeField] AudioClip transitionOutClip;

    [Header("Settings")]
    [SerializeField] float transitionVolume = 0.8f;

    AudioSource _transitionSource;
    EchoesAudioManager _audioMgr;

    void Awake()
    {
        _audioMgr = EchoesAudioManager.EnsureExists();

        _transitionSource = gameObject.AddComponent<AudioSource>();
        _transitionSource.spatialBlend = 0f;
        _transitionSource.playOnAwake = false;
        if (_audioMgr != null)
            _transitionSource.outputAudioMixerGroup = _audioMgr.FindGroup("UI"); // transitions on UI bus
    }

    void OnEnable()
    {
        if (GameStateController.Instance != null)
            GameStateController.Instance.StateChanged += OnGameStateChanged;
    }

    void OnDisable()
    {
        if (GameStateController.Instance != null)
            GameStateController.Instance.StateChanged -= OnGameStateChanged;
    }

    void OnGameStateChanged(GameStateController.GameState newState)
    {
        // Play transition out when leaving gameplay states
        // Play transition in when entering gameplay states
        // This is simplified - adjust per your state machine
        switch (newState)
        {
            case GameStateController.GameState.Playing:
                PlayTransitionIn();
                break;
            case GameStateController.GameState.Paused:
            case GameStateController.GameState.MainMenu:
            case GameStateController.GameState.LevelComplete:
                PlayTransitionOut();
                break;
        }
    }

    public void PlayTransitionIn()
    {
        if (transitionInClip != null)
        {
            _transitionSource.pitch = 1f;
            _transitionSource.PlayOneShot(transitionInClip, transitionVolume);
        }
    }

    public void PlayTransitionOut()
    {
        if (transitionOutClip != null)
        {
            _transitionSource.pitch = 1f;
            _transitionSource.PlayOneShot(transitionOutClip, transitionVolume);
        }
    }

    public static TransitionManager EnsureExists()
    {
        var existing = FindAnyObjectByType<TransitionManager>();
        if (existing != null) return existing;

        var go = new GameObject("TransitionManager");
        return go.AddComponent<TransitionManager>();
    }
}