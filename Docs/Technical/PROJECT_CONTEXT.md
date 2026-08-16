# PROJECT_CONTEXT.md
## Echoes of You — Contexto activo del proyecto

Este archivo es la fuente de verdad de contexto para cualquier IA que trabaje
en el proyecto. Leer completo antes de ejecutar cualquier tarea. Si algo en
este archivo contradice una sugerencia externa, este archivo gana.

Para dirección de diseño, reglas visuales, y filosofía del gameplay, leer
`ECHOES_BIBLE.md` — este archivo cubre arquitectura técnica.

**Última verificación contra código:** 2026-07-26, rama `ws01-cleanup`
(auditoría completa de `Assets/Scripts` y `Assets/Editor`).

---

## 1 — QUÉ ES EL JUEGO

**Echoes of You** — puzzle 3D narrativo en tercera persona.

**Stack técnico verificado:**
- **Unity 6000.4.3f1** (Unity 6)
- **URP 17.4.0** (`com.unity.render-pipelines.universal`)
- **Cinemachine 3.1.7** — API 3.x (`Unity.Cinemachine`, `CinemachineCamera`).
  No usar API 2.x (`CinemachineVirtualCamera`) en código nuevo.
- **UI Toolkit** (UIDocument + UXML/USS) para todo el UI.

El jugador graba sus propios movimientos (hasta **12 segundos estándar,
20 segundos narrativo** — ver `DECISION-ECH-DURATION` en `decisions.yaml`)
y los reproduce como un "eco" — un cuerpo fantasma que repite exactamente lo
que grabó, con colisión activa. El eco no combate, no improvisa, no se puede
deshacer una vez reproducido. Esa irreversibilidad es el tema del juego.

**Cómo está implementado el eco (verificado):**
- `EchoRecorder.maxRecordSeconds = 12f` (serializado, ajustable por
  blueprint). El tope de 20 s "narrativo" es decisión de diseño por nivel,
  no un límite distinto en código.
- Muestreo a **30 Hz en `FixedUpdate`** (`RecordFrame { time, position,
  rotation }`); la reproducción interpola con **Catmull-Rom** + SLERP, por
  eso se ve fluida a cualquier framerate.
- Captura la **voz del micrófono a 48 kHz** durante la grabación y la
  reproduce con el eco (normalizada a pico 0.82).
- `EchoRecorder.maxEchoes = 3` por defecto; `LevelBlueprint.maxEchoes = 1`
  por defecto. Al superar el límite se desvanece el eco más viejo no
  bloqueado (fade 0.65 s).
- El eco usa `CharacterController` y hace `Move()` real — empuja bloques y
  pisa placas. Loop infinito con `degradationPerReplay` (offset temporal
  acumulado por repetición).

---

## 2 — ARQUITECTURA ACTUAL (estado verificado, 2026-07-26)

### Pipeline de generación de niveles (Editor)

**Builder activo y único:** `Assets/Editor/EchoesNewProductionBuilder.cs`.
- Menú: `Echoes of You/Production/Build All Blueprint Levels`.
- Itera **todos** los `LevelBlueprint` (ScriptableObjects) encontrados en
  `Assets/Data/Levels/` — hoy hay exactamente 15 (`Level_01_Blueprint` a
  `Level_15_Blueprint`). El conteo es emergente, no hardcodeado.
- Antes de construir ejecuta `ExecutableSpecValidator.ValidateProject()`
  contra los YAML de `Docs/ExecutableSpecs/`.
- Flujo por nivel: materiales → escena nueva → atmósfera → luces → módulos
  vía `EchoesModuleFactory` → cableado de señales (8 pasadas) → jugador y
  cámara → UI → runtime → guardar escena.

**Builders históricos: ELIMINADOS del repo.** `EchoesLevelBuilder.cs`,
`EchoesProductionBuilder.cs` y sus parciales (`_Levels1_5`, `_Levels6_10`,
`_Levels11_15`) **ya no existen en disco**. Solo sobreviven como texto en
`Tools/Scripts/*.py`, logs y docs viejos. Si un documento los menciona como
"referencia histórica en el repo", está desactualizado.

**ADVERTENCIA CRÍTICA — auto-rebuild (hay DOS mecanismos, ambos
desactivados hoy):**
1. `EchoesQueuedProductionRebuild.cs` — `[InitializeOnLoad]` comentado
   (línea 7) y sin archivo `.flag` en disco. Disparador manual:
   `Echoes of You/Production/Queue Rebuild In Open Editor`.
2. `EchoesAutoBuilderHelper.cs` — mismo patrón, `[InitializeOnLoad]`
   doblemente comentado ("DISABLED for Environment Pass").

**No reactivar ninguno de los dos.** Un rebuild hace `NewScene(EmptyScene)`
y destruye tanto las ediciones manuales de las 15 escenas como **todos los
props colocados por el Environment Pass** (ver abajo).

**Greybox escolar (fase 1, desde 2026-07-26):**
`SchoolGreyboxProductionBuilder.cs` (menú `Echoes of You/Production/Build
All School Greybox Levels`) genera las 15 escenas paralelas
`Level_XX_SchoolGreybox.unity` — solo arquitectura + NavMesh, sin puzzles,
props, luces ni cámara, con validación automática a
`Reports/generated/greybox_validation.json`. ⚠ Estado actual: los 15
niveles FALLAN (`FAIL-ARC-RHYTHM`, `FAIL-NAV-ROUTE`, `FAIL-NAV-COVERAGE`).
`BuildSchoolGreyboxLevels.cs` es un builder auxiliar del mismo commit.
El enum `ModuleType` fue renumerado (31–44): ya no existen
`SchoolBathroom`/`SchoolMaintenanceCorridor`/`SchoolEmergencyCorridor`/
`SchoolOffice`; se añadieron `SchoolEntrance` y `GhostBridge`. Hay una
carpeta nueva `Assets/Scripts/Puzzle/` (AmbientEchoData, ImposedEchoData,
InversionCamera, PressurePlateEchoOnly, RecordFutureExit).

**Environment Pass (colocación de props — pipeline separado y posterior
al builder):**
`EnvironmentPassDataGenerator` (genera `LevelDataSO` en
`Assets/ScriptableObjects/EnvironmentPass/`) → `EnvironmentPassDataLoader`
→ `EnvironmentPassPlacementEngine` (abre escena, respeta zonas de exclusión
de puzzle, coloca props con raycast al suelo) → `EnvironmentPassValidator`
(clearance mínimo 1.5 m). Front-ends: `EnvironmentPassPlacer` (menús
`Tools/Environment Pass/1..8`), `PropPlacementRunner` (headless),
`AutoFixZoneAndPlace`.

**Prefabs base:** `CreateBasePrefabs` → `PrefabBatchBuilder` (41 prefabs
desde el Kenney Furniture Kit) + `DecalPrefabGenerator` (4 decals).
`RoomComposer` (EditorWindow) es la vía manual paralela; exporta
`RoomTemplate` ScriptableObjects.

**Scripts Python:** `Tools/Scripts/` contiene 11 `.py` **muertos** — la
mayoría referencia clases ya eliminadas. No ejecutar ninguno sobre `.cs`;
el pipeline activo es solo C#.

### Arranque de una escena de nivel (runtime)

Orden real de inicialización:
1. `[BeforeSceneLoad]` — `PlayerAnimationRuntimeBootstrap` carga
   `Resources/EchoesLocomotionSettings`; `SceneTransitionManager` se
   auto-inicializa.
2. `[AfterSceneLoad]` — `LevelEnvironmentBootstrap` (motor de arranque de
   escena: escala geometría ×2 según `EchoesWorldMetrics.LevelGeometryScale`,
   limpia dressing legacy, aplica iluminación, inyecta cámara/HUD/perfil);
   `PostProcessingSetup` crea el Volume global URP en runtime;
   `CinemachineEventFocus.EnsureExists()`.
3. `Awake` por `[DefaultExecutionOrder]`: `-40 PlayerAdvancedLocomotion` →
   `-25 PlayerCharacterVisualSetup` → `-20 PlayerAnimationRuntimeBootstrap`
   → `-10 PlayerLocomotionAnimator` → `0` resto de sistemas.
4. Lazy: `EchoesAudioManager.EnsureExists()` al primer uso.

### Archivos de sistemas core

| Archivo | Responsabilidad |
|---|---|
| `Editor/EchoesLevelShell.cs` | Andamiaje de escena en build-time: raíces, atmósfera, luces, jugador, cámara Cinemachine, UI |
| `Editor/EchoesModuleFactory.cs` | Fábrica de geometría por `ModuleType` (47 tipos: 0-20 base, 21-30 fase 3, 31-46 vocabulario escolar) |
| `Editor/EchoesMaterialLibrary.cs` | Materiales canónicos por token de paleta (10 tokens), shaders `Echoes/*` |
| `PlayerController.cs` (+ `_Gravity`, `_Animation`, `_Visual`) | `partial class` en 4 archivos. **`CharacterController`, NO Rigidbody.** Input, salto (buffer 0.10 s + coyote 0.12 s), gravedad con zonas, animación, visual |
| `PlayerAdvancedLocomotion.cs` | Parkour: slide, ledge grab, wall run/jump, air dash. Lee `EchoesLocomotionSettings` |
| `EchoRecorder.cs` / `EchoPlayback.cs` | Grabación 30 Hz + reproducción interpolada del eco (ver §1) |
| `LevelEnvironmentBootstrap.cs` | Bootstrap runtime de escena (escala, iluminación, inyección de cámara/HUD) |
| `GameStateController.cs` | Máquina de estados de nivel + notificaciones a GameFeel/cámara |
| `LevelRuntimeController.cs` | Objetivo, **Q = soft reset** (`IResettableLevelObject`), **T (hold 0.5 s) = hard reset**, telemetría |
| `GameProgress.cs` | Progresión en **PlayerPrefs** (no JSON). Lista los 15 niveles (`LevelScenes[]` + `LevelDisplayNames[]`). |
| `SceneTransitionManager.cs` | Fade UI Toolkit + carga de escenas |
| `GameFeelController.cs` | Singleton de juice: partículas, shake, hitstop, post-proceso por evento |
| `EchoesAudioManager.cs` | Singleton de audio; AudioMixer con 4 buses: Master/Music/SFX/Echo; volúmenes en PlayerPrefs |
| `MainMenuController.cs` / `GameHUD.cs` / `PauseMenu.cs` | UI Toolkit (UXML/USS). PauseMenu "hub" apunta a MainMenu |

### Sistemas de puzzle — estado real

**Cableados y en uso en las escenas:**
`PressurePlate` (+`PressurePlateAlignment`), `DoorController`,
`TimedMovingPlatform`, `PuzzleCondition` (5 modos), `PuzzleSignal` (bus de
señal genérico), `GoalTrigger` → `LevelGoal` → `LevelEscapeSequence` →
`LevelExit`, `EchoKineticBody`/`EchoKineticZone`/`EchoKineticRole`,
`EchoShieldField`, `EchoConflictTrap`, `ChaseHazardMotor`,
`DynamicTransformMotor`, `KineticPushableBlock`, `CharacterPush`.

**Implementados pero SIN instancias en ninguna escena** (lógica viva,
contenido no construido): `PuzzleWire`, `GravityZone`,
`MovingPlatformMomentum`, `KillVolume` (solo instanciado por bootstrap),
`Paradox/ResonanceSystem` + `ResonanceZoneTrigger`, `Paradox/TemporalBridge`
(único Paradox con ruta desde el pipeline de blueprints, `ModuleType 22`).

**Huérfanos totales** (0 referencias de código y 0 instancias — no asumir
que funcionan): `GhostBridge`, `MemoryPlatform`, `EchoDisintegrationZone`,
`Echo/EchoModeController` (configurable desde `LevelRuntimeController.Start`
vía `LevelBlueprint` pero `EchoRecorder.SetMode` sigue ignorando los
parámetros `mode` y `degradation`), `EchoTemporalFragmentBurst`,
`Paradox/ErosionSystem`, `Paradox/LivingArchitectureSystem`,
`HubSceneController` + `HubPortal` (no existe escena de hub — el código
está completo y listo para cuando se cree, no eliminar),
`Lighting/LightingApplier` (la luz real la maneja
`LevelLightingSettings`), `UIBootstrap`, `ParkourPlatformMarker`,
`EchoPathHint` (`LevelBlueprint.pathHints` se consume vía
`EchoesNewProductionBuilder.SpawnPathHintLights` + instancia
`EchoPathHint` en el builder, no en runtime).

Nota: `EchoRecorder.SetMode()` ignora los parámetros `mode` y
`degradation` — solo aplica `recordFuture` y el bloqueo de slots.

### Cableado de señales (flujo real)

```
PressurePlate.PressedChanged
   ├→ DoorController (AND/OR sobre plates[]) → DoorStateChanged
   ├→ PuzzleCondition (AllPlatesSimultaneous / AnyPlateOnce / SequentialOrder / TimedHold / PlateCount)
   └→ GoalTrigger
Eco/Jugador en trigger → EchoKineticZone / EchoShieldField / EchoConflictTrap
   └→ PuzzleSignal.MarkSatisfied() → GoalTrigger
GoalTrigger → LevelGoal (todos satisfechos) → LevelEscapeSequence (persecución,
   20 s límite) → desbloquea LevelExit[] → GameProgress.MarkSceneCompleted()
   → siguiente escena
```

⚠️ `LevelExit` respeta el estado del puzzle desde el merge con la arquitectura
greybox (2026-07-27): `BindGoal` consulta `goal.IsReady` y `OnTriggerEnter`
rechaza si `!_isUnlocked`. (El bypass histórico quedó eliminado.)

### Render pipeline

**URP 17.4.0.** Prohibido `Shader.Find("Standard")` — el
`ExecutableSpecValidator` (regla `MAT-001`, fatal) lo bloquea en CI.

**Materiales: exclusivamente vía `EchoesMaterialLibrary`**, que usa
**shaders custom del proyecto**, no URP/Lit:
- `Echoes/LiminalSurface` — shader base de geometría
- `Echoes/EchoLiminal` — el eco (distorsión, aberración cromática, cap 15 fps)
- `Echoes/LiminalFogVolume` — volúmenes de niebla
- `Echoes/RetroFlatLit` — fallback legacy
- `"Universal Render Pipeline/Lit"` solo como fallback de última instancia.

Los shaders liminales **no exponen Metallic/Smoothness** — sus parámetros
son `_FresnelInvert`, `_FluorescentEdge`, `_StainNoiseScale`, `_WearHeight`,
etc. La regla vieja "Metallic = 0, Smoothness = 0.05" aplicaba a URP/Lit y
ya no describe el pipeline de materiales actual. Para materiales creados
por código en runtime existe `EchoesUrpMaterials.cs` (URP/Lit y URP/Unlit).

**Post-processing:** `PostProcessingSetup.cs` (runtime, en
`Assets/Scripts/`) crea un Volume global URP modulado por
`GameFeelController`. SSAO es Renderer Feature del URP Renderer
(`EchoesURPConfigurator`: Intensity 1.8, Radius 0.6, Quality Medium).
El paquete Post-Processing Stack v2 quedó reemplazado.

**10 tokens de paleta** en `EchoesMaterialLibrary`: `void-black`,
`corridor-navy`, `fluorescent-sick`, `memory-amber`, `echo-cyan`,
`wrongness-red`, `institutional-teal`, `faded-mustard`, `sage-green`,
`dusty-rose`.

### Cámara (estado real — Cinemachine 3.x)

**Stack activo en Level_01..15 (verificado por GUID en las escenas):**
- `FixedPuzzleCameraController` — **controlador principal de gameplay**,
  montado en `Camera.main`; dirige una `CinemachineCamera` +
  `CinemachineTargetGroup` (pesos player/goal/event/echo).
- `CinematicCameraDynamics` — capa expresiva encima (FOV por velocidad,
  dutch, drift de memoria) según `EchoesCameraIdentity` (A–E).
- `EchoCameraTargetGroupManager` — inyecta el peso de los ecos al grupo.
- `EventCameraDirector` — secuencias cortas (mirar botón → puerta → volver).
- `CinemachineEventFocus` — singleton para focos de evento puntuales.
- Perfiles: `CameraProfile` (ScriptableObject) + `LevelCameraProfiles`
  (tabla estática por nombre de escena).

**`ThirdPersonCamera.cs` existe pero NO está en ninguna escena de
producción** — solo en backups de `Assets/_Recovery/`. El código de
coexistencia anti-jitter (`CinemachineRuntimeSetup`, `CameraProfileApplier`
desactivando la TPC) es actualmente ruta muerta, mantenida por si se
reintroduce. `CinemachineGameplayDynamics` solo vive en `Level_04_TEST`.

**El contrato `[CAM-LIFECYCLE-001]` (`ActivateCinemachineForLevel`) que
figuraba en versiones previas de este documento NUNCA se implementó** — el
método no existe en ningún `.cs` y su pseudocódigo usaba API de
Cinemachine 2.x. La regla que sí sigue vigente y sí se cumple:
**exactamente 1 controlador de cámara activo por escena** (si dos escriben
el transform en `LateUpdate`, hay jitter).

---

## 3 — DIRECCIÓN VISUAL (resumen ejecutivo)

**Estética:** escuela liminal PS1/PS2 temprano. Low-poly, color plano,
niebla agresiva, iluminación dura.

**PROHIBIDO:** Modular SciFi MegaKit, Cyberpunk Kit, texturas PBR 2K/4K
de concreto o metal, monolitos, estética de ciencia ficción. Esta decisión
fue explícita y deliberada — no revertir. (`EchoesModuleFactory` excluye
el kit SciFi al resolver modelos.)

**Materiales:** solo vía `EchoesMaterialLibrary` (shaders `Echoes/*`, ver
§2). Sin normal maps, sin AO maps, sin reflejos de entorno
(`reflectionIntensity = 0`).

**Tokens de color principales** (hex canónicos de `CONSTANTS_REGISTRY.yaml`,
que manda sobre cualquier otro doc — `RULE-SOT-001B`):
- `echo-cyan` `#4FC3E8` — el eco, siempre
- `memory-amber` `#FFBF00` — objetos narrativos, Lyra (emission 1.2;
  `#E8B262` está PROHIBIDO por `CONS-MAT-001` aunque aparezca en
  `materials.yaml`)
- `corridor-navy` `#1C2430` — base de pasillos
- `wrongness-red` `#B23A3A` — peligro, uso escaso

**Estados visuales del eco** (implementados en `EchoRecorder`/`EchoPlayback`,
según `ECHO_GRAMMAR` Tabla 8.1): Recording (rim cyan solo jugador) →
Latency 0.8 s (alpha 0.2, congelado) → Playback (EchoLiminal, alpha 0.45)
→ Residual 2.5 s (AnalogGhost, alpha 0.3→0, cap 15 fps).

**Pase de arte técnico (Editor):** `EchoesTechnicalArtPass` (menú
`Echoes of You/Technical Art/…`) aplica iluminación por capítulo a
blueprints+escenas, fog volumes, props narrativos (`EchoesPropDecorator`)
y validación visual (`EchoesVisualValidationPass` →
`Reports/generated/visual_regression_report.json`). No reconstruye escenas
— respeta el Environment Pass. Detalle completo, decisiones y pendientes:
`Docs/Technical/TECHNICAL_ART_PASS.md`.

**Niebla/atmósfera:** el bug histórico de hardcodear está **corregido** —
`EchoesLevelShell.SetupAtmosphere` lee `blueprint.fogColor` y
`blueprint.ambientColor` reales. Matices verificados: `fogDensity` se
**recorta** a `[0.002, 0.04]` (ampliado para los perfiles por capítulo:
Cap. VI usa 0.002), `AmbientMode.Flat`, `ExponentialSquared`, skybox null.
Sombras: **Hard only** (soft prohibidas por `LIGHTING_GRAMMAR`), shadow
distance 40 m, máx. 48 luces por escena.

---

## 4 — ESTADO DE LOS NIVELES

**En disco:** 15 escenas `Level_01..15` + `MainMenu.unity` +
`Level_04_TEST.unity` (prueba) + 15 escenas `Level_XX_SchoolGreybox.unity`
(fase 1 greybox, sin luces/props por diseño). `CameraPassQA` excluye TEST;
`EchoesLightingBakePipeline` y el pase de arte técnico filtran TEST y
greybox. **No existe ninguna escena de hub** — `HubSceneController`
y `HubPortal` son código huérfano.

Organización por capítulos emocionales:

| # | Emoción | Capítulo | Espacio principal |
|---|---|---|---|
| 1 | Desorientación | I — Persistencia | Entrada + pasillo que se repite |
| 2 | Repetición | I — Persistencia | Pasillo de aulas idénticas |
| 3 | Indecisión | I — Persistencia | Bifurcación de pasillos especulares |
| 4 | Espera | II — Coordinación | Aula con timing vertical |
| 5 | Culpa | II — Coordinación | Pasillo laberinto de timing horizontal |
| 6 | Negación | III — Confianza | Biblioteca, puente espectral |
| 7 | Evasión | III — Confianza | Patio trasero, grabación anticipada |
| 8 | Autosabotaje | II — Coordinación | Sala de profesores, dos ecos |
| 9 | Control | III — Confianza | Patio exterior — único espacio abierto |
| 10 | Recuerdos | IV — Optimización | Aula de Lyra, eco ambiental revelador |
| 11 | Conexión | IV — Optimización | Escalera central, grabación limitada |
| 12 | Conflicto | V — Consecuencia | Gimnasio/laboratorio |
| 13 | Verdad | V — Consecuencia | Aula de Lyra fragmentada, grabación única |
| 14 | Aceptación | VI — Aceptación | Fragmentos flotantes en void-black |
| 15 | Integración | VI — Aceptación | Pasillo del Nivel 1, ahora con salida real |

**⚠️ Desajuste de conteo RESUELTO:** `GameProgress.LevelScenes[]` ahora
lista `Level_01..Level_15` (`TotalLevels == 15`). Los niveles 11–15
están dentro de la progresión, el desbloqueo y la selección de nivel del
menú. Verificado en `GameProgress.cs:15-32`.

---

## 5 — LO QUE NO HACER (errores documentados de sesiones anteriores)

- **No revertir la dirección visual a brutalismo/sci-fi.** Ya se hizo, ya se
  deshizo. Hay razones concretas documentadas en `ECHOES_BIBLE.md`.
- **No crear builders adicionales.** Solo `EchoesNewProductionBuilder.cs`.
  (Los builders viejos ya fueron eliminados del repo — no restaurarlos.)
- **No reactivar `[InitializeOnLoad]` en `EchoesQueuedProductionRebuild.cs`
  ni en `EchoesAutoBuilderHelper.cs`.** Un rebuild automático borra las
  ediciones manuales de las 15 escenas Y los props del Environment Pass
  sin aviso.
- **No ejecutar scripts Python sobre archivos `.cs`.** Los de
  `Tools/Scripts/` referencian clases que ya no existen.
- **No usar `Shader.Find("Standard")` ni materiales Built-in.** Todos los
  materiales se crean exclusivamente vía `EchoesMaterialLibrary`
  (shaders `Echoes/*`); para materiales runtime por código, usar
  `EchoesUrpMaterials`.
- **No usar constantes `SciFi*` en ningún script de producción.**
- **No asumir que un sistema funciona porque el script existe.** Ver la
  lista de huérfanos en §2 (hub, Paradox, GhostBridge, EchoModeController,
  etc.) — mucho código está diseñado pero no cableado.
- **No renombrar sistemas como sustituto de rediseñarlos.**
- **No construir más de 3 niveles nuevos antes de validar que los
  primeros 3 funcionan con jugadores reales.**

---

## 6 — CÓMO USAR ESTE ARCHIVO CON UNA IA

1. Dar este archivo + `ECHOES_BIBLE.md` antes de cualquier tarea.
2. Pedir a la IA que señale explícitamente si su propuesta contradice
   algo en alguno de los dos documentos, en lugar de aplicarlo en silencio.
3. Cualquier cambio de dirección (dirección visual, mecánica central,
   estructura de campaña) requiere actualizar este archivo antes de ejecutar.
4. Si la IA no tiene acceso a los archivos `.cs` actuales, pedirle que
   señale qué necesita leer antes de proponer cambios de código —
   no que adivine firmas de función.
