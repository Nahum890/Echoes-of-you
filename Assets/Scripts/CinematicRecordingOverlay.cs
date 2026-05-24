using UnityEngine;

/// <summary>
/// VHS / tape-recording overlay while the player is recording an echo.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CinematicRecordingOverlay : MonoBehaviour
{
    [SerializeField] float recordTimeScale = 0.82f;
    [SerializeField] float scanlineStrength = 0.22f;
    [SerializeField] float grainStrength = 0.14f;
    [SerializeField] float flickerStrength = 0.06f;
    [SerializeField] float desaturateStrength = 0.35f;

    static readonly int PropIntensity = Shader.PropertyToID("_Intensity");
    static readonly int PropScanlines = Shader.PropertyToID("_Scanlines");
    static readonly int PropGrain = Shader.PropertyToID("_Grain");
    static readonly int PropFlicker = Shader.PropertyToID("_Flicker");
    static readonly int PropDesaturate = Shader.PropertyToID("_Desaturate");
    static readonly int PropTime = Shader.PropertyToID("_EffectTime");

    Material _material;
    EchoRecorder _recorder;
    bool _wasRecording;
    float _blend;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureOnMainCamera()
    {
        Camera cameraRef = Camera.main;
        if (cameraRef == null || cameraRef.GetComponent<CinematicRecordingOverlay>() != null)
            return;

        cameraRef.gameObject.AddComponent<CinematicRecordingOverlay>();
    }

    void Awake()
    {
        Shader shader = Shader.Find("Hidden/Echoes/CinematicRecording");
        if (shader != null)
            _material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
    }

    void OnDestroy()
    {
        if (_material != null)
            Destroy(_material);
    }

    void Update()
    {
        if (_recorder == null)
            _recorder = FindAnyObjectByType<EchoRecorder>();

        bool recording = _recorder != null && _recorder.IsRecording;
        float target = recording ? 1f : 0f;
        _blend = Mathf.MoveTowards(_blend, target, Time.unscaledDeltaTime * (recording ? 5f : 3f));

        if (recording && !_wasRecording)
        {
            Time.timeScale = recordTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        else if (!recording && _wasRecording && Time.timeScale < 1f)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        _wasRecording = recording;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_material == null || _blend <= 0.001f)
        {
            Graphics.Blit(source, destination);
            return;
        }

        _material.SetFloat(PropIntensity, _blend);
        _material.SetFloat(PropScanlines, scanlineStrength);
        _material.SetFloat(PropGrain, grainStrength);
        _material.SetFloat(PropFlicker, flickerStrength);
        _material.SetFloat(PropDesaturate, desaturateStrength);
        _material.SetFloat(PropTime, Time.unscaledTime);
        Graphics.Blit(source, destination, _material);
    }
}
