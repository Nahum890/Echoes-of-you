# LEVEL_SPEC_02.md — Level 02 Blueprint Specification (Aula Silenciosa)
## Spec ID: LEVEL-SPEC-02
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 02 ("Aula Silenciosa"). Introduces dual pressure plate coordination across two separated classrooms using Echo persistence.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_02.asset` and runtime level generation.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Experimentation` (Spatial variation of plate holding).
- `Chapter`: Chapter I (Persistencia).

### 5. INPUTS
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`

### 6. OUTPUTS
- Generated Unity scene `Level_02_AulaSilenciosa.unity`.

### 7. RULES

- `[RULE-L02-001]`: **Level Modules**: Level 02 MUST include `SchoolCorridor` (32), `SchoolClassroom_A` (33), `SchoolClassroom_B` (33), and `TransitionSpace` (46).
- `[RULE-L02-002]`: **Lighting Token**: Fog color `#1C2430`, $D_{fog} = 0.008$, Ambient color `#0F141A`.
- `[RULE-L02-003]`: **Camera Profile**: `Discovery` ($FOV = 45.0^\circ$).
- `[RULE-L02-004]`: **Narrative Item**: `Prop_Notebook` (Amber Notebook `#FFBF00`) located in `SchoolClassroom_02`.

### 8. ALGORITHMS

```yaml
level_id: "Level_02"
chapter_id: 1
archetype: "Experimentation"
target_score: 80
modules:
  - id: "mod_01_corridor"
    type: "SchoolCorridor"
    position: [0.0, 0.0, 0.0]
    dimensions: [4.5, 3.2, 18.0]
  - id: "mod_02_classroom_a"
    type: "SchoolClassroom"
    position: [-10.0, 0.0, 18.0]
    dimensions: [10.0, 3.8, 10.0]
  - id: "mod_03_classroom_b"
    type: "SchoolClassroom"
    position: [10.0, 0.0, 18.0]
    dimensions: [10.0, 3.8, 10.0]
puzzle:
  components:
    - type: "PressurePlate"
      position: [-6.0, 0.08, 22.0]
      target_wire: "wire_a"
    - type: "PressurePlate"
      position: [6.0, 0.08, 22.0]
      target_wire: "wire_b"
    - type: "PuzzleCondition"
      logic_gate: "AND"
      target_door: "door_exit"
```

### 9. CONSTRAINTS
- `[CONS-L02-001]`: Prohibido timing window $< 1.0\text{s}$ for plate activation.

### 10. VALIDATION
- `[VAL-L02-001]`: `LevelValidator.cs` asserts `PuzzleCondition` logic gate equals `AND`.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_02.asset`.

### 12. FAILURE CASES
- `[FAIL-L02-001]`: **Unwired Wire Signal**: Wire signal missing. Result: `FAIL-PUZ-02`.

### 13. CROSS REFERENCES
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
