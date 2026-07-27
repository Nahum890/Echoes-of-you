using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Crea y asigna automáticamente el Universal Render Pipeline Asset a Graphics Settings 
/// y a todos los niveles de Quality, eliminando el estado magenta de los shaders retro.
/// </summary>
public static class EchoesUrpSetup
{
    public const string SettingsFolder = "Assets/Settings";
    public const string RendererAssetPath = "Assets/Settings/Echoes_UniversalRenderer.asset";
    public const string UrpAssetPath = "Assets/Settings/Echoes_URPAsset.asset";

    [InitializeOnLoadMethod]
    [MenuItem("Echoes of You/URP/Assign URP Pipeline Asset")]
    public static void AssignUrpPipeline()
    {
        UniversalRenderPipelineAsset urp = EnsureUrpAsset();
        if (urp == null)
        {
            Debug.LogError("[Echoes URP] No se pudo crear ni encontrar el URP Asset.");
            return;
        }

        GraphicsSettings.defaultRenderPipeline = urp;

        int levels = QualitySettings.names.Length;
        int current = QualitySettings.GetQualityLevel();
        for (int i = 0; i < levels; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = urp;
        }
        QualitySettings.SetQualityLevel(current, false);

        AssetDatabase.SaveAssets();
        Debug.Log($"[Echoes URP] Pipeline asignado exitosamente: {AssetDatabase.GetAssetPath(urp)} (Graphics + {levels} niveles de Quality).");
    }

    public static UniversalRenderPipelineAsset EnsureUrpAsset()
    {
        UniversalRenderPipelineAsset urp = FindUrpAsset();
        if (urp != null)
            return urp;

        EnsureDirectory(SettingsFolder);

        // 1. Cargar o crear UniversalRendererData
        UniversalRendererData rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererAssetPath);
        if (rendererData == null)
        {
            rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, RendererAssetPath);
            Debug.Log($"[Echoes URP] Creado UniversalRendererData en {RendererAssetPath}");
        }

        // 2. Crear UniversalRenderPipelineAsset usando CreateAsset con ScriptableRendererData
        urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
        if (urp == null)
        {
            urp = UniversalRenderPipelineAsset.Create(rendererData);
            AssetDatabase.CreateAsset(urp, UrpAssetPath);
            Debug.Log($"[Echoes URP] Creado UniversalRenderPipelineAsset en {UrpAssetPath}");
        }

        AssetDatabase.SaveAssets();
        return urp;
    }

    static UniversalRenderPipelineAsset FindUrpAsset()
    {
        string knownPath = "Assets/UnityTechnologies/ParticlePack/URP.asset";
        var knownAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(knownPath);
        if (knownAsset != null)
            return knownAsset;

        string[] guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (asset != null)
                return asset;
        }
        return null;
    }

    static void EnsureDirectory(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }
    }
}
