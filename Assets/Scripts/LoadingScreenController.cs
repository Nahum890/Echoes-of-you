using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance { get; private set; }

    // UI Document & Elements
    private UIDocument _doc;
    private VisualElement _root;
    private readonly VisualElement[] _fixtures = new VisualElement[3];
    private float _fixtureTime;
    private Label _chapterTitleLabel;
    private Label _quoteLabel;
    private Label _hintTextLabel;
    private Label _pctLabel;
    private VisualElement _barFill;

    private bool _loading;
    private float _progress;

    private static readonly string[] LoreQuotes =
    {
        "Cada paso reordena lo que creías saber.",
        "El pasillo recuerda más que yo.",
        "Lo que dejo sin mirar sigue esperando en la luz.",
        "No es olvidar. Es elegir qué parte mantener.",
        "El eco llega primero. Tal vez no está intentando reemplazarme.",
        "Dos verdades pueden doler sin que una borre a la otra.",
        "Puedo mirar lo que duele sin convertirlo en mi única historia.",
        "El silencio también es una elección.",
        "Lo que pesa no siempre es lo que rompe.",
        "Tal vez ella también se acercaba.",
        "La puerta sigue aquí. Esta vez puedo decidir cómo cruzarla.",
        "Subir no es olvidar lo que queda abajo.",
        "El lugar no me obedece. Tal vez no tiene que hacerlo.",
        "Puedo acompañarlo sin controlarlo.",
        "Esto también es mío, aunque sea de ella."
    };

    private static readonly string[] GameplayHints =
    {
        "Pista: Mantén [ R ] para grabar un eco de tu posición actual.",
        "Pista: Los ecos pueden presionar placas de peso y mantener compuertas abiertas.",
        "Pista: Suelta R para crear un eco que repita tus movimientos.",
        "Pista: Observa el rastro temporal cian para coordinar tus movimientos con el eco.",
        "Pista: El tiempo en el eco reproduce exactamente cada salto y desplazamiento que hagas."
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void AutoInit()
    {
        if (Instance == null && Object.FindAnyObjectByType<LoadingScreenController>() == null)
        {
            var panel = UIBootstrap.PanelSettings;
            var go = new GameObject("LoadingScreenController");
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = panel;
            doc.sortingOrder = 9999;
            var vta = Resources.Load<VisualTreeAsset>("UI/LoadingScreenUI");
#if UNITY_EDITOR
            if (vta == null) vta = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/LoadingScreenUI.uxml");
#endif
            if (vta != null) doc.visualTreeAsset = vta;
            go.AddComponent<LoadingScreenController>();
            if (Application.isPlaying) Object.DontDestroyOnLoad(go);
        }
    }

    public static void TransitionToScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) return;

        if (Instance == null)
            AutoInit();

        if (Instance != null)
        {
            Instance.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoadedCallback;
        InitializeUI();
    }

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        InitializeUI();
    }

    private void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
    {
        RefreshElementReferences();
    }

    private void InitializeUI()
    {
        var panel = UIBootstrap.PanelSettings;
        if (panel == null)
        {
            Debug.LogWarning("[LoadingScreenController] No PanelSettings found.");
            return;
        }

        _doc = GetComponent<UIDocument>();
        if (_doc == null)
        {
            _doc = gameObject.AddComponent<UIDocument>();
            _doc.panelSettings = panel;
            _doc.sortingOrder = 9999; // Above everything during load
            var vta = Resources.Load<VisualTreeAsset>("UI/LoadingScreenUI");
            if (vta != null)
                _doc.visualTreeAsset = vta;
        }
        else
        {
            _doc.sortingOrder = 9999;
            if (_doc.panelSettings == null)
                _doc.panelSettings = panel;
        }

        RefreshElementReferences();

        if (_root != null && !_loading)
        {
            _root.style.display = DisplayStyle.None;
            _root.style.opacity = 0f;
        }
    }

    private void RefreshElementReferences()
    {
        if (_doc == null) _doc = GetComponent<UIDocument>();
        if (_doc == null) return;

        if (_doc.rootVisualElement != null)
        {
            var container = _doc.rootVisualElement;
            container.style.position = Position.Absolute;
            container.style.left = 0;
            container.style.top = 0;
            container.style.right = 0;
            container.style.bottom = 0;
            container.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            container.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            container.style.flexGrow = 1;
        }

        _root = _doc.rootVisualElement.Q<VisualElement>("loading-root") ?? _doc.rootVisualElement;
        if (_root == null) return;

        _root.style.position = Position.Absolute;
        _root.style.left = 0;
        _root.style.top = 0;
        _root.style.right = 0;
        _root.style.bottom = 0;
        _root.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        _root.style.height = new StyleLength(new Length(100, LengthUnit.Percent));

        for (int i = 0; i < _fixtures.Length; i++)
            _fixtures[i] = _root.Q("loading-fixture-" + i);

        _chapterTitleLabel  = _root.Q<Label>("loading-chapter-title");
        _quoteLabel         = _root.Q<Label>("loading-quote");
        _hintTextLabel      = _root.Q<Label>("loading-hint-text");
        _pctLabel           = _root.Q<Label>("loading-pct");
        _barFill            = _root.Q("loading-bar-fill");
    }

    private void Update()
    {
        if (!_loading) return;

        bool reduceMotion = EchoesSettings.ReduceMotion;
        if (!reduceMotion)
            _fixtureTime = Mathf.Repeat(_fixtureTime + Time.unscaledDeltaTime, 2.4f);

        for (int i = 0; i < _fixtures.Length; i++)
        {
            if (_fixtures[i] == null) continue;
            float wave = 0.5f + 0.5f * Mathf.Cos((_fixtureTime / 2.4f - i / 3f) * Mathf.PI * 2f);
            _fixtures[i].style.opacity = reduceMotion ? 1f : Mathf.Lerp(0.32f, 1f, wave * wave);
        }
    }

    public void LoadScene(string sceneName)
    {
        if (_loading) return;
        RefreshElementReferences();
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        _loading = true;
        _progress = 0f;
        _fixtureTime = 0f;

        int levelIdx = GameProgress.GetSceneIndex(sceneName);
        int displayLevelNum = levelIdx >= 0 ? GameProgress.GetChapterIndex(sceneName) + 1 : 1;

        // Configure Title
        string chapterTitle;
        if (sceneName.StartsWith("Level_"))
        {
            string roman = GetRomanNumeral(displayLevelNum);
            string levelName = GameProgress.GetLevelDisplayName(sceneName);
            chapterTitle = $"CAPÍTULO {roman}: {levelName}";
        }
        else
        {
            chapterTitle = sceneName switch
            {
                "MainMenu" => "Menú principal",
                "CreditsScene" => "Echoes of You",
                _ => sceneName
            };
        }

        if (_chapterTitleLabel != null)
            _chapterTitleLabel.text = chapterTitle;

        // Configure Hint
        if (_hintTextLabel != null)
        {
            _hintTextLabel.parent.style.display = levelIdx >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
            int hintIdx = Mathf.Max(0, levelIdx) % GameplayHints.Length;
            if (hintIdx < 0) hintIdx = 0;
            _hintTextLabel.text = GameplayHints[hintIdx];
        }

        if (_quoteLabel != null)
        {
            _quoteLabel.text = levelIdx >= 0 && levelIdx < LoreQuotes.Length
                ? LoreQuotes[levelIdx]
                : string.Empty;
            _quoteLabel.style.display = string.IsNullOrEmpty(_quoteLabel.text) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        // Show loading screen immediately
        if (_doc != null && _doc.rootVisualElement != null)
        {
            _doc.rootVisualElement.style.display = DisplayStyle.Flex;
        }

        if (_root != null)
        {
            _root.style.display = DisplayStyle.Flex;
            _root.style.opacity = 1f;
        }

        UpdateProgressUI();
        // Deja que se dibuje la cartela antes de empezar a cargar la escena.
        yield return null;

        const float loadTimeoutSeconds = 20f;
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

        float lastProgressChange = Time.unscaledTime;
        float lastProgress = 0f;

        if (op != null)
        {
            while (op.progress < 0.9f)
            {
                _progress = Mathf.Clamp01(op.progress / 0.9f) * 0.95f;
                UpdateProgressUI();

                if (op.progress > lastProgress + 0.001f)
                {
                    lastProgress = op.progress;
                    lastProgressChange = Time.unscaledTime;
                }
                if (Time.unscaledTime - lastProgressChange > loadTimeoutSeconds)
                {
                    Debug.LogWarning($"[LoadingScreen] Progress stalled {loadTimeoutSeconds}s — forcing activation of '{sceneName}'.");
                    break;
                }

                yield return null;
            }

            if (!op.isDone)
            {
                op.allowSceneActivation = true;
                yield return op;
            }

            _progress = 1f;
            UpdateProgressUI();
            yield return new WaitForSecondsRealtime(0.45f);
        }

        RefreshElementReferences();

        // Smooth fade out
        float fadeOut = 0f;
        while (!EchoesSettings.ReduceMotion && fadeOut < 0.35f)
        {
            fadeOut += Time.unscaledDeltaTime;
            if (_root != null)
                _root.style.opacity = 1f - Mathf.Clamp01(fadeOut / 0.35f);
            yield return null;
        }

        RefreshElementReferences();

        if (_root != null)
        {
            _root.style.display = DisplayStyle.None;
            _root.style.opacity = 0f;
        }

        if (_doc?.rootVisualElement != null)
            _doc.rootVisualElement.style.display = DisplayStyle.None;
        _loading = false;
    }

    private void UpdateProgressUI()
    {
        if (_root == null) return;

        int pct = Mathf.Clamp(Mathf.RoundToInt(_progress * 100f), 0, 100);

        if (_barFill != null)
            _barFill.style.width = Length.Percent(pct);

        if (_pctLabel != null)
            _pctLabel.text = pct == 100 ? "Listo" : $"Cargando · {pct}%";
    }

    private static string GetRomanNumeral(int number)
    {
        switch (number)
        {
            case 1: return "I";
            case 2: return "II";
            case 3: return "III";
            case 4: return "IV";
            case 5: return "V";
            case 6: return "VI";
            case 7: return "VII";
            case 8: return "VIII";
            case 9: return "IX";
            case 10: return "X";
            default: return number.ToString();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedCallback;
        if (Instance == this) Instance = null;
    }
}

