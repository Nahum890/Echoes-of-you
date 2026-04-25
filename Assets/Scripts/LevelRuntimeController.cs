using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelRuntimeController : MonoBehaviour
{
    [Header("Copy")]
    [SerializeField] string objectiveText = "Llega a la salida.";
    [SerializeField] string introLine = "";
    [SerializeField] string completionLine = "";

    [Header("Reset")]
    [SerializeField] bool allowSoftReset = true;
    [SerializeField] bool allowHardReset = true;
    [SerializeField] float hardResetHoldSeconds = 0.5f;

    GameHUD _hud;
    EchoRecorder _recorder;
    float _hardResetHold;
    bool _completed;

    public static LevelRuntimeController Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        _hud = FindAnyObjectByType<GameHUD>();
        _recorder = FindAnyObjectByType<EchoRecorder>();

        if (_hud != null)
        {
            _hud.SetObjective(objectiveText);

            if (!string.IsNullOrEmpty(introLine))
                _hud.ShowToast(introLine, new Color(0.95f, 0.96f, 0.98f, 1f), 2.6f);
        }
    }

    void Update()
    {
        if (_completed)
            return;

        if (allowSoftReset && Input.GetKeyDown(KeyCode.Q))
            SoftReset();

        if (!allowHardReset)
            return;

        if (Input.GetKey(KeyCode.T))
        {
            _hardResetHold += Time.unscaledDeltaTime;

            if (_hardResetHold >= hardResetHoldSeconds)
                RestartScene();
        }
        else
        {
            _hardResetHold = 0f;
        }
    }

    public void SoftReset()
    {
        _recorder ??= FindAnyObjectByType<EchoRecorder>();
        _hud ??= FindAnyObjectByType<GameHUD>();

        _recorder?.ClearAllEchoes(false);

        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IResettableLevelObject resettable)
                resettable.ResetLevelState();
        }

        _hud?.ShowToast("Ecos limpiados", new Color(0.48f, 0.94f, 0.78f, 1f), 1.1f);
    }

    public void OnLevelCompleted()
    {
        if (_completed)
            return;

        _completed = true;
        _hud ??= FindAnyObjectByType<GameHUD>();

        if (!string.IsNullOrEmpty(completionLine))
            _hud?.ShowToast(completionLine, new Color(1f, 0.83f, 0.42f, 1f), 1.8f);
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
