using UnityEngine;

/// <summary>
/// Tutorial HUD mejorado: muestra objetivo del nivel al iniciar,
/// mensajes contextuales con fade, y hint de controles en nivel 1.
/// Singleton accesible via TutorialHUD.Instance.
/// </summary>
public class TutorialHUD : MonoBehaviour
{
    [Header("Mensajes por nivel (por build index)")]
    [SerializeField] string[] levelObjectives = new string[]
    {
        "Deja un eco en la placa para abrir la puerta",           // Level 1
        "Un eco también puede sostener un puente",                 // Level 2
        "Dos placas, dos ecos — abre la puerta central",           // Level 3
        "El orden importa — no sigas al eco, aprovéchalo",         // Level 4
        "Construye una cadena: un eco habilita al siguiente",      // Level 5
        "Vuelve al núcleo — usa todo lo aprendido"                 // Level 6
    };

    [Header("Diseño")]
    [SerializeField] Color bgColor = new Color(0.016f, 0.039f, 0.055f, 0.62f);
    [SerializeField] Color textColor = new Color(0f, 0.851f, 1f, 1f);           // #00D9FF
    [SerializeField] Color hintColor = new Color(0.6f, 0.6f, 0.7f, 1f);
    [SerializeField] Color objectiveColor = new Color(0.945f, 0.965f, 0.98f, 1f); // #F1F6FA
    [SerializeField] float fadeSpeed = 3f;

    [Header("Objetivo del nivel")]
    [SerializeField] float objectiveShowDuration = 5f;
    [SerializeField] bool showObjectiveOnStart = true;

    string _currentMessage = "";
    string _currentHint = "";
    float _alpha;
    float _targetAlpha;
    float _autoHideTimer;

    // Objective strip (top-center)
    string _objectiveText = "";
    float _objectiveAlpha;
    float _objectiveTargetAlpha;
    float _objectiveTimer;

    Texture2D _pixel;
    GUIStyle _msgStyle;
    GUIStyle _hintStyle;
    GUIStyle _objectiveStyle;
    GUIStyle _objectiveBgStyle;
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

    void Start()
    {
        if (showObjectiveOnStart)
        {
            int sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            // Offset by MainMenu if it's index 0
            int levelIndex = sceneIndex;
            // Try to find the right level index
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName.Contains("01") || sceneName.Contains("_1")) levelIndex = 0;
            else if (sceneName.Contains("02") || sceneName.Contains("_2")) levelIndex = 1;
            else if (sceneName.Contains("03") || sceneName.Contains("_3")) levelIndex = 2;
            else if (sceneName.Contains("04") || sceneName.Contains("_4")) levelIndex = 3;
            else if (sceneName.Contains("05") || sceneName.Contains("_5")) levelIndex = 4;
            else if (sceneName.Contains("06") || sceneName.Contains("_6")) levelIndex = 5;
            else levelIndex = -1;

            if (levelIndex >= 0 && levelIndex < levelObjectives.Length)
            {
                ShowObjective(levelObjectives[levelIndex], objectiveShowDuration);
            }

            // Show controls hint on first level
            if (levelIndex == 0)
            {
                Invoke(nameof(ShowControlsHint), 1.5f);
            }
        }
    }

    void ShowControlsHint()
    {
        ShowMessage("Mantén R para grabar un eco", "El eco repite lo que hiciste — úsalo en la placa", 5f);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (_pixel != null) Destroy(_pixel);
    }

    void Update()
    {
        // Message fade
        _alpha = Mathf.MoveTowards(_alpha, _targetAlpha, fadeSpeed * Time.deltaTime);
        if (_autoHideTimer > 0f)
        {
            _autoHideTimer -= Time.deltaTime;
            if (_autoHideTimer <= 0f)
                _targetAlpha = 0f;
        }

        // Objective fade
        _objectiveAlpha = Mathf.MoveTowards(_objectiveAlpha, _objectiveTargetAlpha, fadeSpeed * Time.deltaTime);
        if (_objectiveTimer > 0f)
        {
            _objectiveTimer -= Time.deltaTime;
            if (_objectiveTimer <= 0f)
                _objectiveTargetAlpha = 0f;
        }
    }

    /// <summary>Mostrar un mensaje de tutorial (bottom-center).</summary>
    public void ShowMessage(string message, string hint = "", float duration = 5f)
    {
        _currentMessage = message;
        _currentHint = hint;
        _targetAlpha = 1f;
        _autoHideTimer = duration;
    }

    /// <summary>Show objective strip at top-center.</summary>
    public void ShowObjective(string text, float duration = 5f)
    {
        _objectiveText = text;
        _objectiveTargetAlpha = 1f;
        _objectiveTimer = duration;
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

        _objectiveStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Normal,
            wordWrap = true,
            normal = { textColor = objectiveColor }
        };
    }

    void OnGUI()
    {
        if (_pixel == null) return;
        InitStyles();

        DrawObjectiveStrip();
        DrawTutorialMessage();
    }

    void DrawObjectiveStrip()
    {
        if (_objectiveAlpha < 0.01f || string.IsNullOrEmpty(_objectiveText))
            return;

        Color oldColor = GUI.color;
        GUI.color = new Color(1, 1, 1, _objectiveAlpha);

        float stripW = Mathf.Min(500f, Screen.width * 0.6f);
        float stripH = 48f;
        float stripX = (Screen.width - stripW) * 0.5f;
        float stripY = 24f;

        // Background
        Color bg = bgColor;
        bg.a = 0.7f * _objectiveAlpha;
        DrawRect(new Rect(stripX, stripY, stripW, stripH), bg);

        // Accent line bottom
        Color accent = objectiveColor;
        accent.a = 0.4f * _objectiveAlpha;
        DrawRect(new Rect(stripX, stripY + stripH - 2, stripW, 2), accent);

        // Text
        _objectiveStyle.normal.textColor = new Color(objectiveColor.r, objectiveColor.g, objectiveColor.b, _objectiveAlpha);
        GUI.Label(new Rect(stripX + 16, stripY + 6, stripW - 32, stripH - 12), _objectiveText, _objectiveStyle);

        GUI.color = oldColor;
    }

    void DrawTutorialMessage()
    {
        if (_alpha < 0.01f) return;

        Color oldColor = GUI.color;
        GUI.color = new Color(1, 1, 1, _alpha);

        float panelW = Mathf.Min(520f, Screen.width * 0.7f);
        float panelH = string.IsNullOrEmpty(_currentHint) ? 60f : 94f;
        float panelX = (Screen.width - panelW) * 0.5f;
        float panelY = Screen.height - panelH - 90f;

        // Background
        Color bg = bgColor;
        bg.a = 0.72f * _alpha;
        DrawRect(new Rect(panelX, panelY, panelW, panelH), bg);

        // Accent line top
        Color accent = textColor;
        accent.a = 0.7f * _alpha;
        DrawRect(new Rect(panelX, panelY, panelW, 3), accent);

        // Main message
        _msgStyle.normal.textColor = new Color(textColor.r, textColor.g, textColor.b, _alpha);
        GUI.Label(new Rect(panelX + 16, panelY + 10, panelW - 32, 32), _currentMessage, _msgStyle);

        // Hint
        if (!string.IsNullOrEmpty(_currentHint))
        {
            _hintStyle.normal.textColor = new Color(hintColor.r, hintColor.g, hintColor.b, _alpha * 0.85f);
            GUI.Label(new Rect(panelX + 16, panelY + 48, panelW - 32, 30), _currentHint, _hintStyle);
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
