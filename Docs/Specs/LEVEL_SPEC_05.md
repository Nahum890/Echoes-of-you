---
level: 5
name: "Laberinto de timing"
archetype: Experimentation
chapter: II
blueprint:
  echoEnabled: true
  maxEchoes: 1
  maxRecordSeconds: 12
  recordFuture: false
  ambientEchoData: false
  imposedEchoData: false
  inversionCamera: false
puzzle:
  timingFloor: 0.4
  components:
    # Three branches at [0,0,22]: left (X=-6), center (X=0), right (X=6).
    - { name: Plate_DoorL_A, type: PressurePlate, position: [-6, 0, 24], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Plate_DoorL_B, type: PressurePlate, position: [-6, 0, 28], scale: [2, 0.12, 2], requiresEcho: false }
    - { name: Plate_DoorC_A, type: PressurePlate, position: [ 0, 0, 24], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Plate_DoorC_B, type: PressurePlate, position: [ 0, 0, 28], scale: [2, 0.12, 2], requiresEcho: false }
    - { name: Plate_DoorR_A, type: PressurePlate, position: [ 6, 0, 24], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Plate_DoorR_B, type: PressurePlate, position: [ 6, 0, 28], scale: [2, 0.12, 2], requiresEcho: false }
    # The doors. Sequence timers per door — fast latching once held.
    - { name: PuzzleCond_SeqL, type: PuzzleCondition, position: [-6, 0, 32], customData: "SequentialOrder|Secuencia izq|Secuencia correcta|Secuencia rota" }
    - { name: PuzzleCond_SeqC, type: PuzzleCondition, position: [ 0, 0, 32], customData: "SequentialOrder|Secuencia medio|Secuencia correcta|Secuencia rota" }
    - { name: PuzzleCond_SeqR, type: PuzzleCondition, position: [ 6, 0, 32], customData: "SequentialOrder|Secuencia der|Secuencia correcta|Secuencia rota" }
    - { name: Door_Left,   type: Door, position: [-6, 2, 34], scale: [4, 4, 0.4], targetSignals: [PuzzleCond_SeqL] }
    - { name: Door_Center, type: Door, position: [ 0, 2, 34], scale: [4, 4, 0.4], targetSignals: [PuzzleCond_SeqC] }
    - { name: Door_Right,  type: Door, position: [ 6, 2, 34], scale: [4, 4, 0.4], targetSignals: [PuzzleCond_SeqR] }
    # Only the CORRECT branch path leads to an exit passable to LevelExit.
    - { name: Plate_Bridge_A, type: PressurePlate, position: [-6, 0, 40], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Platform_Bridge, type: MovingPlatform, position: [-6, 0, 44], scale: [4, 0.3, 4], targetSignals: [Plate_Bridge_A], customData: "0,0,0|0,7,0|3|false" }
    - { name: Door_Exit, type: Door, position: [0, 7, 50], scale: [4, 4, 0.4], targetSignals: [Door_Left] }   # door opens only if L path completed (latched through Door_Left's latchOpen)
    - { name: LevelExit_Area, type: LevelExit, position: [0, 8, 56], customData: "Level_06" }
    - { name: LevelGoal, type: LevelGoal, position: [0, 8, 56], customData: "Graba el orden correcto.|El patrón revela el camino.|.", targetSignals: [Plate_Bridge_A] }
  wiring:
    - { src: [Plate_DoorL_A, Plate_DoorL_B], dst: PuzzleCond_SeqL, gate: SEQUENCE }
    - { src: [Plate_DoorC_A, Plate_DoorC_B], dst: PuzzleCond_SeqC, gate: SEQUENCE }
    - { src: [Plate_DoorR_A, Plate_DoorR_B], dst: PuzzleCond_SeqR, gate: SEQUENCE }
    - { src: Plate_Bridge_A,  dst: Platform_Bridge, gate: AND }
test:
  echoButtonTest: PASS
  timingFloor: 0.4
  expectedSoftlocks: 0
  sequenceBreaks: 0
solution:
  optimal_path:
    # Branch LEFT is the only correct branch (push A→B → opens Door_Left →
    # bridges Plate_Bridge_A which raises Platform_Bridge to upper tier →
    # arrives at LevelExit at upper Y).
    - "Player records Echo: walk from [0,0,16] to [-6,0,24] (cross Plate_DoorL_A) to [-6,0,28] (cross Plate_DoorL_B) — total ~5s."
    - "Echo playback: latency 0.8s → Echo on Plate_DoorL_A at t≈2s, on Plate_DoorL_B at t≈4s → PuzzleCond_SeqL → SetMet(true) → Door_Left opens latched."
    - "Echo continues to [-6,0,40] (Plate_Bridge_A). Player walks forward to [-6,0,30] but the door is open (lathed after sequenceMet). Plate_Bridge_A.IsPressed=true → Platform_Bridge elevates 7m over 2.3s."
    - "Echo residual (2.5s) keeps Plate_Bridge_A held — Platform_Bridge乘客回升 ABOVE → Player stands on platform after it ascends; reaches [0,7,50] → Door_Exit auto-opens; reaches LevelExit at [0,8,56]."
---

# LEVEL 05 — Laberinto de timing (Chapter II close)

**Archetype**: Experimentation — the second time the player sees "time+sequence matters". Now applied to a spatial maze.

## Three Branches
- **Left**: A→B sequence is the correct branch. Door_Left latches. Beyond it, Bridge_A raises the player to the level exit.
- **Center** & **Right**: dead-end branches. They each have a similarly-shaped sequence puzzle, but Door_Center / Door_Right lead to a hazard / drop / empty room, blocking LevelExit.

## Sequence + Latch
- `PuzzleCondition.ConditionType.SequentialOrder` requires Plate_A THEN Plate_B (in order).
- Out-of-order press resets the sequence index to 0.
- On success, `SetMet(true)` + `doorsToOpen` array opens Door (set via base factory wiring — Phase 2 fixes PuzzleCondition factory to honor Sequence-only-plates path).

## Sync Floor
- Sequence floors: minimum cadence 0.4 s between two button presses (not enforced by PuzzleCondition code today but enforced by the validator against the responseDelay of the connections).
- Here both SEQUENCE gates have a built-in cadence dependency; need to verify at Phase-3 stage.

## Echo Button Test
Backward BFS:
- LevelExit ← Door_Exit ← Door_Left ← Plate_Bridge_A → Plate_Bridge_A is requiresEcho=true. PASS.

## Failure Modes
- Player records Echo into Center branch: sequence here passes (topology symmetrical) but the door opens to NOTHING (a hazard / drop). Echo falls, plate releases, door shuts. Visible — the wrong branch's exit room is wall-less / has a kill volume.
- Player records Echo out-of-order: A → B is mistimed → sequence broken → _sequenceIndex resets → toast "Secuencia rota".
- Player dawdles and Echo residual ends BEFORE Platform_Bridge ascent completes (2.4s travel): bridge elevator drops itself to ground floor when Plate_Bridge_A releases — visible as platform slinking back.
- Echo reaches Plate_Bridge_A only AT END of its 5-second recording, residual only 2.5s → plate holds for ~4s total → no margin.
