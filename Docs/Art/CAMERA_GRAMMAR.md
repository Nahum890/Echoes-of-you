# CAMERA_GRAMMAR.md
# Echoes of You 2.0

Version: 1.0

Este documento define el lenguaje visual de la cámara.

La cámara NO sigue únicamente al jugador.

La cámara enseña.

La cámara dirige.

La cámara emociona.

La cámara narra.

La cámara es un recurso de gameplay.

Si una decisión contradice este documento,
este documento tiene prioridad.

---

# FILOSOFÍA

Inspiraciones:

- Portal
- Portal 2
- Inside
- Little Nightmares
- ICO
- Shadow of the Colossus
- Silent Hill 2
- Viewfinder
- The Witness

La cámara nunca existe únicamente para seguir.

Siempre responde a una intención.

Toda cámara debe responder una única pregunta:

"¿Qué necesita entender o sentir el jugador en este momento?"

---

# PRINCIPIOS

## 1. Legibilidad

El jugador siempre debe entender:

• dónde está

• dónde debe ir

• qué controla

• qué activó

• qué cambió

Nunca ocultar información necesaria.

---

## 2. Enseñanza

Cuando aparece una mecánica nueva:

la cámara la presenta.

Nunca esperar que el jugador la descubra por accidente.

---

## 3. Prioridad visual

Siempre existe un foco principal.

Orden de prioridad:

1 Jugador

2 Eco

3 Objetivo

4 Botones

5 Puertas

6 Plataformas

7 Decoración

---

## 4. Movimiento

La cámara casi nunca rota violentamente.

Debe sentirse pesada.

Suave.

Como una cámara física.

No un dron.

---

## 5. Regla del Eco

Siempre que exista un eco activo:

La cámara intenta mantener visibles

Jugador

+

Eco

+

Objetivo

en el mismo plano.

Si esto no es posible:

prioridad:

Objetivo

↓

Eco

↓

Jugador

---

# TIPOS DE CÁMARA

## Learning Camera

Objetivo:

Enseñar una mecánica.

Duración:

2-4 segundos

Acciones:

• gira lentamente

• muestra el puzzle

• vuelve

Nunca quitar control al jugador.

---

## Discovery Camera

Cuando el jugador entra a un espacio importante.

Debe mostrar:

• arquitectura

• punto de interés

• salida

Nunca recorrer todo el nivel.

Solo insinuarlo.

---

## Puzzle Camera

Durante puzzles.

Debe mostrar simultáneamente:

Jugador

Eco

Objetivo

Si alguno desaparece más de 2 segundos:

es un error.

---

## Transition Camera

Entre espacios.

Usar blends.

Nunca cortes bruscos.

---

## Emotional Camera

Durante momentos narrativos.

Características:

FOV menor

Movimiento lento

Más cercanía

Silencio

No shakes.

---

## Suspense Camera

Cuando una acción modifica el entorno.

Ejemplo:

Botón

↓

Puerta

La cámara mira la puerta.

Después vuelve.

---

## Memory Camera

Durante la grabación.

Aplicar:

FOV -3°

Ligero zoom

Noise muy leve

No shakes

No dutch

---

## Replay Camera

Mientras el eco reproduce.

Mantener ambos personajes visibles.

---

## Acceptance Camera

Últimos niveles.

Movimiento más libre.

Más respiración.

Más espacio.

Más calma.

---

# FOV

Exploración

50°

Puzzle

45°

Narrativa

38-42°

Espacios abiertos

55°

Nunca usar FOV extremos.

---

# BLENDS

Gameplay

0.5 s

Narrativa

1.5 s

Discovery

2 s

Nunca Instant Cut salvo muerte.

---

# TARGET GROUPS

Cuando exista:

Jugador

+

Eco

↓

Target Group

Nunca seguir solo al jugador.

---

# CLEAR SHOT

Usar únicamente para evitar:

paredes

columnas

muebles grandes

Nunca cambiar cámara por decoración pequeña.

---

# IMPULSE

Solo usar para:

Activar Eco

Colapso

Gran puerta

Nunca en cada salto.

Nunca al caminar.

---

# NOISE

Reposo

0

Grabación

0.15

Momentos emocionales

0

Nunca usar ruido permanente.

---

# DUTCH

Solo cuando la realidad falla.

Máximo:

5°

Nunca usar por estética.

Debe tener significado.

---

# COMPOSICIÓN

Regla de tercios.

Nunca centrar siempre al jugador.

Dejar espacio hacia donde mira.

---

# CÁMARAS POR CAPÍTULO

Capítulo I

Muy estable.

Curiosa.

Capítulo II

Empieza a observar más.

Capítulo III

Muestra al Eco constantemente.

Capítulo IV

Más cercana.

Capítulo V

Más incómoda.

Capítulo VI

Más tranquila.

---

# REGLAS DE VALIDACIÓN

Cada nivel debe responder:

□ ¿Siempre veo mi objetivo?

□ ¿Siempre veo al eco cuando importa?

□ ¿La cámara enseña la mecánica?

□ ¿La cámara muestra las consecuencias?

□ ¿Existe algún momento donde pierda al jugador?

□ ¿La cámara atraviesa paredes?

□ ¿La cámara produce mareo?

□ ¿Existe algún giro innecesario?

Si alguna respuesta es Sí donde no corresponde:

El nivel falla QA.

---

# PROHIBIDO

❌ Cámara FPS

❌ Rotaciones instantáneas

❌ Shake permanente

❌ Zoom constante

❌ Dutch por estética

❌ Cortes sin intención

❌ Cámara demasiado alta

❌ Cámara demasiado alejada

❌ Cámara que oculta el puzzle

❌ Cámara que pierde al Eco