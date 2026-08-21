# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> El proyecto, sus documentos y sus mensajes de commit están **en español**.
> Mantener ese idioma al escribir código, comentarios, docs y commits.

## Qué es esto

*Echoes of You* — juego de puzzles 3D en Unity **6000.4.3f1** con **URP 17.4**.
La mecánica central son los "ecos": el jugador se graba a 30 Hz y reproduce esa
grabación como un segundo actor con el que colabora. 15 niveles greybox
agrupados en 6 capítulos emocionales, más un módulo de novela visual (VN).

## Antes de nada: leer `claude-docs/`

`claude-docs/` contiene informes de estado escritos por sesiones anteriores de
Claude Code, con cada afirmación marcada ✅ verificado / ⚠️ heredado / ❓ sin
verificar. **Empezar por ahí**, sobre todo:

| | |
|---|---|
| [`01-ESTADO-ACTUAL.md`](claude-docs/01-ESTADO-ACTUAL.md) | Stack, ramas, entorno, trampas del editor |
| [`05-BUGS-Y-PENDIENTES.md`](claude-docs/05-BUGS-Y-PENDIENTES.md) | Lo roto y no arreglado, priorizado |
| [`06-HERRAMIENTAS-Y-FLUJOS.md`](claude-docs/06-HERRAMIENTAS-Y-FLUJOS.md) | Batch mode, compile check, git |

## ⚠️ La documentación miente. Verificar siempre contra el código

Ésta es la característica que más tiempo hace perder en este repo. Hay **118
documentos `.md` en `Docs/`** (332 archivos) que describen un sistema de
"specs ejecutables" mucho más completo que lo implementado:

- **`ExecutableSpecValidator.cs` valida exactamente una regla** (`MAT-001`),
  frente a decenas de `RULE-`/`CONS-`/`FAIL-`/`VAL-` repartidas por los specs.
- **`.github/workflows/docs-validation.yml` es en gran parte ficción.** De los
  10 jobs, **7 llaman a métodos que no existen**: `VisualRegressionTest.Run`,
  `PerformanceStressHarness.Run`, `RoslynSymbolVerifier.VerifyAll`,
  `AssetGuidValidator.ValidateAll`, `ConstantRegistryValidator...`,
  `AuthorityHeaderValidator.ValidateAll` y `LevelValidator.RunAllChecks` (la
  clase `LevelValidator` existe dentro de `SchoolGreyboxProductionBuilder.cs`,
  pero no tiene ese método). Además fija `UNITY_VERSION: 2022.3.20f1` — versión
  equivocada — e invoca `unity` sin instalarlo en el runner. **No usar ese
  workflow como fuente de verdad sobre qué se valida.**
- Los specs de nivel están **duplicados**: `Docs/Specs/LEVEL_SPEC_XX.md` (15) y
  `Docs/Specs/Levels/LEVEL_SPEC_XX.md` (15). Nada indica cuál manda.

`Docs/Authority/SOURCE_OF_TRUTH.md` y `CHANGE_CONTROL.md` se declaran
autoritativos: **no editarlos unilateralmente**, van por control de cambios.

## El patrón de fallo dominante: el no-op silencioso

Cuando algo "debería verse y no se ve", en este repo la causa casi nunca es un
valor mal puesto. Es **código que existe, parece correcto y nunca se ejecuta**:
un early-out por un nombre de propiedad que no casa, un `Find` con un string
equivocado, un shader al que le falta declarar algo. Buscar eso *primero*.

Ejemplos reales ya corregidos: `KenneyTiling` comprobaba `_MainTex`/`_BaseMap`
cuando los shaders `Echoes/*` usan `_BaseTex` (early-out en toda la geometría del
juego); `AssignMaterialTextures` buscaba `"Mat_Cork"` y el material se llama
`Mat_CorkBoard`; los shaders no declaraban `_BaseTex_ST`, así que el tiling del
inspector se ignoraba en 41 materiales.

## Comandos

### Ejecutar Unity sin abrir el editor

Es la vía principal para importar, compilar shaders y ejecutar pases de editor.
Bloquea el proyecto mientras corre (no se puede abrir el editor a la vez).

```bash
"C:/Program Files/Unity/Hub/Editor/6000.4.3f1/Editor/Unity.exe" \
  -batchmode -quit \
  -projectPath "<ruta del proyecto>" \
  -executeMethod EchoesSchoolSurfacePass.RegenerateAndApply \
  -logFile "<ruta del log>"
```

Hay **dos editores instalados** (`6000.4.3f1` y `2022.3.62f1`): apuntar siempre
al primero. Al cerrar suelta un `UnassignedReferenceException` de `TMP_FontAsset`
en `TMP_EditorResourceManager.cs:34` — tic conocido de TMP durante
`EditorApplicationQuit`, ocurre *después* del método y no afecta a nada.

Para leer el log: buscar `error CS`, `Shader error` y el prefijo del propio pase
(`[School Surfaces]`, `[Scene Repair]`…).

### Tests

Unity Test Framework, assembly `Echoes.Tests` (`Assets/Tests/`). El asmdef es
**Editor-only**, así que todo corre en EditMode pese a que un archivo se llame
`SettingsPanelPlayModeTests.cs`.

```bash
# todos
Unity.exe -batchmode -runTests -testMode EditMode -projectPath . -quit

# uno solo
Unity.exe -batchmode -runTests -testMode EditMode -testFilter "VN_EndingResolver" -projectPath . -quit
```

### Compile check de C# sin abrir Unity

Evita dejar el proyecto en Safe Mode. Usa el Roslyn del propio editor y requiere
que `Library/ScriptAssemblies/` exista (o sea, que Unity haya compilado al menos
una vez). **No valida HLSL** — los shaders solo los verifica Unity al importar.

```bash
UD="C:/Program Files/Unity/Hub/Editor/6000.4.3f1/Editor/Data"

for d in Library/ScriptAssemblies/*.dll;  do echo "-r:\"$PWD/$d\"" >> refs.rsp; done
for d in "$UD/Managed/UnityEngine/"*.dll; do echo "-r:\"$d\"" >> refs.rsp; done

"$UD/NetCoreRuntime/dotnet.exe" "$UD/DotNetSdkRoslyn/csc.dll" -nologo \
  -target:library -langversion:9.0 -nostdlib+ -noconfig -out:check.dll \
  -r:"$UD/NetStandard/ref/2.1.0/netstandard.dll" \
  -r:"$UD/NetStandard/compat/2.1.0/shims/netfx/mscorlib.dll" \
  "@refs.rsp" -define:UNITY_EDITOR -define:UNITY_6000_0_OR_NEWER \
  Assets/.../MiScript.cs
```

Dos trampas que producen **errores falsos**: no referenciar
`Managed/UnityEditor.dll` (choca con los `UnityEditor.*Module.dll` y suelta
`CS0433` en `MenuItem`, `SerializedObject`…), y hace falta el shim
`NetStandard/compat/2.1.0/shims/netfx/mscorlib.dll` o sale `CS0012`. Ignorables:
`CS0436` y `CS0618`.

## Arquitectura

### Los niveles se construyen por código, no a mano

`Assets/Editor/` contiene builders y **pases idempotentes** que escriben
directamente en los 15 `.unity`. Ésta es la parte que hay que entender antes de
tocar nada visual, porque un pase mal lanzado sobreescribe trabajo manual.

| Menú (`Echoes of You/…`) | Qué hace |
|---|---|
| `Art > Apply School Surfaces` | Reparte texturas a 17 materiales + emisión a los fluorescentes |
| `Art > Apply School Surfaces (regenerando texturas)` | Idem, pero además regenera las 9 texturas procedurales |
| `Art > Repair Scene Surfaces (All Levels)` | Asigna material a geometría huérfana, recoloca strays, arregla luces y bloom |
| `Art > Generate Lo-Fi Textures` | Las 9 texturas procedurales (128/256 px, Point, sin mipmaps) |
| `URP > PS1 Look > Instalar (…)` | RenderScale + filtro + engancha la renderer feature |

**Tras un `git pull` normalmente no hay que ejecutar nada**: materiales,
texturas, escenas y la config del URP van commiteados como *assets*, no como
código. Los menús solo hacen falta cuando esos assets aún no reflejan el
resultado.

### 🔴 Menú destructivo — no ejecutar

```
Echoes of You > Production > Build All School Greybox Levels (NEW)
```

Hace `NewScene(EmptyScene)` y reescribe los 15 niveles **sin props, luces ni
cámara**. Borra todo el trabajo de ambientación.

### Pipeline visual: el look PS1

Vive en el **pipeline**, no en los materiales — casi todo lo que hace que algo se
vea PS1 es un efecto de pantalla. Ver [`claude-docs/02`](claude-docs/02-PIPELINE-VISUAL.md).

- `Assets/Shaders/PS1Post.shader` + `Assets/Scripts/Rendering/PS1PostFeature.cs`
  — pase full-screen (dither ordenado 4×4, cuantización, scanlines).
- `Assets/Shaders/PS1World.shader` / `PS1Character.shader` / `PS1Common.hlsl` —
  solo geometría: snapping en *clip space*, affine mapping, sombreado plano.

**Dos detalles sostienen el efecto y romperlos no da ningún error**: la RT del
pase debe crearse con `FilterMode.Point` explícito (`FinalBlitPass` decide el
sampler leyendo `source.rt.filterMode`, **no** el `UpscalingFilter` del URP
asset), y la inyección va en `AfterRenderingPostProcessing` — cuantizar antes del
color grading hace que el grading vuelva a estirar los valores y las bandas
desaparezcan. Ambos están comentados en el código por eso.

El pipeline activo es `Assets/Settings/Echoes_URPAsset.asset`. No confundirlo con
`UniversalRenderPipelineGlobalSettings.asset`, que viene del sample ParticlePack
y solo controla default volume profile y shader stripping.

### Materiales

77 materiales en `Assets/Materials/Echoes/`, en dos familias con reglas opuestas:

- **`Mat_Token_*`** los genera `EchoesMaterialLibrary.EnsureMaterials()`, que es
  `[InitializeOnLoadMethod]` — o sea, **reescribe esos `.mat` en cada domain
  reload**. Editarlos a mano no sirve de nada; hay que cambiar el código.
- **`Mat_Arch_*`** no los crea ningún script: son editables y los pases
  persisten.

### Runtime

`Assets/Scripts/` es plano en su mayoría (sin asmdefs salvo los tests), con
subcarpetas por dominio: `Echo/`, `VN/`, `Puzzle/`, `Rendering/`, `Lighting/`,
`Narrative/`, `UI/`, `Input/`. Jugador y eco usan `CharacterController`. La cámara
de gameplay es `FixedPuzzleCameraController` + CinemachineCamera + TargetGroup.

Mapa nivel→capítulo (`EchoesTechnicalArtPass.LevelChapter`) — **no es correlativo**:

```
I   Persistence   1, 2, 3        IV  Optimization  10, 11
II  Coordination  4, 5, 8        V   Consequence   12, 13
III Confidence    6, 7, 9        VI  Acceptance    14, 15
```

### Input: dos sistemas vivos a la vez

`com.unity.inputsystem` 1.20.0 está instalado y `activeInputHandler: 2` (Both),
pero **no existe ningún `.inputactions`** y el gameplay sigue íntegramente en el
Input Manager legacy (~50 llamadas `Input.*` en 17 archivos). `UIBootstrap.cs` y
`UIBootstrap.Input.cs` sí usan `UnityEngine.InputSystem`.
`Assets/Scripts/Input/InputActionMap.cs` es un wrapper sobre el input legacy para
la UI — su comentario de cabecera afirma que el paquete no está instalado, lo
cual **ya no es cierto**.

### UI duplicada en dos árboles

`Assets/UI/` y `Assets/Resources/UI/` contienen copias **idénticas** de los mismos
`.uxml` y `.uss`; solo la de `Resources/` es cargable con `Resources.Load`. Todo
arreglo de UI hay que aplicarlo dos veces, o se corrige una copia y el bug sigue
apareciendo desde la otra.

## Entorno

⚠️ **La ruta del proyecto cambia entre las dos máquinas del usuario.** No dar
ninguna por supuesta: la copia que Unity abre es **la que tiene `Library/`**.
Editar un clon sin `Library/` no llega al editor. Log del editor en
`%LOCALAPPDATA%\Unity\Editor\Editor.log`; para saber si la compilación está
limpia hay que filtrar `error CS` **posteriores a la última línea
`Completed reload`**, porque el log acumula errores viejos y engaña.

## Git

- **No añadir `Co-Authored-By`** a los mensajes de commit.
- Preferencia del usuario: **merge commit, no squash**.
- Ramas divergentes con escenas `.unity` grandes: **merge de una pasada, nunca
  rebase**.
- `git config core.longpaths true` es **obligatorio antes del primer checkout**
  en un clon nuevo, o falla con `Filename too long` en
  `Assets/Nuevos assets/Meshy_AI_*_texture_fbx/`.
- **Sin Git LFS**, y GitHub limita a 100 MB por archivo: los `.zip` de asset
  packs no deben commitearse.
- Tras cualquier merge de arquitectura, **buscar declaraciones duplicadas con
  grep antes de pushear**: git puede auto-mergear sin conflicto dos declaraciones
  del mismo campo si están en zonas distintas del archivo, y el `CS0102` solo
  aparece al compilar en Unity.
- Al commitear tras correr Unity, revisar `ProjectSettings/`: suelen aparecer
  modificados con 0 líneas de cambio real (solo finales de línea). Restaurarlos
  con `git checkout --`.
- Unity puede dejar `Echoes_UniversalRenderer.asset` sin el renderer feature de
  SSAO al reserializar. Si ese diff reaparece, **no commitearlo**.
- Los pases de arte usan `Random.value`: regenerar texturas produce un diff
  distinto cada vez aunque el resultado sea equivalente. Usar
  `Art > Apply School Surfaces` (sin regenerar) para evitar ese churn.

## Crear assets sin abrir Unity

Hay que generar el `.meta` a mano o cada máquina inventará un GUID distinto y las
referencias se romperán. Formato del repo (comprobar siempre que el GUID no
colisione):

```yaml
# .cs — minimalistas, sin salto final
fileFormatVersion: 2
guid: <32 hex>

# .shader / .hlsl
fileFormatVersion: 2
guid: <32 hex>
ShaderImporter:
  externalObjects: {}
  defaultTextures: []
  nonModifiableTextures: []
  preprocessorOverride: 0    # solo .hlsl
  userData:
  assetBundleName:
  assetBundleVariant:
```
