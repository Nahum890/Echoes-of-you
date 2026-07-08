// ============================================================
// TextureCategoryRules.cs
// Echoes of You — URP Texture Optimization System
// Categorizes textures by path/name and returns target settings
// ============================================================

using UnityEditor;
using UnityEngine;

namespace EchoesEditor
{
    public enum TextureCategory
    {
        Environment,    // Walls, floors, architecture
        PropSmall,      // Furniture, small objects
        Character,      // Player, NPC textures
        ParticleVFX,    // Particle effects
        UI,             // UI images, backgrounds
        NormalMap,      // Normal map textures
        HDRISkybox,     // EXR HDRIs / skyboxes
        Unknown
    }

    [System.Serializable]
    public class TextureTargetSettings
    {
        public int maxSize;
        public TextureImporterCompression compression;
        public bool mipmapEnabled;
        public FilterMode filterMode;
        public int anisotropicLevel;
        public bool streamingMipmaps;
        public TextureImporterType textureType;
        public bool srgb;
        public string description;
    }

    public static class TextureCategoryRules
    {
        // -------------------------------------------------------
        // Classify a texture asset by its path and name
        // -------------------------------------------------------
        public static TextureCategory Classify(string assetPath)
        {
            string lower = assetPath.ToLowerInvariant();
            string fileName = System.IO.Path.GetFileNameWithoutExtension(lower);

            // --- UI ---
            if (lower.Contains("/ui/") || lower.Contains("\\ui\\") ||
                lower.Contains("ui toolkit") || lower.Contains("uitoolkit"))
                return TextureCategory.UI;

            // --- Normal Maps by name ---
            if (fileName.EndsWith("_normal") || fileName.EndsWith("_nrm") ||
                fileName.EndsWith("_normalgl") || fileName.Contains("_normal_") ||
                fileName.EndsWith("_n") || fileName.EndsWith("_bump"))
                return TextureCategory.NormalMap;

            // --- HDRI / Skybox ---
            if (lower.EndsWith(".exr") || fileName.Contains("_hdri") ||
                fileName.Contains("skybox") || fileName.Contains("_1k") ||
                fileName.Contains("_2k") && lower.EndsWith(".exr"))
                return TextureCategory.HDRISkybox;

            // --- Particles / VFX ---
            if (lower.Contains("particlepack") || lower.Contains("particle") ||
                lower.Contains("vfx") || lower.Contains("effect") ||
                lower.Contains("smoke") || lower.Contains("fire") ||
                lower.Contains("magic") || lower.Contains("weapon"))
                return TextureCategory.ParticleVFX;

            // --- Characters ---
            if (lower.Contains("character") || lower.Contains("player") ||
                lower.Contains("_player") || lower.Contains("superhero") ||
                lower.Contains("t_hair") || lower.Contains("t_eye") ||
                lower.Contains("animated woman") || lower.Contains("lowpoly-character") ||
                lower.Contains("animatedwoman"))
                return TextureCategory.Character;

            // --- Small Props ---
            if (lower.Contains("kenney") || lower.Contains("furniture") ||
                lower.Contains("prop") || lower.Contains("city pack") ||
                lower.Contains("air duct") || lower.Contains("pipe kit") ||
                lower.Contains("cyberpunk game kit"))
                return TextureCategory.PropSmall;

            // --- Environment / Architecture ---
            if (lower.Contains("architecture") || lower.Contains("modular scifi") ||
                lower.Contains("architecture pack") || lower.Contains("mat_wall") ||
                lower.Contains("mat_floor") || lower.Contains("mat_bridge") ||
                lower.Contains("concrete") || lower.Contains("metal054") ||
                lower.Contains("facade"))
                return TextureCategory.Environment;

            return TextureCategory.Unknown;
        }

        // -------------------------------------------------------
        // Return the target import settings for each category
        // -------------------------------------------------------
        public static TextureTargetSettings GetSettings(TextureCategory category)
        {
            switch (category)
            {
                case TextureCategory.Environment:
                    return new TextureTargetSettings
                    {
                        maxSize = 512,
                        compression = TextureImporterCompression.Compressed,
                        mipmapEnabled = true,
                        filterMode = FilterMode.Bilinear,
                        anisotropicLevel = 4,
                        streamingMipmaps = true,
                        textureType = TextureImporterType.Default,
                        srgb = true,
                        description = "Environment — 512, BC7, MipMaps ON, Streaming ON"
                    };

                case TextureCategory.PropSmall:
                    return new TextureTargetSettings
                    {
                        maxSize = 256,
                        compression = TextureImporterCompression.Compressed,
                        mipmapEnabled = true,
                        filterMode = FilterMode.Bilinear,
                        anisotropicLevel = 2,
                        streamingMipmaps = true,
                        textureType = TextureImporterType.Default,
                        srgb = true,
                        description = "Prop Small — 256, DXT, MipMaps ON, Streaming ON"
                    };

                case TextureCategory.Character:
                    return new TextureTargetSettings
                    {
                        maxSize = 512,
                        compression = TextureImporterCompression.Compressed,
                        mipmapEnabled = true,
                        filterMode = FilterMode.Trilinear,
                        anisotropicLevel = 8,
                        streamingMipmaps = false,
                        textureType = TextureImporterType.Default,
                        srgb = true,
                        description = "Character — 512, BC7, MipMaps ON, Streaming OFF"
                    };

                case TextureCategory.ParticleVFX:
                    return new TextureTargetSettings
                    {
                        maxSize = 256,
                        compression = TextureImporterCompression.Compressed,
                        mipmapEnabled = false,
                        filterMode = FilterMode.Bilinear,
                        anisotropicLevel = 1,
                        streamingMipmaps = false,
                        textureType = TextureImporterType.Default,
                        srgb = true,
                        description = "VFX — 256, DXT5, MipMaps OFF, Streaming OFF"
                    };

                case TextureCategory.UI:
                    return new TextureTargetSettings
                    {
                        maxSize = 1024,
                        compression = TextureImporterCompression.Uncompressed,
                        mipmapEnabled = false,
                        filterMode = FilterMode.Bilinear,
                        anisotropicLevel = 1,
                        streamingMipmaps = false,
                        textureType = TextureImporterType.Sprite,
                        srgb = true,
                        description = "UI — 1024, Sin compresión, MipMaps OFF"
                    };

                case TextureCategory.NormalMap:
                    return new TextureTargetSettings
                    {
                        maxSize = 512,
                        compression = TextureImporterCompression.Compressed,
                        mipmapEnabled = true,
                        filterMode = FilterMode.Trilinear,
                        anisotropicLevel = 4,
                        streamingMipmaps = true,
                        textureType = TextureImporterType.NormalMap,
                        srgb = false,
                        description = "Normal Map — 512, BC5, MipMaps ON, Linear"
                    };

                case TextureCategory.HDRISkybox:
                    return new TextureTargetSettings
                    {
                        maxSize = 1024,
                        compression = TextureImporterCompression.Compressed,
                        mipmapEnabled = true,
                        filterMode = FilterMode.Trilinear,
                        anisotropicLevel = 1,
                        streamingMipmaps = false,
                        textureType = TextureImporterType.Default,
                        srgb = false,
                        description = "HDRI — 1024, BC6H, MipMaps ON, Linear"
                    };

                default:
                    return new TextureTargetSettings
                    {
                        maxSize = 512,
                        compression = TextureImporterCompression.Compressed,
                        mipmapEnabled = true,
                        filterMode = FilterMode.Bilinear,
                        anisotropicLevel = 2,
                        streamingMipmaps = false,
                        textureType = TextureImporterType.Default,
                        srgb = true,
                        description = "Unknown — 512, Default compression"
                    };
            }
        }
    }
}
