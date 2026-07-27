# ECHO GRAMMAR — Canonical Timing & State Specification

**Source of truth**: derived from `Docs/GameDesign/ECHOES_BIBLE.md` (§5 "Sistema del Eco")
and verified against `Assets/Scripts/EchoRecorder.cs`, `Assets/Scripts/EchoPlayback.cs`,
`Assets/Scripts/RecordFrame.cs`. Any discrepancy: code wins for runtime, this doc wins for design intent.

---

## 1. Canonical Numerical Constants

| Constant | Value | Source | Enforced by |
|---|---|---|---|
| `MAX_RECORD_SECONDS` | 12.0 s | ECHOES_BIBLE §5 / prompt table | `LevelBlueprint.maxRecordSeconds = 12f` (LevelBlueprint.cs:114) |
| `ECHO_LATENCY` | 0.8 s | ECHOES_BIBLE §5 "Latencia de inicio" | hard-coded in `EchoPlayback` startup; surfaced to designers via `customData` of `TimedMovingPlatform` and `PuzzleSignal.requiresEcho` windows |
| `RESIDUAL_DURATION` | 2.5 s | ECHOES_BIBLE §5 "Residual" | `EchoPlayback` post-playback coroutine |
| `SYNC_FLOOR` | 0.4 s | prompt / ECHOES_BIBLE §6 (timing of reading) | `LevelBlueprint.timingFloor`, enforced by `PuzzleLevelValidator.TimingFloorSatisfied` |
| `PROJECTION_STEP_UP` | 0.4 m | `EchoRecorder.cs:26` | `maxProjectionStepUp` field |

All times are real-time seconds. They are **deterministic** by design (ECHOES_BIBLE §5: "su imperfección no es aleatoria").

---

## 2. Echo Lifecycle States

The Echo progresses through 4 states. Each has mechanical consequences (not cosmetic):

| # | State | Visual | Collision | Duration | Transition |
|---|---|---|---|---|---|
| 0 | **Absent** | None | Off | — | Player holds Record key → state 1 |
| 1 | **Recording** | Player gets cyan rim light; Echo does not exist visually (it is "in the future"); optional projection pilot | Off | ≤ 12 s (auto-stops at cap) | Player releases key OR `RecordingElapsed >= MAX_RECORD_SECONDS` → state 2 |
| 2 | **Playing** | Cyan translucent, alpha 0.45; in last 20 % of recording time alpha decays to 0.1 | **On** | Exactly `RecordingElapsed` (≤12 s) | Playback elapsed ≥ recorded duration → state 3 |
| 3 | **Residual** | Alpha 0.3 → 0 (ease-in) | **On** (mechanical: usable as platform, blocker, plate-holder) | 2.5 s fixed | After 2.5 s → state 0 (despawn) |

### 2.1 Residual is mechanical

> "El estado residual es mecánico, no solo estético. Un eco en estado residual puede usarse como plataforma, bloqueador, o punto de apoyo. Los niveles deben diseñarse aprovechando esta ventana."
> — ECHOES_BIBLE §5

Designers MUST design at least one level puzzle that uses the 2.5s residual window explicitly (see LEVEL_SPEC_03 "Echo exits before trap" / LEVEL_SPEC_06 "Record crossing before visible" / LEVEL_SPEC_10 "MemoryPlatforms revealed only during residual").

### 2.2 Recording start latency

Player triggers Record key:
- t = 0.0s  → first frame added at player's current position
- t = 0.0s..12.0s → frames appended at FixedUpdate cadence
- t = stop  → playback created, starts with t' = 0 internal timer

Playback's first motion has a **0.8s fixed latency** from when `BeginPlayback` is called to when the Echo body actually starts translating along its frame track. This window is exactly what Level 04's `TimedMovingPlatform(0.8s latency)` exploits, and what `PuzzleCondition.TimedHold` MUST subtract from its `holdDuration` when plate is Echo-pressed (so `holdDuration_effective = holdDuration - 0.8`).

---

## 3. Acceptance Source Rules

| Tag | Detected By | PressurePlate.accept* flag |
|---|---|---|
| `Player` | `PressurePlate.IsAcceptedActor` | `acceptPlayer = true` (default) |
| `Echo` | `PressurePlate.IsAcceptedActor` | `acceptEcho = true` (default) |
| `EchoProjection` | `PressurePlate.IsAcceptedActor` | `acceptEchoProjection = true` (default) |
| `KineticBlock` | `KineticPushableBlock` GetComponentInParent | always (no flag) |

A plate flagged `EchoOnly` (via `PressurePlateEchoOnly` subclass, created in Phase 2) has `acceptPlayer=false, acceptEcho=true, acceptEchoProjection=true` — but never `KineticBlock`. This is the mechanism for LEVEL_SPEC_01's "Plate requires Echo" test.

---

## 4. The Button Test (Prueba del Botón)

> "¿Puede este puzzle resolverse exactamente igual con una caja empujable genérica en lugar del eco? Si la respuesta es sí, el puzzle no pertenece a este juego. Rediseñar."
> — ECHOES_BIBLE §6

**Operationalized** by `PuzzleLevelValidator.EchoButtonTest` (Phase 3):

1. Build the signal graph from `blueprint.modules`:
   - Every puzzle component is a graph node.
   - An entry's `targetSignals[i]` creates a directed edge: that name's node → this node (signal source drives signal target).
   - Every `PressurePlate` (or `PressurePlateEchoOnly`) whose `acceptEcho == true && acceptPlayer == false` is marked `requiresEcho = true`.
2. Find the `LevelExit` node.
3. BFS backward from the exit through all signal edges.
4. **PASS** iff at least one `requiresEcho == true` node lies on a reachable path to the exit.

This is the single accept/reject gate for every level.

---

## 5. Rule of Irreversibility

> "Una grabación reproducida no puede deshacerse excepto re-grabando desde cero."
> — ECHOES_BIBLE §5

Operationalized as:

- Calling `EchoPlayback.BeginPlayback` cannot be cancelled once started.
- Calling `EchoRecorder.ClearAllEchoes` despawns the Echo + frames; to get another Echo the player must record from scratch.
- `ImposedEchoData` (Level 13) extends this: it disables the Record key entirely; the only Echo available is one pre-baked by the designer into `Assets/Data/Solutions/Level_13.solution.asset`.

---

## 6. Sync Floor

For any puzzle requiring simultaneous or sequential buttons within a tolerance window:

- `PuzzleCondition.ConditionType.AllPlatesSimultaneous` requires all referenced plates to read `IsPressed == true` in the same `FixedUpdate` frame. If two actors press the plates within 0.4 s of each other (one typical latência-triggered slip), the window has not been met — the puzzle should re-evaluate next frame, not latch.
- Minimum snap window between two temporally-ordered signal activations is 0.4 s. Below this, `PuzzleLevelValidator.TimingFloorSatisfied` flags `FAIL-PUZ-03`.
- `PuzzleWire.Connection.responseDelay` of 0 s disables the timing floor for that wire. Any non-zero `responseDelay` ≥ 0.4 s is permissible; below 0.4 s is a `FAIL-PUZ-03` soft-only (warning) since the timing floor is about multi-actor puzzles, not single-actor button debouncing.

---

## 7. Recording Capacity Constraints

- Default: 1 Echo slot (ECHOES_BIBLE campaign Chapter I).
- Level 08 Combination: 2 Echo slots (selection conflict enforced by `EchoConflictTrap`).
- Level 14 Acceptance: 0 Echo slots (`maxEchoes = 0` — `EchoRecorder.Update` short-circuits at line 87).
- Any level with `recordFuture = true` (Level 07): the Echo must be recorded **before** the trigger is activated; otherwise the 5 s window consumes the writer's future-tense capability without success.

---

## 8. Filesystem Encoding

Optimal solution replays are stored as `Assets/Data/Solutions/Level_XX.solution.asset` (ScriptableObject). Each asset contains:

```csharp
public sealed class EchoSolutionAsset : ScriptableObject {
    public RecordFrame[] frames;        // optimal player path
    public RecordFrame[] echoFrames;    // optimal echo recording
    public float        echoStartTime;  // t when Echo should start playing back (latency offset aware)
    public float        expectedDurationSeconds;
    public int          expectedSoftlocks;
    public int          expectedSequenceBreaks;
}
```

The headless EditMode playtest loader:
1. Spawns a `NavMeshAgent` actor at `frames[0].position`.
2. Walks the agent through frames at fixed 0.1 s step.
3. At `echoStartTime`, creates an Echo body and feeds `echoFrames` to `EchoPlayback.BeginPlayback`.
4. Asserts `LevelExit` is reached within `expectedDurationSeconds`.
5. Asserts no Update cycle kept the actor outside within 1 m of any frame for more than 30 s (softlock check).
6. Asserts the only path to `LevelExit.IsReady` was via the `LevelGoal.EvaluateState` chain — i.e. no sequence break.

---

## 9. Echo First Frame Stability

`EchoRecorder.FixedUpdate` (line 126–130) duplicates the first frame at `t=0` for interpolation stability. Any `EchoSolutionAsset.frames` created by the generator MUST also duplicate frame 0:

```
frames[0] = (t=0, position=frames[0].position, rotation=frames[0].rotation)
frames[1] = (t=0.1, ...)
```

This guarantees `RecordFrame.Evaluate` (line 30) returns a valid pose immediately at playback start, before the 0.8 s latency bootstrap.
