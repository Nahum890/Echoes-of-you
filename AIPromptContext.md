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
