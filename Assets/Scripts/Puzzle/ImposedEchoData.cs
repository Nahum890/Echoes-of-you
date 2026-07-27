// ImposedEchoData.cs
using System.Reflection;
using UnityEngine;

/// <summary>
/// Locks the EchoRecorder (no player recording) and prepares a pre‑baked echo solution.
/// Used on levels with the <c>imposedEchoData</c> Blueprint flag.
/// </summary>
public class ImposedEchoData : MonoBehaviour
{
    void Awake()
    {
        var recorder = FindAnyObjectByType<EchoRecorder>();
        if (recorder != null)
        {
            // Prevent any recording by setting maxEchoes to 0.
            var field = typeof(EchoRecorder).GetField("maxEchoes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
                field.SetValue(recorder, 0);
        }
        // TODO: Load a pre‑baked RecordFrame[] solution asset and feed it to EchoPlayback.
    }
}
