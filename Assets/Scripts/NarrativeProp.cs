using UnityEngine;

/// <summary>
/// Narrative prop component: places a soft diegetic point-light at runtime
/// to guide player attention via the chapter's colour language.
/// Attach to any prop that carries narrative significance.
/// </summary>
public class NarrativeProp : MonoBehaviour
{
    [Header("Narrative Identity")]
    public string propName;
    public bool isLyraProp = false;

    [Header("Diegetic Hint Light")]
    public Color chapterHintColor = new Color(0.91f, 0.70f, 0.38f, 1f); // memory-amber
    public float hintIntensity = 1.0f;
    public float lightRange = 3f;
    public bool enableOnStart = true;

    void Start()
    {
        if (enableOnStart && hintIntensity > 0f)
        {
            var hint = gameObject.AddComponent<Light>();
            hint.type = LightType.Point;
            hint.color = chapterHintColor;
            hint.intensity = hintIntensity;
            hint.range = lightRange;
            hint.shadows = LightShadows.None;
            hint.renderMode = LightRenderMode.ForcePixel;
        }
    }
}
