---
name: vector-math-puzzles
description: Operaciones de matemática vectorial en Unity 3D aplicadas a mecánicas de puzzle en Echoes of You: Vectores, movimiento del eco, interpolación, gravedad, detección de presión y navegación 3D
license: MIT
compatibility: opencode
---

## Vector Math para Puzzles 3D (Echoes of You)

### Vector3 fundamentals
- `Vector3.up`, `down`, `forward`, `back`, `left`, `right` — ejes cardinales
- `Vector3.zero`, `one` — constantes útiles para reset/resize
- `Vector3.Distance(a, b)` — distancia entre dos puntos (útil para detección de proximidad en placas)
- `Vector3.Dot(a, b)` — producto punto; útil para determinar si dos direcciones apuntan al mismo sentido
- `Vector3.Cross(a, b)` — producto cruz; útil para calcular perpendiculares (rotaciones de cámara, ejes de gravedad)

### Movimiento del eco (grabación y playback)
- Los `RecordFrame` almacenan `Vector3` position y `Quaternion` rotation por frame
- Para interpolación suave entre frames: `Vector3.Lerp(a, b, t)` y `Quaternion.Slerp(a, b, t)`
- Diferencia entre frames: `deltaPosition = currentFrame.position - previousFrame.position`
- Dirección de movimiento: `direction = deltaPosition.normalized`
- Velocidad: `speed = deltaPosition.magnitude / Time.deltaTime`

### Gravedad y direcciones (PlayerController_Gravity)
- Dirección de gravedad configurable: `Vector3 defaultGravityDirection = Vector3.down`
- `gravityBlendSpeed` para transiciones suaves entre direcciones de gravedad
- `Vector3.Slerp` para rotar suavemente la dirección de gravedad
- `Physics.gravity = gravityDirection * gravityStrength`

### Detección de placas de presión
- Uso de `Physics.OverlapBox` o `Physics.OverlapSphere` con `_overlapBuffer` para detección sin alloc
- `Vector3.Distance(plateCenter, objectPosition) < threshold` para verificación radial
- Comparación de tags: `collider.CompareTag("Player")` vs `collider.CompareTag("Echo")`

### Puzzle navigation / path hints
- Array de `Vector3[]` waypoints para rutas de guía visual
- Posicionamiento relativo: `waypoints[i] + Vector3.up * 0.5f`
- Escalado uniforme: `Vector3.one * particleSize`
- Dirección entre waypoints: `next - current` para orientar elementos de ruta

### Pushable blocks (Kinetic)
- Reseteo de estado: `Vector3.zero` para velocidades lineales y angulares
- `transform.position` y `transform.rotation` guardados en `_startPosition`/`_startRotation`
- Para limitar movimiento: `Vector3.Scale` o clamping por eje

### Raycasting y detección
- `Physics.Raycast(origin, direction, out hit, maxDistance, layerMask)` para línea de visión en puzzles
- `direction.normalized` para rayo en dirección específica
- Layer masks bit a bit: `1 << LayerMask.NameToLayer("Default")`

### Conversiones comunes
- `Quaternion.LookRotation(direction)` — rotación que mira hacia una dirección
- `Transform.InverseTransformPoint(worldPos)` — convertir world a local (útil para placas rotadas)
- `Transform.TransformPoint(localPos)` — convertir local a world
- `Mathf.Abs(Vector3.Dot(a.normalized, b.normalized))` — test de paralelismo entre vectores
