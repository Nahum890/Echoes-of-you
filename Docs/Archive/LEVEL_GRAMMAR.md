# LEVEL GRAMMAR — Echoes of You 2.0

## Propósito
Este documento define las reglas obligatorias para diseñar niveles, puzzles y progresión. No describe historia ni arte detallado. Define la gramática del juego: qué puede aparecer, cómo combinarlo y qué nunca debe ocurrir.

---

## 1. Principios base

1. Cada nivel enseña una sola idea principal.
2. Cada nivel puede añadir una variación de una idea anterior, pero no dos ideas nuevas a la vez.
3. El jugador debe entender qué está pasando en los primeros 5 segundos.
4. La solución debe poder descubrirse con observación, no con texto.
5. Cada nivel debe tener un momento “aha” claro.
6. Ningún nivel debe depender de ensayo y error puro.

---

## 2. Estructura estándar de nivel

Todo nivel debe seguir esta secuencia, con variaciones según el capítulo:

- **Entrada**
- **Observación**
- **Aprendizaje**
- **Primer intento**
- **Corrección**
- **Dominio**
- **Salida**

Si un nivel no tiene una zona de observación o una salida clara, no está terminado.

---

## 3. Reglas espaciales

### Pasillos
- Ancho mínimo recomendado: 4 unidades.
- Si el nivel usa cámara cercana, el pasillo puede bajar a 3 unidades, pero no menos.
- Nunca llenar un pasillo con props hasta impedir lectura.

### Aulas
- Deben tener siempre un foco claro: pizarrón, tarima, ventana, puerta o mecanismo.
- Deben poder leerse como “aula” desde la primera vista.
- Nunca usar aulas vacías sin objeto de interés.

### Escaleras
- Nunca deben terminar contra una pared.
- Debe existir espacio libre arriba y abajo.
- Deben tener una lectura clara desde cámara.

### Bibliotecas
- Deben tener zonas de sombra y zonas de lectura.
- Nunca saturarlas de libros o props.
- Debe existir un pasillo principal visible.

### Patio
- Debe dar respiración visual.
- Debe tener un landmark claro.
- No usarlo como vacío sin intención.

### Gimnasio
- Debe sentirse como el espacio más alto y más abierto.
- Se reserva para momentos de tensión o síntesis.
- No usarlo en niveles tempranos salvo necesidad clara.

---

## 4. Gramática de mecánicas

### PressurePlate
- Nunca aparece sola.
- Siempre controla algo visible.
- Nunca activa más de 3 elementos.
- Nunca está escondida detrás de una puerta cerrada.

### DoorController
- Siempre debe tener una razón visible de apertura.
- Nunca debe ser la única respuesta de un nivel.
- Nunca debe abrirse sin que el jugador entienda por qué.

### TimedMovingPlatform
- Debe mostrar su recorrido.
- Debe enseñar el ritmo antes de exigir precisión.
- Nunca debe obligar a un salto ciego.

### GhostBridge
- Debe insinuarse visualmente antes de usarse.
- Debe existir una forma de descubrirlo sin explicación textual.
- No usarlo como sorpresa injusta.

### GravityZone
- Debe introducirse en un espacio de control bajo.
- Debe haber margen para experimentar.
- Nunca mezclarla por primera vez con otra mecánica compleja.

### Echo Recorder / Echo Playback
- El jugador debe ver primero una versión simple de la mecánica.
- Nunca introducir el eco en un entorno demasiado cargado.
- El eco debe reforzar el espacio, no reemplazarlo.

### PuzzleWire / PuzzleCondition
- Deben usarse para reglas visibles y legibles.
- El jugador debe poder inferir la lógica del sistema.
- Nunca utilizar cadenas de condiciones opacas sin feedback.

---

## 5. Regla de combinación

Combinaciones permitidas por fase:

- **Fase I:** una mecánica.
- **Fase II:** una mecánica + una variación.
- **Fase III:** dos mecánicas conocidas.
- **Fase IV:** dos mecánicas + restricción temporal.
- **Fase V:** combinación de síntesis, no de novedades.

Nunca introducir tres mecánicas nuevas simultáneamente.

---

## 6. Regla de cámara aplicada al nivel

- La cámara debe mostrar el objetivo.
- La cámara debe mostrar el riesgo.
- La cámara debe mostrar al eco si el eco es relevante.
- Si no puede ver el problema, el jugador no debe sentir que puede resolverlo.
- Si un puzzle necesita mover la cámara artificialmente para entenderse, el layout está mal.

---

## 7. Ritmo por tipo de nivel

### Tutorial / enseñanza
- 1 mecánica
- 1 objetivo
- 0 trampas
- 0 falsos caminos

### Examen
- 2 mecánicas conocidas
- 1 restricción nueva
- 1 error posible
- 1 momento aha

### Síntesis
- 2 a 3 mecánicas conocidas
- 1 espacio más grande
- 1 respiración
- 1 resolución clara

---

## 8. Qué no debe pasar nunca

- Un nivel sin salida visible.
- Un nivel sin objetivo visible.
- Un nivel con softlock por geometría.
- Un nivel que requiera adivinar.
- Un nivel que solo funcione con texto.
- Un nivel donde la solución sea invisible.
- Un nivel con más de una idea nueva principal.
- Un nivel que parezca una sala de prueba genérica.

---

## 9. Regla de validación rápida

Antes de aprobar un nivel, responde estas cuatro preguntas:

1. ¿Qué enseña?
2. ¿Cómo lo enseña?
3. ¿Qué ve el jugador en los primeros 5 segundos?
4. ¿Cómo evita el softlock?

Si una respuesta falla, el nivel no está listo.
