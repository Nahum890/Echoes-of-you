using System.IO;
using UnityEditor;
using UnityEngine;

public static class EchoesMaterialLibrary
{
    public const string MaterialRoot = "Assets/Materials/Echoes";

    public static Material FloorMat => GetOrCreateFloorMat();
    public static Material PlateMat => GetOrCreatePlateMat();
    public static Material BridgeMat => GetOrCreateBridgeMat();
    public static Material DoorMat => GetOrCreateDoorMat();
    public static Material GoalMat => GetOrCreateGoalMat();
    public static Material PlayerMat => GetOrCreatePlayerMat();
    public static Material EchoMat => GetOrCreateEchoMat();
    public static Material ArchMat => GetOrCreateArchMat();
    public static Material LiminalFogMat => GetOrCreateLiminalFogMat();

    public static Material WallTealMat    => GetOrCreateMaterial("Mat_WallTeal",    HexColor("2B4A4A"));
    public static Material WallMustardMat => GetOrCreateMaterial("Mat_WallMustard", HexColor("5A4A2E"));
    public static Material WallSageMat    => GetOrCreateMaterial("Mat_WallSage",    HexColor("3A4A38"));
    public static Material WallRoseMat    => GetOrCreateMaterial("Mat_WallRose",    HexColor("4A3438"));
    public static Material MemoryMat      => GetOrCreateEmissiveMaterial("Mat_Memory", HexColor("8A5A2E"), HexColor("E8B262") * 0.9f);

    public static void EnsureMaterials()
    {
        Shader standardShader = Shader.Find("Standard");
        if (standardShader == null)
        {
            Debug.LogError("[Echoes Material Library] Could not find Standard shader!");
            return;
        }

        // Trigger loading and creation of all materials
        var floor = FloorMat;
        var arch = ArchMat;
        var bridge = BridgeMat;
        var plate = PlateMat;
        var door = DoorMat;
        var goal = GoalMat;
        var player = PlayerMat;
        var echo = EchoMat;
        var fog = LiminalFogMat;
        var wallTeal    = WallTealMat;
        var wallMustard = WallMustardMat;
        var wallSage    = WallSageMat;
        var wallRose    = WallRoseMat;
        var memory      = MemoryMat;

        if (goal != null)
        {
            goal.EnableKeyword("_EMISSION");
            goal.SetColor("_EmissionColor", new Color(1.0f, 0.7f, 0.35f) * 2.5f);
            goal.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        if (echo != null)
        {
            echo.EnableKeyword("_EMISSION");
            echo.SetColor("_EmissionColor", new Color(0f, 0.5f, 0.65f) * 1.5f);
            echo.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
    }

    private static Material GetOrCreateFloorMat() => GetOrCreateMaterial("Mat_Floor", HexColor("30333D"));
    private static Material GetOrCreatePlateMat() => GetOrCreateEmissiveMaterial("Mat_Plate", HexColor("141A29"), new Color(0f, 0.4f, 0.52f) * 1.5f);
    private static Material GetOrCreateBridgeMat() => GetOrCreateMaterial("Mat_Bridge", HexColor("3B4454"));
    private static Material GetOrCreateDoorMat() => GetOrCreateEmissiveMaterial("Mat_Door", HexColor("7E1E2F"), new Color(0.4f, 0.05f, 0.05f) * 0.8f);
    private static Material GetOrCreateGoalMat() => GetOrCreateEmissiveMaterial("Mat_Exit", HexColor("FFEBB5"), new Color(1.0f, 0.7f, 0.35f) * 2.5f);
    private static Material GetOrCreatePlayerMat() => GetOrCreateMaterial("Mat_Player", HexColor("FFFFFF"));
    private static Material GetOrCreateEchoMat() => GetOrCreateTransparentMaterial("Mat_Echo", new Color(0f, 0.8f, 1f, 0.45f), true);
    private static Material GetOrCreateArchMat() => GetOrCreateMaterial("Mat_Architecture", HexColor("3A3E47"));

    private static Material GetOrCreateLiminalFogMat()
    {
        string path = Path.Combine(MaterialRoot, "Mat_LiminalFog.mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;

        material = new Material(Shader.Find("Standard"));
        material.color = new Color(0.2f, 0.22f, 0.28f, 0.5f);
        SetupTransparentMaterial(material);
        
        EnsureFolderExists(MaterialRoot);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    public static Material GetOrCreateMaterial(string name, Color color, bool emissive = false)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            material.color = color;
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Glossiness", 0.05f);
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 0.8f);
            }
            return material;
        }

        material = new Material(Shader.Find("Standard"));
        material.color = color;
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Glossiness", 0.05f);
        if (emissive)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.8f);
        }

        EnsureFolderExists(MaterialRoot);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    public static Material GetOrCreateEmissiveMaterial(string name, Color albedo, Color emission)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            material.color = albedo;
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Glossiness", 0.05f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
            return material;
        }

        material = new Material(Shader.Find("Standard"));
        material.color = albedo;
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Glossiness", 0.05f);
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emission);

        EnsureFolderExists(MaterialRoot);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    public static Material GetOrCreateTransparentMaterial(string name, Color color, bool emissive)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            material.color = color;
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.4f);
            }
            return material;
        }

        material = new Material(Shader.Find("Standard"));
        material.color = color;
        SetupTransparentMaterial(material);

        if (emissive)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.4f);
        }

        EnsureFolderExists(MaterialRoot);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void SetupTransparentMaterial(Material material)
    {
        material.SetFloat("_Mode", 3); // Transparent mode
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }

    private static void SetupMaterialTextures(
        Material mat, 
        string albedoPath, 
        string normalPath, 
        string aoPath, 
        float bumpScale, 
        float metallic, 
        float smoothness, 
        Vector2? tiling)
    {
        if (mat == null) return;

        if (!string.IsNullOrEmpty(albedoPath))
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            if (tex != null) mat.SetTexture("_MainTex", tex);
        }

        if (!string.IsNullOrEmpty(normalPath))
        {
            Texture2D norm = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if (norm != null)
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", norm);
                mat.SetFloat("_BumpScale", bumpScale);
            }
        }

        if (!string.IsNullOrEmpty(aoPath))
        {
            Texture2D ao = AssetDatabase.LoadAssetAtPath<Texture2D>(aoPath);
            if (ao != null) mat.SetTexture("_OcclusionMap", ao);
        }

        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Glossiness", smoothness);

        if (tiling.HasValue)
        {
            mat.SetTextureScale("_MainTex", tiling.Value);
            if (!string.IsNullOrEmpty(normalPath))
                mat.SetTextureScale("_BumpMap", tiling.Value);
            if (!string.IsNullOrEmpty(aoPath))
                mat.SetTextureScale("_OcclusionMap", tiling.Value);
        }
    }

    private static Color HexColor(string hex, float alpha = 1f)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color c))
        {
            c.a = alpha;
            return c;
        }
        return new Color(1, 0, 1, alpha);
    }

    private static void EnsureFolderExists(string assetPath)
    {
        string[] parts = assetPath.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
