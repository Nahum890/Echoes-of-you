# LEVEL_SPEC_12.md — Level 12 Blueprint Specification (Trampa de Conflicto)
## Spec ID: LEVEL-SPEC-12
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 12 ("Trampa de Conflicto"). Chapter V entry introducing `EchoConflictTrap` hazards penalty.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_12.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Twist` (Conflict hazard subversion).
- `Chapter`: Chapter V (Consecuencia).

### 5. INPUTS
- [ECHO_PRIMITIVE_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_PRIMITIVE_SPEC.md) `[SPEC-111]`

### 6. OUTPUTS
- Generated Unity scene `Level_12_TrampaDeConflicto.unity`.

### 7. RULES
- `[RULE-L12-001]`: **Level Modules**: MUST include `SchoolOffice` (44) $[8.0, 3.8, 8.0]$ and `ConflictTrap` (18).
- `[RULE-L12-002]`: **Lighting Token**: Fog color `#1A1020` (Void Purple), $D_{fog} = 0.020$, Ambient color `#0C0810`.
- `[RULE-L12-003]`: **Camera Profile**: `Suspense` ($FOV = 45.0^\circ$).
- `[RULE-L12-004]`: **Narrative Item**: `Prop_MirrorFrameAmber` (Amber Mirror `#FFBF00`) in `SchoolOffice_01`.

### 8. ALGORITHMS

```yaml
level_id: "Level_12"
chapter_id: 5
archetype: "Twist"
target_score: 80
modules:
  - id: "mod_01_office"
    type: "SchoolOffice"
    position: [0.0, 0.0, 0.0]
    dimensions: [8.0, 3.8, 8.0]
    parameters:
      files_state: "open"
      clock_state: "stopped"
puzzle:
  components:
    - type: "ConflictTrap"
      position: [0.0, 0.0, 4.0]
      parameters:
        penalty: "soft_reset"
    - type: "Door"
      position: [0.0, 0.0, 8.0]
```

### 9. CONSTRAINTS
- `[CONS-L12-001]`: Prohibido conflict traps without red pulse visual material (`#FF0000`).

### 10. VALIDATION
- `[VAL-L12-001]`: `LevelValidator.cs` asserts `EchoConflictTrap` component listener exists.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_12.asset`.

### 12. FAILURE CASES
- `[FAIL-L12-001]`: **Trap SoftReset Missing**: Result: `FAIL-PRM-01`.

### 13. CROSS REFERENCES
- [ECHO_PRIMITIVE_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_PRIMITIVE_SPEC.md) `[SPEC-111]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
