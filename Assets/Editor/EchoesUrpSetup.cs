using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Asigna un Universal Render Pipeline Asset a Graphics Settings y a todos los
/// niveles de Quality de golpe (paso tedioso de la migración a URP).
///
/// Antes de ejecutarlo, crea un URP Asset con:
///   Assets > Create > Rendering > URP Asset (with Universal Renderer)
/// (idealmente en Assets/Settings/). Luego ejecuta el menú de abajo.
/// </summary>
public static class EchoesUrpSetup
{
    [MenuItem("Echoes of You/URP/Assign URP Pipeline Asset")]
    public static void AssignUrpPipeline()
    {
        UniversalRenderPipelineAsset urp = FindUrpAsset();
        if (urp == null)
        {
            EditorUtility.DisplayDialog(
                "URP Setup",
                "No se encontró ningún URP Asset en el proyecto.\n\n" +
                "Créalo con:\n" +
                "  Assets > Create > Rendering > URP Asset (with Universal Renderer)\n\n" +
                "Guárdalo (p.ej. en Assets/Settings/) y vuelve a ejecutar este comando.",
                "OK");
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

        string path = AssetDatabase.GetAssetPath(urp);
        Debug.Log($"[Echoes URP] Pipeline asignado: {path}  (Graphics + {levels} niveles de Quality).");
        EditorUtility.DisplayDialog(
            "URP Setup",
            $"URP asignado en Graphics y en {levels} niveles de Quality:\n{path}\n\n" +
            "Siguientes pasos:\n" +
            "1) Window > Rendering > Render Pipeline Converter > 'Built-in to URP' (convierte los .mat sueltos).\n" +
            "2) Regenera los niveles (Echoes of You > Production > Rebuild...).\n" +
            "3) (Opcional) Activa SSAO como Renderer Feature en el Universal Renderer.",
            "OK");
    }

    static UniversalRenderPipelineAsset FindUrpAsset()
    {
        string[] guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        UniversalRenderPipelineAsset thirdParty = null;
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (asset == null) continue;

            // Preferir un asset propio frente al de ParticlePack (terceros).
            if (!path.Contains("ParticlePack"))
                return asset;
            thirdParty = asset;
        }
        return thirdParty;
    }
}
