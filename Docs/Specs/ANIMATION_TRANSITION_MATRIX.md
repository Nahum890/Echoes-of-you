# ANIMATION_TRANSITION_MATRIX.md — Player Animator Controller & Echo Replay Specs
## Spec ID: SPEC-128
## Version: 1.0 (AI-Executable)

---

### 1. PURPOSE
Defines frame-exact transition parameters, blend durations, parameter types, and layer setups for `PlayerAnimator.controller`, as well as specifying the non-Animator deterministic replay mechanism for Echo ghosts.

### 2. SCOPE
Applies to Player character Animator Controller assets and Echo replay transform evaluation loops.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ANIMATION_STATE_MACHINE.md` (`SPEC-117`).

### 4. DEFINITIONS
- `Direct Transform Playback`: Echo playback mode bypassing Unity Animator Controller to linearly interpolate cached 30Hz position/rotation frames with $t = 0.85$.
- `Layer 1 Additive`: Animator layer blending upper-body recording animation over base movement.

### 5. INPUTS
- [ANIMATION_STATE_MACHINE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ANIMATION_STATE_MACHINE.md) `[SPEC-117]`
- [ANIMATION_TRANSITION_MATRIX.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ANIMATION_TRANSITION_MATRIX.yaml) `[SPEC-128]`

### 6. OUTPUTS
- Animator Controller setup and Echo transform playback configuration.

### 7. RULES
- `[RULE-ANI-001]`: **Echo Replay Bypasses Animator**: Echo entities MUST NOT invoke Unity Animator State Machine; transform position/rotation MUST be driven directly from sample buffer at $30\text{Hz}$.
- `[RULE-ANI-002]`: **Blend Durations**: Transitions MUST strictly observe blend durations in `ANIMATION_TRANSITION_MATRIX.yaml` (Walk $\rightarrow$ Jump: $0.05\text{s}$, Idle $\rightarrow$ Walk: $0.15\text{s}$).

### 8. ALGORITHMS
See canonical configuration at `Docs/Specs/ANIMATION_TRANSITION_MATRIX.yaml`.

### 9. CONSTRAINTS
- `[CONS-ANI-001]`: Prohibido instantiating Animator component on active Echo replay body.

### 10. VALIDATION
- `[VAL-ANI-001]`: `LevelValidator.cs` verifies `PlayerAnimator.controller` parameter table matching `ANIMATION_TRANSITION_MATRIX.yaml`.

### 11. EXAMPLES
- `ANIMATION_TRANSITION_MATRIX.yaml` configuration.

### 12. FAILURE CASES
- `[FAIL-ANI-001]`: **Echo Desync**: Enabling Animator on Echo causing root motion deviation from recorded frame buffer. Result: `FAIL-ANI-01`.

### 13. CROSS REFERENCES
- [ANIMATION_STATE_MACHINE.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ANIMATION_STATE_MACHINE.md) `[SPEC-117]`

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Created canonical SPEC-128 for Animation Transition Matrix.
