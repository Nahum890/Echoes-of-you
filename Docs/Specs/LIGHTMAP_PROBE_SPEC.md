# LIGHTMAP_PROBE_SPEC.md — Lightmapping & Light Probe Baking Specifications
## Spec ID: SPEC-122
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Defines lightmap baking parameters, resolution density, Light Probe placement grid rules, and Reflection Probe restrictions for *Echoes of You 2.0*.

### 2. SCOPE
Applies to Unity Progressive GPU Lightmapper, Light Probe Groups, and scene lighting baking pipelines.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `LIGHTING_GRAMMAR.md` (`SPEC-105`).

### 4. DEFINITIONS
- `Subtractive GI`: Mixed lighting mode combining baked indirect light with realtime direct directional light.
- `NonDirectional`: Lightmap directional mode enforcing flat PS1 retro shading without directional specularity.

### 5. INPUTS
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`

### 6. OUTPUTS
- Baked lightmaps saved in `Assets/Data/Lightmaps/` and Light Probe coefficients.

### 7. RULES

- `[RULE-LGP-001]`: **Lightmap Bake Mode**: Lighting bake MUST use mode `Subtractive` with resolution $10.0\text{ texels/unit}$ and padding $2\text{ texels}$.
- `[RULE-LGP-002]`: **Flat PS1 Shading Settings**: Directional mode MUST equal `NonDirectional`. Ambient occlusion MUST equal `false` (OFF).
- `[RULE-LGP-003]`: **Light Probe Placement**: Minimum probe spacing MUST equal $3.0\text{m}$. Reflection Probes are strictly prohibited (`reflection_probes: "None"`).

### 8. ALGORITHMS

#### Algorithm 8.1: Lightmap & Light Probe Bake Schema

```yaml
bake_mode: "Subtractive"    # Baked GI + Realtime direct lighting
lightmap_resolution_texels_per_unit: 10.0
lightmap_padding_texels: 2
compress_lightmaps: true
lightmap_directional_mode: "NonDirectional"  # PS1 flat aesthetic
ambient_occlusion: false     # OFF — no usar AO con estética plana

# Light Probes
probe_placement_mode: "Manual"
min_probe_spacing_m: 3.0    # una probe cada 3 metros mínimo
probe_data_path: "Assets/Data/LightProbes/"
reflection_probes: "None"   # NO usar Reflection Probes — estética PS1
```

### 9. CONSTRAINTS
- `[CONS-LGP-001]`: Prohibido adding Reflection Probe components to gameplay scenes.

### 10. VALIDATION
- `[VAL-LGP-001]`: `LevelValidator.cs` asserts 0 ReflectionProbe components exist in scene.

### 11. EXAMPLES
- Bake schema above.

### 12. FAILURE CASES
- `[FAIL-LGP-001]`: **Reflection Probe Leak**: Reflection probe present in scene breaking PS1 flat aesthetic. Result: `FAIL-LGP-01`.

### 13. CROSS REFERENCES
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`

### 14. CHANGE HISTORY
- **v4.0 (2026-07-25)**: Created canonical SPEC-122 defining Lightmap baking resolution, Light Probe grid rules, and Reflection Probe prohibition.
