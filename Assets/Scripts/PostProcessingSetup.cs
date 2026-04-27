using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

/// <summary>
/// Auto-configura un Post Processing Volume global al iniciar la escena.
/// Solo funciona si com.unity.postprocessing está instalado.
/// Si no está instalado, el script no genera errores y no hace nada.
/// Limpia volumes duplicados al cambiar de escena.
/// Re-aplica PostProcessLayer en cada cambio de escena para evitar NullReferenceException.
/// </summary>
public class PostProcessingSetup : MonoBehaviour
{
    static PostProcessingSetup _instance;
    static GameObject _volumeGo;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSetup()
    {
        if (_instance != null)
            return;

        GameObject go = new GameObject("PostProcessingSetup");
        _instance = go.AddComponent<PostProcessingSetup>();
        DontDestroyOnLoad(go);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_instance != null)
            _instance.Setup();
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        Setup();
    }

    void Setup()
    {
#if UNITY_POST_PROCESSING_STACK_V2
        SetupPostProcessing();
#else
        Debug.Log("PostProcessingSetup: com.unity.postprocessing no detectado.");
#endif
    }

#if UNITY_POST_PROCESSING_STACK_V2
    void SetupPostProcessing()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Limpiar el Layer si por alguna razón la cámara persiste (aunque en Echoes se recrea)
        PostProcessLayer existingLayer = cam.GetComponent<PostProcessLayer>();
        if (existingLayer == null)
        {
            PostProcessLayer layer = cam.gameObject.AddComponent<PostProcessLayer>();
            layer.volumeTrigger = cam.transform;
            layer.volumeLayer = LayerMask.GetMask("Default");
            layer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
        }

        // Siempre reconstruir el Volume global de 0 para asegurar Profile fresco
        if (_volumeGo != null)
        {
            Destroy(_volumeGo);
        }

        _volumeGo = new GameObject("GlobalPostProcessVolume");
        PostProcessVolume volume = _volumeGo.AddComponent<PostProcessVolume>();
        volume.isGlobal = true;
        volume.priority = 1;

        PostProcessProfile profile = ScriptableObject.CreateInstance<PostProcessProfile>();

        // --- Bloom ---
        Bloom bloom = profile.AddSettings<Bloom>();
        bloom.enabled.value = true;
        bloom.intensity.value = 1.2f;
        bloom.intensity.overrideState = true;
        bloom.threshold.value = 1.1f;
        bloom.threshold.overrideState = true;
        bloom.softKnee.value = 0.5f;
        bloom.softKnee.overrideState = true;
        bloom.diffusion.value = 7f;
        bloom.diffusion.overrideState = true;

        // --- Color Grading ---
        ColorGrading cg = profile.AddSettings<ColorGrading>();
        cg.enabled.value = true;
        cg.gradingMode.value = GradingMode.LowDefinitionRange;
        cg.gradingMode.overrideState = true;
        cg.lift.value = new Vector4(-0.1f, -0.05f, 0.05f, 0f);
        cg.lift.overrideState = true;
        cg.gamma.value = new Vector4(0f, 0f, 0.01f, 0.05f);
        cg.gamma.overrideState = true;
        cg.gain.value = new Vector4(0f, 0f, 0.02f, 0f);
        cg.gain.overrideState = true;
        cg.saturation.value = -15f;
        cg.saturation.overrideState = true;
        cg.contrast.value = 15f;
        cg.contrast.overrideState = true;

        // --- Vignette ---
        Vignette vignette = profile.AddSettings<Vignette>();
        vignette.enabled.value = true;
        vignette.intensity.value = 0.4f;
        vignette.intensity.overrideState = true;
        vignette.smoothness.value = 1.0f;
        vignette.smoothness.overrideState = true;
        vignette.color.value = new Color(0f, 0f, 0f, 1f);
        vignette.color.overrideState = true;

        volume.profile = profile;
    }
#endif
}
