# N01 — PLAN DE IMPLEMENTACIÓN DETALLADO (Delta del diseño)

> Estado: **ESPERA APROBACIÓN USUARIO** antes de tocar Unity.
> Base: revisión real de `Level_01.unity` + `VN_Text.json` + scripts del runtime.

---

## 0. DESCUBRIMIENTOS CLAVE DE LA REVISIÓN REAL

### 0.1 El sistema VN ya funciona — NO hay que armarlo
- `Assets/Resources/VN_Text.json` ya contiene **18 choices** (incluye `ch1_choice_1` con prompt + cyan + amber verbatim) y **5 epilogue** endings (*Aislamiento, Ruminacion, Negociacion, Desesperacion, Aceptacion*).
- `Assets/Resources/VN_ChoiceRegistry.asset` tiene los 20 nodos (17 base + 3 micro) con las flags correctas.
- `LevelRuntimeController.OnLevelCompleted()` (líneas 246-277) **ya dispara** `VN_ChoiceGateController.Instance.Show(levelIdx, isMicro, callback)` cuando Aiden cruza `LevelExit_Area`.
- `VN_ChoiceGateController.Show()` carga el registry por `Resources.Load` si no está asignado (línea 69). El `VN_TextTable.GetChoice()` ya resuelve los prompts.

→ **El problema no es el sistema VN.** Es que el nivel no tiene contenido narrativo y los props no están.

### 0.2 Hallazgos del Level_01.unity real (vs blueprint)

| Item | Blueprint declara | Escena real tiene | Diferencia |
|---|---|---|---|
| PorchHall | 8×3×6 @ z=-3 | `PorchHall` @ z=-3 con 4 columnas + 4 luces `LightFlicker` | OK + extra decorativo |
| CorridorA | 6×3×20 @ z=10 | **NO existe** como nodo separado | Fusionado en `CorridorCentral` |
| CorridorB | 6×3×20 @ z=30 | **NO existe** | Fusionado en `CorridorCentral` |
| CorridorCentral | (no declarado) | 4×55.5 @ z=24.5, Floor+Wall_L+Wall_R, 닫는 z≈-3..52 | **Sustituye A+B** |
| LiminalThreshold | 8×3.5×8 @ z=48 | **NO existe** como nodo | Vacío (`Section_Climax` placeholder z=47.6) |
| PlateA | z=15 | `PlateA` @ z=15 con `PressurePlate` | ✓ |
| ExitGate | z=52 | `ExitGate` @ z=52 con `DoorController` + `KenneyTiling` | ✓ y más rico |
| LevelExit_Area | z=54 | (representado por `LevelExit` component en PlateA o similar) | Verificado por `LevelExit` found |
| PlayerStart | z=-5 | `Player` @ z=-3 (PlayerController + EchoRecorder) | Ligeramente adelantado 2m, neutro |
| Props narrativos | (no declarados) | **0** | Falta todo |
| InteractableObject | (no declarados) | **0** en toda la escena | Falta todo |

### 0.3 Schema de props ya existentes confirmados en el repo
| Prefab path | existe |
|---|---|
| `Assets/Prefabs/Props/RelojPared.prefab` | ✓ |
| `Assets/Prefabs/Props/Perchero.prefab` | ✓ |
| `Assets/Prefabs/Props/AbrigoColgado.prefab` | ✓ |
| `Assets/Prefabs/Props/CarritoConserje.prefab` | ✓ |
| `Assets/Prefabs/Props/Radiador.prefab` | ✓ |
| `Assets/Prefabs/Props/Estanteria.prefab` | ✓ |
| `Assets/Prefabs/Props/Locker.prefab` | ✓ |
| `Assets/Prefabs/Props/LockerPuertaAbierta.prefab` | ✓ |
| `Assets/Prefabs/Props/VentanaMarco.prefab` | ✓ |

---

## 1. ALCANCE DEL PLAN (claro y atomic)

### Qué **NO** haré yo (decisiones del usuario)
- ❌ No modificaré `Level_01_Blueprint.asset` (los `.asset` serializados conviene tocarlos en Unity Inspector)
- ❌ No implementaré el redesign del puzzle (`SealedDoor` + `PlatePorch`) porque requiere editar el blueprint y regenerar con `EchoesNewProductionBuilder`
- ❌ No añadiré el `LiminalThreshold` module que falta
- ❌ No tocaré la arquitectura del `CorridorCentral` fusionado

### Qué **SÍ** haré yo (sin esperar al usuario, una vez aprobado)
1. **Añadir 8 entradas `interaction.n01_*` a `VN_Text.json`** (no se rompe nada; las existentes quedan intactas)
2. **Colocar 8 props narrativos en `Level_01.unity`** (en PorchHall y CorridorCentral)
3. **Añadir `InteractableObject` a cada prop** con `CommentKey` y `DisplayName` correspondientes
4. **Añadir el `VN_ChoiceGateController` si no está** (verificación: no encontrado en escena; vendrá de MainMenu via `DontDestroyOnLoad` — confirmo)

---

## 2. DELTA JSON — AÑADIR A `VN_Text.json`

Estructura actual: array `interaction: [...]` con 20 entradas. **Append** (no replace) 8 nuevas:

```json
{"key":"interaction.n01_clock_porch","title":"Reloj","is_lyra_artifact":false,"tone":{"conviction":"El tiempo no avanza. Yo tampoco.","guilt":"Marque esta hora mil veces. Aun late.","realization":"El tiempo no me debia nada.","acceptance":"Puedo dejar que el reloj corra."}},
{"key":"interaction.n01_coat","title":"Abrigo","is_lyra_artifact":false,"tone":{"conviction":"No es mio. No lo toco.","guilt":"Lo sostuve cuando ella temblaba.","realization":"Tambien abrigue, no solo mire.","acceptance":"Puedo soltar la tela sin soltarla a ella."}},
{"key":"interaction.n01_lockbox","title":"Cajon","is_lyra_artifact":false,"tone":{"conviction":"Esto no se abre con manos.","guilt":"Guarde cosas que no debi cerrar.","realization":"Cerre por miedo, no por cuidado.","acceptance":"Puedo abrirlo sin necesidad de cerrar."}},
{"key":"interaction.n01_records_board","title":"Tablon","is_lyra_artifact":true,"tone":{"conviction":"No miro lo que dejo. No hoy.","guilt":"Sus notas aun penden. Yo las arrugue.","realization":"Dejo rastro, igual que ella.","acceptance":"Puedo leer sus notas sin que me definan."}},
{"key":"interaction.n01_locker","title":"Taquilla","is_lyra_artifact":false,"tone":{"conviction":"Ninguno es mio.","guilt":"La mia esta vacia y la.llene de fichas.","realization":"Tambien use estos pasillos como refugio.","acceptance":"Puedo pasar sin reclamar ninguno."}},
{"key":"interaction.n01_radiator","title":"Radiador","is_lyra_artifact":false,"tone":{"conviction":"Aun calienta. Como si alguien volviera.","guilt":"Me sente aqui a esperar calor que no llegaba.","realization":"El calor no me debia venir de afuera.","acceptance":"Puedo calentarme sin pedirle a esto."}},
{"key":"interaction.n01_stopped_clock_corridor","title":"Reloj","is_lyra_artifact":false,"tone":{"conviction":"Otro reloj. No miro la hora.","guilt":"Prometi volver a las seis. No volvi.","realization":"Detener el reloj no detiene el lunes.","acceptance":"Puedo mirar la hora sin prometer nada."}},
{"key":"interaction.n01_broken_clock","title":"Reloj roto","is_lyra_artifact":true,"tone":{"conviction":"Todo se detuvo aqui. Yo tambien.","guilt":"Lo rompi yo, pero el ya estaba parado.","realization":"El tiempo que detuve era el mio.","acceptance":"Puedo dejar de contar lo perdido."}}
```

**Verificación lint (conviction ≤ 42 chars):**
- `"El tiempo no avanza. Yo tampoco."` → 32 ✓
- `"No es mio. No lo toco."` → 22 ✓
- `"Esto no se abre con manos."` → 26 ✓
- `"No miro lo que dejo. No hoy."` → 28 ✓ (Lyra-artifact)
- `"Ninguno es mio."` → 16 ✓
- `"Aun calienta. Como si alguien volviera."` → 38 ✓
- `"Otro reloj. No miro la hora."` → 28 ✓
- `"Todo se detuvo aqui. Yo tambien."` → 32 ✓ (Lyra-artifact)

**No añado `vn.ch1.choice.1`** al JSON porque ¡ya existe verbatim! (`prompt=Voy a mirar lo que deje cerrado.` / `cyan=Voy a mirar` / `amber=Paso de largo`). Sólo necesito confirmar que `VN_TextTable.GetChoice("ch1_choice_1")` lo resuelve. Tienen que ajustar el prompt del N01_DESIGN anterior si quieres sentir el *"Acabas de cruzar el umbral…"* — esa línea sería un punto de validación en Play Mode.

---

## 3. PROPS A COLOCAR EN `Level_01.unity` (8 objetos con InteractableObject)

### 3.1 PorchHall (z=-3, 8 ancho, 0 profundidad tile — pseudo-espacio de vestíbulo)

| # | Prefab | Posición (local a PorchHall) | Rotation | DisplayName | CommentKey | Lyra? |
|---|---|---|---|---|---|---|
| 1 | `RelojPared.prefab` | (-3.8, 2.5, -1.5) | 0/90/0 (mirar +X) | `Reloj` | `interaction.n01_clock_porch` | no |
| 2 | `Perchero.prefab` | (3.5, 0, -1.8) | 0/180/0 | `Perchero` | `interaction.n01_coat` | no |
| 3 | *Child of #2* `AbrigoColgado.prefab` | (0, 0.3, 0) relativo al perchero | inherit | (no interactuable propio) | — | — |
| 4 | `CarritoConserje.prefab` | (0, 0, 0.5) | 0/0/0 (frente a la salida) | `Cajon` | `interaction.n01_lockbox` | no |
| 5 | `Estanteria.prefab` (o `Prop_RecordsBoard.prefab` si existe) | (-3.8, 0, 1.5) | 0/90/0 (pegar pared izq) | `Tablon` | `interaction.n01_records_board` | **sí** |

> RecordsBoard: en `Assets/Prefabs/Props/Narrative/Prop_RecordsBoard.prefab` (confirmado existe). Preferir este sobre `Estanteria` para landmark LAND-003.

### 3.2 CorridorCentral (z=24.5, 4 ancho, 55.5 largo, Pool z=-3..52)

| # | Prefab | Posición (world) | Rotation | DisplayName | CommentKey | Lyra? |
|---|---|---|---|---|---|---|
| 6 | `Locker.prefab` | (-1.7, 0, 16) | 0/90/0 (pegar pared izq, abrir +X) | `Taquilla` | `interaction.n01_locker` | no |
| 7 | `Radiador.prefab` | (1.6, 0.3, 28) | 0/-90/0 (pared derecha) | `Radiador` | `interaction.n01_radiator` | no |
| 8 | `RelojPared.prefab` | (-1.7, 2.5, 34) | 0/90/0 (pared izq) | `Reloj` | `interaction.n01_stopped_clock_corridor` | no |
| 9 | `RelojPared.prefab` | (1.7, 2.3, 48) | 0/-90/0 (pared der 区 liminal) | `Reloj roto` | `interaction.n01_broken_clock` | **sí** |
| 10 | `LockerPuertaAbierta.prefab` | (-1.7, 0, 42) | 0/90/0 | (visual, sin interaction) | — | — |
| 11 | `VentanaMarco.prefab` | (1.9, 2.0, 48) | 0/-90/0 (pared der liminal) | (visual, sin interaction) | — | — |

**Total: 8 interactuables** (5 + 3) + 3 visuales sin interaction = 11 GameObjects.

### 3.3 EchoPathHint positions
El blueprint declara `pathHints: [(0,.1,-3),(0,.1,10),(0,.1,30),(0,.1,52)]`. **No los veo en la escena como GameObjects** — serán generados por `EchoesLevelShell` o por `LevelEnvironmentBootstrap` en runtime. Confirmar en Play Mode si aparecen visualmente.

---

## 4. CONFIGURACIÓN `InteractableObject` — plantilla para cada prop

```csharp
[RequiredComponent: Collider (trigger, isTrigger=true, radius = triggerRadius*0.5)]
commentKey        = "interaction.n01_<key>"
displayName       = "<títuloenumerado>"
isLyraArtifact    = false|true   (2 artifacts: RecordsBoard + BrokenClock)
requireEchoActive = false
cooldown          = 3.0
triggerRadius     = 2.5
```

**Ejemplo concreto para el Reloj del PorchHall:**
- `commentKey = "interaction.n01_clock_porch"`
- `displayName = "Reloj"`
- `isLyraArtifact = false`
- `triggerRadius = 2.5` (auto-genera SphereCollider trigger)

**Importante:** `InteractableObject.Awake()` (línea 28) llama `EnsureTrigger()` que añade/corrige el collider automáticamente. **No tengo que añadir el collider a mano** — basta con instanciar el prefab y agregar el componente `InteractableObject`, se autogestiona.

---

## 5. TEMAS PENDIENTES (que NO toco en este plan)

### 5.1 Redesign del puzzle (SealedDoor + PlatePorch) — pendiente usuario
Para implementar los 2 GameObjects nuevos (PlatePorch @ z=-1 dentro de PorchHall + SealedDoor @ z=7 entre PorchHall y CorridorCentral) necesito:
1. Editar `Assets/Data/Levels/Level_01_Blueprint.asset` y añadir esos módulos al array `modules:` (edición en Inspector recomendada).
2. Re-run `Echoes > Production > Build All Blueprint Levels` para regenerar la escena. **OJO:** Esto sobreescribe los props que coloqué en Paso 3.2 si no están marcados como fuera del greybox.

**Recomendación:** Implementar primero el plan narrativo (pasos 2-3) → Play Mode → validar flow narrativo → **después** añadir puzzle redesign (que requiere workflow builder/inspector).

### 5.2 `CorridorCentral` fusionado vs 2 Corridors del blueprint
El blueprint declara CorridorA + CorridorB (2 módulos separados). La escena real tiene 1 `CorridorCentral` de 55.5m. **Decisión**: ¿aceptas la escena fusionada (más simple visualmente) o prefieres que regeneremos con el builder para los 2 módulos del blueprint (fácil distinguir `flicker=true` de `flicker=false` en 2 tramos)?

### 5.3 `LiminalThreshold` ausente
El blueprint declara un módulo `LiminalThreshold` @ z=48 que **no existe** en la escena real (sólo placeholder `Section_Climax` vacío). Esto quiere decir que el "umbral narrativo" no está construido. Sería necesario o bien:
- Generar el módulo con `EchoesNewProductionBuilder`, O
- Manualmente crear unGameObject con decal/fog + VentanaMarco z=48 + BrokenClock z=50 (que es lo que mi plan de props hace implícitamente en el punto 9)

---

## 6. VALIDACIÓN POST-IMPLEMENTACIÓN (lo que yo verificaré en Play Mode)

### VALIDACIÓN IMPLEMENTACIÓN N01 — CHECKLIST
- [ ] **8 entradas interaction.n01_** presentes en `VN_Text.json` (sin romper las 20 existentes)
- [ ] **8 InteractableObject** en `Level_01.unity` con CommentKey y DisplayName correctos
- [ ] **2 Lyra-artifacts** (RecordsBoard + BrokenClock) → `VN_EndingFlags.BumpLyraArtifactSeen()` ×2 al inspeccion
- [ ] Player puede presionar E dentro de triggerRadius → spawn del `ShowInspection(title, text)` → HUD Chalkboard 2.5s
- [ ] Texto displayed es conviction (≤42 chars) — verificar que `AidenStageResolver.ResolveForCurrentLevel()` devuelva `Conviction` en N01 (threshold=0)
- [ ] `VN_ChoiceGateController` dispara al cruzar exit → prompt `ch1_choice_1` aparece
- [ ] Cyan → `allow_to_see` persiste en `VN_EndingFlags` (+1 comprehension)
- [ ] Amber → `avoid_looking` persiste (no +1)
- [ ] Scene transiciona a Level_02 tras fade
- [ ] 0 errores consola
- [ ] 60 fps estables

---

## 7. RIESGOS Y CAVEATS

1. **Trigger/collider conflicts** — Los prefabs ya tienen BoxCollider (no-trigger). `InteractableObject.EnsureTrigger()` añade SphereCollider-trigger si no hay collider, **pero si ya hay BoxCollider no-trigger, NO añade el trigger**. **Solución:** añadir a mano un SphereCollider `isTrigger=true` con `radius=1.25` junto con el componente `InteractableObject`. Verificar tras Play Mode.
2. **Layer collisions** — `InteractableObject.OnTriggerEnter` pide `other.CompareTag("Player")`. Player tag = "Player" (✓ confirmado en Player GameObject). Todo correcto.
3. **DontDestroyOnLoad conflict** — `VN_ChoiceGateController` y `InteractionSystem` son singleton `DontDestroyOnLoad`. Si MainMenu los tiene y Level_01 tambien los instancia con `Instance != null` → se destruye el duplicate. Verificar que no hay `GameObject` extra con estos componentes en Level_01.
4. **`CorridorCentral` rotation** — Las posiciones locales de props que paso en §3.2 son **world**, no locales a `CorridorCentral` (que está rotado 0). Ojo si al final rotamos la sección.
5. **`narrativeIntroTitle` en blueprint**: "Nivel 1 — Desorientación" (con guion). Las intros del NARRATIVA_INTERNA usan "Desorientación". Coinciden ✓.

---

## 8. EJECUCIÓN ATÓMICA RECOMENDADA

**Paso A (yo hago):** Añadir entradas JSON (§2)
**Paso B (yo hago):** Crear los 8 GameObjects en Unity (§3) según `manage_gameobject create + components_to_add: ["InteractableObject"]` + `set_property` en `InteractableObject` para setear `commentKey/displayName/isLyraArtifact`
**Paso C (yo hago):** Re-run de `manage_components set_property` para setear los `commentKey` (necesario porque `InteractableObject` no los seteamos en `create`)
**Paso D (yo hago):** `refresh_unity` + `read_console` (errores de compilación)
**Paso E (yo hago):** `manage_scene save` para persistir `Level_01.unity`
**Paso F (yo hago):** Play Mode → validación §6 → feedback

**Decisiones que necesito de ti:**
- ¿Apruebas el plan?
- ¿Hago A → B → C → D → E en secuencia (10-15 min ~en totalización), o prefieres algo más granular?
- ¿Tocamos el puzzle redesign ahora o lo diferimos a ronda 2?
- ¿Aceptas `CorridorCentral` fusionado o lo separamos en A+B con builder?

---

**ESPERA TU OK ANTES DE TOCAR UNITY.**
