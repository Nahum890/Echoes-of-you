using UnityEngine;

/// <summary>
/// HUD minimalista con cuatro piezas:
/// objetivo, estado de eco, prompt contextual y toast breve.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("Visibility")]
    [SerializeField] bool showOnGUI = true;

    [Header("Palette")]
    [SerializeField] Color panelColor = new Color(0.02f, 0.05f, 0.08f, 0.72f);
    [SerializeField] Color panelOutline = new Color(0.28f, 0.68f, 0.82f, 0.35f);
    [SerializeField] Color textColor = new Color(0.94f, 0.97f, 0.99f, 1f);
    [SerializeField] Color infoColor = new Color(0.16f, 0.85f, 1f, 1f);
    [SerializeField] Color successColor = new Color(0.48f, 0.94f, 0.78f, 1f);
    [SerializeField] Color errorColor = new Color(1f, 0.43f, 0.43f, 1f);
    [SerializeField] Color barBackColor = new Color(1f, 1f, 1f, 0.12f);

    int _echoCurrent;
    int _echoMax;
    bool _recording;
    float _recordNorm;
    string _echoState = "Listo";

    string _objective = "";
    string _prompt = "";
    bool _promptSticky;
    float _promptUntil;

    string _toast = "";
    Color _toastColor;
    float _toastUntil;

    Texture2D _pixel;
    GUIStyle _labelStyle;
    GUIStyle _titleStyle;
    GUIStyle _promptStyle;
    GUIStyle _toastStyle;

    void Awake()
    {
        _pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _pixel.SetPixel(0, 0, Color.white);
        _pixel.Apply();
    }

    void InitStyleIfNeeded()
    {
        if (_labelStyle != null)
            return;

        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            wordWrap = true,
            normal = { textColor = textColor }
        };

        _titleStyle = new GUIStyle(_labelStyle)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold
        };

        _promptStyle = new GUIStyle(_labelStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 15
        };

        _toastStyle = new GUIStyle(_promptStyle)
        {
            fontStyle = FontStyle.Bold
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

    public void SetEchoState(string state)
    {
        _echoState = state ?? string.Empty;
    }

    public void SetObjective(string objective)
    {
        _objective = objective ?? string.Empty;
    }

    public void SetPrompt(string prompt, float duration = 2.4f)
    {
        _prompt = prompt ?? string.Empty;
        _promptSticky = duration <= 0f;
        _promptUntil = _promptSticky ? float.PositiveInfinity : Time.unscaledTime + duration;
    }

    public void ClearPrompt()
    {
        _prompt = string.Empty;
        _promptSticky = false;
        _promptUntil = 0f;
    }

    public void ShowToast(string message, Color color, float duration = 1.5f)
    {
        _toast = message ?? string.Empty;
        _toastColor = color;
        _toastUntil = Time.unscaledTime + Mathf.Max(0.1f, duration);
    }

    void Update()
    {
        if (!_promptSticky && Time.unscaledTime > _promptUntil)
            _prompt = string.Empty;

        if (Time.unscaledTime > _toastUntil)
            _toast = string.Empty;
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

        DrawObjective();
        DrawEchoPanel();
        DrawPrompt();
        DrawToast();
    }

    void DrawObjective()
    {
        if (string.IsNullOrEmpty(_objective))
            return;

        float width = Mathf.Min(420f, Screen.width * 0.54f);
        Rect rect = new Rect((Screen.width - width) * 0.5f, 18f, width, 44f);
        DrawPanel(rect);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 10f, rect.width - 28f, 24f), _objective, _titleStyle);
    }

    void DrawEchoPanel()
    {
        if (_echoMax <= 0 && !_recording && _echoCurrent <= 0)
            return;

        Rect rect = new Rect(18f, 18f, 300f, 80f);
        DrawPanel(rect);

        GUI.Label(new Rect(rect.x + 14f, rect.y + 10f, rect.width - 28f, 20f), $"Ecos {_echoCurrent}/{Mathf.Max(0, _echoMax)}", _titleStyle);

        Color old = GUI.color;
        GUI.color = ResolveStateColor();
        GUI.Label(new Rect(rect.x + 14f, rect.y + 30f, rect.width - 28f, 20f), _echoState, _labelStyle);
        GUI.color = old;

        Rect barBack = new Rect(rect.x + 14f, rect.y + 54f, rect.width - 28f, 10f);
        DrawRect(barBack, barBackColor);

        if (_recording)
        {
            Rect barFill = new Rect(barBack.x, barBack.y, barBack.width * _recordNorm, barBack.height);
            DrawRect(barFill, infoColor);
        }
    }

    void DrawPrompt()
    {
        if (string.IsNullOrEmpty(_prompt))
            return;

        float width = Mathf.Min(360f, Screen.width * 0.6f);
        Rect rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 84f, width, 54f);
        DrawPanel(rect);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 8f, rect.width - 28f, rect.height - 16f), _prompt, _promptStyle);
    }

    void DrawToast()
    {
        if (string.IsNullOrEmpty(_toast))
            return;

        float width = Mathf.Min(280f, Screen.width * 0.48f);
        Rect rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 138f, width, 36f);
        DrawPanel(rect);

        Color old = GUI.color;
        GUI.color = _toastColor;
        GUI.Label(new Rect(rect.x + 12f, rect.y + 7f, rect.width - 24f, 22f), _toast, _toastStyle);
        GUI.color = old;
    }

    void DrawPanel(Rect rect)
    {
        DrawRect(rect, panelColor);
        DrawRect(new Rect(rect.x, rect.y, rect.width, 2f), panelOutline);
    }

    void DrawRect(Rect rect, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, _pixel);
        GUI.color = old;
    }

    Color ResolveStateColor()
    {
        if (string.IsNullOrEmpty(_echoState))
            return textColor;

        string lower = _echoState.ToLowerInvariant();
        if (lower.Contains("grab"))
            return infoColor;
        if (lower.Contains("error") || lower.Contains("bloq") || lower.Contains("corta"))
            return errorColor;
        if (lower.Contains("repro") || lower.Contains("eco"))
            return successColor;

        return textColor;
    }
}
