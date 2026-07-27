# LANDMARK_GRAMMAR.md — Landmark Placement & Sightline Predicates
## Spec ID: SPEC-103C
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Defines landmark placement rules, sightline visibility evaluation algorithms, and programmatic predicates for visual orientation props in *Echoes of You 2.0*.

### 2. SCOPE
Applies to `EchoesPropDecorator.cs`, `LevelValidator.cs`, and corridor visual landmark placement.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ANTI_PATTERNS.md` (`SPEC-002`).

### 4. DEFINITIONS
- `Landmark`: A high-contrast visual anchor prop (e.g. `Pizarra`, `Prop_Clock`, `MochilaLyra`) placed at corridor turn thresholds to guide player navigation.
- `Visibility Predicate`: Code-evaluable Raycast check asserting un-occluded line of sight.

### 5. INPUTS
- [ANTI_PATTERNS.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ANTI_PATTERNS.md) `[SPEC-002]`
- [PROP_LIBRARY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PROP_LIBRARY.md) `[SPEC-005]`

### 6. OUTPUTS
- Sightline validation checks executed by `LevelValidator.cs`.

### 7. RULES

- `[RULE-LND-001]`: **Corridor Landmark Predicate**: Any straight corridor length $L_{corridor} > 18.0\text{m}$ MUST satisfy the visibility predicate:
  $$\text{Raycast}(\text{corridorEntrance}, \vec{f}, 18.0\text{m}).\text{hit}.\text{tag} == \text{"Landmark"}$$
- `[RULE-LND-002]`: **Landmark Occlusion Ceiling**: A landmark MUST NOT be occluded by static geometry for $> 0.2\text{s}$ during continuous forward player movement along primary corridor paths.

### 8. ALGORITHMS

#### Algorithm 8.1: Programmatic Raycast Visibility Predicate
```csharp
public static bool EvaluateLandmarkVisibility(Vector3 entrancePos, Vector3 forwardDir, float maxDistance, out string error)
{
    if (Physics.Raycast(entrancePos, forwardDir, out RaycastHit hit, maxDistance))
    {
        if (hit.collider.CompareTag("Landmark"))
        {
            error = null;
            return true; // Predicate Satisfied
        }
    }
    error = "FAIL-ARC-01: Corridor > 18.0m lacks visible Landmark along sightline vector.";
    return false;
}
```

### 9. CONSTRAINTS
- `[CONS-LND-001]`: Prohibido placing landmarks behind solid door colliders without glass panels.

### 10. VALIDATION
- `[VAL-LND-001]`: `LevelValidator.cs` executes Algorithm 8.1 across all corridors $> 18.0\text{m}$.

### 11. EXAMPLES
- Landmark Raycast evaluation log output.

### 12. FAILURE CASES
- `[FAIL-LND-001]`: **Occluded Landmark**: Raycast hits wall instead of landmark. Result: `LevelValidator` flags `FAIL-ARC-01`.

### 13. CROSS REFERENCES
- [ANTI_PATTERNS.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ANTI_PATTERNS.md) `[SPEC-002]`
- [LEVEL_VALIDATOR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Validation/LEVEL_VALIDATOR.md) `[SPEC-301]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec converting landmark visibility rules into code-evaluable Raycast predicates.
