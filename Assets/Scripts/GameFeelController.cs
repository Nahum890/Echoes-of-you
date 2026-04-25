using UnityEngine;

public class GameFeelController : MonoBehaviour
{
    public static GameFeelController Instance { get; private set; }

    [Header("Particulas")]
    [SerializeField] ParticleSystem jumpEffectPrefab;
    [SerializeField] ParticleSystem landingEffectPrefab;
    [SerializeField] ParticleSystem gravityShiftEffectPrefab;
    [SerializeField] ParticleSystem puzzleSolvedEffectPrefab;
    [SerializeField] ParticleSystem recordEffectPrefab;
    [SerializeField] ParticleSystem echoSpawnEffectPrefab;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip landingClip;
    [SerializeField] AudioClip gravityShiftClip;
    [SerializeField] AudioClip puzzleSolvedClip;
    [SerializeField] AudioClip recordClip;
    [SerializeField] AudioClip echoSpawnClip;
    [SerializeField] AudioClip softErrorClip;
    [SerializeField] float defaultVolume = 1f;

    [Header("Camera Shake")]
    [SerializeField] CameraShake cameraShake;
    [SerializeField] ThirdPersonCamera gameplayCamera;
    [SerializeField] float jumpShake = 0.08f;
    [SerializeField] float landingShake = 0.18f;
    [SerializeField] float gravityShake = 0.28f;
    [SerializeField] float puzzleSolvedShake = 0.22f;
    [SerializeField] float recordShake = 0.06f;
    [SerializeField] float echoSpawnShake = 0.1f;

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

        if (cameraShake == null)
            cameraShake = GetComponent<CameraShake>();

        if (gameplayCamera == null)
            gameplayCamera = GetComponent<ThirdPersonCamera>();
    }

    public void PlayJump(Vector3 position, Vector3 up)
    {
        SpawnEffect(jumpEffectPrefab, position, up);
        PlayClip(jumpClip, position, defaultVolume);
        cameraShake?.AddShake(jumpShake);
    }

    public void PlayLanding(Vector3 position, Vector3 up, float impactSpeed)
    {
        SpawnEffect(landingEffectPrefab, position, up);
        PlayClip(landingClip, position, defaultVolume);
        cameraShake?.AddShake(Mathf.Clamp01(landingShake + impactSpeed * 0.015f));
    }

    public void PlayGravityShift(Vector3 position, Vector3 up)
    {
        SpawnEffect(gravityShiftEffectPrefab, position, up);
        PlayClip(gravityShiftClip, position, defaultVolume);
        cameraShake?.AddShake(gravityShake);
    }

    public void PlayPuzzleSolved(Vector3 position)
    {
        SpawnEffect(puzzleSolvedEffectPrefab, position, Vector3.up);
        PlayClip(puzzleSolvedClip, position, defaultVolume);
        cameraShake?.AddShake(puzzleSolvedShake);
        gameplayCamera?.RequestFovPulse(52f, 0.35f);
    }

    public void PlayRecordStart(Vector3 position, Vector3 up)
    {
        SpawnEffect(recordEffectPrefab, position, up);
        PlayClip(recordClip, position, defaultVolume * 0.9f);
        cameraShake?.AddShake(recordShake);
        gameplayCamera?.RequestFovPulse(54f, 0.25f);
    }

    public void PlayEchoSpawn(Vector3 position)
    {
        SpawnEffect(echoSpawnEffectPrefab, position, Vector3.up);
        PlayClip(echoSpawnClip, position, defaultVolume);
        cameraShake?.AddShake(echoSpawnShake);
        gameplayCamera?.RequestFovPulse(55f, 0.2f);
    }

    public void PlaySoftError(Vector3 position)
    {
        PlayClip(softErrorClip, position, defaultVolume * 0.85f);
    }

    void SpawnEffect(ParticleSystem prefab, Vector3 position, Vector3 up)
    {
        if (prefab == null)
            return;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, up.normalized);
        ParticleSystem instance = Instantiate(prefab, position, rotation);
        float lifetime = instance.main.duration + instance.main.startLifetime.constantMax;
        Destroy(instance.gameObject, Mathf.Max(1f, lifetime + 0.5f));
    }

    void PlayClip(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null)
            return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
            return;
        }

        AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}
