# ANIMATION_STATE_MACHINE.md — Animator Controllers & Echo Ghosting Specifications
## Spec ID: SPEC-117
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Defines Animator parameters, state machine transitions, blend durations, layer definitions, and Echo 15fps ghosting playback logic for *Echoes of You 2.0*.

### 2. SCOPE
Applies to `Assets/Animation/Player/PlayerAnimator.controller`, `PlayerController.cs`, and `EchoPlayback.cs`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHO_GRAMMAR.md` (`SPEC-107`).

### 4. DEFINITIONS
- `Echo Ghosting`: Echo playback capping frame sampling to 15fps ($0.0667\text{s}$) to achieve a PS1 retro ghost effect without running a full independent Animator controller.

### 5. INPUTS
- [LOCOMOTION_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LOCOMOTION_SPEC.md) `[SPEC-113]`
- [ECHO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_GRAMMAR.md) `[SPEC-107]`

### 6. OUTPUTS
- Configured Animator Controller asset and `EchoPlayback.cs` frame sampler.

### 7. RULES

- `[RULE-ANI-001]`: **Animator Controller Path**: Player Animator MUST use asset `Assets/Animation/Player/PlayerAnimator.controller`.
- `[RULE-ANI-002]`: **Parameters Definition**: Animator MUST expose `Speed` (float), `IsGrounded` (bool), `IsJumping` (bool), `IsRecording` (bool), `IsInteracting` (bool).
- `[RULE-ANI-003]`: **Echo Playback Mechanics**: Echo MUST set `animator.speed = 0.0`. Echo DOES NOT run an Animator state machine; it directly interpolates positions from the frame buffer capped at 15fps ($15.0\text{ fps}$).

### 8. ALGORITHMS

#### Algorithm 8.1: Animation State Machine Schema

```yaml
animator_controller: "Assets/Animation/Player/PlayerAnimator.controller"
parameters:
  - name: "Speed"          type: float   default: 0.0
  - name: "IsGrounded"     type: bool    default: true
  - name: "IsJumping"      type: bool    default: false
  - name: "IsRecording"    type: bool    default: false
  - name: "IsInteracting"  type: bool    default: false

states:
  Idle:
    motion: "Player_Idle.anim"
    transitions:
      - to: "Walk"     condition: "Speed > 0.1"   duration_s: 0.15
  Walk:
    motion: "Player_Walk.anim"
    speed_param: "Speed"
    transitions:
      - to: "Idle"     condition: "Speed < 0.05"  duration_s: 0.10
      - to: "Jump"     condition: "IsJumping"      duration_s: 0.05
  Jump:
    motion: "Player_Jump.anim"
    transitions:
      - to: "Idle"     condition: "IsGrounded && !IsJumping" duration_s: 0.20
  Record:
    motion: "Player_Record.anim"   # ciclo con mano alzada indicando grabación
    layer: 1   # capa aditiva — no interrumpe locomoción

echo_animator:
  playback_frame_rate_cap_fps: 15.0   # sampleo a 15fps para efecto de ghosting PS1
  animator_speed: 0.0    # Echo NO usa Animator propio — reproduce posiciones del buffer
```

### 9. CONSTRAINTS
- `[CONS-ANI-001]`: Prohibido executing independent Animator state machine updates on Echo clones.

### 10. VALIDATION
- `[VAL-ANI-001]`: `LevelValidator.cs` asserts `PlayerAnimator.controller` contains required 5 parameters.

### 11. EXAMPLES
- Animation schema above.

### 12. FAILURE CASES
- `[FAIL-ANI-001]`: **Missing Animation Parameter**: Parameter `Speed` missing from Animator Controller. Result: `FAIL-ANI-01`.

### 13. CROSS REFERENCES
- [ECHO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_GRAMMAR.md) `[SPEC-107]`

### 14. CHANGE HISTORY
- **v4.0 (2026-07-25)**: Created canonical SPEC-117 defining Animator state machine parameters and 15fps Echo ghosting spec.
