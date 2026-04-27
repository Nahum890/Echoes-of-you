using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Marca el nivel como completado y carga el siguiente destino.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LevelExit : MonoBehaviour
{
    public string nextSceneName;
    public bool loadNextBuildIndex = true;
    public float delaySeconds = 1.35f;

    [Header("Completion Copy")]
    [SerializeField] string completionToast = "";

    bool _triggered;

    void Awake()
    {
        var colliderRef = GetComponent<Collider>();
        colliderRef.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_triggered || !other.CompareTag("Player"))
            return;

        _triggered = true;

        string sceneName = SceneManager.GetActiveScene().name;
        GameProgress.MarkSceneCompleted(sceneName);

        GameHUD hud = FindAnyObjectByType<GameHUD>();
        string toast = string.IsNullOrEmpty(completionToast) ? ResolveCompletionToast(sceneName) : completionToast;
        hud?.ShowToast(toast, new Color(1f, 0.83f, 0.42f, 1f), 1.6f);

        LevelRuntimeController.Instance?.OnLevelCompleted();
        Invoke(nameof(LoadNext), delaySeconds);
    }

    void LoadNext()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        if (loadNextBuildIndex)
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(next);
            else
                SceneManager.LoadScene(0);
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
}
