# Guía Unity — Iluminación por nivel y animaciones del jugador

Esta guía es para hacer los cambios **directamente en Unity**, sin depender del agente de IA.

---

## Parte A — Regenerar niveles (obligatorio tras cambios del builder)

1. Abre el proyecto en **Unity**.
2. Menú superior: **Echoes of You → Production → Rebuild Menu Hub and Levels**.
3. Espera el mensaje en la consola (sin errores rojos).
4. Guarda escenas si Unity lo pide (`Ctrl+S`).

> Si no ejecutas este paso, seguirás jugando escenas viejas (nivel 4 con trampa rota, etc.).

### Solo un nivel

Si el menú no tiene opción por nivel, usa **Rebuild** completo. Tarda un poco pero es lo fiable.

---

## Parte B — Editar iluminación de un nivel

### Método 1 (recomendado): componente `LevelLightingSettings`

Tras regenerar niveles, en cada escena `Level_XX` verás bajo **--- ENVIRONMENT ---** un objeto **LevelLighting**.

1. Abre la escena, por ejemplo `Assets/Scenes/Level_04.unity`.
2. En **Hierarchy**, selecciona **LevelLighting** (hijo de --- ENVIRONMENT ---).
3. En **Inspector**, ajusta:
   - **Directional Intensity / Color / Euler** — sol principal.
   - **Ambient Sky / Equator / Ground** — luz ambiental global.
   - **Fog Color / Fog Density** — niebla.
   - **Point Light Intensity/Range Multiplier** — escala luces puntuales hijas.
   - **Disable Runtime Fill Lights** — marca esto si no quieres que el juego añada luces extra al iniciar.
4. Con la escena abierta (sin Play), los cambios de niebla/ambiente se ven al mover valores (OnValidate).
5. Pulsa **Play** para comprobar en juego.
6. **Guarda la escena** (`Ctrl+S`).

### Método 2: luces individuales

1. En **Hierarchy** → **--- ENVIRONMENT ---** busca objetos como:
   - `Directional Light`
   - `Light_EcoLane`, `Light_Bridge`, `Light_Exit`, `Ambient_*`, etc.
2. Selecciona cada **Light** y en Inspector cambia:
   - **Intensity**
   - **Range** (solo Point)
   - **Color**
3. Mueve la luz con la herramienta **Move** (W) si hace falta.
4. Guarda escena.

### Método 3: atmósfera global de la escena

1. **Window → Rendering → Lighting** (o **Environment** en versiones nuevas).
2. Pestaña **Environment**:
   - **Skybox / Environment Lighting**
   - **Fog** (activar y densidad)
3. O edita **LevelLighting → Fog** (más simple y por nivel).

### Importante

- Si vuelves a ejecutar **Rebuild Menu Hub and Levels**, se **sobrescriben** las escenas generadas. Anota tus valores o copia el objeto LevelLighting antes de regenerar.
- El script de runtime `LevelEnvironmentBootstrap` puede intensificar luces al iniciar; usa **Disable Runtime Fill Lights** en LevelLighting para control total.

---

## Parte C — Arreglar animaciones del jugador (paso a paso)

**Jerarquía correcta tras Rebuild:**

`Player → PlayerVisual → PlayerScaler → Model` (con **Animator** en Model)

Si solo ves una cápsula, ejecuta los pasos 1–2 abajo; el script `PlayerCharacterVisualSetup` también intenta crear el modelo al iniciar partida desde `Resources/EchoesCharacterVisual.prefab`.

Las animaciones fallan casi siempre por: rig no humanoide, controller sin asignar, o asset de configuración vacío.

### Paso 1 — Reparación automática del proyecto

1. En Unity: **Echoes → Repair Player Animation Setup**
2. Espera el log: `[Echoes] Player animation setup repaired...`
3. Si hay errores, continúa con el paso 2.

### Paso 2 — Crear configuración de locomoción

1. **Echoes of You → Production → Ensure Locomotion Settings (Animations)**
2. Comprueba que existe el archivo:
   - `Assets/Resources/EchoesLocomotionSettings.asset`
3. Selecciónalo en el Project; en Inspector debe tener:
   - **Animator Controller** → `PlayerAnimController`
   - **Humanoid Avatar** → avatar del personaje (no None)

### Paso 3 — Verificar el prefab / jugador en escena

1. Abre `Assets/Scenes/Level_01.unity` (o cualquier nivel).
2. En Hierarchy: **--- PLAYER --- → Player → PlayerVisual → Model** (o similar).
3. El objeto **Model** debe tener componente **Animator** con:
   - **Controller** = `PlayerAnimController`
   - **Avatar** = humanoide válido (icono verde, no "None")
   - **Apply Root Motion** = desmarcado

### Paso 4 — Reimportar animaciones FBX (si sigue en T-Pose o sin moverse)

Para cada archivo en `Assets/3D Models/Animaciones/Locomotion/` (`idle`, `walking`, `running`, `jump`, etc.):

1. Clic en el FBX → **Inspector**.
2. Pestaña **Rig**:
   - **Animation Type** = **Humanoid**
   - **Avatar Definition** = **Create From This Model**
3. Pestaña **Animation** → **Import Animation** activado.
4. Clic **Apply**.
5. Repite para el modelo del personaje:
   - `Assets/3D Models/lowpoly-character-freerigged-/.../LowPolyCharacter.fbx`

### Paso 5 — Reconstruir el Animator Controller

1. **Echoes → Setup Player Animator** (si aparece en menú).
2. O abre `Assets/Prefabs/PlayerAnimController.controller` y comprueba estados: Idle, Walk/Run, Jump, etc.
3. Parámetro **Speed** (float) debe existir — el código lo usa.

### Paso 6 — Probar en Play Mode

1. Play en Level_01.
2. Mueve con WASD: el personaje debe alternar idle / caminar.
3. En **Animator** ventana (**Window → Animation → Animator**), con el Model seleccionado, mira si el parámetro **Speed** cambia al moverte.
   - Si Speed cambia pero no anima → problema de clips o estados del controller.
   - Si Speed no cambia → el Animator no está en el modelo correcto o no hay Controller.

### Paso 7 — Consola

Si ves `Animator is not playing an AnimatorController` o `Avatar is invalid`:

- Repite pasos 1–2.
- Asegúrate de que `EchoesLocomotionSettings.asset` está en **Resources** (nombre exacto de carpeta).

---

## Parte D — Nivel 4 nuevo (Cruce doble)

El nivel 4 antiguo (trampa + puente superior) se sustituyó por:

- Pasillo izquierdo: **proyecta con F** hasta la **placa del eco**.
- Puente que sube cuando el eco activa la zona cinética.
- **Placa del jugador** en la isla central.
- Puerta que abre con **ambas placas** activas.

Tras **Rebuild Menu Hub and Levels**, prueba:

1. Mantén **F**, lleva la proyección al pasillo izquierdo sobre la placa.
2. Suelta **F**, cruza el puente.
3. Pisa la placa central con tu cuerpo.
4. Cruza la puerta hacia la salida.

---

## Resumen rápido

| Qué quieres | Qué hacer en Unity |
|-------------|-------------------|
| Iluminación nivel | Objeto **LevelLighting** en la escena o luces bajo ENVIRONMENT |
| Regenerar nivel 4 | **Echoes of You → Production → Rebuild Menu Hub and Levels** |
| Animaciones | **Echoes → Repair…** luego **Ensure Locomotion Settings** |
| Ver si anima | Play + ventana Animator + parámetro Speed |

Si algo sigue fallando, anota: escena, mensaje exacto de la consola (copiar texto), y si el jugador se mueve pero no anima o no se mueve.
