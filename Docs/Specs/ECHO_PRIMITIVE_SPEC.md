# ECHO_PRIMITIVE_SPEC.md — Echo Mechanics Primitives & Hazard Specifications
## Spec ID: SPEC-111
## Version: 3.0 (AI-Executable)

---

### 1. PURPOSE
Specifies the technical architecture, execution parameters, and collision behaviors for advanced Echo primitives in *Echoes of You 2.0* (`EchoKineticBody.cs`, `EchoShieldField.cs`, `EchoConflictTrap.cs`, `EchoDisintegrationZone.cs`).

### 2. SCOPE
Applies to specialized Echo interaction components in `Assets/Scripts/Echo/`. Excludes standard `EchoRecorder.cs` frame logging.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ECHO_GRAMMAR.md` (`SPEC-107`).

### 4. DEFINITIONS
- `EchoKineticBody`: Component allowing an Echo to apply physical impulse to `KineticPushableBlock` ($Mass=50\text{kg}$).
- `EchoShieldField`: Volume protecting the Echo or Player from disintegration hazards ($Radius=3.0\text{m}$).
- `EchoConflictTrap`: Hazard zone triggering scene `SoftReset` if an Echo and Player occupy the same volume simultaneously ($Radius=1.5\text{m}$).
- `EchoDisintegrationZone`: Hazard field instantly dissolving active Echo playback upon contact.

### 5. INPUTS
- [ECHO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_GRAMMAR.md) `[SPEC-107]`
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`

### 6. OUTPUTS
- Runtime trigger interactions driven by `EchoDisintegrationZone.cs` and `EchoConflictTrap.cs`.
- Hazard assertions for `LevelValidator.cs`.

### 7. RULES

- `[RULE-PRM-001]`: **Disintegration Zone Execution**: Contact between an Echo CapsuleCollider and an `EchoDisintegrationZone` trigger MUST immediately dissolve the Echo, set `playbackState = Destroyed`, and release the Echo slot within $0.05\text{s}$.
- `[RULE-PRM-002]`: **Conflict Trap Triggering**: If Player and Echo CapsuleColliders occupy an `EchoConflictTrap` trigger simultaneously for $> 0.1\text{s}$, the scene MUST invoke `LevelRuntimeController.SoftReset()`.
- `[RULE-PRM-003]`: **Shield Field Protection**: An Echo operating inside an `EchoShieldField` radius ($R=3.0\text{m}$) MUST be immune to `EchoDisintegrationZone` dissolve effects.
- `[RULE-PRM-004]`: **Kinetic Push Force**: `EchoKineticBody` MUST apply a constant force $F = 500\text{ N}$ to `KineticPushableBlock` objects during playback.

### 8. ALGORITHMS

#### Table 8.1: Advanced Echo Primitives Catalog

| Component Name | Trigger Type | Radius / Dimensions | Activation Condition | Execution Result |
|---|---|---|---|---|
| `EchoKineticBody.cs` | Box Collision | `[1.0, 1.8, 1.0]` | Recorded push input | Applies $F=500\text{N}$ impulse to pushblock |
| `EchoShieldField.cs` | Sphere Trigger | $R = 3.0\text{ m}$ | Active Echo inside volume | Negates disintegration hazard |
| `EchoConflictTrap.cs` | Box Trigger | `[3.0, 3.0, 3.0]` | Player + Echo co-presence | Triggers scene `SoftReset()` |
| `EchoDisintegrationZone.cs`| Box Trigger | `[4.0, 0.1, 4.0]` | Echo contact | Instant Echo dissolution |

### 9. CONSTRAINTS
- `[CONS-PRM-001]`: Prohibido creating `EchoConflictTrap` zones without clear red pulse visual indicators (`#FF0000`).
- `[CONS-PRM-002]`: Prohibido applying kinetic push force to static architecture walls.

### 10. VALIDATION
- `[VAL-PRM-001]`: `LevelValidator.cs` parses scene triggers and asserts all `EchoConflictTrap` components have valid `SoftReset` event listeners.
- `[VAL-PRM-002]`: Disintegration integration test asserts Echo dissolves in $\le 0.05\text{s}$ upon contact.

### 11. EXAMPLES

#### Example 11.1: EchoDisintegrationZone Collision Routine in C#
```csharp
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Echo"))
    {
        EchoPlayback playback = other.GetComponent<EchoPlayback>();
        if (playback != null && !playback.isShielded)
        {
            playback.DissolveEcho();
        }
    }
}
```

### 12. FAILURE CASES
- `[FAIL-PRM-001]`: **Unshielded Dissolve Failure**: Echo fails to dissolve inside hazard zone. Result: `LevelValidator` flags `FAIL-PRM-01`.

### 13. CROSS REFERENCES
- [ECHO_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/ECHO_GRAMMAR.md) `[SPEC-107]`
- [PUZZLE_GRAMMAR.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/PUZZLE_GRAMMAR.md) `[SPEC-104]`

### 14. CHANGE HISTORY
- **v3.0 (2026-07-25)**: Initial 14-section AI-Executable canonical spec creation for advanced Echo primitives.
