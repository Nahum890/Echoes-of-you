# CAMERA_RECIPES.md — Cinemachine Technical Recipes & Profiles
## Spec ID: SPEC-007
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Specifies deterministic technical recipes for Cinemachine virtual camera profiles in Unity URP via `CameraProfile.cs` and `CameraProfileApplier.cs`. Defines exact numerical values for FOV, Follow Offset, Damping, Pitch, Yaw, Blend Duration, Noise Gain, and TargetGroup Weights for all 11 `CameraProfileType` values.

### 2. SCOPE
Applies to `CameraProfile.cs` assets, `CameraProfileApplier.cs`, `CinemachineVirtualCamera` components, and `EventCameraDirector.cs`. Excludes post-processing color grading.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `CAMERA_GRAMMAR.md` (`SPEC-108`).

### 4. DEFINITIONS
- `CameraProfileType`: C# enum representing camera situations (`Learning`, `Discovery`, `Puzzle`, `Transition`, `Emotional`, `Suspense`, `Memory`, `Replay`, `Acceptance`, `LeapOfFaith`, `Inversion`).
- `Follow Offset`: Vector3 camera offset relative to target `[X, Y, Z]`.
- `TargetGroup Weight`: Weight value assigned to targets inside Cinemachine `TargetGroup` ($W_{player}, W_{echo}$).

### 5. INPUTS
- [CAMERA_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Art/CAMERA_GRAMMAR.md) `[SPEC-108]`
- [ANTI_PATTERNS.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ANTI_PATTERNS.md) `[SPEC-002]`

### 6. OUTPUTS
- Instantiated `CameraProfile.cs` ScriptableObjects in `Assets/Settings/CameraProfiles/`.
- Runtime camera transitions driven by `CameraProfileApplier.cs`.

### 7. RULES

- `[RULE-CAM-001]`: **Profile Recipe Compliance**: Every `CinemachineVirtualCamera` MUST match the numerical parameters of its assigned `CameraProfileType` from Table 8.1.
- `[RULE-CAM-002]`: **Blend Duration Enforcement**: Transitions between camera profiles MUST match the specified `blendDuration` value ($\pm 0.0\text{s}$). Instant cuts ($0.0\text{s}$) are prohibited except on player death/reset.
- `[RULE-CAM-003]`: **TargetGroup Weight Balance**: In `Puzzle` profile recipes, TargetGroup weights MUST equal $W_{player} = 1.0$ and $W_{echo} = 1.0$ with framing radius $R \ge 2.0\text{m}$.
- `[RULE-CAM-004]`: **Roll & Dutch Restrictions**: Roll and Dutch parameters MUST equal $0.0^\circ$, except in `Inversion` ($180.0^\circ$) or `Suspense` ($5.0^\circ$).

### 8. ALGORITHMS

#### Table 8.1: Complete Technical Cinemachine Recipes (11 CameraProfileType Enum Values)

| Profile Type (`CameraProfileType`) | Field of View ($FOV$) | Follow Offset $[X, Y, Z]$ | Damping $[X, Y, Z]$ | Pitch / Yaw | Blend Duration | Noise Gain | TargetGroup Weights | LookAt DeadZone $[W,H]$ | LookAt SoftZone $[W,H]$ | Collider Strategy | Transposer Mode |
|---|---|---|---|---|---|---|---|---|---|---|---|
| **Learning** | $50.0^\circ$ | `[0.0, 3.0, -6.0]` | `[1.2, 1.2, 1.2]` | $12.0^\circ / 0.0^\circ$ | $1.5\text{ s}$ | $0.1$ | Player: 1.0 | `[0.1, 0.1]` | `[0.8, 0.8]` | `PullCameraForward` | `WorldSpace` |
| **Discovery** | $45.0^\circ$ | `[0.0, 3.2, -6.5]` | `[1.0, 1.0, 1.0]` | $15.0^\circ / 0.0^\circ$ | $1.5\text{ s}$ | $0.2$ | Player: 1.0 | `[0.1, 0.1]` | `[0.8, 0.8]` | `PullCameraForward` | `WorldSpace` |
| **Puzzle** | $45.0^\circ$ | `[0.0, 6.0, -9.0]` | `[0.8, 0.8, 0.8]` | $25.0^\circ / 0.0^\circ$ | $2.0\text{ s}$ | $0.0$ | Player: 1.0, Echo: 1.0 | `[0.1, 0.1]` | `[0.8, 0.8]` | `PullCameraForward` | `WorldSpace` |
| **Transition** | $48.0^\circ$ | `[0.0, 3.0, -6.0]` | `[1.4, 1.4, 1.4]` | $14.0^\circ / 0.0^\circ$ | $1.5\text{ s}$ | $0.0$ | Player: 1.0 | `[0.1, 0.1]` | `[0.8, 0.8]` | `PullCameraForward` | `WorldSpace` |
| **Emotional** | $35.0^\circ$ | `[0.0, 2.0, -4.0]` | `[1.5, 1.5, 1.5]` | $10.0^\circ / 0.0^\circ$ | $3.0\text{ s}$ | $0.0$ | Player: 1.0, Prop: 1.0 | `[0.1, 0.1]` | `[0.8, 0.8]` | `PullCameraForward` | `WorldSpace` |
| **Suspense** | $45.0^\circ$ | `[0.0, 3.0, -6.0]` | `[1.0, 1.0, 1.0]` | $15.0^\circ / 0.0^\circ$ | $1.0\text{ s}$ | $0.3$ | Player: 1.0 (Dutch=$5^\circ$) | `[0.1, 0.1]` | `[0.8, 0.8]` | `PullCameraForward` | `WorldSpace` |
| **Memory** | $37.0^\circ$ | `[0.0, 2.2, -4.5]` | `[1.1, 1.1, 1.1]` | $10.0^\circ / 0.0^\circ$ | $2.0\text{ s}$ | $0.15$| Player: 1.0, Echo: 1.0 | `[0.1, 0.1]` | `[0.8, 0.8]` | `PullCameraForward` | `WorldSpace` |
| **Replay** | $45.0^\circ$ | `[0.0, 4.0, -7.5]` | `[0.9, 0.9, 0.9]` | $18.0^\circ / 0.0^\circ$ | $1.5\text{ s}$ | $0.0$ | Player: 1.0, Echo: 1.0 | `[0.1, 0.1]` | `[0.8, 0.8]` | `PullCameraForward` | `WorldSpace` |
| **Acceptance** | $52.0^\circ$ | `[0.0, 3.5, -7.0]` | `[1.6, 1.6, 1.6]` | $15.0^\circ / 0.0^\circ$ | $2.5\text{ s}$ | $0.0$ | Player: 1.0 | `[0.1, 0.1]` | `[0.8, 0.8]` | `PullCameraForward` | `WorldSpace` |
| **LeapOfFaith** | $55.0^\circ$ | `[0.0, 1.5, -4.5]` | `[0.5, 0.5, 0.5]` | $8.0^\circ / 0.0^\circ$ | $0.8\text{ s}$ | $0.5$ | Player: 1.0 | `[0.1, 0.1]` | `[0.8, 0.8]` | `PullCameraForward` | `WorldSpace` | *(Capped at 55° per [CONS-CAM-001])*
| **Inversion** | $45.0^\circ$ | `[0.0, -3.2, -6.5]` | `[1.0, 1.0, 1.0]` | $-15.0^\circ / 180^\circ$| $2.5\text{ s}$ | $0.3$ | Player: 1.0 (Roll=$180^\circ$) | `[0.1, 0.1]` | `[0.8, 0.8]` | `PullCameraForward` | `WorldSpace` |

#### Algorithm 8.2: Cinemachine Collider Global Settings (HALT-6)

```yaml
cinemachine_collider_global_settings:
  strategy: "PullCameraForward"
  collision_filter: "Default,Environment,Architecture"  # LayerMask
  collision_filter_bitmask: 0x00018001  # Default(0) | Environment(15) | Architecture(16) — from COLLISION_BITMASK_MATRIX.yaml CameraCollider row
  ignore_tag: "Player"
  minimum_distance_from_target_m: 1.5
  damping_into_collider: 0.0     # instantáneo cuando obstaculizado
  damping_from_collider: 0.5     # suave al salir
  smoothing_time: 0.2
  # Cinemachine CinemachineCollider.m_OptimalTargetDistance
  optimal_target_distance_m: 2.0
  # Cross-ref: CameraCollider is Layer 19 in COLLISION_BITMASK_MATRIX.yaml
  # This mask matches row "CameraCollider: {layer_index: 19, collides_with_mask: 0x00018001}"
```

### 9. CONSTRAINTS
- `[CONS-CAM-001]`: Prohibido $FOV < 30.0^\circ$ or $FOV > 60.0^\circ$.
- `[CONS-CAM-002]`: Prohibido applying `noiseGain > 0.8` on any camera profile.

### 10. VALIDATION
- `[VAL-CAM-001]`: `CameraProfileApplier.cs` asserts loaded runtime parameters match Table 8.1 values.
- `[VAL-CAM-002]`: Inspector asserts exactly 1 virtual camera is set to active priority at any timestamp.

### 11. EXAMPLES

#### Example 11.1: CameraProfile ScriptableObject Initialization in C#
```csharp
CameraProfile profile = ScriptableObject.CreateInstance<CameraProfile>();
profile.profileType = CameraProfileType.Puzzle;
profile.fieldOfView = 45.0f; // CONT-001 resolved
profile.blendDuration = 2.0f;
profile.followOffset = new Vector3(0.0f, 6.0f, -9.0f);
```

### 12. FAILURE CASES
- `[FAIL-CAM-001]`: **Instant Cut Violation**: Transition blend set to 0.0s. Result: `LevelValidator` flags `FAIL-CAM-01`.
- `[FAIL-CAM-002]`: **Unbalanced TargetGroup**: Echo weight set to 0.0 during puzzle. Result: `FAIL-CAM-02`.

### 13. CROSS REFERENCES
- [CAMERA_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Art/CAMERA_GRAMMAR.md) `[SPEC-108]`
- [ANTI_PATTERNS.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ANTI_PATTERNS.md) `[SPEC-002]`

### 14. CHANGE HISTORY
- **v1.0 (2025-06-15)**: Camera recipes draft.
- **v3.0 (2026-07-25)**: Full refactor into 14-section AI-Executable canonical format. Expanded to 11 camera profiles and resolved CONT-001 (Puzzle FOV = 45.0°).
- **v4.0 (2026-07-25)**: HALT-6 resolved — added Cinemachine Collider global settings (Algorithm 8.2) and expanded Table 8.1 with LookAt DeadZone/SoftZone, Collider strategy, and Transposer mode.
- **v5.0 (2026-07-25)**: Stoppage 8 resolved — Added `collision_filter_bitmask: 0x00018001` cross-referencing COLLISION_BITMASK_MATRIX.yaml CameraCollider row (Layer 19). Architecture confirmed as Layer 16.
