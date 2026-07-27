# LEVEL_SPEC_06.md — Level 06 Blueprint Specification (Laboratorio Quebrado)
## Spec ID: LEVEL-SPEC-06
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 06 ("Laboratorio Quebrado"). Chapter III entry introducing `GhostBridge` / `TemporalBridge` mechanics revealed during Echo presence.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_06.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Teaching` (Ghost bridge introduction).
- `Chapter`: Chapter III (Confianza).

### 5. INPUTS
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`

### 6. OUTPUTS
- Generated Unity scene `Level_06_LaboratorioQuebrado.unity`.

### 7. RULES
- `[RULE-L06-001]`: **Level Modules**: MUST include `SchoolLab` (40) $[12.0, 3.8, 10.0]$ and `TemporalBridge` (22).
- `[RULE-L06-002]`: **Lighting Token**: Fog color `#2A1E1E` (Dusty Rose), $D_{fog} = 0.012$, Ambient color `#140E0E`.
- `[RULE-L06-003]`: **Camera Profile**: `Puzzle` ($FOV = 45.0^\circ$).
- `[RULE-L06-004]`: **Narrative Item**: `Prop_BeakerAmber` (Amber Beaker `#FFBF00`) in `SchoolLab_01`.

### 8. ALGORITHMS

```yaml
level_id: "Level_06"
chapter_id: 3
archetype: "Teaching"
target_score: 80
modules:
  - id: "mod_01_lab"
    type: "SchoolLab"
    position: [0.0, 0.0, 0.0]
    dimensions: [12.0, 3.8, 10.0]
puzzle:
  components:
    - type: "TemporalBridge"
      position: [0.0, 0.0, 5.0]
      parameters:
        visible_during_echo: true
    - type: "LevelGoal"
      position: [0.0, 0.0, 10.0]
```

### 9. CONSTRAINTS
- `[CONS-L06-001]`: Prohibido permanent solid platforms over the chasm (bridge MUST require Echo presence).

### 10. VALIDATION
- `[VAL-L06-001]`: `LevelValidator.cs` asserts `TemporalBridge` component exists.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_06.asset`.

### 12. FAILURE CASES
- `[FAIL-L06-001]`: **Bridge Missing Trigger**: Result: `FAIL-PUZ-01`.

### 13. CROSS REFERENCES
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
