# LEVEL_SPEC_03.md — Level 03 Blueprint Specification (Bifurcación de Lyra)
## Spec ID: LEVEL-SPEC-03
## Version: 4.0 (Vertical Slice Rebuild — Place-First, Decision)

---

## A. DESIGN INTENT (Función)
Introducir: decisión, coordinación, observar al Eco, comprender que una acción pasada puede afectar una ruta presente.

**Debe existir:** bifurcación real + línea de visión + zona de decisión + puerta/recompensa + feedback.

**Flujo objetivo:** Biblioteca normal → descubrir proyector (pasado) → bifurcación arquitectónica real con dos rutas → la ruta correcta solo se abre porque una acción en el pasado cambió el presente final→ feedback claro → decisión con consecuencias (rama nueva desbloquea habilidad).

## B. ARQUITECTURA (Room graph + circulación primero)

### B.1 Room Graph
```
[Entrada/BibliotecaPasada] → [Fork pasillo (zona de decisión)]
                              /                          \
            [Rama Izquierda]              [Rama Derecha]
            (ruta segura, simple)        (ruta recompensa, exige eco)
                              \                          /
                            [Punto de fusión] → [Exit N04]

            └── Línea de visión desde el Fork:
                - Estatua del Fundador en el centro, proyecta sombra direccional.
                - La sombra "apunta" a la rama correcta SOLO después de interactuar con el proyector del pasado.
```
- **Tipo de grafo:** Diamante con bifurcación real. Las dos ramas llevan al punto de fusión, PERO solo la rama derecha da la recompensa narrativa/habilidad. La rama izquierda también completa el nivel (no es failing path), pero con menor recompensa.
- **Bifurcación real:** la elección observa al jugador (no pseudo-elección). Dos narrativas ambientales distintas según la rama elegida.

### B.2 Circulación
- Biblioteca pasada (exploración + clue del proyector).
- Fork: el jugador se detiene en una plataforma elevada 1 m y observa las dos ramas antes de comprometerse.
- Las dos ramas son visualmente distintas: Izquierda = pasillo estrecho mal iluminado (rutina); Derecha = puente abierto con vista al tejado (puzzle/recompensa).
- Anchura de cada rama: Izquierda 2.5 m (comprimida, claustrofobia); Derecha 4.0 m (abierta, esperanza).

### B.3 Anchuras y Alturas (escena, sin multiplicar por LevelGeometryScale=2)
| Nodo | Ancho (X) | Largo (Z) | Alto (Y) | Carácter |
|------|-----------|-----------|----------|----------|
| Biblioteca pasada | 12.0 | 12.0 | 4.0 | Espaciosa, estantes altos 2.5m = maze deliberado |
| Plataforma decisoria (Fork) | 8.0 | 5.0 | 4.0 | Elevada 1.0m, mirador de las dos ramas |
| Rama Izquierda | 2.5 | 14.0 | 2.8 | Estrecha, tensa |
| Rama Derecha | 4.0 | 14.0 | 4.5 | Abierta, puente con baranda |
| Punto de fusión | 7.0 | 6.0 | 4.0 | Reúne la bifurcación |

**Diferenciación:** la bifurcación es visible y arquitectónica, no arbitraria.

### B.4 Puertas
- `Door_LibraryExit`: del aula biblioteca a la Fork, abierta, sin lógica.
- `Door_LeftBranch`: abierta, marco oscuro (ruta NSAttributedString segura).
- `Door_RightBranch`: **cerrada**. La abre una `PressurePlate_ShadowEcho` (proyector+eco) en la Biblioteca pasada. **Bifurcación real:** la rama derecha solo es viable si activó el eco del proyector.
- `Door_Fusion`: cierra el punto de fusión desde el interior de la rama elegida, sale la unlock al pisar la GoalTrigger correspondiente.

### B.5 Escaleras
- La Fork se asoma 1.0 m sobre las ramas (mirador = zona de decisión).
- La Rama Derecha es un puente que sube 1.0 m y luego baja (sensación de ruta elevada).

### B.6 Landmarks y Líneas de visión
- **Landmark central:** `EstatuaFundador` (3.0 m, estatua del fundador del colegio) en el centro del eje del Fork, iluminada cenital por `FluorescentLight`. **La sombra direccional** de la estatua es la pista de decisión: al principio apunta a "ninguna rama" (sombra circular en el suelo).
- **Pista de eco-proyector:** un `NarrativeProp_ChalkDrawing` (dibujo de tiza) en la Biblioteca pasada muestra una flecha hacia la estatua = pista narrativa.
- **Línea de visión decisiva:** desde la plataforma Fork, el jugador ve las dos puertas y la sombra. La sombra apunta a la rama derecha **solo después** de que el eco haya iluminado la placa oculta en la Biblioteca pasada. Esta es la implementación textual de "observar al Eco afecta una ruta presente".

### B.7 Espacios de respiración
- Biblioteca pasada: 12×12 (más espacioso que N02 — el jugador aprende a mirar, no a resolver todavía).
- Plataforma Fork: el jugador puede detenerse, mirar, elegir. **Es la zona de decisión deliberada.**

## C. PROPS (cada prop con rol — N03 es el más narrativo del vertical slice)

| Prop | Room Role | Placement Rule | Scale | Rotation | Clearance | Nav Impact | Narrative Role |
|------|-----------|----------------|-------|----------|-----------|------------|----------------|
| `Prop_Estanteria` (×4) | Biblioteca | Dos filas, lados opuestos, 2.4m altura | 1:1 | Frente a la fila contraria | 1.8m pasillo central | Ninguno | Maze deliberada (no aleatoria). |
| `Prop_PhotoFrame` (fotos PW) | Biblioteca | Repartidas sobre estantes | 1:1 | Variado | 0.3m | Ninguno | Fotos borroneadas = precedencia narrativa. |
| `NarrativeProp_ChalkDrawing` | Biblioteca | Suelo, junto al proyector-pasado | 1:1 | 0° | 0.5m | Ninguno | Flecha indicando la estatua (clue). |
| `Prop_Radio` (proyector-pasado proxy) | Biblioteca | En un atril frente a los estantes, alineado hacia el punto central de luz | 1:1 | Mirando a la placa oculta | 1.0m | Ninguno | Objeto-pasado central del puzzle: emite luz cuando el eco lo reproduce. |
| Estatua del Fundador | Fork | Centro del mirador, sobre pedestal | 3.0m | 0° | 2.0m radio | **Sí**: sombra direccional interpretable | Landmark central de decisión. |
| `Prop_SillaOficina` (×2) | Rama Izquierda | Lado de pared | 1:1 | Frente al pasillo | 1.5m | Ninguno | Rutina/burocracia. |
| `Prop_Stoop/IronRailing` (puente) | Rama Derecha | Baranda del puente | 1:1 | Paralelo al puente | — | **Sí**: delimita el puente | Abierto/esperanza. |
| `Prop_DriedFlowers` | Rama Derecha | En el centro del puente | 1:1 | 0° | 0.5m | Ninguno | Vida pasada, secada = recompensa NPC del pasado. |
| `Prop_TeacherNotebook` (recompensa) | Rama Derecha | Al final del puente, en pedestal | 1:1 | 0° | 1.0m | Ninguno | Recompensa narrativa mayor + desbloquea habilidad. |

**Reglas críticas:** la estatua debe ser visible y legible desde el Fork (no tapada por props). Las dos ramas no deben estar decoradas de forma idéntica (la diferencia visual apoya la decisión).

## D. PUZZLE (Teaching → Experiment → Discovery → Execution → Payoff)

**Pregunta-guía:** "¿Qué está intentando comprender el jugador?" → *"que puedo pre-disponer el presente haciendo algo en el pasado; y que ver esto me permite elegir con intención"*.

### D.1 Mecánica central: eco-proyector + sombra direccional → bifurcación real

| Fase | Acción del jugador | Insight | Implementación |
|------|-------------------|---------|----------------|
| **Teaching** | Entra a la Biblioteca pasada. Ve el proyector (`Prop_Radio`), un decal de flecha hacia "la estatua", y la estatua en la Fork es visible también desde aquí a través de un ventanal. | Asociar proyector↔estatua↔sombras. | `NarrativeProp_ChalkDrawing` + `TutorialTrigger` "Mantén R junto al proyector". |
| **Experiment** | Camina junto al proyector y graba un eco que "encamina" luz hacia la placa de eco oculta en la pared opuesta de la biblioteca. Suelta, el eco reproduce, recorre el pasillo del proyector y emite luz direccional (efecto visual). | El eco puede copiar/continuar una emisión. | El eco reproduce `EchoPlayback` (Standard). Cuando su **frame medio** coincide con un `BoxCollider` fused de `PressurePlate_ShadowEcho` (la placa oculta), la activa. `acceptEcho=true; autoReleaseTimer=0` (latch permanente). |
| **Discovery** | El jugador sube a la Fork. Al mirar la estatua, observa que **la sombra ahora apunta a la rama derecha** (rotation del Directional Light cambiada por `DoorController_RightBranch.SetOpenState(true)` que también rota la estatua ligeramente). | "Lo que hice en el pasado cambió física y visiblemente una ruta presente — y la estatua me lo dice." | `DoorRightBranch.PressedChanged` → rota `EstatuaFundador` 15 grados; `DirectionalLight.rotation` cambia 10°. La luz direccional genera sombra nueva. `FixedPuzzleCameraController` flingea un quick focus en la sombra (feedback). |
| **Execution** | El jugador elige. **Rama izquierda:** libre, llega al punto de fusión, completa el nivel con recompensa menor (incluye el `LevelExit` pero el diario de profesor no aparece). **Rama derecha:** cruza el puente, encuentra el `Prop_TeacherNotebook` = `GoalTrigger_mayor`, desbloquea adicionalmente una nueva habilidad de eco (e.g., nuevo modo `EchoPlaybackMode.Future` configurado en el próximo `LevelBlueprint` de N04). | Decisión real con consecuencias narrativas y de progresión. | `LevelGoal.anyTriggerSatisfiesGoal=true` (cualquier rama completa el nivel). `GoalTrigger_Mayor` (derecha)`, `GoalTrigger_Minor` (izquierda). La Goal mayor despide un `GameProgress.Flag` para N04 (`unlock_future_echo`). |
| **Payoff** | Si eligió la rama derecha: feedback granular — el puente se ilumina (luz cálida), el diario del profesor despliega un panel visual novel breve (24 líneas), toast "Dos decisiones se sostienen." Si eligió izquierda: el nivel completa pero con un panel frase único ("Decidiste lo cierto. No lo mejor."). | Player siente la consecuencia narrativa de la decisión. | `VN_DialogueController` (ya integrado). `LevelExit.completionToast="Dos decisiones se sostienen."` (rama derecha). Toast alterno "Decidiste lo cierto." (rama izquierda). |

### D.2 Objetos de puzzle (mapa)
```yaml
puzzle_components:
  - name: "PressurePlate_ShadowEcho"
    type: PressurePlate
    accept_player: false
    accept_echo: true
    auto_release_timer: 0.0   # latch permanente
    position: {x: 0.0, y: 0.08, z: 6.0}   # biblioteca, pared opuesta al proyector
    target_door: "Door_RightBranch"

  - name: "Door_RightBranch"
    type: DoorController
    latch_open: true
    position: {x: 4.0, y: 0.0, z: 16.0}    # bifurcación derecha
    side_effect:
      rotate_statue_degrees: 15.0
      directional_light_tilt_degrees: 10.0

  - name: "Door_LeftBranch"
    type: DoorController
    latch_open: true
    open_initially: true                    # siempre accesible
    position: {x: -4.0, y: 0.0, z: 16.0}

  - name: "PressurePlate_GoalMayor"
    type: PressurePlate
    accept_player: true
    accept_echo: false
    auto_release_timer: 0.0
    position: {x: 4.0, y: 0.0, z: 30.0}    # rama derecha (puente)
    side_effect: "unlock_future_echo"      # ver nota J

  - name: "GoalTrigger_Mayor"
    type: GoalTrigger
    position: {x: 4.0, y: 0.08, z: 30.0}
    use_plate_pressed_state: true
    pressure_plate: "PressurePlate_GoalMayor"

  - name: "PressurePlate_GoalMinor"
    type: PressurePlate
    accept_player: true
    accept_echo: false
    auto_release_timer: 0.0
    position: {x: -4.0, y: 0.0, z: 30.0}

  - name: "GoalTrigger_Minor"
    type: GoalTrigger
    position: {x: -4.0, y: 0.08, z: 30.0}
    use_plate_pressed_state: true
    pressure_plate: "PressurePlate_GoalMinor"

  - name: "LevelGoal_N03"
    type: LevelGoal
    any_trigger_satisfies_goal: true
    skip_escape_sequence: true

  - name: "LevelExit_N04"
    type: LevelExit
    nextSceneName: "Level_04"
    completionToast: "Dos decisiones se sostienen."   # o "Decidiste lo cierto."
```

## E. CÁMARA
- **Profile:** existente en `LevelCameraProfiles.TryGet("Level_03")` → `EchoesCameraIdentity.SideCinematic` (offset [-6, 1.3, 0.25], FOV 50°). Más lento y cinematográfico lateral para invitar a la observación.
- **Framed view Fork:** `FixedPuzzleCameraController` (existente) anclado en la plataforma Fork, enmarcando la estatua y las dos puertas. Cuando el jugador sube al gatillo del mirador (`TutorialTrigger`), la cámara 3rd-person cede 1.5s a una `EventFocusVCam` que muestra la sombra direccional (feedback compelente).
- **Línea de visión rama derecha:** cámara se reposa para mostrar el puente y el diario del profesor desde el primer paso de la rama (anticipación de recompensa).

## F. ILUMINACIÓN FUNCIONAL
- **Token:** `liminal_n03`. `BoostEarlyLevelLighting` level=3 → 2.3× (auto).
- **Luz cenital estatua:** la estatua lleva un `Spotlight` cenital (ángulo 45°, intensidad 4.0, 5500K). Crea sombra direccional legible.
- **Luz rama izquierda:** PointLight frio 5200K, baja intensidad (1.0), atmósfera rutinaria/burocrática.
- **Luz rama derecha:** PointLight cálido 4000K, intensidad 2.5, "esperanza". Cuando `Door_RightBranch` abre, un cielo azul pálido entra por el tejado abierto del puente (Spotlight area con sky-mock).
- **Sombras:** las sombras dinámicas son mandatorias en N03 porque son la propia pista del puzzle. Verificar `LightShadows.Soft` en el Directional Light y el Spotlight de la estatua.

## G. ECO — CONFIGURACIÓN
```yaml
echo_system:
  enabled: true
  max_echoes: 2
  max_record_seconds: 12
  mode: Standard
  record_future: false           # el future se desbloquea como RECOMPENSA en la rama derecha
  timing_floor: 0.4
```

## H. VALIDACIÓN (no aprobar porque compile)

**VALID-N03:**
- `[V-L03-001]` Spawn en Biblioteca, puede caminar por el maze de estantes, encontrar el proyector y ver el decal de flecha.
- `[V-L03-002]` Grabar un eco que recorre desde el proyector hasta la placa oculta presiona `PressurePlate_ShadowEcho`.
- `[V-L03-003]` La activación de la placa **gira la estatua** y **cambia la sombra direccional** hacia la rama derecha (verifiable visualmente).
- `[V-L03-004]` Sin la activación previa, `Door_RightBranch` está cerrada y la sombra no guía.
- `[V-L03-005]` Las dos ramas llevan al punto de fusión, pero solo la rama derecha da la recompensa mayor (diario + flag `unlock_future_echo`).
- `[V-L03-006]` `LevelExit` carga `Level_04` desde cualquiera de las dos ramas.
- `[V-L03-007]` El `FixedPuzzleCameraController`/FocusVCam muestra la sombra cuando sube al mirador (feedback clave).

**ANTI-SOFTLOCK:**
- `[S-L03-001]` La rama izquierda SIEMPRE está abierta (Door_LeftBranch `openInitially=true`). Si el jugador no entiende el eco-proyector, puede completar el nivel por la ruta segura (no queda atrapado).
- `[S-L03-002]` Si el eco expira sobre la placa, como `autoReleaseTimer=0`, la placa queda ceased latched. La `Door_RightBranch` permanece abierta (latch_open=true).
- `[S-L03-003]` El punto de fusión es único; no es posible entrar a una rama, girar y no poder salir (no hay dead-ends).

**Legibilidad / momento aha:**
- El "aha" se produce al subir al mirador y ver la sombra de la estatua apuntando a la rama derecha *después* de haber manipulado el proyector en el pasado. Timing objetivo: ~2 min tras la activación de la placa.
- La bifurcación es **real**: el jugador percibe que su elección afecta el final del nivel (recompensa mayor vs menor, flag de habilidad). Cela distingue N03 de N02 (timing) y N01 (teaching).

## I. PLAYTEST CIEGO (obligatorio antes de N04)
- 1 jugador ciego. No intervenir.
- Observar de forma específica: (a) ¿notó la sombra? (b) ¿entendió que la sombra era pista? (c) ¿qué rama eligió y por qué? (d) si eligió rama derecha sin haber activado el eco-proyector (no puede, la puerta está cerrada), ¿entontró cómo volver atrás o softlock? (e) ¿percibió diferencias narrativas entre las dos rutas/paneles??)
- Criterio de aprobación: escoge su rama con intención (no al azar), entiende la relación pasado→presente, sin softlock, en ≤10 min. Tras completar, si preguntamos "¿crees que podrías haber elegido mejor?", el jugador responde con comprensión (no con confusión).

## 7. RULES (compatibles con LevelBlueprint existente)
- `[RULE-L03-001]` Nivel contiene: `SchoolLibrary` (303 biblioteca pasada), `TransitionSpace` (Fork mirador), dos rutas (`TransitionSpace`×2 = Left/Right branches), `LevelExit`.
- `[RULE-L03-002]` Fog color `#1C2430`, densidad 0.008 (pre-boost).
- `[RULE-L03-003]` Camera profile `SideCinematic` (existente en `LevelCameraProfiles["Level_03"]`).
- `[RULE-L03-004]` Estatua del Fundador en el centro del Fork; `Prop_TeacherNotebook` como recompensa rama derecha; `Prop_DriedFlowers` decoración puente.
- `[RULE-L03-005]` **NUEVO:** bifurcación arquitectónica real (no pseudo-elección): las dos ramas tienen forma, lighting y props distintos, y el outcome narrativo difiere.
- `[RULE-L03-006]` **NUEVO:** una acción en el pasado (eco-proyector) modifica visible y físicamente el presente (sombra direccional + puerta). Es la manifestación textual del principio "acción pasada afecta ruta presente".
- `[RULE-L03-007]` **NUEVO:** existe un path seguro siempre accesible (rama izquierda) → anti-softlock implícito en el diseño.

## J. Sincronización con progresión
- Si el jugador completa la `GoalTrigger_Mayor` (rama derecha), el componente `EchoCapabilityUnlocker` (ya implementado en `Assets/Scripts/Narrative/EchoCapabilityUnlocker.cs`) escucha `SatisfactionChanged` y persiste la flag `unlock_future_echo` vía `VN_EndingFlags.SetFlag` + `NarrativeSaveBridge.Save()`. N04 puede leer esta flag desde `VN_EndingFlags.GetFlag("unlock_future_echo")` para habilitar `EchoPlaybackMode.Future` en su `LevelBlueprint`.
- Si completa `GoalTrigger_Minor` (rama izquierda), N04 carga normal sin la flag (no es bloqueante, solo diferencia de capacidades).
- Esta es la semilla de la mecánica de decisiones con consecuencias que puede expandirse más adelante. La bifurcación es arquitectónicamente real y completa el nivel; la flag de habilidad es la recompensa adicional de la rama derecha.

## 14. CHANGE HISTORY
- **v4.0 (2026-08-16)**: Rebuild vertical slice. Bifurción real con sombra direccional + eco-proyector. Decision con consecuencias narrativas y flag de habilidad para N04. Eliminada la "single-door pressure" del v3.0.
- **v3.0**: Spec ejecutable original (introducción simple a inversion/perspective).
