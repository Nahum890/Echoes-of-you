# MODULE_LIBRARY.md — ModuleType → Factory Method & Data Schema Mapping
## Spec ID: SPEC-004B
## Version: 1.0 (AI-Executable)

---

### 1. PURPOSE
Maps each `ModuleType` enum value to its `EchoesModuleFactory.Make*` method, valid `customData` keys, and `targetSignals` schema for puzzle wiring in *Echoes of You 2.0*.

### 2. SCOPE
Applies to `EchoesModuleFactory.cs`, `LevelBlueprint` asset generation, and puzzle signal wiring validation.

### 3. AUTHORITY
Level 3 (Declarative Specs). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ROOM_LIBRARY.md` (`SPEC-004`).

### 4. DEFINITIONS
- `ModuleType`: C# enum representing structural template (indices 0–47).
- `customData`: Semicolon-separated key-value string passed to factory method.
- `targetSignals`: Array of signal IDs this module can receive/emit for puzzle logic.

### 5. INPUTS
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`
- [SIGNAL_CIRCUIT_SCHEMA.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/gameplay/SIGNAL_CIRCUIT_SCHEMA.yaml) `[SPEC-EXEC-SIG-001]`

### 6. OUTPUTS
- Factory method bindings in `EchoesModuleFactory.cs`.
- Validation rules for `LevelValidator.cs`.

### 7. RULES
- `[RULE-MOD-001]`: **Factory Method Binding** — Each `ModuleType` MUST map to exactly one `EchoesModuleFactory.Make*` method.
- `[RULE-MOD-002]`: **CustomData Schema** — Valid keys for each module's `customData` MUST match Table 8.1. Invalid keys generate `FAIL-MOD-01`.
- `[RULE-MOD-003]`: **Signal Compatibility** — `targetSignals` for puzzle modules MUST reference valid signal IDs from `SIGNAL_CIRCUIT_SCHEMA.yaml`.

### 8. ALGORITHMS

#### Table 8.1: ModuleType → Factory Method & Data Schema

| Enum Index | ModuleType | Factory Method | Valid customData Keys | targetSignals Schema |
|---|---|---|---|---|
| 0 | `StandardPlatform` | `MakeStandardPlatform` | `style` (enum: concrete, metal, wood) | — |
| 1 | `BridgePlatform` | `MakeBridgePlatform` | `activator` (enum: echo, player, auto) | `signal_out: bridge_activate` |
| 2 | `ElevatorPlatform` | `MakeElevatorPlatform` | `travel_y` (float, m), `speed` (float, m/s) | `signal_in: elevator_call`, `signal_out: elevator_arrived` |
| 3 | `VanishingPlatform` | `MakeVanishingPlatform` | `delay_s` (float) | `signal_in: vanish_trigger` |
| 4 | `PressurePlate` | `MakePressurePlate` | `latch` (bool), `requires_echo` (bool) | `signal_out: plate_pressed`, `signal_out: plate_released` |
| 5 | `Door` | `MakeDoor` | `locked` (bool), `key_id` (string) | `signal_in: unlock`, `signal_out: opened` |
| 6 | `LevelExit` | `MakeLevelExit` | `target_level` (int), `requires_echo` (bool) | `signal_in: activate_exit` |
| 7 | `PlayerStart` | `MakePlayerStart` | `spawn_facing` (enum: N, E, S, W) | — |
| 8 | `MovingPlatform` | `MakeMovingPlatform` | `axis` (enum: x, z), `distance` (float, m), `speed` (float, m/s) | `signal_in: platform_toggle` |
| 9 | `RotatingPlatform` | `MakeRotatingPlatform` | `speed_deg_s` (float) | `signal_in: rotate_toggle` |
| 10 | `TiltedPlatform` | `MakeTiltedPlatform` | `tilt_deg` (float) | — |
| 11 | `DisappearingBridge` | `MakeDisappearingBridge` | `duration_s` (float) | `signal_in: bridge_trigger` |
| 12 | `CheckpointTrigger` | `MakeCheckpointTrigger` | `save_state` (bool) | `signal_out: checkpoint_reached` |
| 13 | `LevelGoal` | `MakeLevelGoal` | `memory_unlock` (bool) | `signal_in: goal_activate`, `signal_out: level_complete` |
| 14 | `TutorialMarker` | `MakeTutorialMarker` | `hud_prompt` (string) | — |
| 15 | `PuzzleSignal` | `MakePuzzleSignal` | `signal_type` (enum: wire, logic, timer) | `signal_in: *`, `signal_out: *` |
| 16 | `PuzzleCondition` | `MakePuzzleCondition` | `logic_gate` (enum: AND, OR, NOT, XOR) | `signal_in: *`, `signal_out: *` |
| 17 | `HazardField` | `MakeHazardField` | `dissolve_echo` (bool) | `signal_in: hazard_toggle` |
| 18 | `ConflictTrap` | `MakeConflictTrap` | `penalty` (enum: soft_reset, hard_reset) | `signal_in: trap_trigger` |
| 19 | `GravitationalSwitch` | `MakeGravitationalSwitch` | `invert_y` (bool) | `signal_in: gravity_toggle`, `signal_out: gravity_inverted` |
| 20 | `EchoRecorderSlot` | `MakeEchoRecorderSlot` | `slot_index` (int: 0–2) | `signal_out: recording_start`, `signal_out: recording_end` |
| 21 | `KineticBlockSlot` | `MakeKineticBlockSlot` | `target_block` (int) | `signal_in: block_place`, `signal_out: block_placed` |
| 22 | `TemporalBridge` | `MakeTemporalBridge` | `visible_during_echo` (bool) | `signal_in: temporal_sync` |
| 23 | `GhostPlatform` | `MakeGhostPlatform` | `solid_during_echo` (bool) | `signal_in: ghost_toggle` |
| 24 | `ShieldFieldZone` | `MakeShieldFieldZone` | `block_hazard` (bool) | `signal_in: shield_toggle` |
| 25 | `FragmentBurstZone` | `MakeFragmentBurstZone` | `burst_count` (int) | `signal_out: burst_trigger` |
| 26 | `KineticPushBlock` | `MakeKineticPushBlock` | `mass_kg` (float) | `signal_in: block_push` |
| 27 | `WeightPlate` | `MakeWeightPlate` | `required_mass_kg` (float) | `signal_out: weight_met` |
| 28 | `TimedGate` | `MakeTimedGate` | `open_time_s` (float) | `signal_in: gate_trigger`, `signal_out: gate_open` |
| 29 | `ResonanceField` | `MakeResonanceField` | `echo_multiplier` (float) | `signal_in: resonance_activate` |
| 30 | `PortalThreshold` | `MakePortalThreshold` | `target_zone` (string) | `signal_in: portal_activate` |
| 31 | `SchoolHall` | `MakeSchoolHall` | `color` (enum: amber, cold, neutral), `fog` (float) | — |
| 32 | `SchoolCorridor` | `MakeSchoolCorridor` | `lockers` (enum: left, right, both, none), `length` (float, m) | — |
| 33 | `SchoolClassroom` | `MakeSchoolClassroom` | `desks` (string: "NxM"), `chaotic` (bool) | — |
| 34 | `SchoolStairwell` | `MakeSchoolStairwell` | `floors` (int), `style` (enum: concrete, metal) | — |
| 35 | `SchoolBathroom` | `MakeSchoolBathroom` | `mirrors` (enum: broken, intact, none) | — |
| 36 | `SchoolStaffRoom` | `MakeSchoolStaffRoom` | `coats` (int), `coffee` (bool) | — |
| 37 | `SchoolLibrary` | `MakeSchoolLibrary` | `shelves` (int), `warm` (bool) | — |
| 38 | `SchoolCourtyard` | `MakeSchoolCourtyard` | `tree` (enum: center, none), `fences` (bool) | — |
| 39 | `SchoolGym` | `MakeSchoolGym` | `bleachers` (enum: left, right, both), `balls` (int) | — |
| 40 | `SchoolLab` | `MakeSchoolLab` | `beakers` (bool) | — |
| 41 | `SchoolMaintenanceCorridor` | `MakeSchoolMaintenanceCorridor` | `pipes` (enum: ceiling, floor), `lights` (enum: dim, normal) | — |
| 42 | `SchoolEmergencyCorridor` | `MakeSchoolEmergencyCorridor` | `alarm` (enum: red, none), `doors` (enum: barred, open) | — |
| 43 | `SchoolLyraClassroom` | `MakeSchoolLyraClassroom` | `notebook` (enum: amber, normal), `glow` (bool) | — |
| 44 | `SchoolOffice` | `MakeSchoolOffice` | `files` (enum: open, closed), `clock` (enum: stopped, running) | — |
| 45 | `SchoolLiminalClassroom` | `MakeSchoolLiminalClassroom` | `floating_desks` (bool) | — |
| 46 | `TransitionSpace` | `MakeTransitionSpace` | `threshold` (enum: arch, door, none) | — |
| 47 | `SchoolEntrance` | `MakeSchoolEntrance` | `doors` (enum: glass, wood), `arches` (bool) | — |

### 9. CONSTRAINTS
- `[CONS-MOD-001]`: Prohibido adding new ModuleTypes without updating this table and `EchoesModuleFactory.cs`.
- `[CONS-MOD-002]`: Prohibido customData keys not listed in Table 8.1 for given ModuleType.

### 10. VALIDATION
- `[VAL-MOD-001]`: `ExecutableSpecValidator.cs` asserts factory method exists for each ModuleType.
- `[VAL-MOD-002]`: `LevelValidator.cs` asserts customData keys match schema.
- `[VAL-MOD-003]`: Signal wiring validator asserts targetSignals reference valid signal IDs.

### 11. EXAMPLES
```csharp
// ModuleType.ElevatorPlatform (index 2)
var placement = new ModulePlacement {
    moduleType = ModuleType.ElevatorPlatform,
    position = new Vector3(0, 0, 0),
    customData = "travel_y=6.0;speed=2.0"
};
```

### 12. FAILURE CASES
- `[FAIL-MOD-001]`: **Invalid CustomData Key** — `travel_y=6.0;invalid_key=true`. Result: `FAIL-MOD-01`.
- `[FAIL-MOD-002]`: **Missing Factory Method** — New ModuleType without Make* method. Result: `FAIL-MOD-02`.

### 13. CROSS REFERENCES
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`
- [SIGNAL_CIRCUIT_SCHEMA.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/gameplay/SIGNAL_CIRCUIT_SCHEMA.yaml) `[SPEC-EXEC-SIG-001]`
- [CONSTANTS_REGISTRY.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CONSTANTS_REGISTRY.yaml) `[SPEC-124]`

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Initial canonical SPEC-004B for Module Library mapping.

(End of file - total 102 lines)