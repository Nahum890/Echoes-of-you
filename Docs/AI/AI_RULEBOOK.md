# AI_RULEBOOK.md — Contrato de Ejecución para Agentes IA
## Spec ID: SPEC-401 | Version: 4.0 (Zero-Inference Compliant)
## Authority: Level 6 (per SOURCE_OF_TRUTH.md §7)

---

### OBLIGACIONES PRE-EJECUCIÓN [AI-OBL-*]

Antes de ejecutar cualquier tarea, el agente DEBE leer en orden:
1. `Docs/Authority/SOURCE_OF_TRUTH.md` (SPEC-000) — jerarquía de autoridad
2. `Docs/ExecutableSpecs/manifest.yaml` — punto de entrada YAML
3. El spec del sistema objetivo de la tarea (ej. SPEC-107 para Echo)
4. Verificar existencia de scripts C# referenciados en `Assets/Scripts/` antes de proponer código

### PROHIBICIONES ABSOLUTAS [AI-PRB-*]

| ID | Prohibición | Fallo |
|---|---|---|
| AI-PRB-001 | NO referenciar scripts C# que no existen en el repositorio | Compilación rota |
| AI-PRB-002 | NO usar `Shader.Find("Standard")` ni Built-in RP | Magenta en runtime |
| AI-PRB-003 | NO crear builders adicionales a `EchoesNewProductionBuilder.cs` | Conflicto |
| AI-PRB-004 | NO marcar tarea completa sin ejecutar validación | Bugs en escena |
| AI-PRB-005 | NO hacer suposiciones. Si un parámetro falta, emitir FAIL-SOT-01 | Halt |
| AI-PRB-006 | NO sobrescribir valores de Frozen Decisions Matrix via YAML | FAIL-SOT-01 |

### CONTRATO DE ENTREGA [AI-COM-*]

Al finalizar, entregar EXACTAMENTE este YAML. No añadir, no omitir campos:
```yaml
report:
  task_id: "string"          # ID de la tarea asignada
  specs_read: ["list"]       # Spec IDs leídos (ej. ["SPEC-000", "SPEC-107"])
  files_modified: ["list"]   # rutas exactas de archivos modificados
  validations_passed: ["list"] # IDs de validación ejecutados y aprobados
  errors_remaining: ["list"] # FAIL-xxx codes pendientes de resolución
  next_agent_required: "string | null" # siguiente agente si la tarea es multi-agente
```
**PROHIBIDO:** El campo de suposiciones no existe en este protocolo.
Si el agente necesitaría asumir algo, debe en su lugar emitir `FAIL-SOT-01` y detener la ejecución.