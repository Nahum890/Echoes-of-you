using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Decorador narrativo por nivel (§3.6 del pase de arte técnico).
///
/// Coloca EXACTAMENTE 1 prop narrativo memory-amber #FFBF00 por nivel
/// (VAL-ENV-001) más su micro-escena asociada (MIC-001..004), según la
/// Tabla 8.2 de ENVIRONMENT_STORYTELLING.md.
///
/// La densidad ambiental de props NO se toca aquí: eso ya lo cubre el
/// Environment Pass (EnvironmentPassPlacementEngine). Este pase solo añade
/// la capa narrativa bajo la raíz "--- NARRATIVE ---" (idempotente: se
/// regenera completa en cada ejecución).
///
/// SUSTITUCIONES: los 10 prefabs amber de L06–L15 declarados en la Tabla 8.2
/// (Prop_BeakerAmber, Prop_KeyAmber, …) NO existen en Assets/Prefabs. Se
/// intenta primero el prefab exacto (si un artista lo añade, se usa solo) y
/// si falta se usa el sustituto tintado amber de la tabla de fallbacks.
/// Cada sustitución queda en el log y en el reporte del pase.
/// </summary>
public static class EchoesPropDecorator
{
    const string NarrativeRootName = "--- NARRATIVE ---";
    const string PropsFolder = "Assets/Prefabs/Props";
    const string NarrativeFolder = "Assets/Prefabs/Props/Narrative";
    const string ArchitectureFolder = "Assets/Prefabs/Architecture";
    const string DecalsFolder = "Assets/Prefabs/Decals";

    struct NarrativeEntry
    {
        public string prefab;      // nombre canónico de la Tabla 8.2
        public string fallback;    // sustituto existente si el canónico falta
        public string module;      // instancia de módulo declarada en el spec
        public Vector3 propPos;    // posición del prop (Tabla 8.2, ±0.01 m)
        public string mic;         // micro-escena asociada
    }

    // ENVIRONMENT_STORYTELLING.md Tabla 8.2. La Y de Level_03 (0.0) viola
    // PROP_GRAMMAR Tabla 8.3 (Y ∈ [0.5, 1.8]) — se eleva a 0.5.
    static readonly Dictionary<int, NarrativeEntry> Table = new()
    {
        [1]  = E("Prop_Coat",             null,                "SchoolCorridor_01",         0f, 1f, 6f,      "MIC-001"),
        [2]  = E("Prop_Notebook",         null,                "SchoolClassroom_02",        3.5f, 0.75f, 3.5f, "MIC-002"),
        [3]  = E("MochilaLyra",           null,                "SchoolLyraClassroom_01",    4f, 0.5f, 4f,    "MIC-002"),
        [4]  = E("Prop_Stopwatch",        null,                "SchoolGym_01",              0f, 1f, 12f,     "MIC-003"),
        [5]  = E("Prop_RecordsBoard",     null,                "SchoolHall_01",             0f, 1.5f, 0f,    "MIC-004"),
        [6]  = E("Prop_BeakerAmber",      "TazaCafe",          "SchoolLab_01",              5f, 0.9f, 5f,    "MIC-004"),
        [7]  = E("Prop_ClockHandAmber",   "Prop_StoppedClock", "SchoolLiminalClassroom_01", 6f, 2.5f, 6f,    "MIC-003"),
        [8]  = E("Prop_DrawingAmber",     "Prop_ChalkDrawing", "SchoolClassroom_03",        2.5f, 0.75f, 3.5f, "MIC-002"),
        [9]  = E("Prop_KeyAmber",         "Prop_LibraryStamp", "SchoolMaintenanceCorridor", 0f, 0.5f, 9f,    "MIC-001"),
        [10] = E("Prop_CompassAmber",     "Cronometro",        "SchoolLyraClassroom_02",    5f, 0.75f, 5f,   "MIC-002"),
        [11] = E("Prop_LocketAmber",      "Prop_PhotoFrame",   "SchoolEmergencyCorridor",   0f, 1f, 9f,      "MIC-001"),
        [12] = E("Prop_MirrorFrameAmber", "VentanaMarco",      "SchoolOffice_01",           4f, 1.2f, 4f,    "MIC-004"),
        [13] = E("Prop_HourglassAmber",   "Cronometro",        "SchoolLibrary_01",          7f, 1f, 8f,      "MIC-003"),
        [14] = E("Prop_RibbonAmber",      "Prop_DriedFlowers", "SchoolCourtyard_01",        12f, 0.5f, 12f,  "MIC-002"),
        [15] = E("Prop_LetterAmber",      "Prop_BlankBook",    "SchoolLiminalClassroom_02", 6f, 0.75f, 6f,   "MIC-002"),
    };

    static NarrativeEntry E(string prefab, string fallback, string module, float x, float y, float z, string mic)
        => new() { prefab = prefab, fallback = fallback, module = module, propPos = new Vector3(x, y, z), mic = mic };

    /// <summary>
    /// Decora un nivel abierto en el editor. Devuelve un resumen de una línea
    /// para el reporte del pase.
    /// </summary>
    public static string DecorateLevel(int levelNum)
    {
        if (!Table.TryGetValue(levelNum, out NarrativeEntry entry))
            return "sin entrada narrativa";

        GameObject oldRoot = GameObject.Find(NarrativeRootName);
        if (oldRoot != null)
            Object.DestroyImmediate(oldRoot);
        GameObject root = new GameObject(NarrativeRootName);

        // Resolución de posición: las coordenadas de la Tabla 8.2 son locales
        // al módulo declarado. Si el módulo existe en la escena, se colocan
        // relativas a él; si no, se usan como posición de mundo (y se anota).
        // NOTA post-greybox: los blueprints nuevos nombran módulos de forma
        // semántica (ZonaA_*, Placa*), así que el fallback de mundo es la vía
        // habitual — el snap al suelo de abajo evita props flotando/enterrados.
        Transform module = FindModule(entry.module);
        Vector3 worldPos = module != null
            ? module.TransformPoint(entry.propPos)
            : entry.propPos;
        string anchorNote = module != null ? $"módulo '{module.name}'" : "MUNDO (módulo no encontrado)";

        // Snap al suelo: conserva la altura de la Tabla 8.2 como offset sobre
        // el piso real encontrado por raycast.
        if (Physics.Raycast(new Vector3(worldPos.x, worldPos.y + 10f, worldPos.z),
                Vector3.down, out RaycastHit hit, 40f))
        {
            worldPos.y = hit.point.y + entry.propPos.y;
        }

        // Prop narrativo amber — exactamente 1 por nivel (VAL-ENV-001)
        string usedName = entry.prefab;
        GameObject prefab = LoadPrefab(entry.prefab);
        bool substituted = false;
        if (prefab == null && entry.fallback != null)
        {
            prefab = LoadPrefab(entry.fallback);
            usedName = entry.fallback;
            substituted = true;
        }

        if (prefab == null)
            return $"FALLO: ni '{entry.prefab}' ni su fallback existen";

        GameObject prop = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
        prop.name = entry.prefab; // conserva el nombre canónico del spec
        prop.transform.position = worldPos;
        ApplyAmberIdentity(prop, entry.prefab, isLyra: true);

        // Micro-escena asociada (RULE-ENV-003: 1–3 por nivel; colocamos 1)
        int micPieces = SpawnMicroScene(entry.mic, root.transform, worldPos);

        string subNote = substituted ? $" [SUSTITUTO: {usedName}]" : "";
        return $"{entry.prefab}{subNote} @ {anchorNote} + {entry.mic} ({micPieces} piezas)";
    }

    // ─── Micro-escenas (ENVIRONMENT_STORYTELLING Tabla 8.1) ──────────────
    // El spec define composición en prosa sin offsets: los offsets locales de
    // aquí son deterministas y documentados (mismo resultado en cada build).

    static int SpawnMicroScene(string micId, Transform parent, Vector3 anchor)
    {
        GameObject micRoot = new GameObject(micId);
        micRoot.transform.SetParent(parent, false);
        micRoot.transform.position = anchor;
        int count = 0;

        switch (micId)
        {
            case "MIC-001": // Evacuación apresurada — faded-mustard
                count += Place(micRoot, "SillaEscolar", new Vector3(1.2f, 0f, 0.6f), new Vector3(95f, 20f, 0f), "faded-mustard");
                count += Place(micRoot, "SillaEscolar", new Vector3(-0.9f, 0f, 1.1f), new Vector3(100f, 160f, 0f), "faded-mustard");
                count += Place(micRoot, "SillaEscolar", new Vector3(0.4f, 0f, -1.3f), new Vector3(92f, 275f, 0f), "faded-mustard");
                count += Place(micRoot, "Mochila", new Vector3(-0.5f, 0f, -0.4f), new Vector3(0f, 30f, 80f), null);
                count += Place(micRoot, "dec_papel_suelo", new Vector3(0.8f, 0.01f, 0.9f), new Vector3(0f, 15f, 0f), null);
                count += Place(micRoot, "dec_papel_suelo", new Vector3(-1.1f, 0.01f, 0.2f), new Vector3(0f, 210f, 0f), null);
                break;

            case "MIC-002": // Rincón de Lyra — memory-amber
                count += Place(micRoot, "PupitreDoble", new Vector3(0.9f, 0f, 0.9f), new Vector3(0f, 195f, 0f), null);
                count += Place(micRoot, "MochilaLyra", new Vector3(1.15f, 0f, 0.55f), new Vector3(0f, 40f, 0f), "memory-amber");
                count += Place(micRoot, "Prop_Notebook", new Vector3(0.85f, 0.78f, 0.95f), new Vector3(0f, 12f, 0f), "memory-amber");
                break;

            case "MIC-003": // Lección interrumpida — dusty-rose
                count += Place(micRoot, "Pizarra", new Vector3(0f, 1.4f, 2.0f), Vector3.zero, "dusty-rose");
                count += Place(micRoot, "dec_tiza_borrada", new Vector3(0.3f, 0.01f, 1.6f), Vector3.zero, null);
                count += Place(micRoot, "RelojPared", new Vector3(1.1f, 2.1f, 2.0f), Vector3.zero, "dusty-rose"); // detenido 03:14
                break;

            case "MIC-004": // Archivo olvidado — institutional-teal
                count += Place(micRoot, "EstanteriaCerrada", new Vector3(1.4f, 0f, 0.8f), new Vector3(0f, 90f, 0f), "institutional-teal");
                count += Place(micRoot, "Prop_AttendanceList", new Vector3(0.9f, 0f, 0.2f), new Vector3(0f, 65f, 4f), null);
                break;
        }

        if (count == 0)
            Object.DestroyImmediate(micRoot);
        return count;
    }

    static int Place(GameObject parent, string prefabName, Vector3 localOffset, Vector3 euler, string token)
    {
        GameObject prefab = LoadPrefab(prefabName);
        if (prefab == null)
        {
            Debug.LogWarning($"[PropDecorator] Prefab de micro-escena no encontrado: {prefabName}");
            return 0;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
        instance.transform.localPosition = localOffset;
        instance.transform.localRotation = Quaternion.Euler(euler);

        if (token != null)
            TintRenderers(instance, token);
        return 1;
    }

    static void ApplyAmberIdentity(GameObject prop, string canonicalName, bool isLyra)
    {
        TintRenderers(prop, "memory-amber");

        NarrativeProp narrative = prop.GetComponent<NarrativeProp>();
        if (narrative == null)
            narrative = prop.AddComponent<NarrativeProp>();
        narrative.propName = canonicalName;
        narrative.isLyraProp = isLyra;
        narrative.chapterHintColor = EchoesMaterialLibrary.HexColor("FFBF00");
        narrative.hintIntensity = 1.2f; // Emission 1.2 (RULE-MAT-001)
        narrative.lightRange = 3f;
    }

    static void TintRenderers(GameObject go, string token)
    {
        Material mat = EchoesMaterialLibrary.GetMaterial(token);
        foreach (Renderer rendererRef in go.GetComponentsInChildren<Renderer>(true))
        {
            var mats = rendererRef.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            rendererRef.sharedMaterials = mats;
        }
    }

    static Transform FindModule(string moduleName)
    {
        // Búsqueda exacta y por prefijo (los builders sufijan instancias).
        GameObject exact = GameObject.Find(moduleName);
        if (exact != null)
            return exact.transform;

        GameObject envRoot = GameObject.Find("--- ENVIRONMENT ---");
        if (envRoot == null)
            return null;

        // Alias mapping for vertical slice modules
        var aliases = new List<string> { moduleName };
        if (moduleName.Contains("SchoolCorridor"))
            aliases.AddRange(new[] { "PasilloA", "PasilloB", "CorredorCentral", "CorredorBifurcacion", "RamaIzquierda", "RamaDerecha" });
        else if (moduleName.Contains("SchoolClassroom"))
            aliases.AddRange(new[] { "AulaIzquierda", "AulaDerecha", "AulaAusente", "AulaEco" });
        else if (moduleName.Contains("SchoolLyraClassroom"))
            aliases.AddRange(new[] { "AulaLyra", "AulaAusente" });
        else if (moduleName.Contains("SchoolHall"))
            aliases.AddRange(new[] { "Entrada", "Hall_Salida", "Hall_Estatua" });

        foreach (var alias in aliases)
        {
            Transform found = envRoot.transform.Find(alias);
            if (found != null) return found;

            string baseName = alias;
            int suffix = alias.LastIndexOf('_');
            if (suffix > 0 && int.TryParse(alias.Substring(suffix + 1), out _))
                baseName = alias.Substring(0, suffix);

            foreach (Transform child in envRoot.transform)
            {
                if (child.name == alias || child.name.StartsWith(baseName))
                    return child;
            }
        }

        return null;
    }

    static GameObject LoadPrefab(string name)
    {
        foreach (string folder in new[] { NarrativeFolder, PropsFolder, ArchitectureFolder, DecalsFolder })
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/{name}.prefab");
            if (prefab != null)
                return prefab;
        }
        return null;
    }
}
