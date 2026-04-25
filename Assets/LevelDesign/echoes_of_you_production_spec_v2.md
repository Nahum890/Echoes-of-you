# ECHOES OF YOU - Production Spec V2

Documento de produccion para game jam.
Este archivo no reemplaza el documento anterior; lo complementa con una version cerrada de 6 escenas y una linea de produccion mas clara.

## 1. Objetivo de produccion

Entregar un puzzle game 3D corto, pulido y totalmente entendible en el que el jugador graba acciones, genera ecos que repiten esas acciones y usa esos ecos para resolver niveles de navegacion y cooperacion con su propio pasado.

El proyecto debe demostrar al jurado cuatro cosas desde el minuto uno:

1. Funciona sin ambiguedad.
2. La mecanica tiene una logica interna solida.
3. La presentacion visual refuerza la mecanica.
4. El equipo entiende por que cada decision existe.

## 2. Pilares de diseno

1. Claridad antes que complejidad. Todo objetivo importante debe verse antes de ser usado.
2. Un sistema, muchas lecturas. El eco siempre hace lo mismo: repetir un registro exacto. Lo que cambia es la lectura espacial y temporal.
3. El escenario ensena solo. Los layouts hacen visible la relacion placa -> puerta -> meta.
4. La falla debe ser corta. Ningun error debe costar mas de 6 a 8 segundos de recuperacion.
5. La fantasia debe ser coherente. Un eco no es un clon de combate; es un recuerdo util.

## 3. Historia corta

El personaje despierta dentro del "Archivo", un espacio abstracto donde su identidad se guarda como decisiones repetidas. Nada esta roto por violencia; esta incompleto por olvido. Cada nivel es un fragmento de memoria: una accion, una correccion, una ruta que alguna vez eligio. Para llegar al nucleo del Archivo, el jugador no pelea ni destruye nada. Aprende a observar lo que hizo, a repetirlo con intencion y a usar su propio pasado para abrir el siguiente paso.

Contexto inicial en pantalla de inicio:

"Despiertas dentro del Archivo.
Cada eco conserva una decision.
Si entiendes lo que hiciste, puedes volver a ti."

Mensajes breves al cerrar cada nivel:

1. Nivel 1: "Primero recuerdas."
2. Nivel 2: "Luego pruebas otra posibilidad."
3. Nivel 3: "Dos decisiones pueden sostenerse entre si."
4. Nivel 4: "El orden cambia lo que se abre."
5. Nivel 5: "La precision revela el patron."
6. Final: "Eres la suma de lo que elegiste repetir, corregir y conservar."

## 4. Sistemas globales

### 4.1 Sistema de ecos

Estado recomendado para shipping jam:

- Duracion maxima de grabacion: `6.0 s`
- Duracion minima valida: `0.35 s`
- Maximo de ecos simultaneos: `2`
- Frecuencia de muestreo: `FixedUpdate` a `0.02 s`
- Tipo de eco: reproduccion en loop exacta de transform grabado
- Tag del eco: `Echo`
- Tag del jugador: `Player`

Uso de los scripts actuales:

- `EchoRecorder.cs`: ya resuelve grabacion y spawn.
- `EchoPlayback.cs`: ya resuelve replay por frames exactos.
- `PressurePlate.cs`: ya acepta `Player` y `Echo`.
- `DoorController.cs`: ya sirve para puertas que desaparecen al cumplirse la condicion.
- `TimedMovingPlatform.cs`: ya sirve para puentes que suben o bajan.

Parametros recomendados en `EchoRecorder`:

- `maxEchoes = 2`
- `maxRecordSeconds = 6`
- `minRecordSeconds = 0.35`

Reglas de diseño del eco:

1. El eco siempre nace en la primera posicion grabada.
2. El eco nunca empuja al jugador ni es empujado por el jugador.
3. El eco si activa placas y triggers.
4. El eco debe verse translucido, con silueta clara y trail corto.
5. Cuando se crea un tercer eco, el mas antiguo se disuelve primero. Como el maximo recomendado es 2, esto casi no aparece y evita confusion.

Como evitar desincronizacion:

1. Reproducir posiciones grabadas, no inputs.
2. Mantener todos los elementos dinamicos criticos en estados binarios simples: placa, puerta, puente.
3. Evitar fisica emergente en el eco. No usar rigidbodies no controlados sobre rutas criticas.
4. No exigir saltos precisos durante una grabacion si esa ruta luego dependera de otro eco.
5. Mantener todas las rutas grabadas bajo `24 m` de longitud util para que `6 s` sea suficiente con velocidad `6 m/s`.

Reinicio:

- `Q` o `LB`: limpiar todos los ecos y resetear objetos dinamicos del nivel sin recargar escena.
- `T` mantenido `0.5 s` o `Y` mantenido `0.5 s`: reiniciar escena completa.
- Si el jugador cae, reload de escena con fade blanco corto de `0.35 s`.

Como entiende el jugador que eco esta activo:

1. El ultimo eco creado tiene borde mas brillante durante `0.75 s`.
2. La UI muestra `Ecos 1/2` o `Ecos 2/2`.
3. Cada eco usa color menta con trail muy fino y pulso cada vez que reinicia el loop.

### 4.2 Camara global

Tipo:

- Tercera persona con seguimiento suave.
- Distancia fija con ajuste menor solo en eventos importantes.
- Rotacion limitada y dirigida por el layout.
- Enfoque permanente en el siguiente objetivo de puzzle.

Configuracion recomendada sobre `ThirdPersonCamera`:

- `focusOffset = (0, 1.40, 0)`
- `distance = 7.20`
- `followDamping = 9`
- `rotationDamping = 10`
- `mouseSensitivity = 1.15`
- `minPitch = 10`
- `maxPitch = 24`
- Campo visual por defecto: `58`
- Campo visual en grabacion: `54` durante `0.25 s`
- Campo visual en exito: `52` durante `0.35 s`

Reglas de direccion visual:

1. La salida del nivel debe ser visible desde el spawn o en los primeros 2 segundos.
2. Toda puerta o puente importante debe verse al mismo tiempo que la placa que lo controla, o conectarse por un haz de luz.
3. El jugador nunca debe necesitar girar 180 grados para entender el puzzle principal.
4. Las islas laterales deben verse en angulo de tres cuartos para mostrar profundidad.
5. Cuando una puerta se abre o un puente sube, la camara solo hace un micro paneo de `0.4 a 0.8 s`; nunca corta de forma agresiva.

Micro polish de camara:

- Shake pequeno solo al resolver un estado importante, nunca al caminar.
- Pulso de camara al iniciar y cerrar grabacion.
- Ligerisima anticipacion hacia la direccion de movimiento del jugador.
- Recentrado automatico suave cuando el jugador deja de mover la camara por `1.2 s`.

Requisito tecnico recomendado:

Agregar a `ThirdPersonCamera` un `authoredForward` por nivel y clamp de yaw de `+-25 grados` para que la lectura no se rompa. Si no entra en tiempo, dejar sensibilidad baja y recenter agresivo.

### 4.3 UI global

Jerarquia visual:

1. Objetivo actual.
2. Estado del eco.
3. Timeline de grabacion.
4. Prompt contextual.
5. Feedback breve de exito o error.

Disposicion recomendada:

- Arriba centro: `ObjectiveStrip`, ancho `420 px`, alto `44 px`
- Arriba izquierda: `EchoPanel`, ancho `300 px`, alto `76 px`
- Abajo centro: `InteractionPrompt`, ancho `320 px`, alto `42 px`
- Centro bajo: `FeedbackToast`, ancho `260 px`, alto `40 px`

Colores de estado:

- Listo: `#F1F6FA`
- Grabando: `#00D9FF`
- Reproduciendo: `#7AF0C8`
- Exito: `#FFD36B`
- Error suave: `#FF6D6D`
- Fondo UI: `rgba(4, 10, 14, 0.62)`

Elementos obligatorios:

1. Indicador de grabacion: chip "REC" con pulso cyan.
2. Linea de tiempo del eco: barra horizontal de 6 segmentos finos o barra continua de `0 a 6 s`.
3. Contador de ecos disponibles: `0/2`, `1/2`, `2/2`.
4. Objetivo actual del nivel: una sola frase.
5. Aviso de interaccion importante: aparece al entrar a un trigger tutorial.
6. Feedback de exito: "Eco creado", "Puerta abierta", "Secuencia completa".
7. Feedback de fracaso: "Secuencia incompleta", "Necesitas otra decision", "Reintenta el orden".

Reglas:

1. Nada tapa el centro de lectura del escenario.
2. Ningun texto permanece mas de `3.5 s` salvo el objetivo.
3. No usar tutoriales largos ni paneles de texto.
4. Toda informacion nueva entra con fade y sale con fade; nunca pop duro.

### 4.4 Input y feeling

Mapa de input teclado:

- `WASD`: mover
- `Space`: salto
- `R` mantener: grabar eco
- `R` soltar o llegar a `6 s`: crear eco
- `Q`: limpiar ecos y reset dinamicos
- `T` mantener `0.5 s`: reiniciar nivel
- `Esc`: pausa

Mapa gamepad:

- `Left Stick`: mover
- `A`: salto
- `RT` mantener: grabar eco
- `LB`: limpiar ecos
- `Y` mantener: reiniciar nivel
- `Start`: pausa

Parametros de control recomendados en `PlayerController`:

- `moveSpeed = 6`
- `sprintMultiplier = 1.0`
- `acceleration = 24`
- `deceleration = 28`
- `rotationSharpness = 14`
- `jumpHeight = 1.55`
- `gravityStrength = 26`
- `groundProbeRadius = 0.24`
- `groundProbeDistance = 0.38`
- `groundedStickForce = 5`

Mejoras tecnicas obligatorias para feeling:

1. Jump buffer: `0.15 s`
2. Coyote time: `0.12 s`
3. Que el jugador pueda saltar siempre con la misma altura minima si el input fue reconocido.
4. Sin sprint. El puzzle depende de distancias consistentes, no de velocidad variable.

Como se comunica cada input:

1. Movimiento: inclinacion leve del cuerpo, pasos suaves, trail corto en aceleracion.
2. Salto: compresion minima previa y particula pequena al aterrizar.
3. Inicio de grabacion: borde cyan en el personaje, sonido de carga, timeline llenandose.
4. Fin de grabacion: aparicion del eco con disolucion menta y chime corto.
5. Reset de ecos: todos los ecos se vuelven particulas y la UI vuelve a "Listo".

Como evitar frustracion:

1. Ningun salto obligatorio mayor a `2.2 m`.
2. Plataformas principales minimo `4 m` de ancho.
3. Todas las placas usan caja de `2 x 2 m` o mayor.
4. Todo puente critico tarda menos de `0.8 s` en activarse.
5. Toda solucion correcta deja al menos `2 s` de margen antes de que el jugador quede bloqueado.

### 4.5 Aesthetic y direccion artistica

Look:

- Vacio oscuro, niebla minima, plataformas grises azuladas, placas menta, grabacion cyan, salida dorada.
- Geometria limpia y recta.
- Nada ornamental que compita con la lectura.

Luz global:

- `DirectionalLight`: rotacion `(35, -25, 0)`, intensidad `1.10`, color `#D8E6F5`
- Luz de objetivo por nivel: un `PointLight` o `Area Light` suave en la meta
- Bloom bajo, no agresivo
- El eco debe reflejarse con material semi transparente y emisivo bajo

## 5. Convenciones de blockout

Reglas de construccion para todas las escenas:

1. Unidad de Unity = `1 m`.
2. Todas las plataformas criticas quedan con `Scale.y = 0.5`.
3. Todas las superficies jugables usan layer `Ground`.
4. El centro del spawn del jugador siempre es `Vector3(0, 1.10, 0)` salvo nota contraria.
5. La direccion principal de lectura es `+Z`.
6. Los vacios son decorativos pero peligrosos; no se usan para precision injusta.

Prefabs base:

- `Player`
- `EchoPrefab`
- `PressurePlate`
- `LevelExit`
- `TimedMovingPlatform`
- `GameHUD`
- `TutorialHUD`

Tamano base de objetos interactivos:

- Placa: `Scale (2.5, 0.20, 2.5)` — mas alta y ancha para visibilidad. El script `PressurePlate` crea automaticamente un `PointLight` encima con glow pulsante.
- Puerta disoluble frontal: `Scale (4.0, 3.5, 0.5)`
- Puerta disoluble lateral: `Scale (0.5, 3.5, 4.0)`
- Puente movil corto: `Scale (3.0, 0.5, 8.0)`
- Puente movil largo: `Scale (3.0, 0.5, 12.0)`
- Trigger de salida: `Scale (2.5, 2.5, 0.8)`
- Pasarela estandar: `Scale (3.0, 0.5, X)` donde X depende de la distancia. Ancho minimo 3m.

Secuencia recomendada de produccion:

1. Ajustar player, eco, HUD y camara una sola vez.
2. Blockout gris de los 6 niveles.
3. Probar todas las soluciones sin arte.
4. Agregar luces, materiales y efectos.
5. Agregar textos de objetivo y mensajes de cierre.

## 6. Nivel 1 - APRENDER - "Primer Rastro"

### Rol en la curva

Ensena movimiento basico, lectura de objetivo, relacion placa -> puerta y primer uso del eco. No introduce riesgo serio.

### Objetivo del nivel

Mantener una placa activa con un eco para cruzar una puerta y llegar a la salida.

### Narrativa breve

El Archivo recuerda una decision simple: "dejar algo de ti atras para seguir".

### Que ensena exactamente

1. El objetivo siempre esta delante.
2. Una placa puede abrir una puerta.
3. Si el jugador abandona la placa, la puerta deja de estar disponible.
4. Un eco puede quedarse haciendo una accion repetida para el jugador actual.

### Objetos necesarios

- 1 `Player`
- 1 `MainCamera`
- 1 `GameHUD`
- 1 `TutorialHUD`
- 6 plataformas estaticas
- 1 `PressurePlate_A`
- 1 `MemoryGate_A`
- 1 `LevelExit`
- 1 `DirectionalLight`
- 1 `GoalLight`
- 1 `OpeningShotAnchor`
- 1 `ObjectiveAnchor`

### Jerarquia recomendada en Unity

```text
Level_01_PrimerRastro
|- Lighting
|  |- DirectionalLight
|  |- GoalLight
|- Camera
|  |- MainCamera
|  |- OpeningShotAnchor
|  |- ObjectiveAnchor
|- UI
|  |- GameHUD
|  |- TutorialHUD
|- Environment
|  |- Platform_Start
|  |- Walk_A
|  |- Platform_Plate
|  |- Walk_B
|  |- Platform_Gate
|  |- Walk_C
|  |- Platform_Exit
|- Mechanics
|  |- PressurePlate_A
|  |- MemoryGate_A
|  |- LevelExit
|- Player
```

### Medidas, posiciones y escalas exactas

```text
Platform_Start      pos ( 0.0, 0.0,  0.0) scale ( 8.0, 0.5,  8.0)
Walk_A              pos ( 0.0, 0.0,  6.0) scale ( 3.0, 0.5,  4.0)
Platform_Plate      pos ( 0.0, 0.0, 11.0) scale ( 6.0, 0.5,  6.0)
Walk_B              pos ( 0.0, 0.0, 17.0) scale ( 3.0, 0.5,  4.0)
Platform_Gate       pos ( 0.0, 0.0, 22.0) scale ( 6.0, 0.5,  6.0)
Walk_C              pos ( 0.0, 0.0, 28.0) scale ( 3.0, 0.5,  6.0)
Platform_Exit       pos ( 0.0, 0.0, 34.0) scale ( 8.0, 0.5,  8.0)

PressurePlate_A     pos ( 0.0, 0.35, 11.0) scale ( 2.5, 0.20, 2.5) trigger true
MemoryGate_A        pos ( 0.0, 1.75, 25.0) scale ( 4.0, 3.50, 0.5) solid collider
LevelExit           pos ( 0.0, 1.25, 37.0) scale ( 2.5, 2.50, 0.8) trigger true

OpeningShotAnchor   pos ( 7.5, 5.8, -7.0) rot (18.0, -34.0, 0.0)
ObjectiveAnchor     pos ( 0.0, 1.8, 25.0)
GoalLight           pos ( 0.0, 3.0, 34.0) range 10 intensity 4 color #FFD36B
Player Spawn        pos ( 0.0, 1.10,  0.0)
```

### Orden exacto de colocacion

1. Colocar `Platform_Start`, `Walk_A`, `Platform_Plate`, `Walk_B`, `Platform_Gate`, `Platform_Exit` en linea recta sobre `+Z`.
2. Colocar `PressurePlate_A` exactamente en el centro de `Platform_Plate`.
3. Colocar `MemoryGate_A` entre `Walk_B` y `Platform_Gate`, centrada.
4. Colocar `LevelExit` al final de `Platform_Exit`.
5. Alinear la camara para que `MemoryGate_A` y `LevelExit` sean visibles desde el spawn.

### Ruta del jugador

1. Camina recto hasta la placa.
2. Ve que la puerta desaparece mientras esta encima.
3. Sale de la placa y ve que la puerta vuelve.
4. Vuelve a la placa.
5. Mantiene `R` durante `2.5 a 3.0 s`.
6. Cuando el eco queda en la placa, el jugador cruza la puerta.
7. Llega a la salida.

### Ruta del eco

Ruta recomendada de solucion:

- Inicio de grabacion en `(0.0, 0.36, 11.0)`
- Permanecer quieto sobre la placa `2.5 s`
- No necesita desplazamiento

### Como se ve desde la camara

- La camara arranca mostrando en el mismo encuadre la placa, la puerta y la salida lejana.
- El centro de pantalla inicial debe estar sobre `MemoryGate_A`.
- Al iniciar grabacion, un micro zoom acerca la accion un `7%`.
- Al crearse el eco, la camara no corta; solo recentra al jugador y deja la puerta visible al frente.

### Que ve el jugador antes de actuar

1. La salida dorada al fondo.
2. Una puerta translucida entre el jugador y la salida.
3. Una unica placa claramente conectada con la puerta por un haz de luz.

### Que siente al resolverlo

Entiende la fantasia base sin texto largo: "puedo dejar una version mia haciendo esto por mi".

### Momento wow

El instante en que el eco aparece, se queda en la placa y la puerta deja de ser un obstaculo personal para convertirse en un recuerdo util.

### Como evita frustracion

1. No hay saltos obligatorios.
2. Todo esta en un solo eje.
3. La solucion correcta se puede ejecutar en menos de `8 s`.
4. Si el jugador limpia ecos, vuelve a intentarlo desde el mismo lugar.

### Como aumenta dificultad sin confundir

Solo introduce una decision: grabar o no grabar. No mezcla timing ni dos objetivos.

### Camara cinematografica por nivel

- Posicion base de opening shot: `(7.5, 5.8, -7.0)`
- Angulo recomendado: `(18, -34, 0)`
- Centro de pantalla: `MemoryGate_A`
- Mover camara: solo al activarse la puerta, paneo corto `0.45 s` hacia la salida
- Zoom: al iniciar y terminar grabacion
- Quieta: mientras el jugador cruza la puerta

### UI por nivel

- Texto inicial arriba centro: "La salida necesita una decision sostenida." color `#F1F6FA`, aparece `0.8 s`, desaparece `3.0 s`
- Texto contextual abajo centro al salir por primera vez de la placa: "Mantener R graba un eco." color `#00D9FF`, aparece `1.2 s`, desaparece `3.2 s`
- Feedback centro bajo al crear eco: "Eco creado" color `#7AF0C8`, `1.2 s`
- Estado del panel de eco: `Listo` -> `Grabando` -> `Reproduciendo`

### Polish del nivel

- Sonido de interaccion: click limpio y corto al activar placa
- Sonido de grabacion: carga suave ascendente
- Sonido de eco: chime menta con reverb corta
- Efecto visual minimo: linea de luz entre placa y puerta, trail corto del eco
- Feedback de exito: puerta desaparece con particulas finas
- Feedback de error suave: si el jugador cruza sin eco, la puerta reaparece con un pulso rojo tenue

## 7. Nivel 2 - EXPERIMENTAR - "Camino Compartido"

### Rol en la curva

Mantiene la misma mecanica, pero cambia el resultado. El eco no abre una puerta; sostiene un puente.

### Objetivo del nivel

Usar un eco para mantener visible un puente y cruzar un vacio central.

### Narrativa breve

El Archivo recuerda que una misma decision puede abrir un camino distinto segun donde la dejes.

### Que ensena exactamente

1. El eco puede servir para habilitar espacio, no solo una puerta.
2. La mecanica es flexible.
3. El jugador puede separarse del eco y beneficiarse desde otro angulo.

### Objetos necesarios

- 1 `Player`
- 1 `MainCamera`
- 1 `GameHUD`
- 1 `TutorialHUD`
- 4 plataformas estaticas
- 1 pasarela lateral estatica
- 1 `PressurePlate_B`
- 1 `Bridge_B` con `TimedMovingPlatform`
- 1 `LevelExit`
- 1 `GoalLight`
- 1 `OpeningShotAnchor`
- 1 `ObjectiveAnchor`

### Jerarquia recomendada en Unity

```text
Level_02_CaminoCompartido
|- Lighting
|- Camera
|- UI
|- Environment
|  |- Platform_Start
|  |- Walk_Left
|  |- Platform_Plate
|  |- Platform_End
|- Mechanics
|  |- PressurePlate_B
|  |- BridgeAnchor_B
|  |  |- Bridge_B
|  |- LevelExit
|- Player
```

### Medidas, posiciones y escalas exactas

```text
Platform_Start      pos ( 0.0, 0.0,  0.0) scale (10.0, 0.5,  8.0)
Walk_Left           pos (-3.5, 0.0,  5.5) scale ( 3.0, 0.5,  5.0)
Platform_Plate      pos (-6.0, 0.0, 10.5) scale ( 5.0, 0.5,  5.0)
Platform_End        pos ( 0.0, 0.0, 19.0) scale (10.0, 0.5,  8.0)

PressurePlate_B     pos (-6.0, 0.35, 10.5) scale ( 2.5, 0.20, 2.5) trigger true
BridgeAnchor_B      pos ( 0.0, 0.0,  9.5)
Bridge_B inactive   local ( 0.0,-4.0, 0.0)
Bridge_B active     local ( 0.0, 0.0, 0.0)
Bridge_B scale      local scale ( 3.0, 0.5, 11.0)
LevelExit           pos ( 0.0, 1.25, 22.0) scale ( 2.5, 2.50, 0.8)

OpeningShotAnchor   pos ( 8.0, 6.2, -5.0) rot (20.0, -42.0, 0.0)
ObjectiveAnchor     pos ( 0.0, 1.8,  9.5)
GoalLight           pos ( 0.0, 3.0, 19.0) range 10 intensity 4 color #FFD36B
Player Spawn        pos ( 0.0, 1.10,  0.0)
```

### Orden exacto de colocacion

1. Colocar `Platform_Start` y `Platform_End` dejando un vacio central para el puente.
2. Colocar `Walk_Left` hacia la isla lateral.
3. Colocar `Platform_Plate` al final de la pasarela lateral.
4. Colocar `BridgeAnchor_B` centrado entre inicio y final.
5. Configurar `Bridge_B` con `TimedMovingPlatform` para subir `4 m` al activarse la placa.

### Ruta del jugador

1. Observa la salida al frente y el puente hundido en el vacio.
2. Ve a la izquierda una isla con una placa.
3. Pisa la placa y ve subir el puente.
4. Suelta la placa y ve bajar el puente.
5. Graba un eco sobre la placa durante `3.0 s`.
6. Regresa al inicio y cruza el puente.
7. Llega a la salida.

### Ruta del eco

- Inicio de grabacion en `(-6.0, 0.36, 11.0)`
- Permanecer sobre la placa `3.0 s`

### Como se ve desde la camara

- La vista inicial muestra el puente hundido en el centro y la isla lateral en el mismo cuadro.
- El centro de pantalla debe quedar sobre el hueco donde aparecera `Bridge_B`.
- Al activarse la placa por primera vez, la camara hace un paneo corto hacia el puente.

### Que ve el jugador antes de actuar

1. Salida visible.
2. Camino interrumpido por un vacio obvio.
3. Una unica alternativa lateral muy legible.

### Que siente al resolverlo

Que el eco no es solo un "peso en boton"; tambien puede convertirse en un constructor de camino.

### Momento wow

Cruzar un puente que el jugador no esta sosteniendo en el presente, sino su propia decision pasada.

### Como evita frustracion

1. La isla lateral esta cerca y no castiga explorar.
2. El puente sube rapido, en `0.8 s` o menos.
3. No hay que saltar mientras el puente se mueve.
4. La falla devuelve al inicio en menos de `5 s`.

### Como aumenta dificultad sin confundir

Cambia la forma del resultado, no la logica base.

### Camara cinematografica por nivel

- Posicion base de opening shot: `(8.0, 6.2, -5.0)`
- Angulo recomendado: `(20, -42, 0)`
- Centro de pantalla: `Bridge_B`
- Mover camara: al primer uso de `PressurePlate_B`
- Zoom: cuando el puente termina de subir por primera vez
- Quieta: mientras el jugador cruza el puente

### UI por nivel

- Texto inicial: "Un eco tambien puede sostener un camino." color `#F1F6FA`, `3.0 s`
- Texto contextual al ver bajar el puente: "Si lo dejas alli, el camino permanece." color `#00D9FF`, `2.8 s`
- Feedback de exito: "Puente estable" color `#FFD36B`, `1.0 s`

### Polish del nivel

- Sonido de interaccion: click mas metalico al activar placa
- Sonido de grabacion: igual a nivel 1
- Sonido de eco: igual a nivel 1
- Efecto visual minimo: el puente emerge con particulas lineales desde el vacio
- Feedback de exito: breve bloom sobre el puente
- Feedback de error suave: si el jugador cae, el tip de reset aparece una sola vez

## 8. Nivel 3 - REFORZAR - "Dos Decisiones"

### Rol en la curva

Consolida la mecanica con mayor complejidad espacial. No hay una mecanica nueva; hay dos usos del mismo eco.

### Objetivo del nivel

Mantener activas dos placas opuestas para abrir una puerta central.

### Narrativa breve

El Archivo recuerda que una sola decision no siempre alcanza; algunas rutas se sostienen en conjunto.

### Que ensena exactamente

1. El mismo sistema puede repetirse dos veces.
2. El jugador puede planificar una solucion acumulativa.
3. La puerta central es una consecuencia de dos decisiones laterales.

### Objetos necesarios

- 1 `Player`
- 1 `MainCamera`
- 1 `GameHUD`
- 1 `TutorialHUD`
- 5 plataformas estaticas
- 3 pasarelas estaticas
- 2 `PressurePlate`
- 1 `MemoryGate_C`
- 1 `LevelExit`
- 2 `ObjectiveBeam`

### Jerarquia recomendada en Unity

```text
Level_03_DosDecisiones
|- Lighting
|- Camera
|- UI
|- Environment
|  |- Platform_Start
|  |- Walk_Left
|  |- Platform_Left
|  |- Walk_Right
|  |- Platform_Right
|  |- Walk_Forward
|  |- Platform_Exit
|- Mechanics
|  |- PressurePlate_Left
|  |- PressurePlate_Right
|  |- MemoryGate_C
|  |- LevelExit
|- Player
```

### Medidas, posiciones y escalas exactas

```text
Platform_Start        pos ( 0.0, 0.0,  0.0) scale ( 8.0, 0.5,  8.0)
Walk_Left             pos (-4.0, 0.0,  4.5) scale ( 3.0, 0.5,  5.0)
Platform_Left         pos (-8.0, 0.0,  9.0) scale ( 5.0, 0.5,  5.0)
Walk_Right            pos ( 4.0, 0.0,  4.5) scale ( 3.0, 0.5,  5.0)
Platform_Right        pos ( 8.0, 0.0,  9.0) scale ( 5.0, 0.5,  5.0)
Walk_Forward          pos ( 0.0, 0.0,  9.0) scale ( 3.0, 0.5,  6.0)
Platform_Exit         pos ( 0.0, 0.0, 18.0) scale (10.0, 0.5,  8.0)

PressurePlate_Left    pos (-8.0, 0.35,  9.0) scale ( 2.5, 0.20, 2.5)
PressurePlate_Right   pos ( 8.0, 0.35,  9.0) scale ( 2.5, 0.20, 2.5)
MemoryGate_C          pos ( 0.0, 1.75, 13.0) scale ( 4.0, 3.50, 0.5)
LevelExit             pos ( 0.0, 1.25, 21.0) scale ( 2.5, 2.50, 0.8)

OpeningShotAnchor     pos ( 0.0, 7.0, -8.0) rot (24.0, 0.0, 0.0)
ObjectiveAnchor       pos ( 0.0, 1.8, 13.0)
GoalLight             pos ( 0.0, 3.0, 18.0) range 10 intensity 4 color #FFD36B
Player Spawn          pos ( 0.0, 1.10,  0.0)
```

### Orden exacto de colocacion

1. Construir un hub central.
2. Abrir dos ramales simetricos a izquierda y derecha.
3. Colocar una placa al final de cada ramal.
4. Colocar la puerta central al frente.
5. Dejar la salida claramente visible detras de la puerta.

### Ruta del jugador

1. Va al ramal izquierdo.
2. Graba un eco quieto sobre la placa izquierda por `2.5 s`.
3. Va al ramal derecho.
4. Graba un eco quieto sobre la placa derecha por `2.5 s`.
5. Regresa al centro.
6. Cruza la puerta abierta.
7. Sale.

### Ruta de los ecos

- Eco 1: quieto en `(-8.0, 0.36, 9.0)`
- Eco 2: quieto en `( 8.0, 0.36, 9.0)`

### Como se ve desde la camara

- La toma inicial muestra el hub central, ambos ramales y la puerta frontal.
- El centro de pantalla esta en `MemoryGate_C`, con ambos laterales visibles en la periferia.
- Cada vez que se crea un eco lateral, la camara hace un micro giro al lado opuesto para sugerir la segunda decision.

### Que ve el jugador antes de actuar

1. Dos placas iguales.
2. Una sola puerta que claramente requiere ambas.
3. Un problema mas amplio, pero todavia simple de leer.

### Que siente al resolverlo

Que ya domina la regla base y puede componerla.

### Momento wow

El momento en que ambos ecos quedan activos y la puerta central desaparece de inmediato, mostrando que el sistema acepta acumulacion.

### Como evita frustracion

1. Todo es simetrico.
2. La puerta usa lectura directa de AND.
3. No hay timing estricto; los ecos hacen loop.
4. Si el jugador crea un eco extra por error, el mas antiguo se borra y la UI lo muestra.

### Como aumenta dificultad sin confundir

Solo duplica el problema previo y lo hace espacialmente mas ancho.

### Camara cinematografica por nivel

- Posicion base de opening shot: `(0.0, 7.0, -8.0)`
- Angulo recomendado: `(24, 0, 0)`
- Centro de pantalla: `MemoryGate_C`
- Mover camara: paneo corto al lado opuesto cuando se activa la primera placa
- Zoom: solo al abrir la puerta central
- Quieta: al cruzar hacia la salida

### UI por nivel

- Texto inicial: "Una sola decision no basta." color `#F1F6FA`, `3.0 s`
- Texto contextual al crear el primer eco: "Todavia falta otra." color `#00D9FF`, `2.2 s`
- Feedback al crear el segundo eco: "Secuencia completa" color `#FFD36B`, `1.2 s`

### Polish del nivel

- Sonido de interaccion: dos tonos complementarios, uno por cada placa
- Sonido de grabacion: igual a niveles previos
- Sonido de eco: pulso ligeramente mas ancho por haber dos ecos
- Efecto visual minimo: dos haces laterales convergen en la puerta
- Feedback de exito: la puerta se disuelve con un flash muy corto en el centro
- Feedback de error suave: si solo una placa esta activa, el haz de la puerta queda a media intensidad

## 9. Nivel 4 - TWIST - "Orden de Lectura"

### Rol en la curva

Cambia la forma de pensar. El jugador ya no graba para abrir el camino que esta recorriendo; graba para que el eco recorra un camino y el presente aproveche otra consecuencia.

### Objetivo del nivel

Grabar una ruta que active una placa lejana y usar, durante la reproduccion, una puerta cercana que depende de esa placa.

### Narrativa breve

El Archivo recuerda que no solo importa lo que haces, sino cuando y en que orden lo haces.

### Que ensena exactamente

1. Una accion grabada puede servir para otro trayecto distinto.
2. El orden placa A -> placa B importa.
3. El jugador puede esperar y leer el tiempo del eco.

### Objetos necesarios

- 1 `Player`
- 1 `MainCamera`
- 1 `GameHUD`
- 1 `TutorialHUD`
- 6 plataformas estaticas
- 3 pasarelas estaticas
- 2 `PressurePlate`
- 2 `MemoryGate`
- 1 `LevelExit`
- 2 `ObjectiveAnchor`

### Jerarquia recomendada en Unity

```text
Level_04_OrdenDeLectura
|- Lighting
|- Camera
|- UI
|- Environment
|  |- Platform_Start
|  |- Walk_Left
|  |- Platform_A
|  |- Walk_A_Forward
|  |- Platform_B
|  |- Walk_Right
|  |- Platform_Exit
|- Mechanics
|  |- PressurePlate_A
|  |- PressurePlate_B
|  |- MemoryGate_A
|  |- MemoryGate_B
|  |- LevelExit
|- Player
```

### Medidas, posiciones y escalas exactas

```text
Platform_Start        pos ( 0.0, 0.0,  0.0) scale (10.0, 0.5,  8.0)
Walk_Left             pos (-4.0, 0.0,  5.0) scale ( 3.0, 0.5,  4.0)
Platform_A            pos (-8.0, 0.0, 10.0) scale ( 6.0, 0.5,  6.0)
Walk_A_Forward        pos (-6.0, 0.0, 15.0) scale ( 3.0, 0.5,  4.0)
Platform_B            pos ( 0.0, 0.0, 22.0) scale (10.0, 0.5,  8.0)
Walk_Right            pos ( 4.0, 0.0,  5.5) scale ( 3.0, 0.5,  5.0)
Platform_Exit         pos ( 8.0, 0.0, 12.0) scale ( 6.0, 0.5,  8.0)

PressurePlate_A       pos (-8.0, 0.35, 10.0) scale ( 2.5, 0.20, 2.5)
PressurePlate_B       pos ( 0.0, 0.35, 22.0) scale ( 2.5, 0.20, 2.5)
MemoryGate_A          pos (-4.0, 1.75, 17.0) scale ( 4.0, 3.50, 0.5)
MemoryGate_B          pos ( 5.5, 1.75,  8.0) scale ( 0.5, 3.50, 4.0)
LevelExit             pos ( 8.0, 1.25, 15.0) scale ( 2.5, 2.50, 0.8)

OpeningShotAnchor     pos ( 9.0, 6.5, -6.0) rot (19.0, -46.0, 0.0)
ObjectiveAnchor       pos ( 5.5, 1.8,  8.0)
GoalLight             pos ( 8.0, 3.0, 12.0) range 10 intensity 4 color #FFD36B
Player Spawn          pos ( 0.0, 1.10,  0.0)
```

### Relaciones exactas entre objetos

1. `PressurePlate_A` abre `MemoryGate_A`.
2. `PressurePlate_B` abre `MemoryGate_B`.
3. `MemoryGate_B` bloquea la ruta corta a la salida.
4. La unica forma de llegar a `PressurePlate_B` es atravesando `MemoryGate_A`.

### Orden exacto de colocacion

1. Construir el hub inicial.
2. Abrir un ramal izquierdo hacia `Platform_A`.
3. Desde `Platform_A`, construir el corredor hacia delante y colocar `MemoryGate_A`.
4. Construir `Platform_B` al frente.
5. Desde el hub, construir un ramal derecho corto hacia la salida y bloquearlo con `MemoryGate_B`.

### Ruta del jugador

Ruta de solucion correcta (2 ecos estaticos, ruta secuencial):

1. Va a `Platform_A`.
2. Graba eco 1 quieto sobre `PressurePlate_A` durante `2.5 s`.
3. Con `MemoryGate_A` abierta por eco 1, cruza hacia `Platform_B`.
4. Graba eco 2 quieto sobre `PressurePlate_B` durante `2.5 s`.
5. Con `MemoryGate_B` abierta por eco 2, vuelve al hub por `Walk_Right`.
6. Cruza `MemoryGate_B` hacia la salida.
7. Sale.

El twist: a diferencia del nivel 3 (paralelo), aqui la ruta es **secuencial** — eco 1 habilita el acceso a la zona donde se graba eco 2. El jugador NO sigue al eco; usa la consecuencia de cada eco desde otro angulo.

### Ruta de los ecos

- Eco 1: quieto en `(-8.0, 0.36, 10.0)` — mantiene `MemoryGate_A` abierta permanentemente
- Eco 2: quieto en `( 0.0, 0.36, 22.0)` — mantiene `MemoryGate_B` abierta permanentemente
- Eco 2 solo pudo ser grabado porque eco 1 ya abrio el acceso a `Platform_B`

### Como se ve desde la camara

- La toma inicial debe mostrar la puerta corta a la derecha y, en el fondo, el corredor largo hacia `PressurePlate_B`.
- El jugador entiende que existe una ruta larga y una ruta corta, pero la corta esta bloqueada.
- La camara debe resaltar primero `MemoryGate_B`, no `MemoryGate_A`, para que el twist tenga una pregunta clara.

### Que ve el jugador antes de actuar

1. Una salida cercana pero cerrada.
2. Un ramal izquierdo que parece mas largo.
3. Una segunda placa lejana al frente.

### Que siente al resolverlo

Un "aha" fuerte: el eco no abre el camino para que el jugador lo siga; abre otro camino en otro momento.

### Momento wow

Esperar en el hub, ver al eco correr por la ruta larga y aprovechar la apertura de la puerta corta.

### Como evita frustracion

1. Las dos puertas se leen desde el inicio.
2. La solucion correcta deja `2 s` de margen en la puerta derecha.
3. El jugador puede reintentar sin rehacer el blockout.
4. El texto contextual solo aparece despues de un intento fallido o al activar la segunda placa por primera vez.

### Como aumenta dificultad sin confundir

Introduce dependencia temporal, pero con solo dos puertas y dos placas.

### Camara cinematografica por nivel

- Posicion base de opening shot: `(9.0, 6.5, -6.0)`
- Angulo recomendado: `(19, -46, 0)`
- Centro de pantalla: `MemoryGate_B`
- Mover camara: paneo a `Platform_B` cuando el jugador pisa `PressurePlate_A` por primera vez
- Zoom: leve al llegar el eco a `PressurePlate_B`
- Quieta: mientras el jugador usa la ruta corta final

### UI por nivel

- Texto inicial: "El orden cambia lo que se abre." color `#F1F6FA`, `3.0 s`
- Texto contextual al activar `PressurePlate_B` por primera vez: "Ahora no sigas al eco. Aprovechalo." color `#00D9FF`, `2.8 s`
- Feedback de exito: "Ruta alternativa abierta" color `#FFD36B`, `1.0 s`

### Polish del nivel

- Sonido de interaccion: dos clicks en cadena, A mas grave, B mas agudo
- Sonido de grabacion: ligeramente mas tenso que niveles previos
- Sonido de eco: pulso audible al pasar por `PressurePlate_B`
- Efecto visual minimo: haz de luz desde `PressurePlate_B` hacia la puerta derecha
- Feedback de exito: la puerta derecha se disuelve con un pulso radial muy limpio
- Feedback de error suave: si el jugador corre por la ruta larga junto al eco, el HUD muestra "Lee el otro acceso"

## 10. Nivel 5 - EJECUCION - "Cadena Estable"

### Rol en la curva

Combina todo lo aprendido antes del final: dos ecos, dependencia entre ellos, lectura espacial clara y ejecucion limpia.

### Objetivo del nivel

Crear un eco que eleve un puente, crear un segundo eco que use ese puente para abrir la salida, y luego usar ambos estados para llegar a la meta.

### Narrativa breve

El Archivo recuerda que algunas decisiones solo existen porque otras ya estaban sosteniendo el espacio.

### Que ensena exactamente

1. Un eco puede crear las condiciones para grabar otro eco.
2. El jugador ya domina la planificacion secuencial.
3. El nivel es una cadena, no una lista de pasos aislados.

### Objetos necesarios

- 1 `Player`
- 1 `MainCamera`
- 1 `GameHUD`
- 1 `TutorialHUD`
- 5 plataformas estaticas
- 2 pasarelas estaticas
- 2 `PressurePlate`
- 1 `TimedMovingPlatform`
- 1 `MemoryGate_D`
- 1 `LevelExit`

### Jerarquia recomendada en Unity

```text
Level_05_CadenaEstable
|- Lighting
|- Camera
|- UI
|- Environment
|  |- Platform_Start
|  |- Walk_Left
|  |- Platform_A
|  |- Platform_B
|  |- Walk_Exit
|  |- Platform_Exit
|- Mechanics
|  |- PressurePlate_A
|  |- PressurePlate_B
|  |- BridgeAnchor_C
|  |  |- Bridge_C
|  |- MemoryGate_D
|  |- LevelExit
|- Player
```

### Medidas, posiciones y escalas exactas

```text
Platform_Start        pos ( 0.0, 0.0,  0.0) scale (10.0, 0.5,  8.0)
Walk_Left             pos (-4.0, 0.0,  5.0) scale ( 3.0, 0.5,  4.0)
Platform_A            pos (-8.0, 0.0, 10.0) scale ( 6.0, 0.5,  6.0)
Platform_B            pos ( 0.0, 0.0, 18.0) scale ( 8.0, 0.5,  8.0)
Walk_Exit             pos ( 4.0, 0.0, 22.0) scale ( 2.0, 0.5,  4.0)
Platform_Exit         pos ( 8.0, 0.0, 26.0) scale ( 8.0, 0.5,  8.0)

PressurePlate_A       pos (-8.0, 0.35, 10.0) scale ( 2.5, 0.20, 2.5)
PressurePlate_B       pos ( 0.0, 0.35, 18.0) scale ( 2.5, 0.20, 2.5)
BridgeAnchor_C        pos ( 0.0, 0.0,  9.0)
Bridge_C inactive     local ( 0.0,-4.0, 0.0)
Bridge_C active       local ( 0.0, 0.0, 0.0)
Bridge_C scale        local scale ( 3.0, 0.5, 12.0)
MemoryGate_D          pos ( 5.5, 1.75, 22.0) scale ( 0.5, 3.50, 4.0)
LevelExit             pos ( 8.0, 1.25, 29.0) scale ( 2.5, 2.50, 0.8)

OpeningShotAnchor     pos ( 9.0, 7.0, -7.0) rot (21.0, -40.0, 0.0)
ObjectiveAnchor       pos ( 0.0, 1.8, 18.0)
GoalLight             pos ( 8.0, 3.0, 26.0) range 10 intensity 4 color #FFD36B
Player Spawn          pos ( 0.0, 1.10,  0.0)
```

### Relaciones exactas entre objetos

1. `PressurePlate_A` activa `Bridge_C`.
2. `Bridge_C` es la unica ruta directa hacia `Platform_B`.
3. `PressurePlate_B` abre `MemoryGate_D`.
4. `MemoryGate_D` bloquea la pasarela final a `Platform_Exit`.

### Ruta del jugador

Secuencia correcta:

1. Graba eco 1 quieto en `PressurePlate_A` durante `2.5 s`.
2. Con `Bridge_C` activo por eco 1, cruza hacia `Platform_B`.
3. Graba eco 2 quieto en `PressurePlate_B` durante `2.5 s`.
4. Vuelve al inicio cruzando `Bridge_C` de regreso (sigue activo por eco 1).
5. Ambos ecos en loop: `Bridge_C` arriba y `MemoryGate_D` abierta.
6. Cruza `Bridge_C` → `Platform_B` → `Walk_Exit` → `MemoryGate_D` → `Platform_Exit`.
7. Sale.

### Ruta de los ecos

- Eco 1: quieto en `(-8.0, 0.36, 10.0)`
- Eco 2: quieto en `( 0.0, 0.36, 18.0)`; este eco solo pudo ser grabado porque eco 1 ya habia elevado `Bridge_C`

### Como se ve desde la camara

- Desde el spawn se ven tres capas claras: isla A a la izquierda, puente hundido al centro y meta al fondo derecha.
- El jugador puede leer toda la cadena con una sola mirada.
- El centro de pantalla inicial se apoya en `Bridge_C`.

### Que ve el jugador antes de actuar

1. Que el puente central no existe aun.
2. Que hay una isla A obvia a la izquierda.
3. Que la salida final sigue mas alla de otro bloqueo.

### Que siente al resolverlo

Que ya no solo usa ecos; construye una dependencia entre decisiones.

### Momento wow

Ver al eco 1 mantener el puente, al eco 2 sostener la puerta final y al jugador atravesar una ruta hecha de decisiones previas.

### Como evita frustracion

1. Cada subobjetivo es visible desde el anterior.
2. La ruta de grabacion de eco 2 ya esta ensenada por niveles previos.
3. Todo el nivel sigue siendo binario y robusto: placa, puente, placa, puerta.
4. Si algo sale mal, `Q` reinicia el estado sin recargar.

### Como aumenta dificultad sin confundir

Suma dependencia entre ecos, no mecanicas nuevas.

### Camara cinematografica por nivel

- Posicion base de opening shot: `(9.0, 7.0, -7.0)`
- Angulo recomendado: `(21, -40, 0)`
- Centro de pantalla: `Bridge_C`
- Mover camara: al activarse `Bridge_C` y al abrirse `MemoryGate_D`
- Zoom: corto al crear el segundo eco
- Quieta: durante la carrera final hacia la salida

### UI por nivel

- Texto inicial: "Construye una cadena estable." color `#F1F6FA`, `3.0 s`
- Texto contextual al crear eco 1: "Ahora usa ese espacio para grabar el siguiente." color `#00D9FF`, `2.8 s`
- Texto contextual al crear eco 2: "La cadena ya existe." color `#7AF0C8`, `2.0 s`
- Feedback de exito al abrir `MemoryGate_D`: "Ruta final abierta" color `#FFD36B`, `1.0 s`

### Polish del nivel

- Sonido de interaccion: placa A grave, placa B media
- Sonido de grabacion: un poco mas brillante en eco 2
- Sonido de eco: doble pulso cuando ambos ecos estan activos
- Efecto visual minimo: dos haces de luz encadenados, A->puente y B->puerta
- Feedback de exito: un bloom fino recorre toda la ruta hasta la salida
- Feedback de error suave: si el jugador intenta grabar eco 2 sin eco 1, el HUD muestra "Antes, sostiene el puente"

## 11. Nivel Final - BOSS PUZZLE - "Nucleo"

### Rol en la curva

Sintesis total. Es el puzzle mas claro, elegante y memorable. No agrega nada nuevo. Solo junta todo con una composicion fuerte.

### Objetivo del nivel

Usar un eco para habilitar el trayecto hacia la segunda placa, usar un segundo eco para activar el puente final al nucleo y caminar hacia el centro del Archivo.

### Narrativa breve

El Archivo deja de ser un conjunto de fragmentos. Las decisiones que el jugador sostuvo en niveles previos ahora reconstruyen el camino de regreso al nucleo.

### Que ensena exactamente

No ensena una regla nueva. Demuestra dominio completo del sistema.

### Objetos necesarios

- 1 `Player`
- 1 `MainCamera`
- 1 `GameHUD`
- 1 `TutorialHUD`
- 5 plataformas estaticas
- 2 pasarelas estaticas
- 2 `PressurePlate`
- 2 `TimedMovingPlatform`
- 1 `CoreExit`
- 1 `DirectionalLight`
- 1 `CoreLight`

### Jerarquia recomendada en Unity

```text
Level_06_Nucleo
|- Lighting
|  |- DirectionalLight
|  |- CoreLight
|- Camera
|  |- MainCamera
|  |- OpeningShotAnchor
|  |- ObjectiveAnchor
|- UI
|  |- GameHUD
|  |- TutorialHUD
|- Environment
|  |- Platform_Start
|  |- Walk_Left
|  |- Platform_A
|  |- Walk_Upper
|  |- Platform_B
|  |- Platform_Core
|- Mechanics
|  |- PressurePlate_A
|  |- PressurePlate_B
|  |- Bridge_Upper_Anchor
|  |  |- Bridge_Upper
|  |- Bridge_Core_Anchor
|  |  |- Bridge_Core
|  |- LevelExit
|- Player
```

### Medidas, posiciones y escalas exactas

```text
Platform_Start          pos ( 0.0, 0.0,-14.0) scale (10.0, 0.5,  8.0)
Walk_Left               pos (-4.0, 0.0,-10.0) scale ( 3.0, 0.5,  4.0)
Platform_A              pos (-8.0, 0.0, -6.0) scale ( 6.0, 0.5,  6.0)
Walk_Upper              pos (-4.0, 0.0,  2.0) scale ( 3.0, 0.5,  4.0)
Platform_B              pos (-4.0, 0.0, 12.0) scale ( 6.0, 0.5,  6.0)
Platform_Core           pos ( 0.0, 0.0,  0.0) scale ( 8.0, 0.5,  8.0)

PressurePlate_A         pos (-8.0, 0.35, -6.0) scale ( 2.5, 0.20, 2.5)
PressurePlate_B         pos (-4.0, 0.35, 12.0) scale ( 2.5, 0.20, 2.5)

Bridge_Upper_Anchor     pos (-4.0, 0.0,  4.0)
Bridge_Upper inactive   local ( 0.0,-4.0, 0.0)
Bridge_Upper active     local ( 0.0, 0.0, 0.0)
Bridge_Upper scale      local scale ( 3.0, 0.5, 12.0)

Bridge_Core_Anchor      pos ( 0.0, 0.0, -5.0)
Bridge_Core inactive    local ( 0.0,-4.0, 0.0)
Bridge_Core active      local ( 0.0, 0.0, 0.0)
Bridge_Core scale       local scale ( 3.0, 0.5, 14.0)

LevelExit               pos ( 0.0, 1.25,  0.0) scale ( 2.5, 2.50, 0.8)
OpeningShotAnchor       pos (10.0, 8.0,-18.0) rot (24.0, -28.0, 0.0)
ObjectiveAnchor         pos ( 0.0, 2.2,  0.0)
CoreLight               pos ( 0.0, 4.0,  0.0) range 14 intensity 6 color #F5FBFF
Player Spawn            pos ( 0.0, 1.10,-14.0)
```

### Relaciones exactas entre objetos

1. `PressurePlate_A` activa `Bridge_Upper`.
2. `Bridge_Upper` hace posible llegar a `PressurePlate_B`.
3. `PressurePlate_B` activa `Bridge_Core`.
4. `Bridge_Core` conecta directamente el spawn con `Platform_Core`.
5. La meta esta en el centro desde el inicio, visible pero inaccesible.

### Orden exacto de colocacion

1. Colocar `Platform_Core` primero. Debe ser el centro emocional y visual.
2. Colocar `Platform_Start` al sur del nucleo.
3. Colocar `Platform_A` al suroeste y unirla con `Walk_Left`.
4. Colocar `Bridge_Upper` como pieza vertical entre el ramal izquierdo y `Platform_B`.
5. Colocar `Platform_B` al norte.
6. Colocar `Bridge_Core` como puente final entre `Platform_Start` y `Platform_Core`.

### Ruta del jugador

Secuencia correcta:

1. Crear eco 1 en `PressurePlate_A` durante `2.5 s`.
2. Con `Bridge_Upper` activo, recorrer la ruta superior y crear eco 2 en `PressurePlate_B` durante `2.5 s`.
3. Volver visualmente al sur. No hace falta reset.
4. Durante la reproduccion, caminar recto por `Bridge_Core`.
5. Entrar al nucleo.

### Ruta de los ecos

- Eco 1: quieto en `(-8.0, 0.36, -6.0)`
- Eco 2: quieto en `(-4.0, 0.36, 12.0)`; este eco solo existe gracias a eco 1

### Como se ve desde la camara

- El spawn ya muestra el nucleo brillante al centro y el puente final hundido delante del jugador.
- La camara debe vender inmediatamente la pregunta del nivel: "como vuelvo ahi?"
- El centro de pantalla inicial debe estar sobre `Platform_Core`.

### Que ve el jugador antes de actuar

1. La meta final.
2. El puente final ausente.
3. El ramal izquierdo accesible.
4. La plataforma norte sugerida por encima.

### Que siente al resolverlo

Satisfaccion limpia y emocional. No supera el sistema; lo comprende por completo.

### Momento wow

Cuando ambos ecos sostienen el espacio y el puente directo al nucleo emerge frente al jugador, dejando una ultima caminata corta y segura hacia el centro.

### Como evita frustracion

1. La meta final es visible siempre.
2. Solo hay dos placas y dos puentes.
3. La ruta de solucion es una sintesis de niveles ya dominados.
4. La ultima caminata no exige precision. Exige comprension.

### Como aumenta dificultad sin confundir

No agrega densidad extra. Agrega peso simbolico y composicion mas fuerte.

### Camara cinematografica por nivel

- Posicion base de opening shot: `(10.0, 8.0, -18.0)`
- Angulo recomendado: `(24, -28, 0)`
- Centro de pantalla: `Platform_Core`
- Mover camara: al activarse `Bridge_Upper` y al emerger `Bridge_Core`
- Zoom: al emerger `Bridge_Core`, FOV `52` por `0.35 s`
- Quieta: durante la caminata final al nucleo

### UI por nivel

- Texto inicial: "Vuelve al nucleo." color `#F1F6FA`, `3.0 s`
- Texto contextual al crear eco 1: "Abre el camino a la decision siguiente." color `#00D9FF`, `2.5 s`
- Texto contextual al crear eco 2: "El centro ya puede recibirte." color `#7AF0C8`, `2.5 s`
- Feedback final: "Identidad restaurada" color `#FFD36B`, `1.6 s`

### Polish del nivel

- Sonido de interaccion: limpio, casi ritual
- Sonido de grabacion: el mismo motif pero con cola ligeramente mas larga
- Sonido de eco: doble armonico al estar ambos activos
- Efecto visual minimo: haces que convergen desde A y B hacia el puente central
- Feedback de exito: `Bridge_Core` emerge con bloom blanco suave y sin violencia
- Feedback de error suave: si el jugador intenta correr al nucleo antes de tiempo, el puente apagado pulsea una vez y la UI recuerda "Todavia falta otra decision"

## 12. Por que la progresion funciona

Aprender -> Nivel 1

- Presenta el verbo base en un entorno lineal.
- No exige leer mas de un sistema a la vez.
- El "aha" ocurre en menos de dos minutos.

Experimentar -> Nivel 2

- Mantiene el mismo eco, pero cambia el efecto espacial.
- El jugador descubre que la mecanica es flexible.

Reforzar -> Nivel 3

- Duplica el uso del sistema sin introducir vocabulario nuevo.
- Repite lo aprendido con una lectura espacial mas amplia.

Twist -> Nivel 4

- La mecanica es la misma, pero ahora importa el orden.
- El jugador deja de seguir al eco y empieza a pensar en consecuencias temporales.

Ejecucion -> Nivel 5

- Junta acumulacion, dependencia y recorrido final.
- Exige que el jugador planifique una cadena robusta.

Boss puzzle -> Final

- Reune todo lo anterior en la version mas legible y elegante.
- El objetivo final esta visible desde el inicio, asi que el cierre es claro y memorable.

## 13. Por que este diseno maximiza la puntuacion del jurado

Funcionamiento:

1. Solo usa un sistema central y dos soportes binarios.
2. Los estados son claros y faciles de testear.
3. Cada nivel tiene una solucion principal muy estable.

Logica y mecanicas:

1. Todo deriva de la misma regla del eco.
2. La progresion no salta de idea en idea.
3. El twist nace del tiempo y del orden, no de meter otra mecanica ajena.

Creatividad e innovacion:

1. El eco se presenta como memoria util, no como gimmick.
2. El mensaje "tus decisiones construyen quien eres" esta integrado al puzzle.
3. El final convierte la logica del sistema en cierre narrativo.

Diseno grafico y UX:

1. El objetivo siempre esta visible.
2. La UI es minima, legible y nunca tapa el escenario.
3. La camara existe para ensenar, no para presumir.
4. La estetica abstracta refuerza memoria, identidad y aprendizaje.

Comprension del proyecto por parte del equipo:

1. El equipo puede explicar cada nivel con una sola frase de diseno.
2. El equipo puede justificar por que solo hay placas, puentes y puertas.
3. El equipo puede mostrar una curva pedagogica exacta: aprender, experimentar, reforzar, twist, ejecucion, sintesis.

## 14. Checklist final de produccion

1. Ajustar `EchoRecorder` a `6 s` y `2` ecos maximos.
2. Ajustar `PlayerController` al set de feeling propuesto.
3. Agregar coyote time y jump buffer.
4. Implementar `Q` para `ClearAllEchoes` y reset de dinamicos.
5. Blockout de `Level_01` a `Level_06`.
6. Probar cada solucion cinco veces seguidas sin fallos graves.
7. Recién despues agregar polish visual y audio.

Si el equipo tiene que recortar tiempo, no recortar niveles a medias. Recortar cualquier feature secundaria y conservar intactos:

1. El sistema de ecos.
2. La lectura de camara.
3. La UI minima.
4. La curva completa de 6 escenas.
