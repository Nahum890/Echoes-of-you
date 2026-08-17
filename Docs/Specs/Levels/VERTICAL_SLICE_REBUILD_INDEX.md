# Vertical Slice Rebuild — N01 / N02 / N03 (Index)

**Estado:** Specs authored · Manifests builder-ready · Shim implementado y compilando.
**Fecha:** 2026-08-16
**Principio rector:** *Construir primero un lugar; después integrar el puzzle.* No salas de puzzles.

---

## Artefactos producidos

### Especificaciones ejecutables ( Specs/Levels/ )
- `LEVEL_SPEC_01.md` v4.0 — **Aula Ausente** (Teaching: espacio, movimiento, concepto de Eco, pasado↔presente).
- `LEVEL_SPEC_02.md` v4.0 — **Reloj Roto** (Experimentation: timing, repetición, relación multi-sala).
- `LEVEL_SPEC_03.md` v4.0 — **Bifurcación de Lyra** (Decision: bifurcación real, línea de visión, sombra diegética, flag de habilidad).

Cada spec contiene las secciones A–J, alineadas con el brief:
- **A** Design Intent (función del nivel).
- **B** Arquitectura: room graph, circulación, anchuras, alturas, puertas, escaleras, líneas de visión, landmarks, espacios de respiración.
- **C** Props: cada prop con ROOM ROLE / PLACEMENT RULE / SCALE / ROTATION / CLEARANCE / NAVIGATION IMPACT / NARRATIVE ROLE.
- **D** Puzzle: Teaching → Experiment → Discovery → Execution → Payoff, con la pregunta-guía "¿qué está intentando comprender el jugador?".
- **E** Cámara (perfiles reales de `LevelCameraProfiles`).
- **F** Iluminación funcional (tokens + luces diegéticas + boost automático).
- **G** Eco — configuración (`maxEchoes`, `maxRecordSeconds`, modo).
- **H** Validación: V-/S-/legibilidad, anti-softlock, momento aha con timing objetivo.
- **I** Playtest ciego (criterios y métricas a registrar).
- **J** (solo N03) Sincronización con progresión.

### Manifests builder-ready ( Specs/Levels/manifests/ )
- `Level_01_Module_Manifest.yaml`
- `Level_02_Module_Manifest.yaml`
- `Level_03_Module_Manifest.yaml`

Esquema compatible con `LevelBlueprint.modules` (`ModulePlacement`: name/type/position/rotation/scale/customData/targetSignals) + extensión `puzzle_components` + `props` + `narrative` + `lighting`. Consumible por el builder existente (`EchoesNewProductionBuilder`).

### Shim de código
- `Assets/Scripts/Narrative/EchoCapabilityUnlocker.cs` — puente `GoalTrigger.SatisfactionChanged` → `VN_EndingFlags.SetFlag("unlock_future_echo")` + `NarrativeSaveBridge.Save()`. Compila limpio (0 errores, 0 warnings propios). Resuelve el gap identificado en la spec N03 §J.

---

## Sistemas existentes reutilizados (verificados contra código real)
- `PressurePlate` (layer 11, acceptPlayer/acceptEcho/autoReleaseTimer, `PressedChanged` event).
- `DoorController` (array de `plates`, `latchOpen`, `invertLogic`, `SetOpenState`).
- `TimedMovingPlatform` (plate-driven, `activeLocal`/`inactiveLocal`, `fastReturn`/`returnMultiplier`).
- `GhostBridge` (PuzzleSignal-driven collider + visuals).
- `LevelGoal` + `GoalTrigger` (`anyTriggerSatisfiesGoal`, `requiredTriggerCount`, `skipEscapeSequence`).
- `LevelExit` (`nextSceneName`, `completionToast`, `BindGoal`, `SetUnlocked`).
- `EchoRecorder` (hold R, 30Hz, `maxEchoes`, `maxRecordSeconds`, SoftReset con Q).
- `LevelCameraProfiles.TryGet(sceneName)` — N01=`WideLiminal`, N02=`DynamicFollow`, N03=`SideCinematic`.
- `LevelEnvironmentBootstrap` (auto-scale ×2, `BoostEarlyLevelLighting` para niveles 1–5, material styling liminal).
- `VN_EndingFlags.SetFlag/GetFlag` + `NarrativeSaveBridge.Save/Load` (persistencia narrativa).

---

## Validación pendiente (no aprobar porque compile)
La verify de specs/manifests es review estática (completada). La validación funcional **requiere**:
1. Generar/construir las escenas Level_01/02/03 desde los manifests (vía builder existente o manualmente).
2. Playtest ciego con ≥1 jugador por nivel, sin intervención, registrando las métricas de la sección I de cada spec.
3. Aplicar la checklist V-/S- de cada spec.

**No construir N04 hasta que N01–N03 pasen playtest ciego.**
