using UnityEngine;

/// <summary>
/// HUD ultra-mínimo: solo indicador de grabación y estado del eco.
/// Sin texto de objetivo, sin prompts largos, sin distracciones.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("Visibility")]
    [SerializeField] bool showOnGUI = true;

    [Header("Palette")]
    [SerializeField] Color panelColor = new Color(0.04f, 0.06f, 0.1f, 0.55f);
    [SerializeField] Color accentColor = new Color(0.1f, 0.5f, 1f, 0.6f);
    [SerializeField] Color textColor = new Color(0.8f, 0.8f, 0.8f, 1f);         // #CCCCCC
    [SerializeField] Color recordColor = new Color(0.9f, 0.15f, 0.15f, 1f);     // Rojo grabación
    [SerializeField] Color echoActiveColor = new Color(0.5f, 0.2f, 1f, 1f);     // Violeta eco
    [SerializeField] Color barBackColor = new Color(1f, 1f, 1f, 0.08f);

    int _echoCurrent;
    int _echoMax;
    bool _recording;
    float _recordNorm;
    string _echoState = "";

    // Mantener interfaz pública para compatibilidad
    string _objective = "";
    string _prompt = "";
    bool _promptSticky;
    float _promptUntil;
    string _toast = "";
    Color _toastColor;
    float _toastUntil;

    Texture2D _pixel;
    GUIStyle _labelStyle;
    GUIStyle _smallStyle;

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
            fontSize = 13,
            fontStyle = FontStyle.Normal,
            normal = { textColor = textColor }
        };

        _smallStyle = new GUIStyle(_labelStyle)
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleCenter
        };
    }

    // --- API pública (compatibilidad) ---
    public void SetEchoCount(int current, int max) { _echoCurrent = Mathf.Max(0, current); _echoMax = Mathf.Max(0, max); }
    public void SetRecording(bool recording, float normalizedTime01) { _recording = recording; _recordNorm = Mathf.Clamp01(normalizedTime01); }
    public void SetEchoState(string state) { _echoState = state ?? string.Empty; }
    public void SetObjective(string objective) { _objective = objective ?? string.Empty; }
    public void SetPrompt(string prompt, float duration = 2.4f) { _prompt = prompt ?? string.Empty; _promptSticky = duration <= 0f; _promptUntil = _promptSticky ? float.PositiveInfinity : Time.unscaledTime + duration; }
    public void ClearPrompt() { _prompt = string.Empty; _promptSticky = false; _promptUntil = 0f; }
    public void ShowToast(string message, Color color, float duration = 1.5f) { _toast = message ?? string.Empty; _toastColor = color; _toastUntil = Time.unscaledTime + Mathf.Max(0.1f, duration); }

    void Update()
    {
        if (!_promptSticky && Time.unscaledTime > _promptUntil) _prompt = string.Empty;
        if (Time.unscaledTime > _toastUntil) _toast = string.Empty;
    }

    void OnDestroy() { if (_pixel != null) Destroy(_pixel); }

    void OnGUI()
    {
        if (!showOnGUI || _pixel == null) return;
        InitStyleIfNeeded();

        DrawRecordingIndicator();
        DrawEchoState();
        DrawObjective();
        DrawToast();
    }

    void DrawObjective()
    {
        if (string.IsNullOrEmpty(_objective)) return;
        Rect r = new Rect(0, 16f, Screen.width, 30f);
        _labelStyle.alignment = TextAnchor.UpperCenter;
        
        Color old = GUI.color;
        GUI.color = textColor;
        GUI.Label(r, _objective, _labelStyle);
        GUI.color = old;
        
        _labelStyle.alignment = TextAnchor.UpperLeft;
    }

    void DrawToast()
    {
        if (string.IsNullOrEmpty(_toast) && string.IsNullOrEmpty(_prompt)) return;
        
        string txt = !string.IsNullOrEmpty(_toast) ? _toast : _prompt;
        Color col = !string.IsNullOrEmpty(_toast) ? _toastColor : accentColor;
        
        Rect r = new Rect(0, Screen.height - 80f, Screen.width, 30f);
        _labelStyle.alignment = TextAnchor.MiddleCenter;
        
        Color old = GUI.color;
        GUI.color = col;
        GUI.Label(r, txt, _labelStyle);
        GUI.color = old;
        
        _labelStyle.alignment = TextAnchor.UpperLeft;
    }

    void DrawRecordingIndicator()
    {
        if (!_recording && _echoCurrent <= 0 && _echoMax <= 0) return;

        // Pequeño panel esquina superior izquierda
        float w = 160f;
        float h = 36f;
        Rect panel = new Rect(16f, 16f, w, h);
        DrawRect(panel, panelColor);
        DrawRect(new Rect(panel.x, panel.y + h - 1f, panel.width, 1f), accentColor);

        if (_recording)
        {
            // Punto rojo pulsante
            float pulse = Mathf.Sin(Time.unscaledTime * 4f) * 0.3f + 0.7f;
            Color dotColor = new Color(recordColor.r, recordColor.g, recordColor.b, pulse);
            DrawRect(new Rect(panel.x + 10f, panel.y + 13f, 10f, 10f), dotColor);

            // Barra de progreso
            Rect barBg = new Rect(panel.x + 28f, panel.y + 15f, w - 40f, 6f);
            DrawRect(barBg, barBackColor);
            DrawRect(new Rect(barBg.x, barBg.y, barBg.width * _recordNorm, barBg.height), recordColor);
        }
        else
        {
            // Icono eco (cuadrado violeta pequeño)
            DrawRect(new Rect(panel.x + 10f, panel.y + 13f, 10f, 10f), echoActiveColor);
            GUI.Label(new Rect(panel.x + 28f, panel.y + 8f, w - 40f, 20f),
                $"{_echoCurrent}/{_echoMax}", _labelStyle);
        }
    }

    void DrawEchoState()
    {
        if (string.IsNullOrEmpty(_echoState)) return;

        // Indicador mínimo abajo-centro
        float w = 120f;
        Rect r = new Rect((Screen.width - w) * 0.5f, Screen.height - 32f, w, 20f);

        Color stateColor = textColor;
        string lower = _echoState.ToLowerInvariant();
        if (lower.Contains("grab")) stateColor = recordColor;
        else if (lower.Contains("repro") || lower.Contains("eco")) stateColor = echoActiveColor;

        Color old = GUI.color;
        GUI.color = stateColor;
        GUI.Label(r, _echoState, _smallStyle);
        GUI.color = old;
    }

    void DrawRect(Rect rect, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, _pixel);
        GUI.color = old;
    }
}
