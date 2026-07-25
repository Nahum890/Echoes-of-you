# AUDIO_MIXER_SCHEMA.md — Audio Mixer Bus Hierarchy & DSP Filtering
## Spec ID: SPEC-127
## Version: 1.0 (AI-Executable)

---

### 1. PURPOSE
Defines the canonical Unity AudioMixer bus layout, exposed volume parameters, DSP bandpass filters, and spatial audio rolloff curves for *Echoes of You 2.0*.

### 2. SCOPE
Applies to `Assets/Audio/EchoesAudioMixer.mixer` and all `AudioSource` components attached to Player, Echo, puzzle wires, tape recorders, and environmental ambient emitters.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `AUDIO_GRAMMAR.md` (`SPEC-112`).

### 4. DEFINITIONS
- `DSP Bandpass`: Low-pass ($3500\text{Hz}$) and High-pass ($300\text{Hz}$) filter combination applied to Echo playback to mimic PS1 cassette audio quality.
- `BGM Ducking`: Automatic gain reduction (from $-6.0\text{dB}$ to $-12.0\text{dB}$) when player enters an active puzzle zone.

### 5. INPUTS
- [AUDIO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/AUDIO_GRAMMAR.md) `[SPEC-112]`
- [audio_mixer_schema.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/audio/audio_mixer_schema.yaml) `[SPEC-EXEC-AUD]`

### 6. OUTPUTS
- Configured Unity AudioMixer asset and runtime volume parameter bindings.

### 7. RULES
- `[RULE-MIX-001]`: **Tape Hiss Constant Bus**: `TapeHiss` bus MUST remain active and un-muted during gameplay at default $-18.0\text{dB}$.
- `[RULE-MIX-002]`: **Echo DSP Restriction**: `SFX_Echo` MUST pass through $300\text{Hz}-3500\text{Hz}$ bandpass filter to maintain cassette aesthetic.
- `[RULE-MIX-003]`: **Spatial Rolloff Range**: 3D spatial audio roll-off MUST use Logarithmic curve with `min_distance = 1.0m` and `max_distance = 18.0m`.

### 8. ALGORITHMS
See canonical YAML schema at `Docs/ExecutableSpecs/audio/audio_mixer_schema.yaml`.

### 9. CONSTRAINTS
- `[CONS-MIX-001]`: Prohibido hardcoding volume values outside the AudioMixer parameter bindings.

### 10. VALIDATION
- `[VAL-MIX-001]`: `LevelValidator.cs` asserts `EchoesAudioMixer.mixer` exists and contains all required exposed parameters.

### 11. EXAMPLES
- `audio_mixer_schema.yaml` schema definition.

### 12. FAILURE CASES
- `[FAIL-MIX-001]`: **Missing Mixer Parameter**: Exposed volume parameter name mismatch. Result: `FAIL-AUD-01`.

### 13. CROSS REFERENCES
- [AUDIO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/AUDIO_GRAMMAR.md) `[SPEC-112]`

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Created canonical SPEC-127 for Audio Mixer hierarchy.
