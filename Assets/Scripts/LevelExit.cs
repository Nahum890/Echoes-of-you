using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Marca el nivel como completado y carga el siguiente destino.
/// Usa SceneTransitionManager para fade suave cuando está disponible.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LevelExit : MonoBehaviour
{
    public string nextSceneName;
    public bool loadNextBuildIndex = true;
    public float delaySeconds = 1.35f;

    [Header("Completion Copy")]
    [SerializeField] string completionToast = "";
#pragma warning disable CS0414
    [SerializeField] string lockedToast = "Completa el puzzle antes de salir.";
#pragma warning restore CS0414

    bool _triggered;
    bool _isUnlocked = false;
    LevelGoal _goal;
    Collider _collider;
    Renderer[] _renderers;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
        _renderers = GetComponentsInChildren<Renderer>(true);

        // Ensure Rigidbody exists for reliable trigger detection
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!_isUnlocked)
        {
            Debug.Log("[LevelExit] Locked — resolve the puzzle first.");
            return;
        }

        if (_triggered)
            return;

        _triggered = true;
        Debug.Log($"[LevelExit] TRIGGERED! Loading: {nextSceneName}");

        string sceneName = SceneManager.GetActiveScene().name;
        GameProgress.MarkSceneCompleted(sceneName);

        string toast = !string.IsNullOrEmpty(completionToast)
            ? completionToast
            : (_goal != null ? _goal.GetCompletionToast() : ResolveCompletionToast(sceneName));
        LevelRuntimeController.Instance?.OnLevelCompleted(transform.position, toast);
        
        // N15: no invocar Invoke(nameof(LoadNext)) si VN gate activo en LevelRuntimeController
        // El gate manejará la carga del siguiente nivel
        var lrc = LevelRuntimeController.Instance;
        if (lrc != null && !lrc.IsLevelCompletedAndGateActive())
        {
            Invoke(nameof(LoadNext), delaySeconds);
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Fallback: if OnTriggerEnter was missed, try on stay
        if (!_triggered)
            OnTriggerEnter(other);
    }

    public void BindGoal(LevelGoal goal)
    {
        _goal = goal;
        SetUnlocked(_goal != null ? _goal.IsReady : false);
    }

    public void SetUnlocked(bool unlocked)
    {
        _isUnlocked = unlocked;
        Debug.Log($"[LevelExit] SetUnlocked({unlocked})");
        UpdateVisualState();
    }

    void LoadNext()
    {
        // Race condition guard: si VN_ChoiceGateController está mostrando una
        // elección, él gestiona la transición de escena. Evitar doble LoadScene.
        var vnGate = Echoes.VN.VN_ChoiceGateController.Instance;
        if (vnGate != null && vnGate.IsShowing)
        {
            Debug.Log("[LevelExit] LoadNext pospuesto — VN gate activo.");
            return;
        }

        Debug.Log($"[LevelExit] LoadNext => target={nextSceneName}");
        PostProcessingSetup.PrepareForSceneReload();

        string target = !string.IsNullOrEmpty(nextSceneName) ? nextSceneName : null;

        if (string.IsNullOrEmpty(target) && loadNextBuildIndex)
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next < SceneManager.sceneCountInBuildSettings)
                target = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(next));
            else
                target = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(0));
        }

        if (string.IsNullOrEmpty(target))
        {
            Debug.LogWarning("[LevelExit] No target scene. Returning to MainMenu.");
            target = "MainMenu";
        }

        if (LoadingScreenController.Instance != null)
        {
            LoadingScreenController.Instance.LoadScene(target);
        }
        else if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(target);
        }
        else
        {
            SceneManager.LoadScene(target);
        }
    }

    static string ResolveCompletionToast(string sceneName)
    {
        switch (sceneName)
        {
            case "Level_01": return "Primero recuerdas.";
            case "Level_02": return "Luego pruebas.";
            case "Level_03": return "Dos decisiones se sostienen.";
            case "Level_04": return "El orden cambia el camino.";
            case "Level_05": return "La precision revela el patron.";
            case "Level_06": return "Tu identidad vuelve al centro.";
            default: return "Recuerdo restaurado.";
        }
    }

    void UpdateVisualState()
    {
        if (_renderers == null)
            return;

        Color tint = _isUnlocked
            ? new Color(0.4f, 0.7f, 1f, 1f)
            : new Color(0.4f, 0.48f, 0.58f, 0.75f);

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer rendererRef = _renderers[i];
            if (rendererRef == null)
                continue;

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            rendererRef.GetPropertyBlock(block);
            block.SetColor("_BaseColor", tint);
            block.SetColor("_EmissionColor", _isUnlocked ? tint * 2.5f : tint * 0.3f);
            rendererRef.SetPropertyBlock(block);
        }
    }
}

