# AUTOMATED_PUZZLE_SOLVER_SPEC.md — Algoritmo de Validación de Solvabilidad de Puzzle
## Spec ID: SPEC-EXEC-SOLVER-001
## Version: 1.0 (AI-Executable)

---

### 1. PURPOSE
Specifies the deterministic BFS-based graph-search algorithm that serves as the implementation reference for `VAL-B-01` ("Echo Button Test"). An automated AI builder or `LevelValidator.cs` MUST use this algorithm to programmatically determine whether a puzzle graph requires an Echo recording or dual-body presence to be solved.

### 2. SCOPE
Applies to `LevelValidator.cs` (`VAL-B-01`), `EchoesNewProductionBuilder.cs` (Pass 5), and any automated CI pipeline that verifies puzzle solvability before scene commit.

### 3. AUTHORITY
Level 1A (Executable Spec). Subordinate only to `SOURCE_OF_TRUTH.md` (`SPEC-000`).

### 4. DEFINITIONS
- `Signal Graph`: A directed acyclic graph (DAG) where nodes represent puzzle components (plates, wires, doors, echo zones) and edges represent signal flow from triggers to locks.
- `Echo-Required Node`: A node whose `requiresEcho == true` or `componentType == "PressurePlate_EchoOnly"`.
- `Reachable Path`: A path from any root source node to a `LevelExit` node that traverses at least one `Echo-Required Node`.

### 5. INPUTS
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`
- [LEVEL_VALIDATOR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Validation/LEVEL_VALIDATOR.md) `[SPEC-301]`

### 6. OUTPUTS
- Boolean `requiresEcho` result.
- Failure code `FAIL-PUZ-01` when no Echo-required path exists.

### 7. RULES
- `[RULE-SOLVER-001]`: The algorithm MUST evaluate the puzzle graph from all source nodes (PressurePlates, EchoZones) to all sink nodes (LevelExit, Door-Unlocked).
- `[RULE-SOLVER-002]`: A puzzle PASSES if and only if at least one mandatory path through the graph contains exactly one or more `Echo-Required Node`.
- `[RULE-SOLVER-003]`: The algorithm MUST complete in $O(V + E)$ time where $V$ = node count, $E$ = edge count.

### 8. ALGORITHMS

#### Algorithm 8.1: Echo Button Test — BFS Signal Graph Traversal

```
INPUT:  PuzzleGraph G = {nodes V, edges E}
        EchoRequiredSet S = { v ∈ V | v.requiresEcho == true }
        ExitNodes X = { v ∈ V | v.type == "LevelExit" }

OUTPUT: requiresEcho ∈ {true, false}
        FAIL-PUZ-01 if requiresEcho == false

PROCEDURE EchoButtonTest(G, S, X):
  visited ← {}
  queue  ← X                      // Backwards BFS from all exits
  requiresEcho ← false

  WHILE queue is not empty:
    node ← queue.dequeue()
    IF node ∈ visited: continue
    visited ← visited ∪ {node}

    IF node ∈ S:                   // Found Echo-required node on path
      requiresEcho ← true
      BREAK

    FOR each predecessor p of node in G:
      queue.enqueue(p)

  RETURN requiresEcho
  IF NOT requiresEcho: RAISE FAIL-PUZ-01
```

#### Algorithm 8.2: C# Reference Implementation

```csharp
public static bool EchoButtonTest(LevelBlueprint blueprint, out string error)
{
    // Build adjacency: for each node, what predecessors feed it?
    var predecessors = new Dictionary<string, List<string>>();
    var echoRequired = new HashSet<string>();

    foreach (var comp in blueprint.puzzle.components)
    {
        if (!predecessors.ContainsKey(comp.id))
            predecessors[comp.id] = new List<string>();

        foreach (var output in comp.signalOutputs ?? new List<string>())
        {
            if (!predecessors.ContainsKey(output))
                predecessors[output] = new List<string>();
            predecessors[output].Add(comp.id);  // comp feeds 'output'
        }

        if (comp.requiresEcho || comp.type == "PressurePlate_EchoOnly")
            echoRequired.Add(comp.id);
    }

    // BFS backwards from exit nodes
    bool requiresEcho = false;
    var visited = new HashSet<string>();
    var queue = new Queue<string>();

    foreach (var exit in blueprint.puzzle.exitNodes)
        queue.Enqueue(exit);

    while (queue.Count > 0)
    {
        string node = queue.Dequeue();
        if (!visited.Add(node)) continue;

        if (echoRequired.Contains(node))
        {
            requiresEcho = true;
            break;
        }

        if (predecessors.TryGetValue(node, out var preds))
            foreach (var p in preds) queue.Enqueue(p);
    }

    if (!requiresEcho)
    {
        error = "FAIL-PUZ-01: Puzzle graph has no Echo-required path. Puzzle is trivially solvable.";
        return false;
    }

    error = null;
    return true;
}
```

### 9. CONSTRAINTS
- `[CONS-SOLVER-001]`: Prohibido executing puzzle generation if `EchoButtonTest()` returns `false`.
- `[CONS-SOLVER-002]`: Every level MUST have `puzzle.exitNodes` populated with at least 1 node ID.

### 10. VALIDATION
- `[VAL-SOLVER-001]`: Unit test asserts `EchoButtonTest()` returns `true` on all 15 level blueprints.
- `[VAL-SOLVER-002]`: Unit test asserts `EchoButtonTest()` returns `false` on a crafted trivial (no-Echo) blueprint and raises `FAIL-PUZ-01`.

### 11. EXAMPLES
- Level 01 blueprint: `PressurePlate → [wire_01] → Door`. If `PressurePlate.requiresEcho == true`, test passes.

### 12. FAILURE CASES
- `[FAIL-SOLVER-001]`: **Cycle Detected in Signal Graph**: BFS never terminates. Result: Circuit breaker at `maxIterations = V * 2` breaks loop and raises `FAIL-PUZ-04`.

### 13. CROSS REFERENCES
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`
- [LEVEL_VALIDATOR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Validation/LEVEL_VALIDATOR.md) `[SPEC-301]`

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Created. Resolves HALT-11 (non-algorithmic Echo Button Test). Adds BFS pseudocode and C# reference implementation.
