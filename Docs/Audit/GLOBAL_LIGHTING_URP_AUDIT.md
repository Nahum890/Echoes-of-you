# GLOBAL_LIGHTING_URP_AUDIT — Auditoría de Pipeline Visual (URP)

- **Fecha:** 2026-08-20
- **Alcance:** Pipeline URP completo de Echoes of You 2.0 (15 niveles + cámaras + post + shaders + materiales)
- **Estado:** AUDITORÍA COMPLETA — sin modificaciones aplicadas
- **Fuentes canónicas:** `Docs/Authority/SOURCE_OF_TRUTH.md`, `Docs/Specs/LIGHTING_GRAMMAR.md`, `SHADER_SPEC.md`, `POST_PROCESSING_SPEC.md` (SPEC-120), `PERFORMANCE_BUDGET_SPEC.md`, `Docs/ExecutableSpecs/visual/*.yaml`
- **Regla aplicada:** `RULE-SOT-001` — conflicto de specs = HALT + resolución vía `CHANGE_CONTROL.md` antes de modificar

---

## 1. Resumen ejecutivo

| Área | Veredicto |
|---|---|
| Pipeline base | URP 17.4.0 (Unity 6000.4.3f1), Forward, SRP Batcher ON, HDR ON, MSAA 1× |
| Renderer Features | **0 features — SSAO NO existe** pese a claims de `EchoesLightingBakePipeline`/`EchoesURPConfigurator` |
| Luces por escena | 9–42 luces (dentro de máx 48), 1 direccional por nivel, 0 sombras soft en edit mode |
| GI horneada | **0 lightmaps y 0 light probes en TODAS las escenas** → luces Baked invisibles |
| Fog/ambient por capítulo | **NINGUNA escena coincide con los valores canónicos** (yaml/TechnicalArtPass) |
| Post-processing | Solo L01–L03 tienen perfil real. L04/L05 perfiles **VACÍOS**. L06–L15 **sin volume → sin post** |
| Runtime override | `BoostEarlyLevelLighting` (L1–5) viola `RULE-LGT-003` (sol cálido ×2.8); `ApplyEchoPlateVisuals` crea sombras **Soft** (viola CONS-LGT-003) |
| Shaders | 9 custom; los 2 de geometría principal (`PS1World` x110, `LiminalSurface` x42) son caros (fbm multi-octava per-pixel + loop de additional lights) |
| Rendimiento | Baseline de play mode PENDIENTE (requiere entrar en Play Mode) |

**Conclusión:** la identidad visual canónica (liminal PS1 duro, Flat, niebla por capítulo, post SPEC-120) **no está aplicada de facto en ninguna escena**. Lo que se ve hoy es: geometría con luces Baked sin GI (oscuridad), ambient Trilight en L01–03, boost cálido en L1–5, y post solo en 3 de 15 niveles.

---

## 2. Pipeline base verificado (Assets/Settings)

### 2.1 Echoes_URPAsset.asset (asignado a los 6 Quality levels vía GraphicsSettings)

| Setting | Valor real | Spec | Estado |
|---|---|---|---|
| Color Space | HDR on, LDR LUT 32 | — | — |
| MSAA | 1 (off) | — | — |
| RenderScale | 1.0 | — | — |
| Depth/Opaque texture | **OFF** | — | ⚠️ `LiminalFogVolume` usa `SampleSceneDepth` — con depth texture off puede fallar o salir negro |
| Main Light | Per-pixel, shadows 2048 | — | — |
| Additional Lights | Per-pixel, máx 8/objeto, **sin sombras** | — | ⚠️ Coste: los shaders custom hacen loop de additional lights con shadowAttenuation que siempre vale 1 |
| Shadow Distance | **50 m** | 40 m (LIGHTING_GRAMMAR/TechnicalArtPass) | ❌ DIVERGENCIA |
| Cascades | **1** | 2 (LIGHTING_GRAMMAR) | ❌ DIVERGENCIA |
| Soft Shadows | OFF | OFF (prohibido) | ✅ |
| SRP Batcher | ON | — | ✅ |
| Light Cookies | ON | — | — |

### 2.2 Echoes_UniversalRenderer.asset (guid 54e4c1d8…)

- **`m_RendererFeatures: []` — 0 renderer features.** `EchoesURPConfigurator.cs` (Editor) añade SSAO pero nunca llegó a persistirse (o se revirtió). `EchoesLightingBakePipeline.cs` afirma "SSAO activo, sombras adicionales 512×512, shadow distance 30m" — **falso vs asset real**.

### 2.3 Serialización

- `ProjectSettings/EditorSettings.asset`: `m_SerializationMode: 2` = **BINARIO**. Las 15 escenas `Level_XX.unity` son binarias (no grep-ables, no diff-friendly). `MainMenu.unity` sigue en YAML. Recomendación: volver a `Force Text` (modo 1) para control de versiones.

---

## 3. Inventario por escena (edit mode, verificado vía editor)

| Escena | Cap. | Niebla (color/densidad/modo) | Ambient | Luces | Baked | Realtime | Lightmaps | Volume global |
|---|---|---|---|---|---|---|---|---|
| L01 | I | #1F2633 / 0.005 / **Exp** | **Trilight** i=1.0 | 42 | 34 | 7 | 0 | Slice_N01 ✅ |
| L02 | I | #1F2633 / 0.005 / Exp | **Trilight** i=1.0 | 30 | 26 | 3 | 0 | Slice_N02 ✅ |
| L03 | I | #1F2633 / 0.005 / Exp | **Trilight** i=1.0 | 38 | 36 | 1 | 0 | Slice_N03 ✅ |
| L04 | II | #1F212E / 0.015 / Exp | Flat #262626 i=0.15 | 11 | 6 | 4 | 0 | N04_PostProc **VACÍO** ❌ |
| L05 | II | #241F29 / 0.018 / Exp | Flat #262626 i=0.15 | 9 | 5 | 3 | 0 | N05_PostProc **VACÍO** ❌ |
| L06–L10 | III–IV | #1A1A1F / 0.008 / Exp | Flat #262626 i=0.25 | 11–17 | 8–14 | 2–3 | 0 | **NINGUNO** ❌ |
| L11–L15 | V–VI | #1A1A1F / 0.008 / Exp | Flat #262626 i=0.25 | 12–14 | 8–10 | 2–4 | 0 | **NINGUNO** ❌ |

### Valores canónicos esperados (yaml + EchoesTechnicalArtPass.cs)

| Cap. | fog_color | fog_density | ambient_color |
|---|---|---|---|
| I | #1C2430 | 0.008 | #0F141A |
| II | #2E3024 | 0.010 | #1A1C14 |
| III | #2A1E1E | 0.012 | #140E0E |
| IV | #3B3024 | 0.015 | #1E1812 |
| V | #1A1020 | 0.020 | #0C0810 |
| VI | #0A0A0D (brief) vs #F0F4FF (yaml) — **conflicto documentado en el propio TechnicalArtPass** | 0.002 | #FFFFFF |

**Ninguna escena coincide.** L06–L15 llevan los valores por defecto de `LevelLightingSettings.cs` (fog #1A1A1F 0.008, amb #262626). L01–L03 no tienen componente LevelLightingSettings (ambient Trilight = violación de `RULE-LGT-001`).

### Composición de luces verificada (Level_02 como muestra, 30 luces)

- 1 direccional: `0.85 lux #F2F2FF`, rot (50,-30,0), **Hard**, bake **Mixed** → ✅ cumple RULE-LGT-003 en edit mode
- 29 point: todas int=1.5; 20× #C9D4B0 (fluorescente enfermo, rango 12), 4× #FFCC73/#FFBF59/#FFBF00/#FFCC66 (hints ámbar 5–28), 3× #3D8FBD (glow placas eco, Realtime, rango 4), 1× #B8E0FF (PlayerRimLight, Realtime)
- **26 de 30 luces son Baked sin lightmaps → no iluminan nada en runtime.** Solo 3 Realtime + 1 Mixed direccional dan luz real.
- Luces duplicadas por nombre: `Light_-1,8_-1,0`, `Light_1,2_2,0`, `Light_-1,8_-4,0`, `Light_-1,8_2,0` (×2 c/u)

### Anomalía de escena (Level_02)

- Raíces: `--- ENVIRONMENT ---`, `--- MECHANICS ---`, `--- UI ---`, AtmosphereController, `Directional Light`, `--- PLAYER ---`, `--- CAMERA ---`, Slice_GlobalVolume, ModalManager, SettingsController, **VNDialogueController ×2 (duplicado)**, VNChoiceGateController
- **NO existe raíz `--- FOG VOLUMES ---`** → los fog volumes de `EchoesTechnicalArtPass.SpawnFogVolumes` nunca se colocaron en ninguna escena (los materiales `Mat_FogVolume_ChI..VI` sí existen)
- 1 sola cámara: "Main Camera" (FOV 60, sin CinemachineBrain). Sistema custom: `SimpleFollowCamera` (orbita 5.5 m, pitch 22°, targetOffset 1.8, spherecast) + `CameraShake` + `GameFeelController`. Cinemachine 3.1.7 instalado pero **sin uso en niveles** (solo hooks opcionales en GameFeelController/EchoesCameraAuthority)

---

## 4. Divergencias vs specs canónicas (tabla consolidada)

| # | Spec | Realidad | Severidad |
|---|---|---|---|
| 1 | Ambient Flat (RULE-LGT-001) | L01–03 Trilight i=1.0 | ❌ CRÍTICA |
| 2 | Shadow distance 40 m / 2 cascadas | URP asset: 50 m / 1 cascada | ❌ ALTA |
| 3 | Sol 0.85 #F2F2FF (RULE-LGT-003) | Edit OK; runtime `BoostEarlyLevelLighting` L1–5: **1.2×2.5–2.8 → hasta 3.36 lux, color (1,0.95,0.85) CÁLIDO** | ❌ CRÍTICA (runtime) |
| 4 | Fog por capítulo (yaml) | Ninguna escena coincide; modo Exponential vs ExponentialSquared del pase | ❌ ALTA |
| 5 | Post SPEC-120 (bloom 0.25/0.9/0.7, vignette 0.35/0.4 #0D0D1A, exp −0.5, contrast 15, sat −8, tonemap None) | L01: exp −0.1/cont 15/**sat +4**/vig 0.25; L02: exp −0.1/cont 12/**sat +8**/vig 0.25; L03: exp −0.3/cont 15/**sat +2**/vig 0.3; L04–15: **sin post** | ❌ CRÍTICA |
| 6 | Runtime volume (PostProcessingSetup.cs) | Bloom 0.25/0.9/0.7 ✅, vignette 0.2/0.4 ❌, exp 0 ❌, cont 10 ❌, sat −5 ❌, **CA 0.12 + FilmGrain 0.45 activos** (spec no los lista), LensDistortion inactivo | ❌ ALTA |
| 7 | YAML `urp_volume_profiles.yaml` vs SPEC-120 | Bloom 0.5/sat −20/vignette #000000 0.2 vs 0.25/−8/#0D0D1A 0.4 → **CONFLICTO LEVEL 3 vs LEVEL 3** | ⛔ HALT (RULE-SOT-001) |
| 8 | 0 soft shadows (CONS-LGT-003) | Edit OK; runtime `ApplyEchoPlateVisuals` crea `LightShadows.Soft` en cada placa eco | ❌ MEDIA (runtime) |
| 9 | Luces ≤48/escena | 9–42 ✅ | ✅ |
| 10 | SSAO (claimed por bake pipeline) | Renderer con 0 features → NO existe | ❌ ALTA (falso claim) |
| 11 | Depth texture para fog volumes | URP asset depth texture **OFF** → `LiminalFogVolume.SampleSceneDepth` sin recurso | ❌ MEDIA |
| 12 | Lightmaps horneados | 0 en todas las escenas → 26–36 luces Baked por escena NO iluminan | ❌ CRÍTICA (visual) |
| 13 | Blueprint `Level_01`: fog 0.012 (Ch III) | TechnicalArtPass/yaml Ch I: 0.008 → blueprints no actualizados | ❌ MEDIA |
| 14 | Blueprint `cameraProfile/lightingProfile` = `{fileID: 0}` | Referencias vacías en Level_01_Blueprint | ❌ MEDIA |
| 15 | Volumes en runtime | `Slice_GlobalVolume` (priority 1) + `GlobalPostProcessVolume` runtime (priority 0) coexisten → blend por prioridad: gana el perfil de escena para parámetros compartidos; CA/grain del runtime se suman siempre | ⚠️ INFO |

---

## 5. Shaders custom — inventario, coste y uso real

### Uso real en Level_02 (128 MeshRenderers; SkinnedMeshRenderers no contados)

| Shader | Materiales asignados (77 total) | Renderers en L02 | Coste |
|---|---|---|---|
| `Echoes/PS1World` | 20 (arquitectura, tokens, suelos) | **110** | 🔴 ALTO: snap vértice + affine + fbm 5 oct (stains) + scanlines + flicker + **loop additional lights con specular por luz** |
| `Echoes/LiminalSurface` | 14 (plasters, tokens, legacy) | **42** | 🔴 ALTO: fbm 5 oct + wear 4 oct + crack 4 oct + detail tex + loop additional lights |
| `Universal Render Pipeline/Lit` | 16 (echo, placas, puertas…) | 3 | 🟢 (fuera de SHADER_SPEC; varios son runtime) |
| `Universal Render Pipeline/Unlit` | 10 (decals) | 3 | 🟢 |
| `Echoes/LiminalFogVolume` | 7 (ChI–VI + Mat_LiminalFog) | 0 | 🟡 sin uso en escenas (fog volumes no colocados) |
| `Echoes/LiminalCeiling` | 1 (Mat_LiminalCeiling) | 0 | 🟡 fbm 4 oct + grid + loop lights; sin uso en L02 |
| `Echoes/PS1Character` | 1 (Mat_Player) | — | 🟢 snap + quantize + scanline (barato) |
| `Echoes/EchoLiminal` | 1 (Mat_Token_echo-cyan) | 1 | 🟢 transparente sin luces (bayer + scanline) |
| `Echoes/AnalogGhost` | 0 | 0 | 🟢 sin uso |
| `Echoes/RetroFlatLit` | 1 (Mat_Fluorescent) | 0 | 🟢 sin uso en L02 |
| `Echoes/LiminalFog` (screen-fog) | 0 | 0 | 🟡 sin uso (Mat_LiminalFog apunta a LiminalFogVolume) |

### Observaciones de coste (vs PERFORMANCE_BUDGET_SPEC: 60 fps / 120 draw calls / 85k tris)

- `PS1World` + `LiminalSurface` cubren ~152/128 renderers de geometría → el loop de additional lights (hasta 8/luz) con `pow(spec)` **por luz y por píxel** + fbm 3–5 octavas es el mayor riesgo de GPU en L01–03 (42/30/38 luces) y empeora con `BoostEarlyLevelLighting` (que aumenta rangos de point lights → más luces en overdraw).
- `LiminalFogVolume` (fbm 3D + 2 muestras extra de depth) se añadiría como overlay fullscreen en pasillos — coste medio, requiere depth texture ON.
- Uso de URP/Lit y URP/Unlit fuera de SHADER_SPEC (que define 6 shaders) — los materiales legacy (Mat_Door, Mat_Echo, decals…) no siguen la gramática de cuantización/scanlines.

---

## 6. Post-processing real vs SPEC-120 (bloom 0.25 / threshold 0.90 / scatter 0.70 / vignette 0.35 / 0.40 #0D0D1A / exp −0.5 / contraste 15 / saturación −8 / tonemap None)

| Origen | Bloom | Vignette | ColorAdj | Notas |
|---|---|---|---|---|
| SPEC-120 | ✅ 0.25/0.9/0.7 | 0.35 / 0.4 #0D0D1A | exp −0.5, cont 15, sat −8 | Referencia |
| Slice_N01 (L01) | ✅ 0.25/0.9/0.7 | 0.25/0.4 #0D0D0D~#0D0D1A | exp −0.1, cont 15, **sat +4** | sat invertida |
| Slice_N02 (L02) | ✅ 0.25/0.9/0.7 | 0.25/0.4 | exp −0.1, cont 12, **sat +8** | sat invertida |
| Slice_N03 (L03) | ✅ 0.25/0.9/0.7 | 0.30/0.4 | exp −0.3, cont 15, **sat +2**, colorFilter (0.94,0.95,1) | sat invertida |
| N04/N05 (L04/L05) | — | — | — | **Perfiles VACÍOS** (components: fileID 0 ×3) |
| L06–L15 | — | — | — | **Sin volume → sin post** |
| Runtime (PostProcessingSetup) | ✅ 0.25/0.9/0.7 | 0.2/0.4 #0D0D1A | exp 0, cont 10, sat −5 | + CA 0.12 + FilmGrain Medium1 0.45 (siempre activos) |

---

## 7. Runtime overrides que rompen la gramática (nivel de juego)

1. **`LevelEnvironmentBootstrap.BoostEarlyLevelLighting` (L1–5):** sol → `max(int, 1.2×boost)` con color **cálido (1, 0.95, 0.85)**, amb +0.12–0.14×boost, niebla ×0.35, point ×boost×0.6, +3 luces `EchoesFill_Boost_*` cálidas. **Anula la intención liminal del capítulo I/II** y rompe RULE-LGT-003. (Comentario del código: "extra brightness so the player can see" — decisión de diseño heredada que contradice la spec actual.)
2. **`ApplyEchoPlateVisuals`:** crea `EchoPlateBlueLight` con `shadows = Soft` → viola CONS-LGT-003.
3. **`ApplyLighting()` fallback** (escenas sin LevelLightingSettings — caso L01–03): FogMode **ExponentialSquared**, sun 0.58 (PlayerPrefs preset "liminal"), point ×0.72→×1.05, +2 fill lights azuladas.
4. **`ApplyArchitectureMaterialStyling`:** reemplaza en runtime materiales oscuros de PS1World por instancias `wallColor`/`accentColor` (no toca assets, OK) y fuerza `_DitherStrength 0.35`.
5. **Doble sistema de volumes:** en niveles con Slice (L01–03) el perfil de escena gana (priority 1 > 0); CA/grain runtime se aplican siempre encima.

---

## 8. Rendimiento — estado del baseline

- **PENDIENTE:** baseline de rendering stats en Play Mode (edit mode no produce stats de escena real).
- Presupuesto vigente (SPEC-121): 60 fps / 16.66 ms / ≤120 draw calls / ≤85k tris / ≤40 batches estáticos / 1.2 GB RAM / 512 MB VRAM.
- Riesgos identificados sin medir: shaders pesados en geometría mayoritaria, overdraw de additional lights en L01–03, decals Unlit (10 materiales) sin batch estático garantizado, luz direccional con sombras 2048 + cascades 1 (distancia 50 m innecesaria vs 40 m de spec).

---

## 9. Anomalías y bugs detectados

1. `VNDialogueController` duplicado como raíz en Level_02 (2 copias).
2. Luces duplicadas por nombre en Level_02 (4 pares).
3. `N04_PostProc.asset` / `N05_PostProc.asset`: perfiles de volume **vacíos** (0 efectos) → post ausente en L04/L05.
4. L06–L15 sin volume global → sin post-processing en 10 de 15 niveles.
5. `EchoesLightingBakePipeline`/`EchoesURPConfigurator` documentan SSAO/features que **no existen** en el renderer real.
6. `Level_01_Blueprint.asset`: fog 0.012 (cap. III) en nivel de cap. I; referencias `cameraProfile`/`lightingProfile` vacías.
7. Depth texture OFF en URP Asset pero `LiminalFogVolume` la requiere (`SampleSceneDepth`).
8. `Mat_LiminalFog` usa `LiminalFogVolume` (no `LiminalFog`); `LiminalFog.shader` sin materiales.
9. Serialización binaria de escenas (m_SerializationMode 2) impide diff/grep y rompió la auditoría por texto.
10. `EchoesPresentationSettings.ApplyLightingPreset("liminal")` (una vez por sesión) fija PlayerPrefs fog 0.011 / sun 0.58 / point 0.72 que luego se mezclan con `ApplyLighting` → valores que dependen del orden de ejecución.

---

## 10. Recomendaciones priorizadas (NO aplicadas — pendientes de aprobación)

### Fase A — Bloqueantes visuales (crítico)
1. **Resolver conflicto Level 3 vs Level 3** (`urp_volume_profiles.yaml` vs `POST_PROCESSING_SPEC.md`) vía `CHANGE_CONTROL.md` → definir el perfil de post único por capítulo.
2. **Unificar post por capítulo:** crear perfiles ChI–ChVI con valores SPEC-120, asignarlos a todas las escenas (15 volumes), rellenar N04/N05, eliminar dependencia del perfil runtime (o alinearlo 1:1 con SPEC-120).
3. **Iluminación:** pasar todas las luces point a Realtime (o bakear lightmaps + probes), aplicar fog/ambient por capítulo desde los blueprints (corregir blueprints primero: L01 density 0.008, refs vacías), y forzar AmbientMode.Flat.
4. **Desactivar `BoostEarlyLevelLighting`** (o escalarlo a un solo parámetro autorizado por CHANGE_CONTROL) y eliminar `LightShadows.Soft` de `ApplyEchoPlateVisuals`.

### Fase B — Spec y assets
5. URP Asset: shadowDistance 40 m, cascades 2, depth texture ON (si se usan fog volumes).
6. Correr `EchoesTechnicalArtPass.RunFullPass` tras resolver el conflicto de cap. VI (brief #0A0A0D vs yaml #F0F4FF) — o aplicar sus valores manualmente por escena.
7. Volver a `m_SerializationMode: 1` (Force Text) para escenas.
8. Limpiar duplicados (VNDialogueController ×2, luces duplicadas).

### Fase C — Rendimiento y shaders
9. Baseline de rendering stats en Play Mode (autorizar play mode) antes de tocar shaders.
10. Evaluar: reducir fbm de PS1World/LiminalSurface a 2–3 octavas, `#pragma multi_compile` para quitar el loop de additional lights en objetos lejanos, o un shader "flat" para geometría distante (LOD). Alinear decals/props legacy a los shaders de la gramática.

### Fase D — QA
11. Generar `Docs/QA/GLOBAL_VISUAL_QA.md` con checklist por capítulo (a-b tests: Flat vs Trilight, sat −8 vs +8, boost on/off) tras aplicar Fase A.

---

## 11. Documentos pendientes detectados

- `Docs/Art/VISUAL_TARGET.md` — **NO EXISTE** (referenciado por brief)
- `Docs/Art/ENVIRONMENT_GRAMMAR.md` — **NO EXISTE**
- `Docs/Art/MATERIAL_GRAMMAR.md` — solo en `Docs/Archive/Obsolete/` (prohibido por SOT) → los materiales se rigen por `SHADER_SPEC.md` + `EchoesMaterialLibrary.cs` (tokens canónicos verificados en los 77 .mat)
- `Docs/ExecutableSpecs/visual/urp_volume_profiles.yaml` — en conflicto con SPEC-120 (ver §6)