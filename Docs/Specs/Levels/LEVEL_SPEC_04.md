# LEVEL_SPEC_04.md — Level 04 Blueprint Specification (Gimnasio Hueco)
## Spec ID: LEVEL-SPEC-04
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 04 ("Gimnasio Hueco"). Introduces Chapter II timing coordination across a double-height gymnasium module with moving platforms.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_04.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Teaching` (Chapter II timing introduction).
- `Chapter`: Chapter II (Coordinación).

### 5. INPUTS
- [ARCHITECTURE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ARCHITECTURE_GRAMMAR.md) `[SPEC-003]`
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`

### 6. OUTPUTS
- Generated Unity scene `Level_04_GimnasioHueco.unity`.

### 7. RULES

- `[RULE-L04-001]`: **Level Modules**: Level 04 MUST include `SchoolGym` (39) $[16.0, 6.0, 24.0]$ and `MovingPlatform` (8).
- `[RULE-L04-002]`: **Lighting Token**: Fog color `#2E3024` (Olive), $D_{fog} = 0.010$, Ambient color `#1A1C14`.
- `[RULE-L04-003]`: **Camera Profile**: `Puzzle` ($FOV = 45.0^\circ$).
- `[RULE-L04-004]`: **Narrative Item**: `Prop_Stopwatch` (Amber Stopwatch `#FFBF00`) located in `SchoolGym_01`.

### 8. ALGORITHMS

```yaml
level_id: "Level_04"
chapter_id: 2
archetype: "Teaching"
target_score: 80
modules:
  - id: "mod_01_gym"
    type: "SchoolGym"
    position: [0.0, 0.0, 0.0]
    dimensions: [16.0, 6.0, 24.0]
    parameters:
      bleachers_side: "left"
      balls_count: 3
puzzle:
  components:
    - type: "MovingPlatform"
      position: [0.0, 1.0, 8.0]
      parameters:
        move_axis: "x"
        move_distance_m: 8.0
        move_speed_mps: 2.0
    - type: "PressurePlate"
      position: [-6.0, 0.08, 12.0]
    - type: "Door"
      position: [0.0, 0.0, 24.0]
```

### 9. CONSTRAINTS
- `[CONS-L04-001]`: Prohibido timing precision $< 0.8\text{s}$ in Chapter II entry.

### 10. VALIDATION
- `[VAL-L04-001]`: `LevelValidator.cs` asserts `SchoolGym` depth equals $24.0\text{f}$.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_04.asset`.

### 12. FAILURE CASES
- `[FAIL-L04-001]`: **Gym Depth Mismatch**: Depth set to 20m. Result: `FAIL-ARC-01`.

### 13. CROSS REFERENCES
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
