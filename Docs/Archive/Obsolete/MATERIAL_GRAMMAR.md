# MATERIAL_GRAMMAR.md — Surface Material Tokens & Texture Specifications
## Spec ID: SPEC-105B
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines surface material tokens, color hex values, URP shader property bindings, and texture channel mappings for architectural and narrative surfaces in *Echoes of You 2.0*.

### 2. SCOPE
Applies to `EchoesUrpMaterials.cs`, material assets in `Assets/Materials/`, static geometry, and narrative props.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `LIGHTING_GRAMMAR.md` (`SPEC-105`).

### 4. DEFINITIONS
- `Material Token`: Key string mapping a visual surface to its assigned URP material asset.
- `Memory-Amber`: Color token `#FFBF00` reserved exclusively for narrative artifacts associated with Lyra.

### 5. INPUTS
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`
- [ECHOES_BIBLE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/ECHOES_BIBLE.md) `[SPEC-101]`

### 6. OUTPUTS
- URP Material assets instantiated by `EchoesUrpMaterials.cs`.

### 7. RULES

- `[RULE-MAT-001]`: **Memory-Amber Hex Binding**: 100% of material assets referencing `Mat_Token_memory-amber` MUST set base color / emission hex to `#FFBF00` with URP emission intensity $1.2\text{ Lux}$.
- `[RULE-MAT-002]`: **Echo Cyan Hex Binding**: Material asset `Mat_Token_echo-cyan` MUST set color hex to `#4FC3E8` with dither transparency enabled.
- `[RULE-MAT-003]`: **Institutional Surface Palette**: Static environmental geometry MUST select material tokens from Table 8.1 exclusively.

### 8. ALGORITHMS

#### Table 8.1: Master Material Token Catalog

| Material Token Key | Color Hex Code | URP Shader Binding | Smoothness | Emission (Lux) | Usage Domain |
|---|---|---|---|---|---|
| `Mat_Token_memory-amber` | `#FFBF00` | `RetroFlatLit.shader` | `0.10` | `1.2 Lux` | Lyra's Narrative Props |
| `Mat_Token_echo-cyan` | `#4FC3E8` | `AnalogGhost.shader` | `0.00` | `0.5 Lux` | Active Echo Playback |
| `Mat_Token_institutional-teal` | `#2D4A4D` | `RetroFlatLit.shader` | `0.05` | `0.0 Lux` | School Lockers & Doors |
| `Mat_Token_faded-mustard` | `#A68A42` | `RetroFlatLit.shader` | `0.05` | `0.0 Lux` | Student Desks & Benches |
| `Mat_Token_dusty-rose` | `#734B4B` | `RetroFlatLit.shader` | `0.05` | `0.0 Lux` | Administrative Furniture |
| `Mat_Token_concrete-gray` | `#4A4D50` | `RetroFlatLit.shader` | `0.00` | `0.0 Lux` | Floors & Structural Columns |

### 9. CONSTRAINTS
- `[CONS-MAT-001]`: Prohibido assigning `#E8B262` or non-standard hex values to `Mat_Token_memory-amber`.

### 10. VALIDATION
- `[VAL-MAT-001]`: `LevelValidator.cs` parses scene material assets and asserts `memory-amber` base color equals `#FFBF00`.

### 11. EXAMPLES

#### Example 11.1: Material Token Definition in C#
```csharp
Material amberMat = new Material(Shader.Find("Universal Render Pipeline/RetroFlatLit"));
amberMat.SetColor("_BaseColor", ColorUtility.TryParseHtmlString("#FFBF00", out Color c) ? c : Color.yellow);
```

### 12. FAILURE CASES
- `[FAIL-MAT-001]`: **Hex Color Mismatch**: Material uses `#E8B262`. Result: `LevelValidator` flags `FAIL-ENV-01`.

### 13. CROSS REFERENCES
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`
- [SHADER_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SHADER_SPEC.md) `[SPEC-109]`

### 14. CHANGE HISTORY
- **v1.0 (2025-05-10)**: Initial material grammar.
- **v3.0 (2026-07-25)**: Full refactor into 14-section AI-Executable canonical format fixing memory-amber hex to `#FFBF00`.