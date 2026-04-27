using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menú principal minimalista con acceso al hub 3D.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Palette")]
    [SerializeField] Color backgroundColor = new Color(0.04f, 0.06f, 0.08f, 1f);
    [SerializeField] Color titleColor = new Color(0.85f, 0.95f, 1f, 1f);
    [SerializeField] Color accentColor = new Color(0.16f, 0.85f, 1f, 1f);
    [SerializeField] Color secondaryColor = new Color(0.62f, 0.72f, 0.78f, 1f);
    [SerializeField] Color buttonHoverColor = new Color(0.16f, 0.85f, 1f, 0.2f);

    [Header("Scenes")]
    [SerializeField] string hubSceneName = "Level_01";

    Texture2D _pixel;
    GUIStyle _titleStyle;
    GUIStyle _subtitleStyle;
    GUIStyle _buttonStyle;
    GUIStyle _buttonHoverStyle;
    GUIStyle _smallStyle;
    float _fadeIn;
    bool _stylesReady;

    void Awake()
    {
        _pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _pixel.SetPixel(0, 0, Color.white);
        _pixel.Apply();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    void Update()
    {
        _fadeIn = Mathf.MoveTowards(_fadeIn, 1f, Time.unscaledDeltaTime * 0.9f);
    }

    void InitStyles()
    {
        if (_stylesReady)
            return;

        _stylesReady = true;

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 62,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = titleColor }
        };

        _subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            normal = { textColor = secondaryColor }
        };

        _buttonStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = titleColor }
        };

        _buttonHoverStyle = new GUIStyle(_buttonStyle)
        {
            normal = { textColor = accentColor }
        };

        _smallStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = secondaryColor }
        };
    }

    void OnDestroy()
    {
        if (_pixel != null)
            Destroy(_pixel);
    }

    void OnGUI()
    {
        if (_pixel == null)
            return;

        InitStyles();
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), backgroundColor);

        float alpha = _fadeIn;
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        Color old = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, alpha);

        GUI.Label(new Rect(0, centerY - 210f, Screen.width, 72f), "ECHOES OF YOU", _titleStyle);
        GUI.Label(
            new Rect(centerX - 260f, centerY - 130f, 520f, 48f),
            "Tus decisiones construyen quien eres. Aprende a usarlas para volver al centro.",
            _subtitleStyle);

        DrawRect(new Rect(centerX - 110f, centerY - 72f, 220f, 2f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.45f * alpha));

        string progressText = $"Memorias restauradas: {GameProgress.GetCompletedCount()}/{GameProgress.TotalLevels}";
        GUI.Label(new Rect(centerX - 180f, centerY - 38f, 360f, 24f), progressText, _smallStyle);

        float buttonWidth = 300f;
        float buttonHeight = 48f;
        float buttonX = centerX - buttonWidth * 0.5f;

        if (DrawButton(new Rect(buttonX, centerY + 8f, buttonWidth, buttonHeight), "JUGAR"))
            SceneManager.LoadScene(hubSceneName);

        if (DrawButton(new Rect(buttonX, centerY + 64f, buttonWidth, buttonHeight), "SELECCION DE NIVELES"))
            SceneManager.LoadScene(hubSceneName);

        if (DrawButton(new Rect(buttonX, centerY + 120f, buttonWidth, buttonHeight), "REINICIAR PROGRESO"))
            GameProgress.ResetProgress();

        if (DrawButton(new Rect(buttonX, centerY + 176f, buttonWidth, buttonHeight), "SALIR"))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        GUI.Label(new Rect(0, Screen.height - 40f, Screen.width, 22f), "Un puzzle sobre memoria, identidad y aprendizaje.", _smallStyle);
        GUI.color = old;
    }

    bool DrawButton(Rect rect, string text)
    {
        Vector2 mouse = Event.current.mousePosition;
        bool hover = rect.Contains(mouse);

        DrawRect(rect, new Color(0.02f, 0.05f, 0.08f, hover ? 0.88f : 0.76f));
        if (hover)
            DrawRect(new Rect(rect.x, rect.y, rect.width, rect.height), buttonHoverColor);

        DrawRect(new Rect(rect.x, rect.y, rect.width, 2f), new Color(accentColor.r, accentColor.g, accentColor.b, hover ? 0.65f : 0.18f));
        GUI.Label(rect, text, hover ? _buttonHoverStyle : _buttonStyle);
        return GUI.Button(rect, GUIContent.none, GUIStyle.none);
    }

    void DrawRect(Rect rect, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, _pixel);
        GUI.color = old;
    }
}
