# TECHNICAL_ART_PASS.md
## Pase de arte técnico — estética liminal PS1/PS2

**Fecha:** 2026-07-26 · **Rama:** `ws01-cleanup` · **Commits:** `276d0005` (docs), `eb2fb0f7` (código)
**Estado:** implementado y subido; **pendiente de ejecutar en el editor de Unity** (esta máquina no tiene Unity instalado — sin `Library/`).

Registro de implementación de la orden de trabajo "Technical Artist AI"
(§3.1–3.7): materiales con los 4 shaders custom, iluminación por capítulo,
fog volumes, estados visuales del eco, post-procesado global, narrativa
ambiental y validación visual/funcional.

---

## 1 — CÓMO EJECUTAR

- **Editor:** menú `Echoes of You → Technical Art → Run Full Pass (All Levels)`.
- **Headless:**
  ```
  Unity.exe -batchmode -projectPath <ruta> -executeMethod EchoesTechnicalArtPass.RunFullPassBatch
  ```
- Menús parciales: `Apply Visual Pass To Open Scene`, `Apply Chapter Lighting
  To Blueprints`, `Create Narrative Echo Recordings`, `Configure URP (Hard
  Shadows, 40m)`, `Validate All Levels (Visual)`.

El pase opera sobre las 15 escenas construidas **sin reconstruirlas**: no
invoca ningún builder y no destruye los props del Environment Pass. Las
raíces `--- FOG VOLUMES ---` y `--- NARRATIVE ---` son idempotentes (se
regeneran completas en cada ejecución).

---

## 2 — ARCHIVOS NUEVOS

| Archivo | Qué hace |
|---|---|
| `Assets/Editor/EchoesTechnicalArtPass.cs` | Orquestador. Perfiles de los 6 capítulos → 15 blueprints + escenas (fog, ambient, sol 0.85 lux `#F2F2FF`, rot 50/-30/0); sombras Hard en todas las luces; shadow distance 40 m; soft shadows OFF en el URP Asset (`m_SoftShadowsSupported` vía SerializedObject); fog volumes; EchoRecordingData narrativos; entrada batchmode. |
| `Assets/Editor/EchoesPropDecorator.cs` | `DecorateLevel(n)`: exactamente 1 prop narrativo `memory-amber #FFBF00` por nivel (VAL-ENV-001) según `ENVIRONMENT_STORYTELLING` Tabla 8.2 + su microescena MIC-001..004, bajo `--- NARRATIVE ---`. |
| `Assets/Editor/EchoesVisualValidationPass.cs` | Por nivel: ≤48 luces (FAIL-LGT-01), 0 sombras Soft, 0 materiales magenta; captura spawn/puzzle/salida (1280×720, offset de cámara del juego) y regresión contra `Docs/Art/ReferenceFrames` (pixel diff ≤2 %, SSIM ≥0.98 por ventanas 8×8). Escribe `Reports/generated/visual_regression_report.json`. |

## 3 — ARCHIVOS MODIFICADOS

| Archivo | Cambio |
|---|---|
| `Assets/Editor/EchoesMaterialLibrary.cs` | Hex canónicos de `CONSTANTS_REGISTRY.yaml` en `TokenToColor` (ver §5.1); `memory-amber` con emission ×1.2; constante `kAnalogGhost` (`Echoes/AnalogGhost`); `ApplyFogVolumeDefaults()` (`_CornerAccumulation 0.35, _LightScatter 0.25, _NoiseScale 0.5, _NoiseSpeed 0.05`). |
| `Assets/Editor/EchoesLevelShell.cs` | Clamp de `fogDensity` ampliado de `[0.012, 0.04]` a `[0.002, 0.04]` (Cap. VI usa 0.002). Luz direccional: `Soft` → `Hard`. |
| `Assets/Scripts/EchoRecorder.cs` | Estado **Recording**: rim light cyan `#4FC3E8` solo en el jugador (point light + `MaterialPropertyBlock`, sin instanciar materiales); se limpia en stop/reset/clear. Trim de ecos usa el residual de 2.5 s. |
| `Assets/Scripts/EchoPlayback.cs` | Ciclo de vida completo del eco (ver §4). **Bugfix:** se buscaba `Shader.Find("Hidden/AnalogGhost")` pero el shader se llama `Echoes/AnalogGhost` — ese bloque nunca se ejecutaba. Opacidad por defecto 0.6 → **0.45**. |
| `Assets/Scripts/PostProcessingSetup.cs` | Valores exactos de `POST_PROCESSING_SPEC` (ver §5.3). |
| `Assets/Scripts/GameFeelController.cs` | Bases alineadas con el nuevo perfil: vignette 0.35, exposure −0.5, saturación idle −8 (grabando −28). Sin esto, el controlador pisaba el spec cada frame. |

---

## 4 — ESTADOS VISUALES DEL ECO (ECHO_GRAMMAR Tabla 8.1)

| Estado | Duración | Implementación |
|---|---|---|
| **Recording** | mientras se mantiene E/R | Rim cyan `#4FC3E8` solo jugador (`EchoRecorder.EnableRecordingRim`). |
| **Latency** | 0.8 s (`CONSTANTS_REGISTRY echo.latency_seconds`) | Alpha 0.2, animator congelado (`speed = 0`), voz retenida. `Start()` no pisa el alpha (guard `_latencyRemaining`). |
| **Playback** | duración grabada | `Echoes/EchoLiminal`: alpha 0.45, `_ScanlineFreq 40`, `_ChromaticAberration 0.025`. |
| **Residual** | 2.5 s (`echo.residual_seconds`) | Swap a `Echoes/AnalogGhost` (Bayer dither, `_FPS 15`), alpha 0.3 → 0, voz en fade (no corte). |

## 5 — VALORES CANÓNICOS APLICADOS

### 5.1 Tokens (hex de `CONSTANTS_REGISTRY.yaml`, Level 2 — RULE-SOT-001B)

`void-black #0A0A0D` · `corridor-navy #1C2430` · `fluorescent-sick #C9D4B0` ·
`memory-amber #FFBF00` (emission 1.2) · `echo-cyan #4FC3E8` (alpha 0.45) ·
`wrongness-red #B23A3A` · `institutional-teal #2B4A4A` · `faded-mustard #5A4A2E` ·
`sage-green #3A4A38` · `dusty-rose #4A3438`

⚠ `#E8B262` para memory-amber está **prohibido** (`CONS-MAT-001`) aunque
siga apareciendo en `Docs/ExecutableSpecs/visual/materials.yaml` (ver §7).

### 5.2 Iluminación por capítulo (densidades fijas del catálogo `lighting_profiles.yaml`)

| Cap. | Niveles | Fog | Density | Ambient |
|---|---|---|---|---|
| I | 1–3 | `#1C2430` | 0.008 | `#0F141A` |
| II | 4, 5, 8 | `#2E3024` | 0.010 | `#1A1C14` |
| III | 6, 7, 9 | `#2A1E1E` | 0.012 | `#140E0E` |
| IV | 10, 11 | `#3B3024` | 0.015 | `#1E1812` |
| V | 12, 13 | `#1A1020` | 0.020 | `#0C0810` |
| VI | 14, 15 | `#0A0A0D` | 0.002 | `#FFFFFF` |

Sol en todos: 0.85 lux, `#F2F2FF`, rotación (50, −30, 0). Ambient Flat,
intensity 0.15, sin skybox, sin reflejos. Fog volumes en pasillos con eje
mayor >18 m (y proporción <0.5) y espacios abiertos >30 m, detectados por
bounds reales de renderers (robusto ante el escalado ×2 del bootstrap).

### 5.3 Post-procesado global

Bloom 0.25 / threshold 0.9 / scatter 0.7 · Vignette 0.35 / 0.4 / `#0D0D1A` ·
ColorAdjustments: contrast 15, saturation −8, postExposure −0.5 ·
Tonemapping None. (Coincide 1:1 con `POST_PROCESSING_SPEC.md`.)

### 5.4 Recordings narrativos (`Assets/Data/EchoRecordings/`)

| Nivel | `imposedEchoData` | `ambientEchoData` |
|---|---|---|
| L05 | `Aiden_Voice_Fragment` | — |
| L10 | `Lyra_Voice_Fragment` | `Lyra_Ambient_Echo` |
| L13 | `Aiden_Forced_Echo` | `Conversation_Fragment` |

Son placeholders sin frames (el blueprint conserva la referencia; los frames
se graban con herramienta de captura). Nota: la cadena de modos avanzados
del eco sigue rota — `EchoModeController` es huérfano y `EchoRecorder.SetMode`
ignora `mode`/`degradation` (fuera del alcance de este pase).

---

## 6 — DECISIONES Y SUSTITUCIONES

1. **10 prefabs amber de L06–L15 no existen** (`Prop_BeakerAmber`,
   `Prop_ClockHandAmber`, `Prop_DrawingAmber`, `Prop_KeyAmber`,
   `Prop_CompassAmber`, `Prop_LocketAmber`, `Prop_MirrorFrameAmber`,
   `Prop_HourglassAmber`, `Prop_RibbonAmber`, `Prop_LetterAmber`).
   El decorador intenta el prefab exacto y si falta usa un sustituto
   existente tintado amber (p. ej. `Prop_StoppedClock`, `Prop_ChalkDrawing`,
   `Cronometro`, `Prop_BlankBook`). Si arte crea los prefabs canónicos, se
   usan automáticamente. Cada sustitución queda en el log del pase.
2. **Densidad ambiental de props NO se duplica**: la cubre el Environment
   Pass existente. Este pase solo añade la capa narrativa (1 prop amber +
   1 microescena por nivel).
3. **Cap. VI con fog `#0A0A0D`** según el brief; el catálogo dice `#F0F4FF`
   ("Resolutive Clarity"). Cambiar a blanco es una línea en
   `EchoesTechnicalArtPass.Chapters["VI"]`.
4. **Coordenadas de la Tabla 8.2** se interpretan como locales al módulo
   declarado (si el módulo existe en la escena) o como posición de mundo
   (fallback, anotado en el reporte). La Y=0.0 de Level_03 se eleva a 0.5
   (PROP_GRAMMAR Tabla 8.3 exige Y ∈ [0.5, 1.8]).
5. **Microescenas MIC-001..004**: el spec define composición en prosa sin
   offsets — los offsets locales del decorador son deterministas (mismo
   resultado en cada ejecución) y editables en `SpawnMicroScene`.
6. **Alphas del eco** según `ECHO_GRAMMAR`/registro (0.45 playback, residual
   0.3→0); el brief decía "0.45 → 0.10" para echo-cyan — divergencia menor
   resuelta a favor del spec canónico.

## 6B — POST-MERGE CON LA ARQUITECTURA GREYBOX (2026-07-27)

El commit `c1f63671` de `main` (posterior al fork de esta rama) introdujo la
arquitectura nueva y se fusionó a `ws01-cleanup`. Impacto sobre este pase:

- **Compatible sin cambios**: el `EchoesModuleFactory` reescrito sigue
  consumiendo `EchoesMaterialLibrary` (FloorMat, WallTealMat, MemoryMat…),
  así que los hex/emisiones corregidos aplican también a la arquitectura
  nueva. `LevelBlueprint` conserva los campos de atmósfera y
  `imposedEchoData`/`ambientEchoData` como `EchoRecordingData`.
- **`LevelExit` corregido en main**: el bypass del chequeo de desbloqueo ya
  no existe (`BindGoal` consulta `goal.IsReady`). Advertencia retirada.
- **Enum `ModuleType` renumerado por main** (31–44): desaparecen
  `SchoolBathroom`, `SchoolMaintenanceCorridor`, `SchoolEmergencyCorridor`
  y `SchoolOffice`; aparecen `SchoolEntrance` y `GhostBridge = 44`. Los
  anclajes de la Tabla 8.2 para L09/L11/L12 referencian módulos que ya no
  existen → siempre usan el fallback de mundo.
- **Blueprints rediseñados** (L09–L15) nombran módulos de forma semántica
  (`ZonaA_UmbralInterior`, `PlacaPatio`…): el decorador ancla por posición
  de mundo con **snap al suelo por raycast** (añadido post-merge).
- **Escenas greybox** (`Level_XX_SchoolGreybox.unity`, fase 1: solo
  arquitectura + NavMesh): excluidas del pase de arte y del bake de
  lightmaps (`EchoesLightingBakePipeline` ahora filtra TEST/greybox).
- ⚠ **La validación greybox de main falla en los 15 niveles**
  (`FAIL-ARC-RHYTHM`, `FAIL-NAV-ROUTE`, `FAIL-NAV-COVERAGE` —
  `Reports/generated/greybox_validation.json`). El greybox está subido
  como WIP; el pase de arte no depende de él, pero el playthrough final sí.

## 7 — PENDIENTES (requieren el editor de Unity)

- [ ] Ejecutar `Run Full Pass (All Levels)` y commitear escenas/blueprints/materiales/`.meta` resultantes.
- [ ] Primera corrida genera la **baseline** en `Docs/Art/ReferenceFrames/` (no existía — las vistas se marcan `baseline_created`; la regresión ≤2 %/SSIM ≥0.98 aplica desde la segunda corrida).
- [ ] `Reports/generated/visual_regression_report.json` (lo escribe la validación).
- [ ] Playthrough funcional de los 15 niveles. (El bypass de `LevelExit` quedó corregido en el merge con main — ahora la salida sí respeta el objetivo.)
- [ ] Resolver los fallos greybox `FAIL-ARC-RHYTHM` / `FAIL-NAV-ROUTE` / `FAIL-NAV-COVERAGE` en los 15 niveles (`greybox_validation.json`).
- [ ] Corregir `#E8B262` en `Docs/ExecutableSpecs/visual/materials.yaml` (y los 4 hex divergentes: institutional-teal, faded-mustard, dusty-rose, fluorescent-sick) vía `CHANGE_CONTROL.md`.
- [ ] Decidir si Cap. VI usa `#0A0A0D` (brief) o `#F0F4FF` (catálogo) para N15.
- [ ] Crear los 10 prefabs amber canónicos de L06–L15 (elimina las sustituciones).
