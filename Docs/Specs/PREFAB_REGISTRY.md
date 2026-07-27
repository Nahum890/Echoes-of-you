# PREFAB_REGISTRY.md — Prefab Alias & Addressables Registry Specification
## Spec ID: SPEC-123
## Version: 5.0 (AI-Executable)

---

### 1. PURPOSE
Defines explicit asset alias-to-filepath mapping, Addressable keys, and immutable prefab references for AI builders in *Echoes of You 2.0*. Prohibits partial name matching (`string.Find`) or loose `Resources.Load()` calls.

### 2. SCOPE
Applies to `EchoesNewProductionBuilder.cs`, `EchoesModuleFactory.cs`, `EchoesPropDecorator.cs`, and `Addressables` group catalogs.

### 3. AUTHORITY
Level 4 (Declarative Specs). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `PROP_LIBRARY.md` (`SPEC-005`). Replaces `ASSET_CATALOG.md`. Canonical GUID mapping is in `Docs/Specs/ASSET_GUID_REGISTRY.yaml` (`SPEC-126`).

### 4. DEFINITIONS
- `Prefab Alias`: Standardized code identifier mapping 1:1 to an immutable Unity project path (`Assets/Prefabs/...`).
- `Addressable Key`: Unity Addressables catalog entry for runtime loading.

### 5. INPUTS
- [PROP_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PROP_LIBRARY.md) `[SPEC-005]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`
- [ASSET_GUID_REGISTRY.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ASSET_GUID_REGISTRY.yaml) `[SPEC-126]` — Single source of truth for alias↔GUID mapping.

### 6. OUTPUTS
- Resolved Addressable keys and GameObject instances during build passes.

### 7. RULES
- `[RULE-REG-001]`: **Immutable Path Lookup** — AI agents and automated scripts MUST resolve prefabs using exact `alias` entries from `ASSET_GUID_REGISTRY.yaml`. Partial string searching or guessing paths is strictly prohibited.
- `[RULE-REG-002]`: **Addressable Key Parity** — 100% of prefabs listed in `ASSET_GUID_REGISTRY.yaml` MUST be registered in Unity Addressables group `Default Local Group`.
- `[RULE-REG-003]`: **GUID Authority** — The GUID value in `ASSET_GUID_REGISTRY.yaml` is the definitive identifier. Prefab paths in this document are informational; GUID is authoritative.

### 8. ALGORITHMS
Canonical alias-to-GUID mapping is defined in `ASSET_GUID_REGISTRY.yaml`. This document provides usage rules only.

#### Usage Pattern in C#
```csharp
// CORRECT: Resolve via alias through Addressables
GameObject prefab = Addressables.LoadAssetAsync<GameObject>("EchoPrefab").WaitForCompletion();

// CORRECT: Resolve via GUID through AssetDatabase (Editor only)
string guid = "8d38dfd767baaa04cb84cc974dc6ea4c";
string path = AssetDatabase.GUIDToAssetPath(guid);
GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

// PROHIBITED: Partial name search
// GameObject prefab = Resources.Load<GameObject>("Echo"); // FAIL-REG-01
```

### 9. CONSTRAINTS
- `[CONS-REG-001]`: Prohibido instantiating prefabs using dynamic string concatenation without registering the alias in `ASSET_GUID_REGISTRY.yaml`.

### 10. VALIDATION
- `[VAL-REG-001]`: Static validator asserts 100% of aliases in `ASSET_GUID_REGISTRY.yaml` exist on disk.
- `[VAL-REG-002]`: Build pipeline asserts all aliases are registered in Addressables `Default Local Group`.

### 11. EXAMPLES
See `ASSET_GUID_REGISTRY.yaml` for complete 100% synchronized alias↔GUID mapping.

### 12. FAILURE CASES
- `[FAIL-REG-001]`: **Partial Name Resolution** — Agent uses `Resources.Load("Echo")` instead of Addressable key. Result: Build pipeline aborts with `FAIL-REG-01`.

### 13. CROSS REFERENCES
- [ASSET_GUID_REGISTRY.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ASSET_GUID_REGISTRY.yaml) `[SPEC-126]`
- [PROP_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PROP_LIBRARY.md) `[SPEC-005]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`

### 14. CHANGE HISTORY
- **v4.0 (2026-07-25)**: Created canonical SPEC-123 for Prefab Registry.
- **v5.0 (2026-07-25)**: Removed inline alias table. Delegated to `ASSET_GUID_REGISTRY.yaml` as single source of truth. Added GUID authority rule.

(End of file - total 58 lines)