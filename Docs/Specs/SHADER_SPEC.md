# SHADER_SPEC.md — URP Custom Shader Specifications & Parameters
## Spec ID: SPEC-109
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Specifies the technical architecture, HLSL shader properties, rendering passes, and material token bindings for custom Universal Render Pipeline (URP) shaders in *Echoes of You 2.0* (`RetroFlatLit.shader`, `AnalogGhost.shader`, `LiminalFog.shader`, `LiminalFogVolume.shader`, `EchoLiminal.shader`, `LiminalSurface.shader`).

### 2. SCOPE
Applies to custom HLSL shader files in `Assets/Shaders/`, URP Render Objects features, and `EchoesUrpMaterials.cs`. Excludes standard unity post-processing passes.

### 3. AUTHORITY
Level 3 (Declarative Spec). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `LIGHTING_GRAMMAR.md` (`SPEC-105`). Runtime data contract defined in `Docs/ExecutableSpecs/visual/shader_spec.yaml` (`SPEC-EXEC-SHD`).

### 4. DEFINITIONS
- `RetroFlatLit`: URPLit derivative shader enforcing flat color shading without smooth specular highlights.
- `AnalogGhost`: Transparent dither ghost shader used exclusively for Echo rendering (`#4FC3E8`).
- `LiminalFog`: Custom screen-space volumetric fog shader replacing standard GI.
- `LiminalFogVolume`: Volume-based fog rendering with corner accumulation.
- `EchoLiminal`: Echo-specific shader with distortion, chromatic aberration, scanlines.
- `LiminalSurface`: Environment surface shader with emission, stains, wear, subsurface.

### 5. INPUTS
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`
- [ECHO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_GRAMMAR.md) `[SPEC-107]`
- [CONSTANTS_REGISTRY.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CONSTANTS_REGISTRY.yaml) `[SPEC-124]` — shader paths, color tokens
- [shader_spec.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/visual/shader_spec.yaml) `[SPEC-EXEC-SHD]` — blend modes, keywords, passes

### 6. OUTPUTS
- HLSL Shader files in `Assets/Shaders/`.
- Runtime material properties initialized by `EchoesUrpMaterials.cs`.

### 7. RULES
- `[RULE-SHD-001]`: **URP Compatibility Requirement** — 100% of custom shaders MUST target URP `RenderPipeline = UniversalPipeline` and include passes for `UniversalForward` and `ShadowCaster`.
- `[RULE-SHD-002]`: **AnalogGhost Dither Parameters** — `AnalogGhost.shader` MUST apply a 2x2 Bayer dither pattern for alpha fading with transparency alpha bounded in $\alpha \in [0.20, 0.80]$.
- `[RULE-SHD-003]`: **RetroFlatLit Shading** — `RetroFlatLit.shader` MUST quantize diffuse lighting into 2 discrete steps ($0.3$ and $1.0$) and force specular reflection to $0.0$.
- `[RULE-SHD-004]`: **LiminalFog Linear Exponent** — `LiminalFog.shader` MUST evaluate linear depth fog with exponent $D_{fog}$ matching `lighting_profiles.yaml` chapter fog densities.
- `[RULE-SHD-005]`: **Blend Mode Authority** — Blend mode, ZWrite, Cull, Queue, and Keywords per shader are defined in `shader_spec.yaml`. Markdown does not duplicate these values.

### 8. ALGORITHMS
Shader catalog, blend modes, and keywords are defined in `shader_spec.yaml`. This document provides architectural rules only.

#### Example 8.1: HLSL Subshader Tag Block (Canonical Pattern)
```hlsl
SubShader
{
    Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
    LOD 100

    Pass
    {
        Name "ForwardLit"
        Tags { "LightMode"="UniversalForward" }
        // HLSL Code
    }
}
```

### 9. CONSTRAINTS
- `[CONS-SHD-001]`: Prohibido using `Shader.Find("Standard")` or Built-in render pipeline tags.
- `[CONS-SHD-002]`: Prohibido 4K PBR texture maps or metallic maps in custom shaders.
- `[CONS-SHD-003]`: Prohibido hardcoding blend modes/ZWrite/Cull in Cull in C#.

### 10. VALIDATION
- `[VAL-SHD-001]`: `LevelValidator.cs` parses scene materials and asserts zero materials reference `Standard`.
- `[VAL-SHD-002]`: Inspector asserts `AnalogGhost` material handles `_Alpha` property dynamically during playback.
- `[VAL-SHD-003]`: `ExecutableSpecValidator.cs` asserts shader blend/ZWrite/Cull match `shader_spec.yaml`.

### 11. EXAMPLES
See `shader_spec.yaml` for canonical blend/keyword definitions.

### 12. FAILURE CASES
- `[FAIL-SHD-001]`: **Render Pipeline Mismatch**: Shader missing `UniversalPipeline` tag. Result: Object renders pink; `LevelValidator` flags `FAIL-SHD-01`.

### 13. CROSS REFERENCES
- [shader_spec.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/visual/shader_spec.yaml) `[SPEC-EXEC-SHD]`
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`
- [ECHO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_GRAMMAR.md) `[SPEC-107]`
- [CONSTANTS_REGISTRY.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CONSTANTS_REGISTRY.yaml) `[SPEC-124]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec creation for URP shaders.
- **v4.0 (2026-07-25)**: Moved numeric catalog to `shader_spec.yaml`. Added LiminalFogVolume, EchoLiminal, LiminalSurface shaders. Defined exact blend/ZWrite/Cull/Queue per shader.

(End of file - total 78 lines)