# SAVE_MIGRATION_SPEC.md — Save Data Versioning & Data Migration Protocol
## Spec ID: SPEC-130
## Version: 1.0 (AI-Executable)

---

### 1. PURPOSE
Specifies version upgrade pipelines, CRC32 checksum verification, corruption recovery protocols, and backup fallback mechanics for save data in *Echoes of You 2.0*.

### 2. SCOPE
Applies to `SaveDataManager.cs` and `save_v1.json` persistence operations.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `SAVE_DATA_SCHEMA.md` (`SPEC-119`).

### 4. DEFINITIONS
- `Save Schema Version`: Monotonically increasing integer tracking data layout changes (`current_schema_version = 1`).
- `CRC32 Checksum`: Hexadecimal checksum validation field protecting save files against silent corruption.

### 5. INPUTS
- [SAVE_DATA_SCHEMA.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SAVE_DATA_SCHEMA.md) `[SPEC-119]`
- [SAVE_MIGRATION_SPEC.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SAVE_MIGRATION_SPEC.yaml) `[SPEC-130]`

### 6. OUTPUTS
- Migration pipeline steps and recovery fallback logic.

### 7. RULES
- `[RULE-SAV-001]`: **Sequential Migration**: If loaded `save_version` $<$ `current_schema_version`, migrations MUST run in exact numerical order.
- `[RULE-SAV-002]`: **Checksum Verification**: If CRC32 checksum mismatches on load, `SaveDataManager` MUST attempt load from `save_v1.backup.json`.
- `[RULE-SAV-003]`: **Downgrade Rejection**: Loading save files with `save_version` $>$ `current_schema_version` MUST immediately emit `FAIL-SAVE-01`.

### 8. ALGORITHMS
See canonical YAML schema at `Docs/Specs/SAVE_MIGRATION_SPEC.yaml`.

### 9. CONSTRAINTS
- `[CONS-SAV-001]`: Prohibido overwriting un-migrated legacy save files without creating backup copy first.

### 10. VALIDATION
- `[VAL-SAV-001]`: Automated test loads corrupted `save_v1.json` and verifies recovery to backup or clean state.

### 11. EXAMPLES
- `SAVE_MIGRATION_SPEC.yaml` schema definition.

### 12. FAILURE CASES
- `[FAIL-SAV-001]`: **Unrecoverable Save Corruption**: Backup missing and primary file invalid CRC. Result: `ResetToNewGame` with log warning.

### 13. CROSS REFERENCES
- [SAVE_DATA_SCHEMA.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SAVE_DATA_SCHEMA.md) `[SPEC-119]`

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Created canonical SPEC-130 for Save Migration protocol.
