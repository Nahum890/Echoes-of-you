# SHADER_SPEC.md — URP Custom Shader Specifications & Parameters
## Spec ID: SPEC-109
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Specifies the technical architecture, HLSL shader properties, rendering passes, and material token bindings for custom Universal Render Pipeline (URP) shaders in *Echoes of You 2.0* (`RetroFlatLit.shader`, `AnalogGhost.shader`, `LiminalFog.shader`).

### 2. SCOPE
Applies to custom HLSL shader files in `Assets/Shaders/`, URP Render Objects features, and `EchoesUrpMaterials.cs`. Excludes standard unity post-processing passes.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `LIGHTING_GRAMMAR.md` (`SPEC-105`).

### 4. DEFINITIONS
- `RetroFlatLit`: URPLit derivative shader enforcing flat color shading without smooth specular highlights.
- `AnalogGhost`: Transparent dither ghost shader used exclusively for Echo rendering (`#4FC3E8`).
- `LiminalFog`: Custom screen-space volumetric fog shader replacing standard GI.

### 5. INPUTS
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`
- [ECHO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_GRAMMAR.md) `[SPEC-107]`

### 6. OUTPUTS
- HLSL Shader files in `Assets/Shaders/`.
- Runtime material properties initialized by `EchoesUrpMaterials.cs`.

### 7. RULES

- `[RULE-SHD-001]`: **URP Compatibility Requirement**: 100% of custom shaders MUST target URP `RenderPipeline = UniversalPipeline` and include passes for `ForwardLit` and `ShadowCaster`.
- `[RULE-SHD-002]`: **AnalogGhost Dither Parameters**: `AnalogGhost.shader` MUST apply a 2x2 Bayer dither pattern for alpha fading with transparency alpha bounded in $\alpha \in [0.20, 0.80]$.
- `[RULE-SHD-003]`: **RetroFlatLit Shading**: `RetroFlatLit.shader` MUST quantize diffuse lighting into 2 discrete steps ($0.3$ and $1.0$) and force specular reflection to $0.0$.
- `[RULE-SHD-004]`: **LiminalFog Linear Exponent**: `LiminalFog.shader` MUST evaluate linear depth fog with exponent $D_{fog}$ matching Table 8.1 in `LIGHTING_GRAMMAR.md`.

### 8. ALGORITHMS

#### Table 8.1: Master URP Custom Shader Catalog

| Shader Name | Target Material Token | Key Properties | Transparency | Pass Types | Purpose |
|---|---|---|---|---|---|
| `RetroFlatLit.shader` | `Mat_Token_*` | `_BaseColor`, `_Steps=2`, `_Smoothness=0` | Opaque | `ForwardLit`, `ShadowCaster` | All static environment geometry |
| `AnalogGhost.shader` | `Mat_Token_echo-cyan` | `_GhostColor=#4FC3E8`, `_DitherScale=2.0`, `_Alpha` | Dither Transparent | `ForwardLit` | Echo playback rendering |
| `LiminalFog.shader` | `Mat_FogVolume` | `_FogColor`, `_FogDensity`, `_StartDist` | Screen Transparent | `UniversalForward` | Volume fog rendering |

### 9. CONSTRAINTS
- `[CONS-SHD-001]`: Prohibido using `Shader.Find("Standard")` or Built-in render pipeline tags.
- `[CONS-SHD-002]`: Prohibido 4K PBR texture maps or metallic maps in custom shaders.

### 10. VALIDATION
- `[VAL-SHD-001]`: `LevelValidator.cs` parses scene materials and asserts zero materials reference `Standard`.
- `[VAL-SHD-002]`: Inspector asserts `AnalogGhost` material handles `_Alpha` property dynamically during playback.

### 11. EXAMPLES

#### Example 11.1: HLSL Subshader Tag Block
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

### 12. FAILURE CASES
- `[FAIL-SHD-001]`: **Render Pipeline Mismatch**: Shader missing `UniversalPipeline` tag. Result: Object renders pink; `LevelValidator` flags `FAIL-SHD-01`.

### 13. CROSS REFERENCES
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`
- [ECHO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_GRAMMAR.md) `[SPEC-107]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec creation for URP shaders.
