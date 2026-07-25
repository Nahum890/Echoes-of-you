using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    const float FadeSpeed = 2f;

    UIDocument _doc;
    VisualElement _fadeOverlay;
    bool _isTransitioning;
    bool _initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoInitialize()
    {
        if (Instance != null)
            return;

        GameObject go = new GameObject("SceneTransitionManager");
        go.AddComponent<SceneTransitionManager>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.ResetFade();
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeUI();
    }

    void InitializeUI()
    {
        if (_initialized)
            return;

        _doc = gameObject.AddComponent<UIDocument>();
        _doc.sortingOrder = 999;

        var root = new VisualElement();
        root.style.position = Position.Absolute;
        root.style.left = 0;
        root.style.top = 0;
        root.style.right = 0;
        root.style.bottom = 0;
        root.style.backgroundColor = new StyleColor(Color.black);
        root.style.opacity = 0f;
        root.pickingMode = PickingMode.Ignore;

        _fadeOverlay = root;
        _doc.rootVisualElement.Add(root);
        _initialized = true;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetFade();
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[SceneTransitionManager] Cannot load an empty scene name.");
            ResetFade();
            return;
        }

        if (_isTransitioning)
        {
            StopAllCoroutines();
            ResetFade();
        }

        StartCoroutine(TransitionRoutine(sceneName));
    }

    IEnumerator TransitionRoutine(string sceneName)
    {
        _isTransitioning = true;

        if (_fadeOverlay != null)
        {
            _fadeOverlay.pickingMode = PickingMode.Position;
            yield return FadeTo(1f);
        }

        AsyncOperation loadOperation = null;
        try
        {
            PostProcessingSetup.PrepareForSceneReload();
            Time.timeScale = 1f;
            loadOperation = SceneManager.LoadSceneAsync(sceneName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SceneTransitionManager] Failed to start loading '{sceneName}': {ex.Message}");
        }

        if (loadOperation != null)
        {
            while (!loadOperation.isDone)
                yield return null;

            yield return new WaitForSecondsRealtime(0.1f);
        }

        yield return FadeTo(0f);
        ResetFade();
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        if (_fadeOverlay == null)
            yield break;

        float alpha = _fadeOverlay.style.opacity.value;
        while (!Mathf.Approximately(alpha, targetAlpha))
        {
            alpha = Mathf.MoveTowards(alpha, targetAlpha, Time.unscaledDeltaTime * FadeSpeed);
            _fadeOverlay.style.opacity = alpha;
            yield return null;
        }

        _fadeOverlay.style.opacity = targetAlpha;
    }

    void ResetFade()
    {
        _isTransitioning = false;

        if (_fadeOverlay == null)
            return;

        _fadeOverlay.style.opacity = 0f;
        _fadeOverlay.pickingMode = PickingMode.Ignore;
    }
}