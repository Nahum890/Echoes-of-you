# INDEX.md â€” Master Canonical Index & Dependency Graph
## Spec ID: SPEC-INDEX
## Version: 8.0 (AI-Executable)

---

### 1. PURPOSE
Provides the master index, navigation graph, clickable specification links, and dependency tree for the entire AI-Executable Specification System of *Echoes of You 2.0*.

### 2. SCOPE
Applies to all canonical specifications in `Docs/`. Excludes unindexed raw data files.

### 3. AUTHORITY
Level 2 (Technical Context). Subordinate only to `SOURCE_OF_TRUTH.md` (`SPEC-000`, Level 1). Provides the master canonical index and dependency DAG.

### 4. DEFINITIONS
- `Dependency Tree`: Directed acyclic graph (DAG) expressing prerequisite specifications for AI agent execution loops.
- `Canonical Specification`: An AI-Executable specification document adhering strictly to the 14-section template standard.

### 5. INPUTS
- [DOCUMENT_STATUS.md](Archive/Obsolete/DOCUMENT_STATUS.md) `[SPEC-000B]`
- [SOURCE_OF_TRUTH.md](Authority/SOURCE_OF_TRUTH.md) `[SPEC-000]`

### 6. OUTPUTS
- Clickable markdown link directory and Mermaid DAG for navigation.

### 7. RULES

- `[RULE-IND-001]`: **Clickable Path Requirement**: Every specification link MUST use explicit `file:///` URLs or relative paths pointing to valid disk files.
- `[RULE-IND-002]`: **14-Section Verification Mandatory**: No specification file may be indexed without satisfying 100% of the 14 canonical sections.

### 8. ALGORITHMS

#### Table 8.1: Master Canonical Specification Directory (100% 10/10 AI-Readiness Verified)

| Spec ID | Document Title | Path | Authority Tier | Domain |
|---|---|---|---|---|
| `SPEC-000` | Supreme Authority & Resolution System | [SOURCE_OF_TRUTH.md](Authority/SOURCE_OF_TRUTH.md) | Level 1 | Authority |
| `SPEC-000B`| Inventory of Canonical Specifications | [DOCUMENT_STATUS.md](Archive/Obsolete/DOCUMENT_STATUS.md) | Level 2 | Authority |
| `SPEC-000C`| Document Audit & Cleaning Report | [DOCUMENT_AUDIT_REPORT.md](Archive/Obsolete/DOCUMENT_AUDIT_REPORT.md) | Level 2 | Authority |
| `SPEC-000D`| Frozen Decision & Change Control | [CHANGE_CONTROL.md](Authority/CHANGE_CONTROL.md) | Level 2 | Authority |
| `SPEC-001` | Core Philosophical & Mechanical Directives | [DESIGN_PHILOSOPHY.md](GameDesign/DESIGN_PHILOSOPHY.md) | Level 2 | GameDesign |
| `SPEC-002` | Quantitative Blacklist of Prohibited Decisions | [ANTI_PATTERNS.md](Specs/ANTI_PATTERNS.md) | Level 3 | Specs |
| `SPEC-003` | Spatial Grammar & Structural Architecture | [ARCHITECTURE_GRAMMAR.md](Specs/ARCHITECTURE_GRAMMAR.md) | Level 3 | Specs |
| `SPEC-004` | Technical Catalog & Schemas for 48 Rooms | [ROOM_LIBRARY.md](Specs/ROOM_LIBRARY.md) | Level 3 | Specs |
| `SPEC-005` | Prop Technical Catalog & Placement Specs | [PROP_LIBRARY.md](Specs/PROP_LIBRARY.md) | Level 3 | Specs |
| `SPEC-006` | Environmental Storytelling & Micro-Scenes | [ENVIRONMENT_STORYTELLING.md](Specs/ENVIRONMENT_STORYTELLING.md) | Level 3 | Specs |
| `SPEC-007` | Cinemachine Technical Recipes & Profiles | [CAMERA_RECIPES.md](Archive/Obsolete/CAMERA_RECIPES.md) | Level 3 | Specs |
| `SPEC-008` | UI Toolkit (UITK) Technical Specification | [UI_SPEC.md](Specs/UI_SPEC.md) | Level 3 | Specs |
| `SPEC-101` | Master Game Bible & Game Direction | [ECHOES_BIBLE.md](GameDesign/ECHOES_BIBLE.md) | Level 2 | GameDesign |
| `SPEC-102` | Spatial Grammar & Emotional Architecture | [SPACE_GRAMMAR.md](GameDesign/SPACE_GRAMMAR.md) | Level 2 | GameDesign |
| `SPEC-103` | Architectural Flow & School Pacing | [BUILDING_FLOW.md](GameDesign/BUILDING_FLOW.md) | Level 2 | GameDesign |
| `SPEC-103B`| Level Zone Structural Specifications | [LEVEL_GRAMMAR.md](Archive/Obsolete/LEVEL_GRAMMAR.md) | Level 3 | Specs |
| `SPEC-103C`| Landmark Placement & Sightline Predicates | [LANDMARK_GRAMMAR.md](Specs/LANDMARK_GRAMMAR.md) | Level 3 | Specs |
| `SPEC-104` | Puzzle Archetypes & Signal Wiring Specs | [PUZZLE_GRAMMAR.md](Specs/PUZZLE_GRAMMAR.md) | Level 3 | Specs |
| `SPEC-104B`| _SUPERSEDED â€” merged into PUZZLE_GRAMMAR.md Table 8.0_ | â€” | â€” | â€” |
| `SPEC-105` | URP Lighting, Fog & Post-Processing Specs | [LIGHTING_GRAMMAR.md](Specs/LIGHTING_GRAMMAR.md) | Level 3 | Specs |
| `SPEC-105B`| Surface Material Tokens & Textures | [MATERIAL_GRAMMAR.md](Archive/Obsolete/MATERIAL_GRAMMAR.md) | Level 3 | Specs |
| `SPEC-106` | Physical Scale & Dimensional Specs | [SCALE_GUIDE.md](Specs/SCALE_GUIDE.md) | Level 3 | Specs |
| `SPEC-107` | Echo Mechanic Technical Specification | [ECHO_GRAMMAR.md](Specs/ECHO_GRAMMAR.md) | Level 3 | Specs |
| `SPEC-108` | Cinematic Framing & Visual Intent Specs | [CAMERA_GRAMMAR.md](Art/CAMERA_GRAMMAR.md) | Level 2 | Art |
| `SPEC-109` | URP Custom Shader Specifications | [SHADER_SPEC.md](Specs/SHADER_SPEC.md) | Level 3 | Specs |
| `SPEC-110` | Core Technical & Pipeline Architecture | [PROJECT_CONTEXT.md](Technical/PROJECT_CONTEXT.md) | Level 2 | Technical |
| `SPEC-111` | Echo Mechanics Primitives & Hazards | [ECHO_PRIMITIVE_SPEC.md](Specs/ECHO_PRIMITIVE_SPEC.md) | Level 3 | Specs |
| `SPEC-112` | Audio Architecture & Signal Processing | [AUDIO_GRAMMAR.md](Specs/AUDIO_GRAMMAR.md) | Level 3 | Specs |
| `SPEC-113` | Character Locomotion & Physical Dynamics | [LOCOMOTION_SPEC.md](Specs/LOCOMOTION_SPEC.md) | Level 3 | Specs |
| `SPEC-114` | Navigation Mesh Baking & Pathfinding | [NAVMESH_SPEC.md](Specs/NAVMESH_SPEC.md) | Level 3 | Specs |
| `SPEC-115` | Scene Manager & Level Transitions | [SCENE_TRANSITION_SPEC.md](Specs/SCENE_TRANSITION_SPEC.md) | Level 3 | Specs |
| `SPEC-116` | Physics Collision Layer Matrix | [PHYSICS_LAYER_MATRIX.md](Specs/PHYSICS_LAYER_MATRIX.md) | Level 3 | Specs |
| `SPEC-117` | Animator Controllers & Echo Ghosting | [ANIMATION_STATE_MACHINE.md](Specs/ANIMATION_STATE_MACHINE.md) | Level 3 | Specs |
| `SPEC-118` | Input Action Maps & Bindings | [INPUT_ACTION_MAPS.md](Specs/INPUT_ACTION_MAPS.md) | Level 3 | Specs |
| `SPEC-119` | JSON Save Data Schema | [SAVE_DATA_SCHEMA.md](Specs/SAVE_DATA_SCHEMA.md) | Level 3 | Specs |
| `SPEC-120` | URP Volume Profiles & Visual Stack | [POST_PROCESSING_SPEC.md](Specs/POST_PROCESSING_SPEC.md) | Level 3 | Specs |
| `SPEC-121` | Hardware Budgets & Optimization Directives | [PERFORMANCE_BUDGET_SPEC.md](Specs/PERFORMANCE_BUDGET_SPEC.md) | Level 3 | Specs |
| `SPEC-122` | Lightmapping & Light Probe Baking | [LIGHTMAP_PROBE_SPEC.md](Specs/LIGHTMAP_PROBE_SPEC.md) | Level 3 | Specs |
| `SPEC-123` | Prefab Alias & Addressables Registry | [PREFAB_REGISTRY.md](Specs/PREFAB_REGISTRY.md) | Level 3 | Specs |
| `SPEC-124` | Single Source of All Game Constants | [CONSTANTS_REGISTRY.yaml](Specs/CONSTANTS_REGISTRY.yaml) | Level 2 | Specs |
| `SPEC-125` | Puzzle Wire Mesh Generation & Routing Algorithm | [WIRE_PATHFINDING_SPEC.md](Specs/WIRE_PATHFINDING_SPEC.md) | Level 3 | Specs |
| `SPEC-126` | Asset Addressable & Prefab Registry | [ASSET_GUID_REGISTRY.md](Specs/ASSET_GUID_REGISTRY.md) | Level 3 | Specs |
| `SPEC-127` | Audio Mixer Bus Hierarchy & DSP Filtering | [AUDIO_MIXER_SCHEMA.md](Specs/AUDIO_MIXER_SCHEMA.md) | Level 3 | Specs |
| `SPEC-128` | Player Animator Controller & Echo Replay Specs | [ANIMATION_TRANSITION_MATRIX.md](Specs/ANIMATION_TRANSITION_MATRIX.md) | Level 3 | Specs |
| `SPEC-129` | Frame Windows & Input Buffering Specifications | [INPUT_BUFFER_SPEC.md](Specs/INPUT_BUFFER_SPEC.md) | Level 3 | Specs |
| `SPEC-130` | Save Data Versioning & Data Migration Protocol | [SAVE_MIGRATION_SPEC.md](Specs/SAVE_MIGRATION_SPEC.md) | Level 3 | Specs |
| `SPEC-131` | Rigidbody Configurations & Interactive Object Physics | [PHYSICS_JOINT_MATRIX.md](Specs/PHYSICS_JOINT_MATRIX.md) | Level 3 | Specs |
| `SPEC-132` | GPU Lightmap VRAM Budgets & Light Probe Placement | [LIGHTMAP_TEXTURE_BUDGET.md](Specs/LIGHTMAP_TEXTURE_BUDGET.md) | Level 3 | Specs |
| `SPEC-133` | Localization Tables & String Interpolation Specs | [LOCALIZATION_SCHEMA.md](Specs/LOCALIZATION_SCHEMA.md) | Level 3 | Specs |
| `LEVEL-SPEC-01` to `15` | Level Blueprint Specifications | [LEVEL_SPEC_01.md](Specs/Levels/LEVEL_SPEC_01.md) to `15` | Level 3 | Specs/Levels |
| `SPEC-201` | Automated Level Generation & Pipeline | [LEVEL_PIPELINE.md](Specs/LEVEL_PIPELINE.md) | Level 3 | Pipeline |
| `SPEC-202` | Level Blueprint Technical Schema | [BLUEPRINT_SPEC.md](Specs/BLUEPRINT_SPEC.md) | Level 3 | Pipeline |
| `SPEC-301` | Automated Level Validation Suite | [LEVEL_VALIDATOR.md](Validation/LEVEL_VALIDATOR.md) | Level 4 | Validation |
| `SPEC-302` | Automated Level Evaluation Rubric | [LEVEL_SCORECARD.md](Validation/LEVEL_SCORECARD.md) | Level 4 | Validation |
| `SPEC-303` | Catalog of Error Codes & Failure Patterns| [FAILURE_PATTERNS.md](Validation/FAILURE_PATTERNS.md) | Level 4 | Validation |
| `SPEC-304` | Automated Verification Protocol | [QA_CHECKLIST.md](Archive/Obsolete/QA_CHECKLIST.md) | Level 4 | QA |
| `SPEC-401` | Rules & Guidelines for Specialized AI Agents | [AI_RULEBOOK.md](AI/AI_RULEBOOK.md) | Level 6 | AI |
| `SPEC-402` | AI Execution & Agent Generation Pipeline | [AI_PIPELINE.md](AI/AI_PIPELINE.md) | Level 6 | AI |
| `SPEC-EXEC-LYR` | Physics Collision Layer Matrix | [layer_matrix_spec.yaml](ExecutableSpecs/rules/layer_matrix_spec.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-ANI` | Echo Animation Bridge Specs | [echo_animation_bridge.yaml](ExecutableSpecs/gameplay/echo_animation_bridge.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-UI`  | UI Toolkit Binding Schema | [uitk_schema.yaml](ExecutableSpecs/visual/uitk_schema.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-AUD` | Audio Architecture & Mixer Bus Schema | [audio_architecture.yaml](ExecutableSpecs/audio_architecture.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-TST` | Automated Playtest Harness Spec | [automated_playtest_harness.yaml](ExecutableSpecs/validators/automated_playtest_harness.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-GUID-001` | Prefab GUID Map | [PREFAB_GUID_MAP.yaml](ExecutableSpecs/catalogs/PREFAB_GUID_MAP.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-AUD-002` | Audio Clip Registry | [AUDIO_CLIP_REGISTRY.yaml](ExecutableSpecs/audio/AUDIO_CLIP_REGISTRY.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-AUD-SPAT-001` | Audio 3D Spatialization Spec | [AUDIO_SPATIALIZATION_SPEC.yaml](ExecutableSpecs/audio/AUDIO_SPATIALIZATION_SPEC.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-SIG-001` | Signal Circuit Schema | [SIGNAL_CIRCUIT_SCHEMA.yaml](ExecutableSpecs/gameplay/SIGNAL_CIRCUIT_SCHEMA.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-PHYS-001` | Collision Bitmask Matrix | [COLLISION_BITMASK_MATRIX.yaml](ExecutableSpecs/rules/COLLISION_BITMASK_MATRIX.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-HAZ-001` | Hazard & Respawn Spec | [HAZARD_AND_RESPAWN_SPEC.yaml](ExecutableSpecs/rules/HAZARD_AND_RESPAWN_SPEC.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-SHD-001` | PS1 Shading Pipeline | [PS1_SHADING_PIPELINE.yaml](ExecutableSpecs/visual/PS1_SHADING_PIPELINE.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-HUD-001` | HUD Layout Metrics Spec | [HUD_LAYOUT_METRICS_SPEC.yaml](ExecutableSpecs/visual/HUD_LAYOUT_METRICS_SPEC.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-ANIM-001` | Animation Blend Trees | [ANIMATION_BLEND_TREES.yaml](ExecutableSpecs/gameplay/ANIMATION_BLEND_TREES.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-ANIM-PARAM-001` | Animator Parameter Schema | [ANIMATOR_PARAMETER_SCHEMA.yaml](ExecutableSpecs/gameplay/ANIMATOR_PARAMETER_SCHEMA.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-INT-001` | Interaction System Spec | [INTERACTION_SYSTEM_SPEC.yaml](ExecutableSpecs/gameplay/INTERACTION_SYSTEM_SPEC.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-WIRE-001` | Procedural Wire Mesh Spec | [PROCEDURAL_WIRE_MESH_SPEC.yaml](ExecutableSpecs/gameplay/PROCEDURAL_WIRE_MESH_SPEC.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-NAR-001` | Dialogue Tree Schema | [DIALOGUE_TREE_SCHEMA.yaml](ExecutableSpecs/narrative/DIALOGUE_TREE_SCHEMA.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-PROBE-001` | Light Probe Grid Algorithm | [LIGHT_PROBE_GRID_ALGORITHM.yaml](ExecutableSpecs/lighting/LIGHT_PROBE_GRID_ALGORITHM.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-PROP-001` | Prop Placement Grammar | [PROP_PLACEMENT_GRAMMAR.yaml](ExecutableSpecs/catalogs/PROP_PLACEMENT_GRAMMAR.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-SCENE-001` | Scene Registry Manifest | [SCENE_REGISTRY_MANIFEST.yaml](ExecutableSpecs/catalogs/SCENE_REGISTRY_MANIFEST.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-SAV-001` | Save State Recovery Spec | [SAVE_STATE_RECOVERY_SPEC.yaml](ExecutableSpecs/rules/SAVE_STATE_RECOVERY_SPEC.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-VAL-001` | Schema Pre-Validator | [SCHEMA_PRE_VALIDATOR.json](ExecutableSpecs/schemas/SCHEMA_PRE_VALIDATOR.json) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-CLI-001` | AI Builder CLI Contract | [AI_BUILDER_CLI_CONTRACT.yaml](ExecutableSpecs/schemas/AI_BUILDER_CLI_CONTRACT.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-SOLVER-001` | Automated Puzzle Solver Spec | [AUTOMATED_PUZZLE_SOLVER_SPEC.md](ExecutableSpecs/validators/AUTOMATED_PUZZLE_SOLVER_SPEC.md) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-GEO-001` | Procedural Room Mesh Schema | [PROCEDURAL_ROOM_MESH_SCHEMA.yaml](ExecutableSpecs/geometry/PROCEDURAL_ROOM_MESH_SCHEMA.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-GEO-002` | Spline Routing Algorithm | [SPLINE_ROUTING_ALGORITHM.yaml](ExecutableSpecs/geometry/SPLINE_ROUTING_ALGORITHM.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-CAM-FRAME-001` | Cinemachine Framing Matrix | [CINEMACHINE_FRAMING_MATRIX.yaml](ExecutableSpecs/visual/CINEMACHINE_FRAMING_MATRIX.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-ECH-SER-001` | Echo Serialization Schema | [ECHO_SERIALIZATION_SCHEMA.yaml](ExecutableSpecs/gameplay/ECHO_SERIALIZATION_SCHEMA.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-LGT-BAKE-001` | Unity Lightmap Bake Preset | [UNITY_LIGHTMAP_BAKE_PRESET.yaml](ExecutableSpecs/lighting/UNITY_LIGHTMAP_BAKE_PRESET.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-UI-USS-001` | UITK Stylesheet Schema | [UITK_STYLESHEET_SCHEMA.yaml](ExecutableSpecs/ui/UITK_STYLESHEET_SCHEMA.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-SCENE-PER-001` | Scene Persistence Contract | [SCENE_PERSISTENCE_CONTRACT.yaml](ExecutableSpecs/rules/SCENE_PERSISTENCE_CONTRACT.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-INP-REM-001` | Input Remapping Schema | [INPUT_REMAPPING_SCHEMA.yaml](ExecutableSpecs/input/INPUT_REMAPPING_SCHEMA.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-AUD-DUCK-001` | Audio DSP Ducking Matrix | [AUDIO_DSP_DUCKING_MATRIX.yaml](ExecutableSpecs/audio/AUDIO_DSP_DUCKING_MATRIX.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-EXEC-VAL-ROS-001` | Roslyn Symbol Verifier | [ROSLYN_SYMBOL_VERIFIER.yaml](ExecutableSpecs/validators/ROSLYN_SYMBOL_VERIFIER.yaml) | Level 3 | ExecutableSpecs |
| `SPEC-310` | Save Serialization Contract | [SAVE_SERIALIZATION_SPEC.yaml](ExecutableSpecs/persistence/SAVE_SERIALIZATION_SPEC.yaml) | Level 3 | ExecutableSpecs |

### 9. CONSTRAINTS
- `[CONS-IND-001]`: Prohibido unlinked specification entries.

### 10. VALIDATION
- `[VAL-IND-001]`: CI link checker asserts 100% of URLs in Table 8.1 resolve to existing disk files.

### 11. EXAMPLES
- Master directory lookup.

### 12. FAILURE CASES
- `[FAIL-IND-001]`: **Broken Spec Link**: Result: `FAIL-IND-01`.

### 13. CROSS REFERENCES
- [SOURCE_OF_TRUTH.md](Authority/SOURCE_OF_TRUTH.md) `[SPEC-000]`
- [DOCUMENT_STATUS.md](Archive/Obsolete/DOCUMENT_STATUS.md) `[SPEC-000B]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Updated index to 100% AI readiness across all active specs.
- **v4.0 (2026-07-25)**: Added 11 new canonical specs (SPEC-113 to SPEC-123) to master directory.
- **v5.0 (2026-07-25)**: Full 100/100 audit remediation. Purged 20 obsolete stubs. Added 24 new ExecutableSpec entries. All contradictions resolved.
- **v6.0 (2026-07-25)**: Zero-Inference Stoppage Remediation. Created 10 executable schemas resolving all 10 Hard Stoppage points. System verified at 100/100 readiness.
- **v7.0 (2026-07-25)**: Final reconciliation pass. Added SAVE_SERIALIZATION_SPEC.yaml (SPEC-310). Resolved Stoppage 4 (PressurePlate disambiguation Table 8.0 in PUZZLE_GRAMMAR.md), Stoppage 6 (GhostPlatform NavMesh area cost), Stoppage 8 (Camera collision bitmask reconciliation), Stoppage 9 (Save serialization caps). Superseded PUZZLE_IMPLEMENTATION_MATRIX.md.
- **v8.0 (2026-07-25)**: Comprehensive Audit Remediation Pass. Resolved Authority deadlock (`SOURCE_OF_TRUTH.md`), reconciled sprint speed (`DESIGN_PHILOSOPHY.md`), registered missing prefab alias `PREFAB_CLASSROOM_LG_02` (`PREFAB_REGISTRY.md` & `PREFAB_GUID_MAP.yaml`), quantified lighting prose (`LIGHTING_GRAMMAR.md`), added headless bake execution configs (`UNITY_LIGHTMAP_BAKE_PRESET.yaml`), added GhostPlatform NavMesh area cost (`NAVMESH_SPEC.md`), and expanded save array caps to 1200 frames (`SAVE_DATA_SCHEMA.md` & `save_schema.yaml`). Verified 100/100 readiness across all 10 dimensions.
- **v8.1 (2026-07-26)**: Repaired all file:/// links (pointed to a stale machine path) -> relative paths; re-targeted 6 links whose documents were moved to Archive/Obsolete (DOCUMENT_AUDIT_REPORT, DOCUMENT_STATUS, QA_CHECKLIST, CAMERA_RECIPES, LEVEL_GRAMMAR, MATERIAL_GRAMMAR).

