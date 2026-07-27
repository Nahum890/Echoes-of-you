---
level: 6
name: "Tu identidad vuelve al centro"
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
    # Chasm at z ∈ [28, 44]. Floor invisible until Echo walks the ghost-desk chain.
    - { name: GhostBridge_Lyra, type: GhostBridge, position: [0, 0, 36], scale: [10, 0.3, 14] }
    - { name: Plate_Echo_Entry, type: PressurePlate, position: [0, 0, 26], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly", targetSignals: [GhostBridge_Lyra] }
    - { name: Plate_Echo_Exit,  type: PressurePlate, position: [0, 0, 46], scale: [2, 0.12, 2], requiresEcho: true, customData: "EchoOnly" }
    - { name: Door_Triggered,    type: Door, position: [0, 2, 50], scale: [4, 4, 0.4], targetSignals: [Plate_Echo_Exit] }
    - { name: LevelExit_Area,     type: LevelExit, position: [0, 1.1, 56], customData: "Level_07" }
    - { name: LevelGoal,          type: LevelGoal, position: [0, 1.1, 56], customData: "El puente existe solo cuando lo recuerdas.|Lo que el eco pisa es verdad.|.", targetSignals: [Plate_Echo_Exit] }
  wiring:
    - { src: Plate_Echo_Entry, dst: GhostBridge_Lyra, gate: AND }  # plate is the PuzzleSignal-satisfying source for the GhostBridge
test:
  echoButtonTest: PASS
  timingFloor: 0.4
  expectedSoftlocks: 0
  sequenceBreaks: 0
solution:
  optimal_path:
    - "Player at [0,0,-4] walks to Plate_Echo_Entry at [0,0,26]. Cannot press Plate_Echo_Entry (EchoOnly)."
    - "Player records an Echo path: from [0,0,16] walk to [0,0,26] (cross Plate_Echo_Entry), then continue forward across the void — distance 14 units all the way to Plate_Echo_Exit at [0,0,46]. Total recording ~6s."
    - "Player releases Record. Echo spawns at [0,0,16]. Latency 0.8s, Echo begins walking."
    - "Echo enters Plate_Echo_Entry → Plate.IsPressed=true → PuzzleSignal.SetSatisfied=true → GhostBridge_Lyra bridgeCollider.enabled=true → Bridge visible (translucent → solid). Player can now RUN ACROSS following the Echo."
    - "Echo continues forward across the bridge (residual frames drive motion until end). Player runs BEHIND the Echo ON the bridge (same path + offset) — they need to be INSIDE the residual-time frame, which lasts 2.5s after playback ends."
    - "Echo reaches Plate_Echo_Exit at t≈6+1=7s → Door_Triggered opens; LevelGoal fires. Player reaches LevelExit."
    failure_modes:
      echo_does_not_cross: "If Player records not-far-enough, Echo never reaches Plate_Echo_Exit, Door stays closed. Player remains trapped on entry side (GhostBridge releases 2.5s after Echo stops). Ask yourself: why didn't you make it grow up to be enough?"
      too_far: "Echo overshoots far side; Bridge stays active but ghost crosses before player can catch up — visible fail."
---

# LEVEL 06 — Tu identidad vuelve al centro (Chapter III — Confianza)

**Archetype**: Twist — Chapter III opens with the player's prior assumption "I am the actor" inverted. Here, the player follows their own past-self across the bridge.

## GhostBridge Behavior (already-implemented at GhostBridge.cs)
The GhostBridge is active iff its referenced PuzzleSignal is `IsSatisfied == true`. Once active, the bridge's collider + visual mesh turn on. When the signal falls back to false (PuzzleSignal default `accumulateOnce=true` so it stays satisfied… unless overridden — solution: set signal's `accumulateOnce=false` so it can reset if plate releases).

For this level, Phase 2 must add the **reset** plumbing: the bridge is tied to `Plate_Echo_Entry.IsPressed`, not to a `PuzzleSignal` directly. The `$WireSignals` extension (8b) will introduce a `GhostBridge→PressurePlate` direct wiring mode.

## The Twist
- The player's prior expectation: "the Echo acts alone; I observe." Here, the player must follow.
- The bridge only exists while Echo stands on the entry plate; if Echo keeps walking past the entry (which the recording demands), the bridge eventually disappears again — UNLESS another "Echo enters plate B" path is taken.
- For maximum effect: the bridge is "highly transient" — your recording needs to press plate A AND keep walking across the bridge to plate B WITHIN the residual time of plate A's release. But if the bridge disappears, the Echo falls.

## Sync Floor
- Plates is single-input → not subject to synchro floor.
- But timingFloor still applies as the project-wide floor (validator flagging floor = 0.4 minimum).

## Echo Button Test
- LevelExit ← Door_Triggered ← Plate_Echo_Exit (requiresEcho=true). PASS.

## Failure Modes
- Player records Echo but doesn't follow: Bridge opens for Echo, Echo crosses, bridge closes. Player has no path; plate_a residual fades. Visible.
- Player enters Bridge but too slow: Bridge's collider disables when the door's PuzzleSignal SetSatisfied=false with accumulateOnce=false; Player falls into chasm (KillVolume catches).
- Player mistimes release; Echo falls off bridge before reaching exit side: bridge opens | Echo dies in chasm | Plate_Echo_Exit never presses | door remains closed. Re-do recording.
