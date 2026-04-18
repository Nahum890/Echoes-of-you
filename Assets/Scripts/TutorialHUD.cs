using UnityEngine;

/// <summary>
/// Muestra mensajes de tutorial con animación de fade in/out.
/// Singleton que recibe mensajes desde TutorialTrigger.
/// </summary>
public class TutorialHUD : MonoBehaviour
{
    [Header("Diseño")]
    [SerializeField] Color bgColor = new Color(0f, 0f, 0f, 0.55f);
    [SerializeField] Color textColor = new Color(0.35f, 0.85f, 1f, 1f);
    [SerializeField] Color hintColor = new Color(0.6f, 0.6f, 0.7f, 1f);
    [SerializeField] float fadeSpeed = 3f;

    string _currentMessage = "";
    string _currentHint = "";
    float _alpha;
    float _targetAlpha;
    float _autoHideTimer;

    Texture2D _pixel;
    GUIStyle _msgStyle;
    GUIStyle _hintStyle;
    bool _stylesReady;

    public static TutorialHUD Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _pixel.SetPixel(0, 0, Color.white);
        _pixel.Apply();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (_pixel != null) Destroy(_pixel);
    }

    void Update()
    {
        _alpha = Mathf.MoveTowards(_alpha, _targetAlpha, fadeSpeed * Time.deltaTime);

        if (_autoHideTimer > 0f)
        {
            _autoHideTimer -= Time.deltaTime;
            if (_autoHideTimer <= 0f)
                _targetAlpha = 0f;
        }
    }

    /// <summary>Mostrar un mensaje de tutorial.</summary>
    public void ShowMessage(string message, string hint = "", float duration = 5f)
    {
        _currentMessage = message;
        _currentHint = hint;
        _targetAlpha = 1f;
        _autoHideTimer = duration;
    }

    /// <summary>Ocultar el mensaje actual.</summary>
    public void HideMessage()
    {
        _targetAlpha = 0f;
    }

    void InitStyles()
    {
        if (_stylesReady) return;
        _stylesReady = true;

        _msgStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = textColor }
        };

        _hintStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Italic,
            wordWrap = true,
            normal = { textColor = hintColor }
        };
    }

    void OnGUI()
    {
        if (_pixel == null || _alpha < 0.01f) return;
        InitStyles();

        Color oldColor = GUI.color;
        GUI.color = new Color(1, 1, 1, _alpha);

        float panelW = Mathf.Min(500f, Screen.width * 0.7f);
        float panelH = string.IsNullOrEmpty(_currentHint) ? 60f : 90f;
        float panelX = (Screen.width - panelW) * 0.5f;
        float panelY = Screen.height - panelH - 80f; // parte inferior de pantalla

        // Fondo con bordes redondeados (sin bordes reales, pero con padding)
        Color bg = bgColor;
        bg.a *= _alpha;
        DrawRect(new Rect(panelX, panelY, panelW, panelH), bg);

        // Línea accent superior
        Color accent = textColor;
        accent.a = 0.6f * _alpha;
        DrawRect(new Rect(panelX, panelY, panelW, 3), accent);

        // Mensaje principal
        GUI.Label(new Rect(panelX + 16, panelY + 8, panelW - 32, 30), _currentMessage, _msgStyle);

        // Hint secundario
        if (!string.IsNullOrEmpty(_currentHint))
        {
            GUI.Label(new Rect(panelX + 16, panelY + 42, panelW - 32, 30), _currentHint, _hintStyle);
        }

        GUI.color = oldColor;
    }

    void DrawRect(Rect rect, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, _pixel);
        GUI.color = old;
    }
}
