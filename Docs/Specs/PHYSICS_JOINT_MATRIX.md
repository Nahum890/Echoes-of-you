# PHYSICS_JOINT_MATRIX.md — Rigidbody Configurations & Interactive Object Physics
## Spec ID: SPEC-131
## Version: 1.0 (AI-Executable)

---

### 1. PURPOSE
Defines exact Rigidbody mass values, constraints, drag coefficients, kinematic behaviors, and activation weight thresholds for interactive puzzle elements (push blocks, moving platforms, weight plates).

### 2. SCOPE
Applies to interactive physical GameObjects in `Assets/Prefabs/Puzzle/` and Level Builder instantiation passes.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `PHYSICS_LAYER_MATRIX.md` (`SPEC-116`).

### 4. DEFINITIONS
- `Kinetic Push Block`: Mass $= 50.0\text{kg}$ block pushable by player/Echo, constrained to Y-axis rotation.
- `Weight Plate`: Pressure plate activated when total mass on trigger $\ge 95.0\text{kg}$ (target $100.0\text{kg}$).

### 5. INPUTS
- [PHYSICS_LAYER_MATRIX.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PHYSICS_LAYER_MATRIX.md) `[SPEC-116]`
- [PHYSICS_JOINT_MATRIX.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PHYSICS_JOINT_MATRIX.yaml) `[SPEC-131]`

### 6. OUTPUTS
- `Rigidbody` component configurations applied to interactive props during level generation.

### 7. RULES
- `[RULE-JNT-001]`: **Push Block Constraints**: `KineticPushBlock` MUST set `mass = 50.0kg`, `drag = 0.5`, and freeze X/Z rotations.
- `[RULE-JNT-002]`: **Kinematic Moving Platform**: `MovingPlatform` MUST set `isKinematic = true` and `useGravity = false`.
- `[RULE-JNT-003]`: **Weight Plate Threshold**: `WeightPlate` activation threshold MUST be $95.0\text{kg}$ ($\ge 95\%$ of $100\text{kg}$ required mass).

### 8. ALGORITHMS
See canonical configuration at `Docs/Specs/PHYSICS_JOINT_MATRIX.yaml`.

### 9. CONSTRAINTS
- `[CONS-JNT-001]`: Prohibido non-kinematic moving platforms driven by physics joints.

### 10. VALIDATION
- `[VAL-JNT-001]`: `LevelValidator.cs` checks all push blocks have frozen X/Z rotations and valid mass settings.

### 11. EXAMPLES
- `PHYSICS_JOINT_MATRIX.yaml` schema definition.

### 12. FAILURE CASES
- `[FAIL-JNT-001]`: **Platform Drift**: Non-kinematic platform pushed out of alignment by player collision. Result: `FAIL-JNT-01`.

### 13. CROSS REFERENCES
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Created canonical SPEC-131 for Physics Joint Matrix.
