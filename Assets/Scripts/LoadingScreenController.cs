using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance { get; private set; }

    // UI
    UIDocument _doc;
    VisualElement _root;
    VisualElement _progressArc;
    VisualElement _arcFill;
    VisualElement _barFill;
    Label _pctLabel;
    Label _quoteLabel;
    Label _syncStatusLabel;

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
        SceneManager.sceneLoaded += OnSceneLoadedCallback;
        InitializeUI();
    }

    void OnEnable()
    {
        // Restore static ref after domain reload (DontDestroyOnLoad objects survive but statics are cleared)
        if (Instance == null)
        {
            Instance = this;
            // Re-query root since UIDocument may have been re-created during domain reload
            if (_doc == null) _doc = GetComponent<UIDocument>();
            if (_doc != null && _root == null)
            {
                var vta = Resources.Load<VisualTreeAsset>("UI/LoadingScreenUI");
                if (vta != null && _doc.visualTreeAsset == null)
                    _doc.visualTreeAsset = vta;
                _root = _doc.rootVisualElement;
                if (_root != null)
                {
                    _progressArc = _root.Q("loading-progress-arc");
                    _arcFill = _root.Q("loading-arc-fill");
                    _barFill = _root.Q("loading-bar-fill");
                    _pctLabel = _root.Q<Label>("loading-pct");
                    _quoteLabel = _root.Q<Label>("loading-quote");
                    _syncStatusLabel = _root.Q<Label>("loading-sync-status");
                    _root.style.display = DisplayStyle.None;
                }
            }
        }
    }

    void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
    {
        // After scene load, UIDocument re-attaches to panel — re-query root
        if (_doc != null)
        {
            _root = _doc.rootVisualElement;
            if (_root != null)
            {
                _progressArc = _root.Q("loading-progress-arc");
                _arcFill = _root.Q("loading-arc-fill");
                _barFill = _root.Q("loading-bar-fill");
                _pctLabel = _root.Q<Label>("loading-pct");
                _quoteLabel = _root.Q<Label>("loading-quote");
                _syncStatusLabel = _root.Q<Label>("loading-sync-status");
            }
        }
    }

    void InitializeUI()
    {
        var panel = UIBootstrap.PanelSettings;
        if (panel == null)
        {
            Debug.LogWarning("[LoadingScreenController] No PanelSettings found.");
            return;
        }

        // Create UIDocument if missing
        _doc = GetComponent<UIDocument>();
        if (_doc == null)
        {
            _doc = gameObject.AddComponent<UIDocument>();
            _doc.panelSettings = panel;
            _doc.sortingOrder = 5000; // Above everything during load
            var vta = Resources.Load<VisualTreeAsset>("UI/LoadingScreenUI");
            if (vta == null)
            {
                Debug.LogError("[LoadingScreenController] LoadingScreenUI.uxml not found in Resources/UI/");
                return;
            }
            _doc.visualTreeAsset = vta;
        }

        _root = _doc.rootVisualElement;
        if (_root == null) return;

        _progressArc = _root.Q("loading-progress-arc");
        _arcFill = _root.Q("loading-arc-fill");
        _barFill = _root.Q("loading-bar-fill");
        _pctLabel = _root.Q<Label>("loading-pct");
        _quoteLabel = _root.Q<Label>("loading-quote");
        _syncStatusLabel = _root.Q<Label>("loading-sync-status");

        // Start hidden
        _root.style.display = DisplayStyle.None;
    }

    public void LoadScene(string sceneName)
    {
        if (_loading) return;
        if (_root == null) InitializeUI();
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

        // Set lore quote
        if (_quoteLabel != null) _quoteLabel.text = $"\"{_loreQuote}\"";
        if (_syncStatusLabel != null) _syncStatusLabel.text = "SYNC_STATUS: RECOVERING_DUAL_MEMORY";

        // Show loading screen
        if (_root != null)
        {
            _root.style.display = DisplayStyle.Flex;
            _root.AddToClassList("loading-visible");
            _root.style.opacity = 1f;
        }

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
                if (_fadeInAlpha > 0f) _fadeInAlpha -= Time.unscaledDeltaTime * 3f;
                UpdateProgressUI();

                // Watchdog: if progress stalls for too long, force activation
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
            // Fallback: simulate brief load
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.unscaledDeltaTime;
                _progress = Mathf.Clamp01(t / 0.5f);
                UpdateProgressUI();
                yield return null;
            }
            _progress = 1f;
            UpdateProgressUI();
        }

        // Brief hold at 100% for visual completion
        yield return new WaitForSecondsRealtime(0.4f);

        // Re-query root in case scene load invalidated it
        if (_doc != null) _root = _doc.rootVisualElement;

        // Fade out
        _fadingOut = true;
        _fadeOutAlpha = 0f;
        while (_fadeOutAlpha < 1f)
        {
            _fadeOutAlpha += Time.unscaledDeltaTime * 2.5f;
            if (_root != null)
                _root.style.opacity = 1f - Mathf.Clamp01(_fadeOutAlpha);
            yield return null;
        }

        // Re-query root before hiding (scene may have just activated)
        if (_doc != null) _root = _doc.rootVisualElement;

        // Hide
        if (_root != null)
        {
            _root.style.display = DisplayStyle.None;
            _root.RemoveFromClassList("loading-visible");
            _root.style.opacity = 1f;
        }

        _loading = false;
    }

    void UpdateProgressUI()
    {
        if (_root == null) return;

        // Progress bar
        if (_barFill != null)
            _barFill.style.width = Length.Percent(_progress * 100f);

        // Percentage label
        if (_pctLabel != null)
            _pctLabel.text = Mathf.RoundToInt(_progress * 100f) + "%";

        // Update sync status text at milestones
        if (_syncStatusLabel != null)
        {
            if (_progress < 0.33f)
                _syncStatusLabel.text = "SYNC_STATUS: RECOVERING_DUAL_MEMORY";
            else if (_progress < 0.66f)
                _syncStatusLabel.text = "SYNC_STATUS: ALIGNING_ECHO_SIGNATURE";
            else if (_progress < 0.95f)
                _syncStatusLabel.text = "SYNC_STATUS: STABILIZING_CORRIDOR";
            else
                _syncStatusLabel.text = "SYNC_STATUS: MEMORY_SYNCHRONIZED";
        }

        // Progress arc — toggle classes based on progress
        _root.RemoveFromClassList("loading-arc-25");
        _root.RemoveFromClassList("loading-arc-50");
        _root.RemoveFromClassList("loading-arc-75");
        _root.RemoveFromClassList("loading-arc-100");

        if (_progress >= 0.95f)
            _root.AddToClassList("loading-arc-100");
        else if (_progress >= 0.70f)
            _root.AddToClassList("loading-arc-75");
        else if (_progress >= 0.40f)
            _root.AddToClassList("loading-arc-50");
        else if (_progress >= 0.15f)
            _root.AddToClassList("loading-arc-25");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedCallback;
        if (Instance == this) Instance = null;
    }
}
