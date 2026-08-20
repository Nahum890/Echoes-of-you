# CHANGE_CONTROL.md — Frozen Architectural Decision & Change Control System
## Spec ID: SPEC-000D
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Records all frozen architectural decisions, change requests, and contradiction resolutions for *Echoes of You 2.0*. Prevents unauthorized renegotiation of core system parameters by human developers or AI agents.

### 2. SCOPE
Applies to 100% of specification documents in `Docs/`, C# scripts in `Assets/Scripts/`, and data assets in `Assets/Data/`.

### 3. AUTHORITY
Level 2 (Technical Context & Change Registry). Subordinate only to `SOURCE_OF_TRUTH.md` (`SPEC-000`, Level 1).

### 4. DEFINITIONS
- `Frozen Decision`: A non-negotiable architectural decision requiring explicit change control authorization to modify.
- `Contradiction Resolution`: A binding arbitration entry resolving conflicting specs across files.

### 5. INPUTS
- [SOURCE_OF_TRUTH.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Authority/SOURCE_OF_TRUTH.md) `[SPEC-000]`
- [DOCUMENT_AUDIT_REPORT.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Authority/DOCUMENT_AUDIT_REPORT.md) `[SPEC-000C]`

### 6. OUTPUTS
- Binding decision entries enforced by `LevelValidator.cs` and `AI_RULEBOOK.md`.

### 7. RULES

- `[RULE-CC-001]`: **Immutable Resolution Rule**: All entries logged in Section 8 (Table 8.1) are frozen. No AI agent or automated script may modify these parameters without updating this document.
- `[RULE-CC-002]`: **Change Authorization Requirement**: Any architectural change MUST be logged with a unique `CC-2026-xxx` entry.

### 8. ALGORITHMS

#### Table 8.1: Frozen Decisions & Contradiction Resolution Log (`CONT-001` to `CONT-012`)

| Decision ID | Conflict Ref | Superseded / Prohibited Option B | Official / Binding Option A | Resolution Justification |
|---|---|---|---|---|
| `CC-2026-001` | `CONT-001` | Puzzle Camera FOV $= 50.0^\circ$ | **Puzzle Camera FOV $= 45.0^\circ$** | `CAMERA_GRAMMAR` (Level 2) overrides `CAMERA_RECIPES` (Level 3). |
| `CC-2026-002` | `CONT-002` | Level 1 Fog Density $= 0.016$ / $0.012-0.04$ | **Level 1 Fog Density $= 0.008$** | `LIGHTING_GRAMMAR` Chapter I spec is authoritative. |
| `CC-2026-003` | `CONT-003` | `memory-amber` Hex $= \text{\#E8B262}$ | **`memory-amber` Hex $= \text{\#FFBF00}$** | `ECHOES_BIBLE` (Level 1) overrides lower-tier material specs. |
| `CC-2026-004` | `CONT-004` | Ambiguous corridor width ($3.0\text{m}$ vs $4.5\text{m}$) | **Main Corridor $= 4.5\text{m}$, Secondary $= 3.0\text{m}$** | Standardized hierarchy across scale, architecture, and room specs. |
| `CC-2026-005` | `CONT-005` | `Courier Prime` / `PT Mono` typography | **`Inter` / `Roboto` Typography** | `UI_SPEC` (`SPEC-008`) overrides obsolete `VISUAL_TARGET` §14. |
| `CC-2026-006` | `CONT-006` | `SchoolGym` Depth $= 20.0\text{m}$ | **`SchoolGym` Depth $= 24.0\text{m}$** | `ARCHITECTURE_GRAMMAR` volumetric spec overrides room template. |
| `CC-2026-007` | `CONT-007` | Dead links in `LEVEL_PIPELINE.md` | **Canonical Spec Links Exclusively** | Point pipeline to `ARCHITECTURE_GRAMMAR`, `ROOM_LIBRARY`, etc. |
| `CC-2026-008` | `CONT-008` | Scorecard Pass Threshold $= 85/100$ | **Minimum Passing Score $= 80/100$** | Standardized passing threshold across scorecard and validator. |
| `CC-2026-009` | `CONT-009` | Absolute URP/Lit shader mandate across all materials | **URP/Lit for static geometry + Custom shaders for Echo/FX** | Resolves conflict between SOURCE_OF_TRUTH.md L63 and SHADER_SPEC.md [SPEC-109]. |
| `CC-2026-010` | `HALT-2` | Unrestricted YAML override over Frozen Decisions | **YAML overrides Markdown EXCEPT Frozen Decisions Matrix** | Resolves deadlock RULE-SOT-001B vs RULE-SOT-002 in SOURCE_OF_TRUTH.md. |
| `CC-2026-011` | `HALT-7` | AI_RULEBOOK listed as Level 4 in INDEX | **AI_RULEBOOK & AI_PIPELINE assigned Level 5** | Aligns INDEX.md authority tier with SOURCE_OF_TRUTH.md §7. |
| `CC-2026-012` | `CONT-012` | `urp_volume_profiles.yaml` v1.0 post values (bloom 0.8/0.5, vignette `#000000` 0.2, exp 0, sat −20) + `fog_settings` block (Linear `#0F141A` 0.015, 5–35 m) | **SPEC-120 `POST_PROCESSING_SPEC.md` v4.0 values win; fog owned by chapter profiles (`lighting_profiles.yaml`)** | Real scene volumes match SPEC-120 bloom (0.25/0.9/0.7); `GameFeelController` pulses reference SPEC-120 baselines; `LIGHTING_GRAMMAR` RULE-LGT-004/005 delegate fog to chapter profiles. Executable mirror: `Docs/Art/POST_PROCESS_SPEC.md` `[SPEC-144]`, `urp_volume_profiles.yaml` v2.0. |

### 9. CONSTRAINTS
- `[CONS-CC-001]`: Prohibido deleting historic change entries from Table 8.1.

### 10. VALIDATION
- `[VAL-CC-001]`: `LevelValidator.cs` parses Table 8.1 and asserts 100% of runtime parameters match Option A values.

### 11. EXAMPLES
- Table 8.1 decision check.

### 12. FAILURE CASES
- `[FAIL-CC-001]`: **Unauthorized Parameter Override**: Result: `FAIL-CC-01`.

### 13. CROSS REFERENCES
- [SOURCE_OF_TRUTH.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Authority/SOURCE_OF_TRUTH.md) `[SPEC-000]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Full refactor into 14-section AI-Executable canonical format logging `CONT-001` through `CONT-008`.
