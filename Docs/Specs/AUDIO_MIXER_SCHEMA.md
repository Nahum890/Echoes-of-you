# AUDIO_MIXER_SCHEMA.md — Audio Mixer Bus Hierarchy & DSP Filtering
## Spec ID: SPEC-127
## Version: 2.0 (AI-Executable)

---

### 1. PURPOSE
Defines the canonical Unity AudioMixer bus layout, exposed volume parameters, DSP bandpass filters, and spatial audio rolloff curves for *Echoes of You 2.0*.

### 2. SCOPE
Applies to `Assets/Audio/EchoesAudioMixer.mixer` and all `AudioSource` components attached to Player, Echo, puzzle wires, tape recorders, and environmental ambient emitters.

### 3. AUTHORITY
Level 4 (Declarative Specs). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `AUDIO_GRAMMAR.md` (`SPEC-112`). Runtime data contract defined in `Docs/ExecutableSpecs/audio/audio_dsp_spec.yaml` (`SPEC-EXEC-AUD-DSP`).

### 4. DEFINITIONS
- `DSP Bandpass`: Low-pass ($3500\text{Hz}$) and High-pass ($300\text{Hz}$) filter combination applied to Echo playback to mimic PS1 cassette audio quality.
- `BGM Ducking`: Automatic gain reduction (from $-6.0\text{dB}$ to $-12.0\text{dB}$) when player enters an active puzzle zone.

### 5. INPUTS
- [AUDIO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/AUDIO_GRAMMAR.md) `[SPEC-112]`
- [audio_dsp_spec.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/audio/audio_dsp_spec.yaml) `[SPEC-EXEC-AUD-DSP]`

### 6. OUTPUTS
- Configured Unity AudioMixer asset and runtime volume parameter bindings.

### 7. RULES
- `[RULE-MIX-001]`: **Tape Hiss Constant Bus** — `TapeHiss` bus MUST remain active and un-muted during gameplay at default $-18.0\text{dB}$.
- `[RULE-MIX-002]`: **Echo DSP Restriction** — `SFX_Echo` MUST pass through $300\text{Hz}-3500\text{Hz}$ bandpass filter to maintain cassette aesthetic.
- `[RULE-MIX-003]`: **Spatial Rolloff Range** — 3D spatial audio roll-off MUST use Logarithmic curve with `min_distance = 1.0m` and `max_distance = 18.0m`.
- `[RULE-MIX-004]`: **Ducking Formula** — Gain transition follows exponential approach: `gain = 1.0 - (1.0 - target_gain) * (1 - exp(-t / tau))` with $\tau_{attack} = 0.05\text{s}$, $\tau_{decay} = 0.2\text{s}$.
- `[RULE-MIX-005]`: **Hardcoded Volume Prohibition** — All volume values MUST be controlled via AudioMixer exposed parameters; no `AudioSource.volume` direct assignment in gameplay code.

### 8. ALGORITHMS
Bus hierarchy, DSP parameters, and ducking equations are defined in `audio_dsp_spec.yaml`. The Markdown document does not duplicate numeric tables.

#### Ducking Exponential Formula
$$ \text{gain}(t) = 1.0 - (1.0 - G_{target}) \cdot (1 - e^{-t / \tau}) $$

| Phase | $\tau$ (seconds) | Description |
|-------|------------------|-------------|
| Attack | $0.05$ | Fast ducking when entering puzzle zone |
| Decay | $0.2$ | Smooth return when exiting puzzle zone |

### 9. CONSTRAINTS
- `[CONS-MIX-001]`: Prohibido hardcoding volume values outside the AudioMixer parameter bindings.

### 10. VALIDATION
- `[VAL-MIX-001]`: `LevelValidator.cs` asserts `EchoesAudioMixer.mixer` exists and contains all required exposed parameters.
- `[VAL-MIX-002]`: `ExecutableSpecValidator.cs` asserts bandpass frequencies match `audio_dsp_spec.yaml`.

### 11. EXAMPLES
See `audio_dsp_spec.yaml` for canonical bus hierarchy and DSP configuration.

### 12. FAILURE CASES
- `[FAIL-MIX-001]`: **Missing Mixer Parameter** — Exposed volume parameter name mismatch. Result: `FAIL-AUD-01`.

### 13. CROSS REFERENCES
- [AUDIO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/AUDIO_GRAMMAR.md) `[SPEC-112]`
- [audio_dsp_spec.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/audio/audio_dsp_spec.yaml) `[SPEC-EXEC-AUD-DSP]`

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Created canonical SPEC-127 for Audio Mixer hierarchy.
- **v2.0 (2026-07-25)**: Added exact DSP ducking formula with $\tau$ values. Moved numeric bus/DSP parameters to `audio_dsp_spec.yaml`. Added cross-reference to ExecutableSpec YAML.

(End of file - total 58 lines)