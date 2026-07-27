---
level: 11
name: "Lo que no alcanzas a grabar no existe"
archetype: Teaching
chapter: IV
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
    - { name: LevelExit_Area, type: LevelExit, position: [0, 1.1, 56], customData: "Level_12" }
    - { name: LevelGoal, type: LevelGoal, position: [0, 1.1, 56], customData: "Solo el eco puede abrir la salida.|.", targetSignals: [Plate_Echo_Only] }
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
    - "Player records an Echo that presses the plate, opening the door and allowing exit."
---

# LEVEL 11 — Lo que no alcanzas a grabar no existe (Chapter IV — Teaching)

**Archetype**: Teaching — re‑emphasises the core Echo‑only mechanic without additional twists.

## Design Notes
- No extra flags; standard Echo‑only pressure plate.
- The title reflects the narrative rather than a new mechanic.

## Echo Button Test
- Simple BFS confirms PASS.

## Failure Modes
- Recording too short or missing the plate.
