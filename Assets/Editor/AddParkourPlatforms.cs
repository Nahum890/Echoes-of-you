#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool: añade plataformas de parkour ligero a los niveles existentes.
/// Las plataformas son OPCIONALES — nunca bloquean la ruta principal del puzzle.
/// Menú: Echoes > Add Parkour Platforms
/// </summary>
public static class AddParkourPlatforms
{
    // Material compartido para plataformas parkour
    static Material _parkourMat;

    [MenuItem("Echoes/Add Parkour Platforms To Current Scene")]
    public static void AddToCurrentScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Crear o encontrar el parent
        GameObject parkourRoot = GameObject.Find("ParkourElements");
        if (parkourRoot == null)
        {
            parkourRoot = new GameObject("ParkourElements");
            Undo.RegisterCreatedObjectUndo(parkourRoot, "Add Parkour Platforms");
        }

        switch (sceneName)
        {
            case "Level_01":
                AddLevel01Parkour(parkourRoot.transform);
                break;
            case "Level_02":
                AddLevel02Parkour(parkourRoot.transform);
                break;
            case "Level_03":
                AddLevel03Parkour(parkourRoot.transform);
                break;
            case "Level_04":
                AddLevel04Parkour(parkourRoot.transform);
                break;
            case "Level_05":
                AddLevel05Parkour(parkourRoot.transform);
                break;
            case "Level_06":
                AddLevel06Parkour(parkourRoot.transform);
                break;
            case "Level_07":
                AddLevel07Parkour(parkourRoot.transform);
                break;
            case "Level_08":
                AddLevel08Parkour(parkourRoot.transform);
                break;
            default:
                Debug.Log($"AddParkourPlatforms: sin configuración parkour para '{sceneName}'");
                return;
        }

        Debug.Log($"AddParkourPlatforms: parkour ligero añadido a '{sceneName}'");
    }

    // ========================================================================
    // Level 01 - Tutorial: un escalón sutil para enseñar que se puede saltar
    // ========================================================================
    static void AddLevel01Parkour(Transform parent)
    {
        // Escalón lateral como demostración de salto
        // No afecta la ruta principal (placa → puerta → salida)
        CreateParkourPlatform(parent, "Step_Demo",
            new Vector3(4.5f, 0.6f, 6.0f),     // Al lado de Walk_A
            new Vector3(2.5f, 0.5f, 2.5f),
            "Escalón de demostración — el jugador descubre que puede saltar");
    }

    // ========================================================================
    // Level 02 - Puente: plataforma elevada con vista al puzzle
    // ========================================================================
    static void AddLevel02Parkour(Transform parent)
    {
        // Mirador: plataforma elevada que da vista panorámica del puente
        CreateParkourPlatform(parent, "Lookout",
            new Vector3(5.0f, 1.2f, 5.0f),
            new Vector3(3.0f, 0.5f, 3.0f),
            "Mirador opcional — vista del puente y la placa");

        // Escalón de acceso
        CreateParkourPlatform(parent, "Lookout_Step",
            new Vector3(3.5f, 0.6f, 3.0f),
            new Vector3(2.0f, 0.5f, 2.0f),
            "Escalón para acceder al mirador");
    }

    // ========================================================================
    // Level 03 - Dos Decisiones: atajos entre ramales
    // ========================================================================
    static void AddLevel03Parkour(Transform parent)
    {
        // Plataforma central elevada — atajo visual entre ambos ramales
        CreateParkourPlatform(parent, "Central_Overlook",
            new Vector3(0f, 1.4f, 5.0f),
            new Vector3(3.0f, 0.5f, 3.0f),
            "Vista central elevada — el jugador puede ver ambas placas");

        // Escalón izquierdo
        CreateParkourPlatform(parent, "Step_Left",
            new Vector3(-2.0f, 0.7f, 4.5f),
            new Vector3(2.0f, 0.5f, 2.0f),
            "Acceso izquierdo al overlook");

        // Escalón derecho
        CreateParkourPlatform(parent, "Step_Right",
            new Vector3(2.0f, 0.7f, 4.5f),
            new Vector3(2.0f, 0.5f, 2.0f),
            "Acceso derecho al overlook");
    }

    // ========================================================================
    // Level 04 - Twist: mirador para leer la secuencia
    // ========================================================================
    static void AddLevel04Parkour(Transform parent)
    {
        // Mirador elevado para ver toda la secuencia de placas
        CreateParkourPlatform(parent, "Sequence_Lookout",
            new Vector3(-6.0f, 1.6f, 12.0f),
            new Vector3(4.0f, 0.5f, 4.0f),
            "Mirador estratégico — ver el orden de la secuencia");

        // Escalón de acceso
        CreateParkourPlatform(parent, "Lookout_Step_1",
            new Vector3(-4.5f, 0.8f, 10.5f),
            new Vector3(2.5f, 0.5f, 2.5f),
            "Escalón 1 al mirador");
    }

    // ========================================================================
    // Level 05 - Precisión: atajo escalonado
    // ========================================================================
    static void AddLevel05Parkour(Transform parent)
    {
        // Serie de 3 plataformas escalonadas como atajo lateral
        CreateParkourPlatform(parent, "Shortcut_Step_1",
            new Vector3(5.0f, 0.8f, 8.0f),
            new Vector3(3.0f, 0.5f, 3.0f),
            "Atajo escalonado — paso 1");

        CreateParkourPlatform(parent, "Shortcut_Step_2",
            new Vector3(5.0f, 1.6f, 12.0f),
            new Vector3(3.0f, 0.5f, 3.0f),
            "Atajo escalonado — paso 2");

        CreateParkourPlatform(parent, "Shortcut_Step_3",
            new Vector3(5.0f, 2.4f, 16.0f),
            new Vector3(3.0f, 0.5f, 3.0f),
            "Atajo escalonado — paso 3 (punto más alto)");
    }

    // ========================================================================
    // Level 06 - Final: plataformas de celebración
    // ========================================================================
    static void AddLevel06Parkour(Transform parent)
    {
        // Plataformas laterales elevadas que dan vista al nivel completo
        CreateParkourPlatform(parent, "Victory_Left",
            new Vector3(-7.0f, 1.2f, 10.0f),
            new Vector3(3.0f, 0.5f, 3.0f),
            "Plataforma de victoria — vista izquierda");

        CreateParkourPlatform(parent, "Victory_Right",
            new Vector3(7.0f, 1.2f, 10.0f),
            new Vector3(3.0f, 0.5f, 3.0f),
            "Plataforma de victoria — vista derecha");
    }

    // ========================================================================
    // Level 07 & 08: parkour de exploración
    // ========================================================================
    static void AddLevel07Parkour(Transform parent)
    {
        CreateParkourPlatform(parent, "Explore_High",
            new Vector3(0f, 2.0f, 15.0f),
            new Vector3(3.5f, 0.5f, 3.5f),
            "Plataforma alta de exploración");

        CreateParkourPlatform(parent, "Explore_Step",
            new Vector3(-2.0f, 1.0f, 13.0f),
            new Vector3(2.5f, 0.5f, 2.5f),
            "Escalón de acceso");
    }

    static void AddLevel08Parkour(Transform parent)
    {
        CreateParkourPlatform(parent, "Final_Overlook",
            new Vector3(0f, 1.8f, 8.0f),
            new Vector3(4.0f, 0.5f, 4.0f),
            "Vista panorámica final");

        CreateParkourPlatform(parent, "Final_Step",
            new Vector3(2.0f, 0.9f, 6.0f),
            new Vector3(2.0f, 0.5f, 2.0f),
            "Escalón de acceso");
    }

    // ========================================================================
    // Helper
    // ========================================================================
    static void CreateParkourPlatform(Transform parent, string name, Vector3 position, Vector3 scale, string description)
    {
        // Evitar duplicados
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            Debug.Log($"  '{name}' ya existe, actualizando posición");
            existing.position = position;
            existing.localScale = scale;
            return;
        }

        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = name;
        platform.transform.SetParent(parent, false);
        platform.transform.position = position;
        platform.transform.localScale = scale;
        platform.layer = LayerMask.NameToLayer("Ground");

        // Material con tono ligeramente diferente al suelo normal
        Renderer renderer = platform.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (_parkourMat == null)
            {
                _parkourMat = new Material(Shader.Find("Standard"));
                _parkourMat.color = new Color(0.22f, 0.26f, 0.32f, 1f); // Gris-azul más claro que el suelo normal
                _parkourMat.EnableKeyword("_EMISSION");
                _parkourMat.SetColor("_EmissionColor", new Color(0.08f, 0.12f, 0.18f, 1f)); // Glow sutil
                _parkourMat.SetFloat("_Metallic", 0.3f);
                _parkourMat.SetFloat("_Glossiness", 0.55f);
            }
            renderer.sharedMaterial = _parkourMat;
        }

        // Añadir marcador visual
        platform.AddComponent<ParkourPlatformMarker>();

        Undo.RegisterCreatedObjectUndo(platform, $"Add Parkour Platform: {name}");
        Debug.Log($"  + {name} @ {position} scale {scale} — {description}");
    }
}
#endif
