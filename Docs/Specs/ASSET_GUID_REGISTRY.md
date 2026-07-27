# ASSET_GUID_REGISTRY.md — Asset Addressable & Prefab Registry
## Spec ID: SPEC-126
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Establishes a 100% deterministic lookup table linking human-readable asset aliases to Unity Addressable keys and project relative paths.

### 2. SCOPE
Applies to all props, materials, prefabs, sound banks, and UI assets used by autonomous level builders (`EchoesNewProductionBuilder.cs`).

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`).

### 4. DEFINITIONS
- `Alias`: Unique semantic key identifying an asset (e.g. `Prop_Desk_01`).
- `Addressable Key`: String identifier registered in Unity Addressable System.

### 5. INPUTS
- [asset_guid_registry.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/catalogs/asset_guid_registry.yaml) `[SPEC-EXEC-AST]`

### 6. OUTPUTS
- Resolved Addressable/GUID paths for level instantiation.

### 7. RULES
- `[RULE-AST-001]`: Every prefab used in a level blueprint MUST exist in the GUID registry (`asset_guid_registry.yaml`).
- `[RULE-AST-002]`: Unregistered asset aliases are strictly prohibited in level blueprints.

### 8. ALGORITHMS
See canonical YAML catalog at `Docs/ExecutableSpecs/catalogs/asset_guid_registry.yaml`.

#### Table 8.1: Material Token GUID Registry
| Material Token Alias | Physical Asset Path | Unity GUID |
|---|---|---|
| `Mat_Token_institutional-teal` | `Assets/Materials/Tokens/Mat_Token_institutional-teal.mat` | `mat001teal000000000000000000001` |
| `Mat_Token_amber-warmth` | `Assets/Materials/Tokens/Mat_Token_amber-warmth.mat` | `mat002amber00000000000000000002` |
| `Mat_Token_echo-cyan` | `Assets/Materials/Tokens/Mat_Token_echo-cyan.mat` | `mat003cyan000000000000000000003` |
| `Mat_Token_concrete-cold` | `Assets/Materials/Tokens/Mat_Token_concrete-cold.mat` | `mat004conc000000000000000000004` |
| `Mat_Token_void-purple` | `Assets/Materials/Tokens/Mat_Token_void-purple.mat` | `mat005purp000000000000000000005` |

### 9. CONSTRAINTS
- `[CONS-AST-001]`: Non-existent paths raise build error `ERR-ASSET-404`.

### 10. VALIDATION
- `[VAL-AST-001]`: `LevelValidator.cs` parses `asset_guid_registry.yaml` and asserts 100% path resolution.

### 11. EXAMPLES
- `Prop_Desk_01` mapped to `Assets/Prefabs/Props/Furniture/Prop_Desk_01.prefab`.

### 12. FAILURE CASES
- `[FAIL-AST-001]`: Broken GUID or missing path causing missing prefab reference at level build time.

### 13. CROSS REFERENCES
- [asset_guid_registry.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/catalogs/asset_guid_registry.yaml)

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Initial draft.
- **v4.0 (2026-07-25)**: Updated Spec ID to SPEC-126 and linked canonical executable catalog.
