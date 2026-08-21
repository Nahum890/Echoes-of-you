using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Asigna a cada material de arquitectura la superficie que le corresponde.
///
/// El problema que resuelve: toda la familia Mat_Arch_* llevaba la MISMA textura
/// de yeso de pared (tex_plaster_wall_128) con tiling 2x4. Es decir, las
/// taquillas, los bancos, las escaleras, las columnas y hasta el reloj estaban
/// texturizados como si fueran un trozo de pared. Sin diferenciacion de
/// superficie no hay lectura de "escuela", por muchos materiales que haya.
///
/// Estos materiales no los crea ningun script (a diferencia de los Mat_Token_*),
/// asi que EchoesMaterialLibrary.EnsureMaterials() no los pisa y este pase es
/// idempotente: se puede volver a correr cuando se quiera.
/// </summary>
public static class EchoesSchoolSurfacePass
{
    private const string MaterialRoot = "Assets/Materials/Echoes";
    private const string TextureRoot = "Assets/Textures/LoFi";

    // Orden de preferencia: los shaders Echoes/* usan _BaseTex, el de techo usa
    // _CeilingTex, y URP Lit usa _BaseMap.
    private static readonly string[] TextureProperties = { "_BaseTex", "_CeilingTex", "_BaseMap", "_MainTex" };

    private struct Surface
    {
        public string material;
        public string texture;
        public Vector2 tiling;
        public string reason;

        /// <summary>
        /// true: la textura trae su propio color y el _BaseColor debe ser blanco
        /// (taquillas, puertas). false: la textura es neutra y el _BaseColor del
        /// material la tine — es lo que conserva la identidad de color de cada
        /// capitulo en las paredes.
        /// </summary>
        public bool whiteBase;

        public Surface(string material, string texture, float tileX, float tileY, bool whiteBase, string reason)
        {
            this.material = material;
            this.texture = texture;
            this.tiling = new Vector2(tileX, tileY);
            this.whiteBase = whiteBase;
            this.reason = reason;
        }
    }

    // El tiling es por CARA, no por metro: los cubos de Unity tienen UV 0..1 en
    // cada cara. Para la geometria que escala mucho esta el componente
    // KenneyTiling, que lo recalcula segun el tamano real del objeto.
    private static readonly Surface[] Surfaces =
    {
        // Mobiliario y carpinteria: la textura manda el color.
        new Surface("Mat_Arch_Locker",   "tex_locker_metal_128",   1f, 1f, true,  "taquillas: chapa pintada, una puerta por cara"),
        new Surface("Mat_Arch_Metal",    "tex_locker_metal_128",   2f, 2f, true,  "herrajes y barandillas"),
        new Surface("Mat_Arch_Seating",  "tex_school_wood_128",    2f, 1f, true,  "bancos de pasillo"),
        new Surface("Mat_Arch_Stairs",   "tex_linoleum_floor_128", 3f, 3f, true,  "peldanos: mismo linoleo que el suelo"),
        new Surface("Mat_Door",          "tex_door_painted_128",   1f, 1f, true,  "puertas de aula con ventanuco"),
        new Surface("Mat_CorkBoard",     "tex_cork_board_128",     1f, 1f, true,  "tablon de anuncios"),
        new Surface("Mat_Metal",         "tex_locker_metal_128",   1f, 1f, true,  "metal generico"),
        new Surface("Mat_LiminalCeiling","tex_ceiling_tile_128",   4f, 4f, true,  "placas de techo acustico"),

        // Paredes: yeso NEUTRO tenido por el _BaseColor de cada material, para
        // no perder el color de capitulo. Son las que mas superficie ocupan en
        // las escenas (Mat_WallTeal aparece en 20 renderers de Level_06) y
        // estaban SIN textura ninguna.
        new Surface("Mat_WallTeal",      "tex_plaster_neutral_128", 2f, 4f, false, "pared teal (Ch I-II)"),
        new Surface("Mat_WallRose",      "tex_plaster_neutral_128", 2f, 4f, false, "pared rosa"),
        new Surface("Mat_WallMustard",   "tex_plaster_neutral_128", 2f, 4f, false, "pared mostaza"),
        new Surface("Mat_WallSage",      "tex_plaster_neutral_128", 2f, 4f, false, "pared sage"),
        new Surface("Mat_Arch_Column",   "tex_plaster_neutral_128", 1f, 3f, false, "columnas: yeso estirado en vertical"),
        new Surface("Mat_Arch_Clock",    "tex_plaster_neutral_128", 1f, 1f, false, "reloj de pared"),
        new Surface("Mat_PlasterWall",   "tex_plaster_neutral_128", 2f, 4f, false, "yeso generico"),

        // Variantes liminales que quedaron a medias: cuatro tenian textura y
        // estas dos no, sin razon aparente.
        new Surface("Mat_LiminalPlaster_Teal", "tex_plaster_neutral_128", 2f, 4f, false, "variante liminal teal"),
        new Surface("Mat_LiminalPlaster_Void", "tex_plaster_neutral_128", 2f, 4f, false, "variante liminal void"),

        // Las otras cuatro variantes liminales SI tenian textura, pero la
        // generica Tex_Liminal_Plaster_512, que no lee como yeso de escuela.
        // Son 65 usos repartidos por el bloque jugable.
        new Surface("Mat_LiminalPlaster_Navy",    "tex_plaster_neutral_128", 2f, 4f, false, "variante liminal navy"),
        new Surface("Mat_LiminalPlaster_Mustard", "tex_plaster_neutral_128", 2f, 4f, false, "variante liminal mostaza"),
        new Surface("Mat_LiminalPlaster_Rose",    "tex_plaster_neutral_128", 2f, 4f, false, "variante liminal rosa"),
        new Surface("Mat_LiminalPlaster_Sage",    "tex_plaster_neutral_128", 2f, 4f, false, "variante liminal sage"),

        // Estos dos llevaban tex_plaster_wall_128, que trae color teal PROPIO
        // (2B4A4A). Con _BaseColor blanco tenia de teal toda la arquitectura de
        // los seis capitulos. Con la neutra + whiteBase=false cada uno conserva
        // su color: Mat_Architecture es gris (0.353) y sale yeso neutro, y
        // institutional-teal es teal por _BaseColor, no por textura.
        new Surface("Mat_Architecture",             "tex_plaster_neutral_128", 2f, 4f, false, "arquitectura generica: 95 usos en el bloque"),
        new Surface("Mat_Token_institutional-teal", "tex_plaster_neutral_128", 2f, 4f, false, "token teal usado como superficie"),

        // Suelos. Ya tenian el linoleo asignado, pero al no estar en la tabla
        // su tiling no lo gestionaba nadie, y es la superficie que mas pantalla
        // ocupa. KenneyTiling lo reescala luego segun el tamano real.
        new Surface("Mat_LinoleumFloor", "tex_linoleum_floor_128", 4f, 4f, false, "suelo de linoleo: 49 usos en el bloque"),
        new Surface("Mat_Floor",         "tex_linoleum_floor_128", 4f, 4f, false, "suelo generico"),
    };

    [MenuItem("Echoes of You/Art/Apply School Surfaces", false, 30)]
    public static void ApplySchoolSurfaces()
    {
        var applied = new List<string>();
        var skipped = new List<string>();

        foreach (Surface surface in Surfaces)
        {
            string matPath = $"{MaterialRoot}/{surface.material}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                skipped.Add($"{surface.material}: no existe en {MaterialRoot}");
                continue;
            }

            string texPath = $"{TextureRoot}/{surface.texture}.png";
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(texPath);
            if (tex == null)
            {
                skipped.Add($"{surface.material}: falta {texPath} " +
                            "(corre antes 'Echoes of You/Art/Generate Lo-Fi Textures')");
                continue;
            }

            string prop = FindTextureProperty(mat);
            if (prop == null)
            {
                skipped.Add($"{surface.material}: el shader '{mat.shader?.name}' no expone " +
                            "ninguna propiedad de textura conocida");
                continue;
            }

            mat.SetTexture(prop, tex);
            mat.SetTextureScale(prop, surface.tiling);
            mat.SetTextureOffset(prop, Vector2.zero);

            // Solo se blanquea el color cuando la textura ya trae el suyo. En las
            // paredes hay que CONSERVARLO: es lo que las distingue por capitulo.
            if (surface.whiteBase && mat.HasProperty("_BaseColor"))
            {
                Color c = mat.GetColor("_BaseColor");
                mat.SetColor("_BaseColor", new Color(1f, 1f, 1f, c.a));
            }

            EditorUtility.SetDirty(mat);
            applied.Add($"{surface.material} <- {surface.texture} @ {surface.tiling.x}x{surface.tiling.y} ({surface.reason})");
        }

        ConfigureLightFixtures(applied, skipped);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[School Surfaces] Aplicados {applied.Count}/{Surfaces.Length}:\n  " + string.Join("\n  ", applied));
        if (skipped.Count > 0)
        {
            Debug.LogWarning($"[School Surfaces] Saltados {skipped.Count}:\n  " + string.Join("\n  ", skipped));
        }
    }

    /// <summary>Corre el generador de texturas y despues el pase, en el orden correcto.</summary>
    [MenuItem("Echoes of You/Art/Apply School Surfaces (regenerando texturas)", false, 31)]
    public static void RegenerateAndApply()
    {
        LoFiTextureGenerator.GenerateAllTextures();
        ApplySchoolSurfaces();
    }

    /// <summary>
    /// Hace que los fluorescentes se vean COMO fuente de luz, no solo su efecto.
    /// Mat_Fluorescent no tenia emision, asi que los tubos eran geometria gris
    /// oscura con una point light invisible al lado: se veia el charco de luz en
    /// el suelo pero no de donde salia.
    /// </summary>
    private static void ConfigureLightFixtures(List<string> applied, List<string> skipped)
    {
        // Blanco verdoso de tubo fluorescente viejo, por encima de 1 para que
        // el bloom lo recoja como fuente.
        var fixtures = new (string material, Color emission)[]
        {
            ("Mat_Fluorescent", new Color(0.86f, 0.92f, 0.80f) * 2.2f),
        };

        foreach (var (name, emission) in fixtures)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/{name}.mat");
            if (mat == null)
            {
                skipped.Add($"{name}: no existe");
                continue;
            }

            if (!mat.HasProperty("_EmissionColor"))
            {
                skipped.Add($"{name}: el shader '{mat.shader?.name}' no expone _EmissionColor");
                continue;
            }

            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(mat);
            applied.Add($"{name} <- emision (fuente de luz visible)");
        }
    }

    private static string FindTextureProperty(Material mat)
    {
        foreach (string prop in TextureProperties)
        {
            if (mat.HasProperty(prop))
            {
                return prop;
            }
        }
        return null;
    }
}
