using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Renderer Feature que aplica el framebuffer PS1 (dither ordenado + cuantizacion
/// de color + scanlines) sobre toda la imagen.
///
/// Va DESPUES del post-processing canonico (SPEC-144) a proposito: si se cuantiza
/// antes, el color grading vuelve a estirar los valores y las bandas desaparecen.
/// Y va ANTES del upscale, porque el render target activo en ese punto sigue a la
/// resolucion interna que marca RenderScale en Echoes_URPAsset.
///
/// Los valores viven aqui (en el renderer) y se empujan al material cada frame,
/// asi hay una unica autoridad y no dos sitios que se contradicen.
/// </summary>
[DisallowMultipleRendererFeature("Echoes PS1 Post")]
public class PS1PostFeature : ScriptableRendererFeature
{
    [Serializable]
    public class Settings
    {
        [Tooltip("Bits por canal. La PS1 usaba 5 (15-bit color).")]
        [Range(1f, 8f)] public float colorDepth = 5f;

        [Tooltip("0 = banding puro, 1 = dither ordenado 4x4 completo (como el hardware).")]
        [Range(0f, 1f)] public float ditherStrength = 1f;

        [Tooltip("Tamano de cada celda del dither en pixeles internos.")]
        [Range(1f, 4f)] public float ditherScale = 1f;

        [Tooltip("Oscurecimiento de las lineas de scan. 0 las desactiva.")]
        [Range(0f, 1f)] public float scanlineStrength = 0.12f;

        [Tooltip("Cada cuantos pixeles internos cae una linea oscura.")]
        [Range(2f, 8f)] public float scanlinePeriod = 2f;

        [Tooltip("Mostrar el efecto tambien en la Scene View del editor.")]
        public bool applyInSceneView = false;

        public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;
    }

    private static class ShaderIDs
    {
        public static readonly int ColorDepth = Shader.PropertyToID("_ColorDepth");
        public static readonly int DitherStrength = Shader.PropertyToID("_DitherStrength");
        public static readonly int DitherScale = Shader.PropertyToID("_DitherScale");
        public static readonly int ScanlineStrength = Shader.PropertyToID("_ScanlineStrength");
        public static readonly int ScanlinePeriod = Shader.PropertyToID("_ScanlinePeriod");
    }

    public Settings settings = new Settings();

    [SerializeField, HideInInspector]
    private Shader m_Shader;

    private Material m_Material;
    private PS1PostPass m_Pass;
    private bool m_WarnedMissingShader;

    public override void Create()
    {
        if (m_Shader == null)
        {
            m_Shader = Shader.Find("Echoes/PS1Post");
        }

        m_Pass = new PS1PostPass
        {
            renderPassEvent = settings.injectionPoint
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_Shader == null)
        {
            if (!m_WarnedMissingShader)
            {
                Debug.LogWarning("[Echoes PS1] No se encontro el shader 'Echoes/PS1Post'. La feature queda inactiva.");
                m_WarnedMissingShader = true;
            }
            return;
        }

        if (m_Material == null)
        {
            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);
        }

        m_Material.SetFloat(ShaderIDs.ColorDepth, settings.colorDepth);
        m_Material.SetFloat(ShaderIDs.DitherStrength, settings.ditherStrength);
        m_Material.SetFloat(ShaderIDs.DitherScale, settings.ditherScale);
        m_Material.SetFloat(ShaderIDs.ScanlineStrength, settings.scanlineStrength);
        m_Material.SetFloat(ShaderIDs.ScanlinePeriod, settings.scanlinePeriod);

        m_Pass.renderPassEvent = settings.injectionPoint;
        m_Pass.Setup(m_Material, settings.applyInSceneView);
        renderer.EnqueuePass(m_Pass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_Material);
        m_Material = null;
    }

    private class PS1PostPass : ScriptableRenderPass
    {
        private const string PassName = "Echoes PS1 Framebuffer";

        private Material m_Material;
        private bool m_ApplyInSceneView;

        public void Setup(Material material, bool applyInSceneView)
        {
            m_Material = material;
            m_ApplyInSceneView = applyInSceneView;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_Material == null)
            {
                return;
            }

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            if (cameraData.cameraType == CameraType.Preview || cameraData.cameraType == CameraType.Reflection)
            {
                return;
            }

            if (cameraData.cameraType == CameraType.SceneView && !m_ApplyInSceneView)
            {
                return;
            }

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            if (resourceData.isActiveTargetBackBuffer)
            {
                // Sin textura intermedia no se puede leer y escribir el color en el
                // mismo pase. requiresIntermediateTexture deberia evitarlo, pero si
                // el frame setup lo fuerza igualmente, es mejor saltar que corromper.
                return;
            }

            RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;

            TextureHandle source = resourceData.activeColorTexture;

            // FilterMode.Point explicito y NO por defecto: FinalBlitPass elige entre
            // sampler nearest y bilinear leyendo el filterMode de esta RT, no el
            // ajuste UpscalingFilter del URP asset. Si esto acaba en Bilinear, el
            // upscale difumina los pixeles y el efecto entero se pierde.
            TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, descriptor, "_EchoesPS1PostTarget", false, FilterMode.Point);

            RenderGraphUtils.BlitMaterialParameters blit =
                new RenderGraphUtils.BlitMaterialParameters(source, destination, m_Material, 0);
            renderGraph.AddBlitPass(blit, PassName);

            resourceData.cameraColor = destination;
        }
    }
}
