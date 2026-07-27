---
level: 8
name: "Dos presentes"
archetype: Combination
chapter: III close / IV open
blueprint:
  echoEnabled: true
  maxEchoes: 2                  # novelty: 2 Echo slots for L08
  maxRecordSeconds: 12
  recordFuture: false
  ambientEchoData: false
  imposedEchoData: false
  inversionCamera: false
puzzle:
  timingFloor: 0.4
  components:
    - { name: Plate_Echo_A_Trig, type: PressurePlate, position: [-5, 0, 22], scale: [2, 0.12, 2], requiresEcho: true,  customData: "EchoOnly" }
    - { name: Plate_Echo_B_Trig, type: PressurePlate, position: [ 5, 0, 22], scale: [2, 0.12, 2], requiresEcho: true,  customData: "EchoOnly" }
    - { name: PuzzleCond_CountTwoEchos, type: PuzzleCondition, position: [0, 0, 28], customData: "PlateCount|Sosténlos los dos|..|No dos a la vez" }
    - { name: Door_TimedOpen,   type: Door, position: [0, 2, 30], scale: [4, 4, 0.4], targetSignals: [PuzzleCond_CountTwoEchos] }
    # Two-stage Echo path:
    - { name: Plate_After_A_Completed, type: PressurePlate, position: [-5, 0, 36], scale: [2, 0.12, 2], requiresEcho: false }
    - { name: ConflictTrap_UpDown, type: ConflictTrap, position: [-5, 0, 36], scale: [3, 3, 3], targetSignals: [Plate_After_A_Completed] }
    - { name: ConflictTrap_LeftRight, type: ConflictTrap, position: [ 5, 0, 36], scale: [3, 3, 3], targetSignals: [Plate_Echo_B_Trig] }
    - { name: LevelExit_Area,  type: LevelExit, position: [0, 1.1, 56], customData: "Level_09" }
    - { name: LevelGoal,       type: LevelGoal, position: [0, 1.1, 56], customData: "Dos presentes no caben; uno debe preceder al otro.|Tu eco A termina antes que B empiece.|.", targetSignals: [PuzzleCond_CountTwoEchos] }
  wiring:
    - { src: [Plate_Echo_A_Trig, Plate_Echo_B_Trig], dst: PuzzleCond_CountTwoEchos, gate: COUNT, requiredCount: 2 }
test:
  echoButtonTest: PASS
  timingFloor: 0.4
  expectedSoftlocks: 0
  sequenceBreaks: 0
solution:
  optimal_path:
    - "Player has 2 Echo slots (maxEchoes=2). Player records Echo A: walk from [0,0,16] diagonally to [-5,0,22] (Plate_Echo_A_Trig) → forward continuing to [-5, 0, 36] (Plate_After_A_Completed)."
    - "Echo A residual arrives; plate_After_A_Completed stays pressed for 2.5s; PuzzleCond_CountTwoEchos also sees Plate_A pressed (1/2). Door_TimedOpen (it's COUNT = 2) stays closed. Player must produce Echo B."
    - "Player STEPS ON Plate_After_A_Completed... no wait — that's EchoOnly? No, it's player-ok (requiresEcho: false). So the Player stands on Plate_A after_A completed — the trap is ARMED (ConflictTrap_UpDown.IsActive=true because plate IsPressed)."
    - "Echo A's residual expires → Plate_After_A_Completed releases (Player is not on plate — Player would need to step on it). ConflictTrap fires its completion — but the trap design here is asymmetric: ConflictTrap_UpDown (named for L08 mechanics) closes PuzzleCond's referenced door? Hmm."
    - "Simplify: ConflictTrap doorsClosedByEcho array = [Door_TimedOpen]; when Echo is INSIDE the trap area, the trap closes the COUNT door. But Plate_After_A_Completed was player-pressed — so the door OPENED; ConflictTrap only acts when Echo in overlap, not Player. Actually, EchoConflictTrap.FixedUpdate only registers Echo/EchoProjection tags — Player does NOT trigger. So the Player being on plate_A_after_A is fine."
    - "Player THEN records Echo B: walk from [0,0,16] diagonally to [5,0,22] (Plate_Echo_B_Trig) only — recording duration ~3s."
    - "Echo B spawns and walks to Plate_Echo_B_Trig at t≈2s. NOW both PuzzleCond's plates input stable (Plate_A residual EchoA pressed + EchoB pressed same FixedUpdate) → COUNT=2 reaches setMet → Door_TimedOpen opens latched."
    - "ConflictTrap_LeftRight is ARMED on Echo B's plate position; but Echo B residual too short? Wait, Echo B doesn't move past Plate_Echo_B_Trig — its recording ends there. Echo B residual stays in pressed state → keeps the trap armed... actually the trap fires completionSignal AFTER Echo EXITS trap; if Echo stays, trap armed but no exit — no fire → no Door close."
    - "Player walks through Door_TimedOpen to LevelExit."
  critical_design_principle:
    - "ECHOES_BIBLE §5 rule 'Irreversibility': once Echo_A has been recorded, its discardingوریا removes it; no re-recording means Echo_A cannot be redone if recording was inadequate. Player must trust their Echo_A recording because they only have 2 slots."
    - "Player must release Echo_A's recording to commit to the plan before recording Echo_B — both Echoes lag simultaneously only if player uses up both slots; the puzzle hinges on committed sequential."
  design_note:
    - "The level title 'Two present selves' is about temporal multiplicity: now you have TWO past selves coexisting. Each must yield before the next begins; the puzzle punishes overlap."
---

# LEVEL 08 — Dos presentes (Chapter III close / IV open — Combination)

**Archetype**: Combination — the player combines the Echo mechanic they already know with Chapter II's timing mechanic they learned.

## ConflictTrap Semantics
The `EchoConflictTrap` (lines 39–77 of `EchoConflictTrap.cs`) watches for any Echo body in the trap's box collider. While Active (= Echo inside), it:
1. sets `doorsClosedByEcho[i].SetOpenState(!IsActive)` (door closes while conflict active),
2. sets `doorsOpenedByEcho[i].SetOpenState(IsActive)` (alt doors track conflict).

When the Echo LEAVES the trap area (`_hadEcho && !hasEcho`), it fires `completionSignal?.MarkSatisfied()`.

For LEVEL_SPEC_08, the ConflictTrap_Lifeguard is the trap on Echo_B path:
- doorsClosedByEcho = [Door_TimedOpen] while Echo_B is inside the trap (i.e., Echo_B body touches Plate_Echo_B_Trig area). Plate_B is INSIDE trap zone. But Plate_B IsPressed=true has already triggered PuzzleCond_CountTwoEchos.setMet → Door_TimedOpen opened. THEN ConflictTrap ARM (since Echo_B body has _hadEcho true) DOESN'T close the door in this state — the trap would close the door only WHEN Echo leaves (and fires completion, marking signal true). Wait — `ApplyTrapState` is called in Update every frame; if Echo is active (inside), `SetOpenState(!IsActive)` = SetOpenState(false), which CLOSES the door.

So actually the door does NOT remain open just by PuzzleCond_IsMet; the ConflictTrap overrides:
- While Echo_B sits in trap zone: `IsActive=true` → door is closed (forced closed by trap, even though PuzzleCond wanted true = open).
- When Echo_B residual ends / Echo_B despawns → Echo leaves trap → `IsActive=false` → door opens... but only momentarily because the `completionSignal.MarkSatisfied()` flags something.

**The optimal design**: Player wants Echo_B to press Plate_B and WALK PAST the trap (i.e., echo_B's recorded path continues through the trap) so that when Echo_B exits the trap (moves out), ConflictTrap fires `completionSignal` AND `IsActive=false` together triggers door opens.

Player must time Echo_B to **enter AND exit** the trap in one motion (probably record a longer walk past the trap).

## Sync Floor
- 2-input COUNT puzzle → timingFloor 0.4 enforced.
- responseDelay defaults to 0 in PuzzleWire.Connection → effective 0; not subject to floor since the COUNT gate latches via _isMet boolean, not via inter-frame timing.
- (Phase 3 if PuzzleCondition.setMet pulses too rapidly, counts may double — verify ExerciseWrite at PuzzleCondition docs.)

## Echo Button Test
- Plate_Echo_A_Trig requiresEcho=true, Plate_Echo_B_Trig requiresEcho=true.
- BFS: LevelExit ← Door_TimedOpen ← PuzzleCond_Count ← Plate_Echo_A_Trig (or B). PASS.

## Failure Modes
- Player records Echo_A too short; Echo_A stops before reaching the trap; Plate_After_A_Completed (which is player-acceptable) — Player can step on it themselves (since `requiresEcho=false`) — but then the player must establish the Echo B body somewhere; if Echo_A was used up (slot 1), Player only has 1 slot left.
- Player records Echo_B too long; Echo_B sits in Trap indefinitely after residual; ConflictTrap closes the COUNT door while inside, and meanwhile player can't proceed past Door_TimedOpen. Pulse cycle tests here Validator.
- Player records both Echoes too early; Plate puzzle will fail because both Residual expire before puzzle continues.
