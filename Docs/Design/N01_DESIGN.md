# N01 — DESORIENTACIÓN · Documento de Diseño y Redesign

> **Capítulo I — Negación** · Puzzle `echo_introduction` (archetype: `teaching`) · VN node `ch1_choice_1`
> **Fase del proceso:** Documento de diseño (Paso 1). El usuario implementa en Unity (Paso 2). Yo valido en Play Mode (Paso 2.5).
> **Versión:** 1.0 · Estado: EN ESPERA DE IMPLEMENTACIÓN

---

## 0. INPUTS DEL DISEÑO (Fuente de verdad, no modificar)

| Campo | Valor verbatim |
|---|---|
| `levelName` | `Level_01` → `nextLevel: Level_02` |
| Blueprint asset | `Assets/Data/Levels/Level_01_Blueprint.asset` |
| Act | `actNumber: 1` |
| Title (asset) | `Nivel 1 — Desorientación` |
| Intent | `Desorientación` / `negacion` / cap. `I_Negacion` |
| Dual theme | *"El recuerdo existe aunque no quiera verlo"* |
| Psychological function | *"Presentar el mecanismo del recuerdo (Eco) que Aiden quiere ignorar."* |
| Emotional arc | `denial → avoidance → confrontation → shame → apertura_a_ver` |
| Primary rooms | `[SchoolHall, SchoolCorridor]` |
| Archetype | `teaching` (`PUZ-ARCH-001`): observation → safe_experiment → single_execution → payoff |
| Echo ideal | `echo_introduction`, 1 eco, 12s máx, modo Standard, button_test `pass_required` |
| VN stage threshold | `N01: 0` → siempre **conviction** (etapa 1 — voz defensiva, corta, deflexiva) |
| VN node | `ch1_choice_1`: cyan `allow_to_see` / amber `avoid_looking` |
| Echo capability | `ECHO-CAP-001 activate_pressure_plate` (lvl N01) |
| Pop-up voice spec | `introspection_node.tone_by_stage.conviction` (N01–N04) |

### Doc-drift detectado (NO bloquea el redesign, pero sapere)
- `Level_01_Blueprint.asset` ya declaró **SchoolHall+2×Corridor+LiminalThreshold** (loop) con exit @ z=52, con `PlateA → ExitGate`. Esto **reemplaza** el `Docs/Specs/Levels/LEVEL_SPEC_01.md` v3.0 que describe una secuencia lineal PlayerStart→SchoolEntrance→Corridor→Classroom→Exit @ z=43. **Se usa el asset como verdad operativa.**
- Capítulo nombrado `I_Negacion` en `level_intents.yaml` (v2.0 narrativo) vs. `I_Persistence` en `puzzle_archetypes.yaml` (v1.0 gameplay). En este doc usamos **`I_Negacion`** (narrativa actualizada).

---

## 1. ARQUITECTURA ACTUAL (Lo que YA está construido en `Level_01.unity`)

Regenerado por `EchoesNewProductionBuilder` a partir del blueprint. Layout lineal en Z:

```
z= -5  PlayerStart (PlayerController spawns aquí)
z= -3  PorchHall    (SchoolHall 8×3×6) — el "porche silencioso"
z= 10  CorridorA    (SchoolCorridor 6×3×20, flicker=true)  ← puerta al final oculta
z= 15  [PlateA PressurePlate]                  ← gate de ExitGate, wiring `targetSignals:[PlateA]`
z= 30  CorridorB    (SchoolCorridor 6×3×20, flicker=false)
z= 48  LiminalThreshold (8×3.5×8)              ← umbral narrativo
z= 52  ExitGate     (Door 4×3×0.5)  ← abre con PlateA
z= 54  LevelExit_Area + LevelGoal             ← trigger fin de nivel → Level_02
```

**pathHints declarados**: `(0,.1,-3) (0,.1,10) (0,.1,30) (0,.1,52)` — eco-cues en el suelo.

**Atmósfera blueprint**: `fogColor {0.11,0.14,0.19}`, `fogDensity 0.008`, `skyColor {0.06,0.08,0.12}`, `ambient {0.06,0.08,0.10}`, `directional {0.95,0.95,1.0} intensity 0.85`, rotación `(50,-30,0)`.

**Narrativa intro**: título *Nivel 1 — Desorientación* · "Un porche silencioso te da la bienvenida a un pasillo escolar sin fin. La memoria repite su estructura." · duración 6s.

---

## 2. REDESIGN DEL PUZZLE — `echo_introduction` → DIVERTIDO

### 2.1 Diagnóstico del puzzle actual
- **Estado actual:** CorridorA tiene una placa (PlateA z=15). PlateA → abre ExitGate z=52. Placa requiere `pass_required` (eco debe pisarla). Jugador camina → graba eco → eco reproduce → eco pisa placa → puerta abre. **1 mecánica, 1 uso, 0 fricción.**
- **Por qué aburre:** Tutorial explícito sin tensión. Sin hook, la placa no se ve (salvo pathHint). Fracaso = grabas otra vez. Repetitivo. Pazúa curva 0.

### 2.2 Hook (primeros 10s — *"¿Qué hago? ¿Por qué importa?"*)
Jugador spawn en PorchHall. Frente a él: **Puerta sellada** al inicio del CorridorA (no la ExitGate; nueva puerta sellada entre PorchHall→CorridorA que sólo abre cuando el eco está activo). La HUD muestra: *"El umbral no se abre solo."* — primer pop-up diegético (≤42 chars, conviction).
- Jugador explora PorchHall. Ve puerta sellada → inspecciona → *"Yo no necesito entrar."* (conviction). Toca la cerradura → **no avance**.
- Entra el primer pathHint amarillo (z=-3) que apunta hacia la placa dentro del CorridorA. La vista está bloqueada por la puerta. **Pregunta natural:** *"¿Cómo abro algo que está detrás de la puerta?"*

### 2.3 Setup (10–40s — descubrimiento del Eco)
- camino por el borde del PorchHall revela un **cajón cerrado** (prop `CarritoConserje` o `CajaCartonCerrada`) frente a la puerta sellada. Al tocarlo (`interaction.n01_lockbox`) → pop-up conviction: *"Esto no se abre con manos."*
- Cerca, una **pizarra o muro de piedra** con tiza evidente (decal `chalk_trace`). Al inspeccionar → *"Alguien graba lo que hago. Yo no pedí eso."* (conviction). **HINT implícito: existe un sistema de grabación/reproducción.**
- HUD introduce control de grabación (Pop-up Teaching): *"Pulsa [R] para grabar. Camina. [R] otra vez para soltar."* — tope 42 chars. [Esto es **el único** texto tutorial no-diegético del nivel.]

### 2.4 Complicación (40–90s — variación espacial)
1. **Primer intento ingenuo (se enseña el fracaso):** jugador graba caminando hacia la puerta sellada → suelta eco → eco golpea la puerta sellada. Puerta sellada **no se abre** (no acepta eco como llave; requiere eco pisando **PlateA dentro** del CorridorA).
   - Pop-up conviction: *"Yo no fui la que se fue."* (idéntico al spec verbatim). Fracaso informativo: *la grabación reproduce adónde fuiste, no adónde quisiste ir.*
   - **Retry <10s:** eco limpiado automáticamente al fallar; HUD toast: *"Grabación descartada."* (≤42 chars).
2. **Segundo intento — insight:** jugador nota `EchoPathHint` (z=10) ya dentro del CorridorA — *imposible de llegar* caminando. La luz fica parpadea (`flicker=true`) sólo cuando grabas. **Señal diegética:** grabar dentro del PorchHall crea eco; el eco no puede atravesar la puerta sellada porque tú tampoco podrías.
   - Insight玩家的: *"¿Y si grabo caminando dentro del corredor?"* → pero no puede entrar…
3. **Truco enseñado por el eco previo (Aha #1):** la placa en z=15 está **dentro** de CorridorA. Player no puede entrar (puerta sellada). Pero la puerta sellada realmente sólo cierra a **Aiden en vivo**, no a su eco-import. **Wait — no, eso rompe la regla.** Aha alternativo:
- **Solución limpia (no requiere excepciones):** La puerta sellada (entre PorchHall→CorridorA) se abre con *cualquier* presión — es decir, abre si Aiden o el eco pisan una **Segunda Placa (PlatePorch)** al fondo del PorchHall (z=+1, junto a la entrada). PlateA (z=15 dentro de CorridorA) abre ExitGate. El puzzle es:
  - **PlatePorch** (Nueva, dentro de PorchHall) → abre Puerta Sellada → permite entrar a CorridorA.
  - **PlateA** (z=15 dentro de CorridorA) → abre ExitGate z=52 → salida.
  - **No puedes pisar ambas a la vez.**
  - → Debes grabar pisando PlatePorch dejar eco reproduciendo en la placa, luego correr hacia CorridorA y desde ahí grabar otro eco que pise PlateA. **Pero tienes sólo 1 slot de eco (maxEchoes 1)** →.Blocker: necesitas encadenar.
- **Rediseño definitivo (más limpio, respeta maxEchoes=1 y teaching):**

#### ARQUITECTURA FINAL DEL PUZZLE N01 (3 beats):

```
[PorchHall]                                       [CorridorA]                    [ExitGate]
                                          ┌──────(puerta sellada)─────┐
 z=-5 PlayerStart                          z=8                        z=15 PlateA
 z=-3 pathHint (Porch)                     z=10 pathHint              ... → z=52 ExitGate
                                           z=12 second pathHint      ...
 z=-1 PlatePorch (RECUERDA: nueva placa)   z=14 *ECO PATH GOES HERE*
```

- **PlatePorch** z≈-1 (dentro del PorchHall, justo antes de la puerta sellada).
- **Puerta Sellada** z≈7 (entre PorchHall y CorridorA). Abre con PlatePorch.
- **PlateA** z=15 (ya en el blueprint original). Abre ExitGate z=52.

**Beat 1 — SAFE EXPERIMENT (0–40s)**
- Aiden descubre que no puede entrar (puerta sellada con `interaction.n01_sealed_door`).
- HUD enseña "Pulsa [R] para grabar. [R] otra vez para soltar."
- Aiden graba caminando hacia PlatePorch en z=-1 → suelta eco → eco pisa PlatePorch → Puerta Sellada abre → **Aiden camina entrando en vivo durante el replay**. Aha #1: *el eco hace lo que yo hice.* Pop-up conviction: *"Yo no necesito entrar aqui."* (cuando inspecciona la puerta antes de resolverla).

**Beat 2 — CONFRONTATION (40–80s)**
- Ya dentro de CorridorA, Aiden ve PlateA z=15 → inspecciona → *"Esto pide memoria, no pies."* Pop-up conviction ≤42 chars.
- Pero Aiden ya no tiene eco (consumió el slot en Beat 1). Debe grabar de nuevo. Si vuelve a PlatePorch, la puerta sellada se cerrará otra vez. **Aha #2:** Debe grabar **caminando dentro de CorridorA**, soltar el eco en PlateA, y correr a la salida mientras el eco mantiene abierta ExitGate.
- Fracaso educativo: si Aiden graba pero camina hacia atrás (z negativa) el eco no llegará a PlateA. Toast conviction: *"Grabación descartada."* Retry <10s.
- Fracaso #2: si Aiden graba correctamente pero se queda mirando (no corre a ExitGate), ExitGate se cierra cuando el eco baja de la placa (eco no latching) → *"El pasado no se sostiene sin testigo."* conviction, ≤42 chars.

**Beat 3 — PAYOFF (80–95s)**
- Aiden graba dentro de CorridorA caminando z=8→15, suelta eco, eco continua caminando hacia PlateA z=15, mantiene placa pisada durante ~2s — ventana en la que Aiden corre hacia ExitGate z=52 → cruza LiminalThreshold (z=48) → pasa ExitGate abierta → entra en LevelExit_Area z=54 → nivel completa.
- LiminalThreshold = beats visto/liminal: niebla cerrada, post-proc PS1, fog density sube gradualmente z=40→48. **Aha #3:** *"Desorientación superada."* (frase verbatim del blueprint `puzzleCompleteText`).

### 2.4 Pacing
| Beat | Duración | Emoción (arc) | Mecánica |
|---|---|---|---|
| Hook (PorchHall) | 10s | `denial` | Inspección puerta sellada, *"no necesito entrar"* |
| Setup | 30s | `avoidance` | Lee pizarra, cajón, aprende a grabar (R) |
| Beat 1 (Safe Experiment) | 40s | `avoidance→confrontation` | Grabar→soltar→eco pisa plate→puerta abre |
| Beat 2 (Confrontation) | 40s | `confrontation` | Repite mecánica con insight: grabar dentro, correr |
| Beat 3 (Payoff) | 15s | `apertura_a_ver` | Cruza umbral liminal → done |
| Fracaso | <10s | `shame` | *"Grabación descartada"* / *"El pasado no se sostiene"* |

**Total: ~135s** (curva Fácil → Satisfactorio). Sin textos tutoriales after Setup.

### 2.5 Componentes nuevos a añadir (en Unity)

| Componente | Posición | Tipo prefab | Wiring |
|---|---|---|---|
| `PlatePorch` (PressurePlate) | (0,.05,-1) | `PMK_PressurePlate_EchoOnly` | `targetSignals: [SealedDoor]` |
| `SealedDoor` (Door) | (0,1.5,7) | `PMK_DoorFrame_8x4` con Door logic | signal target, `latching=false` |
| (Existentes sin tocar) |  |  |  |
| `PlateA` | (0,.05,15) | ya en blueprint | → ExitGate |
| `ExitGate` | (0,1.5,52) | ya en blueprint | ← PlateA, `latching=false` |

> **Prioridad:** modificar `Level_01_Blueprint.asset` para añadir `PlatePorch`(type 4) y `SealedDoor`(type 5). **No editar la escena a mano** — el builder regenera.

---

## 3. LAYOUT DE DECORACIÓN + PROPS NARRATIVOS

### 3.1 Filosofía por habitación (`room_meaning`)
> Vestíbulo — *no quiere entrar.* Corredor — *no quiere mirar las puertas.*

**PorchHall (z=-7..-1)** — "El vestíbulo que no quiere entrar"
- Iluminación: la única fuente cálida del nivel (point light amber ~2700K, intensidad 0.6). Niebla más densa localmente (volumen fog 0.025). Tono sepia.
- Props narrativos (todos con `InteractableObject`):
  - `RelojPared` (reloj parado) en pared z=-6, izquierda. `interaction.n01_stopped_clock` — *"El tiempo no avanza. Yo tampoco."* conviction. **No es Lyra-artifact.**
  - `Perchero` con `AbrigoColgado` en pared z=-6, derecha. `interaction.n01_coat` — *"No es mio. No lo toco."* (No-Lyra), deflectivo.
  - `CajaCartonCerrada` / `CarritoConserje` frente a puerta sellada. `interaction.n01_lockbox` — *"Esto no se abre con manos."*
  - Decal `ChalkDrawing` sobre el suelo z=-3 con texto/hint (tiza de dos manos pequeñas). Solo visual.
  - `Prop_RecordsBoard` (landmark LAND-003) en pared z=-7 (entrada). `interaction.n01_records_board` — *"No miro lo que dejó. No hoy."* conviction. **Es Lyra-artifact** (bump counter).

**CorridorA (z=8..28)** — "El corredor que no quiere mirar las puertas"
- Iluminación: fluorescentes parpadeando (lámparas en techo cada 6m, `Fluorescente` prefab, **flicker=true** — pulso breve al grabar). Color frío 4000K. Fog más tenue localmente (density 0.006).
- Props a lo largo (todos con `InteractableObject`):
  - 2 × `Locker` + 1 `LockerPuertaAbierta` en pared izquierda z=12, z=24. `interaction.n01_locker` — *"Ninguno es mio."*
  - `Prop_ChalkDrawing` en el suelo z=14 (decal tiza, *manos separándose*) — visuals hint hacia PlateA.
  - `Radiador` pared derecha z=20. `interaction.n01_radiator` — *"Esto aun calienta. Como si alguien tuviera que volver."*
  - `RelojPared` parado en z=18. `interaction.n01_stopped_clock_corridor` — *"Otro reloj. No miro la hora."*
  - 3 × `EchoPathHint` prefabs (z=10, z=15 junto a PlateA, z=20) — al apuntar el haz de luz de los pathHints ya declarados.

**LiminalThreshold (z=44..52)** — "El umbral"
- Iluminación: **sin luz local** — niebla aumenta densidad z=44→52 (0.008 blueprint → subir a 0.05 dentro del объем). Luz direccional del exterior se atenúa.
- Props:
  - `VentanaMarco` en pared izq z=46 — venas de luz fría en polvo (lumen volumétrico).
  - Decal `MoistureStain` oscura en pared z=48 — núcleo del umbral.
  - `Prop_StoppedClock` colgando de pared z=50, roto. `interaction.n01_broken_clock` — *"Todo se detuvo aqui. Yo tambien."* conviction. **Lyra-artifact.**
  - `EchoPathHint` z=48 (el último).

### 3.2 Landmarks (validan `landmarks.yaml`)
- `LAND-005 StoppedClock` en SchoolHall + SchoolCorridor (cumple).
- `LAND-003 RecordsBoard` en SchoolHall (cumple).
- `LAND-VAL-001`: tramos máx 18m sin landmark → CorridorA (20m) y CorridorB (20m) superan. **Mitigar:** poner un landmark cada <18m (reloj parado z=18, cartelera z=24). **Añadido a este redesign.**

### 3.3 Post-proc por capítulo (I_Negacion)
- **Tonemap:** ACES, lift +0.02, gains 0.9 (imágenes planas, low-contrast).
- **Vignette:** intensidad 0.45, radio 0.5 — sensación de túnel.
- **Chromatic Aberration:** 0.15 (sutil PS1 aberration, suave).
- **Film Grain PS1:** 0.08 (referente al shader `PS1World.shader` o post-proc custom si está).
- **Color Override URP Volume (global):** temperatura -5 (frío), tinte +3 (verde leve — escuela duelo).
- **AnalogGhost:** residual echo shader durante playback — opacidad 0.5 a 0.7 (vapor del eco).

---

## 4. DIÁLOGOS DE INTERACCIÓN — `CommentKey` (tecleados en `VN_Text.json`)

> Sistema: `InteractableObject.commentKey` (string) → `VN_TextTable.Get(key, stage)` donde `stage = conviction` (N01; threshold stage_threshold_by_level `N01:0`).
> Formato JSON: `{"key": "...", "title": "...", "is_lyra_artifact": bool, "tone": {"conviction": "...", "guilt": "...", "realization": "...", "acceptance": "..."}}`. En N01 **solo conviction** se mostrará pero se rellenan todas las tonos (otros niveles las usan). Tono: defensivo, corto, deflexivo. **Máx 42 chars (pop-up diegético).**

| CommentKey | Title | Lyra? | Conviction (USAR EN N01) | Notas |
|---|---|---|---|---|
| `interaction.n01_stopped_clock` | Reloj | no | `"El tiempo no avanza. Yo tampoco."` | 31 chars ✓ |
| `interaction.n01_coat` | Abrigo | no | `"No es mio. No lo toco."` | 21 chars ✓ |
| `interaction.n01_lockbox` | Cajón | no | `"Esto no se abre con manos."` | 26 chars ✓ |
| `interaction.n01_records_board` | Tablón | **sí** | `"No miro lo que dejó. No hoy."` | 26 chars ✓ · bump Lyra counter |
| `interaction.n01_locked_door` | Puerta sellada | no | `"Yo no necesito entrar aqui."` | 27 chars ✓ (verbatim voice sample spec) |
| `interaction.n01_locker` | Taquilla | no | `"Ninguno es mio."` | 15 chars ✓ |
| `interaction.n01_radiator` | Radiador | no | `"Aun calienta. Como si alguien volviera."` | 39 chars ✓ |
| `interaction.n01_stopped_clock_corridor` | Reloj | no | `"Otro reloj. No miro la hora."` | 29 chars ✓ |
| `interaction.n01_broken_clock` | Reloj roto | **sí** | `"Todo se detuvo aqui. Yo tambien."` | 32 chars ✓ · bump Lyra counter |
| `interaction.n01_chalk_floor` | Tiza en suelo | no | `"Alguien graba lo que hago. No pedi eso."` | 39 chars ✓ |

**Coherencia con `DIALOGUE_TREE_SCHEMA.yaml` `tone_by_stage.conviction`:** persona *"defensiva, corta, deflexiva"*, evita `["tenía_razón","ella_empezó","no_me_culpa"]`, tema temporal = "solo presente y futuro hipotético", no admite pasado.

**Flags VN seteados por interacción (Lyra-artifacts contados):** `BumpLyraArtifactSeen()` incrementa counter usado en `VN_EndingResolver`. N01: 3 artifacts posibles (RecordsBoard + BrokenClock + 1 opcional en N02).

---

## 5. VN CHOICES — `ch1_choice_1` (Base, post-nivel)

> Setup: tras cruzar LevelExit_Area (z=54), `VN_ChoiceGateController` abre `VN_ChoiceGateUI.uxml` con `promptKey: vn.ch1.choice.1`.

### 5.1 Textos VN (clave `vn.ch1.choice.1`)
> Prompt label único, 2 opciones. Cyan/amber setean flags en `VN_EndingFlags`. Máx ~80 chars por botón.

```
vn.ch1.choice.1.prompt = "Acabas de cruzar el umbral. ¿Qué haces con lo que viste?"
  choice.vn.ch1.choice.1.cyan  = "Voy a mirar."      → flag allow_to_see   (+1 comprehension)
  choice.vn.ch1.choice.1.amber = "Voy a pasar de largo." → flag avoid_looking (+0 comprehension)
```

- Cyan añade el flag `allow_to_see` a `VN_EndingFlags` → +1 comprehension_score.
- Amber añade `avoid_looking` → sin bonus.
- Sijte: `stage_threshold_by_level.N01 = 0` → N01 termina siempre en **conviction** (sin importar flags). Pero el `allow_to_see` persiste hasta N15.

### 5.2 Micro-choice N01 — **NO** (N01 no tiene micro-choice; las micro son N03, N07, N13 según tabla).

### 5.3 VN flags declarados en este nivel
- `allow_to_see` (cyan choice)
- `avoid_looking` (amber choice)
- `_LyraArtifactSeen_N01` implícito vía `BumpLyraArtifactSeen()` (counter compartido).

---

## 6. AUDIO POR HABITACIÓN

> `GameHUD` + `GameFeelController` disparan. 8 .wav/.mp3 disponibles. Mapping N01:

| Zona | Source arch | Evento | Archivo |
|---|---|---|---|
| PorchHall (loop) | ambient loop | nada | `274213__bexhillcollege__college-hallway-ambience.wav` (lowpass 600Hz) |
| CorridorA (loop) | ambient loop | nada | `Ventilation.wav` (más seco) |
| CorridorB (loop) | ambient loop | nada | `144046__gchase__room_tone_ambience_medium_control_low_hum.wav` |
| PlatePorch / PlateA (OnPress) | SFX | placa pisada | `CLICK.mp3` (mono) |
| SealedDoor open | SFX | puerta | `PUERTA.mp3` (lowpass 1.5kHz) |
| ExitGate open | SFX | puerta grande | `PUERTA.mp3` (sin filtro) |
| Grabar iniciar | SFX | R press | `GRABACIÓN INICIO.mp3` |
| Eco creado | SFX | R release | `CREACIÓN DE ECO.mp3` |
| Playback loop | SFX | eco activo | `LOOP DE ECO.mp3` (loop, vol 0.4) |
| Toast "Grabación descartada" | UI SFX | fracaso | `reset.mp3` |
| LiminalThreshold (z=44+) | drone sub-bass | transición | `607238__szegvari__electric-dream-synth-drone.wav` (enter z=42) |
| VN Choice Gate open | UI SFX | post-nivel | `574865__trp__vhs-tape-clicks-rewind-play-mechanical-05.flac` |
| Reloj parado inspección | SFX | on inspect | `freesound_community-clock-chime-88027.mp3` (primer 0.3s) |

---

## 7. PLACEHOLDERS PARA EL USUARIO

> Estos NO existen aún — el usuario (tú) prepara/placeholder.

### N01 PLACEHOLDERS:
**TEXTURAS** (deck rokadas bajo `Assets/Art/N01/`):
- □ `n01_wall_hall.png` — pared del vestíbulo, mármol fracturado tono sepia (PS1 128×128, low-bit)
- □ `n01_floor_hall.png` — suelo mosaico escolar, baldosa moteada
- □ `n01_wall_corridor.png` — azulejo escolar verde apagado
- □ `n01_floor_corridor.png` — linóleo grisáceo con rayones
- □ `n01_wall_threshold.png` — textura "liminal material" (vacío difuso, ocupar shader `LiminalSurface` sin textura como fallback — **placeholder opcional**)
- □ `n01_chalk_decal.png` — tiza manos-dos manos dibujo alpha-cut
- □ `n01_moisture_decal.png` — humedad esquina z=48

**AUDIO**:
- □ `n01_porch_drone_alt.wav` — si `bexhillcollege` no es tonal correcto, alternativa sepia loop (60–80bpm)
- □ `vn.ch1.choice.1.prompt.wav` — opcional: voz en off del prompt VN (silencio si no se graba → texto claw en UI)

**TEXTOS VN** (añadir a `Assets/Resources/VN_Text.json` en bloque `vn`):
- □ `vn.ch1.choice.1.prompt` = *"Acabas de cruzar el umbral. ¿Qué haces con lo que viste?"*
- □ `vn.ch1.choice.1.cyan` = *"Voy a mirar."*
- □ `vn.ch1.choice.1.amber` = *"Voy a pasar de largo."*
- □ (Las 10 claves `interaction.n01_*` de la §4 ya están listas arriba; copiar verbatim al JSON)

**PROPS ESPECÍFICOS** (colocar en Unity — todos son prefabs que YA existen):
- □ `RelojPared.prefab` ×2 (z=-6, z=18) — RelojParado via shader/inspector
- □ `Perchero.prefab` + `AbrigoColgado.prefab` parentados (z=-6)
- □ `CarritoConserje.prefab` o `CajaCartonCerrada.prefab` (delante de SealedDoor z=6)
- □ `Prop_RecordsBoard.prefab` (z=-7)
- □ `Locker.prefab` ×2 + `LockerPuertaAbierta.prefab` ×1 (z=12, z=24 pared izq)
- □ `Radiador.prefab` (z=20 pared der)
- □ `Prop_StoppedClock.prefab` (z=50, roto — rotación inclinada, decal `Cracks`)
- □ `VentanaMarco.prefab` (z=46 pared izq)
- □ Decals `ChalkDrawing` (z=-3, z=14), `MoistureStain` (z=48), `Cracks` (z=50)
- □ **EchoPathHint positionsZ** (pathHints blueprint): -3, 10, 30, 48 (ya declaradas; el builder coloca los prefabs)

**ILUMINACIÓN ESPECIAL**:
- □ Point light amber 2700K, intensidad 0.6, rango 8 — sobre `RelojPared` z=-6 (PorchHall)
- □ 4 × `Fluorescente.prefab` en techo CorridorA z=12,18,24 → **flicker=true** (controlado por `GameFeelController` cuando `EchoRecorder.IsRecording == true`)
- □ Volumne fog `LiminalFogVolume` en z=44..52 con densidad creciente (0.020→0.050)

**VN FLAGS**:
- □ `allow_to_see` (cyan)
- □ `avoid_looking` (amber)
- □ Lyra artifacts counter increments: `RecordsBoard` + `BrokenClock` (3 máximo posible en N01)

---

## 8. VALIDACIÓN PLAY MODE — CHECKLIST (Paso 2.5)

> Yo (agente) ejecutaré este checklist conectado a Unity Play Mode tras tu implementación.

### VALIDACIÓN N01 — CHECKLIST:
**PUZZLE:**
- [ ] Hook visible en <10s sin texto explícito (puerta sellada + pizarra de tiza)
- [ ] Plataforma de grabación clara (HUD Teaching pop-up, ≤42 chars; sólo **una** vez)
- [ ] PlatePorch detectada por eco 360° (esfera collider)
- [ ] Beat 1 transitivo: grabar→soltar→eco pisa→puerta abre
- [ ] Fracaso #1 (grabar contra puerta sellada): toast "Grabación descartada" <10s
- [ ] Beat 2: insight "grabar dentro de CorridorA", demanda insight jugador
- [ ] Fracaso #2 (grabar y quedarse): "El pasado no se sostiene sin testigo" <42 chars
- [ ] ExitGate se cierra cuando eco abandona PlateA (`latching=false`)
- [ ] LiminalThreshold fog densidad creciente visualizado z=40..48
- [ ] *"Aha moment"* de Aha#1 → Aha#2 → *"Desorientación superada"* en <120s

**NARRATIVA (pop-ups):**
- [ ] 10 claves `interaction.n01_*` con texto `conviction` ≤42 chars
- [ ] Tono: defensivo, corto, deflexivo, persona *"Aiden"*, no `tenía_razón/ella_empezó/no_me_culpa`
- [ ] 2 Lyra-artifacts (`RecordsBoard`, `BrokenClock`) → `VN_EndingFlags.BumpLyraArtifactSeen()` ×2

**VN (post-nivel):**
- [ ] `VN_ChoiceGateController` dispare al cruzar `LevelExit_Area` (z=54)
- [ ] `vn.ch1.choice.1.prompt` texto en UI Toolkit (UI de Stitch aprobado)
- [ ] Cyan → flag `allow_to_see` persistido en `VN_EndingFlags`
- [ ] Amber → flag `avoid_looking` persistido
- [ ] No micro-choice en N01 (comprobado)

**ILUMINACIÓN / ATMÓSFERA:**
- [ ] PorchHall luz amber 2700K, point light rango 8, fog 0.025
- [ ] CorridorA fluorescente flicker SOLO durante grabación (`EchoRecorder.IsRecording`)
- [ ] LiminalThreshold sin luz local, fog 0.020→0.050 creciente z=40..52
- [ ] Post-proc URP Volume global: ACES + vignette 0.45 + grain 0.08 + chromatic 0.15
- [ ] `607238__szegvari__electric-dream-synth-drone.wav` entra z=42 (bajo)

**AUDIO:**
- [ ] Loop ambient PorchHall: `bexhillcollege` lowpass 600Hz
- [ ] Loop ambient CorridorA: `Ventilation.wav` (seco)
- [ ] SFX plate press: `CLICK.mp3`
- [ ] SFX R press / R release grabación: `GRABACIÓN INICIO` / `CREACIÓN DE ECO`
- [ ] Loop playback eco: `LOOP DE ECO.mp3` vol 0.4
- [ ] SFX puerta: `PUERTA.mp3` (con lowpass puerta sellada / sin filtro ExitGate)

**PROPS / COLISIONES:**
- [ ] Player no se hunde en PorchHall/Corridor/Threshold (colliders floor)
- [ ] Eco colisiona física (cuerpo replay) con PlatePorch, PlateA, SealedDoor, ExitGate
- [ ] Placas detectan a eco 360° (esfera trigger, no caja)
- [ ] Props narrativos colocados con `InteractableObject` + `triggerRadius 2.5`
- [ ] Landmarks <18m (reloj z=-6, reloj z=18, cartelera z=24) cumplen `LAND-VAL-001`

**TÉCNICO:**
- [ ] 0 errores en consola (`read_console` filter=error)
- [ ] 60 fps estables (examinar `stats_get` drawcalls <200)
- [ ] `Level_02` carga al cruzar exit (no colgarse en transition)
- [ ] Builder regenerable: si `EchoesNewProductionBuilder` re-run, sólo los modules del blueprint se regeneran (props puestos a mano persisten? — verificar con usuario)

**VN ENDINGS downstream:**
- [ ] `allow_to_see` incrementa `comprehension_score`
- [ ] Sin `allow_to_see` + reiterar `avoid_looking` en N02/N03 → aumenta ruta al ending *Aislamiento* (Aiden en Corredor N01)
- [ ] Tres Lyra-artifacts posibles en N01; counter persiste (`Don'tDestroyOnLoad` en `VN_EndingFlags`)

---

## 9. ROL EN EL ARCO PROYECTO

- **Setup:** N01 establece el **Eco** como mecánica + Aiden en **conviction** (defensiva total). `allow_to_see`/`avoid_looking` son **el primer cisma narrativo** del proyecto.
- **Echo-through:** el Beat 1 → Beat 2 transición establece el insight fundamental: *"El eco hace lo que yo hice"* — base para todos los puzzles posteriores (N02 simultaneous plates, N05 timing maze, N08 dual echoes).
- **Ending gate:** el flag `allow_to_see` (false si amber) es la semilla del ending **Aislamiento**, cuyo epílogo (`VN_ENDINGS_REDEFINED.yaml` line 129) retorna a Aiden al corredor del **N01** — simetría proyectada para N15.

---

## 10. CAMBIOS EN BLUEPRINT (resumen para edición)

En `Assets/Data/Levels/Level_01_Blueprint.asset`, append/mod en el bloque `modules:`:

```diff
   - name: PorchHall
     type: 31
     position: {x: 0, y: 0, z: -3}
+    customData: sealed_door=true;porch_light=amber
+  - name: PlatePorch
+    type: 4
+    position: {x: 0, y: 0.05, z: -1}
+    rotation: {x: 0, y: 0, z: 0}
+    scale: {x: 1, y: 1, z: 1}
+    targetSignals:
+    - SealedDoor
+  - name: SealedDoor
+    type: 5
+    position: {x: 0, y: 1.5, z: 7}
+    rotation: {x: 0, y: 0, z: 0}
+    scale: {x: 4, y: 3, z: 0.5}
+    customData: non-latching
   - name: CorridorA
     type: 32
     position: {x: 0, y: 0, z: 10}
     customData: flicker=true
   - name: PlateA
     type: 4
     position: {x: 0, y: 0.05, z: 15}
+    targetSignals:
+    - ExitGate
   - name: ExitGate
     type: 5
     position: {x: 0, y: 1.5, z: 52}
+    customData: non-latching
```

> PathHints se mantienen. Coordenadas de módulos existentes no se tocan. **La edición del asset `.asset` conviene hacerla en Unity Inspector (hazlo el usuario — yo no edito .asset serializado a mano salvo fix crítico).**

---

**DOCUMENTO LISTO PARA IMPLEMENTACIÓN.**

---

## ENTREGABLE — RESUMEN PARA EL USUARIO

1. ✅ Documento diseño completo (este .md) en `Docs/Design/N01_DESIGN.md`
2. 📋 Lista placeholders §7 (texturas, audio, textos VN, props, luces, flags)
3. ✔ Checklist validación Play Mode §8 (yo corro tras tu build)

### Próximo paso
- **Tú:** Implementar en Unity (modifica `Level_01_Blueprint.asset` §10, coloca props §3, escribe 10 claves interaction §4 + 3 claves VN §5.1 en `VN_Text.json`, setea audio §6, luces §3.3).
- **Después:** Juegas el nivel en Play Mode → Yo valido con §8 checklist → Feedback → Iteras → APROBADO → N02.
