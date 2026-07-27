# SAVE_DATA_SCHEMA.md — JSON Save System & Serialization Specifications
## Spec ID: SPEC-119
## Version: 5.0 (AI-Executable)

---

### 1. PURPOSE
Defines the strict JSON Schema (Draft-07) for save game serialization, level index bounds, memory array constraints, and save file storage paths for *Echoes of You 2.0*.

### 2. SCOPE
Applies to `SaveSystem.cs`, save file read/write operations, and level progression data.

### 3. AUTHORITY
Level 4 (Declarative Specs). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`). Runtime data contract defined in `Docs/ExecutableSpecs/persistence/save_schema.yaml` (`SPEC-EXEC-SAV`).

### 4. DEFINITIONS
- `Save File Path`: `Application.persistentDataPath + "/save_v1.json"`.
- `Save Version`: Integer constant (`1`) identifying schema format.
- `Recorded Echo Frames`: Array of position/rotation/timestamp tuples; max length = `echo.max_record_seconds_standard` × 60fps = 7200 frames.

### 5. INPUTS
- [SCENE_TRANSITION_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SCENE_TRANSITION_SPEC.md) `[SPEC-115]`
- [CONSTANTS_REGISTRY.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CONSTANTS_REGISTRY.yaml) `[SPEC-124]` — echo max record seconds

### 6. OUTPUTS
- Validated `save_v1.json` file written to disk upon level completion.

### 7. RULES
- `[RULE-SAV-001]`: **Save File Path** — Save data MUST write to `Application.persistentDataPath + "/save_v1.json"`.
- `[RULE-SAV-002]`: **JSON Schema Compliance** — 100% of serialized save files MUST validate against `save_schema.yaml` without error.
- `[RULE-SAV-003]`: **Level Index Constraints** — `currentLevelIndex` MUST satisfy $1 \le index \le 15$. `levelCompletionFlags` MUST contain exactly 15 boolean elements.
- `[RULE-SAV-004]`: **Echo Frame Cap** — `recordedEchoFrames` maxItems is dynamic: `CONSTANTS_REGISTRY.yaml#echo.max_record_seconds_standard` × 60 = 7200 frames.
- `[RULE-SAV-005]`: **Serialization Format** — Binary serialization (e.g., `BinaryFormatter`) is PROHIBITED. JSON only.

### 8. ALGORITHMS
Runtime JSON Schema is defined in `Docs/ExecutableSpecs/persistence/save_schema.yaml`. The Markdown document does not duplicate numeric schema parameters.

### 9. CONSTRAINTS
- `[CONS-SAV-001]`: Prohibido binary serialization methods (e.g. `BinaryFormatter`). JSON serialization only.

### 10. VALIDATION
- `[VAL-SAV-001]`: Automated serialization unit test asserts JSON validation against `save_schema.yaml`.

### 11. EXAMPLES
See `save_schema.yaml` for canonical schema definition.

### 12. FAILURE CASES
- `[FAIL-SAV-001]`: **Corrupt Save File** — Invalid JSON syntax on load. Result: `SaveSystem` renames corrupt file to `.bak` and generates fresh save.

### 13. CROSS REFERENCES
- [SCENE_TRANSITION_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SCENE_TRANSITION_SPEC.md) `[SPEC-115]`
- [CONSTANTS_REGISTRY.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CONSTANTS_REGISTRY.yaml) `[SPEC-124]`
- [save_schema.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/persistence/save_schema.yaml) `[SPEC-EXEC-SAV]`

### 14. CHANGE HISTORY
- **v4.0 (2026-07-25)**: Created canonical SPEC-119 defining JSON Schema (Draft-07) for save data serialization.
- **v5.0 (2026-07-25)**: Moved numeric schema to YAML catalog. Fixed echo frame cap to 7200 (12s × 60fps). Added dynamic maxItems reference.

(End of file - total 72 lines)