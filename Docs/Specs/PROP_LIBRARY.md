# PROP_LIBRARY.md — Prop Technical Catalog & Placement Specifications
## Spec ID: SPEC-005
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Defines the technical catalog, BoundingBox dimensions $[W, H, D]$, navigation clearances ($R_{clearance} \ge 1.2\text{m}$), wall offsets, rotation snaps, and state behaviors for all 142 props from the Kenney Furniture Kit and Architecture Pack used in *Echoes of You 2.0*.

### 2. SCOPE
Applies to prop decoration routines (`EchoesPropDecorator`), prefab assets in `Assets/Prefabs/`, and `LevelValidator.cs`. Excludes static room wall/ceiling geometry.

### 3. AUTHORITY
Level 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ROOM_LIBRARY.md` (`SPEC-004`). Consolidates `PROP_GRAMMAR.md`, `ASSET_CATALOG.md`, and `ASSET_ORGANIZATION.md`.

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
- `[RULE-PRP-001]`: **Navigation Clearance Floor** — All props placed in playable rooms MUST maintain an unobstructed navigation clearance $R_{clearance} \ge 1.2\text{m}$ from primary movement paths.
- `[RULE-PRP-002]`: **Wall Offset Snap** — Wall-mounted furniture (lockers, bookshelves) MUST be positioned at $D_{wall} = 0.05\text{m}$ from the base wall collider to eliminate z-fighting and mesh clipping.
- `[RULE-PRP-003]`: **Rotation Snapping** — 100% of structural props MUST be oriented in exact $90.0^\circ$ increments ($0^\circ, 90^\circ, 180^\circ, 270^\circ$). Exception: Student desks in chaotic classrooms allow jitter $\pm 15.0^\circ$.
- `[RULE-PRP-004]`: **Zero Runtime Rigidbody** — Static decor props MUST NOT attach active `Rigidbody` components in runtime (prevents Echo desynchronization).

### 8. ALGORITHMS

#### Table 8.1: Complete Technical Catalog of 142 Props

| Prefab Name | Category | BoundingBox $[W, H, D]$ | Navigation Clearance | Wall Offset | Rotation Snap | State Behavior | Material Token |
|---|---|---|---|---|---|---|---|
| `Arch_Locker` | Furniture | `[0.80, 1.90, 0.50]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Static / Locked Door | `Mat_Token_institutional-teal` |
| `Arch_Chair` | Furniture | `[0.45, 0.85, 0.45]` | $0.8\text{ m}$ | N/A | $180.0^\circ (\pm 15^\circ)$ | Tucked under desk | `Mat_Token_faded-mustard` |
| `Arch_Column` | Architecture | `[0.50, 3.20, 0.50]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Static Column | `Mat_Token_concrete` |
| `Arch_Desk` | Furniture | `[1.30, 0.75, 0.50]` | $1.2\text{ m}$ | $1.50\text{ m}$ | $180.0^\circ (\pm 15^\circ)$ | Static / Overturned $90^\circ$ | `Mat_Token_faded-mustard` |
| `Arch_Doorway` | Architecture | `[1.20, 2.40, 0.15]` | $1.2\text{ m}$ | Wall-mounted | $0.0^\circ$ | Door Frame Static | `Mat_Token_wood` |
| `Arch_Fence` | Architecture | `[2.00, 1.20, 0.10]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Static Fence | `Mat_Token_metal` |
| `Arch_Floor` | Architecture | `[4.50, 0.20, 4.50]` | N/A | N/A | $0.0^\circ$ | Static Floor | `Mat_Token_linoleum` |
| `Arch_Locker` | Furniture | `[0.80, 1.90, 0.50]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Static / Locked Door | `Mat_Token_institutional-teal` |
| `Arch_Shelf` | Furniture | `[1.80, 2.40, 0.45]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Static Bookshelf | `Mat_Token_institutional-teal` |
| `Arch_Stairs` | Architecture | `[2.00, 3.00, 4.00]` | $1.2\text{ m}$ | N/A | $0.0^\circ$ | Static Stairs | `Mat_Token_concrete` |
| `Arch_Trashcan` | Furniture | `[0.40, 0.60, 0.40]` | $1.2\text{ m}$ | $0.10\text{ m}$ | $90.0^\circ$ | Static Trash | `Mat_Token_plastic` |
| `Arch_Tree` | Nature | `[2.00, 5.00, 2.00]` | $1.5\text{ m}$ | N/A | $0.0^\circ$ | Static Tree | `Mat_Token_foliage` |
| `Arch_Wall` | Architecture | `[4.50, 3.20, 0.30]` | N/A | Wall-mounted | $0.0^\circ$ | Static Wall | `Mat_Token_plaster` |
| `Arch_WallWindow` | Architecture | `[2.00, 1.50, 0.15]` | N/A | Wall-mounted | $0.0^\circ$ | Window Frame | `Mat_Token_glass` |
| `Arch_Bench` | Furniture | `[1.60, 0.45, 0.50]` | $1.2\text{ m}$ | $0.10\text{ m}$ | $90.0^\circ$ | Static Fixed | `Mat_Token_faded-mustard` |

| Prefab Name | Category | BoundingBox $[W, H, D]$ | Navigation Clearance | Wall Offset | Rotation Snap | State Behavior | Material Token |
|---|---|---|---|---|---|---|---|
| `EventFocusVCam` | Camera | `[0.10, 0.10, 0.10]` | N/A | N/A | $0.0^\circ$ | Virtual Camera | — |
| `dec_arrastre` | Decal | `[0.50, 0.01, 0.50]` | N/A | Floor-mounted | $0.0^\circ$ | Floor Drag Marks | `Mat_Token_decal` |
| `dec_aviso_corcho` | Decal | `[0.60, 0.40, 0.01]` | N/A | Wall-mounted | $0.0^\circ$ | Cork Board | `Mat_Token_cork` |
| `dec_crack_liminal` | Decal | `[0.50, 0.01, 0.50]` | N/A | Floor-mounted | $0.0^\circ$ | Liminal Crack | `Mat_Token_decal` |
| `dec_floor_drag` | Decal | `[0.50, 0.01, 0.50]` | N/A | Floor-mounted | $0.0^\circ$ | Floor Drag | `Mat_Token_decal` |
| `dec_foto_borrosa` | Decal | `[0.30, 0.40, 0.01]` | N/A | Wall-mounted | $0.0^\circ$ | Blurry Photo | `Mat_Token_decal` |
| `dec_grieta` | Decal | `[0.50, 0.01, 0.50]` | N/A | Floor-mounted | $0.0^\circ$ | Crack | `Mat_Token_decal` |
| `dec_humedad` | Decal | `[0.50, 0.01, 0.50]` | N/A | Floor-mounted | $0.0^\circ$ | Moisture Stain | `Mat_Token_decal` |
| `dec_lyra_notes` | Decal | `[0.30, 0.40, 0.01]` | N/A | Wall-mounted | $0.0^\circ$ | Lyra's Notes | `Mat_Token_memory-amber` |
| `dec_moisture_lines` | Decal | `[0.50, 0.01, 0.50]` | N/A | Floor-mounted | $0.0^\circ$ | Moisture Lines | `Mat_Token_decal` |
| `dec_nota_adhesiva` | Decal | `[0.10, 0.10, 0.01]` | N/A | Wall-mounted | $0.0^\circ$ | Sticky Note | `Mat_Token_paper` |
| `dec_papel_suelo` | Decal | `[0.20, 0.01, 0.20]` | N/A | Floor-mounted | $0.0^\circ$ | Floor Paper | `Mat_Token_paper` |
| `dec_tiza_borrada` | Decal | `[0.50, 0.01, 0.50]` | N/A | Floor-mounted | $0.0^\circ$ | Erased Chalk | `Mat_Token_decal` |
| `EchoPathHint` | LevelKit | `[0.50, 0.50, 0.50]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Path Indicator | `Mat_Token_echo-cyan` |
| `FluorescentLight` | Lighting | `[1.20, 0.10, 0.30]` | N/A | Ceiling-mounted | $0.0^\circ$ | Light Fixture | `Mat_Token_light` |

| Prefab Name | Category | BoundingBox $[W, H, D]$ | Navigation Clearance | Wall Offset | Rotation Snap | State Behavior | Material Token |
|---|---|---|---|---|---|---|---|
| `AbrigoColgado` | Furniture | `[0.60, 1.20, 0.15]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Hanging Coat | `Mat_Token_fabric` |
| `Balon` | Props | `[0.25, 0.25, 0.25]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Physics Ball | `Mat_Token_rubber` |
| `BancoMadera` | Furniture | `[1.60, 0.45, 0.50]` | $1.2\text{ m}$ | $0.10\text{ m}$ | $90.0^\circ$ | Static Bench | `Mat_Token_wood` |
| `Basurero` | Furniture | `[0.40, 0.60, 0.40]` | $1.2\text{ m}$ | $0.10\text{ m}$ | $90.0^\circ$ | Static Trash | `Mat_Token_plastic` |
| `CajaCartonAbierta` | Props | `[0.50, 0.40, 0.50]` | $1.2\text{ m}$ | N/A | $90.0^\circ$ | Open Box | `Mat_Token_cardboard` |
| `CajaCartonCerrada` | Props | `[0.50, 0.50, 0.50]` | $1.2\text{ m}$ | N/A | $90.0^\circ$ | Closed Box | `Mat_Token_cardboard` |
| `CarritoConserje` | Furniture | `[0.80, 1.00, 0.50]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Janitor Cart | `Mat_Token_metal` |
| `Cartelera` | Architecture | `[1.00, 0.70, 0.05]` | N/A | Wall-mounted | $0.0^\circ$ | Bulletin Board | `Mat_Token_cork` |
| `Casco` | Props | `[0.30, 0.25, 0.30]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Helmet | `Mat_Token_plastic` |
| `Cronometro` | Narrative | `[0.15, 0.20, 0.05]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Pulse Cyan `#00FFFF` | `Mat_Token_echo-cyan` |
| `Escritorio` | Furniture | `[1.40, 0.75, 0.70]` | $1.5\text{ m}$ | $1.00\text{ m}$ | $0.0^\circ / 180.0^\circ$ | Teacher Desk | `Mat_Token_wood` |
| `Estanteria` | Furniture | `[1.80, 2.40, 0.45]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Static Bookshelf | `Mat_Token_institutional-teal` |
| `EstanteriaCerrada` | Furniture | `[1.80, 2.40, 0.45]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Closed Bookshelf | `Mat_Token_institutional-teal` |
| `Extintor` | Props | `[0.15, 0.50, 0.15]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Fire Extinguisher | `Mat_Token_metal` |
| `Fluorescente` | Lighting | `[1.20, 0.10, 0.30]` | N/A | Ceiling-mounted | $0.0^\circ$ | Fluorescent Light | `Mat_Token_light` |
| `LamparaTecho` | Lighting | `[0.40, 0.30, 0.40]` | N/A | Ceiling-mounted | $0.0^\circ$ | Ceiling Lamp | `Mat_Token_light` |
| `Libros` | Props | `[0.30, 0.05, 0.20]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Book Stack | `Mat_Token_paper` |
| `Locker` | Furniture | `[0.80, 1.90, 0.50]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Static Locker | `Mat_Token_institutional-teal` |
| `LockerPuertaAbierta` | Furniture | `[0.80, 1.90, 0.50]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Open Locker | `Mat_Token_institutional-teal` |
| `MesaKenney` | Furniture | `[1.30, 0.75, 0.50]` | $1.2\text{ m}$ | $1.50\text{ m}$ | $180.0^\circ (\pm 15^\circ)$ | Student Desk | `Mat_Token_wood` |
| `MesaProfesor` | Furniture | `[1.60, 0.75, 0.80]` | $1.5\text{ m}$ | $1.00\text{ m}$ | $0.0^\circ / 180.0^\circ$ | Teacher Desk | `Mat_Token_wood` |
| `MesaRedonda` | Furniture | `[1.00, 0.75, 1.00]` | $1.2\text{ m}$ | $1.50\text{ m}$ | $0.0^\circ$ | Round Table | `Mat_Token_wood` |
| `Mochila` | Props | `[0.35, 0.40, 0.25]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Backpack | `Mat_Token_fabric` |
| `MochilaLyra` | Narrative | `[0.35, 0.40, 0.25]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Glow Amber `#FFBF00` | `Mat_Token_memory-amber` |
| `PapeleraKenney` | Furniture | `[0.40, 0.60, 0.40]` | $1.2\text{ m}$ | $0.10\text{ m}$ | $90.0^\circ$ | Trash Bin | `Mat_Token_plastic` |
| `Paraguas` | Props | `[0.10, 1.00, 0.10]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Umbrella | `Mat_Token_fabric` |
| `Perchero` | Furniture | `[0.50, 1.70, 0.50]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Coat Rack | `Mat_Token_metal` |
| `Pizarra` | Architecture | `[3.20, 1.40, 0.08]` | N/A | Wall-mounted | $0.0^\circ$ | Chalkboard | `Mat_Token_ChalkboardMat` |
| `PlantaMaceta` | Props | `[0.40, 0.80, 0.40]` | $1.2\text{ m}$ | N/A | $0.0^\circ$ | Potted Plant | `Mat_Token_foliage` |
| `PupitreDoble` | Furniture | `[1.30, 0.75, 1.00]` | $1.2\text{ m}$ | $1.50\text{ m}$ | $180.0^\circ (\pm 15^\circ)$ | Double Desk | `Mat_Token_wood` |
| `Radiador` | Architecture | `[1.00, 0.60, 0.15]` | N/A | Wall-mounted | $0.0^\circ$ | Radiator | `Mat_Token_metal` |
| `Radio` | Props | `[0.20, 0.15, 0.10]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Radio | `Mat_Token_plastic` |
| `RelojPared` | Props | `[0.35, 0.35, 0.05]` | N/A | Wall-mounted | $0.0^\circ$ | Wall Clock | `Mat_Token_plastic` |
| `SillaEscolar` | Furniture | `[0.45, 0.85, 0.45]` | $0.8\text{ m}$ | N/A | $180.0^\circ (\pm 15^\circ)$ | Student Chair | `Mat_Token_plastic` |
| `SillaOficina` | Furniture | `[0.60, 1.00, 0.60]` | $0.8\text{ m}$ | N/A | $180.0^\circ$ | Office Chair | `Mat_Token_fabric` |
| `TazaCafe` | Props | `[0.10, 0.12, 0.10]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Coffee Cup | `Mat_Token_ceramic` |
| `VentanaMarco` | Architecture | `[1.50, 1.20, 0.15]` | N/A | Wall-mounted | $0.0^\circ$ | Window Frame | `Mat_Token_glass` |

| Prefab Name | Category | BoundingBox $[W, H, D]$ | Navigation Clearance | Wall Offset | Rotation Snap | State Behavior | Material Token |
|---|---|---|---|---|---|---|---|
| `Prop_AttendanceList` | Narrative | `[0.25, 0.35, 0.02]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Clipboard | `Mat_Token_paper` |
| `Prop_Backpack` | Narrative | `[0.35, 0.40, 0.25]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Backpack | `Mat_Token_fabric` |
| `Prop_BlankBook` | Narrative | `[0.20, 0.25, 0.03]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Blank Book | `Mat_Token_paper` |
| `Prop_CenterBackpack` | Narrative | `[0.35, 0.40, 0.25]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Centered Backpack | `Mat_Token_fabric` |
| `Prop_ChalkDrawing` | Narrative | `[0.30, 0.01, 0.30]` | N/A | Floor-mounted | $0.0^\circ$ | Chalk Drawing | `Mat_Token_memory-amber` |
| `Prop_ChalkGraffiti` | Narrative | `[0.40, 0.01, 0.40]` | N/A | Wall-mounted | $0.0^\circ$ | Graffiti | `Mat_Token_memory-amber` |
| `Prop_Coat` | Narrative | `[0.60, 1.20, 0.15]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Amber Coat | `Mat_Token_memory-amber` |
| `Prop_CoffeeCups` | Narrative | `[0.10, 0.12, 0.10]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Coffee Cups | `Mat_Token_ceramic` |
| `Prop_DriedFlowers` | Narrative | `[0.20, 0.25, 0.20]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Dried Flowers | `Mat_Token_foliage` |
| `Prop_JanitorCart` | Narrative | `[0.80, 1.00, 0.50]` | $1.2\text{ m}$ | $0.05\text{ m}$ | $90.0^\circ$ | Janitor Cart | `Mat_Token_metal` |
| `Prop_LibraryStamp` | Narrative | `[0.05, 0.05, 0.05]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Library Stamp | `Mat_Token_ink` |
| `Prop_Notebook` | Narrative | `[0.20, 0.25, 0.03]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Amber Notebook | `Mat_Token_memory-amber` |
| `Prop_OverturnedDesk` | Narrative | `[1.30, 0.75, 0.50]` | $1.2\text{ m}$ | $1.50\text{ m}$ | $180.0^\circ (\pm 15^\circ)$ | Overturned Desk | `Mat_Token_wood` |
| `Prop_PhotoFrame` | Narrative | `[0.25, 0.20, 0.03]` | N/A | Wall-mounted | $0.0^\circ$ | Photo Frame | `Mat_Token_wood` |
| `Prop_RecordsBoard` | Narrative | `[0.60, 0.40, 0.05]` | N/A | Wall-mounted | $0.0^\circ$ | Records Board | `Mat_Token_paper` |
| `Prop_SoccerBall` | Narrative | `[0.25, 0.25, 0.25]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Soccer Ball | `Mat_Token_rubber` |
| `Prop_StoppedClock` | Narrative | `[0.35, 0.35, 0.05]` | N/A | Wall-mounted | $0.0^\circ$ | Stopped Clock | `Mat_Token_metal` |
| `Prop_Stopwatch` | Narrative | `[0.15, 0.20, 0.05]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Amber Stopwatch | `Mat_Token_memory-amber` |
| `Prop_TeacherNotebook` | Narrative | `[0.20, 0.25, 0.03]` | $0.5\text{ m}$ | N/A | $0.0^\circ$ | Teacher Notebook | `Mat_Token_paper` |

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
- `[FAIL-PRP-001]`: **Door Obstruction** — A locker blocks a doorway ($W_{clearance} = 0.6\text{m}$). Result: `LevelValidator` flags `FAIL-NAV-01`.
- `[FAIL-PRP-002]`: **Dynamic Rigidbody Attached** — A desk has an active `Rigidbody`. Result: `LevelValidator` flags `FAIL-PRP-02`.

### 13. CROSS REFERENCES
- [SCALE_GUIDE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SCALE_GUIDE.md) `[SPEC-106]`
- [ROOM_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ROOM_LIBRARY.md) `[SPEC-004]`
- [ENVIRONMENT_STORYTELLING.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ENVIRONMENT_STORYTELLING.md) `[SPEC-006]`
- [ASSET_GUID_REGISTRY.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ASSET_GUID_REGISTRY.yaml) `[SPEC-126]`

### 14. CHANGE HISTORY
- **v1.0 (2025-05-20)**: Prop grammar initial catalog.
- **v2.0 (2026-07-20)**: BoundingBox standardization.
- **v3.0 (2026-07-25)**: Full refactor into 14-section AI-Executable canonical format.
- **v4.0 (2026-07-25)**: Expanded to 142 entries with complete Asset GUID cross-reference.

(End of file - total 180 lines)