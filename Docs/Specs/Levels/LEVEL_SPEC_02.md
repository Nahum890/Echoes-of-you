# LEVEL_SPEC_02.md — Level 02 Blueprint Specification (Reloj Roto)
## Spec ID: LEVEL-SPEC-02
## Version: 4.0 (Vertical Slice Rebuild — Place-First, Timing)

---

## A. DESIGN INTENT (Función)
Introducir: timing, repetición, relación entre varias salas.

**No crear una cadena de habitaciones idénticas.** Cada aula tiene forma, contenido y rol distintos. El nivel se lee como un ala del colegio con aulas adyacentes y un reloj central roto.

**Debe existir:** espacio arquitectónico + aulas + transición + puzzle + consecuencia (multi-sala).

**Flujo objetivo:** Hall central → Aula Laboratorio (pasado) → observar ventana a Aula Ciencias (presente) → sincronizar pulsaciones repetidas → consecuencia visible en Hall → salida.

## B. ARQUITECTURA (Room graph + circulación primero)

### B.1 Room Graph
```
                        [Hall central con Reloj Roto (landmark)]
                       /                                        \
        [Aula Laboratorio (pasado)]    ←—ventana de visión—→    [Aula Ciencias (presente)]
                       \                                        /
                        \____[Atrio silencioso (consecuencia)]__/
                                          |
                                    [Transición N03]
```
- **Tipo de grafo:** Bucle (loop). El jugador sale del Hall, entra al Laboratorio, ve a través de un ventanal el Aula Ciencias, opera el puzzle, regresa al Hall (donde cambió algo), avanza al Atrio.
- **Razón arquitectónica:** un pasillo de aulas reales suele ser un bucle o tener un eje不断发展 alumno; así no es una cadena gris.

### B.2 Circulación
- Eje principal: Hall en el origen; el Laboratorio se abre a la izquierda (+X), Ciencias a la derecha (−X). Ambas aulas conectan al Atrio por la parte de atrás (−Z).
- El jugador puede mirar a través del ventanal de visión entre Laboratorio y Ciencias (línea de visión de causa-efecto).
- Anchura de pasillo entre aulas y Hall: **4.0 m**.

### B.3 Anchuras y Alturas (escena, sin multiplicar por LevelGeometryScale=2)
| Nodo | Ancho (X) | Largo (Z) | Alto (Y) | Carácter |
|------|-----------|-----------|----------|----------|
| Hall central | 10.0 | 10.0 | 5.0 | Tee alto, Reloj como landmark central |
| Aula Laboratorio (pasado) | 8.0 | 6.0 | 3.5 | Compacta, mesa central de trabajo |
| Aula Ciencias (presente) | 8.0 | 10.0 | 4.5 | Tiered (3 niveles de pupitres), perspectiva privilegiada |
| Atrio silencioso | 6.0 | 8.0 | 3.6 | Quiet, consecuencia narrativa |

**Diferenciación clave:** las dos aulas **no son idénticas**. El Laboratorio es plano y pequeño (causa); Ciencias es tiered y más grande (efecto, donde el jugador observa la plataforma moviéndose).

### B.4 Puertas
- `Door_HallLeft`: doble puerta al Laboratorio, abierta.
- `Door_HallRight`: doble puerta a Ciencias, abierta.
- `Door_AtrioConsequence`: **cerrada**, abierta por `DoorController` vinculado a `PressurePlate_AtrioEcho`.
- `Door_N03`: abierta, es la salida a Level_03.

### B.5 Escaleras
- Ciencias es tiered: 3 niveles de pupitres con escalones de 0.4 m entre niveles. Esto crea una **gradiente visual** que ayuda al jugador a encontrar el punto de observación privilegiado (la primera fila, más alta).

### B.6 Landmarks y Líneas de visión
- **Landmark A:** `Prop_RelojPared` en el centro del Hall, fixado en un soporte, 3.0 m diámetro de cara,_manecillas paradas a las 12:00. Visible desde ambas puertas aúlicas.
- **Landmark B:** `Prop_RelojPared` del aula Ciencias sincronizado visualmente con `TimedMovingPlatform` (la aguja avanza al ritmo de la plataforma).
- **Línea de visión Eco-Timing:** el ventanal entre Laboratorio y Ciencias permite al jugador ver simultáneamente su mano en la palanca (Laboratorio) y la plataforma respondiendo (Ciencias). Este es el núcleo pedagógico del timing multi-sala.

## C. PROPS (cada prop con rol — N02 es más denso que N01 pero NO repetitivo)

| Prop | Room Role | Placement Rule | Scale | Rotation | Clearance | Nav Impact | Narrative Role |
|------|-----------|----------------|-------|----------|-----------|------------|----------------|
| `Prop_RelojPared` (grande) | Hall | Centro, soporte a 3.0 m | 1.5× | 0°, frente a +Z | 2.0m radio | Ninguno | Tiempo detenido = eco. Landmark central. |
| `Prop_Escritorio` (mesa de laboratorio) | Laboratorio | Centro | 1:1 | 0° | 1.0m alrededor | **Sí**: soporta palanca sobre la mesa | Estación de trabajo del Profesor (causa). |
| `Prop_Cronometro` (palanca-puzzle) | Laboratorio | Sobre la mesa | 1:1 | 0° | 0.5m | Ninguno | Interfaz del timer. |
| `Prop_PupitreDoble` (×6, NO ×12) | Ciencias tiered | 3 filas de 2, escalonadas | 1:1 | Frente a la plataforma | 1.2m entre filas | Ninguno (alumno espectador) | Se aprecia la plataforma desde los pupitres. |
| `Prop_MesaProfesor` | Ciencias | Frente, junto a la pared con el ventanal | 1:1 | Frente a los pupitres | 1.0m | Ninguno | El profesor observaba los experimentos. |
| `Prop_RelojPared` (pequeño) | Ciencias | Pared sobre la plataforma | 1:1 | — | — | Ninguno | Landmark B: sincroniza con la plataforma. |
| `Prop_Libros` (×3) | Atrio | Repartidos | 1:1 | Variado | 0.5m | Ninguno | Silencio academic despues del escape. |

**Regla estricta:** la repetición es intencional (filas de pupitres tiered = identidad de aula). No usar ≥5 copias idénticas consecutivas en línea recta.

## D. PUZZLE (Teaching → Experiment → Discovery → Execution → Payoff)

**Pregunta-guía:** "¿Qué está intentando comprender el jugador?" → *"cómo el pasado puedo manipular el presente a distancia, en momento exacto"*.

### D.1 Mecánica central: `TimedMovingPlatform` + `PressurePlate` eco-stay

El jugador **no puede quedarse en la palanca** del Laboratorio y al mismo tiempo cruzar la plataforma de Ciencias (las salas están separadas). Necesita al eco para mantener la palanca.

| Fase | Acción del jugador | Insight | Implementación |
|------|-------------------|---------|----------------|
| **Teaching** | Entra al Hall, ve el Reloj Roto (parado = se asociará a Eco y a evitarlo). Ve dos aulas a los lados. | Exploración;(sentido de "qué es esto". | `TutorialTrigger` "Camina entre las aulas". `Prop_RelojPared` manecillas a 12:00. |
| **Experiment** | Entra al Laboratorio, tira de `Prop_Cronometro` (palanca). Se activa un timer visual (barra de progreso en pantalla via `TutorialHUD`). | Acción en una sala produce efecto en otra, mediado por tiempo. | La palanca presiona `PressurePlate_LeverEcho`(Laboratorio), que dispara `TimedMovingPlatform` en Ciencias, visible via ventanal. |
| **Discovery** | El jugador suelta la palanca para ir a cruzar la plataforma de Ciencias, pero esta se detiene al soltar (lógica de placa → plataforma). Descubre que necesita que la palanca esté sostenida. | Para resolver timing multi-sala, necesita algo que "permanezca" en su lugar. | `TimedMovingPlatform.RefreshTarget` depende de `PressurePlate_LeverEcho.IsPressed`; `fastReturn=false` para que sea evidente que se detiene. |
| **Execution** | El jugador graba un eco corto (2–4 s) en el Laboratorio que baja la palanca ↔ el eco se queda allí reproduciéndose en bucle sosteniendo `PressurePlate_LeverEcho`. Mientras tanto, el jugador corre por el Hall hacia Ciencias y cruza la plataforma móvil. La plataforma debe alcanzar el punto de cruce **durante** la reproducción del eco. | Coordinación cuerpo+eco con timing. **Repetición:** si falla (plataforma no llega), el jugador vuelve y redice el eco más largo. | `EchoRecorder` con `maxRecordSeconds=12s`. `PressurePlate_LeverEcho.acceptEcho=true; autoReleaseTimer=0.15s` (mantiene presión mientras el eco está físicamente encima en cada frame). `TimedMovingPlatform.activeLocal` = posición de cruce, `travelSpeed=2.5m/s`, `returnMultiplier=8`. |
| **Payoff** | al cruzar la plataforma en Ciencias, el jugador pisa la `PressurePlate_AtrioEcho` en el lado opuesto, que abre `Door_AtrioConsequence`. En el Atrio, un `GoalTrigger_Diario` (diario del profesor) lo marca como completo. El Reloj Roto del Hall **avanza su aguja a las 12:03** (feedback diegético multi-sala: el pasado modificó el presente). | Consecuencia tangible visual entre salas. | `LevelGoal.requiredTriggerCount=1`. `GoalTrigger_Diario.signals` activan `EventCameraDirector` para un quick shot del reloj avanzado (GameFeel feedback). |

### D.2 Objeto de puzzle (mapa)
```yaml
puzzle_components:
  - name: "PressurePlate_LeverEcho"
    type: PressurePlate
    accept_player: true
    accept_echo: true
    auto_release_timer: 0.15
    position: {x: -12.0, y: 0.08, z: 5.0}   # Laboratorio, sobre mesa
    target_platform: "TimedMovingPlatform_01"

  - name: "TimedMovingPlatform_01"
    type: TimedMovingPlatform
    inactiveLocal: {x: 0, y: 0, z: 0}
    activeLocal: {x: 0, y: 0, z: 6.0}      # recorre Ciencias
    travelSpeed: 2.5
    fastReturn: true
    returnMultiplier: 8.0
    position: {x: 12.0, y: 0.0, z: 3.0}    # Ciencias presente

  - name: "PressurePlate_AtrioEcho"
    type: PressurePlate
    accept_player: true
    accept_echo: false
    auto_release_timer: 0.0
    position: {x: 12.0, y: 0.08, z: 9.0}   # final de Ciencias
    target_door: "Door_AtrioConsequence"

  - name: "Door_AtrioConsequence"
    type: DoorController
    latch_open: true                        # el timer latch-open representa el cambio permanente
    position: {x: 0.0, y: 0.0, z: 8.0}      # entre Ciencias y el Atrio

  - name: "GoalTrigger_Diario"
    type: GoalTrigger
    position: {x: 0.0, y: 0.9, z: 12.0}    # Atrio
    linked_exit: "LevelExit_N03"

  - name: "LevelExit_N03"
    type: LevelExit
    nextSceneName: "Level_03"
    completionToast: "Luego pruebas."
```

## E. CÁMARA
- **Profile:** existente en `LevelCameraProfiles.TryGet("Level_02")` → `EchoesCameraIdentity.DynamicFollow` (offset [-2.5, 1.7, -4], FOV 56°). Cámara dinámica para enseñar la acción de la plataforma en movimiento.
- **Auto-frame:** al activar la palanca en el Laboratorio, `EventCameraDirector` enfoca la plataforma de Ciencias a través del ventanal (1.2s) para que el jugador vea la consecuencia inmediata.

## F. ILUMINACIÓN FUNCIONAL
- **Token:** `liminal_n02`. `BoostEarlyLevelLighting` level=2 → 2.5× (auto).
- **Luz cenital del Hall:** `FluorescentLight` (prefab existente) sobre el Reloj Roto, para subrayarlo como landmark.
- **Luz de laboratorio:** luz cálida en la mesa (PointLight 3200K) — el laboratorio es "cálido", vivo (pasado).
- **Luz de Ciencias:** luz fría/azulada (PointLight 5500K) — Ciencias es "frío" (presente sin el eco). El contraste cálido/frío es pista narrativa de pasado↔presente.
- **Atrio:** luz zenital suave, 4300K — el silencio después del escape.

## G. ECO — CONFIGURACIÓN
```yaml
echo_system:
  enabled: true
  max_echoes: 2           # +1 slot que N01: permite retener el eco-en-palanca y grabar más
  max_record_seconds: 12
  mode: Standard
  record_future: false
  timing_floor: 0.4       # ya definido en LevelBlueprint.timingFloor
```

## H. VALIDACIÓN (no aprobar porque compile)

**VALID-N02:**
- `[V-L02-001]` Spawn en Hall, puede caminar a ambas aulas y ver el Reloj Roto.
- `[V-L02-002]` La palanca del Laboratorio presiona la placa y mueve la plataforma en Ciencias.
- `[V-L02-003]` Soltar la palanca detiene/retrocede la plataforma (Feedback de necesidad del eco).
- `[V-L02-004]` Un eco grabado en la palanca la sostiene lo suficiente para cruzar la plataforma (ventana timing ≥1.5 s).
- `[V-L02-005]` `Door_AtrioConsequence` abre tras pisar la placa de Ciencias.
- `[V-L02-006]` El Reloj Roto del Hall avanza a 12:03 como feedback diegético (visible desde el Atrio).
- `[V-L02-007]` `LevelExit` carga `Level_03`.

**ANTI-SOFTLOCK:**
- `[S-L02-001]` La plataforma tiene `fastReturn` con `returnMultiplier=8` → el retroceso es rápido (paciencia menor al reintentar).
- `[S-L02-002]` Las dos aulas tienen vías de retorno señalizadas (decals de polvo) hacia el Hall; el jugador no se queda atrapado en Ciencias. Si el eco expira antes de cruzar, el jugador puede reeludirse y reintentar.
- `[S-L02-003]` `LevelGoal.skipEscapeSequence=true` si la plataforma se queda en estado activo (previene estado incierto).

**Legibilidad / momento aha:**
- El "aha" ocurre cuando el jugador entiende, a través del ventanal, que el eco puede vigilar la palanca mientras él atraviesa la sala opuesta. Timing objetivo: ~1.5–2 min tras el primer intento fallido.

## I. PLAYTEST CIEGO (obligatorio antes de N04)
- 1 jugador ciego. No intervenir.
- Registrar: dónde se detuvo (típicamente al soltar la palanca), qué intentó antes de usar el eco, cuántos intentos tardó en sincronizar, si entendió la conexión multi-sala(ventanal).
- Criterio de aprobación: entiende por sí solo que el eco "puede quedarse sosteniendo algo" en ≤7 min, sin pedir ayuda, sin quedarse atascado en Ciencias.

## 7. RULES (compatibles con LevelBlueprint existente)
- `[RULE-L02-001]` Nivel contiene: `SchoolCorridor` (Hall), `SchoolClassroom` ×2 (Lab+Ciencias con forma distinta), `TransitionSpace` (Atrio), `LevelExit`.
- `[RULE-L02-002]` Fog color `#1C2430`, densidad 0.008 (pre-boost).
- `[RULE-L02-003]` Camera profile `DynamicFollow` (existente en `LevelCameraProfiles["Level_02"]`).
- `[RULE-L02-004]` `Prop_RelojPared` en el Hall (mega) y en Ciencias (pequeño sincronizado con la plataforma).
- `[RULE-L02-005]` **NUEVO:** las dos aulas no son idénticas (Laboratorio plano/pequeño; Ciencias tiered/grande).
- `[RULE-L02-006]` **NUEVO:** existe un ventanal de visión directa entre las dos aulas (causa-efecto visible).
- `[RULE-L02-007]` **NUEVO:** consecuencia diegética multi-sala: el reloj del Hall avanza tras resolver.

## 14. CHANGE HISTORY
- **v4.0 (2026-08-16)**: Rebuild vertical slice. Timing + repetition multi-sala con bucle arquitectónico y reloj-landmark con反馈 diegético. Eliminada la "chain de habitaciones idénticas" del v3.0.
- **v3.0**: Spec ejecutable original (dual pressure plates en salas separadas).
