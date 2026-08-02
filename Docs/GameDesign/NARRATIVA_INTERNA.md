# NARRATIVA_INTERNA.md — Narrativa Central & Modelo de Procesamiento Cognitivo
## Spec ID: DOC-102
## Version: 1.0 (AI-Executable)
## Authority: Level 2 (Contexto Técnico y Filosofía)

---

### 1. PROPÓSITO Y DOMINIO

Define la narrativa central de *Echoes of You 2.0* y cómo esa narrativa se comunica al jugador exclusivamente mediante sistemas diegéticos (gameplay + voz interna + VN decision gates sobre el perdón). Rige:

- La identidad de Aiden (la protagonista) y su embarcamento en el entorno.
- El mensaje dual del juego y por qué NUNCA puede reducirse a uno de los dos lados.
- El modelo de etapas por el que Aiden "pasa" psicológicamente (y cómo el sistema Catch-22 puede ATASCARLA en una etapa si el jugador no acumula comprensión).
- La voz interna (`interaction.*`) y su tono por etapa.
- Las decisiones de Visual Novel (20 nodes) y cómo alimentan el resolver de 5 endings renombrados.

**Este documento es dependencia obligatoria de**: `ECHOES_BIBLE.md`, `DESIGN_PHILOSOPHY.md`, `emotional_arc.yaml`, `dialogue_tree_schema.yaml`, `VN_ENDINGS_REDEFINED.yaml`, `AI_AGENT_CONTRACTS.md (VisualNovelAgent)`, y todo script en `Assets/Scripts/Interaction/` y `Assets/Scripts/VN/`.

---

### 2. AIDEN — PROTAGONISTA

| Detalle | Canon |
|---|---|
| Pronombre | Ella (chica) |
| Edad aparente | 18-21 |
| Origen | Estudiante / ex-estudiante de un colegio liminal |
| Estado mental al iniciar el juego | Convencida de tener la razón sobre un error que cometió contra alguien querido |
| La persona clave | **Lyra** — alguien con quien Aiden tuvo un vínculo profundo |
| Naturaleza del vínculo Aiden ↔ Lyra | **AMBIGUA** (ver ANTI-BIB-004). El juego no usa nunca etiquetas "amiga", "novia", "pareja". El jugador decide su lectura según los pop-ups. |
| Qué hizo Aiden | *"Errores"* — el juego NUNCA especifica legiblemente. Solo se muestra por objetos amber e introspección gradual. Cada jugador dimensiona la magnitud según su lectura. |
| Por qué regresó al colegio (mente) | Para procesar. Pero procesar significa **aceptar y soltar**, no **reparar**. El juego le niega todo botón "deshacer" — tanto mecánicamente (BIB-VIS-001) como narrativamente (no hay Lyra a quien pedir perdón). |

---

### 3. EL MENSAJE DUAL — TESIS IRRESOLUBLE

El juego NO comunica un mensaje único. Comunica una **tensión intencional** entre dos verdades que no se cancelan:

#### Tesis A: "El pasado no se puede cambiar."
- Mecánicamente: el Eco grabado es irreversible (BIB-VIS-001).
- Narrativamente: no hay ninguna escena donde Aiden "arregle el pasado". El recuerdo solo puede ser observado, recorrido otra vez, o soltado — pero nunca editado.
- Filosóficamente: "Aceptarlo no es decir 'estuvo bien'. Es decir 'estuvo, y por eso soy quien soy'."

#### Tesis B: "Mejorar como persona es posible."
- Mecánicamente: cada nivel enseña una habilidad nueva para resolver variantes más finas del mismo patrón. Si Aiden hubiera seguido rechazando aprender, estaría igual que en N01.
- Narrativamente: la voz interna evoluciona si el jugador acumula comprehensión — pasando de convicción a aceptación.
- Filosóficamente: "Aceptar no es resignarse. Es comprender para dejar de repetir, y dejar ir para poder crecer."

#### Por qué NO se resuelve en un único mensaje
Si el juego dijera "el pasado se puede borrar" — traicionaría el rol del Eco.
Si el juego dijera "no hay nada que hacer, el pasado lo define todo" — significaría que el Gameplay (aprender mecánicas nuevas en cada nivel) no tiene propósito.

La **tensión no resuelta es el punto**. El jugador SALE del juego con la pregunta abierta: ¿hagouyo lo que puedo para ser mejor persona hoy, aunque lo de ayer fue como fue?

`ANTI-BIB-005` protege esta tensión a nivel textual: ningún pop-up, ending, o epílogo puede concluir una de las dos tesis.

---

### 4. EL ENTORNO COMO MENTE DE AIDEN

El colegio liminal no es un colegio. Cada aula, corredor y objeto representa una estructura cognitiva en proceso:

| Estructura física | Significado psicológico |
|---|---|
| Vestíbulo (SchoolHall) | El umbral — Aiden decide (o no) entrar a procesar |
| Corredor (SchoolCorridor) | El camino entre pensamientos |
| Aula (SchoolClassroom) | Una memoria específica trabajada |
| Biblioteca (SchoolLibrary) | Memorias declarativas — accesibles pero distorsionables |
| Patio (SchoolCourtyard) | Lo OPEN-AIR — memoria donde el cuerpo interviene (conversación pendiente) |
| Sala de profesores (SchoolStaffRoom) | Juez interno — Aiden castigando a Aiden |
| Aula de Lyra (SchoolLyraClassroom) | Donde Lyra vivía — objetos amber, duele tocar, enseña mirar |
| Aula liminal (SchoolLiminalClassroom) | Espacio neutro — donde el recuerdo ya no defiende ni ataca |
| Escalinata (SchoolStairwell) | Subir desde el fragmento hacia el integrado |

**El "salir del colegio"** = Aiden ha procesado lo suficiente para NO regresar. Es el event-flag `salir_del_colegio` y SOLO se marca en el ending **Aceptación**. En los otros cuatro endings, Aiden sigue adentro — porque aún no procesó.

---

### 5. LAS 4 ETAPAS DE PROCESAMIENTO (Regla de Voz)

Aiden atraviesa cuatro etapas psicológicas durante los 15 niveles. La voz interna (`interaction.*` pop-ups) refleja la etapa actual, PERO hay un Catch-22: la etapa "actual" no es solo level_index — es `min(level_stage, comprehension_stage)`.

| Etapa | Niveles nominados | Tono de voz | Ejemplo pop-up |regex pronombre matching |
|---|---|---|---|---|
| 1. **Convicción** | N01-N04 | Defensiva, corta, deflexiva. Aiden CREE que tiene razón. | "Yo no fui la que se fue." | solo presente y futuro hipotético |
| 2. **Culpa** | N05-N08 | Pesada, fragmentada, autoacusatoria. El peso empieza. | "Pude haber callado menos." | presente, condicional de reproche |
| 3. **Realización** | N09-N12 | Tentativa, en búsqueda, inciertas. Aiden ve parcialmente. | "Esto también lo armé yo." | presente, condicional, tal-vez |
| 4. **Aceptación** | N13-N15 | Calma, presente, hacia adelante. Sostiene sin apretar. | "Puedo soltar esto sin romperlo." | presente y futuro hipotético (sin "era", sin "fui") |

### 5.1 REGLA CATCH-22 (la columna vertebral del diseño)

**Si el jugador NO acumula flags de comprensión, Aiden SE QUEDA en una etapa anterior aunque esté en un nivel avanzado.**

| Si en N13 el `comprehension_score` es... | La voz de Aiden en N13 resonará como... |
|---|---|
| alto (≥ 8 flags) | etapa 4 (Aceptación) → "Puedo soltar esto." |
| medio (4-7 flags) | etapa 3 (Realización) → "Tal vez esto también lo armé yo." |
| bajo (0-3 flags) | etapa 1 o 2 (Convicción/Culpa) → "Yo no fui la que se fue." (aún en N13) |

**Cómo se acumula comprensión**:
- Inspeccionar objetos amber (`lyra_artifact_seen_count`): +1 cada uno
- Inspeccionar objetos no-amber relevantes: +0.5 cada uno
- Elegir opciones "soltar" / "permitir otra versión" en VN choices: +1 cada uno
- Elegir opciones "mantener patrón" / "atarse al recuerdo": +0

El motivo de diseño: **Aiden los "logra" comprender solo si el jugador también lo logra**. Pasar N13 sin haber mirado los objetos es una forma de negación — y la voz interna lo confirma negándose a evolucionar.

```mermaid
graph TD
    A[Lectura pop-up level N] --> B{comprehension_score >= stage_threshold[N]?}
    B -->|Sí| C[Stage efectivo = stage_by_level[N]]
    B -->|No| D[Stage efectivo = stage_by_comprehension]
    C --> E[Seleccionar texto de tone = stage_by_level]
    D --> E
    E --> F[Renderizar Chalkboard HUD: 1ª persona, ≤42 chars, 2.5s auto-dismiss]
```

---

### 6. LOS 20 VN DECISION NODES — PERDÓN VS REINCIDENCIA

Cada decision node post-nivel plantea a Aiden un choice entre (a) **mantener el patrón que la traba** y (b) **abrir un espacio para otra versión**. Estos choices NO tienen respuestas universalmente buenas — la lectura correcta depende del patrón acumulado.

| Node ID | Nivel | Tipo | Choices | Flags |
|---|---|---|---|---|
| ch1_choice_1 | N01 | base | (cyan) "Voy a mirar" / (amber) "Voy a pasar de largo" | allow_to_see / avoid_looking |
| ch2_choice_1 | N02 | base | "Estoy repitiendo" / "Solo es coincidencia" | pattern_seen / pattern_denied |
| ch3_choice_1 | N03 | base | "Permito otra versión" / "Me quedo con la mía" | allow_other_version / hold_my_version |
| ch3_choice_2 | N03 | micro | "Confiar en el Eco" / "Dudar del Eco" | trust_echo / doubt_echo |
| ch4_choice_1 | N04 | base | "Pude haber hablado" / "No debí decir nada" | admit_silence / justify_silence |
| ch5_choice_1 | N05 | base | "Toqué la taquilla" / "No la toco" | touched_locker / refused_locker |
| ch6_choice_1 | N06 | base | "Mi recuerdo es real" / "Lo que recuerdo es lo que necesito" | memory_is_real / memory_defensive |
| ch7_choice_1 | N07 | base | "Arreglar el futuro" / "Dejar el presente abierto" | fix_future / leave_present_open |
| ch7_choice_2 | N07 | micro | "Grabar otra versión" / "Dejar una sola toma" | grab_second_take / single_take |
| ch8_choice_1 | N08 | base | "Quedarme una versión" / "Aceptar que tengo dos" | pick_one_self / hold_both_selves |
| ch9_choice_1 | N09 | base | "Controlar todo" / "Soltar el control" | control_persist / release_control |
| ch10_choice_1 | N10 | base | "Tocar el recuerdo de Lyra" / "Solo mirar" | touch_lyra_object / observe_lyra_object |
| ch11_choice_1 | N11 | base | "Subir sola" / "Coordinar conmigo" | solo_climb / self_coordination |
| ch12_choice_1 | N12 | base | "Exigir una verdad" / "Cargar con dos" | demand_truth / carry_two |
| ch13_choice_1 | N13 | base | "Salvar el recuerdo que más duele" / "Soltar el que más duele" | salvage_pain / let_go_pain |
| ch13_choice_2 | N13 | micro | "Toma perfecta" / "Toma imperfecta honesta" | perfect_take / imperfect_take |
| ch14_choice_1 | N14 | base | "Seguir controlando el Eco" / "Solo acompañar el Eco" | control_echo / follow_echo |
| ch15_choice_1 | N15 | base (FINAL) | "Romper el patrón" / "Repetir el patrón" | **break_pattern_n15** / **repeat_pattern_n15** |

**Decisiones que inyectan "comprehension":** `allow_to_see`, `pattern_seen`, `allow_other_version`, `trust_echo`, `admit_silence`, `touched_locker`, `memory_is_real`, `single_take`, `release_control`, `touch_lyra_object`, `self_coordination`, `carry_two`, `let_go_pain`, `imperfect_take`, `follow_echo`, `break_pattern_n15`. Si el jugador elige estas opciones consistentemente → comprensión alta → camino a Aceptación.

**Decisiones que mantienen el patrón (no inyectan comprehensión):** `avoid_looking`, `pattern_denied`, `hold_my_version`, `doubt_echo`, `justify_silence`, `refused_locker`, `memory_defensive`, `fix_future`, `grab_second_take`, `pick_one_self`, `control_persist`, `observe_lyra_object`, `solo_climb`, `demand_truth`, `salvage_pain`, `perfect_take`, `control_echo`, `repeat_pattern_n15`. El jugador que elige siempre estas termina en Aislamiento o Ruminación.

**Micro-choices son intencionalmente ambiguas**: Trust/Doubt Eco tiene sentido en N03 pero es contraproducente en N07. La opción que agrega comprensión NO es siempre la misma. Detectar cuál es la parte del puzzle cognitivo que Aiden necesita.

---

### 7. LOS 5 ENDINGS — DEFINICIONES PSICOLÓGICAS

Los endings están renombrados en v2.0 (ver `VN_ENDINGS_REDEFINED.yaml`). Resumen:

### 7.1 Aislamiento (ex Void) — Negación persistente
Aiden elige consistentemente evitar mirar. Nunca tocó la taquilla de Lyra. El colegio sigue tal cual al final. La última imagen: Aiden en el mismo corredor de N01, sin haber salido. La voz interna: "Yo no necesito estar aquí." El juego no la contradice. Tú decides si eso es paz o encarcelamiento.

### 7.2 Ruminación (ex Obsession) — Culpa persistente / autosabotaje
Aiden admitió culpa pero no la soltó. Repite el recuerdo exacto esperando que duela menos. No duele menos. La última imagen: Aiden en el aula de Lyra, repitiendo el mismo Eco 12 segundos tras 12 segundos. Voz: "Otra vez y esta vez saldrá bien." Salida del colegio: NO.

### 7.3 Negociación (ex Release) — Realización parcial pero evasiva
Aiden vio parte del panorama pero negocia con la verdad. Construye una versión nueva del recuerdo, dice "ya está bien". Pero sigue visitando la taquilla vacía cada noche. Última imagen: sale del colegio pero regresa a la puerta sin entrar. Voz: "Casi. Mañana termino de soltarlo." Salida del colegio: parcial.

### 7.4 Desesperación (ex Resonance) — Comprensión pero desesperación
Aiden vio TODO el panorama y la magnitud la aplastó. El dolor significa que importó, así que se lo queda. No es paz, es miedo. Última imagen: Aiden en el vestíbulo, no sale, no entra, no decide. Voz: "Esto dolerá por siempre y por eso la amé." Salida del colegio: NO.

### 7.5 Aceptación (ex Integration) — Aceptación y mejora activa
Aiden sostiene el recuerdo sin apretarlo. Tocó los objetos amber. Soltó el que más dolía (N13). Dejó de controlar el Eco en N14. Rompió el patrón en N15. Última imagen: Aiden sale del colegio, no mira atrás, el colegio sigue ahí pero ella ya no lo habita.

**Voz final (única excepción permitida al monopolio introspection_node, marcada como entry* en `VN_Text.es.yaml`)**:
> "Puedo llevar esto conmigo sin que me defina."

NO dice "estuvo bien". NO dice "perdoné". NO dice "mejoré". Solo: puede cargar con ello. El mensaje dual queda abierto.

`salir_del_colegio` = TRUE.

---

### 8. EL SISTEMA DE POP-UPS (INSPECCIÓN) — RESUMEN TÉCNICO

Ver spec técnico completo: `Docs/UI/INSPECT_POPUP_SPEC.md`. Resumen aquí:

| Atributo | Regla |
|---|---|
| Lugar de render | HUD::Chalkboard (no nuevo componente) |
| Persona gramatical | 1ª persona (regex `^(Yo\|Quisiera\|Esto\|Puedo\|Aún\|Tanto\|Pude\|Tal vez\|Nunca\|Siempre\|A veces\|Me)`) |
| Máximo caracteres | 42 |
| Duración auto-dismiss | 2.5s ±1.0 |
| Tiempo verbal | Presente o futuro hipotético. Prohibido pasado simple evocativo ("era", "fui", "tuve"). |
| Tono por etapa | 4 etapas (Convicción → Culpa → Realización → Aceptación), ver Sección 5 |
| Catch-22 Clamp | Si `comprehension_score < stage_threshold[level]`, tono se clampea a etapa anterior |
| Cooldown | 3 segundos entre inspecciones del mismo objeto |
| Palabras prohibidas | ["amiga", "novia", "pareja", "relación", "amistad", "tenía razón", "tenía la culpa"] |
| Disparador | InteractableObject (componente) + tecla E + proximidad < 2.5m |

---

### 9. INTEGRACIÓN CON VN_SYSTEM

La Visual Novel NO es un modo separado del juego. Es el sistema de decisiones que **cierra cada nivel** (post-LevelExit) y alimentan `VN_EndingResolver`:

1. Player completa puzzle de nivel N.
2. `LevelExit` dispara `VN_ChoiceGateController.Show(levelIndex, onComplete)`.
3. Aparece overlay full-screen: figura Aiden de espaldas + polaroid + 2 EchoButton (cyan/amber) con labels psicológicos.
4. Player elige con A (cyan) o D (amber). NO hay botón "omitir" — la opción por defecto es la última elegida en la sesión (timeout.getSeconds = 0 → default_first).
5. `VN_EndingFlags.SetFlag(node_id, choice)` registra la decisión.
6. `comprehension_score` se actualiza (la choice inyectó +1 si fue "abrir" / +0 si fue "mantener").
7. `onComplete(choice)` → carga el siguiente nivel.

Al final de N15, post-decision 15, el `VN_EndingResolver.Resolve()` lee todos los flags, computa patrón, y devuelve endingID:
-Ending == "Aislamiento" → cargar `Epilogue_Aislamiento.unity`
-... y así.
- Aceptación es el único que cierra con `salir_del_colegio = true`.

---

### 10. VALIDACIÓN CRUZADA

| Regla | Cómo se valida | Documentos de referencia |
|---|---|---|
| Aiden = chica (pronombre "ella") | `TextInspector` recorre `VN_Text.es.yaml` y `interaction.*` y verifica 0 "él", 0 "chico", 0 "niño" usando la voz de Aiden. | ECHOES_BIBLE v3.2, este doc Sección 2 |
| Ambigüedad relacional | `ambiguity_police` valida 0 ocurrencias de ["amiga", "novia", "pareja", "relación", "amistad", "nos amamos"]. | ANTI-BIB-004, este doc Sección 8 |
| Anti-autojustificación | `dual_thesis_police` valida 0 conclusiones tipo ["tenía razón", "tenía la culpa", "estuvo bien", "perdoné"]. | ANTI-BIB-005 |
| Tono por etapa | `tone_by_stage_validator` verifica `tone_by_level` matches `min(stage_by_level, stage_by_comprehension)`. | RULE-PHI-005, este doc Sección 5.1 |
| 1ª persona regex | `introspection_1st_person_validator` aplica regex `^(Yo|Quisiera|Esto|Puedo|Aún|Tanto|Pude|Tal vez|Nunca|Siempre|A veces|Me)`. | dialogue_tree_schema v2.0 |
| 32 paths resolver | `VN_EndingResolverTests` cubre 32 combinaciones canónicas. | AI_AGENT_CONTRACTS VisualNovelAgent |
| Aceptación requiere `salir_del_colegio` true | Epilogue_Aceptacion.cs asserts `salir_del_colegio == true` al cargar. Otros = false. | este doc Sección 7.5 |

---

### 11. CROSS REFERENCES

- [SOURCE_OF_TRUTH.md] (Level 1) — Autoridad suprema
- [ECHOES_BIBLE.md] (Level 2) `[SPEC-101]` — Reglas ejecutables y anti-patrones
- [DESIGN_PHILOSOPHY.md] (Level 2) `[SPEC-001]` — Directivas emocionales/mech
- [DIALOGUE_TREE_SCHEMA.yaml] (Level 3) — Schema runtime introspection_node
- [emotional_arc.yaml] (Level 3) v2.0 — Arcos por nivel
- [level_intents.yaml] (Level 3) v2.0 — Intenciones por nivel
- [VN_ENDINGS_REDEFINED.yaml] (Level 3) — Endings renombrados y flags
- [AI_AGENT_CONTRACTS.md] `[SPEC-403]` — Contrato VisualNovelAgent
- [INSPECT_POPUP_SPEC.md] — Spec técnico pop-ups

---

### 12. CHANGE HISTORY

- **v1.0 (2026-08-02)**: Creación. Documento inédito requerido por la reescritura narrativa dual de ECHOES_BIBLE v3.2. Aporta: identidad Aiden (chica), ambigüedad relacional con Lyra por ANTI-BIB-004, mensaje dual irresoluble por ANTI-BIB-005, modelo de 4 etapas con Catch-22, 20 VN decision nodes psicológicos, 5 endings renombrados, integración técnica con HUD::Chalkboard y VN_ChoiceGateController.
