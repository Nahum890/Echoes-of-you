# AI_RESPONSIBILITIES.md — Matriz de Permisos y Prohibiciones por Agente
## Echoes of You v3.0

---

## 1. MATRIZ DE ACCESO A DOCUMENTOS Y SCRIPTS POR ROL

| Agente | Docs Autorizados | Scripts Permitidos | Scripts PROHIBIDOS |
|---|---|---|---|
| **ARQUITECTO** | SOURCE_OF_TRUTH, SCHOOL_ARCHITECTURE, ROOM_TEMPLATES, TRANSITION_GRAMMAR, SCALE_GUIDE | Ninguno directo; propone módulos | EchoesNewProductionBuilder.cs |
| **DISEÑADOR DE PUZZLES** | ECHOES_BIBLE, ECHO_GRAMMAR, PUZZLE_GRAMMAR, BLUEPRINT_SPEC | PuzzleSignals, PuzzleWire, LevelBlueprint.cs | PlayerController.cs |
| **CONSTRUCTOR BLUEPRINT** | BLUEPRINT_SPEC, MODULE_LIBRARY, PROP_GRAMMAR | LevelBlueprint.cs (Asset creation) | GameFeelController.cs |
| **GENERADOR ESCENA** | BLUEPRINT_SPEC, MODULE_LIBRARY | EchoesNewProductionBuilder.cs, EchoesModuleFactory.cs | EchoesLevelBuilder.cs (LEGACY) |
| **QA ENGINE** | LEVEL_VALIDATOR, FAILURE_PATTERNS, SCALE_GUIDE | EnvironmentPassValidator.cs, CameraPassQA.cs | PlayerController.cs |
| **VISUAL PASS** | LIGHTING_GRAMMAR, MATERIAL_GRAMMAR, COMPOSITION, SHAPE_LANGUAGE | EchoesMaterialLibrary.cs, LightingApplier.cs | - |
| **DIRECTOR XD** | EMOTIONAL_FLOW, NARRATIVE_ENVIRONMENT, ECHOES_BIBLE | Ninguno directo; revisa escenas | - |

---

## 2. CAPACIDADES Y RESTRICCIONES

- `AI-RESP-001`: Ningún agente debe modificar `PlayerController.cs` salvo autorización explícita del desarrollador humano.
- `AI-RESP-002`: Solo el `QA ENGINE` puede ejecutar builds de prueba o modificar `BuildSettings`.
- `AI-RESP-003`: Solo el `DIRECTOR XD` puede cambiar la emoción o narrativa de un nivel ya aprobado por otro agente.