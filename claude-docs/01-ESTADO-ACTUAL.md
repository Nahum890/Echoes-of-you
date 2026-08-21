# 01 — Estado actual

Corte: **2026-08-20**. Re-verificado el mismo día desde el **segundo portátil**
(ver [Entorno](#entorno-de-desarrollo) — la ruta del proyecto no es la misma en
las dos máquinas).

## Stack

✅ **Verificado** (`ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`)

| | |
|---|---|
| Unity | 6000.4.3f1 |
| Render pipeline | URP 17.4.0 (core y universal-config a la par) |
| Cámara | Cinemachine 3.1.7 |
| UI | UI Toolkit (UXML/USS) + algo de IMGUI en la VN |
| Repo | `Nahum890/Echoes-of-you` — el usuario es **colaborador**, no dueño |

⚠️ **Heredado:** el dueño del repo sube arquitectura nueva a `main` con
frecuencia. Antes de trabajar, comparar `HEAD` con `origin/main`.

## Pipeline de render activo

✅ **Verificado.** `GraphicsSettings.m_CustomRenderPipeline` y los 6 niveles de
calidad apuntan a `Assets/Settings/Echoes_URPAsset.asset`
(guid `11848ac93bbdbf44393e8dc922ed5887`), que a su vez referencia
`Echoes_UniversalRenderer.asset`.

Lo único que sigue viniendo del sample ParticlePack es
`UniversalRenderPipelineGlobalSettings.asset` (guid `25eaace5...`), que es otra
cosa: default volume profile y shader stripping, no el pipeline.

> Una nota de sesiones anteriores decía "el URP activo es el de ParticlePack".
> **Era incorrecta** y confundía el pipeline asset con el global settings.

## Ramas

✅ **Verificado** (`git ls-remote`, 2026-08-20)

Remotas — solo tres:

```
main                  0b47754a   ← todo el trabajo de esta sesión
ps1-liminal-shaders   07400319   ← ya mergeada en main, borrable
ws01-cleanup          a4719883   ← absorbida por main, borrable
```

⚠️ **Locales que no existen en remoto.** En el portátil 2 hay además siete ramas
sin rastrear o con upstream borrado. No confundirlas con trabajo pendiente sin
mirarlas antes:

```
Arreglar-animaciones                db759145
Greybox                             bb4d0940
Mejorar_la_estetica_y_jugabilidad   db091b24   (upstream: gone)
arrglar-plate-fov                   00b2e10b
cambios-camara                      501ef041
exp/ual1-retest                     7e0790f9
urp-migration                       ba23430b   (upstream: gone)
```

### Stashes

✅ **Verificado.** Hay **cuatro**, no uno. Solo el primero es reciente:

```
stash@{0}  pre-pull-20260820 cambios locales de arte/escenas   ← ver abajo
stash@{1}  WIP on exp/ual1-retest
stash@{2}  On Prueba: !!GitHub_Desktop<Prueba>
stash@{3}  WIP on main: ae50a01 Refactor code structure...
```

`stash@{0}` se creó al traer `main` en el portátil 2: había 50 archivos de arte
sin commitear (materiales, texturas LoFi, escenas 03-06, config URP) que
bloqueaban el fast-forward. Es **churn regenerado**, no trabajo nuevo: los pases
de arte usan `Random.value`, así que producen un diff distinto en cada ejecución
aunque el resultado sea equivalente, y lo que traía el pull ya viene commiteado
como asset. Descartable salvo por las capturas untracked que arrastró
(`Assets/Screenshots/audit_L06_*.png`).

Los tres restantes son de sesiones antiguas y ninguno se ha evaluado.

## Trabajo de esta sesión (los 4 commits en `main`)

| Commit | Qué |
|---|---|
| `07400319` | Look PS1 movido al pipeline + reparación de los shaders de mundo y personaje |
| `85483521` | Texturizado de superficies: 3 mecanismos que eran no-ops |
| `60f6e438` | Geometría huérfana, fog volumes recolocados, fuentes de luz visibles |
| `0b47754a` | Estos documentos (`claude-docs/`) |

Detalle en [02](02-PIPELINE-VISUAL.md), [03](03-MATERIALES-Y-SUPERFICIES.md) y
[04](04-AUDITORIA-ESCENAS.md).

## Entorno de desarrollo

✅ **Verificado**

- ⚠️ **La ruta del proyecto es distinta en cada portátil.** No dar ninguna por
  supuesta: comprobar dónde hay `Library/`, porque esa es la copia que Unity
  abre. Editar un clon sin `Library/` **no llega al editor**.

  | | Ruta | `Library/` |
  |---|---|---|
  | Portátil 1 | `C:\Users\User\OneDrive\Escritorio\Proyectos\games\echoes-of-you` | sí |
  | Portátil 1 | `C:\dev\echoes-of-you` | no — clon muerto |
  | Portátil 2 | `C:\Users\lol xdd\OneDrive\Documentos\Colegio\Echoes of you` | sí ✅ |

- **Dos editores instalados** en el portátil 2: `6000.4.3f1` (el del proyecto) y
  `2022.3.62f1`. Al lanzar batch mode, apuntar explícitamente al primero.
- Log del editor: `%LOCALAPPDATA%\Unity\Editor\Editor.log`. Para saber si la
  compilación actual está limpia hay que filtrar `error CS` **posteriores a la
  última línea `Completed reload`** — el log acumula errores viejos y engaña.
- ⚠️ **Rutas largas:** el checkout falla con `Filename too long` en
  `Assets/Nuevos assets/Meshy_AI_*_texture_fbx/`. Resuelto con
  `git config core.longpaths true`. Ponerlo **antes** del primer checkout en un
  clon nuevo.
- ⚠️ **Límite de GitHub:** 100 MB por archivo, y se decidió **no usar Git LFS**.
  Los `.zip` de asset packs no deben commitearse (ya pasó con uno de 123 MB).

## Trampas del editor

✅ **Verificado**

- **`EchoesMaterialLibrary.EnsureMaterials()` es `[InitializeOnLoadMethod]`**:
  reescribe `Assets/Materials/Echoes/` en cada domain reload. Editar esos `.mat`
  a mano no sirve de nada; hay que cambiar el código de la librería.
  Excepción: los `Mat_Arch_*` **no** los crea código, así que sí son editables.
- **Al reserializar, Unity puede dejar `Echoes_UniversalRenderer.asset` sin el
  renderer feature de SSAO.** Si el diff reaparece, no commitearlo.
- ⚠️ **`Echoes of You > Production > Build All School Greybox Levels (NEW)` es
  destructivo**: hace `NewScene(EmptyScene)` y reescribe los 15 niveles sin
  props, luces ni cámara. **No ejecutarlo.**

## Arquitectura, en una frase

⚠️ **Heredado** (auditoría 2026-07-26/27, no re-verificado en esta sesión)

15 niveles greybox construidos por código, agrupados en 6 capítulos emocionales,
con un sistema de "ecos" (grabación del jugador a 30 Hz + Catmull-Rom) como
mecánica central. Jugador y eco con `CharacterController`. Cámara real de
gameplay: `FixedPuzzleCameraController` + CinemachineCamera + TargetGroup.

Mapa nivel→capítulo (`EchoesTechnicalArtPass.LevelChapter`) ✅ **verificado**:

```
I   Persistence   1, 2, 3        IV  Optimization  10, 11
II  Coordination  4, 5, 8        V   Consequence   12, 13
III Confidence    6, 7, 9        VI  Acceptance    14, 15
```

Nótese que **no es correlativo**: el 8 es del capítulo II y el 9 del III.
