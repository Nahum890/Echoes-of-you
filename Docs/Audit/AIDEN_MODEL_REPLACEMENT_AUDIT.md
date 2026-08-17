# AIDEN MODEL REPLACEMENT — AUDIT

**Tarea:** Reemplazar el modelo 3D visual de Aiden ("Animated Woman/Casual.fbx") por el nuevo modelo Tripo (`Assets/3D Models/AidenModelo/tripo_convert_8c69ca29-d9ed-4a73-b874-496df0a4c1cd.fbx`) de forma quirúrgica y no destructiva.
**Proyecto:** Echoes of You 2.0 · Unity 6000.4.3f1 · URP 17.4.0 · Cinemachine 3.1.7
**Creado:** 2026-08-17
**Estado:** AUDITORÍA COMPLETADA — pendiente rigging Mixamo del modelo nuevo

---

## 1. CURRENT MODEL

### 1.1 Identidad del modelo actual
- **FBX:** `Assets/3D Models/Animated Woman/Casual.fbx` · guid `0c223ee8d6ce16948803cf86a18c8d2a`
- **Mesh:** `Woman` (SkinnedMeshRenderer, escala 100 en prefab baked, 41 bones, rootBone fileID `8569883378742689548`)
- **Estilo:** "Animated Woman" / Mixamo (bones tipo `RightHandIndex3`, `LeftHandThumb4_end`)
- **AABB mesh:** center (-0.216, -0.451, 0.391) · extent (2.096, 4.893, 1.452) → alto ≈ 4.89m * escala visual

### 1.2 Puntos de configuración (sources of truth)
- **`Assets/Resources/EchoesLocomotionSettings.asset`** · guid `09dff367dfeead44bb8c95332653b2a9`
  - `animatorController` → `Assets/Prefabs/PlayerAnimController.controller` (guid `eeab5e21720dedd48bb135f9df6cefa8`)
  - `humanoidAvatar` → `Assets/3D Models/Animated Woman/Casual.fbx` fileID 9000000 (guid `0c223ee8...`)
  - `characterModelPrefab` → `Assets/Resources/EchoesCharacterVisual.prefab` (guid `600f6d0477dbf784487345955e041dfb`)
- **Prefabs baked del modelo:** `Assets/Resources/EchoesCharacterVisual.prefab` y `Assets/Resources/EchoesEchoVisual.prefab` (byte-idénticos salvo nombre del root). Ambos referencian Casual.fbx (mesh fileID `5830160947526515847`, avatar fileID 9000000).

### 1.3 Jerarquía visual esperada por el código (CONTRATO)
- **Player (runtime):**
  ```
  --- PLAYER --- (root, tag Player, layer 6 Ground)
   └── Player                  (CharacterController h=2.2 r=0.36 center=(0,1.1,0), Rigidbody kinematic)
        ├── PlayerVisual       (PlayerCharacterVisualSetup)
        │    └── PlayerScaler  (escala local; auto-fit a 2.2m)
        │         └── Model    ←nombre EXACTO; si no existe o es "stale" se borra
        ├── GroundCheck        (localPos (0,-0.96,0))
        ├── CameraFocus        (localPos (0, clamp(cc.h*0.68, 1.05, 1.3), 0.08))
        └── PlayerRimLight     (point light)
  ```
- **Echo (runtime, EchoPlayback):**
  ```
  EchoPrefab (root, tag Echo, layer 9 Echo)
   └── Visual                 (VisualChildName="Visual")
        └── EchoScaler        (ScalerChildName="EchoScaler")
             └── Model        (ModelChildName="Model")
  ```
- **Constantes (scripts):** `PlayerCharacterVisualSetup` y `EchoPlayback` definen `ModelChildName="Model"`, `ScalerChildName` (`PlayerScaler`/`EchoScaler`), `VisualChildName` (`PlayerVisual`/`Visual`).

### 1.4 Bootstraps que asignan Animator/Avatar
| Script:Línea | Qué asigna | Condiciones |
|---|---|---|
| `PlayerCharacterVisualSetup.cs:107-109` | `runtimeAnimatorController = settings.animatorController`; `animator.avatar = settings.humanoidAvatar` | solo si `avatar.isValid` |
| `PlayerAnimationRuntimeBootstrap.cs:41-45` | idem (RuntimeInitializeOnLoadMethod, DefaultExecutionOrder(-20)) | omite roots con `EchoPlayback` |
| `PlayerLocomotionAnimator.cs` | bootstrap del animator en player (DefaultExecutionOrder(-10)) | — |
| `EchoPlayback.cs:677-679` | `runtimeAnimatorController = settings.animatorController`; `animator.avatar = settings.humanoidAvatar` | runtime, eco |
| `EchoPlayback.cs:792-794` | `animator.runtimeAnimatorController = playerAnim.runtimeAnimatorController`; `animator.avatar = playerAnim.avatar` | fallback: clonar del player vivo |
| `EchoPlayback.cs:800-802` | editor fallback: `Assets/Prefabs/PlayerAnimController.controller` | — |
| `PlayerController_Visual.cs:66-68` | editor fallback: `runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Prefabs/PlayerAnimController.controller")` | solo si null |
| `PlayerController_Visual.cs:73-75` | **editor fallback (RIESGO):** `animator.avatar = AssetDatabase.LoadAssetAtPath<Avatar>("Assets/3D Models/Animated Woman/Casual.fbx")` | solo si avatar null/inválido/no human |
| `EchoesLevelShell.cs:404-407` | editor build: `runtimeAnimatorController = Assets/Prefabs/PlayerAnimController.controller`; `avatar = LoadAvatarFromCharacterModel()` (Casual.fbx) | solo en BuildProceduralPlayer (editor) |

### 1.5 Comportamiento clave del sistema
- **Auto-fit por bounds, NO por bones:** `PlayerCharacterVisualSetup.cs:81-95` y `EchoPlayback.cs:533-554` escalan `PlayerScaler`/`EchoScaler` para que `bounds.size.y` → 2.2m (`EchoHeight`) multiplicado por `EchoesPresentationSettings.CharacterVisualScale` (PlayerPrefs, clamp 0.2-1.2, default 1).
- **Física = fuente de verdad:** CharacterController (h=2.2, r=0.36, center=(0,1.1,0), stepOffset 0.3, slopeLimit 45, skinWidth 0.08) NO se toca. Lo escala el visual.
- **No hay dependencia de bones en código:** 0 usos de `GetBoneTransform`/`HumanBodyBones`/`Head` en `Assets/Scripts` o `Assets/Editor`. Todo el alineado es por nombre de jerarquía + renderer bounds.
- **Rechazo de modelo stale (echo):** `EchoPlayback.IsStaleModel` (L.519-529) rechaza cualquier mesh cuyo `sharedMesh.name` contenga `"LowPoly"`. → El nuevo mesh NO debe llamarse "LowPoly".
- **`EchoPlayback.SpawnEchoModel` prioridades (L.565-606):**
  - P1: `EchoesLocomotionSettings.characterModelPrefab` ( fuente común con player)
  - P2: `FindLivePlayerModelSource()` (player vivo, path `PlayerVisual/PlayerScaler/Model`)
  - P3: `Resources.Load<GameObject>("EchoesEchoVisual")` (fallback)

### 1.6 Animator Controller existente (NO inventar parámetros)
- **Asset:** `Assets/Prefabs/PlayerAnimController.controller` (guid `eeab5e21...`)
- **Estados:** Locomotion (WalkRunBlend: idle/walk/run blendtree), Falling, Death, JumpIdle, JumpWalk, JumpRun
- **Parámetros (controller):** `Speed`, `VelocityX`, `VelocityZ`, `VerticalSpeed`, `Turn`, `IsGrounded`, `IsRecording`, `IsEchoPlayback`, `Falling`, `Jump`, `JumpStart`, `Death`, `Respawn`, `State`
- **Parámetros escritos por PlayerController (constantes L.12-24):** `Speed`, `IsGrounded`, `IsRecording`, `Grounded` (legacy), `Falling`, `Jump`, `VerticalSpeed`, `Turn`, `HardLanding`, `StartRun`, `StopRun`, `Death`, `Respawn`
- **Clips referenciados (AnimationClip guids):**
  - `b2d32aaab4eba5c48a4a1e2e492369d5` → Idle (idle_NoRoot.anim)
  - `de1bfd409aabc284e879df0737881a99` → Walk (walking_NoRoot.anim)
  - `fe86b49cf02b1e944966b80e5f39f3ea` → Run (running_NoRoot.anim)
  - `d7c1007ef0fad194b8d1d28f6b4fecc4` → Jump (jump_NoRoot.anim)
- **Animaciones origen:** Mixamo (carpeta `Assets/3D Models/Animaciones/Stripped/` con `.anim` retargetados NoRoot). Los clips están como sub-assets `.anim` (no embebidos en FBX). Son humanoid → retargetean a cualquier avatar humanoid válido.

### 1.7 Cámara (NO mover)
- `EchoesLevelShell.SpawnGameplayCamera` (L.252-283): `SimpleFollowCamera` target=player, targetOffset=(0,1.8,0), distance=5.5, pitch=22
- `CameraFocus` (hijo del player, L.243): localPos (0,1.75,0.18). En runtime `PlayerController_Visual.EnsureCameraFocus` (L.91-104) lo reposiciona a `y = clamp(cc.height*0.68, 1.05, 1.3)`, `z = 0.08`.
- Otros focos: `EchoCameraFocus` (`EchoCameraTargetGroupManager`, `FixedPuzzleCameraController`), `CameraEventFocus`, `GoalFocus`. **`CameraTarget` no existe en el proyecto.**

### 1.8 Materiales actuales
- **Player:** `EchoesMaterialLibrary.PlayerMat` (vía `EchoesLevelShell.CreateCapsuleVisual`, editor). En runtime los prefabs baked (`EchoesCharacterVisual.prefab`) ya traen su material.
- **Echo:** `Resources/Mat_Echo.mat` (shader URP guid `933532a4fcc9baf4fa0491de14d08ed7`, `_BaseColor=(0.31,0.76,0.91,0.45)`, `_EmissionColor=(0,0.5,0.65,1)`, `_Surface=1` transparent). Aplicado en `EchoPlayback.ApplyEchoMaterials` (L.705-729) clonando material por slot. Residual: `Shader.Find("Echoes/AnalogGhost")` → `Mat_Echo_Residual`.

---

## 2. NEW MODEL

### 2.1 Identidad
- **FBX original:** `Assets/3D Models/AidenModelo/tripo_convert_8c69ca29-d9ed-4a73-b874-496df0a4c1cd.fbx` · guid `5dba287bfccdffc42b292ea51c7e8891` · 5.87 MB
- **Texturas (.fbm):** `anime_character_3d_model_basecolor.JPEG`, `_metallic.JPEG`, `_normal.JPEG`, `_rm.JPEG`, `_roughness.JPEG` (RGB JPEG)
- **Origen:** Tripo 3D (AI-generated)

### 2.2 Estado de importación actual (Unity ya lo importó)
- **Meta actual del FBX:**
  - `animationType: 2` (Humanoid)
  - `avatarSetup: 0` (Create From This Model)
  - `autoGenerateAvatarMappingIfUnspecified: 1`
  - `globalScale: 1`, `useFileUnits: 1`, `bakeAxisConversion: 0`
  - `materialImportMode: 2` (Import via materialDescription)
  - `human: []` / `skeleton: []` (vacío → auto-mapping)
- **Material generado:** `tripo_mat_8c69ca29` (sub-asset)

### 2.3 HALLAZGO CRÍTICO — El FBX NO ESTÁ RIGGEADO
Inspección vía `execute_code` en Unity:

```
root: tripo_convert_8c69ca29-d9ed-4a73-b874-496df0a4c1cd | childCount=0
root comps: Transform, MeshFilter, MeshRenderer
SMR: False   ← NO SkinnedMeshRenderer
MR: True     ← MeshRenderer estático
MR mesh verts=111947 name=tripo_node_8c69ca29
MR worldCenterBounds=Center:(0.00,0.50,0.00), Extents:(0.42,0.50,0.09)
== all subassets ==
  GameObject | tripo_convert_8c69ca29-...
  Material  | tripo_mat_8c69ca29
  Mesh      | tripo_node_8c69ca29
  Transform | tripo_convert_8c69ca29-...
  MeshRenderer | ...
  MeshFilter   | ...
== all transforms ==
  tripo_convert_8c69ca29-d9ed-4a73-b874-496df0a4c1cd | children=0   ← 1 solo transform, 0 huesos
```

**Implicaciones:**
- NO hay sub-asset Avatar — la importación Humanoid no pudo crear avatar válido.
- NO hay huesos → el Animator Controller (clips Mixamo humanoid) NO puede retargetear.
- MeshRenderer estático → en Play Mode el modelo no se animaría.
- Bounds → alto ≈ 1.0m (0.50+0.50), ancho 0.84m, profundo 0.18m: claramente orientado "de frente" (Z muy delgado indica que la espalda/front están en plano XY).

### 2.4 Plan de rigging (decisión del usuario → Opción A: Mixamo auto-rig)
1. **Usuario sube** `tripo_convert_8c69ca29...fbx` a https://www.mixamo.com → "Upload a character" → auto-rig (T-pose auto) → descargar FBX con skin y rig.
2. **Usuario coloca** el archivo en `Assets/3D Models/AidenModelo/Aiden_rigged.fbx` (FBX for Unity, With Skin, Frames All).
3. **Ingeniería (yo) importa** con rig Humanoid + auto-crear Avatar, valida `avatar.isValid && avatar.isHuman`, verifica mapeo humanoid (Hips→Spine→..., LeftUpLeg/RightUpLeg, etc.).

### 2.5 Material URP (cuando el FBX riggeado esté importado)
- El FBX de Mixamo traerá el material embebido. Convertir a URP/Lit:
  - Albedo: `anime_character_3d_model_basecolor.JPEG`
  - Metallic: `_metallic.JPEG` (canal R)
  - Smoothness: `_roughness.JPEG` (invertida → `_Smoothness`)
  - Normal: `_normal.JPEG`
  - ORM: `_rm.JPEG` (R=Occlusion, G=Roughness, B=Metallic) — aunque ya hay texturas separadas; usar una de las dos.
- **Validación:** ningún material debe usar shader Standard/Built-in. Todos URP.
- La piel y el pelo del modelo Aiden llevarán los mismos mapas.

---

## 3. COMPATIBILITY — PARTIAL (BLOCKED on rigging)

| Item | Estado | Nota |
|---|---|---|
| Animator Controller reutilizable | **PASS** (pending rig) | `PlayerAnimController.controller` funciona con cualquier avatar humanoid válido; clips Mixamo ya retargetean humanoid. |
| Mapeo humanoid (Hips, Spine, LeftArm...) | **BLOCKED** | requiere FBX riggeado por Mixamo. |
| Jerarquía visual (Model/PlayerScaler/PlayerVisual) | **PASS** | se construye en runtime; nuevo prefab debe respetar nombres. |
| Auto-fit a 2.2m via bounds | **PASS** |mecanismo por renderer bounds, no huesos; funciona con cualquier SkinnedMeshRenderer. |
| Echo (SpawnEchoModel) | **PASS** | P1 = `EchoesLocomotionSettings.characterModelPrefab`, mismo prefab que player. |
| Rechazo LowPoly (`IsStaleModel`) | **OK** | el nuevo mesh no debe llamarse "LowPoly". Asegurar nombre del mesh `Aiden` o similar. |
| Materiales URP | **BLOCKED** | requiere FBX riggeado; regenerar mats con shader URP/Lit. |
| Fallbacks editor que hardcodean Casual.fbx | **RIESGO CONTROLADO** | `PlayerController_Visual.cs:73` y `EchoesLevelShell.cs:374,426` solo actúan si avatar null/inválido o en BuildProceduralPlayer. Ver sección 4. |
| Orientación del modelo | **POR VERIFICAR** | el mesh Tripo tiene bounds Z=0.18 (de frente). Verificar T-pose y forward tras rig. Ajustar rotación visual (Armature child) sí, **NO** el root PlayerController. |
| Sustitución del prefab `EchoesCharacterVisual.prefab` | **PASS** | un solo punto: `EchoesLocomotionSettings.characterModelPrefab`. |
| Escenas N01-N05 | **PASS** | ninguna escena (`Assets/Scenes/*.unity`) referencia el modelo por guid. Todo se construye vía `EchoesLevelShell` en runtime. No requiere tocar escenas. |
| Cámaras | **PASS** | `CameraFocus` se reposiciona por `CharacterController.height`. El nuevo modelo no afecta. |
| CharacterController (física) | **PASS — NO TOCAR** | el visual se ajusta a la CC, no al revés. |

### 3.1 Decisión COMPATIBILITY
**PARTIAL → PASS** una vez completado el rig Mixamo + Avatar válido + prefab reemplazo en `EchoesLocomotionSettings`. **No se requiere nuevo Animator Controller ni nuevos parámetros.** Los clips existentes (Mixamo humanoid) retargetean al nuevo avatar.

---

## 4. RIESGOS Y FALLBACKS EDITOR (A REVISAR ANTES DEL SWAP)

| Script:Línea | Riesgo | Mitigación |
|---|---|---|
| `PlayerController_Visual.cs:73-75` | Editor-repair fuerza `avatar = Casual.fbx` si avatar inválido | Asegurar que el nuevo prefab traiga `Animator.avatar` válido desde el start (vía `EchoesLocomotionSettings`). Si quedara null, este fallback lo reemplazaría por el viejo. **Opcional:** actualizar este path al nuevo FBX Aiden_rigged. |
| `EchoesLevelShell.cs:374,426` | `CreateCapsuleVisual` instancia `Casual.fbx` como visual en BuildProceduralPlayer | Solo se ejecuta si `SpawnPlayer` no encuentra `Resources/Prefabs/Player.prefab` (que no existe) → en la pipeline actual SIEMPRE cae aquí en editor. **PERO** el visual final se ajusta en runtime por `PlayerCharacterVisualSetup` desde `EchoesLocomotionSettings`. Esto solo afecta la cápsula de fallback del editor. Se puede dejar o actualizar path al nuevo FBX. |
| `MainMenuCinematicWorld.cs:634` | Usa `Resources.Load("EchoesEchoVisual")` para el menú | El fallback P3 del eco. Si `EchoesEchoVisual.prefab` sigue apuntando a Casual, el menú seguirá mostrando el modelo viejo. **Decisión:** actualizarlo también o dejarlo (menú no es prioritario si la gameplay ya está OK). |

---

## 5. PLAN DE EJECUCIÓN (DEPENDIENTE DEL RIG MIXAMO)

1. **[Usuario] Rig Mixamo:** subir `tripo_convert_8c69ca29...fbx`, descargar FBX riggeado como `Assets/3D Models/AidenModelo/Aiden_rigged.fbx` (FBX for Unity, With Skin, Frames All).
2. **[Backup Git]** checkpoint del estado (los archivos del nuevo modelo son aditivos). Respetar modificaciones preexistentes ajenas (`Assets/Editor/EchoesSliceRebuild.cs`, `Assets/Scenes/Level_02.unity`, `Assets/Scenes/Level_03.unity`, `Assets/Settings/Volumes/Slice_N03_PostProc.asset`) — no son de esta tarea.
3. **[Import]** `Aiden_rigged.fbx` con rig Humanoid + `avatarSetup: 0` (auto). Validar `avatar.isValid && avatar.isHuman`.
4. **[Materiales URP]** crear nuevo material `AidenMat` con shader URP/Litusing las texturas `.fbm` (basecolor/metallic/normal/roughness). Asignar al SkinnedMeshRenderer.
5. **[Prefab]** crear `Assets/Resources/EchoesCharacterVisual_Aiden.prefab`:
   - root con Animator (`avatar` = Aiden_rigged avatar, `runtimeAnimatorController` = PlayerAnimController.controller)
   - child `Armature` (bones), child SkinnedMeshRenderer con mesh Aiden, escala correcta
   - respetar contrato "Model" si la jerarquía externa lo requiere (aunque runtime la regenera)
6. **[Test Animator aislado]** escena temporal: instanciar prefab, asignar controller, disparar parámetros (Speed=1, IsGrounded=true, Jump trigger, IsRecording=true, Death trigger). Verificar estados WalkRunBlend/JumpIdle/JumpWalk/JumpRun/Death se reproducen sin warnings de retarget.
7. **[Swap]** editar `Assets/Resources/EchoesLocomotionSettings.asset`:
   - `characterModelPrefab` → EchoesCharacterVisual_Aiden.prefab
   - `humanoidAvatar` → Avatar de Aiden_rigged.fbx
   - `animatorController` → sin cambios
8. **[Editor fallbacks]** actualizar path en `PlayerController_Visual.cs:73` (Casual.fbx → Aiden_rigged.fbx) y `EchoesLevelShell.cs:374,426` para que el fallback de editor también use Aiden.
9. **[Play Mode]** abrir Level_03, Play. Verificar: idle/walk/run/jump/fall/death, grabación (R), spawn echo (E), interacción con placas/NarrativeTriggers, SimpleFollowCamera sigue al player correctamente.
10. **[Regression]** N01, N02, N03, N04, N05 + Main Menu. Sin errors de console. Sin "missing reference". Sin warnings de Animator ("does not have avatar", "bone not found").
11. **[Docs]** completar `Docs/QA/AIDEN_MODEL_REPLACEMENT_QA.md` con matrix de pruebas.

---

## 6. CHANGE BUDGET (pre-swap, pendiente de actualización post-swap)

| Archivo | Tipo | Cambio | Estado |
|---|---|---|---|
| `Assets/3D Models/AidenModelo/Aiden_rigged.fbx` | ADD | FBX riggeado Mixamo | pendiente |
| `Assets/3D Models/AidenModelo/Aiden_rigged.fbx.meta` | ADD | import settings (rig humanoid, avatar) | pendiente |
| `Assets/Resources/EchoesCharacterVisual_Aiden.prefab` | ADD | prefab visual nuevo | pendiente |
| `Assets/Resources/EchoesCharacterVisual_Aiden.prefab.meta` | ADD | guid | pendiente |
| `Assets/Resources/EchoesLocomotionSettings.asset` | MODIFY | `characterModelPrefab` + `humanoidAvatar` | pendiente |
| `Assets/Scripts/PlayerController_Visual.cs` | MODIFY | fallback editor path Casual → Aiden (L.73) | pendiente (opcional) |
| `Assets/Editor/EchoesLevelShell.cs` | MODIFY | fallback editor path Casual → Aiden (L.374, L.426) | pendiente (opcional) |
| `Assets/Resources/Materials/AidenMat.mat` | ADD | material URP/Lit | pendiente |
| `Docs/Audit/AIDEN_MODEL_REPLACEMENT_AUDIT.md` | ADD | este documento | creado |
| `Docs/QA/AIDEN_MODEL_REPLACEMENT_QA.md` | ADD | matriz QA | pendiente |

**Archivos que NO se tocan:**
- `Assets/Prefabs/PlayerAnimController.controller` (sin cambios; clips Mixamo ya están bien)
- `Assets/Scripts/EchoPlayback.cs`, `EchoRecorder.cs`, `PlayerController*.cs`, `PlayerCharacterVisualSetup.cs`, `PlayerLocomotionAnimator.cs`, `PlayerAnimationRuntimeBootstrap.cs`
- `Assets/Prefabs/EchoPrefab.prefab`
- Cualquier escena (`Assets/Scenes/*.unity`)
- `CharacterController` (física intocable)
- Cualquier script de cámara, puzzle, level, UI, narrativa, save, input

---

## 7. DEFINITION OF DONE

- [ ] Aiden_rigged.fbx importado, avatar `isValid && isHuman`.
- [ ] Prefab `EchoesCharacterVisual_Aiden` con materiales URP y escala correcta.
- [ ] `EchoesLocomotionSettings.asset` apunta al nuevo prefab/avatar.
- [ ] En Play Mode: Aiden se ve, camina, corre, salta, cae, muere.
- [ ] Grabación (R) funciona, eco usa el mismo modelo Aiden.
- [ ] SimpleFollowCamera + CameraFocus funcionan sin cambios.
- [ ] CharacterController del player NO tocado (h=2.2, r=0.36, center=(0,1.1,0)).
- [ ] 0 errors, 0 missing references en console.
- [ ] 0 warnings de Animator (avatar/bones/retarget).
- [ ] N01-N05 jugables end-to-end.
- [ ] Main Menu sin missing references.
- [ ] `Docs/QA/AIDEN_MODEL_REPLACEMENT_QA.md` firmado.

---

## 8. BONE DEPENDENCY TABLE

**No aplica por diseño:** el proyecto no tiene dependencia de bones por nombre en el código (0 usos de `GetBoneTransform`/`HumanBodyBones`/`transform.Find("<bone_name>")` para huesos). El alineado del modelo al CharacterController (player) y al `EchoHeight` (eco) es por **renderer bounds**, no por huesos.

Lo único que depende del rig es el **Animator Controller**, que retargetea vía el avatar Mecanim. La tabla de huesos humanoid que Mixamo generará (Hips, Spine, Spine1, Spine2, Neck, Head, LeftShoulder, LeftArm, LeftForeArm, LeftHand, RightShoulder, RightArm, RightForeArm, RightHand, LeftUpLeg, LeftLeg, LeftFoot, LeftToe, RightUpLeg, RightLeg, RightFoot, RightToe) es la que Mecanim mapea automáticamente.

| Sistema | Dependencia de bones | Cómo se ajusta al nuevo modelo |
|---|---|---|
| PlayerCharacterVisualSetup (auto-fit) | Ninguna (bounds) | Igual — no requiere acción |
| EchoPlayback.ApplyModelAutoFit | Ninguna (bounds) | Igual — no requiere acción |
| EchoPlayback.AlignEchoModelToFeet | Ninguna (bounds) | Igual — no requiere acción |
| PlayerAnimController.controller (clips Mixamo) | Mecanim humanoid mapping | Retarget vía avatar de Aiden_rigged (automático) |
| Materiales del eco (ApplyEchoMaterials) | Ninguna (slots de renderer) | Igual — sobreescribe material slot a slot |

---

## 9. ROLLBACK RULE

Si en cualquier punto el swap falla (avatar inválido, animaciones no retargetean, console errors, N01-N05 no jugable):
1. Revertir `Assets/Resources/EchoesLocomotionSettings.asset` a su estado en HEAD 5373374.
2. Eliminar prefab y FBX Aiden_rigged (no afecta nada más).
3. Volver al estado estable previo. **No acumular parches sobre un setup roto.**
4. Diagnosticar causa raíz antes de un nuevo intento.

---

**Fin del documento de auditoría. Próximo里程碑: usuario completa rig Mixamo y coloca `Aiden_rigged.fbx`.**
