# BRIEF ESPACIAL — 15 NIVELES
## Echoes of You 2.0 — Qué construir en Unity
## Usar junto con ECHOES_BIBLE.md y PROJECT_CONTEXT.md

Este documento describe exactamente qué zonas y geometría componen
cada nivel. No describe mecánicas (eso está en la Biblia). Describe
el espacio físico que el jugador recorre.

Materiales a usar: solo los de EchoesMaterialLibrary.
Nunca crear materiales nuevos fuera de esa librería.

---

## NIVEL 1 — Desorientación
**Atmósfera:** fogColor `1C2430`, density 0.016
**Material de pared:** WallTealMat

### Zonas (en orden de recorrido)

**Zona A — Entrada de la escuela**
- Porche exterior cubierto: 3 columnas de hormigón, techo bajo, suelo mojado
- Puerta doble de madera — abierta, un panel torcido
- Cartelera de corcho a la derecha: avisos en papel, algunos desprendidos
- 1 banco de madera largo, vacío

**Zona B — Pasillo A**
- Ancho 4 unidades, alto 3 unidades, largo 20 unidades
- Lockers a ambos lados — algunos abiertos, algunos abolladuras
- Fluorescente cada 7 metros — el del centro parpadea
- Ventanas altas (1.5 unidades) a la derecha, vista a patio oscuro
- Piso: linóleo gris con junta entre baldosas

**Zona C — Pasillo B (idéntico a A)**
- Copia exacta del pasillo A — misma geometría, misma posición de lockers
- Diferencia: las ventanas dan a otro pasillo, no al patio
- El fluorescente del centro NO parpadea — todo funciona
- Efecto: el jugador no nota la diferencia en la primera pasada

**Zona D — Umbral final**
- Apertura más ancha (6 unidades)
- El eco aparece aquí y cruza hacia la oscuridad
- No hay nada más. Sin puerta, sin luz al fondo.

### Objetos narrativos
- Un abrigo colgado en el último locker abierto (color `memory-amber`)
- Un cuaderno caído en el suelo a mitad del Pasillo A

---

## NIVEL 2 — Repetición
**Atmósfera:** fogColor `1A2028`, density 0.020
**Material de pared:** WallTealMat (pasillo) + WallMustardMat (aulas)

### Zonas

**Zona A — Pasillo de acceso**
- Pasillo corto (10 unidades), sin lockers, con puertas de aula a ambos lados
- Todas las puertas cerradas excepto las de los puzzles

**Zona B — Aula 1** (puzzle 1)
- Espacio: 10 × 8 × 3 unidades
- 4 filas de 5 pupitres dobles, ordenados
- Pizarrón al frente, escritorio del profesor
- Mecanismo de puzzle integrado debajo de la tarima del profesor

**Zona C — Pasillo de conexión** (entre aulas)
- 8 unidades de pasillo, ventana al fondo

**Zona D — Aula 2** (puzzle 2)
- Igual que Aula 1 pero algunos pupitres corridos, 2 caídos
- La pizarrón tiene tiza borrada parcialmente

**Zona E — Aula 3** (puzzle 3)
- Igual que Aula 2 pero más desordenada
- 3 sillas en el pasillo central, papeles en el suelo

### Objetos narrativos
- Fotografía clavada en la cartelera del pasillo (sin texto legible, figura femenina)
- Cuaderno abierto en el escritorio del profesor del Aula 3

---

## NIVEL 3 — Indecisión
**Atmósfera:** fogColor `1C232E`, density 0.022
**Materiales:** WallTealMat (lado Aiden) + WallRoseMat (lado Lyra)

### Zonas

**Zona A — Hall de bifurcación**
- Espacio circular de 8 unidades de diámetro
- Dos pasillos idénticos visibles simultáneamente desde el centro
- La cámara encuadra ambos pasillos en el mismo plano

**Zona B — Pasillo izquierdo (Aiden)**
- Lockers, carteles deportivos, trofeos rotos en vitrina
- Color institucional frío

**Zona C — Pasillo derecho (Lyra)**
- Lockers abiertos, cartelera con dibujos a lápiz, 1 silla con mochila
- Color rose apagado, 1 objeto en memory-amber al fondo

**Zona D — Sala de confluencia**
- Donde ambos pasillos se unen
- Puerta de salida al centro, flanqueada por ventanas altas
- El suelo tiene dos colores distintos que se encuentran aquí

---

## NIVEL 4 — Espera (Timing vertical)
**Atmósfera:** fogColor `201C22`, density 0.020
**Material de pared:** WallMustardMat

### Zonas

**Zona A — Pre-observación**
- Sala pequeña (6 × 6) donde el mecanismo de plataformas es visible pero no alcanzable
- El jugador lee el ritmo sin consecuencias
- Cartelera con horario de clases en la pared

**Zona B — Aula con desnivel**
- Tarima del profesor (elevada 1 unidad) al frente
- Las plataformas del puzzle son parte de la tarima y estanterías laterales
- No parecen plataformas de videojuego — parecen el mobiliario del aula

**Zona C — Corredor de escape**
- Pasillo lateral que conecta con la salida
- Solo accesible si se resuelve el puzzle
- Ventana al exterior al fondo

### Objetos narrativos
- Reloj de pared detenido a las 15:40
- Nota adhesiva en el escritorio (ilegible desde lejos)

---

## NIVEL 5 — Culpa (Laberinto horizontal)
**Atmósfera:** fogColor `141820`, density 0.032 (máximo hasta aquí)
**Material de pared:** ArchMat (mantenimiento) con Pipe Kit como detalle

### Zonas

**Zona A — Entrada al pasillo técnico**
- Cambio de material abrupto: de WallTeal a ArchMat gris
- Tuberías expuestas en el techo
- Iluminación de emergencia (roja, escasa)

**Zona B — Zona de observación del laberinto**
- Rejilla o ventana desde la que el jugador ve el laberinto completo sin poder entrar
- Aprende el ritmo de las puertas desde aquí

**Zona C — Laberinto de mantenimiento**
- Red de pasillos de 2.5 unidades de ancho
- Puertas que se cierran con ritmo fijo
- 3 ramas: 1 correcta, 2 sin salida
- Al fondo de cada rama sin salida: algo que no debería estar ahí (una silla de aula, una taza)

**Zona D — Sala central del laberinto**
- Donde ocurre el puzzle de la voz grabada
- Una sola bombilla colgante
- El eco queda atrapado en bucle aquí al final del nivel

### Objetos narrativos
- Extintor caído
- Casco de mantenimiento en el suelo
- Fotografía de grupo escolar pegada en la pared con cinta adhesiva, parcialmente arrancada

---

## NIVEL 6 — Negación (Salto de fe)
**Atmósfera:** fogColor `161C18`, density 0.030
**Material de pared:** WallSageMat (biblioteca)

### Zonas

**Zona A — Entrada a la biblioteca**
- Puerta doble de madera, ventana lateral pequeña
- Zona de lectura con mesas y sillas — todo quieto, sin nadie

**Zona B — Pasillo de estanterías** (zona de aprendizaje)
- Estanterías de 3.5 unidades de alto, pasillo de 2 unidades de ancho
- Abismo pequeño entre dos estanterías — puente invisible visible desde aquí
- No letal. El jugador cae y vuelve.

**Zona C — Sala del abismo principal**
- Abismo de 8 unidades de ancho, 12 de profundidad
- El puente espectral aparece solo durante la grabación/reproducción
- Vista larga desde la entrada: el jugador ve el otro lado antes de saber cómo llegar

**Zona D — Sala de llegada**
- Espejo de la sala de inicio de la biblioteca
- Mismas estanterías, pero algunos libros caídos, más desordenada

### Objetos narrativos
- Un libro abierto en una de las mesas de lectura, páginas en blanco
- Sello de biblioteca en el mostrador de entrada

---

## NIVEL 7 — Evasión (Grabación anticipada)
**Atmósfera:** fogColor `141A24`, density 0.018
**Material de pared:** WallTealMat + transición a WallMustardMat

### Zonas

**Zona A — Corredor de emergencia**
- Pasillo lateral con señalética de emergencia
- Puerta de salida de emergencia visible al fondo (cerrada)
- Barra anti-pánico en la puerta

**Zona B — Patio trasero parcial**
- Espacio semiabierto: paredes a 3 lados, cielo abierto arriba
- Elemento temporal que dura 5 segundos (una persiana que sube, una puerta que gira)
- El jugador ve el ciclo varias veces antes de intentar

**Zona C — Sala de llegada**
- Cuarto de almacenamiento: cajas apiladas, material escolar viejo
- Salida marcada con light-amber débil

### Objetos narrativos
- Aviso de "SALIDA DE EMERGENCIA" con una de las letras caída
- Un balón de fútbol en el rincón del patio

---

## NIVEL 8 — Autosabotaje (Dos ecos)
**Atmósfera:** fogColor `1C1E26`, density 0.026
**Material de pared:** WallMustardMat (sala de profesores)

### Zonas

**Zona A — Antesala**
- Pequeño vestíbulo con buzón de notas y tablón de anuncios
- El tablón tiene dos avisos contradictorios (ilegibles)

**Zona B — Sala de profesores**
- Espacio amplio: 14 × 10 × 3 unidades
- Mesas con sillas individuales (no pupitres dobles)
- Cafetera vieja en la encimera
- Ventanas a un patio interior (no accesible)
- Los dos mecanismos de puzzle están integrados en el mobiliario

**Zona C — Cuarto de fotocopiadora**
- Sala pequeña lateral
- La fotocopiadora es parte del puzzle visual (no mecánico)
- 1 mesa con papeles apilados

### Objetos narrativos
- 2 tazas de café en mesas distintas — una con café, una vacía
- Lista de asistencia en la pizarra blanca, nombres borrados excepto uno

---

## NIVEL 9 — Control (Patio exterior)
**Atmósfera:** fogColor `2A3038`, density 0.013 (mínimo — el respiro)
**Sin material de pared — espacio exterior**

### Zonas

**Zona A — Salida al patio** (desde el interior)
- Puerta doble con vidrio — se ve el patio antes de entrar
- El cambio de FOV ocurre al cruzar esta puerta

**Zona B — Patio central** (único espacio sin techo)
- 30 × 30 unidades, suelo de cemento con líneas desgastadas de cancha
- Aros de baloncesto sin red a los lados
- Bancos de madera en el perímetro
- Árbol aislado en una esquina (único elemento orgánico del juego)
- El peligro del puzzle es visible desde el centro

**Zona C — Galería perimetral**
- Corredor cubierto de 3 unidades que rodea el patio
- Columnas regulares, acceso a otras zonas del edificio (cerradas)

**Zona D — Salida opuesta**
- Puerta al fondo del patio, levemente iluminada

### Objetos narrativos
- Carrito de conserje abandonado
- Aro de baloncesto con la red parcialmente colgando
- Pintada (graffiti) de tiza en el suelo, casi borrada

---

## NIVEL 10 — Recuerdos (Eco de Lyra)
**Atmósfera:** fogColor `201E22`, density 0.020
**Material de pared:** WallRoseMat

### Zonas

**Zona A — Umbral del aula de Lyra**
- Marco de puerta más elaborado que el resto — madera más clara
- El espacio ya se ve antes de entrar — algo lo distingue

**Zona B — Aula de Lyra**
- Pupitres en semicírculo en lugar de filas rectas
- Pizarrón con dibujos parcialmente borrados
- Ventanas grandes — más luz que los otros niveles
- El único pupitre en `memory-amber` es el de Lyra (último de la fila derecha)
- Las plataformas invisibles están en el espacio aéreo del aula

**Zona C — Despacho lateral**
- Sala pequeña conectada al aula (antes era el cuarto del proyector)
- Donde se reproduce la voz de Lyra

**Zona D — Pasillo de salida**
- Más oscuro que el aula
- La temperatura visual baja de golpe al salir del aula

### Objetos narrativos
- Mochila debajo del pupitre de memory-amber
- Dibujo en la pizarra: dos siluetas, una más alta que la otra
- Flores secas en un vaso en la repisa de la ventana

---

## NIVEL 11 — Conexión (Escalera + grabación limitada)
**Atmósfera:** fogColor `1E222C`, density 0.024
**Material de pared:** WallTealMat

### Zonas

**Zona A — Base de la escalera**
- Hall de escalera estándar: espacio 6 × 6 unidades
- Buzón de anuncios, planta en maceta seca

**Zona B — Primer tramo**
- 12 peldaños, barandas metálicas
- Ventana en el rellano — vista a exterior

**Zona C — Descanso intermedio**
- Rellano de 4 × 4 unidades — aquí ocurre la mecánica central
- 1 banco de madera empotrado
- Vista a ambos tramos de escalera desde aquí

**Zona D — Segundo tramo**
- 12 peldaños idénticos al primero
- El juego de luz cambia — fluorescente del segundo tramo más débil

**Zona E — Llegada**
- Pasillo del piso superior, diferente en material (WallMustardMat)
- Indica que se llegó a una zona nueva del edificio

### Objetos narrativos
- Fotografía clavada en la pared del rellano (inidentificable)
- Paraguas olvidado en la base de la escalera

---

## NIVEL 12 — Conflicto (Dos ecos contradictorios)
**Atmósfera:** fogColor `181A22`, density 0.028
**Material de pared:** ArchMat (gimnasio/laboratorio — más duro)

### Zonas

**Zona A — Acceso al gimnasio**
- Puerta metálica pesada, diferente a todas las anteriores
- Franja horizontal pintada en wrongness-red en la pared — decorativa, no activa

**Zona B — Gimnasio principal**
- Espacio más alto de todo el juego: 6 unidades de techo
- Suelo de parqué desgastado con líneas de cancha
- Espalderas de madera en las paredes laterales
- El espacio dividido en dos mitades visibles simultáneamente

**Zona C — Almacén de material**
- Sala lateral: colchonetas apiladas, aros, conos
- Donde el segundo eco tiene su punto de inicio

**Zona D — Salida elevada**
- Requiere usar una de las dos soluciones del puzzle
- Puerta en el nivel superior, accesible por una escalera fija lateral

### Objetos narrativos
- Cronómetro detenido colgado de la pared
- Listado de récords escolares con nombres borrados excepto "Aiden — 2003"

---

## NIVEL 13 — Verdad (Grabación única)
**Atmósfera:** fogColor `1C1A20`, density 0.022
**Material de pared:** WallRoseMat (variante fragmentada)

### Zonas

**Zona A — Umbral roto**
- El marco de la puerta está inclinado — la geometría empieza a fallar aquí
- El pasillo de acceso tiene una pared que no llega al techo

**Zona B — Aula de Lyra fragmentada**
- La misma planta del Nivel 10 pero con diferencias:
  - 3 pupitres flotan ligeramente sobre el suelo
  - La pizarrón tiene la mitad fuera de la pared
  - Las ventanas dan a negro, no al exterior
- Es el mismo espacio que el jugador recuerda, pero roto

**Zona C — Espacio de la conversación**
- Centro del aula, despejado
- Aquí se reproduce la grabación no elegida

**Zona D — Salida imposible**
- La puerta de salida está en una pared que no debería existir
- El jugador llega a ella después de resolver el puzzle

### Objetos narrativos
- El pupitre de memory-amber, volcado
- La mochila del Nivel 10 está en el centro del aula

---

## NIVEL 14 — Aceptación (Inversión)
**Atmósfera:** fogColor `0A0A0D`, density 0.014
**Sin material de pared — fragmentos en vacío**

### Zonas

**Zona A — Entrada al vacío**
- Un pasillo normal que termina en negro absoluto
- No hay suelo al cruzar el umbral — los fragmentos flotan

**Zona B — Fragmento izquierdo (eco)**
- Trozo de suelo escolar de 8 × 3 unidades flotando
- Locker, fluorescente, barandilla al borde — elementos reconocibles
- El eco comienza aquí su secuencia pregrabada

**Zona C — Fragmento derecho (Aiden)**
- Espejo del fragmento izquierdo
- Mismo suelo, mismo locker en espejo, misma barandilla
- La cámara ve ambos fragmentos desde el centro, en plano frontal

**Zona D — Punto de confluencia**
- Fragmento central donde ambas rutas se encuentran
- Los dos interruptores aquí — se activan simultáneamente
- Cuando se activan, el vacío gana un tono de `corridor-navy`

### Objetos narrativos
- Ninguno. El vacío es el objeto.
- Solo permanece el eco de Aiden — su silueta al final.

---

## NIVEL 15 — Integración
**Atmósfera:** fogColor `1C2430`, density 0.015
**Material de pared:** WallTealMat (idéntico al Nivel 1)

### Zonas

**Zona A — El mismo pasillo del Nivel 1**
- Geometría exactamente igual que el Pasillo A del Nivel 1
- Mismos lockers, mismo fluorescente que parpadea en el mismo lugar
- Diferencia: la puerta al fondo existe. En el Nivel 1 no había nada.

**Zona B — Zona de los tres puzzles**
- Tres espacios conectados al pasillo, sin puertas:
  1. Un hueco en la pared con un mecanismo simple (cap. I)
  2. Una escalera corta con una plataforma temporal (cap. II/III)
  3. Una sala pequeña con el eco como último uso del juego (cap. IV)
- Todos simples. Todos elegantes.

**Zona C — La salida**
- La puerta doble del edificio escolar
- Vista al exterior: blanco difuso, no hay detalle
- Aiden camina hacia ella. La cámara no corta.

### Objetos narrativos
- El abrigo en el locker del Nivel 1 ya no está
- El cuaderno del suelo está cerrado y apoyado contra la pared
- Los créditos aparecen proyectados sobre las paredes del pasillo en color memory-amber
