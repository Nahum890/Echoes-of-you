# 06 — Herramientas y flujos

## Menús relevantes

✅ **Verificado**

### Los que hay que correr tras un clon nuevo

```
1. Echoes of You > Art > Apply School Surfaces (regenerando texturas)
2. Echoes of You > URP > PS1 Look > Instalar (equilibrado 672x378)
3. Echoes of You > Art > Repair Scene Surfaces (All Levels)
```

**Ojo:** si se hace `git pull` de un repo donde ya se corrieron, **no hace falta
ejecutar nada**. Los materiales, texturas, escenas y la config del URP van
commiteados como *assets*, no como código. Los menús solo son necesarios cuando
esos assets aún no reflejan el resultado.

### Otros útiles

| Menú | Qué |
|---|---|
| `Art > Generate Lo-Fi Textures` | Regenera las 9 texturas procedurales |
| `URP > PS1 Look > Desinstalar` | RenderScale a 1, filtro Automatic, quita la feature |
| `Technical Art > Validate All Levels (Visual)` | Compara contra baselines en `Docs/Art/ReferenceFrames` |
| `URP > Setup SSAO and Graphics` | Recrea la feature de SSAO si Unity la borró |

### ⚠️ Destructivo — no ejecutar

```
Echoes of You > Production > Build All School Greybox Levels (NEW)
```

Hace `NewScene(EmptyScene)` y reescribe los 15 niveles **sin props, luces ni
cámara**. Borra todo el trabajo de ambientación.

## Ejecutar Unity sin abrir el editor

✅ **Verificado, usado varias veces en esta sesión**

⚠️ La ruta del editor es literal y hay **dos versiones instaladas**
(`6000.4.3f1` y `2022.3.62f1`): apuntar siempre a la primera. Y `<ruta del
proyecto>` cambia entre los dos portátiles — ver [01](01-ESTADO-ACTUAL.md#entorno-de-desarrollo).

Sirve para importar, compilar shaders y ejecutar menús sin tocar la GUI:

```bash
"C:/Program Files/Unity/Hub/Editor/6000.4.3f1/Editor/Unity.exe" \
  -batchmode -quit \
  -projectPath "<ruta del proyecto>" \
  -executeMethod EchoesSchoolSurfacePass.RegenerateAndApply \
  -logFile "<ruta del log>"
```

Notas:
- Bloquea el proyecto mientras corre: no se puede abrir el editor a la vez.
- La primera vez tras un pull grande tarda bastante (reimporta todo).
- Al cerrar suelta un `UnassignedReferenceException` de `TMP_FontAsset` en
  `TMP_EditorResourceManager.cs:34`. Es un tic conocido de TMP durante
  `EditorApplicationQuit`, **ocurre después** del método y no afecta a nada.
- Para leer el log: buscar `error CS`, `Shader error` y el prefijo del propio
  método (`[School Surfaces]`, `[Scene Repair]`...).

## Compile check de C# sin abrir Unity

✅ **Verificado**

Evita dejar el proyecto en Safe Mode. Usa el Roslyn que trae el propio editor:

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

Dos trampas que dan **errores falsos**:

- **No** referenciar `Managed/UnityEditor.dll`: choca con los
  `UnityEditor.*Module.dll` de `Managed/UnityEngine/` y suelta `CS0433` en
  `MenuItem`, `SerializedObject`, etc. Usar solo los módulos.
- Hace falta el shim `NetStandard/compat/2.1.0/shims/netfx/mscorlib.dll` o sale
  `CS0012` al tocar tipos de `Assembly-CSharp*.dll`.

Ignorables: `CS0436` (el archivo también existe en la DLL referenciada) y
`CS0618` (API obsoleta).

Requiere que `Library/ScriptAssemblies/` exista, o sea que Unity haya compilado
al menos una vez. **No valida HLSL**: los shaders solo los verifica Unity al
importarlos.

## Capturar frames del juego

Para ver el resultado sin abrir el editor: un script temporal en `Assets/Editor`
que abra la escena, cree una cámara, renderice a `RenderTexture` y guarde un PNG.
Ejecutar con `-executeMethod`.

Aprendizajes:
- Encuadrar por `bounds` de los `MeshRenderer`, **filtrando** los que estén lejos
  del origen y los volúmenes de niebla, o un solo objeto disparado arruina el
  encuadre.
- Poner `cam.clearFlags = SolidColor` con un color oscuro. Si no, la cámara
  desechable cae al **skybox azul por defecto** de Unity en modo edición y
  parece un bug del juego. En runtime `PostProcessingSetup.cs:159` pone el fondo
  en `(0.039, 0.039, 0.051)`.
- Borrar el script temporal después: no debe llegar al commit.

## Git

✅ **Verificado / ⚠️ heredado**

- Flujo usado en esta sesión: rama → commit → `checkout main` → `merge --ff-only`
  → `push origin main`.
- **No añadir `Co-Authored-By`** a los mensajes de commit ⚠️.
- Preferencia del usuario: **merge commit, no squash** ⚠️.
- Para reconciliar ramas divergentes con escenas `.unity` grandes: **merge de una
  pasada, nunca rebase** ⚠️.
- `git config core.longpaths true` es obligatorio en clones nuevos ✅.
- Tras cualquier merge de arquitectura, **buscar declaraciones duplicadas con
  grep antes de pushear**: git puede auto-mergear sin conflicto dos
  declaraciones del mismo campo si están en zonas distintas del archivo, y el
  `CS0102` solo aparece al compilar en Unity ⚠️.
- Al commitear tras correr Unity, revisar `ProjectSettings/`: suelen aparecer
  como modificados con **0 líneas de cambio real** (solo finales de línea).
  Restaurarlos con `git checkout --` antes de commitear ✅.

## Cómo se generan los `.meta` a mano

✅ **Verificado**

Si se crean assets sin abrir Unity, hay que generar sus `.meta` o cada máquina
inventará un GUID distinto y las referencias se romperán. Formato que usa este
repo:

```yaml
# .cs — el repo los tiene minimalistas, sin salto final
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

Comprobar siempre que el GUID no colisione con ninguno existente.
