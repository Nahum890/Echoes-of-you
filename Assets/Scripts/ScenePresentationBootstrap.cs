using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ScenePresentationBootstrap : MonoBehaviour
{
    static bool _initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyPresentation(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        ApplyPresentation(scene);
    }

    static void ApplyPresentation(Scene scene)
    {
        CleanupExisting();

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.16f, 0.18f, 0.18f, 1f);
        RenderSettings.fogDensity = Mathf.Max(RenderSettings.fogDensity, 0.028f);
        RenderSettings.ambientLight = new Color(0.18f, 0.2f, 0.2f, 1f);

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.08f, 0.09f, 0.09f, 1f);
        }
    }

    static void CleanupExisting()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] roots = activeScene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] != null && roots[i].name == "RuntimePresentation")
                Destroy(roots[i]);
        }
    }

}
