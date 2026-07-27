using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

public class CameraProfileApplier : MonoBehaviour
{
    public static CameraProfileApplier Instance { get; private set; }

    public CameraProfile currentProfile;
    private Camera mainCam;
    private CinemachineCamera vcam;
    private CinemachineBrain brain;
    private ThirdPersonCamera _thirdPersonCamera;
    private bool _wasThirdPersonCameraEnabled;

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
            brain = mainCam.GetComponent<CinemachineBrain>();
            if (!brain) brain = mainCam.gameObject.AddComponent<CinemachineBrain>();

            // CRITICAL: Disable ThirdPersonCamera to prevent jitter (both systems writing transform in LateUpdate)
            _thirdPersonCamera = mainCam.GetComponent<ThirdPersonCamera>();
            if (_thirdPersonCamera != null)
            {
                _wasThirdPersonCameraEnabled = _thirdPersonCamera.enabled;
                if (_thirdPersonCamera.enabled)
                {
                    _thirdPersonCamera.enabled = false;
                    Debug.Log("[CameraProfileApplier] ThirdPersonCamera disabled — Cinemachine is now the active camera controller.");
                }
            }
        }

        vcam = FindAnyObjectByType<CinemachineCamera>();
        if (!vcam)
        {
            var vcamObj = new GameObject("GameplayVCam");
            vcam = vcamObj.AddComponent<CinemachineCamera>();
            vcamObj.AddComponent<CinemachineFollow>();
            vcamObj.AddComponent<CinemachineRotationComposer>();
            vcam.Priority = 10;
        }
    }

    void OnDestroy()
    {
        // Restore ThirdPersonCamera if it was originally enabled
        if (_thirdPersonCamera != null && _wasThirdPersonCameraEnabled)
        {
            _thirdPersonCamera.enabled = true;
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
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        Transform echo = GameObject.Find("EchoPlayer")?.transform ?? GameObject.FindGameObjectWithTag("Echo")?.transform;
        Transform obj = GameObject.FindGameObjectWithTag("Objective")?.transform ?? GameObject.Find("LevelGoal")?.transform;
        ApplyProfile(profile, player, echo, obj);
    }

    public void ApplyProfile(CameraProfile profile, Transform player, Transform echo, Transform objective)
    {
        if (profile == null) return;
        currentProfile = profile;

        if (!vcam) InitCameraSystem();
        if (!vcam) return;

        // Body / Follow
        var follow = vcam.GetComponent<CinemachineFollow>();
        if (!follow) follow = vcam.gameObject.AddComponent<CinemachineFollow>();
        follow.FollowOffset = profile.followOffset;
        follow.TrackerSettings.BindingMode = Unity.Cinemachine.TargetTracking.BindingMode.WorldSpace;
        follow.TrackerSettings.PositionDamping = new Vector3(profile.xDamping, profile.yDamping, profile.zDamping);
        follow.TrackerSettings.RotationDamping = new Vector3(profile.pitch, profile.yaw, profile.roll);

        // Aim / Rotation Composer
        var composer = vcam.GetComponent<CinemachineRotationComposer>();
        if (!composer) composer = vcam.gameObject.AddComponent<CinemachineRotationComposer>();
        composer.TargetOffset = Vector3.up * 1.5f;
        composer.Composition.DeadZone.Size = profile.deadZone;
        composer.Composition.HardLimits.Size = profile.softZone;
        composer.Damping = new Vector2(profile.aimLag, profile.aimLag);

        // Lens
        var lens = vcam.Lens;
        lens.FieldOfView = profile.FOV;
        lens.NearClipPlane = profile.nearClip;
        lens.FarClipPlane = profile.farClip;
        lens.Dutch = profile.dutch;
        vcam.Lens = lens;

        // Priority
        vcam.Priority = profile.priority;

        // Targets: Player + Echo + Objective (Group Composer for Puzzle/Replay)
        UpdateTargets(player, echo, objective);
    }

    void UpdateTargets(Transform player, Transform echo, Transform objective)
    {
        if (!vcam) return;
        var group = vcam.GetComponent<CinemachineTargetGroup>();
        if (!group) group = vcam.gameObject.AddComponent<CinemachineTargetGroup>();

        var targetsList = new List<CinemachineTargetGroup.Target>();
        if (player != null)
            targetsList.Add(new CinemachineTargetGroup.Target { Object = player, Weight = 1f, Radius = 1f });
        if (echo != null)
            targetsList.Add(new CinemachineTargetGroup.Target { Object = echo, Weight = 1f, Radius = 1f });
        if (objective != null)
            targetsList.Add(new CinemachineTargetGroup.Target { Object = objective, Weight = 0.5f, Radius = 0.5f });

        group.Targets = targetsList;
        if (targetsList.Count > 0)
        {
            vcam.Follow = group.transform;
            vcam.LookAt = group.transform;
        }
    }

    /// <summary>Static convenience wrapper — forwards to the singleton instance.</summary>
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
        if (vcam != null)
        {
            var lens = vcam.Lens;
            lens.FieldOfView = Mathf.Max(35f, lens.FieldOfView - 3f);
            vcam.Lens = lens;
        }
        else if (mainCam != null)
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

    // Switches llamados desde gameplay
    public void SwitchToMemory(Transform player, Transform echo, Transform obj)
    {
        var profile = ScriptableObject.CreateInstance<CameraProfile>();
        profile.profileType = CameraProfileType.Memory;
        profile.fieldOfView = 48f;
        profile.followOffset = new Vector3(-4f, 2.1f, -5f);
        profile.aimLag = 0.8f;
        profile.dutch = 2.5f;
        ApplyProfile(profile, player, echo, obj);
    }

    public void SwitchToReplay(Transform player, Transform echo, Transform obj)
    {
        var profile = ScriptableObject.CreateInstance<CameraProfile>();
        profile.profileType = CameraProfileType.Replay;
        profile.fieldOfView = 50f;
        profile.followOffset = new Vector3(-3.5f, 2.5f, -5.5f);
        profile.aimLag = 0.3f;
        ApplyProfile(profile, player, echo, obj);
    }

    public void SwitchToSuspense(Transform target, float duration)
    {
        var profile = ScriptableObject.CreateInstance<CameraProfile>();
        profile.profileType = CameraProfileType.Suspense;
        profile.fieldOfView = 40f;
        profile.followOffset = new Vector3(-2f, 1.5f, -3f);
        profile.aimLag = 1.2f;
        ApplyProfile(profile, target, null, null);
    }

    public void SwitchToPuzzle(Transform player, Transform echo, Transform obj)
    {
        var profile = ScriptableObject.CreateInstance<CameraProfile>();
        profile.profileType = CameraProfileType.Puzzle;
        profile.fieldOfView = 52f;
        profile.followOffset = new Vector3(-5f, 4f, -7f);
        profile.aimLag = 0.5f;
        ApplyProfile(profile, player, echo, obj);
    }

    public void SwitchToEmotional(Transform player, Transform echo, Transform obj)
    {
        var profile = ScriptableObject.CreateInstance<CameraProfile>();
        profile.profileType = CameraProfileType.Emotional;
        profile.fieldOfView = 42f;
        profile.followOffset = new Vector3(-2.5f, 1.8f, -3.5f);
        profile.aimLag = 0.6f;
        ApplyProfile(profile, player, echo, obj);
    }

    public void SwitchToAcceptance(Transform player, Transform echo, Transform obj)
    {
        var profile = ScriptableObject.CreateInstance<CameraProfile>();
        profile.profileType = CameraProfileType.Acceptance;
        profile.fieldOfView = 45f;
        profile.followOffset = new Vector3(-3f, 2f, -5f);
        profile.aimLag = 0.4f;
        ApplyProfile(profile, player, echo, obj);
    }
}