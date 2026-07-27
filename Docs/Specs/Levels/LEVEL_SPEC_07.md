# LEVEL_SPEC_07.md — Level 07 Blueprint Specification (Eco Futuro)
## Spec ID: LEVEL-SPEC-07
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 07 ("Eco Futuro"). Chapter III experimentation with future recording buffers and liminal classrooms.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_07.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Experimentation` (Future recording buffer).
- `Chapter`: Chapter III (Confianza).

### 5. INPUTS
- [ECHO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_GRAMMAR.md) `[SPEC-107]`

### 6. OUTPUTS
- Generated Unity scene `Level_07_EcoFuturo.unity`.

### 7. RULES
- `[RULE-L07-001]`: **Level Modules**: MUST include `SchoolLiminalClassroom` (45) $[12.0, 5.0, 12.0]$.
- `[RULE-L07-002]`: **Lighting Token**: Fog color `#2A1E1E` (Dusty Rose), $D_{fog} = 0.012$, Ambient color `#140E0E`.
- `[RULE-L07-003]`: **Camera Profile**: `Memory` ($FOV = 37.0^\circ$).
- `[RULE-L07-004]`: **Narrative Item**: `Prop_ClockHandAmber` (Amber Clock Hand `#FFBF00`) located in `SchoolLiminalClassroom_01`.

### 8. ALGORITHMS

```yaml
level_id: "Level_07"
chapter_id: 3
archetype: "Experimentation"
target_score: 80
modules:
  - id: "mod_01_liminal"
    type: "SchoolLiminalClassroom"
    position: [0.0, 0.0, 0.0]
    dimensions: [12.0, 5.0, 12.0]
puzzle:
  components:
    - type: "EchoRecorderSlot"
      position: [0.0, 0.1, 4.0]
      parameters:
        record_future: true
    - type: "Door"
      position: [0.0, 0.0, 12.0]
```

### 9. CONSTRAINTS
- `[CONS-L07-001]`: Prohibido record time $> 12.0\text{s}$.

### 10. VALIDATION
- `[VAL-L07-001]`: `LevelValidator.cs` asserts `record_future` flag is valid.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_07.asset`.

### 12. FAILURE CASES
- `[FAIL-L07-001]`: **Record Timeout**: Result: `FAIL-ECH-01`.

### 13. CROSS REFERENCES
- [ECHO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_GRAMMAR.md) `[SPEC-107]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
