using UnityEngine;

/// <summary>
/// HUD mínimo sin dependencias de uGUI/TMP para evitar errores de paquetes.
/// Muestra estado por OnGUI.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("OnGUI")]
    [SerializeField] bool showOnGUI = true;
    [SerializeField] Vector2 panelPosition = new Vector2(18f, 18f);
    [SerializeField] Vector2 panelSize = new Vector2(360f, 120f);
    [SerializeField] Color panelColor = new Color(0f, 0f, 0f, 0.45f);
    [SerializeField] Color barBackColor = new Color(1f, 1f, 1f, 0.15f);
    [SerializeField] Color recordingColor = new Color(1f, 0.45f, 0.2f, 1f);
    [SerializeField] Color idleColor = new Color(0.35f, 0.85f, 1f, 1f);

    int _echoCurrent;
    int _echoMax;
    bool _recording;
    float _recordNorm;

    Texture2D _pixel;
    GUIStyle _labelStyle;

    void Awake()
    {
        _pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _pixel.SetPixel(0, 0, Color.white);
        _pixel.Apply();
    }

    void InitStyleIfNeeded()
    {
        if (_labelStyle != null) return;
        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            normal = { textColor = Color.white }
        };
    }

    public void SetEchoCount(int current, int max)
    {
        _echoCurrent = Mathf.Max(0, current);
        _echoMax = Mathf.Max(0, max);
    }

    public void SetRecording(bool recording, float normalizedTime01)
    {
        _recording = recording;
        _recordNorm = Mathf.Clamp01(normalizedTime01);
    }

    void OnDestroy()
    {
        if (_pixel != null)
            Destroy(_pixel);
    }

    void OnGUI()
    {
        if (!showOnGUI || _pixel == null)
            return;

        InitStyleIfNeeded();

        Rect panel = new Rect(panelPosition.x, panelPosition.y, panelSize.x, panelSize.y);
        DrawRect(panel, panelColor);

        GUI.Label(new Rect(panel.x + 12f, panel.y + 10f, panel.width - 24f, 24f), $"Ecos: {_echoCurrent} / {_echoMax}", _labelStyle);
        GUI.Label(new Rect(panel.x + 12f, panel.y + 34f, panel.width - 24f, 24f), _recording ? "Grabando..." : "Mantén R para grabar", _labelStyle);

        Rect barBack = new Rect(panel.x + 12f, panel.y + 68f, panel.width - 24f, 18f);
        DrawRect(barBack, barBackColor);

        Rect barFill = new Rect(barBack.x, barBack.y, barBack.width * _recordNorm, barBack.height);
        DrawRect(barFill, _recording ? recordingColor : idleColor);
    }

    void DrawRect(Rect rect, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, _pixel);
        GUI.color = old;
    }
}
