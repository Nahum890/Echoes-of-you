# ECHOES OF YOU — Documento de Diseño de Niveles

> Documento técnico de construcción para Unity.
> Cada nivel usa EXCLUSIVAMENTE los scripts existentes del proyecto.

---

## Referencia Rápida de Scripts

| Script                 | Función clave                                                        |
|------------------------|----------------------------------------------------------------------|
| `PlayerController`     | CharacterController, moveSpeed=5, jumpHeight=1.25, gravity=-24       |
| `EchoRecorder`         | Mantener **R** para grabar (máx 10 s, máx 3 ecos)                   |
| `EchoPlayback`         | Reproduce un bucle con CharacterController                           |
| `PressurePlate`        | Trigger que detecta tags **"Player"** y **"Echo"**                   |
| `DoorController`       | AND de todas las plates → mueve localPosition (closed→open)          |
| `TimedMovingPlatform`  | Mueve entre inactiveLocal/activeLocal mientras plate está presionada |
| `LevelExit`            | Trigger con tag "Player" → carga siguiente escena                    |
| `GameHUD`              | OnGUI: muestra ecos y barra de grabación                             |
| `ThirdPersonCamera`    | Sigue al target con offset, ratón para yaw/pitch                     |

### Tags requeridos (configurar en Project Settings → Tags)
- `Player`
- `Echo`

### Layers recomendados
- `Ground` (para groundCheck del PlayerController)

---

## Prefabs necesarios (crear una vez)

### Prefab: EchoPrefab
1. Crear un **Empty GameObject** llamado `EchoPrefab`
2. Agregar **CharacterController** (Height=1.8, Radius=0.3, Center Y=0.9)
3. Agregar componente **EchoPlayback**
4. Como hijo, crear un **Cube** con Scale (0.6, 1.8, 0.6), Material azul semi-transparente
5. Tag = **"Echo"**
6. Guardar como Prefab en `Assets/Prefabs/EchoPrefab.prefab`

### Prefab: Player
1. Crear **Empty GameObject** llamado `Player`
2. Agregar **CharacterController** (Height=1.8, Radius=0.3, Center Y=0.9)
3. Agregar **PlayerController**
4. Agregar **EchoRecorder** → Asignar `echoPrefab` al prefab de arriba
5. Como hijo, crear un **Cube** con Scale (0.6, 1.8, 0.6), Material blanco
6. Como hijo, crear **Empty** llamado `GroundCheck` en localPosition (0, 0.1, 0)
7. Tag = **"Player"**
8. Guardar como Prefab en `Assets/Prefabs/Player.prefab`

---

# NIVEL 1 — "El Primer Eco"

## 1. Objetivo del Nivel

El jugador aprende la mecánica básica: grabar un eco para mantener presionado un botón mientras cruza una puerta.

## 2. Lista de Objetos

| #  | Nombre              | Tipo                | Script(s)                      |
|----|---------------------|---------------------|--------------------------------|
| 1  | Player              | Prefab              | PlayerController, EchoRecorder |
| 2  | MainCamera          | Camera              | ThirdPersonCamera              |
| 3  | GameHUD             | Empty               | GameHUD                        |
| 4  | Floor_Start         | Cube                | —                              |
| 5  | Floor_Button        | Cube                | —                              |
| 6  | Floor_Gate          | Cube                | —                              |
| 7  | Floor_End           | Cube                | —                              |
| 8  | PressurePlate_1     | Cube + BoxCollider  | PressurePlate                  |
| 9  | DoorFrame           | Empty (padre)       | —                              |
| 10 | Door                | Cube                | DoorController                 |
| 11 | LevelExit           | Cube (trigger)      | LevelExit                      |
| 12 | DirectionalLight    | Light               | —                              |

## 3. Posiciones, Rotaciones y Escalas

```
PLATAFORMAS (Floor)
──────────────────────────────────────────────────────────
Objeto            | Position              | Scale
──────────────────────────────────────────────────────────
Floor_Start       | (0, 0, 0)             | (6, 0.5, 6)
Floor_Button      | (0, 0, 9)             | (4, 0.5, 4)
Floor_Gate        | (0, 0, 16)            | (4, 0.5, 3)
Floor_End         | (0, 0, 23)            | (6, 0.5, 6)

JUGADOR
──────────────────────────────────────────────────────────
Player            | (0, 1.5, 0)           | (1, 1, 1)

CÁMARA
──────────────────────────────────────────────────────────
MainCamera        | (0, 4, -6)            | (1, 1, 1)
  → ThirdPersonCamera.target = Player.transform

BOTÓN
──────────────────────────────────────────────────────────
PressurePlate_1   | (0, 0.35, 9)          | (1.5, 0.1, 1.5)
  → Material: Verde
  → BoxCollider.isTrigger = true (se fuerza en Awake)

PUERTA
──────────────────────────────────────────────────────────
DoorFrame (Empty) | (0, 0.25, 13.5)       | (1, 1, 1)
  └─ Door (Cube)  | localPos (0, 0, 0)    | (3, 2.8, 0.3)
     → DoorController:
       plates           = [PressurePlate_1]
       closedLocalPosition = (0, 0, 0)
       openLocalPosition   = (0, 2.8, 0)
       moveSpeed           = 3

SALIDA
──────────────────────────────────────────────────────────
LevelExit         | (0, 0.5, 25)          | (2, 2, 0.3)
  → Material: Amarillo emisivo
  → BoxCollider.isTrigger = true
  → LevelExit.loadNextBuildIndex = true

HUD
──────────────────────────────────────────────────────────
GameHUD (Empty)   | (0, 0, 0)             | (1, 1, 1)

LUZ
──────────────────────────────────────────────────────────
DirectionalLight  | (0, 10, 0)            | Rot: (50, -30, 0)
```

## 4. Mapa Visual (Vista Superior)

```
        Z →
    ┌──────────┐
    │          │   Floor_Start (6×6)
    │  PLAYER  │   Player spawn: (0, 1.5, 0)
    │          │
    └──────────┘
         │  (gap ~3u)  ← el jugador camina, no salta
    ┌────────┐
    │ BUTTON │     Floor_Button (4×4)
    │  ■ PP  │     PressurePlate en centro
    └────────┘
         │
     ═══════      DoorFrame + Door en Z=13.5
         │
    ┌────────┐
    │  GATE  │     Floor_Gate (4×3)
    └────────┘
         │
    ┌──────────┐
    │   EXIT   │   Floor_End (6×6) + LevelExit
    │   ★      │
    └──────────┘
```

> **Nota:** No hay gaps entre plataformas. Las plataformas están alineadas en Z para que el jugador solo camine en línea recta. Los bordes se conectan:
> - Floor_Start va de Z=-3 a Z=3
> - Floor_Button va de Z=7 a Z=11
> - Floor_Gate va de Z=14.5 a Z=17.5
> - Floor_End va de Z=20 a Z=26
>
> **Caminos de conexión** (opcional para mejor flujo): agregar cubo puente de Scale (2, 0.5, 4) entre cada plataforma.

### Puentes de conexión

```
Objeto            | Position              | Scale
──────────────────────────────────────────────────────────
Bridge_1          | (0, 0, 5)             | (2, 0.5, 4)
Bridge_2          | (0, 0, 12.5)          | (2, 0.5, 3)
Bridge_3          | (0, 0, 19)            | (2, 0.5, 3)
```

## 5. Jerarquía en Unity

```
Scene: Level_01
├── --- ENVIRONMENT ---
│   ├── Floor_Start          (Cube, static)
│   ├── Bridge_1             (Cube, static)
│   ├── Floor_Button         (Cube, static)
│   ├── Bridge_2             (Cube, static)
│   ├── Floor_Gate           (Cube, static)
│   ├── Bridge_3             (Cube, static)
│   └── Floor_End            (Cube, static)
│
├── --- MECHANICS ---
│   ├── PressurePlate_1      (Cube, PressurePlate.cs)
│   ├── DoorFrame            (Empty)
│   │   └── Door             (Cube, DoorController.cs)
│   └── LevelExit            (Cube, LevelExit.cs)
│
├── --- PLAYER ---
│   └── Player               (Prefab instance)
│
├── --- CAMERA ---
│   └── Main Camera          (ThirdPersonCamera.cs)
│
├── --- UI ---
│   └── GameHUD              (Empty, GameHUD.cs)
│
└── DirectionalLight
```

## 6. Paso a Paso de Construcción

1. **Crear escena** `Level_01` → `File > New Scene > Basic (Built-in)`
2. **Plataformas:**
   - Crear Cube → renombrar `Floor_Start` → Position (0,0,0), Scale (6, 0.5, 6)
   - Crear Cube → `Bridge_1` → Position (0, 0, 5), Scale (2, 0.5, 4)
   - Crear Cube → `Floor_Button` → Position (0, 0, 9), Scale (4, 0.5, 4)
   - Crear Cube → `Bridge_2` → Position (0, 0, 12.5), Scale (2, 0.5, 3)
   - Crear Cube → `Floor_Gate` → Position (0, 0, 16), Scale (4, 0.5, 3)
   - Crear Cube → `Bridge_3` → Position (0, 0, 19), Scale (2, 0.5, 3)
   - Crear Cube → `Floor_End` → Position (0, 0, 23), Scale (6, 0.5, 6)
   - Material: Crear material `Mat_Floor` color gris oscuro (#333333), aplicar a todos
   - Marcar todos como **Static** en Inspector
3. **PressurePlate:**
   - Crear Cube → renombrar `PressurePlate_1`
   - Position (0, 0.35, 9), Scale (1.5, 0.1, 1.5)
   - Material: Crear `Mat_Plate` color verde (#00CC66)
   - Agregar componente `PressurePlate`
   - El BoxCollider ya existe; el script lo marca como trigger en Awake
4. **Puerta:**
   - Crear Empty → renombrar `DoorFrame` → Position (0, 0.25, 13.5)
   - Como hijo de DoorFrame: Crear Cube → renombrar `Door`
   - Door localPosition (0, 0, 0), Scale (3, 2.8, 0.3)
   - Material: Crear `Mat_Door` color rojo oscuro (#882222)
   - Agregar componente `DoorController` al objeto `Door`
   - En Inspector de DoorController:
     - plates: Size=1, Element 0 = `PressurePlate_1`
     - closedLocalPosition = (0, 0, 0)
     - openLocalPosition = (0, 2.8, 0)
     - moveSpeed = 3
5. **LevelExit:**
   - Crear Cube → renombrar `LevelExit`
   - Position (0, 0.5, 25), Scale (2, 2, 0.3)
   - Material: `Mat_Exit` color amarillo emisivo (#FFDD00, Emission on)
   - Agregar componente `LevelExit`
   - BoxCollider → isTrigger = true (el script lo fuerza)
   - loadNextBuildIndex = true
6. **Player:**
   - Instanciar Prefab Player en Position (0, 1.5, 0)
   - Asignar echoPrefab en EchoRecorder si no está en prefab
7. **Cámara:**
   - Seleccionar Main Camera → agregar `ThirdPersonCamera`
   - target = Player.transform
   - offset = (0, 2.2, -4.5)
8. **HUD:**
   - Crear Empty → `GameHUD` → agregar componente `GameHUD`
9. **Luz:** Directional Light, Rotation (50, -30, 0)
10. **Tags:** Asegurar que Player tiene tag "Player" y EchoPrefab tiene tag "Echo"
11. **Build Settings:** Agregar `Level_01` al build settings (índice 0 o 1)

## 7. Solución del Puzzle

1. El jugador **camina** desde Floor_Start hasta Floor_Button (donde está el PressurePlate)
2. El jugador se **para sobre el PressurePlate** → la puerta se abre
3. El jugador **se aleja** del PressurePlate → la puerta se cierra
4. **El jugador entiende** que necesita algo que mantenga el botón presionado
5. El jugador vuelve al PressurePlate, se para encima
6. **Mantiene R** para grabar (≈3-4 segundos es suficiente, solo necesita estar parado)
7. Al soltar R, aparece un **eco** que repite el bucle: caminar al plate y quedarse
8. Como el eco se queda en bucle sobre el plate → la puerta **permanece abierta**
9. El jugador **cruza la puerta** y llega al LevelExit
10. **Nivel completado** → carga Level_02

> **Tiempo estimado de resolución:** 1-2 minutos
> **Margen de error:** El eco tiene ~10s de bucle, más que suficiente para cruzar

---

# NIVEL 2 — "El Puente Fantasma"

## 1. Objetivo del Nivel

El jugador aprende a usar el eco para activar una plataforma temporal que le permite cruzar un vacío.

## 2. Lista de Objetos

| #  | Nombre                   | Tipo              | Script(s)                      |
|----|--------------------------|-------------------|--------------------------------|
| 1  | Player                   | Prefab            | PlayerController, EchoRecorder |
| 2  | MainCamera               | Camera            | ThirdPersonCamera              |
| 3  | GameHUD                  | Empty             | GameHUD                        |
| 4  | Floor_Start              | Cube              | —                              |
| 5  | Floor_ButtonIsland       | Cube              | —                              |
| 6  | Floor_End                | Cube              | —                              |
| 7  | PressurePlate_Bridge     | Cube              | PressurePlate                  |
| 8  | BridgeAnchor             | Empty (padre)     | —                              |
| 9  | MovingBridge             | Cube              | TimedMovingPlatform            |
| 10 | LevelExit                | Cube (trigger)    | LevelExit                      |
| 11 | DirectionalLight         | Light             | —                              |

## 3. Posiciones, Rotaciones y Escalas

```
PLATAFORMAS
──────────────────────────────────────────────────────────
Objeto              | Position              | Scale
──────────────────────────────────────────────────────────
Floor_Start         | (0, 0, 0)             | (8, 0.5, 8)
Floor_ButtonIsland  | (-5, 0, 12)           | (4, 0.5, 4)
Floor_End           | (0, 0, 20)            | (8, 0.5, 8)

PUENTES DE CONEXIÓN
──────────────────────────────────────────────────────────
Bridge_ToIsland     | (-2.5, 0, 6)          | (2, 0.5, 4)
  → Conecta Floor_Start con Floor_ButtonIsland

JUGADOR
──────────────────────────────────────────────────────────
Player              | (0, 1.5, 0)           | (1, 1, 1)

CÁMARA
──────────────────────────────────────────────────────────
MainCamera          | (0, 4, -6)            | (1, 1, 1)

BOTÓN EN ISLA LATERAL
──────────────────────────────────────────────────────────
PressurePlate_Bridge | (-5, 0.35, 12)       | (1.5, 0.1, 1.5)
  → Material: Verde

PLATAFORMA MÓVIL (PUENTE)
──────────────────────────────────────────────────────────
BridgeAnchor (Empty)| (0, 0, 8)            | (1, 1, 1)
  └─ MovingBridge   | localPos (0, 0, 0)    | (3, 0.5, 8)
     → TimedMovingPlatform:
       plate           = PressurePlate_Bridge
       inactiveLocal   = (0, -5, 0)      ← hundida (invisible)
       activeLocal     = (0, 0, 0)        ← sube a nivel del suelo
       travelSpeed     = 4

SALIDA
──────────────────────────────────────────────────────────
LevelExit           | (0, 0.5, 23)          | (2, 2, 0.3)
  → Material: Amarillo emisivo

LUZ
──────────────────────────────────────────────────────────
DirectionalLight    | (0, 10, 0)            | Rot: (50, -30, 0)
```

## 4. Mapa Visual (Vista Superior)

```
       X ←──→
       Z ↑

    ┌──────────────┐
    │              │  Floor_Start (8×8)
    │   PLAYER     │  Player en (0, 1.5, 0)
    │              │
    └──┬───────────┘
       │ Bridge_ToIsland
    ┌──┴──┐
    │ISLA │  Floor_ButtonIsland (4×4) en X=-5
    │ ■PP │  PressurePlate en centro
    └─────┘
                ·····
              · VACÍO ·     ← entre Floor_Start y Floor_End
                ·····       ← MovingBridge aparece aquí cuando PP activo
              ┌───────┐
              │BRIDGE │     ← MovingBridge (3×8) sube desde abajo
              └───────┘
    ┌──────────────┐
    │              │  Floor_End (8×8)
    │     EXIT ★   │
    │              │
    └──────────────┘
```

**Flujo espacial:**
- Floor_Start: Z = -4 a 4
- Vacío: Z = 4 a 16 (sin suelo directo)
- MovingBridge cubre: Z = 4 a 12 (centrado en Z=8) cuando activo
- Floor_End: Z = 16 a 24
- Isla del botón: desplazada a la izquierda (X=-5), Z = 10 a 14

## 5. Jerarquía en Unity

```
Scene: Level_02
├── --- ENVIRONMENT ---
│   ├── Floor_Start             (Cube, static)
│   ├── Bridge_ToIsland         (Cube, static)
│   ├── Floor_ButtonIsland      (Cube, static)
│   └── Floor_End               (Cube, static)
│
├── --- MECHANICS ---
│   ├── PressurePlate_Bridge    (Cube, PressurePlate.cs)
│   ├── BridgeAnchor            (Empty)
│   │   └── MovingBridge        (Cube, TimedMovingPlatform.cs)
│   └── LevelExit               (Cube, LevelExit.cs)
│
├── --- PLAYER ---
│   └── Player                  (Prefab instance)
│
├── --- CAMERA ---
│   └── Main Camera             (ThirdPersonCamera.cs)
│
├── --- UI ---
│   └── GameHUD                 (Empty, GameHUD.cs)
│
└── DirectionalLight
```

## 6. Paso a Paso de Construcción

1. **Crear escena** `Level_02`
2. **Plataformas:**
   - Cube → `Floor_Start` → Position (0, 0, 0), Scale (8, 0.5, 8). Material `Mat_Floor`
   - Cube → `Bridge_ToIsland` → Position (-2.5, 0, 6), Scale (2, 0.5, 4). Material `Mat_Floor`
   - Cube → `Floor_ButtonIsland` → Position (-5, 0, 12), Scale (4, 0.5, 4). Material `Mat_Floor`
   - Cube → `Floor_End` → Position (0, 0, 20), Scale (8, 0.5, 8). Material `Mat_Floor`
   - Marcar todos como **Static**
3. **PressurePlate:**
   - Cube → `PressurePlate_Bridge` → Position (-5, 0.35, 12), Scale (1.5, 0.1, 1.5)
   - Material `Mat_Plate` (verde)
   - Agregar `PressurePlate`
4. **Plataforma Móvil:**
   - Crear Empty → `BridgeAnchor` → Position (0, 0, 8)
   - Como hijo: Cube → `MovingBridge` → localPosition (0, -5, 0), Scale (3, 0.5, 8)
   - Material: Crear `Mat_Bridge` color azul (#3366CC)
   - **NO** marcar como Static
   - Agregar `TimedMovingPlatform` al `MovingBridge`:
     - plate = `PressurePlate_Bridge`
     - inactiveLocal = (0, -5, 0)
     - activeLocal = (0, 0, 0)
     - travelSpeed = 4
5. **LevelExit:**
   - Cube → `LevelExit` → Position (0, 0.5, 23), Scale (2, 2, 0.3)
   - Material `Mat_Exit`, `LevelExit` script, isTrigger
6. **Player:** Instanciar en (0, 1.5, 0)
7. **Cámara:** Main Camera con `ThirdPersonCamera`, target = Player
8. **HUD:** Empty con `GameHUD`
9. **Verificar tags:** Player="Player", EchoPrefab="Echo"
10. **Build Settings:** Agregar `Level_02`

## 7. Solución del Puzzle

1. El jugador **explora** Floor_Start y ve el vacío hacia Floor_End
2. Ve la isla lateral (Floor_ButtonIsland) con el PressurePlate
3. **Camina** por Bridge_ToIsland hasta la isla
4. Se **para sobre el PressurePlate** → el MovingBridge sube desde abajo
5. Se **aleja** del plate → el puente se hunde de nuevo
6. **Entiende:** necesita un eco que mantenga el plate presionado
7. Se para sobre el plate, **mantiene R** para grabar ~5 segundos (parado sobre el plate)
8. Suelta R → aparece el eco que repite el bucle sobre el plate
9. El eco mantiene el plate presionado → **el puente permanece arriba**
10. El jugador **regresa** a Floor_Start y cruza el MovingBridge
11. Llega a Floor_End → toca el LevelExit
12. **Nivel completado** → carga Level_03

> **Tiempo estimado de resolución:** 2-3 minutos
> **Margen de error:** El puente tarda ~1.25s en subir (5u / 4 speed). El jugador tiene ~10s de bucle del eco para cruzar, y solo necesita ~4s caminando (8u ÷ 5u/s ≈ 1.6s sobre el puente)

---

# NIVEL 3 — "Sincronía"

## 1. Objetivo del Nivel

El jugador debe activar DOS botones simultáneamente, lo que requiere grabar 2 ecos (uno para cada botón) y después llegar a la puerta.

## 2. Lista de Objetos

| #  | Nombre                | Tipo               | Script(s)                      |
|----|-----------------------|--------------------|--------------------------------|
| 1  | Player                | Prefab             | PlayerController, EchoRecorder |
| 2  | MainCamera            | Camera             | ThirdPersonCamera              |
| 3  | GameHUD               | Empty              | GameHUD                        |
| 4  | Floor_Central         | Cube               | —                              |
| 5  | Floor_Left            | Cube               | —                              |
| 6  | Floor_Right           | Cube               | —                              |
| 7  | Floor_Gate            | Cube               | —                              |
| 8  | Floor_End             | Cube               | —                              |
| 9  | Bridge_Left           | Cube               | —                              |
| 10 | Bridge_Right          | Cube               | —                              |
| 11 | Bridge_Gate           | Cube               | —                              |
| 12 | PressurePlate_Left    | Cube               | PressurePlate                  |
| 13 | PressurePlate_Right   | Cube               | PressurePlate                  |
| 14 | DoorFrame             | Empty (padre)      | —                              |
| 15 | Door                  | Cube               | DoorController                 |
| 16 | LevelExit             | Cube (trigger)     | LevelExit                      |
| 17 | DirectionalLight      | Light              | —                              |

## 3. Posiciones, Rotaciones y Escalas

```
PLATAFORMAS
──────────────────────────────────────────────────────────
Objeto              | Position              | Scale
──────────────────────────────────────────────────────────
Floor_Central       | (0, 0, 0)             | (6, 0.5, 6)
Floor_Left          | (-8, 0, 6)            | (4, 0.5, 4)
Floor_Right         | (8, 0, 6)             | (4, 0.5, 4)
Floor_Gate          | (0, 0, 14)            | (4, 0.5, 4)
Floor_End           | (0, 0, 22)            | (6, 0.5, 6)

PUENTES
──────────────────────────────────────────────────────────
Bridge_Left         | (-4, 0, 4)            | (2, 0.5, 5)
Bridge_Right        | (4, 0, 4)             | (2, 0.5, 5)
Bridge_Gate         | (0, 0, 10)            | (2, 0.5, 4)
Bridge_End          | (0, 0, 18)            | (2, 0.5, 4)

JUGADOR
──────────────────────────────────────────────────────────
Player              | (0, 1.5, 0)           | (1, 1, 1)

BOTONES
──────────────────────────────────────────────────────────
PressurePlate_Left  | (-8, 0.35, 6)         | (1.5, 0.1, 1.5)
  → Material: Verde

PressurePlate_Right | (8, 0.35, 6)          | (1.5, 0.1, 1.5)
  → Material: Verde

PUERTA (requiere AMBOS botones = AND)
──────────────────────────────────────────────────────────
DoorFrame (Empty)   | (0, 0.25, 12)         | (1, 1, 1)
  └─ Door (Cube)    | localPos (0, 0, 0)    | (3, 2.8, 0.3)
     → DoorController:
       plates               = [PressurePlate_Left, PressurePlate_Right]
       closedLocalPosition  = (0, 0, 0)
       openLocalPosition    = (0, 2.8, 0)
       moveSpeed            = 3

SALIDA
──────────────────────────────────────────────────────────
LevelExit           | (0, 0.5, 24)          | (2, 2, 0.3)
  → Material: Amarillo emisivo

LUZ
──────────────────────────────────────────────────────────
DirectionalLight    | (0, 10, 0)            | Rot: (50, -30, 0)
```

## 4. Mapa Visual (Vista Superior)

```
         X ←──────→
         Z ↑

                    ┌──────────┐
                    │   EXIT   │  Floor_End (6×6)
                    │    ★     │
                    └──────────┘
                         │  Bridge_End
                    ┌────────┐
                    │  GATE  │  Floor_Gate (4×4)
                    └────────┘
                         │  Bridge_Gate
                     ═══════     DoorFrame + Door en Z=12
                         │
     ┌─────┐        ┌────────┐        ┌─────┐
     │ ■PP │        │        │        │ ■PP │
     │LEFT │        │        │        │RIGHT│
     └──┬──┘        │CENTRAL │        └──┬──┘
        │ Bridge_L  │ PLAYER │  Bridge_R │
        └───────────┤        ├───────────┘
                    │        │
                    └────────┘

PP LEFT  = PressurePlate_Left  en (-8, 0.35, 6)
PP RIGHT = PressurePlate_Right en (8, 0.35, 6)
```

**Flujo espacial:**
- Floor_Central: centro del nivel, punto de inicio
- Floor_Left y Floor_Right: islas laterales con un PressurePlate cada una
- Bridge_Left/Right: pasarelas de 2u de ancho que conectan con las islas
- La puerta (Door) en Z=12 bloquea el paso a Floor_Gate
- DoorController tiene `plates = [Left, Right]` → **ambos deben estar presionados (AND)**

## 5. Jerarquía en Unity

```
Scene: Level_03
├── --- ENVIRONMENT ---
│   ├── Floor_Central           (Cube, static)
│   ├── Bridge_Left             (Cube, static)
│   ├── Bridge_Right            (Cube, static)
│   ├── Floor_Left              (Cube, static)
│   ├── Floor_Right             (Cube, static)
│   ├── Bridge_Gate             (Cube, static)
│   ├── Floor_Gate              (Cube, static)
│   ├── Bridge_End              (Cube, static)
│   └── Floor_End               (Cube, static)
│
├── --- MECHANICS ---
│   ├── PressurePlate_Left      (Cube, PressurePlate.cs)
│   ├── PressurePlate_Right     (Cube, PressurePlate.cs)
│   ├── DoorFrame               (Empty)
│   │   └── Door                (Cube, DoorController.cs)
│   └── LevelExit               (Cube, LevelExit.cs)
│
├── --- PLAYER ---
│   └── Player                  (Prefab instance)
│
├── --- CAMERA ---
│   └── Main Camera             (ThirdPersonCamera.cs)
│
├── --- UI ---
│   └── GameHUD                 (Empty, GameHUD.cs)
│
└── DirectionalLight
```

## 6. Paso a Paso de Construcción

1. **Crear escena** `Level_03`
2. **Plataformas:**
   - Cube → `Floor_Central` → Position (0, 0, 0), Scale (6, 0.5, 6)
   - Cube → `Bridge_Left` → Position (-4, 0, 4), Scale (2, 0.5, 5)
   - Cube → `Bridge_Right` → Position (4, 0, 4), Scale (2, 0.5, 5)
   - Cube → `Floor_Left` → Position (-8, 0, 6), Scale (4, 0.5, 4)
   - Cube → `Floor_Right` → Position (8, 0, 6), Scale (4, 0.5, 4)
   - Cube → `Bridge_Gate` → Position (0, 0, 10), Scale (2, 0.5, 4)
   - Cube → `Floor_Gate` → Position (0, 0, 14), Scale (4, 0.5, 4)
   - Cube → `Bridge_End` → Position (0, 0, 18), Scale (2, 0.5, 4)
   - Cube → `Floor_End` → Position (0, 0, 22), Scale (6, 0.5, 6)
   - Material `Mat_Floor`, marcar todos como **Static**
3. **PressurePlates:**
   - Cube → `PressurePlate_Left` → Position (-8, 0.35, 6), Scale (1.5, 0.1, 1.5)
   - Cube → `PressurePlate_Right` → Position (8, 0.35, 6), Scale (1.5, 0.1, 1.5)
   - Material `Mat_Plate` a ambos
   - Agregar `PressurePlate` a ambos
4. **Puerta:**
   - Empty → `DoorFrame` → Position (0, 0.25, 12)
   - Hijo: Cube → `Door` → localPosition (0, 0, 0), Scale (3, 2.8, 0.3)
   - Material `Mat_Door`
   - Agregar `DoorController`:
     - plates: Size=2
       - Element 0 = `PressurePlate_Left`
       - Element 1 = `PressurePlate_Right`
     - closedLocalPosition = (0, 0, 0)
     - openLocalPosition = (0, 2.8, 0)
     - moveSpeed = 3
5. **LevelExit:** Cube → Position (0, 0.5, 24), Scale (2, 2, 0.3), `LevelExit` script
6. **Player:** Instanciar en (0, 1.5, 0)
7. **Cámara:** Main Camera + `ThirdPersonCamera`, target = Player
8. **HUD:** Empty + `GameHUD`
9. **Verificar:**
   - EchoRecorder.maxEchoes = 3 (default, suficiente para 2 ecos)
   - Tags correctos
10. **Build Settings:** Agregar `Level_03`

## 7. Solución del Puzzle

1. El jugador empieza en Floor_Central, ve **dos caminos** (izquierda y derecha) con botones
2. Ve la puerta bloqueando el paso hacia adelante
3. **Primer eco — Botón izquierdo:**
   - Camina por Bridge_Left hasta Floor_Left
   - Se para sobre PressurePlate_Left
   - **Mantiene R** para grabar ~4 segundos (parado sobre el plate)
   - Suelta R → eco #1 aparece repitiendo el bucle sobre PressurePlate_Left
4. **Segundo eco — Botón derecho:**
   - Regresa a Floor_Central
   - Camina por Bridge_Right hasta Floor_Right
   - Se para sobre PressurePlate_Right
   - **Mantiene R** para grabar ~4 segundos
   - Suelta R → eco #2 aparece repitiendo el bucle sobre PressurePlate_Right
5. Ahora **ambos ecos mantienen presionados los dos plates** simultáneamente
6. DoorController detecta `AllPressed() == true` → **la puerta se abre**
7. El jugador regresa a Floor_Central y **cruza la puerta** hacia Floor_Gate → Floor_End
8. Toca LevelExit → **juego completado**

> **Tiempo estimado de resolución:** 3-5 minutos
> **Cálculo de tiempos críticos:**
> - Grabar eco 1: ~4s
> - Caminar de izquierda a derecha: ~6s (16u ÷ 5u/s ≈ 3.2s, con desvío ~5-6s)
> - Grabar eco 2: ~4s
> - Caminar de derecho a puerta: ~5s
> - Total desde eco 1: ~15s. Como los ecos hacen loop, NO hay tiempo límite. ✅

---

## Checklist de Verificación Final

### Antes de probar cada nivel, verificar:

- [ ] Tags configurados: `Player` y `Echo` en Project Settings → Tags & Layers
- [ ] Layer `Ground` creado y asignado a TODAS las plataformas/puentes
- [ ] PlayerController.groundMask incluye `Ground`
- [ ] Cada PressurePlate tiene BoxCollider (el script lo pone trigger)
- [ ] DoorController.plates correctamente asignados
- [ ] TimedMovingPlatform.plate correctamente asignado (Level 02)
- [ ] EchoRecorder.echoPrefab asignado
- [ ] ThirdPersonCamera.target asignado
- [ ] LevelExit tiene BoxCollider trigger
- [ ] Escenas agregadas a Build Settings en orden: Level_01, Level_02, Level_03
- [ ] EchoPrefab tiene tag "Echo" y CharacterController

### Errores comunes y prevención:

| Problema | Causa | Solución |
|----------|-------|----------|
| Eco no activa PressurePlate | Tag "Echo" no asignado al prefab | Verificar tag en EchoPrefab |
| Puerta no se abre | plates[] no asignado en Inspector | Drag & drop los PressurePlate |
| Jugador cae al vacío | groundCheck no detecta suelo | Verificar Layer Ground y groundMask |
| Eco atraviesa el suelo | Falta CharacterController en eco | Verificar que EchoPrefab tiene CC |
| Puente no se mueve | plate no asignado en TimedMovingPlatform | Verificar referencia en Inspector |

---

## Materiales Recomendados

| Material     | Color Hex  | Uso                        |
|-------------|------------|----------------------------|
| Mat_Floor   | #333333    | Todas las plataformas      |
| Mat_Plate   | #00CC66    | PressurePlates             |
| Mat_Door    | #882222    | Puertas                    |
| Mat_Exit    | #FFDD00    | LevelExit (con Emission)   |
| Mat_Bridge  | #3366CC    | MovingBridge (Level 02)    |
| Mat_Player  | #FFFFFF    | Cubo del jugador           |
| Mat_Echo    | #4488FF    | Cubo del eco (Alpha ~0.5)  |

> Para Mat_Echo usar Rendering Mode = **Transparent** y Alpha = 128.
