# LEVEL_SPEC_13.md — Level 13 Blueprint Specification (Paradoja Temporal)
## Spec ID: LEVEL-SPEC-13
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 13 ("Paradoja Temporal"). Chapter V mastery integrating library shelves and temporal bridges.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_13.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Mastery` (Chapter V synthesis).
- `Chapter`: Chapter V (Consecuencia).

### 5. INPUTS
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`

### 6. OUTPUTS
- Generated Unity scene `Level_13_ParadojaTemporal.unity`.

### 7. RULES
- `[RULE-L13-001]`: **Level Modules**: MUST include `SchoolLibrary` (37) $[14.0, 4.5, 16.0]$ and `TemporalBridge` (22).
- `[RULE-L13-002]`: **Lighting Token**: Fog color `#1A1020` (Void Purple), $D_{fog} = 0.020$, Ambient color `#0C0810`.
- `[RULE-L13-003]`: **Camera Profile**: `Puzzle` ($FOV = 45.0^\circ$).
- `[RULE-L13-004]`: **Narrative Item**: `Prop_HourglassAmber` (Amber Hourglass `#FFBF00`) in `SchoolLibrary_01`.

### 8. ALGORITHMS

```yaml
level_id: "Level_13"
chapter_id: 5
archetype: "Mastery"
target_score: 80
modules:
  - id: "mod_01_library"
    type: "SchoolLibrary"
    position: [0.0, 0.0, 0.0]
    dimensions: [14.0, 4.5, 16.0]
    parameters:
      shelves_count: 6
      warm_lighting: true
puzzle:
  components:
    - type: "TemporalBridge"
      position: [0.0, 0.0, 8.0]
    - type: "LevelGoal"
      position: [0.0, 0.0, 16.0]
```

### 9. CONSTRAINTS
- `[CONS-L13-001]`: Prohibido shelf clearance $< 1.2\text{m}$.

### 10. VALIDATION
- `[VAL-L13-001]`: `LevelValidator.cs` asserts library dimensions $[14.0, 4.5, 16.0]$.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_13.asset`.

### 12. FAILURE CASES
- `[FAIL-L13-001]`: **Shelf Clearance Deficit**: Result: `FAIL-NAV-01`.

### 13. CROSS REFERENCES
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
