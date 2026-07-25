# LEVEL_SPEC_10.md — Level 10 Blueprint Specification (Aula Invertida)
## Spec ID: LEVEL-SPEC-10
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 10 ("Aula Invertida"). Chapter IV experimentation introducing gravity inversion switches (`GravitationalSwitch`) and camera inversion profile.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_10.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Experimentation` (Gravity inversion).
- `Chapter`: Chapter IV (Optimización).

### 5. INPUTS
- [CAMERA_RECIPES.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CAMERA_RECIPES.md) `[SPEC-007]`

### 6. OUTPUTS
- Generated Unity scene `Level_10_AulaInvertida.unity`.

### 7. RULES
- `[RULE-L10-001]`: **Level Modules**: MUST include `SchoolLyraClassroom` (43) $[12.0, 4.2, 12.0]$ and `GravitationalSwitch` (19).
- `[RULE-L10-002]`: **Lighting Token**: Fog color `#3B3024` (Amber Deep), $D_{fog} = 0.015$, Ambient color `#1E1812`.
- `[RULE-L10-003]`: **Camera Profile**: `Inversion` ($FOV = 45.0^\circ$, Roll = $180.0^\circ$).
- `[RULE-L10-004]`: **Narrative Item**: `Prop_CompassAmber` (Amber Compass `#FFBF00`) in `SchoolLyraClassroom_02`.

### 8. ALGORITHMS

```yaml
level_id: "Level_10"
chapter_id: 4
archetype: "Experimentation"
target_score: 80
modules:
  - id: "mod_01_lyra"
    type: "SchoolLyraClassroom"
    position: [0.0, 0.0, 0.0]
    dimensions: [12.0, 4.2, 12.0]
puzzle:
  components:
    - type: "GravitationalSwitch"
      position: [0.0, 1.5, 6.0]
      parameters:
        invert_y: true
    - type: "Door"
      position: [0.0, 0.0, 12.0]
```

### 9. CONSTRAINTS
- `[CONS-L10-001]`: Prohibido Roll $\ne 180.0^\circ$ during inversion state.

### 10. VALIDATION
- `[VAL-L10-001]`: `LevelValidator.cs` asserts `GravitationalSwitch` handles inversion.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_10.asset`.

### 12. FAILURE CASES
- `[FAIL-L10-001]`: **Camera Roll Deficit**: Result: `FAIL-CAM-01`.

### 13. CROSS REFERENCES
- [CAMERA_RECIPES.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CAMERA_RECIPES.md) `[SPEC-007]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
