# AUDIO_INTEGRATION_AUDIT.md — Pre-Integration Audit (Audio Integration Pass 2.0)

## Spec ID: AUDIT-AUD-002
## Version: 1.0
## Date: 2026-08-16
## Authority: Audit document (read-only evidence base) — subordinate to `SOURCE_OF_TRUTH.md` `[SPEC-000]`. Companion to `AUDIO_AUDIT.md` `[AUDIT-AUD-001]`.
## Scope: Non-destructive integration of the new 2.0 audio assets into the EXISTING audio architecture, without creating parallel systems.

---

## CURRENT AUDIO ARCHITECTURE

### 1.1 Singletons & management
| Component | File | Role |
|---|---|---|
| `EchoesAudioManager` | `Assets/Scripts/EchoesAudioManager.cs` | Singleton (DontDestroyOnLoad). Loads `Resources/EchoesAudioMixer.mixer`. 4 volume setters (Master/Music/SFX/Echo) → PlayerPrefs. `FindGroup(name)` lookup. `EnsureExists()` factory. **No** snapshot API, **no** event API, **no** ducking, **no** pooling. |
| `GameFeelController` | `Assets/Scripts/GameFeelController.cs` | **De facto runtime audio dispatcher today.** 1 SFX AudioSource + 3 ambient AudioSources. Exposes 16 `[SerializeField] AudioClip` slots (jump, landing, hard landing, footstep, scrape, gravityShift, puzzleSolved, record, recordStop, echoSpawn, echoFade, softError, platePress, doorMove, playerDeath, respawn + 4 ambient: ambientLoop, industrialDrone, ventilationHum, clockChime). All `Play*` methods called via `GameFeelController.Instance?.Play...`. Spawns transient `AudioSource` GOs for one-shots (`PlayClip3D` → `new GameObject("OneShotAudio")` + `Destroy`) — **no pooling**. Has procedural AudioClip fallbacks (`CreateToneClip`/`CreateNoiseClip`/`CreateClickClip`) for null slots. |

### 1.2 AudioMixer (current, verified via builder)
- Asset: `Assets/Resources/EchoesAudioMixer.mixer`
- Builder: `Assets/Editor/EchoesAudioMixerBuilder.cs` (`[InitializeOnLoad]`, Reflection against `UnityEditor.Audio.AudioMixerController`).
- Hierarchy actually produced (4 groups only):
  ```
  Master [exp: MasterVolume]
  ├─ Music [exp: MusicVolume]
  ├─ SFX   [exp: SFXVolume]
  └─ Echo  [exp: EchoVolume]
  ```
- **No** Ambience, Voice, UI, Foley, TapeHiss buses.
- **No** snapshots (ducking/music-states unimplementable in the current asset).
- **No** DSP effects / bandpass filters.
- Idempotent: if `FindMatchingGroups("Master").Length == 0` the asset is deleted and regenerated; otherwise the existing asset is returned untouched (builder does NOT patch missing groups — it only generates from scratch).

### 1.3 AudioSource routing (current)
| Source | Bus | Note |
|---|---|---|
| `GameFeelController.audioSource` (SFX) | `SFX` | OK |
| `GameFeelController._ambientSource1/2/3` (room tone/industrial/vent) | **`Music`** | **BUG** — ambient sources routed to Music bus, polluting music ducking. |
| `GameFeelController.PlayClip3D` one-shots | `SFX` | OK but transient allocs |
| `EchoPlayback._audioSource` | `Echo` | OK; rolloff WRONG (`minDistance=4`, `maxDistance=42`, `doppler=0.05`, `spread=18`) — violates `RULE-AUD-004/005` |
| `MenuHoverSystem._uiAudioSource` | **none (ungrouped → Master)** | **BUG** — no UI bus |
| `MenuHoverSystem._crtAmbientSource` | **none (ungrouped → Master)** | **BUG** — no UI bus |
| `VN_DialogueController._voiceSource` | **none (ungrouped → Master)** | **BUG** — no Voice bus |
| `LightFlicker.audioSource` | none | Fluorescent hum, no verified activation logic |

### 1.4 Asset organisation (current)
Two disconnected clip roots:
1. `Assets/Efectos de sonido/` — Spanish folder name with spaces/accents; legacy Freesound clips + 9 menu mp3s.
2. `Assets/Resources/Audio/` — new 2.0 assets (player/ui/narrative subfolders) + music/transition clips + `VN/voice` + `VN/ambient`.
- No canonical `Assets/Audio/{Category}/` tree exists, despite every YAML referencing it.
- `.meta` files exist for all clips (preserved during any future reorganisation).

### 1.5 Import settings (current)
Unverified without a live Unity instance. The audit assumes defaults; Phase 6 of the integration pass will apply category-specific import settings (UI/SFX mono+decompress-on-load, music streaming, ambience stereo/3D-by-context, echo 3D+bandpass).

---

## EXISTING EVENTS (gameplay hooks already present in code)

These C# hooks are the **safe extension points** — the integration may subscribe to them WITHOUT modifying gameplay logic.

| Hook | File | Signature | Currently plays audio? |
|---|---|---|---|
| `PlayerController.OnJumped` | `PlayerController.cs:11` | `Action` | No (only GameFeel + camera subscribe) |
| `PlayerController.OnLanded` | `PlayerController.cs:10` | `Action<float>` (downward speed) | No (GameFeel subscribes via `OnLanded += Wait…`) |
| `PlayerController_Animation` footstep event | `PlayerController_Animation.cs:68` | direct `GameFeelController.Instance?.PlayFootstep(...)` | Yes — via GameFeel |
| `EchoRecorder.RecordingStarted` | `EchoRecorder.cs:128` | `Action` | Yes — GameFeel `PlayRecordStart` |
| `EchoRecorder.RecordingStopped` | `EchoRecorder.cs:130` | `Action<bool>` | Yes — GameFeel `PlayRecordStop` |
| `EchoRecorder.EchoCreated` | `EchoRecorder.cs:124` | `Action<int>` | Yes — GameFeel `PlayEchoSpawn` |
| `EchoRecorder.EchoesCleared` | `EchoRecorder.cs:126` | `Action` | No |
| `EchoPlayback.PhaseChanged` | `EchoPlayback.cs:29` | `Action<EchoPlaybackPhase>` | No (used for visual states) |
| `EchoPlayback.BeginPlayback` | `EchoPlayback.cs:166` | method | No direct audio (GameFeel not called here) |
| `EchoPlayback.FadeOutAndDestroy` | `EchoPlayback.cs:268` | method | Yes — GameFeel `PlayEchoFade` invoked inside |
| `PressurePlate.PressedChanged` | `PressurePlate.cs:45` | `Action<bool>` | Yes — via `GameFeelController.PlayPlatePress` (called from plate? — see MISSING EVENTS) |
| `DoorController.DoorStateChanged` | `DoorController.cs:20` | `Action<bool>` | Yes — via GameFeel `PlayDoorMove` |
| `PuzzleCondition.ConditionChanged` | `PuzzleCondition.cs:51` | `Action<bool>` | No direct audio |
| `PuzzleSignal.SignalChanged` | `PuzzleSignal.cs:15` | `Action<PuzzleSignal,bool>` | Yes — GameFeel `PlayPuzzleSolved` (success only; failure path missing) |
| `GoalTrigger.SatisfactionChanged` | `GoalTrigger.cs:20` | `Action<GoalTrigger,bool>` | No audio |
| `GameStateController.StateChanged` | `GameStateController.cs:28` | `Action<GameFlowState,GameFlowState>` | No audio |
| `LevelRuntimeController` soft/hard reset | `LevelRuntimeController.cs` | Q/T keys | No audio |
| `LightFlicker.OnIntensityChange` | `LightFlicker.cs:16` | `Action<float>` | No (fluorescent hum not triggered) |

### UI hooks (UITK callbacks, not C# events — subscribe via RegisterCallback)
| Hook | File | Audio? |
|---|---|---|
| `MenuHoverSystem` hover in/out/click/nav | `MenuHoverSystem.cs:184-211` | Yes — `PlayHoverIn/Out/Click/NavMove` |
| `MainMenuController` | `MainMenuController.cs` | inherits MenuHoverSystem |
| `PauseMenu` pause/resume | `PauseMenu.cs` | No — **no audio on open/close today** |
| `GameHUD` toast/objective | `GameHUD.cs` | No audio |
| `VN_DialogueController` open/advance | `VN_DialogueController.cs` | No audio (voice source exists but no SFX on open/advance) |
| `VN_ChoiceGateController` hover/confirm | `VN_ChoiceGateController.cs` | No audio |
| `InteractionPromptController` shown/confirmed | `InteractionPromptController.cs` | No audio |

### Scene transitions
| Hook | File | Audio? |
|---|---|---|
| `SceneTransitionManager` begin fade | `SceneTransitionManager.cs` | No |
| `SceneTransitionManager` load complete | `SceneTransitionManager.cs` | No |

---

## EXISTING MIXER
See §1.2 above. 4 groups, 4 exposed params, 0 snapshots, 0 DSP effects. Detailed in `AUDIO_AUDIT.md` §4.

---

## EXISTING CLIPS (already wired into the runtime before this pass)

### Clips referenced via `GameFeelController` `[SerializeField]` slots (16)
These are wired in the Inspector per-scene/prefab. The procedural `EnsureRuntimeFallbackAudio()` generates synthetic tones when a slot is null — so the game "has sound" even with no assets, but the new 2.0 real clips supercede the fallbacks.

| Slot | Fallback (if null) | Real clip candidates |
|---|---|---|
| jumpClip | `CreateToneClip("SFX_JumpAir",…)` | `Resources/Audio/player/sfx_jump.mp3` (1s) |
| landingClip | `CreateNoiseClip("SFX_LandingSoft",…)` | `Resources/Audio/player/sfx_landing_soft.mp3` (1s) |
| hardLandingClip | `CreateNoiseClip("SFX_LandingHard",…)` | `Resources/Audio/player/sfx_landing_hard.mp3` (1s) |
| footstepClip | `CreateNoiseClip("SFX_Footstep",…)` | `Efectos de sonido/setps.mp3` (4s, needs split into 4) |
| movementScrapeClip | `CreateToneClip("SFX_MechanicServo",…)` | `Resources/Audio/player/sfx_movement_scrape.mp3` (1s) |
| gravityShiftClip | `CreateToneClip("SFX_GravityShift",…)` | **MISSING** — no `sfx_gravity_shift` asset |
| puzzleSolvedClip | `CreateToneClip("SFX_PuzzleSolved",…)` | `Resources/Audio/player/sfx_puzzle_success.mp3` (2s) |
| recordClip | `CreateToneClip("SFX_RecordStart",…)` | `Efectos de sonido/GRABACIÓN INICIO.mp3` (12s) |
| recordStopClip | `CreateToneClip("SFX_RecordStop",…)` | `Efectos de sonido/reset.mp3` (12s) |
| echoSpawnClip | `CreateToneClip("SFX_EchoSpawn",…)` | `Efectos de sonido/CREACIÓN DE ECO.mp3` (12s) |
| echoFadeClip | `CreateToneClip("SFX_EchoFadeAway",…)` | `Resources/Audio/player/sfx_echo_residual_tail.mp3` (2s) |
| softErrorClip | `CreateToneClip("SFX_SoftError",…)` | **MISSING** |
| platePressClip | `CreateClickClip("SFX_PlateClick",…)` | `Efectos de sonido/CLICK.mp3` (2s) |
| doorMoveClip | `CreateToneClip("SFX_DoorServo",…)` | `Efectos de sonido/PUERTA.mp3` (12s) |
| playerDeathClip | `CreateToneClip("SFX_PlayerDeath",…)` | `Resources/Audio/player/sfx_player_death.mp3` (1s) |
| respawnClip | `CreateToneClip("SFX_Respawn",…)` | `Resources/Audio/player/sfx_respawn.mp3` (1s) |

### Clips referenced via `MenuHoverSystem` (5)
| Slot | Real candidate |
|---|---|
| hoverInClip | `Efectos de sonido/menu/Hover in click.mp3` (1s) |
| hoverOutClip | `Efectos de sonido/menu/Hover Out Clip.mp3` (1s) |
| clickConfirmClip | `Efectos de sonido/menu/Click Confirm Clip.mp3` (1s) |
| navMoveClip | `Efectos de sonido/menu/Nav Move Clip.mp3` (1s) |
| crtAmbientClip | `Efectos de sonido/menu/crt.mp3` (30s loop) |

---

## DUPLICATE SYSTEMS

### 2.1 Procedural clip generation (GameFeelController)
`GameFeelController.EnsureRuntimeFallbackAudio()` synthesizes 16 `AudioClip`s in-memory when inspector slots are null (`AudioClip.Create` + `SetData`).
- **Status**: Not a duplicate "system" per se; it is graceful degradation. But it means the game sounds even with zero real assets. During integration, real 2.0 clips must be wired into the slots so the fallbacks become dormant (they only fire on null). **DO NOT remove `EnsureRuntimeFallbackAudio`** — it is the safety net for missing clips and is explicitly compatible with `AUDIO_DIRECTION.md §8.2` (graceful degradation).
- **Risk**: If a real clip is wired AND the fallback also fires, two sources play. Verified: fallback only fires on `== null`, so wiring a real clip disables the fallback for that slot. **Safe.**

### 2.2 No second AudioManager / no parallel AudioBus
- `grep` for `AudioManager`/`AudioDirector`/`AudioEventBus`/`MusicStateMachine`/`AudioEventRegistry` in `Assets/Scripts`: **0 matches**. The architecture proposed in `AUDIO_DIRECTION.md` (AudioEventBus, MusicStateMachine, AudioEventRegistry ScriptableObject) **does NOT exist yet** — those are the *target* system, not the current one.
- **Conclusion**: There is exactly ONE audio system today (`EchoesAudioManager` + `GameFeelController`). No duplicate systems to reconcile.

### 2.3 `EchoPlayback` per-source DSP
`EchoPlayback.ConfigureSpatialVoicePlayback` + `ApplyAnalogAudioFilters` add `AudioLowPassFilter`/`AudioHighPassFilter` per-source to simulate cassette bandpass, because the mixer has no bandpass DSP. This is a workaround, not a duplicate system — moving DSP to the mixer (per `AUDIO_DIRECTION §5.7`) is a Phase 3+ task and is **out of scope** for the non-destructive asset wiring pass unless explicitly required to connect an event.

---

## MISSING EVENTS (per directive §3 event map — no audio feedback today)

| Event | Existing hook | Audio today? | New clip available? |
|---|---|---|---|
| Movement scrape | `GameFeelController.PlayMovementScrape` | Yes (fallback) | `sfx_movement_scrape.mp3` ✓ |
| Gravity shift | `GameFeelController.PlayGravityShift` | Yes (fallback) | **`sfx_gravity_shift` MISSING** |
| Recording warning (buffer ≥80%) | `EchoRecorder` — no dedicated hook | No | `sfx_recording_warning.mp3` ✓ |
| Echo despawn | `EchoPlayback.FadeOutAndDestroy` | Yes (PlayEchoFade) | `sfx_echo_residual_tail.mp3` covers fade; despawn stinger MISSING |
| Puzzle failure | `PuzzleSignal` false branch | No | `sfx_puzzle_failure.mp3` ✓ (in player/ — odd path) |
| Button/lever | none | No | `sfx_button.mp3` ✓ (in player/ — odd path) |
| Telephone | `PuzzleSignal`/level trigger | No | `164843__…telephone-bell-ring.wav` ✓ (legacy) |
| Clock chime | `GameFeelController.PlayEerieChime` | Yes | `freesound_community-clock-chime-88027.mp3` ✓ (legacy) |
| UI hover out / confirm / nav | `MenuHoverSystem` | Yes | reuse legacy menu clips |
| CRT hum | `MenuHoverSystem` | Yes | reuse `crt.mp3` |
| Pause open | `PauseMenu.Pause` | **No** | **`ui_pause_open` MISSING** (only `ui_pause_close` present, misnamed `ui_pause_closemp3.mp3`) |
| Pause close | `PauseMenu.Resume` | **No** | `ui_pause_closemp3.mp3` ✓ (rename target) |
| Choice hover | `VN_ChoiceGateController` | No | **`ui_choice_hover` MISSING** |
| Choice confirm | `VN_ChoiceGateController` | No | **`ui_choice_confirm` MISSING** |
| Interaction available | `InteractionPromptController` show | No | `ui_interaction_available.mp3` ✓ |
| Interaction confirm | `InteractionPromptController` confirm | No | **`ui_interaction_confirm` MISSING** |
| Interaction denied | invalid interaction | No | `ui_interaction_denied.mp3` ✓ |
| Dialogue open | `VN_DialogueController` open | No | `ui_dialogue_open.mp3` ✓ |
| Dialogue advance | `VN_DialogueController` advance | No | `ui_dialogue_advance.mp3` ✓ |
| Modal confirm | modal confirm | No | **`ui_modal_confirm` MISSING** |
| Toast | `GameHUD` toast | No | `ui_toast.mp3` ✓ |
| Lyra/Aiden voice 001/002 | `VN_DialogueController._voiceSource` | Source exists, no clips wired | 4 wav exist (but show 0 KB / 0:00 — **placeholder/empty**) |
| Memory discovery | `MemorySystem` (no hook found) | No | `sfx_memory_discovery.mp3` ✓ |
| Memory ambience zone | zone enter | No | `amb_memory_whisper.mp3` ✓ |
| Level transition out | `SceneTransitionManager` begin | No | `sfx_level_transition_out.mp3` ✓ (3s) |
| Level transition in | `SceneTransitionManager` end | No | `sfx_level_transition_in.mp3` ✓ (2s) |
| Credits | credits scene | No | `mus_credits.mp3` ✓ (90s) |
| Music: menu theme | MainMenu load | No (no music state machine) | reuse `menu/maint_thememenump3.mp3` ✓ |
| Music: exploration drone | state Exploration | No | `01-mus_exploration_drone…_081626.mp3` ✓ (30s) |
| Music: tension drone | state Tension | No | `seamless-loopno-melodypromptcreate-a-dark_081626.mp3` ✓ (30s, **UNRESOLVED name** — likely tension drone) |
| Music: puzzle texture | state Puzzle | No | `mus_puzzle_texture.mp3` ✓ (30s) |
| Music: memory piano | state Memory | No | `mus_memory_piano.mp3` ✓ (20s) |
| Music: dialogue bed | state Dialogue | No | `mus_dialogue_bed.mp3` ✓ (20s) |
| Music: ending theme | state Ending | No | `mus_ending_theme.mp3` ✓ (60s) |
| Music: exploration texture | state Exploration (layer) | No | **MISSING** (only single drone available; texture layer absent) |
| Ambience: tape hiss | mixer init | No | reuse `476025__…hiss-noise.wav` ✓ (legacy) |
| Ambience: room tone | scene load | Yes (GameFeel ambient loop) | reuse `Resources/Audio/VN/ambient/room_tone.wav` ✓ (0 KB — placeholder) |
| Ambience: room tone w/hum | scene load (alt) | No | reuse `144046__gchase__room_tone…low_hum.wav` ✓ (legacy, 2:13) |
| Ambience: hallway | hallway zone | No | reuse `274213__bexhillcollege__college-hallway-ambience.wav` ✓ (legacy, 1:01) |
| Ambience: ventilation | duct zone | Yes (GameFeel ventilation) | reuse `Ventilation.wav` ✓ (legacy, 1:30) |
| Ambience: industrial hum | basement | Yes (GameFeel industrial) | reuse `Industrial-hum.wav` ✓ (legacy, 2:29) |
| Ambience: distant clangs | random punctual | No | reuse `507465__danjocross__four-quiet-distant-clangs.aiff` ✓ (legacy) |
| Ambience: metal/concrete | foley scrape | No | reuse `166118__…metal-concrete.wav` ✓ (legacy, 8s) |

---

## SAFE EXTENSION POINTS

The integration MUST extend the existing system, not replace it. The following are the approved non-destructive entry points:

1. **`EchoesAudioMixerBuilder.EnsureAudioMixer()`** — extend the builder to add the missing groups (Ambience, Voice, UI, Foley, TapeHiss + SFX subgroups Player/Puzzle/Echo_under_SFX) via the existing Reflection `CreateChildGroup` path. Idempotent: only patch groups that don't exist. **Preserve the failsafe delete-and-regenerate branch.**
2. **`EchoesAudioManager`** — add `TransitionToSnapshot`, `GetGroup(enum)`, and 7 new volume setters (Ambience, Player, Puzzle, UI, Foley, Voice, TapeHiss). **Do not remove** the existing 4 setters — the Settings UI calls them.
3. **`GameFeelController` `[SerializeField] AudioClip` slots** — wire the new 2.0 real clips into the existing slots via Inspector (per-scene/prefab). The fallback `EnsureRuntimeFallbackAudio()` silently dormantifies. **No gameplay code change required.**
4. **C# event subscriptions (read-only)** — subscribe to `PressurePlate.PressedChanged`, `DoorController.DoorStateChanged`, `PuzzleSignal.SignalChanged`, `EchoRecorder.RecordingStarted/Stopped`, `EchoPlayback.PhaseChanged`, `PlayerController.OnJumped/OnLanded`, `GameStateController.StateChanged` from a NEW `AudioEventListener` MonoBehaviour. These hooks already exist; subscribing does NOT modify the publisher.
5. **UITK callbacks** — UI events are subscribed via `RegisterCallback<FocusEvent/BlurEvent/ClickEvent>` in the existing controllers (`MainMenuController`, `PauseMenu`, `GameHUD`, `VN_*`). Adding audio calls in the existing `OnButton*` methods or via a new listener is non-destructive.
6. **`MenuHoverSystem` AudioSource** — route `_uiAudioSource.outputAudioMixerGroup` to a new `SFX_UI` group once it exists.
7. **`SceneTransitionManager`** — add `Play(LevelTransitionOut/In)` calls at the existing begin/end markers.

---

## RISKS

| Risk | Severity | Mitigation |
|---|---|---|
| Wiring a real clip + fallback both fire | Low | Fallback only fires on `== null`; verified safe. |
| Moving `Efectos de sonido/` clips breaks Inspector references | High | **DO NOT move** in this pass; reference by GUID. Reorganisation is a Phase 5 task and requires `.meta` preservation + reference repoint. |
| Voice wavs (lyra/aiden/room_tone) are 0 KB placeholders | High | Mark `STATUS = UNRESOLVED`; do not wire — graceful degradation only. |
| `seamless-loopno-melodypromptcreate-a-dark_081626.mp3` filename is ambiguous | Medium | Catalogued as `mus_tension_drone` candidate but tagged `UNRESOLVED` until aural verification. |
| `ui_pause_closemp3.mp3` has double `.mp3` in name | Low | Rename target only if no scene/prefab references the literal name; otherwise use as-is and document. |
| Generating new mixer groups via Reflection can corrupt the asset | High | Extend builder idempotently: only create groups that don't exist; pre-commit the `.mixer` before edits (there is none today — git tracking). |
| `GameFeelController` transient `AudioSource` allocs (`PlayClip3D` → `new GameObject` + `Destroy`) | Medium | Out of scope for non-destructive pass; pooling is a Phase 13+ optimization. |
| Mixer has no snapshots → no ducking/music-state implementable | High | Implementing snapshots is the `AUDIO_DIRECTION.md` target system. For the non-destructive asset pass, music state changes can use direct `AudioSource.volume` lerp on dedicated music stems as a transitional measure, documented as a deviation. Full snapshot system is a follow-up. |
| `EchoPlayback` rolloff wrong (4m/42m, doppler 0.05, spread 18) | High | Correcting this is a 1-line change in `EchoPlayback.ConfigureSpatialVoicePlayback` and is explicitly mandated by `AUDIO_DIRECTION §5.7`. Safe: pure config, no timing logic. **Approved.** |
| Builder `[InitializeOnLoad]` regenerates mixer on every domain reload | Low | Existing idempotency returns existing mixer if `Master` group present; only fires when asset missing/corrupt. Adding groups must follow same idempotent pattern. |
| Two `Efectos de sonido/` duplicates (Nuevos assets/ folder shadows originals) | Low | Ignore the `Nuevos assets/Efectos de sonido/` copies; they are duplicates already in `Resources/Audio`. |

---

## FILES THAT MUST NOT BE MODIFIED

Per directive §17, the following gameplay-logic files MUST NOT be touched except where an audio hook directly requires a 1-line wiring. If a change seems needed: STOP, document, propose smallest safe integration point.

### Gameplay logic (DO NOT MODIFY)
| File | Reason |
|---|---|
| `PlayerController.cs` / `PlayerController_Gravity.cs` / `PlayerController_Animation.cs` / `PlayerController_Visual.cs` | Movement, gravity, animation timing. The `OnJumped`/`OnLanded` events already exist — subscribe from outside. The `PlayerController_Animation.cs:68` footstep call is the one existing audio entry; keep it. |
| `EchoRecorder.cs` | Recording logic, frame capture, timing. `RecordingStarted/Stopped/EchoCreated/EchoesCleared` events already exist — subscribe from outside. |
| `EchoPlayback.cs` | **EXCEPTION**: `ConfigureSpatialVoicePlayback` rolloff values may be corrected (1-line config, mandated by spec). No timing/playback logic changes. `FadeOutAndDestroyRoutine` volume-write deviation is documented but NOT changed in this pass. |
| `PressurePlate.cs` | `PressedChanged` event exists. Subscribe from outside. |
| `DoorController.cs` | `DoorStateChanged` event exists. Subscribe from outside. |
| `PuzzleCondition.cs` / `PuzzleSignal.cs` | Signal graph logic. Subscribe to `ConditionChanged`/`SignalChanged` from outside. |
| `GoalTrigger.cs` / `LevelGoal.cs` / `LevelExit.cs` | Goal/exit logic. Subscribe from outside. |
| `LevelRuntimeController.cs` | Reset logic. If reset SFX needed, hook `Q`/`T` via a listener, not inside. |
| `LevelEnvironmentBootstrap.cs` | Scene bootstrap. |
| `EchoesNewProductionBuilder.cs` / `Editor/EchoesLevelShell.cs` | Build pipeline. |
| All `Assets/Editor/Echoes*Builder*.cs` | Generation pipeline. |
| `SceneTransitionManager.cs` | **EXCEPTION**: may add `Play(LevelTransitionOut/In)` at existing begin/end markers if no cleaner hook exists; 2-line addition, documented. Prefer subscription. |
| `GameStateController.cs` | State machine. `StateChanged` event exists — subscribe. |
| `MainMenuController.cs` / `PauseMenu.cs` / `GameHUD.cs` | **EXCEPTION**: may add audio calls in existing event handlers if no `AudioEventListener` subscription path is feasible; prefer subscription. |
| `VN_DialogueController.cs` / `VN_ChoiceGateController.cs` / `VN_OverlayController.cs` | VN logic. Prefer UITK callback subscription from a listener. |
| `InteractionPromptController.cs` / `InteractionSystem.cs` | Interaction logic. Prefer subscription. |
| All shaders / materials / camera controllers / post-processing | Out of scope. |

### Audio files (DO NOT DELETE / DO NOT OVERWRITE)
- All `.meta` files for audio assets.
- `Assets/Efectos de sonido/**` legacy clips (reference by GUID; do not move/delete in this pass).
- `Assets/Resources/Audio/VN/voice/**` wavs (even if 0 KB — they may be intentional placeholders).
- The procedural `EnsureRuntimeFallbackAudio()` fallbacks inside `GameFeelController` (graceful degradation safety net).

---

## SUMMARY

The current audio system is a **single** system (`EchoesAudioManager` + `GameFeelController` + `MenuHoverSystem`) with 4 mixer groups, no snapshots, no event bus, and 16 serialized clip slots that already drive ~9 gameplay events via procedural fallbacks. The new 2.0 assets live in `Assets/Resources/Audio/` (the "recursos/audio" location). 

The non-destructive integration pass will:
1. Extend the mixer builder to add the missing 5 buses (idempotent, Reflection-preserved).
2. Wire the new real clips into the existing `GameFeelController` slots (replaces fallback dormant, no gameplay change).
3. Add a new `AudioEventListener` that subscribes to existing C# events (no publisher modification).
4. Leave missing/UNRESOLVED clips on graceful-degradation (no throw, no broken refs).
5. NOT modify gameplay timing/puzzle/camera/UI architecture.

**The audit identifies zero duplicate audio systems and confirms the existing architecture is the single extension target.**

## CROSS REFERENCES
- `Docs/Authority/SOURCE_OF_TRUTH.md` `[SPEC-000]`
- `Docs/Technical/PROJECT_CONTEXT.md` `[SPEC-110]`
- `Docs/GameDesign/ECHOES_BIBLE.md` `[SPEC-101]`
- `Docs/Specs/AUDIO_DIRECTION.md` `[SPEC-AUD-DIR-001]` — target architecture (AudioEventBus, MusicStateMachine — DO NOT BUILD in non-destructive pass unless no equivalent exists)
- `Docs/Specs/AUDIO_GRAMMAR.md` `[SPEC-112]`
- `Docs/Specs/AUDIO_MIXER_SCHEMA.md` `[SPEC-127]`
- `Docs/Audit/AUDIO_AUDIT.md` `[AUDIT-AUD-001]` — companion evidence base
- `Docs/Archive/Obsolete/QA_CHECKLIST.md` `[SPEC-304]` (archived — NOT source of truth)

## CHANGE HISTORY
- **v1.0 (2026-08-16)**: Pre-integration audit for the non-destructive 2.0 asset wiring pass. Catalogued current architecture, existing events, existing clips, safe extension points, risks, and files that must not be modified. Confirmed no duplicate audio systems exist.
