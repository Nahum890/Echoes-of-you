using Unity.Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
public class EchoCameraTargetGroupManager : MonoBehaviour
{
    [Header("Weights")]
    [SerializeField] float playerWeightWithEcho = 1.0f;
    [SerializeField] float echoWeight = 1.0f;
    [SerializeField] float goalWeightWithEcho = 0.5f;
    [SerializeField] float echoBlendSharpness = 8f;

    CinemachineTargetGroup _targetGroup;
    FixedPuzzleCameraController _fixedCamera;
    Transform _echoFocus;
    Transform _playerFocus;
    Transform _goalFocus;

    EchoRecorder _recorder;
    EchoPlayback[] _echoes;
    float _currentEchoWeight;
    float _lastEchoCount;
    bool _echoActive;

    void Awake()
    {
        _targetGroup = FindAnyObjectByType<CinemachineTargetGroup>();
        _fixedCamera = GetComponent<FixedPuzzleCameraController>();
        if (_fixedCamera != null)
        {
            _playerFocus = _fixedCamera.playerFocus;
            _goalFocus = _fixedCamera.goalFocus;
            _echoFocus = _fixedCamera.echoFocus;
        }
        if (_echoFocus == null && _targetGroup != null)
            _echoFocus = _targetGroup.transform.Find("EchoCameraFocus");

        CacheRecorder();
    }

    void Start()
    {
        CacheRecorder();
    }

    void CacheRecorder()
    {
        if (_recorder != null) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _recorder = player.GetComponent<EchoRecorder>();
            if (_recorder != null)
            {
                _recorder.EchoCreated += OnEchoCreated;
                _recorder.EchoesCleared += OnEchoesCleared;
                _recorder.RecordingStarted += OnRecordingStarted;
                _recorder.RecordingStopped += OnRecordingStopped;
            }
        }
    }

    void OnDestroy()
    {
        if (_recorder != null)
        {
            _recorder.EchoCreated -= OnEchoCreated;
            _recorder.EchoesCleared -= OnEchoesCleared;
            _recorder.RecordingStarted -= OnRecordingStarted;
            _recorder.RecordingStopped -= OnRecordingStopped;
        }
    }

    void LateUpdate()
    {
        if (_targetGroup == null || _recorder == null)
        {
            CacheRecorder();
            return;
        }

        RefreshEchoReference();
        UpdateEchoWeight();
    }

    void RefreshEchoReference()
    {
        _echoes = FindObjectsByType<EchoPlayback>();
        float aliveCount = 0f;
        EchoPlayback activeEcho = null;
        for (int i = 0; i < _echoes.Length; i++)
        {
            if (_echoes[i] != null && _echoes[i].IsPlaying)
            {
                aliveCount++;
                activeEcho = _echoes[i];
            }
        }
        _echoActive = aliveCount > 0f;
        _lastEchoCount = aliveCount;

        if (_echoActive && _echoFocus != null && activeEcho != null)
        {
            Vector3 echoPos = activeEcho.transform.position;
            echoPos.y += 0.9f;
            _echoFocus.position = echoPos;
        }
    }

    void UpdateEchoWeight()
    {
        if (_echoFocus == null || _targetGroup == null)
            return;

        float desiredEchoWeight = _echoActive ? echoWeight : 0f;
        _currentEchoWeight = Mathf.Lerp(
            _currentEchoWeight,
            desiredEchoWeight,
            DampingFactor(echoBlendSharpness, Time.unscaledDeltaTime));

        float echoFactor = Mathf.Clamp01(_currentEchoWeight / echoWeight);
        float adjustedPlayerWeight = Mathf.Lerp(
            _fixedCamera != null ? _fixedCamera.playerWeight : 1.35f,
            playerWeightWithEcho,
            echoFactor);
        float adjustedGoalWeight = Mathf.Lerp(
            _fixedCamera != null ? _fixedCamera.goalWeight : 0.52f,
            goalWeightWithEcho,
            echoFactor);

        SetMemberWeight(_echoFocus, _currentEchoWeight, 1.0f);
        if (_playerFocus != null)
            SetMemberWeight(_playerFocus, adjustedPlayerWeight, 0.6f);
        if (_goalFocus != null)
            SetMemberWeight(_goalFocus, adjustedGoalWeight, 1.4f);
    }

    void OnEchoCreated(int count)
    {
        _echoActive = true;
    }

    void OnEchoesCleared()
    {
        _echoActive = false;
        _lastEchoCount = 0f;
    }

    void OnRecordingStarted()
    {
    }

    void OnRecordingStopped(bool _)
    {
    }

    void SetMemberWeight(Transform member, float weight, float radius)
    {
        if (_targetGroup == null || member == null)
            return;

        int index = EnsureMember(member, radius);
        if (index < 0)
            return;

        CinemachineTargetGroup.Target entry = _targetGroup.Targets[index];
        entry.Object = member;
        entry.Radius = radius;
        entry.Weight = Mathf.Max(0f, weight);
        _targetGroup.Targets[index] = entry;
    }

    int EnsureMember(Transform member, float radius)
    {
        var list = _targetGroup.Targets;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Object == member)
                return i;
        }

        list.Add(new CinemachineTargetGroup.Target
        {
            Object = member,
            Weight = 0f,
            Radius = radius
        });
        return list.Count - 1;
    }

    static float DampingFactor(float sharpness, float deltaTime)
    {
        return 1f - Mathf.Exp(-Mathf.Max(0f, sharpness) * deltaTime);
    }
}