# UX_SYSTEM_SPEC.md — Especificación Técnica de UX para Echoes of You 2.0

## 1. Propósito y autoridad
- **SPEC‑UX‑001** – define la arquitectura, estilo visual, flujo de estados y canales de feedback para toda la UI del juego.
- **Nivel** – 3 (subordinado a `UI_SPEC.md` (SPEC‑008) y a la documentación de diseño del proyecto).
- **Alcance** – Todas las pantallas UI basadas en UI Toolkit (`UIDocument`). Excluye UI 3D world‑space (texto en mundo) y menús legacy de uGUI.

## 2. Preguntas clave de UX (respuestas oficiales)
| Pregunta | Respuesta |
|---|---|
| 1. ¿Qué necesita saber el jugador **ahora**? | *Objetivo actual*, *estado de grabación*, *cuántos ecos tiene*, *si hay interacción disponible*, *si una narración está activa*.
| 2. ¿Cuándo lo necesita? | **Instantáneamente** al cambiar de estado (p.ej., al iniciar/grabar/terminar un Eco, al entrar en un puzzle, al activar diálogo, al pausar).
| 3. ¿Dónde debe mostrarse? | **HUD** (capa base) para información constante; **Paneles superpuestos** (pausa, settings, VN) con fondo semitransparente que deje visible el mundo; **Prompt** centrado para interacción.
| 4. ¿Qué información debe desaparecer? | Prompt cuando el objeto sale del rango; HUD cuando no hay grabación (opacidad 0); diálogos al terminar; mensajes toast después de su duración.
| 5. ¿Qué puede comunicarse mediante sonido o animación? | *Límites de grabación*, *éxitos/fallos de puzzle*, *errores críticos* (p.ej., grabación muy corta), *cambio de estado (play/pause)*, *feedback de interacción (clic, hover)*.

## 3. Modelo de estados UI
```csharp
public enum UXState {
    GameplayIdle,
    InteractionAvailable,
    Recording,
    RecordingLimitWarning,
    RecordingComplete,
    EchoSpawned,
    EchoPlayback,
    PuzzleSuccess,
    PuzzleFailure,
    DialogueActive,
    ChoiceActive,
    Paused,
    Settings,
    Ending
}
```
### Tabla de reacción por estado
| Estado | Triggers | Visible UI | Oculto | Audio | Animación |
|---|---|---|---|---|---|
| **GameplayIdle** | `GameState == Exploration && !IsRecording && !CurrentTarget` | Objetivo (texto), barra de estabilidad mínima | HUD completo (opacidad 0) | Ambient “idle” (bajo) | Fade‑in 0.1 s (opacidad →0.85) al pasar a Recording |
| **InteractionAvailable** | `InteractionPrompt.Show` | Prompt `[E] <texto>` (cyan/amber) | HUD | `ui_click` (SFX) | Slide‑up 0.15 s, escala 1.02 |
| **Recording** | `EchoRecorder.RecordingStarted` | HUD barra rojo‑cian, contador de tiempo, dot `REC` | Prompt oculto | `record_start` (loop) | Bar fill 0→1, overlay VHS `CinematicRecordingOverlay` fade‑in 0.2 s |
| **RecordingLimitWarning** | `elapsed / max >= 0.85` | Barra amber pulse, texto “¡casi al límite!” | – | `warning_beep` (rápido) | Pulse 1 Hz sobre barra |
| **RecordingComplete** | `EchoRecorder.RecordingStopped(true)` | Toast “Eco guardado (N/M)”, disminución barra →0 | Prompt oculto | `record_stop` | Fade‑out toast 0.4 s |
| **EchoSpawned** | `EchoRecorder.EchoCreated` | Slot de eco coloreado cyan + pulso | – | `echo_spawn` | Slot pulse 0.3 s |
| **EchoPlayback** | `EchoPlayback.LifecycleChanged(Playback)` | Indicador de playback (barra pequeña), eco semi‑transparente | HUD opaco 0.85 | `echo_playback` | Fade‑in 0.2 s, movimiento suave |
| **PuzzleSuccess** | `PuzzleCondition.ConditionChanged(true)` | Toast cyan “Puzzle resuelto”, foco cámara corto | – | `puzzle_success` | Camera focus + vignette 0.2 s |
| **PuzzleFailure** | `PuzzleCondition.ConditionChanged(false)` | Toast rojo “Error”, pantalla leve tint rojo | – | `puzzle_fail` | Screen flash 0.1 s |
| **DialogueActive** | `VN_DialogueController.IsActive` ó `VN_OverlayController.IsOpen` | Panel diálogos (texto, nombre, retrato Aiden) sobre fondo semitransparente | HUD → opacidad 0 | `dialogue_typewriter` + `voice_clip` | Cross‑fade 0.3 s, foco cámara evento |
| **ChoiceActive** | `VN_ChoiceGateController.IsShowing` | Panel de elección con botones cyan / amber | HUD | `choice_select` / `choice_confirm` | Botones con foco ring 0.15 s, transición 0.3 s |
| **Paused** | `PauseMenu.Open` | Notebook panel (stats, objetivo, botones) | HUD → 0 | `pause_open` | Panel slide‑in 0.2 s |
| **Settings** | `SettingsController.Open` | Panel de pestañas (Audio, Video, Controls, Accesibilidad, Gameplay) | Pausa | `settings_open` | Tab cross‑fade 0.15 s |
| **Ending** | `VN_EpilogueController.Resolve` | Epílogo full‑screen con voz Aiden, botón continuar | Todo lo anterior oculto | `ending_voice` | Fade‑in 0.5 s |

## 4. Reglas visuales (RULE‑UX‑001…)
| Regla | Descripción |
|---|---|
| **RULE‑UX‑001** | *Exclusividad UI Toolkit*: 100 % de los menús deben ser `UIDocument`. No se permite `Canvas` ni `OnGUI` salvo caso de migración (ya planificado). |
| **RULE‑UX‑002** | *Transiciones de foco*: `:focus` y `:hover` deben usar `transition-duration: 0.15s` (ver `EchoesHoverEffects.uss`). |
| **RULE‑UX‑003** | *Navigación dual*: soporte teclado (`WASD/Arrows`, `Enter`, `Escape`) y gamepad (`D‑Pad`, `South`, `East`, `Start`). Implementar con `InputSystemUIInputModule`. |
| **RULE‑UX‑004** | *Liminal HUD Opacity*: HUD base = 0, sube a 0.85 **solo** cuando `EchoRecorder.isRecording == true`. Fade 0.10 s. |
| **RULE‑UX‑005** | *Feedback multicanal crítico*: cualquier error, éxito de puzzle, límite de grabación **debe** tener al menos **visual + audio + animación**. |
| **RULE‑UX‑006** | *No sobrescribir juego*: los paneles nunca deben bloquear la cámara o impedir que el jugador se mueva a menos que estén en modo modal (`FocusManager.TrapFocus`). |
| **RULE‑UX‑007** | *Identidad visual*: Mantener logo (`Assets/UI/logo.png`) y retratos de Aiden. Rojo solo como **énfasis**, nunca como color primario. |
| **RULE‑UX‑008** | *Accesibilidad*: `highContrast` y `reduceMotion` deben afectar a **todos** los `VisualElement` vía CSS custom properties (`--high-contrast`, `--reduce-motion`). |
| **RULE‑UX‑009** | *Escalado UI*: `UIScale` (Small/Default/Large/XL) se lee de `PlayerPrefs` y se aplica vía `GameHUD.ApplySavedUIScale()`. |
| **RULE‑UX‑010** | *Tipografía*: Fuente `Inter` (sans‑serif) en todos los textos UI, peso 400‑600. No se permiten fuentes manuscritas. |

## 5. Layout métricos (referencia a `HUD_LAYOUT_METRICS_SPEC.yaml`)
- Safe‑area top = 80 px, bottom = 80 px.
- HUD Container anchored **top‑right** (record panel) y **bottom‑left** (slots). 
- Prompt centrado, ancho máximo 320 px, margen 12 px. 
- Modal de pausa ancho 640 px, alto 480 px, centered. 
- Settings panel ancho 720 px, barra lateral 180 px.

## 6. Mapeo de audio (referencia a `EchoesAudioManager` groups)
| Acción | AudioGroup | Clip / descripción |
|---|---|---|
| Record start | `Echo` | `record_start_loop` (loop 0.8 s) |
| Record stop | `Echo` | `record_stop` (corte) |
| Echo spawn | `Echo` | `echo_spawn` (short chirp) |
| Puzzle success | `SFX` | `puzzle_success` (ding) |
| Puzzle fail | `SFX` | `puzzle_fail` (buzz) |
| Dialogue line | `SFX` | voice clip (per‑line) |
| Choice confirm | `SFX` | `choice_confirm` |
| Pause open/close | `Music` | `pause_fade_in/out` |
| Settings toggle | `SFX` | `toggle_click` |
| Ending voice | `Echo` | `ending_voice` |

## 7. Navegación y foco (referencia a `UI_NAVIGATION_GRAPH.yaml`)
- **ScreenLayers**: `Base (Gameplay)`, `Overlay (Pause/Settings/Dialogue)`, `Modal (Confirmations)`, `Loading`.
- Cada `NavigationManager.Push(screenId, root, layer)` crea una pila; `FocusManager.TrapFocus(root)` al abrir modal.
- Gamepad foco inicia en el primer `Button` con `:focus-visible`.

## 8. Glosario de terminología (renombrado sci‑fi → liminal)
- *Neural Archives* → **Archivo de Memoria**
- *Calibration* → **Calibración**
- *Protocol* → **Procedimiento**
- *Telemetry* → **Registro**
- *Schematic* → **Esquema**
- *Archivist* → **Archivista**
- *Disconnect* → **Desconexión**
- *Stability* → **Consistencia**
- *Node* → **Núcleo**
- *Integrity* → **Integridad**

## 9. Referencias cruzadas
- `UI_SPEC.md` (RULE‑UI‑001…‑004) – base del HUD y pantalla principal.
- `HUD_LAYOUT_METRICS_SPEC.yaml` – métricas exactas de posición.
- `uitk_schema.yaml` – convenciones de naming de UXML/USS.
- `UI_NAVIGATION_GRAPH.yaml` – grafo de navegación por gamepad/teclado.

---
*Este documento constituye la guía de desarrollo para todas las fases UI que se ejecutarán a continuación.*