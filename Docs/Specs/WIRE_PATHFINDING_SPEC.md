# WIRE_PATHFINDING_SPEC.md — Puzzle Wire Mesh Generation & Routing Algorithm
## Spec ID: SPEC-125
## Version: 1.0 (AI-Executable)

---

### 1. PURPOSE
Defines the deterministic pathfinding and mesh generation algorithm for `PuzzleWire.cs`, ensuring wire meshes route smoothly over NavMesh geometry without clipping through walls or floating unnaturally.

### 2. SCOPE
Applies to signal transmission wiring generated during Pass 2 of `EchoesNewProductionBuilder.cs` and runtime wire procedural mesh generation between pressure plates, triggers, and doors.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `PUZZLE_GRAMMAR.md` (`SPEC-104`).

### 4. DEFINITIONS
- `Floor Projected Path`: NavMesh path calculated between source and target, offset vertically by `vertex_floor_clearance_m` ($0.05\text{m}$).
- `Wire Cylinder Mesh`: Procedural cylindrical tube mesh generated along wire waypoints with `mesh_radius_m` ($0.04\text{m}$).

### 5. INPUTS
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`
- [NAVMESH_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/NAVMESH_SPEC.md) `[SPEC-114]`
- [CONSTANTS_REGISTRY.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CONSTANTS_REGISTRY.yaml) `[SPEC-124]`

### 6. OUTPUTS
- Generated `MeshFilter` and `MeshRenderer` components on `PuzzleWire` GameObjects.

### 7. RULES
- `[RULE-WIR-001]`: **NavMesh Project Routing**: PuzzleWire path MUST be generated via `NavMesh.CalculatePath` floor projection when waypoints $> 1$.
- `[RULE-WIR-002]`: **Direct Line Fallback**: If NavMesh path is invalid or non-existent, fallback to direct line with `WARN-WIRE-01` warning log.
- `[RULE-WIR-003]`: **Dimensions**: Wire mesh radius MUST equal $0.04\text{m}$, floor clearance MUST equal $0.05\text{m}$.

### 8. ALGORITHMS

#### Algorithm 8.1: PuzzleWire Mesh Generation Algorithm (COMPLETE)
```csharp
// Algorithm 8.1 COMPLETE: PuzzleWire with height-change handling
public static Vector3[] CalculateWirePath(Vector3 source, Vector3 target)
{
    const float FLOOR_CLEARANCE = 0.05f;
    const float STEP_SAMPLE_DIST = 0.5f;  // Sample every 0.5m

    NavMeshPath path = new NavMeshPath();
    NavMesh.CalculatePath(source + Vector3.up * 0.1f,
                          target + Vector3.up * 0.1f,
                          NavMesh.AllAreas, path);

    if (path.status == NavMeshPathStatus.PathComplete)
    {
        // Project each waypoint to the real floor using downward Raycast
        Vector3[] result = new Vector3[path.corners.Length];
        for (int i = 0; i < path.corners.Length; i++)
        {
            if (Physics.Raycast(path.corners[i] + Vector3.up * 2f,
                                Vector3.down, out RaycastHit hit, 4f,
                                LayerMask.GetMask("Default", "Architecture")))
                result[i] = hit.point + Vector3.up * FLOOR_CLEARANCE;
            else
                result[i] = path.corners[i] + Vector3.up * FLOOR_CLEARANCE;
        }
        return result;
    }

    // FALLBACK when NavMesh fails: linear interpolation over the floor
    // Sample at STEP_SAMPLE_DIST intervals with downward Raycast
    int steps = Mathf.Max(2, Mathf.CeilToInt(
        Vector3.Distance(source, target) / STEP_SAMPLE_DIST));
    Vector3[] fallback = new Vector3[steps];
    for (int i = 0; i < steps; i++)
    {
        Vector3 lerped = Vector3.Lerp(source, target, (float)i / (steps - 1));
        if (Physics.Raycast(lerped + Vector3.up * 3f, Vector3.down,
                            out RaycastHit h, 6f,
                            LayerMask.GetMask("Default", "Architecture")))
            fallback[i] = h.point + Vector3.up * FLOOR_CLEARANCE;
        else
            fallback[i] = lerped + Vector3.up * FLOOR_CLEARANCE;
    }
    Debug.LogWarning($"WARN-WIRE-01: NavMesh fallback used for wire {source}→{target}");
    return fallback;
}
```

#### Algorithm 8.2: Wire Cylinder Mesh Construction
```csharp
// Algorithm 8.2: Wire cylinder mesh generation
// [RULE-WIR-004] Cylinder mesh must use exactly 8 sides (sides parameter)
public static Mesh BuildWireMesh(Vector3[] waypoints, float radius = 0.04f, int sides = 8)
{
    // Generates a cylinder of 'sides' faces between each pair of waypoints.
    // Total vertices = waypoints.Length × (sides + 1)  [rings of vertices]
    // Total triangles = (waypoints.Length - 1) × sides × 2  [two tris per face]
    // UV: u = angle / (2π), v = accumulated distance / total length (seamless tiling)
    // radius must equal 0.04m per RULE-WIR-003
    // sides = 8 default for performance; max 12 for close-up wires
    if (waypoints == null || waypoints.Length < 2)
    {
        Debug.LogError("FAIL-WIR-002: BuildWireMesh requires at least 2 waypoints.");
        return null;
    }
    // ... mesh generation implementation by EchoesModuleFactory.cs
    // See Assets/Scripts/Puzzle/PuzzleWire.cs BuildMesh() for runtime implementation
    throw new System.NotImplementedException("Implement in PuzzleWire.cs");
}
```


### 9. CONSTRAINTS
- `[CONS-WIR-001]`: Prohibido rendering un-projected wires floating $> 0.1\text{m}$ above floor surfaces.

### 10. VALIDATION
- `[VAL-WIR-001]`: `LevelValidator.cs` verifies all `PuzzleWire` objects have non-null mesh references and valid waypoint arrays.

### 11. EXAMPLES
- Algorithm 8.1 C# implementation snippet above.

### 12. FAILURE CASES
- `[FAIL-WIR-001]`: **Unconnected Circuit**: PuzzleWire fails to generate mesh. Result: `FAIL-WIR-01`.

### 13. CROSS REFERENCES
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Created canonical SPEC-125 for wire pathfinding algorithm.
