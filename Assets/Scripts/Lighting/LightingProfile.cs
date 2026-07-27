using UnityEngine;

[CreateAssetMenu(menuName = "Echoes/Lighting Profile", fileName = "LightingProfile_")]
public class LightingProfile : ScriptableObject {
    public enum Archetype { FluorescentStandard, FluorescentDying, WindowNatural, EmergencyRed, VoidNone, WarmMemory }

    public Archetype archetype = Archetype.FluorescentStandard;
    public Color dominantColor = new Color(0.79f, 0.83f, 0.69f);
    public float intensity = 2.5f;
    public float flickerProbability = 0.3f;
    public float flickerSpeed = 0.2f;
    public float intensityVariance = 0.4f;
    public Texture2D cookieTexture; // cookie_window_grid, cookie_blinds, cookie_tree
    public float cookieSize = 10f;
    public bool castsShadows = true;
    public float shadowSoftness = 0.5f;

    // Liminal overrides
    public bool anomalousColorBleed = false;
    public float subsurfaceGlow = 0f;
}
