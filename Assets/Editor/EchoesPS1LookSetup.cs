using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Configura la capa "PS1" del pipeline: resolucion interna baja + upscale
/// nearest-neighbour + la Renderer Feature de dither/cuantizacion/scanlines.
///
/// Es la parte del look que NO va en los materiales. Todo lo que aqui se toca
/// vive en Echoes_URPAsset y Echoes_UniversalRenderer.
/// </summary>
public static class EchoesPS1LookSetup
{
    // Render scale es relativo a la resolucion de pantalla, no absoluto.
    // Estas cifras son la resolucion interna resultante a 1920x1080.
    private const float ScaleHarsh = 0.25f;   // 480x270
    private const float ScaleBalanced = 0.35f; // 672x378
    private const float ScaleSoft = 0.5f;      // 960x540

    private const int UpscalingFilterPoint = (int)UpscalingFilterSelection.Point;

    [MenuItem("Echoes of You/URP/PS1 Look/Instalar (equilibrado 672x378)", priority = 20)]
    public static void InstallBalanced() => Install(ScaleBalanced);

    [MenuItem("Echoes of You/URP/PS1 Look/Instalar (fuerte 480x270)", priority = 21)]
    public static void InstallHarsh() => Install(ScaleHarsh);

    [MenuItem("Echoes of You/URP/PS1 Look/Instalar (suave 960x540)", priority = 22)]
    public static void InstallSoft() => Install(ScaleSoft);

    [MenuItem("Echoes of You/URP/PS1 Look/Desinstalar", priority = 40)]
    public static void Uninstall()
    {
        UniversalRenderPipelineAsset urp = LoadUrpAsset();
        UniversalRendererData rendererData = LoadRendererData();
        if (urp == null || rendererData == null)
        {
            return;
        }

        SerializedObject urpSo = new SerializedObject(urp);
        SetFloat(urpSo, "m_RenderScale", 1f);
        SetInt(urpSo, "m_UpscalingFilter", (int)UpscalingFilterSelection.Auto);
        urpSo.ApplyModifiedProperties();
        EditorUtility.SetDirty(urp);

        PS1PostFeature feature = FindFeature(rendererData);
        if (feature != null)
        {
            rendererData.rendererFeatures.Remove(feature);
            AssetDatabase.RemoveObjectFromAsset(feature);
            Object.DestroyImmediate(feature, true);
            EditorUtility.SetDirty(rendererData);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Echoes PS1] Look desinstalado: RenderScale 1.0, filtro Automatic, feature eliminada.");
    }

    private static void Install(float renderScale)
    {
        EchoesUrpSetup.EnsureUrpAsset();

        UniversalRenderPipelineAsset urp = LoadUrpAsset();
        UniversalRendererData rendererData = LoadRendererData();
        if (urp == null || rendererData == null)
        {
            return;
        }

        // 1. Resolucion interna + upscale de vecino mas cercano.
        //    Sin el filtro Point, el upscale bilinear difumina los pixeles y
        //    se pierde todo el efecto (era el estado por defecto del proyecto).
        SerializedObject urpSo = new SerializedObject(urp);
        SetFloat(urpSo, "m_RenderScale", renderScale);
        SetInt(urpSo, "m_UpscalingFilter", UpscalingFilterPoint);
        urpSo.ApplyModifiedProperties();
        EditorUtility.SetDirty(urp);

        // 2. Renderer Feature del framebuffer PS1.
        PS1PostFeature feature = FindFeature(rendererData);
        if (feature == null)
        {
            feature = ScriptableObject.CreateInstance<PS1PostFeature>();
            feature.name = "Echoes PS1 Post";
            AssetDatabase.AddObjectToAsset(feature, rendererData);
            rendererData.rendererFeatures.Add(feature);
            Debug.Log("[Echoes PS1] Renderer Feature 'Echoes PS1 Post' anadida a Echoes_UniversalRenderer.");
        }

        Shader shader = Shader.Find("Echoes/PS1Post");
        if (shader == null)
        {
            Debug.LogError("[Echoes PS1] No se encontro el shader 'Echoes/PS1Post'. Revisa Assets/Shaders/PS1Post.shader.");
        }
        else
        {
            // Se serializa la referencia al shader para que no dependa de
            // Shader.Find en runtime (que fallaria en build si nadie lo referencia).
            SerializedObject featureSo = new SerializedObject(feature);
            SerializedProperty shaderProp = featureSo.FindProperty("m_Shader");
            if (shaderProp != null)
            {
                shaderProp.objectReferenceValue = shader;
            }
            featureSo.ApplyModifiedProperties();
        }

        feature.SetActive(true);
        EditorUtility.SetDirty(feature);
        EditorUtility.SetDirty(rendererData);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int width = Mathf.RoundToInt(1920f * renderScale);
        int height = Mathf.RoundToInt(1080f * renderScale);
        Debug.Log($"[Echoes PS1] Look instalado. RenderScale {renderScale:0.00} " +
                  $"(~{width}x{height} a 1080p), upscale Nearest-Neighbor, dither+cuantizacion 5-bit activos. " +
                  "Ajusta los valores en Echoes_UniversalRenderer > Echoes PS1 Post.");
    }

    private static PS1PostFeature FindFeature(UniversalRendererData rendererData)
    {
        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature is PS1PostFeature ps1)
            {
                return ps1;
            }
        }
        return null;
    }

    private static UniversalRenderPipelineAsset LoadUrpAsset()
    {
        var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(EchoesUrpSetup.UrpAssetPath);
        if (urp == null)
        {
            Debug.LogError($"[Echoes PS1] No se pudo cargar el URP asset en {EchoesUrpSetup.UrpAssetPath}");
        }
        return urp;
    }

    private static UniversalRendererData LoadRendererData()
    {
        var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(EchoesUrpSetup.RendererAssetPath);
        if (data == null)
        {
            Debug.LogError($"[Echoes PS1] No se pudo cargar el renderer en {EchoesUrpSetup.RendererAssetPath}");
        }
        return data;
    }

    private static void SetFloat(SerializedObject so, string path, float value)
    {
        SerializedProperty prop = so.FindProperty(path);
        if (prop == null)
        {
            Debug.LogWarning($"[Echoes PS1] Propiedad '{path}' no encontrada en {so.targetObject.name}.");
            return;
        }
        prop.floatValue = value;
    }

    private static void SetInt(SerializedObject so, string path, int value)
    {
        SerializedProperty prop = so.FindProperty(path);
        if (prop == null)
        {
            Debug.LogWarning($"[Echoes PS1] Propiedad '{path}' no encontrada en {so.targetObject.name}.");
            return;
        }
        prop.intValue = value;
    }
}
