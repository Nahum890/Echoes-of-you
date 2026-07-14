# AUDITORÍA COMPLETA — Echoes of You
## Equipo de Revisión AAA
### Fecha: Julio 2026

---

# SCORE GENERAL

| Categoría | Puntaje /10 | Estado |
|-----------|:-----------:|--------|
| Arquitectura | 4.5 | Deuda técnica severa por migración visual |
| Gameplay | 5.5 | Mecánica central prometedora, ejecución incompleta |
| Puzzle Design | 3.5 | No hay puzzles jugables verificados, solo infraestructura |
| Arte | 3.0 | Dirección artística excelente en docs, ejecución 0% |
| Narrativa | 6.0 | Biblia sólida, implementación narrativa no verificable |
| UI / UX | 2.5 | Dos sistemas peleándose, terminología sci-fi, bugs masivos |
| Audio | 4.0 | Sistemas funcionales, assets genéricos, sin identidad |
| Código | 4.0 | 2403 líneas de dead code, duplicación masiva, sin tests |
| Optimización | 3.5 | FindObjectOfType per-frame, sin pooling, GC pressure |
| Originalidad | 7.5 | Lo más valioso del proyecto — la mecánica del eco es genuina |
| Potencial Comercial | 5.0 | Concepto fuerte, ejecución actual no comercializable |

**Promedio General: 4.4 / 10**

---

# 1 — ARQUITECTURA (4.5/10)

## God Objects y Monolitos

### CRÍTICO: `EchoesProductionBuilder.cs` — 2403 líneas de código muerto
Archivo envuelto en `#if false // LEGACY` que contiene:
- 25+ rutas hardcodeadas a Modular SciFi MegaKit
- 15 funciones `BuildLevel01()`..`BuildLevel15()` duplicadas
- Sistemas completos de creación de geometría, luces, niebla
- Referencias a SciFiColumnAstra, SciFiWallBand, SciFiPropCrate
- Constantes como `SciFiRoot = "Assets/3D Models/Modular SciFi MegaKit[Standard]/..."`

**Debe eliminarse.** Su presencia confunde a cualquier IA o desarrollador nuevo y dobla el tamaño del repositorio innecesariamente. Ya existe `EchoesNewProductionBuilder.cs` (493 líneas) como reemplazo activo.

### CRÍTICO: `GameFeelController.cs` — 704 líneas, todo en una clase
Monolito que maneja:
- Partículas (dust motes)
- Audio (one-shots, loops, ambientes)
- Cámara (shake, FOV pulse)
- Slow-motion
- Post-procesado (modulación de perfil de volumen)
- Footsteps, landing, scrape

**Problemas específicos:**
- `FindAnyObjectByType<EchoRecorder>()` llamado **2 veces por frame** en Update (líneas 221, 223)
- `FindAnyObjectByType<AudioListener>()` llamado cada frame (línea 221)
- `new GameObject("OneShotAudio")` sin pooling — cada paso, cada salto, cada interacción crea un GO temporario
- `RequestCameraPulse()` es un **no-op completo** (línea 518: `return;` inmediato)
- `_slowMotionTarget`, `_fovPulseTimer`, `_fovPulseDuration` son serializados pero **nunca se usan**
- No hay separación de responsabilidades

### ALTO: `MainMenuController.cs` — 1181 líneas
Menú principal que también maneja settings, hover effects, navegación, estadísticas, terminal log, dashboard. Debería ser 3-4 clases separadas.

## Duplicación Masiva de Código

### `SetSerializedValue` — 4 copias con firmas diferentes
| Ubicación | Líneas | Soporta arrays | Non-Unity reflection |
|-----------|--------|----------------|---------------------|
| `EchoesNewProductionBuilder.cs` | 421-467 | Sí | No |
| `EchoesModuleFactory.cs` | 1137-1181 | No | Sí |
| `EchoesLevelShell.cs` | 463-498 | No | No |
| `EchoesProductionBuilder.cs` (muerto) | 2293-2329 | No | No |

**Solución:** Crear `EchoesEditorUtility.cs` con una sola implementación.

### `SetupTransparentMaterial` — 3 copias
`EchoesMaterialLibrary:181`, `EchoesModuleFactory:1107`, `EchoesProductionBuilder` (muerto).

### Settings UI — 230 líneas × 3
`MainMenuController:531-760`, `PauseMenu:318-550`, `MainMenu:134-183`. La misma lógica de sliders, presets, resolución, aplicado de settings, con diferencias sutiles que garantizan bugs.

### Lógica de detección de overlap — 4 copias casi idénticas
`EchoKineticBody:67-105`, `EchoShieldField:51-106`, `EchoConflictTrap:39-77`, `PressurePlate` (variante).

## Dependencias Frágiles

- `GameObject.Find(targetName)` en `EchoesNewProductionBuilder:378` — O(n), frágil ante renames
- `Motor.GetType().GetMethod("Configure").Invoke(...)` en `ModuleFactory:1356` — reflection frágil
- `EnsureOptionalComponent(string typeName)` en 3 archivos — reflection por string, falla silenciosamente si la clase cambia de assembly
- `CinemachineRuntimeSetup` usa reflection masiva para todo — incompatible con Cinemachine 3.x

## Nombres Inconsistentes

- Código en español e inglés mezclado (comentarios en español, variables/código en inglés)
- `EchoShieldField` — terminología sci-fi para un juego escolar
- `MemoryCorridor` vs `Memoria` — mezcla de idiomas
- `GetChapterColor` con comentarios en español
- `Facade001_1K-JPG` — nomenclatura PBR en proyecto que PROHÍBE texturas PBR

## Riesgos Futuros

- **Builder único no es suficiente:** Si `EchoesNewProductionBuilder` se corrompe, no hay Plan B
- **Sin tests automatizados:** 0 tests. Cualquier refactor es a ciegas
- **Sin CI/CD:** No hay pipeline de integración continua
- **PlayerPrefs como único sistema de guardado:** Sin slots, sin async I/O, sin cifrado
- **No hay sistema de logging:** Solo `Debug.Log` que no persiste

---

# 2 — GAMEPLAY (5.5/10)

## La mecánica del Eco — ¿Sostiene el juego?

**Sí, conceptualmente.** La idea de grabar movimientos y convivir con el resultado irreversible es genuinamente original y emocionalmente potente. Pero:

### Problemas Fundamentales

1. **maxRecordSeconds = 6 en EchoRecorder.cs, pero la Biblia dice 20 segundos.**
   - `EchoRecorder.cs:16`: `[SerializeField] float maxRecordSeconds = 6f;`
   - `ECHOES_BIBLE.md:107`: "El eco es la grabación de los movimientos del jugador (hasta 20 segundos)"
   - El timeline del HUD tiene `totalSecs = 10f` hardcodeado (BUG-23)
   - **Hay 3 valores distintos para el mismo concepto: 6, 10, 20.** Una inconsistencia crítica.

2. **No hay puzzles jugables verificables.** Todo el sistema de puzzle es infraestructura (placas, puertas, condiciones). Los niveles existen como escenas Unity generadas por builder, pero:
   - Los blueprints existen como ScriptableObjects
   - No hay manera de inspeccionar un puzzle completo desde el código
   - Los niveles no tienen diseño de puzzle validado — solo tienen geometría
   - **NO PUDO VERIFICARSE** que ningún nivel tenga puzzles que cumplan la "prueba del botón" de la Biblia

3. **SoftReset con Q como deshacer global.**
   - `LevelRuntimeController.SoftReset()` atado a la tecla Q
   - Limpia todos los ecos instantáneamente
   - La Biblia dice: "Una grabación reproducida no puede deshacerse excepto re-grabando desde cero"
   - Tener un hotkey que hace undo global contradice EL TEMA CENTRAL DEL JUEGO

4. **No se respeta la "Regla de una idea" (Portal Clarity Rule).**
   - Los 15 niveles tienen múltiples mecánicas según la Biblia
   - Pero los blueprints y el builder no diferencian capítulos
   - No hay validación de que un nivel introduzca solo UNA idea

5. **La irreversibilidad no está forzada por el código.**
   - `EchoRecorder.ClearAllEchoes()` se llama desde `LevelRuntimeController.SoftReset()`
   - No hay costo por limpiar ecos
   - No hay castigo por mal uso del eco (más allá de recargar escena)
   - Los niveles del Capítulo I tienen "consecuencias negativas" disponibles (EchoConflictTrap, EchoDisintegrationZone) cuando la Biblia las prohíbe explícitamente

6. **maxEchoes = 2 pero no hay progresión.**
   - `EchoRecorder.cs:15`: `[SerializeField] int maxEchoes = 2;`
   - Todos los niveles tienen el mismo límite de ecos
   - La Biblia describe niveles con `maxEchos = 0` (Nivel 1, Nivel 14), `maxEchos = 1` etc.
   - **No se respeta la progresión de la campaña**

### La voz grabada — sistema frágil

`EchoRecorder` tiene un sistema de captura de voz por micrófono que:
- Depende de `Microphone.devices` (solo funciona en Runtime real, no en Editor)
- Crea `AudioClip.Create` por cada grabación — GC allocation masiva
- No tiene fallback para cuando no hay micrófono (el eco se crea sin voz, OK, pero no hay síntesis)
- **Narrativamente crítico:** Los 3 momentos de voz (Niveles 5, 10, 13) dependen de que el jugador hable en el micrófono. Si el jugador no habla, el momento narrativo se pierde.

---

# 3 — DISEÑO DE PUZZLES (3.5/10)

## Comparativa contra referentes

| Principio | Portal | Witness | Braid | Cocoon | Echoes (actual) |
|-----------|--------|---------|-------|--------|-----------------|
| Una idea por nivel | ✅ | ✅ | ✅ | ✅ | ❌ No verificado |
| Sin tutorial texto | ✅ | ✅ | ✅ | ✅ | ❌ TutorialHUD existe pero no se usa |
| Fracaso legible | ✅ | ✅ | ✅ | ✅ | ❌ Las placas/puertas no comunican error |
| Solución única elegante | ✅ | ❌ | ✅ | ✅ | ❌ No verificado |
| Momento AHA | ✅ | ✅ | ✅ | ✅ | ❌ No verificado |
| Rejugabilidad | ✅ | ✅ | ❌ | ❌ | ❌ No diseñado |
| Exploración como puzzle | ❌ | ✅ | ❌ | ✅ | ✅ En la Biblia, no en código |

## Lo que falta

1. **No hay progresión de dificultad verificable.** Los 15 niveles existen como escenas pero el diseño de puzzles no puede auditarse desde el código. Los blueprints son listas de módulos, no descripciones de puzzles.

2. **La "Prueba del Botón" no se puede validar.** La Biblia dice: "¿Puede este puzzle resolverse exactamente igual con una caja empujable genérica en lugar del eco?" Si la respuesta es sí, el puzzle no pertenece al juego. No hay manera de verificar esto desde la arquitectura actual.

3. **Mecánicas prometidas que NO PUDIERON VERIFICARSE:**
   - Puente espectral (GhostBridge existe como script de 93 líneas pero ¿está implementado en niveles?)
   - Grabación limitada (maxRecordSeconds es 6, no 20)
   - Eco ambiental de Lyra (no hay script LyraEcho)
   - Dos ecos contradictorios (EchoConflictTrap existe, ¿pero cableado?)
   - Inversión (Nivel 14, Aiden sigue al eco — no hay sistema que lo soporte)
   - Degradación del eco con uso repetido (mencionado en Biblia, no implementado)
   - Latencia de 0.8s (mencionado en Biblia, EchoPlayback no lo implementa)
   - Estado residual de 2.5s (mencionado en Biblia, FadeOutAndDestroy usa 0.55s por defecto)

4. **Curva de dificultad plana.** Todos los niveles tienen `maxEchoes = 2`, `maxRecordSeconds = 6`, mismos sistemas de puzzle disponibles. No hay sensación de progresión mecánica.

---

# 4 — DIRECCIÓN ARTÍSTICA (3.0/10)

## Brecha entre documento y ejecución

ECHOES_BIBLE.md y VISUAL_TARGET.md son documentos de dirección artística EXCELENTES. El problema es que **no se han ejecutado**.

### SHADERS: No existen (CRÍTICO)

`Assets/Shaders/` está **vacío**.

VISUAL_TARGET.md requiere **3 shaders personalizados**:
- `RetroFlatLit.shader` — geometría arquitectónica, sin normales, sin AO, Smoothness 0.05
- `AnalogGhost.shader` — eco con dither transparency, 15 FPS, scanlines
- `LiminalFog.shader` — niebla lineal agresiva, no volumétrica

**NO EXISTE NINGUNO.** Todos los materiales usan `Universal Render Pipeline/Lit` estándar. La estética retro prometida no es posible sin estos shaders.

### Materiales actuales vs. target

| Target (VISUAL_TARGET.md) | Realidad |
|--------------------------|----------|
| `128x128` texturas Point filter | No hay texturas — colores planos |
| Sin normal maps, sin AO | No hay — correcto |
| `RetroFlatLit` shader | URP/Lit — incorrecto |
| Metallic 0, Smoothness 0.05 | ✅ Correcto |
| 60-30-10 regla de color | ❌ No verificable en niveles |
| Dither transparency en eco | URP/Lit alpha — incorrecto |
| 15 FPS animación de eco | 60 FPS interpolados — incorrecto |
| Niebla lineal agresiva | URP Fog — no configurado |

### PBR Contaminación

- `Assets/Materials/Facade001_1K-JPG_Color.jpg` + Normal + Roughness — PBR 1K
- `Assets/Materials/cloudy_netted_nursery_1k.exr` — HDRI de entorno (prohibido)
- `Assets/Materials/Concrete047A_2K-JPG.zip` y `Concrete048_2K-JPG.zip` — texturas 2K PBR (deben eliminarse)
- `Assets/Materials/Metal054B_2K-JPG.zip`, `Metal009_2K-JPG.zip`, `Metal027_2K-JPG.zip` — metales PBR

### Modular SciFi MegaKit — AÚN PRESENTE

`Assets/3D Models/Modular SciFi MegaKit[Standard]/` **sigue en el proyecto** a pesar de estar explícitamente prohibido. Ocupa espacio, confunde y podría ser referenciado por error.

### Colección de assets

- `Assets/3D Models/Architecture Pack 001-zip/` — ✅ permitido
- `Assets/3D Models/kenney_furniture-kit/` — ✅ permitido
- `Assets/3D Models/City Pack.undefined-zip/` — ✅ permitido (para Nivel 9)
- `Assets/3D Models/Stylized Nature MegaKit.undefined-zip/` — ✅ permitido
- `Assets/3D Models/Animated Woman/` — **esto no debería estar aquí** — modelo animado de mujer
- `Assets/3D Models/Universal Base Characters[Standard]/` — personajes base
- `Assets/3D Models/lowpoly-character-freerigged-/` — personaje lowpoly

### Props faltantes

VISUAL_TARGET.md lista props que NO EXISTEN:
- Pupitre doble escolar prefab ❌
- Silla escolar modular prefab ❌
- Radiador de fundición prefab ❌
- Estantería de biblioteca ❌
- Cartelera escolar prefab ❌
- Decals (humedad, tiza, marcas de arrastre, grietas) ❌

---

# 5 — UI / UX (2.5/10)

## Problemas de Identidad

### Terminología sci-fi GENERALIZADA

El problema más grave: la UI parece de un juego de naves espaciales.

| Término Sci-Fi | Archivo | Líneas |
|---------------|---------|--------|
| "Neural Archives" | MainMenuController | 26+ ocurrencias |
| "Calibration" / "Calibrar" | MainMenuController, PauseMenu | 15+ |
| "Telemetry" | MainMenuController | 5+ |
| "Protocol" | GameHUD, MainMenuController | 8+ |
| "Schematic" | GameHUD | clase completa |
| "Stability Map" | MainMenuController | panel completo |
| "Terminal log" con [SISTEMA], [SINC], [AVISO] | MainMenuController | 50+ líneas |
| "Archivist Rank" | MainMenuController, GameProgress | 30+ |
| "Integrity" | MainMenuController | 5+ |
| "Node" / "nodos de memoria" | MainMenuController, GameHUD | 10+ |
| "DESCONECTAR" | MainMenuController | botón |
| "Disconnect" | MainMenuController | panel |

**VISUAL_TARGET.md es explícito:** "El error más grave de la versión actual del proyecto es la interfaz tecnológica y clínica."

### Bugs Críticos (de UI_ANALYSIS.md, la mayoría sin corregir)

**BUG-01 — Game Over nunca se muestra.** 🔴
`GameOverController.Show()` no tiene ningún llamador. La muerte recarga escena directamente.

**BUG-02 — GameProgress solo conoce 10 de 15 niveles.** 🔴
Los niveles 11-15 no registran progreso. El contador "X/10" es incorrecto.

**BUG-03 — hero-title no existe en UXML.** 🟠
Título dinámico invisible. Null-guardado, no crashea, pero la feature está muerta.

**BUG-04 — stats de pausa en elementos inexistentes.** 🟠
Tiempo/ecos/muertes/expediente invisibles.

**BUG-05 — Cursor bloqueado al salir de pausa al menú.** 🟠

**BUG-06 — Confirmación de salida nunca cableada.** 🟠
`nav-exit` llama `QuitGame()` directo sin confirmación.

**BUG-07 — Sin OnDisable: callbacks duplicados.** 🟠

**BUG-08 — Slots de eco nunca se colorean.** 🟡
Clase CSS incorrecta.

**BUG-10 — Selectores USS inexistentes.** 🟡
preset-button--active, level-button--locked/completed, scale-large/xl no existen.

**BUG-11 — UI no auto-sanadora.** 🟡
Depende de rebuild manual del builder. Sin fallback.

**BUG-12 — Transiciones inconsistentes.** 🟡
SceneTransitionManager solo existe en MainMenu.

**BUG-13 — Labels del dashboard inexistentes.** 🟡
Decenas de queries a elementos que no existen en UXML.

**BUG-14 — Doble sistema de hover.** 🟡
MainMenuController + MenuHoverSystem se pisan.

**BUG-15 — Sin navegación por teclado/gamepad.** 🟡
FocusEvent no enganchado.

**BUG-20 — TutorialHUD no cachea GameHUD.** 🟡
FindAnyObjectByType en cada llamada.

**BUG-22 — MainMenu.cs legacy sigue presente.** ⚪
184 líneas de código muerto que puede interferir.

**BUG-23 — Timeline hardcodeado a 10s.** ⚪
Debería leer maxRecordSeconds de EchoRecorder.

**BUG-24 — USS cargado como ThemeStyleSheet.** ⚪
LoadAssetAtPath<ThemeStyleSheet>(.uss) devuelve null.

**BUG-28 — Slider de niebla no funciona.** ⚪
Dos sistemas escribiendo fogDensity.

**BUG-29 — Encoding mojibake.** ⚪
Acentos rotos en UXML/USS.

## Arquitectura de UI

- **Dos sistemas en paralelo:** UITK (nuevo) + uGUI (legacy, SceneTransitionManager, MainMenu.cs)
- **Sin prefabs:** UI horneada en cada escena por el builder. No hay fallback.
- **Sin prefabs de runtime:** Los cambios requieren reconstruir 15 escenas.
- **Sin tests de UI:** 0 verificaciones de que los UXML tengan los elementos que el código espera.

---

# 6 — AUDIO (4.0/10)

## Lo que funciona

- `EchoesAudioManager` con AudioMixer y 4 canales (Master/Music/SFX/Echo) — ✅
- GameFeelController con síntesis de audio en runtime (sine/tone/click) — ✅
- Footsteps, landing, grabación, eco, menú — ✅ (integrados)

## Lo que no funciona

### Assets de audio genéricos/sci-fi
- `freesound_community-sci-fi-charge-up-37395.mp3` — **SONIDO SCI-FI PROHIBIDO**
- `607238__szegvari__electric-dream-synth-drone-electric-cinematic.wav` — drone sintético
- `166118__deleted_user_2104797__metal-concrete.wav` — metal/concreto
- `Industrial-hum.wav` — zumbido industrial
- `Ventilation.wav` — ventilación

### Falta diseño sonoro específico
- No hay sonido de cinta de cassette (especificado en VISUAL_TARGET.md)
- No hay filtro bandpass para pasos del eco (especificado)
- No hay ruido de cinta (tape hiss) para grabaciones
- No hay síntesis de voz para los 3 momentos narrativos si el micrófono no está disponible
- No hay ambientes escolares específicos (zumbido de fluorescentes, timbre, páginas, pasos en linóleo)

### Rendimiento
- `new GameObject("OneShotAudio")` por cada sonido — **sin pooling de audio**
- `FindGroup()` sin caché — busca en el mixer cada vez
- `FindAnyObjectByType<AudioListener>()` cada frame en GameFeelController

---

# 7 — NARRATIVA (6.0/10)

## Lo bueno
- La Biblia narrativa es EXCELENTE. Los 6 capítulos, el arco de Aiden y Lyra, la progresión emocional
- La "prueba sin texto" es un criterio de diseño MUY sólido
- La regla de Lyra-solo-3-formas-de-presencia es elegante
- El final sin cinemática, con créditos proyectados en paredes, es POTENTE

## Lo preocupante
- **No hay implementación verificable de la narrativa.** Los niveles existen como escenas genéricas. La narrativa está toda en documentos, no en el juego.
- **No hay sistema de environmental storytelling.** No hay decals, props narrativos, o sistemas que coloquen objetos de historia.
- **No hay sistema de Lyra.** El eco ambiental de Lyra (Nivel 10) no tiene un script dedicado. `LevelBlueprint.cs` no tiene campos para ecos narrativos.
- **Los 3 momentos de voz dependen del micrófono del jugador.** Si el jugador no habla durante la grabación, el momento narrativo se pierde.
- **No hay sistema de capítulos.** El juego no diferencia entre Capítulo I y Capítulo VI. La progresión emocional depende enteramente del orden de niveles, no de cambios sistémicos.
- **No hay cinemáticas ni transiciones narrativas.** Esto es deliberado (Biblia) pero también significa que no hay momentos de impacto garantizados.

---

# 8 — CÓDIGO (4.0/10)

## Dead Code

| Archivo | Líneas | Estado |
|---------|--------|--------|
| `EchoesProductionBuilder.cs` | 2403 | #if false — LEGACY |
| `MainMenu.cs` | 184 | Reemplazado por MainMenuController |
| `EchoesProductionBuilder_Levels1_5.cs` | ? | Referencia histórica no activa |
| `EchoesProductionBuilder_Levels6_10.cs` | ? | Referencia histórica no activa |
| `EchoesProductionBuilder_Levels11_15.cs` | ? | Referencia histórica no activa |
| `ResonanceSystem` mencionado en NewProductionBuilder:222 | - | Clase que probablemente no existe |
| `EchoKineticZone` mencionado en NewProductionBuilder:374 | - | Clase que puede no existir |
| `GameOverController` | 156 | Nunca se invoca |

## Código Potencialmente Roto

1. **`EchoesNewProductionBuilder.cs:222`** — `ResonanceSystem resonance = inst.gameObject.GetComponent<ResonanceSystem>();`
   Si `ResonanceSystem` no existe en el proyecto, compila pero nunca asigna.

2. **`EchoesNewProductionBuilder.cs:374`** — `EchoKineticZone relay = inst.gameObject.GetComponent<EchoKineticZone>();`
   Si `EchoKineticZone` no existe (solo existe `EchoKineticRole` y `EchoKineticBody`), es código muerto.

3. **`EchoPlayback.cs:596-603`** — `EnsureOptionalComponent(string typeName)` con reflection entre assemblies. Si el tipo no está en ninguna assembly cargada, falla silenciosamente.

4. **`ProjectSettings/VFXManager.asset` modificado** — cambio no stageado en git. ¿Qué cambió?

## Falta de Tests

- **0 tests unitarios.**
- **0 tests de integración.**
- **0 tests de UI.**
- **0 tests de regresión.**
- Cualquier cambio en el builder requiere validar manualmente 15 escenas.

## Problemas de Compilación

- `csc.rsp` presente en `Assets/` — esto sugiere que hay argumentos de compilación personalizados
- El proyecto referencia `MCPForUnity.Editor` y `MCPForUnity.Runtime` — herramientas externas de MCP
- `Echoes of you.slnx` — solución de Visual Studio

---

# 9 — RENDIMIENTO (3.5/10)

## Problemas por Frame

| Problema | Archivo | Severidad |
|----------|---------|-----------|
| `FindAnyObjectByType<EchoRecorder>()` × 2 cada frame | GameFeelController.cs:221,223 | 🔴 |
| `FindAnyObjectByType<AudioListener>()` cada frame | GameFeelController.cs:221 | 🟠 |
| `FindAnyObjectByType<GameHUD>()` cada llamada tutorial | TutorialHUD.cs:75,88,98 | 🟠 |
| `new GameObject("OneShotAudio")` sin pooling | GameFeelController.cs | 🔴 |
| Renderer.materials cada vez (no MaterialPropertyBlock) | HubPortal, EchoPlayback, PressurePlate | 🟠 |
| `Physics.SphereCastAll` (array allocation) | PlayerController_Gravity.cs:102 | 🟡 |
| `Physics.OverlapBoxNonAlloc` con buffer 16/32 | PressurePlate, EchoKineticBody, etc. | 🟡 |
| Corrutinas one-shot sin tracking | MenuButtonHoverState.cs | 🟡 |
| 36 VisualElements dinámicos por hover | MenuButtonHoverState.cs:79-127 | 🟡 |
| Settings UI re-crea lista de resoluciones cada OnEnable | MainMenuController.cs:558-582 | 🟡 |

## Draw Calls y Batch

- Sin LODs en ningún modelo
- Sin GPU instancing configurado
- Sin occlusion culling verificado
- Materiales duplicados por instancia (cada eco clona materiales con `new Material()`)
- Sin atlas de texturas

## Memoria

- `PlayerPrefs` para todo el progreso (15 niveles, settings, echo opacity)
- `AudioClip.Create` para cada grabación de voz
- `new Material()` en runtime para cada eco
- Sin límite de allocaciones para partículas de polvo

---

# 10 — ANIMACIONES (4.0/10)

## Estado Actual

- **Aiden:** Animator controller funcional con blend trees. Parámetros: Speed, IsGrounded, IsRecording, VerticalSpeed, Turn, HardLanding, etc.
- **Eco:** Clona el Animator del jugador. Probablemente usa el mismo controller.
- **Problema:** El eco se mueve a 60 FPS interpolados. La Biblia exige 12-15 FPS con dither.

## Problemas

1. **No hay Animator Controller dedicado para el eco.** El eco usa `_anim.speed = 0.88f` (línea 280) en lugar de un sistema de frame-skipping.

2. **`EchoesAnimatorParams.cs`** itera todos los parámetros del animator en cada set (O(n) para cada `SetFloat`, `SetBool`). Con 5-6 parámetros por frame, es ineficiente.

3. **Root motion desactivado** en el eco (`animator.applyRootMotion = false`). Esto significa que las animaciones del eco no afectan su posición, que es controlada por el CharacterController. Correcto, pero puede causar sliding visual si los pies no coinciden con el movimiento.

4. **PlayerAnimationRuntimeBootstrap** — parece aplicar configuración de animación en runtime. No se pudo verificar su contenido completo.

5. **No hay animaciones de transición narrativas.** Sin cinemáticas (deliberado), pero también sin animaciones de ambiente (hojas cayendo, polvo moviéndose, puertas crujiendo).

---

# 11 — IDENTIDAD

## ¿Por qué alguien jugaría Echoes of You y no Portal?

**La pregunta correcta es: ¿Por qué alguien jugaría Echoes of You, punto?**

La respuesta actual es: por la mecánica del eco irreversible. Es genuinamente única. No hay otro juego donde grabes tu propio movimiento y tengas que convivir con el resultado.

## ¿Qué lo hace único?

- **La irreversibilidad del eco como tema.** No es un puzzle más, es una frase sobre el pasado.
- **La conexión emocional.** Aiden no resuelve puzzles abstractos — resuelve su propia memoria.
- **La estética liminal PS1.** Escuela vacía, niebla, soledad. Si se ejecuta bien, es inolvidable.

## ¿Qué parece genérico?

- **La UI actual.** Parece de un asset flip de sci-fi.
- **Los materiales actuales.** Sin los shaders retro, parece un proyecto URP genérico.
- **La infraestructura de puzzle.** Placas y puertas es el gameplay más genérico posible.
- **Los assets 3D.** Sin el tratamiento retro, los modelos de Kenney se ven como placeholder.

## ¿Qué parte debería potenciarse?

1. **La degradación del eco.** Es la mecánica más única y la menos implementada.
2. **El estado residual de 2.5s.** Usar el eco como plataforma física es brillante.
3. **La exploración como puzzle.** La Biblia dice que el espacio es el puzzle, no el contenedor.
4. **El momento "No grabo lo que hice, grabo lo que voy a necesitar".** Es el mayor salto conceptual.

---

# 12 — RIESGOS

## Softlocks Potenciales

1. **Dos ecos que se cancelan (Nivel 12).** Si el jugador graba dos ecos contradictorios, ¿puede recuperarse? `EchoConflictTrap` existe, pero ¿hay camino de salida si el jugador se bloquea?

2. **Puente espectral sin eco.** Si el eco se desvanece mientras el jugador está sobre el puente, ¿cae al vacío? GhostBridge no parece tener handling para esto.

3. **Grabación demasiado corta.** EchoRecorder rechaza grabaciones <2 frames. ¿Y si el jugador necesita exactamente 1 frame para un puzzle? No hay fallback.

## Problemas de Cámara

4. **Cinemachine vs ThirdPersonCamera.** Resuelto en código (se desactivan mutuamente) pero frágil. Un cambio en Cinemachine API rompe el setup.

5. **Cámara bloqueada por geometría.** La Biblia lo reconoce: "Si la geometría bloquea la cámara, el layout está mal." No hay verificación automática de esto.

## Problemas de Guardado

6. **PlayerPrefs en plataformas.** No funciona en WebGL correctamente. En consolas puede no estar disponible.

7. **Sin guardado en medio de nivel.** Si el jugador cierra el juego en el Nivel 10, vuelve al Nivel 1.

## Problemas de Input

8. **Escape para pausa.** También es tecla predeterminada para skip de Unity. Podría haber conflictos.

9. **Q para limpiar ecos.** Demasiado cerca de WASD. Podría presionarse accidentalmente.

## Problemas de Builder

10. **EchoesQueuedProductionRebuild** puede borrar 15 escenas sin confirmación si el flag está activo. PROJECT_CONTEXT.md lo advierte.

11. **Sin versionamiento de escenas.** Las escenas generadas no se comparan bien en diff. Si alguien edita una escena manualmente y luego ejecuta el builder, los cambios se pierden.

---

# TOP 50 MEJORAS PRIORIZADAS

## Impacto: 🔴 Alto · 🟠 Medio · 🟡 Bajo
## Costo: $ (bajo, <1 semana) · $$ (medio, 1-3 semanas) · $$$ (alto, >1 mes)

| # | Impacto | Costo | Prioridad | Mejora | Tiempo | Riesgo |
|---|---------|-------|-----------|--------|--------|--------|
| 1 | 🔴 | $ | **CRÍTICA** | **Eliminar EchoesProductionBuilder.cs** (2403 líneas dead code con SciFi) | 1 día | Bajo |
| 2 | 🔴 | $ | **CRÍTICA** | **Eliminar Modular SciFi MegaKit de Assets/3D Models** | 1 hora | Bajo |
| 3 | 🔴 | $$ | **CRÍTICA** | **Crear shaders RetroFlatLit, AnalogGhost, LiminalFog** | 2-3 semanas | Medio |
| 4 | 🔴 | $$ | **CRÍTICA** | **Eliminar toda terminología sci-fi de la UI** (Neural, Calibration, Protocol, Telemetry, Schematic, Node, Integrity, Archivist, Disconnect) | 2 semanas | Medio |
| 5 | 🔴 | $ | **CRÍTICA** | **BUG-01: Conectar GameOverController.Show() o eliminar definitivamente** | 1 día | Bajo |
| 6 | 🔴 | $ | **CRÍTICA** | **BUG-02: Ampliar GameProgress a 15 niveles** | 1 día | Bajo |
| 7 | 🔴 | $ | **CRÍTICA** | **BUG-03: Añadir hero-title a MainMenuUI.uxml** | 1 hora | Bajo |
| 8 | 🔴 | $ | **CRÍTICA** | **BUG-04: Añadir labels de stats a PauseMenuUI.uxml** | 1 día | Bajo |
| 9 | 🔴 | $ | **CRÍTICA** | **BUG-05: Fix cursor al salir de pausa al menú** | 1 día | Bajo |
| 10 | 🔴 | $$ | **CRÍTICA** | **Unificar maxRecordSeconds: EchoRecorder(6) → Biblia(20) → HUD(10)** | 2 días | Alto |
| 11 | 🔴 | $$ | **CRÍTICA** | **BUG-11: Convertir UI en prefabs + bootstrap de runtime** | 2-3 semanas | Alto |
| 12 | 🔴 | $ | **CRÍTICA** | **BUG-06: Cablear confirmación de salida** | 1 día | Bajo |
| 13 | 🔴 | $$ | **ALTA** | **Implementar degradación del eco por uso repetido** (Biblia, Nivel 6) | 2 semanas | Medio |
| 14 | 🔴 | $$ | **ALTA** | **Implementar estado residual de 2.5s en EchoPlayback** (hoy usa 0.55s) | 1 semana | Medio |
| 15 | 🔴 | $$ | **ALTA** | **Implementar latencia de 0.8s en inicio de eco** (mencionado en Biblia, no existe) | 1 semana | Medio |
| 16 | 🔴 | $ | **ALTA** | **BUG-07: Añadir OnDisable con Unregister en MainMenuController** | 1 día | Bajo |
| 17 | 🔴 | $ | **ALTA** | **BUG-08: Renombrar clase CSS hud-echo-slot--active → --filled** | 1 hora | Bajo |
| 18 | 🔴 | $ | **ALTA** | **BUG-10: Añadir selectores USS faltantes** | 1 día | Bajo |
| 19 | 🟠 | $$$ | **ALTA** | **Implementar eco ambiental de Lyra** (Nivel 10, script dedicado) | 3-4 semanas | Alto |
| 20 | 🟠 | $$ | **ALTA** | **Implementar eco de voz narrativa sin micrófono** (síntesis, audio pregrabado, fallback) | 2 semanas | Medio |
| 21 | 🟠 | $ | **ALTA** | **BUG-12: Hacer SceneTransitionManager auto-sanador** | 3 días | Medio |
| 22 | 🟠 | $ | **ALTA** | **BUG-13: Alinear nombres UXML↔código en dashboard** | 3 días | Medio |
| 23 | 🟠 | $ | **ALTA** | **BUG-14: Unificar sistema de hover** | 3 días | Medio |
| 24 | 🟠 | $ | **ALTA** | **BUG-15: Enganchar FocusEvent para navegación teclado/gamepad** | 3 días | Medio |
| 25 | 🟠 | $$ | **ALTA** | **Refactor: Centralizar SetSerializedValue en EchoesEditorUtility** | 1 semana | Bajo |
| 26 | 🟠 | $$ | **ALTA** | **Refactor: GameFeelController dividir en 3-4 clases** | 2 semanas | Alto |
| 27 | 🟠 | $ | **ALTA** | **BUG-22: Eliminar MainMenu.cs legacy** | 1 día | Bajo |
| 28 | 🟠 | $$ | **ALTA** | **BUG-20: Cachear GameHUD en TutorialHUD + retry init** | 3 días | Bajo |
| 29 | 🟠 | $ | **ALTA** | **BUG-23: timeline totalSecs desde EchoRecorder.maxRecordSeconds** | 1 día | Bajo |
| 30 | 🟠 | $$ | **ALTA** | **BUG-24: Centralizar GetOrCreatePanelSettings con .tss correcto** | 3 días | Medio |
| 31 | 🟠 | $ | **ALTA** | **BUG-28: Unificar escritura de fogDensity (menú vs niveles)** | 1 día | Bajo |
| 32 | 🟠 | $ | **ALTA** | **BUG-29: Corregir encoding UTF-8 de UXML/USS** | 1 día | Bajo |
| 33 | 🟠 | $$ | **ALTA** | **Implementar progresión de maxEchoes y maxRecordSeconds por capítulo** | 1 semana | Medio |
| 34 | 🟠 | $ | **ALTA** | **BUG-09: Añadir guardia _initialized en GameOverController** | 1 día | Bajo |
| 35 | 🟠 | $$ | **MEDIA** | **Implementar texturas 128×128 Point filter para materiales escolares** | 2 semanas | Medio |
| 36 | 🟠 | $$ | **MEDIA** | **Eliminar PBR leftovers: Facade001, Concret047A, Metal054B, HDRI** | 2 días | Bajo |
| 37 | 🟠 | $ | **MEDIA** | **BUG-18: Añadir retry init en GameHUD** | 1 día | Bajo |
| 38 | 🟠 | $ | **MEDIA** | **BUG-27: Rastrear y limpiar corrutinas one-shot de hover** | 3 días | Bajo |
| 39 | 🟠 | $$ | **MEDIA** | **BUG-26: Migrar a InputSystemUIInputModule** | 1 semana | Medio |
| 40 | 🟠 | $ | **MEDIA** | **BUG-25: Verificar orden de render fade uGUI vs UITK** | 2 días | Bajo |
| 41 | 🟠 | $$$ | **MEDIA** | **Implementar props escolares: pupitres, sillas, radiador, estanterías, pizarra** | 3-4 semanas | Medio |
| 42 | 🟠 | $$$ | **MEDIA** | **Implementar decals: humedad, tiza, marcas de arrastre, grietas** | 2-3 semanas | Bajo |
| 43 | 🟠 | $ | **MEDIA** | **Crear prefabs de todos los módulos de puzzle (no generar en runtime)** | 1 semana | Alto |
| 44 | 🟠 | $$ | **MEDIA** | **Pooling de audio: reutilizar GameObjects OneShotAudio** | 1 semana | Medio |
| 45 | 🟠 | $ | **MEDIA** | **Pooling de partículas de polvo** | 3 días | Bajo |
| 46 | 🟠 | $$ | **MEDIA** | **Implementar diseño sonoro escolar: zumbido fluorescentes, timbre, pasos linóleo** | 2 semanas | Medio |
| 47 | 🟠 | $ | **MEDIA** | **Eliminar freesound_community-sci-fi-charge-up-37395.mp3** | 1 hora | Bajo |
| 48 | 🟠 | $$ | **MEDIA** | **Refactor: Crear SettingsManager compartido para UI** | 2 semanas | Alto |
| 49 | 🟠 | $ | **MEDIA** | **BUG-16: Unificar estado de pausa global** | 3 días | Medio |
| 50 | 🟠 | $$ | **MEDIA** | **Implementar sistema de capítulos con reglas de mecánicas permitidas** | 2 semanas | Medio |

---

# ROADMAP

## FASE 0 — TRIAGE (Semana 1)
*Lo que se hace AHORA para evitar desastre:*

| Día | Tarea | ¿Por qué ahora? |
|-----|-------|-----------------|
| 1 | Eliminar `EchoesProductionBuilder.cs` | 2403 líneas de dead code que confunden |
| 1 | Eliminar Modular SciFi MegaKit de Assets | Prohibido explícitamente |
| 1 | Eliminar texturas PBR 2K/1K de Assets/Materials | Contaminan estética |
| 1 | Eliminar sci-fi-charge-up-37395.mp3 | Sonido sci-fi prohibido |
| 2 | BUG-02: Ampliar GameProgress a 15 niveles | Flujo de juego roto |
| 2 | BUG-01: Conectar GameOver o eliminarlo | Feature muerta |
| 2 | BUG-03/04/08/10: Mismatches UXML↔código | Múltiples features invisibles |
| 3 | BUG-05/06/07: Cursor, confirmación, ciclo de vida | Bugs de UX críticos |
| 3 | Unificar maxRecordSeconds a 20 | 3 valores distintos = bug garantizado |
| 3 | SoftReset: añadir confirmación o delay | No puede ser deshacer gratuito |

## FASE 1 — IDENTIDAD VISUAL (Semanas 2-6)
*Sin esto, el juego no tiene identidad:*

| Semana | Tarea |
|--------|-------|
| 2-4 | Crear 3 shaders: RetroFlatLit, AnalogGhost, LiminalFog |
| 4-5 | Eliminar terminología sci-fi de TODA la UI (refactor mayor) |
| 5-6 | Implementar texturas 128×128 Point filter (madera, linóleo, pizarra, yeso) |
| 5-6 | Crear props escolares: pupitres, sillas, radiador, estanterías, pizarra |

## FASE 2 — UI UNIFICADA (Semanas 6-9)
*La UI no puede seguir siendo un Frankenstein:*

| Semana | Tarea |
|--------|-------|
| 6-7 | Convertir UI en prefabs + bootstrap de runtime (BUG-11) |
| 7 | Eliminar MainMenu.cs legacy, SceneTransitionManager uGUI |
| 7-8 | SettingsManager compartido (eliminar 230×3 líneas duplicadas) |
| 8-9 | Migrar efectos hover a CSS (transition/@keyframes) |
| 8-9 | Navegación completa por teclado/gamepad |

## FASE 3 — MECÁNICAS FALTANTES (Semanas 9-14)
*Sin esto, los niveles no tienen puzzle:*

| Semana | Tarea |
|--------|-------|
| 9-10 | Implementar degradación del eco por uso repetido |
| 10-11 | Implementar estado residual de 2.5s + latencia de 0.8s |
| 11-12 | Implementar eco ambiental de Lyra (script + blueprint) |
| 12-13 | Implementar sistema de voz narrativa sin micrófono |
| 13-14 | Progresión por capítulos: maxEchoes, maxRecordSeconds, mecánicas permitidas |
| 14 | Programa de validación: "Prueba del Botón" para cada puzzle |

## FASE 4 — NIVELES REALES (Semanas 14-20)
*Aquí se construye el juego de verdad:*

| Semana | Tarea |
|--------|-------|
| 14-15 | Nivel 1 funcional con puzzle validado |
| 15-16 | Nivel 2 funcional (eco básico, 3 puzzles en escalera) |
| 16-17 | Nivel 3 funcional (bifurcación, dos caminos) |
| 17-18 | Playtest de Capítulo I completo (Niveles 1-3) |
| 18-19 | Iterar basado en playtest |
| 19-20 | Nivel 4-6 funcionales |

## FASE 5 — VERTICAL SLICE (Semanas 20-24)
*El momento de la verdad:*

| Semana | Tarea |
|--------|-------|
| 20-21 | Capítulos I-II completos (Niveles 1-5) |
| 21-22 | Shaders retro aplicados a todos los materiales |
| 22-23 | UI final con temática analógica escolar |
| 23-24 | Vertical Slice: 5 niveles pulidos, jugables de principio a fin |
| 24 | Playtest interno + iteración |

## FASE 6 — DEMO STEAM (Semanas 24-30)

| Semana | Tarea |
|--------|-------|
| 24-26 | Capítulos I-III completos (Niveles 1-7) |
| 26-27 | Optimización de rendimiento (LODs, batching, occlusion, pooling) |
| 27-28 | Localización (inglés + español) |
| 28-29 | Steam integration (build, achievements, cloud saves) |
| 29-30 | Demo pública en Steam (Niveles 1-3) |

## FASE 7 — VERSIÓN 1.0 (Semanas 30-48)

| Mes | Hito |
|-----|------|
| Mes 8 | Todos los 15 niveles jugables con todas las mecánicas |
| Mes 9 | Playtest completo + iteración mayor |
| Mes 10 | Pulido de UI/UX, sonido, arte final |
| Mes 11 | QA completo, bug fixing, accesibilidad |
| Mes 12 | Lanzamiento 1.0 en Steam + itchio |

## LO QUE NO VALE LA PENA HACER

- **NO** expandir a más de 15 niveles. El scope actual ya es agresivo.
- **NO** añadir multijugador, modo cooperativo, o leaderboards. Destruye el tema.
- **NO** añadir combate, enemigos, o peligros que no sean ecos. La Biblia lo prohíbe.
- **NO** añadir cinemáticas renderizadas. El poder del juego está en el gameplay narrativo.
- **NO** invertir en porting a consolas hasta que la Vertical Slice esté validada.
- **NO** crear más builders. El pipeline actual funciona (con deuda, pero funciona).
- **NO** reescribir EchoRecorder/EchoPlayback desde cero. Son funcionales. Hay que extenderlos, no reiniciarlos.
- **NO** añadir sistema de diálogos ramificados. Lyra tiene 3 formas de presencia, no necesita más.
- **NO** añadir modo foto, galería de arte, o extras desbloqueables. No es el momento.
- **NO** perseguir 60 FPS en Nintendo Switch antes de que el juego funcione bien en PC.

---

# RESUMEN EJECUTIVO

Echoes of You tiene:

1. **Una mecánica central GENUINAMENTE ORIGINAL.** La irreversibilidad del eco como tema y mecánica. Esto es extremadamente raro en el panorama indie actual.

2. **Una biblia de diseño EXCELENTE.** Los documentos ECHOES_BIBLE.md, VISUAL_TARGET.md y BRIEF_ESPACIAL son de calidad AAA en concepto.

3. **Una arquitectura funcional pero endeudada.** El código funciona, pero la migración del tema SciFi a Escolar dejó heridas abiertas: dead code, terminología inconsistente, assets prohibidos.

4. **Una UI fracturada.** Dos sistemas peleándose, terminología sci-fi, bugs que ocultan features completas.

5. **Una dirección artística en documentos pero no en pantalla.** Sin los shaders custom, el juego se ve como un proyecto URP genérico.

6. **Puzzles que no existen como diseño validado.** Los niveles tienen geometría pero no hay puzzles jugables verificables.

**La pregunta no es si el proyecto tiene potencial. Lo tiene.**
**La pregunta es si el equipo va a ejecutar la visión que ya escribió.**

La brecha entre los documentos y el código es de 6-8 meses de trabajo enfocado. No es imposible. Pero requiere decisiones difíciles: eliminar código querido, reescribir UI, crear shaders desde cero, y sobre todo — VALIDAR QUE UN SOLO NIVEL SEA DIVERTIDO antes de construir los otros 14.

---

*Fin de la auditoría.*
*Equipo de Revisión AAA — Julio 2026*
