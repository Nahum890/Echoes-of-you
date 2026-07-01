using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    const float FadeSpeed = 2f;

    Canvas _canvas;
    Image _fadeImage;
    bool _isTransitioning;

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
        CreateCanvas();
        ResetFade();
        SceneManager.sceneLoaded += OnSceneLoaded;
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

    void CreateCanvas()
    {
        // Destroy any existing FadeCanvas children to avoid stale references.
        // transform.Find can return destroyed-but-not-yet-GC'd objects that pass
        // C# null checks but throw MissingReferenceException on any Unity API call.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            try
            {
                Transform child = transform.GetChild(i);
                if (child != null && child.name == "FadeCanvas")
                    DestroyImmediate(child.gameObject);
            }
            catch (MissingReferenceException) { }
        }

        // Create fresh canvas hierarchy — no stale references possible
        GameObject canvasObject = new GameObject("FadeCanvas");
        canvasObject.transform.SetParent(transform, false);

        _canvas = canvasObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject imgObj = new GameObject("FadeImage");
        imgObj.transform.SetParent(canvasObject.transform, false);
        _fadeImage = imgObj.AddComponent<Image>();

        RectTransform rt = _fadeImage.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
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
        CreateCanvas();
        _fadeImage.raycastTarget = true;

        yield return FadeTo(1f);

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
        if (_fadeImage == null)
            yield break;

        float alpha = _fadeImage.color.a;
        while (!Mathf.Approximately(alpha, targetAlpha))
        {
            alpha = Mathf.MoveTowards(alpha, targetAlpha, Time.unscaledDeltaTime * FadeSpeed);
            _fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        _fadeImage.color = new Color(0f, 0f, 0f, targetAlpha);
    }

    void ResetFade()
    {
        _isTransitioning = false;

        if (_fadeImage == null)
            return;

        _fadeImage.color = new Color(0f, 0f, 0f, 0f);
        _fadeImage.raycastTarget = false;
    }
}
