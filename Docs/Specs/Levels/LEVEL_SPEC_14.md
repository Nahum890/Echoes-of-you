# LEVEL_SPEC_14.md — Level 14 Blueprint Specification (Umbral del Limbo)
## Spec ID: LEVEL-SPEC-14
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 14 ("Umbral del Limbo"). Chapter VI entry featuring courtyard transition and open-air fog.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_14.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Combination` (Courtyard transition).
- `Chapter`: Chapter VI (Integración).

### 5. INPUTS
- [ARCHITECTURE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ARCHITECTURE_GRAMMAR.md) `[SPEC-003]`

### 6. OUTPUTS
- Generated Unity scene `Level_14_UmbralDelLimbo.unity`.

### 7. RULES
- `[RULE-L14-001]`: **Level Modules**: MUST include `SchoolCourtyard` (38) $[24.0, 8.0, 24.0]$.
- `[RULE-L14-002]`: **Lighting Token**: Fog color `#F0F4FF` (White Clean), $D_{fog} = 0.002$, Ambient color `#FFFFFF`.
- `[RULE-L14-003]`: **Camera Profile**: `Acceptance` ($FOV = 52.0^\circ$).
- `[RULE-L14-004]`: **Narrative Item**: `Prop_RibbonAmber` (Amber Ribbon `#FFBF00`) in `SchoolCourtyard_01`.

### 8. ALGORITHMS

```yaml
level_id: "Level_14"
chapter_id: 6
archetype: "Combination"
target_score: 80
modules:
  - id: "mod_01_courtyard"
    type: "SchoolCourtyard"
    position: [0.0, 0.0, 0.0]
    dimensions: [24.0, 8.0, 24.0]
    parameters:
      tree_position: "center"
      fences_enabled: true
puzzle:
  components:
    - type: "PortalThreshold"
      position: [0.0, 0.0, 12.0]
    - type: "LevelGoal"
      position: [0.0, 0.0, 24.0]
```

### 9. CONSTRAINTS
- `[CONS-L14-001]`: Prohibido dense fog in Chapter VI ($D_{fog} > 0.005$).

### 10. VALIDATION
- `[VAL-L14-001]`: `LevelValidator.cs` asserts fog density equals $0.002\text{f}$.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_14.asset`.

### 12. FAILURE CASES
- `[FAIL-L14-001]`: **Dense Fog Violation**: Result: `FAIL-LGT-01`.

### 13. CROSS REFERENCES
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
