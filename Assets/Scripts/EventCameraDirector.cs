using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
public class EventCameraDirector : MonoBehaviour
{
    [Header("Sequence Timing")]
    [SerializeField] float maxSequenceDuration = 2f;
    [SerializeField] float buttonLookDuration = 0.5f;
    [SerializeField] float doorLookDuration = 0.8f;
    [SerializeField] float returnBlendDuration = 0.7f;

    [Header("Blend")]
    [SerializeField] CinemachineBlendDefinition.Styles blendStyle = CinemachineBlendDefinition.Styles.EaseInOut;
    [SerializeField] float blendTime = 0.5f;

    [Header("Lens")]
    [SerializeField] float eventFov = 50f;

    CinemachineCamera _playerVCam;
    CinemachineBrain _brain;
    CinemachineCamera _tempVCam;
    bool _sequenceRunning;

    void Awake()
    {
        _brain = FindAnyObjectByType<CinemachineBrain>();
        _playerVCam = GameObject.Find("PlayerVCam")?.GetComponent<CinemachineCamera>();
        if (_playerVCam == null)
            _playerVCam = FindAnyObjectByType<CinemachineCamera>();
    }

    void Start()
    {
        CacheReferences();
    }

    void CacheReferences()
    {
        if (_brain == null)
            _brain = FindAnyObjectByType<CinemachineBrain>();
        if (_playerVCam == null)
            _playerVCam = GameObject.Find("PlayerVCam")?.GetComponent<CinemachineCamera>();
    }

    public void ShowActivation(Vector3 buttonPosition, Vector3 doorPosition)
    {
        if (_sequenceRunning)
            return;
        StartCoroutine(ActivationSequence(buttonPosition, doorPosition));
    }

    public void ShowActivation(Transform button, Transform door)
    {
        if (button == null || door == null)
            return;
        ShowActivation(button.position, door.position);
    }

    IEnumerator ActivationSequence(Vector3 buttonPos, Vector3 doorPos)
    {
        _sequenceRunning = true;
        CacheReferences();

        CinemachineBlendDefinition storedBlend = default;
        if (_brain != null)
        {
            storedBlend = _brain.DefaultBlend;
            _brain.DefaultBlend = new CinemachineBlendDefinition(blendStyle, blendTime);
        }

        GameObject tempObj = new GameObject("EventCamera_Temp");
        tempObj.transform.position = buttonPos + new Vector3(-4f, 3f, -5f);
        _tempVCam = tempObj.AddComponent<CinemachineCamera>();
        _tempVCam.Priority = new PrioritySettings { Value = 30 };
        _tempVCam.LookAt = CreateLookAtTarget(buttonPos);
        var lens = _tempVCam.Lens;
        lens.FieldOfView = eventFov;
        _tempVCam.Lens = lens;

        yield return new WaitForSeconds(buttonLookDuration);

        _tempVCam.LookAt = CreateLookAtTarget(doorPos);
        Vector3 doorCamPos = doorPos + new Vector3(-4f, 3f, -5f);
        _tempVCam.transform.position = doorCamPos;

        yield return new WaitForSeconds(doorLookDuration);

        float remaining = Mathf.Max(0.1f, maxSequenceDuration - buttonLookDuration - doorLookDuration);
        if (_playerVCam != null)
            _playerVCam.Priority = new PrioritySettings { Value = 30 };
        if (_tempVCam != null)
            _tempVCam.Priority = new PrioritySettings { Value = 0 };

        yield return new WaitForSeconds(returnBlendDuration);

        if (_playerVCam != null)
            _playerVCam.Priority = new PrioritySettings { Value = 20 };

        yield return new WaitForSeconds(Mathf.Max(0f, remaining - returnBlendDuration));

        Cleanup(tempObj);
        if (_brain != null)
            _brain.DefaultBlend = storedBlend;
        _sequenceRunning = false;
    }

    Transform CreateLookAtTarget(Vector3 position)
    {
        GameObject targetObj = new GameObject("EventLookAtTarget");
        targetObj.transform.position = position;
        targetObj.transform.SetParent(_tempVCam != null ? _tempVCam.transform : transform, false);
        return targetObj.transform;
    }

    void Cleanup(GameObject tempObj)
    {
        if (_tempVCam != null)
        {
            Destroy(_tempVCam.gameObject);
            _tempVCam = null;
        }
        if (_playerVCam != null)
            _playerVCam.Priority = new PrioritySettings { Value = 20 };
    }

    void OnDestroy()
    {
        if (_tempVCam != null)
            Destroy(_tempVCam.gameObject);
    }
}