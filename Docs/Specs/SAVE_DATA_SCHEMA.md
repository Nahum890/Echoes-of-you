# SAVE_DATA_SCHEMA.md — JSON Save System & Serialization Specifications
## Spec ID: SPEC-119
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Defines the strict JSON Schema (Draft-07) for save game serialization, level index bounds, memory array constraints, and save file storage paths for *Echoes of You 2.0*.

### 2. SCOPE
Applies to `SaveSystem.cs`, save file read/write operations, and level progression data.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`). Replaces `SERIALIZATION_SPEC.md`.

### 4. DEFINITIONS
- `Save File Path`: `Application.persistentDataPath + "/save_v1.json"`.
- `Save Version`: Integer constant (`1`) identifying schema format.

### 5. INPUTS
- [SCENE_TRANSITION_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SCENE_TRANSITION_SPEC.md) `[SPEC-115]`

### 6. OUTPUTS
- Validated `save_v1.json` file written to disk upon level completion.

### 7. RULES

- `[RULE-SAV-001]`: **Save File Path**: Save data MUST write to `Application.persistentDataPath + "/save_v1.json"`.
- `[RULE-SAV-002]`: **JSON Schema Compliance**: 100% of serialized save files MUST validate against the JSON Schema in Algorithm 8.1 without error.
- `[RULE-SAV-003]`: **Level Index Constraints**: `currentLevelIndex` MUST satisfy $1 \le index \le 15$. `levelCompletionFlags` MUST contain exactly 15 boolean elements.

### 8. ALGORITHMS

#### Algorithm 8.1: Master Save Data JSON Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "EchoesOfYou_SaveData",
  "type": "object",
  "required": ["saveVersion", "currentLevelIndex", "unlockedMemories"],
  "properties": {
    "saveVersion": { "type": "integer", "const": 1 },
    "currentLevelIndex": { "type": "integer", "minimum": 1, "maximum": 15 },
    "unlockedMemories": {
      "type": "array",
      "items": { "type": "string", "pattern": "^MEM-[0-9]{3}$" },
      "maxItems": 15
    },
    "totalPlaytimeSeconds": { "type": "number", "minimum": 0.0 },
    "levelCompletionFlags": {
      "type": "array",
      "items": { "type": "boolean" },
      "minItems": 15,
      "maxItems": 15
    }
  }
}
```

### 9. CONSTRAINTS
- `[CONS-SAV-001]`: Prohibido binary serialization methods (e.g. `BinaryFormatter`). JSON serialization only.

### 10. VALIDATION
- `[VAL-SAV-001]`: Automated serialization unit test asserts JSON validation against schema.

### 11. EXAMPLES
- Schema above.

### 12. FAILURE CASES
- `[FAIL-SAV-001]`: **Corrupt Save File**: Invalid JSON syntax on load. Result: `SaveSystem` renames corrupt file to `.bak` and generates fresh save.

### 13. CROSS REFERENCES
- [SCENE_TRANSITION_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SCENE_TRANSITION_SPEC.md) `[SPEC-115]`

### 14. CHANGE HISTORY
- **v4.0 (2026-07-25)**: Created canonical SPEC-119 defining JSON Schema (Draft-07) for save data serialization.
