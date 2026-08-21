# 03 — Materiales y superficies

## El hallazgo central

El proyecto tiene **77 materiales** en `Assets/Materials/Echoes/` y un generador
procedural de texturas. No faltaba nada. Lo que había eran **mecanismos rotos en
silencio**: código que existía, parecía correcto y nunca se ejecutaba.

Los cuatro, todos ✅ **verificados y corregidos**:

| Mecanismo | Por qué no hacía nada |
|---|---|
| `KenneyTiling.cs` | Comprobaba `_MainTex` y `_BaseMap`; los shaders `Echoes/*` usan **`_BaseTex`** → early-out en **toda** la geometría del juego |
| `TRANSFORM_TEX` en los shaders | `PS1World` y `LiminalSurface` no declaraban `_BaseTex_ST` → el tiling del inspector se ignoraba (41 materiales) |
| `AssignMaterialTextures` | Buscaba `"Mat_Cork"`; el material se llama **`Mat_CorkBoard`** |
| Materiales sin asignar | 92 renderers con `NULL` + 123 con `'Lit'` — ver [04](04-AUDITORIA-ESCENAS.md) |

El patrón es el mismo en los cuatro: **un no-op silencioso**. Nada falla, nada
avisa, simplemente no pasa. Vale la pena tenerlo presente al revisar este
proyecto: cuando algo "debería verse y no se ve", buscar primero un early-out o
un nombre que no casa, antes que un valor mal puesto.

## Sistema de materiales

### Tokens (creados por código)

`EchoesMaterialLibrary` genera `Mat_Token_*` con `[InitializeOnLoadMethod]`, o
sea **en cada domain reload**. Los tokens son de identidad cromática por
capítulo: `corridor-navy`, `institutional-teal`, `faded-mustard`, `sage-green`,
`dusty-rose`, `fluorescent-sick`, `void-black`, `memory-amber`, `wrongness-red`,
`echo-cyan`.

⚠️ **Editar estos `.mat` a mano no sirve**: la librería los reescribe. Hay que
cambiar el código.

### Arquitectura (a mano)

Los `Mat_Arch_*` **no los crea ningún script**. Son editables y los pases sí
persisten.

### Los dos pases

| Menú | Qué hace | Archivo |
|---|---|---|
| `Art > Apply School Surfaces (regenerando texturas)` | Reparte texturas a **17 materiales** + emisión a los fluorescentes | `EchoesSchoolSurfacePass.cs` |
| `Art > Repair Scene Surfaces (All Levels)` | Asigna material a la geometría huérfana, recoloca strays, arregla luces y bloom | `EchoesSceneRepairPass.cs` |

Ambos son **idempotentes**. El primero toca assets de material; el segundo
escribe en los 15 `.unity`.

## El problema del yeso teñido

`tex_plaster_wall_128` tiene color propio (`2B4A4A`, teal). Sirve para paredes
teal, pero **convertiría en teal las paredes de todos los capítulos** si se
aplicase con `_BaseColor` blanco.

Solución: se generó `tex_plaster_neutral_128`, un yeso **gris** (gotelé +
ondulación de llana + algo de pintura descascarillada), y el pase tiene un flag
por entrada:

```csharp
whiteBase = true   // la textura trae su color → _BaseColor a blanco (taquillas, puertas)
whiteBase = false  // la textura es neutra → CONSERVA el _BaseColor (paredes)
```

Así la pared rosa sigue siendo rosa y solo gana relieve.

## Generador de texturas

`Assets/Editor/LoFiTextureGenerator.cs`, menú `Art > Generate Lo-Fi Textures`.
Genera **9 texturas** procedurales a 128/256 px con **filtro Point y sin
mipmaps** (correcto para PS1).

| Original | Añadidas en esta sesión |
|---|---|
| yeso de pared, linóleo, madera, pizarra, corcho | **taquilla** (chapa, rejilla, tirador), **placa de techo acústico** (perfil en T, picado, humedad), **puerta de aula** (dos paneles, ventanuco), **yeso neutro** |

### Truco de verificación

Estos generadores son código de editor que no se puede ejecutar fuera de Unity.
**Replicar el bucle de píxeles en Python + PIL y mirar el PNG** encontró dos
fallos reales antes de darlos por buenos:

- El picado del techo salía como **rayado diagonal**: `(xi*7 + yi*13) % 11` con
  aritmética modular genera una retícula, no dispersión. Hizo falta un hash de
  verdad (`Hash01`, determinista, para que el patrón no cambie al regenerar).
- La puerta quedaba plana con un solo panel. Se pasó a dos paneles rehundidos
  con travesaño.

Merece la pena hacerlo antes de aceptar cualquier textura procedural.

## Tabla de superficies

`EchoesSchoolSurfacePass.cs` — el tiling es **por cara** (los cubos de Unity
tienen UV 0..1 en cada cara); de escalar según el tamaño real se encarga
`KenneyTiling`, ahora que funciona.

⚠️ **Los tilings son una primera pasada**, calculados sobre las dimensiones de
los cubos de `BuildSchoolGreyboxLevels` (locker 0.4×1.8×0.8). La tabla está
pensada para tocarla al ver el resultado. En la única captura disponible el
rayado de la taquilla sale demasiado marcado.

## Regeneración de texturas: efecto secundario

`RegenerateAndApply` regenera **también** las 5 texturas que ya existían. Usan
`Random.value`, así que el ruido cambia en cada ejecución aunque se vean
equivalentes. Si no se quiere ese churn en el diff, usar
`Art > Apply School Surfaces` (sin regenerar).
