# BUILDING_FLOW.md — Architectural Flow & School Pacing
## Spec ID: SPEC-103
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines circulation graphs, chapter pacing curves, volumetric rhythm, and room sequence algorithms across all 6 chapters of *Echoes of You 2.0*.

### 2. SCOPE
Applies to `LevelBlueprint` asset structure, `EchoesNewProductionBuilder.cs`, and chapter sequence logic.

### 3. AUTHORITY
Nivel 2 (Contexto Técnico y Filosofía). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`). Consolidates `EXPLORATION_MODEL.md` and `EMOTIONAL_FLOW.md`.

### 4. DEFINITIONS
- `Chapter Pacing Curve`: Progression of puzzle difficulty and spatial complexity across Chapters I through VI.
- `Circulation Node`: A room module in the level topological graph.

### 5. INPUTS
- [ECHOES_BIBLE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/ECHOES_BIBLE.md) `[SPEC-101]`
- [SPACE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/SPACE_GRAMMAR.md) `[SPEC-102]`

### 6. OUTPUTS
- Chapter sequence rules for `LevelBlueprint` assets.

### 7. RULES

- `[RULE-FLO-001]`: **6-Chapter Progression Arc**: Level sequence MUST strictly map across all 6 chapters defined in Table 8.1.
- `[RULE-FLO-002]`: **Single Dominant Space Rule**: Every level layout MUST contain exactly 1 Dominant Space (`SchoolHall` or `SchoolGym`) acting as the spatial anchor.

### 8. ALGORITHMS

#### Table 8.1: Master 6-Chapter Flow & Archetype Matrix

| Chapter ID | Chapter Name | Assigned Levels | Pacing Theme | Dominant Room Module | Fog Density ($D_{fog}$) |
|---|---|---|---|---|---|
| **I** | Persistencia | `Level_01` - `Level_03` | Mechanics Introduction | `SchoolHall` (31) | `0.008` |
| **II** | Coordinación | `Level_04` - `Level_05` | Dual Action Timing | `SchoolGym` (39) | `0.010` |
| **III** | Confianza | `Level_06` - `Level_08` | Ghost Platform Trust | `SchoolLab` (40) | `0.012` |
| **IV** | Optimización | `Level_09` - `Level_11` | Inversion & Timing | `SchoolMaintenanceCorridor` (41) | `0.015` |
| **V** | Consecuencia | `Level_12` - `Level_13` | Conflict Hazards | `SchoolOffice` (44) | `0.020` |
| **VI** | Integración | `Level_14` - `Level_15` | Resolution & Clarity | `SchoolCourtyard` (38) | `0.002` |

### 9. CONSTRAINTS
- `[CONS-FLO-001]`: Prohibido introducing Chapter VII or non-standard chapter indices.

### 10. VALIDATION
- `[VAL-FLO-001]`: `LevelValidator.cs` parses blueprint chapter assignments and asserts `chapter_id` $\in [1, 6]$.

### 11. EXAMPLES
- Chapter assignment YAML block.

### 12. FAILURE CASES
- `[FAIL-FLO-001]`: **Invalid Chapter Assignment**: Result: `FAIL-SYS-01`.

### 13. CROSS REFERENCES
- [ECHOES_BIBLE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/ECHOES_BIBLE.md) `[SPEC-101]`
- [SPACE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/SPACE_GRAMMAR.md) `[SPEC-102]`

### 14. CHANGE HISTORY
- **v1.0 (2025-04-05)**: Flow initial draft.
- **v3.0 (2026-07-25)**: Full refactor into 14-section AI-Executable canonical format updating chapter count to 6.
