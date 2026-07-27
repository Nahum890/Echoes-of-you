# BLUEPRINT_SPEC.md — Especificación Ejecutable de LevelBlueprint
## Echoes of You v3.0

---

## 1. ESTRUCTURA Y CAMPOS DE `LevelBlueprint.cs` (`BLU-SPEC-*`)

Cada nivel en el proyecto está definido declarativamente por un ScriptableObject `LevelBlueprint` ubicado en `Assets/Data/Levels/Level_XX_Blueprint.asset`.

```text
LevelBlueprint Asset
├── Basic Information (Name, NextLevel, Act, Archetype)
├── Echo Limits & Modes (EchoEnabled, MaxEchoes, MaxRecordSeconds, EchoMode, RecordFuture, Profiles)
├── Paradox Systems (ActiveParadoxes Array)
├── Atmosphere & Lighting (FogColor, FogDensity, SkyColor, AmbientColor, DirLight)
├── Narrative Texts (IntroTitle, IntroDesc, Objectives)
├── Visual Guides & Pacing (PathHints Vector3 Array)
└── Placed Modules (List<ModulePlacement>)
```

---

## 2. SCHEMA COMPLETO EN FORMATO YAML DE UN BLUEPRINT DE REFERENCIA

```yaml
# Level_02_Blueprint.yaml (Schema de Referencia)
levelName: "Level_02"
nextLevel: "Level_03"
next_scene_build_index: 3
actNumber: 1
archetype: "Standard"

# Configuración Echo
echoEnabled: true
maxEchoes: 1
maxRecordSeconds: 12.0
echoMode: "Standard"
recordFuture: false
degradationPerReplay: 0.0
lockEchoSlots: false
lockedSlotIndices: []

# Perfiles de Cámara e Iluminación
cameraProfile: "Assets/Data/Camera/CamProfile_Learning.asset"
lightingProfile: "Assets/Data/Lighting/LightProfile_Act1.asset"

# Atmósfera URP
fogColor: {r: 0.12, g: 0.14, b: 0.20, a: 1.0}
fogDensity: 0.008
skyColor: {r: 0.20, g: 0.22, b: 0.28, a: 1.0}
ambientColor: {r: 0.06, g: 0.08, b: 0.12, a: 1.0}
directionalLightRotation: {x: 50.0, y: -30.0, z: 0.0}
directionalLightColor: {r: 0.95, g: 0.95, b: 1.0, a: 1.0}
directionalLightIntensity: 0.8

# Textos Narrativos
narrativeIntroTitle: "Nivel 02 — Repetición"
narrativeIntroDesc: "Tres aulas idénticas. La misma decisión se repite hasta que aprendes a coordinarla."
puzzleObjectiveText: "Sincroniza los accesos usando la huella del Eco."

# Lista de Módulos (ModulePlacement)
modules:
  - name: "PlayerStart"
    type: "PlayerStart"
    position: {x: 0.0, y: 0.0, z: 0.0}
    rotation: {x: 0.0, y: 0.0, z: 0.0}
    scale: {x: 1.0, y: 1.0, z: 1.0}
    customData: ""
    targetSignals: []

  - name: "Corridor_Main"
    type: "SchoolCorridor"
    position: {x: 0.0, y: 0.0, z: 12.0}
    rotation: {x: 0.0, y: 0.0, z: 0.0}
    scale: {x: 1.0, y: 1.0, z: 1.0}
    customData: "length=24;lockers=left"
    targetSignals: []

  - name: "Classroom_A"
    type: "SchoolClassroom"
    position: {x: -8.0, y: 0.0, z: 12.0}
    rotation: {x: 0.0, y: 90.0, z: 0.0}
    scale: {x: 1.0, y: 1.0, z: 1.0}
    customData: "desks=4x4;chaotic=false"
    targetSignals: ["Signal_DoorA"]

  - name: "Plate_ClassroomA"
    type: "PressurePlate"
    position: {x: -8.0, y: 0.05, z: 15.0}
    rotation: {x: 0.0, y: 0.0, z: 0.0}
    scale: {x: 1.5, y: 0.08, z: 1.5}
    customData: "wire_color=cyan"
    targetSignals: ["Signal_DoorA"]

  - name: "Door_ClassroomA"
    type: "Door"
    position: {x: -3.0, y: 0.0, z: 12.0}
    rotation: {x: 0.0, y: 90.0, z: 0.0}
    scale: {x: 1.0, y: 1.0, z: 1.0}
    customData: "speed=2.0"
    targetSignals: ["Signal_DoorA"]

  - name: "LevelExit"
    type: "LevelExit"
    position: {x: 0.0, y: 0.0, z: 28.0}
    rotation: {x: 0.0, y: 0.0, z: 0.0}
    scale: {x: 1.0, y: 1.0, z: 1.0}
    customData: "next_scene=Level_03"
    targetSignals: []
```

---

## 3. REGLAS DE SINTAXIS Y INTEGRIDAD DE BLUEPRINT (`BLU-VAL-*`)

- `VAL-BLU-001`: Todo Blueprint DEBE contener exactamente 1 módulo de tipo `PlayerStart` y al menos 1 módulo de tipo `LevelExit`.
- `VAL-BLU-002`: Los nombres en `targetSignals` de un emisor (ej. `PressurePlate`) deben coincidir exactamente con las señales esperadas por el receptor (ej. `Door`).
- `VAL-BLU-003`: La propiedad `maxRecordSeconds` no puede ser menor a `1.0f` ni superior a `20.0f` (per `DECISION-ECH-DURATION` en `decisions.yaml`). Niveles narrativos (`isNarrativeLevel: true`) pueden usar hasta `20.0f`. Ningún blueprint puede superar `20.0f` bajo ninguna circunstancia.
- `VAL-BLU-004`: `cameraProfile` y `lightingProfile` no deben ser `null` en un Blueprint listo para producción.

---

## 4. REGLAS DE GEOMETRÍA DE MÓDULOS (`BLU-MESH-*`)

```yaml
# [RULE-MESH-001] Wall Construction
wall_construction:
  wall_thickness_m: 0.20          # All interior walls
  exterior_wall_thickness_m: 0.30 # Perimeter walls
  wall_height_m: 3.20             # Matches architecture grid y = 3.2
  ceiling_mesh: "PlanarCap"       # Single flat face closing the module ceiling
  ceiling_offset_y: 3.20          # = wall_height_m
  floor_mesh: "PlanarCap"         # Single flat face for module floor
  floor_offset_y: 0.0

# [RULE-MESH-002] Door Cutout Geometry
door_cutout:
  width_m: 1.20
  height_m: 2.40
  offset_from_wall_center_m: 0.0  # Centered in wall
  frame_inset_m: 0.05             # Frame recesses 5cm into wall thickness
  uv_scale: {u: 1.0, v: 1.0}     # 1 tile per 1m of frame

# [RULE-MESH-003] UV Mapping Global
uv_tiling:
  floor:   {u_per_meter: 0.5, v_per_meter: 0.5}  # 1 tile every 2m for floors
  wall:    {u_per_meter: 1.0, v_per_meter: 1.0}   # 1 tile per meter for walls
  ceiling: {u_per_meter: 0.5, v_per_meter: 0.5}

# [RULE-MESH-004] Seam Prevention (Z-fighting)
module_connector_overlap_m: 0.02  # Adjacent modules overlap 2cm to prevent gaps
z_fighting_prevention: "Overlap"  # NOT "Gap" — a 0.0m gap produces black lines
```

Estas reglas son aplicadas por `EchoesModuleFactory.cs` durante Pass 1 de `EchoesNewProductionBuilder.cs`. Ver también `PROCEDURAL_MESH_SPEC.yaml` (SPEC-134) para especificación completa.
