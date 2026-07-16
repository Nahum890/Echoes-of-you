# ASSET ORGANIZATION — Echoes of You

Assets importados de `Nuevos Assets/` reorganizados en la jerarquía de Unity.
Fecha: Julio 2026

---

## MODELOS 3D (`Assets/3D Models/Props/`)

### AulaCompleta/
| Archivo | Asset |
|---------|-------|
| `Aula.fbx` | Aula completa con mobiliario (Maya/C4D export) |

### Pupitres/ (todos .blend en _Source/)
| Archivo | Asset |
|---------|-------|
| `Desk.blend` | Pupitre individual |
| `Long_Desk.blend` | Pupitre largo (doble) |
| `Table.blend` | Mesa genérica |
| `Table_2.blend` | Mesa variante 2 |
| `Teachers_Desk.blend` | Escritorio de profesor |

### Sillas/ (todos .blend en _Source/)
| Archivo | Asset |
|---------|-------|
| `Chair_1.blend` ~ `Chair_4.blend` | 4 variantes de silla escolar |
| `Stool_1.blend`, `Stool_2.blend` | 2 taburetes |

### Pizarras/
| Archivo | Asset |
|---------|-------|
| `chalkboard_clean.blend` | Pizarra limpia |
| `chalkboard_drawn.blend` | Pizarra con dibujos |

### Biblioteca/
| Archivo | Asset |
|---------|-------|
| `Bookshelf.blend` | Estantería librería |
| `Cubby.blend` | Casillero abierto |
| `Empty_Cubby.blend` | Casillero vacío |
| `Empty_Shelf.blend` | Estante vacío |

### Decoracion/ (todos .blend en _Source/)
| Archivo | Asset | BRIEF_ESPACIAL reference |
|---------|-------|--------------------------|
| `Book.blend` | Libro abierto | Niveles 6, 10 |
| `Book_Closed.blend` | Libro cerrado | Nivel 15 |
| `Book_Stack.blend` | Pila de libros | - |
| `Book_Stack_2.blend` | Pila de libros 2 | - |
| `Book_with_Pencil.blend` | Libro con lápiz | Nivel 6 |
| `Comic_Book.blend` | Cómic | - |
| `Paper_Pile.blend` ~ `Paper_Pile_3.blend` | Pilas de papel | Nivel 8 |
| `Paper_stack.blend` ~ `Paper_stack_4.blend` | Pilas de papel | - |
| `Homework_Paper.blend` | Tarea escolar | - |
| `Doodle_Paper.blend` | Papel con garabatos | Nivel 3 (Lyra) |
| `Blank_Paper.blend` | Papel en blanco | - |
| `Paper_with_Pencil.blend` | Papel con lápiz | - |
| `Pen.blend` | Bolígrafo | - |
| `Pencil.blend` | Lápiz | - |
| `Pencil_Jar.blend` | Bote de lápices | - |
| `Eraser.blend` | Goma de borrar | - |
| `Jar_Stack.blend`, `Jar_Stack_2.blend` | Frascos apilados | - |

### Exterior/
| Archivo | Asset | Nivel |
|---------|-------|-------|
| `CleaningCart.fbx` | Carrito de conserje | Nivel 9 |
| `SoccerBall.fbx` | Balón de fútbol | Nivel 7 |

### Varios/
| Archivo | Asset | Nivel |
|---------|-------|-------|
| `Bag.fbx` | Mochila escolar | Nivel 10, 13 |
| `BC_hard_hat.obj` | Casco de mantenimiento | Nivel 5 |
| `Extintor.gltf` | Extintor | Nivel 5 |
| `Stopwatch.fbx` | Cronómetro deportivo | Nivel 12 |
| `Paraguas.obj` | Paraguas plegado | Nivel 11 |
| `wall_clock.obj` | Reloj de pared analógico | Nivel 4 |
| `Mug_low-poly.zip` | Taza de café | Nivel 8 |
| `TerracottaPots.zip` | Macetas terracota | Nivel 11 |
| `BotonNivel.fbx` | Botón de selección de niveles (menú principal) | MainMenu |

---

## TEXTURAS

### `Assets/Textures/School/`
| Archivo | Descripción |
|---------|-------------|
| `tex_school_wood_128.png` | Madera escolar para pupitres |
| `tex_linoleum_floor_128.png` | Linóleo gris con juntas |
| `tex_chalkboard_256.png` | Pizarra verde con tiza |
| `tex_plaster_wall_128.png` | Yeso rugoso pintado |
| `tex_cork_board_128.png` | Corcho para carteleras |

### `Assets/Textures/Decals/`
| Archivo | Descripción |
|---------|-------------|
| `dec_floor_drag.png` | Marcas de arrastre en suelo |
| `dec_moisture_lines.png` | Manchas de humedad en pared |

### `Assets/Textures/Props/`
Texturas PBR de los modelos descargados (CleaningTools, Plant_Pots, Aula, Extintor, Casco, Balón, Reloj).

---

## AUDIO (`Assets/Efectos de sonido/`)

| Archivo | Descripción |
|---------|-------------|
| `setps.mp3` | Pasos (footsteps) |
| `...fluorescent-light-turn-on-hum...flac` | Zumbido fluorescente |
| `...worn-video-cassette-audio-hiss...wav` | Ruido de cinta VHS/cassette |
| `...vhs-tape-clicks...flac` | Clics mecánicos de cassette |
| `...old-fashioned-school-telephone-bell...wav` | Timbre escolar |
| `...college-hallway-ambience...wav` | Ambiente de pasillo escolar |

---

## BLENDER SOURCE FILES (`Assets/3D Models/_Source/`)

Archivos .blend originales. Unity los importa automáticamente si Blender está instalado. Para exportar a FBX: `Archivo > Exportar > FBX (.fbx)`.

---

## PREFABS PENDIENTES

Los siguientes prefabs deben crearse en Unity desde los modelos:

| Prefab | Modelo(s) | Material |
|--------|-----------|----------|
| `Props/PupitreDoble.prefab` | `Long_Desk.blend` | `WallMustardMat` |
| `Props/PupitreSimple.prefab` | `Desk.blend` | `WallMustardMat` |
| `Props/SillaEscolar.prefab` | `Chair_1.blend` | `WallTealMat` |
| `Props/EscritorioProfesor.prefab` | `Teachers_Desk.blend` | `WallMustardMat` |
| `Props/Pizarra.prefab` | `chalkboard_clean.blend` | `WallSageMat` |
| `Props/Estanteria.prefab` | `Bookshelf.blend` | `MemoryMat` |
| `Props/Cartelera.prefab` | `Empty_Shelf.blend` | `WallMustardMat` |
| `Props/Mochila.prefab` | `Bag.fbx` | `MemoryMat` |
| `Props/Balon.prefab` | `SoccerBall.fbx` | `PlateMat` |
| `Props/RelojPared.prefab` | `wall_clock.obj` | `WallMustardMat` |
| `Decals/dec_moisture_lines.prefab` | Quad + texture | decal material |
| `Decals/dec_floor_drag.prefab` | Quad + texture | decal material |

---

## PRÓXIMOS PASOS

1. Abrir Unity, los modelos se importarán automáticamente
2. Configurar `Filter Mode = Point (no filter)` en todas las texturas School y Decals
3. Configurar texturas a 128×128 si no lo están
4. Crear prefabs desde los modelos FBX/OBJ importados
5. Asignar materiales existentes (EchoesMaterialLibrary) a cada prefab
6. Los modelos PBR (CleaningTools, PlantPots) — decidir si usar flat materials o mantener PBR
7. El modelo Meshy AI — identificar qué es y renombrar
