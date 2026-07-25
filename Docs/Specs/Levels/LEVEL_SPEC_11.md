# LEVEL_SPEC_11.md — Level 11 Blueprint Specification (Corredor Degradado)
## Spec ID: LEVEL-SPEC-11
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 11 ("Corredor Degradado"). Chapter IV combination featuring emergency corridor navigation and timed alarms.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_11.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Combination` (Degraded environment optimization).
- `Chapter`: Chapter IV (Optimización).

### 5. INPUTS
- [ARCHITECTURE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ARCHITECTURE_GRAMMAR.md) `[SPEC-003]`

### 6. OUTPUTS
- Generated Unity scene `Level_11_CorredorDegradado.unity`.

### 7. RULES
- `[RULE-L11-001]`: **Level Modules**: MUST include `SchoolEmergencyCorridor` (42) $[3.0, 3.2, 18.0]$ and `HazardField` (17).
- `[RULE-L11-002]`: **Lighting Token**: Fog color `#3B3024` (Amber Deep), $D_{fog} = 0.015$, Ambient color `#1E1812`.
- `[RULE-L11-003]`: **Camera Profile**: `Suspense` ($FOV = 45.0^\circ$, Dutch = $5.0^\circ$).
- `[RULE-L11-004]`: **Narrative Item**: `Prop_LocketAmber` (Amber Locket `#FFBF00`) in `SchoolEmergencyCorridor`.

### 8. ALGORITHMS

```yaml
level_id: "Level_11"
chapter_id: 4
archetype: "Combination"
target_score: 80
modules:
  - id: "mod_01_emerg"
    type: "SchoolEmergencyCorridor"
    position: [0.0, 0.0, 0.0]
    dimensions: [3.0, 3.2, 18.0]
    parameters:
      alarm_color: "red"
      door_type: "bar"
puzzle:
  components:
    - type: "HazardField"
      position: [0.0, 0.1, 8.0]
      parameters:
        dissolve_echo: true
    - type: "LevelGoal"
      position: [0.0, 0.0, 18.0]
```

### 9. CONSTRAINTS
- `[CONS-L11-001]`: Prohibido hazard zones without red alert material.

### 10. VALIDATION
- `[VAL-L11-001]`: `LevelValidator.cs` asserts `HazardField` component exists.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_11.asset`.

### 12. FAILURE CASES
- `[FAIL-L11-001]`: **Unmarked Hazard**: Result: `FAIL-PRM-01`.

### 13. CROSS REFERENCES
- [ECHO_PRIMITIVE_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_PRIMITIVE_SPEC.md) `[SPEC-111]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
