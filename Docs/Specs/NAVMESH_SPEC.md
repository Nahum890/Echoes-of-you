# NAVMESH_SPEC.md — Navigation Mesh Baking & Pathfinding Specifications
## Spec ID: SPEC-114
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Defines exact agent baking parameters, area cost definitions, off-mesh link restrictions, and NavMesh storage locations for level generation in *Echoes of You 2.0*.

### 2. SCOPE
Applies to Unity NavMeshSurface baking during Pass 2 of `EchoesNewProductionBuilder.cs` and runtime path queries.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `LEVEL_PIPELINE.md` (`SPEC-201`).

### 4. DEFINITIONS
- `Humanoid Agent`: Standard Unity NavMesh agent matching player/Echo capsule footprint.
- `Min Region Area`: Minimum isolated surface area ($2.0\text{ m}^2$) required to keep a NavMesh island.

### 5. INPUTS
- [LEVEL_PIPELINE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LEVEL_PIPELINE.md) `[SPEC-201]`
- [LOCOMOTION_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LOCOMOTION_SPEC.md) `[SPEC-113]`

### 6. OUTPUTS
- Baked `NavMeshData` asset saved in `Assets/Data/Levels/NavMesh/`.
- Coverage metrics for `LevelValidator.cs`.

### 7. RULES

- `[RULE-NAV-001]`: **Agent Geometry Alignment**: NavMesh Humanoid agent MUST set `radius = 0.35m`, `height = 1.80m`, `maxSlope = 45.0^\circ`, `stepHeight = 0.30m`.
- `[RULE-NAV-002]`: **Jump Links Disabled**: `dropHeight` MUST equal $2.0\text{m}$, `jumpDistance` MUST equal $0.0\text{m}$. Off-mesh links are strictly prohibited (`off_mesh_links_enabled = false`).
- `[RULE-NAV-003]`: **Min Region Area Clean**: `minRegionArea` MUST equal $2.0\text{ m}^2$. Isolated islands $< 2.0\text{ m}^2$ MUST be discarded automatically during bake.

### 8. ALGORITHMS

#### Algorithm 8.1: Master NavMesh Configuration Schema (HALT-8)

```yaml
navmesh_bake_config:
  agent_type: "Humanoid"
  agent_radius_m: 0.35
  agent_height_m: 1.80
  max_slope_deg: 45.0
  step_height_m: 0.30
  drop_height_m: 2.0
  jump_distance_m: 0.0    # sin salto NavMesh; Echo y Player saltan manualmente
  bake_mode: "Editor_Static"    # horneado en editor durante Pass 2
  navmesh_data_path: "Assets/Data/Levels/NavMesh/"
  min_region_area: 2.0     # m²: elimina islas de NavMesh menores a 2m²
  off_mesh_links_enabled: false

area_types:
  - name: "Walkable"
    index: 0
    cost: 1.0
  - name: "NotWalkable"
    index: 1
    cost: 9999.0
  - name: "Jump"
    index: 2
    cost: 2.0   # para gaps de 0.5m a 1.0m
```

### 9. CONSTRAINTS
- `[CONS-NAV-001]`: Prohibido runtime dynamic NavMesh baking during gameplay frames.

### 10. VALIDATION
- `[VAL-NAV-001]`: `LevelValidator.cs` asserts NavMesh walkable surface covers $\ge 95\%$ of playable room floors (`VAL-I-01`).

### 11. EXAMPLES
- Config schema above.

### 12. FAILURE CASES
- `[FAIL-NAV-001]`: **NavMesh Gap**: Door threshold missing NavMesh walkable tile. Result: Validator flags `FAIL-NAV-01`.

### 13. CROSS REFERENCES
- [LEVEL_PIPELINE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LEVEL_PIPELINE.md) `[SPEC-201]`
- [LEVEL_VALIDATOR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Validation/LEVEL_VALIDATOR.md) `[SPEC-301]`

### 14. CHANGE HISTORY
- **v4.0 (2026-07-25)**: Created canonical SPEC-114 incorporating HALT-8 NavMesh baking parameters.
