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
    private Label _recIdLabel;
    private Label _statusTagLabel;
    private Label _syncStatusLabel;
    private Label _hexTagLabel;
    private VisualElement _vectorCanvas;
    private VisualElement _coreDot;
    private Label _chapterTitleLabel;
    private Label _quoteLabel;
    private Label _hintTextLabel;
    private Label _pctLabel;
    private VisualElement _barFill;
    private VisualElement _bottomLiveDot;

    private bool _loading;
    private float _progress;
    private float _currentRotation;
    private Coroutine _typewriterCoroutine;

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
        "Pista: Pulsa [ F ] para limpiar todos los ecos activos de la sala.",
        "Pista: Observa el rastro temporal cian para coordinar tus movimientos con el eco.",
        "Pista: El tiempo en el eco reproduce exactamente cada salto y desplazamiento que hagas."
    };

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
            _doc.sortingOrder = 5000; // Above everything during load
            var vta = Resources.Load<VisualTreeAsset>("UI/LoadingScreenUI");
            if (vta != null)
                _doc.visualTreeAsset = vta;
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

        _root = _doc.rootVisualElement;
        if (_root == null) return;

        _recIdLabel         = _root.Q<Label>("loading-rec-id");
        _statusTagLabel     = _root.Q<Label>("loading-status-tag");
        _syncStatusLabel    = _root.Q<Label>("loading-sync-status");
        _hexTagLabel        = _root.Q<Label>("loading-hex-tag");
        _vectorCanvas       = _root.Q("loading-vector-canvas");
        _coreDot            = _root.Q("loading-core-dot");
        _chapterTitleLabel  = _root.Q<Label>("loading-chapter-title");
        _quoteLabel         = _root.Q<Label>("loading-quote");
        _hintTextLabel      = _root.Q<Label>("loading-hint-text");
        _pctLabel           = _root.Q<Label>("loading-pct");
        _barFill            = _root.Q("loading-bar-fill");
        _bottomLiveDot      = _root.Q("loading-bottom-live-dot");

        if (_vectorCanvas != null)
        {
            _vectorCanvas.generateVisualContent -= OnGenerateVectorCanvas;
            _vectorCanvas.generateVisualContent += OnGenerateVectorCanvas;
        }
    }

    private void Update()
    {
        if (!_loading) return;

        // Smooth rotation of Broken O
        _currentRotation = (_currentRotation + Time.unscaledDeltaTime * 45f) % 360f;

        // Live dot pulsating
        if (_coreDot != null)
        {
            float pulse = Mathf.Sin(Time.unscaledTime * 5f) * 0.35f + 0.65f;
            _coreDot.style.opacity = pulse;
        }

        if (_bottomLiveDot != null)
        {
            float pulse = Mathf.Sin(Time.unscaledTime * 7f) * 0.4f + 0.6f;
            _bottomLiveDot.style.opacity = pulse;
        }

        // Random micro glitch jitter on chapter title
        if (_chapterTitleLabel != null && Random.value < 0.05f)
        {
            float ox = Random.Range(-2f, 2f);
            float oy = Random.Range(-1.5f, 1.5f);
            _chapterTitleLabel.style.translate = new StyleTranslate(new Translate(ox, oy));
        }
        else if (_chapterTitleLabel != null && Random.value < 0.2f)
        {
            _chapterTitleLabel.style.translate = new StyleTranslate(new Translate(0, 0));
        }

        _vectorCanvas?.MarkDirtyRepaint();
    }

    private void OnGenerateVectorCanvas(MeshGenerationContext ctx)
    {
        var p = ctx.painter2D;
        Vector2 center = new Vector2(128f, 128f);
        float radius = 100f;

        // 1. Broken outer arcs (Ivory with gaps)
        p.strokeColor = new Color(0.89f, 0.88f, 0.86f, 0.55f);
        p.lineWidth = 2.2f;
        p.lineCap = LineCap.Round;

        float rot = _currentRotation;
        p.BeginPath();
        p.Arc(center, radius, rot, rot + 80f);
        p.Stroke();

        p.BeginPath();
        p.Arc(center, radius, rot + 110f, rot + 220f);
        p.Stroke();

        p.BeginPath();
        p.Arc(center, radius, rot + 250f, rot + 330f);
        p.Stroke();

        // 2. Inner dashed circle
        p.strokeColor = new Color(0.58f, 0.56f, 0.52f, 0.35f);
        p.lineWidth = 1.2f;
        p.lineCap = LineCap.Round;
        float innerRadius = 78f;
        for (int i = 0; i < 8; i++)
        {
            float segStart = -rot * 0.7f + i * 45f;
            p.BeginPath();
            p.Arc(center, innerRadius, segStart, segStart + 22f);
            p.Stroke();
        }

        // 3. Dynamic Cyan Progress Arc (fills from -90 deg as _progress advances)
        float sweep = Mathf.Clamp(_progress * 360f, 0f, 360f);
        if (sweep > 0.5f)
        {
            p.strokeColor = new Color(0.39f, 0.83f, 0.98f, 0.35f);
            p.lineWidth = 6f;
            p.lineCap = LineCap.Round;
            p.BeginPath();
            p.Arc(center, radius, -90f, -90f + sweep);
            p.Stroke();

            p.strokeColor = new Color(0.72f, 0.94f, 1f, 0.95f);
            p.lineWidth = 3f;
            p.lineCap = LineCap.Round;
            p.BeginPath();
            p.Arc(center, radius, -90f, -90f + sweep);
            p.Stroke();
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

        int levelIdx = GameProgress.GetSceneIndex(sceneName);
        int displayLevelNum = levelIdx >= 0 ? levelIdx + 1 : 1;

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
            chapterTitle = $"ARCHIVO: {sceneName.ToUpperInvariant()}";
        }

        if (_chapterTitleLabel != null)
            _chapterTitleLabel.text = chapterTitle;

        // Configure Rec ID & Hex
        if (_recIdLabel != null)
            _recIdLabel.text = $"REC_ID: S_ARCH_{displayLevelNum:D2}";

        if (_hexTagLabel != null)
            _hexTagLabel.text = $"0x{Random.Range(0x100000, 0xFFFFFF):X6}";

        // Configure Hint
        if (_hintTextLabel != null)
        {
            int hintIdx = (displayLevelNum - 1) % GameplayHints.Length;
            if (hintIdx < 0) hintIdx = 0;
            _hintTextLabel.text = GameplayHints[hintIdx];
        }

        // Configure Lore Quote with Typewriter Effect
        string targetQuote = (levelIdx >= 0 && levelIdx < LoreQuotes.Length)
            ? LoreQuotes[levelIdx]
            : "El silencio es solo un eco que aún no ha aprendido a hablar.";

        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterRoutine(targetQuote));

        // Show loading screen & fade in
        if (_root != null)
        {
            _root.style.display = DisplayStyle.Flex;
            _root.AddToClassList("loading-visible");
        }

        float fadeTimer = 0f;
        while (fadeTimer < 0.25f)
        {
            fadeTimer += Time.unscaledDeltaTime;
            if (_root != null)
                _root.style.opacity = Mathf.Clamp01(fadeTimer / 0.25f);
            yield return null;
        }
        if (_root != null) _root.style.opacity = 1f;

        UpdateProgressUI();

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

        float watchdogStart = Time.unscaledTime;
        float lastProgressChange = Time.unscaledTime;
        float lastProgress = 0f;

        if (op != null)
        {
            while (op.progress < 0.9f)
            {
                _progress = Mathf.Clamp01(op.progress / 0.9f);
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

            _progress = 1f;
            UpdateProgressUI();

            if (!op.isDone)
            {
                op.allowSceneActivation = true;
                while (!op.isDone && Time.unscaledTime - watchdogStart < loadTimeoutSeconds + 10f)
                    yield return null;
            }
        }
        else
        {
            // Fallback simulated load
            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.unscaledDeltaTime;
                _progress = Mathf.Clamp01(t / 0.6f);
                UpdateProgressUI();
                yield return null;
            }
            _progress = 1f;
            UpdateProgressUI();
        }

        // Brief hold at 100% for visual closure
        yield return new WaitForSecondsRealtime(0.45f);

        RefreshElementReferences();

        // Smooth fade out
        float fadeOut = 0f;
        while (fadeOut < 0.35f)
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
            _root.RemoveFromClassList("loading-visible");
            _root.style.opacity = 0f;
        }

        _loading = false;
    }

    private IEnumerator TypewriterRoutine(string fullText)
    {
        if (_quoteLabel == null) yield break;

        _quoteLabel.text = "\"\"";
        yield return new WaitForSecondsRealtime(0.15f);

        for (int i = 1; i <= fullText.Length; i++)
        {
            if (_quoteLabel == null) yield break;
            _quoteLabel.text = $"\"{fullText.Substring(0, i)}\"";
            yield return new WaitForSecondsRealtime(0.025f);
        }
    }

    private void UpdateProgressUI()
    {
        if (_root == null) return;

        int pct = Mathf.Clamp(Mathf.RoundToInt(_progress * 100f), 0, 100);

        if (_barFill != null)
            _barFill.style.width = Length.Percent(pct);

        if (_pctLabel != null)
            _pctLabel.text = $"CARGANDO... {pct}%";

        if (_syncStatusLabel != null)
        {
            if (_progress < 0.35f)
                _syncStatusLabel.text = "TEMPORAL SYNC: IN PROGRESS";
            else if (_progress < 0.75f)
                _syncStatusLabel.text = "TEMPORAL SYNC: ALIGNING FRAGMENTS";
            else
                _syncStatusLabel.text = "TEMPORAL SYNC: SYNCHRONIZED";
        }

        _vectorCanvas?.MarkDirtyRepaint();
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

