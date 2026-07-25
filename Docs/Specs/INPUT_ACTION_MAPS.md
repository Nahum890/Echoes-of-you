# INPUT_ACTION_MAPS.md — Unity Input System Action Maps Specification
## Spec ID: SPEC-118
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Defines the input action maps, bindings for Keyboard/Mouse and Gamepad, composite vector definitions, and analog deadzones for *Echoes of You 2.0*.

### 2. SCOPE
Applies to `Assets/Settings/EchoesInputActions.inputactions`, `PlayerInput` components, and UI navigation.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`).

### 4. DEFINITIONS
- `Player Map`: Action map active during 3D gameplay locomotion and interaction.
- `UI Map`: Action map active during pause menus, memory logs, and dialog overlays.

### 5. INPUTS
- [LOCOMOTION_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LOCOMOTION_SPEC.md) `[SPEC-113]`

### 6. OUTPUTS
- Configured `.inputactions` asset consumed by `PlayerInput`.

### 7. RULES

- `[RULE-INP-001]`: **Input Asset Location**: Input asset MUST exist at `Assets/Settings/EchoesInputActions.inputactions`.
- `[RULE-INP-002]`: **Action Map Structure**: Asset MUST contain exactly 2 action maps: `Player` and `UI`.
- `[RULE-INP-003]`: **Analog Deadzone**: Gamepad analog stick deadzone MUST equal $0.15 \pm 0.0$.

### 8. ALGORITHMS

#### Algorithm 8.1: Input Action Maps Configuration Schema

```yaml
input_asset: "Assets/Settings/EchoesInputActions.inputactions"
action_maps:
  - name: "Player"
    actions:
      Move:
        type: Value
        control_type: Vector2
        bindings:
          - path: "<Keyboard>/w"  composite: WASD Up
          - path: "<Keyboard>/a"  composite: WASD Left
          - path: "<Keyboard>/s"  composite: WASD Down
          - path: "<Keyboard>/d"  composite: WASD Right
          - path: "<Gamepad>/leftStick"
      Jump:
        type: Button
        bindings: ["<Keyboard>/space", "<Gamepad>/buttonSouth"]
      Interact:
        type: Button
        bindings: ["<Keyboard>/e", "<Gamepad>/buttonWest"]
      RecordEcho:
        type: Button
        bindings: ["<Keyboard>/r", "<Gamepad>/leftShoulder"]
      SoftReset:
        type: Button
        bindings: ["<Keyboard>/q", "<Gamepad>/select"]
  - name: "UI"
    actions:
      Navigate:
        type: Value
        bindings: ["<Keyboard>/arrowKeys", "<Gamepad>/dpad", "<Gamepad>/leftStick"]
      Submit:
        type: Button
        bindings: ["<Keyboard>/enter", "<Gamepad>/buttonSouth"]
      Cancel:
        type: Button
        bindings: ["<Keyboard>/escape", "<Gamepad>/buttonEast"]
gamepad_deadzone: 0.15    # zona muerta de stick analógico
```

### 9. CONSTRAINTS
- `[CONS-INP-001]`: Prohibido hardcoding legacy `Input.GetKey()` or `Input.GetAxis()` calls in C# scripts.

### 10. VALIDATION
- `[VAL-INP-001]`: `LevelValidator.cs` parses `EchoesInputActions.inputactions` and verifies binding paths.

### 11. EXAMPLES
- Input schema above.

### 12. FAILURE CASES
- `[FAIL-INP-001]`: **Unbound Action**: `RecordEcho` action missing gamepad binding. Result: `FAIL-INP-01`.

### 13. CROSS REFERENCES
- [LOCOMOTION_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LOCOMOTION_SPEC.md) `[SPEC-113]`

### 14. CHANGE HISTORY
- **v4.0 (2026-07-25)**: Created canonical SPEC-118 defining input action maps, bindings, and analog deadzone rules.
