# LANGUAGE VOCABULARY
## Echoes of You 2.0 — Fase 1 Entregable
## Fuente canónica: ECHOES_BIBLE.md + BRIEF_ESPACIAL_15_NIVELES.md

---

## PARTE 1 — VOCABULARIO DEL SISTEMA

---

```
ELEMENTO: ECO
SIGNIFICADO: Una decisión pasada que todavía ocupa el mundo.
             No es un clon. Es la repetición determinista de lo que el jugador hizo.
             Su rigidez es el diseño — no puede adaptarse a lo que cambió.
PUEDE:
  - Caminar, correr, saltar replicando la grabación exacta
  - Activar placas de presión y mantenerlas activas durante reproducción
  - Ocupar volumen físico (colisión activa en todos sus estados)
  - Revelar geometría invisible sin su presencia (puentes espectrales)
  - Permanecer 2.5s en estado Residual (colisión activa, opacidad 0.3→0)
  - Reproducir voz (solo Niveles 5, 10 y 13)
  - Ser usado como plataforma/bloqueador durante estado Residual
NO PUEDE:
  - Improvisar, decidir o reaccionar al entorno actual
  - Deshacerse retroactivamente
  - Ser controlado en tiempo real durante reproducción
  - Combatir o tener objetivos propios
  - Aparecer en más de 2 instancias simultáneas
COLOR/VISUAL:
  - Grabando: rim light cian en Aiden, eco no existe visualmente
  - Reproduciendo: cian translúcido #4FC3E8 alpha 0.45 (último 20%: alpha→0.1)
  - Residual: alpha 0.3→0 ease-in, 2.5s, colisión activa
  - Animaciones forzadas a max 15 FPS (stop-motion analógico)
AUDIO:
  - Pisadas con bandpass filter (sin agudos ni graves extremos)
  - Tape hiss sutil durante cualquier acción del eco
  - Fin (Residual): cassette stop — click seco, sin fade musical
  - Sin música cuando el eco está activo
```

---

```
ELEMENTO: PLACA DE PRESIÓN
SIGNIFICADO: Un umbral de decisión en el suelo. El peso de estar presente.
PUEDE:
  - Activarse con jugador o eco
  - Permanecer activa con peso sobre ella
  - Conectar a puertas con lógica AND
  - Integrarse naturalmente en el suelo
NO PUEDE:
  - Activarse sin presencia física
  - Ser la única mecánica de un nivel (es infraestructura, no experiencia)
COLOR/VISUAL:
  - Mat_Plate #141A29, emisión cyan al activar
  - Ligeramente hundida en el suelo, integrada en arquitectura
AUDIO:
  - Click mecánico suave al activar/desactivar
```

---

```
ELEMENTO: PUERTA
SIGNIFICADO: La consecuencia visible del estado del mundo.
             Cerrada: "todavía no". Abierta: "ya ocurrió".
PUEDE:
  - Abrirse cuando todas sus placas estén activas (AND)
  - Cerrarse si una placa se desactiva
  - Ser puerta de aula, mantenimiento, o metálica según espacio
  - Tener delay configurable de resultado (nunca instantánea)
NO PUEDE:
  - Parecer portal tecnológico o esclusa de laboratorio
  - Producir fanfarria al abrirse
COLOR/VISUAL:
  - Mat_Door #7E1E2F con emisión roja débil
  - Animación de apertura visible
AUDIO:
  - Crujido de madera o metal según el tipo de puerta
```

---

```
ELEMENTO: GHOST BRIDGE (Puente espectral)
SIGNIFICADO: Algo que existe en el tiempo del eco pero no en el tiempo presente.
             Cosas que solo son reales cuando alguien las recuerda.
PUEDE:
  - Materializarse durante grabación o reproducción del eco
  - Proveer colisión sólida cuando activo
  - Ser cruzado mientras el eco lo activa
NO PUEDE:
  - Existir de forma permanente
  - Activarse sin el eco
  - Usarse antes del Capítulo III
COLOR/VISUAL:
  - Translúcido cian #4FC3E8 alpha 0.3 cuando activo
  - Invisible cuando inactivo — el abismo es real
AUDIO:
  - Zumbido suave de aparición
```

---

```
ELEMENTO: PLATAFORMA TEMPORIZADA
SIGNIFICADO: El ritmo del mundo existe sin el jugador. Hay que aprenderlo.
PUEDE:
  - Moverse en ciclos fijos y predecibles
  - Ser parte del mobiliario del aula (tarima, estantería, escalón)
  - Requerir observación antes de acción
NO PUEDE:
  - Ser más de 4 por escena
  - Parecer plataforma de videojuego abstracta (necesita contexto arquitectónico)
  - Moverse de forma aleatoria
COLOR/VISUAL:
  - Mat_Bridge #3B4454 o material del espacio
  - Movimiento legible desde zona de observación previa
AUDIO:
  - Sonido mecánico suave sincronizado con el movimiento
```

---

```
ELEMENTO: ZONA DE GRAVEDAD
SIGNIFICADO: El mundo de Aiden no es consistente. Las reglas pueden cambiar.
PUEDE:
  - Alterar gravedad en un volumen
  - Afectar a jugador y eco
NO PUEDE:
  - Aparecer antes del Capítulo IV
  - Introducirse sin preparación visual previa
COLOR/VISUAL:
  - Borde sutil en wrongness-red #B23A3A cuando peligrosa
AUDIO:
  - Cambio sutil de mezcla de sonido ambiente al entrar
```

---

```
ELEMENTO: MECANISMO DE SECUENCIA
SIGNIFICADO: El orden importa más que la acción.
PUEDE:
  - Exigir orden específico de activación de ecos
  - Requerir tiempo muerto deliberado en una grabación
  - Combinarse con placas y puertas
NO PUEDE:
  - Introducirse antes del Capítulo II
  - Usarse con más de 2 ecos simultáneos
  - Resolverse sin que el fracaso sea legible sin texto
COLOR/VISUAL:
  - Integrado en arquitectura, sin forma propia
AUDIO:
  - Cada paso de la secuencia tiene sonido propio y progresivo
```

---

```
ELEMENTO: SALIDA DEL NIVEL
SIGNIFICADO: El final de un entendimiento, no el final de un nivel.
PUEDE:
  - Estar visible antes de ser alcanzable
  - Producir transición suave
  - Existir como puerta real (no portal)
NO PUEDE:
  - Producir fanfarria o celebración visual
  - Interrumpir con pantalla de resultados inmediata
COLOR/VISUAL:
  - Mat_Exit #FFEBB5 con emisión amber fuerte
  - Vista larga desde la zona de llegada del nivel
AUDIO:
  - Sonido de apertura es el único feedback de éxito
```

---

```
ELEMENTO: OBJETO NARRATIVO
SIGNIFICADO: Prueba de que alguien estuvo aquí. No decoración.
PUEDE:
  - Aparecer en cualquier nivel
  - Guiar la mirada sin ser interactivo
  - Usar memory-amber cuando tiene peso narrativo directo
NO PUEDE:
  - Tener texto que explique su significado
  - Aparecer en exceso (pierde peso)
  - Ser interactivo o coleccionable
COLOR/VISUAL:
  - memory-amber #E8B262 solo para el objeto más importante del espacio
  - Los demás usan materiales del espacio donde están
AUDIO:
  - Sin sonido propio
```

---

## PARTE 2 — GRAMÁTICA DE COMBINACIONES

```
ECO + PLACA = "El eco puede estar donde yo no estoy."
Capítulo: I | Niveles: 2 (p.1)

ECO + PLACA + PLACA = "Presencia simultánea en dos puntos."
Capítulo: I | Niveles: 2 (p.2), 3

TIMING DE GRABACIÓN + PLACA = "El momento de inicio importa tanto como lo que grabo."
Capítulo: I (final) | Niveles: 2 (p.3), 4

ECO + PLATAFORMA TEMPORIZADA = "Coordinar con el ritmo del mundo."
Capítulo: II | Niveles: 4, 5

ECO + GHOST BRIDGE = "Confiar en algo que ya no controlo."
Capítulo: III ⛔ | Niveles: 6

GRABACIÓN ANTICIPADA + GHOST BRIDGE = "Grabo el cruce antes de que el puente exista."
Capítulo: III ⛔ | Niveles: 6, 7

GRABACIÓN ANTICIPADA + ELEMENTO TEMPORAL = "Grabo lo que voy a necesitar que mi pasado haya hecho."
Capítulo: III ⛔ | Niveles: 7

ECO A + ECO B + ORDEN = "El tiempo muerto es una decisión, no un error."
Capítulo: II (avanzado) | Niveles: 8, 12

ECO COMO CRONÓMETRO + PELIGRO = "El eco me dice cuándo cruzar, no cómo."
Capítulo: III ⛔ | Niveles: 9

ECO AMBIENTAL + VISIBILIDAD = "Necesitar el punto de vista de alguien que ya no está."
Capítulo: IV ⛔ | Niveles: 10

DURACIÓN LIMITADA + ELECCIÓN = "No todo puede quedar en el eco."
Capítulo: IV ⛔ | Niveles: 11

ECO A vs ECO B + CONTRADICCIÓN = "Decidir qué costo estás dispuesto a pagar."
Capítulo: V ⛔ | Niveles: 12

GRABACIÓN NO ELEGIDA + RESTRICCIÓN = "Trabajar con lo que hay, no con lo que quiero."
Capítulo: V ⛔ | Niveles: 13

INVERSIÓN (sin grabación) + SIMETRÍA = "El eco lidera. El jugador sigue."
Capítulo: VI ⛔ | Niveles: 14
```

---

## PARTE 3 — TAXONOMÍA DE FAMILIAS DE PUZZLE

| Familia | Principio | Cap. intro | Niveles |
|---|---|---|---|
| Timing vertical | Cuándo grabar en eje Y | II | 4 |
| Timing horizontal | Cuándo grabar en eje X (laberinto) | II | 5 |
| Anticipación causal | Grabar lo que aún no ocurrió | III | 6, 7 |
| Revelación espacial | El eco revela lo invisible | III | 6, 9, 10 |
| Restricción temporal | La grabación no alcanza para todo | IV | 11 |
| Orden de activación | La secuencia importa más que la acción | II | 2, 8 |
| Consecuencia negativa | El eco puede perjudicar | V | 5 (intro suave), 12, 13 |
| Inversión | El eco lidera, el jugador sigue | VI | 14 |
| Síntesis | Todo el sistema en forma más limpia | VI | 15 |

**Reglas:**
- Ningún nivel usa una familia antes de su capítulo de introducción.
- La introducción siempre es en la forma más simple posible.
- La Síntesis (N15) puede usar cualquier familia anterior, reducida.
- Niveles 1 y 3 son narrativos/vocabulario — sin familia de puzzle propia.

---

## PARTE 4 — REPORTE DE PIPELINE (estado verificado)

```
✅ EchoesLevelBuilder.cs — eliminado
✅ EchoesProductionBuilder.cs — solo referencia, no ejecutar
✅ EchoesNewProductionBuilder.cs — único builder activo
✅ EchoesQueuedProductionRebuild flag — desactivado (.flag no existe)
✅ update_all_production.py — en Tools/Scripts/, no ejecutar
✅ Constantes SciFi* — solo como término de exclusión en ResolveAssetPath()
✅ SetupAtmosphere — lee blueprint.fogColor y blueprint.ambientColor (EchoesLevelShell.cs L35-42)
⚠️  Cámara — verificar en Unity: Cinemachine Y ThirdPersonCamera en proyecto
✅ Materiales magenta — pipeline URP confirmado, riesgo bajo
✅ RENDER PIPELINE — URP activo (GraphicsSettings.asset L40 m_CustomRenderPipeline no nulo)
✅ EchoesMaterialLibrary — usa "Universal Render Pipeline/Lit", correcto
```

---

## PARTE 5 — FRASES DE UNA LÍNEA

| # | Frase |
|---|---|
| 1 | "El pasado ya ocupa el mundo — todavía no sé cómo." |
| 2 | "El eco puede estar donde yo no estoy." |
| 3 | "Con el eco puedo estar en dos lugares, pero no en ninguno a la vez." |
| 4 | "El momento de inicio de la grabación importa tanto como lo que grabo." |
| 5 | "Mi pasado puede atraparme si lo uso sin pensar en las consecuencias." |
| 6 | "Debo cruzar antes de ver que puedo cruzar." |
| 7 | "No grabo lo que hice — grabo lo que voy a necesitar que mi pasado haya hecho." |
| 8 | "El tiempo vacío en una grabación es una decisión, no un error." |
| 9 | "El eco no me ayuda a cruzar — me dice cuándo cruzar." |
| 10 | "Necesito el punto de vista de alguien que ya no está para ver el camino." |
| 11 | "No todo puede quedar en el eco — elijo qué parte del pasado conservo." |
| 12 | "Dos versiones del pasado no pueden coexistir — decido cuál costo pago." |
| 13 | "Trabajo con la grabación que hay, aunque no sea la que habría elegido." |
| 14 | "El eco lidera. Yo sigo." |
| 15 | "Todo lo que aprendí, usado una vez más, con el peso de haberlo aprendido." |

---

## PARTE 6 — FILTRO DE LOS CUATRO CRITERIOS (vertical slice)

### Nivel 1
1. **¿Qué enseña?** El espacio de Aiden se repite — la identidad del lugar es inestable.
2. **¿Usa lo anterior?** Primer nivel. Construye vocabulario espacial.
3. **¿Emoción?** Confusión suave y reconocimiento interrumpido.
4. **¿Por qué solo aquí?** El Pasillo B idéntico al A no es un truco — es el eco anunciado antes de que el jugador lo controle. El eco aparece sin ser convocado. Eso no podría existir en Portal.

### Nivel 7
1. **¿Qué enseña?** Causalidad inversa: grabar el futuro para que el pasado lo cumpla.
2. **¿Usa lo anterior?** Timing (Cap. II), Ghost Bridge (Nivel 6).
3. **¿Emoción?** El vértigo de entender que el tiempo puede funcionar al revés.
4. **¿Por qué solo aquí?** En Portal se manipula el espacio. Aquí se manipula cuándo ocurrió algo. La solución requiere entender que el "yo del pasado" puede obedecer órdenes que el "yo del presente" todavía no dio.

### Nivel 14
1. **¿Qué enseña?** La inversión total: el jugador ya no graba, sigue al eco.
2. **¿Usa lo anterior?** Todo el sistema — pero ausente. La ausencia del grabado es la mecánica.
3. **¿Emoción?** La quietud de dejar de intentar controlar.
4. **¿Por qué solo aquí?** Requiere 13 niveles grabando para que no poder grabar tenga peso. La simetría solo existe porque la asimetría fue aprendida.
