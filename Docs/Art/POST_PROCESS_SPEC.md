# POST_PROCESS_SPEC.md — Canonical Post-Processing Implementation Specification
## Spec ID: SPEC-144
## Version: 1.0 (AI-Executable)

---

### 1. PURPOSE
Defines the **single binding post-processing stack** for all 15 levels of *Echoes of You 2.0* and resolves the contradiction between `POST_PROCESSING_SPEC.md` (`SPEC-120`) and `Docs/ExecutableSpecs/visual/urp_volume_profiles.yaml` (registered as `CC-2026-012` in `CHANGE_CONTROL.md`).

### 2. SCOPE
Applies to URP global volumes in `Level_01..15`, the scene profiles `Slice_N01..N03_PostProc.asset`, the empty `N04/N05_PostProc.asset`, the runtime volume created by `Assets/Scripts/PostProcessingSetup.cs`, and the camera flag `renderPostProcessing`.

### 3. AUTHORITY
Level 3 (Art Implementation). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `POST_PROCESSING_SPEC.md` (`SPEC-120`, canonical numeric source). Chapter fog/ambient remain in `lighting_profiles.yaml` per `RULE-LGT-004/005`.

### 4. DEFINITIONS
- `Canonical Stack`: the exact parameter set of `SPEC-120` (RULE-PST-002/003/004).
- `Analog Layer`: Chromatic Aberration + Film Grain owned by `GameFeelController` (event pulses), the only permitted addition to the Canonical Stack.
- `Global Post Volume`: a global `Volume` (weight 1.0) present in every level scene with the Canonical Stack profile.

### 5. INPUTS
- [POST_PROCESSING_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/POST_PROCESSING_SPEC.md) `[SPEC-120]`
- [CHANGE_CONTROL.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Authority/CHANGE_CONTROL.md) `CC-2026-012`
- `Assets/Scripts/GameFeelController.cs` (pulse baselines)

### 6. OUTPUTS
- One canonical `VolumeProfile` asset shared by the 15 global volumes.
- `PostProcessingSetup.cs` aligned 1:1 with the Canonical Stack.
- `urp_volume_profiles.yaml` updated to match (see RULE-PST-G06).

### 7. RULES

- `[RULE-PST-G01]`: **Canonical Stack** (binding, per `SPEC-120`):
  - Bloom: `intensity 0.25`, `threshold 0.90`, `scatter 0.70`, `tint #FFFFFF`, `highQualityFiltering = false`.
  - Vignette: `intensity 0.35`, `smoothness 0.40`, `color #0D0D1A`, `rounded = false`.
  - Color Adjustments: `postExposure -0.5`, `contrast 15.0`, `saturation -8.0`, `hueShift 0.0`, `colorFilter #FFFFFF`.
  - Tonemapping: `mode = None`.
- `[RULE-PST-G02]`: **Every level scene MUST have exactly one Global Post Volume** referencing the canonical profile. Current gaps to fix: `N04/N05` profiles are empty (0 effects) and `Level_06..15` have no volume at all.
- `[RULE-PST-G03]`: **No per-chapter post variation.** Per-chapter identity comes exclusively from lighting (fog/ambient per `lighting_profiles.yaml`). The divergent saturation (`+2/+4/+8`), exposure (`-0.1/-0.3`), vignette (`0.25/0.30`) and colorFilter of `Slice_N01..03` are legacy and MUST be replaced.
- `[RULE-PST-G04]`: **Camera flag**: Main Camera `renderPostProcessing = true` in all levels (currently verified true in Level_02).
- `[RULE-PST-G05]`: **Runtime volume** (`PostProcessingSetup.cs`) MUST match the Canonical Stack exactly (currently: vignette 0.2, exp 0.0, contrast 10, saturation −5 → out of spec). Only the Analog Layer may deviate: `ChromaticAberration.intensity 0.12` base and `FilmGrain (Medium1, 0.45)` as event-driven artifacts, returned to baseline by `GameFeelController` pulses (baselines already match SPEC-120 for vignette/saturation/exposure).
- `[RULE-PST-G06]`: **YAML alignment**: `urp_volume_profiles.yaml` MUST be rewritten to the Canonical Stack values with a header citing `CC-2026-012`; its `fog_settings` block (Linear `#0F141A` 0.015, 5–35 m) is **deleted** — fog is owned by chapter profiles (`lighting_profiles.yaml`), not by post.

### 8. ALGORITHMS

#### Algorithm 8.1: Canonical Stack Schema (single source)
```yaml
# CC-2026-012: SPEC-120 wins over urp_volume_profiles.yaml v1.0
bloom:      { active: true, intensity: 0.25, threshold: 0.90, scatter: 0.70, tint: "#FFFFFF", high_quality_filtering: false }
vignette:   { active: true, intensity: 0.35, smoothness: 0.40, color: "#0D0D1A", rounded: false }
color_adj:  { active: true, post_exposure: -0.5, contrast: 15.0, saturation: -8.0, hue_shift: 0.0, color_filter: "#FFFFFF" }
tonemapping:{ mode: "None" }
analog_layer: { chromatic_aberration: 0.12, film_grain: { type: "Medium1", intensity: 0.45 } }  # GameFeel-owned only
```

#### Algorithm 8.2: Scene Adoption
```text
1. Create/assign canonical profile to the global volume of each Level_01..15.
2. Delete or empty the legacy Slice/N04/N05 profiles (kept for reference only).
3. Align PostProcessingSetup.cs runtime volume to the Canonical Stack.
4. Validate: VAL-PST-G01.
```

### 9. CONSTRAINTS
- `[CONS-PST-001]`: Prohibido enabling ACES or Neutral Tonemapping.
- `[CONS-PST-G01]`: No additional effects (LensDistortion, DepthOfField, etc.) without change control.

### 10. VALIDATION
- `[VAL-PST-G01]`: All 15 scenes: exactly 1 global volume, profile == Canonical Stack (assert bloom 0.25, vignette 0.35/#0D0D1A, saturation −8, exposure −0.5, tonemapping None); camera post enabled.

### 11. EXAMPLES
- Level_02: `Slice_GlobalVolume` → profile with Canonical Stack (replaces Slice_N02 legacy values).

### 12. FAILURE CASES
- `[FAIL-PST-G01]`: Volume missing or empty profile in any level (current L04–L15) → fail.
- `[FAIL-PST-G02]`: Runtime volume values differ from SPEC-120 → fail.

### 13. CROSS REFERENCES
- [POST_PROCESSING_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/POST_PROCESSING_SPEC.md) `[SPEC-120]`
- [GLOBAL_LIGHTING_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Art/GLOBAL_LIGHTING_SPEC.md) `[SPEC-143]`
- [CHANGE_CONTROL.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Authority/CHANGE_CONTROL.md) `CC-2026-012`

### 14. CHANGE HISTORY
- **v1.0 (2026-08-20)**: Created; resolves CONT-012 (SPEC-120 vs urp_volume_profiles.yaml) and codifies the Analog Layer policy.