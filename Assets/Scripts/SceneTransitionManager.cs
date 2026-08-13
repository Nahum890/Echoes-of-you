using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    const float FadeSpeed = 4f; // más rápido

    bool _isTransitioning;
    bool _initialized;

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    // DESHABILITADO: auto-inicialización causaba interferencia con carga directa de escenas
    // static void AutoInitialize()
    // {
    //     if (Instance != null)
    //         return;
    //
    //     GameObject go = new GameObject("SceneTransitionManager");
    //     go.AddComponent<SceneTransitionManager>();
    //     DontDestroyOnLoad(go);
    // }

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

        // ELIMINADO EL FADE NEGRO POR SOLICITUD DE USUARIO (evita pantalla negra permanente)
        _initialized = true;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SceneTransitionManager] Scene loaded: {scene.name}");
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

        Debug.Log($"[SceneTransitionManager] LoadScene requested: {sceneName}");

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[SceneTransitionManager] Scene '{sceneName}' NOT in Build Settings!");
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
        Debug.Log($"[SceneTransitionManager] TransitionRoutine started for {sceneName}");

        // Safety: try/finally para garantizar ResetFade incluso si hay excepción
        try
        {
            AsyncOperation loadOperation = null;
            try
            {
                Debug.Log("[SceneTransitionManager] Calling PostProcessingSetup.PrepareForSceneReload()...");
                PostProcessingSetup.PrepareForSceneReload();
                Time.timeScale = 1f;
                Debug.Log($"[SceneTransitionManager] Starting LoadSceneAsync for {sceneName}...");
                loadOperation = SceneManager.LoadSceneAsync(sceneName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneTransitionManager] Failed to start loading '{sceneName}': {ex.Message}");
            }

            if (loadOperation != null)
            {
                Debug.Log("[SceneTransitionManager] Waiting for loadOperation.isDone...");
                while (!loadOperation.isDone)
                    yield return null;

                Debug.Log("[SceneTransitionManager] Load complete, waiting 0.1s...");
                yield return new WaitForSecondsRealtime(0.1f);
            }
            else
            {
                Debug.LogError("[SceneTransitionManager] loadOperation is NULL!");
            }

            Debug.Log("[SceneTransitionManager] Fading from black...");
            yield return FadeTo(0f);
            Debug.Log("[SceneTransitionManager] Fade from black complete");
        }
        finally
        {
            // GARANTÍA: siempre resetear al finalizar (éxito o excepción)
            Debug.Log("[SceneTransitionManager] finally block - ResetFade");
            ResetFade();
        }
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        yield return null;
    }

    public void ResetFade()
    {
        _isTransitioning = false;
    }
}
