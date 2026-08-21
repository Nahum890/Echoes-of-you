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

        public Surface(string material, string texture, float tileX, float tileY, string reason)
        {
            this.material = material;
            this.texture = texture;
            this.tiling = new Vector2(tileX, tileY);
            this.reason = reason;
        }
    }

    // El tiling es por CARA, no por metro: los cubos de Unity tienen UV 0..1 en
    // cada cara. Para la geometria que escala mucho esta el componente
    // KenneyTiling, que lo recalcula segun el tamano real del objeto.
    private static readonly Surface[] Surfaces =
    {
        new Surface("Mat_Arch_Locker",   "tex_locker_metal_128",  1f, 1f, "taquillas: chapa pintada, una puerta por cara"),
        new Surface("Mat_Arch_Metal",    "tex_locker_metal_128",  2f, 2f, "herrajes y barandillas"),
        new Surface("Mat_Arch_Seating",  "tex_school_wood_128",   2f, 1f, "bancos de pasillo"),
        new Surface("Mat_Arch_Stairs",   "tex_linoleum_floor_128", 3f, 3f, "peldanos: mismo linoleo que el suelo"),
        new Surface("Mat_Arch_Column",   "tex_plaster_wall_128",  1f, 3f, "columnas: yeso, pero estirado en vertical"),
        new Surface("Mat_Arch_Clock",    "tex_plaster_wall_128",  1f, 1f, "reloj de pared: yeso sin estirar"),
        new Surface("Mat_Door",          "tex_door_painted_128",  1f, 1f, "puertas de aula con ventanuco"),
        new Surface("Mat_CorkBoard",     "tex_cork_board_128",    1f, 1f, "tablon de anuncios"),
        new Surface("Mat_Metal",         "tex_locker_metal_128",  1f, 1f, "metal generico"),
        new Surface("Mat_LiminalCeiling","tex_ceiling_tile_128",  4f, 4f, "placas de techo acustico"),
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

            // Con textura, el color base tiene que ser blanco o multiplica dos
            // veces y la superficie sale mucho mas oscura de lo autorizado.
            if (mat.HasProperty("_BaseColor"))
            {
                Color c = mat.GetColor("_BaseColor");
                mat.SetColor("_BaseColor", new Color(1f, 1f, 1f, c.a));
            }

            EditorUtility.SetDirty(mat);
            applied.Add($"{surface.material} <- {surface.texture} @ {surface.tiling.x}x{surface.tiling.y} ({surface.reason})");
        }

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
