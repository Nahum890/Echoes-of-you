# ECHOES BIBLE
### Documento maestro de diseño — Echoes of You
### Versión 2.0 — Julio 2026

---

> **Función de este documento:** esta Biblia tiene autoridad final sobre cualquier
> sugerencia que la contradiga. Si una IA, auditoría externa, o análisis futuro
> recomienda algo que entra en conflicto con lo escrito aquí, este documento
> gana. La única forma de cambiar una decisión tomada en estas páginas es que
> el desarrollador la actualice explícitamente. Las auditorías proponen. Esta
> Biblia decide.

---

## 0 — MANIFIESTO

Echoes of You no es un juego sobre puzzles.
Es un juego sobre lo que hacemos con lo que ya no podemos cambiar.

El jugador no controla un clon.
Controla una decisión pasada que todavía ocupa el mundo.

El eco no es una herramienta.
Es una huella activa.

Cada acción grabada permanece.
No se rebobina.
No se deshace.
Solo se convive.

---

## 1 — VISIÓN EN UNA FRASE

> **Echoes of You es sobre la imposibilidad de deshacer el pasado, no sobre controlarlo.**

Esta frase es el criterio de aceptación de cualquier mecánica, nivel, o
decisión de diseño. Si algo contradice esta frase, no pertenece al juego.

---

## 2 — QUÉ ES ESTE JUEGO

- Un puzzle 3D narrativo en tercera persona.
- Un juego de planificación, timing y consecuencia — no de reflejos.
- Un juego donde el espacio es el puzzle, no el contenedor del puzzle.
- Una exploración de la memoria de Aiden: un joven que postergó una
  conversación con Lyra hasta que ya no fue posible tenerla.
- Un juego que el jugador debe poder entender emocionalmente sin leer
  una sola línea de texto de historia.

---

## 3 — QUÉ NO ES ESTE JUEGO

Estas prohibiciones son permanentes. No se renegocian.

- **No es un obby.** Las plataformas no son el punto.
- **No es un juego de botones y puertas.** "Pisa placa → abre puerta → cruza"
  no es un puzzle: es infraestructura. Puede existir como base técnica.
  No puede existir como experiencia completa de un nivel.
- **No es un juego de reflejos.** La dificultad viene de pensar, no de reaccionar rápido.
- **No es un walking simulator vacío.** El jugador siempre tiene algo que resolver o descubrir.
- **No es una sucesión de salas cerradas.** El mundo debe sentirse como
  un lugar, no como una colección de pruebas de laboratorio.
- **No es Portal.** Portal enseña a pensar en espacio. Echoes enseña a
  pensar en tiempo y consecuencia. La referencia es válida para claridad
  de diseño. No es una licencia para copiar estética ni estructura.

---

## 4 — LOS CUATRO PILARES

Todo en el juego sirve a estos cuatro pilares, en este orden de prioridad.
Si algo no sirve a ninguno de los cuatro, se elimina.

### Pilar 1 — Timing
El corazón del sistema. Pero no timing de reflejos — timing de lectura.
El jugador aprende cuándo grabar, cuándo esperar, cuándo activar.
La ventana temporal importa más que la velocidad de ejecución.

### Pilar 2 — Exploración
El jugador no debe entrar a un espacio y ver la solución.
Debe recorrerlo, leer su lógica y encontrar el ángulo correcto.
La exploración no es decoración ni relleno: es el medio por el que
el jugador aprende las reglas antes de que el puzzle las exija.

### Pilar 3 — Consecuencia
Usar mal el eco no solo falla el puzzle.
Cambia el estado del espacio: bloquea rutas, cierra opciones, encuadra
al jugador en una posición difícil. El eco puede ser aliado o trampa,
y la diferencia la decide únicamente el jugador con sus propias acciones.

### Pilar 4 — Revelación
Algunas cosas solo existen durante la grabación, durante la reproducción,
o desde un ángulo específico. El espacio tiene capas temporales. El jugador
aprende a leer qué existe cuándo, no solo dónde.

---

## 5 — LA MECÁNICA DEL ECO — ESPECIFICACIÓN COMPLETA

### Qué es el eco

El eco es la grabación de los movimientos del jugador (hasta 20 segundos)
reproducida como un cuerpo físico en el mundo. No es un clon. No es una IA.
Es una repetición determinista e imperfecta de lo que el jugador hizo.

La imperfección no es aleatoria: el eco no improvisa ni comete errores
imprevistos. Su "imperfección" es narrativa y mecánica — es una versión
del jugador que no conoce el estado actual del mundo, que ejecuta lo que
se le ordenó sin adaptarse a lo que cambió. Esa rigidez es el diseño.

### Qué puede hacer el eco

- Caminar, correr, saltar.
- Empujar objetos si fueron tomados o empujados durante la grabación.
- Activar interruptores, pisarlos, mantenerlos activos.
- Ocupar volumen físico (colisión activa).
- Reproducir voz grabada (uso narrativo excepcional — ver Sección 9).
- Revelar geometría que es invisible sin su presencia (puentes espectrales, caminos de niebla).
- Permanecer en su posición final durante 2.5 segundos en estado "residual"
  (colisión activa, opacidad decreciente) antes de desaparecer.

### Qué NO puede hacer el eco

- Combatir. Jamás.
- Improvisar, decidir, o reaccionar al entorno.
- Deshacer retroactivamente lo que ya grabó.
- Ser controlado en tiempo real una vez iniciada la reproducción.

### Estados visuales del eco (obligatorios)

| Estado | Visual | Duración |
|---|---|---|
| **Grabando** | Aiden recibe un rim light cian. El eco no existe visualmente — está en el futuro. | Hasta soltar la tecla |
| **Reproduciendo** | Eco cian translúcido, alpha 0.45. En el último 20% del tiempo, alpha desciende a 0.1. | Duración grabada |
| **Residual** | Alpha 0.3 → 0 en curva ease-in. Colisión activa durante estos 2.5s. | 2.5 segundos |

El estado residual es mecánico, no solo estético. Un eco en estado residual
puede usarse como plataforma, bloqueador, o punto de apoyo. Los niveles
deben diseñarse aprovechando esta ventana.

### Latencia de inicio

El eco tiene una demora fija de 0.8 segundos desde que se activa hasta que
comienza a moverse. Esta demora es determinista — siempre la misma. Niveles
que exigen timing preciso deben diseñarse considerando esta ventana.

### Regla de irreversibilidad

Una grabación reproducida no puede deshacerse excepto re-grabando desde
cero. Esta es la regla más importante del sistema: la irreversibilidad
del eco es el tema del juego, no una limitación técnica.

---

## 6 — GRAMÁTICA DE GAMEPLAY

### La prueba del botón

> ¿Puede este puzzle resolverse exactamente igual con una caja empujable
> genérica en lugar del eco?

Si la respuesta es sí, el puzzle no pertenece a este juego. Rediseñar.

### La regla de una idea (Portal Clarity Rule)

> ¿Puede describirse lo que este nivel enseña en una sola frase?

Si la descripción requiere más de una frase (dos mecánicas nuevas,
dos variaciones simultáneas), el nivel enseña demasiado. Dividir.

### Estructura obligatoria por capítulo

Grupos de niveles organizados en tríadas:

1. Introducir la idea sola, sin ruido.
2. Variarla sola — misma idea, contexto distinto.
3. Combinarla con la idea del capítulo anterior — nunca con dos ideas nuevas.

### La regla de ejecución física

Todo puzzle debe tener una fase donde el jugador actúe físicamente en el
espacio. Si el jugador puede resolver un puzzle sin moverse después de
grabar, el puzzle es demasiado pasivo.

### La regla del fracaso legible

El fracaso de un puzzle debe comunicarse visualmente, sin texto. Si
el fracaso requiere un mensaje de error para ser entendido, el diseño
del espacio falló.

### La regla de nombre vs. comportamiento

Renombrar una clase sin cambiar qué hace mecánicamente no es rediseño.
Es maquillaje. Un nombre nuevo sin comportamiento nuevo no es progreso.

---

## 7 — ESTRUCTURA DE CAMPAÑA — LOS SEIS CAPÍTULOS

### CAPÍTULO I — Persistencia
*"¿Qué ocurre cuando mi pasado permanece?"*

El jugador aprende que el eco existe en el mundo como cuerpo físico.
Puzzles deliberadamente simples — el objetivo es construir vocabulario.
Mecánicas permitidas: eco básico, plataformas, timing vertical simple.
Mecánicas prohibidas: consecuencias negativas, exploración múltiple, grabación limitada.

### CAPÍTULO II — Coordinación
*"¿Cómo trabajo con una versión anterior de mí?"*

El timing se vuelve la pregunta central. Plataformas en movimiento,
mecanismos con ritmo, timing vertical y horizontal.
Incluye: laberinto de timing con consecuencia de quedar atrapado.

### CAPÍTULO III — Confianza
*"¿Puedo confiar en algo que ya no puedo controlar?"*

El espacio tiene capas invisibles. Cosas que no existen de forma permanente.
El salto de fe es el momento central: cruzar un puente espectral que solo
existe durante la grabación, antes de verlo completamente.

### CAPÍTULO IV — Optimización
*"Si solo puedo conservar parte del pasado, ¿qué elijo?"*

La duración de grabación es un recurso escaso. El jugador decide qué parte
de su acción pasada es más valiosa conservar. Conecta directamente con el
tema narrativo: no todo puede quedar en el eco.

### CAPÍTULO V — Consecuencia
*"¿Qué ocurre cuando mi pasado me perjudica?"*

El eco deja de ser siempre aliado. Plataformas letales activadas por el
eco, zonas de bloqueo, errores con peso real.

### CAPÍTULO VI — Aceptación
*Ninguna pregunta nueva — solo nuevas combinaciones.*

No hay mecánicas nuevas. El jugador usa todo lo anterior en sus formas
más elegantes. La solución del nivel final es la más limpia del juego,
no la más compleja.

---

## 8 — DISEÑO DE NIVELES — REGLAS DE ESPACIO

### Estructura obligatoria de cada nivel

```
ENTRADA / OBSERVACIÓN
El jugador lee el espacio sin presión.

ZONA DE APRENDIZAJE (exploración libre)
Micro-espacio donde experimenta la mecánica sin consecuencias permanentes.
Aquí se enseña sin explicar.

NÚCLEO DEL PUZZLE
El desafío real.

ZONA DE EJECUCIÓN
El momento físico: cruzar, saltar, llegar.

SALIDA / RESPIRACIÓN
Espacio breve post-solución donde el jugador digiere lo que entendió.
```

### Lo que el espacio comunica antes que el puzzle

1. **Escala**: ¿cuán grande es el problema?
2. **Jerarquía**: ¿qué es lo más importante?
3. **Ritmo**: ¿hay algo que se mueve? ¿A qué velocidad?
4. **Peligro**: ¿qué puede matar o bloquear?

Si el jugador necesita texto para entender alguno de estos cuatro puntos,
el layout del nivel está mal.

### La cámara como parte del diseño

| Espacio | Offset | FOV |
|---|---|---|
| Pasillo | `(-3f, 5f, -7f)` | 50 — compresión deliberada |
| Aula | `(-6f, 6f, -9f)` | 58 — abarca filas sin distorsión |
| Biblioteca | `(-2f, 7f, -5f)` | 45 — vertical, presión de estanterías |
| Patio (Nivel 9, único uso) | `(-10f, 9f, -15f)` | 72 — apertura máxima |
| Aula de Lyra (Niveles 10, 13) | `(-5f, 6f, -10f)` | 62 — ve el eco ambiental a distancia |
| Inversión (Nivel 14) | `(0f, 5f, -16f)` | 55 — frontal, eje de simetría |

Si la geometría bloquea la cámara, el layout está mal — no la cámara.

---

## 9 — NARRATIVA — REGLAS DE INTEGRACIÓN

### La prueba sin texto

Eliminar mentalmente todo el texto de un momento narrativo. Si el jugador
no entiende algo sobre Aiden o Lyra solo con lo que vio o hizo, ese momento
depende del texto y debe rediseñarse como espacio o mecánica.

### Lyra — tres formas de presencia, solo tres

1. **Arquitectura**: espacios que tienen su escala, su temperatura visual.
2. **Ecos ambientales no interactivos**: aparecen, hacen algo simple, desaparecen. No responden. Son presencia, no mecánica.
3. **Fragmentos de voz**: nunca completos. El jugador nunca escucha la conversación entera.

Lyra nunca es un eco controlable.

### La voz grabada — tres momentos en toda la campaña

- **Nivel 5 (Culpa):** la voz es de Aiden. Un fragmento que no quería escuchar.
- **Nivel 10 (Recuerdos):** la voz es de Lyra. Incompleta.
- **Nivel 13 (Verdad):** la grabación de la conversación. No toda.

Fuera de estos tres, el eco es silencioso.

---

## 10 — DIRECCIÓN ARTÍSTICA — DECISIÓN CERRADA

> Esta sección no se reabre sin autorización explícita del desarrollador.
> Si un análisis externo recomienda brutalismo monumental, ciencia ficción,
> o estética Control/NaissanceE — ese análisis no conoce esta decisión.
> Ignorarlo en este punto o escalar a revisión humana antes de actuar.

### La dirección: Escuela liminal PS1

El mundo de Aiden es su escuela, recordada de forma imperfecta. La
arquitectura empieza siendo reconocible y se deforma progresivamente.

Regla de progresión visual gratuita: cuanto más cerca está el jugador
de la verdad, menos estable es la arquitectura.

**Referencias válidas:**
Silent Hill PS1, LSD Dream Emulator, Yume Nikki, Mizzurna Falls,
Persona 1, Paratopic, Crow Country, Fear the Spotlight.

**Referencias prohibidas como base:**
Control, NaissanceE, brutalismo monumental, monolitos, sci-fi.
Descartadas por costo de producción inalcanzable y disonancia narrativa.

### Paleta de tokens

| Token | Hex | Rol |
|---|---|---|
| `void-black` | `#0A0A0D` | Fondo absoluto. Solo Nivel 14. |
| `corridor-navy` | `#1C2430` | Base de suelos y fondos. |
| `fluorescent-sick` | `#C9D4B0` | Luz fluorescente de techo. |
| `memory-amber` | `#E8B262` | Objetos narrativos. Espacios de Lyra. |
| `echo-cyan` | `#4FC3E8` | El eco. Siempre. Sin excepción. |
| `wrongness-red` | `#B23A3A` | Peligro. Uso escaso. Nunca decorativo. |
| `institutional-teal` | `#2B4A4A` | Paredes de pasillo. |
| `faded-mustard` | `#5A4A2E` | Paredes de aula. |
| `sage-green` | `#3A4A38` | Paredes de biblioteca. |
| `dusty-rose` | `#4A3438` | Espacios de Lyra (Niveles 10 y 13). |

`memory-amber` solo aparece en objetos que el jugador debe notar
narrativamente. Si se usa en todo, deja de significar nada.

### Materiales

- Planos. `Metallic = 0`. `Glossiness ≈ 0.05`.
- Sin texturas PBR 2K/4K de concreto o metal realista.
- Sin reflejos de entorno (`reflectionIntensity = 0`).
- `AmbientMode.Flat`, no Trilight.

### Iluminación

- Una sola fuente de luz dominante por espacio.
- Falloff agresivo. Charcos de luz, no resplandor ambiental.
- La niebla corta la visibilidad a 15-20 metros. Densidad entre 0.012 y 0.04.
- `LightShadows.Hard`. Intensidad direccional baja (0.4).

### Atmósfera por nivel

| # | Nivel | fogColor | fogDensity | ambientColor |
|---|---|---|---|---|
| 1 | Desorientación | `1C2430` | 0.016 | `262C36` |
| 2 | Repetición | `1A2028` | 0.020 | `20242C` |
| 3 | Indecisión | `1C232E` | 0.022 | `242A34` |
| 4 | Espera | `201C22` | 0.020 | `282430` |
| 5 | Culpa | `141820` | 0.032 | `1A1E26` |
| 6 | Negación | `161C18` | 0.030 | `1E241C` |
| 7 | Evasión | `141A24` | 0.018 | `1C2230` |
| 8 | Autosabotaje | `1C1E26` | 0.026 | `22242C` |
| 9 | Control | `2A3038` | 0.013 | `363C44` |
| 10 | Recuerdos | `201E22` | 0.020 | `2C241E` |
| 11 | Conexión | `1E222C` | 0.024 | `262A34` |
| 12 | Conflicto | `181A22` | 0.028 | `1E2028` |
| 13 | Verdad | `1C1A20` | 0.022 | `241E22` |
| 14 | Aceptación | `0A0A0D` | 0.014 | `12121A` |
| 15 | Integración | `1C2430` | 0.015 | `30342C` |

---

## 11 — PROGRESIÓN DETALLADA DE LOS 15 NIVELES

Esta sección es la especificación de diseño de cada nivel. Cada nivel tiene
una idea única, un espacio, una mecánica introducida, y un momento central.
Si un nivel empieza a acumular más de una idea, se divide — nunca se fusionan.

---

### NIVEL 1 — Desorientación
**Capítulo I — Persistencia**
**Espacio:** Entrada de la escuela + pasillo que se repite sin terminar.
**Mecánica introducida:** Ninguna. `maxEchos = 0`.

El jugador navega solo. El pasillo A es idéntico al pasillo B. La repetición
espacial es el diseño, no un error. Al final del nivel, el eco de Aiden
aparece por primera vez sin ser convocado: cruza un umbral y desaparece.
Aiden no lo persiguió. Llegó tarde. Como siempre.

**Emoción:** confusión suave, reconocimiento del espacio, primera grieta.
**Tiempo estimado:** 8-12 minutos. No puede frustrarse.

---

### NIVEL 2 — Repetición
**Capítulo I — Persistencia**
**Espacio:** Pasillo de aulas idénticas.
**Mecánica introducida:** Eco básico. Duración máxima disponible: 8 segundos.

Tres puzzles en escalera deliberada:

1. Una placa que el eco sostiene mientras Aiden cruza. Solución obvia.
   Enseña: el eco puede estar donde yo no estoy.
2. Dos placas simultáneas. El eco activa una, Aiden la otra.
   Enseña: sincronización espacial.
3. Una placa temporizada: el eco debe pisarla en un momento específico
   de su recorrido. Enseña: la grabación tiene tiempo, no solo posición.

El tercer puzzle es el primero donde el momento de inicio de la grabación
importa tanto como lo que se graba.

**Emoción:** primer contacto con el vocabulario del sistema.

---

### NIVEL 3 — Indecisión
**Capítulo I — Persistencia**
**Espacio:** Bifurcación de dos pasillos paralelos y visibles simultáneamente.
**Mecánica introducida:** El eco toma un camino mientras Aiden toma el otro.

Los dos pasillos son especulares. La cámara está diseñada para mostrar
ambos a la vez — el jugador ve lo que está perdiendo mientras elige.

Un pasillo tiene objetos de Aiden. El otro tiene objetos de Lyra. El juego
no lo dice. El jugador lo nota.

Los puzzles requieren presencia simultánea en ambos lados. El eco sostiene
el lado de Lyra mientras Aiden recorre el suyo, o al revés.

**Emoción:** la indecisión tiene costo — pero hay una forma de estar en
dos lugares a la vez.

---

### NIVEL 4 — Espera *(Timing vertical)*
**Capítulo II — Coordinación**
**Espacio:** Aula con desnivel interno. Plataformas apiladas.
**Mecánica introducida:** Timing de anticipación vertical. Latencia de 0.8s como variable de diseño.

Plataformas que suben y bajan con ritmo fijo. El jugador debe observar el
ciclo completo antes de grabar. La grabación empieza en el momento correcto
del ciclo, no en cualquier momento.

**El error no es perder tiempo:** es quedar abajo sin ángulo de recuperación.

**Mecánica clave:** el jugador aprende que el momento de inicio de la
grabación importa tanto como lo que graba. Grabar en el momento equivocado
es grabar mal.

**Zona de aprendizaje previa:** una sala pequeña donde el jugador puede
ver el ritmo de una plataforma sin consecuencias antes del puzzle real.

**Emoción:** la espera calibrada como acto activo, no pasivo.

---

### NIVEL 5 — Culpa *(Timing horizontal — laberinto)*
**Capítulo II — Coordinación**
**Espacio:** Pasillo de mantenimiento ramificado. Pipe Kit como detalle visual.
**Mecánica introducida:** Timing horizontal. Quedar atrapado como consecuencia real.

Pasillos que se cierran con ritmo fijo. El jugador debe grabar una ruta
que atraviese el laberinto en el orden correcto. Si dobla tarde, queda
atrapado en una rama sin salida. El nivel castiga la impulsividad.

**Estructura de enseñanza:**
La zona de exploración previa (sin riesgo) permite aprender el ritmo de
los mecanismos. El jugador entiende la regla antes de que cueste aplicarla.
Después viene el intento real.

**La voz grabada — primera aparición:** en el puzzle central, el jugador
puede grabar una frase. Cuando el eco la reproduce, Aiden escucha su
propia voz diciendo algo que no quería escuchar. No se explica qué es.

**El nivel termina:** con Aiden frente a su eco reproduciendo una acción
en bucle. El eco no lo ve. Aiden lo mira. La cámara no corta.

**Emoción:** el pasado como obstáculo. La primera vez que el eco no es
solo una herramienta.

---

### NIVEL 6 — Negación *(Salto de fe — puente espectral)*
**Capítulo III — Confianza**
**Espacio:** Biblioteca. Pasillo de estanterías altas. Referencia directa
a la imagen de Clock Tower que definió la dirección de arte.
**Mecánica introducida:** Geometría temporal. Cosas que solo existen durante
la grabación o reproducción.

El puente no está visible de forma permanente. Durante la grabación o
reproducción del eco, el puente espectral se materializa. Fuera de ese
tiempo, el abismo es real.

**El salto de fe:** el jugador debe cruzar el puente mientras su eco lo
activa — lo que significa grabar el cruce antes de ver el puente completo.
No puede activar el eco y grabarse cruzando al mismo tiempo. Debe grabar
el cruce primero, confiando en que el eco revelará el puente en el momento
correcto.

**Zona de aprendizaje previa:** un abismo pequeño (no letal) donde el
puente espectral es visible sin consecuencias. El jugador descubre que
"durante la grabación, el puente existe". Después viene el abismo real.

**La degradación del eco:** en este nivel se introduce visualmente que el
eco se vuelve más translúcido con cada reproducción adicional. Un eco
usado demasiadas veces en el mismo puzzle pierde fidelidad — pequeñas
desincronizaciones de 0.1-0.3 segundos. Esto no es aleatoriedad: es
determinismo que el jugador puede aprender a contabilizar.

**Emoción:** confiar en algo que ya no controlas. Primera vez que el
jugador siente que el eco sabe algo que él no sabe.

---

### NIVEL 7 — Evasión *(Grabación anticipada)*
**Capítulo III — Confianza**
**Espacio:** Corredor de emergencia + patio trasero parcial.
**Mecánica introducida:** Grabar el futuro, no el presente.

Un elemento existe solo por 5 segundos (una plataforma, una ventana de
paso). No es posible activarlo y grabarse cruzándolo al mismo tiempo.

**La solución:** grabar el cruce primero. Activar el elemento después.
Cuando el eco se reproduce, la condición ya existe y el cruce ocurre.

**Si el jugador intenta la solución obvia** (activar primero, grabar
después), el timing nunca funciona. El fracaso visible enseña el problema.
El jugador infiere la solución sin texto.

**Descubrimiento central:** "No grabo lo que hice. Grabo lo que voy a
necesitar que mi pasado haya hecho." Este es el mayor salto conceptual
del sistema de todo el juego.

**Emoción:** la causalidad invertida. El pasado sirve al futuro.

---

### NIVEL 8 — Autosabotaje *(Dos ecos — orden de activación)*
**Capítulo II — Coordinación avanzada**
**Espacio:** Sala de profesores.
**Mecánica introducida:** Dos ecos simultáneos. El orden importa más que la acción.

El nivel no introduce los dos ecos directamente. Primero hay puzzles con
un eco. A mitad del nivel, el segundo eco se vuelve disponible sin
explicación. El jugador intenta usar ambos con la lógica del primero y falla.

**El puzzle central:** Eco A debe terminar su acción antes de que Eco B
empiece. Si Eco B actúa primero, destruye la condición que Eco A necesita.
El jugador aprende a grabar los primeros segundos de Eco B sin hacer nada
— solo esperando. Un eco que espera deliberadamente.

**Descubrimiento:** "El tiempo muerto en una grabación es una decisión,
no un error."

Los dos ecos representan dos decisiones que Aiden tomó en el mismo momento.
El nivel no lo dice. Lo muestra en la arquitectura.

**Emoción:** dos versiones de sí mismo que no cooperan automáticamente.
El autosabotaje no lo produce el juego — lo produce el jugador.

---

### NIVEL 9 — Control *(Patio exterior — el único respiro)*
**Capítulo III — Confianza avanzada**
**Espacio:** Patio exterior central. Uso único en toda la campaña.
**Mecánica introducida:** El eco como cronómetro de precisión.

Después de ocho niveles de interiores opresivos, este espacio se abre.
Sin techo. Niebla mínima (fogDensity 0.013). El jugador respira por
primera vez. Este contraste es el diseño.

El jugador usa al eco no para hacer algo directamente, sino para saber
cuándo cruzar. El eco activa un mecanismo que pausa un peligro durante
exactamente N segundos. El jugador debe calcular cuándo activar el eco
para tener la ventana exacta que necesita.

**Descubrimiento:** "El eco no me ayuda a cruzar. Me dice cuándo cruzar."

Los puzzles son los más complejos hasta ahora en papel. Pero la experiencia
es de dominio, no de frustración. El jugador llega con todo el vocabulario
aprendido y los puzzles responden a eso.

**Emoción:** el primer destello de claridad después del fondo emocional.
Aiden empieza a entender que puede elegir qué recordar y cómo usarlo.

---

### NIVEL 10 — Recuerdos *(Eco ambiental de Lyra)*
**Capítulo IV — Optimización**
**Espacio:** Aula de Lyra. Material `WallRoseMat` (`#4A3438`).
**Mecánica introducida:** Eco ambiental no controlable. El jugador lee un eco que no es suyo.

El eco ambiental de Lyra sigue un recorrido fijo que el jugador no controla.
Cuando Lyra pasa cerca de una plataforma invisible, esa plataforma se vuelve
visible en un radio de 2 metros. El jugador debe seguir a Lyra para ver el
camino — no para imitarla, sino para usar la visibilidad que ella genera.

El eco de Lyra no se detiene. Si el jugador se queda atrás, las plataformas
se oscurecen de nuevo. Eso es persecución. Eso es físico.

**La segunda vuelta:** cuando el eco de Lyra completa su recorrido y
desaparece, Aiden ha memorizado parcialmente las posiciones. El segundo
intento prueba esa memoria.

**La voz grabada — segunda aparición:** Aiden escucha una grabación que
no hizo él. Es la voz de Lyra. Incompleta. El jugador la escucha y tiene
que usarla para resolver el puzzle. El juego no la completa.

**Por qué esto introduce a Lyra sin una sola línea de diálogo:** el
jugador necesita a Lyra para ver. Sin ella, el camino es invisible.
Ella no está explicada — es necesaria.

**Emoción:** necesitar el punto de vista de alguien que ya no está.

---

### NIVEL 11 — Conexión *(Grabación limitada — escalera)*
**Capítulo IV — Optimización**
**Espacio:** Escalera central con descanso intermedio.
**Mecánica introducida:** Grabación limitada. La decisión de qué gravar importa.

El recorrido de la escalera es más largo que el tiempo de grabación
disponible. El jugador no puede grabar todo.

**La decisión:** el eco puede "sostener" el descanso intermedio mientras
Aiden sube el segundo tramo — pero solo si el jugador grabó posicionarse
en el descanso, no el tramo entero. Elegir mal significa que el eco
termina en el lugar incorrecto y la sincronización falla.

**Descubrimiento:** "No todo puede quedar en el eco."

**Emoción:** primera decisión de prioridad dentro de la grabación.
Conecta directamente con el tema: Aiden no pudo conservar todo.

---

### NIVEL 12 — Conflicto *(Dos ecos contradictorios)*
**Capítulo V — Consecuencia**
**Espacio:** Gimnasio o laboratorio. Espacio más alto que los anteriores.
**Mecánica introducida:** Dos ecos con objetivos contradictorios. Elegir qué versión del pasado traer.

Los dos ecos disponibles no pueden coexistir en este nivel. Si usas ambos,
se cancelan. El jugador debe elegir cuál eco predomina — y esa elección
define cuál parte del nivel puede completarse.

**Sin solución correcta:** hay dos soluciones con costes distintos.
El nivel prepara al jugador emocionalmente para escuchar algo que no
quiere escuchar en el Nivel 13.

**Emoción:** resolver el conflicto no es encontrar la solución perfecta.
Es decidir qué costo estás dispuesto a pagar.

---

### NIVEL 13 — Verdad *(Grabación única no elegida)*
**Capítulo V — Consecuencia**
**Espacio:** Aula de Lyra, versión fragmentada. `LiminalVariant` del módulo de aula.
**Mecánica introducida:** Una grabación que Aiden no hizo conscientemente. No puede regrabar sobre ella.

Al inicio del nivel hay una grabación activa que el jugador no realizó.
Se reproduce sola. El nivel exige que el jugador use esa grabación — y no
puede regrabar sobre ella. Debe trabajar con lo que hay.

**La voz grabada — tercera y última aparición:** la grabación tiene voz.
Es la conversación con Lyra. No completa. El jugador escucha lo que Aiden
dijo. No lo que no dijo — lo que dijo cuando habló. La verdad no es que
Aiden no habló. La verdad es lo que dijo cuando habló.

**El nivel es más corto que los anteriores.** La densidad es emocional,
no mecánica. El momento más difícil del juego en términos emocionales.

**Emoción:** la verdad sin posibilidad de reeditar.

---

### NIVEL 14 — Aceptación *(Inversión — Aiden sigue al eco)*
**Capítulo VI — Aceptación**
**Espacio:** Fragmentos de escuela flotando en `void-black`. Sin pasillo que los conecte.
**Mecánica introducida:** El eco lleva el ritmo. Aiden lo sigue. `maxEchos = 0`.

El eco ya tiene su secuencia al entrar al nivel. Se reproduce solo.
Aiden debe imitarlo en espejo, en tiempo real, sincronizadamente.

**Dos zonas especulares:** eco en el lado izquierdo, Aiden en el derecho.
Un interruptor a cada lado. Deben activarse simultáneamente. La cámara
está centrada en el eje de simetría — la única cámara del juego que
mira directamente de frente al jugador.

**Sin grabación.** El jugador que llega buscando grabar descubre que
no puede. El nivel lo pone en el rol que siempre tuvo el eco.

**Emoción:** Aiden deja de intentar controlar. Sigue. El gameplay es la
narrativa. Ninguna cinemática puede hacer esto con la misma precisión.

---

### NIVEL 15 — Integración *(El círculo se cierra)*
**Capítulo VI — Aceptación**
**Espacio:** El pasillo de entrada del Nivel 1. Ahora termina en una puerta real.
**Mecánica introducida:** Ninguna. Todo el sistema en su forma más limpia.

Tres puzzles. Cada uno usa un subconjunto distinto del sistema. El puzzle
final usa el eco de la manera más básica posible — como en el Nivel 2 —
pero en un contexto que lo hace completamente distinto. El jugador que
llegó aquí entiende por qué es distinto. El jugador del Nivel 2 no podría.

**El momento final:** Aiden usa un eco. El eco hace algo. Aiden lo deja
ir. El eco desaparece — no con animación dramática, sino porque la
grabación terminó. Aiden sigue. La puerta está abierta.

**Sin cinemática final.** El jugador camina hacia la salida. La cámara
no corta. Los créditos empiezan sobre el espacio, no sobre pantalla negra.

**Emoción:** nada nuevo. Todo lo que aprendió, usado una vez más, con
el peso de haberlo aprendido.

---

## 12 — ASSETS PERMITIDOS Y PROHIBIDOS

| Pack | Uso correcto | Uso prohibido |
|---|---|---|
| Architecture Pack 001 | Estructura base: paredes, pasillos, techos, columnas | Piezas que se vean industriales o sci-fi |
| Kenney Furniture Kit | Interior: escritorios, sillas, estantes | Decoración exterior |
| Pipe Kit | Quiebre visual en Nivel 5 (mantenimiento) | Base arquitectónica de cualquier nivel |
| Air Duct Kit | Detalle técnico en Nivel 5 | Arquitectura base |
| City Pack | Elementos exteriores del patio — Nivel 9 únicamente | Ciudad abierta, escenario base |
| Modular SciFi MegaKit | **Ninguno. Prohibido completamente.** | Todo |
| Cyberpunk Kit | **Ninguno. Prohibido completamente.** | Todo |
| Stylized Nature Megakit | Un árbol o roca aislada en patio, máximo | Base de cualquier espacio |

---

## 13 — ARQUITECTURA DE CÓDIGO — REGLAS

1. **Un solo builder activo.** Exactamente uno puede escribir escenas.
2. **Sin regeneración silenciosa.** Ningún proceso puede regenerar escenas
   sin confirmación explícita del desarrollador.
3. **`SetupAtmosphere` usa los parámetros que recibe.** Sin techo artificial.
   Sin valores hardcodeados.
4. **Una cámara activa por nivel.** Cinemachine o ThirdPersonCamera, no ambas.
5. **Sin constantes `SciFi*` activas.** Ninguna ruta hardcodeada al
   Modular SciFi MegaKit en ningún script de producción.
6. **El renombrado no es rediseño.**

---

## 12 — LO QUE NUNCA DEBE OCURRIR

Esta sección existe porque todas estas cosas ya ocurrieron en el proyecto.

- Reconstruir 15 niveles antes de que 3 funcionen.
- Escribir una Biblia nueva porque la anterior no se ejecutó.
- Agregar shaders complejos antes de validar que un nivel se siente bien.
- Confundir una auditoría bien escrita con trabajo ejecutado.
- Usar un nombre narrativo para un sistema sin cambiar su comportamiento.
- Abrir decisiones ya tomadas (dirección visual, pivote a escuela) porque
  una nueva IA no conoce el historial del proyecto.
- Construir infraestructura de pipeline más compleja que los niveles que genera.

---

## 13 — CRITERIO FINAL

> Un solo nivel que haga que cualquiera que lo juegue entienda inmediatamente
> por qué Echoes of You no podría existir con ninguna otra mecánica que no
> sea la de los ecos. Cuando ese nivel exista y se juegue diez veces y siga
> siendo bueno en la número diez, el resto del proyecto tiene suelo firme.
> Antes de eso, no.
