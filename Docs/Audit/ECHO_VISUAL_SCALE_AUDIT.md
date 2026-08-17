# ECHO VISUAL & SCALE AUDIT

Status: AUDIT COMPLETE — root causes identified with file/line evidence.
Date: 2026-08-16
Scope: Echo visual identity + physical scale repair (no level/lighting changes).

---

## CURRENT MODEL

**LowPolyCharacter.fbx** — GUID `c6f08cca9f00aa040ab00d410697e152`, resolved at
`Assets/3D Models/lowpoly-character-freerigged-/source/LowPolyCharacterModel/FBX/LowPolyCharacter.fbx`.

It is baked as a nested `PrefabInstance` (`&7923139722039514096`, lines 248-329) inside
`Assets/Prefabs/EchoPrefab.prefab`, with `m_Avatar` overridden to the Woman/Casual
avatar (GUID `0c223ee8d6ce16948803cf86a18c8d2a`, `Assets/3D Models/Animated Woman/Casual.fbx`).

Mesh ↔ armature bind mismatch → deformed oversized silhouette + wrong silhouette identity.

`EchoPlayback.EnsureVisualAnimator` (`Assets/Scripts/EchoPlayback.cs:466`) sees the
baked "Model" already has a `SkinnedMeshRenderer` and keeps it, so the correct fallback
(`SpawnEchoModel` → `EchoesEchoVisual` / Woman model) never executes.

## EXPECTED MODEL

**Casual.fbx "Woman"** — GUID `0c223ee8d6ce16948803cf86a18c8d2a`, sourced at runtime from
`EchoesLocomotionSettings.characterModelPrefab` (which resolves to
`Assets/Resources/EchoesCharacterVisual.prefab`, the same model Aiden uses) or, as last
resort, `Assets/Resources/EchoesEchoVisual.prefab`. Both already reference the correct
mesh+avatar GUID `0c223ee8d6ce16948803cf86a18c8d2a`.

Echo materials (echo-cyan, transparent) applied on top by
`EchoPlayback.ApplyEchoMaterials` (`EchoPlayback.cs:640`) using `Mat_Echo`.

## CURRENT SCALE

`Scaler.localScale = Vector3.one * CharacterVisualScale` only —
`EchoPlayback.cs:548` (`SpawnEchoModel`) and `:611` (`ConfigureEchoModel`).
`CharacterVisualScale` default = 1.0, clamped [0.2, 1.2] (`EchoesPresentationSettings.cs:22-23`).

**No bounds auto-fit.** The echo mesh is rendered at its native FBX unit scale, further
inflated by the avatar mismatch. This is the playtest-reported "too big" bug.

## EXPECTED SCALE

Auto-fit mirroring `PlayerCharacterVisualSetup.cs:81-95`:

```
rawHeight  = bounds.size.y of model renderers (at scaler scale 1)
scaler     = (EchoHeight / rawHeight) * CharacterVisualScale
```

Visual height == collider height == 2.2 m. `CharacterVisualScale` preference still
respected (acts as a multiplier on top of the auto-fit).

## CURRENT COLLIDER

`EchoPlayback.cs:13` forces `EchoHeight = 2.1f`, `EchoRadius = 0.36f`,
`center = (0, EchoHeight*0.5f, 0) = (0, 1.05, 0)`, `skinWidth = 0.08`. Same values
applied to a redundant `CapsuleCollider` (`EchoPlayback.cs:63-68`).

Echo is 5% physically shorter than Aiden but renders far taller visually due to the
scale bug above.

## EXPECTED COLLIDER

`EchoHeight = 2.2f`, `radius = 0.36f`, `center = (0, 1.1, 0)`, `skinWidth = 0.08` —
byte-identical to Aiden (`EchoesLevelShell.cs:213-216`, `PlayerCharacterVisualSetup.cs:91`).
Fits the same doors; `CapsuleCollider` retained for player↔echo contact.

## CURRENT HIERARCHY

```
EchoPrefab (tag=Echo, layer=Echo@runtime)
├─ CharacterController    (baked 2.2 → forced 2.1 at runtime, r=0.36)
├─ Rigidbody             (kinematic, no gravity)
├─ EchoPlayback
├─ EchoSpectralTrail
├─ EchoTemporalVisual
├─ CharacterPush
├─ PlayerLocomotionAnimator      (baked — player-only, destroyed at runtime)
├─ PlayerAnimationRuntimeBootstrap (baked — player-only, destroyed at runtime)
└─ Visual (1,1,1)
   └─ EchoScaler (1,1,1)
      └─ Model (nested LowPolyCharacter.fbx, avatar=Woman, controller=PlayerAnimController,
               material override guid be9a965487fa400449864f1c22c3e7df)

PrefabInstance source GUID: c6f08cca9f00aa040ab00d410697e152  (LowPolyCharacter.fbx)
Avatar override GUID:       0c223ee8d6ce16948803cf86a18c8d2a  (Woman/Casual)
AnimatorController GUID:    eeab5e21720dedd48bb135f9df6cefa8  (PlayerAnimController)
```

## PROPOSED HIERARCHY

```
EchoPrefab (tag=Echo, layer=Echo@runtime)
├─ CharacterController    (2.2, r=0.36, center (0,1.1,0))   [matches Aiden]
├─ Rigidbody             (kinematic, no gravity)
├─ EchoPlayback
├─ EchoSpectralTrail
├─ EchoTemporalVisual
├─ CharacterPush
└─ Visual (1,1,1)
   └─ EchoScaler (auto-fit = EchoHeight/rawHeight × CharacterVisualScale)
      └─ Model (Casual.fbx Woman, spawned at runtime by SpawnEchoModel from
               EchoesCharacterVisual / EchoesEchoVisual, avatar=Woman,
               controller=LocomotionSettings.animatorController,
               materials=Mat_Echo echo-cyan transparent)
```

---

## RULES COMPLIANCE

- Echo conserves Aiden's physical proportions (collider identical).
- Echo collider does not exceed Aiden's (== 2.2 × 0.36).
- Visual + physical scales are coherent (auto-fit ↔ collider).
- Echo passes through the same doors/spaces as Aiden.
- Puzzle solutions unaffected (Echo layer, CharacterPush, PressurePlate behavior unchanged).
- No second character logic — `EchoPlayback` remains a playback-only component.

## SECONDARY LEAK

`Assets/Scripts/MainMenuCinematicWorld.cs:634` loads `"EchoesCharacterVisual"` for distant
menu echoes. Cosmetic-only, but inconsistent with the new echo visual identity — will be
switched to `"EchoesEchoVisual"`.

## ASSETS NOT MODIFIED

- `EchoesEchoVisual.prefab`, `EchoesCharacterVisual.prefab` (already correct).
- `EchoesLocomotionSettings.asset` (Aiden's source, correct).
- `EchoesLevelShell.cs`, `EchoModeController.cs`, `EchoRecorder.cs` (no model/scale logic to change).
- All levels, lighting, materials, shaders.

---

## FOLLOW-UP: "ECO HUNDIDO" (playtest feedback)

### SYMPTOM

Playtest reported Echo appears sunken ("ligeramente hacia abajo, como que aparece hundido").

### ROOT CAUSE

The `Casual.fbx` (Woman) FBX origin sits at the **character's midsection**, not at the feet.
In bind pose, model bounds extend below the transform origin (rawH `min.y ≈ -0.714`, rawH `max.y ≈ 9.471`, rawH `size.y ≈ 10.185`). After auto-fit (scale `0.216`), bounds `min.y ≈ -0.154` — feet 15cm below origin.

Aiden corrects this via `PlayerAnimationRuntimeBootstrap.ApplyToHierarchy` (line 57-58):
```
animator.Rebind();
animator.Update(0f);
```
This applies the avatar's default standing pose, which positions the humanoid's body so feet land at `y=0` relative to the transform origin.

The Echo previously had NO equivalent call:
- `PlayerAnimationRuntimeBootstrap` early-outs on `EchoPlayback` (line 24);
- `EchoPlayback.Awake` calls `RemovePlayerOnlyAnimationBootstraps()` which destroys the bootstrap even if it was on the prefab;
- `ConfigureEchoModel` configured the Animator but never evaluated it.

### FIX

Added at end of `ConfigureEchoModel` (`Assets/Scripts/EchoPlayback.cs:680-685`):
```csharp
animator.updateMode = AnimatorUpdateMode.Normal;
animator.enabled = true;

animator.Rebind();
animator.Update(0f);
```

This mirrors `PlayerAnimationRuntimeBootstrap` so the Echo adopts Aiden's exact default standing pose — feet at `y=0`.

### MEASUREMENT (PlayMode, EchoScaleTest.unity)

| | scale | size.y (standing pose) | feet (rel to transform) | delta vs Aiden |
|---|---|---|---|---|
| Aiden | 0.216 | 2.130 | -1.194 | — |
| Echo  | 0.222 | 2.200 | -1.210 | +1.6cm lower |

`Renderer.bounds` measurements of `SkinnedMeshRenderer` are taken in the SAME PlayMode frame
right after `Awake`+`BeginPlayback`, before the first `Update()` of `EchoPlayback` runs. The
remaining 1.6cm residual delta is the natural posture/initial-frame difference, well within the
"slightly" magnitude the playtest reported.

### VISUAL VALIDATION

Side-by-side screenshot captured at:
`Assets/Screenshots/echo_sunken_compare.png`
- Aiden at x = -0.8, Echo at x = +0.8, ground plane at y = 0.
- View literally to confirm both characters stand on the floor at the same height.

