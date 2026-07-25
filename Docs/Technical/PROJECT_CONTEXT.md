# PROJECT_CONTEXT.md
## Echoes of You — Contexto activo del proyecto

Este archivo es la fuente de verdad de contexto para cualquier IA que trabaje
en el proyecto. Leer completo antes de ejecutar cualquier tarea. Si algo en
este archivo contradice una sugerencia externa, este archivo gana.

Para dirección de diseño, reglas visuales, y filosofía del gameplay, leer
`ECHOES_BIBLE.md` — este archivo cubre arquitectura técnica.

---

## 1 — QUÉ ES EL JUEGO

**Echoes of You** — puzzle 3D narrativo en tercera persona, Universal Render
Pipeline (URP), Unity 2022+.

El jugador graba sus propios movimientos (hasta **12 segundos (estándar), 20 segundos (narrativo)** — ver `DECISION-ECH-DURATION` en `decisions.yaml`, tecla configurable)
y los reproduce como un "eco" — un cuerpo fantasma que repite exactamente lo
que grabó, con colisión activa. El eco no combate, no improvisa, no se puede
deshacer una vez reproducido. Esa irreversibilidad es el tema del juego.

---

## 2 — ARQUITECTURA ACTUAL (estado verificado, Julio 2026)

### Pipeline de generación de niveles

**Builder activo y único:** `Assets/Editor/EchoesNewProductionBuilder.cs`
(basado en ScriptableObjects `LevelBlueprint`, directorio `Assets/Data/Levels/`).

**Builders eliminados o inactivos:**
- `EchoesLevelBuilder.cs` — eliminado.
- `EchoesProductionBuilder.cs` — dividido en clases parciales de referencia
  (`EchoesProductionBuilder_Levels1_5.cs`, `_Levels6_10.cs`, `_Levels11_15.cs`).
  No ejecutar — es referencia histórica, no pipeline activo.

**ADVERTENCIA CRÍTICA:** `EchoesQueuedProductionRebuild.cs` tiene un flag
que puede auto-regenerar los 15 niveles al recompilar Unity si está activo.
Este flag debe estar desactivado en todo momento. Verificar antes de
cualquier tarea que modifique escenas.

**Scripts Python:** movidos a `Tools/Scripts/`. No ejecutar
`update_all_production.py` ni ningún script Python sobre archivos `.cs` —
el pipeline activo es solo C#.

### Archivos de sistemas core

| Archivo | Responsabilidad |
|---|---|
| `EchoesLevelShell.cs` | Atmósfera, cámara, jugador, UI por nivel |
| `EchoesModuleFactory.cs` | Construcción de geometría y módulos por `ModuleType` |
| `EchoesMaterialLibrary.cs` | Materiales del proyecto (tokens de color) |
| `PlayerController.cs` | Clase parcial — movimiento base, input, física de salto |
| `PlayerController_Gravity.cs` | Gravedad, zonas de gravedad, detección de suelo |
| `PlayerController_Animation.cs` | Animaciones, triggers, sonido de pisadas |
| `PlayerController_Visual.cs` | Setup visual, foco Cinemachine, links de avatar |
| `EchoRecorder.cs` | Grabación de movimientos del jugador |
| `EchoPlayback.cs` | Reproducción del eco grabado |

### Sistemas de puzzle disponibles

Estas son las primitivas mecánicas existentes. Úsalas como infraestructura.
No son puzzles completos por sí solas:

`PressurePlate`, `DoorController`, `TimedMovingPlatform`, `GhostBridge`,
`GravityZone`, `EchoKineticBody`, `EchoShieldField`, `EchoConflictTrap`,
`EchoDisintegrationZone`, `PuzzleWire` (AND/OR/latch/delay), `PuzzleCondition`
(contador N, secuencia, hold temporizado), `PuzzleSignal`.

### Render pipeline

**Universal Render Pipeline (URP).** Pipeline activo confirmado (Julio 2026).
`EchoesMaterialLibrary.cs` usa `"Universal Render Pipeline/Lit"` como shader base.
No usar `Shader.Find("Standard")` — resulta en shaders magenta en URP.
El post-processing usa URP Volume system. `EchoesURPConfigurator.cs` gestiona
la configuración de URP. `Metallic = 0`, `Smoothness = 0.05` son válidos en URP Lit.
No crear materiales fuera de `EchoesMaterialLibrary` — nunca materiales raw con shader Standard.

### Cámara

Hay dos sistemas de cámara en el proyecto: Cinemachine (activo en algunos
niveles) y `ThirdPersonCamera` (custom). Solo uno puede controlar el
transform en `LateUpdate` en un nivel dado — si ambos están activos al
mismo tiempo, hay jitter. Verificar que solo uno está activo por escena.

### §3C Camera Lifecycle Contract

```csharp
// [CAM-LIFECYCLE-001] Camera lifecycle contract
// ThirdPersonCamera.cs and Cinemachine CANNOT be active simultaneously.
// Activation protocol (called by EchoesLevelShell.cs):
public static void ActivateCinemachineForLevel(LevelBlueprint blueprint)
{
    // STEP 1: Disable ThirdPersonCamera if it exists
    ThirdPersonCamera tpc = FindObjectOfType<ThirdPersonCamera>();
    if (tpc != null)
    {
        tpc.enabled = false;    // Disable the component (do NOT destroy)
        // NOT Destroy() — Player prefab may be reused between levels
    }

    // STEP 2: Activate the level's Virtual Camera
    CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
    if (vcam == null)
        throw new System.Exception("FAIL-CAM-01: No CinemachineVirtualCamera found in scene.");

    vcam.enabled = true;
    vcam.Priority = 20;   // Higher priority than any other VCam in the scene

    // STEP 3: Apply blueprint profile
    CameraProfileApplicator.Apply(vcam, blueprint.cameraProfile);
}

// [CAM-LIFECYCLE-002] Validator check
// LEVEL_VALIDATOR runs this on each scene before build:
// Assert: no more than 1 ThirdPersonCamera component with enabled = true in hierarchy
```

---

## 3 — DIRECCIÓN VISUAL (resumen ejecutivo)

**Estética:** escuela liminal PS1/PS2 temprano. Low-poly, color plano,
niebla agresiva, iluminación dura.

**PROHIBIDO:** Modular SciFi MegaKit, Cyberpunk Kit, texturas PBR 2K/4K
de concreto o metal, monolitos, estética de ciencia ficción. Esta decisión
fue explícita y deliberada — no revertir.

**Materiales:** `Metallic = 0`, `Glossiness = 0.05`. Sin normal maps. Sin AO maps.
Sin reflejos de entorno.

**Tokens de color principales:**
- `echo-cyan` `#4FC3E8` — el eco, siempre
- `memory-amber` `#E8B262` — objetos narrativos, Lyra
- `corridor-navy` `#1C2430` — base de pasillos
- `wrongness-red` `#B23A3A` — peligro, uso escaso

**Niebla:** `SetupAtmosphere` DEBE usar los parámetros `blueprint.fogColor`
y `blueprint.ambientColor` reales. Hay un bug histórico donde estos valores
se hardcodeaban — verificar que la versión actual los lee. `fogDensity`
entre 0.012 y 0.04. `AmbientMode.Flat`.

---

## 4 — ESTADO DE LOS NIVELES

15 escenas en disco. Organización por capítulos emocionales:

| # | Emoción | Capítulo | Espacio principal |
|---|---|---|---|
| 1 | Desorientación | I — Persistencia | Entrada + pasillo que se repite |
| 2 | Repetición | I — Persistencia | Pasillo de aulas idénticas |
| 3 | Indecisión | I — Persistencia | Bifurcación de pasillos especulares |
| 4 | Espera | II — Coordinación | Aula con timing vertical |
| 5 | Culpa | II — Coordinación | Pasillo laberinto de timing horizontal |
| 6 | Negación | III — Confianza | Biblioteca, puente espectral |
| 7 | Evasión | III — Confianza | Patio trasero, grabación anticipada |
| 8 | Autosabotaje | II — Coordinación | Sala de profesores, dos ecos |
| 9 | Control | III — Confianza | Patio exterior — único espacio abierto |
| 10 | Recuerdos | IV — Optimización | Aula de Lyra, eco ambiental revelador |
| 11 | Conexión | IV — Optimización | Escalera central, grabación limitada |
| 12 | Conflicto | V — Consecuencia | Gimnasio/laboratorio |
| 13 | Verdad | V — Consecuencia | Aula de Lyra fragmentada, grabación única |
| 14 | Aceptación | VI — Aceptación | Fragmentos flotantes en void-black |
| 15 | Integración | VI — Aceptación | Pasillo del Nivel 1, ahora con salida real |

**Estado de conteo:** 15 escenas en disco. `GameProgress` y `MainMenu`
pueden tener conteos distintos — verificar coherencia antes de modificar
cualquier archivo de progreso.

---

## 5 — LO QUE NO HACER (errores documentados de sesiones anteriores)

- **No revertir la dirección visual a brutalismo/sci-fi.** Ya se hizo, ya se
  deshizo. Hay razones concretas documentadas en `ECHOES_BIBLE.md`.
- **No crear builders adicionales.** Solo `EchoesNewProductionBuilder.cs`.
- **No ejecutar scripts Python sobre archivos `.cs`.** El pipeline es solo C#.
- **No usar `Shader.Find("Standard")` ni materiales Built-in.** El pipeline
  es URP — los materiales deben usar `"Universal Render Pipeline/Lit"`.
  Todos los materiales se crean exclusivamente vía `EchoesMaterialLibrary`.
- **No usar constantes `SciFi*` en ningún script de producción.**
- **No activar `EchoesQueuedProductionRebuild` sin ser consciente de que
  borra ediciones manuales de las 15 escenas sin aviso.**
- **No renombrar sistemas como sustituto de rediseñarlos.**
- **No construir más de 3 niveles nuevos antes de validar que los
  primeros 3 funcionan con jugadores reales.**

---

## 6 — CÓMO USAR ESTE ARCHIVO CON UNA IA

1. Dar este archivo + `ECHOES_BIBLE.md` antes de cualquier tarea.
2. Pedir a la IA que señale explícitamente si su propuesta contradice
   algo en alguno de los dos documentos, en lugar de aplicarlo en silencio.
3. Cualquier cambio de dirección (dirección visual, mecánica central,
   estructura de campaña) requiere actualizar este archivo antes de ejecutar.
4. Si la IA no tiene acceso a los archivos `.cs` actuales, pedirle que
   señale qué necesita leer antes de proponer cambios de código —
   no que adivine firmas de función.
