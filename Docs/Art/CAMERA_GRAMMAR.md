# CAMERA_GRAMMAR.md — Cinematic Framing & Visual Intent Specifications
## Spec ID: SPEC-108
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Specifies visual framing rules, cinematic focus priority, camera weighting logic, and composition constraints for *Echoes of You 2.0*. Directs visual attention toward objectives, the Echo, and puzzle components without explicit screen pointers.

### 2. SCOPE
Applies to `CinemachineVirtualCamera`, `CinemachineTargetGroup`, `EventCameraDirector.cs`, and `CinemachineEventFocus.cs`. Excludes URP light intensities.

### 3. AUTHORITY
Nivel 2 (Dirección de Arte y Cámara). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `DESIGN_PHILOSOPHY.md` (`SPEC-001`).

### 4. DEFINITIONS
- `Focus Priority`: Ordinal priority ranking determining camera framing focus.
- `CinemachineTargetGroup`: Component balancing framing weights between Player, Echo, and Objectives.
- `Dutch Angle`: Camera roll tilt angle ($0.0^\circ$ standard).

### 5. INPUTS
- [DESIGN_PHILOSOPHY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/DESIGN_PHILOSOPHY.md) `[SPEC-001]`
- [CAMERA_RECIPES.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CAMERA_RECIPES.md) `[SPEC-007]`

### 6. OUTPUTS
- Configured `CinemachineVirtualCamera` components in gameplay scenes.
- Focus assertions for `LevelValidator.cs`.

### 7. RULES

- `[RULE-CAM-001]`: **Focus Priority Order**: Camera target priority MUST strictly follow the ordinal ranking:
  1. *Level Goal (`LevelGoal`)*
  2. *Echo (`EchoPlayback`)*
  3. *Player (`PlayerController`)*
  4. *Door (`DoorController`)* / *Pressure Plate (`PressurePlate`)*
- `[RULE-CAM-002]`: **Blend Duration Bounds**: All camera transitions MUST use `CinemachineBlend` with duration $0.5\text{s} \le T_{blend} \le 3.0\text{s}$. Instant cuts ($0.0\text{s}$) are prohibited except on player death/reset.
- `[RULE-CAM-003]`: **Dutch Angle Restriction**: `Dutch = 0.0^\circ` in 100% of standard situations. Allowed only `Dutch = 5.0^\circ` in `Suspense` and `Dutch = 180.0^\circ` in `Inversion`.
- `[RULE-CAM-004]`: **Occlusion Prevention**: Every virtual camera in enclosed spaces MUST include a `CinemachineCollider` component with `Min Distance From Target = 0.5m`.

### 8. ALGORITHMS

#### Table 8.1: Visual Intent Camera Matrix

| Profile Intent | FOV Range | Blend Duration | Noise Gain | Dutch Angle | TargetGroup Weights |
|---|---|---|---|---|---|
| **Standard Exploration** | $50.0^\circ$ | $1.5\text{ s}$ | $0.1$ | $0.0^\circ$ | Player: 1.0 |
| **Puzzle Framing** | $45.0^\circ$ | $0.5\text{ s}$ | $0.0$ | $0.0^\circ$ | Player: 1.0, Echo: 1.0, Goal: 1.0 |
| **Narrative Focus** | $38.0^\circ - 42.0^\circ$ | $1.5\text{ s}$ | $0.0$ | $0.0^\circ$ | Player: 1.0, Prop: 1.0 |
| **Open Space** | $55.0^\circ$ | $2.0\text{ s}$ | $0.2$ | $0.0^\circ$ | Player: 1.0 |
| **Suspense Event** | $45.0^\circ$ | $1.0\text{ s}$ | $0.3$ | $5.0^\circ$ | Triggered Object: 1.0 |

### 9. CONSTRAINTS
- `[CONS-CAM-001]`: Prohibido FPS first-person camera perspectives.
- `[CONS-CAM-002]`: Prohibido permanent camera shake during normal walking locomotion.

### 10. VALIDATION
- `[VAL-CAM-001]`: `LevelValidator.cs` verifies zero cameras have `blendDuration == 0.0f` (except death reset).
- `[VAL-CAM-002]`: Inspector asserts CinemachineTargetGroup contains active player and echo references during puzzle playback.

### 11. EXAMPLES

#### Example 11.1: Cinemachine TargetGroup Config in C#
```csharp
CinemachineTargetGroup targetGroup = GetComponent<CinemachineTargetGroup>();
targetGroup.AddMember(playerTransform, 1.0f, 2.0f);
targetGroup.AddMember(echoTransform, 1.0f, 2.0f);
targetGroup.AddMember(goalTransform, 1.0f, 3.0f);
```

### 12. FAILURE CASES
- `[FAIL-CAM-001]`: **Target Lost Failure**: Camera loses line of sight to player for $> 0.3\text{s}$. Result: `LevelValidator` flags `FAIL-CAM-03`.
- `[FAIL-CAM-002]`: **Instant Cut Violation**: Transition blend set to 0.0s. Result: `FAIL-CAM-01`.

### 13. CROSS REFERENCES
- [DESIGN_PHILOSOPHY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/DESIGN_PHILOSOPHY.md) `[SPEC-001]`
- [CAMERA_RECIPES.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/CAMERA_RECIPES.md) `[SPEC-007]`

### 14. CHANGE HISTORY
- **v1.0 (2025-06-10)**: Camera grammar draft.
- **v3.0 (2026-07-25)**: Full refactor into 14-section AI-Executable canonical format.