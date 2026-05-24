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
    GameStateController _gameState;
    float _hardResetHold;
    float _playTimeSaveTimer;
    int _sessionEchoes;
    int _sessionDeaths;
    float _sessionPlaySeconds;
    bool _completed;
    bool _restartRequested;

    public static LevelRuntimeController Instance { get; private set; }

    public int SessionEchoes => _sessionEchoes;
    public int SessionDeaths => _sessionDeaths;
    public float SessionPlaySeconds => _sessionPlaySeconds;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _gameState = FindAnyObjectByType<GameStateController>();
        if (_gameState == null)
            _gameState = new GameObject("GameStateController").AddComponent<GameStateController>();
    }

    void Start()
    {
        // Ocultar y bloquear el cursor para una mejor experiencia de gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _hud = FindAnyObjectByType<GameHUD>();
        _recorder = FindAnyObjectByType<EchoRecorder>();
        if (_recorder != null)
            _recorder.EchoCreated += OnEchoCreated;

        string sceneName = SceneManager.GetActiveScene().name;
        if (GameProgress.GetSceneIndex(sceneName) >= 0)
            GameProgress.SetLastPlayedScene(sceneName);

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
            GameFeelController.Instance?.PlayRespawn(player.transform.position);

        if (_hud != null)
        {
            _hud.SetObjective(ResolveObjective());

            if (!string.IsNullOrEmpty(introLine))
                _hud.ShowToast(introLine, new Color(0.95f, 0.96f, 0.98f, 1f), 2.6f);
                
            CheckAndShowProgressiveHint();
        }
    }
    
    void CheckAndShowProgressiveHint()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        int deaths = PlayerPrefs.GetInt("Deaths_" + sceneName, 0);
        
        if (deaths < 3) return;

        string hint = "";
        if (sceneName == "Level_01")
            hint = deaths == 1 ? "Mantén F y manda la proyección a la zona azul lateral." : "Suelta F cuando la proyección esté sobre la zona azul; cruza mientras el puente sube.";
        else if (sceneName == "Level_02")
            hint = deaths == 1 ? "La zona azul izquierda empuja el bloque gigante." : "Proyecta con F hacia la zona azul, suelta F, y usa el elevador cuando el bloque libere la ruta.";
        else if (sceneName == "Level_03")
            hint = deaths == 1 ? "Lleva la proyección al pasillo izquierdo (zona azul)." : "Suelta F cuando el eco esté en la zona azul, cruza el puente y activa la placa final.";
        else if (sceneName == "Level_04")
            hint = deaths == 1 ? "Proyecta con F hasta la placa del pasillo izquierdo." : "Suelta F, cruza por el centro y pisa tu placa en la isla; hacen falta las dos placas.";
        else if (sceneName == "Level_05")
            hint = deaths == 1 ? "Divide el puzzle en tres consecuencias." : "Un eco mueve el bloque, otro cubre la energia, y otro entra/sale de la trampa.";
        else if (sceneName == "Level_06")
            hint = deaths == 1 ? "No busques una sola grabacion perfecta." : "Usa tres ecos: contrapeso, proteccion del nucleo y trampa final.";
        else if (sceneName == "Level_07")
            hint = deaths == 1 ? "Sala de enlace: proyecta al pasillo izquierdo." : "Activa la placa del eco y luego la tuya en el corredor central.";
        else if (sceneName == "Level_08")
            hint = deaths == 1 ? "Los bordes rojos matan: no saltes al vacío." : "Proyecta al pasillo izquierdo, suelta F y cruza el puente central.";
        else if (sceneName == "Level_09")
            hint = deaths == 1 ? "El muro rojo mata si lo tocas sin eco." : "Atraviesa el muro con F; cuando se vuelva azul, cruza.";
        else if (sceneName == "Level_10")
            hint = deaths == 1 ? "Evita el foso rojo al inicio." : "Activa la placa del eco y luego la tuya para abrir el núcleo.";

        if (!string.IsNullOrEmpty(hint))
            _hud.ShowToast("Pista: " + hint, new Color(0.5f, 0.8f, 1f, 1f), 4.5f);
    }

    void OnEchoCreated(int _)
    {
        _sessionEchoes++;
    }

    void Update()
    {
        if (_completed)
            return;

        if (GameProgress.GetSceneIndex(SceneManager.GetActiveScene().name) >= 0)
        {
            _sessionPlaySeconds += Time.deltaTime;
            _playTimeSaveTimer += Time.deltaTime;
            if (_playTimeSaveTimer >= 5f)
            {
                GameProgress.AddPlayTime(_playTimeSaveTimer);
                _playTimeSaveTimer = 0f;
                GameProgress.SavePlayTime();
            }
        }

        if (allowSoftReset && Input.GetKeyDown(KeyCode.Q))
            SoftReset();

        if (!allowHardReset)
            return;

        if (Input.GetKey(KeyCode.T))
        {
            _hardResetHold += Time.unscaledDeltaTime;

            if (_hardResetHold >= hardResetHoldSeconds)
                RequestRestart(0f);
        }
        else
        {
            _hardResetHold = 0f;
        }
    }

    public void SoftReset()
    {
        if (_restartRequested || _completed)
            return;

        _recorder ??= FindAnyObjectByType<EchoRecorder>();
        _hud ??= FindAnyObjectByType<GameHUD>();

        _recorder?.ClearAllEchoes(false);

        MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IResettableLevelObject resettable)
                resettable.ResetLevelState();
        }

        _hud?.ShowToast("Ecos limpiados", new Color(0.48f, 0.94f, 0.78f, 1f), 1.1f);
        _hud?.SetObjective(ResolveObjective());
    }

    public void SetObjective(string nextObjective)
    {
        objectiveText = nextObjective;
        _hud ??= FindAnyObjectByType<GameHUD>();
        _hud?.SetObjective(nextObjective);
    }

    public void HandlePlayerDeath(Vector3 position, float restartDelay = 1.2f)
    {
        if (_completed || _restartRequested)
            return;

        string sceneName = SceneManager.GetActiveScene().name;
        int deaths = PlayerPrefs.GetInt("Deaths_" + sceneName, 0);
        PlayerPrefs.SetInt("Deaths_" + sceneName, deaths + 1);
        PlayerPrefs.Save();
        _sessionDeaths++;

        _completed = true;
        _hud ??= FindAnyObjectByType<GameHUD>();
        _hud?.ShowToast("Reiniciando memoria...", new Color(1f, 0.43f, 0.43f, 1f), restartDelay);
        _gameState?.NotifyPlayerDeath(position);
        RequestRestart(restartDelay);
    }

    public void OnLevelCompleted(Vector3 position, string toastOverride = "")
    {
        if (_completed)
            return;

        _completed = true;
        _hud ??= FindAnyObjectByType<GameHUD>();
        _hud?.SetObjective("Memoria restaurada.");

        string toast = !string.IsNullOrEmpty(toastOverride) ? toastOverride : completionLine;
        if (!string.IsNullOrEmpty(toast))
            _hud?.ShowToast(toast, new Color(1f, 0.83f, 0.42f, 1f), 1.8f);

        _gameState?.NotifyLevelCompleted(position);
    }

    public void RequestRestart(float delaySeconds)
    {
        if (_restartRequested)
            return;

        _restartRequested = true;
        _gameState ??= FindAnyObjectByType<GameStateController>();
        if (_gameState != null)
        {
            _gameState.RequestSceneRestart(delaySeconds);
            return;
        }

        PostProcessingSetup.PrepareForSceneReload();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PrepareForSceneReload()
    {
        _recorder ??= FindAnyObjectByType<EchoRecorder>();
        _recorder?.ClearAllEchoes(false);

        MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IResettableLevelObject resettable)
                resettable.ResetLevelState();
        }
    }

    string ResolveObjective()
    {
        LevelGoal goal = FindAnyObjectByType<LevelGoal>();
        return goal != null ? goal.ObjectiveText : objectiveText;
    }

    void FlushPlayTime()
    {
        if (_playTimeSaveTimer > 0f)
        {
            GameProgress.AddPlayTime(_playTimeSaveTimer);
            _playTimeSaveTimer = 0f;
            GameProgress.SavePlayTime();
        }
    }

    void OnDestroy()
    {
        if (_recorder != null)
            _recorder.EchoCreated -= OnEchoCreated;

        FlushPlayTime();

        if (Instance == this)
            Instance = null;
    }
}
