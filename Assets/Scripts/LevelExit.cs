using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Al entrar el jugador, carga la siguiente escena por nombre o por índice + 1.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LevelExit : MonoBehaviour
{
    public string nextSceneName;
    public bool loadNextBuildIndex = true;
    public float delaySeconds = 0.35f;

    bool _triggered;

    void Awake()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_triggered || !other.CompareTag("Player"))
            return;

        _triggered = true;
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
}
