# LEVEL_SPEC_08.md — Level 08 Blueprint Specification (Puente de Memoria)
## Spec ID: LEVEL-SPEC-08
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 08 ("Puente de Memoria"). Chapter III twist integrating `GhostBridge` with multiple ECHO platforms across classrooms.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_08.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Twist` (Ghost bridge subversion).
- `Chapter`: Chapter III (Confianza).

### 5. INPUTS
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`

### 6. OUTPUTS
- Generated Unity scene `Level_08_PuenteDeMemoria.unity`.

### 7. RULES
- `[RULE-L08-001]`: **Level Modules**: MUST include `SchoolClassroom` (33) and `GhostPlatform` (23).
- `[RULE-L08-002]`: **Lighting Token**: Fog color `#2A1E1E` (Dusty Rose), $D_{fog} = 0.012$, Ambient color `#140E0E`.
- `[RULE-L08-003]`: **Camera Profile**: `Puzzle` ($FOV = 45.0^\circ$).
- `[RULE-L08-004]`: **Narrative Item**: `Prop_DrawingAmber` (Amber Drawing `#FFBF00`) in `SchoolClassroom_03`.

### 8. ALGORITHMS

```yaml
level_id: "Level_08"
chapter_id: 3
archetype: "Twist"
target_score: 80
modules:
  - id: "mod_01_classroom"
    type: "SchoolClassroom"
    position: [0.0, 0.0, 0.0]
    dimensions: [10.0, 3.8, 10.0]
puzzle:
  components:
    - type: "GhostPlatform"
      position: [0.0, 0.0, 6.0]
      parameters:
        solid_during_echo: true
    - type: "LevelGoal"
      position: [0.0, 0.0, 12.0]
```

### 9. CONSTRAINTS
- `[CONS-L08-001]`: Prohibido permanent floor mesh across ghost bridge section.

### 10. VALIDATION
- `[VAL-L08-001]`: `LevelValidator.cs` asserts `GhostPlatform` component exists.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_08.asset`.

### 12. FAILURE CASES
- `[FAIL-L08-001]`: **Platform Mesh Static**: Result: `FAIL-PUZ-01`.

### 13. CROSS REFERENCES
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
