using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera))]
public class CinemachineEventFocus : MonoBehaviour
{
    public static CinemachineEventFocus Instance { get; private set; }

    CinemachineCamera _vcam;
    Transform _lookAtDummy;
    float _releaseTime = float.MaxValue;

[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureExists()
    {
        if (Instance != null && Instance._vcam != null) return;

        // SimpleFollowCamera is canonical (Cinemachine replaced). Skip EventFocus vcam injection.
        Camera main = Camera.main;
        if (main != null && main.GetComponent<SimpleFollowCamera>() != null && main.GetComponent<SimpleFollowCamera>().enabled)
            return;

        var existing = Object.FindAnyObjectByType<CinemachineEventFocus>();
        if (existing != null && existing._vcam != null)
        {
            Instance = existing;
            return;
        }

        var prefab = Resources.Load<GameObject>("Camera/EventFocusVCam");
        if (prefab == null)
        {
            var go = new GameObject("EventFocusVCam");
            go.transform.SetParent(null);
            var vcam = go.AddComponent<CinemachineCamera>();
            vcam.Priority = new Unity.Cinemachine.PrioritySettings { Enabled = true, Value = 25 };
            vcam.Lens.FieldOfView = 52f;
            vcam.Follow = null;

            var dummy = new GameObject("EventFocusLookAt");
            dummy.transform.SetParent(go.transform);
            vcam.LookAt = dummy.transform;

            go.AddComponent<CinemachineEventFocus>();
            return;
        }

        Object.Instantiate(prefab);
    }

void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _vcam = GetComponent<CinemachineCamera>();

        if (_vcam == null)
        {
            Destroy(gameObject);
            return;
        }

        if (GetComponent<CinemachineRotationComposer>() == null)
            gameObject.AddComponent<CinemachineRotationComposer>();

        if (_vcam.LookAt == null)
        {
            var dummy = new GameObject("EventFocusLookAt");
            dummy.transform.SetParent(transform);
            _vcam.LookAt = dummy.transform;
        }

        _lookAtDummy = _vcam.LookAt as Transform;
        _vcam.Priority = new Unity.Cinemachine.PrioritySettings { Enabled = true, Value = 0 };
    }

void LateUpdate()
    {
        if (_vcam == null || !_vcam.IsValid) return;

        if (Time.unscaledTime > _releaseTime)
        {
            _vcam.Priority = new Unity.Cinemachine.PrioritySettings { Enabled = true, Value = 0 };
            return;
        }
    }

public void RequestEventFocus(Vector3 worldPoint, float weight = 0.35f, float holdSeconds = 0.45f, float pulseFov = 50f)
    {
        if (_vcam == null || _lookAtDummy == null) return;

        _lookAtDummy.position = worldPoint;
        _releaseTime = Time.unscaledTime + Mathf.Max(0.1f, holdSeconds);

        _vcam.Priority = new Unity.Cinemachine.PrioritySettings
        {
            Enabled = true,
            Value = (int)Mathf.Lerp(22f, 26f, Mathf.Clamp01(weight))
        };

        var lens = _vcam.Lens;
        lens.FieldOfView = Mathf.Max(30f, pulseFov);
        _vcam.Lens = lens;
    }
}