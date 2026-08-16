# ECHOES_BIBLE.md — Biblia Maestra de Diseño y Dirección de Juego
## ID del Documento: [DOC-101]
## Versión: 3.0 (AI-Executable)

---

### 1. PROPÓSITO Y DOMINIO
Constituye el documento normativo maestro de diseño, pilares mecánicos y dirección temática de *Echoes of You 2.0*. Define la visión inviolable del juego como un **mensaje dual inseparable**:

1. **Aceptar el pasado**: "El pasado no se puede cambiar. Llevarlo con firmeza, no con negación, es el primer paso."
2. **Mejorar como persona**: "Aceptar no es resignarse. Es comprender para dejar de repetir, y dejar ir para poder crecer."

El entorno del juego es **la mente de Aiden** — una chica procesando errores que cometió con alguien querido (Lyra). Cada pasillo, aula y placa representa un mecanismo psicológico en tensión: el recuerdo que quiere modificarse vs el recuerdo que solo puede aceptarse. El Eco es la metáfora central: una acción grabada que no se puede deshacer, pero cuya comprensión transforma al que la observa.

Aiden comienza creyendo que tiene razón. Si el jugador comprende el panorama general, ella evoluciona hacia la aceptación y el crecimiento. Si no, los finales malos la atrapan en la negación, la culpa o la repetición del patrón. El contrato supremo rige el comportamiento del Eco, la evolución de la voz interna de Aiden, y los 4 pilares de gameplay.

### 2. DEPENDENCIAS E INPUTS
- [SOURCE_OF_TRUTH.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Authority/SOURCE_OF_TRUTH.md) `[DOC-000]`
- [DESIGN_PHILOSOPHY.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/DESIGN_PHILOSOPHY.md) `[DOC-001]`
- [NARRATIVA_INTERNA.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/GameDesign/NARRATIVA_INTERNA.md) `[DOC-102]`

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
- `[BIB-TXT-003]`: **Ausencia de Diálogo Externo**: Prohibida la inclusión de diálogos parlantes, bocadillos de texto diegético de tercera persona, narrador externo, o lore text logs durante el recorrido de los niveles. Excepción explícita: Pop-ups de inspección (`interaction.*`) que representan la **voz interna de Aiden en 1ª persona**, máximo $42$ caracteres, mostrados únicamente dentro del sistema `Chalkboard` del HUD. Estos pop-ups muestran la evolución psicológica de Aiden: al principio cree tener razón, y el texto refleja esa convicción; con el tiempo (si el jugador comprende el panorama general) el tono evoluciona hacia la duda y la aceptación. Nota: Los títulos de nivel HUD y textos de objetivos de interfaz (`UI_SPEC.md`, `BLUEPRINT_SPEC.md`) están explícitamente permitidos.
- `[BIB-AIN-006]`: **Voz Interna Variable por Etapa**: Los pop-ups de inspección representan la voz interna de Aiden (POV 1ª persona). Prohibido usar tiempo pasado evocativo ("era buen momento") — solo presente o futuro hipotético ("Puedo cambiar esto"). El tono debe variar según el `emotional_arc` de cada nivel para reflejar el proceso psicológico: N01-N04 (negación, convicción de tener razón), N05-N08 (culpa, primer quiebre), N09-N12 (realización, comprensión parcial), N13-N15 (aceptación, soltar). Si Aiden no comprende el panorama, el tono permanece atrapado en etapas tempranas incluso en niveles avanzados.
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
| Duración Pop-up Inspección (voz interna) | `2.5` | segundos | `±1.0` |
| Máximo Caracteres por Pop-up | `42` | chars | `Max 42` |

---

### 6. ANTI-PATRONES PROHIBIDOS
- `[ANTI-BIB-001]`: Prohibido transformar el juego en un platformer de reflejos rápidos (obby).
- `[ANTI-BIB-002]`: Prohibido reducir los puzzles a secuencias triviales de "Pisa placa $\rightarrow$ abre puerta $\rightarrow$ cruza" sin componente de sincronización temporal con Eco.
- `[ANTI-BIB-003]`: Prohibido emplear textos expositivos para explicar la historia, la causa del distanciamiento entre Aiden y Lyra, o el significado moral del juego. El mensaje dual ("aceptar el pasado" + "mejorar como persona") debe emerja del gameplay y la evolución de la voz interna, nunca de un narrador o texto explicativo.
- `[ANTI-BIB-004]`: **Ambigüedad Relacional Protegida**: Prohibido clarificar la naturaleza exacta de la relación entre Aiden y Lyra (pareja, amistad, o algo intermedio). La ambigüedad es esencial y temáticamente central. Los pop-ups pueden contener micro-gestos íntimos ("Me sentaba demasiado cerca") pero NUNCA palabras como "amiga", "novia", "pareja", "relación". El jugador debe concluir su propia lectura.
- `[ANTI-BIB-005]`: **No Auto-Justificación Final**: Prohibido que ningún texto del juego, ending, o epílogo confirme que Aiden "tenía razón" o "tenía la culpa". La tensión entre aceptar-sin-cambiarse y mejora-activa debe permanecer sin resolver textualmente; solo la mecánica y la evolución del tono pueden sugerir el camino.

### 7. CRITERIOS DE VALIDACIÓN Y QA
- `[QA-BIB-V01]`: `LevelValidator.cs` verifica que todos los niveles contengan exactamente 1 `LevelGoal` y cumplan la ventana de timing $\ge 0.4\text{s}$.
- `[QA-BIB-V02]`: Se confirma la ausencia total de scripts de diálogo parlante externo en los componentes de la escena de gameplay.
- `[QA-BIB-V03]`: Validador de texto (`TextInspector.cs`) confirma: (a) 0 textos en 3ª persona, (b) 0 ocurrencias de palabras ["amiga", "novia", "pareja", "relación"] en `VN_Text.es.yaml`, (c) todos los `interaction.*` pop-ups usan 1ª persona y ≤42 chars.
- `[QA-BIB-V04]`: Validador de tono valida que el `tone_by_level` de cada `interaction.*` matches el rango de etapa esperado (N01-N04: denial/conviction; N05-N08: guilt; N09-N12: realization; N13-N15: acceptance). Un pop-up "aceptación" en N02 = `FAIL-BIB-04`.

### 8. CHANGE HISTORY
- **v1.0 (2025-02-14)**: Initial vision draft.
- **v3.0 (2026-07-25)**: Full refactor into 14-section AI-Executable canonical format.
- **v3.1 (2026-07-25)**: CC-2026-010/011 — RULE-SOT-001B scope + AI specs Level 5.
- **v3.2 (2026-08-02)**: Reescritura narrativa — Aiden como chica; narrativa dual (aceptar el pasado + mejorar como persona); voz interna 1ª persona con tono variable por etapa; ANTI-BIB-004 (ambigüedad relacional), ANTI-BIB-005 (no auto-justificación final); BIB-AIN-006 (voz interna variable). Docs de input: NARRATIVA_INTERNA.md [DOC-102].
