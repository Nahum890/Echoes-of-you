using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static, non-destructive registry of 2.0 audio assets shipped under
/// <c>Assets/Resources/Audio/</c>. Provides lazy <see cref="Resources.Load"/>
/// lookups keyed by logical event names (matching AUDIO_ASSET_REGISTRY.md).
///
/// Priority order at runtime:
///   1. Clip already assigned in Inspector  (unchanged — Inspector wins)
///   2. Clip resolved here from Resources/Audio/...  (2.0 asset on disk)
///   3. Procedural fallback synthesized by existing system  (unchanged)
///
/// This class does NOT modify any existing system's behaviour; it is only
/// queried by surgical additions in <see cref="GameFeelController"/> and
/// <see cref="MenuHoverSystem"/> that request a clip here BEFORE falling
/// back to synthesis. If Unity cannot load the asset (missing file), the
/// procedural fallbacks continue to work exactly as before.
///
/// See:
///   Docs/Audio/AUDIO_ASSET_REGISTRY.md  (canonical manifest, §3)
///   Docs/Specs/AUDIO_DIRECTION.md      (target architecture; this is a
///   transitional bridge, NOT the final AudioEventBus)
/// </summary>
public static class EchoesAudioAssets
{
    // ═══════════════════════════════════════════════════════════════
    // EVENT-NAME CONSTANTS (logical keys used across the codebase)
    // ═══════════════════════════════════════════════════════════════

    public const string SfxJump             = "sfx_jump";
    public const string SfxLandingSoft     = "sfx_landing_soft";
    public const string SfxLandingHard     = "sfx_landing_hard";
    public const string SfxMovementScrape  = "sfx_movement_scrape";
    public const string SfxPlayerDeath     = "sfx_player_death";
    public const string SfxRespawn         = "sfx_respawn";
    public const string SfxPuzzleSuccess   = "sfx_puzzle_success";
    public const string SfxPuzzleFailure   = "sfx_puzzle_failure";
    public const string SfxRecordingWarn   = "sfx_recording_warning";
    public const string SfxButton          = "sfx_button";
    public const string SfxEchoResidualTail= "sfx_echo_residual_tail";
    public const string SfxLevelTransitionIn  = "sfx_level_transition_in";
    public const string SfxLevelTransitionOut = "sfx_level_transition_out";

    public const string MusExplorationDrone   = "mus_exploration_drone";
    public const string MusPuzzleTexture      = "mus_puzzle_texture";
    public const string MusMemoryPiano        = "mus_memory_piano";
    public const string MusDialogueBed        = "mus_dialogue_bed";
    public const string MusEndingTheme        = "mus_ending_theme";
    public const string MusCredits            = "mus_credits";

    public const string AmbMemoryWhisper      = "amb_memory_whisper";
    public const string SfxMemoryDiscovery    = "sfx_memory_discovery";

    public const string UiDialogueAdvance     = "ui_dialogue_advance";
    public const string UiDialogueOpen        = "ui_dialogue_open";
    public const string UiInteractionAvailable= "ui_interaction_available";
    public const string UiInteractionDenied   = "ui_interaction_denied";
    public const string UiPauseClose          = "ui_pause_close";
    public const string UiToast               = "ui_toast";

    // ═══════════════════════════════════════════════════════════════
    // RESOURCE PATHS  (relative to Assets/Resources/, without extension)
    // ═══════════════════════════════════════════════════════════════
    //
    // NOTE: The exploration-drone file shipped with an opaque vendor
    // filename ("01-mus_exploration_dronewavduration-30-secondsseamless-
    // loopno_081626.mp3") and the pause-close file is "ui_pause_closemp3.mp3".
    // These are mapped by canonical logical name below; a future asset
    // rename pass can normalize the on-disk names without touching callers
    // because everyone references clips via the constants above.

    static readonly Dictionary<string, string> _paths = new()
    {
        { SfxJump,                "Audio/player/sfx_jump" },
        { SfxLandingSoft,        "Audio/player/sfx_landing_soft" },
        { SfxLandingHard,        "Audio/player/sfx_landing_hard" },
        { SfxMovementScrape,     "Audio/player/sfx_movement_scrape" },
        { SfxPlayerDeath,        "Audio/player/sfx_player_death" },
        { SfxRespawn,            "Audio/player/sfx_respawn" },
        { SfxPuzzleSuccess,      "Audio/player/sfx_puzzle_success" },
        { SfxPuzzleFailure,      "Audio/player/sfx_puzzle_failure" },
        { SfxRecordingWarn,      "Audio/player/sfx_recording_warning" },
        { SfxButton,             "Audio/player/sfx_button" },
        { SfxEchoResidualTail,   "Audio/player/sfx_echo_residual_tail" },
        { SfxLevelTransitionIn,  "Audio/sfx_level_transition_in" },
        { SfxLevelTransitionOut, "Audio/sfx_level_transition_out" },

        { MusExplorationDrone,   "Audio/01-mus_exploration_dronewavduration-30-secondsseamless-loopno_081626" },
        { MusPuzzleTexture,      "Audio/mus_puzzle_texture" },
        { MusMemoryPiano,        "Audio/mus_memory_piano" },
        { MusDialogueBed,        "Audio/mus_dialogue_bed" },
        { MusEndingTheme,        "Audio/mus_ending_theme" },
        { MusCredits,            "Audio/mus_credits" },

        { AmbMemoryWhisper,      "Audio/narrative/amb_memory_whisper" },
        { SfxMemoryDiscovery,    "Audio/narrative/sfx_memory_discovery" },

        { UiDialogueAdvance,     "Audio/ui/ui_dialogue_advance" },
        { UiDialogueOpen,        "Audio/ui/ui_dialogue_open" },
        { UiInteractionAvailable,"Audio/ui/ui_interaction_available" },
        { UiInteractionDenied,   "Audio/ui/ui_interaction_denied" },
        { UiPauseClose,          "Audio/ui/ui_pause_closemp3" },
        { UiToast,               "Audio/ui/ui_toast" },
    };

    // ═══════════════════════════════════════════════════════════════
    // CACHE
    // ═══════════════════════════════════════════════════════════════

    static readonly Dictionary<string, AudioClip> _cache = new();

    /// <summary>
    /// Attempts to resolve a clip by logical event name. Returns <c>true</c>
    /// and assigns <paramref name="clip"/> when the asset is loaded successfully;
    /// returns <c>false</c> (clip = null) when the event is unknown or the
    /// file is missing on disk. The caller MUST keep its existing procedural
    /// fallback intact — this method resolves only files that actually ship.
    /// </summary>
    public static bool TryGet(string eventName, out AudioClip clip)
    {
        clip = null;
        if (string.IsNullOrEmpty(eventName))
            return false;

        if (_cache.TryGetValue(eventName, out clip))
            return clip != null;

        if (!_paths.TryGetValue(eventName, out var resourcePath))
        {
            _cache[eventName] = null; // negative cache — don't retry path lookup
            return false;
        }

        clip = Resources.Load<AudioClip>(resourcePath);
        _cache[eventName] = clip; // cache hit OR miss (null) so we don't hammer Resources

        if (clip == null)
            Debug.LogWarning($"[EchoesAudioAssets] '{eventName}' not found at Resources/{resourcePath}. Procedural fallback will be used.");

        return clip != null;
    }

    /// <summary>
    /// Convenience: returns the resolved clip or <c>null</c>. Identical
    /// semantics to <see cref="TryGet"/> but without the out-parameter dance.
    /// </summary>
    public static AudioClip Get(string eventName)
    {
        TryGet(eventName, out AudioClip clip);
        return clip;
    }

    /// <summary>
    /// Clears the in-memory cache. Primarily for editor tooling / hot-reload.
    /// </summary>
    public static void ClearCache()
    {
        _cache.Clear();
    }
}
