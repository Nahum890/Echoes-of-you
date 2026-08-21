# claude-docs

Informes de estado del proyecto *Echoes of You*, escritos por Claude Code.

## Cómo leer esto

Cada afirmación está marcada con su nivel de confianza. Es importante: en este
proyecto hay **118 documentos `.md` en `Docs/`** (332 archivos en total) y una
parte describe un estado que el código ya no tiene.

| Marca | Significado |
|---|---|
| ✅ **Verificado** | Comprobado contra el código, los assets o ejecutando Unity. Cada documento indica su fecha de corte. |
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

El [`CLAUDE.md`](../CLAUDE.md) de la raíz es el resumen operativo destilado de
estos seis documentos: lo que hay que saber antes de tocar nada.

## Advertencia general sobre los docs del proyecto

⚠️ **La capa de enforcement de specs es en gran parte ficción.**
`ExecutableSpecValidator.cs` valida **una sola regla** (`MAT-001`) frente a
decenas de `RULE-`/`CONS-`/`FAIL-`/`VAL-` repartidas por los specs.

Matiz sobre `LevelValidator`, citado por `VAL-INP-001` y `FAIL-CAM-01`: no
existe el **archivo** `LevelValidator.cs`, pero sí una clase `LevelValidator`
dentro de `Assets/Editor/SchoolGreyboxProductionBuilder.cs:158`. Expone
`ValidateScene` y `ValidateGroupA_Architecture`, **no** el `RunAllChecks` que le
pide el CI ✅ **verificado 2026-08-20**.

Y el CI en sí tampoco valida lo que dice — ver
[05](05-BUGS-Y-PENDIENTES.md#el-workflow-de-ci-es-en-gran-parte-ficción).

Nada impide que los docs y el código se separen, y de hecho se han separado.
**Verificar contra el código antes de creer cualquier documento**, incluidos
estos.
