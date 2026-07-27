using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class EchoesURPConfigurator
{
    [MenuItem("Echoes of You/URP/Setup SSAO and Graphics")]
    public static void SetupSSAOAndGraphics()
    {
        EchoesUrpSetup.EnsureUrpAsset();
        string rendererDataPath = EchoesUrpSetup.RendererAssetPath;
        UniversalRendererData rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererDataPath);
        if (rendererData == null)
        {
            Debug.LogError("[Echoes URP Config] No se pudo cargar URP_Renderer.asset en " + rendererDataPath);
            return;
        }

        ScriptableRendererFeature ssaoFeature = null;
        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature != null && feature.GetType().Name == "ScreenSpaceAmbientOcclusion")
            {
                ssaoFeature = feature;
                break;
            }
        }

        if (ssaoFeature == null)
        {
            ssaoFeature = ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();
            ssaoFeature.name = "SSAO";
            
            AssetDatabase.AddObjectToAsset(ssaoFeature, rendererData);
            rendererData.rendererFeatures.Add(ssaoFeature);
            
            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();
            Debug.Log("[Echoes URP Config] Creada y agregada la Renderer Feature de SSAO.");
        }
        else
        {
            Debug.Log("[Echoes URP Config] SSAO ya existe en URP_Renderer.");
        }

        // Configurar los parámetros de SSAO usando SerializedObject para máxima robustez
        SerializedObject so = new SerializedObject(ssaoFeature);
        SerializedProperty settingsProp = so.FindProperty("m_Settings");
        if (settingsProp != null)
        {
            SerializedProperty intensityProp = settingsProp.FindPropertyRelative("Intensity");
            SerializedProperty radiusProp = settingsProp.FindPropertyRelative("Radius");
            SerializedProperty directLightProp = settingsProp.FindPropertyRelative("DirectLightingStrength");
            SerializedProperty qualityProp = settingsProp.FindPropertyRelative("Quality");

            if (intensityProp != null) intensityProp.floatValue = 1.8f;
            if (radiusProp != null) radiusProp.floatValue = 0.6f;
            if (directLightProp != null) directLightProp.floatValue = 0.25f;
            if (qualityProp != null) qualityProp.intValue = 1; // 1 = Medium o similar según enum de URP

            so.ApplyModifiedProperties();
            Debug.Log("[Echoes URP Config] Parámetros de SSAO configurados: Intensidad=1.8, Radio=0.6, Fuerza Luz Directa=0.25.");
        }
        else
        {
            // Intentar a nivel raíz si la versión de URP no usa m_Settings
            SerializedProperty intensityProp = so.FindProperty("Intensity");
            SerializedProperty radiusProp = so.FindProperty("Radius");
            SerializedProperty directLightProp = so.FindProperty("DirectLightingStrength");

            if (intensityProp != null) intensityProp.floatValue = 1.8f;
            if (radiusProp != null) radiusProp.floatValue = 0.6f;
            if (directLightProp != null) directLightProp.floatValue = 0.25f;

            so.ApplyModifiedProperties();
            Debug.Log("[Echoes URP Config] Parámetros de SSAO configurados en la raíz del ScriptableObject.");
        }

        // Ejecutar la asignación de URP a Graphics Settings y Quality levels
        EchoesUrpSetup.AssignUrpPipeline();
    }
}
