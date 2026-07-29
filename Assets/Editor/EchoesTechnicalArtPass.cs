using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Pase de arte técnico — estética liminal PS1/PS2 (Technical Artist AI, §3.1–3.7).
///
/// Aplica sobre las 15 escenas existentes SIN reconstruirlas (no toca el builder,
/// no destruye los props del Environment Pass):
///  - Iluminación por capítulo (LIGHTING_GRAMMAR / lighting_profiles.yaml) en
///    blueprints y escenas: fog, ambient, sol 0.85 lux #F2F2FF (50,-30,0).
///  - Sombras duras, shadow distance 40 m, en luces de escena y en el URP Asset.
///  - Fog volumes (Echoes/LiminalFogVolume) en pasillos >18 m y espacios >30 m.
///  - Props narrativos y micro-escenas vía EchoesPropDecorator.
///  - EchoRecordingData narrativos (N05/N10/N13) asignados a sus blueprints.
///
/// Ejecutar: menú "Echoes of You/Technical Art/Run Full Pass (All Levels)"
/// o batchmode: -executeMethod EchoesTechnicalArtPass.RunFullPassBatch
/// </summary>
public static class EchoesTechnicalArtPass
{
    const string SceneRoot = "Assets/Scenes";
    const string BlueprintRoot = "Assets/Data/Levels";
    const string RecordingsRoot = "Assets/Data/EchoRecordings";
    const string FogVolumeRootName = "--- FOG VOLUMES ---";

    // ─── Perfiles por capítulo ────────────────────────────────────────────
    // Densidades fijas del catálogo lighting_profiles.yaml (dentro de los
    // rangos del brief). Cap. VI usa fog #0A0A0D + ambient #FFFFFF según el
    // brief del pase; el catálogo dice #F0F4FF — discrepancia documentada en
    // el reporte, gana el brief.
    public struct ChapterProfile
    {
        public string id;
        public Color fogColor;
        public float fogDensity;
        public Color ambientColor;
        public Color sunColor;
        public float sunIntensity;
        public Vector3 sunRotation;
    }

    static readonly Vector3 SunRot = new Vector3(50f, -30f, 0f);

    public static readonly Dictionary<string, ChapterProfile> Chapters = new()
    {
        ["I"]   = Profile("I",   "1C2430", 0.008f, "0F141A"),
        ["II"]  = Profile("II",  "2E3024", 0.010f, "1A1C14"),
        ["III"] = Profile("III", "2A1E1E", 0.012f, "140E0E"),
        ["IV"]  = Profile("IV",  "3B3024", 0.015f, "1E1812"),
        ["V"]   = Profile("V",   "1A1020", 0.020f, "0C0810"),
        ["VI"]  = Profile("VI",  "0A0A0D", 0.002f, "FFFFFF"),
    };

    // Nivel → capítulo (PROJECT_CONTEXT §4: capítulos emocionales)
    public static readonly Dictionary<int, string> LevelChapter = new()
    {
        [1] = "I", [2] = "I", [3] = "I",
        [4] = "II", [5] = "II", [8] = "II",
        [6] = "III", [7] = "III", [9] = "III",
        [10] = "IV", [11] = "IV",
        [12] = "V", [13] = "V",
        [14] = "VI", [15] = "VI",
    };

    static ChapterProfile Profile(string id, string fogHex, float density, string ambientHex) => new()
    {
        id = id,
        fogColor = EchoesMaterialLibrary.HexColor(fogHex),
        fogDensity = density,
        ambientColor = EchoesMaterialLibrary.HexColor(ambientHex),
        sunColor = EchoesMaterialLibrary.HexColor("F2F2FF"),
        sunIntensity = 0.85f,
        sunRotation = SunRot,
    };

    // ─── Entradas ─────────────────────────────────────────────────────────

    [MenuItem("Echoes of You/Technical Art/Run Full Pass (All Levels)", false, 400)]
    public static void RunFullPass()
    {
        EchoesMaterialLibrary.EnsureMaterials();
        ConfigureUrpHardShadows();
        ApplyChapterLightingToBlueprints();
        CreateNarrativeEchoRecordings();

        var report = new List<string>();
        for (int level = 1; level <= 15; level++)
        {
            string scenePath = $"{SceneRoot}/Level_{level:00}.unity";
            if (!File.Exists(scenePath))
            {
                report.Add($"Level_{level:00}: ESCENA NO ENCONTRADA");
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            ApplyVisualPassToScene(scene, level, report);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        EchoesVisualValidationPass.ValidateAllLevels();

        Debug.Log("[TechnicalArtPass] Pase completo:\n" + string.Join("\n", report));
    }

    /// Punto de entrada para batchmode (Unity -batchmode -executeMethod ...).
    public static void RunFullPassBatch()
    {
        RunFullPass();
        EditorApplication.Exit(0);
    }

    [MenuItem("Echoes of You/Technical Art/Apply Visual Pass To Open Scene", false, 401)]
    public static void ApplyToOpenScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        int level = ParseLevelNumber(scene.name);
        if (level < 1)
        {
            Debug.LogWarning($"[TechnicalArtPass] '{scene.name}' no es una escena Level_XX.");
            return;
        }

        var report = new List<string>();
        ApplyVisualPassToScene(scene, level, report);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[TechnicalArtPass]\n" + string.Join("\n", report));
    }

    public static int ParseLevelNumber(string sceneName)
    {
        if (sceneName == null || !sceneName.StartsWith("Level_")) return -1;
        // Escenas de prueba y greybox fase 1 quedan fuera del pase de arte:
        // el greybox es arquitectura pura sin props/luces por diseño.
        if (sceneName.Contains("TEST") || sceneName.Contains("SchoolGreybox")) return -1;
        return int.TryParse(sceneName.Substring(6, 2), out int n) ? n : -1;
    }

    // ─── 3.2 Iluminación por capítulo ────────────────────────────────────

    [MenuItem("Echoes of You/Technical Art/Apply Chapter Lighting To Blueprints", false, 402)]
    public static void ApplyChapterLightingToBlueprints()
    {
        for (int level = 1; level <= 15; level++)
        {
            string path = $"{BlueprintRoot}/Level_{level:00}_Blueprint.asset";
            var blueprint = AssetDatabase.LoadAssetAtPath<LevelBlueprint>(path);
            if (blueprint == null)
            {
                Debug.LogWarning($"[TechnicalArtPass] Blueprint no encontrado: {path}");
                continue;
            }

            ChapterProfile profile = Chapters[LevelChapter[level]];
            blueprint.fogColor = profile.fogColor;
            blueprint.fogDensity = profile.fogDensity;
            blueprint.ambientColor = profile.ambientColor;
            blueprint.directionalLightColor = profile.sunColor;
            blueprint.directionalLightIntensity = profile.sunIntensity;
            blueprint.directionalLightRotation = profile.sunRotation;
            EditorUtility.SetDirty(blueprint);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[TechnicalArtPass] Iluminación por capítulo aplicada a los 15 blueprints.");
    }

    static void ApplyVisualPassToScene(Scene scene, int level, List<string> report)
    {
        ChapterProfile profile = Chapters[LevelChapter[level]];

        // RenderSettings del capítulo (config obligatoria URP: ambient Flat)
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = profile.fogColor;
        RenderSettings.fogDensity = profile.fogDensity;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = profile.ambientColor;
        RenderSettings.ambientIntensity = 0.15f; // LIGHTING_GRAMMAR ambient_intensity_lux
        RenderSettings.skybox = null;
        RenderSettings.reflectionIntensity = 0f;

        // Luz direccional del capítulo + sombras duras en TODAS las luces
        int softFixed = 0;
        foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include))
        {
            if (light.type == LightType.Directional)
            {
                light.color = profile.sunColor;
                light.intensity = profile.sunIntensity;
                light.transform.rotation = Quaternion.Euler(profile.sunRotation);
                if (light.shadows != LightShadows.None)
                    light.shadows = LightShadows.Hard;
            }
            else if (light.shadows == LightShadows.Soft)
            {
                light.shadows = LightShadows.Hard; // 0 Soft Shadows (LIGHTING_GRAMMAR)
                softFixed++;
            }
        }

        // Sincronizar el componente LevelLightingSettings si existe
        var lightingSettings = Object.FindAnyObjectByType<LevelLightingSettings>();
        if (lightingSettings != null)
        {
            lightingSettings.fogColor = profile.fogColor;
            lightingSettings.fogDensity = profile.fogDensity;
            EditorUtility.SetDirty(lightingSettings);
        }

        int fogVolumes = SpawnFogVolumes(profile);
        string narrative = EchoesPropDecorator.DecorateLevel(level);

        report.Add($"Level_{level:00} [Cap.{profile.id}]: fog {ColorUtility.ToHtmlStringRGB(profile.fogColor)} d={profile.fogDensity:0.000}, " +
                   $"{softFixed} luces soft→hard, {fogVolumes} fog volumes, narrativa: {narrative}");
    }

    // ─── 3.3 Fog Volumes ─────────────────────────────────────────────────
    // Pasillos con eje mayor > 18 m (Corner Accumulation) y espacios abiertos
    // > 30 m (Height Factor). Se detecta por bounds de renderers de los
    // módulos bajo la raíz de entorno — robusto ante el escalado ×2 del
    // LevelEnvironmentBootstrap.

    static int SpawnFogVolumes(ChapterProfile profile)
    {
        GameObject oldRoot = GameObject.Find(FogVolumeRootName);
        if (oldRoot != null)
            Object.DestroyImmediate(oldRoot);

        GameObject envRoot = GameObject.Find("--- ENVIRONMENT ---");
        if (envRoot == null)
            return 0;

        GameObject fogRoot = new GameObject(FogVolumeRootName);
        int count = 0;

        foreach (Transform module in envRoot.transform)
        {
            Renderer[] renderers = module.GetComponentsInChildren<Renderer>(false);
            if (renderers.Length == 0)
                continue;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            float major = Mathf.Max(bounds.size.x, bounds.size.z);
            float minor = Mathf.Min(bounds.size.x, bounds.size.z);

            bool corridor = major > 18f && minor < major * 0.5f;
            bool openSpace = major > 30f && minor > 30f * 0.5f;
            if (!corridor && !openSpace)
                continue;

            var volume = GameObject.CreatePrimitive(PrimitiveType.Cube);
            volume.name = $"FogVolume_{module.name}";
            volume.transform.SetParent(fogRoot.transform, true);
            volume.transform.position = bounds.center;
            volume.transform.localScale = bounds.size;
            Object.DestroyImmediate(volume.GetComponent<Collider>());

            var rendererRef = volume.GetComponent<MeshRenderer>();
            rendererRef.sharedMaterial = BuildFogVolumeMaterial(profile);
            rendererRef.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rendererRef.receiveShadows = false;
            count++;
        }

        if (count == 0)
            Object.DestroyImmediate(fogRoot);
        return count;
    }

    static Material BuildFogVolumeMaterial(ChapterProfile profile)
    {
        string matPath = $"{EchoesMaterialLibrary.MaterialRoot}/Mat_FogVolume_Ch{profile.id}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            Shader shader = Shader.Find(EchoesMaterialLibrary.kLiminalFogVolume);
            if (shader == null)
            {
                Debug.LogError("[TechnicalArtPass] Shader Echoes/LiminalFogVolume no encontrado.");
                shader = Shader.Find(EchoesMaterialLibrary.kLiminalSurface);
            }
            mat = new Material(shader) { name = $"Mat_FogVolume_Ch{profile.id}" };
            EchoesMaterialLibrary.EnsureFolderExists(EchoesMaterialLibrary.MaterialRoot);
            AssetDatabase.CreateAsset(mat, matPath);
        }

        if (mat.HasProperty("_FogColor")) mat.SetColor("_FogColor", profile.fogColor);
        if (mat.HasProperty("_FogDensity")) mat.SetFloat("_FogDensity", profile.fogDensity);
        EchoesMaterialLibrary.ApplyFogVolumeDefaults(mat);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // ─── 3.7 Echo Recording Data narrativos ──────────────────────────────
    // Slots del blueprint: imposedEchoData / ambientEchoData. Asignación:
    //  L05 imposed = Aiden_Voice_Fragment
    //  L10 ambient = Lyra_Ambient_Echo, imposed = Lyra_Voice_Fragment
    //  L13 imposed = Aiden_Forced_Echo, ambient = Conversation_Fragment

    [MenuItem("Echoes of You/Technical Art/Create Narrative Echo Recordings", false, 403)]
    public static void CreateNarrativeEchoRecordings()
    {
        EchoesMaterialLibrary.EnsureFolderExists(RecordingsRoot);

        var assignments = new (int level, string imposed, string ambient)[]
        {
            (5,  "Aiden_Voice_Fragment", null),
            (10, "Lyra_Voice_Fragment",  "Lyra_Ambient_Echo"),
            (13, "Aiden_Forced_Echo",    "Conversation_Fragment"),
            // L15 tenía ambientEchoData=1 (flag bool del commit greybox);
            // al volver el campo a EchoRecordingData, el flag se preserva
            // asignando el eco ambiental de Lyra.
            (15, null,                   "Lyra_Ambient_Echo"),
        };

        foreach (var (level, imposedName, ambientName) in assignments)
        {
            var blueprint = AssetDatabase.LoadAssetAtPath<LevelBlueprint>(
                $"{BlueprintRoot}/Level_{level:00}_Blueprint.asset");

            EchoRecordingData imposed = imposedName != null ? GetOrCreateRecording(imposedName) : null;
            EchoRecordingData ambient = ambientName != null ? GetOrCreateRecording(ambientName) : null;

            if (blueprint != null)
            {
                if (imposed != null) blueprint.imposedEchoData = imposed;
                if (ambient != null) blueprint.ambientEchoData = ambient;
                EditorUtility.SetDirty(blueprint);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[TechnicalArtPass] EchoRecordingData narrativos creados y asignados (L05/L10/L13).");
    }

    static EchoRecordingData GetOrCreateRecording(string assetName)
    {
        string path = $"{RecordingsRoot}/{assetName}.asset";
        var data = AssetDatabase.LoadAssetAtPath<EchoRecordingData>(path);
        if (data != null)
            return data;

        data = ScriptableObject.CreateInstance<EchoRecordingData>();
        data.name = assetName;
        // Placeholder: los frames reales se graban en editor con la herramienta
        // de captura; un asset vacío no reproduce nada pero mantiene la referencia.
        AssetDatabase.CreateAsset(data, path);
        return data;
    }

    // ─── Configuración URP obligatoria ───────────────────────────────────

    [MenuItem("Echoes of You/Technical Art/Configure URP (Hard Shadows, 40m)", false, 404)]
    public static void ConfigureUrpHardShadows()
    {
        QualitySettings.shadows = UnityEngine.ShadowQuality.HardOnly;
        QualitySettings.shadowDistance = 40f;

        var pipeline = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        if (pipeline == null)
        {
            Debug.LogWarning("[TechnicalArtPass] No hay URP Asset asignado en GraphicsSettings.");
            return;
        }

        pipeline.shadowDistance = 40f;

        // m_SoftShadowsSupported no tiene setter público — vía SerializedObject.
        var so = new SerializedObject(pipeline);
        SerializedProperty soft = so.FindProperty("m_SoftShadowsSupported");
        if (soft != null && soft.boolValue)
        {
            soft.boolValue = false;
            so.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(pipeline);
        AssetDatabase.SaveAssets();
        Debug.Log($"[TechnicalArtPass] URP: soft shadows OFF, shadow distance 40m ({pipeline.name}).");
    }
}
