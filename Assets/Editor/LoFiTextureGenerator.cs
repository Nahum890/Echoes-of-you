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
        CreateLockerMetal();
        CreateCeilingTile();
        CreateDoorPainted();

        AssetDatabase.Refresh();

        ConfigureImporter("tex_school_wood_128.png", 128);
        ConfigureImporter("tex_linoleum_floor_128.png", 128);
        ConfigureImporter("tex_chalkboard_256.png", 256);
        ConfigureImporter("tex_cork_board_128.png", 128);
        ConfigureImporter("tex_plaster_wall_128.png", 128);
        ConfigureImporter("tex_locker_metal_128.png", 128);
        ConfigureImporter("tex_ceiling_tile_128.png", 128);
        ConfigureImporter("tex_door_painted_128.png", 128);

        AssetDatabase.SaveAssets();
        Debug.Log("[Lo-Fi Textures] Successfully generated and configured 8 lo-fi textures.");
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

    // Taquillas: chapa pintada con junta vertical por puerta, rejilla de
    // ventilacion arriba y tirador. Es el elemento que mas dice "escuela".
    private static void CreateLockerMetal()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
        Color body = HexToColor("3E5148");
        Color seam = HexToColor("1A231F");
        Color vent = HexToColor("222D28");
        Color handle = HexToColor("6E7A72");

        int doorWidth = 64;   // dos puertas por tile
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int xInDoor = x % doorWidth;
                Color col = body;

                // Junta entre puertas y marco superior/inferior.
                bool isSeam = xInDoor < 2 || xInDoor > doorWidth - 3 || y < 2 || y > size - 3;

                // Rejilla de ventilacion en el tercio superior de cada puerta.
                bool isVent = y > size - 40 && y < size - 12 &&
                              (y % 6 < 3) && xInDoor > 12 && xInDoor < doorWidth - 12;

                // Tirador vertical en el lado derecho de cada puerta.
                bool isHandle = xInDoor > doorWidth - 14 && xInDoor < doorWidth - 9 &&
                                y > size / 2 - 14 && y < size / 2 + 6;

                if (isSeam) col = seam;
                else if (isVent) col = vent;
                else if (isHandle) col = handle;

                // Roces y abolladuras.
                float scuff = (Random.value - 0.5f) * 0.07f;
                float dent = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.10f - 0.05f;
                float v = Mathf.Clamp01(1.0f + scuff + dent);
                tex.SetPixel(x, y, new Color(col.r * v, col.g * v, col.b * v, 1f));
            }
        }
        tex.Apply();
        SavePng(tex, "tex_locker_metal_128.png");
    }

    // Placa de techo acustico con perfil en T y perforaciones. El otro gran
    // significante institucional, y el techo hoy no tiene textura ninguna.
    private static void CreateCeilingTile()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
        Color panel = HexToColor("6B6A63");
        Color grid = HexToColor("4A4A45");
        Color hole = HexToColor("53534D");

        int tileSize = 64;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int xi = x % tileSize;
                int yi = y % tileSize;
                Color col = panel;

                bool isGrid = xi < 2 || yi < 2;
                // Picado disperso. Con aritmetica modular (xi*7+yi*13)%11 salia
                // una reticula diagonal que se leia como rayado, no como placa
                // acustica: hace falta un hash de verdad. Estable entre
                // regeneraciones porque no usa Random.
                bool isHole = !isGrid && Hash01(xi >> 1, yi >> 1) > 0.80f;

                if (isGrid) col = grid;
                else if (isHole) col = hole;

                // Manchas de humedad: es un techo de escuela abandonada.
                float damp = Mathf.PerlinNoise(x * 0.04f, y * 0.04f);
                float stain = damp > 0.62f ? -(damp - 0.62f) * 0.9f : 0f;
                float v = Mathf.Clamp01(1.0f + stain + (Random.value - 0.5f) * 0.04f);
                tex.SetPixel(x, y, new Color(col.r * v, col.g * v, col.b * v, 1f));
            }
        }
        tex.Apply();
        SavePng(tex, "tex_ceiling_tile_128.png");
    }

    // Puerta de aula: madera pintada con panel rehundido y ventanuco.
    private static void CreateDoorPainted()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
        Color paint = HexToColor("4A4038");
        Color groove = HexToColor("2A241F");
        Color glass = HexToColor("303A3E");

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Color col = paint;

                // Dos paneles rehundidos (uno alto, uno bajo) separados por el
                // travesano central: con un solo panel la puerta quedaba plana.
                bool lowerPanel = x > 16 && x < size - 16 && y > 10 && y < 52;
                bool upperPanel = x > 16 && x < size - 16 && y > 60 && y < size - 46;
                bool panelEdge = (lowerPanel && (x < 19 || x > size - 19 || y < 13 || y > 49)) ||
                                 (upperPanel && (x < 19 || x > size - 19 || y < 63 || y > size - 49));

                // Ventanuco superior con vidrio armado.
                bool inGlass = x > 26 && x < size - 26 && y > size - 40 && y < size - 12;
                bool glassFrame = inGlass && (x < 29 || x > size - 29 || y < size - 37 || y > size - 15);

                // Borde exterior de la hoja.
                bool doorEdge = x < 3 || x > size - 4 || y < 3 || y > size - 4;

                if (doorEdge || panelEdge || glassFrame) col = groove;
                else if (inGlass) col = glass;

                // Los paneles rehundidos reciben algo menos de luz.
                float recess = (lowerPanel || upperPanel) && !panelEdge ? -0.07f : 0f;
                float wear = (Random.value - 0.5f) * 0.06f;
                float v = Mathf.Clamp01(1.0f + wear + recess);
                tex.SetPixel(x, y, new Color(col.r * v, col.g * v, col.b * v, 1f));
            }
        }
        tex.Apply();
        SavePng(tex, "tex_door_painted_128.png");
    }

    /// <summary>Hash determinista en [0,1). No usa Random para que el patron
    /// generado sea identico cada vez que se regenera la textura.</summary>
    private static float Hash01(int x, int y)
    {
        int h = x * 374761393 + y * 668265263;
        h = (h ^ (h >> 13)) * 1274126177;
        return ((h ^ (h >> 16)) & 0x7FFFFFF) / (float)0x7FFFFFF;
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
