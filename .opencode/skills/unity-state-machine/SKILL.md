---
name: unity-state-machine
description: Implementación de state machines en Unity para el proyecto Echoes of You: enum-based FSM, transiciones, GameFlowState, y patrones para sistemas de juego (player, eco, grabación)
license: MIT
compatibility: opencode
---

## Patrones de State Machine para Echoes of You

### Enum-based FSM (patrón usado en el proyecto)
- Definir estados como `public enum GameFlowState { Exploration, Recording, PlayerDead, LevelCompleted, Restarting }`
- Almacenar el estado actual como propiedad: `public GameFlowState CurrentState { get; private set; }`
- Centralizar cambios de estado en método `SetState()` con lógica de entrada/salida

### Estructura recomendada
```csharp
public enum MyState { Idle, Active, Completed }

public class MySystem : MonoBehaviour
{
    public MyState CurrentState { get; private set; } = MyState.Idle;

    void SetState(MyState newState)
    {
        if (newState == CurrentState) return;
        ExitState(CurrentState);
        CurrentState = newState;
        EnterState(CurrentState);
    }

    void EnterState(MyState state) { /* switch */ }
    void ExitState(MyState state) { /* switch */ }
}
```

### GameFlowState (ya implementado en GameStateController)
- `Exploration` → estado por defecto, movimiento libre
- `Recording` → entrada: activar cámara de seguimiento, desactivar HUD contexto; salida: spawnear eco
- `PlayerDead` → activar cámara de muerte, timer de restart
- `LevelCompleted` → secuencia de fin de nivel, transición a siguiente escena
- `Restarting` → bloqueo de input, reload de escena

### Sistema de grabación y eco
- Estados: `Idle` → `Recording` → `Playing` (EchoPlayback)
- La grabación captura `RecordFrame` (posición, rotación, tiempo)
- Usar `event Action<int> EchoCreated` para notificar cambios de estado

### Transiciones seguras
- Verificar `if (newState == CurrentState) return;` para evitar re-entrada
- Usar corrutinas para transiciones con retardo: `StartCoroutine(RestartSceneRoutine(delay))`
- Estado `Restarting` con flag `_restartQueued` para prevenir múltiples restarts

### Buenas prácticas
- Estados inmutables (no modificar desde fuera, solo a través de métodos públicos como `SetRecording()`, `NotifyPlayerDeath()`)
- Eventos para comunicación entre sistemas en lugar de acoplamiento directo
- Singleton pattern (`public static GameStateController Instance`) para acceso global al state machine
