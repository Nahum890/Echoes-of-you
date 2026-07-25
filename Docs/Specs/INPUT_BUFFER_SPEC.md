# INPUT_BUFFER_SPEC.md — Frame-Rate-Independent Input Buffering Specifications
## Spec ID: SPEC-129
## Version: 2.0 (AI-Executable — framerate-independent)

---

### 1. PURPOSE
Defines exact millisecond-based input buffering windows, coyote time tolerances, and key hold confirmation durations for responsive player controls. All values use `Time.time` float comparisons to guarantee frame-rate independence at 30, 60, 120, and 240 Hz.

### 2. SCOPE
Applies to `InputBufferManager.cs` and player locomotion/interaction handlers in `Assets/Scripts/Player/`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `INPUT_ACTION_MAPS.md` (`SPEC-118`).

### 4. DEFINITIONS
- `Input Buffer Window`: Pre-landing grace period of **100ms** where jump inputs are queued and executed upon touching ground. Implementation: `(Time.time - lastJumpPressTime) <= 0.100f`.
- `Coyote Time`: Post-edge grace period of **120ms** permitting jump initiation after walking off a ledge. Implementation: `(Time.time - lastGroundedTime) <= 0.120f`.

### 5. INPUTS
- [INPUT_ACTION_MAPS.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/INPUT_ACTION_MAPS.md) `[SPEC-118]`
- [INPUT_BUFFER_SPEC.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/INPUT_BUFFER_SPEC.yaml) `[SPEC-129]`

### 6. OUTPUTS
- Queue management logic for player inputs.

### 7. RULES
- `[RULE-INP-001]`: **Jump Buffer Window**: Jump input MUST buffer for up to **100ms** prior to grounding. Implementation: `(Time.time - lastJumpPressTime) <= 0.100f`.
- `[RULE-INP-002]`: **Coyote Grace Period**: Player MUST be allowed to jump for up to **120ms** after losing grounded state. Implementation: `(Time.time - lastGroundedTime) <= 0.120f`.
- `[RULE-INP-003]`: **SoftReset Hold Time**: Soft reset MUST require continuous button hold for **500ms** to prevent accidental trigger. Implementation: `(Time.time - softResetHoldStart) >= 0.500f`.
- `[RULE-INP-004]`: **Framerate Independence**: Input buffer NEVER uses frame counters. All comparisons use `Time.time` (seconds). See `FRAME_RATE_INDEPENDENCE_SPEC.yaml` (SPEC-142).

### 8. ALGORITHMS
See canonical YAML schema at `Docs/Specs/INPUT_BUFFER_SPEC.yaml`.

### 9. CONSTRAINTS
- `[CONS-INP-001]`: Prohibido hardcoding frame counts outside `INPUT_BUFFER_SPEC.yaml`.

### 10. VALIDATION
- `[VAL-INP-001]`: Unit tests assert jump buffer accepts inputs up to frame -6 and discards frame -7.

### 11. EXAMPLES
- `INPUT_BUFFER_SPEC.yaml` schema definition.

### 12. FAILURE CASES
- `[FAIL-INP-001]`: **Dropped Jump**: Input executed 2 frames prior to ground contact dropped due to missing buffer queue. Result: `FAIL-INP-01`.

### 13. CROSS REFERENCES
- [LOCOMOTION_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LOCOMOTION_SPEC.md) `[SPEC-113]`

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Created canonical SPEC-129 for Input Buffer parameters.
