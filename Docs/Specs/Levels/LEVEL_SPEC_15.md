# LEVEL_SPEC_15.md — Level 15 Blueprint Specification (Aceptación Final)
## Spec ID: LEVEL-SPEC-15
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 15 ("Aceptación Final"). Final climax synthesis of the entire game journey, returning to the initial classroom space in complete resolution clarity.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_15.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Mastery` (Final game synthesis).
- `Chapter`: Chapter VI (Integración).

### 5. INPUTS
- [ECHOES_BIBLE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/ECHOES_BIBLE.md) `[SPEC-101]`
- [DESIGN_PHILOSOPHY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/DESIGN_PHILOSOPHY.md) `[SPEC-001]`

### 6. OUTPUTS
- Generated Unity scene `Level_15_AceptacionFinal.unity`.

### 7. RULES
- `[RULE-L15-001]`: **Level Modules**: MUST include `SchoolLiminalClassroom` (45) $[12.0, 5.0, 12.0]$ and `LevelExit` (6).
- `[RULE-L15-002]`: **Lighting Token**: Fog color `#F0F4FF` (White Clean), $D_{fog} = 0.002$, Ambient color `#FFFFFF`.
- `[RULE-L15-003]`: **Camera Profile**: `Acceptance` ($FOV = 52.0^\circ$).
- `[RULE-L15-004]`: **Narrative Item**: `Prop_LetterAmber` (Amber Letter `#FFBF00`) located at the center desk of `SchoolLiminalClassroom_02`.

### 8. ALGORITHMS

```yaml
level_id: "Level_15"
chapter_id: 6
archetype: "Mastery"
target_score: 80
modules:
  - id: "mod_01_final"
    type: "SchoolLiminalClassroom"
    position: [0.0, 0.0, 0.0]
    dimensions: [12.0, 5.0, 12.0]
    parameters:
      floating_desks: false
puzzle:
  components:
    - type: "LevelGoal"
      position: [0.0, 0.0, 6.0]
      parameters:
        final_resolution: true
```

### 9. CONSTRAINTS
- `[CONS-L15-001]`: Prohibido UI dialog boxes or text credits during level completion.

### 10. VALIDATION
- `[VAL-L15-001]`: `LevelValidator.cs` asserts Level 15 goal unlocks game completion state.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_15.asset`.

### 12. FAILURE CASES
- `[FAIL-L15-001]`: **Text Exposure**: Result: `FAIL-PHI-02`.

### 13. CROSS REFERENCES
- [ECHOES_BIBLE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/ECHOES_BIBLE.md) `[SPEC-101]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
