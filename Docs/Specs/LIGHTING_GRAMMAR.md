# LIGHTING_GRAMMAR.md — URP Lighting, Fog & Post-Processing Specifications
## Spec ID: SPEC-105
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Specifies technical URP lighting parameters, shadow settings, fog density volumes, and post-processing profiles in *Echoes of You 2.0*. Ensures 100% compliance with PS1/PS2 ambient rendering (`AmbientMode.Flat`, ambient intensity `0.15 Lux`, hard shadow filter radius `0.0m`, max shadow distance `40.0m`).

### 2. SCOPE
Applies to `LevelLightingSettings.cs`, `EchoesLevelEnvironmentBootstrap.cs`, URP Volume assets, and light components. Excludes 3D mesh geometry.

### 3. AUTHORITY
Level 4 (Declarative Specs). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `DESIGN_PHILOSOPHY.md` (`SPEC-001`). Runtime data contract defined in `Docs/ExecutableSpecs/catalogs/lighting_profiles.yaml` (`SPEC-EXEC-LGT`).

### 4. DEFINITIONS
- `Flat Ambient Lighting`: URP `AmbientMode.Flat` setting with fixed intensity `0.15 Lux` without complex baked global illumination.
- `Hard Shadows`: Non-filtered shadow map projection (`shadowFilterRadius = 0.0m`, `40.0m` max distance, 2 cascades).
- `Light Cap`: Maximum allowable active light components per scene ($48$ max).

### 5. INPUTS
- [DESIGN_PHILOSOPHY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/DESIGN_PHILOSOPHY.md) `[SPEC-001]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`
- [CONSTANTS_REGISTRY.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CONSTANTS_REGISTRY.yaml) `[SPEC-124]`

### 6. OUTPUTS
- URP Volume Profiles and `LevelLightingSettings.cs` components.
- Lighting validation assertions for `LevelValidator.cs`.

### 7. RULES
- `[RULE-LGT-001]`: **Ambient & Shadow Mode Constraint** — `RenderSettings.ambientMode` MUST equal `AmbientMode.Flat`. Shadow type MUST equal `Hard Shadows` with `shadowDistance = 40.0m` and 2 cascades.
- `[RULE-LGT-002]`: **Per-Scene Light Ceiling** — Total active light components (`Light`) per scene MUST NOT exceed $48$ points for performance.
- `[RULE-LGT-003]`: **Sun Light Alignment** — Main directional light intensity MUST equal $I_{sun} = 0.85\text{ Lux}$, color `#F2F2FF`, rotation `Vector3(50.0, -30.0, 0.0)`.
- `[RULE-LGT-004]`: **Post-Processing Profile Parameters** — Defined in `lighting_profiles.yaml`. Chapter-specific fog/ambient in `lighting_profiles.yaml#chapters`.
- `[RULE-LGT-005]`: **Fog Density per Chapter** — $D_{fog}$ values are primitive in `CONSTANTS_REGISTRY.yaml` (`fog_density_chapter_I = 0.008`). Chapter profiles in YAML reference this primitive.

### 8. ALGORITHMS
Chapter Lighting Tokens, Fog Matrix, and Post-Processing parameters are defined in `lighting_profiles.yaml`. The Markdown document does not duplicate numeric tables.

### 9. CONSTRAINTS
- `[CONS-LGT-001]`: Prohibido using `Shader.Find("Standard")` or Built-in render pipeline tags.
- `[CONS-LGT-002]`: Prohibido exceeding 48 active lights per scene.
- `[CONS-LGT-003]`: Prohibido soft shadows (`LightShadows.Soft`).

### 10. VALIDATION
- `[VAL-LGT-001]`: `LevelValidator.cs` parses scene lights and asserts count $\le 48$ (`FAIL-LGT-01`).
- `[VAL-LGT-002]`: `LevelValidator.cs` asserts all lights use `LightShadows.Hard` (`FAIL-LGT-02`).
- `[VAL-LGT-003]`: `LevelValidator.cs` asserts ambient mode is `Flat` and fog density matches chapter profile.

### 11. EXAMPLES
See `lighting_profiles.yaml` for chapter-specific fog colors, ambient colors, and post-processing parameters.

### 12. FAILURE CASES
- `[FAIL-LGT-001]`: **Light Count Overflow** — Scene contains 50 lights. Result: Validator logs `FAIL-LGT-01` and halts build.
- `[FAIL-LGT-002]`: **Soft Shadows Detected** — Light uses `LightShadows.Soft`. Result: Validator logs `FAIL-LGT-02`.

### 13. CROSS REFERENCES
- [lighting_profiles.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/catalogs/lighting_profiles.yaml) `[SPEC-EXEC-LGT]`
- [CONSTANTS_REGISTRY.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CONSTANTS_REGISTRY.yaml) `[SPEC-124]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec creation for URP shaders.
- **v4.0 (2026-07-25)**: Moved numeric tables to `lighting_profiles.yaml`. Replaced "Cool Blue" prose with exact RGBA values. Linked fog density to CONSTANTS_REGISTRY primitive.

(End of file - total 68 lines)