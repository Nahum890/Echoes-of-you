using UnityEngine;

public static class GameSettings
{
    const string MouseSensitivityKey = "MouseSensitivity";
    const float DefaultSensitivity = 1f;
    const float MinSensitivity = 0.1f;
    const float MaxSensitivity = 3f;

    const string UIScaleFactorKey = "UIScaleFactor";
    const string UIScaleNameKey = "UIScale";
    const float DefaultUIScale = 1.0f;

    public static readonly float[] UIScalePresets = { 0.80f, 0.90f, 1.00f, 1.15f, 1.30f };
    public static readonly string[] UIScaleNames = { "Compacto", "Reducido", "Normal", "Grande", "Extra Grande" };

    static float _cachedSensitivity = -1f;
    static float _cachedUIScale = -1f;

    public static float MouseSensitivity
    {
        get
        {
            if (_cachedSensitivity < 0f)
                _cachedSensitivity = PlayerPrefs.GetFloat(MouseSensitivityKey, DefaultSensitivity);
            return _cachedSensitivity;
        }
        set
        {
            float clamped = Mathf.Clamp(value, MinSensitivity, MaxSensitivity);
            if (System.Math.Abs(_cachedSensitivity - clamped) > 0.001f)
            {
                _cachedSensitivity = clamped;
                PlayerPrefs.SetFloat(MouseSensitivityKey, clamped);
                PlayerPrefs.Save();
            }
        }
    }

    public static float UIScaleFactor
    {
        get
        {
            if (_cachedUIScale < 0f)
            {
                _cachedUIScale = PlayerPrefs.GetFloat(UIScaleFactorKey, DefaultUIScale);
            }
            return _cachedUIScale;
        }
        set
        {
            float clamped = Mathf.Clamp(value, 0.70f, 1.50f);
            if (System.Math.Abs(_cachedUIScale - clamped) > 0.001f)
            {
                _cachedUIScale = clamped;
                PlayerPrefs.SetFloat(UIScaleFactorKey, clamped);
                PlayerPrefs.SetString(UIScaleNameKey, GetNameForScale(clamped));
                PlayerPrefs.Save();
                ApplyCurrentUIScale();
            }
        }
    }

    public static string UIScaleName => GetNameForScale(UIScaleFactor);

    public static void SetSensitivity(float value)
    {
        MouseSensitivity = value;
    }

    public static float GetSensitivity() => MouseSensitivity;

    public static void SetUIScale(float factor)
    {
        UIScaleFactor = factor;
    }

    public static void SetUIScaleByName(string name)
    {
        switch (name)
        {
            case "Compacto":
            case "Pequeño":
            case "Small":
                SetUIScale(0.80f);
                break;
            case "Reducido":
                SetUIScale(0.90f);
                break;
            case "Normal":
                SetUIScale(1.00f);
                break;
            case "Grande":
            case "Large":
                SetUIScale(1.15f);
                break;
            case "Extra Grande":
            case "Extra Large":
                SetUIScale(1.30f);
                break;
            default:
                SetUIScale(1.00f);
                break;
        }
    }

    public static string GetNameForScale(float scale)
    {
        if (scale <= 0.85f) return "Compacto";
        if (scale <= 0.95f) return "Reducido";
        if (scale <= 1.08f) return "Normal";
        if (scale <= 1.22f) return "Grande";
        return "Extra Grande";
    }

    public static void ApplyCurrentUIScale()
    {
        float scale = UIScaleFactor;
        var ps = UIBootstrap.PanelSettings;
        if (ps != null)
        {
            ps.scale = scale;
        }

        string scaleName = GetNameForScale(scale);
        var allDocs = Object.FindObjectsByType<UnityEngine.UIElements.UIDocument>(FindObjectsInactive.Exclude);
        foreach (var doc in allDocs)
        {
            if (doc != null && doc.rootVisualElement != null)
            {
                var root = doc.rootVisualElement;
                root.RemoveFromClassList("scale-large");
                root.RemoveFromClassList("scale-xl");
                if (scale >= 1.25f)
                    root.AddToClassList("scale-xl");
                else if (scale >= 1.12f)
                    root.AddToClassList("scale-large");

                doc.gameObject.SendMessage("ApplySavedUIScale", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}