using System.Reflection;
using UnityEngine;
using Unity.Cinemachine;

public class CameraProfileApplier : MonoBehaviour
{
    public static CameraProfileApplier Instance { get; private set; }

    public CameraProfile CurrentProfile { get => _currentProfile; set => _currentProfile = value; }
    CameraProfile _currentProfile;
    // Alias for backward compatibility with editor scripts
    public CameraProfile currentProfile { get => _currentProfile; set => _currentProfile = value; }
    CinemachineBrain _brain;
    CinemachineCamera _playerVCam;

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
        _brain = FindAnyObjectByType<CinemachineBrain>();
        _playerVCam = FindAnyObjectByType<CinemachineCamera>();
        if (_brain != null && _playerVCam != null)
        {
            _playerVCam.Priority = 10;
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
        _currentProfile = profile;

        if (_playerVCam == null)
            _playerVCam = FindAnyObjectByType<CinemachineCamera>();
        if (_playerVCam == null) return;

        // Apply to CinemachineFollow (v3 Transposer equivalent)
        var follow = _playerVCam.GetComponent<CinemachineFollow>();
        if (follow != null)
        {
            follow.FollowOffset = profile.followOffset;
        }

        // Apply to CinemachineRotationComposer (v3 Composer equivalent)
        var composer = _playerVCam.GetComponent<CinemachineRotationComposer>();
        if (composer != null)
        {
            composer.TargetOffset = profile.followOffset; // use followOffset as aim offset
            composer.Damping = new Vector2(profile.aimLag, profile.aimLag);
        }

        // Lens
        var lens = _playerVCam.Lens;
        lens.FieldOfView = profile.FOV;
        lens.NearClipPlane = profile.nearClip;
        lens.FarClipPlane = profile.farClip;
        lens.Dutch = profile.dutch;
        _playerVCam.Lens = lens;
    }

    public void ApplyProfile(CameraProfile profile, Transform player, Transform echo, Transform objective)
    {
        ApplyProfile(profile);
    }

    public static void Apply(LevelCameraProfiles.Profile profile)
    {
        if (Instance == null) return;
        if (Instance._playerVCam != null)
        {
            var lens = Instance._playerVCam.Lens;
            lens.FieldOfView = profile.fov;
            Instance._playerVCam.Lens = lens;
        }
    }

    void OnRecordingStarted()
    {
        if (_playerVCam != null)
        {
            var lens = _playerVCam.Lens;
            lens.FieldOfView = Mathf.Max(35f, lens.FieldOfView - 3f);
            _playerVCam.Lens = lens;
        }
    }

    void OnRecordingStopped(bool _)
    {
        if (_currentProfile != null)
        {
            ApplyProfile(_currentProfile);
        }
    }

    CinemachineCamera CreateTemporaryVCam(string name, Vector3 followOffset, float aimLag, float fov, float dutch, float blendDuration)
    {
        if (_brain == null) _brain = FindAnyObjectByType<CinemachineBrain>();

        var go = new GameObject($"TempVCam_{name}");
        var vcam = go.AddComponent<CinemachineCamera>();
        vcam.Priority = 20;

        var follow = go.AddComponent<CinemachineFollow>();
        follow.FollowOffset = followOffset;
        // Note: BindingMode is not available in v3 CinemachineFollow

        var composer = go.AddComponent<CinemachineRotationComposer>();
        composer.Damping = new Vector2(aimLag, aimLag);
        // Note: DeadZone/SoftZone/ScreenX/ScreenY/Lookahead not available in v3 CinemachineRotationComposer

        var lens = vcam.Lens;
        lens.FieldOfView = fov;
        lens.NearClipPlane = 0.3f;
        lens.FarClipPlane = 300f;
        lens.Dutch = dutch;
        vcam.Lens = lens;

        if (_brain != null)
        {
            _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blendDuration);
        }

        return vcam;
    }

    public void SwitchToMemory(Transform player, Transform echo, Transform obj)
    {
        var vcam = CreateTemporaryVCam("Memory", new Vector3(-4f, 2.1f, -5f), 0.8f, 52f, 2.5f, 1.5f);
        if (player != null)
        {
            var follow = vcam.GetComponent<CinemachineFollow>();
            // Note: Follow target assignment may differ in v3
            SetProperty(follow, "Follow", player);
        }
        Destroy(vcam.gameObject, 3f);
    }

    public void SwitchToReplay(Transform player, Transform echo, Transform obj)
    {
        var vcam = CreateTemporaryVCam("Replay", new Vector3(-3.5f, 2.5f, -5.5f), 0.3f, 52f, 0f, 1f);
        if (player != null)
        {
            var follow = vcam.GetComponent<CinemachineFollow>();
            SetProperty(follow, "Follow", player);
        }
        Destroy(vcam.gameObject, 2.5f);
    }

    public void SwitchToSuspense(Transform target, float duration)
    {
        var vcam = CreateTemporaryVCam("Suspense", new Vector3(-2f, 1.5f, -3f), 1.2f, 48f, 0f, duration);
        if (target != null)
        {
            var follow = vcam.GetComponent<CinemachineFollow>();
            SetProperty(follow, "Follow", target);
        }
        Destroy(vcam.gameObject, duration + 1f);
    }

    public void SwitchToPuzzle(Transform player, Transform echo, Transform obj)
    {
        var vcam = CreateTemporaryVCam("Puzzle", new Vector3(-5f, 4f, -7f), 0.5f, 55f, 0f, 1.5f);
        if (player != null)
        {
            var follow = vcam.GetComponent<CinemachineFollow>();
            SetProperty(follow, "Follow", player);
        }
        Destroy(vcam.gameObject, 3f);
    }

    public void SwitchToEmotional(Transform player, Transform echo, Transform obj)
    {
        var vcam = CreateTemporaryVCam("Emotional", new Vector3(-2.5f, 1.8f, -3.5f), 0.6f, 50f, 0f, 1.5f);
        if (player != null)
        {
            var follow = vcam.GetComponent<CinemachineFollow>();
            SetProperty(follow, "Follow", player);
        }
        Destroy(vcam.gameObject, 3f);
    }

    public void SwitchToAcceptance(Transform player, Transform echo, Transform obj)
    {
        var vcam = CreateTemporaryVCam("Acceptance", new Vector3(-3f, 2f, -5f), 0.4f, 52f, 0f, 1.5f);
        if (player != null)
        {
            var follow = vcam.GetComponent<CinemachineFollow>();
            SetProperty(follow, "Follow", player);
        }
        Destroy(vcam.gameObject, 3f);
    }

    static void SetProperty(object obj, string propertyName, object value)
    {
        PropertyInfo property = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null && property.CanWrite && value != null)
            property.SetValue(obj, value);
    }
}