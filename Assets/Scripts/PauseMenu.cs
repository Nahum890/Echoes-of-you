using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menú de pausa. ESC para abrir/cerrar. Pausa el juego con TimeScale = 0.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("Diseño")]
    [SerializeField] Color overlayColor = new Color(0.04f, 0.04f, 0.08f, 0.85f);
    [SerializeField] Color accentColor = new Color(0.35f, 0.85f, 1f, 1f);
    [SerializeField] Color buttonHoverColor = new Color(0.35f, 0.85f, 1f, 0.25f);

    [Header("Escena del menú principal")]
    [SerializeField] string mainMenuScene = "MainMenu";

    bool _paused;
    Texture2D _pixel;
    GUIStyle _titleStyle;
    GUIStyle _buttonStyle;
    GUIStyle _buttonHoverStyle;
    GUIStyle _hintStyle;
    bool _stylesReady;

    public bool IsPaused => _paused;

    void Awake()
    {
        _pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _pixel.SetPixel(0, 0, Color.white);
        _pixel.Apply();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_paused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        _paused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Resume()
    {
        _paused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void InitStyles()
    {
        if (_stylesReady) return;
        _stylesReady = true;

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 42,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = accentColor }
        };

        _buttonStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        _buttonHoverStyle = new GUIStyle(_buttonStyle)
        {
            normal = { textColor = accentColor }
        };

        _hintStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Italic,
            normal = { textColor = new Color(0.5f, 0.5f, 0.6f, 1f) }
        };
    }

    void OnDestroy()
    {
        if (_pixel != null) Destroy(_pixel);
        // Restore timeScale when destroyed
        Time.timeScale = 1f;
    }

    void OnGUI()
    {
        if (!_paused || _pixel == null) return;
        InitStyles();

        // Overlay oscuro
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), overlayColor);

        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;

        // Título
        GUI.Label(new Rect(0, cy - 160, Screen.width, 60), "PAUSA", _titleStyle);

        // Línea decorativa
        float lineW = 160f;
        DrawRect(new Rect(cx - lineW * 0.5f, cy - 90, lineW, 2),
                 new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f));

        // Botones
        float btnW = 260f;
        float btnH = 48f;
        float btnX = cx - btnW * 0.5f;

        if (DrawButton(new Rect(btnX, cy - 50, btnW, btnH), "CONTINUAR"))
            Resume();

        if (DrawButton(new Rect(btnX, cy + 10, btnW, btnH), "REINICIAR NIVEL"))
        {
            Resume();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (DrawButton(new Rect(btnX, cy + 70, btnW, btnH), "MENÚ PRINCIPAL"))
        {
            Resume();
            SceneManager.LoadScene(mainMenuScene);
        }

        if (DrawButton(new Rect(btnX, cy + 130, btnW, btnH), "SALIR"))
        {
#if UNITY_EDITOR
            Resume();
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Hint
        GUI.Label(new Rect(0, cy + 200, Screen.width, 25),
                  "Presiona ESC para continuar", _hintStyle);
    }

    bool DrawButton(Rect rect, string text)
    {
        Vector2 mouse = Event.current.mousePosition;
        bool hover = rect.Contains(mouse);

        if (hover)
            DrawRect(rect, buttonHoverColor);

        GUI.Label(rect, text, hover ? _buttonHoverStyle : _buttonStyle);
        return hover && Event.current.type == EventType.MouseDown && Event.current.button == 0;
    }

    void DrawRect(Rect rect, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, _pixel);
        GUI.color = old;
    }
}
