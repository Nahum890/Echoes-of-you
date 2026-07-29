---
level: 2
name: "Dos presiones"
archetype: Experimentation
chapter: I
blueprint:
  echoEnabled: true
  maxEchoes: 1
  maxRecordSeconds: 12
  recordFuture: false
  ambientEchoData: false
  imposedEchoData: false
  inversionCamera: false
puzzle:
  timingFloor: 0.4       # ENFORCED — two plates must arrive simultaneously
  components:
    - { name: Plate_Player_A,   type: PressurePlate, position: [-3, 0, 25], scale: [2, 0.12, 2], requiresEcho: false }
    - { name: Plate_Echo_B,     type: PressurePlate, position: [ 3, 0, 25], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Door_Sync,         type: Door,          position: [ 0, 2, 32], scale: [4, 4, 0.4], targetSignals: [Plate_Player_A, Plate_Echo_B] }
    - { name: LevelExit_Area,     type: LevelExit,      position: [ 0, 1.1, 56], customData: "Level_03" }
    - { name: LevelGoal,          type: LevelGoal,      position: [ 0, 1.1, 56], customData: "Dos presiones sostienen.|Tú y tu eco son uno.|.", targetSignals: [Plate_Echo_B] }
  wiring:
    - { src: [Plate_Player_A, Plate_Echo_B], dst: Door_Sync, gate: AND }
test:
  echoButtonTest: PASS
  timingFloor: 0.4
  expectedSoftlocks: 0
  sequenceBreaks: 0
solution:
  optimal_path:
    - "Player walks to [-3, 0, 25] and steps onto Plate_Player_A"
    - "Player begins recording an Echo at [-3, 0, 25] — recording forward toward [3, 0, 25] for 3s"
    - "Player releases Record; Echo spawns at [-3, 0, 25] and walks 6m right (+z|+x) in ≤3s (latency 0.8s start-up)"
    - "Player must stay on Plate_Player_A the WHOLE time Echo is on Plate_Echo_B — within ~2.2s total"
    - "Both plates report IsPressed=true in same FixedUpdate frame → door opens"
    - "Player proceeds to LevelExit"
---

# LEVEL 02 — Dos presiones (Chapter I)

**Archetype**: Experimentation — the player has already learned "Echo opens things" (L01). Now they learn "Echo is one actor in a multi-actor puzzle".

## Sync Floor
- `PuzzleWire.Connection.logic = AND` requires both plates `IsPressed = true` in the SAME `FixedUpdate` frame.
- Real arrival delta between Plate_Player_A (player steps) and Plate_Echo_B (Echo latency + travel) typically 2–3s. Sync floor is observed.
- Validator will Assert `timingFloor ≥ 0.4` AND `PuzzleWire.connection.responseDelay >= 0.4 when sync gates have >= 2 inputs`. No DELAY on this AND connection; sync floor enforced at evaluation-only.

## Echo Button Test
- Plate_Player_A: requiresEcho = false (player-pressed)
- Plate_Echo_B: requiresEcho = true (EchoOnly)
- Backward BFS: Door_Sync → Plate_Echo_B → no further upstreams → exit path includes requiresEcho=true. PASS.

## Failure Modes
- Player steps off Plate_Player_A too early → Plate_Echo_B still glowing but AND false, door closes/cannot open. Legible fail: door visibly closes.
- Player records Echo but it spawns and walks into the door opening in front of player — Echo has no path to Plate_Echo_B because door not open yet. Redesign check: position plates so Echo's recorded path naturally walks	left-of-door (i.e. plate position [3,0,25] is LEFT of door [0,2,32] when facing +Z — verified).
