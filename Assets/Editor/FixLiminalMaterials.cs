using UnityEngine;
using UnityEditor;

public class FixLiminalMaterials
{
    [MenuItem("Echoes of You/Production/Fix Liminal Material Textures")]
    public static void FixAll()
    {
        int fixedCount = 0;

        // LiminalSurface materials use _BaseTex
        // URP/Lit materials use _BaseMap

        fixedCount += FixMaterial("Assets/Materials/Echoes/Mat_Architecture.mat", "Assets/Textures/LoFi/tex_plaster_wall_128.png", "_BaseTex", new Vector2(2, 4));
        fixedCount += FixMaterial("Assets/Materials/Echoes/Mat_Floor.mat", "Assets/Textures/LoFi/tex_linoleum_floor_128.png", "_BaseMap", new Vector2(5, 5));
        fixedCount += FixMaterial("Assets/Materials/Echoes/Mat_Book.mat", "Assets/Textures/LoFi/tex_school_wood_128.png", "_BaseTex", Vector2.one);
        fixedCount += FixMaterial("Assets/Materials/Echoes/Mat_Chalkboard.mat", "Assets/Textures/LoFi/tex_chalkboard_256.png", "_BaseTex", Vector2.one);
        fixedCount += FixMaterial("Assets/Materials/Echoes/Mat_Cork.mat", "Assets/Textures/LoFi/tex_cork_board_128.png", "_BaseTex", Vector2.one);

        // Also fix Mat_Wall if it exists
        FixWallMaterial();

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Materials Fixed", $"Fixed {fixedCount} liminal materials with textures.", "OK");
    }

    static int FixMaterial(string matPath, string texPath, string texProp, Vector2 tiling)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null) { Debug.LogWarning($"Material not found: {matPath}"); return 0; }

        var tex = AssetDatabase.LoadAssetAtPath<Texture>(texPath);
        if (tex == null) { Debug.LogWarning($"Texture not found: {texPath}"); return 0; }

        if (mat.HasProperty(texProp))
        {
            mat.SetTexture(texProp, tex);
            mat.SetTextureScale(texProp, tiling);
        }
        else
        {
            // Try _BaseMap for URP/Lit, _BaseTex for LiminalSurface, _MainTex fallback
            if (mat.HasProperty("_BaseMap")) { mat.SetTexture("_BaseMap", tex); mat.SetTextureScale("_BaseMap", tiling); }
            else if (mat.HasProperty("_BaseTex")) { mat.SetTexture("_BaseTex", tex); mat.SetTextureScale("_BaseTex", tiling); }
            else if (mat.HasProperty("_MainTex")) { mat.SetTexture("_MainTex", tex); mat.SetTextureScale("_MainTex", tiling); }
        }

        EditorUtility.SetDirty(mat);
        Debug.Log($"[FixMaterials] {matPath} <- {texPath} (prop={texProp}, tiling={tiling})");
        return 1;
    }

    static void FixWallMaterial()
    {
        // Check if Mat_Wall exists
        var wall = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Echoes/Mat_Wall.mat");
        if (wall != null)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Textures/LoFi/tex_plaster_wall_128.png");
            string prop = wall.HasProperty("_BaseTex") ? "_BaseTex" : (wall.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex");
            wall.SetTexture(prop, tex);
            wall.SetTextureScale(prop, new Vector2(2, 4));
            EditorUtility.SetDirty(wall);
        }
    }
}
