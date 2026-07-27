using System.IO;
using UnityEditor;
using UnityEngine;

public static class LoFiTextureGenerator
{
    private const string TargetFolder = "Assets/Textures/LoFi";

    [MenuItem("Echoes of You/Art/Generate Lo-Fi Textures")]
    public static void GenerateAllTextures()
    {
        if (!Directory.Exists(TargetFolder))
        {
            Directory.CreateDirectory(TargetFolder);
            AssetDatabase.Refresh();
        }

        CreateSchoolWood();
        CreateLinoleumFloor();
        CreateChalkboard();
        CreateCorkBoard();
        CreatePlasterWall();

        AssetDatabase.Refresh();

        ConfigureImporter("tex_school_wood_128.png", 128);
        ConfigureImporter("tex_linoleum_floor_128.png", 128);
        ConfigureImporter("tex_chalkboard_256.png", 256);
        ConfigureImporter("tex_cork_board_128.png", 128);
        ConfigureImporter("tex_plaster_wall_128.png", 128);

        AssetDatabase.SaveAssets();
        Debug.Log("[Lo-Fi Textures] Successfully generated and configured 5 lo-fi textures.");
    }

    private static void CreateSchoolWood()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
        Color baseColor = HexToColor("5A4A2E");

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float grain = Mathf.Sin(x * 0.15f + Mathf.PerlinNoise(x * 0.05f, y * 0.2f) * 6.0f) * 0.06f;
                float noise = (Random.value - 0.5f) * 0.08f;
                float v = Mathf.Clamp01(1.0f + grain + noise);
                tex.SetPixel(x, y, new Color(baseColor.r * v, baseColor.g * v, baseColor.b * v, 1f));
            }
        }
        tex.Apply();
        SavePng(tex, "tex_school_wood_128.png");
    }

    private static void CreateLinoleumFloor()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
        Color baseColor = HexToColor("1C2430");
        Color seamColor = HexToColor("0D1218");

        int tileSize = 32;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isSeam = (x % tileSize == 0) || (y % tileSize == 0);
                float noise = (Random.value - 0.5f) * 0.06f;
                Color col = isSeam ? seamColor : baseColor;
                float v = Mathf.Clamp01(1.0f + noise);
                tex.SetPixel(x, y, new Color(col.r * v, col.g * v, col.b * v, 1f));
            }
        }
        tex.Apply();
        SavePng(tex, "tex_linoleum_floor_128.png");
    }

    private static void CreateChalkboard()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
        Color baseColor = HexToColor("22362A");
        Color chalkColor = HexToColor("D8DFD0");

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float smudge = Mathf.PerlinNoise(x * 0.03f, y * 0.03f);
                float fineChalk = (Random.value > 0.85f) ? Random.value * 0.15f : 0f;
                float chalkBlend = Mathf.Clamp01((smudge > 0.55f ? (smudge - 0.55f) * 0.4f : 0f) + fineChalk);

                Color col = Color.Lerp(baseColor, chalkColor, chalkBlend);
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        SavePng(tex, "tex_chalkboard_256.png");
    }

    private static void CreateCorkBoard()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
        Color corkBase = HexToColor("7C5A38");
        Color corkDark = HexToColor("4D3620");

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n = Random.value;
                Color col = Color.Lerp(corkDark, corkBase, n);
                if ((x == 20 && y == 30) || (x == 80 && y == 100) || (x == 110 && y == 40))
                {
                    col = HexToColor("1A1008");
                }
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        SavePng(tex, "tex_cork_board_128.png");
    }

    private static void CreatePlasterWall()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
        Color baseColor = HexToColor("2B4A4A");

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noise = (Random.value - 0.5f) * 0.05f;
                float v = Mathf.Clamp01(1.0f + noise);
                tex.SetPixel(x, y, new Color(baseColor.r * v, baseColor.g * v, baseColor.b * v, 1f));
            }
        }
        tex.Apply();
        SavePng(tex, "tex_plaster_wall_128.png");
    }

    private static void SavePng(Texture2D tex, string fileName)
    {
        byte[] bytes = tex.EncodeToPNG();
        string path = Path.Combine(TargetFolder, fileName);
        File.WriteAllBytes(path, bytes);
        Object.DestroyImmediate(tex);
    }

    private static void ConfigureImporter(string fileName, int maxSize)
    {
        string path = Path.Combine(TargetFolder, fileName).Replace("\\", "/");
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.sRGBTexture = true;
            importer.maxTextureSize = maxSize;
            importer.SaveAndReimport();
        }
    }

    private static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color c))
            return c;
        return Color.white;
    }
}
