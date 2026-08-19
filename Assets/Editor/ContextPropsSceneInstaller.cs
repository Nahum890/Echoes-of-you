using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Echoes.Interaction;

/// <summary>
/// Instala los props contextuales (catálogo ContextualPropCatalog) DENTRO de las
/// escenas Level_01..03 para que sean visibles en edit mode (no solo en runtime).
///
/// Convención de coordenadas (SPEC):
///  - El mundo se escala x2 en runtime (LevelGeometryScale) SOLO sobre los hijos
///    directos de los roots escalados (--- ENVIRONMENT --- / --- MECHANICS --- /
///    --- DRESSING --- / --- DECOR ---): localPosition.xz *= 2, localScale *= 2,
///    localPosition.y SIN cambios (LevelEnvironmentBootstrap.ScaleRootChildrenOnce).
///  - Los props NO se escalan como objetos: se dimensionan en metros reales
///    (PlayerHeight = 2.2). El spawner de runtime ancla cada prop al objeto real
///    de la escena (ya escalado) + offset.
///  - Por eso, al hornear: cada prop se hace HIJO DIRECTO del mismo root escalado
///    que su ancla, con localPosition = anchorLocal + offset/2 en xz (el /2 se
///    convierte en x2 al escalar runtime) e y = 0.1 + offset.y (y no se escala).
///    localScale = def.scale/2 para que al escalar x2 quede def.scale.
///  - Si el ancla NO está bajo un root escalado, el prop se hornea como objeto
///    raíz de escena con posición de mundo y escala def.scale (el ancla no se
///    mueve en runtime).
///
/// Idempotente: elimina los objetos "Prop_*" ya instalados (raíz o bajo roots
/// escalados) antes de reinstalar.
/// </summary>
public static class ContextPropsSceneInstaller
{
    static readonly string[] ScaledRootNames =
    {
        "--- ENVIRONMENT ---",
        "--- MECHANICS ---",
        "--- DRESSING ---",
        "--- DECOR ---"
    };

    [MenuItem("Echoes of You/Props/Install Contextual Props (Levels 1-3)")]
    public static void InstallAll()
    {
        int installed = 0, skipped = 0;
        InstallAllCore(out installed, out skipped);
        EditorUtility.DisplayDialog("Contextual Props", $"Instalados {installed} props en Levels 1-3.\nOmitidos: {skipped} (ancla no encontrada).", "OK");
    }

    /// <summary>Núcleo sin diálogo modal (invocable desde consola/MCP sin bloquear el editor).</summary>
    public static void InstallAllCore(out int installedTotal, out int skippedTotal)
    {
        installedTotal = 0;
        skippedTotal = 0;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[ContextPropsSceneInstaller] Abortado: salir de Play Mode primero.");
            return;
        }

        string originalScene = SceneManager.GetActiveScene().path;

        for (int level = 1; level <= 3; level++)
        {
            string scenePath = $"Assets/Scenes/Level_{level:D2}.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                Debug.LogWarning($"[ContextPropsSceneInstaller] Escena no encontrada: {scenePath}");
                continue;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int installed = InstallInScene(level, out int skipped);
            installedTotal += installed;
            skippedTotal += skipped;
            Debug.Log($"[ContextPropsSceneInstaller] {scenePath}: instalados {installed}, omitidos {skipped} (ancla ausente).");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(originalScene) && AssetDatabase.LoadAssetAtPath<SceneAsset>(originalScene) != null)
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);

        AssetDatabase.SaveAssets();
    }

    static int InstallInScene(int levelNum, out int skipped)
    {
        skipped = 0;

        ContextualPropCatalog.Def[] defs = ContextualPropCatalog.GetDefs(levelNum);
        if (defs == null || defs.Length == 0)
            return 0;

        RemoveInstalledProps();

        int installed = 0;
        for (int i = 0; i < defs.Length; i++)
        {
            ContextualPropCatalog.Def def = defs[i];
            if (string.IsNullOrEmpty(def.anchorName))
            {
                Debug.LogWarning($"[ContextPropsSceneInstaller] Nivel {levelNum}: '{def.name}' sin anchorName.");
                skipped++;
                continue;
            }

            GameObject anchor = FindAnchorInScene(def.anchorName);
            if (anchor == null)
            {
                Debug.LogWarning($"[ContextPropsSceneInstaller] Nivel {levelNum}: ancla '{def.anchorName}' NO existe en la escena. Prop '{def.name}' omitido.");
                skipped++;
                continue;
            }

            Transform scaledRoot = FindScaledRoot(anchor.transform);
            Vector3 anchorPos = anchor.transform.position;

            GameObject go = CreatePropGameObject(def);
            if (go == null)
            {
                skipped++;
                continue;
            }

            go.name = def.name;
            if (scaledRoot != null)
            {
                // Prop como hijo directo del root escalado: en runtime ScaleRootChildrenOnce
                // multiplica localPosition.xz y localScale x2 (y se queda igual).
                go.transform.SetParent(scaledRoot, false);
                Vector3 local = scaledRoot.InverseTransformPoint(anchorPos);
                go.transform.localPosition = new Vector3(
                    local.x + def.offset.x * 0.5f,
                    0.1f + def.offset.y,
                    local.z + def.offset.z * 0.5f);
                go.transform.localScale = def.scale * 0.5f;
                go.transform.localRotation = Quaternion.Euler(def.rotation);
            }
            else
            {
                // Ancla no escalada en runtime: prop raíz de escena con posición de mundo.
                go.transform.position = new Vector3(
                    anchorPos.x + def.offset.x,
                    0.1f + def.offset.y,
                    anchorPos.z + def.offset.z);
                go.transform.localScale = def.scale;
                go.transform.rotation = Quaternion.Euler(def.rotation);
            }

            var io = go.AddComponent<InteractableObject>();
            io.SetContext(def.displayName, def.commentKey, def.isLyraArtifact, def.triggerRadius, def.category);

            if (def.category == InteractableCategory.Ambient)
                go.AddComponent<AmbientReaction>();
            else if (def.category == InteractableCategory.Gameplay)
                go.AddComponent<GameplayInteraction>();

            EditorUtility.SetDirty(go);
            installed++;
        }

        return installed;
    }

    /// <summary>
    /// Busca el ancla también en objetos INACTIVOS: algunos módulos de las escenas
    /// (p. ej. PlacaJugador_Corredor en Level_02) están inactivos en edit mode y se
    /// activan en runtime; GameObject.Find solo encuentra objetos activos.
    /// </summary>
    static GameObject FindAnchorInScene(string name)
    {
        Transform[] all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].name == name)
                return all[i].gameObject;
        }
        return null;
    }

    /// <summary>Devuelve el root escalado del que el objeto es descendiente (o null).</summary>
    static Transform FindScaledRoot(Transform target)
    {
        Transform current = target;
        while (current != null)
        {
            Transform parent = current.parent;
            if (parent == null)
                return null;
            if (IsScaledRootName(parent.name))
                return parent;
            current = parent;
        }
        return null;
    }

    static bool IsScaledRootName(string name)
    {
        for (int i = 0; i < ScaledRootNames.Length; i++)
        {
            if (ScaledRootNames[i] == name)
                return true;
        }
        return false;
    }

    static GameObject CreatePropGameObject(ContextualPropCatalog.Def def)
    {
        if (!string.IsNullOrEmpty(def.prefabPath))
        {
            GameObject prefab = Resources.Load<GameObject>(def.prefabPath);
            if (prefab != null)
            {
                var inst = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (inst != null)
                    return inst;
            }
            Debug.LogWarning($"[ContextPropsSceneInstaller] Prefab Resources/{def.prefabPath} no encontrado; usando cubo fallback.");
        }

        // Fallback: cubo temático con el color de su categoría (collider sólido;
        // la proximidad la detecta el SphereCollider trigger de InteractableObject).
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var rend = go.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = def.category switch
            {
                InteractableCategory.Gameplay => new Color(0.55f, 0.75f, 0.9f),
                InteractableCategory.Ambient => new Color(0.6f, 0.62f, 0.5f),
                _ => new Color(1f, 0.72f, 0.22f)
            };
            rend.sharedMaterial = mat;
        }
        return go;
    }

    static void RemoveInstalledProps()
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] == null)
                continue;

            if (roots[i].name.StartsWith("Prop_"))
            {
                Object.DestroyImmediate(roots[i]);
                continue;
            }

            if (!IsScaledRootName(roots[i].name))
                continue;

            RemovePropsRecursive(roots[i].transform);
        }
    }

    static void RemovePropsRecursive(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child == null)
                continue;

            if (child.name.StartsWith("Prop_"))
            {
                Object.DestroyImmediate(child.gameObject);
            }
            else
            {
                RemovePropsRecursive(child);
            }
        }
    }
}