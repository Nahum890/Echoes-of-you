using UnityEngine;

public class LightingApplier : MonoBehaviour {
    public static LightingApplier Instance { get; private set; }

    void Awake() => Instance = this;

    public void ApplyProfile(LightingProfile profile, Transform roomRoot, int levelNum = 1) {
        if (profile == null || roomRoot == null) return;

        // 1. Directional Light con COOKIE
        var sun = roomRoot.GetComponentInChildren<Light>(true);
        if (sun != null && sun.type == LightType.Directional) {
            sun.color = profile.dominantColor;
            sun.intensity = profile.intensity;
            sun.cookie = profile.cookieTexture;
            sun.cookieSize2D = new Vector2(profile.cookieSize, profile.cookieSize);
            sun.shadows = profile.castsShadows ? LightShadows.Soft : LightShadows.None;
        }

        // 2. Flicker DEFAULT en TODAS luces Point/Spot del room
        var lights = roomRoot.GetComponentsInChildren<Light>(true);
        foreach (var light in lights) {
            if (light.type == LightType.Point || light.type == LightType.Spot) {
                if (Random.value < profile.flickerProbability) {
                    var flicker = light.gameObject.GetComponent<LightFlicker>();
                    if (flicker == null) flicker = light.gameObject.AddComponent<LightFlicker>();
                    flicker.baseIntensity = light.intensity;
                    flicker.flickerSpeed = profile.flickerSpeed + Random.Range(-0.05f, 0.05f);
                    flicker.intensityVariance = profile.intensityVariance;
                    flicker.OnIntensityChange += (n) => EchoesAudioManager.PlayFluorescentHum(light.transform.position, n);
                }
                light.cookie = profile.cookieTexture;
                light.shadows = profile.castsShadows ? LightShadows.Soft : LightShadows.None;
            }
        }

        // 3. Liminal overrides en materiales del room
        if (profile.anomalousColorBleed || profile.subsurfaceGlow > 0) {
            var renderers = roomRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers) {
                foreach (var mat in r.materials) {
                    if (mat != null && mat.HasProperty("_FluorescentEdge")) {
                        mat.SetFloat("_FluorescentEdge", mat.GetFloat("_FluorescentEdge") + profile.subsurfaceGlow * 0.5f);
                    }
                }
            }
        }
    }

    // Cookie procedural generator
    public static Texture2D GenerateWindowGridCookie(int size = 128) {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                bool line = (x % 16 == 0) || (y % 16 == 0);
                tex.SetPixel(x, y, line ? Color.black : Color.white);
            }
        }
        tex.Apply();
        return tex;
    }

    public static Texture2D GenerateBlindsCookie(int size = 128) {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                tex.SetPixel(x, y, (y % 8 < 4) ? Color.black : Color.white);
            }
        }
        tex.Apply();
        return tex;
    }
}
