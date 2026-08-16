# NARRATIVE_INTERACTION_ARCHITECTURE.md
## Spec ID: SPEC-ARCH-NARR-001
## Version: 1.0 (Propuesta — Pendiente de Aprobacion)
## Authority: Level 3 (Especificacion Tecnica Declarativa)
## Autor: Lead Narrative Systems Designer / Unity Gameplay Architect

---

## 1. PROPOSITO

Define la arquitectura modular que conecta los 8 pilares del flujo narrativo de
*Echoes of You 2.0*:

```
3D Exploration -> Interactable Props -> Dialogue / Visual Novel
-> Player Choices -> Narrative Variables -> Level Changes
-> Ending Evaluation
```

Esta arquitectura **extiende** los sistemas existentes (VN_EndingFlags,
VN_EndingResolver, VN_ChoiceGateController, AidenStageResolver,
VN_StageThresholds, VN_TextTable, InteractionSystem, InteractableObject)
**sin reemplazarlos**. Cada sistema existente se audita, se identifica su
estado real, y se determina compatibilidad antes de cualquier modificacion.

---

## 2. AUDITORIA DE SISTEMAS EXISTENTES

### 2.1 Sistema de Interaccion Actual

| Archivo | Estado | Hallazgos |
|---|---|---|
| `InteractableObject.cs` | Funcional, incompleto | Tiene `commentKey`, `isLyraArtifact`, `requireEchoActive`, `cooldown=3s`, `triggerRadius=2.5m`. Auto-crea SphereCollider trigger. **Falta**: INTERACTABLE_ID, LEVEL_ID, INTERACTION_TYPE, DIALOGUE_ID, MEMORY_EFFECT, CHOICE_EFFECT, ONE_TIME_ONLY, VISUAL_STATE_AFTER_INTERACTION. |
| `InteractionSystem.cs` | Funcional, con defectos | Singleton DontDestroyOnLoad. Usa SphereCast desde camara + fallback proximidad. `Register()`/`Unregister()` estan **vacios** (no hacen nada). Mezcla dos canales UI: a veces VN_DialogueController, a veces GameHUD.ShowInspection. **Incompatible con** RULE-INSPECT-009 (solo Chalkboard). |
| `InteractionPromptController.cs` | Funcional | Singleton. Prompt contextual con fade USS. Pero InteractionSystem **no lo usa para proximidad** — lo llama cada frame si hay target. |
| `NarrativeProp.cs` | Funcional, desconectado | Coloca point-light diegetico amber. **No tiene** InteractableObject adjunto por defecto. Esta desconectado del sistema de interaccion. |

**Veredicto**: El sistema de interaccion es **parcialmente compatible**. La
logica de deteccion (raycast + proximidad) funciona, pero los metadatos
estructurados faltan. `Register()`/`Unregister()` vacios significan que el
sistema de trigger enter/exit es dead code. Hay que **extender**
InteractableObject con un ScriptableObject de datos y arreglar el canal UI.

### 2.2 Input

| Archivo | Estado | Hallazgos |
|---|---|---|
| `InputActionMap.cs` | Funcional | Wrapper unificado sobre UnityEngine.Input legacy (NO Input System package). Expone Navigate, Submit, Cancel, Pause, Record (R), Playback (E), SoftReset. Singleton DontDestroyOnLoad. |
| `InteractionSystem.cs` | Inconsistente | Usa `Input.GetKeyDown(KeyCode.E)` directamente, no InputActionMap. |
| `VN_ChoiceGateController.cs` | Inconsistente | Usa `Input.GetKeyDown(KeyCode.A/D)` directamente. |

**Veredicto**: InputActionMap existe y es robusto, pero los sistemas de
narrativa/interaccion **no lo usan**. Hay que migrar InteractionSystem y
VN_ChoiceGateController a InputActionMap para consistencia. **No es
incompatible** — es una deuda tecnica.

### 2.3 UI

| Archivo | Estado | Hallazgos |
|---|---|---|
| `GameHUD.cs` | Funcional | UI Toolkit. Tiene Chalkboard subsystem (`ShowChalkboard`/`ShowInspection` con auto-dismiss 2.5s), Toast, Objective, Key Prompt, Record panel, Echo slots. |
| `VN_DialogueController.cs` | Funcional, superpuesto | Overlay VN con typewriter, sprites (left/center/right), voice audio. Singleton DontDestroyOnLoad. **Compite** con Chalkboard para pop-ups de inspeccion. |
| `VN_ChoiceGateController.cs` | Funcional | Overlay full-screen con 2 botones cyan/amber. Singleton DontDestroyOnLoad. Usa UIDocument separado. |
| `InteractionPromptController.cs` | Funcional | Prompt contextual con fade. Singleton. |

**Veredicto**: La UI es **compatible**. El problema es que InteractionSystem
mezcla VN_DialogueController (overlay narrativo con sprites) con
GameHUD.Chalkboard (pop-up simple). El spec canon (INSPECT_POPUP_SPEC
RULE-INSPECT-009) dice que los pop-ups de inspeccion van **solo** en
Chalkboard. VN_DialogueController debe reservarse para secuencias
narrativas multi-linea (dialogue sequences), no para inspeccion simple.

### 2.4 Game State

| Archivo | Estado | Hallazgos |
|---|---|---|
| `LevelRuntimeController.cs` | Funcional | Singleton por escena. Soft reset (Q), hard reset (T), death, completion. Hook a VN_ChoiceGateController en OnLevelCompleted. |
| `GameStateController.cs` | Referenciado | Maneja notificaciones de death/completion y restart de escena. |
| `GameProgress.cs` | Funcional, incompleto | Static class con PlayerPrefs. Guarda: unlocked count, completed scenes, deaths, play time, echoes, sessions. **NO guarda flags narrativos**. |

**Veredicto**: El game state es **compatible**. LevelRuntimeController ya
tiene el hook correcto al VN gate. Falta un estado narrativo central
persistente.

### 2.5 Save System

| Archivo | Estado | Hallazgos |
|---|---|---|
| `GameProgress.cs` | Funcional | PlayerPrefs-based. No guarda flags narrativos. |
| `SAVE_DATA_SCHEMA.md` | Spec existe | Define JSON schema para save_v1.json con echo frames. Pero **no incluye** flags narrativos en el schema. |
| `VN_EndingFlags.cs` | No persistente | MonoBehaviour DontDestroyOnLoad. Dictionary<string,bool> flags en memoria. **Se pierde al cerrar el juego**. |

**Veredicto**: El save system es **incompatible con la narrativa**. Los
flags narrativos (VN_EndingFlags) no se persisten a disco. Si el jugador
cierra el juego, pierde todo el progreso narrativo. Esto es **critico**
para el sistema de endings. Hay que anadir persistencia de flags al
save system sin romper el schema existente.

### 2.6 Echo System

| Archivo | Estado | Hallazgos |
|---|---|---|
| `EchoRecorder.cs` | Funcional | Singleton. IsRecording, maxRecordSeconds, ClearAllEchoes, EchoCreated event. |
| `EchoPlayback` | Referenciado | IsPlaying. |
| `EchoModeController` | Referenciado | Configura modos de eco. |

**Veredicto**: El echo system es **totalmente compatible**. No necesita
modificaciones para la narrativa. La interaccion narrativa puede consultar
`EchoRecorder.IsRecording` y `EchoPlayback.IsPlaying` via el flag
`requireEchoActive` existente.

### 2.7 Puzzle System

| Archivo | Estado | Hallazgos |
|---|---|---|
| `PressurePlate.cs`, `PuzzleWire.cs`, etc. | Funcional | Sistema de placas, senales, intenciones, condiciones. |
| `LevelGoal.cs` | Funcional | Define el objetivo del nivel. IsReady desbloquea LevelExit. |
| `LevelExit.cs` | Funcional | OnTriggerEnter carga siguiente escena. Verifica VN gate activo. |

**Veredicto**: El puzzle system es **totalmente compatible**. LevelExit ya
tiene guard contra VN_ChoiceGateController. No necesita modificaciones.

### 2.8 Level Flow

| Archivo | Estado | Hallazgos |
|---|---|---|
| `SceneTransitionManager.cs` | Funcional | Singleton DontDestroyOnLoad. Fade (actualmente deshabilitado). LoadScene con validacion. |
| `LevelRuntimeController.OnLevelCompleted()` | Funcional | Hook a VN_ChoiceGateController.Show(levelIdx, isMicro, callback). Callback carga siguiente escena. |
| `LevelExit.LoadNext()` | Funcional | Verifica VN gate activo antes de cargar. |
| `VN_EpilogueController.cs` | Funcional | Carga escena de epilogo additive en CreditsScene. |

**Veredicto**: El level flow es **compatible**. Hay una potencial race
condition entre LevelExit.LoadNext y LevelRuntimeController.OnLevelCompleted
(ambos pueden invocar el gate), pero LevelExit ya tiene guard. El flujo
MainMenu -> Level_01 -> ... -> Level_15 -> CreditsScene -> Epilogue_* funciona.

---

## 3. PRINCIPIOS DE DISENO

1. **Datos separados de logica**: Todo dato narrativo vive en ScriptableObjects
   o JSON (Resources). La logica solo lee/escribe via APIs.
2. **No reemplazar sistemas funcionales**: Extender, no reescribir.
3. **Respetar el canon**: BIB-TXT-003, BIB-AIN-006, ANTI-BIB-004/005,
   RULE-PHI-005, RULE-INSPECT-001 a 010.
4. **Props como partes del mundo**: Los props NO son botones flotantes. La
   interaccion emerge de la proximidad + prompt contextual diegetico.
5. **Un solo canal de inspeccion**: Chalkboard HUD (RULE-INSPECT-009).
   VN_DialogueController se reserva para secuencias narrativas multi-linea.
6. **Persistencia narrativa**: Los flags y variables narrativas se guardan
   a disco junto con el progreso de nivel.

---

## 4. ARQUITECTURA DE COMPONENTES

### 4.1 InteractableObject (Extendido)

El componente MonoBehaviour existente se extiende con una referencia a un
**InteractableData** ScriptableObject que contiene todos los metadatos.
El componente sigue siendo el punto de entrada fisico (collider, posicion),
pero la definicion narrativa vive en el asset.

```
InteractableObject (MonoBehaviour)  --referencia-->  InteractableData (ScriptableObject)
  |                                                    |
  | Collider, triggerRadius, posicion                   | INTERACTABLE_ID, LEVEL_ID,
  | Cooldown, requireEchoActive                         | INTERACTION_TYPE, PROMPT_TEXT,
  | isLyraArtifact (legacy, migrado a data)             | DIALOGUE_ID, MEMORY_EFFECT,
  |                                                     | CHOICE_EFFECT, ONE_TIME_ONLY,
  |                                                     | VISUAL_STATE_AFTER_INTERACTION
```

**InteractableData (ScriptableObject) — Campos requeridos:**

| Campo | Tipo | Descripcion |
|---|---|---|
| `interactableId` | string | ID unico (ej: "N05_locker_lyra") |
| `levelId` | string | ID del nivel (ej: "Level_05") |
| `interactionType` | enum | `Inspect` \| `Dialogue` \| `Choice` \| `Memory` |
| `promptText` | string | Texto del prompt contextual (ej: "Examinar") |
| `dialogueId` | string | ID de DialogueSequence (si type=Dialogue o Choice) |
| `commentKey` | string | Key de VN_TextTable (si type=Inspect, legacy compat) |
| `memoryEffect` | MemoryEffect | Que memoria/flag se activa al interactuar |
| `choiceEffect` | ChoiceEffect | Que flags/variables se modifican |
| `oneTimeOnly` | bool | Si true, solo se puede interactuar una vez por sesion |
| `visualStateAfter` | VisualStateChange | Cambio visual post-interaccion (material, emission, visibility) |
| `isLyraArtifact` | bool | Migrado de InteractableObject (legacy compat) |
| `requireEchoActive` | bool | Migrado de InteractableObject |
| `cooldown` | float | Migrado de InteractableObject (default 3.0s) |
| `triggerRadius` | float | Migrado de InteractableObject (default 2.5m) |

**Compatibilidad**: InteractableObject mantiene sus campos serializados
existentes para no romper escenas ya configuradas. Si `interactableData`
esta asignado, los campos del SO prevalecen. Si no, usa los campos legacy.

### 4.2 InteractionPrompt

El prompt contextual ya existe (`InteractionPromptController`). Se
**conecta** al sistema de proximidad real (trigger enter/exit) en lugar
del polling por raycast cada frame.

```
InteractableObject.OnTriggerEnter(player)
  -> InteractionSystem.Register(this)
  -> InteractionSystem evalua nearest
  -> InteractionPromptController.ShowPrompt(key, action, primary)
  
InteractableObject.OnTriggerExit(player)
  -> InteractionSystem.Unregister(this)
  -> si no hay mas targets: InteractionPromptController.HidePrompt()
```

**Modificacion**: `InteractionSystem.Register()` y `Unregister()` dejan
de estar vacios. Mantiene una lista ordenada por distancia. Muestra
prompt del nearest. Input E dispara la interaccion del nearest.

### 4.3 DialogueTrigger

Componente que **orquesta** la transicion entre gameplay y narrativa.
No es un collider mas — es el coordinador de estado que:

1. Detecta la interaccion (via InteractableObject o trigger autonomo).
2. Pide al `NarrativeStateController` que bloquee el movimiento del player.
3. Abre la interfaz narrativa correcta segun `interactionType`:
   - `Inspect` -> GameHUD.ShowInspection (Chalkboard, 2.5s auto-dismiss)
   - `Dialogue` -> VN_DialogueController.PlaySequence (typewriter, sprites)
   - `Choice` -> VN_ChoiceGateController.Show (overlay cyan/amber)
   - `Memory` -> MemorySystem.RegisterMemory + Chalkboard
4. Al cerrar, restaura el gameplay (desbloquea movimiento, reactiva HUD).

```
DialogueTrigger (MonoBehaviour)
  |
  +-> OnInteract()
       |
       +-> NarrativeStateController.EnterNarrativeMode(interactionType)
       |     -> PlayerController.SetMovementEnabled(false)  [si Dialogue/Choice]
       |     -> GameHUD.SetVisible(false)                    [si Dialogue/Choice]
       |     -> InteractionPromptController.HidePrompt()
       |
       +-> Segun interactionType:
       |     Inspect  -> GameHUD.ShowInspection(title, text)
       |     Dialogue -> VN_DialogueController.PlaySequence(lines)
       |     Choice   -> VN_ChoiceGateController.Show(levelIdx, isMicro, onComplete)
       |     Memory   -> MemorySystem.Register(data.memoryEffect)
       |                 + GameHUD.ShowInspection(title, text)
       |
       +-> onComplete / onClose:
             NarrativeStateController.ExitNarrativeMode()
               -> PlayerController.SetMovementEnabled(true)
               -> GameHUD.SetVisible(true)
               -> Aplicar VisualStateChange si definido
```

**Bloqueo de movimiento**: Solo se bloquea para `Dialogue` y `Choice`
(escenas narrativas full-screen). `Inspect` y `Memory` no bloquean
movimiento (pop-up diegetico, el player puede seguir caminando —
auto-dismiss a 2.5s).

### 4.4 DialogueSequence

ScriptableObject que define una **secuencia de lineas de dialogo** para
una escena narrativa (type=Dialogue). No es lo mismo que un pop-up de
inspeccion — es una secuencia multi-linea con sprites, voz y avance
manual.

```
DialogueSequence (ScriptableObject)
  |
  +-> dialogueId: string           (ej: "N10_lyra_classroom_scene")
  +-> levelId: string             (ej: "Level_10")
  +-> triggerCondition: string    (flag condition, ej: "touch_lyra_object == false")
  +-> lines: DialogueNode[]
  +-> onCompleteAction: NarrativeAction[]  (flags/variables a modificar)
  +-> oneTimeOnly: bool
```

**Nota**: DialogueSequence usa VN_DialogueController existente para
reproducir las lineas (typewriter, sprites, voice). No reemplaza el
controller — lo alimenta con datos estructurados.

### 4.5 DialogueNode

Define una **linea individual** dentro de una DialogueSequence.

```
DialogueNode (Serializable class)
  |
  +-> speakerId: string           ("aiden" | "" para narrador soft en epilogo)
  +-> textKey: string             (key de VN_TextTable o texto directo)
  +-> spritePath: string          (Resources path, ej: "VN/Sprites/aiden/Aiden_Pensativa")
  +-> spritePosition: enum        (None | Left | Center | Right)
  +-> voiceClipPath: string       (Resources path, opcional)
  +-> autoAdvance: bool            (si true, avanza solo tras advanceDelay)
  +-> advanceDelay: float         (segundos, si autoAdvance)
  +-> conditions: string[]        (flags requeridos para mostrar esta linea)
  +-> choices: DialogueChoice[]   (si la linea presenta elecciones)
```

**Compatibilidad**: DialogueNode se mapea directamente a
`VN_DialogueController.DialogueLine` existente. Es la capa de datos
que el controller consume.

### 4.6 DialogueChoice

Define una **eleccion** dentro de un DialogueNode. Esta es la unidad
atomica de decision narrativa.

```
DialogueChoice (Serializable class)
  |
  +-> choiceId: string            (CHOICE_ID, ej: "ch10_touch_lyra")
  +-> displayText: string         (DISPLAY_TEXT, ej: "Tocar el recuerdo")
  +-> color: enum                 (Cyan | Amber — lenguaje cromatico canon)
  +-> conditions: string[]        (CONDITIONS — flags requeridos para mostrar)
  +-> effects: NarrativeAction[]  (EFFECTS — acciones al elegir)
  +-> flagsAdded: string[]        (FLAGS_ADDED — flags que se setean a true)
  +-> flagsRemoved: string[]      (FLAGS_REMOVED — flags que se setean a false)
  +-> variableChanges: VariableChange[]  (VARIABLE_CHANGES — variables a modificar)
  +-> nextNode: string            (NEXT_NODE — ID del siguiente DialogueNode, o "" para terminar)
  +-> comprehensionDelta: int     (+1 si "abrir", +0 si "mantener" — feedea Catch-22)
```

**VariableChange (Serializable class):**
```
VariableChange
  +-> variableName: string        (ej: "comprehension_score")
  +-> operation: enum             (Set | Add | Subtract | Multiply)
  +-> value: float
```

**NarrativeAction (Serializable class):**
```
NarrativeAction
  +-> type: enum                  (SetFlag | ClearFlag | SetVariable | LoadScene | ShowMemory | PlaySound | SetVisualState)
  +-> target: string              (flag name, variable name, scene name, etc.)
  +-> value: string               (valor, si aplica)
```

### 4.7 NarrativeVariable

Sistema de **variables continuas** narrativas. Extiende VN_EndingFlags
que solo maneja flags binarios. Las variables permiten valores
enteros/flotantes para cosas como:

- `comprehension_score` (ya existe, migrar aqui)
- `lyra_artifact_seen_count` (ya existe, migrar aqui)
- `objects_inspected_count` (nuevo)
- `echoes_created_total` (nuevo, feedea endings)
- `levels_completed_count` (nuevo)

```
NarrativeVariableStore (ScriptableObject)
  |
  +-> variables: List<VariableDef>
  
VariableDef
  +-> name: string
  +-> type: enum (Int | Float | String | Bool)
  +-> defaultValue: string (serializado como string para flexibilidad)
  +-> description: string
```

**Runtime**: `NarrativeStateController` mantiene un diccionario
`Dictionary<string, float>` para variables numericas y delega a
`VN_EndingFlags` para flags binarios (compatibilidad).

### 4.8 NarrativeFlag

Sistema de **flags binarios** narrativos. Ya existe como
`VN_EndingFlags` (Dictionary<string,bool>). La arquitectura **no lo
reemplaza** — lo extiende con:

1. **Persistencia**: Guardar/cargar flags a disco (JSON).
2. **Catalogo**: ScriptableObject `NarrativeFlagCatalog` que define todos
   los flags validos, su tipo (openness/pattern/exit), y su descripcion.
3. **Validacion**: Verificar que solo se seteen flags del catalogo.

```
NarrativeFlagCatalog (ScriptableObject)
  |
  +-> flags: List<FlagDef>
  
FlagDef
  +-> flagName: string           (ej: "allow_to_see")
  +-> category: enum              (Openness | PatternHolding | Exit | Memory | Custom)
  +-> comprehensionDelta: int      (+1 openness, +0 pattern)
  +-> description: string
```

**Compatibilidad**: VN_EndingFlags sigue siendo el runtime store.
NarrativeFlagCatalog es el asset de definicion. La persistencia se
anade como capa nueva (NarrativeSaveBridge).

### 4.9 MemorySystem

Sistema que rastrea **que objetos/memorias ha inspeccionado el jugador**
y los efectos narrativos de cada inspeccion. Esto ya existe parcialmente
en VN_EndingFlags (`BumpLyraArtifactSeen`), pero sin granularidad por
objeto.

```
MemorySystem (MonoBehaviour, Singleton)
  |
  +-> RegisterMemory(memoryEffect: MemoryEffect)
  |     -> Marca el objeto como inspeccionado en el save
  |     -> Aplica flags/variables del memoryEffect
  |     -> Si es Lyra artifact: BumpLyraArtifactSeen() + comprehension
  |     -> Si oneTimeOnly: marca como visto (no repetible)
  |
  +-> HasBeenInspected(interactableId: string): bool
  |
  +-> GetInspectedCount(): int
  |
  +-> GetLyraArtifactsSeen(): int  (delegado a VN_EndingFlags)
  |
  +-> SaveToDisk() / LoadFromDisk()
  
MemoryEffect (Serializable class)
  +-> memoryId: string            (ej: "MEM-001_COAT_SEEN")
  +-> flagsAdded: string[]
  +-> variableChanges: VariableChange[]
  +-> comprehensionDelta: int
  +-> isLyraArtifact: bool
```

**Compatibilidad**: MemorySystem delega `BumpLyraArtifactSeen()` a
VN_EndingFlags existente. No reemplaza la logica de comprehension.

### 4.10 EndingEvaluator

Extiende `VN_EndingResolver` existente. El resolver actual lee flags
binarios y comprehension_score. EndingEvaluator anade:

1. **Evaluacion por variables continuas** (no solo flags).
2. **Evaluacion progresiva** (puede evaluar en cualquier punto, no
   solo al final de N15).
3. **Debug/preview** en editor para validar paths.

```
EndingEvaluator (static class)
  |
  +-> Evaluate(flags, variables, lyraArtifactsSeen) -> EndingID
  |     -> Delega a VN_EndingResolver.Resolve() para el algoritmo canonico
  |     -> Anade validacion de variables continuas (advisory, no override)
  |
  +-> EvaluateFromRuntime() -> EndingID
  |     -> Lee VN_EndingFlags + NarrativeStateController
  |
  +-> GetEndingPreview() -> EndingPreview
  |     -> Devuelve el ending actual + flags faltantes para cada ending
  |     -> Util para debug y para el diseno de niveles
  |
  +-> GetProgressTowards(endingId) -> float (0..1)
       -> Progreso hacia un ending especifico
```

**Compatibilidad**: EndingEvaluator **delega** a VN_EndingResolver
para el algoritmo canonico de 6 pasos. No lo reemplaza. Anade capa
de variables y preview.

---

## 5. DIAGRAMA DE FLUJO

```mermaid
graph TD
    subgraph EXPLORATION [3D Exploration]
        A[Player camina por el nivel]
        B[InteractableObject collider detecta proximidad]
    end

    subgraph PROMPT [Interaction Prompt]
        B --> C[InteractionSystem.Register]
        C --> D[InteractionPromptController.ShowPrompt]
        D --> E{Player presiona E?}
        E -->|No| A
        E -->|Si| F[DialogueTrigger.OnInteract]
    end

    subgraph NARRATIVE [Narrative Interface]
        F --> G{interactionType?}
        G -->|Inspect| H[GameHUD.ShowInspection<br/>Chalkboard 2.5s auto-dismiss<br/>NO bloquea movimiento]
        G -->|Memory| I[MemorySystem.RegisterMemory<br/>+ GameHUD.ShowInspection<br/>NO bloquea movimiento]
        G -->|Dialogue| J[NarrativeStateController.EnterNarrativeMode<br/>Bloquea movimiento + oculta HUD]
        G -->|Choice| J
        J --> K[VN_DialogueController.PlaySequence<br/>o VN_ChoiceGateController.Show]
    end

    subgraph DECISION [Player Choices]
        K --> L[Player elige / avanza]
        L --> M[DialogueChoice procesado]
        M --> N[FLAGS_ADDED -> VN_EndingFlags.SetFlag]
        M --> O[FLAGS_REMOVED -> VN_EndingFlags.SetFlag false]
        M --> P[VARIABLE_CHANGES -> NarrativeStateController]
        M --> Q[NEXT_NODE -> siguiente DialogueNode o cierre]
    end

    subgraph STATE [Narrative Variables & Flags]
        N --> R[VN_EndingFlags actualizado]
        O --> R
        P --> S[NarrativeStateController actualizado]
        R --> T[comprehension_score recalculado]
        S --> T
        T --> U[AidenStageResolver reevalua etapa]
        U --> V[Tono de pop-ups actualizado<br/>Catch-22 clamp]
    end

    subgraph WORLD [World State Update]
        I --> W[VisualStateChange aplicado<br/>material/emission/visibility]
        M --> W
        W --> X[MemorySystem marca como inspeccionado<br/>oneTimeOnly respeta]
    end

    subgraph RESTORE [Gameplay Restore]
        H --> A
        I --> A
        Q --> Y[NarrativeStateController.ExitNarrativeMode]
        Y --> Z[PlayerController.SetMovementEnabled true]
        Y --> AA[GameHUD.SetVisible true]
        AA --> A
    end

    subgraph LEVEL [Level Changes]
        AB[LevelGoal.IsReady] --> AC[LevelExit unlocked]
        AC --> AD[LevelExit trigger]
        AD --> AE[LevelRuntimeController.OnLevelCompleted]
        AE --> AF{VN_ChoiceGateController<br/>tiene nodo para este nivel?}
        AF -->|Si| AG[Show choice gate<br/>cyan/amber overlay]
        AF -->|No| AH[LoadNext scene]
        AG --> AI[Player elige A/D]
        AI --> AJ[VN_EndingFlags.SetFlag<br/>comprehension bumped]
        AJ --> AH
    end

    subgraph ENDING [Ending Evaluation]
        AH --> AK{Es N15 post-decision?}
        AK -->|No| A
        AK -->|Si| AL[Load CreditsScene]
        AL --> AM[VN_EpilogueController.Start]
        AM --> AN[EndingEvaluator.EvaluateFromRuntime]
        AN --> AO[EndingID resuelto]
        AO --> AP[LoadSceneAdditive<br/>Epilogue_{ending}]
        AP --> AQ[EpilogueController muestra<br/>voz final + salir_del_colegio]
    end

    subgraph SAVE [Persistence]
        R --> AR[NarrativeSaveBridge.Save]
        S --> AR
        X --> AR
        AR --> AS[save_narrative.json<br/>escrito a persistentDataPath]
        AS --> AT[GameProgress.SavePlayTime<br/>PlayerPrefs]
    end
```

---

## 6. DIAGRAMA DE CAPAS

```
┌─────────────────────────────────────────────────────────────┐
│                    CAPA DE DATOS (ScriptableObjects)         │
│                                                             │
│  InteractableData    DialogueSequence    NarrativeFlagCatalog│
│  (SO por prop)       (SO por escena)     (SO catalogo)       │
│                                                             │
│  DialogueNode        DialogueChoice      NarrativeVariableStore│
│  (serializable)      (serializable)     (SO catalogo)       │
│                                                             │
│  MemoryEffect        VisualStateChange   VN_ChoiceRegistry   │
│  (serializable)      (serializable)     (SO existente)       │
└─────────────────────────────────────────────────────────────┘
                              │ lee/escribe
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  CAPA DE LOGICA (MonoBehaviours)             │
│                                                             │
│  InteractableObject   InteractionSystem   InteractionPrompt  │
│  (existente, extendido)(existente, fix)   (existente)        │
│                                                             │
│  DialogueTrigger      NarrativeStateController  MemorySystem │
│  (NUEVO)              (NUEVO)                  (NUEVO)       │
│                                                             │
│  VN_EndingFlags       VN_EndingResolver    EndingEvaluator  │
│  (existente)          (existente)          (NUEVO, delega)  │
│                                                             │
│  AidenStageResolver   VN_StageThresholds   VN_TextTable     │
│  (existente)          (existente)          (existente)      │
│                                                             │
│  NarrativeSaveBridge                                       │
│  (NUEVO)                                                  │
└─────────────────────────────────────────────────────────────┘
                              │ usa
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  CAPA DE UI (UI Toolkit)                     │
│                                                             │
│  GameHUD.Chalkboard   VN_DialogueController  VN_ChoiceGate   │
│  (existente)          (existente)           (existente)     │
│                                                             │
│  InteractionPromptController                               │
│  (existente)                                               │
└─────────────────────────────────────────────────────────────┘
                              │ integra con
                              ▼
┌─────────────────────────────────────────────────────────────┐
│              CAPA DE GAMEPLAY (Sistemas Existentes)           │
│                                                             │
│  PlayerController    EchoRecorder    EchoPlayback            │
│  LevelRuntimeController    LevelExit    LevelGoal            │
│  SceneTransitionManager    GameProgress    GameStateController│
└─────────────────────────────────────────────────────────────┘
```

---

## 7. ARCHIVOS A CREAR

| # | Archivo | Tipo | Responsabilidad |
|---|---|---|---|
| 1 | `Assets/Scripts/Narrative/Data/InteractableData.cs` | ScriptableObject | Definicion de datos de props interactuables (INTERACTABLE_ID, LEVEL_ID, INTERACTION_TYPE, etc.) |
| 2 | `Assets/Scripts/Narrative/Data/DialogueSequence.cs` | ScriptableObject | Secuencia de lineas de dialogo para escenas narrativas |
| 3 | `Assets/Scripts/Narrative/Data/DialogueNode.cs` | Serializable class | Linea individual de dialogo (speaker, text, sprite, choices) |
| 4 | `Assets/Scripts/Narrative/Data/DialogueChoice.cs` | Serializable class | Eleccion dentro de un DialogueNode (CHOICE_ID, DISPLAY_TEXT, CONDITIONS, EFFECTS, FLAGS_ADDED, FLAGS_REMOVED, VARIABLE_CHANGES, NEXT_NODE) |
| 5 | `Assets/Scripts/Narrative/Data/NarrativeAction.cs` | Serializable class | Accion narrativa atomica (SetFlag, SetVariable, LoadScene, etc.) |
| 6 | `Assets/Scripts/Narrative/Data/VariableChange.cs` | Serializable class | Cambio de variable continua (name, operation, value) |
| 7 | `Assets/Scripts/Narrative/Data/MemoryEffect.cs` | Serializable class | Efecto de memoria al inspeccionar (memoryId, flags, variables, comprehension) |
| 8 | `Assets/Scripts/Narrative/Data/VisualStateChange.cs` | Serializable class | Cambio visual post-interaccion (material, emission, visibility) |
| 9 | `Assets/Scripts/Narrative/Data/InteractionType.cs` | Enum | Inspect, Dialogue, Choice, Memory |
| 10 | `Assets/Scripts/Narrative/Data/NarrativeFlagCatalog.cs` | ScriptableObject | Catalogo de flags validos (flagName, category, comprehensionDelta) |
| 11 | `Assets/Scripts/Narrative/Data/NarrativeVariableStore.cs` | ScriptableObject | Catalogo de variables continuas (name, type, default) |
| 12 | `Assets/Scripts/Narrative/DialogueTrigger.cs` | MonoBehaviour | Orquesta la transicion gameplay -> narrativa y restaura |
| 13 | `Assets/Scripts/Narrative/NarrativeStateController.cs` | MonoBehaviour, Singleton | Estado narrativo central: variables, bloqueo de movimiento, modo narrativo |
| 14 | `Assets/Scripts/Narrative/MemorySystem.cs` | MonoBehaviour, Singleton | Rastrea inspecciones, aplica efectos de memoria, persiste |
| 15 | `Assets/Scripts/Narrative/EndingEvaluator.cs` | Static class | Extiende VN_EndingResolver con variables continuas y preview |
| 16 | `Assets/Scripts/Narrative/NarrativeSaveBridge.cs` | Static class | Puente entre VN_EndingFlags/NarrativeStateController y disco (JSON) |
| 17 | `Assets/Scripts/Narrative/NarrativeConditionEvaluator.cs` | Static class | Evalua condiciones string (flag == true, var >= 5) para DialogueNode/Choice |

**Total**: 17 archivos nuevos.

---

## 8. ARCHIVOS A MODIFICAR

| # | Archivo | Modificacion | Riesgo |
|---|---|---|---|
| 1 | `Assets/Scripts/Interaction/InteractableObject.cs` | Anadir referencia `InteractableData data`. Si data != null, prevalece sobre campos legacy. Anadir `OnTriggerEnter/Exit` que llamen `Register/Unregister` reales. Migrar `isLyraArtifact` a leer de data si existe. | **Bajo** — campos legacy se conservan, data es opcional. |
| 2 | `Assets/Scripts/Interaction/InteractionSystem.cs` | Implementar `Register()`/`Unregister()` reales (lista ordenada por distancia). Mostrar prompt del nearest. Usar `InputActionMap.PlaybackPressed` en vez de `Input.GetKeyDown(E)`. Delegar a `DialogueTrigger` si el InteractableObject tiene uno. Eliminar el uso de VN_DialogueController para inspeccion simple (RULE-INSPECT-009). | **Medio** — cambia el canal UI de inspeccion. Puede romper escenas que dependen del comportamiento actual. |
| 3 | `Assets/Scripts/UI/InteractionPromptController.cs` | Anadir metodo `ShowPromptFor(InteractableObject obj)` que lee el promptText de InteractableData. | **Bajo** — metodo nuevo, no rompe existentes. |
| 4 | `Assets/Scripts/VN/VN_EndingFlags.cs` | Anadir `LoadFromDisk()`/`SaveToDisk()` via NarrativeSaveBridge. Anadir `GetVariables()` para NarrativeStateController. | **Bajo** — metodos nuevos, no cambia comportamiento existente. |
| 5 | `Assets/Scripts/LevelRuntimeController.cs` | En `OnLevelCompleted()`, verificar si el InteractableObject que disparo el exit tiene un DialogueTrigger con tipo Choice antes de invocar el gate global. | **Medio** — logica de branching, puede afectar el flow de niveles. |
| 6 | `Assets/Scripts/GameProgress.cs` | Anadir `SaveNarrativeState()`/`LoadNarrativeState()` que delega a NarrativeSaveBridge. | **Bajo** — metodos nuevos. |

**Total**: 6 archivos modificados.

---

## 9. ESQUEMA DE DATOS — EJEMPLO CONCRETO

### 9.1 InteractableData (ScriptableObject asset)

```csharp
[CreateAssetMenu(fileName = "InteractableData", menuName = "Echoes/Narrative/InteractableData")]
public class InteractableData : ScriptableObject
{
    [Header("Identity")]
    public string interactableId = "N05_locker_lyra";
    public string levelId = "Level_05";
    public InteractionType interactionType = InteractionType.Memory;

    [Header("Prompt")]
    public string promptText = "Examinar";

    [Header("Dialogue / Inspect")]
    public string commentKey = "interaction.locker_lyra";  // VN_TextTable key
    public string dialogueId = "";  // si type=Dialogue o Choice

    [Header("Effects")]
    public MemoryEffect memoryEffect;
    public DialogueChoice choiceEffect;  // si type=Choice inline
    public bool oneTimeOnly = true;
    public VisualStateChange visualStateAfter;

    [Header("Legacy Compat (migrado a data)")]
    public bool isLyraArtifact = true;
    public bool requireEchoActive = false;
    public float cooldown = 3.0f;
    public float triggerRadius = 2.5f;
}
```

### 9.2 DialogueChoice (Serializable)

```csharp
[System.Serializable]
public class DialogueChoice
{
    [Header("Identity")]
    public string choiceId = "ch5_touched_locker";

    [Header("Display")]
    public string displayText = "Tocar la taquilla";
    public ChoiceColor color = ChoiceColor.Cyan;

    [Header("Conditions")]
    public string[] conditions = { "level_index == 5" };

    [Header("Effects")]
    public NarrativeAction[] effects;
    public string[] flagsAdded = { "touched_locker" };
    public string[] flagsRemoved = { };
    public VariableChange[] variableChanges = { };

    [Header("Flow")]
    public string nextNode = "";  // "" = cerrar
    public int comprehensionDelta = 1;  // +1 openness, +0 pattern
}
```

### 9.3 Prop Interactable — Esquema completo

```
INTERACTABLE_ID:    N05_locker_lyra
LEVEL_ID:          Level_05
POSITION:          [4.0, 0.0, 4.0]  (de ENVIRONMENT_STORYTELLING Table 8.2)
INTERACTION_TYPE:  Memory
PROMPT_TEXT:       "Examinar"
DIALOGUE_ID:       ""  (usa commentKey para Chalkboard)
MEMORY_EFFECT:
  memoryId:        MEM-005_LOCKER_LYRA_SEEN
  flagsAdded:      ["touched_locker"]
  comprehensionDelta: 1
  isLyraArtifact:  true
CHOICE_EFFECT:     null  (no es un choice inline)
ONE_TIME_ONLY:    true
VISUAL_STATE_AFTER_INTERACTION:
  emissionIntensity: 0.3  (atenúa el amber tras tocar — ya no duele tanto)
  materialColor:    #FFBF00 -> #D4A030  (amber mas tenue)
```

---

## 10. FLUJO DE INTERACCION DETALLADO (8 pasos)

### Paso 1: Detectar proximidad
```
Player camina hacia el prop.
InteractableObject.OnTriggerEnter(player collider)
  -> InteractionSystem.Register(this)
  -> InteractionSystem recalcula nearest
```

### Paso 2: Mostrar prompt contextual
```
InteractionSystem detecta que este prop es el nearest
  -> InteractionPromptController.ShowPrompt("[E]", "Examinar", primary: isLyraArtifact)
  -> Prompt aparece con fade (USS .prompt-hidden removido)
```

### Paso 3: Permitir interaccion
```
Player presiona E
  -> InputActionMap.PlaybackPressed == true
  -> InteractionSystem verifica cooldown (3s) y requireEchoActive
  -> Si pasa: DialogueTrigger.OnInteract()
```

### Paso 4: Bloquear o adaptar movimiento
```
DialogueTrigger lee interactionType:
  Inspect / Memory -> NO bloquea movimiento (pop-up diegetico)
  Dialogue / Choice -> NarrativeStateController.EnterNarrativeMode()
    -> PlayerController.SetMovementEnabled(false)
    -> GameHUD.SetVisible(false)
    -> Cursor.lockState = None
    -> Cursor.visible = true
```

### Paso 5: Abrir la interfaz narrativa
```
Segun interactionType:
  Inspect -> GameHUD.ShowInspection(title, text)  [Chalkboard 2.5s]
  Memory  -> MemorySystem.RegisterMemory(memoryEffect)
             + GameHUD.ShowInspection(title, text)
  Dialogue -> VN_DialogueController.PlaySequence(dialogueSequence.lines)
  Choice   -> VN_ChoiceGateController.Show(levelIdx, isMicro, onComplete)
```

### Paso 6: Registrar decisiones
```
Si hay choices:
  Player elige (A=cyan, D=amber, o click)
  -> DialogueChoice procesado:
     -> flagsAdded -> VN_EndingFlags.SetFlag(flag, true)
     -> flagsRemoved -> VN_EndingFlags.SetFlag(flag, false)
     -> variableChanges -> NarrativeStateController.ApplyVariableChange()
     -> comprehensionDelta -> VN_EndingFlags.AddComprehension()
     -> effects -> NarrativeAction[] ejecutados
```

### Paso 7: Actualizar estado del mundo
```
MemorySystem marca el objeto como inspeccionado
  -> Si oneTimeOnly: InteractableObject.enabled = false (no repetible)
  -> VisualStateChange aplicado:
     -> Material emission atenuada
     -> O renderer desactivado
     -> O color cambiado
  -> NarrativeSaveBridge.Save() -> disco
```

### Paso 8: Restaurar gameplay
```
Si era Dialogue/Choice:
  -> NarrativeStateController.ExitNarrativeMode()
     -> PlayerController.SetMovementEnabled(true)
     -> GameHUD.SetVisible(true)
     -> Cursor.lockState = Locked
     -> Cursor.visible = false
  -> InteractionPromptController.HidePrompt() (si no hay mas targets)
  -> Player vuelve a explorar
```

---

## 11. PERSISTENCIA NARRATIVA

### 11.1 Formato de guardado narrativo

Archivo: `Application.persistentDataPath + "/save_narrative.json"`

```json
{
  "schemaVersion": 1,
  "flags": {
    "allow_to_see": true,
    "touched_locker": true,
    "salir_del_colegio": false
  },
  "variables": {
    "comprehension_score": 7,
    "lyra_artifact_seen_count": 3,
    "objects_inspected_count": 12
  },
  "inspectedInteractables": [
    "N01_coat_amber",
    "N02_notebook_amber",
    "N05_locker_lyra"
  ],
  "lastUpdated": "2026-08-13T21:30:00"
}
```

### 11.2 Bridge con GameProgress

`NarrativeSaveBridge` es una capa nueva que **no toca** el schema de
`save_v1.json` (echo frames). Vive en su propio archivo
(`save_narrative.json`). `GameProgress` anade dos metodos que delegan:

```csharp
public static void SaveNarrativeState() => NarrativeSaveBridge.Save();
public static void LoadNarrativeState() => NarrativeSaveBridge.Load();
```

**Momentos de guardado**:
- Al completar un nivel (LevelRuntimeController.OnLevelCompleted).
- Al inspeccionar un objeto oneTimeOnly (MemorySystem.RegisterMemory).
- Al cerrar la aplicacion (OnApplicationQuit).

---

## 12. COMPATIBILIDAD CON CANON

| Regla Canon | Como se cumple |
|---|---|
| BIB-TXT-003 (cero dialogo externo) | Inspect/Memory usan Chalkboard (voz interna). Dialogue usa VN_DialogueController solo para escenas narrativas designadas (no durante gameplay de puzzle). |
| BIB-AIN-006 (tono variable por etapa) | AidenStageResolver ya existe y se integra. DialogueTrigger lo consulta para resolver el tono. |
| ANTI-BIB-004 (ambiguedad relacional) | Los datos narrativos (InteractableData, DialogueSequence) se validan contra el catalogo de palabras prohibidas. |
| ANTI-BIB-005 (no autojustificacion) | DialogueChoice.effects no pueden setear flags conclusivos. NarrativeFlagCatalog valida. |
| RULE-PHI-005 (Catch-22) | AidenStageResolver ya aplica el clamp. No se modifica. |
| RULE-INSPECT-001 a 010 | Inspect usa Chalkboard (009), 42 chars (002), 1a persona (001), cooldown 3s (008), auto-dismiss 2.5s (007). |
| RULE-ENV-001 a 005 | InteractableData respeta las posiciones de ENVIRONMENT_STORYTELLING Table 8.2. |
| Frozen Decisions (SOURCE_OF_TRUTH) | No se modifica ningun valor congelado. URP, Echo 12s, PS1 aesthetic, single camera — todo se respeta. |

---

## 13. RIESGOS Y MITIGACIONES

| # | Riesgo | Severidad | Mitigacion |
|---|---|---|---|
| R1 | **Romper escenas existentes** al cambiar InteractionSystem (canal UI de inspeccion) | Alto | Los campos legacy de InteractableObject se conservan. Si `interactableData == null`, comportamiento identico al actual. Migracion gradual. |
| R2 | **Race condition** entre LevelExit.LoadNext y LevelRuntimeController.OnLevelCompleted | Medio | LevelExit ya tiene guard (`vnGate.IsShowing`). Anadir guard adicional: si DialogueTrigger esta en modo narrativo, LevelExit espera. |
| R3 | **Performance**: Register/Unregister por trigger puede causar spikes si hay muchos props | Bajo | InteractionSystem mantiene una lista simple (List<InteractableObject>). El nearest se calcula solo cuando cambia la lista (no cada frame). |
| R4 | **Save corruption**: JSON narrativo se corrompe | Medio | NarrativeSaveBridge usa try/catch + backup .bak (igual que SaveSystem spec FAIL-SAV-001). Si falla la carga, flags se resetean a defaults. |
| R5 | **VN_EndingFlags ya tiene DontDestroyOnLoad**: anadir persistencia puede causar doble carga | Bajo | NarrativeSaveBridge.Load() solo carga si VN_EndingFlags.Instance.flags esta vacio (primera carga de sesion). |
| R6 | **Input migration**: cambiar de Input.GetKeyDown a InputActionMap puede romper bindings | Bajo | InputActionMap ya expone `PlaybackPressed` que mapea a E. Migracion es 1:1. |
| R7 | **VisualStateChange puede romper validadores** (RULE-ENV-002 amber exclusivity) | Medio | VisualStateChange solo puede atenuar o desactivar, nunca cambiar a un color no-amber. Validado en MemorySystem. |
| R8 | **DialogueChoice con NEXT_NODE puede crear loops infinitos** | Bajo | DialogueTrigger tiene un maximo de 50 nodos por secuencia. Si excede, corta. |
| R9 | **NarrativeStateController singleton vs LevelRuntimeController singleton** | Bajo | NarrativeStateController es DontDestroyOnLoad (sobrevive cargas de escena). LevelRuntimeController es por escena. No conflict. |
| R10 | **Modificar LevelRuntimeController.OnLevelCompleted** puede romper el flow de 15 niveles | Alto | La modificacion es un check adicional antes del gate existente. Si no hay DialogueTrigger, comportamiento identico. |

---

## 14. ORDEN DE IMPLEMENTACION PROPUESTO

### Fase A: Fundacion de Datos (sin tocar runtime)
1. Crear todos los ScriptableObjects y serializable classes (archivos 1-11).
2. Crear enums y datos. Sin MonoBehaviour, sin romper nada.
3. **Validacion**: Compila limpio. 0 errores.

### Fase B: Logica Narrativa (nuevos singletons)
4. Crear NarrativeStateController (singleton, variables, modo narrativo).
5. Crear MemorySystem (singleton, inspecciones, persistencia en memoria).
6. Crear NarrativeSaveBridge (static, JSON save/load).
7. Crear EndingEvaluator (static, delega a VN_EndingResolver).
8. Crear NarrativeConditionEvaluator (static, parsea condiciones).
9. **Validacion**: Compila limpio. 0 errores. Tests unitarios de EndingEvaluator.

### Fase C: Trigger y Integracion
10. Crear DialogueTrigger (MonoBehaviour, orquesta).
11. Modificar InteractableObject (anadir referencia a data, fix Register/Unregister).
12. Modificar InteractionSystem (implementar Register/Unregister, usar InputActionMap, delegar a DialogueTrigger).
13. Modificar InteractionPromptController (ShowPromptFor).
14. **Validacion**: Compila limpio. Play mode: player camina, prompt aparece, E inspecciona, Chalkboard 2.5s.

### Fase D: Persistencia
15. Modificar VN_EndingFlags (LoadFromDisk/SaveToDisk).
16. Modificar GameProgress (SaveNarrativeState/LoadNarrativeState).
17. **Validacion**: Inspeccionar objeto, cerrar juego, reabrir, flag persiste.

### Fase E: Integracion de Level Flow
18. Modificar LevelRuntimeController (check DialogueTrigger antes de gate).
19. **Validacion**: Flujo completo N01 -> N15 -> ending -> epilogo.

### Fase F: Datos Narrativos (assets)
20. Crear InteractableData assets para los 15 niveles (basado en ENVIRONMENT_STORYTELLING Table 8.2).
21. Crear DialogueSequence assets para escenas narrativas designadas.
22. Crear NarrativeFlagCatalog asset (migrar flag_catalog de VN_ENDINGS_REDEFINED.yaml).
23. Crear NarrativeVariableStore asset.
24. **Validacion**: Cada nivel tiene sus props con InteractableData. Inspeccion funciona end-to-end.

---

## 15. CRITERIOS DE ACEPTACION

1. **Props como partes del mundo**: El prompt aparece solo por proximidad
   (trigger enter), no por raycast. No hay botones flotantes. El prompt es
   diegetico (esquina inferior, estilo Chalkboard).
2. **Deteccion de proximidad**: Register/Unregister funcionan. El prompt
   muestra el prop mas cercano. Al alejarse, el prompt desaparece.
3. **Bloqueo adaptativo**: Inspect/Memory no bloquean movimiento.
   Dialogue/Choice bloquean movimiento y ocultan HUD.
4. **Interfaz narrativa**: Se abre la interfaz correcta segun interactionType.
   Inspect -> Chalkboard. Dialogue -> VN_DialogueController. Choice -> Gate.
5. **Registro de decisiones**: Cada choice setea flags, variables y
   comprehension. VN_EndingFlags se actualiza.
6. **Estado del mundo**: VisualStateChange se aplica. oneTimeOnly se respeta.
7. **Restauracion**: Al cerrar la narrativa, el movimiento se restaura, el
   HUD reaparece, el cursor se bloquea.
8. **Persistencia**: Los flags y variables se guardan a disco. Al reabrir
   el juego, el estado narrativo se restaura.
9. **Ending evaluation**: EndingEvaluator resuelve el mismo ending que
   VN_EndingResolver (delegacion). 32 paths canonicos siguen verde.
10. **Canon**: 0 violaciones de BIB-TXT-003, ANTI-BIB-004/005,
    RULE-INSPECT-001 a 010, RULE-PHI-005.

---

## 16. CROSS REFERENCES

- [SOURCE_OF_TRUTH.md](../Authority/SOURCE_OF_TRUTH.md) `[SPEC-000]`
- [ECHOES_BIBLE.md](../GameDesign/ECHOES_BIBLE.md) `[SPEC-101]`
- [DESIGN_PHILOSOPHY.md](../GameDesign/DESIGN_PHILOSOPHY.md) `[SPEC-001]`
- [NARRATIVA_INTERNA.md](../GameDesign/NARRATIVA_INTERNA.md) `[DOC-102]`
- [INSPECT_POPUP_SPEC.md](../UI/INSPECT_POPUP_SPEC.md) `[SPEC-UI-009]`
- [VN_PLUS_POPUPS_PLAN_v2.md](../UI/VN_PLUS_POPUPS_PLAN_v2.md)
- [ENVIRONMENT_STORYTELLING.md](../Specs/ENVIRONMENT_STORYTELLING.md) `[SPEC-006]`
- [PROP_GRAMMAR.md](../Specs/PROP_GRAMMAR.md) `[SPEC-005B]`
- [SAVE_DATA_SCHEMA.md](../Specs/SAVE_DATA_SCHEMA.md) `[SPEC-119]`
- [DIALOGUE_TREE_SCHEMA.yaml](../ExecutableSpecs/narrative/DIALOGUE_TREE_SCHEMA.yaml) `[SPEC-EXEC-NAR-001]`
- [NARRATIVE_STATE_MACHINE.yaml](../ExecutableSpecs/narrative/NARRATIVE_STATE_MACHINE.yaml) `[SPEC-137]`
- [VN_ENDINGS_REDEFINED.yaml](../ExecutableSpecs/narrative/VN_ENDINGS_REDEFINED.yaml)
- [INTERACTION_SYSTEM_SPEC.yaml](../ExecutableSpecs/gameplay/INTERACTION_SYSTEM_SPEC.yaml) `[SPEC-EXEC-INT-001]`
- [INPUT_ACTION_MAPS.md](../Specs/INPUT_ACTION_MAPS.md) `[SPEC-118]`

---

## 17. CHANGE HISTORY

- **v1.0 (2026-08-13)**: Propuesta inicial. Auditoria completa de 8 sistemas.
  Arquitectura de 10 componentes. 17 archivos a crear, 6 a modificar.
  Diagrama de flujo. 10 riesgos identificados. Pendiente de aprobacion.
