# AUDIO_GRAMMAR.md — Audio Architecture & Signal Processing Specifications
## Spec ID: SPEC-112
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Specifies the technical sound design architecture, DSP bandpass filter cutoffs, spatial attenuation curves, tape hiss noise profiles, and event triggers for *Echoes of You 2.0* via `EchoesAudioManager.cs`.

### 2. SCOPE
Applies to `EchoesAudioManager.cs`, Unity AudioMixer assets, AudioSource components, and spatial sound triggers. Excludes visual shader rendering.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `DESIGN_PHILOSOPHY.md` (`SPEC-001`).

### 4. DEFINITIONS
- `Liminal Bandpass Filter`: AudioMixer DSP filter applying Highpass $300\text{Hz}$ and Lowpass $3500\text{Hz}$ to simulate lo-fi retro audio.
- `Tape Hiss Profile`: Continuous ambient background hiss audio stream at volume $-24.0\text{dB}$.
- `Spatial Attenuation`: Logarithmic distance roll-off curve for 3D sound emitters ($MinDist=1.0\text{m}, MaxDist=18.0\text{m}$).

### 5. INPUTS
- [DESIGN_PHILOSOPHY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/DESIGN_PHILOSOPHY.md) `[SPEC-001]`
- [PROJECT_CONTEXT.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Technical/PROJECT_CONTEXT.md) `[SPEC-110]`

### 6. OUTPUTS
- AudioMixer snapshot states driven by `EchoesAudioManager.cs`.
- Audio assertions for `LevelValidator.cs`.

### 7. RULES

- `[RULE-AUD-001]`: **Liminal Bandpass Filter Assignment**: The master AudioMixer output MUST route through a Bandpass filter with Highpass cutoff $F_{high} = 300\text{ Hz} \pm 0.0$ and Lowpass cutoff $F_{low} = 3500\text{ Hz} \pm 0.0$.
- `[RULE-AUD-002]`: **Tape Hiss Ambience**: All gameplay levels MUST play a continuous tape hiss loop at volume $V_{hiss} = -24.0\text{ dBFS}$.
- `[RULE-AUD-003]`: **Echo Recording Audio Cue**: Activating Echo recording MUST trigger a high-pitch tape rewind sound effect at pitch $1.25$ and duration $0.3\text{s}$.
- `[RULE-AUD-004]`: **Spatial Sound Attenuation**: 100% of in-world 3D AudioSources MUST set `spatialBlend = 1.0` (3D), `minDistance = 1.0m`, `maxDistance = 18.0m`, and `rolloffMode = Logarithmic`.
- `[RULE-AUD-005]`: **3D Spatial Audio Attenuation Curve (HALT-10)**: Every 3D AudioSource emitter in gameplay scenes MUST enforce the logarithmic attenuation parameters defined in Algorithm 8.2. Reverb zones and doppler shifts are strictly disabled ($0.0$).

### 8. ALGORITHMS

#### Table 8.1: Master Audio Event Catalog

| Event Key | Trigger Source | Sound Asset | Pitch | Volume (dBFS) | Spatial Blend |
|---|---|---|---|---|---|
| `SND_ECHO_RECORD_START` | `EchoRecorder.StartRecording()` | `sfx_tape_rewind.wav` | `1.25` | `-6.0 dB` | `0.0` (2D) |
| `SND_ECHO_PLAYBACK_LOOP` | `EchoPlayback.StartPlayback()` | `sfx_echo_hum.wav` | `1.00` | `-12.0 dB` | `1.0` (3D) |
| `SND_PLATE_ACTIVATE` | `PressurePlate.OnTriggerEnter()`| `sfx_plate_click.wav` | `0.95` | `-8.0 dB` | `1.0` (3D) |
| `SND_DOOR_OPEN` | `DoorController.OpenDoor()` | `sfx_door_slide.wav` | `0.90` | `-10.0 dB` | `1.0` (3D) |
| `SND_SOFT_RESET` | `LevelRuntimeController.SoftReset()`| `sfx_tape_stop.wav` | `0.80` | `-4.0 dB` | `0.0` (2D) |

#### Algorithm 8.2: 3D Spatial Audio Attenuation Curve Spec (HALT-10)

```yaml
# [RULE-AUD-005] Curva de Atenuación de Audio 3D
spatial_audio_config:
  rolloff_mode: "Logarithmic"
  min_distance_m: 1.0
  max_distance_m: 18.0
  spatial_blend: 1.0        # 0.0 = 2D puro, 1.0 = 3D puro
  reverb_zone_mix: 0.0      # sin reverb zones — el DSP bandpass simula el espacio
  doppler_level: 0.0        # sin doppler
  spread_degrees: 0.0
  # Curva logarítmica: V(d) = minDistance / max(minDistance, d)
  # A 1.0m: volumen 100%. A 9.0m: ~11%. A 18.0m: ~6%.
```

### 9. CONSTRAINTS
- `[CONS-AUD-001]`: Prohibido sudden un-attenuated volume spikes $> 0.0\text{dBFS}$.
- `[CONS-AUD-002]`: Prohibido orchestral or upbeat music tracks; background audio MUST remain atmospheric ambient drone.

### 10. VALIDATION
- `[VAL-AUD-001]`: `LevelValidator.cs` parses scene AudioSources and asserts 100% of in-world emitters have `maxDistance <= 18.0f`.
- `[VAL-AUD-002]`: AudioMixer inspector asserts `Bandpass` filter parameters match $300\text{Hz}$ and $3500\text{Hz}$.

### 11. EXAMPLES

#### Example 11.1: AudioSource Setup in C#
```csharp
AudioSource source = gameObject.AddComponent<AudioSource>();
source.spatialBlend = 1.0f; // 3D
source.minDistance = 1.0f;
source.maxDistance = 18.0f;
source.rolloffMode = AudioRolloffMode.Logarithmic;
```

### 12. FAILURE CASES
- `[FAIL-AUD-001]`: **2D Sound Leak**: In-world door slide sound set to 2D spatial blend. Result: `LevelValidator` flags `FAIL-AUD-01`.

### 13. CROSS REFERENCES
- [DESIGN_PHILOSOPHY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/DESIGN_PHILOSOPHY.md) `[SPEC-001]`
- [PROJECT_CONTEXT.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Technical/PROJECT_CONTEXT.md) `[SPEC-110]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec creation for audio architecture.
- **v4.0 (2026-07-25)**: HALT-10 resolved — added RULE-AUD-005 and spatial_audio_config 3D attenuation curve spec (Algorithm 8.2).
