# GLOBAL_LIGHTING_SPEC.md — Global Lighting Implementation Specification (URP)
## Spec ID: SPEC-143
## Version: 1.0 (AI-Executable)

---

### 1. PURPOSE
Defines the **binding implementation values** for the global lighting pipeline of *Echoes of You 2.0*: URP asset settings, per-scene sun/ambient/fog, light modes, fog volumes, and runtime override policy. Supersedes the divergent ad-hoc values found in scenes (see `Docs/Audit/GLOBAL_LIGHTING_URP_AUDIT.md`).

### 2. SCOPE
Applies to `Assets/Settings/Echoes_URPAsset.asset`, `Assets/Settings/Echoes_UniversalRenderer.asset`, all 15 `Level_XX.unity` scenes, `Assets/Scripts/LevelLightingSettings.cs`, `Assets/Scripts/LevelEnvironmentBootstrap.cs`, `Assets/Scripts/PostProcessingSetup.cs`, `Assets/Data/Levels/Level_XX_Blueprint.asset`, and `Assets/Editor/EchoesTechnicalArtPass.cs`.

### 3. AUTHORITY
Level 3 (Art Implementation). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`), `LIGHTING_GRAMMAR.md` (`SPEC-105`), `lighting_profiles.yaml` (chapter values), `ECHOES_BIBLE.md` (tokens). Post-processing values delegated to `POST_PROCESS_SPEC.md` (`SPEC-144`) and `POST_PROCESSING_SPEC.md` (`SPEC-120`). Performance budget from `PERFORMANCE_BUDGET_SPEC.md` (`SPEC-121`).

### 4. DEFINITIONS
- `Canonical Sun`: Directional light `intensity = 0.85`, `color = #F2F2FF`, `rotation = (50, -30, 0)`, `shadows = Hard`, `lightmapBakeType = Mixed`.
- `Canonical Ambient`: `AmbientMode.Flat`, per-chapter `ambient_color` from `lighting_profiles.yaml`, `ambientIntensity = 0.15` (`SPEC-105` §7).
- `Canonical Fog`: per-chapter `fog_color`/`fog_density` from `lighting_profiles.yaml`, mode `ExponentialSquared` (`EchoesTechnicalArtPass`).
- `Runtime Override`: any script mutation of RenderSettings/light values during play.

### 5. INPUTS
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`
- `Docs/ExecutableSpecs/visual/lighting_profiles.yaml` (chapter fog/ambient)
- `Docs/Technical/TECHNICAL_ART_PASS.md` (fog volume placement rules)

### 6. OUTPUTS
- `Assets/Settings/Echoes_URPAsset.asset` (fixed values, see RULE-LGT-G01)
- 15 scenes with canonical sun/ambient/fog/lights/volumes
- `Assets/Data/Levels/Level_XX_Blueprint.asset` (chapter values + filled references)
- Runtime scripts aligned (no grammar-breaking overrides)

### 7. RULES

- `[RULE-LGT-G01]`: **URP Asset** MUST have: `shadowDistance = 40.0`, cascades `2`, soft shadows `OFF`, MSAA `1`, HDR `ON`, `depthTexture = ON` (required by `Echoes/LiminalFogVolume`), main light shadows `2048`, additional lights per-pixel limit `≤ 8` (target `4`), SRP Batcher `ON`, no renderer features unless approved by change control.
- `[RULE-LGT-G02]`: **Canonical Sun** (§4) in every level scene. No script may change its intensity/color/rotation at runtime (`EchoesPresentationSettings`/`LevelEnvironmentBootstrap` MUST NOT touch it).
- `[RULE-LGT-G03]`: **Canonical Ambient** (§4) per chapter in every scene. `AmbientMode.Trilight`/`Skybox` prohibited (`RULE-LGT-001`).
- `[RULE-LGT-G04]`: **Canonical Fog** (§4) per chapter, applied from the blueprint via `LevelLightingSettings` (or `RenderSettings` fallback). Density values are primitives per `RULE-LGT-005` (`fog_density_chapter_I = 0.008`).
- `[RULE-LGT-G05]`: **Light Modes**: point/spot lights MUST be `Realtime` with `shadows = None` (flat PS1 look, `CONS-LGT-003`), OR the scene MUST have baked lightmaps+probes covering its Baked lights. **No Baked light without a lightmap** (current state: 26–36 Baked lights per scene with 0 lightmaps → invisible). Light count per scene ≤ 48 (`LIGHT-VAL-002`).
- `[RULE-LGT-G06]`: **Fog Volumes**: `Echoes/LiminalFogVolume` cubes in corridors (major axis > 18 m, minor < 50%) and open spaces (> 30 m), under root `--- FOG VOLUMES ---`, materials `Mat_FogVolume_ChI..VI`, `shadowCastingMode = Off`.
- `[RULE-LGT-G07]`: **Runtime Override Policy**: `LevelEnvironmentBootstrap.BoostEarlyLevelLighting` MUST be disabled (it breaks `RULE-LGT-003`: warm sun up to 3.36 lux). `ApplyEchoPlateVisuals` glow light MUST use `shadows = None` (Soft prohibited). `PostProcessingSetup` runtime volume MUST match `SPEC-123` exactly. `EchoesPresentationSettings` preset applies to menu only.
- `[RULE-LGT-G08]`: **Blueprints**: 15 `Level_XX_Blueprint.asset` MUST carry chapter values (`EchoesTechnicalArtPass.Chapters`), `maxRecordSeconds` per narrative, and valid `cameraProfile`/`lightingProfile` references (currently `{fileID: 0}`).

### 8. ALGORITHMS

#### Algorithm 8.1: Per-Scene Lighting Recipe
```text
1. Directional: Canonical Sun (rule G02).
2. RenderSettings: ambientMode = Flat; ambientLight = chapter.ambient_color; ambientIntensity = 0.15.
3. fog = true; fogMode = ExponentialSquared; fogColor = chapter.fog_color; fogDensity = chapter.fog_density.
4. Point/spot lights: Realtime, shadows None (rule G05); colors per LightingProfile archetypes.
5. Global volume: POST_PROCESS_SPEC profile (SPEC-123).
6. Fog volumes: SpawnFogVolumes(chapter) (rule G06).
7. No runtime boost; echo plate glows shadow None (rule G07).
```

### 9. CONSTRAINTS
- `[CONS-LGT-003]`: Prohibido soft shadows (`LightShadows.Soft`).
- `[CONS-CC-001]`: Frozen decisions in `CHANGE_CONTROL.md` Table 8.1 are binding.

### 10. VALIDATION
- `[VAL-LGT-G01]`: Per scene: ambient mode == Flat; fog density == chapter value; sun == Canonical Sun; 0 Soft shadows; volume profile == canonical post; Baked lights only if lightmaps > 0.
- `[VAL-LGT-G02]`: `Echoes_URPAsset.asset`: shadowDistance 40, cascades 2, depthTexture ON.

### 11. EXAMPLES
- Level_02 (Cap. I): fog `#1C2430` d `0.008`, ambient `#0F141A` 0.15, sun `0.85 #F2F2FF (50,-30,0)` Hard Mixed, 30 lights Realtime (26 Baked converted), `--- FOG VOLUMES ---` with FogVolume modules, volume `Slice_N02_PostProc` → canonical post.

### 12. FAILURE CASES
- `[FAIL-LGT-G01]`: Trilight/Skybox ambient detected → fail.
- `[FAIL-LGT-G02]`: Baked light without lightmaps → fail.
- `[FAIL-LGT-G03]`: Runtime warm boost active in L1–5 → fail.

### 13. CROSS REFERENCES
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`
- [POST_PROCESS_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Art/POST_PROCESS_SPEC.md) `[SPEC-144]`
- [GLOBAL_LIGHTING_URP_AUDIT.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Audit/GLOBAL_LIGHTING_URP_AUDIT.md)

### 14. CHANGE HISTORY
- **v1.0 (2026-08-20)**: Created after GLOBAL_LIGHTING_URP_AUDIT; codifies binding values to close 15 divergences found in scenes.