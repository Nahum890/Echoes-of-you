# AUDIO_DIRECTION.md — Game Audio Direction & Unity Audio System Architecture
## Spec ID: SPEC-AUD-DIR-001
## Version: 1.0
## Date: 2026-08-15
## Audience: Game Audio Director, Unity Audio System Architect, Audio Implementer
## Authority: Level 3 (Executable Specification). Subordinate to `SOURCE_OF_TRUTH.md` `[SPEC-000]` and `DESIGN_PHILOSOPHY.md` `[SPEC-001]`. Supersedes the mixer/runtime portions of `AUDIO_GRAMMAR.md` `[SPEC-112]` and `AUDIO_MIXER_SCHEMA.md` `[SPEC-127]` where they conflict.

---

## 1. PURPOSE

Defines the complete target audio system for *Echoes of You 2.0*: AudioMixer bus hierarchy, snapshots, scripts, event catalog, music state machine, transition strategy, and clip asset manifest. Operationalises the audit findings in `Docs/Audit/AUDIO_AUDIT.md`.

## 2. SCOPE

Applies to:
- `Assets/Resources/EchoesAudioMixer.mixer`
- `Assets/Scripts/Audio/AudioEventBus.cs`
- `Assets/Scripts/Audio/AudioEventRegistry.cs`
- `Assets/Scripts/Audio/MusicStateMachine.cs`
- `Assets/Scripts/Audio/AudioEventListener.cs`
- `Assets/Scripts/EchoesAudioManager.cs` (extended)
- `Assets/Editor/EchoesAudioMixerBuilder.cs` (extended)
- `Assets/Scripts/EchoPlayback.cs` (corrected)
- All gameplay scripts that subscribe to audio events.

Excludes visual shader rendering, narrative script content, and final clip mastering (handled by the audio asset pipeline).

## 3. DESIGN PRINCIPLES

1. **No constant music.** Default state is `SILENCE`. Music enters only when the narrative or mechanical context demands it, and exits again.
2. **Mixer is the single source of truth for volume.** No `AudioSource.volume` direct writes in gameplay code (enforces `RULE-MIX-005`).
3. **Snapshots drive ducking and music states.** `AudioMixer.TransitionToSnapshot` is the only transition API. No per-frame DSP sidechain code.
4. **Bandpass DSP lives on the mixer bus**, not on individual AudioSources (`SFX_Echo` and `TapeHiss` only).
5. **Piano exemption.** `CONS-MIX-001` (no orchestral/upbeat) is maintained for `EXPLORATION/TENSION/PUZZLE/DIALOGUE` (drones only). A controlled piano-solo exemption applies to `MEMORY` and `ENDING` only — drones still permitted, never ensembles.
6. **Decoupled event firing.** Gameplay code calls `AudioEventBus.Play(EventId, position?)`. The bus resolves the registry entry, applies cooldown / randomization / spatialization, and reuses a pooled `AudioSource`.
7. **Asset paths are absolute under `Assets/Audio/...`.** Missing clips log a warning and skip playback (graceful degradation) rather than throwing.

## 4. AUDIO MIXER — TARGET

### 4.1 Bus hierarchy

```
Master [exp: MasterVolume]            default 0.0 dB
├─ Music     [exp: MusicVolume]       default -6.0 dB
│  └─ Music_Stems                     (no exposed param — DSP child for bandpass toggle)
├─ Ambience  [exp: AmbienceVolume]   default -12.0 dB
├─ SFX       [exp: SFXVolume]         default 0.0 dB
│  ├─ SFX_Player  [exp: SFXPlayerVolume]   default 0.0 dB
│  ├─ SFX_Puzzle  [exp: SFXPuzzleVolume]   default 0.0 dB
│  ├─ SFX_Echo    [exp: SFXEchoVolume]     default -3.0 dB
│  │     DSP: AudioHighPassFilter 300 Hz + AudioLowPassFilter 3500 Hz
│  ├─ SFX_UI      [exp: SFXUIVolume]       default -3.0 dB
│  └─ SFX_Foley   [exp: SFXFoleyVolume]    default -3.0 dB
├─ Voice     [exp: VoiceVolume]       default -3.0 dB
└─ TapeHiss  [exp: TapeHissVolume]   default -18.0 dB  always_on
      DSP: AudioHighPassFilter 300 Hz + AudioLowPassFilter 3500 Hz
```

### 4.2 Exposed parameters (10)

`MasterVolume`, `MusicVolume`, `AmbienceVolume`, `SFXVolume`, `SFXPlayerVolume`, `SFXPuzzleVolume`, `SFXEchoVolume`, `SFXUIVolume`, `SFXFoleyVolume`, `VoiceVolume`, `TapeHissVolume`.

`EchoesAudioManager` extends with setters/getters for each, retaining log10→dB conversion and PlayerPrefs persistence (`Audio.*` keys).

### 4.3 Music snapshots (7 — emotional pacing)

| Snapshot | Music dB | Ambience dB | Notes |
|---|---|---|---|
| `SND_SILENCE` | -80 | -80 | Default. No music, ambient near-silent. |
| `SND_EXPLORATION` | -9 | -18 | Subtle drone after 8 s idle. |
| `SND_TENSION` | -7 | -15 | Threat building. |
| `SND_PUZZLE` | -14 | -22 | Music ducked, player focus on mechanic. |
| `SND_MEMORY` | -6 | -30 | Piano-solo stem elevated. |
| `SND_DIALOGUE` | -18 | -24 | Voice-forward, music cut (0.05 s). |
| `SND_ENDING` | -3 | -40 | Piano + strings (controlled exemption), closing. |

### 4.4 Overlay snapshots (2 — transient ducking, blended via `TransitionToSnapshot` with short times)

| Snapshot | Effect | Fade in | Fade out |
|---|---|---|---|
| `SND_VOICE_ACTIVE` | Music -6 dB duck, Ambience -6 dB duck | 0.05 s | 0.30 s |
| `SND_UI_TRANSIENT` | SFX_Echo -6 dB duck for hover clarity | 0.02 s | 0.18 s |

### 4.5 DSP — bandpass filters

Only two buses carry the cassette bandpass (`RULE-AUD-001`):
- `SFX_Echo` → `AudioHighPassFilter.cutoffFrequency = 300` + `AudioLowPassFilter.cutoffFrequency = 3500`.
- `TapeHiss` → identical.

Per-source `AudioLowPassFilter` / `AudioHighPassFilter` in `EchoPlayback.cs` are removed. Analog vs Standard echo discrimination is handled by routing the Echo AudioSource to a sub-bus variant via `SetFloat` on a parameter, or by toggling the bus DSP effect at runtime (configurable in the registry).

## 5. SCRIPTS

### 5.1 `EchoesAudioManager.cs` (extended)

Existing singleton preserved. Additions:
- `Dictionary<string, AudioMixerSnapshot> _snapshots` — caches all 9 snapshots by name.
- `TransitionToSnapshot(string name, float seconds)` — wraps `AudioMixer.TransitionToSnapshot`.
- `AudioMixerGroup GetGroup(AudioBus bus)` — cache for enum-keyed lookups.
- Setters/getters for the 6 new exposed params (Ambience, Player, Puzzle, UI, Foley, Voice, TapeHiss).
- `void SetGroupEnabled(string groupName, bool enabled)` — utility to mute/unmute a bus cleanly via its exposed param.

### 5.2 `AudioEventBus.cs` (new — `Assets/Scripts/Audio/`)

Static façade. Responsibilities:
- `Play(AudioEventId id, Vector3? position = null)` — looks up `AudioEventRegistry`, enforces cooldown (per-event `_lastPlayed[id]`), samples random pitch/volume within the registry range, routes to the right pooled source.
- Source pool: 16 reusable `AudioSource` GameObjects under a single `__AudioPool__` root, pre-warmed at scene load. 3D sources reuse from the pool; 2D sources share a single source per bus.
- `PlayOneShot3D(AudioClip clip, Vector3 pos, AudioBus bus, float vol, float pitch)` — fallback for ad-hoc non-registry events.
- Cooldown entries decayed in `Update` (only if `Instance` alive).

### 5.3 `AudioEventRegistry.cs` (new — ScriptableObject)

Asset: `Assets/Resources/AudioEventRegistry.asset`.

```csharp
public enum AudioBus { Master, Music, Ambience, SFX_Player, SFX_Puzzle, SFX_Echo, SFX_UI, SFX_Foley, Voice, TapeHiss }

[CreateAssetMenu(menuName = "Echoes/Audio Event Registry")]
public class AudioEventRegistry : ScriptableObject
{
    public List<EventEntry> events;
    [System.Serializable] public class EventEntry {
        public AudioEventId id;
        public AudioClip clip;          // nullable — null => bus logs warning, skips
        public AudioClip[] clipVariants;// optional, overrides clip if non-empty
        public AudioBus bus;
        public float volumeMin = 1.0f;
        public float volumeMax = 1.0f;
        public float pitchMin = 1.0f;
        public float pitchMax = 1.0f;
        [Range(0,256)] public int priority = 128;
        public bool spatial = true;
        public bool loop = false;
        public float cooldown = 0f;
        public float maxDistance = 18f;
        public float minDistance = 1f;
        public AudioRolloffMode rolloff = AudioRolloffMode.Logarithmic;
        public float doppler = 0f;
        public float spread = 0f;
    }
}
```

The registry is loaded by `AudioEventBus` on first access and cached.

### 5.4 `MusicStateMachine.cs` (new — `Assets/Scripts/Audio/`)

```csharp
public enum MusicState { Silence, Exploration, Tension, Puzzle, Memory, Dialogue, Ending }

public class MusicStateMachine : MonoBehaviour
{
    public static MusicState Current { get; private set; } = MusicState.Silence;
    static float _idleTimer;

    public static void TransitionTo(MusicState next, float? fadeOverride = null);
    public static void NotifyPlayerAction();     // resets _idleTimer; if was Silence and > threshold already, schedule Exploration
    public static void NotifyPuzzleEnter();      // -> Puzzle
    public static void NotifyPuzzleExit();       // -> Tension or Exploration
    public static void NotifyMemoryEnter();      // -> Memory
    public static void NotifyDialogueOpen();     // -> Dialogue
    public static void NotifyEnding();           // -> Ending
}
```

#### 5.4.1 Snapshot transition times (seconds)

| From \ To | Silent  | Exploration | Tension | Puzzle | Memory | Dialogue | Ending |
|-----------|---------|-------------|---------|--------|--------|----------|--------|
| Silent     | 0       | 2.0         | 1.5     | 1.5    | 3.0    | 0.05     | 4.0    |
| Exploration| 2.5     | 0           | 1.2     | 1.0    | 3.0    | 0.05     | 4.0    |
| Tension    | 2.5     | 2.0         | 0       | 0.8    | 3.0    | 0.05     | 4.0    |
| Puzzle     | 3.0     | 2.0         | 1.2     | 0      | 3.0    | 0.05     | 4.0    |
| Memory     | 4.0     | 4.0         | 4.0     | 4.0    | 0      | 0.05     | 4.0    |
| Dialogue   | 2.0     | 2.0         | 2.0     | 2.0    | 2.0    | 0        | 4.0    |
| Ending     | 5.0     | 5.0         | 5.0     | 5.0    | 5.0    | 5.0      | 0      |

#### 5.4.2 "No constant music" enforcement
- The default state after scene load is `Silence`.
- `Exploration` only enters when `_idleTimer > 8.0 s` AND no puzzle/dialogue/memory is queued.
- Any non-idle player action (interaction, puzzle activation, dialogue) overrides to the relevant state and resets `_idleTimer`.
- `Dialogue` is a hard cut (0.05 s) — narrative clarity.

### 5.5 `AudioEventListener.cs` (new — `Assets/Scripts/Audio/`)

Subscribes to existing gameplay hooks and translates them to `AudioEventBus.Play` + `MusicStateMachine` calls. No gameplay logic of its own.

| Game-side hook | Bus call | Music call |
|---|---|---|
| `PlayerController.OnJumped` | `Play(PlayerJump, transform.position)` | `NotifyPlayerAction()` |
| `PlayerController.OnLanded` (hard/soft) | `Play(PlayerLanding…)` | `NotifyPlayerAction()` |
| `PlayerController_Animation` footstep anim event | `Play(Footstep, pos)` | — |
| `EchoRecorder.StartRecording` | `Play(RecordStart)` | — |
| `EchoRecorder.StopRecordingAndSpawn` | `Play(RecordStop)` + `Play(EchoSpawn, pos)` | — |
| Buffer ≥ 80% full | `Play(RecordingWarning)` | — |
| `EchoPlayback.BeginPlayback` | `Play(EchoPlayback, pos)` | — |
| `EchoPlayback.FadeOutAndDestroy` | `Play(EchoDespawn, pos)` | — |
| `PressurePlate` OnActivate | `Play(PlateActivate, pos)` | — |
| `PuzzleSignal` solved | `Play(PuzzleSuccess, pos)` | `NotifyPuzzleExit()` |
| `PuzzleSignal` failed | `Play(PuzzleFailure, pos)` | — |
| `PuzzleIntent` enter zone | — | `NotifyPuzzleEnter()` |
| `InteractionPromptController` shown | `Play(InteractionAvailable)` | — |
| `InteractionPromptController` confirmed | `Play(InteractionConfirm)` | — |
| Interaction invalid | `Play(InteractionDenied)` | — |
| `VN_DialogueController` on open | `Play(DialogueOpen)` | `NotifyDialogueOpen()` |
| `VN_DialogueController` on advance | `Play(DialogueAdvance)` | — |
| `VN_ChoiceGateController` hover | `Play(ChoiceHover)` | — |
| `VN_ChoiceGateController` confirm | `Play(ChoiceConfirm)` | — |
| `PauseMenu.Pause` | `Play(PauseOpen)` | `TransitionTo(Silence, 0.3)` |
| `PauseMenu.Resume` | `Play(PauseClose)` | restore previous music state |
| `SceneTransitionManager` begin | `Play(LevelTransitionOut)` | `TransitionTo(Silence, 2.0)` |
| `SceneTransitionManager` end | `Play(LevelTransitionIn)` | `TransitionTo(Exploration, 2.0)` (if level context) |
| `MemorySystem` discovery | `Play(MemoryDiscovery, pos)` | `NotifyMemoryEnter()` |

### 5.6 `EchoesAudioMixerBuilder.cs` (extended)

The builder's Reflection path is preserved. New responsibilities:
- On regenerate, create the full 9-group tree (Master → Music, Ambience, SFX (→ Player, Puzzle, Echo, UI, Foley), Voice, TapeHiss).
- Add `AudioHighPassFilter` + `AudioLowPassFilter` effects to `SFX_Echo` and `TapeHiss` groups (via Reflection against `AudioMixerEffectController`).
- Create the 7 music + 2 overlay snapshots and set their per-group volume overrides as defined in §4.3 / §4.4.
- Expose all 11 parameters via the existing `ExposeVolumeParameter` helper, extended to the new names.
- Idempotent: if the existing mixer already has the full set of groups and snapshots (by name), do not regenerate; only patch missing pieces.

### 5.7 `EchoPlayback.cs` (corrected)

`ConfigureSpatialVoicePlayback` is rewritten:
```csharp
_audioSource.spatialBlend = 1f;
_audioSource.dopplerLevel = 0f;        // was 0.05f
_audioSource.spread = 0f;               // was 18f
_audioSource.minDistance = 1f;          // was 4f
_audioSource.maxDistance = 18f;         // was 42f
_audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
_audioSource.bypassReverbZones = true;
```
`ApplyAnalogAudioFilters` and `RemoveVoiceDegradingFilters` are removed — DSP now lives on the `SFX_Echo` bus. Standard vs Analog echo is selected by routing to either `SFX_Echo` (Analog) or `SFX_Echo_PostBP` (Standard) — if a single-bus variant is desired, the `SFX_Echo` DSP is toggled via a `SetFloat` on a custom exposed param `EchoBandpassEnabled`.

`FadeOutAndDestroyRoutine` no longer writes `_audioSource.volume` directly. It transitions the source's mixer group to `SND_ECHO_FADE` snapshot (or routes to a fade sub-bus), restoring `RULE-MIX-005` compliance.

## 6. EVENT CATALOG (66 events)

All events follow the schema: `EVENT_ID | TRIGGER | SOUND TYPE | PRIORITY | VOLUME RANGE (dBFS) | PITCH RANGE | SPATIAL | COOLDOWN | RANDOMIZATION | MIXER GROUP`.

Priority convention: 0 = highest (voice), 256 = lowest (ambient tail).

### 6.1 MUSIC (8 events) — driven by MusicStateMachine, not AudioEventBus

| EVENT_ID | TRIGGER | SOUND TYPE | PRIORITY | VOLUME (dBFS) | PITCH | SPATIAL | COOLDOWN | RANDOMIZATION | MIXER GROUP |
|---|---|---|---|---|---|---|---|---|---|
| `MUS_EXPLORATION` | state Exploration | drone loop | 200 | -9 (-3 within bus) | 1.0 | 2D | — | none | Music |
| `MUS_TENSION` | state Tension | drone loop | 200 | -7 | 1.0 | 2D | — | none | Music |
| `MUS_PUZZLE` | state Puzzle | texture loop | 200 | -14 | 1.0 | 2D | — | none | Music |
| `MUS_MEMORY` | state Memory | piano solo loop | 100 | -6 | 1.0 | 2D | — | none | Music |
| `MUS_DIALOGUE` | state Dialogue | pad bed loop | 200 | -18 | 1.0 | 2D | — | none | Music |
| `MUS_ENDING` | state Ending | piano+strings theme | 50 | -3 | 1.0 | 2D | — | none | Music |
| `MUS_MENU` | MainMenu scene load | theme loop | 200 | -6 | 1.0 | 2D | — | none | Music |
| `MUS_CREDITS` | Credits scene | ending variation | 100 | -4 | 1.0 | 2D | — | none | Music |

`MUS_SILENCE` is not an event — it is the default fade-to-zero Music state.

### 6.2 AMBIENCE (8 events)

| EVENT_ID | TRIGGER | SOUND TYPE | PRIORITY | VOLUME (dBFS) | PITCH | SPATIAL | COOLDOWN | RANDOMIZATION | MIXER GROUP |
|---|---|---|---|---|---|---|---|---|---|
| `AMB_TAPE_HISS` | mixer init | tape hiss loop | 220 | -18 | 1.0 | 2D | — | none | TapeHiss |
| `AMB_ROOM_TONE` | scene load | room tone loop | 230 | -12 | 1.0 | 2D | — | none | Ambience |
| `AMB_ROOM_HUM` | scene load (alt) | room tone w/hum | 230 | -14 | 1.0 | 2D | — | none | Ambience |
| `AMB_HALLWAY` | hallway zone | hallway loop | 220 | -16 | 1.0 | 2D | — | none | Ambience |
| `AMB_VENTILATION` | duct zone | vent hum 3D | 220 | -10 | 1.0 | 3D (1m/18m log) | — | none | Ambience |
| `AMB_INDUSTRIAL` | basement | drone 3D | 220 | -10 | 1.0 | 3D (1m/18m log) | — | none | Ambience |
| `AMB_DISTANT_CLANG` | random punctual | clang one-shot | 180 | -8 | 0.95–1.05 | 3D (1m/25m log) | 8 s | pitch + clip variant | Ambience |
| `AMB_METAL_CONCRETE` | foley scrape | scrape one-shot | 180 | -10 | 1.0 | 3D (1m/18m log) | 0.5 s | none | SFX_Foley |

### 6.3 PLAYER (8 events)

| EVENT_ID | TRIGGER | SOUND TYPE | PRIORITY | VOLUME (dBFS) | PITCH | SPATIAL | COOLDOWN | RANDOMIZATION | MIXER GROUP |
|---|---|---|---|---|---|---|---|---|---|
| `SFX_FOOTSTEP` | anim event | footstep one-shot | 100 | -6 ±1 | 0.92–1.08 | 3D (1m/12m log) | 0.35 s | pitch + 1 of 4 variants | SFX_Player |
| `SFX_JUMP` | `OnJumped` | jump impulse | 90 | -5 | 1.0 | 3D (1m/15m log) | 0.10 s | none | SFX_Player |
| `SFX_LANDING_SOFT` | `OnLanded` soft | body landing | 90 | -8 | 1.0 | 3D (1m/15m log) | 0.10 s | none | SFX_Player |
| `SFX_LANDING_HARD` | `OnLanded` hard | impact | 80 | -4 | 0.95 | 3D (1m/18m log) | 0.10 s | none | SFX_Player |
| `SFX_MOVEMENT_SCRAPE` | slide anim | scrape | 140 | -10 | 1.0 | 3D (1m/10m log) | 0.20 s | none | SFX_Foley |
| `SFX_GRAVITY_SHIFT` | gravity change | whoosh | 100 | -5 | 1.0 | 3D (1m/18m log) | 0.40 s | none | SFX_Player |
| `SFX_PLAYER_DEATH` | fatal puzzle fail | death stinger | 70 | -3 | 0.85 | 2D | — | none | SFX_Player |
| `SFX_RESPAWN` | respawn | restart breath | 90 | -7 | 1.0 | 2D | — | none | SFX_Player |

### 6.4 ECHO (7 events)

| EVENT_ID | TRIGGER | SOUND TYPE | PRIORITY | VOLUME (dBFS) | PITCH | SPATIAL | COOLDOWN | RANDOMIZATION | MIXER GROUP |
|---|---|---|---|---|---|---|---|---|---|
| `SFX_ECHO_RECORD_START` | `StartRecording` | tape rewind | 80 | -3 | 1.25 | 2D | 0.20 s | none | SFX_Echo |
| `SFX_ECHO_RECORD_STOP` | `StopRecordingAndSpawn` | tape stop | 80 | -4 | 0.80 | 2D | 0.20 s | none | SFX_Echo |
| `SFX_RECORDING_WARNING` | buffer ≥ 80% | warning beep | 60 | -5 | 0.90–1.10 | 2D | 0.40 s | pitch | SFX_Echo |
| `SFX_ECHO_SPAWN` | after `StopRecordingAndSpawn` | materialize | 70 | -5 | 1.0 | 3D (1m/25m log, blend 0.7) | 0.50 s | none | SFX_Echo |
| `SFX_ECHO_PLAYBACK_LOOP` | `BeginPlayback` | ghost hum loop | 90 | -9 | 1.0 | 3D (1m/18m log) | — | none | SFX_Echo |
| `SFX_ECHO_VOICE` | `EndLatency` voice play | voice loop | 40 | -3 | 1.0 | 3D (1m/18m log) | — | none | Voice |
| `SFX_ECHO_DESPAWN` | `FadeOutAndDestroy` | siss fade-out | 90 | -8 → -80 (snapshot fade) | 0.95 | 3D (1m/18m log) | — | none | SFX_Echo |

### 6.5 PUZZLE (7 events)

| EVENT_ID | TRIGGER | SOUND TYPE | PRIORITY | VOLUME (dBFS) | PITCH | SPATIAL | COOLDOWN | RANDOMIZATION | MIXER GROUP |
|---|---|---|---|---|---|---|---|---|---|
| `SFX_PLATE_ACTIVATE` | `PressurePlate.OnTriggerEnter` | click | 100 | -6 | 0.95 | 3D (1m/18m log) | 0.10 s | none | SFX_Puzzle |
| `SFX_DOOR_OPEN` | `DoorController.OpenDoor` | door slide | 110 | -10 | 0.90 | 3D (0.5m/20m log) | 0.50 s | none | SFX_Puzzle |
| `SFX_PUZZLE_SUCCESS` | `PuzzleSignal` solved | resolve stinger | 70 | -3 | 1.0 | 2D | — | none | SFX_Puzzle |
| `SFX_PUZZLE_FAILURE` | `PuzzleSignal` failed | discharge | 80 | -5 | 0.92 | 2D | — | none | SFX_Puzzle |
| `SFX_TELEPHONE` | N15 trigger | bell ring | 90 | -5 | 1.0 | 3D (1m/18m log) | — | none | SFX_Puzzle |
| `SFX_CLOCK_CHIME` | clock trigger | bell chime | 120 | -8 | 1.0 | 3D (1m/15m log) | — | none | SFX_Puzzle |
| `SFX_BUTTON_GENERIC` | lever/switch | heavy click | 120 | -7 | 0.98 | 3D (1m/18m log) | 0.15 s | none | SFX_Puzzle |

### 6.6 UI (16 events)

| EVENT_ID | TRIGGER | SOUND TYPE | PRIORITY | VOLUME (dBFS) | PITCH | SPATIAL | COOLDOWN | RANDOMIZATION | MIXER GROUP |
|---|---|---|---|---|---|---|---|---|---|
| `UI_MENU_HOVER_IN` | menu hover in | click | 64 | -8 | 1.0 | 2D | 0.05 s | none | SFX_UI |
| `UI_MENU_HOVER_OUT` | menu hover out | click | 64 | -8 | 0.85 | 2D | 0.05 s | none | SFX_UI |
| `UI_MENU_CONFIRM` | menu confirm | click | 60 | -6 | 1.10 | 2D | 0.05 s | none | SFX_UI |
| `UI_NAV_MOVE` | nav key | tick | 80 | -10 | 0.95–1.05 | 2D | 0.05 s | pitch | SFX_UI |
| `UI_CRT_HUM` | menu scene load | CRT hum loop | 220 | -16 | 1.0 | 2D | — | none | SFX_UI |
| `UI_PAUSE_OPEN` | `PauseMenu.Pause` | open leather | 70 | -6 | 1.0 | 2D | — | none | SFX_UI |
| `UI_PAUSE_CLOSE` | `PauseMenu.Resume` | close inverse | 70 | -6 | 0.90 | 2D | — | none | SFX_UI |
| `UI_CHOICE_HOVER` | VN choice hover | soft click | 70 | -10 | 1.0 | 2D | 0.05 s | none | SFX_UI |
| `UI_CHOICE_CONFIRM` | VN choice confirm | click | 60 | -7 | 1.10 | 2D | 0.10 s | none | SFX_UI |
| `UI_INTERACTION_AVAILABLE` | prompt shown | chime | 100 | -8 | 1.0 | 2D | 0.5 s | none | SFX_UI |
| `UI_INTERACTION_CONFIRM` | prompt confirmed | click | 80 | -6 | 1.10 | 2D | 0.10 s | none | SFX_UI |
| `UI_INTERACTION_DENIED` | invalid interaction | negation | 80 | -7 | 0.85 | 2D | 0.10 s | none | SFX_UI |
| `UI_DIALOGUE_OPEN` | `VN_DialogueController` open | book/cassette open | 80 | -6 | 1.0 | 2D | — | none | SFX_UI |
| `UI_DIALOGUE_ADVANCE` | text advance | typewriter blip | 150 | -12 | 0.97–1.03 | 2D | 0.04 s | pitch | SFX_UI |
| `UI_MODAL_CONFIRM` | modal confirm | click | 70 | -7 | 1.10 | 2D | — | none | SFX_UI |
| `UI_TOAST` | toast notification | bell | 120 | -8 | 1.0 | 2D | 0.50 s | none | SFX_UI |

### 6.7 NARRATIVE (8 events)

| EVENT_ID | TRIGGER | SOUND TYPE | PRIORITY | VOLUME (dBFS) | PITCH | SPATIAL | COOLDOWN | RANDOMIZATION | MIXER GROUP |
|---|---|---|---|---|---|---|---|---|---|
| `VOICE_LYRA_001` | VN line | voice | 0 | -3 | 1.0 | 2D | — | none | Voice |
| `VOICE_LYRA_002` | VN line | voice | 0 | -3 | 1.0 | 2D | — | none | Voice |
| `VOICE_AIDEN_001` | VN line | voice | 0 | -3 | 1.0 | 2D | — | none | Voice |
| `VOICE_AIDEN_002` | VN line | voice | 0 | -3 | 1.0 | 2D | — | none | Voice |
| `SFX_MEMORY_DISCOVERY` | `MemorySystem` discovery | crystal chime | 50 | -3 | 1.0 | 2D | — | none | SFX_Puzzle |
| `AMB_MEMORY_WHISPER` | memory zone enter | whisper bed loop | 120 | -14 | 0.95–1.05 | 3D (1m/10m log) | — | pitch | Ambience |
| `UI_DIALOGUE_OPEN` (narrative reuse) | — | — | — | — | — | — | — | — | — |
| `UI_DIALOGUE_ADVANCE` (narrative reuse) | — | — | — | — | — | — | — | — | — |

### 6.8 ENDING / TRANSITION (4 events)

| EVENT_ID | TRIGGER | SOUND TYPE | PRIORITY | VOLUME (dBFS) | PITCH | SPATIAL | COOLDOWN | RANDOMIZATION | MIXER GROUP |
|---|---|---|---|---|---|---|---|---|---|
| `SFX_LEVEL_TRANSITION_OUT` | `SceneTransitionManager` begin | whoosh + tape filter | 60 | -5 | 1.0 | 2D | — | none | SFX_Foley |
| `SFX_LEVEL_TRANSITION_IN` | `SceneTransitionManager` end | inverse load | 80 | -8 | 1.0 | 2D | — | none | SFX_Foley |
| `MUS_ENDING` (reuse) | ending card reveal | piano+strings | 50 | -3 | 1.0 | 2D | — | none | Music |
| `MUS_CREDITS` (reuse) | credits roll | ending variation | 100 | -4 | 1.0 | 2D | — | none | Music |

## 7. TRANSITION STRATEGY — music in detail

### 7.1 Stem layering
Each music state activates up to 3 stems (low drone, texture, piano/strings) over the single `Music` bus. Stems are independent `AudioSource`s faded in/out by the `MusicStateMachine` via `AudioSource.volume` lerps ONLY during stem transitions (this is the documented exception to `RULE-MIX-005`; the master volume still travels on the mixer). Each stem's target volume is set by the snapshot on `Music` itself; the stem AudioSource volumes act as trim within the bus.

Stem-to-state binding:
| State | Stems |
|---|---|
| SILENCE | none |
| EXPLORATION | drone (0.7) |
| TENSION | drone (0.9) + dissonant texture (0.5) |
| PUZZLE | metronomic texture (0.6) |
| MEMORY | piano solo (1.0) |
| DIALOGUE | pad bed (0.3) |
| ENDING | piano (1.0) + strings (0.7) |
| MENU/CREDITS | stems per clip definition |

### 7.2 State-machine snapshot API
```csharp
MusicStateMachine.TransitionTo(MusicState.Puzzle, fadeOverride: 0.8f);
```
Internally:
1. Resolves target snapshot name (e.g. `SND_PUZZLE`).
2. Looks up transition time from the §5.4.1 matrix (or uses `fadeOverride`).
3. Calls `EchoesAudioManager.Instance.TransitionToSnapshot(name, time)`.
4. Cross-fades stem AudioSources in parallel via coroutine.
5. Updates `Current`.

### 7.3 No-music enforcement rules (programmatic)
- After any scene load, `MusicStateMachine.Current = Silence`. No music plays until a state is explicitly pushed.
- `NotifyPlayerAction()` starts an idle timer. Music only enters `Exploration` after `idleThreshold = 8 s` of no other state being active.
- Puzzle/dialogue/memory enterings interrupt the idle timer and clear any pending `Exploration` transition.
- Level transitions force `TransitionTo(Silence, 2.0)` on begin and `TransitionTo(Exploration, 2.0)` on end (only if no puzzle is queued at level start).

## 8. ASSET MANIFEST

### 8.1 Critical clips required before integration can be feature-complete
| ID | Path (canonical) | Source / status |
|---|---|---|
| `mus_exploration_drone.wav` | `Assets/Audio/Music/Mus_Exploration_Drone.wav` | ❌ |
| `mus_tension_drone.wav` | `Assets/Audio/Music/Mus_Tension_Drone.wav` | ❌ |
| `mus_puzzle_texture.wav` | `Assets/Audio/Music/Mus_Puzzle_Texture.wav` | ❌ |
| `mus_memory_piano.wav` | `Assets/Audio/Music/Mus_Memory_Piano.wav` | ❌ |
| `mus_dialogue_bed.wav` | `Assets/Audio/Music/Mus_Dialogue_Bed.wav` | ❌ |
| `mus_ending_theme.wav` | `Assets/Audio/Music/Mus_Ending_Theme.wav` | ❌ |
| `mus_menu_theme.wav` | `Assets/Audio/Music/Mus_Menu_Theme.wav` | ♻️ reuse `Efectos de sonido/menu/maint_thememenump3.mp3` |
| `mus_credits.wav` | `Assets/Audio/Music/Mus_Credits.wav` | ❌ |
| `amb_tape_hiss.wav` | `Assets/Audio/Ambience/Amb_Tape_Hiss.wav` | ♻️ reuse `476025__...worn-video-cassette-audio-hiss-noise.wav` |
| `amb_room_tone.wav` | `Assets/Audio/Ambience/Amb_Room_Tone.wav` | ♻️ reuse `Resources/Audio/VN/ambient/room_tone.wav` |
| `amb_room_tone_hum.wav` | `Assets/Audio/Ambience/Amb_Room_Tone_Hum.wav` | ♻️ reuse `144046__gchase__room_tone...low_hum.wav` |
| `amb_hallway.wav` | `Assets/Audio/Ambience/Amb_Hallway.wav` | ♻️ reuse `274213__bexhillcollege__college-hallway-ambience.wav` |
| `amb_ventilation.wav` | `Assets/Audio/Ambience/Amb_Ventilation.wav` | ♻️ reuse `Ventilation.wav` |
| `amb_industrial_hum.wav` | `Assets/Audio/Ambience/Amb_Industrial_Hum.wav` | ♻️ reuse `Industrial-hum.wav` |
| `amb_distant_clangs.wav` | `Assets/Audio/Ambience/Amb_Distant_Clangs.wav` | ♻️ reuse `507465__danjocross__four-quiet-distant-clangs.aiff` |
| `amb_metal_concrete.wav` | `Assets/Audio/Ambience/Amb_Metal_Concrete.wav` | ♻️ reuse `166118__deleted_user_2104797__metal-concrete.wav` |
| `sfx_footstep_01..04.wav` | `Assets/Audio/SFX/Player/Footstep_0X.wav` | ♻️ split `setps.mp3` into 4 variations |
| `sfx_jump.wav` | `Assets/Audio/SFX/Player/Jump.wav` | ❌ |
| `sfx_landing_soft.wav` | `Assets/Audio/SFX/Player/Landing_Soft.wav` | ❌ |
| `sfx_landing_hard.wav` | `Assets/Audio/SFX/Player/Landing_Hard.wav` | ❌ |
| `sfx_movement_scrape.wav` | `Assets/Audio/SFX/Player/Movement_Scrape.wav` | ❌ |
| `sfx_gravity_shift.wav` | `Assets/Audio/SFX/Player/Gravity_Shift.wav` | ❌ |
| `sfx_player_death.wav` | `Assets/Audio/SFX/Player/Death.wav` | ❌ |
| `sfx_respawn.wav` | `Assets/Audio/SFX/Player/Respawn.wav` | ❌ |
| `sfx_echo_record_start.wav` | `Assets/Audio/SFX/Echo/Record_Start.wav` | ♻️ reuse `GRABACIÓN INICIO.mp3` |
| `sfx_echo_record_stop.wav` | `Assets/Audio/SFX/Echo/Record_Stop.wav` | ♻️ reuse `reset.mp3` |
| `sfx_recording_warning.wav` | `Assets/Audio/SFX/Echo/Recording_Warning.wav` | ❌ |
| `sfx_echo_spawn.wav` | `Assets/Audio/SFX/Echo/Echo_Spawn.wav` | ♻️ reuse `CREACIÓN DE ECO.mp3` |
| `sfx_echo_playback_loop.wav` | `Assets/Audio/SFX/Echo/Echo_Playback_Loop.wav` | ♻️ reuse `LOOP DE ECO.mp3` |
| `sfx_echo_despawn.wav` | `Assets/Audio/SFX/Echo/Echo_Despawn.wav` | ❌ |
| `sfx_plate_click.wav` | `Assets/Audio/SFX/Puzzle/Plate_Click.wav` | ♻️ reuse `CLICK.mp3` |
| `sfx_door_open.wav` | `Assets/Audio/SFX/Puzzle/Door_Open.wav` | ♻️ reuse `PUERTA.mp3` |
| `sfx_puzzle_success.wav` | `Assets/Audio/SFX/Puzzle/Puzzle_Success.wav` | ❌ |
| `sfx_puzzle_failure.wav` | `Assets/Audio/SFX/Puzzle/Puzzle_Failure.wav` | ❌ |
| `sfx_telephone.wav` | `Assets/Audio/SFX/Puzzle/Telephone.wav` | ♻️ reuse `164843__...old-fashioned-school-telephone-bell-ring.wav` |
| `sfx_clock_chime.wav` | `Assets/Audio/SFX/Puzzle/Clock_Chime.wav` | ♻️ reuse `freesound_community-clock-chime-88027.mp3` |
| `sfx_button.wav` | `Assets/Audio/SFX/Puzzle/Button.wav` | ❌ |
| `ui_hover_in.wav` etc. | `Assets/Audio/SFX/UI/...` | ♻️ reuse `menu/...` clips |
| `ui_pause_open.wav` | `Assets/Audio/SFX/UI/Pause_Open.wav` | ❌ |
| `ui_pause_close.wav` | `Assets/Audio/SFX/UI/Pause_Close.wav` | ❌ |
| `ui_interaction_available.wav` | `Assets/Audio/SFX/UI/Interaction_Available.wav` | ❌ |
| `ui_interaction_denied.wav` | `Assets/Audio/SFX/UI/Interaction_Denied.wav` | ❌ |
| `ui_dialogue_open.wav` | `Assets/Audio/SFX/UI/Dialogue_Open.wav` | ❌ |
| `ui_dialogue_advance.wav` | `Assets/Audio/SFX/UI/Dialogue_Advance.wav` | ❌ |
| `ui_toast.wav` | `Assets/Audio/SFX/UI/Toast.wav` | ❌ |
| `voice_lyra/aiden_001..02.wav` | `Assets/Audio/Voice/Lyra_001.wav` etc. | ♻️ reuse existing 4 clips |
| `sfx_memory_discovery.wav` | `Assets/Audio/SFX/Narrative/Memory_Discovery.wav` | ❌ |
| `amb_memory_whisper.wav` | `Assets/Audio/Ambience/Amb_Memory_Whisper.wav` | ❌ |
| `sfx_level_transition_out.wav` | `Assets/Audio/SFX/Transition/Level_Out.wav` | ❌ |
| `sfx_level_transition_in.wav` | `Assets/Audio/SFX/Transition/Level_In.wav` | ❌ |

### 8.2 Graceful degradation
- `AudioEventBus.Play` looks up the `EventEntry` by `AudioEventId`; if `clip == null` and `clipVariants` is empty/null, the bus logs `"[Audio] {id} missing clip — skipping"` (warning) and returns without error.
- The system is therefore fully installable today against the existing ~26 clips. Missing clips become audible the moment they are dropped into the canonical path.

### 8.3 Asset reorganisation pass (non-breaking)
Step 1: copy (not move) all `Efectos de sonido/*` clips into the canonical `Assets/Audio/{Category}/` tree with normalized ASCII names.
Step 2: regenerate the `AudioEventRegistry` asset references to point at the canonical paths.
Step 3: keep `Efectos de sonido/` as a legacy alias folder during migration; remove after build verification.

## 9. VALIDATION

- `EchoesAudioMixerBuilder.EnsureAudioMixer()` asserts the 9 groups + 9 snapshots + 11 exposed params after rebuild.
- `LevelValidator.cs` is extended with `VAL-AUD-DIR-001`: every gameplay scene has an `EchoesAudioManager` and an `AudioEventListener`.
- `VAL-AUD-DIR-002`: every 3D `AudioSource` in scene satisfies `minDistance >= 0.5 && maxDistance <= 25 && dopplerLevel == 0 && spread == 0`. (EchoPlayback rolloff corrected.)
- `VAL-AUD-DIR-003`: no gameplay code outside `MusicStateMachine` writes `AudioSource.volume` directly (Roslyn symbol verifier check).

## 10. RULE STATUS AFTER IMPLEMENTATION

| Rule | After impl |
|---|---|
| RULE-AUD-001 (Master bandpass) | satisfied — bandpass on SFX_Echo + TapeHiss buses |
| RULE-AUD-002 (TapeHiss -24/-18) | satisfied — TapeHiss bus always_on |
| RULE-AUD-003 (Record pitch 1.25) | satisfied — `SFX_ECHO_RECORD_START` pitch 1.25 |
| RULE-AUD-004 (3D rolloff 1m/18m) | satisfied — EchoPlayback corrected |
| RULE-AUD-005 (no doppler/spread) | satisfied — defaults enforced, EchoPlayback corrected |
| RULE-MIX-001 (TapeHiss always) | satisfied |
| RULE-MIX-002 (Echo bandpass) | satisfied at mixer level |
| RULE-MIX-003 (rolloff range) | satisfied |
| RULE-MIX-004 (ducking exponential) | satisfied via snapshot transitions (exponential blend) |
| RULE-MIX-005 (no direct volume writes) | satisfied except documented MusicStateMachine stem-trim exception |

## 11. IMPLEMENTATION ORDER

1. Extend `EchoesAudioMixerBuilder.cs` → regenerate `EchoesAudioMixer.mixer` with 9 groups + 9 snapshots + 11 params + 2 bandpass DSP. Verify via `read_console`.
2. Extend `EchoesAudioManager.cs` with snapshot + group cache + new setters. Verify compilation.
3. Create `AudioEventRegistry.cs` (ScriptableObject) and `AudioEventRegistry.asset` populated with the 66 entries from §6.
4. Create `AudioEventBus.cs` with pool (16 sources) and cooldown.
5. Create `MusicStateMachine.cs` with the §5.4.1 transition-time matrix and the no-music-enforcement rules.
6. Create `AudioEventListener.cs` subscribing the hooks from §5.5.
7. Correct `EchoPlayback.cs` spatial + DSP routing.
8. Asset reorg: copy `Efectos de sonido/*` to canonical `Assets/Audio/*` tree.
9. Drag the 66 scriptable-registry entries' clip slots: fill with existing 26 known clips; leave others null with TODO comment in the asset description field.
10. Run `read_console` to confirm zero errors. Run `LevelValidator` (`VAL-AUD-DIR-001/002/003`) on at least N01 and N15.

## 12. CROSS REFERENCES

- `Docs/Audit/AUDIO_AUDIT.md` — evidence base
- `Docs/Specs/AUDIO_GRAMMAR.md` `[SPEC-112]` — superseded in part
- `Docs/Specs/AUDIO_MIXER_SCHEMA.md` `[SPEC-127]` — superseded in part
- `Docs/ExecutableSpecs/audio_architecture.yaml` `[SPEC-EXEC-AUD-001]`
- `Docs/ExecutableSpecs/audio/audio_mixer_schema.yaml`
- `Docs/ExecutableSpecs/audio/audio_dsp_spec.yaml`
- `Docs/ExecutableSpecs/audio/AUDIO_CLIP_REGISTRY.yaml`
- `Docs/ExecutableSpecs/audio/AUDIO_SPATIALIZATION_SPEC.yaml`
- `Docs/ExecutableSpecs/audio/AUDIO_DSP_DUCKING_MATRIX.yaml`
- `Docs/GameDesign/DESIGN_PHILOSOPHY.md` `[SPEC-001]`

## 13. CHANGE HISTORY

- **v1.0 (2026-08-15)**: Initial direction following approved audit. Defined 9-bus / 9-snapshot mixer, 4 new scripts, 1 ScriptableObject registry of 66 events, music state machine with explicit no-constant-music enforcement, corrected EchoPlayback routing, asset manifest with graceful degradation for missing clips.
