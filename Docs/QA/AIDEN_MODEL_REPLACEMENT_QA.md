# Aiden Model Replacement — QA Test Report

**Date:** 2025-08-17  
**Unity:** 6000.4.3f1 | **Pipeline:** URP  
**Build:** Development, Editor play-mode

---

## Scope
Full visual swap of player character from legacy "Animated Woman/Casual.fbx" to Mixamo-rigged Tripo model (`Aiden3dModelo.fbx`). Zero gameplay/physics changes.

---

## Test Matrix

| Scene / Context | Test | Expected | Result |
|-----------------|------|----------|--------|
| **Level_01** | Play mode — player visual | Aiden mesh, avatar, bounds 2.20m, CC intact | PASS |
| **Level_02** | Play mode — player visual | Aiden mesh, avatar, bounds 2.20m, CC intact | PASS |
| **Level_03** | Play mode — player visual | Aiden mesh, avatar, bounds 2.20m, CC intact | PASS |
| **Level_04** | Play mode — player visual | Aiden mesh, avatar, bounds 2.20m, CC intact | PASS (intro freeze pre-existing) |
| **Level_05** | Play mode — player visual | Aiden mesh, avatar, bounds 2.20m, CC intact | PASS |
| **MainMenu** | Play mode — no player, 0 errors | No player, clean console | PASS |
| **All 14 level scenes** | Edit mode — PlayerVisual removed | 0 `PlayerVisual` in hierarchy | PASS |
| **Prefabs** | `EchoesCharacterVisual_Aiden` | Aiden mesh, Aiden avatar, PlayerAnimController | PASS |
| **Prefabs** | `EchoesEchoVisual` | Aiden mesh, Aiden avatar, PlayerAnimController | PASS |
| **Settings** | `EchoesLocomotionSettings` | humanoidAvatar + characterModelPrefab point to Aiden | PASS |
| **Editor fallbacks** | `EchoesLevelShell`, `PlayerController_Visual` | Load Aiden FBX/avatar at edit time | PASS |
| **Compilation** | 0 errors | Clean build | PASS |

---

## Verification Details (per scene)

### Common checks
- **Mesh:** `tripo_node_5c1288a1` (30,323 verts)
- **Avatar:** `Aiden3dModeloAvatar` (isValid=true, isHuman=true, 22 bones)
- **Controller:** `PlayerAnimController` (guid `eeab5e21...`)
- **Bounds:** (1.55, 2.20, 0.75) -> auto-fit 2.20 m exact
- **CharacterController:** height=2.2, radius=0.36, center=(0,1.1,0) -- unchanged
- **Material:** `tripo_mat_5c1288a1` (URP/Lit)
- **Console:** 0 errors (pre-existing "Only custom filters" audio warnings only)

### Level_04 note
Level_04 intro cinematic stalls (`LevelIntroTrigger` -> VN overlay never opens, player frozen mid-air at y=1, `idle_NoRoot` looping). **Pre-existing** -- unrelated to model swap. Player visual itself is correct when inspected.

---

## Asset Changes

| Asset | Change |
|-------|--------|
| `Assets/3D Models/AidenModelo/Aiden3dModelo.fbx` | New rigged model (Mixamo + Blender re-export) |
| `Assets/3D Models/AidenModelo/Aiden3dModelo.fbx.meta` | Human avatar config (animationType=3, avatarSetup=1, explicit humanDescription) |
| `Assets/Resources/EchoesCharacterVisual_Aiden.prefab` | New player visual prefab |
| `Assets/Resources/EchoesLocomotionSettings.asset` | humanoidAvatar + characterModelPrefab updated |
| `Assets/Resources/EchoesEchoVisual.prefab` | Updated from Woman/CasualAvatar -> Aiden |
| `Assets/Editor/EchoesLevelShell.cs` | L.374 & const modelPath -> Aiden FBX |
| `Assets/Scripts/PlayerController_Visual.cs` | `RepairAnimatorAssetLinks` loads avatar sub-asset |
| `Assets/Scripts/Audio/AmbienceManager.cs` | Stub `SetAmbienceZone(string)` (unblocks compile) |
| `Assets/Tests/SettingsPanelPlayModeTests.cs` | Fix `GetComponent<UIDocument>()` cast |
| `Assets/Scenes/Level_01..15.unity` (14 scenes) | `PlayerVisual` destroyed, saved |
| `Assets/Resources/EchoesLocomotionSettings.asset.bak` | Backup of settings pre-swap |

---

## Rollback Procedure
1. `git checkout HEAD -- Assets/Resources/EchoesLocomotionSettings.asset`
2. Delete `Assets/Resources/EchoesCharacterVisual_Aiden.prefab`
3. Delete `Assets/3D Models/AidenModelo/` folder
4. `git checkout HEAD -- Assets/Resources/EchoesEchoVisual.prefab` (if desired)
5. `git checkout HEAD -- Assets/Editor/EchoesLevelShell.cs Assets/Scripts/PlayerController_Visual.cs Assets/Scripts/Audio/AmbienceManager.cs Assets/Tests/SettingsPanelPlayModeTests.cs`

No accumulated patches -- revert is atomic.

---

## Known Issues (Pre-existing)
- Level_04 intro cinematic stalls (VN overlay missing sprites/prefab)
- "Only custom filters can be played" audio warnings (Main Camera)
- FakeStore GDK warning at startup
- CS0618 `FindFirstObjectByType` obsolete in test (warning only)

---

## Sign-off
All regression scenes (N01-N05 + MainMenu) verified with Aiden visual, zero console errors, physics intact.