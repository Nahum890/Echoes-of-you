# 05 — Bugs conocidos y pendientes

Lo encontrado y **no** arreglado, por prioridad. Nada de esto está en marcha.

Corte: **2026-08-20**. Las entradas marcadas *re-verificado* se comprobaron ese
día contra el código; las de la sección heredada, no.

## Alto — rompe funcionalidad

### Diez `.uxml` no cargan en absoluto — la librería de componentes entera
✅ **Verificado** (log de importación de Unity + inspección directa, 2026-08-20)

No son dos componentes, son **cinco**, y están duplicados en dos rutas — diez
archivos en total, todos con el mismo fallo:

```
EchoButton.uxml   EchoCard.uxml   EchoDropdown.uxml   EchoPanel.uxml   EchoTabs.uxml
    × Assets/Resources/UI/Components/  y  Assets/UI/Components/

Xml is not valid: An XML comment cannot contain '--'.
```

La causa es que el comentario de cabecera contiene un ejemplo de uso con las
propias clases BEM del componente (`echo-card--available`,
`echo-button--primary`), y el `--` de la clase cierra el comentario XML.

Fallo de parseo → **ninguno de los cinco componentes existe en runtime**. Vienen
del pull de los componentes UI Stitch.

⚠️ **Consecuencia concreta y silenciosa:** `LevelSelectController.cs:56` hace

```csharp
if (_cardGrid == null || _cardTemplate == null) return;   // ← EchoCard.uxml
```

Es decir: si `EchoCard.uxml` no parsea, **el selector de niveles se queda vacío
sin lanzar ningún error**. Otro no-op silencioso, y en el camino por el que el
jugador entra a los niveles.

Arreglo trivial: quitar el `--` de dentro de los comentarios de cabecera (el
ejemplo de uso lleva las propias clases BEM del componente, y el `--` de
`echo-card--available` cierra el comentario). **Hay que tocar las diez copias**,
o arreglar de paso la duplicación.

### USS con unidades no soportadas
✅ **Verificado**

Misma duplicación que los `.uxml` — cada archivo existe en `Assets/UI/` y en
`Assets/Resources/UI/`, así que son **6 ocurrencias de `em`**, no 3:

```
Assets/UI/ChapterIntroUI.uss (43, 53)            Unsupported unit: '0.1em' / '-0.02em'
Assets/Resources/UI/ChapterIntroUI.uss (43, 53)  idem
Assets/UI/LevelCompleteUI.uss (70)               Unsupported unit: '0.1em'
Assets/Resources/UI/LevelCompleteUI.uss (70)     idem
EchoCard.uss (58)   Unsupported selector: '.echo-card--locked:not(.echo-card--locked):hover'
```

UI Toolkit no admite `em`. Cambiar a `px`.

### La UI está duplicada en dos árboles
✅ **Verificado** (2026-08-20) — hallazgo nuevo

`Assets/UI/` y `Assets/Resources/UI/` contienen copias **idénticas** de los
mismos `.uxml` y `.uss`. Solo la de `Resources/` es cargable con
`Resources.Load`. Todo arreglo de UI hay que aplicarlo dos veces o se corrige
una copia y se sigue viendo el bug desde la otra. Decidir cuál manda y borrar la
otra evitaría esta clase entera de problemas.

### Los niveles 4-15 no se han podido verificar visualmente
✅ **Verificado que no se puede**, ❓ **sin verificar la causa**

`bounds` de ~409 × 262 × 257 con centro en `y = -68`. O hay geometría dispersa
lejos (por debajo del umbral de 500 del pase de reparación) o hay algo raro en
la composición. Es el bloqueo principal para poder validar el resto del juego
igual que se validó Level_01.

### El workflow de CI es en gran parte ficción
✅ **Verificado 2026-08-20** — hallazgo nuevo

`.github/workflows/docs-validation.yml` define 10 jobs encadenados hasta un
"Final Certification Gate" que anuncia *"AI Production Readiness: 100/100"*.
**7 de los 10 llaman a métodos que no existen en el proyecto:**

| Job | `-executeMethod` | Estado |
|---|---|---|
| validate-specs | `ExecutableSpecValidator.ValidateProject` | ✅ existe |
| build-all-levels | `EchoesNewProductionBuilder.BuildAllBlueprints` | ✅ existe |
| validate-generated-scenes | `LevelValidator.RunAllChecks` | ⚠️ la clase existe, el método **no** |
| visual-regression | `VisualRegressionTest.Run` | ❌ la clase no existe |
| performance-stress | `PerformanceStressHarness.Run` | ❌ |
| roslyn-symbol-verifier | `RoslynSymbolVerifier.VerifyAll` | ❌ |
| guid-registry-sync | `AssetGuidValidator.ValidateAll` | ❌ |
| constant-registry-sync | `ConstantRegistryValidator.ValidateNoHardcodedPrimitives` | ❌ |
| authority-header-consistency | `AuthorityHeaderValidator.ValidateAll` | ❌ |
| vn-unit-tests | `-runTests -testFilter "VN_EndingResolver"` | ✅ la clase existe |

Además, tres problemas que harían fallar **todos** los jobs por igual:

1. `UNITY_VERSION: 2022.3.20f1` — el proyecto es **6000.4.3f1**.
2. Se invoca `unity` como comando en `ubuntu-latest` **sin instalarlo** (no hay
   `game-ci/unity-builder` ni equivalente). El binario no está en el runner.
3. Los pasos `Check ... Exit Code` hacen `if [ $? -ne 0 ]` en un step **nuevo**,
   y cada step de GitHub Actions es un shell distinto: `$?` siempre vale 0. Esas
   comprobaciones son **no-ops** aunque el paso anterior falle.

❓ **Sin verificar:** si el workflow ha llegado a ejecutarse alguna vez. Dado lo
anterior, no puede haber pasado en verde de forma legítima.

**No usar este workflow como fuente de verdad sobre qué se valida en el
proyecto.** Es la misma clase de problema que el resto de la capa de specs:
describe un sistema que no está implementado.

## Medio — afecta al aspecto, al rendimiento o a la coherencia

### La migración de input está a medias (dato corregido)
✅ **Re-verificado 2026-08-20.** Sustituye a la nota heredada, que decía justo lo
contrario y **ya no era cierta**.

Estado real hoy:

| | Nota heredada (2026-07) | Realidad ✅ |
|---|---|---|
| `com.unity.inputsystem` | no instalado | **instalado, 1.20.0** |
| `activeInputHandler` | `0` (solo legacy) | **`2` (Both)** |
| `.inputactions` | ninguno | **sigue sin haber ninguno** |
| Llamadas `Input.*` legacy | 19 en 9 archivos | **50 en 17 archivos** |

O sea: alguien instaló el paquete y puso el proyecto en modo *Both*, pero no
llegó a crear el asset de acciones ni a migrar nada. El gameplay sigue
íntegramente en el Input Manager legacy — y ha crecido, no menguado.

⚠️ **Modo *Both* no es gratis**: se inicializan los dos backends, y `Input.*`
legacy convive con `InputSystem`. `UIBootstrap.cs` y `UIBootstrap.Input.cs` ya
hacen `using UnityEngine.InputSystem`, así que hoy hay dos sistemas de input
vivos a la vez en el mismo proyecto.

⚠️ **`Assets/Scripts/Input/InputActionMap.cs` documenta lo viejo en su cabecera**:
dice literalmente *"NO usa el package com.unity.inputsystem (no instalado en el
proyecto)"*. Ese comentario es ahora falso. El wrapper en sí funciona — envuelve
`UnityEngine.Input` legacy y expone acciones canónicas a la UI — pero su
justificación caducó.

Frente a `INPUT_ACTION_MAPS.md` (SPEC-118), `RULE-INP-001` (el asset debe
existir) y `CONS-INP-001` (prohibido `Input.GetKey`) siguen **sin cumplirse**.
Migrar de verdad toca el núcleo de locomoción.


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
| Texturas en Bilinear | **44** texturas con `filterMode: 1` frente a 9 en Point (las 9 son las procedurales, que el generador ya pone bien). A resolución interna baja el resto emborrona el pixel art ✅ |
| API obsoleta | Warnings de `FindObjectsOfType` / `FindObjectsSortMode` por todo el proyecto ⚠️ heredado |

## Heredado de sesiones anteriores, sin re-verificar

⚠️ Todo lo de esta sección es de la auditoría 2026-07-26/27. **Puede haber
caducado** — verificar antes de actuar.

- ~~**El input es 100% legacy.** `activeInputHandler: 0`, `com.unity.inputsystem`
  no instalado, 19 llamadas en 9 archivos.~~ **Caducado** — ver la entrada
  re-verificada en la sección de arriba.
- **`LivingArchitecture` ya no existe** como archivo: se puede quitar de la lista
  de huérfanos de abajo. El resto sí siguen presentes ✅ **re-verificado**.
- **No existe escena de hub**: `HubSceneController` y `HubPortal` son huérfanos;
  `PauseMenu.hubSceneName = "MainMenu"`.
- **Scripts huérfanos** (0 referencias, 0 instancias): `EchoModeController` (lo
  que rompe los modos de eco Ambient/Imposed/Inversion/Mirror), `GhostBridge`,
  `MemoryPlatform`, `EchoDisintegrationZone`, `Paradox/Erosion`,
  `LightingApplier`, `EchoPathHint`,
  `ParkourPlatformMarker`, `EchoTemporalFragmentBurst`.
- **La validación greybox falla en los 15 niveles** (ARC-RHYTHM, NAV-ROUTE,
  NAV-COVERAGE).
- **Specs de nivel duplicados**: `Docs/Specs/LEVEL_SPEC_XX.md` (15) vs
  `Docs/Specs/Levels/LEVEL_SPEC_XX.md` (15). Nada indica cuál manda.
- **Discrepancia sin resolver**: la Frozen Decisions Matrix de
  `SOURCE_OF_TRUTH.md` dice "URP/Lit" para la geometría escolar, pero los
  materiales usan los shaders `Echoes/*`. No editar el SOT unilateralmente:
  va por `CHANGE_CONTROL`.
- ~~`Level_04_TEST.unity` sobra~~ — **resuelto**: la escena ya no está en
  `Assets/Scenes/`. Quedan sin embargo otras que parecen scratch:
  `Temp_AulaFBX.unity`, `VN_Dialogue_Test.unity` y tres
  `Assets/InitTestScene<guid>.unity` sueltas en la raíz de `Assets/`
  ✅ **verificado 2026-08-20**.
