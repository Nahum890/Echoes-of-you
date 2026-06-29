using UnityEngine;

public static class EchoesPresentationSettings
{
    const string VisualScaleKey = "Echoes.CharacterVisualScale";
    const string AnimSpeedKey = "Echoes.AnimPlaybackSpeed";
    const string ProceduralKey = "Echoes.ProceduralMotion";
    const string MenuScaleKey = "Echoes.MenuTextScale";
    const string GameFogKey = "Echoes.GameFogDensity";
    const string GameSunKey = "Echoes.GameSunIntensity";
    const string GamePointLightsKey = "Echoes.GamePointLightMul";
    const string GameAmbientKey = "Echoes.GameAmbientMul";

    public const float DefaultVisualScale = 1f;
    public const float DefaultAnimSpeed = 1f;
    public const float DefaultMenuTextScale = 1.25f;
    public const float DefaultGameFogDensity = 0.011f;
    public const float DefaultGameSunIntensity = 0.58f;
    public const float DefaultGamePointLightMul = 0.72f;
    public const float DefaultGameAmbientMul = 0.56f;

    public static float CharacterVisualScale =>
        Mathf.Clamp(PlayerPrefs.GetFloat(VisualScaleKey, DefaultVisualScale), 0.2f, 1.2f);

    public static float AnimationPlaybackSpeed =>
        Mathf.Clamp(PlayerPrefs.GetFloat(AnimSpeedKey, DefaultAnimSpeed), 0.5f, 2f);

    public static bool ProceduralMotionEnabled =>
        PlayerPrefs.GetInt(ProceduralKey, 1) == 1;

    public static float MenuTextScale =>
        Mathf.Clamp(PlayerPrefs.GetFloat(MenuScaleKey, DefaultMenuTextScale), 1f, 1.6f);

    public static float GameFogDensity =>
        Mathf.Clamp(PlayerPrefs.GetFloat(GameFogKey, DefaultGameFogDensity), 0f, 0.02f);

    public static float GameSunIntensity =>
        Mathf.Clamp(PlayerPrefs.GetFloat(GameSunKey, DefaultGameSunIntensity), 0.4f, 3f);

    public static float GamePointLightMultiplier =>
        Mathf.Clamp(PlayerPrefs.GetFloat(GamePointLightsKey, DefaultGamePointLightMul), 0.3f, 3f);

    public static float GameAmbientMultiplier =>
        Mathf.Clamp(PlayerPrefs.GetFloat(GameAmbientKey, DefaultGameAmbientMul), 0.4f, 2.5f);

    public static void Save(float visualScale, float animSpeed, bool procedural, float menuTextScale)
    {
        PlayerPrefs.SetFloat(VisualScaleKey, visualScale);
        PlayerPrefs.SetFloat(AnimSpeedKey, animSpeed);
        PlayerPrefs.SetInt(ProceduralKey, procedural ? 1 : 0);
        PlayerPrefs.SetFloat(MenuScaleKey, menuTextScale);
        PlayerPrefs.Save();
    }

    public static void SaveLighting(float fogDensity, float sunIntensity, float pointLightMul, float ambientMul)
    {
        PlayerPrefs.SetFloat(GameFogKey, fogDensity);
        PlayerPrefs.SetFloat(GameSunKey, sunIntensity);
        PlayerPrefs.SetFloat(GamePointLightsKey, pointLightMul);
        PlayerPrefs.SetFloat(GameAmbientKey, ambientMul);
        PlayerPrefs.Save();
    }

    public static readonly string[] LightingPresetIds = { "liminal", "bruma", "claridad", "penumbra" };
    public static readonly string[] LightingPresetLabels = { "LIMINAL", "BRUMA", "CLARIDAD", "PENUMBRA" };

    public static bool TryGetLightingPreset(string presetId, out float fog, out float sun, out float point, out float ambient)
    {
        switch (presetId)
        {
            case "liminal":
                fog = 0.011f;
                sun = 0.58f;
                point = 0.72f;
                ambient = 0.56f;
                return true;
            case "bruma":
                fog = 0.014f;
                sun = 0.5f;
                point = 0.62f;
                ambient = 0.48f;
                return true;
            case "claridad":
                fog = 0.007f;
                sun = 0.82f;
                point = 0.92f;
                ambient = 0.72f;
                return true;
            case "penumbra":
                fog = 0.016f;
                sun = 0.42f;
                point = 0.52f;
                ambient = 0.38f;
                return true;
            default:
                fog = DefaultGameFogDensity;
                sun = DefaultGameSunIntensity;
                point = DefaultGamePointLightMul;
                ambient = DefaultGameAmbientMul;
                return false;
        }
    }

    public static void ApplyLightingPreset(string presetId)
    {
        if (!TryGetLightingPreset(presetId, out float fog, out float sun, out float point, out float ambient))
            return;

        SaveLighting(fog, sun, point, ambient);
    }
}
