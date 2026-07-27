# Docs/ExecutableSpecs/build/Build_Deployment_Pipeline.md
# Build & Deployment Pipeline Specification
## Spec ID: SPEC-203
## Version: 1.0 (AI-Executable)

---

### 1. PURPOSE
Defines the complete CI/CD build pipeline for *Echoes of You 2.0* across all target platforms with automated validation gates.

### 2. SCOPE
Applies to GitHub Actions workflows, Unity Cloud Build, and local build scripts. Covers Windows64, Linux64, MacOS targets.

### 3. AUTHORITY
Level 4 (Pipeline). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`). Runtime contract in `build_deployment_pipeline.yaml` (`SPEC-EXEC-BUILD`).

### 4. DEFINITIONS
- `Build Target`: Platform-specific player build (Win64, Linux64, MacOS).
- `Validation Gate`: Automated check that must pass before build promotion.

### 5. INPUTS
- [build_deployment_pipeline.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/build/build_deployment_pipeline.yaml) `[SPEC-EXEC-BUILD]`
- [PERFORMANCE_BUDGET_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PERFORMANCE_BUDGET_SPEC.md) `[SPEC-121]`

### 6. OUTPUTS
- Platform-specific build artifacts in `Builds/{target}/{version}/`.
- Validation reports in `Reports/`.

### 7. RULES
- `[RULE-BLD-001]`: **Target Platforms** — Build MUST produce Win64, Linux64, MacOS players.
- `[RULE-BLD-002]`: **Compression** — LZ4 compression MUST be enabled.
- `[RULE-BLD-003]`: **Scripting Backend** — IL2CPP MUST be used; Mono is PROHIBITED.
- `[RULE-BLD-004]`: **Engine Code Stripping** — `strip_engine_code: true` MUST be set.
- `[RULE-BLD-005]`: **Scene List Source** — Scenes MUST be sourced from `LEVEL_SPEC_01..15` manifest.
- `[RULE-BLD-006]`: **Pre-Build Validation** — `ExecutableSpecValidator`, `EchoesNewProductionBuilder`, `LevelValidator` MUST pass before build.

### 8. ALGORITHMS
Pipeline stages defined in `build_deployment_pipeline.yaml`:
1. `ValidateSpecs` — Run `ExecutableSpecValidator.ValidateProject`
2. `BuildBlueprints` — Run `EchoesNewProductionBuilder.BuildAllBlueprints`
3. `ValidateScenes` — Run `LevelValidator.RunAllChecks` (all 15 levels ≥ 80/100)
4. `VisualRegression` — Run `VisualRegressionTest.Run` (pixel diff ≤ 2%)
5. `PerformanceStress` — Run `PerformanceStressHarness.Run` (frame time ≤ 16.67ms)
6. `VNUnitTests` — Run `VN_EndingResolver` tests (32 paths)
7. `BuildPlayers` — Build for all 3 targets
8. `PostBuildValidation` — Verify build artifacts

### 9. CONSTRAINTS
- `[CONS-BLD-001]`: Prohibido manual build steps; all steps MUST be automated.
- `[CONS-BLD-002]`: Prohibido shipping Debug or Development builds.

### 10. VALIDATION
- `[VAL-BLD-001]`: CI pipeline asserts zero compile errors, zero missing refs.
- `[VAL-BLD-002]`: All 15 levels score ≥ 80/100.
- `[VAL-BLD-003]`: Visual regression pixel diff ≤ 2%.
- `[VAL-BLD-004]`: Frame time ≤ 16.67ms sustained.

### 11. EXAMPLES
See `.github/workflows/docs-validation.yml` for canonical workflow.

### 12. FAILURE CASES
- `[FAIL-BLD-001]`: **Validation Gate Failure** — Any gate fails → pipeline aborts.

### 13. CROSS REFERENCES
- [build_deployment_pipeline.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/ExecutableSpecs/build/build_deployment_pipeline.yaml) `[SPEC-EXEC-BUILD]`
- [PERFORMANCE_BUDGET_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PERFORMANCE_BUDGET_SPEC.md) `[SPEC-121]`

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Initial canonical SPEC-203 for Build & Deployment Pipeline.

(End of file - total 62 lines)