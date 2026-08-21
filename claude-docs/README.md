# claude-docs

Informes de estado del proyecto *Echoes of You*, escritos por Claude Code.

## Cómo leer esto

Cada afirmación está marcada con su nivel de confianza. Es importante: en este
proyecto hay ~250 documentos en `Docs/` y una parte describe un estado que el
código ya no tiene.

| Marca | Significado |
|---|---|
| ✅ **Verificado** | Comprobado contra el código, los assets o ejecutando Unity en esta sesión (2026-08-20). |
| ⚠️ **Heredado** | Viene de sesiones anteriores. Era cierto cuando se escribió; puede haber caducado. |
| ❓ **Sin verificar** | Sospecha razonada, no comprobada. Tratar como hipótesis. |

## Índice

| Documento | Contenido |
|---|---|
| [01 — Estado actual](01-ESTADO-ACTUAL.md) | Stack, ramas, entorno, qué hay en `main` |
| [02 — Pipeline visual](02-PIPELINE-VISUAL.md) | El look PS1: arquitectura, instalación, ajuste |
| [03 — Materiales y superficies](03-MATERIALES-Y-SUPERFICIES.md) | Sistema de materiales, pases, generador de texturas |
| [04 — Auditoría de escenas](04-AUDITORIA-ESCENAS.md) | Datos por nivel: materiales, luces, niebla |
| [05 — Bugs y pendientes](05-BUGS-Y-PENDIENTES.md) | Lo encontrado y no arreglado, priorizado |
| [06 — Herramientas y flujos](06-HERRAMIENTAS-Y-FLUJOS.md) | Menús, batch mode, compile check, git |

## Advertencia general sobre los docs del proyecto

⚠️ **La capa de enforcement de specs es en gran parte ficción.**
`ExecutableSpecValidator.cs` valida **una sola regla** (`MAT-001`) frente a
decenas de `RULE-`/`CONS-`/`FAIL-`/`VAL-` repartidas por los specs.
`LevelValidator.cs`, citado por `VAL-INP-001` y `FAIL-CAM-01`, **no existe**.

Nada impide que los docs y el código se separen, y de hecho se han separado.
**Verificar contra el código antes de creer cualquier documento**, incluidos
estos.
