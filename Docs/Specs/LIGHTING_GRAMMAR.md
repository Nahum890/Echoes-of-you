# LIGHTING_GRAMMAR.md — URP Lighting, Fog & Post-Processing Specifications
## Spec ID: SPEC-105
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Specifies technical URP lighting parameters, shadow settings, fog density volumes, and post-processing profiles in *Echoes of You 2.0*. Ensures 100% compliance with the PS1/PS2 liminal aesthetic using flat ambient lighting and hard retro shadows.

### 2. SCOPE
Applies to `LevelLightingSettings.cs`, `EchoesLevelEnvironmentBootstrap.cs`, URP Volume assets, and light components. Excludes 3D mesh geometry.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `DESIGN_PHILOSOPHY.md` (`SPEC-001`). Consolidates `MATERIAL_GRAMMAR.md`.

### 4. DEFINITIONS
- `Flat Ambient Lighting`: URP `AmbientMode.Flat` setting without complex baked global illumination.
- `Hard Shadows`: Non-filtered shadow map projection ($40.0\text{m}$ max distance, 2 cascades).
- `Light Cap`: Maximum allowable active light components per scene ($48$ max).

### 5. INPUTS
- [DESIGN_PHILOSOPHY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/DESIGN_PHILOSOPHY.md) `[SPEC-001]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`

### 6. OUTPUTS
- URP Volume Profiles and `LevelLightingSettings.cs` components.
- Lighting validation assertions for `LevelValidator.cs`.

### 7. RULES

- `[RULE-LGT-001]`: **Ambient & Shadow Mode Constraint**: `RenderSettings.ambientMode` MUST equal `AmbientMode.Flat`. Shadow type MUST equal `Hard Shadows` with `shadowDistance = 40.0m` and 2 cascades.
- `[RULE-LGT-002]`: **Per-Scene Light Ceiling**: Total active light components (`Light`) per scene MUST NOT exceed $48$ points for performance.
- `[RULE-LGT-003]`: **Sun Light Alignment**: Main directional light intensity MUST equal $I_{sun} = 0.85\text{ Lux}$, color `#F2F2FF`, rotation `Vector3(50.0, -30.0, 0.0)`.
- `[RULE-LGT-004]`: **Post-Processing Profile Parameters**:
  - *Bloom*: Intensity $0.25$, Threshold $0.9$, Scatter $0.7$.
  - *Vignette*: Intensity $0.35$, Smoothness $0.4$, Color `#0D0D1A`.
  - *Color Adjustments*: Contrast $+15.0$, Saturation $-8.0$, Post Exposure $-0.5$.

### 8. ALGORITHMS

#### Table 8.1: Chapter Lighting Tokens & Fog Matrix

| Chapter | Chapter Title | Fog Color (Hex) | Fog Density ($D_{fog}$) | Ambient Color (Hex) | Visual Mood |
|---|---|---|---|---|---|
| **I** | Persistencia | `#1C2430` (Navy) | `0.008` | `#0F141A` | Institutional Cold |
| **II** | Coordinación | `#2E3024` (Olive) | `0.010` | `#1A1C14` | Strategic Neutral |
| **III** | Confianza | `#2A1E1E` (Dusty Rose) | `0.012` | `#140E0E` | Intimate Warm |
| **IV** | Optimización | `#3B3024` (Amber Deep) | `0.015` | `#1E1812` | Nostalgic Warm |
| **V** | Consecuencia | `#1A1020` (Void Purple)| `0.020` | `#0C0810` | Liminal Cold |
| **VI** | Integración | `#F0F4FF` (White Clean)| `0.002` | `#FFFFFF` | Resolutive Clarity |

### 9. CONSTRAINTS
- `[CONS-LGT-001]`: Prohibido `Shader.Find("Standard")` or non-URP shaders.
- `[CONS-LGT-002]`: Prohibido local point/spot light intensity $> 5.0\text{ Lux}$ or range $> 25.0\text{m}$.

### 10. VALIDATION
- `[VAL-LGT-001]`: `LevelValidator.cs` asserts total light count $\le 48$ in scene hierarchy.
- `[VAL-LGT-002]`: Inspector asserts URP `ambientMode == AmbientMode.Flat` and `shadowDistance == 40.0f`.

### 11. EXAMPLES

#### Example 11.1: Level Lighting Config in C#
```csharp
RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
RenderSettings.ambientLight = ColorUtility.TryParseHtmlString("#0F141A", out Color c) ? c : Color.black;
RenderSettings.fog = true;
RenderSettings.fogMode = FogMode.Exponential;
RenderSettings.fogDensity = 0.008f;
RenderSettings.fogColor = ColorUtility.TryParseHtmlString("#1C2430", out Color fc) ? fc : Color.gray;
```

### 12. FAILURE CASES
- `[FAIL-LGT-001]`: **Light Cap Exceeded**: Scene contains 52 active lights. Result: `LevelValidator` flags `FAIL-LGT-01`.
- `[FAIL-LGT-002]`: **Soft Shadows Detected**: Dynamic light uses Soft Shadows. Result: `FAIL-LGT-02`.

### 13. CROSS REFERENCES
- [DESIGN_PHILOSOPHY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/DESIGN_PHILOSOPHY.md) `[SPEC-001]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`

### 14. CHANGE HISTORY
- **v1.0 (2025-05-25)**: Initial lighting parameters.
- **v3.0 (2026-07-25)**: Full refactor into 14-section AI-Executable canonical format.