using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Ajusta iluminación y atmósfera del nivel desde el Inspector.
/// Colócalo en un objeto vacío (p. ej. "LevelLighting") bajo --- ENVIRONMENT ---.
/// </summary>
public class LevelLightingSettings : MonoBehaviour
{
    [Header("Luz direccional")]
    public bool overrideDirectional = true;
    public float directionalIntensity = 0.98f;
    public Color directionalColor = new Color(0.78f, 0.82f, 0.9f, 1f);
    public Vector3 directionalEuler = new Vector3(42f, -35f, 0f);

    [Header("Ambiente global")]
    public bool overrideAmbient = true;
    public Color ambientSky = new Color(0.32f, 0.34f, 0.42f);
    public Color ambientEquator = new Color(0.22f, 0.24f, 0.3f);
    public Color ambientGround = new Color(0.12f, 0.13f, 0.17f);
    [Range(0f, 1f)] public float reflectionIntensity = 0.42f;

    [Header("Niebla")]
    public bool enableFog = true;
    public Color fogColor = new Color(0.18f, 0.2f, 0.28f);
    [Range(0f, 0.02f)] public float fogDensity = 0.0045f;

    [Header("Luces puntuales (hijos de este objeto)")]
    public bool applyToChildPointLights = true;
    [Min(0.1f)] public float pointLightIntensityMultiplier = 1f;
    [Min(0.1f)] public float pointLightRangeMultiplier = 1f;

    [Header("Relleno automático")]
    [Tooltip("Si está activo, el bootstrap NO añade luces EchoesFill_ extra.")]
    public bool disableRuntimeFillLights;

    public void ApplyNow()
    {
        if (overrideAmbient)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ambientSky;
            RenderSettings.ambientEquatorColor = ambientEquator;
            RenderSettings.ambientGroundColor = ambientGround;
            RenderSettings.reflectionIntensity = reflectionIntensity;
        }

        RenderSettings.fog = enableFog;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;

        if (overrideDirectional)
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null || light.type != LightType.Directional)
                    continue;

                light.intensity = directionalIntensity;
                light.color = directionalColor;
                light.transform.rotation = Quaternion.Euler(directionalEuler);
                break;
            }
        }

        if (!applyToChildPointLights)
            return;

        Light[] childLights = GetComponentsInChildren<Light>(true);
        for (int i = 0; i < childLights.Length; i++)
        {
            Light light = childLights[i];
            if (light == null || light.type != LightType.Point)
                continue;

            light.intensity *= pointLightIntensityMultiplier;
            light.range *= pointLightRangeMultiplier;
        }
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
            ApplyNow();
    }

    void Start()
    {
        ApplyNow();
    }
}
