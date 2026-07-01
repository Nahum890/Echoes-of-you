using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Utilidades centrales para crear y configurar materiales del Universal Render
/// Pipeline (URP) por código. Reemplaza el patrón Built-in basado en el shader
/// "Standard" y en el _Mode/_ALPHABLEND_ON de transparencia.
///
/// Es un script de runtime (no de Editor) a propósito, para que tanto los
/// builders del Editor como el código de juego puedan reutilizarlo.
/// </summary>
public static class EchoesUrpMaterials
{
    /// Shader Lit de URP (equivalente a "Standard" del Built-in).
    public const string LitShaderName = "Universal Render Pipeline/Lit";

    /// Shader Unlit de URP (para efectos planos/partículas que antes usaban Unlit).
    public const string UnlitShaderName = "Universal Render Pipeline/Unlit";

    public static Shader LitShader => Shader.Find(LitShaderName);

    /// Crea un material URP/Lit opaco con color base. `material.color` mapea a
    /// _BaseColor en URP/Lit gracias al atributo [MainColor].
    public static Material CreateLit(Color color, float metallic = 0f, float smoothness = 0.05f)
    {
        var m = new Material(LitShader);
        m.color = color;
        m.SetFloat("_Metallic", metallic);
        m.SetFloat("_Smoothness", smoothness);
        return m;
    }

    /// Configura un material URP/Lit existente como transparente (alpha blend).
    /// Equivalente al SetupTransparentMaterial del Built-in (_Mode=3), pero usando
    /// el modelo de "surface type" de URP.
    public static void MakeTransparent(Material m)
    {
        if (m == null) return;
        m.SetFloat("_Surface", 1f); // 0 = Opaque, 1 = Transparent
        m.SetFloat("_Blend", 0f);   // 0 = Alpha
        m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)RenderQueue.Transparent; // 3000
    }

    /// Crea un material URP/Lit transparente con color (incluye alpha).
    public static Material CreateTransparent(Color color)
    {
        var m = new Material(LitShader);
        m.color = color;
        MakeTransparent(m);
        return m;
    }

    /// Activa emisión en un material URP/Lit.
    public static void SetEmission(Material m, Color emission)
    {
        if (m == null) return;
        m.EnableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", emission);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
    }
}
