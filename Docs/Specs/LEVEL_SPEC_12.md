---
level: 12
name: "Cada eco cierra una puerta en el otro"
archetype: Combination
chapter: IV
blueprint:
  echoEnabled: true
  maxEchoes: 2
  maxRecordSeconds: 12
  recordFuture: false
  ambientEchoData: false
  imposedEchoData: false
  inversionCamera: false
puzzle:
  timingFloor: 0.4
    components:
      - { name: PlayerStart, type: PlayerStart, position: [0, 0, -4], scale: [1, 1, 1] }
      - { name: Plate_Echo_A, type: PressurePlate, position: [-3, 0, 24], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Plate_Echo_B, type: PressurePlate, position: [ 3, 0, 24], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Ghost_Bridge, type: GhostBridge, position: [0, 0, 30], customData: "" }
    - { name: Door_Final, type: Door, position: [0, 2, 36], scale: [4, 4, 0.4], targetSignals: [Ghost_Bridge] }
    - { name: LevelExit_Area, type: LevelExit, position: [0, 1.1, 56], customData: "Level_13" }
    - { name: LevelGoal, type: LevelGoal, position: [0, 1.1, 56], customData: "Los ecos cierran puertas simultáneas.|.", targetSignals: [Ghost_Bridge] }
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
    - "Record two Echoes (A and B) that simultaneously activate the GhostBridge, opening the final door."
---

# LEVEL 12 — Cada eco cierra una puerta en el otro (Chapter IV — Combination)

**Archetype**: Combination — combines two Echo‑only plates to trigger a GhostBridge.

## Design Notes
- Requires two Echo slots (`maxEchoes = 2`).
- The GhostBridge becomes solid only when both plates are pressed.
- The player cannot press the plates directly due to `EchoOnly`.

## Echo Button Test
- Path: LevelExit ← Door_Final ← Ghost_Bridge ← (Plate_Echo_A & Plate_Echo_B). PASS.

## Failure Modes
- Recording only one Echo leaves the bridge inactive.
- Timing mismatch causing the bridge to deactivate before the door opens.
