# LEVEL_SPEC_05.md — Level 05 Blueprint Specification (Atrio Olvidado)
## Spec ID: LEVEL-SPEC-05
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 05 ("Atrio Olvidado"). Chapter II experimentation featuring multi-level stairwell and records board.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_05.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Experimentation` (Multi-level timing).
- `Chapter`: Chapter II (Coordinación).

### 5. INPUTS
- [BUILDING_FLOW.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/BUILDING_FLOW.md) `[SPEC-103]`

### 6. OUTPUTS
- Generated Unity scene `Level_05_AtrioOlvidado.unity`.

### 7. RULES
- `[RULE-L05-001]`: **Level Modules**: MUST include `SchoolHall` (31) $[12.0, 5.0, 12.0]$ and `SchoolStairwell` (34) $[8.0, 7.6, 8.0]$.
- `[RULE-L05-002]`: **Lighting Token**: Fog color `#2E3024` (Olive), $D_{fog} = 0.010$, Ambient color `#1A1C14`.
- `[RULE-L05-003]`: **Camera Profile**: `Discovery` ($FOV = 45.0^\circ$).
- `[RULE-L05-004]`: **Narrative Item**: `Prop_RecordsBoard` (Amber Board `#FFBF00`) located in `SchoolHall_01`.

### 8. ALGORITHMS

```yaml
level_id: "Level_05"
chapter_id: 2
archetype: "Experimentation"
target_score: 80
modules:
  - id: "mod_01_hall"
    type: "SchoolHall"
    position: [0.0, 0.0, 0.0]
    dimensions: [12.0, 5.0, 12.0]
  - id: "mod_02_stairwell"
    type: "SchoolStairwell"
    position: [0.0, 0.0, 12.0]
    dimensions: [8.0, 7.6, 8.0]
puzzle:
  components:
    - type: "PressurePlate"
      position: [0.0, 0.08, 6.0]
    - type: "Door"
      position: [0.0, 3.8, 16.0]
```

### 9. CONSTRAINTS
- `[CONS-L05-001]`: Prohibido stair risers $> 0.18\text{m}$.

### 10. VALIDATION
- `[VAL-L05-001]`: `LevelValidator.cs` asserts stairwell height equals $7.6\text{f}$.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_05.asset`.

### 12. FAILURE CASES
- `[FAIL-L05-001]`: **Stair Snagging**: Result: `FAIL-SCL-02`.

### 13. CROSS REFERENCES
- [SCALE_GUIDE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SCALE_GUIDE.md) `[SPEC-106]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
