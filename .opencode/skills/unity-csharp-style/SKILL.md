---
name: unity-csharp-style
description: Aplica las convenciones de código C# para Unity usadas en Echoes of You: nomenclatura, organización de scripts, partial classes, eventos, serialización y patrones MonoBehaviour
license: MIT
compatibility: opencode
---

## Convenciones de código C# para Unity (Echoes of You)

### Nomenclatura
- `PascalCase` para clases, métodos públicos, propiedades públicas y eventos
- `camelCase` (con prefijo `_`) para campos privados y serializados (`[SerializeField]`): `_pressed`, `_timeUntilRelease`
- `camelCase` para parámetros de métodos y variables locales
- `UpperCamelCase` para constantes: `const string AnimatorParamSpeed = "Speed"`
- `PascalCase` + `Id` sufijo para IDs de shader cacheados: `static readonly int ColorId = Shader.PropertyToID("_Color")`

### Organización de scripts
- Orden dentro de la clase: eventos públicos (`public System.Action` / `public event Action<>`), constantes `const`, campos serializados `[SerializeField]`, campos públicos, campos privados `_cache`, propiedades públicas, método `Awake`, métodos propios, métodos de interfaz (ej: `IResettableLevelObject`)
- Usar `[Header("Categoría")]` para agrupar campos relacionados en el Inspector
- `[RequireComponent(typeof(...))]` para dependencias obligatorias
- Preferir `MaterialPropertyBlock` en lugar de crear materiales instanciados para cambios de color por objeto

### Partial classes
- Usar `partial class` para dividir lógica en archivos separados (ej: `PlayerController.cs`, `PlayerController_Gravity.cs`, `PlayerController_Animation.cs`)
- Cada archivo partial se enfoca en un aspecto específico (movimiento, gravedad, animación, visual)

### Eventos y comunicación
- Usar `System.Action` para callbacks simples: `public System.Action<float> OnLanded`
- Usar `public event Action<bool> PressedChanged` para eventos con suscripción
- Preferir eventos de C# sobre `UnityEvent` cuando no se necesita asignación en Inspector

### Serialización
- `[SerializeField]` para campos privados que deben aparecer en el Inspector
- Campos públicos para valores que otras clases necesitan leer/escribir directamente
- Usar `[Range(min, max)]` para valores acotados

### Patrones MonoBehaviour
- Preferir `Awake` para inicialización propia y caching de componentes (`GetComponent`)
- Usar `Start` para inicialización que depende de otros objetos ya despiertos
- `Update` / `FixedUpdate` / `LateUpdate` según corresponda al tipo de lógica
- Cachear referencias a componentes en lugar de llamar `GetComponent` cada frame

### UI (OnGUI)
- El proyecto usa interfaces OnGUI (`MainMenu`, `PauseMenu`, `TutorialHUD`, `GameHUD`) en lugar de uGUI
- Mantener la UI autocontenida en sus propios scripts con lógica de entrada y renderizado separadas
