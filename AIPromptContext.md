# Contexto del Proyecto: "Echoes of You"

Eres un Asistente / Level Designer Senior trabajando en el proyecto de Unity "Echoes of You". 

## 1. Visión General del Juego
* **Género:** Puzzle 3D Minimalista.
* **Mecánica Central:** El jugador puede moverse y saltar. Mantiene presionada la tecla 'R' (hasta 10 segundos) para grabar sus movimientos. Al soltarla, aparece un "eco" (clon fantasma) que repite exactamente lo que hizo el jugador.
* **Objetivo:** Abrir puertas interactuando con placas de presión (que pueden ser pisadas por el jugador o los ecos) para llegar a la salida (LevelExit).

## 2. Puntos Clave de la Arquitectura
* **Todo se genera con código:** Hay un script de editor llamado `Assets/Editor/EchoesLevelBuilder.cs` que construye ABSOLUTAMENTE TODO.
  * **¡URGENTE!** En el menú de Unity, bajo **"Echoes of You > FIX — Rebuild Everything From Scratch"**, se puede borrar y regenerar limpiamente todos los Materiales (usando Standard shader), el Prefab del Eco, el Menú Principal, y todos los Niveles. Las escenas se añaden a los Build Settings automáticamente.
* **Scripts Core:**
  * `PlayerController.cs`: Movimiento de CharacterController, gravedad y saltos.
  * `EchoRecorder.cs` y `EchoPlayback.cs`: Graban los `Vector3` y rotaciones y spawnean ecos.
  * `PressurePlate.cs`: Detectan colliders con tag `Player` o `Echo`.
  * `DoorController.cs`: Compuertas lógicas AND; se abren solo si *todas* sus placas asignadas están presionadas.
* **Interfaces Estéticas (OnGUI):**
  * `MainMenu.cs`, `PauseMenu.cs`, y `TutorialHUD.cs` / `GameHUD.cs` controlan la UI sin depender de lienzos uGUI voluminosos.


## 3. Asignación Actual (Tu Misión como AI)

El usuario quiere expandir la duración del juego porque los primeros 3 niveles se completan muy rápido. 

**Objetivo 1:** Diseñar y programar nuevos niveles de mayor complejidad:
* Tienen que ser más difíciles, requiriendo coordinación exacta entre múltiples ecos y el jugador (el límite de grabaciones es configurable).
* Debes actualizar el archivo `EchoesLevelBuilder.cs` o escribir métodos nuevos (ej. `BuildLevel04()`, `BuildLevel05()`) y añadirlos a `BuildAll()`.
* Utiliza una topología más vertical, pasillos con bloqueos, plataformas móviles precisas (`TimedMovingPlatform.cs`), y dobles tiempos de espera.

**Objetivo 2:** Mejora Visual
* Aprovecha mejor los materiales y funciones que ya genera `EchoesLevelBuilder.cs`, dándole un toque de color, luz direccional más contrastada (sombras más duras o de colores), y mejorando el acomodo estético del entorno dentro de la simplicidad low-poly que tiene.

Recuerda: Si algo se rompe tras tus cambios, siempre pide al usuario que corra el comando "FIX — Rebuild Everything From Scratch" del menú de Unity `Echoes of You`.
