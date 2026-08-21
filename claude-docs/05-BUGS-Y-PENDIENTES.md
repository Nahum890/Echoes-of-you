# 05 — Bugs conocidos y pendientes

Lo encontrado y **no** arreglado, por prioridad. Nada de esto está en marcha.

## Alto — rompe funcionalidad

### Dos `.uxml` no cargan en absoluto
✅ **Verificado** (log de importación de Unity)

```
Assets/Resources/UI/Components/EchoCard.uxml
  Xml is not valid: An XML comment cannot contain '--'. Line 5, position 49

Assets/UI/Components/EchoButton.uxml
  Xml is not valid: ... Line 4, position 53
```

Fallo de parseo → los componentes `EchoCard` y `EchoButton` **no existen en
runtime**. Vienen del pull de los componentes UI Stitch. Arreglo trivial: quitar
el `--` de dentro de los comentarios XML.

### USS con unidades no soportadas
✅ **Verificado**

```
ChapterIntroUI.uss (43, 53)   Unsupported unit: '0.1em' / '-0.02em'
LevelCompleteUI.uss (70)      Unsupported unit: '0.1em'
EchoCard.uss (58)             Unsupported selector: '.echo-card--locked:not(.echo-card--locked):hover'
```

UI Toolkit no admite `em`. Cambiar a `px`.

### Los niveles 4-15 no se han podido verificar visualmente
✅ **Verificado que no se puede**, ❓ **sin verificar la causa**

`bounds` de ~409 × 262 × 257 con centro en `y = -68`. O hay geometría dispersa
lejos (por debajo del umbral de 500 del pase de reparación) o hay algo raro en
la composición. Es el bloqueo principal para poder validar el resto del juego
igual que se validó Level_01.

## Medio — afecta al aspecto o al rendimiento

### El token ámbar está reventado
✅ **Verificado** (inventario de escena + captura)

`Mat_Token_memory-amber` (`#FFBF00`, emisión ×1.2) aparece en **65 renderers de
Level_06** y **10 de Level_01**, y en la captura sale como una masa amarilla
quemada. ❓ Sospecha: se usó como fallback y se extendió más de la cuenta.
Concuerda con una nota de sesión anterior sobre "10 props amber sustituidos".

### `LiminalSurface` rompe el SRP Batcher
✅ **Verificado**

Sus pases ShadowCaster y DepthOnly declaran un `UnityPerMaterial` distinto al del
forward (solo `float _DepthDistort;`). Para que el SRP Batcher funcione, **todos**
los pases deben declarar el mismo bloque. Afecta a los 19 materiales que usan ese
shader. Es preexistente.

### Tilings sin ajustar
✅ **Verificado**

La tabla de `EchoesSchoolSurfacePass.cs` es una primera pasada calculada sobre
las dimensiones de los cubos del builder. En la captura de Level_01 el rayado de
la taquilla sale demasiado marcado.

### La causa del bug de locale sigue viva
❓ **Sin verificar**

El pase recoloca los fog volumes, pero no se localizó qué código los serializa
con el separador decimal mal. Si algo vuelve a generarlos, volverán a salir a
100 km.

## Bajo — deuda y limpieza

| Cosa | Detalle |
|---|---|
| Paquete muerto | `com.unity.postprocessing 3.5.4` sigue en el manifest con **0 referencias**. Borrable junto al define `UNITY_POST_PROCESSING_STACK_V2` ⚠️ heredado |
| Ramas | `ps1-liminal-shaders` y `ws01-cleanup` ya mergeadas, borrables. Más un `stash@{0}` sin valor ✅ |
| Log cosmético | `[School Surfaces] Aplicados 18/17`: la emisión de los fluorescentes va en la misma lista pero el denominador solo cuenta la tabla ✅ |
| Texturas en Bilinear | 43 texturas con `filterMode: 1` frente a 9 en Point (las 9 son las procedurales, que el generador ya pone bien). A resolución interna baja el resto emborrona el pixel art ✅ |
| API obsoleta | Warnings de `FindObjectsOfType` / `FindObjectsSortMode` por todo el proyecto ⚠️ heredado |

## Heredado de sesiones anteriores, sin re-verificar

⚠️ Todo lo de esta sección es de la auditoría 2026-07-26/27. **Puede haber
caducado** — verificar antes de actuar.

- **El input es 100% legacy y viola su propio spec.** `activeInputHandler: 0`,
  `com.unity.inputsystem` **no instalado**, ningún `.inputactions`, **19 llamadas
  a `Input.GetKey/GetAxis`** en 9 archivos. `INPUT_ACTION_MAPS.md` (SPEC-118)
  exige lo contrario: `RULE-INP-001` (el asset debe existir), `CONS-INP-001`
  (prohibido `Input.GetKey`). Implementación: **0%**. Migrar toca el núcleo de
  locomoción.
- **No existe escena de hub**: `HubSceneController` y `HubPortal` son huérfanos;
  `PauseMenu.hubSceneName = "MainMenu"`.
- **Scripts huérfanos** (0 referencias, 0 instancias): `EchoModeController` (lo
  que rompe los modos de eco Ambient/Imposed/Inversion/Mirror), `GhostBridge`,
  `MemoryPlatform`, `EchoDisintegrationZone`, `Paradox/Erosion`,
  `LivingArchitecture`, `LightingApplier`, `EchoPathHint`,
  `ParkourPlatformMarker`, `EchoTemporalFragmentBurst`.
- **La validación greybox falla en los 15 niveles** (ARC-RHYTHM, NAV-ROUTE,
  NAV-COVERAGE).
- **Specs de nivel duplicados**: `Docs/Specs/LEVEL_SPEC_XX.md` (15) vs
  `Docs/Specs/Levels/LEVEL_SPEC_XX.md` (15). Nada indica cuál manda.
- **Discrepancia sin resolver**: la Frozen Decisions Matrix de
  `SOURCE_OF_TRUTH.md` dice "URP/Lit" para la geometría escolar, pero los
  materiales usan los shaders `Echoes/*`. No editar el SOT unilateralmente:
  va por `CHANGE_CONTROL`.
- `Level_04_TEST.unity` sobra y `EchoesLightingBakePipeline` no la filtra.
