using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menú de pausa limpio para gameplay.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("Palette")]
    [SerializeField] Color overlayColor = new Color(0.02f, 0.05f, 0.08f, 0.88f);
    [SerializeField] Color accentColor = new Color(0.16f, 0.85f, 1f, 1f);
    [SerializeField] Color hoverColor = new Color(0.16f, 0.85f, 1f, 0.2f);

    [Header("Scenes")]
    [SerializeField] string hubSceneName = "Level_07";
    [SerializeField] string mainMenuScene = "MainMenu";

    bool _paused;
    Texture2D _pixel;
    GUIStyle _titleStyle;
    GUIStyle _buttonStyle;
    GUIStyle _buttonHoverStyle;
    GUIStyle _hintStyle;

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

    void InitStylesIfNeeded()
    {
        if (_titleStyle != null)
            return;

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 40,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        _buttonStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
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
            normal = { textColor = new Color(0.72f, 0.78f, 0.84f, 1f) }
        };
    }

    void OnDestroy()
    {
        if (_pixel != null)
            Destroy(_pixel);

        Time.timeScale = 1f;
    }

    void OnGUI()
    {
        if (!_paused || _pixel == null)
            return;

        InitStylesIfNeeded();

        DrawRect(new Rect(0, 0, Screen.width, Screen.height), overlayColor);

        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;
        GUI.Label(new Rect(0, centerY - 160f, Screen.width, 48f), "PAUSA", _titleStyle);
        DrawRect(new Rect(centerX - 90f, centerY - 96f, 180f, 2f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.45f));

        float width = 280f;
        float height = 46f;
        float x = centerX - width * 0.5f;

        if (DrawButton(new Rect(x, centerY - 44f, width, height), "CONTINUAR"))
            Resume();

        if (DrawButton(new Rect(x, centerY + 10f, width, height), "REINICIAR NIVEL"))
        {
            Resume();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (DrawButton(new Rect(x, centerY + 64f, width, height), "VOLVER AL HUB"))
        {
            Resume();
            SceneManager.LoadScene(hubSceneName);
        }

        if (DrawButton(new Rect(x, centerY + 118f, width, height), "MENU PRINCIPAL"))
        {
            Resume();
            SceneManager.LoadScene(mainMenuScene);
        }

        GUI.Label(new Rect(0, centerY + 186f, Screen.width, 22f), "ESC para continuar", _hintStyle);
    }

    bool DrawButton(Rect rect, string text)
    {
        Vector2 mouse = Event.current.mousePosition;
        bool hover = rect.Contains(mouse);

        DrawRect(rect, new Color(0.02f, 0.05f, 0.08f, hover ? 0.88f : 0.76f));
        if (hover)
            DrawRect(rect, hoverColor);

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
