// RecordFutureExit.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// Unlocks the LevelExit for a limited window after a successful Echo recording.
/// Used on levels with the <c>recordFuture</c> Blueprint flag.
/// </summary>
public class RecordFutureExit : MonoBehaviour
{
    [SerializeField] private LevelExit exit;
    [SerializeField] private EchoRecorder recorder;
    [SerializeField] private float window = 5f;

    void Awake()
    {
        if (exit == null)
            exit = GetComponent<LevelExit>();
        if (recorder == null)
            recorder = FindAnyObjectByType<EchoRecorder>();
    }

    void OnEnable()
    {
        if (recorder != null)
            recorder.RecordingStopped += OnRecordingStopped;
    }

    void OnDisable()
    {
        if (recorder != null)
            recorder.RecordingStopped -= OnRecordingStopped;
    }

    private void OnRecordingStopped(bool success)
    {
        if (success)
            StartCoroutine(UnlockWindow());
    }

    private IEnumerator UnlockWindow()
    {
        if (exit != null)
            exit.SetUnlocked(true);
        yield return new WaitForSeconds(window);
        bool finalState = LevelGoal.Instance != null && LevelGoal.Instance.IsReady;
        if (exit != null)
            exit.SetUnlocked(finalState);
    }
}
