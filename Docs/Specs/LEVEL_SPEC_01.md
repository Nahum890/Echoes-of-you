---
level: 1
name: "Desorientación"
archetype: Teaching
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
  timingFloor: 0.4
  components:
    - { name: Plate_Echo_A,     type: PressurePlate, position: [0, 0, 28], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Door_Teaching,     type: Door,          position: [0, 2, 32], scale: [4, 4, 0.4], targetSignals: [Plate_Echo_A] }
    - { name: LevelExit_Area,    type: LevelExit,      position: [0, 1.1, 56], customData: "Level_02" }
    - { name: LevelGoal,         type: LevelGoal,      position: [0, 1.1, 56], customData: "Camina hacia el fondo.|Tu eco abre lo que tu cuerpo no puede.|.", targetSignals: [Plate_Echo_A] }
  wiring:
    - { src: Plate_Echo_A, dst: Door_Teaching, gate: AND }
    - { src: Plate_Echo_A, dst: LevelGoal,    gate: COUNT, requiredCount: 1 }
test:
  echoButtonTest: PASS    # BFS path: Plate_Echo_A (requiresEcho=true) → Door_Teaching → LevelExit
  timingFloor: 0.4
  expectedSoftlocks: 0
  sequenceBreaks: 0
solution:
  optimal_path:
    - "Player walks through corridor to position [0, 0, 26]"
    - "Records forward motion for 3s (frames from [0,0,26] → [0,0,28])"
    - "Releases Record — Echo spawns and walks 3m forward, lands on Plate_Echo_A at t≈0.8 (latency)"
    - "Door_Teaching opens (latchOpen=false per factory default)"
    - "Player proceeds through door to LevelExit_Area at [0,1.1,56]"
---

# LEVEL 01 — Desorientación (Chapter I — Persistencia)

**Archetype**: Teaching — first contact with the Echo system. The level introduces exactly one mechanic: "tu eco es tu llave del pasado".

## Design Notes
- The single `PressurePlateEchoOnly` plate is the only way to open `Door_Teaching`. Player stepping on it does NOT open the door — only the Echo does.
- The door is non-latching (`latchOpen: false`); if the Echo steps off, the door closes. This forces the player to learn "record forward timing" so the Echo finishes its motion ON the plate (taking advantage of the 2.5 s residual window).
- No multi-input timing puzzle — sync floor is not in play. The `timingFloor: 0.4` value is set as the global default but isn't enforced here (single input).

## Echo Button Test
The signal graph simplifies to:
```
Plate_Echo_A (requiresEcho=true) ─AND─> Door_Teaching ─> LevelExit
```
BFS from LevelExit backwards succeeds (one node reaches Plate_Echo_A, which has requiresEcho=true). PASS.

## Failure Modes
- Player walks on the plate thinking it opens the door — visible feedback: plate glows (activeColor cyan), but door stays closed. ECHOES_BIBLE §6 "fracaso legible" satisfied via wrong actor on plate.
- Player records too short (<0.2s) — toast "Grabación muy corta" (EchoRecorder line 180).
