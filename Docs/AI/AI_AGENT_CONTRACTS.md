# AI_AGENT_CONTRACTS.md — Rigid I/O Contracts for Specialized AI Agents
## Spec ID: SPEC-403
## Version: 1.0 (Zero-Inference Compliant)

---

### 1. PURPOSE
Defines rigid Input/Output contracts for each specialized AI agent in the *Echoes of You 2.0* generation pipeline. Each agent MUST read only its authorized inputs, produce exactly its specified outputs, and pass validation before handoff.

### 2. SCOPE
Applies to all automated level generation sessions. Enforced by `AI_RULEBOOK.md` (`SPEC-401`) and `ExecutableSpecValidator.cs`.

### 3. AUTHORITY
Level 5 (AI Protocols). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`).

### 4. AGENT CONTRACTS

#### 4.1 LevelDesignAgent
**Role**: Creates pure metric layout (rooms, corridors, connections) — zero puzzles.

| Field | Specification |
|---|---|
| **Inputs** | `SOURCE_OF_TRUTH.md`, `ANTI_PATTERNS.md`, `ARCHITECTURE_GRAMMAR.md`, `ROOM_LIBRARY.md`, `LEVEL_SPEC_XX.md` |
| **Outputs** | `LEVEL_SPEC_XX.yaml` (layout metric only: module placements, positions, rotations, customData for architecture modules only) |
| **Validation** | `LevelValidator` Groups A (Architecture) + D (Navigation) only |
| **Prohibited** | Puzzle components, signal wiring, echo recorder slots, narrative props |
| **Success Criteria** | All Group A + D assertions pass; zero FAIL-ARC/NAV codes |

**Input Schema** (`LEVEL_SPEC_XX.yaml`):
```yaml
level_layout:
  modules:
    - module_type: "SchoolCorridor"
      position: [0, 0, 0]
      rotation: [0, 0, 0]
      custom_data: "lockers=left;length=24"
    - module_type: "SchoolClassroom"
      position: [12, 0, 0]
      rotation: [0, 90, 0]
      custom_data: "desks=4x4;chaotic=false"
  connections:
    - from: "SchoolCorridor_0"
      to: "SchoolClassroom_0"
      door_type: "Door"
```

---

#### 4.2 PuzzleDesignerAgent
**Role**: Adds puzzle components, signal wiring, echo timing to approved layout.

| Field | Specification |
|---|---|
| **Inputs** | `SOURCE_OF_TRUTH.md`, `ANTI_PATTERNS.md`, `PUZZLE_GRAMMAR.md`, `ECHO_GRAMMAR.md`, `LEVEL_SPEC_XX.yaml` (from LevelDesignAgent) |
| **Outputs** | `LEVEL_SPEC_XX.puzzle` (puzzle components, wiring, timing, echo recorder slots, pressure plates, ghost platforms) |
| **Validation** | `LevelValidator` Group B (Echo Button Test mandatory) |
| **Prohibited** | Modifying room positions, architecture modules, lighting, props |
| **Success Criteria** | Group B passes; Echo Button Test (`VAL-B-01`) returns true |

**Output Schema** (`LEVEL_SPEC_XX.puzzle`):
```yaml
puzzle:
  components:
    - id: "pressure_plate_01"
      module_type: "PressurePlate"
      position: [4.5, 0.04, 2.0]
      custom_data: "latch=true;requires_echo=true"
    - id: "ghost_platform_01"
      module_type: "GhostPlatform"
      position: [8, 0.15, 4]
      custom_data: "solid_during_echo=true"
  wiring:
    - source: "pressure_plate_01"
      signal: "plate_pressed"
      target: "ghost_platform_01"
      signal: "bridge_activate"
  timing:
    sync_floor_s: 0.4
    min_record_s: 1.0
```

---

#### 4.3 EnvironmentArtistAgent
**Role**: Places props, applies lighting overrides, places narrative objects.

| Field | Specification |
|---|---|
| **Inputs** | `SOURCE_OF_TRUTH.md`, `ANTI_PATTERNS.md`, `PROP_LIBRARY.md`, `ENVIRONMENT_STORYTELLING.md`, `LIGHTING_GRAMMAR.md`, `LEVEL_SPEC_XX.yaml` (from LevelDesignAgent) |
| **Outputs** | `prop_placements.yaml`, `lighting_overrides.yaml` |
| **Validation** | `LevelValidator` Groups C (Lighting & URP) + E (Camera & Framing) |
| **Prohibited** | Modifying room layout, puzzle components, camera profiles |
| **Success Criteria** | Groups C + E pass; exactly 1 narrative prop per level with `#FFBF00` |

**Output Schema** (`prop_placements.yaml`):
```yaml
prop_placements:
  - prefab: "Arch_Desk"
    position: [2.0, 0.0, 2.0]
    rotation: [0, 180, 0]
    bounding_box: [1.30, 0.75, 0.50]
  - prefab: "MochilaLyra"
    position: [4.0, 0.75, 4.0]
    rotation: [0, 0, 0]
    material_token: "Mat_Token_memory-amber"
lighting_overrides:
  - room: "SchoolClassroom_0"
    fog_density: 0.008
    ambient_override: [0.059, 0.078, 0.102, 1.0]
```

---

#### 4.4 CameraTechnicalAgent
**Role**: Assigns Cinemachine camera profiles per room, validates framing.

| Field | Specification |
|---|---|
| **Inputs** | `SOURCE_OF_TRUTH.md`, `ANTI_PATTERNS.md`, `CAMERA_GRAMMAR.md`, `CINEMACHINE_FRAMING_MATRIX.yaml`, `LEVEL_SPEC_XX.yaml` |
| **Outputs** | `camera_profile_assignments.yaml` |
| **Validation** | `LevelValidator` Group E (TargetGroup weights, no instant cuts) |
| **Prohibited** | Modifying room layout, puzzles, props, lighting |
| **Success Criteria** | Group E passes; all rooms have `cameraProfile != null`; blend times 0.5–3.0s; dutch ≤ 5° |

**Output Schema** (`camera_profile_assignments.yaml`):
```yaml
camera_profiles:
  - room: "SchoolCorridor_0"
    profile: "Corridor_Linear"
    priority: 10
    blend_in_s: 1.0
    blend_out_s: 1.0
  - room: "SchoolClassroom_0"
    profile: "Classroom_Static"
    priority: 20
    blend_in_s: 0.5
    blend_out_s: 0.5
  - room: "Puzzle_Core"
    profile: "Puzzle_Orbit"
    priority: 30
    blend_in_s: 1.5
    blend_out_s: 1.5
```

---

#### 4.5 VisualNovelAgent
**Role**: Creates choice nodes, epilogue scenes, ending resolution logic.

| Field | Specification |
|---|---|
| **Inputs** | `SOURCE_OF_TRUTH.md`, `DESIGN_PHILOSOPHY.md`, `VN_CHOICE_REGISTRY.yaml`, `VN_ENDING_RESOLVER.yaml` |
| **Outputs** | `choice_nodes.yaml`, `epilogue_scenes.yaml` |
| **Validation** | `VN_EndingResolver` unit tests (32 paths must resolve correctly) |
| **Prohibited** | Modifying gameplay levels, puzzles, camera, props |
| **Success Criteria** | All 32 ending paths resolve to valid epilogue; no dead ends |

**Output Schema** (`choice_nodes.yaml`):
```yaml
choice_nodes:
  - node_id: "ch3_choice_1"
    text_key: "vn.ch3.choice.1"
    conditions: []
    outcomes:
      - target_node: "ch3_path_a"
        flags_set: ["path_a_taken"]
      - target_node: "ch3_path_b"
        flags_set: ["path_b_taken"]
  - node_id: "final_choice"
    text_key: "vn.final.choice"
    timeout_seconds: 0
    timeout_action: "default_first"
```

**Output Schema** (`epilogue_scenes.yaml`):
```yaml
epilogue_scenes:
  - ending_id: "ending_acceptance"
    required_flags: ["path_a_taken", "memory_all_unlocked"]
    scene: "Assets/Scenes/Epilogue_Acceptance.unity"
    narration_key: "vn.epilogue.acceptance"
  - ending_id: "ending_integration"
    required_flags: ["path_b_taken"]
    scene: "Assets/Scenes/Epilogue_Integration.unity"
    narration_key: "vn.epilogue.integration"
```

---

### 5. HANDOFF PROTOCOL

| From Agent | To Agent | Artifact | Validation Required |
|---|---|---|---|
| LevelDesignAgent | PuzzleDesignerAgent | `LEVEL_SPEC_XX.yaml` | Groups A+D pass |
| PuzzleDesignerAgent | EnvironmentArtistAgent | `LEVEL_SPEC_XX.puzzle` | Group B passes |
| EnvironmentArtistAgent | CameraTechnicalAgent | `prop_placements.yaml`, `lighting_overrides.yaml` | Groups C+E pass |
| CameraTechnicalAgent | VisualNovelAgent | `camera_profile_assignments.yaml` | Group E passes |
| (All) | Build Pipeline | Complete level spec bundle | All groups A–E pass |

### 6. FAILURE HANDLING

| Failure Code | Trigger | Resolution |
|---|---|---|
| `FAIL-AGT-01` | Agent reads unauthorized input | Halt; audit input access |
| `FAIL-AGT-02` | Agent output missing required field | Halt; regenerate with schema |
| `FAIL-AGT-03` | Validation group fails after agent pass | Return to agent with error log |
| `FAIL-AGT-04` | Agent modifies prohibited domain | Halt; rollback changes |

### 7. CROSS REFERENCES
- [SOURCE_OF_TRUTH.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Authority/SOURCE_OF_TRUTH.md) `[SPEC-000]`
- [AI_RULEBOOK.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/AI/AI_RULEBOOK.md) `[SPEC-401]`
- [LEVEL_VALIDATOR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Validation/LEVEL_VALIDATOR.md) `[SPEC-301]`

### 8. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Initial canonical SPEC-403 defining 5-agent rigid contracts.

(End of file - total 108 lines)