---
level: 14
name: "Sin pasado, solo espejo"
archetype: Acceptance
chapter: V
blueprint:
  echoEnabled: false
  maxEchoes: 0
  maxRecordSeconds: 12
  recordFuture: false
  ambientEchoData: false
  imposedEchoData: false
  inversionCamera: true
puzzle:
  timingFloor: 0.4
    components:
      - { name: PlayerStart, type: PlayerStart, position: [0, 0, -4], scale: [1, 1, 1] }
      - { name: InversionCam, type: InversionCamera }
    - { name: Door_Final, type: Door, position: [0, 2, 32], scale: [4, 4, 0.4] }
    - { name: LevelExit_Area, type: LevelExit, position: [0, 1.1, 56], customData: "Level_15" }
    - { name: LevelGoal, type: LevelGoal, position: [0, 1.1, 56], customData: "Sin eco, solo espejo abre la salida.|.", targetSignals: [] }
  wiring: []
test:
  echoButtonTest: PASS
  timingFloor: 0.4
  expectedSoftlocks: 0
  sequenceBreaks: 0
solution:
  optimal_path:
    - "Player navigates the mirrored world using inverted controls to reach the exit."
---

# LEVEL 14 — Sin pasado, solo espejo (Chapter V — Acceptance)

**Archetype**: Acceptance — eliminates echo mechanics; the player must rely on mirrored camera and inverted controls.

## Design Notes
- `echoEnabled` is false and `maxEchoes` is 0, removing any recording ability.
- `InversionCamera` component mirrors the view and flips horizontal input.
- The puzzle consists of a simple door that opens automatically (no signal needed).

## Echo Button Test
- No Echo is required; the test passes by default.

## Failure Modes
- Player may become confused by inverted controls; no additional failure feedback is provided.
