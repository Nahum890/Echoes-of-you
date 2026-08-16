using UnityEngine;

public static class GameSettings
{
    const string MouseSensitivityKey = "MouseSensitivity";
    const float DefaultSensitivity = 1f;
    const float MinSensitivity = 0.1f;
    const float MaxSensitivity = 3f;

    static float _cachedSensitivity = -1f;

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

    public static void SetSensitivity(float value)
    {
        MouseSensitivity = value;
    }

    public static float GetSensitivity() => MouseSensitivity;
}