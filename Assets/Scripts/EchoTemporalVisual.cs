using UnityEngine;

/// <summary>
/// Broken-memory look for echoes: flicker, unstable edges, desaturated ghost tones.
/// </summary>
[DisallowMultipleComponent]
public class EchoTemporalVisual : MonoBehaviour
{
    [SerializeField] Transform visualRoot;
    [SerializeField] float flickerSpeed = 14f;
    [SerializeField] float glitchInterval = 0.11f;
    [SerializeField] Color echoTint = new Color(0.78f, 0.86f, 0.92f, 0.72f);
    [SerializeField] Color glitchTint = new Color(0.35f, 0.95f, 1f, 0.85f);

    Renderer[] _renderers;
    MaterialPropertyBlock _block;
    float _nextGlitch;
    float _glitchUntil;

    void Awake()
    {
        if (visualRoot == null)
        {
            Transform visual = transform.Find("Visual") ?? transform.Find("PlayerVisual");
            visualRoot = visual != null ? visual : transform;
        }

        _renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        _block = new MaterialPropertyBlock();
        CacheMaterials();
    }

    void CacheMaterials()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer r = _renderers[i];
            if (r == null)
                continue;

            Material[] mats = r.sharedMaterials;
            for (int m = 0; m < mats.Length; m++)
            {
                if (mats[m] == null)
                    continue;
                SetupTransparentEchoMaterial(mats[m]);
            }
        }
    }

    void LateUpdate()
    {
        if (Time.time >= _nextGlitch)
        {
            _nextGlitch = Time.time + Random.Range(glitchInterval * 0.45f, glitchInterval * 1.35f);
            _glitchUntil = Time.time + Random.Range(0.02f, 0.06f);
        }

        UpdateFlickerAndGlitch();
    }

    void UpdateFlickerAndGlitch()
    {
        float pulse = Mathf.PerlinNoise1D(Time.time * flickerSpeed) * 0.5f + 0.5f;
        bool glitching = Time.time <= _glitchUntil;
        Color tint = glitching ? glitchTint : echoTint;
        float alpha = Mathf.Lerp(0.38f, 0.82f, pulse) * (glitching ? 1f : 0.85f);

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer r = _renderers[i];
            if (r == null)
                continue;

            r.GetPropertyBlock(_block);
            _block.SetColor("_BaseColor", new Color(tint.r, tint.g, tint.b, alpha));
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_EmissionColor"))
                _block.SetColor("_EmissionColor", tint * (glitching ? 1.8f : 0.55f));
            r.SetPropertyBlock(_block);
        }
    }

    static void SetupTransparentEchoMaterial(Material material)
    {
        if (!material.HasProperty("_Color"))
            return;

        material.SetFloat("_Surface", 1f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        if (material.HasProperty("_EmissionColor"))
            material.EnableKeyword("_EMISSION");
    }
}
