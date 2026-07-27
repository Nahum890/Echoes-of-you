# LEVEL_SPEC_03.md — Level 03 Blueprint Specification (Rincón de Lyra)
## Spec ID: LEVEL-SPEC-03
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 03 ("Rincón de Lyra"). Introduces Chapter I mastery with `SchoolLyraClassroom` and key amber item `MochilaLyra`.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_03.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Mastery` (Chapter I synthesis).
- `Chapter`: Chapter I (Persistencia).

### 5. INPUTS
- [ENVIRONMENT_STORYTELLING.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ENVIRONMENT_STORYTELLING.md) `[SPEC-006]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`

### 6. OUTPUTS
- Generated Unity scene `Level_03_RinconDeLyra.unity`.

### 7. RULES

- `[RULE-L03-001]`: **Level Modules**: Level 03 MUST include `SchoolHall` (31), `SchoolLyraClassroom` (43), and `LevelExit` (6).
- `[RULE-L03-002]`: **Lighting Token**: Fog color `#1C2430`, $D_{fog} = 0.012$, Ambient color `#0F141A`.
- `[RULE-L03-003]`: **Camera Profile**: `Emotional` ($FOV = 35.0^\circ$).
- `[RULE-L03-004]`: **Narrative Item**: `MochilaLyra` (Amber Backpack `#FFBF00`, emission $1.2\text{ Lux}$) in `SchoolLyraClassroom_01`.

### 8. ALGORITHMS

```yaml
level_id: "Level_03"
chapter_id: 1
archetype: "Mastery"
target_score: 80
modules:
  - id: "mod_01_hall"
    type: "SchoolHall"
    position: [0.0, 0.0, 0.0]
    dimensions: [12.0, 5.0, 12.0]
  - id: "mod_02_lyra_room"
    type: "SchoolLyraClassroom"
    position: [0.0, 0.0, 12.0]
    dimensions: [12.0, 4.2, 12.0]
    parameters:
      notebook: "amber"
      glow: true
puzzle:
  components:
    - type: "PressurePlate_EchoOnly"
      position: [0.0, 0.08, 16.0]
    - type: "LevelGoal"
      position: [0.0, 0.0, 20.0]
```

### 9. CONSTRAINTS
- `[CONS-L03-001]`: Prohibido non-amber narrative items in Level 03.

### 10. VALIDATION
- `[VAL-L03-001]`: `LevelValidator.cs` asserts `MochilaLyra` carries color `#FFBF00`.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_03.asset`.

### 12. FAILURE CASES
- `[FAIL-L03-001]`: **Missing Amber Item**: Result: `FAIL-ENV-02`.

### 13. CROSS REFERENCES
- [ENVIRONMENT_STORYTELLING.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ENVIRONMENT_STORYTELLING.md) `[SPEC-006]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec.
