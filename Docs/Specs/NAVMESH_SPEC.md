# NAVMESH_SPEC.md — Navigation Mesh Baking & Pathfinding Specifications
## Spec ID: SPEC-114
## Version: 5.0 (AI-Executable)

---

### 1. PURPOSE
Defines exact agent baking parameters, area cost definitions, off-mesh link restrictions, and NavMesh storage locations for level generation in *Echoes of You 2.0*.

### 2. SCOPE
Applies to Unity NavMeshSurface baking during Pass 2 of `EchoesNewProductionBuilder.cs` and runtime path queries.

### 3. AUTHORITY
Level 4 (Declarative Specs). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `LEVEL_PIPELINE.md` (`SPEC-201`). Runtime data contract defined in `Docs/ExecutableSpecs/rules/navmesh_spec.yaml` (`SPEC-EXEC-NAV`).

### 4. DEFINITIONS
- `Humanoid Agent`: Standard Unity NavMesh agent matching player/Echo capsule footprint (radius `0.35m`, height `1.80m`).
- `Min Region Area`: Minimum isolated surface area ($2.0\text{ m}^2$) required to keep a NavMesh island.
- `GhostPlatform Area Cost`: NavMesh area cost for platforms only traversable during Echo playback = `1.5`.

### 5. INPUTS
- [LEVEL_PIPELINE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LEVEL_PIPELINE.md) `[SPEC-201]`
- [LOCOMOTION_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LOCOMOTION_SPEC.md) `[SPEC-113]`
- [CONSTANTS_REGISTRY.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CONSTANTS_REGISTRY.yaml) `[SPEC-124]` — agent radius/height

### 6. OUTPUTS
- Baked `NavMeshData` asset saved in `Assets/Data/Levels/NavMesh/`.
- Coverage metrics for `LevelValidator.cs`.

### 7. RULES
- `[RULE-NAV-001]`: **Agent Geometry Alignment** — NavMesh Humanoid agent MUST set `radius = 0.35m`, `height = 1.80m`, `maxSlope = 45.0°`, `stepHeight = 0.30m`.
- `[RULE-NAV-002]`: **Jump Links Disabled** — `dropHeight` MUST equal $2.0\text{m}$, `jumpDistance` MUST equal $0.0\text{m}$. Off-mesh links are strictly prohibited (`off_mesh_links_enabled = false`).
- `[RULE-NAV-003]`: **Min Region Area Clean** — `minRegionArea` MUST equal $2.0\text{ m}^2$. Isolated islands $< 2.0\text{ m}^2$ MUST be discarded automatically during bake.
- `[RULE-NAV-004]`: **GhostPlatform Area Cost** — NavMesh area index 4 (GhostPlatform) MUST have cost `1.5` (traversable during Echo playback only).

### 8. ALGORITHMS
Master NavMesh Configuration Schema is defined in `navmesh_spec.yaml`. The Markdown document does not duplicate numeric parameters.

### 9. CONSTRAINTS
- `[CONS-NAV-001]`: Prohibido runtime dynamic NavMesh baking during gameplay frames.
- `[CONS-NAV-002]`: Prohibido `off_mesh_links_enabled = true`.

### 10. VALIDATION
- `[VAL-NAV-001]`: `LevelValidator.cs` asserts NavMesh walkable surface covers $\ge 95\%$ of playable room floors (`VAL-I-01`).
- `[VAL-NAV-002]`: `LevelValidator.cs` asserts door thresholds have NavMesh walkable tiles (`FAIL-NAV-01`).

### 11. EXAMPLES
See `navmesh_spec.yaml` for canonical configuration schema.

### 12. FAILURE CASES
- `[FAIL-NAV-001]`: **NavMesh Gap** — Door threshold missing NavMesh walkable tile. Result: Validator flags `FAIL-NAV-01`.

### 13. CROSS REFERENCES
- [navmesh_spec.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/rules/navmesh_spec.yaml) `[SPEC-EXEC-NAV]`
- [LEVEL_PIPELINE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LEVEL_PIPELINE.md) `[SPEC-201]`
- [LEVEL_VALIDATOR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Validation/LEVEL_VALIDATOR.md) `[SPEC-301]`
- [CONSTANTS_REGISTRY.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CONSTANTS_REGISTRY.yaml) `[SPEC-124]`

### 14. CHANGE HISTORY
- **v4.0 (2026-07-25)**: Created canonical SPEC-114 incorporating HALT-8 NavMesh baking parameters.
- **v5.0 (2026-07-25)**: Moved numeric schema to `navmesh_spec.yaml`. Added GhostPlatform area cost = 1.5. Linked agent radius/height to CONSTANTS_REGISTRY.

(End of file - total 74 lines)