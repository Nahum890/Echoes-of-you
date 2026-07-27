# POST_PROCESSING_SPEC.md — URP Volume Profiles & Retro Visual Stack Specifications
## Spec ID: SPEC-120
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Defines exact numerical parameters for Universal Render Pipeline (URP) Volume Profiles, Bloom, Vignette, Color Adjustments, and Tonemapping overrides for *Echoes of You 2.0*.

### 2. SCOPE
Applies to URP Volume Profile asset `Assets/Settings/Volumes/GlobalVolume.asset`, `VolumeProfile` components, and global post-processing settings.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `LIGHTING_GRAMMAR.md` (`SPEC-105`). Replaces `SPEC-204`.

### 4. DEFINITIONS
- `Liminal Visual Stack`: Post-processing settings maintaining flat PS1 retro colors without aggressive tonemapping or bloom blur.

### 5. INPUTS
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`

### 6. OUTPUTS
- Configured URP Volume Profile asset `GlobalVolume.asset`.

### 7. RULES

- `[RULE-PST-001]`: **Volume Profile Path**: Global Volume Profile MUST exist at `Assets/Settings/Volumes/GlobalVolume.asset`.
- `[RULE-PST-002]`: **Bloom Parameters**: Bloom intensity MUST equal $0.25$, threshold MUST equal $0.90$, scatter MUST equal $0.70$, tint `#FFFFFF`, `highQualityFiltering = false` (OFF for PS1 aesthetic).
- `[RULE-PST-003]`: **Vignette & Color Adjustments**: Vignette intensity MUST equal $0.35$, smoothness $0.40$, color `#0D0D1A`. Color Adjustments MUST set `postExposure = -0.5`, `contrast = 15.0`, `saturation = -8.0`, `hueShift = 0.0`.
- `[RULE-PST-004]`: **Tonemapping Disabled**: Tonemapping mode MUST equal `None` to preserve flat HSL color palettes.

### 8. ALGORITHMS

#### Algorithm 8.1: URP Volume Profile Parameters Schema

```yaml
urp_volume_profile: "Assets/Settings/Volumes/GlobalVolume.asset"
bloom:
  intensity: 0.25
  threshold: 0.90
  scatter: 0.70
  tint: "#FFFFFF"
  high_quality_filtering: false   # OFF para estética PS1
vignette:
  intensity: 0.35
  smoothness: 0.40
  color: "#0D0D1A"
  rounded: false
color_adjustments:
  post_exposure: -0.5
  contrast: 15.0
  color_filter: "#FFFFFF"
  hue_shift: 0.0
  saturation: -8.0
tonemapping:
  mode: "None"      # Sin tonemapping — mantiene colores planos del PS1
```

### 9. CONSTRAINTS
- `[CONS-PST-001]`: Prohibido enabling ACES or Neutral Tonemapping modes.

### 10. VALIDATION
- `[VAL-PST-001]`: `LevelValidator.cs` parses `GlobalVolume.asset` and asserts `Bloom.intensity == 0.25f`.

### 11. EXAMPLES
- Volume schema above.

### 12. FAILURE CASES
- `[FAIL-PST-001]`: **Tonemapping Enabled**: Tonemapping set to ACES causing color wash out. Result: `FAIL-PST-01`.

### 13. CROSS REFERENCES
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`

### 14. CHANGE HISTORY
- **v4.0 (2026-07-25)**: Created canonical SPEC-120 upgrading SPEC-204 with complete URP Volume Profile parameters.
