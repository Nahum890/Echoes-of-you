# Soluciones de niveles - Echoes of You

Este documento describe la ruta base esperada de cada nivel. No es la unica solucion posible cuando hay varias grabaciones, pero si es la solucion de validacion.

## Controles relevantes

- `R` o `E`: graba el cuerpo real del jugador. Al soltar, se crea un eco que repite esa ruta.
- `F`: proyeccion de eco. Mientras se mantiene, el cuerpo del jugador queda quieto y se controla una proyeccion. Al soltar `F`, esa proyeccion se convierte en eco.
- `Q`: limpia los ecos.
- `Shift`: acelera al jugador o a la proyeccion.

## Nivel 1 - El Primer Eco

Idea: el eco no es un ayudante; es una accion pasada que sigue ocurriendo.

Solucion base:

1. Mantener `F` desde la entrada.
2. Pilotar la proyeccion hacia la zona azul lateral izquierda.
3. Soltar `F` cuando la proyeccion esta dentro de la zona azul.
4. El eco activa el puente temporal desde la zona azul.
5. Cruzar la fractura mientras el puente esta elevado.

Twist: el jugador puede quedarse quieto, pero el eco sigue ejecutando la accion proyectada.

Aha moment: puedo coordinar mi cuerpo actual con una accion temporal separada.

## Nivel 2 - Espacio Dinamico

Idea: el eco modifica la geometria, no solo abre una puerta.

Solucion base:

1. Mantener `F`.
2. Enviar la proyeccion a la zona azul del lateral izquierdo.
3. Soltar `F` para crear el eco.
4. El eco empuja el bloque gigante y cambia la ruta jugable.
5. Esperar la ventana del elevador vertical.
6. Subir por la ruta que el bloque dejo libre y entrar al portal.

Twist: el mismo bloque que abre ruta tambien puede cortar o bloquear lectura espacial si se activa con mal timing.

Aha moment: el espacio es parte del puzzle y puede ser reescrito por el eco.

## Nivel 3 - Proteccion

Idea: el eco puede recibir o bloquear una amenaza antes que el jugador.

Solucion base:

1. No intentar cruzar primero; la energia roja mata al jugador.
2. Mantener `F` desde la plataforma inicial.
3. Pilotar la proyeccion a traves del primer pulso rojo, la isla segura y el segundo pulso rojo.
4. Soltar `F`.
5. Seguir al eco real: cada campo rojo se vuelve azul mientras el eco lo atraviesa.
6. Cruzar cada pulso solo durante esa ventana azul.

Twist: la solucion no es correr mejor, sino poner una consecuencia temporal delante del jugador.

Aha moment: el eco tambien puede proteger.

## Nivel 4 - Conflicto

Idea: el pasado tambien puede perjudicarte.

Solucion base:

1. Reconocer que la ruta central es una trampa: si un eco pasa por ahi, la puerta final se cierra.
2. Mantener `F` y mandar la proyeccion por la ruta lateral.
3. Usar la zona azul lateral para mover el puente superior.
4. Soltar `F`.
5. Esperar a que el eco abandone la trampa o evitar que la active.
6. Cruzar por la ruta superior cuando la puerta final no este bloqueada.

Twist: la ruta obvia funciona para el jugador, pero falla cuando la repite el eco.

Aha moment: no basta con grabar una accion util; hay que evitar acciones pasadas peligrosas.

## Nivel 5 - Caos Controlado

Idea: combinar mover, proteger y conflicto en una coreografia.

Solucion base:

1. Crear un eco para mover el bloque de ruta en la zona azul correspondiente.
2. Crear otro eco que atraviese el muro de energia final para volverlo seguro en la ventana correcta.
3. Crear un tercer eco que active y abandone la trampa, evitando que el bloqueo final se mantenga cerrado.
4. Usar los elevadores sincronizados y el rotador central para llegar al ascensor final.
5. Cruzar el muro de energia cuando este azul.
6. Entrar al portal.

Twist: todos los sistemas son simultaneos; resolver uno sin considerar los otros no basta.

Aha moment: el jugador ya no usa ecos individuales, compone un sistema.

## Nivel 6 - Dominio

Idea: prueba final de dominio sistemico.

Solucion base:

1. Proyectar un eco al contrapeso para mover la estructura principal.
2. Proyectar o grabar otro eco para cubrir el muro de energia del nucleo.
3. Usar el tercer eco para activar y abandonar la trampa final, de modo que la puerta no quede cerrada al entrar.
4. Leer los rotadores y ascensores como ventanas de timing, no como plataformas estaticas.
5. Subir al nucleo cuando bloque, energia y trampa esten en fase.
6. Entrar al portal final.

Twist: repetir una solucion vieja de forma aislada falla; hay que encadenar consecuencias.

Aha moment: el eco es una accion reutilizable dentro de un sistema vivo.

## Criterios de validacion

- Cada nivel tiene una ruta base documentada.
- Cada nivel usa al menos un sistema dinamico.
- Los niveles 3 a 6 pueden resolverse sin pedir que el jugador cruce primero una zona imposible.
- Los botones pueden existir, pero ningun nivel depende solo de "eco mantiene boton".
- El humo azul al crear eco esta desactivado; queda feedback de audio/camara/postproceso.
