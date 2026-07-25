# PHYSICS_LAYER_MATRIX.md — Unity Physics Collision Matrix Specification
## Spec ID: SPEC-116
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Defines the canonical 12 Unity Physics Layers and the exact boolean collision matrix governing physical interactions across Player, Echo, PuzzleWire, Hazard, and GhostPlatform.

### 2. SCOPE
Applies to Unity `Physics2D` / `Physics3D` Collision Matrix settings in `ProjectSettings/DynamicsManager.asset`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`).

### 4. DEFINITIONS
- `GhostPlatform` (Layer 12): Special collision layer for temporary platforms/bridges toggled by Echo state.
- `Collision Pair`: Boolean assignment dictating whether two physics layers interact physically.

### 5. INPUTS
- [ECHO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_GRAMMAR.md) `[SPEC-107]`
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`

### 6. OUTPUTS
- Physics collision matrix configuration applied to Unity Editor.

### 7. RULES

- `[RULE-LAY-001]`: **Player-Echo Non-Collision**: Layer 8 (`Player`) and Layer 9 (`Echo`) MUST NOT collide ($[Player, Echo] = 0$). Player and Echo pass through each other freely.
- `[RULE-LAY-002]`: **GhostPlatform Default State**: Layer 12 (`GhostPlatform`) MUST NOT collide with Layer 8 (`Player`) or Layer 9 (`Echo`) when Echo is inactive ($0$).
- `[RULE-LAY-003]`: **Hazard Enforcement**: Layer 11 (`Hazard`) MUST collide with Layer 8 (`Player`) and Layer 9 (`Echo`) ($1$).

### 8. ALGORITHMS

#### Algorithm 8.1: Physics Collision Matrix Schema

```yaml
# Unity Physics Collision Matrix — 12 capas definidas
# 1 = colisionan, 0 = no colisionan
layers:
  0: "Default"
  8: "Player"
  9: "Echo"
  10: "PuzzleWire"
  11: "Hazard"
  12: "GhostPlatform"
  13: "UI"
  14: "NavMeshOnly"

collision_matrix:
  # Format: [LayerA, LayerB]: colide?
  [Default, Default]: 1
  [Default, Player]: 1
  [Default, Echo]: 1
  [Player, Player]: 0
  [Player, Echo]: 0    # Player y Echo NO colisionan entre sí
  [Player, Hazard]: 1
  [Echo, Hazard]: 1
  [Echo, GhostPlatform]: 0  # Echo atraviesa GhostPlatform cuando está inactivo
  [GhostPlatform, Player]: 0  # Player atraviesa GhostPlatform cuando Echo inactivo
  # Note: GhostPlatform layer dynamic switch to Default (Layer 0) is driven by GhostBridge.cs script when Echo is active
```

### 9. CONSTRAINTS
- `[CONS-LAY-001]`: Prohibido enabling physical collision between Player and Echo.

### 10. VALIDATION
- `[VAL-LAY-001]`: `LevelValidator.cs` parses `Physics.GetIgnoreLayerCollision(8, 9)` and asserts `true` (no collision).

### 11. EXAMPLES
- Matrix schema above.

### 12. FAILURE CASES
- `[FAIL-LAY-001]`: **Player-Echo Collision Deadlock**: Player gets stuck inside an Echo capsule because collision was enabled. Result: `FAIL-LAY-01`.

### 13. CROSS REFERENCES
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`

### 14. CHANGE HISTORY
- **v4.0 (2026-07-25)**: Created canonical SPEC-116 defining the complete boolean physics collision layer matrix.
