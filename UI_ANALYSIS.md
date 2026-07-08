# Análisis de UI — Echoes of You

> Documentación completa de la interfaz, catálogo de errores y mejoras.
> Fecha: 2026-07-08 · Rama: `main` (tip `e43c09c`, post-migración URP).
> Alcance: todos los scripts de UI en `Assets/Scripts` + assets en `Assets/UI` + spawning en `Assets/Editor`.

---

## 0. Estado de correcciones (2026-07-08)

**Corregidos en esta pasada:**
- **Error de compilación** `CS0104 Cursor ambiguo` en `MenuHoverSystem.cs` → cualificado a `UnityEngine.Cursor`.
- **BUG-01** → `GameOverController` **eliminado** (script + `GameOverUI.uxml` + `.meta` + todas las referencias en los builders). Decisión del usuario: no reconectar, borrar.
- **BUG-02** → `GameProgress` ampliado a 15 niveles (scenes + display names) + botones `btn-level-11..15` añadidos a `MainMenuUI.uxml`.
- **BUG-03** → `hero-title` añadido a `MainMenuUI.uxml`.
- **BUG-04** → labels `lbl-pause-*` añadidos a `PauseMenuUI.uxml`.
- **BUG-05** → cursor: nuevo helper `PauseMenu.UnpauseForMenu()` mantiene el cursor visible al ir a hub/menú.
- **BUG-07** → guardia `_wired` en `MainMenuController` evita doble registro de callbacks (nota: la "inflación de sesiones" era falsa; `RecordSessionStarted` ya estaba guardada).
- **BUG-08** → `hud-echo-slot--active` → `hud-echo-slot--filled` en `GameHUD`.
- **BUG-10** → selectores USS añadidos: `.preset-button--active`, `.level-button--locked/--completed`, `.scale-large`, `.scale-xl`; `.footer-item--inactive` explícita.
- **BUG-18** → `GameHUD` reintenta init (`QueryElements` extraído + retry en `Update`).
- **BUG-20** → `TutorialHUD` cachea `GameHUD` y respeta `duration` (auto-clear del objetivo).

**Pendientes (requieren decisión de diseño o refactor mayor):** BUG-06 (flujo confirmación salida/reset), BUG-11 (UI a prefabs/bootstrap), BUG-12 (transiciones unificadas), BUG-13 (resto de labels del dashboard, varios en contenedores `display:none`), BUG-14 (unificar hover + `translate` inline), BUG-15 (foco teclado/gamepad), BUG-19 (barras stability/recall sin datos), BUG-21 (Hub inexistente), BUG-22 (borrar `MainMenu.cs` legacy — verificar antes que no esté en la escena), BUG-23 (timeline hardcodeado), BUG-24 (PanelSettings/tss), BUG-25/26 (sorting fade / input module), BUG-27 (corrutinas hover), BUG-28 (niebla del menú), BUG-29 (encoding UTF-8 masivo). BUG-09/16/17 quedaron **resueltos por eliminación** de GameOver.

> ⚠️ **Requiere rebuild:** las 15 escenas tienen horneado el GameObject `GameOverUI` (ahora con script/uxml faltantes). Ejecutar `Echoes of You > Production > Rebuild` para regenerar escenas sin él y con los builders actualizados.

---

## 1. Arquitectura general

El juego usa **dos sistemas de UI en paralelo**:

| Sistema | Uso | Archivos |
|---|---|---|
| **UI Toolkit** (UXML/USS + `UIDocument`) | UI principal: menú, HUD, pausa, game over | `MainMenuController`, `GameHUD`, `PauseMenu`, `GameOverController`, `MenuHoverSystem`, `MenuButtonHoverState` |
| **uGUI** (Canvas/Image/Button) | Fade de transición + menú legacy | `SceneTransitionManager` (fade), `MainMenu.cs` (legacy sin uso) |

**Assets UI** (`Assets/UI/`):
- UXML: `MainMenuUI.uxml`, `GameHUDUI.uxml`, `PauseMenuUI.uxml`, `GameOverUI.uxml`
- USS: `EchoesTheme.uss` (tema global), `EchoesHoverEffects.uss` (hover del menú)
- `EchoesPanelSettings.asset` (PanelSettings compartido, tema → `UnityDefaultRuntimeTheme.tss`)

**Cómo se instancia la UI:** NO hay prefabs ni bootstrap de runtime. Toda la UI se **hornea en cada escena `.unity`** al ejecutar los builders de editor (`EchoesProductionBuilder.cs` y variantes). Cada `UIDocument` recibe su UXML vía `AssetDatabase.LoadAssetAtPath` y comparte el PanelSettings. **Consecuencia:** si una escena no se reconstruyó o se editó a mano perdiendo el objeto, no hay HUD/pausa y no existe fallback (ver BUG-11).

### Flujo de pantallas

```
MainMenu (MainMenuController + SceneTransitionManager + EventSystem)
   │  "New Game" → siempre Level_01 (ignora progreso)
   │  LoadLevel() → SceneTransitionManager.LoadScene  [CON fade, valida escena]
   ▼
Level_01 … Level_15   (GameHUD + PauseMenu + GameOverUI horneados por builder)
   │
   ├─ Puzzle resuelto → LevelExit → SceneTransitionManager.LoadScene(next)  [CON fade]
   ├─ Muerte → LevelRuntimeController.HandlePlayerDeath → GameStateController
   │            → SceneManager.LoadScene(buildIndex)  [SIN fade]  (recarga escena)
   ├─ Esc → PauseMenu: Reanudar / Recalibrar / Hub / MainMenu  [SIN fade]
   └─ GameOverUI.Show()  ← NUNCA SE LLAMA (pantalla muerta)
   │
Level_15.LevelExit → "MainMenu"
```

No existe escena "Hub" real: `HubPortal`/`HubSceneController` están huérfanos y los botones "Hub" van a `MainMenu`.

---

## 2. Documentación por componente

### 2.1 Menú principal — `MainMenuController.cs` (1158 líneas, UI Toolkit) — ACTIVO
Controlador del menú "EXPEDIENTE DE RECUERDOS". `[RequireComponent(UIDocument, MenuHoverSystem)]`. Se inicializa en `OnEnable` cacheando referencias del `rootVisualElement`. Gestiona:
- **Navegación:** 4 botones (`nav-newgame`, `nav-levels`, `nav-settings`, `nav-exit`) con paneles de preview a la derecha.
- **Settings:** sliders/toggles/dropdowns de audio, vídeo, sensibilidad e iluminación. Persistencia en `PlayerPrefs`; audio real vía `EchoesAudioManager`, iluminación/escala vía `EchoesPresentationSettings`.
- **Selección de nivel:** botones `btn-level-01..10` → `LoadLevel`.
- **Salida:** `nav-exit` → `QuitGame()` **sin confirmación**.

### 2.2 Efectos de menú — `MenuHoverSystem.cs` (407) + `MenuButtonHoverState.cs` (621)
Capa de efectos "PS2/VHS/CRT" sobre los 4 botones nav: audio, tint de panel, gradiente, state machine por botón (Idle/Hover/Pressed) que inyecta ghost layers, aberración cromática, ruido, scanlines y partículas como hijos del botón.

### 2.3 `MainMenu.cs` (184, uGUI) — LEGACY / MUERTO
Controlador de menú paralelo basado en Canvas/Button de uGUI, previo a la migración URP. No usa `GameProgress` ni `EchoesAudioManager` (escribe `AudioListener.volume` sin persistir). Redundante con `MainMenuController`.

### 2.4 HUD de gameplay — `GameHUD.cs` (423, UI Toolkit)
HUD analógico "cinta/casete": panel de grabación, slots de eco, objetivo, toast/prompt, footer REC/ECO, timeline. Modelo **push**: no lee lógica, expone API (`SetEchoCount`, `SetRecording`, `SetObjective`, `ShowToast`, …) alimentada por `EchoRecorder.RefreshHud()`. `Update()` refresca con `Time.unscaledTime` (funciona con `timeScale=0`). Las `Q<>` se cachean correctamente en `OnEnable`.

### 2.5 Menú de pausa — `PauseMenu.cs` (638, UI Toolkit)
Pausa ("Libreta de Recuerdo") + panel de ajustes completo. Toggle con **Escape**. `Pause()` → `timeScale=0` + cursor libre; `Resume()` → `timeScale=1` + cursor bloqueado. `OnDestroy` restaura `timeScale=1`. Init protegida por `_initialized` con retry en `Update`.

### 2.6 Game Over — `GameOverController.cs` (156, UI Toolkit)
Overlay de muerte con mensajes/poemas aleatorios. `Show()` → cursor visible, `timeScale=0`, muestra overlay. 4 botones: retry / hub / menu / exit. **Nunca se invoca** (ver BUG-01).

### 2.7 Tutorial — `TutorialHUD.cs` (104)
Singleton que no dibuja nada propio; enruta a `GameHUD` (`ShowMessage`→`SetPrompt`, etc.). Hace `FindAnyObjectByType<GameHUD>()` en cada llamada.

### 2.8 Transiciones — `SceneTransitionManager.cs` (158, uGUI)
Singleton `DontDestroyOnLoad` con fade a negro (Canvas `sortingOrder=999`, fade con `unscaledDeltaTime`). Dedup correcto de instancias. **Solo lo usan `LevelExit` y `MainMenuController`**; el resto cambia de escena sin fade.

---

## 3. Catálogo de errores

Severidad: 🔴 crítico · 🟠 alto · 🟡 medio · ⚪ bajo. Los marcados **[verificado]** se confirmaron leyendo el código directamente.

### 🔴 Críticos

**BUG-01 — La pantalla de Game Over nunca se muestra. [verificado]**
`GameOverController.cs:80` (`Show()`). No hay ningún llamador en todo `Assets/Scripts` (grep = 0). La muerte se resuelve en `LevelRuntimeController.HandlePlayerDeath` → recarga de escena; `GameStateController.NotifyPlayerDeath` solo hace `SetState(PlayerDead)`. El `GameOverUI` se hornea en las 15 escenas pero es **inalcanzable**. → Conectar `Show()` desde `HandlePlayerDeath`, o eliminar la clase.

**BUG-02 — `GameProgress` solo conoce 10 de los 15 niveles. [verificado]**
`GameProgress.cs:15-27`: `LevelScenes` lista `Level_01..Level_10`, pero existen y están en Build Settings `Level_11..Level_15`. Para esos niveles `GetSceneIndex` devuelve `-1` → `MarkSceneCompleted` es no-op, `IsSceneUnlocked` = false, el contador "X/10" es incorrecto. Al terminar Level_10 se carga Level_11 pero el progreso nunca lo registra. → Ampliar la lista a 15 (idealmente derivarla de Build Settings).

### 🟠 Altos

**BUG-03 — `hero-title` no existe en el UXML → título dinámico invisible. [verificado]**
`MainMenuController.cs:120` cachea `Q<Label>("hero-title")`, usado en 6 sitios para mostrar "Aula 104", "Archivos Escolares", etc. `grep hero-title MainMenuUI.uxml` = 0. Toda la lógica de swap de título es no-op (null-guardado, no crashea). → Añadir el elemento al UXML o eliminar el código.

**BUG-04 — Estadísticas del menú de pausa apuntan a elementos inexistentes. [verificado]**
`PauseMenu.cs` consulta `lbl-pause-fragment/time/echoes/deaths/total` (5 refs); ninguno existe en `PauseMenuUI.uxml`. Todo `RefreshPauseStats` (tiempo/ecos/muertes/expediente) es invisible. → Añadir los labels al UXML.

**BUG-05 — Cursor bloqueado/oculto al salir de pausa hacia el menú. [verificado]**
`PauseMenu.cs:109-123` (`hubAction`/`menuAction`) llaman `Resume()` (que hace `Cursor.lockState=Locked; visible=false`) **antes** de `LoadScene(MainMenu)`. El menú abre con cursor invisible/bloqueado. `GameOverController.GoToHub/GoToMenu` sí lo hacen bien (no tocan el cursor). → No restaurar cursor de gameplay al ir a un menú; extraer helpers `SetGameplayCursor()`/`SetMenuCursor()`.

**BUG-06 — Botones de confirmación de salida/reset nunca cableados.**
`MainMenuUI.uxml` define `btn-exit-cancel`/`btn-exit-confirm` y `reset-progress-panel` (+ `btn-reset-progress-cancel`), pero `nav-exit` llama `QuitGame()` directo sin mostrar confirmación, y `reset-progress-panel` nunca pierde `.hidden` (su botón confirmar es inalcanzable). → Cablear el flujo de confirmación real.

**BUG-07 — Sin `OnDisable`: doble registro de callbacks y sesiones infladas.**
`MainMenuController` registra todo en `OnEnable` (clicks, hover, sliders) sin `OnDisable` que desregistre. Un disable/enable duplica navegación y broadcasts. Además `GameProgress.RecordSessionStarted()` en `OnEnable:151` infla el contador de sesiones en cada re-enable. → Mover `RecordSessionStarted` a `Start` (una vez) y añadir `OnDisable` con `Unregister`.

### 🟡 Medios

**BUG-08 — Slots de eco activos nunca se colorean (mismatch de clase CSS). [verificado]**
`GameHUD.cs:243` añade `hud-echo-slot--active`, pero `EchoesTheme.uss` solo define `hud-echo-slot--filled`. Los slots disponibles no reciben el color dorado. → Renombrar en código a `--filled`.

**BUG-09 — Doble registro de `GameOverController` al re-activar.**
`GameOverController.cs:68-71` registra `RegisterCallback<ClickEvent>` en cada `OnEnable` sin `Unregister` ni guardia `_initialized` (a diferencia de `PauseMenu`). Un disable/enable duplica retry/hub/menu. → Añadir guardia o desregistrar.

**BUG-10 — Selectores USS inexistentes → varios feedbacks visuales muertos.**
Código referencia clases que no existen en el USS:
- `preset-button--active` (resaltado de preset de iluminación/sensibilidad activo)
- `level-button--locked` / `level-button--completed` (estilo de nivel en el mapa)
- `scale-large` / `scale-xl` (escala de UI/texto — **completamente no funcional**)
→ Añadir los selectores o alinear al nombre existente (`level-card--active`).

**BUG-11 — La UI no es auto-sanadora; depende de rebuild manual del builder.**
`GameHUD`/`PauseMenu`/`GameOverUI`/`MainMenuUI` solo existen si el builder de editor corrió para esa escena (horneados en el `.unity`). A diferencia de `CinematicRecordingOverlay` y `GameStateController` (que se auto-crean vía `RuntimeInitializeOnLoadMethod`), la UI no tiene fallback. Escena editada a mano sin el objeto → sin HUD ni pausa. → Convertir la UI en prefab(s) instanciados por un bootstrap de runtime.

**BUG-12 — Transiciones inconsistentes; `SceneTransitionManager` solo se crea en MainMenu.**
Solo `LevelExit` y `MainMenuController` usan el fade; pausa/muerte/portal/GameOver cambian de escena directo. Además el builder solo crea el `SceneTransitionManager` en la escena MainMenu → si entras en Play directo en un `Level_*`, `Instance == null` y `LevelExit` cae al camino sin fade. → Enrutar todo por el manager y hacerlo auto-sanador (`RuntimeInitializeOnLoadMethod`).

**BUG-13 — Muchos labels del dashboard del menú apuntan a nombres inexistentes.**
`MainMenuController` consulta labels que no existen en el UXML, dejando features invisibles: `lbl-reset-hint` (feedback de confirmación de borrado en 2 pasos), `lbl-level-01..10` (estados BLOQUEADO/COMPLETO), barras `coherence`/`progress` (el UXML define `recall`/`errors`), `lbl-continue-hint`, `lbl-last-fragment`, `lbl-map-progress`, y los `lbl-preview-*` de `RefreshCalibrationPreview` (no-op completo). → Alinear nombres UXML↔código.

**BUG-14 — Doble sistema de hover pisándose.**
`MainMenuController.SetupHoverCallbacks` y `MenuHoverSystem.RegisterButton` registran ambos `MouseEnter/MouseLeave` sobre los mismos 4 botones. Además `MenuButtonHoverState.cs:300` setea `translate` **inline** que sobrescribe el `translate: 6px 0px` de `.nav-item-v2:hover` (los inline ganan a USS y no se limpian) → el deslizamiento de hover queda anulado. → Unificar en un solo sistema; quitar el `translate` inline.

**BUG-15 — Navegación con teclado/gamepad no cambia panel/ambiente/título.**
El cambio de preview-panel, mundo 3D y título están solo en `OnNavHover` (registrado en `MouseEnter`). Con mando/teclado (`FocusEvent`) no ocurren. → Enganchar `FocusEvent` al mismo handler; definir foco inicial y orden.

### ⚪ Bajos

**BUG-16 — Choque Pausa ↔ Game Over (si BUG-01 se arregla).** Estando en Game Over (`timeScale=0`), `PauseMenu.Update` sigue leyendo Escape → `Pause()`/`Resume()` puede des-pausar bajo el overlay. Falta un estado global de pausa único.

**BUG-17 — `CinematicRecordingOverlay` puede des-pausar.** `CinematicRecordingOverlay.cs:64-68` fuerza `timeScale=1` si `<1` al dejar de grabar; si coincide con pausa, la rompe. → Comprobar estado de pausa.

**BUG-18 — `GameHUD` no reintenta init.** Si `rootVisualElement` es null en el primer `OnEnable` (`GameHUD.cs:82`), el HUD queda permanentemente en blanco (no tiene el retry de `PauseMenu`).

**BUG-19 — Barras `hud-stability`/`hud-recall` y bloque "protocolo" del HUD cacheados pero nunca actualizados** (`GameHUD.cs:97-98,108-110`). UI visible sin datos.

**BUG-20 — `TutorialHUD` hace `FindAnyObjectByType<GameHUD>()` en cada llamada** (`75,88,98`). Cachear. Además `ShowObjective` ignora el parámetro `duration` → objetivo nunca se auto-borra; y `levelObjectives` está todo vacío.

**BUG-21 — Etiquetas engañosas.** `GameOverController` `btn-exit` ("✕ Cerrar") va a `GoToMenu()`, no a `Application.Quit()`. Botones "Hub" van a `MainMenu` (no existe escena Hub).

**BUG-22 — `MainMenu.cs` legacy divergente.** Escribe `AudioListener.volume` sin persistir; dos rutas de settings inconsistentes si conviviera con `MainMenuController`. → Eliminar.

**BUG-23 — Timeline con duración hardcodeada.** `GameHUD.cs:356`: `totalSecs = 10f` fijo; si `EchoRecorder.maxRecordSeconds ≠ 10`, el sello de tiempo es incorrecto.

**BUG-24 — `GetOrCreatePanelSettings` carga un `.uss` como `ThemeStyleSheet`.** `EchoesProductionBuilder.cs:840-855` (y duplicado en `EchoesLevelShell.cs`): `LoadAssetAtPath<ThemeStyleSheet>("...EchoesTheme.uss")` devuelve null (un `.uss` no es `.tss`) y la variable ni se asigna. Inofensivo hoy (el asset ya existe), pero si se borra, la regeneración crea un PanelSettings sin tema.

**BUG-25 — Fade uGUI sobre paneles UI Toolkit: orden de render no garantizado.** El `FadeCanvas` (uGUI, `sortingOrder=999`) y los `UIDocument` (UITK, sorting propio 0/10/20) usan sistemas de sorting independientes. Verificar visualmente que el fade cubre los paneles.

**BUG-26 — `StandaloneInputModule` legacy.** Los builders añaden `EventSystem` + `StandaloneInputModule`; si el proyecto usa el nuevo Input System, cualquier UI uGUI dependiente del EventSystem no recibiría eventos (UITK tiene su propio input). → Usar `InputSystemUIInputModule` si aplica.

**BUG-27 — Corrutinas de hover one-shot sin rastrear.** `MenuButtonHoverState.StopAllLoops` solo para 4 corrutinas; las one-shot (ghost/chroma/noise/echo/partículas) se acumulan con movimiento rápido del ratón.

**BUG-28 — Conflicto de niebla en el menú.** `ApplySettings` escribe `RenderSettings.fogDensity`, pero `MainMenuCinematicWorld.Update` lo sobrescribe cada frame → el slider "FogDensity" no tiene efecto y su valor inicial es inestable.

**BUG-29 — Encoding roto en UXML/USS.** Acentos mojibake (`ResoluciÃ³n`, `MÃºsica`) y placeholder `"0/15"` desactualizado. → Guardar como UTF-8.

---

## 4. Mejoras priorizadas

### Prioridad alta
1. **Arreglar mismatches UXML↔código** (BUG-03, 04, 08, 10, 13). Muchas features están invisibles. Recomendado: un test de arranque que haga `Q` de cada nombre esperado y logee los null como guardia de regresión.
2. **Conectar o eliminar Game Over** (BUG-01) y **completar `GameProgress` a 15 niveles** (BUG-02) — ambos rompen el flujo shipped.
3. **Ciclo de vida de callbacks** (BUG-07, 09): `OnDisable` con `Unregister`, `RecordSessionStarted` una sola vez, guardias `_initialized`.
4. **Gestión de cursor unificada** (BUG-05): helpers `SetGameplayCursor()`/`SetMenuCursor()`.

### Prioridad media
5. **Unificar el sistema de hover** (BUG-14) y **accesibilidad de foco** teclado/gamepad (BUG-15).
6. **Convertir la UI en prefabs + bootstrap de runtime** (BUG-11) para eliminar la dependencia de rebuild manual y el spawning duplicado en cada builder.
7. **Enrutar todas las transiciones por `SceneTransitionManager`** y hacerlo auto-sanador (BUG-12); validar escena y usar `try/finally` para garantizar `ResetFade`.
8. **Un único gestor de `Time.timeScale`/estado de pausa** compartido por Pausa/GameOver/overlay (BUG-16, 17).
9. **Decidir sobre Hub** (BUG-21): crear escena real o eliminar `HubPortal`/`HubSceneController` y renombrar botones.
10. **Eliminar `MainMenu.cs` legacy** (BUG-22).

### Prioridad baja (rendimiento / pulido)
11. **`GameHUD.Update`**: escribir `text`/`color`/clases solo ante cambios (hoy ensucia layout cada frame; crea `StyleColor` por frame). Las `Q<>` ya están cacheadas — el coste está en las escrituras.
12. **Cachear `GameHUD` en `TutorialHUD`** (BUG-20); retry-init en `GameHUD` (BUG-18).
13. **Migrar efectos de hover animables a USS** (`transition`/`@keyframes`) para reducir GC/corrutinas (BUG-27).
14. **Centralizar `GetOrCreatePanelSettings`** en un helper único y cargar el `.tss` correcto (BUG-24).
15. **Corregir encoding UTF-8** de UXML/USS y placeholders (BUG-29).
16. Alimentar `totalSecs` del timeline desde `EchoRecorder.maxRecordSeconds` (BUG-23).

---

## 5. Resumen ejecutivo

Los problemas de mayor impacto son **funcionalidad muerta por desconexión**, no crashes:
- **Game Over nunca se dispara** (BUG-01) y **`GameProgress` ignora 5 niveles** (BUG-02) → flujo de juego roto.
- **Decenas de elementos UXML↔código desalineados** (BUG-03, 04, 08, 10, 13) → título dinámico, stats de pausa, estados de nivel, escala de UI y feedback de ecos/iluminación **invisibles** (null-guardados, no crashean).
- **Ciclo de vida frágil** (BUG-05, 07, 09) → cursor bloqueado al menú, doble registro de callbacks, sesiones infladas.
- **Deuda arquitectónica**: dos sistemas de UI (UITK + uGUI legacy), UI horneada por builder sin fallback, transiciones inconsistentes, doble sistema de hover.

La causa raíz de la mayoría es que la UI y su UXML **evolucionaron por separado** sin una capa de verificación que garantice que cada nombre consultado en código exista en el UXML/USS.
