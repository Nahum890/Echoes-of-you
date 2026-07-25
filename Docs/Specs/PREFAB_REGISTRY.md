# PREFAB_REGISTRY.md — Prefab Alias & Addressables Registry Specification
## Spec ID: SPEC-123
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Defines explicit asset alias-to-filepath mapping, Addressable keys, and immutable prefab references for AI builders in *Echoes of You 2.0*. Prohibits partial name matching (`string.Find`) or loose `Resources.Load()` calls.

### 2. SCOPE
Applies to `EchoesNewProductionBuilder.cs`, `EchoesModuleFactory.cs`, `EchoesPropDecorator.cs`, and `Addressables` group catalogs.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `PROP_LIBRARY.md` (`SPEC-005`). Replaces `ASSET_CATALOG.md`.

### 4. DEFINITIONS
- `Prefab Alias`: Standardized code identifier mapping 1:1 to an immutable Unity project path (`Assets/Prefabs/...`).

### 5. INPUTS
- [PROP_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PROP_LIBRARY.md) `[SPEC-005]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`

### 6. OUTPUTS
- Resolved Addressable keys and GameObject instances during build passes.

### 7. RULES

- `[RULE-REG-001]`: **Immutable Path Lookup**: AI agents and automated scripts MUST resolve prefabs using exact `alias`, `path`, or `addressable_key` entries from Table 8.1. Partial string searching or guessing paths is strictly prohibited.
- `[RULE-REG-002]`: **Addressable Key Parity**: 100% of prefabs listed in Table 8.1 MUST be registered in Unity Addressables group `Default Local Group`.

### 8. ALGORITHMS

#### Algorithm 8.1: Master Prefab Registry Schema

```yaml
# Mapeo canónico de alias a rutas de prefab Unity
# Un agente NUNCA debe usar string.Find() o Resources.Load() con nombres parciales.
# Siempre usar la ruta exacta o la Addressable key.
registry:
  - alias: "PlayerPrefab"
    path: "Assets/Prefabs/Player/Player.prefab"
    addressable_key: "Player"
  - alias: "EchoPrefab"
    path: "Assets/Prefabs/Echo/EchoBody.prefab"
    addressable_key: "EchoBody"
  - alias: "Prop_Desk_01"
    path: "Assets/Prefabs/Props/Furniture/Prop_Desk_01.prefab"
    addressable_key: "Prop_Desk_01"
  - alias: "Prop_Locker_01"
    path: "Assets/Prefabs/Props/Furniture/Prop_Locker_01.prefab"
    addressable_key: "Prop_Locker_01"
  - alias: "Prop_Coat"
    path: "Assets/Prefabs/Narrative/Prop_Coat.prefab"
    addressable_key: "Prop_Coat"
  - alias: "PuzzleWirePrefab"
    path: "Assets/Prefabs/Puzzle/PuzzleWire.prefab"
    addressable_key: "PuzzleWire"
  - alias: "PressurePlatePrefab"
    path: "Assets/Prefabs/Puzzle/PressurePlate.prefab"
    addressable_key: "PressurePlate"
  - alias: "DoorPrefab"
    path: "Assets/Prefabs/Architecture/Door.prefab"
    addressable_key: "Door"

rule: "Prohibido referencia por nombre parcial. La ruta 'path' es inmutable."
```

### 9. CONSTRAINTS
- `[CONS-REG-001]`: Prohibido instantiating prefabs using dynamic string concatenation without registering the alias in Table 8.1.

### 10. VALIDATION
- `[VAL-REG-001]`: Static validator asserts 100% of paths in Table 8.1 exist on disk.

### 11. EXAMPLES
- Registry schema above.

### 12. FAILURE CASES
- `[FAIL-REG-001]`: **Missing Prefab Asset**: File specified in path does not exist on disk. Result: `FAIL-REG-01`.

### 13. CROSS REFERENCES
- [PROP_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PROP_LIBRARY.md) `[SPEC-005]`

### 14. CHANGE HISTORY
- **v4.0 (2026-07-25)**: Created canonical SPEC-123 mapping 8 critical pipeline prefabs to immutable paths and Addressable keys.
