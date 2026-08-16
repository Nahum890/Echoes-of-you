using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance { get; private set; }

    const float RefW = 1920f;
    const float RefH = 1080f;

    Texture2D _bgTex;
    bool _loading;
    float _progress;
    string _levelDisplayName;
    string _loreQuote;
    float _fadeInAlpha = 1f;
    bool _fadingOut;
    float _fadeOutAlpha;

    static readonly string[] LoreQuotes =
    {
        "Cada paso reordena lo que creias saber.",
        "El pasillo recuerda mas que yo.",
        "Lo que dejo sin mirar sigue esperando en la luz.",
        "No es olvidar. Es elegir que parte mantener.",
        "El eco llega primero. Tal vez no esta intentando reemplazarme.",
        "Dos verdades pueden doler sin que una borre a la otra.",
        "Puedo mirar lo que duele sin convertirlo en mi unica historia.",
        "El silencio tambien es una eleccion.",
        "Lo que pesa no siempre es lo que rompe.",
        "Tal vez ella tambien se acercaba.",
        "La puerta sigue aqui. Esta vez puedo decidir como cruzarla.",
        "Subir no es olvidar lo que queda abajo.",
        "El lugar no me obedece. Tal vez no tiene que hacerlo.",
        "Puedo acompanarlo sin controlarlo.",
        "Esto tambien es mio, aunque sea de ella.",
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _bgTex = Resources.Load<Texture2D>("UI/void_fog_bg");
    }

    public void LoadScene(string sceneName)
    {
        if (_loading) return;
        StartCoroutine(LoadRoutine(sceneName));
    }

    IEnumerator LoadRoutine(string sceneName)
    {
        _loading = true;
        _progress = 0f;
        _fadeInAlpha = 1f;
        _fadingOut = false;

        int levelIdx = GameProgress.GetSceneIndex(sceneName);
        _levelDisplayName = levelIdx >= 0 && levelIdx < GameProgress.TotalLevels
            ? GameProgress.GetLevelDisplayName(sceneName)
            : sceneName;

        _loreQuote = levelIdx >= 0 && levelIdx < LoreQuotes.Length
            ? LoreQuotes[levelIdx]
            : "Algo espera al otro lado.";

        AsyncOperation op = null;
        try
        {
            PostProcessingSetup.PrepareForSceneReload();
            Time.timeScale = 1f;
            op = SceneManager.LoadSceneAsync(sceneName);
            if (op != null) op.allowSceneActivation = false;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LoadingScreen] Failed to start loading '{sceneName}': {ex.Message}");
        }

        if (op != null)
        {
            while (op.progress < 0.9f)
            {
                _progress = Mathf.Clamp01(op.progress / 0.9f);
                if (_fadeInAlpha > 0f) _fadeInAlpha -= Time.unscaledDeltaTime * 3f;
                yield return null;
            }

            _progress = 1f;
            op.allowSceneActivation = true;

            while (!op.isDone)
                yield return null;
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }

        yield return new WaitForSecondsRealtime(0.4f);

        _fadingOut = true;
        _fadeOutAlpha = 0f;
        while (_fadeOutAlpha < 1f)
        {
            _fadeOutAlpha += Time.unscaledDeltaTime * 2.5f;
            yield return null;
        }

        _loading = false;
    }

    void OnGUI()
    {
        if (!_loading) return;

        float scale = Mathf.Min(Screen.width / RefW, Screen.height / RefH);
        float ox = (Screen.width - RefW * scale) * 0.5f;
        float oy = (Screen.height - RefH * scale) * 0.5f;
        var oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(ox, oy, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));

        GUI.color = new Color(0.02f, 0.02f, 0.04f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, RefW, RefH), Texture2D.whiteTexture);

        if (_bgTex != null)
            GUI.DrawTexture(new Rect(0f, 0f, RefW, RefH), _bgTex, ScaleMode.ScaleAndCrop);

        GUI.color = new Color(0f, 0f, 0.04f, Mathf.Lerp(0.6f, 0.85f, _progress));
        GUI.DrawTexture(new Rect(0f, 0f, RefW, RefH), Texture2D.whiteTexture);

        int titleSize = Mathf.RoundToInt(42 * scale);
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = titleSize,
            normal = { textColor = new Color(1f, 0.7f, 0.18f) },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        GUI.Label(new Rect(RefW * 0.1f, RefH * 0.2f, RefW * 0.8f, 60f), "Cargando memoria...", titleStyle);

        int nameSize = Mathf.RoundToInt(28 * scale);
        GUIStyle nameStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = nameSize,
            normal = { textColor = new Color(0.9f, 0.9f, 0.95f) },
            alignment = TextAnchor.MiddleCenter
        };
        GUI.Label(new Rect(RefW * 0.1f, RefH * 0.28f, RefW * 0.8f, 40f), _levelDisplayName, nameStyle);

        int quoteSize = Mathf.RoundToInt(22 * scale);
        GUIStyle quoteStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = quoteSize,
            normal = { textColor = new Color(0.65f, 0.65f, 0.72f, 0.9f) },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Italic,
            wordWrap = true
        };
        GUI.Label(new Rect(RefW * 0.15f, RefH * 0.38f, RefW * 0.7f, 100f), _loreQuote, quoteStyle);

        float barW = RefW * 0.5f;
        float barH = 12f;
        float barX = (RefW - barW) * 0.5f;
        float barY = RefH * 0.62f;

        GUI.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);
        GUI.DrawTexture(new Rect(barX, barY, barW, barH), Texture2D.whiteTexture);

        GUI.color = new Color(1f, 0.7f, 0.18f, 0.9f);
        GUI.DrawTexture(new Rect(barX, barY, barW * _progress, barH), Texture2D.whiteTexture);

        GUI.color = new Color(1f, 0.7f, 0.18f, 0.6f);
        GUI.DrawTexture(new Rect(barX, barY, barW, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(barX, barY + barH - 2f, barW, 2f), Texture2D.whiteTexture);

        int pctSize = Mathf.RoundToInt(20 * scale);
        GUIStyle pctStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = pctSize,
            normal = { textColor = new Color(1f, 0.7f, 0.18f) },
            alignment = TextAnchor.MiddleCenter
        };
        GUI.Label(new Rect(barX, barY + 25f, barW, 30f), Mathf.RoundToInt(_progress * 100f) + "%", pctStyle);

        GUI.color = Color.white;

        if (_fadingOut)
        {
            GUI.color = new Color(0f, 0f, 0f, _fadeOutAlpha);
            GUI.DrawTexture(new Rect(0f, 0f, RefW, RefH), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        if (_fadeInAlpha > 0f)
        {
            GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(_fadeInAlpha));
            GUI.DrawTexture(new Rect(0f, 0f, RefW, RefH), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        GUI.matrix = oldMatrix;
    }
}
