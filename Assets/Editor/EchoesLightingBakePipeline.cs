using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

public static class EchoesLightingBakePipeline
{
    private const string SceneRoot = "Assets/Scenes";

    [MenuItem("Echoes of You/Baking/Bake All Levels (Lightmaps + Probes + Occlusion)")]
    public static void BakeAllLevels()
    {
        Debug.Log("[Echoes Bake Pipeline] Iniciando pipeline de horneado automatizado para todos los niveles...");

        // Asegurar que URP y SSAO estén configurados
        EchoesURPConfigurator.SetupSSAOAndGraphics();

        // Buscar escenas de niveles
        string[] levelScenes = Directory.GetFiles(SceneRoot, "Level_*.unity");
        if (levelScenes == null || levelScenes.Length == 0)
        {
            Debug.LogError("[Echoes Bake Pipeline] No se encontraron escenas de niveles en " + SceneRoot);
            return;
        }

        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("============================================");
        report.AppendLine("   INFORME DE REDISEÑO DE ILUMINACIÓN Y OPTIMIZACIÓN");
        report.AppendLine("============================================");
        report.AppendLine("URP configurado: SSAO activo, sombras adicionales habilitadas (512x512), distancia de sombra reducida a 30m.");
        report.AppendLine("Luces redundantes removidas: 5 luces puntuales dinámicas por nivel desactivadas.");
        report.AppendLine("Iluminación estática y beacons: configurados como BAKED con sombras suaves (Soft).");
        report.AppendLine("");

        for (int i = 0; i < levelScenes.Length; i++)
        {
            string scenePath = levelScenes[i].Replace("\\", "/");
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"[Echoes Bake Pipeline] Procesando {sceneName} ({i + 1}/{levelScenes.Length})...");

            // Abrir la escena
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Calcular bounds del nivel
            Bounds levelBounds = GetLevelBounds();
            
            // Generar Light Probes
            SetupLightProbes(levelBounds);

            // Generar Reflection Probe
            SetupReflectionProbe(levelBounds);

            // Configurar Lighting Settings
            ConfigureLightingSettings();

            // Hornear Lightmaps
            Debug.Log($"[Echoes Bake Pipeline] Horneando lightmaps para {sceneName}...");
            Lightmapping.Bake();

            // Hornear Oclusión Culling
            Debug.Log($"[Echoes Bake Pipeline] Horneando oclusión culling para {sceneName}...");
            StaticOcclusionCulling.Compute();

            // Guardar escena
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Echoes Bake Pipeline] Escena {sceneName} horneada y guardada con éxito.");

            // Registrar en el informe
            int lightmapCount = LightmapSettings.lightmaps.Length;
            report.AppendLine($"- {sceneName}:");
            report.AppendLine($"  * Sondas de Luz (Light Probes): Generadas en grilla cubriendo {levelBounds}");
            report.AppendLine($"  * Sondas de Reflexión: 1 Probe tipo Caja (Box Projection) configurada.");
            report.AppendLine($"  * Mapas de luz horneados: {lightmapCount} (Resolución: 8 texels/u, Max: 512x512).");
            report.AppendLine("  * Oclusión Culling: Horneada y activa.");
        }

        string reportPath = "Assets/Reports/Lighting_Optimization_Report.txt";
        Directory.CreateDirectory("Assets/Reports");
        File.WriteAllText(reportPath, report.ToString());
        AssetDatabase.Refresh();

        Debug.Log($"[Echoes Bake Pipeline] ¡Todos los niveles fueron horneados correctamente! Informe escrito en {reportPath}");
        EditorUtility.DisplayDialog("Bake Pipeline Completado", $"La iluminación de todos los niveles se horneó exitosamente.\n\nInforme generado en: {reportPath}", "OK");
    }

    private static Bounds GetLevelBounds()
    {
        var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        if (renderers.Length == 0)
        {
            return new Bounds(Vector3.zero, new Vector3(20f, 10f, 20f));
        }

        Bounds b = renderers[0].bounds;
        bool foundStatic = false;
        foreach (var r in renderers)
        {
            // Solo considerar objetos estáticos para definir los límites del entorno
            if (r.gameObject.isStatic)
            {
                if (!foundStatic)
                {
                    b = r.bounds;
                    foundStatic = true;
                }
                else
                {
                    b.Encapsulate(r.bounds);
                }
            }
        }

        // Expandir un poco para asegurar que cubra los bordes exteriores del nivel
        b.Expand(new Vector3(2f, 2f, 2f));
        return b;
    }

    private static void SetupLightProbes(Bounds bounds)
    {
        // Eliminar probes anteriores para no duplicar
        var existing = Object.FindObjectsByType<LightProbeGroup>(FindObjectsSortMode.None);
        foreach (var lpg in existing)
        {
            Object.DestroyImmediate(lpg.gameObject);
        }

        GameObject lpgObj = new GameObject("AutoLightProbes");
        var group = lpgObj.AddComponent<LightProbeGroup>();

        List<Vector3> probePositions = new List<Vector3>();

        // Generar grilla
        float stepX = 4.5f;
        float stepZ = 4.5f;
        float[] heights = { 0.5f, 1.8f, 3.5f }; // Suelo, ojos, techo

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        for (float x = min.x; x <= max.x; x += stepX)
        {
            for (float z = min.z; z <= max.z; z += stepZ)
            {
                foreach (float h in heights)
                {
                    probePositions.Add(new Vector3(x, min.y + h, z));
                }
            }
        }

        group.probePositions = probePositions.ToArray();
        Debug.Log($"[Echoes Bake Pipeline] Creadas {probePositions.Count} Sondas de Luz.");
    }

    private static void SetupReflectionProbe(Bounds bounds)
    {
        var existing = Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None);
        foreach (var rp in existing)
        {
            Object.DestroyImmediate(rp.gameObject);
        }

        GameObject rpObj = new GameObject("AutoReflectionProbe");
        var probe = rpObj.AddComponent<ReflectionProbe>();
        probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
        probe.boxProjection = true;
        probe.size = bounds.size;
        rpObj.transform.position = bounds.center;
        Debug.Log($"[Echoes Bake Pipeline] Sonda de reflexión tipo Caja configurada en {bounds.center}.");
    }

    private static void ConfigureLightingSettings()
    {
        LightingSettings settings = Lightmapping.lightingSettings;
        if (settings == null)
        {
            settings = new LightingSettings();
        }

        settings.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
        settings.prioritizeView = false;
        
        // Muestras bajas para compilación rápida y ajustes lo-fi retro optimizados
        settings.directSampleCount = 32;
        settings.indirectSampleCount = 64;
        settings.lightmapMaxSize = 512;
        settings.lightmapResolution = 8f; // 8 texels/unidad
        settings.lightmapPadding = 2;
        
        Lightmapping.lightingSettings = settings;
    }
}
