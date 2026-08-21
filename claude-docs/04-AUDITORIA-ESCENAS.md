# 04 — Auditoría de escenas

Auditoría de los 15 niveles ejecutada en Unity batch mode el **2026-08-20**.
Todo ✅ **verificado**. Los números son el estado **antes** de la reparación.

## Tabla por nivel

| Nivel | Stray | NULL | `'Lit'` | Luces | Emisivos | Fog d |
|---|---|---|---|---|---|---|
| Level_01 | 4 | 5 | 0 | 42 | 9 | 0.008 |
| Level_02 | 2 | 4 | 0 | 30 | 42 | 0.008 |
| Level_03 | 3 | 7 | 0 | 38 | 57 | 0.008 |
| Level_04 | 0 | 9 | 8 | 11 | 10 | 0.010 |
| Level_05 | 0 | 9 | 8 | 9 | 7 | 0.010 |
| Level_06 | 0 | 7 | 8 | 17 | 48 | 0.012 |
| Level_07 | 0 | 7 | 11 | 11 | 14 | 0.012 |
| Level_08 | 0 | 4 | 11 | 14 | 52 | 0.010 |
| Level_09 | 0 | 4 | 11 | 16 | 6 | 0.012 |
| Level_10 | 0 | 5 | 11 | 16 | 56 | 0.015 |
| Level_11 | 0 | 7 | 11 | 12 | 7 | 0.015 |
| Level_12 | 0 | 6 | 11 | 12 | 29 | 0.020 |
| Level_13 | 0 | 6 | 11 | 13 | 30 | 0.020 |
| Level_14 | 0 | 6 | 11 | 12 | 17 | 0.002 |
| Level_15 | 0 | 6 | 11 | 14 | 34 | 0.002 |
| **Total** | **9** | **92** | **123** | | | |

Se lee bien la división: los niveles **1-3** (capítulo I, hechos con otro
builder) solo tenían suelos y puertas sin material y sí tenían los fog volumes
descolocados. Del **4 al 15** además faltaban las paredes, 8 u 11 por nivel.

## Hallazgo 1 — las dos superficies principales no tenían material

```
 92 renderers con material NULL
    Floor x11, CorridorFloor x9, ClassroomFloor x9, CornerFloor x7,
    ResonanceBase x4, PatioFloor x2, Rooftop_L/R x4, StartPlatform x2,
    ExitPlatform x2, PuertaAula x2, Fence_Line x2...

123 renderers con 'Lit'  (el material gris por defecto de URP)
    Wall_Front, Wall_L, Wall_R  en Entrada, CorridorCentral, Hall_Salida...
```

**Los suelos sin material y las paredes con el gris por defecto.** Las dos
superficies que más pantalla ocupan. Ésta era la razón de fondo de que los
niveles no se leyeran como una escuela y de que se vieran casi negros: no era
falta de luz, era geometría sin superficie.

Corregido: **215 renderers** asignados por patrón de nombre, respetando los que
ya estaban bien puestos.

## Hallazgo 2 — bug de locale en los fog volumes

Los volúmenes de niebla de los niveles 1-3 estaban a ~100 km del origen:

```
FogVolume_Entrada       @ (0, 2, 38740)      → 38.740
FogVolume_AulaDerecha   @ (11, 2, 40012)     → 40.012
FogVolume_Hall_Estatua  @ (0, 3, 50033)      → 50.033
FogVolume_AulaAusente   @ (0, 2, 60035)      → 60.035
FogVolume_PasilloB      @ (0, 2, 80020)      → 80.020
FogVolume_PasilloA      @ (0, 2, 100001)     → 100.001
```

Todas son **exactamente ×1000**: alguien formateó o parseó `"100.001"` en una
locale donde el punto es separador de millares. Los volúmenes de niebla por
capítulo llevaban sin hacer nada desde entonces.

Efecto colateral: los `bounds` de Level_01 eran de **200.018 unidades**, lo que
hacía inútil cualquier encuadre automático de cámara.

❓ **Sin verificar:** no se localizó *qué* código produce esa serialización. El
pase repara el síntoma, no la causa. Si algo vuelve a generar esos volúmenes,
volverán a salir mal.

## Hallazgo 3 — el bloom estaba invertido respecto a su propio spec

Las 15 escenas tenían:

```
Bloom(intensidad 0.9, umbral 0.25)
```

`RULE-PST-G01` de `POST_PROCESS_SPEC.md` manda **intensidad 0.25, umbral 0.90**.
Están intercambiados. Con umbral 0.25 casi toda la imagen entra en bloom, se lava
y roba contraste justo donde hace falta. Corregido a los valores del spec.

## Hallazgo 4 — iluminación

```
Point    x9-x41   intensidad 0.55-1.8   rango 0-28   sombras None (mayoría)
Directional x1    intensidad 0.85       sombras Hard
Ambient  Flat     Ch I: RGBA(0.059, 0.078, 0.102)   Ch III: RGBA(0.078, 0.055, 0.055)
                  intensity 0.15
Volume   ColorAdjustments(exposure -0.5, contrast 15, saturation -8)
         Tonemapping: None
```

- **35 luces puntuales tenían rango 0** y no iluminaban nada. Corregidas a 8 m.
- ⚠️ **`Tonemapping: None` con `postExposure -0.5`** es lo que aplasta las
  sombras a negro puro en vez de comprimirlas con una curva. Está así por
  `RULE-PST-G01`. **No se tocó**: tras arreglar los 215 materiales el problema
  de visibilidad se resolvió sin necesidad. Queda como palanca si vuelve a hacer
  falta.
- `Mat_Fluorescent` no tenía emisión: los tubos eran geometría gris con una
  point light invisible al lado — se veía el charco de luz pero no la fuente.
  Ahora emite blanco verdoso a 2.2 (por encima de 1 para que el bloom lo recoja).

## Resultado de la reparación

```
215 materiales asignados · 9 objetos recolocados · 35 luces · 15 bloom
```

Sin avisos: ninguna coordenada resistió la división por 1000 y ningún material
del mapa faltaba.

## ⚠️ Límite de esta verificación

**Solo se pudo comprobar visualmente Level_01.** Los niveles 4-15 dan `bounds`
de ~409 × 262 × 257 unidades con el centro en `y = -68`, lo que deja cualquier
cámara automática fuera de la geometría.

O esos niveles tienen geometría dispersa lejos — por debajo del umbral de 500
del pase, así que no se tocó — o hay algo raro en su composición. **No se sabe
cómo se ven del 2 al 15.** Que los pases corrieran sin errores no es lo mismo
que que se vean bien.
