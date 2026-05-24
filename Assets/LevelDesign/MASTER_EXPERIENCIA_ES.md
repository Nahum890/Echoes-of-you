# Echoes of You — Guía maestra de experiencia

Documento de referencia para diseño, código y nivel. Objetivo: **memoria jugable que colapsa en tiempo real**.

---

## Pilares de sensación

| Debe sentirse | No debe sentirse |
|---------------|------------------|
| Ágil, inteligente, presionado, sincronizado, cinematográfico | Estático, lento, por salas, solo “parar y pensar”, simulador de botones |

**El movimiento es el puzzle.** Cada mecánica se enseña con luz, geometría y cámara — sin cajas de texto.

---

## Movimiento del jugador

Implementado en `PlayerController` + `PlayerAdvancedLocomotion` + `EchoesLocomotionSettings`.

| Mecánica | Control por defecto | Notas |
|----------|---------------------|-------|
| Momentum de sprint | Mantener Shift al correr | Bonus de velocidad acumulativo |
| Slide | **C** (sprint + suelo) | Conserva velocidad |
| Agarre de borde | Automático al caer cerca de un borde | Espacio para subir |
| Wall jump | Espacio junto a pared en el aire | Impulso lateral + vertical |
| Wall run corto | Automático en pared con velocidad | Gravedad reducida |
| Air dash | **Alt izquierdo** en el aire | Desbloqueado desde nivel 7 (`Level_07`) |
| Momentum de plataforma | Automático en objetos con `MovingPlatformMomentum` | Sin parada al aterrizar |

**Aterrizaje:** `landingVelocityRetention = 1` — no frena al tocar suelo.

---

## Ecos (más allá de placas)

Roles en `EchoKineticRole` / zonas `EchoKineticZone`:

- Traversal sincronizado
- Activador de timing
- Escudo temporal (`EchoShieldField`)
- Activador de plataformas
- Relay de momentum
- Cebo de peligro
- Apertura de ruta
- Modificador de geometría (`EchoKineticBody`, `DynamicTransformMotor`)

Grabación: **E / R** (cuerpo) · **F** (proyección) · **Q** reinicia nivel / limpia ecos.

---

## Cámara — identidades A–E

Configuradas por nivel en `LevelCameraProfiles` y aplicadas por `CinematicCameraDynamics`.

| Tipo | Uso | Niveles ejemplo |
|------|-----|-----------------|
| **A** Wide Liminal | Exploración, soledad | 01, 09 |
| **B** Dynamic Follow | Parkour, intensidad | 02, 04, 08 |
| **C** Side Cinematic | Precisión, siluetas | 03, 07 |
| **D** Top Descent | Torre / caída | 06 |
| **E** Memory | Historia, drift, inestabilidad | 05, 10 |

Marcadores opcionales: `LevelPacingMarker` cambia el tono de cámara al entrar en una sección.

---

## Estructura obligatoria de cada nivel

1. Sección de **movimiento**
2. Sección de **sincronización** (eco + jugador)
3. **Escalada** de dificultad
4. Momento **aha**
5. **Clímax** de traversal

Tras resolver el puzzle: **secuencia de escape** (`LevelEscapeSequence`) — la salida no es inmediata; hay que huir del colapso (20–40 s de movimiento continuo recomendado).

---

## Arquetipos de nivel

Definir en `LevelExperienceBlueprint`:

| Arquetipo | Inspiración | Componentes clave |
|-----------|-------------|-------------------|
| **Chase** | Ghostrunner | `ChaseHazardMotor` + eco abre ruta |
| **Mirror Path** | Portal 2 | Dos rutas espejo, `EchoKineticZone` espejo |
| **Moving City** | Titanfall 2 | `DynamicTransformMotor` + timing de eco |
| **Vertical Fall** | Mirror's Edge | Cámara D + plataformas en caída |
| **Multi-Layer Timeline** | — | Varios ecos simultáneos (`maxEchoes` ≥ 3) |

---

## Lenguaje espacial

**Evitar:** salas cuadradas, cajas cerradas, pisos planos solo-puzzle.

**Preferir:** puentes gigantes, estructuras colgantes, escaleras rotas, rutas en capas, hitos lejanos visibles, loops de traversal, rutas alternativas.

El jugador debe **ver siempre** hacia dónde irá.

---

## Menú principal

`MainMenuCinematicWorld` (auto en escena `MainMenu`):

- Arquitectura flotante procedural
- Ecos distantes caminando
- Niebla pulsante
- Cámara orbital lenta
- UI Toolkit encima (no fondo negro plano)

---

## Ritmo por campaña

| Fase | Niveles | Enfoque |
|------|---------|---------|
| Early | 01–03 | Enseñanza por geometría |
| Mid | 04–06 | Combinación movimiento + eco |
| Late | 07–09 | Dominio y presión |
| Endgame | 10 | Improvisación, timeline múltiple |

---

## Generación con script (Unity Editor)

Menú: **Echoes of You → Production → Rebuild Menu Hub and Levels**

| Script | Niveles | Notas |
|--------|---------|--------|
| `EchoesProductionBuilder` | MainMenu + **Level_01 … Level_10** | Script principal actualizado |
| `EchoesLevelBuilder` | Solo Level_01–07 | Legacy — no usar para 08–10 |
| `LevelValidator` | Valida los **10** niveles tras rebuild |

Cada rebuild incluye: jugador con parkour, cámara Cinemachine, `LevelExperienceBlueprint`, `LevelEscapeSequence`, 5 `LevelPacingMarker`, chase en 05/06/08, build settings.

Arquetipo por nivel al generar:

| Nivel | Arquetipo |
|-------|-----------|
| 01, 04, 07, 09 | Standard |
| 02 | Moving City |
| 03 | Mirror Path |
| 05, 10 | Multi-Layer Timeline |
| 06 | Vertical Fall (+ chase) |
| 08 | Chase (+ hazard) |

---

## Checklist al diseñar un nivel en Unity

1. Añadir `LevelExperienceBlueprint` (arquetipo + referencias de secciones).
2. Colocar `LevelPacingMarker` en los 5 actos (triggers sin texto).
3. Asignar zonas `EchoKineticZone` con roles variados (no solo placas).
4. Plataformas móviles: `TimedMovingPlatform` + `MovingPlatformMomentum`.
5. Punto final de escape: Transform en `LevelEscapeSequence.escapeRouteEnd`.
6. Para CHASE: asignar `ChaseHazardMotor` en el blueprint.
7. Validar 20–40 s de movimiento continuo entre checkpoints.

---

## Archivos clave del código

```
Assets/Scripts/
  PlayerController.cs
  PlayerAdvancedLocomotion.cs
  EchoesLocomotionSettings.cs
  MovingPlatformMomentum.cs
  LevelCameraProfiles.cs
  EchoesCameraIdentity.cs
  CinematicCameraDynamics.cs
  EchoKineticZone.cs / EchoKineticRole.cs
  LevelExperienceBlueprint.cs
  LevelEscapeSequence.cs
  LevelPacingMarker.cs
  ChaseHazardMotor.cs
  MainMenuCinematicWorld.cs
```

---

*Última actualización: implementación maestra de gameplay / nivel / cámara.*
