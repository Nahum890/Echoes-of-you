---
level: 10
name: "Lyra ya caminó. Síuela"
archetype: Experimentation
chapter: IV
blueprint:
  echoEnabled: true
  maxEchoes: 1
  maxRecordSeconds: 12
  recordFuture: false
  ambientEchoData: true
  imposedEchoData: false
  inversionCamera: false
puzzle:
  timingFloor: 0.4
  components:
    - { name: PlayerStart, type: PlayerStart, position: [0, 0, -4], scale: [1, 1, 1] }
    - { name: AmbientGhost, type: AmbientEchoData }
    - { name: Plate_Echo_Only, type: PressurePlate, position: [0, 0, 28], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Door_Timed, type: Door, position: [0, 2, 32], scale: [4, 4, 0.4], targetSignals: [Plate_Echo_Only] }
    - { name: LevelExit_Area, type: LevelExit, position: [0, 1.1, 56], customData: "Level_11" }
    - { name: LevelGoal, type: LevelGoal, position: [0, 1.1, 56], customData: "Lyra guidó el eco para abrir la salida.|.", targetSignals: [Plate_Echo_Only] }
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
    - "Ambient ghost Lyra walks the level revealing hidden MemoryPlatforms; the player uses the Echo to press the plate and open the door."
---

# LEVEL 10 — Lyra ya caminó. Síuela (Chapter IV — Experimentation)

**Archetype**: Experimentation — adds ambient Lyra ghost mechanic while preserving basic pressure‑plate puzzle.

## Design Notes
- `AmbientEchoData` disables player recording and spawns a ghost that reveals hidden MemoryPlatforms.
- The puzzle still requires an Echo‑only plate, but the Echo is supplied by the ambient ghost (implementation handled by the AmbientEchoData component).

## Echo Button Test
- Even though player recording is disabled, the ghost provides the required Echo press, satisfying the BFS path.

## Failure Modes
- If the ambient ghost fails to traverse the plate, the door remains locked.
