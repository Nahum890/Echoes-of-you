# LEVEL_SPEC_01.md — Level 01 Blueprint Specification (Pasillo Ausente)
## Spec ID: LEVEL-SPEC-01
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the executable technical blueprint for Level 01 ("Pasillo Ausente"). Introduces basic locomotion, corridor navigation, single pressure plate interaction, and initial Echo recording.

### 2. SCOPE
Applies to `LevelBlueprint` asset `Level_01.asset`, `EchoesNewProductionBuilder.cs`, and `LevelRuntimeController.cs` for Level 01.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`).

### 4. DEFINITIONS
- `Level Archetype`: `Teaching` (Zero-risk learning level).
- `Chapter`: Chapter I (Persistencia).

### 5. INPUTS
- [ARCHITECTURE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ARCHITECTURE_GRAMMAR.md) `[SPEC-003]`
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`

### 6. OUTPUTS
- Generated Unity scene `Level_01_PasilloAusente.unity`.

### 7. RULES

- `[RULE-L01-001]`: **Level Dimensions & Modules**: Level 01 MUST consist of 5 explicit modules: `PlayerStart` (7), `SchoolEntrance` (47), `SchoolCorridor` (32), `SchoolClassroom` (33), and `LevelExit` (6).
- `[RULE-L01-002]`: **Lighting Token**: Fog color `#1C2430`, $D_{fog} = 0.008$, Ambient color `#0F141A`.
- `[RULE-L01-003]`: **Camera Profile**: `Learning` ($FOV = 40.0^\circ$, Offset `[0.0, 2.8, -5.5]`).
- `[RULE-L01-004]`: **Narrative Item**: `Prop_Coat` (Amber Coat `#FFBF00`) located in `SchoolCorridor_01`.

### 8. ALGORITHMS

#### Table 8.1: Level 01 Module Graph Layout

```yaml
level_id: "Level_01"
chapter_id: 1
archetype: "Teaching"
target_score: 80
modules:
  - id: "mod_00_spawn"
    type: "PlayerStart"
    enum_index: 7
    transform:
      position: {x: 0.0, y: 0.0, z: 0.0}
      rotation: {x: 0.0, y: 0.0, z: 0.0}
    dimensions: [3.0, 3.2, 3.0]
  - id: "mod_01_entrance"
    type: "SchoolEntrance"
    enum_index: 47
    transform:
      position: {x: 0.0, y: 0.0, z: 3.0}
      rotation: {x: 0.0, y: 0.0, z: 0.0}
    dimensions: [4.5, 3.8, 6.0]
  - id: "mod_02_corridor"
    type: "SchoolCorridor"
    enum_index: 32
    transform:
      position: {x: 0.0, y: 0.0, z: 9.0}
      rotation: {x: 0.0, y: 0.0, z: 0.0}
    dimensions: [4.5, 3.2, 24.0]
    parameters:
      length_m: 24.0
      lockers_side: "left"
      lockers:
        side: "left"
        spacing_m: 1.2
        prefab_alias: "Prop_Locker"
  - id: "mod_03_classroom"
    type: "SchoolClassroom"
    enum_index: 33
    transform:
      position: {x: 0.0, y: 0.0, z: 33.0}
      rotation: {x: 0.0, y: 0.0, z: 0.0}
    dimensions: [10.0, 3.8, 10.0]
  - id: "mod_04_exit"
    type: "LevelExit"
    enum_index: 6
    transform:
      position: {x: 0.0, y: 0.0, z: 43.0}
      rotation: {x: 0.0, y: 0.0, z: 0.0}
    dimensions: [3.0, 3.5, 3.0]
puzzle:
  components:
    - type: "PressurePlate"
      enum_index: 4
      transform:
        position: {x: 0.0, y: 0.08, z: 21.0}
        rotation: {x: 0.0, y: 0.0, z: 0.0}
      target_wire: "wire_01"
    - type: "Door"
      enum_index: 5
      transform:
        position: {x: 0.0, y: 0.0, z: 33.0}
        rotation: {x: 0.0, y: 0.0, z: 0.0}
      required_signal: "signal_plate_01"
```

### 9. CONSTRAINTS
- `[CONS-L01-001]`: Prohibido hazard zones or disintegration fields in Level 01.

### 10. VALIDATION
- `[VAL-L01-001]`: `LevelValidator.cs` confirms Level 01 scene contains `PlayerStart` (7) and `LevelExit` (6) and passes `The Echo Button Test`.

### 11. EXAMPLES
- Blueprint asset `Assets/Data/Levels/Level_01.asset`.

### 12. FAILURE CASES
- `[FAIL-L01-001]`: **Missing Entry Spawn**: Spawn point not assigned. Result: `FAIL-SYS-01`.

### 13. CROSS REFERENCES
- [LIGHTING_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LIGHTING_GRAMMAR.md) `[SPEC-105]`
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Updated Level 01 Blueprint spec with explicit PlayerStart (7), LevelExit (6), and SchoolEntrance (47) module declarations.
