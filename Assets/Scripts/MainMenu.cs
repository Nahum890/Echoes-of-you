using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menú principal minimalista usando OnGUI.
/// Estética: fondo oscuro, título grande, botones centrados.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Diseño")]
    [SerializeField] Color backgroundColor = new Color(0.06f, 0.06f, 0.1f, 1f);
    [SerializeField] Color titleColor = new Color(0.35f, 0.85f, 1f, 1f);
    [SerializeField] Color subtitleColor = new Color(0.6f, 0.6f, 0.7f, 1f);
    [SerializeField] Color buttonTextColor = Color.white;
    [SerializeField] Color buttonHoverColor = new Color(0.35f, 0.85f, 1f, 0.3f);

    [Header("Escena")]
    [SerializeField] string firstLevelScene = "Level_01";

    Texture2D _pixel;
    GUIStyle _titleStyle;
    GUIStyle _subtitleStyle;
    GUIStyle _buttonStyle;
    GUIStyle _buttonHoverStyle;
    float _fadeIn = 0f;
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
        _fadeIn = Mathf.MoveTowards(_fadeIn, 1f, Time.unscaledDeltaTime * 0.8f);
    }

    void InitStyles()
    {
        if (_stylesReady) return;
        _stylesReady = true;

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 64,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = titleColor }
        };

        _subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Italic,
            normal = { textColor = subtitleColor }
        };

        _buttonStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 26,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = buttonTextColor }
        };

        _buttonHoverStyle = new GUIStyle(_buttonStyle)
        {
            normal = { textColor = titleColor }
        };
    }

    void OnDestroy()
    {
        if (_pixel != null) Destroy(_pixel);
    }

    void OnGUI()
    {
        if (_pixel == null) return;
        InitStyles();

        // Fondo completo
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), backgroundColor);

        float alpha = _fadeIn;
        Color oldColor = GUI.color;
        GUI.color = new Color(1, 1, 1, alpha);

        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        // ─── Título ───
        Rect titleRect = new Rect(0, centerY - 180, Screen.width, 80);
        GUI.Label(titleRect, "ECHOES OF YOU", _titleStyle);

        // ─── Subtítulo ───
        Rect subRect = new Rect(0, centerY - 100, Screen.width, 30);
        GUI.Label(subRect, "Un puzzle sobre ti mismo", _subtitleStyle);

        // ─── Línea decorativa ───
        float lineW = 200f;
        DrawRect(new Rect(centerX - lineW * 0.5f, centerY - 55, lineW, 2),
                 new Color(titleColor.r, titleColor.g, titleColor.b, 0.4f * alpha));

        // ─── Botones ───
        float btnW = 280f;
        float btnH = 50f;
        float btnX = centerX - btnW * 0.5f;

        if (DrawButton(new Rect(btnX, centerY - 10, btnW, btnH), "JUGAR"))
        {
            SceneManager.LoadScene(firstLevelScene);
        }

        if (DrawButton(new Rect(btnX, centerY + 55, btnW, btnH), "SALIR"))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ─── Créditos ───
        Rect creditRect = new Rect(0, Screen.height - 40, Screen.width, 25);
        GUI.Label(creditRect, "Game Jam 2026", _subtitleStyle);

        GUI.color = oldColor;
    }

    bool DrawButton(Rect rect, string text)
    {
        Vector2 mouse = Event.current.mousePosition;
        bool hover = rect.Contains(mouse);

        if (hover)
        {
            DrawRect(rect, buttonHoverColor);
        }

        return GUI.Button(rect, text, hover ? _buttonHoverStyle : _buttonStyle);
    }

    void DrawRect(Rect rect, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, _pixel);
        GUI.color = old;
    }
}
