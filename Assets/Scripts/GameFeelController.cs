using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;

/// <summary>
/// Sistema de Game Feel con jerarquía de intensidad.
/// Separa feedback visual, audio, cámara y tiempo.
/// Cada evento tiene un peso diferente para evitar que todo se sienta igual.
/// POST-PROCESSING: Pulsos discretos (NO MoveTowards cada frame).
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
    [SerializeField] bool enableBitcrusher = true;
    [Range(0f, 1f)][SerializeField] float bitcrusherDryWet = 0.18f;
    AudioDistortionFilter _bitcrusherFilter;
    [SerializeField] AudioClip landingClip;
    [SerializeField] AudioClip hardLandingClip;
    [SerializeField] AudioClip footstepClip;
    [SerializeField] AudioClip movementScrapeClip;
    [SerializeField] AudioClip gravityShiftClip;
    [SerializeField] AudioClip puzzleSolvedClip;
    [SerializeField] AudioClip recordClip;
    [SerializeField] AudioClip recordStopClip;
    [SerializeField] AudioClip echoSpawnClip;
    [SerializeField] AudioClip echoFadeClip;
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
    // Bridge Cinemachine: cuando una CinemachineBrain controla la cámara (PS1 desactivado),
    // CameraShake.PositionOffset/RotationOffset no tienen consumidor (ThirdPersonCamera se
    // deshabilita solo). Este impulse source inyecta el shake en la pipeline Cinemachine.
    CinemachineImpulseSource _impulseSource;
    CinemachineBrain _cinemachineBrain;
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

    [Header("FOV Pulse")]
    float _fovPulseTarget;
    float _fovPulseTimer;
    float _fovPulseDuration;

    AudioSource _ambientSource1;
    AudioSource _ambientSource2;
    AudioSource _ambientSource3;

    float _nextFootstepTime;
    float _nextScrapeTime;
    float _nextMechanicTickTime;
    float _nextPlatePressTime;
    float _nextEchoSpawnTime;
    float _nextEchoFadeTime;

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

        if (enableBitcrusher)
        {
            _bitcrusherFilter = GetComponent<AudioDistortionFilter>();
            if (_bitcrusherFilter == null)
                _bitcrusherFilter = gameObject.AddComponent<AudioDistortionFilter>();
            _bitcrusherFilter.distortionLevel = Mathf.Lerp(0.05f, 0.35f, bitcrusherDryWet);
        }

        if (cameraShake == null)
            cameraShake = GetComponent<CameraShake>();

        // Cinemachine impulse bridge — crear dinámicamente si falta.
        _cinemachineBrain = FindAnyObjectByType<CinemachineBrain>();
        if (_cinemachineBrain != null)
        {
            _impulseSource = GetComponent<CinemachineImpulseSource>();
            if (_impulseSource == null)
                _impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }

        if (gameplayCamera == null)
            gameplayCamera = GetComponent<ThirdPersonCamera>();
        if (gameplayCamera == null)
            gameplayCamera = ThirdPersonCamera.ResolveActive();
        if (fixedGameplayCamera == null)
            fixedGameplayCamera = GetComponent<FixedPuzzleCameraController>();
        if (fixedGameplayCamera == null)
            fixedGameplayCamera = FixedPuzzleCameraController.ResolveActive();

        EnsureRuntimeFallbackEffects();
        EnsureRuntimeFallbackAudio();
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
            _ambientSource1.spatialBlend = 0f;
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

        if (clockChimeClip != null)
        {
            InvokeRepeating(nameof(PlayEerieChime), 15f, 45f);
        }
    }

    void PlayEerieChime()
    {
        if (clockChimeClip == null) return;

        Vector3 chimePos = Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 8f : transform.position;
        PlayClip3D(clockChimeClip, chimePos, defaultVolume * 0.45f, 0.72f);
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

        // FOV pulse recovery
        if (_fovPulseTimer > 0f)
        {
            _fovPulseTimer -= Time.unscaledDeltaTime;
        }
    }

    // ═══════════════════════════════════════════
    // POST-PROCESSING: PULSOS DISCRETOS (coroutines)
    // ═══════════════════════════════════════════

    public void PulseCA(float target, float duration)
    {
        StartCoroutine(PulseCA_CR(target, duration));
    }

    IEnumerator PulseCA_CR(float target, float dur)
    {
        var profile = PostProcessingSetup.RuntimeProfile;
        if (profile == null) yield break;
        if (!profile.TryGet<ChromaticAberration>(out var ca)) yield break;

        float start = ca.intensity.value;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            ca.intensity.value = Mathf.Lerp(start, target, t / dur);
            yield return null;
        }
        t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            ca.intensity.value = Mathf.Lerp(target, 0.12f, t / dur); // base is 0.12f
            yield return null;
        }
        ca.intensity.value = 0.12f;
    }

    public void PulseVignette(float target, float duration)
    {
        StartCoroutine(PulseVignette_CR(target, duration));
    }

    IEnumerator PulseVignette_CR(float target, float dur)
    {
        var profile = PostProcessingSetup.RuntimeProfile;
        if (profile == null) yield break;
        if (!profile.TryGet<Vignette>(out var vignette)) yield break;

        float start = vignette.intensity.value;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            vignette.intensity.value = Mathf.Lerp(start, target, t / dur);
            yield return null;
        }
        t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            vignette.intensity.value = Mathf.Lerp(target, 0.35f, t / dur); // base is 0.35f (SPEC-120)
            yield return null;
        }
        vignette.intensity.value = 0.35f;
    }

    public void PulseExposure(float target, float duration)
    {
        StartCoroutine(PulseExposure_CR(target, duration));
    }

    IEnumerator PulseExposure_CR(float target, float dur)
    {
        var profile = PostProcessingSetup.RuntimeProfile;
        if (profile == null) yield break;
        if (!profile.TryGet<ColorAdjustments>(out var grading)) yield break;

        float start = grading.postExposure.value;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            grading.postExposure.value = Mathf.Lerp(start, target, t / dur);
            yield return null;
        }
        t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            grading.postExposure.value = Mathf.Lerp(target, -0.5f, t / dur); // base is -0.5f
            yield return null;
        }
        grading.postExposure.value = -0.5f;
    }

    public void PulseGrain(float target, float duration)
    {
        StartCoroutine(PulseGrain_CR(target, duration));
    }

    IEnumerator PulseGrain_CR(float target, float dur)
    {
        var profile = PostProcessingSetup.RuntimeProfile;
        if (profile == null) yield break;
        if (!profile.TryGet<FilmGrain>(out var grain)) yield break;

        float start = grain.intensity.value;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            grain.intensity.value = Mathf.Lerp(start, target, t / dur);
            yield return null;
        }
        t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            grain.intensity.value = Mathf.Lerp(target, 0.45f, t / dur); // base is 0.45f
            yield return null;
        }
        grain.intensity.value = 0.45f;
    }

    public void PulseSaturation(float target, float duration)
    {
        StartCoroutine(PulseSaturation_CR(target, duration));
    }

    IEnumerator PulseSaturation_CR(float target, float dur)
    {
        var profile = PostProcessingSetup.RuntimeProfile;
        if (profile == null) yield break;
        if (!profile.TryGet<ColorAdjustments>(out var grading)) yield break;

        float start = grading.saturation.value;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            grading.saturation.value = Mathf.Lerp(start, target, t / dur);
            yield return null;
        }
        t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            grading.saturation.value = Mathf.Lerp(target, -8f, t / dur); // base is -8f (SPEC-120)
            yield return null;
        }
        grading.saturation.value = -8f;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            Instance = null;
        }
    }

    // ═══════════════════════════════════════════
    // EVENTOS DE JUEGO — usan pulsos discretos
    // ═══════════════════════════════════════════

    public void PlayJump(Vector3 position, Vector3 up)
    {
        SpawnEffect(jumpEffectPrefab, position, up);
        PlayClip3D(jumpClip, position, defaultVolume * 0.7f, 1.04f, "SFX_Player");
        cameraShake?.AddShake(jumpShake);
    }

    public void PlayLanding(Vector3 position, Vector3 up, float impactSpeed)
    {
        bool hard = impactSpeed >= 13f;
        SpawnEffect(hard ? hardLandingEffectPrefab : landingEffectPrefab, position, up);
        float vol = Mathf.Lerp(0.5f, 1f, Mathf.Clamp01(impactSpeed / 12f));
        PlayClip3D(hard && hardLandingClip != null ? hardLandingClip : landingClip, position, defaultVolume * vol, hard ? 0.82f : 0.96f, "SFX_Player");
        cameraShake?.AddShake(Mathf.Clamp01(landingShake + impactSpeed * 0.015f));
        
        if (hard)
        {
            PulseCA(0.3f, 0.15f);
            PulseVignette(0.55f, 0.12f);
            PulseExposure(-0.7f, 0.1f);
        }
    }

    public void PlayFootstep(Vector3 position, Vector3 up, float speed)
    {
        if (Time.time < _nextFootstepTime)
            return;

        _nextFootstepTime = Time.time + Mathf.Lerp(0.45f, 0.25f, Mathf.InverseLerp(3f, 12f, speed));
        SpawnEffect(footstepDustPrefab, position, up);
        // Footstep audio removed per immersion feedback — only particles.
        // If re-enabled, use a subtle clip with volume ~0.12f and 0.5s min interval.
    }

    public void PlayMovementScrape(Vector3 position, Vector3 up, float intensity)
    {
        if (Time.time < _nextScrapeTime)
            return;

        _nextScrapeTime = Time.time + 1.5f;
        SpawnEffect(movementScrapePrefab, position, up);
        // Movement scrape audio disabled per immersion feedback — only particles.
        // If re-enabled, use a subtle clip with volume ~0.08f and 1.5s min interval.
    }

    public void PlayGravityShift(Vector3 position, Vector3 up)
    {
        SpawnEffect(gravityShiftEffectPrefab, position, up);
        PlayClip3D(gravityShiftClip, position, defaultVolume, 0.74f);
        cameraShake?.AddShake(gravityShake);
        PulseCA(0.5f, 0.18f);
        PulseVignette(0.6f, 0.15f);
        ApplySlowMotion(0.4f, 0.12f);
    }

    public void PlayPuzzleSolved(Vector3 position)
    {
        SpawnEffect(puzzleSolvedEffectPrefab, position, Vector3.up);
        PlayClip3D(puzzleSolvedClip, position, defaultVolume * 1.2f, 0.78f, "SFX_Player");
        cameraShake?.AddShake(puzzleSolvedShake + 0.18f);
        PulseCA(0.4f, 0.25f);
        PulseVignette(0.55f, 0.2f);
        PulseExposure(-0.2f, 0.15f);
        PulseGrain(0.6f, 0.2f);
        ApplySlowMotion(slowMotionScale, slowMotionDuration);
    }

    public void PlayRecordStart(Vector3 position, Vector3 up)
    {
        PlayClip3D(recordClip, position, defaultVolume * 0.9f, 0.86f, "SFX_Player");
        cameraShake?.AddShake(recordShake);
        PulseCA(0.42f, 0.3f);
        PulseVignette(0.6f, 0.25f);
        PulseExposure(-0.75f, 0.2f);
        PulseGrain(0.55f, 0.2f);
        PulseSaturation(-28f, 0.25f);
        ApplyRecordingTimeFeel(0.82f);
    }

    public void PlayRecordStop(Vector3 position)
    {
        PlayClip3D(recordStopClip, position, defaultVolume * 0.9f, 1.08f, "SFX_Player");
    }

    public void PlayEchoSpawn(Vector3 position)
    {
        if (Time.time < _nextEchoSpawnTime)
            return;
        _nextEchoSpawnTime = Time.time + 0.25f;

        PlayClip3D(echoSpawnClip, position, defaultVolume * 1.05f, 0.68f, "SFX_Echo");
        cameraShake?.AddShake(echoSpawnShake * 0.45f);
        PulseCA(0.35f, 0.18f);
        PulseVignette(0.55f, 0.15f);
    }

    public void PlaySoftError(Vector3 position)
    {
        PlayClip3D(softErrorClip, position, defaultVolume * 0.6f, 0.72f, "SFX_Player");
        PulseCA(0.25f, 0.12f);
        PulseVignette(0.5f, 0.1f);
    }

    public void PlayPlatePress(Vector3 position)
    {
        if (Time.time < _nextPlatePressTime)
            return;
        _nextPlatePressTime = Time.time + 0.35f;

        PlayClip3D(platePressClip, position, defaultVolume * 0.85f, 0.9f, "SFX_Foley");
        cameraShake?.AddShake(0.04f);
        PulseCA(0.15f, 0.08f);
    }

    public void PlayDoorMove(Vector3 position)
    {
        PlayClip3D(doorMoveClip, position, defaultVolume * 0.9f, 0.82f, "SFX_Foley");
        cameraShake?.AddShake(0.06f);
    }

    public void PlayPlayerDeath(Vector3 position)
    {
        SpawnEffect(deathEffectPrefab, position, Vector3.up);
        PlayClip3D(playerDeathClip, position, defaultVolume * 1.2f, 0.55f, "SFX_Player");
        cameraShake?.AddShake(deathShake);
        // Cinemachine bridge: impulso fuerte de muerte.
        if (_impulseSource != null && _cinemachineBrain != null)
            _impulseSource.GenerateImpulse(Mathf.Clamp01(deathShake) * 0.9f);
        PulseCA(0.8f, 0.4f);
        PulseVignette(0.7f, 0.35f);
        PulseExposure(-1f, 0.3f);
        ApplySlowMotion(0.2f, 0.34f);
    }

    public void PlayRespawn(Vector3 position)
    {
        SpawnEffect(respawnEffectPrefab, position, Vector3.up);
        PlayClip3D(respawnClip, position, defaultVolume, 0.92f, "SFX_Player");
        cameraShake?.AddShake(0.12f);
        PulseCA(0.4f, 0.2f);
        PulseVignette(0.55f, 0.15f);
        PulseExposure(-0.2f, 0.15f);
    }

    public void PlayEchoFade(Vector3 position)
    {
        if (Time.time < _nextEchoFadeTime)
            return;
        _nextEchoFadeTime = Time.time + 0.3f;

        PlayClip3D(echoFadeClip, position, defaultVolume * 0.72f, 0.58f, "SFX_Echo");
    }

    public void PlayMechanicTick(Vector3 position, float weight = 1f)
    {
        if (Time.time < _nextMechanicTickTime)
            return;

        _nextMechanicTickTime = Time.time + 0.2f;
        PlayClip3D(movementScrapeClip, position, defaultVolume * Mathf.Clamp(weight, 0.25f, 1.2f), 0.92f);
    }

    public void PlayCameraShake(float intensity)
    {
        cameraShake?.AddShake(intensity);
        // Cinemachine bridge: impulse (force escalada, Cinemachine re-amplifica).
        if (_impulseSource != null && _cinemachineBrain != null)
            _impulseSource.GenerateImpulse(Mathf.Clamp01(intensity) * 0.6f);
    }

    // ═══════════════════════════════════════════
    // SUBSISTEMAS INTERNOS
    // ═══════════════════════════════════════════

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

    void EnsureRuntimeFallbackAudio()
    {
        if (jumpClip == null) jumpClip = CreateToneClip("SFX_JumpAir", 0.09f, 340f, 540f, 0.26f, WaveKind.Sine);
        if (landingClip == null) landingClip = CreateNoiseClip("SFX_LandingSoft", 0.14f, 0.38f, 0.45f);
        if (hardLandingClip == null) hardLandingClip = CreateNoiseClip("SFX_LandingHard", 0.24f, 0.75f, 0.32f);
        if (footstepClip == null) footstepClip = CreateNoiseClip("SFX_Footstep", 0.055f, 0.22f, 0.55f);
        if (movementScrapeClip == null) movementScrapeClip = CreateToneClip("SFX_MechanicServo", 0.18f, 96f, 42f, 0.42f, WaveKind.Saw);
        if (gravityShiftClip == null) gravityShiftClip = CreateToneClip("SFX_GravityShift", 0.42f, 160f, 38f, 0.5f, WaveKind.Sine);
        if (puzzleSolvedClip == null) puzzleSolvedClip = CreateToneClip("SFX_PuzzleSolved", 0.5f, 360f, 720f, 0.46f, WaveKind.Sine);
        if (recordClip == null) recordClip = CreateToneClip("SFX_RecordStart", 0.2f, 520f, 260f, 0.34f, WaveKind.Triangle);
        if (recordStopClip == null) recordStopClip = CreateToneClip("SFX_RecordStop", 0.16f, 260f, 520f, 0.28f, WaveKind.Triangle);
        if (echoSpawnClip == null) echoSpawnClip = CreateToneClip("SFX_EchoSpawn", 0.42f, 620f, 210f, 0.38f, WaveKind.Sine);
        if (echoFadeClip == null) echoFadeClip = CreateToneClip("SFX_EchoFadeAway", 0.55f, 240f, 48f, 0.32f, WaveKind.Sine);
        if (softErrorClip == null) softErrorClip = CreateToneClip("SFX_SoftError", 0.16f, 180f, 90f, 0.26f, WaveKind.Square);
        if (platePressClip == null) platePressClip = CreateClickClip("SFX_PlateClick", 0.075f, 0.82f);
        if (doorMoveClip == null) doorMoveClip = CreateToneClip("SFX_DoorServo", 0.34f, 82f, 44f, 0.48f, WaveKind.Saw);
        if (playerDeathClip == null) playerDeathClip = CreateToneClip("SFX_PlayerDeath", 0.55f, 180f, 36f, 0.5f, WaveKind.Saw);
        if (respawnClip == null) respawnClip = CreateToneClip("SFX_Respawn", 0.42f, 190f, 520f, 0.4f, WaveKind.Sine);
    }

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

    void PlayClip3D(AudioClip clip, Vector3 position, float volume, float pitch = 1f, string mixerGroup = "SFX")
    {
        if (clip == null)
            return;

        GameObject audioObject = new GameObject("OneShotAudio");
        audioObject.transform.position = position;
        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume * 1.35f; // Aumentar ligeramente para presencia
        source.pitch = pitch;
        source.spatialBlend = 1f;
        source.minDistance = 3f;
        source.maxDistance = 25f;
        source.rolloffMode = AudioRolloffMode.Linear;

        var audioMgr = EchoesAudioManager.EnsureExists();
        if (audioMgr != null)
        {
            source.outputAudioMixerGroup = audioMgr.FindGroup(mixerGroup);
        }

        // Si el bitcrusher está activo, replicarlo en el OneShotAudio para game feel consistente
        if (enableBitcrusher)
        {
            var distortion = audioObject.AddComponent<AudioDistortionFilter>();
            distortion.distortionLevel = Mathf.Lerp(0.08f, 0.4f, bitcrusherDryWet);
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

    enum WaveKind { Sine, Triangle, Square, Saw }

    AudioClip CreateClickClip(string name, float lengthSeconds, float amplitude)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * lengthSeconds));
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float normalized = i / (float)sampleCount;
            float envelope = Mathf.Exp(-normalized * 34f);
            float snap = Mathf.Sin(2f * Mathf.PI * 1450f * t) * 0.55f + Mathf.Sin(2f * Mathf.PI * 310f * t) * 0.45f;
            samples[i] = snap * envelope * amplitude;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateToneClip(string name, float lengthSeconds, float startFrequency, float endFrequency, float amplitude, WaveKind wave)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * lengthSeconds));
        float[] samples = new float[sampleCount];
        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float normalized = i / (float)(sampleCount - 1);
            float freq = Mathf.Lerp(startFrequency, endFrequency, normalized);
            phase += freq / sampleRate;
            phase -= Mathf.Floor(phase);
            float attack = Mathf.Clamp01(normalized / 0.04f);
            float release = Mathf.Clamp01((1f - normalized) / 0.22f);
            samples[i] = EvaluateWave(phase, wave) * attack * release * amplitude;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateNoiseClip(string name, float lengthSeconds, float amplitude, float decay)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * lengthSeconds));
        float[] samples = new float[sampleCount];
        float last = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float normalized = i / (float)(sampleCount - 1);
            float envelope = Mathf.Pow(1f - normalized, Mathf.Max(0.1f, decay * 5f));
            last = Mathf.Lerp(last, Random.Range(-1f, 1f), 0.32f);
            samples[i] = last * envelope * amplitude;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    static float EvaluateWave(float phase, WaveKind wave)
    {
        switch (wave)
        {
            case WaveKind.Triangle:
                return 1f - 4f * Mathf.Abs(Mathf.Round(phase - 0.25f) - (phase - 0.25f));
            case WaveKind.Square:
                return phase < 0.5f ? 1f : -1f;
            case WaveKind.Saw:
                return phase * 2f - 1f;
            default:
                return Mathf.Sin(phase * Mathf.PI * 2f);
        }
    }
}
