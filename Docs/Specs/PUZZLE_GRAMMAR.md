# PUZZLE GRAMMAR — Archetype Specification

**Source of truth**: derived from `Docs/GameDesign/ECHOES_BIBLE.md` §6 "Gramática de Gameplay" + §7 "Estructura de Campaña".
Maps directly to `ModuleType` enum (`Assets/Scripts/LevelBlueprint.cs:22-75`) and the 15-row puzzle map in the task prompt.

---

## 1. The 5 Archetypes

| Archetype | Does | Asks | Forbids |
|---|---|---|---|
| **Teaching** | Introduces one mechanic alone, in a clean room | "Have you understood X?" | Any variation of X; any second mechanic; ambiguity |
| **Experimentation** | Variations on X, same idea in a new context | "Can you apply X in a new condition?" | A new mechanic; a twist the player hasn't been taught |
| **Combination** | X combined with the chapter's previous mechanic | "Can you use both ideas in one space?" | Either idea alone being sufficient; teaching a third idea |
| **Twist** | X used in a way that inverts an assumption | "Did you trust X too much?" | Visible warning of the twist before the reveal |
| **Mastery** | X at peak precision; no slack | "Can you execute X under pressure?" | Forgiving timing; safety nets that the prior chapter didn't have |
| **Acceptance** (extension of ECHOES_BIBLE §7 cap VI) | The mechanic is taken from the player | "Can you be present without your past self?" | Re-introduction of the system |
| **Integration** | Three sub-puzzles chained, each from a different chapter | "Can you hold all of these ideas simultaneously?" | Introducing any new mechanic |

---

## 2. Required / Allowed / Forbidden Components per Archetype

### Teaching
- **Required**: 1 PressurePlate (or PressurePlateEchoOnly) → 1 DoorController; 1 LevelExit.
- **Allowed**: 3-4 StandardPlatform / SchoolCorridor / SchoolClassroom for traversal.
- **Forbidden**: PuzzleCondition, HazardField, ConflictTrap, TimedMovingPlatform, TemporalBridge, PuzzleSignal, 2 PressurePlates, ambientEchoData, imposedEchoData, recordFuture, inversionCamera.

### Experimentation
- **Required**: 2-3 PressurePlates OR 1 PuzzleCondition + 0-1 HazardField/ConflictTrap; 1 DoorController; 1 LevelExit.
- **Allowed**: Half the chapter's normalized vocabulary (TimedMovingPlatform from Ch I, columns/platforms).
- **Forbidden**: TemporalBridge, recordFuture, ambientEchoData, imposedEchoData, inversionCamera, PuzzleSignal in excess of 1, ghost bridge visibility.

### Combination
- **Required**: 2 distinct puzzle types BOTH mandatory to reach the exit; 0-1 ConflictTrap to enforce order; 2 Echo slots (default `maxEchoes = 2`).
- **Allowed**: full chapter vocabulary + prior chapter vocabulary; 1 PuzzleSignal.
- **Forbidden**: TemporalBridge as solo mechanic; recordFuture; ambientEchoData; imposedEchoData; inversionCamera.

### Twist
- **Required**: One of {TemporalBridge, recordFuture, mutually-exclusive Echoes, maxRecordSeconds < traversal time}; player's prior expectation inverted.
- **Allowed**: Any component conditional on the twist mechanism.
- **Forbidden**: Explicit tutorial text warning; safety nets that recover prior chapters did not have; tutorial hint lights (`LEVEL_SPEC_XX.yml` pathHints suppressed in Level >= 6).

### Mastery
- **Required**: One of {PuzzleSignal + LevelGoal timer, imposedEchoData, ambientEchoData}; precise timing window; Sync Floor ≥ 0.4 s.
- **Allowed**: Full vocabulary of the book; full chapter vocabulary; no Echo slot limit ≤ 2; no path hints.
- **Forbidden**: Latch-open doors that survive a mis-press; any retry affordance that previous chapter didn't have.

### Acceptance
- **Required**: `maxEchoes = 0` OR `inversionCamera = true`; no echo available for the puzzle; player must act in real-time.
- **Allowed**: All non-Echo mechanics of the chapter; one Echo-shaped "shell" (memory platform, ghost-bridge visible already — but no recording).
- **Forbidden**: Recording capability; introducing a non-recorded Echo as a partner; PuzzleCondition with Echo-only plates.

### Integration
- **Required**: Three concatenated micro-puzzles, each authored in one of {Teaching, Experimentation, Combination, Twist, Mastery} archetype pools.
- **Allowed**: All vocabulary of the book; up to 2 Echo slots.
- **Forbidden**: A 4th puzzle; any mechanic the book doesn't define.

---

## 3. The "Una Idea" Rule (Portal Clarity Rule)

> "¿Puede describirse lo que este nivel enseña en una sola frase?"
> — ECHOES_BIBLE §6

**Per-level one-liner** (this is the only acceptable `narrativeIntroTitle` length):

| Level | One-line |
|---|---|
| 01 | Tu pasado abre puertas que el presente no puede. |
| 02 | Dos presiones al mismo tiempo son más que dos presiones. |
| 03 | Tu eco puede ahogarte si se queda después de ayudar. |
| 04 | El eco se demora; aprende a anticiparlo. |
| 05 | La memoria exacta es la única memoria provechosa. |
| 06 | Lo que ves es presente. Lo que recuerdas es verdad. |
| 07 | Graba tu intención futura antes de necesitarla. |
| 08 | Dos presentes no caben; uno debe preceder al otro. |
| 09 | El instante justo abre la salida. |
| 10 | Lyra ya caminó. Síuela. |
| 11 | Lo que no alcanzas a grabar no existe. |
| 12 | Cada eco cierra una puerta en el otro. |
| 13 | Otro grabó por ti. Baila su pieza. |
| 14 | Sin pasado, solo espejo. |
| 15 | Tres capítulos en una sola pieza. |

---

## 4. Failure Criterion (Regla de Fracaso Legible)

> "El fracaso de un puzzle debe comunicarse visualmente, sin texto."
> — ECHOES_BIBLE §6

A failed Echo recording MUST produce, at minimum, one of these legible signals:

- Echo falls into the void (kill volume / out-of-bounds despawn).
- Echo stops mid-plate because the player released too early (residual state visibly fades, plate releases, door closes).
- ConflictTrap spins warning rotors; door slams shut.
- HazardField stays red while conflict trap is armed.
- TemporalBridge stays ghost-blue translucent when Echo is absent.

Forbidden failure modes:

- Toast messages with technical error text ("Condition not met in 0.4s") — only colored toast encouraged ("Tu eco se quedó demasiado tiempo.") and it must reinforce the visible state.
- Silent reset with no visual change.
- Exit-violating softlocks (door never reopens for the next attempt).

---

## 5. ECHO_GRAMMAR Coupling

Every archetype MUST honor `ECHO_GRAMMAR.md` timings when used:

- Architectures with `TimedHold` PuzzleCondition must add 0.8 s to expected `holdDuration` subtracted from real recorded Echo time.
- Architectures with `TemporalBridge` must time the visible window to overlap the Echo's `Recording + Residual = RecordingElapsed + 2.5s`.
- Architectures with `recordFuture` must emit the 5 s unlock window from `RecordingStopped` event time, not from `OnTriggerEnter` time.
- Any sync puzzle (level 02, 05, 11) must request timingFloor check `>= 0.4s`; the validator enforces.

---

## 6. Vocabulary Mapping

This grammar is **backed by the existing `ModuleType` enum** in `Assets/Scripts/LevelBlueprint.cs:22-75`:

| Archetype component | ModuleType int | Factory method (`EchoesModuleFactory.cs`) |
|---|---|---|
| PressurePlate | 4 | `MakePressurePlate` |
| PressurePlateEchoOnly (Phase 2) | 4 + `customData="EchoOnly"` | `MakePressurePlate` → variant dispatch (Phase 2) |
| Door | 5 | `MakeDoor` |
| MovingPlatform (timed) | 8 | `MakeMovingPlatform` → adds `TimedMovingPlatform` |
| PuzzleSignal | 15 | `MakePuzzleSignal` |
| PuzzleCondition | 16 | `MakePuzzleCondition` |
| HazardField | 17 | `MakeHazardField` (→ `EchoShieldField`) |
| ConflictTrap | 18 | `MakeConflictTrap` (→ `EchoConflictTrap`) |
| MomentumRelay | 19 | `MakeMomentumRelay` (→ `EchoKineticZone`) |
| LevelGoal | 13 | `MakeLevelGoal` |
| LevelExit | 6 | `MakeLevelExit` |
| TemporalBridge | 22 | `MakeTemporalBridge` |
| GhostBridge | 44 (Phase 2) | `MakeGhostBridge` (Phase 2) |

Phase 2 adds `ModuleType.GhostBridge = 44` and 5 MonoBehaviours (`PressurePlateEchoOnly`, `RecordFutureExit`, `AmbientEchoData`, `ImposedEchoData`, `InversionCamera`) — see Phase 2 spec.
