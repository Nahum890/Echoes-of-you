using UnityEngine;
using UnityEngine.Audio;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

/// <summary>
/// Sistema de Game Feel con jerarquía de intensidad.
/// Separa feedback visual, audio, cámara y tiempo.
/// Cada evento tiene un peso diferente para evitar que todo se sienta igual.
/// </summary>
public class GameFeelController : MonoBehaviour
{
    public static GameFeelController Instance { get; private set; }

    // --- Intensidad ---
    public enum Intensity { Low, Medium, High, Critical }

    [Header("Particulas")]
    [SerializeField] ParticleSystem jumpEffectPrefab;
    [SerializeField] ParticleSystem landingEffectPrefab;
    [SerializeField] ParticleSystem hardLandingEffectPrefab;
    [SerializeField] ParticleSystem footstepDustPrefab;
    [SerializeField] ParticleSystem movementScrapePrefab;
    [SerializeField] ParticleSystem gravityShiftEffectPrefab;
    [SerializeField] ParticleSystem puzzleSolvedEffectPrefab;
    [SerializeField] ParticleSystem recordEffectPrefab;
    [SerializeField] ParticleSystem echoSpawnEffectPrefab;
    [SerializeField] ParticleSystem deathEffectPrefab;
    [SerializeField] ParticleSystem respawnEffectPrefab;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip landingClip;
    [SerializeField] AudioClip hardLandingClip;
    [SerializeField] AudioClip footstepClip;
    [SerializeField] AudioClip movementScrapeClip;
    [SerializeField] AudioClip gravityShiftClip;
    [SerializeField] AudioClip puzzleSolvedClip;
    [SerializeField] AudioClip recordClip;
    [SerializeField] AudioClip recordStopClip;
    [SerializeField] AudioClip echoSpawnClip;
    [SerializeField] AudioClip softErrorClip;
    [SerializeField] AudioClip platePressClip;
    [SerializeField] AudioClip doorMoveClip;
    [SerializeField] AudioClip playerDeathClip;
    [SerializeField] AudioClip respawnClip;
    [SerializeField] float defaultVolume = 1f;

    [Header("Deep Ambient Loops")]
    [SerializeField] AudioClip ambientLoopClip;
    [SerializeField] AudioClip industrialDroneClip;
    [SerializeField] AudioClip ventilationHumClip;
    [SerializeField] AudioClip clockChimeClip;

    [Header("Camera Shake")]
    [SerializeField] CameraShake cameraShake;
    [SerializeField] ThirdPersonCamera gameplayCamera;
    [SerializeField] FixedPuzzleCameraController fixedGameplayCamera;
    [SerializeField] float jumpShake = 0.08f;
    [SerializeField] float landingShake = 0.18f;
    [SerializeField] float gravityShake = 0.28f;
    [SerializeField] float puzzleSolvedShake = 0.22f;
    [SerializeField] float recordShake = 0.06f;
    [SerializeField] float echoSpawnShake = 0.1f;
    [SerializeField] float deathShake = 0.42f;

    [Header("Slow Motion")]
    [SerializeField] float slowMotionScale = 0.3f;
    [SerializeField] float slowMotionDuration = 0.15f;
    float _slowMotionTimer;
    float _slowMotionTarget = 1f;

    [Header("FOV Pulse")]
    [SerializeField] float baseFOV = 50f;
    float _fovPulseTarget;
    float _fovPulseTimer;
    float _fovPulseDuration;
    float _postProcessImpulse;
    float _vignetteImpulse;
    float _exposureImpulse;
    float _nextFootstepTime;
    float _nextScrapeTime;

    AudioSource _ambientSource1;
    AudioSource _ambientSource2;
    AudioSource _ambientSource3;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.65f;

        var audioMgr = EchoesAudioManager.EnsureExists();
        if (audioMgr != null)
        {
            audioSource.outputAudioMixerGroup = audioMgr.FindGroup("SFX");
        }

        if (cameraShake == null)
            cameraShake = GetComponent<CameraShake>();

        if (gameplayCamera == null)
            gameplayCamera = GetComponent<ThirdPersonCamera>();
        if (gameplayCamera == null)
            gameplayCamera = ThirdPersonCamera.ResolveActive();
        if (fixedGameplayCamera == null)
            fixedGameplayCamera = GetComponent<FixedPuzzleCameraController>();
        if (fixedGameplayCamera == null)
            fixedGameplayCamera = FixedPuzzleCameraController.ResolveActive();

        EnsureRuntimeFallbackEffects();
    }

    void Start()
    {
        SetupAmbientAudio();
    }

    void SetupAmbientAudio()
    {
        var audioMgr = EchoesAudioManager.EnsureExists();
        AudioMixerGroup musicGroup = audioMgr != null ? audioMgr.FindGroup("Music") : null;

        // 1. Room Tone loop
        if (ambientLoopClip != null)
        {
            _ambientSource1 = gameObject.AddComponent<AudioSource>();
            _ambientSource1.clip = ambientLoopClip;
            _ambientSource1.loop = true;
            _ambientSource1.volume = 0.15f * defaultVolume;
            _ambientSource1.spatialBlend = 0f; // 2D background ambience
            if (musicGroup != null) _ambientSource1.outputAudioMixerGroup = musicGroup;
            _ambientSource1.Play();
        }

        // 2. Industrial Drone / Synth loop
        if (industrialDroneClip != null)
        {
            _ambientSource2 = gameObject.AddComponent<AudioSource>();
            _ambientSource2.clip = industrialDroneClip;
            _ambientSource2.loop = true;
            _ambientSource2.volume = 0.12f * defaultVolume;
            _ambientSource2.spatialBlend = 0f;
            if (musicGroup != null) _ambientSource2.outputAudioMixerGroup = musicGroup;
            _ambientSource2.Play();
        }

        // 3. Ventilation Hum loop
        if (ventilationHumClip != null)
        {
            _ambientSource3 = gameObject.AddComponent<AudioSource>();
            _ambientSource3.clip = ventilationHumClip;
            _ambientSource3.loop = true;
            _ambientSource3.volume = 0.08f * defaultVolume;
            _ambientSource3.spatialBlend = 0f;
            if (musicGroup != null) _ambientSource3.outputAudioMixerGroup = musicGroup;
            _ambientSource3.Play();
        }

        // Periodically chime the clock for eerie atmosphere
        if (clockChimeClip != null)
        {
            InvokeRepeating(nameof(PlayEerieChime), 15f, 45f);
        }
    }

    void PlayEerieChime()
    {
        if (clockChimeClip == null) return;
        
        // Find player position or camera position
        Vector3 chimePos = Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 8f : transform.position;
        PlayClip3D(clockChimeClip, chimePos, defaultVolume * 0.45f, 0.72f); // slow pitch chime for eerie uncanniness
    }

    void Update()
    {
        // Slow motion recovery
        if (_slowMotionTimer > 0f)
        {
            _slowMotionTimer -= Time.unscaledDeltaTime;
            if (_slowMotionTimer <= 0f)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
            }
        }

        // FOV pulse recovery (para Cinemachine, controlado indirectamente)
        if (_fovPulseTimer > 0f)
        {
            _fovPulseTimer -= Time.unscaledDeltaTime;
        }

        UpdatePostProcessingAndCameraEffects();
    }

    void UpdatePostProcessingAndCameraEffects()
    {
        EchoRecorder recorder = Object.FindAnyObjectByType<EchoRecorder>();
        bool isRecording = recorder != null && recorder.IsRecording;
        bool hasEchoes = recorder != null && recorder.EchoCount > 0;

        // Soft dynamic audio muffling when player is recording (recalls memory sensory-deprivation)
        AudioListener listener = Object.FindAnyObjectByType<AudioListener>();
        if (listener != null)
        {
            var lowPass = listener.GetComponent<AudioLowPassFilter>();
            if (isRecording)
            {
                if (lowPass == null)
                {
                    lowPass = listener.gameObject.AddComponent<AudioLowPassFilter>();
                    lowPass.lowpassResonanceQ = 1.0f;
                }
                lowPass.enabled = true;
                lowPass.cutoffFrequency = Mathf.MoveTowards(lowPass.cutoffFrequency, 850f, Time.unscaledDeltaTime * 4000f);
            }
            else
            {
                if (lowPass != null)
                {
                    lowPass.cutoffFrequency = Mathf.MoveTowards(lowPass.cutoffFrequency, 22000f, Time.unscaledDeltaTime * 50000f);
                    if (lowPass.cutoffFrequency >= 20000f)
                        lowPass.enabled = false;
                }
            }
        }

        _postProcessImpulse = Mathf.MoveTowards(_postProcessImpulse, 0f, Time.unscaledDeltaTime * 1.8f);
        _vignetteImpulse = Mathf.MoveTowards(_vignetteImpulse, 0f, Time.unscaledDeltaTime * 1.2f);
        _exposureImpulse = Mathf.MoveTowards(_exposureImpulse, 0f, Time.unscaledDeltaTime * 1.1f);

        float targetCA = 0.08f + _postProcessImpulse * 0.42f;
        float targetLD = -_postProcessImpulse * 22f;
        float targetVignette = 0.33f + _vignetteImpulse * 0.28f;
        float targetExposure = -0.08f + _exposureImpulse * 0.32f;

        if (isRecording)
        {
            targetCA = Mathf.Max(targetCA, 0.42f + Mathf.PingPong(Time.unscaledTime * 5f, 0.1f));
            targetLD = Mathf.Min(targetLD, -22f + Mathf.PingPong(Time.unscaledTime * 10f, 5f));
            targetVignette = Mathf.Max(targetVignette, 0.52f);
            targetExposure = Mathf.Min(targetExposure, -0.22f);
        }
        else if (hasEchoes)
        {
            // Aberración media y distorsión sutil que pulsa como un latido
            targetCA = Mathf.Max(targetCA, 0.22f + Mathf.Sin(Time.unscaledTime * 4.5f) * 0.04f);
            targetLD = Mathf.Min(targetLD, -6f + Mathf.Sin(Time.unscaledTime * 3f) * 3f);

            // Pulso/respiración de cámara durante la presencia de ecos
            float breatheFov = baseFOV + Mathf.Sin(Time.unscaledTime * 3f) * 1.5f;
            RequestCameraPulse(breatheFov, 0.05f);
        }

#if UNITY_POST_PROCESSING_STACK_V2
        PostProcessProfile profile = PostProcessingSetup.RuntimeProfile;
        if (profile != null)
        {
            if (profile.TryGetSettings<ChromaticAberration>(out var ca))
            {
                ca.intensity.value = Mathf.MoveTowards(ca.intensity.value, targetCA, Time.unscaledDeltaTime * 1.5f);
            }
            if (profile.TryGetSettings<LensDistortion>(out var ld))
            {
                ld.intensity.value = Mathf.MoveTowards(ld.intensity.value, targetLD, Time.unscaledDeltaTime * 60f);
            }
            if (profile.TryGetSettings<Vignette>(out var vignette))
            {
                vignette.intensity.value = Mathf.MoveTowards(vignette.intensity.value, targetVignette, Time.unscaledDeltaTime * 1.8f);
            }
            if (profile.TryGetSettings<ColorGrading>(out var grading))
            {
                grading.postExposure.value = Mathf.MoveTowards(grading.postExposure.value, targetExposure, Time.unscaledDeltaTime * 1.5f);
                float targetSaturation = isRecording ? -42f : -28f;
                grading.saturation.value = Mathf.MoveTowards(grading.saturation.value, targetSaturation, Time.unscaledDeltaTime * (isRecording ? 28f : 8f));
            }
            if (profile.TryGetSettings<Grain>(out var grain))
            {
                float targetGrain = isRecording ? 0.55f : 0.28f;
                grain.intensity.value = Mathf.MoveTowards(grain.intensity.value, targetGrain, Time.unscaledDeltaTime * 2.2f);
            }
        }
#endif
    }

    void OnDestroy()
    {
        // Asegurar que timeScale se restaura si el objeto se destruye
        if (Instance == this)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            Instance = null;
        }
    }

    // ═══════════════════════════════════════════
    // EVENTOS DE JUEGO — cada uno con peso distinto
    // ═══════════════════════════════════════════

    // Low: salto básico
    public void PlayJump(Vector3 position, Vector3 up)
    {
        SpawnEffect(jumpEffectPrefab, position, up);
        PlayClip3D(jumpClip, position, defaultVolume * 0.7f, 1.04f);
        cameraShake?.AddShake(jumpShake);
        RequestPostImpulse(0.12f, 0.08f, 0.03f);
    }

    // Medium: aterrizaje proporcional al impacto
    public void PlayLanding(Vector3 position, Vector3 up, float impactSpeed)
    {
        bool hard = impactSpeed >= 13f;
        SpawnEffect(hard ? hardLandingEffectPrefab : landingEffectPrefab, position, up);
        float vol = Mathf.Lerp(0.5f, 1f, Mathf.Clamp01(impactSpeed / 12f));
        PlayClip3D(hard && hardLandingClip != null ? hardLandingClip : landingClip, position, defaultVolume * vol, hard ? 0.82f : 0.96f);
        cameraShake?.AddShake(Mathf.Clamp01(landingShake + impactSpeed * 0.015f));
        RequestCameraPulse(hard ? 55f : 57f, hard ? 0.18f : 0.09f);
        RequestPostImpulse(hard ? 0.34f : 0.16f, hard ? 0.22f : 0.09f, hard ? -0.12f : -0.04f);
    }

    public void PlayFootstep(Vector3 position, Vector3 up, float speed)
    {
        if (Time.time < _nextFootstepTime)
            return;

        _nextFootstepTime = Time.time + Mathf.Lerp(0.16f, 0.08f, Mathf.InverseLerp(3f, 12f, speed));
        SpawnEffect(footstepDustPrefab, position, up);
        PlayClip3D(footstepClip, position, defaultVolume * Mathf.Lerp(0.18f, 0.42f, Mathf.InverseLerp(2f, 12f, speed)), Mathf.Lerp(0.92f, 1.08f, Random.value));
    }

    public void PlayMovementScrape(Vector3 position, Vector3 up, float intensity)
    {
        if (Time.time < _nextScrapeTime)
            return;

        _nextScrapeTime = Time.time + 0.22f;
        SpawnEffect(movementScrapePrefab, position, up);
        PlayClip3D(movementScrapeClip, position, defaultVolume * Mathf.Lerp(0.08f, 0.22f, intensity), 1.15f);
    }

    // High: cambio de gravedad
    public void PlayGravityShift(Vector3 position, Vector3 up)
    {
        SpawnEffect(gravityShiftEffectPrefab, position, up);
        PlayClip3D(gravityShiftClip, position, defaultVolume, 0.74f);
        cameraShake?.AddShake(gravityShake);
        RequestCameraPulse(54f, 0.22f);
        RequestPostImpulse(0.55f, 0.26f, 0.1f);
        ApplySlowMotion(0.4f, 0.12f);
    }

    // Critical: puzzle resuelto — máximo feedback
    public void PlayPuzzleSolved(Vector3 position)
    {
        SpawnEffect(puzzleSolvedEffectPrefab, position, Vector3.up);
        PlayClip3D(puzzleSolvedClip, position, defaultVolume * 1.2f, 0.78f);
        cameraShake?.AddShake(puzzleSolvedShake + 0.18f);
        RequestCameraPulse(52f, 0.55f);
        RequestPostImpulse(0.68f, 0.38f, 0.18f);
        ApplySlowMotion(slowMotionScale, 0.2f);
    }

    // Medium: inicio de grabación
    public void PlayRecordStart(Vector3 position, Vector3 up)
    {
        PlayClip3D(recordClip, position, defaultVolume * 0.9f, 0.86f);
        cameraShake?.AddShake(recordShake);
        RequestCameraPulse(48f, 0.2f);
        RequestPostImpulse(0.42f, 0.22f, -0.06f);
        ApplyRecordingTimeFeel(0.82f);
    }

    // Low: fin de grabación
    public void PlayRecordStop(Vector3 position)
    {
        PlayClip3D(recordStopClip, position, defaultVolume * 0.9f, 1.08f);
    }

    // High: eco creado — momento clave, slow motion breve
    public void PlayEchoSpawn(Vector3 position)
    {
        PlayClip3D(echoSpawnClip, position, defaultVolume * 1.05f, 0.68f);
        cameraShake?.AddShake(echoSpawnShake * 0.45f);
        RequestCameraPulse(50f, 0.12f);
        RequestPostImpulse(0.18f, 0.08f, 0.02f);
    }

    // Low: error suave
    public void PlaySoftError(Vector3 position)
    {
        PlayClip3D(softErrorClip, position, defaultVolume * 0.6f, 0.72f);
        RequestPostImpulse(0.18f, 0.14f, -0.06f);
    }

    // Low: placa presionada
    public void PlayPlatePress(Vector3 position)
    {
        PlayClip3D(platePressClip, position, defaultVolume * 0.85f, 0.9f);
        cameraShake?.AddShake(0.04f);
        RequestPostImpulse(0.08f, 0.04f, 0.02f);
    }

    // Medium: puerta moviéndose
    public void PlayDoorMove(Vector3 position)
    {
        PlayClip3D(doorMoveClip, position, defaultVolume * 0.9f, 0.82f);
        cameraShake?.AddShake(0.06f);
    }

    // High: muerte del jugador
    public void PlayPlayerDeath(Vector3 position)
    {
        SpawnEffect(deathEffectPrefab, position, Vector3.up);
        PlayClip3D(playerDeathClip, position, defaultVolume * 1.2f, 0.55f);
        cameraShake?.AddShake(deathShake);
        RequestCameraPulse(44f, 0.6f);
        RequestPostImpulse(0.9f, 0.62f, -0.2f);
        ApplySlowMotion(0.2f, 0.34f);
    }

    public void PlayRespawn(Vector3 position)
    {
        SpawnEffect(respawnEffectPrefab, position, Vector3.up);
        PlayClip3D(respawnClip, position, defaultVolume, 0.92f);
        cameraShake?.AddShake(0.12f);
        RequestCameraPulse(56f, 0.22f);
        RequestPostImpulse(0.42f, 0.18f, 0.12f);
    }

    // ═══════════════════════════════════════════
    // SUBSISTEMAS INTERNOS
    // ═══════════════════════════════════════════

    // Slow motion controlado — breve, no rompe gameplay
    void ApplySlowMotion(float scale, float duration)
    {
        Time.timeScale = Mathf.Clamp(scale, 0.1f, 1f);
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        _slowMotionTimer = duration;
    }

    void ApplyRecordingTimeFeel(float scale)
    {
        EchoRecorder recorder = Object.FindAnyObjectByType<EchoRecorder>();
        if (recorder == null || !recorder.IsRecording)
            return;

        Time.timeScale = Mathf.Clamp(scale, 0.65f, 1f);
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    void RequestPostImpulse(float chromaticLens, float vignette, float exposure)
    {
        _postProcessImpulse = Mathf.Clamp01(Mathf.Max(_postProcessImpulse, chromaticLens));
        _vignetteImpulse = Mathf.Clamp01(Mathf.Max(_vignetteImpulse, vignette));
        _exposureImpulse = Mathf.Clamp(_exposureImpulse + exposure, -1f, 1f);
    }

    void RequestCameraPulse(float targetFov, float holdSeconds)
    {
        CinematicCameraDynamics cinematicCam = FindAnyObjectByType<CinematicCameraDynamics>();
        if (cinematicCam != null)
        {
            cinematicCam.RequestFovPulse(targetFov, holdSeconds);
            return;
        }

        if (gameplayCamera == null)
            gameplayCamera = ThirdPersonCamera.ResolveActive();
        if (fixedGameplayCamera == null)
            fixedGameplayCamera = FixedPuzzleCameraController.ResolveActive();

        if (gameplayCamera != null)
        {
            gameplayCamera.RequestFovPulse(targetFov, holdSeconds);
            return;
        }

        fixedGameplayCamera?.RequestFovPulse(targetFov, holdSeconds);
    }

    // Partículas — spawn y auto-destroy
    void SpawnEffect(ParticleSystem prefab, Vector3 position, Vector3 up)
    {
        if (prefab == null)
            return;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, up.normalized);
        ParticleSystem instance = Instantiate(prefab, position, rotation);
        instance.gameObject.SetActive(true);
        instance.Play(true);
        float lifetime = instance.main.duration + instance.main.startLifetime.constantMax;
        Destroy(instance.gameObject, Mathf.Max(1f, lifetime + 0.5f));
    }

    // Audio 3D — usa PlayClipAtPoint para espacialidad real
    void PlayClip3D(AudioClip clip, Vector3 position, float volume, float pitch = 1f)
    {
        if (clip == null)
            return;

        GameObject audioObject = new GameObject("OneShotAudio");
        audioObject.transform.position = position;
        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.spatialBlend = 1f;
        source.minDistance = 2f;
        source.maxDistance = 18f;
        source.rolloffMode = AudioRolloffMode.Linear;
        
        var audioMgr = EchoesAudioManager.EnsureExists();
        if (audioMgr != null)
        {
            source.outputAudioMixerGroup = audioMgr.FindGroup("SFX");
        }

        source.Play();
        Destroy(audioObject, Mathf.Max(0.1f, clip.length / Mathf.Max(0.05f, Mathf.Abs(pitch))) + 0.1f);
    }

    void EnsureRuntimeFallbackEffects()
    {
        if (jumpEffectPrefab == null) jumpEffectPrefab = CreateParticlePrefab("FX_JumpDust", new Color(0.55f, 0.62f, 0.68f, 0.34f), 18, 0.28f, 1.5f, 0.08f, ParticleSystemShapeType.Cone);
        if (landingEffectPrefab == null) landingEffectPrefab = CreateParticlePrefab("FX_LandingDust", new Color(0.58f, 0.62f, 0.68f, 0.42f), 34, 0.36f, 2.1f, 0.12f, ParticleSystemShapeType.Circle);
        if (hardLandingEffectPrefab == null) hardLandingEffectPrefab = CreateParticlePrefab("FX_HardLandingBurst", new Color(0.72f, 0.78f, 0.86f, 0.62f), 72, 0.48f, 3.6f, 0.18f, ParticleSystemShapeType.Circle);
        if (footstepDustPrefab == null) footstepDustPrefab = CreateParticlePrefab("FX_FootstepDust", new Color(0.5f, 0.55f, 0.6f, 0.26f), 10, 0.22f, 1.0f, 0.06f, ParticleSystemShapeType.Circle);
        if (movementScrapePrefab == null) movementScrapePrefab = CreateParticlePrefab("FX_MovementScrape", new Color(0.82f, 0.9f, 1f, 0.38f), 8, 0.18f, 1.7f, 0.045f, ParticleSystemShapeType.Cone);
        if (gravityShiftEffectPrefab == null) gravityShiftEffectPrefab = CreateParticlePrefab("FX_GravityShift", new Color(0.35f, 0.82f, 1f, 0.72f), 90, 0.75f, 4.2f, 0.09f, ParticleSystemShapeType.Sphere);
        if (puzzleSolvedEffectPrefab == null) puzzleSolvedEffectPrefab = CreateParticlePrefab("FX_PuzzleSolved", new Color(1f, 0.82f, 0.42f, 0.82f), 120, 0.9f, 5.6f, 0.1f, ParticleSystemShapeType.Sphere);
        // Recording uses CinematicRecordingOverlay + post FX — no smoke burst.
        if (deathEffectPrefab == null) deathEffectPrefab = CreateParticlePrefab("FX_DeathDissolve", new Color(0.05f, 0.12f, 0.2f, 0.7f), 110, 1.1f, 3.0f, 0.14f, ParticleSystemShapeType.Sphere);
        if (respawnEffectPrefab == null) respawnEffectPrefab = CreateParticlePrefab("FX_RespawnReform", new Color(0.65f, 0.9f, 1f, 0.72f), 120, 0.95f, 3.5f, 0.08f, ParticleSystemShapeType.Sphere);
    }

    ParticleSystem CreateParticlePrefab(string name, Color color, int burstCount, float lifetime, float speed, float size, ParticleSystemShapeType shapeType)
    {
        GameObject go = new GameObject(name);
        go.SetActive(false);
        DontDestroyOnLoad(go);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.12f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.55f, lifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.35f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.55f, size * 1.65f);
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

        var shape = ps.shape;
        shape.shapeType = shapeType;
        shape.radius = shapeType == ParticleSystemShapeType.Sphere ? 0.65f : 0.35f;
        shape.angle = 24f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.15f, 0.85f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.24f;
        noise.frequency = 0.7f;
        noise.scrollSpeed = 0.35f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(color, 0f), new GradientColorKey(Color.white, 0.4f), new GradientColorKey(color, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(color.a, 0.12f), new GradientAlphaKey(color.a * 0.45f, 0.55f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = gradient;

        var rendererRef = go.GetComponent<ParticleSystemRenderer>();
        rendererRef.material = new Material(Shader.Find("Sprites/Default"));
        rendererRef.renderMode = ParticleSystemRenderMode.Billboard;
        rendererRef.sortMode = ParticleSystemSortMode.Distance;
        return ps;
    }
}
