# ASSETS FALTANTES — Echoes of You

Inventario de todo lo que los documentos de diseño (ECHOES_BIBLE.md, VISUAL_TARGET.md, BRIEF_ESPACIAL.md) especifican pero no existe en el proyecto.

Organizado por tipo de creación: **IA generativa** (modelos, texturas, sonidos), **Código** (sistemas de juego), **Shader** (GLSL/HLSL), **Prefab** (montaje en Unity).

---

# PARTE 1 — MODELOS 3D (IA generativa)

Se pueden generar con herramientas como **Meshy.ai**, **Rodin**, **Trellis**, o modelado rápido en **Blender + IA**.

## Props escolares core

### Pupitre Doble Escolar
```
Prompt IA (Meshy/Rodin):
"Low-poly 3D model of a vintage school double desk, wooden tabletop with scratches, green metal tubular legs, no textures, solid colors, PS1/PS2 aesthetic, 300-500 triangles, suitable for Unity"
```
- Archivo destino: `Assets/3D Models/Props/PupitreDoble.fbx`
- Material: `WallMustardMat` (madera) + `WallTealMat` (patas)

### Silla Escolar Modular
```
Prompt IA:
"Low-poly 3D model of a school chair, hard dark blue plastic seat and backrest, thin bent metal legs, late 90s style, no textures, flat colors, PS1 aesthetic, 200-400 triangles"
```
- Archivo destino: `Assets/3D Models/Props/SillaEscolar.fbx`

### Radiador de Fundición
```
Prompt IA:
"Low-poly 3D model of a cast iron radiator, vintage school heater, vertical segments, mounted on wall, rust marks as vertex color, industrial style, 200-300 triangles"
```
- Archivo destino: `Assets/3D Models/Props/Radiador.fbx`

### Estantería de Biblioteca
```
Prompt IA:
"Low-poly 3D model of a tall wooden library bookshelf, 6 shelves, filled with books with plain spines, dark wood color, school library style, PS1 era, 400-600 triangles"
```
- Archivo destino: `Assets/3D Models/Props/Estanteria.fbx`

### Pizarra de Aula
```
Prompt IA:
"Low-poly 3D model of a green chalkboard on wall, wooden frame, slightly worn surface with chalk dust marks, school classroom style, 100-200 triangles"
```
- Archivo destino: `Assets/3D Models/Props/Pizarra.fbx`

### Cartelera Escolar
```
Prompt IA:
"Low-poly 3D model of a school cork bulletin board, wooden frame, pinned papers and notices, some peeling off, corridor wall style, 150-300 triangles"
```
- Archivo destino: `Assets/3D Models/Props/Cartelera.fbx`

### Perchero de Pasillo
```
Prompt IA:
"Low-poly 3D model of a school hallway coat rack, metal bar with individual hooks mounted on wall, one forgotten coat hanging, memory-amber color coat, 200-400 triangles"
```
- Archivo destino: `Assets/3D Models/Props/Perchero.fbx`

## Props decorativos

| Prop | Prompt IA | Archivo |
|------|-----------|---------|
| Reloj pared | "Low-poly 3D model of an analog wall clock, stopped at 3:40, school classroom style, PS1 aesthetic, 100-200 triangles" | `Props/RelojPared.fbx` |
| Extintor | "Low-poly 3D model of a red fire extinguisher on floor, fallen on its side, school maintenance room, 150-250 triangles" | `Props/Extintor.fbx` |
| Casco mantenimiento | "Low-poly 3D model of a yellow hard hat on the floor, construction worker helmet, 100-150 triangles" | `Props/Casco.fbx` |
| Maceta planta seca | "Low-poly 3D model of a dried dead plant in a terracotta pot, school hallway decoration, 150-250 triangles" | `Props/Maceta.fbx` |
| Taza café | "Low-poly 3D model of a ceramic coffee mug, teachers room style, half full with coffee, 100-200 triangles" | `Props/TazaCafe.fbx` |
| Balón fútbol | "Low-poly 3D model of a classic black and white soccer ball, worn, on the ground, 100-200 triangles" | `Props/Balon.fbx` |
| Mochila | "Low-poly 3D model of a school backpack, fabric, one strap hanging, on floor, nostalgic 90s style, 200-300 triangles" | `Props/Mochila.fbx` |
| Paraguas | "Low-poly 3D model of a folded umbrella, forgotten at staircase base, dark blue, 100-200 triangles" | `Props/Paraguas.fbx` |
| Flores secas | "Low-poly 3D model of dried flowers in a glass jar, on windowsill, faded colors, 150-250 triangles" | `Props/FloresSecas.fbx` |
| Carrito conserje | "Low-poly 3D model of a janitor cart with cleaning supplies, bucket and mop, school style, 300-500 triangles" | `Props/CarritoConserje.fbx` |
| Cronómetro | "Low-poly 3D model of a wall-mounted sports stopwatch, stopped, gymnasium style, 100-150 triangles" | `Props/Cronometro.fbx` |

---

# PARTE 2 — TEXTURAS (IA generativa)

Se pueden generar en segundos con **Midjourney, DALL-E, Stable Diffusion, o Clipdrop**. Configurar siempre: salida 128×128 o 256×256, PNG sin compresión, Filter Mode = Point (no bilinear).

| Asset | Prompt | Resolución | Archivo destino |
|-------|--------|-----------|-----------------|
| Madera escolar | "Pixelated 128x128 tileable texture of old school pine wood desk, scratched surface, warm brown, PS1 flat low-res texture, no PBR, solid colors with subtle noise" | 128×128 | `Assets/Textures/tex_school_wood_128.png` |
| Linóleo suelo | "Pixelated 128x128 tileable texture of gray institutional linoleum floor tiles, visible grout lines, slightly dirty, PS1 flat style" | 128×128 | `Assets/Textures/tex_linoleum_floor_128.png` |
| Pizarra verde | "Pixelated 256x256 texture of green chalkboard, partially erased chalk marks, worn surface, school classroom, PS1 lo-fi" | 256×256 | `Assets/Textures/tex_chalkboard_256.png` |
| Yeso pared | "Pixelated 128x128 tileable texture of painted plaster wall, slight rough surface, institutional color, subtle dirt noise, PS1 flat style" | 128×128 | `Assets/Textures/tex_plaster_wall_128.png` |
| Corcho | "Pixelated 128x128 tileable texture of cork board, dense noise pattern with pin holes, warm brown, PS1 low-res" | 128×128 | `Assets/Textures/tex_cork_board_128.png` |

---

# PARTE 3 — SONIDOS (IA generativa + freesound)

Se pueden generar con **ElevenLabs** (voces), **AIVA/Suno** (música/ambientes), o descargar de **freesound.org** con atribución.

### Voces narrativas (ElevenLabs / Voice AI)

| Fragmento | Texto sugerido | Voz | Archivo |
|-----------|---------------|-----|---------|
| Voz Aiden (Nivel 5) | "I should have said something. I just... stood there." | Hombre joven, 17-18 años, tono arrepentido | `Audio/Voz/VozAiden_Fragmento.wav` |
| Voz Lyra (Nivel 10) | "...maybe tomorrow? Or... I don't know. Forget it." | Mujer joven, misma edad, tono inseguro | `Audio/Voz/VozLyra_Fragmento1.wav` |
| Voz Lyra (Nivel 13) | "You never... you never actually listened, did you?" | Mujer, tono dolido pero no enojado | `Audio/Voz/VozLyra_Fragmento2.wav` |

### Efectos de sonido (freesound.org o IA)

| Sonido | Descripción | Fuente sugerida | Archivo |
|--------|-------------|-----------------|---------|
| Tape hiss loop | Ruido de fondo de cinta de cassette magnética | freesound: "cassette hiss loop" | `Audio/SFX/audio_tape_hiss_loop.wav` |
| Zumbido fluorescente | Zumbido eléctrico de tubo fluorescente de techo | freesound: "fluorescent light hum" | `Audio/SFX/fluorescent_hum.wav` |
| Timbre escolar | Timbre de escuela旧, tono electromecánico | freesound: "school bell ring" | `Audio/SFX/school_bell.wav` |
| Pisadas linóleo | Pasos sobre linóleo escolar | freesound: "footsteps on linoleum" | `Audio/SFX/footsteps_linoleum.wav` |
| Pisadas eco distorsionadas | Misma pisada con filtro bandpass + hiss | Procesar pisada linóleo con audacity | `Audio/SFX/footsteps_echo_distorted.wav` |
| VHS mecanismo | Clics de cassette, rebobinado, play | freesound: "vhs tape clicks mechanical" | `Audio/SFX/tape_mechanical.wav` |
| Ambiente aula vacía | Silencio de aula sin nadie, leve resonancia | freesound: "empty classroom ambience" | `Audio/SFX/classroom_ambient.wav` |

Para generar pisadas distorsionadas del eco, procesar el archivo base con:
```
Audacity: Efecto > Filtro de paso banda (300Hz-4000Hz)
         + Pista > Generar > Ruido (tape hiss, mezclar al 15%)
         + Efecto > Distorsión suave
```

---

# PARTE 4 — DECALS (IA + Unity)

Los decals deben ser **planos poligonales** (no projection system). Se crean como un quad con textura transparente.

| Decal | Textura prompt (128×128) | Archivo prefab |
|-------|-------------------------|----------------|
| Humedad | "Pixel art moisture stain on concrete wall corner, dark water damage shape, transparent PNG, 128x128" | `Prefabs/Decals/dec_moisture_lines.prefab` |
| Notas Lyra | "Pixel art chalk drawing on green board, two stick figures one taller, heart shape, rough sketch style, 256x128" | `Prefabs/Decals/dec_lyra_notes.prefab` |
| Arrastre sillas | "Pixel art black scuff marks on gray linoleum floor, chair leg drag trails, semi-transparent, 128x128" | `Prefabs/Decals/dec_floor_drag.prefab` |
| Grietas | "Pixel art black crack lines on wall, polygonal jagged fracture shape, transparent PNG, PS1 style, 128x128" | `Prefabs/Decals/dec_crack_liminal.prefab` |

---

# PARTE 5 — SHADERS (código HLSL/GLSL)

No se generan con IA de imágenes — se escriben en código. Sin embargo, se puede usar **Claude/GPT-4 para escribir shaders URP** con prompts como:

```
Escribe un shader URP HLSL para Unity. 
- Nombre: RetroFlatLit
- Propiedades: Albedo color, sin texturas
- Metallic = 0, Smoothness = 0.05
- Sin normal maps, sin AO
- Sombreado plano (flat shading via vertex snapping opcional)
- Compatible con SRP Batcher
```

```
Escribe un shader URP HLSL para Unity.
- Nombre: AnalogGhost
- Propósito: renderizar el eco del jugador
- Alpha dithered (patron 4x4 Bayer), no alpha blending suave
- Emisión cian (#4FC3E8)
- Frame rate limitado a 15 FPS en animación de vértices
- Scanlines overlay sutiles
- Transparencia con dither, no blending
```

```
Escribe un shader URP HLSL para Unity.
- Nombre: LiminalFog
- Propósito: niebla lineal agresiva que corta geometría
- Sin niebla volumétrica
- Color sólido, corte abrupto a 15-20 metros
- Sin gradientes suaves
```

---

# PARTE 6 — SISTEMAS DE JUEGO (solo código C#)

| Sistema | Descripción | Archivo destino |
|---------|-------------|-----------------|
| Eco Ambiental Lyra | Script que controla un eco no interactivo con ruta fija, activa plataformas invisibles en radio 2m | `Assets/Scripts/LyraAmbientEcho.cs` |
| Degradación Eco | Sistema que reduce fidelidad visual y añade desincronización 0.1-0.3s por uso repetido | Extension en `EchoPlayback.cs` |
| Estado Residual 2.5s | Cambiar FadeOutAndDestroy de 0.55s → 2.5s con colisión activa y opacidad decreciente | `EchoPlayback.cs` |
| Latencia 0.8s | Añadir delay fijo entre activación del eco y primer movimiento | `EchoPlayback.cs` |
| Grabación Limitada | maxRecordSeconds configurable por nivel desde LevelBlueprint | `EchoRecorder.cs` + `LevelBlueprint.cs` |
| Eco Pregrabado | Grabación que se reproduce sola, no se puede sobrescribir | `Assets/Scripts/EchoPregrabado.cs` |
| Inversión | Nivel 14: Aiden sigue al eco. Sin grabación. Cámara frontal simétrica. | Modo en `EchoRecorder.cs` |
| Sistema Capítulos | ScriptableObject que define qué mecánicas están permitidas en cada capítulo | `Assets/Scripts/CapituloRules.cs` |
| FixedPuzzleCamera | Cámara fija con posición/FOV por puzzle según tabla de Biblia §8 | `Assets/Scripts/FixedPuzzleCameraController.cs` (existe? verificar) |

---

# PARTE 7 — PREFABS (montaje en Unity)

Los prefabs se ensamblan en Unity a partir de los modelos 3D + materiales existentes.

| Prefab | Modelo base | Materiales | Ubicación |
|--------|-------------|------------|-----------|
| PupitreDoble | `PupitreDoble.fbx` | `WallMustardMat` (madera), `WallTealMat` (metal) | `Assets/Prefabs/Props/PupitreDoble.prefab` |
| SillaEscolar | `SillaEscolar.fbx` | `WallTealMat` (asiento), `WallSageMat` (patas) | `Assets/Prefabs/Props/SillaEscolar.prefab` |
| Radiador | `Radiador.fbx` | `ArchMat` (metal) | `Assets/Prefabs/Props/Radiador.prefab` |
| Estanteria | `Estanteria.fbx` | `WallSageMat` + `MemoryMat` (detalle) | `Assets/Prefabs/Props/Estanteria.prefab` |
| Pizarra | `Pizarra.fbx` | `WallSageMat` (verde pizarra) | `Assets/Prefabs/Props/Pizarra.prefab` |
| Cartelera | `Cartelera.fbx` | `WallMustardMat` (marco) | `Assets/Prefabs/Props/Cartelera.prefab` |
| Perchero | `Perchero.fbx` | `ArchMat` (metal) | `Assets/Prefabs/Props/Perchero.prefab` |
| MesaProfesor | Si se necesita: escritorio de profesor | `WallMustardMat` | `Assets/Prefabs/Props/MesaProfesor.prefab` |

Cada decal es un prefab: Quad + Material con textura transparente + `FilterMode = Point`.

---

# RESUMEN: QUÉ PEDIR A LA IA

## En orden de prioridad para la Vertical Slice:

| # | Tipo | Cantidad | Herramienta IA recomendada |
|---|------|----------|---------------------------|
| 1 | **Modelos core** | 7 props (pupitre, silla, radiador, estantería, pizarra, cartelera, perchero) | Meshy.ai / Trellis / Rodin |
| 2 | **Texturas** | 5 texturas 128×128 | Midjourney / DALL-E / Stable Diffusion |
| 3 | **Sonidos** | 7 SFX + 3 voces | freesound.org + ElevenLabs |
| 4 | **Decals** | 4 texturas + 4 prefabs Quad | DALL-E (texturas) + Unity (prefabs) |
| 5 | **Props decorativos** | 11 props pequeños | Meshy.ai (batch) |
| 6 | **Shaders** | 3 shaders URP | Claude o GPT-4 (código HLSL) |
| 7 | **Sistemas código** | 9 sistemas C# | Claude o GPT-4 (código C#) |

## Prompt general para Meshy.ai / Trellis (generar todos los props de una vez):

```
Generate low-poly 3D models in FBX format with the following specifications:
- PS1 / early PS2 aesthetic
- Under 500 triangles per model
- No textures required (solid vertex colors or flat materials)
- Pivot at base center
- Scale: 1 unit = 1 meter
- Compatible with Unity URP

Models needed:
1. School desk: double wooden tabletop, green metal legs, scratched surface
2. School chair: blue plastic seat, thin metal legs
3. Cast iron radiator: wall-mounted, vertical segments
4. Library bookshelf: tall, 6 shelves, dark wood, books with plain spines
5. Green chalkboard: wooden frame, chalk marks
6. Cork bulletin board: wooden frame, pinned papers
7. Hallway coat rack: wall bar with hooks, one coat hanging
8. Analog wall clock: stopped at 3:40
9. Fire extinguisher: red, fallen on side
10. Yellow hard hat: on floor
11. Ceramic coffee mug: half full
12. Soccer ball: classic black and white, worn
13. School backpack: fabric, one strap
14. Folded umbrella: dark blue
15. Janitor cart: bucket, mop, cleaning supplies
16. Sports stopwatch: wall-mounted, stopped
```
---

*Documento generado por auditoría — Julio 2026*
