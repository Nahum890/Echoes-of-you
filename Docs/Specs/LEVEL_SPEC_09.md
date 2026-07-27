---
level: 9
name: "El instante justo abre la salida"
archetype: Twist
chapter: III
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
    - { name: PlayerStart, type: PlayerStart, position: [0, 0, -4], scale: [1, 1, 1] }
    - { name: Plate_Echo_Only, type: PressurePlate, position: [0, 0, 28], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Door_Timed, type: Door, position: [0, 2, 32], scale: [4, 4, 0.4], targetSignals: [Plate_Echo_Only] }
    - { name: LevelExit_Area, type: LevelExit, position: [0, 1.1, 56], customData: "Level_10" }
    - { name: LevelGoal, type: LevelGoal, position: [0, 1.1, 56], customData: "El instante justo abre la salida.|.", targetSignals: [Plate_Echo_Only] }
  wiring:
    - { src: Plate_Echo_Only, dst: Door_Timed, gate: AND }
    - { src: Plate_Echo_Only, dst: LevelGoal, gate: COUNT, requiredCount: 1 }
test:
  echoButtonTest: PASS
  timingFloor: 0.4
  expectedSoftlocks: 0
  sequenceBreaks: 0
solution:
  optimal_path:
    - "Player walks to the pressure plate, records an Echo that activates the door, then proceeds through the exit."
---

# LEVEL 09 — El instante justo abre la salida (Chapter III — Twist)

**Archetype**: Twist — introduces a precise timing requirement for the exit to open.

## Design Notes
- The single `PressurePlateEchoOnly` plate must be pressed by the Echo; the player cannot open the door directly.
- The `recordFuture` flag is **false**; the UnlockWindow is handled by `RecordFutureExit` only on recordFuture levels.
- No additional mechanics are introduced.

## Echo Button Test
- BFS path: LevelExit ← Door_Timed ← Plate_Echo_Only (requiresEcho=true). PASS.

## Failure Modes
- Player records too short or fails to reach the plate before the Echo spawns.
- Player steps on the plate themselves (blocked by `PressurePlateEchoOnly`).
