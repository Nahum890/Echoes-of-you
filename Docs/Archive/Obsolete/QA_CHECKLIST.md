# QA_CHECKLIST.md — Automated Verification Protocol
## Spec ID: SPEC-304
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Establishes the automated checklist suite executed during Pass 6 of scene generation by `LevelValidator.cs`.

### 2. SCOPE
Applies to automated level generation and CI/CD validation.

### 3. AUTHORITY
Nivel 4 (Validación y QA). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `LEVEL_VALIDATOR.md` (`SPEC-301`).

### 4. DEFINITIONS
- `QA Item`: An individual automated assertion checking room bounds, lighting, camera, or puzzle components.

### 5. INPUTS
- [LEVEL_VALIDATOR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Validation/LEVEL_VALIDATOR.md) `[SPEC-301]`

### 6. OUTPUTS
- Automated QA pass report.

### 7. RULES
- `[RULE-QAC-001]`: **100% Automated Pass**: All items in Table 8.1 MUST return `Passed` status before scene export.

### 8. ALGORITHMS

#### Table 8.1: Master Automated QA Verification Items

| Item ID | Verification Target | Assertion Rule | Validator Reference |
|---|---|---|---|
| `QA-001` | Room Heights | Classroom $3.8\text{m}$, Corridor $3.2\text{m}$ | `VAL-A-01` |
| `QA-002` | Door Openings | Single $1.20\text{m} \times 2.40\text{m}$, Double $2.40\text{m} \times 2.40\text{m}$ | `VAL-A-02` |
| `QA-003` | NavMesh Corridor | Free width $W_{clearance} \ge 1.20\text{m}$ | `VAL-D-01` |
| `QA-004` | Echo Button Test | Solvability requires Echo recording | `VAL-B-01` |
| `QA-005` | Light Count Cap | Active light components $\le 48$ | `VAL-C-01` |
| `QA-006` | Shadow Type | URP `Light.shadows == LightShadows.Hard` | `VAL-C-02` |
| `QA-007` | Camera Controllers | Active camera controllers in scene $== 1$ | `VAL-E-01` |
| `QA-008` | Amber Item Color | Key narrative object matches `#FFBF00` | `VAL-ENV-01` |

### 9. CONSTRAINTS
- `[CONS-QAC-001]`: Prohibido subjective checklist items in automated QA protocol.

### 10. VALIDATION
- `[VAL-QAC-001]`: `LevelValidator.cs` parses Table 8.1 and asserts 100% item compliance.

### 11. EXAMPLES
- Automated QA log report.

### 12. FAILURE CASES
- `[FAIL-QAC-001]`: **QA Failure**: Result: `FAIL-SYS-01`.

### 13. CROSS REFERENCES
- [LEVEL_VALIDATOR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Validation/LEVEL_VALIDATOR.md) `[SPEC-301]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec creation for automated QA protocol.
