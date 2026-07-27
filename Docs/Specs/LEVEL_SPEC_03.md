---
level: 3
name: "El eco que se queda"
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
  timingFloor: 0.4
  components:
    - { name: Plate_Purple_A, type: PressurePlate, position: [-3,  0, 24], scale: [2, 0.12, 2], requiresEcho: true,  customData: "EchoOnly" }
    - { name: Plate_Purple_B, type: PressurePlate, position: [ 3,  0, 24], scale: [2, 0.12, 2], requiresEcho: true,  customData: "EchoOnly" }
    - { name: Plate_Player_C, type: PressurePlate, position: [ 0,  0, 16], scale: [2, 0.12, 2], requiresEcho: false }
    - { name: PuzzleCond_Simultaneous, type: PuzzleCondition, position: [0, 0, 30], customData: "AllPlatesSimultaneous|Sostén los dos púrpuras|Condición cumplida|Uno no basta" }
    - { name: Door_Trapped,   type: Door, position: [0, 2, 32], scale: [4, 4, 0.4], targetSignals: [PuzzleCond_Simultaneous] }
    - { name: Hazard_TrapField, type: HazardField, position: [0, 0, 22], scale: [10, 3, 6], requiresEcho: true, targetSignals: [] }
    - { name: Conflict_ExitTrap, type: ConflictTrap, position: [0, 0, 22], scale: [10, 3, 6], targetSignals: [Door_Trapped] }
    - { name: LevelExit_Area, type: LevelExit, position: [0, 1.1, 56], customData: "Level_04" }
    - { name: LevelGoal, type: LevelGoal, position: [0, 1.1, 56], customData: "Sostén las dos presiones púrpuras.|El eco que se queda te encierra.|Luego sigues.", targetSignals: [PuzzleCond_Simultaneous] }
  wiring:
    - { src: [Plate_Purple_A, Plate_Purple_B], dst: PuzzleCond_Simultaneous, gate: ALLPLATES }
    - { src: PuzzleCond_Simultaneous, dst: Door_Trapped, gate: LATCH }
    - { src: Door_Trapped, dst: Conflict_ExitTrap, gate: AND }
test:
  echoButtonTest: PASS
  timingFloor: 0.4
  expectedSoftlocks: 0
  sequenceBreaks: 0
solution:
  optimal_path:
    - "Player paths to Plate_Player_C at [0, 0, 16] (no obstacle yet)"
    - "Player records an Echo path: start [0,0,16] → [-3,0,24] → [3,0,24] within 5s (latency 0.8s, motion 1splat1plat + 1splatX-secondplatX second)"
    - "Player releases Record. Echo spawned at [0,0,16] begins walking through the recorded path."
    - "Echo reaches Plate_Purple_A at t≈2s (latency 0.8 + travel 1.2). Plate_A glows púrpura."
    - "Echo traverses to Plate_Purple_B by t≈3s. Plate_B glows púrpura. NOW both true in FixedUpdate → PuzzleCond_Simultaneous fires → Door opens."
    - "Conflict_ExitTrap spies Echo on Hazard_TrapField. ARMED — door will slam if Echo exits the HazardField while armed."
    - "Echo residual 2.5s: Echo stops walking at last frame (Plate_Purple_B), retains plate_Held collision for 2.5s."
    - "Player walks through open Door_Trapped, reaches LevelExit before Echo's residual expires (-- so Echo stays on Hazard → ConflictTrap never disarms). Player reaches exit AT t≈6.5s. Total Echo time used: ≈5s. Well within 12s and within the residual safety."
  failure_modes:
    echo_exits_too_late: "Echo walks back off plates — AND fails, door re-closes (latch=false). ConflictTrap disarms but path lost."
    echo_exits_too_early: "Bug — but the ConflictTrap fires AFTER release — the door closes too soon — Player trapped on the wrong side of door. Debug supports ECHOES_BIBLE §6 ('el fracaso legible')."
---

# LEVEL 03 — El eco que se queda (Chapter I)

**Archetype**: Experimentation — the player learns "your Echo's residual is not just flavor; it's the trap."

## Key Mechanic
- Two Echo-only plates in the HazardField.
- Player sits idle on Plate_Player_C (player-only plate) which has no impact on the sync AND — it's just a marker / position-anchor for the recording start.
- The Echo must reach BOTH plates within fixedUpdate window AND stay occupying the HazardField until the player is past the door.
- If the Echo leaves the HazardField (because the recording ends and residual expires **before** the player exits the door), the ConflictTrap slams the door — softlock if player is in the doorframe.

## Sync Floor
- Both púrpura plates must arrive true in same FixedUpdate.
- Echo's recorded path must traverse A → B with NO lag at A; otherwise the first plate will release before the second presses.
- Validator: `timingFloor >= 0.4`. AND with two inputs → responseDelay check (none needed, both latencía-offset by 0.8 s together).

## Echo Button Test
Backward BFS:
- Level-goal ← PuzzleCond_Simultaneous
- PuzzleCond_Simultaneous ← Plate_Purple_A (requiresEcho=true) and Plate_Purple_B (requiresEcho=true)
- PATH includes requiresEcho=true. PASS.

## Failure Modes
Three distinct failure modes per the spec — each visible without toast text:
1. **Echo timing too slow**: plates press in different FixedUpdate frames, AND never true — door stays closed. Plate_Echo visible desaturate indicator (activeColor turns off).
2. **Echo exits HazardField early**: Conflict_ExitTrap flips IsActive=true → IsActive=false transition fires `completionSignal.MarkSatisfied()` (line 73 of EchoConflictTrap.cs); Harmony of this firing slams the door closed (door opens :=false). Player can't progress.
3. **Player dawdles**: Echo's recorded motion ran out of frames at the last plate position; residual kept plate held for 2.5s; ConflictTrap stayed disarmed (no IsActive fall). Then Echo despawns → plates release → AND fails → door closes. Player caught on wrong side.
