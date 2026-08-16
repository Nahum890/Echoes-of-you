# AUDITORÍA COMPLETA DE UX – Echoes of You 2.0

## 1. Resumen ejecutivo
- **Puntuación UX /10:** **2.5** (según `AUDITORIA_COMPLETA.md`).
- **Principales problemas:** UI fragmentada (UITK + IMGUI), terminología sci‑fi incongruente, múltiples bugs críticos (BOT‑01…‑29), ausencia de eventos centralizados de estado, falta de accesibilidad completa, y **reglas visuales** poco aplicadas.

## 2. Inventario de sistemas UI existentes
| Sistema | Archivo principal | UXML / USS | Comentario |
|---|---|---|---|
| HUD Gameplay | `Assets/Scripts/GameHUD.cs` | `GameHUDUI.uxml / .uss` | Opacidad 0→0.85 solo durante `EchoRecorder.isRecording` (RULE‑UI‑004). Falta coloreado de slots (BUG‑08) y barra de tiempo hardcodeada (BUG‑23).
| Prompt de interacción | `InteractionPromptController.cs` + `InteractionPromptInitializer.cs` | `InteractionPromptUI.uxml / .uss` | Muestra `[E] <texto>`; no tiene eventos de audio/anim.
| Menú principal | `MainMenuController.cs` | `MainMenuUI.uxml / .uss` | Monolito de 1 180 líneas, terminología sci‑fi, héroe‑título ausente (BUG‑03).
| Menú de pausa | `PauseMenu.cs` | `PauseMenuUI.uxml / .uss` | Oculta HUD, bloquea cámara; sin estadísticas visibles (BUG‑04) y cursor bloqueado al volver (BUG‑05).
| Settings | `SettingsController.cs` | `SettingsUI.uxml / .uss` | 5 pestañas; `EchoTabs.uxml` tiene cierre `</uxXML>` inválido; sci‑fi en descripciones.
| VN Diálogo (UITK) | `VN_DialogueController.cs` | `VN_DialogueUI.uxml / .uss` | Bien estructurado, usa UI Toolkit.
| VN Overlay (IMGUI) | `VN_OverlayController.cs` | **IMGUI** (`OnGUI`) – usado para inspecciones.
| VN Choice Gate (IMGUI) | `VN_ChoiceGateController.cs` | **IMGUI** (`OnGUI`).
| Loading Screen | `LoadingScreenController.cs` | **IMGUI** (`OnGUI`).
| Input | `InputActionMap.cs` | Wrapper legacy sobre `UnityEngine.Input`.
| Audio | `EchoesAudioManager.cs`, `GameFeelController.cs` | Mixer con grupos Master/Music/SFX/Echo.
| Navegación | `NavigationManager.cs`, `FocusManager.cs` | Sistema de pantalla‑stack y foco.

## 3. Hallazgos por subsistema
### HUD
- No muestra título del nivel ni objetivo cuando el jugador está quieto (estado `GAMEPLAY_IDLE`).
- Slots de eco nunca cambian de color (BUG‑08).
- Barra de grabación está hardcodeada a 10 s (BUG‑23) mientras `EchoRecorder.maxRecordSeconds` es 6 s.
- Falta feedback visual de *record limit warning* y *playback residual*.
### Interacción
- Prompt no reproduce sonido ni animación de aparición.
- Se muestra aun cuando la pantalla VN está abierta (debe suprimirse).
### Menú principal y pausa
- Terminología sci‑fi (Neural Archives, Calibration, Protocol) incongruente con la estética liminal.
- Falta hero‑title en Main Menu (`BUG‑03`).
- Stats de pausa invisibles (`BUG‑04`).
- Cursor queda bloqueado al cerrar pausa (`BUG‑05`).
- Salida del juego sin confirmación (`BUG‑06`).
- Doble sistema de hover (`BUG‑14`).
### Settings
- `EchoTabs.uxml` cierra con `</uxXML>` (parse error). 
- Terminología sci‑fi en descriptores.
- Falta soporte de `highContrast` y `reduceMotion` en UI visual.
### VN / Visual Novel
- `VN_OverlayController` y `VN_ChoiceGateController` usan IMGUI, violando `[RULE‑UI‑001]`. 
- No existe transición suave entre gameplay y VN (no hay cross‑fade ni foco de cámara). 
- Aiden sprites dependen de `AidenStageResolver` – correcto pero UI está en IMGUI.
### Input
- No se usa el paquete `com.unity.inputsystem`. El proyecto depende de `InputActionMap` wrapper, lo que impide la navegación por gamepad en UI (`BUG‑15`).
### Accesibilidad
- El panel de Accesibilidad está presente pero no se propaga a HUD/VN (p.ej., `reduceMotion` no afecta animaciones de HUD).

## 4. Bugs críticos a corregir *in‑module*
- **BUG‑03**: Hero‑title ausente en `MainMenuUI.uxml`.
- **BUG‑04**: Stats de pausa invisibles en `PauseMenuUI.uxml`.
- **BUG‑05**: Cursor bloqueado al cerrar pausa.
- **BUG‑06**: Salida sin confirmación.
- **BUG‑08**: Slots de eco nunca coloreados.
- **BUG‑10**: Selectores USS faltantes (hover/focus).
- **BUG‑13**: Queries a UXML inexistentes (MainMenu).
- **BUG‑15**: Navegación por teclado / gamepad no funciona.
- **BUG‑23**: Timeline hardcodeada a 10 s.
- **BUG‑26**: Falta `InputSystemUIInputModule`.
- **BUG‑29**: Encoding mojibake en varios UXML/USS.
- **SCI‑FI → Liminal**: Renombrar terminología en todos los textos UI.
- **EchoTabs.uxml**: Cambio a `</UXML>`.

## 5. Recomendaciones de alto nivel
1. **Migrar a Input System** – instalar `com.unity.inputsystem` y reemplazar `StandaloneInputModule` por `InputSystemUIInputModule` (cumple `RULE‑UI‑003`).
2. **Unificar eventos** – agregar eventos aditivos a `GameStateController`, `EchoPlayback`, y `PuzzleCondition`/`GoalTrigger` para que la UI suscriba (decisión aceptada).
3. **Migrar VN IMGUI → UITK** – los tres controladores (`VN_OverlayController`, `VN_ChoiceGateController`, `LoadingScreenController`) se reescriben como `UIDocument` con sus UXML correspondientes.
4. **Crear fase extra de Main Menu** – reparar hero‑title, eliminar sci‑fi, actualizar UI y añadir foco de teclado.
5. **Iterar fase‑por‑fase** siguiendo el orden solicitado: HUD → Interacción → Echo Feedback → Puzzle Feedback → Pausa → Diálogo → Elección → Settings → Main Menu.
6. **Documentar**: generar `Docs/Audit/UX_COMPLETE_AUDIT.md` (este documento) y `Docs/Specs/UX_SYSTEM_SPEC.md` (especificación técnica completa). Cada fase producirá `Docs/Audit/MODULE_XX_*.md` con criterios de aceptación, casos de prueba y limitaciones.

## 6. Glosario de terminología (remplazado)
| Sci‑fi | Liminal / Escolar |
|---|---|
| Neural Archives | **Archivo de Memoria** |
| Calibration | **Calibración** (mantener) |
| Protocol | **Protocolo** (renombrado a *Procedimiento*) |
| Telemetry | **Telemetría** → *Registro* |
| Schematic | **Esquema** |
| Archivist | **Archivista** |
| Disconnect | **Desconexión** |
| Stability | **Estabilidad** → *Consistencia* |
| Node | **Núcleo** |
| Integrity | **Integridad** |

---
*Este documento será la base de la fase de auditoría antes de la implementación.*