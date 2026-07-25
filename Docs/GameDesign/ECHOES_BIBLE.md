# ECHOES_BIBLE.md — Biblia Maestra de Diseño y Dirección de Juego
## ID del Documento: [DOC-101]
## Versión: 3.0 (AI-Executable)

---

### 1. PROPÓSITO Y DOMINIO
Constituye el documento normativo maestro de diseño, pilares mecánicos y dirección temática de *Echoes of You 2.0*. Define la visión inviolable del juego: "Imposibilidad de deshacer el pasado, no sobre controlarlo". Rigió como el contrato supremo sobre el comportamiento del Eco, la narrativa implícita de Aiden y Lyra, y los 4 pilares de gameplay.

### 2. DEPENDENCIAS E INPUTS
- [SOURCE_OF_TRUTH.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Authority/SOURCE_OF_TRUTH.md) `[DOC-000]`
- [DESIGN_PHILOSOPHY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/DESIGN_PHILOSOPHY.md) `[DOC-001]`

### 3. OUTPUTS GENERATED
- Directivas normativas para todos los subsistemas de juego y specs ejecutables (`Docs/Specs/*.md`).
- Criterios de evaluación cualitativos y cuantitativos para la suite de QA.

### 4. REGLAS EJECUTABLES Y MEDIBLES

- `[BIB-VIS-001]`: **Principio Irreversible**: Prohibido implementar cualquier sistema de cancelación o rebobinado instantáneo (Undo) de un Eco grabado durante su ciclo activo de reproducción.
- `[BIB-PIL-002]`: **Jerarquía de los 4 Pilares**: Todo elemento de juego debe servir explícitamente a uno de los 4 pilares:
  1. *Timing de Lectura*: Ventana temporal de grabación determinista ($12.0\text{s}$).
  2. *Exploración Espacial*: Recorrido mínimo obligatorio de $6.0\text{m}$ antes de la primera interacción con placa/botón.
  3. *Consecuencia Física*: El Eco actúa como obstáculo collider estricto durante reproducción.
  4. *Revelación Temporal*: Elementos visibles únicamente bajo resonancia de Eco (puentes fantasma).
- `[BIB-TXT-003]`: **Ausencia de Diálogo Parlante**: Prohibida la inclusión de diálogos parlantes o bocadillos de texto diegético en pantalla durante el recorrido de los niveles. Nota: Los títulos de nivel HUD y textos de objetivos de interfaz (`UI_SPEC.md`, `BLUEPRINT_SPEC.md`) están explícitamente permitidos.
- `[BIB-DUR-004]`: **Duración de Grabación Standard**: `maxRecordSeconds` asignado exactamente en $12.0\text{s}$ (configurable hasta $20.0\text{s}$ únicamente en salas narrativas especificadas en `LevelBlueprint`).
- `[BIB-RST-005]`: **Regla del SoftReset ('Q')**: La tecla 'Q' ejecuta un `SoftReset` que recoloca la posición del personaje en el spawn del nivel y vacía los slots de Eco sin destruir el estado de progreso ni las memorias desbloqueadas en la sesión actual, preservando la irreversibilidad mecánica del Eco.
- `[BIB-LAT-006]`: **Latencia del Eco**: La reproducción del Eco inicia con un retardo fijo de $0.8\text{s}$ tras soltar el botón de grabación; el retardo de sincronización de la animación del modelo debe ser $\le 0.05\text{s}$.

### 5. MATRIZ DE CONFIGURACIÓN Y VALORES (PARÁMETROS MAESTROS DEL JUEGO)

| Parámetro Maestro | Valor Métrica Exacta | Unidad | Tolerancia QA |
|---|---|---|---|
| Duración Grabación Eco Standard | `12.0` | segundos | `±0.0` |
| Duración Grabación Eco Max Narrativo | `20.0` | segundos | `±0.0` |
| Latencia Inicio Reproducción Eco | `0.8` | segundos | `±0.0` |
| Retardo Máximo Animación Eco | `0.05` | segundos | `Max 0.05s` |
| Tecla Teclado SoftReset Posición/Slots | `Q` | KeyCode | Exacto |
| Velocidad Desplazamiento Caminata | `2.8` | m/s | `±0.0` |
| Velocidad Desplazamiento Carrera | `4.2` | m/s | `±0.0` |
| Número Total de Niveles Principales | `15` | Niveles | `Exacto` |
| Tolerancia de Timing en Puzzles | `0.4` | segundos | `Min 0.4s` |
| Resplandor Token Narrativo (`memory-amber`) | `#FFBF00` / `1.2 Lux` | RGB / Lux | Exacto |

---

### 6. ANTI-PATRONES PROHIBIDOS
- `[ANTI-BIB-001]`: Prohibido transformar el juego en un platformer de reflejos rápidos (obby).
- `[ANTI-BIB-002]`: Prohibido reducir los puzzles a secuencias triviales de "Pisa placa $\rightarrow$ abre puerta $\rightarrow$ cruza" sin componente de sincronización temporal con Eco.
- `[ANTI-BIB-003]`: Prohibido emplear textos expositivos para explicar la historia o la causa del distanciamiento entre Aiden y Lyra.

### 7. CRITERIOS DE VALIDACIÓN Y QA
- `[QA-BIB-V01]`: `LevelValidator.cs` verifica que todos los niveles contengan exactamente 1 `LevelGoal` y cumplan la ventana de timing $\ge 0.4\text{s}$.
- `[QA-BIB-V02]`: Se confirma la ausencia total de scripts de diálogo parlante en los componentes de la escena de gameplay.
