# SCENE_TRANSITION_SPEC.md — Scene Manager & Level Transition Specification
## Spec ID: SPEC-115
## Version: 4.0 (AI-Executable)

---

### 1. PURPOSE
Defines exact asynchronous scene loading sequences, screen fader timings, progress saving contracts, and LevelExit trigger execution for *Echoes of You 2.0*.

### 2. SCOPE
Applies to `GoalTrigger.cs`, `LevelExit`, `ScreenFader.cs`, `SaveSystem.cs`, and `SceneManager` async operations.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `ROOM_LIBRARY.md` (`SPEC-004`).

### 4. DEFINITIONS
- `ScreenFader`: UI Overlay handling screen color fades (`#000000`).
- `Async Load`: Non-blocking scene activation via `AsyncOperation.allowSceneActivation`.

### 5. INPUTS
- [SAVE_DATA_SCHEMA.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SAVE_DATA_SCHEMA.md) `[SPEC-119]`
- [UI_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/UI_SPEC.md) `[SPEC-008]`

### 6. OUTPUTS
- Asynchronous level transitions driven by `GoalTrigger.cs`.

### 7. RULES

- `[RULE-TRN-001]`: **Fade Out Duration**: Screen Fader MUST fade to black (`#000000`) in $0.5\text{s} \pm 0.0\text{s}$.
- `[RULE-TRN-002]`: **State Persistence**: Synchronous JSON save operation MUST execute immediately after Fade Out completes before scene unloading.
- `[RULE-TRN-003]`: **Fade In Duration**: Upon scene activation completion, Screen Fader MUST fade from black to clear in $1.0\text{s} \pm 0.0\text{s}$.

### 8. ALGORITHMS

#### Algorithm 8.1: Level Transition Contract in C# (HALT-9)

```csharp
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelTransitionManager
{
    // Contrato de transición de escena
    // Llamado por GoalTrigger.cs cuando el jugador alcanza el LevelExit
    public static async void LoadNextLevel(int currentLevelIndex)
    {
        // 1. Fade Out: 0.5s a negro (#000000)
        await ScreenFader.FadeOut(0.5f);
        
        // 2. Guardar estado: sincrónico, JSON serialization
        SaveSystem.SaveProgress(currentLevelIndex + 1, EchoMemoryBank.unlockedMemories);
        
        // 3. Load siguiente escena asincrónicamente
        string sceneName = $"Level_{(currentLevelIndex + 1):D2}_Name";
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;
        while (op.progress < 0.9f) await Task.Yield();
        
        // 4. Activar nueva escena
        op.allowSceneActivation = true;
        
        // 5. Fade In: 1.0s desde negro
        await ScreenFader.FadeIn(1.0f);
    }
}
```

### 9. CONSTRAINTS
- `[CONS-TRN-001]`: Prohibido synchronous blocking `SceneManager.LoadScene()` calls on the main thread.

### 10. VALIDATION
- `[VAL-TRN-001]`: Playtest harness confirms scene activation occurs strictly when `progress >= 0.9f`.

### 11. EXAMPLES
- C# async method above.

### 12. FAILURE CASES
- `[FAIL-TRN-001]`: **Missing Scene in Build Settings**: Scene name missing from Build Settings. Result: SceneManager logs `FAIL-TRN-01`.

### 13. CROSS REFERENCES
- [SAVE_DATA_SCHEMA.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/SAVE_DATA_SCHEMA.md) `[SPEC-119]`

### 14. CHANGE HISTORY
- **v4.0 (2026-07-25)**: Created canonical SPEC-115 resolving HALT-9 Scene Manager transition API specification.
