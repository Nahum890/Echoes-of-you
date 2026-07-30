using UnityEngine;

public class CameraProfileApplier : MonoBehaviour
{
    public static CameraProfileApplier Instance { get; private set; }

    public CameraProfile currentProfile;
    private Camera mainCam;
    private SimpleFollowCamera _simpleCam;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitCameraSystem();
    }

    private void InitCameraSystem()
    {
        mainCam = Camera.main;
        if (mainCam != null)
        {
            _simpleCam = mainCam.GetComponent<SimpleFollowCamera>();
        }
    }

    void OnEnable()
    {
        if (EchoRecorder.Instance != null)
        {
            EchoRecorder.Instance.RecordingStarted += OnRecordingStarted;
            EchoRecorder.Instance.RecordingStopped += OnRecordingStopped;
        }
    }

    void OnDisable()
    {
        if (EchoRecorder.Instance != null)
        {
            EchoRecorder.Instance.RecordingStarted -= OnRecordingStarted;
            EchoRecorder.Instance.RecordingStopped -= OnRecordingStopped;
        }
    }

    public void ApplyProfile(CameraProfile profile)
    {
        if (profile == null) return;
        currentProfile = profile;
        if (!mainCam) InitCameraSystem();
        if (!mainCam) return;

        mainCam.fieldOfView = profile.FOV;
        mainCam.nearClipPlane = profile.nearClip;
        mainCam.farClipPlane = profile.farClip;

        if (_simpleCam != null)
        {
            _simpleCam.distance = Mathf.Clamp(
                Mathf.Abs(profile.followOffset.z) > 0.1f ? Mathf.Abs(profile.followOffset.z) : 5.5f,
                _simpleCam.minDistance, _simpleCam.maxDistance);
            _simpleCam.pitch = profile.pitch;
            _simpleCam.yaw = profile.yaw;
        }
    }

    public void ApplyProfile(CameraProfile profile, Transform player, Transform echo, Transform objective)
    {
        ApplyProfile(profile);
    }

    public static void Apply(LevelCameraProfiles.Profile profile)
    {
        if (Instance == null) return;
        if (Instance.mainCam != null)
        {
            Instance.mainCam.fieldOfView = profile.fov;
        }
    }

    void OnRecordingStarted()
    {
        if (mainCam != null)
        {
            mainCam.fieldOfView = Mathf.Max(35f, mainCam.fieldOfView - 3f);
        }
    }

    void OnRecordingStopped(bool _)
    {
        if (currentProfile != null)
        {
            ApplyProfile(currentProfile);
        }
    }

    public void SwitchToMemory(Transform player, Transform echo, Transform obj)
    {
        var p = ScriptableObject.CreateInstance<CameraProfile>();
        p.profileType = CameraProfileType.Memory;
        p.followOffset = new Vector3(-4f, 2.1f, -5f);
        p.aimLag = 0.8f;
        p.dutch = 2.5f;
        ApplyProfile(p, player, echo, obj);
    }

    public void SwitchToReplay(Transform player, Transform echo, Transform obj)
    {
        var p = ScriptableObject.CreateInstance<CameraProfile>();
        p.profileType = CameraProfileType.Replay;
        p.followOffset = new Vector3(-3.5f, 2.5f, -5.5f);
        p.aimLag = 0.3f;
        ApplyProfile(p, player, echo, obj);
    }

    public void SwitchToSuspense(Transform target, float duration)
    {
        var p = ScriptableObject.CreateInstance<CameraProfile>();
        p.profileType = CameraProfileType.Suspense;
        p.followOffset = new Vector3(-2f, 1.5f, -3f);
        p.aimLag = 1.2f;
        ApplyProfile(p);
    }

    public void SwitchToPuzzle(Transform player, Transform echo, Transform obj)
    {
        var p = ScriptableObject.CreateInstance<CameraProfile>();
        p.profileType = CameraProfileType.Puzzle;
        p.followOffset = new Vector3(-5f, 4f, -7f);
        p.aimLag = 0.5f;
        ApplyProfile(p, player, echo, obj);
    }

    public void SwitchToEmotional(Transform player, Transform echo, Transform obj)
    {
        var p = ScriptableObject.CreateInstance<CameraProfile>();
        p.profileType = CameraProfileType.Emotional;
        p.followOffset = new Vector3(-2.5f, 1.8f, -3.5f);
        p.aimLag = 0.6f;
        ApplyProfile(p, player, echo, obj);
    }

    public void SwitchToAcceptance(Transform player, Transform echo, Transform obj)
    {
        var p = ScriptableObject.CreateInstance<CameraProfile>();
        p.profileType = CameraProfileType.Acceptance;
        p.followOffset = new Vector3(-3f, 2f, -5f);
        p.aimLag = 0.4f;
        ApplyProfile(p, player, echo, obj);
    }
}
