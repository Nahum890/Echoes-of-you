---
level: 7
name: "Graba tu intención futura"
archetype: Twist
chapter: III
blueprint:
  echoEnabled: true
  maxEchoes: 1
  maxRecordSeconds: 12
  recordFuture: true            # novelty: L07 is the only level with this flag
  ambientEchoData: false
  imposedEchoData: false
  inversionCamera: false
puzzle:
  timingFloor: 0.4
  components:
    - { name: Plate_AliveNow_A, type: PressurePlate, position: [-3, 0, 22], scale: [2, 0.12, 2], requiresEcho: false }
    - { name: Plate_AliveNow_B, type: PressurePlate, position: [ 3, 0, 22], scale: [2, 0.12, 2], requiresEcho: false }
    - { name: PuzzleCond_Simultaneous, type: PuzzleCondition, position: [0, 0, 28], customData: "AllPlatesSimultaneous|..|Sostén las dos presentes"|..|Solo una no basta" }
    - { name: Door_FutureGated, type: Door, position: [0, 2, 32], scale: [4, 4, 0.4], targetSignals: [PuzzleCond_Simultaneous] }
    - { name: LevelExit_Area,    type: LevelExit, position: [0, 1.1, 56], customData: "Level_08", targetSignals: [] }
    - { name: LevelGoal,         type: LevelGoal, position: [0, 1.1, 56], customData: "Sostén las dos presentes en tu futuro.|Lo grabaste antes de necesitarlo.|.", targetSignals: [PuzzleCond_Simultaneous] }
  wiring:
    - { src: [Plate_AliveNow_A, Plate_AliveNow_B], dst: PuzzleCond_Simultaneous, gate: ALLPLATES }
    - { src: PuzzleCond_Simultaneous, dst: Door_FutureGated, gate: LATCH }
test:
  echoButtonTest: PASS
  timingFloor: 0.4
  expectedSoftlocks: 0
  sequenceBreaks: 0
solution:
  optimal_path:
    - "Player walks from start to [0,0,16] (facing the gate door)."
    - "Plate_AliveNow_A is at [-3,0,22]. Plate_AliveNow_B is at [3,0,22]. The plates are reachable by ONE actor walking diagonally — impossible to hold both simultaneously by one body."
    - "Player records an Echo: walk forward into the room at [0,0,16] → diagonally to [-3,0,22] (Plate_A) at t=2s → continue forward toward [+3,0,22] (Plate_B) arriving at t=4s. Recording duration ~4s."
    - "Player releases Record. Echo spawns at [0,0,16] and waits 0.8s latency."
    - "t=0.8s Echo starts translating. At t≈2s reaches Plate_A. AT THAT MOMENT, the player (in present) walks from [0,0,40] (an upper-tier standard platform) ACROSS a small bridge and IMMEDIATELY steps onto Plate_B."
    - "Echo arrives at Plate_A (Echo IsPressed=true). Player is on Plate_B (Player IsPressed=true). Both pressed same FixedUpdate (assuming player timing nailed) → PuzzleCond_Simultaneous setMet=true."
    - "Door_FutureGated opens, latched."
    - "Echo continues toward Plate_B (its recorded path tries to walk onto plate B where Player stands)."
    - "Echo residual time arrives; Echo stops at Plate_B position; residual 2.5s; Plate_B still registers BOTH Player AND Echo as `IsPressed` — never reverts."
    - "Player walks through open door, reaches LevelExit."
  failure_modes:
    late_recording: "Player does Recording STOP and tries to immediately press Plate_A themselves — but Player walks back, doesn't wait for Echo to be in position. Plate_B only self is pressed, PuzzleCond stays false. Door closed."
    late_player_action: "Player does the recording but then wanders — Echo's residual ends (echoes' recording end+2.5 is the total window). After residual, Plate_A releases AND Plate_B is empty (if Player not there yet). Door closed."
    no_recording_too_short: "If Player tries to skip the recording and run across, the Echo doesn't have a body to occupy Plate_A, AND plate_B alone won't open the door."
  design_note:
    - "The level title 'Graba tu intención futura' signals that the Echo IS the future; recording the Echo is your future-self committing to an action that happens later."
    - "The exit is unlocked by the UnlockWindow: 5 seconds. Player has 5 s from `EchoRecorder.RecordingStopped` time to press both plates via Echo + own body, before LevelExit auto-locks. This is mediated by RecordFutureExit Phase 2 class."
---

# LEVEL 07 — Graba tu intención futura (Chapter III — Twist: recordFuture)

**Archetype**: Twist — the player commits to an action before they need it. Their recording IS the future.

## Puzzle Mechanic
- Two plates in spatial positions impossible to single-handedly hold simultaneously.
- One plate (A) is reached by the Echo playback — recording made before.
- Other plate (B) is held by the player directly in real time.
- Both must fire in same FixedUpdate ⇒ AllPlatesSimultaneous setMet=true ⇒ Door_Now opens latched.
- **UnlockWindow 5s**: After `EchoRecorder.RecordingStopped` is invoked (player releases Record key), the LevelExit `_isUnlocked=true` for 5 s before returning to false. Player must press Plate_B within 5 s of releasing the recording.

## Component: RecordFutureExit (Phase 2 new MonoBehaviour)
Created as `Assets/Scripts/Puzzle/RecordFutureExit.cs`:

```csharp
public class RecordFutureExit : MonoBehaviour {
    [SerializeField] LevelExit exit;
    [SerializeField] EchoRecorder recorder;
    [SerializeField] float window = 5f;

    void OnEnable() { recorder.RecordingStopped += OnRecordingStopped; }
    void OnDisable() { recorder.RecordingStopped -= OnRecordingStopped; }

    void OnRecordingStopped(bool success) {
        if (success) StartCoroutine(WaitForWindow());
    }
    System.Collections.IEnumerator WaitForWindow() {
        exit.SetUnlocked(true);
        yield return new WaitForSeconds(window);
        // If LevelGoal not ready, reverse; if ready, leave.
        exit.SetUnlocked(LevelGoal.Instance != null && LevelGoal.Instance.IsReady);
    }
}
```

The door here is gated by `PuzzleCond_Simultaneous.IsMet`, NOT by `LevelExit._isUnlocked`. So the player has TWO simultaneous gates:
1. **Door passability**: gated by PuzzleCond_Simultaneous.
2. **Exit reachability**: gated by 5 s unlock window.

If both hold simultaneously → player passes through.

## Sync Floor
Two-input AND puzzle — `PuzzleWire.Connection.logic=OR` or direct PuzzleCondition with AllPlatesSimultaneous.
- timingFloor=0.4 enforced by PuzzleCondition evaluation timing.
- Window of 5s is generous relative to 0.4s sync floor — no interplay issue.

## Echo Button Test
- Backward BFS: LevelExit ← LevelGoal ← PuzzleCond_Simultaneous ← Plate_AliveNow_A (allow both players AND Echos) — `requiresEcho` is false (this is the only level where the BACKWARD test would fail on student plapes).
- IMPORTANT: The level depends on the Echo being REQUIRED transit-wise (the recorded Echo presses Plate_A and the player presses Plate_B in real time). The Echo Button Test as currently stated looks for "any requiresEcho==true node on path to exit"; L07 has none — that's the actual Twist.
- The validator (Phase 3) **must add a special-case for recordFuture levels**: the requiresEcho gate is satisfied by the `recordFuture==true` flag on the blueprint (denoting the Echo's temporal dependency is theREC), without needing a `PressurePlateEchoOnly` source node.
- Pass criteria (in Phase 3 code):
  ```
  if (blueprint.recordFuture) return true;   // Echo dependency is the EXIT's recordFuture
  ```

## Failure Modes
- Player walks directly to Plates without recording → no Echo body to press Plate_A; ORwise fails.
- Player records too-short, Echo doesn't move → plate_a never presses.
- Player records the Echo pressing Plate_A AND Plate_B (left both to Echo): wouldn't have a player actor on Plate_B. Wait — could the Echo press BOTH in sequence? No: AllPlatesSimultaneous explicitly requires SAME FixedUpdate; sequential okay if Echo's recording duration is 0.
- Player dawdles after stopping recording → outside the 5s window → LevelExit stays locked, regardless of PuzzleCond firing.
