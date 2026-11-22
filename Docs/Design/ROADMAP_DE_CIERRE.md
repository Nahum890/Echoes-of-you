# Echoes of You · Roadmap de cierre

**Objetivo de entrega.** Una demo de Windows que se pueda jugar desde el menú hasta los créditos, cerrar y continuar al día siguiente. El alcance propuesto son los seis niveles habilitados. N07–N15 quedan en reserva hasta que haya una demo que otra persona pueda terminar.

## Punto de partida de la prueba del 20 de noviembre

| Tramo | Evidencia en Unity | Pendiente para darlo por jugable |
| --- | --- | --- |
| Menú y N01 | El renderer original dibujó el aula. Cancelar el nuevo expediente conservó el progreso. Un eco inmóvil sostuvo la placa mientras el personaje llegó a la salida. La elección cargó N02. | Repetir desde cero pulsando R y recorriendo el nivel con teclado. En la prueba se suministró una grabación al reproductor y se movió el controlador con colisiones. |
| N02 | Las dos placas admitieron ecos y el objetivo se activó al sostenerlas simultáneamente. | Verificar el salto y el recorrido posterior hasta la salida elevada. |
| N03 | El volumen de caída alcanzaba ambas metas. Al bajarlo, los contactos de prueba distinguieron la ruta izquierda y la derecha, incluida la recompensa exclusiva. | Recorrer físicamente los pasillos completos y comprobar la lectura de la bifurcación. |
| N04 | El contacto en orden con A, B y C activó la meta. | Comprobar el acceso a cada placa desde el punto de aparición, la penalización por orden incorrecto y la visibilidad desde la cámara. |
| N05 y N06 | En N05, colocar el eco delante de la cortina neutralizó la barrera y activó la meta. En N06, el volumen de caída quedó debajo del puente y el eco sostuvo la placa al retirarse el personaje. | Comprobar el impulso y aterrizaje en N05 y terminar el cruce de N06 hasta los créditos. |
| Narrativa | Una elección devolvió el movimiento. Inspeccionar dos veces un recuerdo concedió un único artefacto. Tras guardar, vaciar el estado y cargar, se recuperó el mismo puntaje. | Probar una partida larga con cierre del proceso y ambos expedientes desde el menú. |

## Hitos en orden de dependencia

| Hito | Trabajo | Criterio para cerrarlo |
| --- | --- | --- |
| 1. Primera partida | Probar el ejecutable en un equipo sin el proyecto. Cubrir menú, tutorial, grabación con R, pausa, muerte, reintento y elección de N01. | Un jugador nuevo deja un eco en la placa y llega a N02 desde el menú sin ayuda. Tras cerrar y continuar, aparece en el nivel correcto. |
| 2. Tres niveles completos | Recorrer N02 desde el suelo hasta la salida elevada. En N03 probar ambos pasillos en partidas separadas. Revisar qué ve el jugador al llegar a cada puerta y desde dónde encuentra la placa. | N02 exige las dos placas simultáneas. Las dos rutas de N03 llegan a N04. Solo la ruta derecha desbloquea el eco futuro y conserva la recompensa al recargar. |
| 3. Cierre del bloque | Recorrer N04 en ambos estados del eco futuro. En N05 verificar barrera, impulso y aterrizaje. En N06 comprobar qué pasa al soltar la placa antes y durante el cruce. Probar cada muerte y reintento. | Una partida continua llega a los créditos. Una elección distinta cambia el resultado previsto. Continuar, reiniciar y empezar un expediente nuevo producen tres resultados distintos y correctos. |
| 4. Lectura visual y dificultad | Empezar por las sillas que tapan N04 y el contraste de la barrera de N05. Hacer una ronda con tres personas que desconozcan los puzzles. Anotar sala, objetivo que entendieron, intentos, abandono y ayuda recibida. | Al menos dos de tres completan el primer bloque de tres niveles sin indicaciones externas. Una confusión repetida por dos personas vuelve a diseño antes de agregar contenido. |
| 5. Candidato a entrega | Generar una build limpia y repetir el recorrido en dos equipos Windows. Medir tiempos de carga y fotogramas en N04–N06. Ejecutar las pruebas de Unity y sustituir los controles de CI que invocan métodos inexistentes. | Conservar ejecutable, versión de Unity, resultados y defectos conocidos. Cero pantallas negras, bloqueos de progresión o pérdidas de partida reproducibles en esa build. |

## Decisiones de alcance

- Mantener la grabación y la repetición como núcleo. Cada nivel debe enseñar un uso que el jugador pueda describir después de resolverlo.
- Usar N03 como bifurcación, siguiendo la escena jugable y la especificación de las dos rutas. El pase de editor de la galería vertical describe otro diseño y volver a ejecutarlo desactiva piezas de esta versión.
- Evaluar ritmo y voz de Aiden con gente que juegue desde cero. Si la misma acción se explica en diálogo, tutorial y HUD, conservar la indicación que aparece cuando el jugador puede actuar.
- Si N04–N06 siguen bloqueando la entrega, la decisión de recorte es publicar tres niveles con cierre propio. Eso requiere adaptar el final y volver a probar la progresión.

Las pruebas instrumentadas en el editor sirven para localizar el mecanismo que falla. La aprobación de cada hito corresponde al recorrido con controles normales en el ejecutable.

## Comprobación de la exportación

En Windows, abrir el proyecto con su ruta completa. Una exportación de prueba desde una copia bajo una ruta abreviada de Windows generó un ejecutable que arrancaba con scripts desconectados. En esa copia, Unity reconoció la clase del jugador y pasó las comprobaciones en Play al usar la ruta larga. Antes de entregar, comprobar que el menú recibe entrada, que «Nuevo expediente» carga N01 y que se puede crear un eco en el ejecutable.
