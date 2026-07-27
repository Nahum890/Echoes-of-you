---
level: 15
name: "Tres capítulos en una sola pieza"
archetype: Integration
chapter: V
blueprint:
  echoEnabled: true
  maxEchoes: 2
  maxRecordSeconds: 12
  recordFuture: false
  ambientEchoData: true
  imposedEchoData: false
  inversionCamera: true
puzzle:
  timingFloor: 0.4
    components:
      - { name: PlayerStart, type: PlayerStart, position: [0, 0, -4], scale: [1, 1, 1] }
      - { name: AmbientGhost, type: AmbientEchoData }
      - { name: InversionCam, type: InversionCamera }
      - { name: Plate_Echo_A, type: PressurePlate, position: [-3, 0, 24], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Plate_Echo_B, type: PressurePlate, position: [ 3, 0, 24], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Ghost_Bridge, type: GhostBridge, position: [0, 0, 30] }
    - { name: Door_Final, type: Door, position: [0, 2, 36], scale: [4, 4, 0.4], targetSignals: [Ghost_Bridge] }
    - { name: LevelExit_Area, type: LevelExit, position: [0, 1.1, 56], customData: "MainMenu" }
    - { name: LevelGoal, type: LevelGoal, position: [0, 1.1, 56], customData: "Integración final de todos los mecanismos.|.", targetSignals: [Ghost_Bridge] }
  wiring:
    - { src: Plate_Echo_A, dst: Ghost_Bridge, gate: AND }
    - { src: Plate_Echo_B, dst: Ghost_Bridge, gate: AND }
    - { src: Ghost_Bridge, dst: Door_Final, gate: AND }
    - { src: Ghost_Bridge, dst: LevelGoal, gate: COUNT, requiredCount: 1 }
test:
  echoButtonTest: PASS
  timingFloor: 0.4
  expectedSoftlocks: 0
  sequenceBreaks: 0
solution:
  optimal_path:
    - "Player records two Echoes to activate the GhostBridge, navigates the inverted camera world, and reaches the exit."
---

# LEVEL 15 — Tres capítulos en una sola pieza (Chapter V — Integration)

**Archetype**: Integration — combines ambient ghost, inversion camera, multiple Echo‑only plates, and a GhostBridge into a single comprehensive puzzle.

## Design Notes
- Uses all three Phase 2 features: `AmbientEchoData`, `InversionCamera`, `GhostBridge`.
- Requires two Echo slots (`maxEchoes = 2`).
- The player must manage inverted controls while coordinating Echo actions.

## Echo Button Test
- The combined graph ensures at least one `requiresEcho` node leads to the exit. PASS.

## Failure Modes
- Mis‑timing of Echo recordings, inverted control confusion, or failure of the GhostBridge to solidify.
