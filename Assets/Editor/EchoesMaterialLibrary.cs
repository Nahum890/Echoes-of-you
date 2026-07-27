using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EchoesMaterialLibrary
{
    public const string MaterialRoot = "Assets/Materials/Echoes";

    // SHADERS LIMINALES (CONSTANTS_REGISTRY primitives.shaders)
    public const string kLiminalSurface = "Echoes/LiminalSurface";   // Arquitectura
    public const string kLiminalFogVolume = "Echoes/LiminalFogVolume"; // Volúmenes de niebla
    public const string kEchoLiminal = "Echoes/EchoLiminal";         // Eco en playback
    public const string kAnalogGhost = "Echoes/AnalogGhost";         // Residual / legacy fantasma
    public const string kRetroFlatLit = "Echoes/RetroFlatLit"; // Legacy fallback

    private static readonly Dictionary<string, Material> _materials = new();

    // TOKENS DE PALETA (ECHOES_BIBLE.md)
    public static Material VoidBlackMat        => GetMaterial("void-black");
    public static Material CorridorNavyMat     => GetMaterial("corridor-navy");
    public static Material FluorescentSickMat  => GetMaterial("fluorescent-sick");
    public static Material MemoryAmberMat      => GetMaterial("memory-amber");
    public static Material EchoCyanMat         => GetMaterial("echo-cyan");
    public static Material WrongnessRedMat     => GetMaterial("wrongness-red");
    public static Material InstitutionalTealMat=> GetMaterial("institutional-teal");
    public static Material FadedMustardMat     => GetMaterial("faded-mustard");
    public static Material SageGreenMat        => GetMaterial("sage-green");
    public static Material DustyRoseMat        => GetMaterial("dusty-rose");

    // MAPEO CANÓNICO A TOKENS
    public static Material FloorMat            => CorridorNavyMat;
    public static Material PlateMat            => FluorescentSickMat;
    public static Material BridgeMat           => EchoCyanMat;
    public static Material DoorMat             => WrongnessRedMat;
    public static Material GoalMat             => MemoryAmberMat;
    public static Material PlayerMat           => GetOrCreateMaterialInternal("Mat_Player", Color.white);
    public static Material EchoMat             => EchoCyanMat;
    public static Material ArchMat             => GetOrCreateMaterialInternal("Mat_Architecture", HexColor("3A3E47"));
    public static Material LiminalFogMat       => GetOrCreateFogMaterial();

    public static Material WallTealMat         => InstitutionalTealMat;
    public static Material WallMustardMat      => FadedMustardMat;
    public static Material WallSageMat         => SageGreenMat;
    public static Material WallRoseMat         => DustyRoseMat;
    public static Material MemoryMat           => MemoryAmberMat;
    public static Material ChalkboardMat       => GetOrCreateMaterialInternal("Mat_Chalkboard", HexColor("2A3A2A", 0.9f));
    public static Material BookMat             => GetOrCreateMaterialInternal("Mat_Book", HexColor("3A2A1A"));
    public static Material CrackMat            => GetOrCreateMaterialInternal("Mat_Crack", HexColor("0A0A0D"));

    [InitializeOnLoadMethod]
    public static void EnsureMaterials()
    {
        EnsureFolderExists(MaterialRoot);

        // FORZAR creación de todos los materiales token
        _ = VoidBlackMat; _ = CorridorNavyMat; _ = FluorescentSickMat; _ = MemoryAmberMat;
        _ = EchoCyanMat; _ = WrongnessRedMat; _ = InstitutionalTealMat; _ = FadedMustardMat;
        _ = SageGreenMat; _ = DustyRoseMat;
        _ = FloorMat; _ = ArchMat; _ = BridgeMat; _ = PlateMat; _ = DoorMat; _ = GoalMat;
        _ = PlayerMat; _ = EchoMat; _ = LiminalFogMat; _ = ChalkboardMat; _ = BookMat; _ = CrackMat;

        AssetDatabase.SaveAssets();
        Debug.Log("[EchoesMaterialLibrary] ✓ Materiales liminales asegurados");
    }

    public static Material GetMaterial(string tokenName)
    {
        if (string.IsNullOrEmpty(tokenName)) return CorridorNavyMat;
        if (_materials.TryGetValue(tokenName, out var cached) && cached != null) return cached;
        Material mat = CreateTokenMaterial(tokenName);
        _materials[tokenName] = mat;
        return mat;
    }

    private static Material CreateTokenMaterial(string token)
    {
        string matName = $"Mat_Token_{token}";
        string path = Path.Combine(MaterialRoot, $"{matName}.mat").Replace("\\", "/");
        
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        
        // SELECCIÓN DE SHADER POR TOKEN
        string shaderName = token switch
        {
            "echo-cyan" => kEchoLiminal,
            "liminal-fog" => kLiminalFogVolume,
            _ => kLiminalSurface  // TODO usa LiminalSurface por defecto
        };

        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            Debug.LogError($"[EchoesMaterialLibrary] Shader NO encontrado: {shaderName}. Usando fallback.");
            shader = Shader.Find("Echoes/LiminalSurface") ?? Shader.Find("Universal Render Pipeline/Lit");
        }

        if (mat == null)
        {
            mat = new Material(shader) { name = $"Mat_Token_{token}" };
            EnsureFolderExists(MaterialRoot);
            AssetDatabase.CreateAsset(mat, path);
        }
        else if (mat.shader != shader)
        {
            mat.shader = shader;
        }

        Color color = TokenToColor(token);
        mat.color = color;

        // CONFIGURACIÓN LIMINAL POR TOKEN
        ConfigureLiminalProperties(token, mat, color);

        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void ConfigureLiminalProperties(string token, Material mat, Color baseColor)
    {
        if (mat.shader != null && mat.shader.name.Contains("LiminalSurface"))
        {
            // PROPIEDADES LIMINAL SURFACE
            mat.SetFloat("_FresnelInvert", 0.3f);
            mat.SetFloat("_FluorescentEdge", 0.1f);
            mat.SetFloat("_StainNoiseScale", 4f);
            mat.SetFloat("_StainThreshold", 0.7f);
            mat.SetFloat("_WearNoiseScale", 8f);
            mat.SetFloat("_WearHeight", 1.5f);
            mat.SetFloat("_SpecularAnomaly", 0.15f);
            mat.SetFloat("_DepthDistort", 0.005f);

            // COLORES LIMINALES POR TOKEN
            switch (token)
            {
                case "corridor-navy":
                    mat.SetFloat("_FresnelInvert", 0.35f);
                    mat.SetFloat("_FluorescentEdge", 0.12f);
                    mat.SetFloat("_StainThreshold", 0.65f);
                    mat.SetFloat("_WearHeight", 1.5f);
                    mat.SetColor("_SubsurfaceTint", new Color(0, 0.08f, 0.12f));
                    mat.SetColor("_StainColor", new Color(0.05f, 0.08f, 0.12f));
                    mat.SetColor("_WearColor", new Color(0.08f, 0.1f, 0.14f));
                    break;
                case "faded-mustard":
                    mat.SetFloat("_FresnelInvert", 0.25f);
                    mat.SetFloat("_FluorescentEdge", 0.08f);
                    mat.SetFloat("_StainThreshold", 0.7f);
                    mat.SetFloat("_WearHeight", 1.2f);
                    mat.SetColor("_SubsurfaceTint", new Color(0.08f, 0.06f, 0.02f));
                    break;
                case "sage-green":
                    mat.SetFloat("_FresnelInvert", 0.3f);
                    mat.SetFloat("_FluorescentEdge", 0.1f);
                    mat.SetColor("_SubsurfaceTint", new Color(0.02f, 0.08f, 0.04f));
                    break;
                case "dusty-rose":
                    mat.SetFloat("_FresnelInvert", 0.2f);
                    mat.SetFloat("_FluorescentEdge", 0.05f);
                    mat.SetColor("_SubsurfaceTint", new Color(0.08f, 0.04f, 0.05f));
                    break;
                case "void-black":
                    mat.SetFloat("_FresnelInvert", 0.1f);
                    mat.SetFloat("_FluorescentEdge", 0f);
                    mat.SetColor("_SubsurfaceTint", Color.black);
                    break;
                case "memory-amber":
                    mat.SetFloat("_FluorescentEdge", 0.15f);
                    mat.EnableKeyword("_EMISSION");
                    // Emission 1.2 (RULE-MAT-001 / ENVIRONMENT_STORYTELLING)
                    mat.SetColor("_EmissionColor", HexColor("FFBF00") * 1.2f);
                    break;
                case "fluorescent-sick":
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", new Color(0.79f, 0.83f, 0.69f) * 1.2f);
                    break;
                case "wrongness-red":
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", new Color(0.70f, 0.23f, 0.23f) * 1.5f);
                    break;
                case "echo-cyan":
                    // Echo usa shader propio
                    break;
            }
        }
        else if (mat.shader != null && mat.shader.name.Contains("EchoLiminal"))
        {
            // ECHO LIMINAL PROPERTIES
            mat.SetFloat("_DistortionStrength", 0.12f);
            mat.SetFloat("_ChromaticAberration", 0.025f);
            mat.SetFloat("_DepthOffset", -0.08f);
            mat.SetFloat("_ScanlineFreq", 40f);
            mat.SetFloat("_ScanlineSpeed", 2f);
            mat.SetFloat("_TemporalJitter", 0.008f);
            mat.SetFloat("_DitherStrength", 0.5f);
            mat.SetFloat("_FifteenFPSCap", 1f);
            mat.SetColor("_ResonanceGlow", new Color(0, 0.8f, 1f, 0.3f));
            mat.SetColor("_BaseColor", new Color(0.31f, 0.765f, 0.91f, 0.45f));
        }

        // EMISSION PARA TOKENES BRILLANTES
        if (token == "echo-cyan" || token == "memory-amber" || token == "fluorescent-sick" || token == "wrongness-red")
        {
            mat.EnableKeyword("_EMISSION");
            Color emissionColor = TokenToColor(token);
            if (token == "echo-cyan") emissionColor = new Color(0f, 0.5f, 0.65f) * 2f;
            else if (token == "memory-amber") emissionColor = HexColor("FFBF00") * 1.2f;
            else if (token == "fluorescent-sick") emissionColor = new Color(0.79f, 0.83f, 0.69f) * 1.2f;
            else if (token == "wrongness-red") emissionColor = new Color(0.70f, 0.23f, 0.23f) * 1.5f;
            mat.SetColor("_EmissionColor", emissionColor);
        }
    }

    // HEX canónicos de CONSTANTS_REGISTRY.yaml (primitives.colors) — el registro
    // manda sobre cualquier otro doc (RULE-SOT-001B). materials.yaml decía
    // #E8B262 para memory-amber: PROHIBIDO por CONS-MAT-001, el canon es #FFBF00.
    public static Color TokenToColor(string token)
    {
        return token switch
        {
            "void-black"         => HexColor("0A0A0D"),          // #0A0A0D
            "corridor-navy"      => HexColor("1C2430"),          // #1C2430
            "fluorescent-sick"   => HexColor("C9D4B0"),          // #C9D4B0
            "memory-amber"       => HexColor("FFBF00"),          // #FFBF00 (registro, NO #E8B262)
            "echo-cyan"          => HexColor("4FC3E8", 0.45f),   // #4FC3E8, alpha playback 0.45
            "wrongness-red"      => HexColor("B23A3A"),          // #B23A3A
            "institutional-teal" => HexColor("2B4A4A"),          // #2B4A4A
            "faded-mustard"      => HexColor("5A4A2E"),          // #5A4A2E
            "sage-green"         => HexColor("3A4A38"),          // #3A4A38
            "dusty-rose"         => HexColor("4A3438"),          // #4A3438
            _                    => HexColor("1C2430")
        };
    }

    private static Material GetOrCreateMaterialInternal(string name, Color color, bool emissive = false)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find(kLiminalSurface) ?? Shader.Find(kRetroFlatLit);

        if (mat == null)
        {
            mat = new Material(shader) { name = name };
            EnsureFolderExists(MaterialRoot);
            AssetDatabase.CreateAsset(mat, path);
        }
        else if (mat.shader != shader)
        {
            mat.shader = shader;
        }

        mat.color = color;
        if (emissive)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.8f);
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material GetOrCreateFogMaterial()
    {
        string path = Path.Combine(MaterialRoot, "Mat_LiminalFog.mat").Replace("\\", "/");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find(kLiminalFogVolume) ?? Shader.Find("Echoes/LiminalFog") ?? Shader.Find(kRetroFlatLit);

        if (mat == null)
        {
            mat = new Material(shader) { name = "Mat_LiminalFog" };
            EnsureFolderExists(MaterialRoot);
            AssetDatabase.CreateAsset(mat, path);
        }
        else if (mat.shader != shader)
        {
            mat.shader = shader;
        }

        mat.color = HexColor("1C2430");
        if (mat.HasProperty("_FogColor")) mat.SetColor("_FogColor", HexColor("1C2430"));
        if (mat.HasProperty("_FogDensity")) mat.SetFloat("_FogDensity", 0.02f);
        ApplyFogVolumeDefaults(mat);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    /// Parámetros canónicos de los fog volumes (pase de arte técnico §3.3).
    public static void ApplyFogVolumeDefaults(Material mat)
    {
        if (mat == null) return;
        if (mat.HasProperty("_CornerAccumulation")) mat.SetFloat("_CornerAccumulation", 0.35f);
        if (mat.HasProperty("_LightScatter")) mat.SetFloat("_LightScatter", 0.25f);
        if (mat.HasProperty("_NoiseScale")) mat.SetFloat("_NoiseScale", 0.5f);
        if (mat.HasProperty("_NoiseSpeed")) mat.SetFloat("_NoiseSpeed", 0.05f);
    }

    public static Material GetOrCreateMaterial(string name, Color color, bool emissive = false)
        => GetOrCreateMaterialInternal(name, color, emissive);

    public static Material GetOrCreateEmissiveMaterial(string name, Color albedo, Color emission)
    {
        Material mat = GetOrCreateMaterialInternal(name, albedo, true);
        mat.SetColor("_EmissionColor", emission);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    public static Material GetOrCreateTransparentMaterial(string name, Color color, bool emissive)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find(kLiminalSurface) ?? Shader.Find(kRetroFlatLit);

        if (mat == null)
        {
            mat = new Material(shader) { name = name };
            EnsureFolderExists(MaterialRoot);
            AssetDatabase.CreateAsset(mat, path);
        }
        else if (mat.shader != shader)
        {
            mat.shader = shader;
        }

        mat.color = color;
        mat.SetFloat("_Surface", 1f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        if (emissive)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 1.4f);
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    public static Color HexColor(string hex, float alpha = 1f)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color c))
        {
            c.a = alpha;
            return c;
        }
        return new Color(1, 0, 1, alpha);
    }

    public static void EnsureFolderExists(string assetPath)
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

    public static bool TryGetMaterial(string canonicalName, out Material mat)
    {
        mat = GetMaterialByName(canonicalName);
        return mat != null;
    }

    private static Material GetMaterialByName(string canonicalName)
    {
        return canonicalName switch
        {
            "Mat_Memory"      => MemoryAmberMat,
            "Mat_WallTeal"    => InstitutionalTealMat,
            "Mat_WallMustard" => FadedMustardMat,
            "Mat_WallSage"    => SageGreenMat,
            "Mat_WallRose"    => DustyRoseMat,
            "Mat_Floor"       => FloorMat,
            "Mat_Architecture"=> ArchMat,
            "Mat_Plate"       => PlateMat,
            "Mat_Bridge"      => BridgeMat,
            "Mat_Door"        => DoorMat,
            "Mat_Exit"        => GoalMat,
            "Mat_Echo"        => EchoMat,
            "Mat_Chalkboard"  => ChalkboardMat,
            "Mat_Book"        => BookMat,
            "Mat_Crack"       => CrackMat,
            _                 => GetMaterial(canonicalName)
        };
    }
}
