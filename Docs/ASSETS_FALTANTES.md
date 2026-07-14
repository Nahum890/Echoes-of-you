# ASSETS FALTANTES
## Inventario de assets requeridos por los documentos de diseño que NO existen en el proyecto
### Fuente: ECHOES_BIBLE.md · VISUAL_TARGET.md · BRIEF_ESPACIAL.md
### Generado: Julio 2026

---

## SHADERS (VISUAL_TARGET.md §4, §19)

| Asset | Documento | Prioridad |
|-------|-----------|-----------|
| `RetroFlatLit.shader` | VISUAL_TARGET §4 — Shader maestro para geometría arquitectónica | 🔴 Alta |
| `AnalogGhost.shader` | VISUAL_TARGET §4 — Shader del eco con dither transparency, 15 FPS, scanlines | 🔴 Alta |
| `LiminalFog.shader` | VISUAL_TARGET §4 — Niebla lineal agresiva que corta geometría a 15-20m | 🔴 Alta |

---

## TEXTURAS LO-FI (VISUAL_TARGET.md §5, §19)

| Asset | Resolución | Uso | Prioridad |
|-------|-----------|-----|-----------|
| `tex_school_wood_128.png` | 128×128 | Madera escolar para pupitres, sillas, estanterías | 🟠 Media |
| `tex_linoleum_floor_128.png` | 128×128 | Baldosas de linóleo gris con juntas marcadas | 🟠 Media |
| `tex_chalkboard_256.png` | 256×256 | Pizarra verde desgastada con restos de tiza | 🟠 Media |
| `tex_plaster_wall_128.png` | 128×128 | Yeso rugoso pintado con patrón de suciedad | 🟡 Baja |
| `tex_cork_board_128.png` | 128×128 | Corcho para carteleras | 🟡 Baja |

---

## DECALS (VISUAL_TARGET.md §6, §19)

| Prefab | Descripción | Uso | Prioridad |
|--------|-------------|-----|-----------|
| `dec_moisture_lines.prefab` | Humedad analógica en esquinas de techos | Capítulos I-VI | 🟠 Media |
| `dec_lyra_notes.prefab` | Dibujos/palabras de Lyra en pizarras | Capítulo IV (Niveles 10, 13) | 🟠 Media |
| `dec_floor_drag.prefab` | Marcas de arrastre de sillas en linóleo | Guía visual sutil en aulas | 🟡 Baja |
| `dec_crack_liminal.prefab` | Grietas poligonales en paredes | Capítulo III+ (fragmentación) | 🟡 Baja |

---

## PROPS ESCOLARES (VISUAL_TARGET.md §7, §19)

| Prefab | Descripción | Prioridad |
|--------|-------------|-----------|
| `PupitreDobleEscolar.prefab` | Madera contrachapada rayada + patas metálicas verdes | 🔴 Alta |
| `SillaEscolarModular.prefab` | Plástico duro azul/verde + patas metal doblado | 🔴 Alta |
| `RadiadorFundicion.prefab` | Radiador de calefacción antiguo con óxido | 🟠 Media |
| `EstanteriaBiblioteca.prefab` | Estantes de madera oscura con libros sin lomos | 🟠 Media |
| `PizarraAula.prefab` | Pizarra de tiza con marco de madera | 🟠 Media |
| `CarteleraEscolar.prefab` | Marco de madera + fondo de corcho + avisos | 🟡 Baja |
| `PercheroPasillo.prefab` | Barra metálica con ganchos, un abrigo `memory-amber` olvidado | 🟡 Baja |

---

## SYSTEMAS DE JUEGO (ECHOES_BIBLE.md §5-7)

| Sistema | Descripción | Capítulo | Prioridad |
|---------|-------------|----------|-----------|
| `EchoAmbientalLyra` | Eco no controlable de Lyra que revela caminos invisibles | IV (Nivel 10) | 🔴 Alta |
| `DegradacionEco` | Eco pierde fidelidad visual con uso repetido (desincronización 0.1-0.3s) | III (Nivel 6) | 🔴 Alta |
| `EstadoResidual` | Eco permanece 2.5s después de reproducción (hoy: 0.55s) | Todos | 🔴 Alta |
| `LatenciaInicio` | Eco tiene demora fija de 0.8s al activarse (hoy: no existe) | Todos | 🔴 Alta |
| `GrabacionLimitada` | maxRecordSeconds configurable por nivel (hoy: 6s fijo, debería ser 20s) | IV (Nivel 11) | 🔴 Alta |
| `EcoPregrabado` | Grabación que existe sin que el jugador la hiciera | V (Nivel 13) | 🟠 Media |
| `InversionEco` | Aiden sigue al eco en lugar de grabarlo | VI (Nivel 14) | 🟠 Media |
| `SistemaCapitulos` | Reglas de mecánicas permitidas/prohibidas por capítulo | Todos | 🟠 Media |

---

## AUDIO (VISUAL_TARGET.md §12, §19)

| Asset | Descripción | Prioridad |
|-------|-------------|-----------|
| `audio_tape_hiss_loop.wav` | Ruido de cinta magnética para grabaciones de eco | 🟠 Media |
| `FiltroBandpass_Eco` | Filtro DSP que distorsiona pasos del eco (bandpass + tape hiss) | 🟠 Media |
| `SchoolRoom_Ambient.wav` | Ambiente de aula vacía con zumbido de fluorescentes | 🟠 Media |
| `VozLyra_Fragmento*.wav` | Fragmentos de voz de Lyra (filtrados, incompletos) para Niveles 10 y 13 | 🟠 Media |
| `VozAiden_Fragmento.wav` | Fragmento de voz de Aiden para Nivel 5 | 🟡 Baja |
| `TimbreEscolar.wav` | Timbre de escuela para transiciones | 🟡 Baja |
| `PisadasLinoleo.wav` | Pasos sobre linóleo escolar (presente y eco distorsionado) | 🟡 Baja |

---

## SISTEMAS DE CÁMARA (ECHOES_BIBLE.md §8)

| Sistema | Descripción | Prioridad |
|---------|-------------|-----------|
| `FixedPuzzleCameraController` | Cámara fija para composiciones específicas de puzzle | 🟠 Media |
| `ConfigCamaraPorNivel` | Sistema que aplique offsets y FOV según tabla de la Biblia (línea 283-290) | 🟠 Media |

---

## SISTEMAS NARRATIVOS (ECHOES_BIBLE.md §9)

| Sistema | Descripción | Prioridad |
|---------|-------------|-----------|
| `EnvironmentalStorytelling` | Sistema para colocar props narrativos (cuaderno, abrigo, fotografía, tazas) por nivel | 🟠 Media |
| `CreditosEnPared` | Sistema que proyecta créditos en geometría del Nivel 15 | 🟡 Baja |

---

## PIPELINE / HERRAMIENTAS (PROJECT_CONTEXT.md, VISUAL_TARGET.md)

| Asset | Descripción | Prioridad |
|-------|-------------|-----------|
| `EchoesEditorUtility.cs` | Helper con SetSerializedValue centralizado (hoy duplicado en 4 archivos) | 🟡 Baja |
| `SettingsManager` | ScriptableObject compartido para settings de UI (hoy duplicado en 3 lugares) | 🟡 Baja |
| `AtlasTexturas` | Atlas de texturas para batching de materiales escolares | 🟡 Baja |

---

## PROPS NARRATIVOS POR NIVEL (BRIEF_ESPACIAL.md)

| Nivel | Objeto Narrativo | Estado |
|-------|-----------------|--------|
| 1 | Abrigo `memory-amber` en locker | ❌ No existe |
| 1 | Cuaderno caído en pasillo | ❌ No existe |
| 2 | Fotografía femenina en cartelera | ❌ No existe |
| 2 | Cuaderno abierto en escritorio profesor | ❌ No existe |
| 3 | Objeto `memory-amber` al fondo del pasillo Lyra | ❌ No existe |
| 4 | Reloj de pared detenido a las 15:40 | ❌ No existe |
| 4 | Nota adhesiva en escritorio | ❌ No existe |
| 5 | Extintor caído | ❌ No existe |
| 5 | Casco de mantenimiento en suelo | ❌ No existe |
| 5 | Fotografía escolar parcialmente arrancada | ❌ No existe |
| 6 | Libro abierto con páginas en blanco | ❌ No existe |
| 6 | Sello de biblioteca en mostrador | ❌ No existe |
| 7 | Letra caída en cartel "SALIDA DE EMERGENCIA" | ❌ No existe |
| 7 | Balón de fútbol en rincón | ❌ No existe |
| 8 | 2 tazas de café (una con café, una vacía) | ❌ No existe |
| 8 | Lista de asistencia con nombres borrados | ❌ No existe |
| 9 | Carrito de conserje abandonado | ❌ No existe |
| 9 | Aro de baloncesto sin red | ❌ No existe |
| 9 | Pintada de tiza en suelo | ❌ No existe |
| 10 | Mochila debajo de pupitre `memory-amber` | ❌ No existe |
| 10 | Dibujo en pizarra: dos siluetas | ❌ No existe |
| 10 | Flores secas en vaso en repisa | ❌ No existe |
| 11 | Fotografía clavada en rellano | ❌ No existe |
| 11 | Paraguas olvidado en base escalera | ❌ No existe |
| 12 | Cronómetro detenido en pared | ❌ No existe |
| 12 | Listado récords: "Aiden — 2003" | ❌ No existe |
| 13 | Pupitre `memory-amber` volcado | ❌ No existe |
| 13 | Mochila del Nivel 10 en centro del aula | ❌ No existe |
| 15 | Abrigo del Nivel 1 ya no está | ❌ No existe (estado cambiado) |
| 15 | Cuaderno cerrado apoyado contra pared | ❌ No existe |

**Total props narrativos faltantes: 30**

---

## ARCHIVOS DE CÓDIGO ARCHIVADOS

Los siguientes archivos fueron movidos a `_ARCHIVE/DeadCode/` por ser código legacy que ya no pertenece al pipeline activo:

| Archivo | Razón |
|---------|-------|
| `EchoesProductionBuilder.cs` | 2403 líneas de dead code SciFi envuelto en `#if false` |
| `MainMenu.cs` | Menú uGUI legacy reemplazado por `MainMenuController` (UITK) |
| `Animated Woman/` | Modelo de personaje no utilizado en la escuela |
| `freesound_community-sci-fi-charge-up-37395.mp3` | Efecto de sonido sci-fi prohibido por VISUAL_TARGET.md |

---

*Documento generado por auditoría automática. Los assets listados aquí son los que los documentos de diseño especifican pero no existen en el proyecto actual.*
