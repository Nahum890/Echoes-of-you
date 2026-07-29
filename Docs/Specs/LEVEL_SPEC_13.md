---
level: 13
name: "Otro grabó por ti. Baila su pieza."
archetype: Mastery
chapter: V
blueprint:
  echoEnabled: true
  maxEchoes: 0
  maxRecordSeconds: 12
  recordFuture: false
  ambientEchoData: false
  imposedEchoData: true
  inversionCamera: false
puzzle:
  timingFloor: 0.4
    components:
      - { name: PlayerStart, type: PlayerStart, position: [0, 0, -4], scale: [1, 1, 1] }
      - { name: ImposedEcho, type: ImposedEchoData }
    - { name: Door_Final, type: Door, position: [0, 2, 32], scale: [4, 4, 0.4], targetSignals: [ImposedEcho] }
    - { name: LevelExit_Area, type: LevelExit, position: [0, 1.1, 56], customData: "Level_14" }
    - { name: LevelGoal, type: LevelGoal, position: [0, 1.1, 56], customData: "El eco pre‑grabado abre la puerta.|.", targetSignals: [ImposedEcho] }
  wiring:
    - { src: ImposedEcho, dst: Door_Final, gate: AND }
    - { src: ImposedEcho, dst: LevelGoal, gate: COUNT, requiredCount: 1 }
test:
  echoButtonTest: PASS
  timingFloor: 0.4
  expectedSoftlocks: 0
  sequenceBreaks: 0
solution:
  optimal_path:
    - "The pre‑baked echo solution (provided by ImposedEchoData) activates the door and goal without player recording."
---

# LEVEL 13 — Otro grabó por ti. Baila su pieza. (Chapter V — Mastery)

**Archetype**: Mastery — challenges the player with a pre‑recorded echo; no recording capability.

## Design Notes
- `maxEchoes` is set to 0 and `imposedEchoData` is true, disabling player recording.
- `ImposedEchoData` is responsible for feeding the pre‑baked echo solution.
- The player only needs to reach the exit after the door opens.

## Echo Button Test
- The puzzle graph includes the `ImposedEcho` node, satisfying the required Echo press automatically. PASS.

## Failure Modes
- If the pre‑baked solution fails to fire the signal, the door remains locked.
