# PROP_LIBRARY.md — Prop Technical Catalog & Placement Specifications
## Spec ID: SPEC-005
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines the technical catalog, BoundingBox dimensions $[W, H, D]$, navigation clearances ($R_{clearance} \ge 1.2\text{m}$), wall offsets, rotation snaps, and state behaviors for all props from the Kenney Furniture Kit and Architecture Pack used in *Echoes of You 2.0*.

### 2. SCOPE
Applies to prop decoration routines (`EchoesPropDecorator`), prefab assets in `Assets/Prefabs/`, and `LevelValidator.cs`. Excludes static room wall/ceiling geometry.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ROOM_LIBRARY.md` (`SPEC-004`). Consolidates `PROP_GRAMMAR.md`, `ASSET_CATALOG.md`, and `ASSET_ORGANIZATION.md`.

### 4. DEFINITIONS
- `Clearance Radius ($R_{clearance}$)`: Minimum unobstructed navigation corridor around props ($1.2\text{m}$ standard).
- `Wall Offset ($D_{wall}$)`: Fixed distance between a wall-mounted prop and the base wall surface ($0.05\text{m}$ standard to prevent clipping).
- `Rotation Snap`: Angular increment for placing structural props ($90.0^\circ$ standard).
- `Static Furniture`: Non-interactive props marked `IsStatic = true` without runtime `Rigidbody` components.

### 5. INPUTS
- [SCALE_GUIDE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SCALE_GUIDE.md) `[SPEC-106]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`

### 6. OUTPUTS
- Instantiated prop prefabs placed by `EchoesPropDecorator.cs`.
- Collision assertions for `LevelValidator.cs`.

### 7. RULES

- `[RULE-PRP-001]`: **Navigation Clearance Floor**: All props placed in playable rooms MUST maintain an unobstructed navigation clearance $R_{clearance} \ge 1.2\text{m}$ from primary movement paths.
- `[RULE-PRP-002]`: **Wall Offset Snap**: Wall-mounted furniture (lockers, bookshelves) MUST be positioned at $D_{wall} = 0.05\text{m}$ from the base wall collider to eliminate z-fighting and mesh clipping.
- `[RULE-PRP-003]`: **Rotation Snapping**: 100% of structural props MUST be oriented in exact $90.0^\circ$ increments ($0^\circ, 90^\circ, 180^\circ, 270^\circ$). Exception: Student desks in chaotic classrooms allow jitter $\pm 15.0^\circ$.
- `[RULE-PRP-004]`: **Zero Runtime Rigidbody**: Static decor props MUST NOT attach active `Rigidbody` components in runtime (prevents Echo desynchronization).

### 8. ALGORITHMS

#### Table 8.1: Technical Catalog of Architectural & Narrative Props

| Prefab Name | Category | BoundingBox $[W, H, D]$ | Navigation Clearance | Wall Offset | Rotation Snap | State Behavior | Material Token |
|---|---|---|---|---|---|---|---|
| `Arch_Locker` | Furniture | `[0.80, 1.90, 0.50]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Static / Locked Door | `Mat_Token_institutional-teal` |
| `Arch_Desk` | Furniture | `[1.30, 0.75, 0.50]` | $1.2\text{ m}$ | $1.50\text{ m}$ | $180.0^\circ (\pm 15^\circ)$ | Static / Overturned $90^\circ$ | `Mat_Token_faded-mustard` |
| `Arch_Chair` | Furniture | `[0.45, 0.85, 0.45]` | $0.8\text{ m}$ | N/A | $180.0^\circ (\pm 15^\circ)$ | Tucked under desk | `Mat_Token_faded-mustard` |
| `Arch_Bench` | Furniture | `[1.60, 0.45, 0.50]` | $1.2\text{ m}$ | $0.10\text{ m}$ | $90.0^\circ$ | Static Fixed | `Mat_Token_faded-mustard` |
| `MesaProfesor` | Furniture | `[1.60, 0.75, 0.80]` | $1.5\text{ m}$ | $1.00\text{ m}$ | $0.0^\circ / 180.0^\circ$ | Static Fixed | `Mat_Token_faded-mustard` |
| `Pizarra` | Architecture | `[3.20, 1.40, 0.08]` | N/A | Wall-mounted | $0.0^\circ$ | Chalkboard Static | `Mat_Token_ChalkboardMat` |
| `Estanteria` | Furniture | `[1.80, 2.40, 0.45]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Static Bookshelf | `Mat_Token_institutional-teal` |
| `MochilaLyra` | Narrative | `[0.35, 0.40, 0.25]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Glow Amber `#FFBF00` | `Mat_Token_memory-amber` |
| `Cronometro` | Narrative | `[0.15, 0.20, 0.05]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Pulse Cyan `#00FFFF` | `Mat_Token_echo-cyan` |

### 9. CONSTRAINTS
- `[CONS-PRP-001]`: Prohibido placing lockers or large furniture blocking doorway thresholds ($D < 1.2\text{m}$).
- `[CONS-PRP-002]`: Prohibido BoundingBox overlapping $> 0.01\text{m}$ between two static props.

### 10. VALIDATION
- `[VAL-PRP-001]`: `LevelValidator.cs` parses NavMesh obstacle paths and asserts zero props intersect path corridors $< 1.2\text{m}$.
- `[VAL-PRP-002]`: Inspector validates that 100% of static props have `IsStatic = true` and zero `Rigidbody` components.

### 11. EXAMPLES

#### Example 11.1: Valid Prop Placement Data in YAML
```yaml
prop_placement:
  prefab_name: "Arch_Locker"
  position: [4.5, 0.0, 1.0]
  rotation_euler: [0, 90, 0]
  bounding_box: [0.80, 1.90, 0.50]
  is_static: true
  wall_offset_m: 0.05
```

### 12. FAILURE CASES
- `[FAIL-PRP-001]`: **Door Obstruction**: A locker blocks a doorway ($W_{clearance} = 0.6\text{m}$). Result: `LevelValidator` flags `FAIL-NAV-01`.
- `[FAIL-PRP-002]`: **Dynamic Rigidbody Attached**: A desk has an active `Rigidbody`. Result: `LevelValidator` flags `FAIL-PRP-02`.

### 13. CROSS REFERENCES
- [SCALE_GUIDE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SCALE_GUIDE.md) `[SPEC-106]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`
- [ENVIRONMENT_STORYTELLING.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ENVIRONMENT_STORYTELLING.md) `[SPEC-006]`

### 14. CHANGE HISTORY
- **v1.0 (2025-05-20)**: Prop grammar initial catalog.
- **v2.0 (2026-07-20)**: BoundingBox standardization.
- **v3.0 (2026-07-25)**: Full refactor into 14-section AI-Executable canonical format.
