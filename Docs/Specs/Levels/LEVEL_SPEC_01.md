# LEVEL_SPEC_01.md — Level 01 Blueprint Specification (Aula Ausente)
## Spec ID: LEVEL-SPEC-01
## Version: 4.0 (Vertical Slice Rebuild — Place-First)

---

## A. DESIGN INTENT (Función)
Introducir: espacio, movimiento, concepto de Eco, relación pasado↔presente.

**No construir una habitación vacía con botón y puerta.** Debe parecer parte de un colegio real. Primero un lugar; después el puzzle integrado en ese lugar.

**Flujo objetivo:** Entrada → espacio de exploración → primer descubrimiento → primer uso del Eco → puzzle simple → recompensa → transición.

## B. ARQUITECTURA (Room graph + circulación primero)

### B.1 Room Graph (5 nodos + 1 ramificación visual)
```
[Entrada/Recepción] → [Pasillo principal] → [Aula Lyra (pasado)] ⇄ [Aula Lyra (presente)] → [Invernadero (recompensa)] → [Transición N02]
                                            └── ventanal de línea de visión al invernadero (landmark)
```
- **Tipo de grafo:** Lineal con "peek-ahead" (el jugador ve el invernadero antes de poder alcanzarlo).
- **少数民族 razón:** el peek-ahead genera motivación y enseña que la exploración recompensa; no es una sala vacía.

### B.2 Circulación
- Eje principal en +Z. El jugador siempre avanza "hacia adelante".
- Una sola bifurcación arquitectónica falsa: el pasillo tiene un candado cerrado a la izquierda (Aula Lyra presente). El jugador debe ir primero al aula pasado (derecha).
- Anchura de circulación mínima: **1.8 m** entre mobiliario (permite paso del CharacterController r=0.36 + cámara).

### B.3 Anchuras y Alturas (escena, sin multiplicar por LevelGeometryScale=2)
| Nodo | Ancho (X) | Largo (Z) | Alto (Y) | Nota |
|------|-----------|-----------|----------|------|
| Entrada | 6.0 | 5.0 | 4.0 | techo alto = Respiración |
| Pasillo | 3.0 | 18.0 | 3.2 | comprimido = Tensión leve |
| Aula pasada | 10.0 | 10.0 | 3.8 | = espacio para摆放 pupitres |
| Aula presente | 10.0 | 10.0 | 3.8 | = igual forma que pasada (eco legible) |
| Invernadero | 8.0 | 8.0 | 5.0 | = recompensa abierta |

**Regla de alturas:** el paso pasillo→aula contiene una escalón de **0.5 m** (sensación de ascenso al "recuerdo").

### B.4 Puertas
- `Door_Entrance`: doble puerta de cristal, abierta, sin lógica (puro paso).
- `Door_LyraPast`: marco de madera, abiernta, sin lógica.
- `Door_LyraPresent`: **puerta cerrada** (lógica: la abre una `PressurePlate` eco-only en el aula pasada). Esta es la puerta de puzzle.
- `Door_Greenhouse`: puerta abierta, marco ligero (recompensa).

### B.5 Escaleras
- Un único escalón de 0.5 m entre pasillo y aula pasada. No usar rampa (el escalón enseña al jugador el cambio vertical).

### B.6 Líneas de visión y Landmarks
- **Landmark A (narrativo):** Pizarra antigua en aula pasada, visible a través de `Door_LyraPast` desde el pasillo.
- **Landmark B (motivación):** Ventanal del invernadero visible desde el pasillo a través del aula presente (luz cálida + follaje).
- **Línea de visión Eco:** desde el centro del aula presente, el jugador puede ver la silueta del pupitre-fantasma a través del ventanal entre las dos aulas (cuando el Eco se reproduce).

### B.7 Espacios de respiración
- Entrada (techo 4.0 m): el jugador aparece y mira alrededor.
- Aula pasada: espacio abierto de 10×10, permite caminar en círculo y comparar.
- Invernadero: espacio abierto final.

## C. PROPS (cada prop con rol)

| Prop | Room Role | Placement Rule | Scale | Rotation | Clearance | Nav Impact | Narrative Role |
|------|-----------|----------------|-------|----------|-----------|------------|----------------|
| `Prop_Coat` (Abrigo colgado) | Entrada | Perchero junto a la puerta, -1.5m del centro | 1:1 | Mirando a la puerta | 0 m | Ninguno (no bloquea) | Lyra vino aquí. Primer hilo narrativo. |
| `Prop_Locker` (×3, no ×5) | Pasillo | Lado izquierdo, spacing 1.2m | 1:1 | Frente al pasillo | 1.8m a la derecha | Ninguno | Lockers vacíos = escuela abandonada. |
| `Prop_RelojPared` (Reloj parado) | Pasillo | Pared del fondo del pasillo, 2.4m altura | 1:1 | Pegado a pared | — | Ninguno | Tiempo detenido (eco). |
| `Prop_Pizarra` (Pizarra antigua) | Aula pasada | Pared norte, centrada | 2.0×1.2 | Pegado a pared | 1.5m libre al frente | Ninguno | Landmark narrativo A. |
| `Prop_PupitreDoble` (Aula pasada) | Aula pasada | Centro, posición exacta donde el eco lo recrea | 1:1 | 0° | 1.5m alrededor | **Sí**: soporte para alcanzar interruptor | Objeto "pasado" central del puzzle. |
| (ningún pupitre en Aula presente) | Aula presente | — | — | — | — | — | La ausencia es el clue: "falta algo". |
| `Prop_PlantaMaceta` (×2) | Invernadero | Esquinas opuestas | 1:1 | 0° | 0.5m | Ninguno | Vida, recompensa orgánica. |
| `Prop_Notebook` (Lyra) | Invernadero | Mesa baja centrada | 1:1 | 0° | 1.0m | Ninguno | Recuerdo-restaurado (lore). |

**Reglas estrictas forbidden:** props superpuestos, escalas arbitrarias, muebles bloqueando circulación, objetos flotando, ≥5 copias idénticas consecutivas, props que tapen puertas o placas.

## D. PUZZLE (Teaching → Experiment → Discovery → Execution → Payoff)

**Pregunta-guía:** "¿Qué está intentando comprender el jugador?" → Respuesta válida: *"qué significó el pupitre que ya no está"* — no *"solo llegar a la puerta"*.

| Fase | Acción del jugador | Insight | Implementación |
|------|-------------------|---------|----------------|
| **Teaching** | Camina por el aula presente; ve el espacio vacío donde iría un pupitre (marca de polvo en el suelo + un `Decal_PapelSuelo`). | "Falta algo aquí." | Decal + `TutorialTrigger` mostrando "Mantén R para grabar tu paso". |
| **Experiment** | Va al aula pasada, camina sobre el pupitre-origen (punto marcado con `NarrativeProp_ChalkDrawing`), mantiene R para grabar. | El Eco repite el movimiento sobre el pupitre físico. | `EchoRecorder` (hold R, 30Hz, ≥2 frames). `PressurePlate_LyraEcho` configurada `acceptPlayer=false; acceptEcho=true; autoReleaseTimer=8s`. Esta placa está debajo del pupitre, en el aula presente. |
| **Discovery** | El eco, al reproducirse, recorre el aula pasada y*al pasar por la posición equivalente en el aula presente* presiona la placa eco-only. | "El eco puede activar lo que yo, presente, no puedo." | `PressurePlate_LyraEcho.PressedChanged` → `DoorController_LyraPresent.SetOpenState(true)`. |
| **Execution** | El jugador cruza al aula presente (ahora abierta) y alcanza el interruptor alto de la pared del fondo (a 3.2 m). Para alcanzarlo, escala sobre el **pupitre-fantasma** del eco, que mientras se reproduce sigue presionando la placa sosteniendo la puerta. | Coordinación cuerpo+eco (suave, sin timing estricto en N01). | El interruptor es un segundo `PressurePlate_SwitchHigh` (`acceptPlayer=true; autoReleaseTimer=0` = se quedan). Esta placa abre `Door_Greenhouse`. |
| **Payoff** | La puerta del invernadero se abre. Luz cálida entra. El jugador camina, lee el `Prop_Notebook`, aparece el toast "Primero recuerdas." | Recompensa + avance narrativo + transición natural a N02. | `GoalTrigger_Notebook` → `LevelGoal` → `LevelExit.nextSceneName="Level_02"`. `LevelExit.completionToast="Primero recuerdas."` |

### D.1 Objetos de puzzle (mapa)
```yaml
puzzle_components:
  - name: "PressurePlate_LyraEcho"
    type: PressurePlate
    layer: 11 (PressurePlate)
    accept_player: false
    accept_echo: true
    auto_release_timer: 8.0
    position: {x: 0.0, y: 0.08, z: 33.0}   # centro aula presente
    target_door: "Door_LyraPresent"

  - name: "Door_LyraPresent"
    type: DoorController
    latch_open: false        #uelve a cerrar si la placa se suelta
    position: {x: 0.0, y: 0.0, z: 30.0}

  - name: "PressurePlate_SwitchHigh"
    type: PressurePlate
    accept_player: true
    auto_release_timer: 0.0  # latch permanente
    position: {x: 0.0, y: 3.2, z: 38.0}
    target_door: "Door_Greenhouse"

  - name: "Door_Greenhouse"
    type: DoorController
    latch_open: true
    position: {x: 0.0, y: 0.0, z: 41.0}

  - name: "GoalTrigger_Notebook"
    type: GoalTrigger
    position: {x: 0.0, y: 0.9, z: 46.0}
    linked_exit: "LevelExit_N02"
```

## E. CÁMARA
- **Profile:** existente en `LevelCameraProfiles.TryGet("Level_01")` → `EchoesCameraIdentity.WideLiminal` (offset [-1.5, 2.5, -3.5], FOV 50°). No requiere cambios en `LevelCameraProfiles`.
- **Auto-frame triggers:** al entrar al aula presente, el `EventCameraDirector` enfoca el pupitre-fantasma del eco (1.5s) para legibilidad del efecto.

## F. ILUMINACIÓN FUNCIONAL
- **Token:** `liminal_n01` (ya existe preset "liminal" aplicado por `LevelEnvironmentBootstrap`).
- **Boost:** `BoostEarlyLevelLighting` level=1 → 2.8× (ya automático para niveles 1–5).
- **Luz cálida del invernadero:** un `PointLight` 4200K, intensidad 2.2, rango 8, dentro del invernadero. Visible desde el pasillo = motivación.
- **Fog:** densidad reducida 0.35× por boost (visibilidad = legibilidad del eco).

## G. ECO — CONFIGURACIÓN
```yaml
echo_system:
  enabled: true
  max_echoes: 1
  max_record_seconds: 12
  mode: Standard
  record_future: false
  key_hint: "R"  # mantener para grabar
```

## H. VALIDACIÓN (no aprobar porque compile)

**VALID-N01:**
- `[V-L01-001]` Spawn libre de colisión (player no flota, no cae).
- `[V-L01-002]` Movimiento sin trabas desde Entrada→Pasillo→Aula pasada.
- `[V-L01-003]` Cámara Learning no choca con paredes del pasillo.
- `[V-L01-004]` Eco registrado en aula pasada reproduce y presiona `PressurePlate_LyraEcho` en ≤2s.
- `[V-L01-005]` `Door_LyraPresent` abre y cierra según la placa eco.
- `[V-L01-006]` Interruptor alto solo alcanzable escalando el pupitre-fantasma (no por salto normal).
- `[V-L01-007]` `Door_Greenhouse` abre de forma permanente (latch).
- `[V-L01-008]` `LevelExit` carga `Level_02`.

**ANTI-SOFTLOCK:**
- `[S-L01-001]` Si el eco no reproduce correctamente, el jugador puede grabar otro (auto-release 8s permite reintentar).
- `[S-L01-002]` El interruptor alto tiene área de trigger de 2×2 m, no requiere pixel-perfect.
- `[S-L01-003]` Key `Q` ejecuta SoftReset de posición y slots de eco (ya en `EchoRecorder`).

**Legibilidad / momento aha:**
- El pupitre-fantasma debe ser visible y bem diferenciado (rim light cyan + material `Runtime_EchoPlate_Blue`). El "aha" ocurre cuando el jugador entiende que el eco puede activar lo que él físico no. Timing objetivo: ~30–45 s tras el primer eco exitoso.

## I. PLAYTEST CIEGO (obligatorio antes de N04)
- 1 jugador que no conozca la solución.
- No intervenir.
- Registrar: dónde se detuvo, dónde se perdió, qué miró, qué intentó, cuándo entendió, cuánto tardó.
- Criterio de aprobación: el jugador entiende por sí solo que el eco es una "llave del pasado" en ≤5 min, sin softlock, sin pedir ayuda.

## 7. RULES (compatibles con LevelBlueprint existente)
- `[RULE-L01-001]` Nivel contiene: `PlayerStart`, `SchoolEntrance`, `SchoolCorridor`, `SchoolClassroom` (×2: pasada+presente), `GhostBridge` o `DoorController`×2, `LevelExit`.
- `[RULE-L01-002]` Fog color `#1C2430`, densidad 0.008 (pre-boost).
- `[RULE-L01-003]` Camera profile `WideLiminal` (existente en `LevelCameraProfiles["Level_01"]`).
- `[RULE-L01-004]` `Prop_Coat` en `SchoolEntrance`; `Prop_Notebook` en invernadero (recompensa).
- `[RULE-L01-005]` **NUEVO:** prohibido hazard zones o kill volumes en N01.
- `[RULE-L01-006]` **NUEVO:** el aula presente debe estar mobiliario-vacía excepto por el decal de polvo (luggres narrativo de ausencia).

## 14. CHANGE HISTORY
- **v4.0 (2026-08-16)**: Rebuild vertical slice. Place-first-then-puzzle. Eco teaching con pupitre-fantasma + interruptor-alto. Invernadero como recompensa motivacional (peek-ahead). Eliminada la placa-en-pasillo simple del v3.0.
- **v3.0**: Spec ejecutable original.
