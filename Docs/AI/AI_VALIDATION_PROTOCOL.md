# AI_VALIDATION_PROTOCOL.md — Protocolo de Verificación y Cierre de Tareas
## Echoes of You v3.0

---

## 1. PROTOCOLO DE CIERRE DE TAREA (CHECKLIST OBLIGATORIO) (`AI-VAL-CHK-*`)

Antes de marcar una tarea como `COMPLETADA` en `DOCUMENT_STATUS.md` o en un reporte, el agente debe ejecutar los siguientes 8 pasos:

```text
[1] CONFIRMAR AUTORIDAD DOCUMENTAL
    -> ¿Lo que hice está respaldado por SOURCE_OF_TRUTH.md y por la Spec correspondiente?

[2] CONFIRMAR ARCHIVOS REALES
    -> ¿Los scripts que usé o cité existen físicamente en Assets/Scripts o Editor?

[3] CONFIRMAR COMPILACIÓN
    -> ¿El código compila sin errores? Verificar `ConsoleWindow` o `read_console`.

[4] EJECUTAR VALIDACIONES CORRESPONDIENTES
    -> Ejecutar checks del LEVEL_VALIDATOR.md relevantes a mi dominio.

[5] REVISAR REGRESIONES
    -> ¿Mi cambio afectó otro nivel o rompió una señal existente?

[6] IDENTIFICAR Y REPORTAR CONTRADICCIONES
    -> Si encontré una discrepancia entre la documentación y el código, la registré en CHANGE_CONTROL.md.

[7] REPORTAR CHECKS OMITIDOS
    -> Si no pude ejecutar una validación (por falta de runtime o assets), lo anoté en el reporte.

[8] EMITIR VEREDICTO FINAL
    -> "APPROVED" / "NEEDS REVIEW" / "BLOCKED".
```

---

## 2. EJEMPLO DE REPORTE DE CIERRE (`AI-VAL-RPT-*`)

```yaml
task_id: "N02_PuzzleFix_001"
agent_role: "DISEÑADOR DE PUZZLES"
docs_read: ["ECHOES_BIBLE.md", "PUZZLE_GRAMMAR.md", "BLUEPRINT_SPEC.md"]
files_modified: ["Assets/Data/Levels/Level_02_Blueprint.asset"]
validations_passed:
  - "VAL-B-001: Clearance > 1.2m en aula"
  - "VAL-B-002: Señales correctas en circuito"
errors_remaining: []
contradictions_found: ["CC-2026-004: Camera conflict"]
veredict: "APPROVED"
```