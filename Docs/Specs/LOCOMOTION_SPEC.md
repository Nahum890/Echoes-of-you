# LOCOMOTION_SPEC.md — Player Character Locomotion & Physical Dynamics
## Spec ID: SPEC-113
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Defines complete kinematic locomotion physics, CharacterController parameters, gravity constants, coyote time buffers, jump impulse calculations, and input action bindings for *Echoes of You 2.0*.

### 2. SCOPE
Applies to `PlayerController.cs`, `CharacterController` components, and Unity Physics ground checks. Excludes Echo playback locomotion (`EchoPlayback.cs`).

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHOES_BIBLE.md` (`SPEC-101`). Replaces `SPEC-203`.

### 4. DEFINITIONS
- `Walk Speed`: Horizontal velocity while walking ($2.8\text{ m/s}$).
- `Run Speed`: Maximum horizontal velocity when holding Shift ($4.2\text{ m/s}$).
- `Coyote Time`: Grace period ($0.12\text{s}$) allowing jump input after leaving a ledge.
- `Jump Buffer`: Input buffering window ($0.10\text{s}$) for jump commands pressed prior to grounding.

### 5. INPUTS
- [INPUT_ACTION_MAPS.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/INPUT_ACTION_MAPS.md) `[SPEC-118]`
- [PHYSICS_LAYER_MATRIX.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PHYSICS_LAYER_MATRIX.md) `[SPEC-116]`

### 6. OUTPUTS
- World-space displacement per frame driven via `CharacterController.Move()`.
- Grounded state flags consumed by `PlayerAnimator`.

### 7. RULES

- `[RULE-LOC-001]`: **Locomotion Speeds**: `walkSpeed` MUST equal $2.8\text{ m/s} \pm 0.0$. `runSpeed` MUST equal $4.2\text{ m/s} \pm 0.0$.
- `[RULE-LOC-002]`: **Jump & Gravity Dynamics**: Initial `jumpForce` MUST equal $5.5\text{ m/s} \pm 0.0$ under gravity $g = -18.0\text{ m/s}^2$. Terminal fall velocity cap MUST equal $-25.0\text{ m/s}$.
- `[RULE-LOC-003]`: **CharacterController Geometry**: Radius MUST equal $0.35\text{m}$, height MUST equal $1.80\text{m}$, center `[0.0, 0.90, 0.0]`, `stepOffset = 0.30m`, `slopeLimit = 45.0^\circ`, `skinWidth = 0.08m`.
- `[RULE-LOC-004]`: **Buffer Mechanics**: `coyoteTime = 0.12s`, `jumpBufferTime = 0.10s`. Ground check sphere MUST use `radius = 0.25m` at Y offset $-0.02\text{m}$.

### 8. ALGORITHMS

#### Algorithm 8.1: Locomotion Parameters Schema

```yaml
locomotion_config:
  walk_speed_mps: 2.8
  run_speed_mps: 4.2       # activado al mantener Shift
  gravity_mps2: -18.0      # más fuerte que -9.81 para sensación de peso
  jump_force_mps: 5.5      # velocidad inicial vertical en el momento del salto
  jump_apex_time_s: 0.55   # tiempo hasta apex calculado de jumpForce y gravity
  ground_check_radius_m: 0.25
  ground_check_offset_y: -0.02
  max_fall_speed_mps: -25.0
  coyote_time_s: 0.12      # gracia de borde de plataforma
  jump_buffer_s: 0.10      # buffer de input de salto antes de tocar suelo
  character_controller:
    radius: 0.35
    height: 1.80
    center: [0.0, 0.90, 0.0]
    step_offset: 0.30
    slope_limit: 45.0
    skin_width: 0.08

input_actions:
  move: "Player/Move"          # Vector2, WASD + LeftStick
  jump: "Player/Jump"          # Button, Space + SouthButton
  interact: "Player/Interact"  # Button, E + WestButton
  record_echo: "Player/RecordEcho" # Button, R + LeftShoulder
  soft_reset: "Player/SoftReset"   # Button, Q + Select
```

#### Algorithm 8.2: Kinematic Displacement Implementation in C#

```csharp
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private float verticalVelocity;
    private float coyoteTimer;
    private float jumpBufferTimer;

    public void ProcessLocomotion(Vector2 moveInput, bool jumpPressed, bool isSprinting)
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded) coyoteTimer = 0.12f;
        else coyoteTimer -= Time.deltaTime;

        if (jumpPressed) jumpBufferTimer = 0.10f;
        else jumpBufferTimer -= Time.deltaTime;

        float targetSpeed = isSprinting ? 4.2f : 2.8f;
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Vector3 velocity = moveDir * targetSpeed;

        if (coyoteTimer > 0f && jumpBufferTimer > 0f)
        {
            verticalVelocity = 5.5f;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
        }
        else if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2.0f;
        }
        else
        {
            verticalVelocity += -18.0f * Time.deltaTime;
            if (verticalVelocity < -25.0f) verticalVelocity = -25.0f;
        }

        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}
```

```yaml
physics_limits:
  max_slope_limit_deg: 45.0
  step_offset_m: 0.30
  ground_check_sphere_radius_m: 0.25
  ground_check_offset_y_m: -0.02
```

### 9. CONSTRAINTS
- `[CONS-LOC-001]`: Prohibido modifying gravity constant dynamically outside of `GravitationalSwitch` room modules.

### 10. VALIDATION
- `[VAL-LOC-001]`: Inspector validation asserts `CharacterController.radius == 0.35f` and `height == 1.80f`.

### 11. EXAMPLES
- Locomotion schema above.

### 12. FAILURE CASES
- `[FAIL-LOC-001]`: **Grounding Glitch**: Step offset $> 0.30\text{m}$ causes character to climb 1m puzzle blocks. Result: Validator flags `FAIL-LOC-01`.

### 13. CROSS REFERENCES
- [ECHO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_GRAMMAR.md) `[SPEC-107]`
- [INPUT_ACTION_MAPS.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/INPUT_ACTION_MAPS.md) `[SPEC-118]`

### 14. CHANGE HISTORY
- **v4.0 (2026-07-25)**: Created canonical SPEC-113 upgrading SPEC-203 with complete 14-section format, coyote time, jump buffer, and CharacterController contract.
