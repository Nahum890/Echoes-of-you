using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Configura el post-procesado del juego con el stack del Universal Render
/// Pipeline (URP): crea un Volume global en runtime con un VolumeProfile y
/// habilita el post-procesado en la cámara.
///
/// Reemplaza la implementación anterior basada en Post-Processing Stack v2
/// (Built-in), que no se renderizaba bajo URP.
///
/// Nota: el Ambient Occlusion en URP es SSAO (un Renderer Feature en el asset
/// del renderer), no un override de Volume, por lo que se configura en el
/// editor sobre el Universal Renderer y no aquí.
/// </summary>
public class PostProcessingSetup : MonoBehaviour
{
    static PostProcessingSetup _instance;

    static GameObject _volumeGo;
    static Volume _volume;
    static VolumeProfile _runtimeProfile;

    Coroutine _setupRoutine;

    /// Perfil de Volume en runtime, modulado por GameFeelController.
    public static VolumeProfile RuntimeProfile => _runtimeProfile;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSetup()
    {
        EnsureInstance();
        _instance?.ScheduleSetup();
    }

    public static void PrepareForSceneReload()
    {
        if (_instance == null)
            return;

        _instance.CleanupRuntimeObjects();
    }

    static void EnsureInstance()
    {
        if (_instance != null)
            return;

        GameObject go = new GameObject("PostProcessingSetup");
        _instance = go.AddComponent<PostProcessingSetup>();
        DontDestroyOnLoad(go);
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _instance?.ScheduleSetup();
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ScheduleSetup();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (_setupRoutine != null)
        {
            StopCoroutine(_setupRoutine);
            _setupRoutine = null;
        }
    }

    void OnDestroy()
    {
        if (_instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        CleanupRuntimeObjects();
        _instance = null;
    }

    void ScheduleSetup()
    {
        if (_setupRoutine != null)
            StopCoroutine(_setupRoutine);

        _setupRoutine = StartCoroutine(SetupWhenCameraReady());
    }

    IEnumerator SetupWhenCameraReady()
    {
        CleanupRuntimeObjects();

        for (int frame = 0; frame < 40; frame++)
        {
            Camera cameraRef = Camera.main;
            if (cameraRef != null)
            {
                SetupPostProcessing(cameraRef);
                _setupRoutine = null;
                yield break;
            }

            yield return null;
        }

        _setupRoutine = null;
    }

    void SetupPostProcessing(Camera cameraRef)
    {
        if (cameraRef == null)
            return;

        // Habilitar el post-procesado en la cámara (equivalente al checkbox
        // "Post Processing" del UniversalAdditionalCameraData).
        var camData = cameraRef.GetUniversalAdditionalCameraData();
        if (camData != null)
            camData.renderPostProcessing = true;

        CleanupRuntimeObjects();

        _runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();

        Bloom bloom = _runtimeProfile.Add<Bloom>(true);
        bloom.intensity.Override(0.85f);
        bloom.threshold.Override(1.2f);
        bloom.scatter.Override(0.6f);

        Tonemapping tonemapping = _runtimeProfile.Add<Tonemapping>(true);
        tonemapping.mode.Override(TonemappingMode.ACES);

        ColorAdjustments grading = _runtimeProfile.Add<ColorAdjustments>(true);
        grading.postExposure.Override(-0.08f);
        grading.contrast.Override(22f);
        grading.saturation.Override(-28f);

        Vignette vignette = _runtimeProfile.Add<Vignette>(true);
        vignette.intensity.Override(0.33f);
        vignette.smoothness.Override(0.92f);
        vignette.color.Override(Color.black);

        ChromaticAberration ca = _runtimeProfile.Add<ChromaticAberration>(true);
        ca.intensity.Override(0.08f);

        LensDistortion ld = _runtimeProfile.Add<LensDistortion>(true);
        ld.intensity.Override(0f);

        FilmGrain grain = _runtimeProfile.Add<FilmGrain>(true);
        grain.type.Override(FilmGrainLookup.Medium1);
        grain.intensity.Override(0.28f);
        grain.response.Override(0.8f);

        _volumeGo = new GameObject("GlobalPostProcessVolume");
        DontDestroyOnLoad(_volumeGo);
        _volume = _volumeGo.AddComponent<Volume>();
        _volume.isGlobal = true;
        _volume.priority = 1f;
        _volume.profile = _runtimeProfile;
    }

    void CleanupRuntimeObjects()
    {
        if (_volumeGo != null)
        {
            Destroy(_volumeGo);
            _volumeGo = null;
            _volume = null;
        }

        if (_runtimeProfile != null)
        {
            Destroy(_runtimeProfile);
            _runtimeProfile = null;
        }
    }
}
