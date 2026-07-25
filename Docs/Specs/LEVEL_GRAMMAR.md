# LEVEL_GRAMMAR.md — Level Zone Structural Specifications
## Spec ID: SPEC-103B
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines mandatory module templates, relative grid coordinates, and minimum prop requirements for the 5 functional level zones across *Echoes of You 2.0*.

### 2. SCOPE
Applies to `LevelBlueprint` asset creation and `EchoesNewProductionBuilder.cs`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ARCHITECTURE_GRAMMAR.md` (`SPEC-003`).

### 4. DEFINITIONS
- `Level Zone`: Functional sub-region of a level layout (Zone 1: Entry, Zone 2: Learning, Zone 3: Puzzle Core, Zone 4: Climax, Zone 5: Exit).

### 5. INPUTS
- [ARCHITECTURE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ARCHITECTURE_GRAMMAR.md) `[SPEC-003]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`

### 6. OUTPUTS
- Zone structural definitions enforced by `LevelValidator.cs`.

### 7. RULES

- `[RULE-LVZ-001]`: **5-Zone Structure Requirement**: Every level blueprint MUST instantiate rooms mapping across all 5 functional zones defined in Table 8.1.
- `[RULE-LVZ-002]`: **Zone Module Assignment**: Zone room instances MUST select `ModuleType` enums assigned to their zone in Table 8.1 exclusively.

### 8. ALGORITHMS

#### Table 8.1: Master 5-Zone Structural Specification

| Zone ID | Zone Name | Mandatory `ModuleType` Enums | Relative Position Offset $[X, Y, Z]$ | Required Component Elements |
|---|---|---|---|---|
| **Zone 1** | Entry / Introduction | `PlayerStart` (7), `SchoolEntrance` (47) | `[0.0, 0.0, 0.0]` | Player spawn point, low fog |
| **Zone 2** | Mechanical Learning | `SchoolCorridor` (32) | `[0.0, 0.0, 9.0]` | Lockers, single pressure plate |
| **Zone 3** | Core Puzzle Challenge | `SchoolClassroom` (33) / `SchoolLab` (40) | `[0.0, 0.0, 33.0]` | `PuzzleCondition.cs`, `PuzzleWire.cs` |
| **Zone 4** | Narrative Climax | `SchoolLyraClassroom` (43) / `SchoolHall` (31)| `[0.0, 0.0, 45.0]` | `memory-amber` item (`#FFBF00`) |
| **Zone 5** | Exit / Resolution | `LevelExit` (6) | `[0.0, 0.0, 57.0]` | Goal trigger, scene transition |

### 9. CONSTRAINTS
- `[CONS-LVZ-001]`: Prohibido omitting Zone 1 (Entry) or Zone 5 (Exit) in any level layout.

### 10. VALIDATION
- `[VAL-LVZ-001]`: `LevelValidator.cs` parses scene rooms and asserts all 5 functional zones are present.

### 11. EXAMPLES
- Blueprint YAML layout block.

### 12. FAILURE CASES
- `[FAIL-LVZ-001]`: **Missing Zone**: Result: `FAIL-SYS-01`.

### 13. CROSS REFERENCES
- [ARCHITECTURE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ARCHITECTURE_GRAMMAR.md) `[SPEC-003]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec creation for level zone structures.
