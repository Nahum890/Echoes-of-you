# 02 — Pipeline visual (el look PS1)

Todo ✅ **verificado**: el código se leyó, compiló y se ejecutó en Unity.

## La decisión de arquitectura

El look PS1 estaba implementado **por material**. Es la capa equivocada: casi
todo lo que hace que algo se vea PS1 es un efecto de *pantalla*.

Consecuencias de tenerlo en el material: solo cubría los opacos (no partículas,
no transparencias, no el eco), no había un sitio único donde ajustarlo, y el
rasgo más definitorio de la consola — la **resolución interna baja** — no
existía en absoluto (`m_RenderScale: 1`, `m_RendererFeatures: []`).

**Ahora:** el framebuffer PS1 (dither + cuantización + scanlines) vive en un
pase full-screen. Los shaders de geometría solo hacen lo suyo: snapping de
vértices, affine mapping y sombreado plano.

## Piezas

| Archivo | Qué hace |
|---|---|
| `Assets/Shaders/PS1Post.shader` | Pase full-screen: dither ordenado 4×4, cuantización, scanlines |
| `Assets/Scripts/Rendering/PS1PostFeature.cs` | Renderer Feature (RenderGraph, URP 17.4) |
| `Assets/Editor/EchoesPS1LookSetup.cs` | Instalador: RenderScale + filtro + engancha la feature |
| `Assets/Shaders/PS1Common.hlsl` | Snapping y ruido compartidos por los dos shaders de geometría |
| `Assets/Shaders/PS1World.shader` | Arquitectura y props |
| `Assets/Shaders/PS1Character.shader` | Personajes (sin affine mapping: marea sobre malla animada) |

## Instalación

```
Echoes of You > URP > PS1 Look > Instalar (equilibrado 672x378)
```

Presets: fuerte `0.25` (480×270), equilibrado `0.35` (672×378), suave `0.5`
(960×540). Hay un **Desinstalar** que devuelve RenderScale a 1 y quita la
feature.

Ajuste fino: `Echoes_UniversalRenderer > Echoes PS1 Post`. Los valores viven en
la feature (una sola autoridad) y se empujan al material cada frame.

## Dos detalles que sostienen el efecto

Romper cualquiera de los dos lo anula **sin dar ningún error**. Están
comentados en el código por eso.

### 1. El filtro del upscale no sale del ajuste que parece

`FinalBlitPass.cs:117` elige entre sampler nearest y bilinear leyendo
`source.rt.filterMode` — **no** el `UpscalingFilter` del URP asset. Por eso la
RT del pase se crea con `FilterMode.Point` **explícito** en vez de confiar en el
valor por defecto:

```csharp
UniversalRenderer.CreateRenderGraphTexture(
    renderGraph, descriptor, "_EchoesPS1PostTarget", false, FilterMode.Point);
```

Si eso cae en `Bilinear`, el upscale difumina los píxeles y no queda nada.

### 2. El punto de inyección

`AfterRenderingPostProcessing`. Según `UniversalRendererRenderGraph.cs`:

```
1434  uber post-processing (bloom, color grading, vignette)
1443  ← pases custom en AfterRenderingPostProcessing  ← aquí
1466  FinalBlit (hace el upscale)
```

O sea: cuantiza **a resolución interna**, **después** del color grading y
**antes** del upscale. Si se cuantiza antes del grading, el grading vuelve a
estirar los valores y las bandas desaparecen.

Detalle relacionado: como la feature existe, `hasPassesAfterPostProcessing` es
`true`, lo que impide que URP renderice directo al backbuffer. Eso es lo que
garantiza que haya textura intermedia sobre la que trabajar.

## El dither

Se usa la **matriz real del hardware PSX**:

```
-4  0 -3  1
 2 -2  3 -1
-3  1 -4  0
 3 -1  2 -2
```

Su media es −0.5, por eso se normaliza con `(v + 4.5) / 8` y no con `+4`: así la
media queda en 0.5 y el dither no oscurece la imagen.

La cuantización ocurre en **espacio de display**, no lineal
(`LinearToSRGB` → cuantizar → `SRGBToLinear`). En lineal las bandas se amontonan
en las sombras y se ve mal.

## Bugs que tenían los shaders de geometría

Todos corregidos en `07400319`. Se listan porque explican comportamientos que
llevaban tiempo sin sentido:

| Bug | Efecto |
|---|---|
| Snapping en espacio de **objeto** | Deformaba el modelo una vez; **nunca temblaba**. El efecto PS1 nace de snappear en clip space |
| Affine mapping escalando la UV por `1/w` | No es affine mapping. Además sampleaba la textura **dos veces** (una línea muerta) |
| Sin `multi_compile` de sombras | `GetMainLight()` devolvía `shadowAttenuation = 1` siempre: **la direccional no proyectaba sombra** sobre estos materiales |
| Sin `multi_compile_fog` / `MixFog` | La niebla por capítulo de `lighting_profiles.yaml` **no llegaba** a 41 materiales |
| Scanlines sobre `worldPos.y` | Pegadas a la geometría: invisibles en el suelo, y a 120 bandas/metro hacían aliasing |
| `TEXTURE2D` dentro del `CBUFFER` (Character) | Ilegal; rompe SRP Batcher |
| Sin ambient ni luces adicionales (Character) | Como los niveles se iluminan con point lights, **el personaje salía casi negro** |
| Sin pases ShadowCaster/DepthOnly/DepthNormals (Character) | No proyectaba sombra, no escribía profundidad, no daba normales al SSAO |
| Sin `_BaseTex_ST` / `TRANSFORM_TEX` | **El tiling del inspector se ignoraba** — ver [03](03-MATERIALES-Y-SUPERFICIES.md) |

## El módulo VN no se ve afectado por el RenderScale

✅ **Verificado en el código de URP**, no supuesto.

`Assets/UI/EchoesPanelSettings.asset` tiene `m_RenderMode: 0`
(ScreenSpaceOverlay) y `m_TargetTexture: {fileID: 0}`. URP dibuja esos paneles
en `DrawScreenSpaceUIPass.cs:30-31`, que fija
`textureDesc.width = Screen.width` — resolución nativa, explícitamente no el
descriptor escalado de la cámara. El `ScaleMode` del panel también se calcula
contra `Screen`.

`VN_ChoiceGateController.cs:153` es IMGUI (`OnGUI`) y usa `Screen.width`
directamente, que el RenderScale tampoco toca.

**Resultado:** texto y retratos nítidos a 1080p sobre un mundo pixelado. Es una
decisión estética, no un bug. Si molesta, la opción barata es reimportar los PNG
de `Assets/UI/VN/Sprites/` a menor resolución con filtro Point. Meter la UI
dentro del efecto (vía `targetTexture`) obliga a resolver el mapeo del ratón con
`SetScreenToPanelSpaceFunction` y rompería el IMGUI del ChoiceGate.

Lo que **sí** baja de resolución es el mundo 3D del menú principal
(`MainMenuCinematicWorld`), porque eso renderiza por cámara.
