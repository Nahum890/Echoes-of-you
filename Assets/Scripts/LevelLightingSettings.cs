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
    public float directionalIntensity = 0.85f;
    public Color directionalColor = new Color(0.95f, 0.95f, 1f, 1f);
    public Vector3 directionalEuler = new Vector3(50f, -30f, 0f);

    [Header("Ambiente global")]
    public bool overrideAmbient = true;
    public Color ambientColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    [Range(0f, 1f)] public float reflectionIntensity = 0.18f;

    [Header("Niebla")]
    public bool enableFog = true;
    public Color fogColor = new Color(0.1f, 0.1f, 0.12f, 1f);
    [Range(0f, 0.02f)] public float fogDensity = 0.008f;

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
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.reflectionIntensity = reflectionIntensity;
        }

        RenderSettings.fog = enableFog;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;

        if (overrideDirectional)
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude);
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null || light.type != LightType.Directional)
                    continue;

                light.intensity = directionalIntensity;
                light.color = directionalColor;
                light.transform.rotation = Quaternion.Euler(directionalEuler);
                light.shadows = LightShadows.Hard;
                QualitySettings.shadowDistance = 40f;
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
