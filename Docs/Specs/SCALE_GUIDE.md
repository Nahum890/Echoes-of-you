# SCALE_GUIDE.md — Physical Scale & Dimensional Specifications
## Spec ID: SPEC-106
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Establishes mandatory metrics, bounding boxes, and physical dimensions for player locomotion, doorways, corridors, stairs, pressure plates, and furniture props in *Echoes of You 2.0*. Serves as the single metric contract to prevent scale mismatches.

### 2. SCOPE
Applies to `PlayerController.cs`, `CharacterController`, architectural prefabs, room modules, and furniture items. Excludes URP light intensities.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ARCHITECTURE_GRAMMAR.md` (`SPEC-003`).

### 4. DEFINITIONS
- `Player Bounding Box`: Capsule volume $[0.70\text{m}, 1.80\text{m}, 0.70\text{m}]$ with radius $R_{player} = 0.35\text{m}$.
- `Doorway Clearance`: Net opening dimensions for single ($1.20\text{m} \times 2.40\text{m}$) and double ($2.40\text{m} \times 2.40\text{m}$) doors.
- `Stair Riser / Tread`: Stair step rise $H_{riser} = 0.18\text{m}$ and tread depth $D_{tread} = 0.28\text{m}$.

### 5. INPUTS
- [SOURCE_OF_TRUTH.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Authority/SOURCE_OF_TRUTH.md) `[SPEC-000]`
- [ARCHITECTURE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ARCHITECTURE_GRAMMAR.md) `[SPEC-003]`

### 6. OUTPUTS
- Physical dimensions for `CharacterController` in `PlayerController.cs`.
- Collider bounds assertions for `LevelValidator.cs`.

### 7. RULES

- `[RULE-SCL-001]`: **Player Physical Bounds**: Player character MUST match CapsuleCollider height $H=1.80\text{m}$, radius $R=0.35\text{m}$, step offset $0.30\text{m}$, and eye level $1.60\text{m}$.
- `[RULE-SCL-002]`: **Doorway Clearance Minimum**: Single door frames MUST measure $W=1.20\text{m}, H=2.40\text{m}$. Double door frames MUST measure $W=2.40\text{m}, H=2.40\text{m}$.
- `[RULE-SCL-003]`: **Stair Tread Bounds**: Stair steps MUST maintain riser height $H_{riser} = 0.18\text{m} \pm 0.0$ and tread depth $D_{tread} = 0.28\text{m} \pm 0.0$.
- `[RULE-SCL-004]`: **Pressure Plate Clearance**: Standard pressure plates MUST measure $[1.50\text{m}, 0.08\text{m}, 1.50\text{m}]$ with a surrounding $0.50\text{m}$ unobstructed buffer.

### 8. ALGORITHMS

#### Table 8.1: Master Scale & Dimension Matrix

| Entity Category | Entity Name | Dimensions $[W, H, D]$ | Key Parameter | Tolerance |
|---|---|---|---|---|
| **Character** | Player / Echo | `[0.70, 1.80, 0.70]` | $R=0.35\text{m}$, Eye $1.60\text{m}$ | `±0.0m` |
| **Door** | Single Door Frame | `[1.20, 2.40, 0.15]` | Opening $1.20\text{m} \times 2.40\text{m}$ | `±0.0m` |
| **Door** | Double Door Frame | `[2.40, 2.40, 0.15]` | Opening $2.40\text{m} \times 2.40\text{m}$ | `±0.0m` |
| **Architecture** | Corridor | `[4.50, 3.20, 24.0]` | Free Height $3.20\text{m}$ | `±0.0m` |
| **Architecture** | Classroom | `[10.0, 3.80, 10.0]` | Free Height $3.80\text{m}$ | `±0.0m` |
| **Stair** | Step Riser / Tread | `[2.20, 0.18, 0.28]` | Rise $0.18\text{m}$, Tread $0.28\text{m}$ | `±0.0m` |
| **Puzzle** | Pressure Plate | `[1.50, 0.08, 1.50]` | Buffer clearance $0.50\text{m}$ | `±0.0m` |
| **Puzzle** | Kinetic Block | `[1.00, 1.00, 1.00]` | Mass $50\text{ kg}$ | `±0.0m` |

### 9. CONSTRAINTS
- `[CONS-SCL-001]`: Prohibido scaling architectural GameObjects via `transform.localScale` to non-integer or variance $> \pm 0.05$.
- `[CONS-SCL-002]`: Prohibido stair risers $> 0.20\text{m}$ (causes CharacterController snagging).

### 10. VALIDATION
- `[VAL-SCL-001]`: `LevelValidator.cs` parses scene colliders and asserts 100% of door openings have $H \ge 2.40\text{m}$.
- `[VAL-SCL-002]`: Locomotion test asserts CharacterController height $1.80\text{f}$ and radius $0.35\text{f}$.

### 11. EXAMPLES

#### Example 11.1: CharacterController Initialization in C#
```csharp
CharacterController controller = GetComponent<CharacterController>();
controller.height = 1.80f;
controller.radius = 0.35f;
controller.stepOffset = 0.30f;
controller.center = new Vector3(0f, 0.90f, 0f);
```

### 12. FAILURE CASES
- `[FAIL-SCL-001]`: **Door Clearance Deficit**: Door frame height set to $2.0\text{m}$. Result: `LevelValidator` flags `FAIL-SCL-01`.
- `[FAIL-SCL-002]`: **Stair Snagging**: Stair riser set to $0.25\text{m}$. Result: `FAIL-SCL-02`.

### 13. CROSS REFERENCES
- [ARCHITECTURE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ARCHITECTURE_GRAMMAR.md) `[SPEC-003]`
- [PROP_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PROP_LIBRARY.md) `[SPEC-005]`

### 14. CHANGE HISTORY
- **v1.0 (2025-03-20)**: Initial scale guide.
- **v3.0 (2026-07-25)**: Full refactor into 14-section AI-Executable canonical format.
