# LEVEL_SPEC_09.md — Level 09 Blueprint Specification (Transición Oscura)
## Spec ID: LEVEL-SPEC-09
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 09 ("Transición Oscura"). Chapter IV entry featuring maintenance corridor navigation and tight timing optimization.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_09.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Teaching` (Optimization entry).
- `Chapter`: Chapter IV (Optimización).

### 5. INPUTS
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`

### 6. OUTPUTS
- Generated Unity scene `Level_09_TransicionOscura.unity`.

### 7. RULES
- `[RULE-L09-001]`: **Level Modules**: MUST include `SchoolMaintenanceCorridor` (41) $[3.0, 3.2, 18.0]$ and `TimedGate` (28).
- `[RULE-L09-002]`: **Lighting Token**: Fog color `#3B3024` (Amber Deep), $D_{fog} = 0.015$, Ambient color `#1E1812`.
- `[RULE-L09-003]`: **Camera Profile**: `Learning` ($FOV = 40.0^\circ$).
- `[RULE-L09-004]`: **Narrative Item**: `Prop_KeyAmber` (Amber Key `#FFBF00`) in `SchoolMaintenanceCorridor`.

### 8. ALGORITHMS

```yaml
level_id: "Level_09"
chapter_id: 4
archetype: "Teaching"
target_score: 80
modules:
  - id: "mod_01_maint"
    type: "SchoolMaintenanceCorridor"
    position: [0.0, 0.0, 0.0]
    dimensions: [3.0, 3.2, 18.0]
    parameters:
      pipes: "ceiling"
      lights: "dim"
puzzle:
  components:
    - type: "TimedGate"
      position: [0.0, 0.0, 10.0]
      parameters:
        open_time_s: 3.0
    - type: "PressurePlate"
      position: [0.0, 0.08, 4.0]
```

### 9. CONSTRAINTS
- `[CONS-L09-001]`: Prohibido corridor width $< 3.0\text{m}$.

### 10. VALIDATION
- `[VAL-L09-001]`: `LevelValidator.cs` asserts maintenance corridor dimensions $[3.0, 3.2, 18.0]$.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_09.asset`.

### 12. FAILURE CASES
- `[FAIL-L09-001]`: **Corridor Clearance Deficit**: Result: `FAIL-NAV-01`.

### 13. CROSS REFERENCES
- [ARCHITECTURE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ARCHITECTURE_GRAMMAR.md) `[SPEC-003]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
