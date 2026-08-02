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
**Role**: Creates psychological decision nodes (how Aiden processes her error with Lyra), epilogue scenes, and ending resolution logic mapped to the 5 stages of psychological acceptance.

| Field | Specification |
|---|---|
| **Inputs** | `SOURCE_OF_TRUTH.md`, `DESIGN_PHILOSOPHY.md`, `ECHOES_BIBLE.md`, `NARRATIVA_INTERNA.md` `[DOC-102]`, `VN_ENDINGS_REDEFINED.yaml`, `emotional_arc.yaml` v2.0 |
| **Outputs** | `choice_nodes.yaml` (decisiones psicológicas), `epilogue_scenes.yaml` (5 endings) |
| **Validation** | `VN_EndingResolver` unit tests (32 paths must resolve correctly to exactly 1 of 5 endings); `ambiguity_police` (0 etiquetas relacionales); `dual_thesis_police` (cada ending toca las dos mitades del mensaje dual sin resolverlo textualmente) |
| **Prohibited** | Modifying gameplay levels, puzzles, camera, props; clarifying nature of Aiden-Lyra relationship (ANTI-BIB-004); writing any ending that concludes "Aiden tenía razón" or "Aiden tenía la culpa" (ANTI-BIB-005) |
| **Success Criteria** | All 32 ending paths resolve to valid epilogue; no dead ends; 5 endings exhausted; 0 forbidden words; each epilogue leaves the dual thesis intentionally open |

**Ending Vocabulary (v2.0, renamed)**:

| Legacy ID | New ID (canon) | Psychological Arm | What Aiden does in the epilogue |
|---|---|---|---|
| `Void` | `Aislamiento` | Negación persistente | Se encierra en la mente, otra buelta del corredor. No salió. |
| `Obsession` | `Ruminación` | Culpa persistente / auto-sabotaje | Repite el recuerdo exacto, sin variantes, esperando que esta vez duela menos. No duele menos. |
| `Release` | `Negociación` | Realización parcial pero evade | Construye una versión nueva del recuerdo, dice "ya está bien así", pero sigue toca la taquilla vacía cada noche. |
| `Resonance` | `Desesperación` | Comprensión pero desesperación | Ve todo el panorama y la magnitud aterra. El final no es paz, es pavor. Inclina a creer que no hay salida — se aferra al dolor como prueba de que importó. |
| `Integration` | `Aceptación` | Aceptación + mejora activa | Sostiene el recuerdo sin apretarlo. Sale del colegio. El pasado sigue ahí pero ya no la define. Puede moverse. |

**Output Schema** (`choice_nodes.yaml`):
```yaml
choice_nodes:
  - node_id: "ch3_choice_1"
    level_index: 3
    text_key: "vn.ch3.choice.1"  # reflejado a tone_by_stage en dialogue_tree_schema v2.0
    conditions: []
    outcomes:
      - target_node: "ch3_hold_on"
        flags_set: ["hold_on_to_pattern"]
        psychological_meaning: "Aiden decide mantener la versión que se cuenta a sí misma"
      - target_node: "ch3_let_try"
        flags_set: ["allow_other_version"]
        psychological_meaning: "Aiden permite que exista otra versión del mismo recuerdo"
  - node_id: "final_choice_n15"
    level_index: 15
    text_key: "vn.ch15.final.choice"
    timeout_seconds: 0
    timeout_action: "default_first"
    note: "El final_choice NO determina el ending por sí solo — solo pesa"
```


**Output Schema** (`epilogue_scenes.yaml`):
```yaml
epilogue_scenes:
  - ending_id: "Aislamiento"           # ex Void
    psychological_arm: "negación_persistente"
    required_flags_pattern:
      any_of: ["hold_on_to_pattern", "blame_external", "repeat_pattern_n15"]
      required_score_comprehension: "low"
    scene: "Assets/Scenes/Epilogue_Aislamiento.unity"
    narration_key: "vn.epilogue.aislamiento"
    text_invariant: "salir_del_colegio" (FALSE — Aiden no sale)

  - ending_id: "Ruminación"            # ex Obsession
    psychological_arm: "culpa_persistente"
    required_flags_pattern:
      any_of: ["self_blame_loop", "replay_exact_memory", "no_variation"]
      required_score_comprehension: "low-mid"
    scene: "Assets/Scenes/Epilogue_Ruminacion.unity"
    narration_key: "vn.epilogue.ruminacion"

  - ending_id: "Negociación"          # ex Release
    psychological_arm: "realización_parcial_evasiva"
    required_flags_pattern:
      any_of: ["allow_other_version", "construct_new_memory"]
      requires_missing: ["active_acceptance_flag"]
    scene: "Assets/Scenes/Epilogue_Negociacion.unity"
    narration_key: "vn.epilogue.negociacion"

  - ending_id: "Desesperación"        # ex Resonance
    psychological_arm: "comprensión_sin_suelo"
    required_flags_pattern:
      requires_comprehension: "high"
      any_of: ["see_full_picture", "magnitude_clear"]
      requires_missing: ["let_go_any_object"]
    scene: "Assets/Scenes/Epilogue_Desperacion.unity"
    narration_key: "vn.epilogue.desesperacion"

  - ending_id: "Aceptación"           # ex Integration
    psychological_arm: "aceptación_y_mejora_activa"
    required_flags_pattern:
      requires_comprehension: "high"
      requires_all: ["allow_other_version", "let_go_any_object", "active_acceptance_flag", "break_pattern_n15"]
    scene: "Assets/Scenes/Epilogue_Aceptacion.unity"
    narration_key: "vn.epilogue.aceptacion"
    text_invariant: "salir_del_colegio" (TRUE — Aiden sale sin mirar para atrás)

# NOTES
# 1. "salir_del_colegio" como invariant: la escena N01-15 transcurre EN la mente
#    de Aiden; el ending Aceptación se marca precisamente porque Aiden sale del
#    colegio (deja de habitar el espacio mental del trauma). Otros endings
#    quedan atrapados en el recinto.
# 2. comprehension_score y lyra_artifact_seen_count (ver dialogue_tree_schema.yaml
#    v2.0 narrative_variables) alimentan el resolver.
# 3. A la DIFFERENCIA de los endings legados, Aceptación NO es "happy ending":
#    no confirma que Aiden tenía razón ni borra el dolor. Solo: ella puede moverse.
# 4. Todos los endings del epílogo deben pasar ambos validadores:
#    - ambiguity_police: 0 etiquetas relacionales
#    - dual_thesis_police: texto del epílogo no concluye la tesis dual
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
| `FAIL-AGT-05` | VisualNovelAgent introduce palabra prohibida de ANTI-BIB-004 ("amiga", "novia", "pareja", "relación") | Halt; regen con vocabulario alternativo |
| `FAIL-AGT-06` | VisualNovelAgent produce ending que concluye ANTI-BIB-005 ("tenía razón/culpa") | Halt; regen dejando la dualidad abierta |

### 7. CROSS REFERENCES
- [SOURCE_OF_TRUTH.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Authority/SOURCE_OF_TRUTH.md) `[SPEC-000]`
- [AI_RULEBOOK.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/AI/AI_RULEBOOK.md) `[SPEC-401]`
- [LEVEL_VALIDATOR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Validation/LEVEL_VALIDATOR.md) `[SPEC-301]`

### 8. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Initial canonical SPEC-403 defining 5-agent rigid contracts.
- **v1.1 (2026-08-02)**: Reescritura narrativa dual — VisualNovelAgent contract redraft:
  - Endings renombrados: Void→Aislamiento, Obsession→Ruminación, Release→Negociación, Resonance→Desesperación, Integration→Aceptación.
  - Inputs añaden `NARRATIVA_INTERNA.md [DOC-102]` y `VN_ENDINGS_REDEFINED.yaml`.
  - `epilogue_scenes.yaml` schema ampliado (required_flags_pattern, psychological_arm, text_invariant, comprehension requirements).
  - Nuevos validadores: `ambiguity_police` (ANTI-BIB-004), `dual_thesis_police` (ANTI-BIB-005).
  - Nuevos códigos de falla: FAIL-AGT-05 (vocabulario relacional prohibido), FAIL-AGT-06 (autojustificación final).
  - `salir_del_colegio` text_invariant introducido como marcador de evento real (Aiden sale del espacio mental del trauma ↔ Aceptación).

(End of file - total 108 lines)