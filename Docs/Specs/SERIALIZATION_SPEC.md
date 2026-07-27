# SERIALIZATION_SPEC.md — Game State & Save Data Schema
## Spec ID: SPEC-206
## Version: 1.0 (AI-Executable)

---

### 1. PURPOSE
Defines the serialization contracts, JSON/YAML data structures, and persistence rules for game saves in *Echoes of You 2.0*.

### 2. SCOPE
Applies to save files, memory unlocks, level progression, and Echo recording frame serialization.

### 3. AUTHORITY
Level 3 Declarative Specification (supported and overridden at runtime by `Docs/ExecutableSpecs/schemas/save_schema.yaml`).

### 4. DEFINITIONS
- `SaveState`: Root serialized JSON payload containing user progress.
- `EchoFrame`: Vector timestamp snapshot containing position `[x, y, z]`, rotation `[x, y, z, w]`, and interaction flag.

### 5. INPUTS
- Game state controllers (`GameStateController.cs`, `EchoRecorder.cs`).

### 6. OUTPUTS
- Persistent JSON save binary stored in `Application.persistentDataPath`.

### 7. RULES
- `[RULE-SER-001]`: Save state MUST include `currentLevelIndex` (int $1 \dots 15$).
- `[RULE-SER-002]`: Save state MUST include `unlockedMemories` string array.
- `[RULE-SER-003]`: Recorded frames MUST serialize `position`, `rotation`, and `interactState` per timestamp.

### 8. ALGORITHMS
```json
{
  "version": "1.0",
  "currentLevelIndex": 1,
  "unlockedMemories": ["MEM_L01_01"],
  "recordedEchoFrames": [
    {
      "timestamp": 0.0,
      "position": [0.0, 1.0, 0.0],
      "rotation": [0.0, 0.0, 0.0, 1.0],
      "interactState": false
    }
  ]
}
```

### 9. CONSTRAINTS
- Unrecognized fields must be discarded on deserialize without causing exceptions.

### 10. VALIDATION
- Schema validated against `save_schema.yaml`.

### 11. EXAMPLES
- Standard save JSON payload structure.

### 12. FAILURE CASES
- Corrupted frame arrays abort deserialization and restore auto-backup save.

### 13. CROSS REFERENCES
- [save_schema.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/schemas/save_schema.yaml)

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Initial deterministic spec release.
