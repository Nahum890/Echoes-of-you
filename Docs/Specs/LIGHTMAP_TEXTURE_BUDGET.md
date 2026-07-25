# LIGHTMAP_TEXTURE_BUDGET.md — GPU Lightmap VRAM Budgets & Light Probe Placement
## Spec ID: SPEC-132
## Version: 1.0 (AI-Executable)

---

### 1. PURPOSE
Defines maximum lightmap texture atlas resolutions, BC6H compression formats, directional modes, light probe grid densities, and VRAM memory budgets per level size tier.

### 2. SCOPE
Applies to Unity `LightingSettings.asset` and lightmap baking passes during level generation.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `LIGHTMAP_PROBE_SPEC.md` (`SPEC-122`).

### 4. DEFINITIONS
- `NonDirectional`: Single lightmap atlas per scene capturing flat illumination matching PS1 static lighting style.
- `VRAM Budget`: Max VRAM allocated to baked lightmaps ($16\text{MB}$ small, $32\text{MB}$ medium, $48\text{MB}$ large).

### 5. INPUTS
- [LIGHTMAP_PROBE_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTMAP_PROBE_SPEC.md) `[SPEC-122]`
- [LIGHTMAP_TEXTURE_BUDGET.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTMAP_TEXTURE_BUDGET.yaml) `[SPEC-132]`

### 6. OUTPUTS
- Configured Lightmap settings asset and baked lightmap textures.

### 7. RULES
- `[RULE-LMAP-001]`: **Max Lightmap Size**: `max_lightmap_size_px` MUST NOT exceed 1024px.
- `[RULE-LMAP-002]`: **Directional Mode**: Directional mode MUST be set to `NonDirectional` (PS1 aesthetic requirement).
- `[RULE-LMAP-003]`: **BC6H HDR Compression**: Lightmap compression MUST be enabled using `BC6H` format.

### 8. ALGORITHMS
See canonical configuration at `Docs/Specs/LIGHTMAP_TEXTURE_BUDGET.yaml`.

### 9. CONSTRAINTS
- `[CONS-LMAP-001]`: Prohibido real-time directional lightmap baking during scene execution.

### 10. VALIDATION
- `[VAL-LMAP-001]`: `LevelValidator.cs` asserts total baked lightmap memory does not exceed VRAM tier budget.

### 11. EXAMPLES
- `LIGHTMAP_TEXTURE_BUDGET.yaml` configuration.

### 12. FAILURE CASES
- `[FAIL-LMAP-001]`: **Lightmap Budget Overflow**: Level 01 lightmaps exceeding 16MB VRAM budget. Result: `FAIL-LMAP-01`.

### 13. CROSS REFERENCES
- [LIGHTMAP_PROBE_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTMAP_PROBE_SPEC.md) `[SPEC-122]`

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Created canonical SPEC-132 for Lightmap Texture Budgets.
