# SOURCE_OF_TRUTH.md — Supreme Authority & Resolution System
## Spec ID: SPEC-000
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
This document establishes the supreme authority hierarchy, conflict resolution protocol, and frozen architectural decisions for *Echoes of You 2.0*. It serves as the ultimate tie-breaker for human developers and specialized AI agents when instructions or specifications conflict.

### 2. SCOPE
Applies to 100% of codebase assets, level blueprints, C# scripts, URP shaders, and specification documents in `Docs/`. Explicitly excludes external third-party package internals (e.g. Cinemachine package source).

### 3. AUTHORITY
Nivel 1 (Supreme Level Authority). Overrides all other documents in `Docs/`, all comments in `Assets/Scripts/**/*.cs`, and all temporary prompts.

### 4. DEFINITIONS
- `Authority Level`: Priority rank from Level 1 (Highest) to Level 8 (Lowest).
- `Conflict`: Any condition where two specifications or code files dictate contradictory rules.
- `Frozen Decision`: A architectural decision that cannot be altered without an explicit entry in `Docs/Authority/CHANGE_CONTROL.md`.
- `Deterministic Standard`: A quantitative rule expressed in exact SI units ($m, s, ^\circ, Lux, Hz, Hex$).

### 5. INPUTS
- [CHANGE_CONTROL.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Authority/CHANGE_CONTROL.md) `[SPEC-000D]`
- Repository root state (`Assets/` and `Docs/`).

### 6. OUTPUTS
- Binding resolution directives for AI execution loops.
- Compliance tokens for `LevelValidator.cs` and `AI_RULEBOOK.md`.

### 7. RULES

- `[RULE-SOT-001]`: **Hierarchy Precedence Rule**: When two documents or specifications conflict, the resolution MUST follow the strict hierarchy rank without exception:
  1. *Level 1*: `SOURCE_OF_TRUTH.md` (`SPEC-000`), `CONSTANTS_REGISTRY.yaml` (`SPEC-124`), `ECHOES_BIBLE.md` (`SPEC-101`), `CHANGE_CONTROL.md` (`SPEC-000D`). Note: `CONSTANTS_REGISTRY.yaml` (`SPEC-124`) is the single canonical source of truth for all numerical primitives.
  2. *Level 1A (Runtime Data Contract)*: `Docs/ExecutableSpecs/**/*.yaml` (Contrato de Datos Runtime Ejecutable).
  3. *Level 2*: `PROJECT_CONTEXT.md` (`SPEC-110`), `DESIGN_PHILOSOPHY.md` (`SPEC-001`)
  4. *Level 3 (Declarative Specs)*: `Docs/Specs/*.md` (`SPEC-002` to `SPEC-202`) (Especificación Técnica Declarativa de Soporte)
  5. *Level 4*: `Docs/Validation/*.md` (`SPEC-301` to `SPEC-304`)
  6. *Level 5*: `Docs/AI/*.md` (`SPEC-401` to `SPEC-402`)
  7. *Level 6*: Active Production Code (`Assets/Scripts/**/*.cs`, `Assets/Editor/EchoesNewProductionBuilder.cs`)
  8. *Level 7*: Data Assets (`Assets/Data/Levels/*.asset`)
  9. *Level 8*: Historical/Obsolescent files (`Docs/Archive/*`)
- `[RULE-SOT-001B]`: **YAML Runtime Precedence (SCOPED)**: Los archivos YAML en `Docs/ExecutableSpecs/*.yaml` prevalecen sobre Markdown SOLO para:
  - Parámetros de instanciación de escena (posiciones, rotaciones, customData)
  - Parámetros de presentación visual (colores de perfiles no congelados)
  - Metadatos de nivel no congelados

  Los archivos YAML **NUNCA** pueden sobrescribir entradas en la Frozen Decisions Matrix (Algorithm 8.2). Toda entrada en Algorithm 8.2 es inviolable en TODOS los niveles. Intentar sobrescribir un valor congelado mediante YAML genera `FAIL-SOT-01`.
- `[RULE-SOT-002]`: **Frozen Decision Binding**: The decisions listed in Section 8 (Algorithms) are locked and immutable. No AI agent or human developer may deviate from these decisions.
- `[RULE-SOT-003]`: **Ambiguity Resolution Protocol**: If an unlisted contradiction is encountered, execution MUST halt immediately and prompt for resolution via `CHANGE_CONTROL.md`.

### 8. ALGORITHMS

#### Algorithm 8.1: Conflict Resolution Decision Tree
```mermaid
graph TD
    A[Conflict Detected Between Doc X and Doc Y] --> B{Determine Authority Level of Doc X and Y}
    B -->|Level(X) < Level(Y)| C[Doc X Prevails — Enforce Rule X]
    B -->|Level(Y) < Level(X)| D[Doc Y Prevails — Enforce Rule Y]
    B -->|Level(X) == Level(Y)| E{Covered in Frozen Decisions Matrix?}
    E -->|Yes| F[Enforce Option A from Frozen Matrix]
    E -->|No| G[Halt Execution & Log Conflict in CHANGE_CONTROL.md]
```

#### Algorithm 8.2: Frozen Decisions Matrix

| Topic / System | Prohibited / Obsolescent Option B | Official / Binding Option A | Technical Directive |
|---|---|---|---|
| **Constants Registry** | Valores hardcodeados dispersos en Markdown/YAML | **`CONSTANTS_REGISTRY.yaml` `[SPEC-124]`** | Todos los valores numéricos DEBEN referenciar claves de `CONSTANTS_REGISTRY.yaml`. Hardcodear valores cubiertos por este registro genera `FAIL-SOT-01`. |
| **Render Pipeline** | Standard / Built-in Shader | **Universal Render Pipeline (URP)** | Material base utiliza `Universal Render Pipeline/Lit`. Props especiales de Echo y atmósfera utilizan custom shaders en `Assets/Shaders/` según `SHADER_SPEC.md` `[SPEC-109]`. La restricción aplica SOLO a shaders de geometría estática de colegio. Prohibido `Shader.Find("Standard")`. |
| **Active Builder** | `EchoesLevelBuilder.cs` / `EchoesProductionBuilder.cs` | **`EchoesNewProductionBuilder.cs`** | `EchoesNewProductionBuilder.cs` is the single active generator. |
| **Echo Max Duration** | 6.0s / 10.0s | **12.0s Standard Max** (20.0s Narrative) | `EchoRecorder.maxRecordSeconds = 12.0f` standard default. Ref: `CONSTANTS_REGISTRY.yaml` `echo.max_record_seconds_standard`. |
| **Art Style Target** | Sci-Fi / Cyberpunk / 4K PBR | **PS1/PS2 Liminal School Aesthetic** | Flat colors, HSL saturation $[0.10, 0.35]$, linear fog. |
| **Camera Controller** | Dual controllers in `LateUpdate` | **Single Active Camera Controller** | Exactly 1 active camera controller operating per scene. |

### 9. CONSTRAINTS
- `[CONS-SOT-001]`: Prohibido delegar la resolución de conflictos a algoritmos de probabilidad o suposiciones heurísticas.
- `[CONS-SOT-002]`: Prohibido modificar las asignaciones de Nivel de Autoridad sin actualizar la Version a 3.x.

### 10. VALIDATION
- `[VAL-SOT-001]`: Automated CI script parses all Markdown files and verifies that no Level 3+ document attempts to override a Level 1 rule.
- `[VAL-SOT-002]`: Verification script checks `Assets/Scripts/` for references to obsolete builder classes (`EchoesLevelBuilder`) and asserts 0 references.

### 11. EXAMPLES

#### Example 11.1: Valid Resolution of Pipeline Conflict
```yaml
conflict:
  file_a: "Docs/Archive/LIGHTING_GUIDE.md (Level 8)"
  rule_a: "Use Standard Shader with Metallic 0.8"
  file_b: "Docs/Specs/LIGHTING_GRAMMAR.md (Level 3)"
  rule_b: "Use URP Lit Shader with Flat Colors"
resolution:
  winner: "Docs/Specs/LIGHTING_GRAMMAR.md (Level 3 > Level 8)"
  action: "Apply URP Lit Shader with Flat Colors"
```

### 12. FAILURE CASES
- `[FAIL-SOT-001]`: **Unresolved Conflict Deadlock**: An unlisted conflict between documents of equal authority level occurs. Result: Build pipeline aborts with exit code 101.
- `[FAIL-SOT-002]`: **Obsolete Code Execution**: Code invokes `EchoesLevelBuilder.cs` instead of `EchoesNewProductionBuilder.cs`. Result: Compiler error `ERR-BLD-01`.

### 13. CROSS REFERENCES
- [ECHOES_BIBLE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/ECHOES_BIBLE.md) `[SPEC-101]`
- [CHANGE_CONTROL.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Authority/CHANGE_CONTROL.md) `[SPEC-000D]`
- [INDEX.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/INDEX.md) `[SPEC-INDEX]`

### 14. CHANGE HISTORY
- **v1.0 (2025-03-10)**: Initial draft of authority rules.
- **v2.0 (2026-07-20)**: Added Frozen Decisions Matrix.
- **v3.0 (2026-07-25)**: Full refactor into 14-section AI-Executable canonical format.
- **v3.1 (2026-07-25)**: CC-2026-010 — RULE-SOT-001B scoped to non-frozen params. CC-2026-011 — AI specs correctly assigned Level 5. CONSTANTS_REGISTRY.yaml added to Frozen Decisions Matrix.
