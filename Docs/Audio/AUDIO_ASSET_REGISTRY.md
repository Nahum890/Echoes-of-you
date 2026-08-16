# AUDIO_ASSET_REGISTRY.md — Inventario de Recursos de Audio (Echoes of You 2.0)

## Spec ID: AUDIT-AUD-003
## Version: 1.0
## Date: 2026-08-16
## Authority: Inventory document — subordinate to `SOURCE_OF_TRUTH.md` `[SPEC-000]`. Companion to `AUDIO_INTEGRATION_AUDIT.md` `[AUDIT-AUD-002]`.
## Source location: `Assets/Resources/Audio/` (referido como "recursos/audio" en la directiva) + legacy `Assets/Efectos de sonido/`.

---

## CONVENCIÓN DE ESTADOS

| Status | Significado |
|---|---|
| `RESOLVED` | Clip identificado + nombre coincide con evento + listo para cableado. |
| `REUSE` | Clip legacy reutilizado para un evento del 2.0 (mismo rol, nombre no canónico). |
| `UNRESOLVED` | Nombre ambiguo o archivo placeholder — NO asignar automáticamente hasta verificación aural. |
| `MISSING` | El evento del mapa §3 no tiene clip en disco — degradación elegante. |
| `PLACEHOLDER` | Archivo existe pero 0 KB / 0 s — es un stub, no contenido real. |

Canales: `M` = Mono, `S` = Stereo. Spatial: `2D` = UI/música/voz, `3D` = fuente espacial.

---

## 1. MUSIC (mus_*) — Mixer: Music

| Asset | Category | Event | Duración | Loop | Spatial | Size KB | Channels | GUID | Status |
|---|---|---|---|---|---|---|---|---|---|
| `Resources/Audio/01-mus_exploration_dronewavduration-30-secondsseamless-loopno_081626.mp3` | Music | MUS_EXPLORATION (drone) | 0:30 | Sí | 2D | 485.5 | S | `2f69e182d7f9bee45b60bba02e433335` | RESOLVED (rename target `mus_exploration_drone.wav`) |
| `Resources/Audio/seamless-loopno-melodypromptcreate-a-dark_081626.mp3` | Music | MUS_TENSION (drone) | 0:30 | Sí | 2D | 485.5 | S | `d2ab5769f4637ae4b9d67a9ade144f56` | UNRESOLVED (nombre opaco; verificado: tono oscuro sin melodía, 30s seamless — candidato fuerte a tension drone. Confirmar auralmente antes de asignar.) |
| `Resources/Audio/mus_puzzle_texture.mp3` | Music | MUS_PUZZLE (texture) | 0:30 | Sí | 2D | 485.5 | S | `17762047417463a47bb31db77292096c` | RESOLVED |
| `Resources/Audio/mus_memory_piano.mp3` | Music | MUS_MEMORY (piano) | 0:20 | Sí | 2D | 329.2 | S | `806240283e7aedc4fa3b7eb52c46a5a8` | RESOLVED |
| `Resources/Audio/mus_dialogue_bed.mp3` | Music | MUS_DIALOGUE (pad bed) | 0:20 | Sí | 2D | 329.2 | S | `081f21dd3a32b63479cb294fe1498f93` | RESOLVED |
| `Resources/Audio/mus_ending_theme.mp3` | Music | MUS_ENDING | 0:60 | No | 2D | 954.1 | S | `40d004f07c783214e9d5bda0e6479b87` | RESOLVED |
| `Resources/Audio/mus_credits.mp3` | Music | MUS_CREDITS | 1:30 | Sí | 2D | 1423.1 | S | `ed3b9ce78bb451948bdec33bfe3dac6c` | RESOLVED |
| `Efectos de sonido/menu/maint_thememenump3.mp3` | Music | MUS_MENU | 2:15 | Sí | 2D | 2110.2 | S | (legacy) | REUSE → MUS_MENU |
| *(no asset)* | Music | MUS_EXPLORATION (texture layer) | — | — | — | — | — | — | MISSING — solo drone disponible; texture layer ausente. Degradación elegante. |

---

## 2. AMBIENCE (amb_*) — Mixer: Ambience (bus a crear) / TapeHiss

| Asset | Category | Event | Duración | Loop | Spatial | Size KB | Channels | Status |
|---|---|---|---|---|---|---|---|---|
| `Efectos de sonido/476025__deleted_user_10149686__simulation-of-worn-video-cassette-audio-hiss-noise.wav` (legacy) | Ambience | AMB_TAPE_HISS | 0:05 | Sí | 2D | 900.8 | S | REUSE → AMB_TAPE_HISS (RULE-AUD-002) |
| `Resources/Audio/VN/ambient/room_tone.wav` | Ambience | AMB_ROOM_TONE | 0:00 | Sí | 2D | 0 | — | PLACEHOLDER — 0 KB, necesita re-export |
| `Efectos de sonido/144046__gchase__room_tone_ambience_medium_control_low_hum.wav` (legacy) | Ambience | AMB_ROOM_TONE_HUM | 2:13 | Sí | 2D | 37651.1 | S | REUSE → AMB_ROOM_TONE_HUM |
| `Efectos de sonido/274213__bexhillcollege__college-hallway-ambience.wav` (legacy) | Ambience | AMB_HALLWAY | 1:01 | Sí | 2D | 17257.6 | S | REUSE → AMB_HALLWAY |
| `Efectos de sonido/Ventilation.wav` (legacy) | Ambience | AMB_VENTILATION | 1:30 | Sí | 3D | 51111.1 | S | REUSE → AMB_VENTILATION (ya en GameFeel) |
| `Efectos de sonido/Industrial-hum.wav` (legacy) | Ambience | AMB_INDUSTRIAL | 2:29 | Sí | 3D | 41907 | S | REUSE → AMB_INDUSTRIAL (ya en GameFeel) |
| `Efectos de sonido/507465__danjocross__four-quiet-distant-clangs.aiff` (legacy) | Ambience | AMB_DISTANT_CLANG | ~8s | No | 3D | 4332.5 | S | REUSE → AMB_DISTANT_CLANG |
| `Efectos de sonido/166118__deleted_user_2104797__metal-concrete.wav` (legacy) | Ambience | AMB_METAL_CONCRETE / SFX_MOVEMENT_SCRAPE (foley) | 0:08 | No | 3D | 1553.7 | S | REUSE (foley alt) |
| `Efectos de sonido/607238__szegvari__electric-dream-synth-drone-electric-cinematic.wav` (legacy) | Ambience | ambient drone (unassigned) | 0:12 | Sí | 2D | 3375 | S | UNRESOLVED — posible drone musical, no mapeado a evento §3 |
| `Efectos de sonido/637546__kyles__fluorescent-light-turn-on-hum-buzz-various.flac` (legacy) | Ambience | fluorescent hum (LightFlicker) | 1:33 | Sí | 3D | 3711.8 | S | REUSE → fluorescent hum |
| `Efectos de sonido/24167__patchen__confused-jet-3.flac` (legacy) | Ambience | unassigned | 0:32 | No | 2D | 2390.1 | S | UNRESOLVED |
| `Efectos de sonido/574865__trp__vhs-tape-clicks-rewind-play-mechanical-05.flac` (legacy) | Ambience/SFX | tape mechanical (N01 VN gate) | 1:10 | No | 2D | 11711.2 | S | REUSE → VN choice gate stinger (per N01_DESIGN.md) |
| `Resources/Audio/narrative/amb_memory_whisper.mp3` | Narrative/Ambience | AMB_MEMORY_WHISPER | 0:10 | Sí | 3D | 172.9 | — | RESOLVED |

---

## 3. PLAYER (sfx_footstep/sfx_jump/etc.) — Mixer: SFX_Player (sub-bus) / SFX_Foley

| Asset | Category | Event | Duración | Loop | Spatial | Size KB | GUID | Status |
|---|---|---|---|---|---|---|---|---|
| `Efectos de sonido/setps.mp3` (legacy) | Player | SFX_FOOTSTEP (variant source) | 0:04 | No | 3D | 79.2 | (legacy) | REUSE — **split en 4 variaciones** (sfx_footstep_01..04) requerido; actualmente un solo compuesto |
| `Resources/Audio/player/sfx_jump.mp3` | Player | SFX_JUMP | 0:01 | No | 3D | 32.5 | `59126ec24c1cd1040bf94fd30212e623` | RESOLVED |
| `Resources/Audio/player/sfx_landing_soft.mp3` | Player | SFX_LANDING_SOFT | 0:01 | No | 3D | 32.5 | `ba1286e3a49fbd84aa3898ed8495ae9b` | RESOLVED |
| `Resources/Audio/player/sfx_landing_hard.mp3` | Player | SFX_LANDING_HARD | 0:01 | No | 3D | 32.5 | `2b091d0f016c75d4a9f64d0d9c3ded52` | RESOLVED |
| `Resources/Audio/player/sfx_movement_scrape.mp3` | Player | SFX_MOVEMENT_SCRAPE | 0:01 | No | 3D | 32.5 | `92ce199c555745b42bce95e6e8f8a5d2` | RESOLVED |
| *(no asset)* | Player | SFX_GRAVITY_SHIFT | — | — | 3D | — | — | MISSING — usa fallback procedural `CreateToneClip("SFX_GravityShift")` hasta que se entregue el clip |
| `Resources/Audio/player/sfx_player_death.mp3` | Player | SFX_PLAYER_DEATH | 0:01 | No | 2D | 32.5 | `e30a39e54acf5624e81ad155ca1e46ea` | RESOLVED |
| `Resources/Audio/player/sfx_respawn.mp3` | Player | SFX_RESPAWN | 0:01 | No | 2D | 32.5 | `197d4797208db0842a38c86d2331ab83` | RESOLVED |

---

## 4. ECHO (sfx_echo_*) — Mixer: SFX_Echo (bandpass 300–3500 Hz)

| Asset | Category | Event | Duración | Loop | Spatial | Size KB | GUID | Status |
|---|---|---|---|---|---|---|---|---|
| `Efectos de sonido/GRABACIÓN INICIO.mp3` (legacy) | Echo | SFX_ECHO_RECORD_START | 0:12 | No | 2D | 188.6 | (legacy) | REUSE (recortar a ~0.3s según RULE-AUD-003) |
| `Efectos de sonido/reset.mp3` (legacy) | Echo | SFX_ECHO_RECORD_STOP / SILENT_RESET | 0:12 | No | 2D | 188.6 | (legacy) | REUSE |
| `Resources/Audio/player/sfx_recording_warning.mp3` | Echo | SFX_RECORDING_WARNING | 0:01 | No | 2D | 32.5 | `ed85cd435aa020049a8d952eecb4b01a` | RESOLVED (ubicación `player/` — considerar mover a `Echo/` en fase 5) |
| `Efectos de sonido/CREACIÓN DE ECO.mp3` (legacy) | Echo | SFX_ECHO_SPAWN | 0:12 | No | 3D (blend 0.7) | 188.6 | (legacy) | REUSE |
| `Efectos de sonido/LOOP DE ECO.mp3` (legacy) | Echo | SFX_ECHO_PLAYBACK_LOOP | 0:12 | Sí | 3D | 188.6 | (legacy) | REUSE |
| `Resources/Audio/player/sfx_echo_residual_tail.mp3` | Echo | SFX_ECHO_DESPAWN / residual tail | 0:02 | No | 3D | 48 | `964e125899799054cb206d7a6b1149e5` | RESOLVED (cubre fade/residual; despawn stinger explícito MISSING) |
| *(no asset)* | Echo | SFX_ECHO_DESPAWN (stinger explícito) | — | — | — | — | — | MISSING — residual_tail cubre el residual; el stinger de despawn no está separado. Degradación elegante. |

[Voz del eco: ver §6 NARRATIVE — Voice bus]

---

## 5. PUZZLE (sfx_plate_click/sfx_door_open/etc.) — Mixer: SFX_Puzzle (sub-bus)

| Asset | Category | Event | Duración | Loop | Spatial | Size KB | GUID | Status |
|---|---|---|---|---|---|---|---|---|
| `Efectos de sonido/CLICK.mp3` (legacy) | Puzzle | SFX_PLATE_ACTIVATE | 0:02 | No | 3D | 32.3 | (legacy) | REUSE → SFX_PLATE_ACTIVATE |
| `Efectos de sonido/PUERTA.mp3` (legacy) | Puzzle | SFX_DOOR_OPEN | 0:12 | No | 3D | 188.6 | (legacy) | REUSE → SFX_DOOR_OPEN (recortar) |
| `Resources/Audio/player/sfx_puzzle_success.mp3` | Puzzle | SFX_PUZZLE_SUCCESS | 0:02 | No | 2D | 48 | `2a3c69ee8e63d56478a159e2e5a04c27` | RESOLVED (ubicación `player/` — mover a `Puzzle/` en fase 5) |
| `Resources/Audio/player/sfx_puzzle_failure.mp3` | Puzzle | SFX_PUZZLE_FAILURE | 0:02 | No | 2D | 48 | `4cc50e0cce83fcd4f8520006eb34c184` | RESOLVED (ubicación `player/` — mover a `Puzzle/` en fase 5) |
| `Efectos de sonido/164843__plymouthjcliffords__old-fashioned-school-telephone-bell-ring.wav` (legacy) | Puzzle | SFX_TELEPHONE | 0:05 | No | 3D | 503.6 | (legacy) | REUSE → SFX_TELEPHONE |
| `Efectos de sonido/freesound_community-clock-chime-88027.mp3` (legacy) | Puzzle | SFX_CLOCK_CHIME | 0:04 | No | 3D | 89.1 | (legacy) | REUSE → SFX_CLOCK_CHIME (ya en GameFeel `PlayEerieChime`) |
| `Resources/Audio/player/sfx_button.mp3` | Puzzle | SFX_BUTTON_GENERIC | 0:01 | No | 3D | 32.5 | `37cc01f96fec7904db247518cdd31209` | RESOLVED (ubicación `player/` — mover a `Puzzle/` en fase 5) |

---

## 6. UI (ui_*) — Mixer: SFX_UI (sub-bus)

| Asset | Category | Event | Duración | Loop | Spatial | Size KB | GUID | Status |
|---|---|---|---|---|---|---|---|---|
| `Efectos de sonido/menu/Hover in click.mp3` (legacy) | UI | UI_MENU_HOVER_IN | 0:01 | No | 2D | 16.8 | (legacy) | REUSE (ya en MenuHoverSystem) |
| `Efectos de sonido/menu/Hover Out Clip.mp3` (legacy) | UI | UI_MENU_HOVER_OUT | 0:01 | No | 2D | 16.8 | (legacy) | REUSE (ya en MenuHoverSystem) |
| `Efectos de sonido/menu/Click Confirm Clip.mp3` (legacy) | UI | UI_MENU_CONFIRM | 0:01 | No | 2D | 16.8 | (legacy) | REUSE (ya en MenuHoverSystem) |
| `Efectos de sonido/menu/Nav Move Clip.mp3` (legacy) | UI | UI_NAV_MOVE | 0:01 | No | 2D | 16.8 | (legacy) | REUSE (ya en MenuHoverSystem) |
| `Efectos de sonido/menu/crt.mp3` (legacy) | UI | UI_CRT_HUM | 0:30 | Sí | 2D | 469.8 | (legacy) | REUSE (ya en MenuHoverSystem) |
| *(no asset)* | UI | UI_PAUSE_OPEN | — | — | 2D | — | — | MISSING — solo `ui_pause_close` existe |
| `Resources/Audio/ui/ui_pause_closemp3.mp3` | UI | UI_PAUSE_CLOSE | 0:01 | No | 2D | 32.5 | `978fd01ef12b83d419545b859559594d` | RESOLVED (nombre con doble `mp3` — renombrar a `ui_pause_close.mp3` en fase 5 SOLO si no hay refs) |
| *(no asset)* | UI | UI_CHOICE_HOVER | — | — | 2D | — | — | MISSING |
| *(no asset)* | UI | UI_CHOICE_CONFIRM | — | — | 2D | — | — | MISSING |
| `Resources/Audio/ui/ui_interaction_available.mp3` | UI | UI_INTERACTION_AVAILABLE | 0:01 | No | 2D | 32.5 | `505929860defa1e4fa148fd981ad0898` | RESOLVED |
| *(no asset)* | UI | UI_INTERACTION_CONFIRM | — | — | 2D | — | — | MISSING |
| `Resources/Audio/ui/ui_interaction_denied.mp3` | UI | UI_INTERACTION_DENIED | 0:01 | No | 2D | 32.5 | `a773a3035c51d3048a1b0541e0c10f12` | RESOLVED |
| `Resources/Audio/ui/ui_dialogue_open.mp3` | UI | UI_DIALOGUE_OPEN | 0:01 | No | 2D | 32.5 | `67fea059076fc2342879abd5fdeaa87d` | RESOLVED |
| `Resources/Audio/ui/ui_dialogue_advance.mp3` | UI | UI_DIALOGUE_ADVANCE | 0:01 | No | 2D | 32.5 | `68f85fa98d295ad408e5337dafa6f187` | RESOLVED |
| *(no asset)* | UI | UI_MODAL_CONFIRM | — | — | 2D | — | — | MISSING |
| `Resources/Audio/ui/ui_toast.mp3` | UI | UI_TOAST | 0:01 | No | 2D | 32.5 | `515ef4a76dfd6c4409a15224bfdaf9d7` | RESOLVED |

---

## 7. NARRATIVE (voice_lyra/aiden + memory) — Mixer: Voice

| Asset | Category | Event | Duración | Loop | Spatial | Size KB | GUID | Status |
|---|---|---|---|---|---|---|---|---|
| `Resources/Audio/VN/voice/lyra_line_001.wav` | Narrative | VOICE_LYRA_001 | 0:00 | No | 2D | 0 | `82aada34ab70e9a4b854659f7ec752a7` | PLACEHOLDER — 0 KB, necesita audio real |
| `Resources/Audio/VN/voice/lyra_line_002.wav` | Narrative | VOICE_LYRA_002 | 0:00 | No | 2D | 0 | `77d78dd9f8d9ac341bbfde157bbd2fd7` | PLACEHOLDER — 0 KB |
| `Resources/Audio/VN/voice/aiden_line_001.wav` | Narrative | VOICE_AIDEN_001 | 0:00 | No | 2D | 0 | `f4cceb4d475c0ad46bbbe202871618c9` | PLACEHOLDER — 0 KB |
| `Resources/Audio/VN/voice/aiden_line_002.wav` | Narrative | VOICE_AIDEN_002 | 0:00 | No | 2D | 0 | `002abba8b73f56f40a02a6aff28a82e4` | PLACEHOLDER — 0 KB |
| `Resources/Audio/narrative/sfx_memory_discovery.mp3` | Narrative | SFX_MEMORY_DISCOVERY | 0:02 | No | 2D | 48 | `725c53c11285a5c40b08f449433ac229` | RESOLVED |
| `Resources/Audio/narrative/amb_memory_whisper.mp3` | Narrative | AMB_MEMORY_WHISPER | 0:10 | Sí | 3D | 172.9 | `e0378bf6ecb55d24f9e5457e04c411ff` | RESOLVED |

---

## 8. TRANSITIONS / ENDING — Mixer: SFX_Foley / Music

| Asset | Category | Event | Duración | Loop | Spatial | Size KB | GUID | Status |
|---|---|---|---|---|---|---|---|---|
| `Resources/Audio/sfx_level_transition_out.mp3` | Transition | SFX_LEVEL_TRANSITION_OUT | 0:03 | No | 2D | 63.5 | `799efcf9920d6c94e99863c7cb2f8dc6` | RESOLVED |
| `Resources/Audio/sfx_level_transition_in.mp3` | Transition | SFX_LEVEL_TRANSITION_IN | 0:02 | No | 2D | 48 | `48adff0780c86974f9b24da03200b06c` | RESOLVED |
| `Resources/Audio/mus_credits.mp3` | Ending | MUS_CREDITS | 1:30 | Sí | 2D | 1423.1 | `ed3b9ce78bb451948bdec33bfe3dac6c` | RESOLVED (duplicado de §1, único uso) |

---

## 9. RESUMEN DE COBERTURA

### 9.1 Totales contra el mapa de eventos §3 (directiva)

| Categoría | Eventos requeridos | RESOLVED + REUSE | MISSING | UNRESOLVED | PLACEHOLDER |
|---|---|---|---|---|---|
| MUSIC | 8 | 7 (+1 REUSE menu) | 1 (exploration texture layer) | 0 (tension es UNRESOLVED pero con clip) | 0 |
| AMBIENCE | 8 | 8 (todos REUSE legacy) | 0 | 0 | 1 (room_tone.wav placeholder) |
| PLAYER | 8 | 7 | 1 (gravity_shift) | 0 | 0 |
| ECHO | 7 | 6 (5 REUSE + 1 RESOLVED) | 1 (despawn stinger) | 0 | 0 |
| PUZZLE | 7 | 7 (4 REUSE + 3 RESOLVED) | 0 | 0 | 0 |
| UI | 16 | 13 (5 REUSE + 8 RESOLVED) | 3 (pause_open, choice_hover, choice_confirm, interaction_confirm, modal_confirm) — ver nota | 0 | 0 |
| NARRATIVE | 6 | 2 (sfx_memory_discovery, amb_memory_whisper) | 0 | 0 | 4 (voces 0 KB) |
| TRANSITIONS | 3 | 3 | 0 | 0 | 0 |

**Totales: 63 eventos en el mapa. 53 con clip asignable. 6 MISSING. 1 UNRESOLVED (tension drone). 4 PLACEHOLDER (voces). 1 PLACEHOLDER (room_tone).**

Nota UI MISSING: `ui_pause_open`, `ui_choice_hover`, `ui_choice_confirm`, `ui_interaction_confirm`, `ui_modal_confirm` — 5 ausentes (corregido conteo: son 5, no 3). Degradación elegante para estos.

### 9.2 Regla de no asignación automática aplicada

Ningún clip con `STATUS = UNRESOLVED` o `PLACEHOLDER` se asigna automáticamente. El sistema runtime (`EnsureRuntimeFallbackAudio` procedural + null-check skip) maneja la ausencia sin errores.

### 9.3 Import settings propuestos (fase 6 — no aplicados todavía)

| Categoría | Load Type | Background Load | Bitrate | Spatial | Notas |
|---|---|---|---|---|---|
| UI (ui_*.mp3, ≤1s) | Decompress On Load | Preload Audio Data: Sí | Vorbis/PCM | 2D, Mono (si mono source) | Sin silencio inicial |
| SFX 2D (death, respawn, success, failure) | Decompress On Load | Preload: Sí | Vorbis | 2D | — |
| SFX 3D (jump, landing, scrape, plate, door, button) | Decompress On Load | Preload: Sí | Vorbis | 3D, min 1m / max 18m log | Rolloff corregido en EchoPlayback |
| Echo loop + voice | Compressed In Memory | Preload: No (stream-like) | Vorbis | 3D | Bandpass en bus SFX_Echo |
| Música (mus_*) | Streaming | Preload: No | Vorbis, ~96-128 kbps | 2D, Stereo | Loop según §1 |
| Ambience loops (Ventilation, Industrial) | Streaming o Compressed In Memory | Preload: según ram | Vorbis | 2D/3D según §2 | Loop Sí |
| Voice (lyra/aiden) | Decompress On Load | Preload: Sí | Vorbis | 2D | Placeholder 0 KB hasta re-export |

---

## 10. NOTAS DE UBICACIÓN / REORGANIZACIÓN (fase 5 — no ejecutada todavía)

- Los archivos en `Resources/Audio/player/` incluyen clips de Puzzle (`sfx_puzzle_success`, `sfx_puzzle_failure`, `sfx_button`). Propuesta no destructiva: en la fase 5, copiar (no mover) a `Resources/Audio/Puzzle/` preservando `.meta` GUID; las refs por GUID sobreviven al movimiento SIEMPRE que el `.meta` acompañe. **No ejecutar hasta tener Unity vivo para re-resolver refs.**
- El clip `01-mus_exploration_dronewavduration-30-secondsseamless-loopno_081626.mp3` se renombra lógicamente a `mus_exploration_drone.wav` en la fase 5; el nombre físico se mantiene hasta verificar refs (Unity repunta por GUID, no por nombre, salvo `Resources.Load` por string — **ningún script usa `Resources.Load` con este nombre**, verificable).
- `seamless-loopno-melodypromptcreate-a-dark_081626.mp3` queda `UNRESOLVED` hasta confirmación aural. NO renombrar/asignar hasta entonces.

---

## CROSS REFERENCES
- `Docs/Audit/AUDIO_INTEGRATION_AUDIT.md` `[AUDIT-AUD-002]` — auditoría pre-integración
- `Docs/Specs/AUDIO_DIRECTION.md` `[SPEC-AUD-DIR-001]` §8 — asset manifest canónico (cross-check source)
- `Docs/Specs/AUDIO_GRAMMAR.md` `[SPEC-112]`
- `Docs/Specs/AUDIO_MIXER_SCHEMA.md` `[SPEC-127]`

## CHANGE HISTORY
- **v1.0 (2026-08-16)**: Inventario inicial de los recursos en `Assets/Resources/Audio/` + legacy `Assets/Efectos de sonido/`. Catalogados 63 eventos del mapa, con GUID, duración, tamaño, canales, mixer target y estado. Aplicada regla de no-asignación automática para UNRESOLVED/PLACEHOLDER.
