// AmbientEchoData.cs
using System.Reflection;
using UnityEngine;

/// <summary>
/// Disables player recording and (optionally) spawns an ambient Lyra ghost that reveals hidden MemoryPlatforms.
/// This component is activated on levels with the <c>ambientEchoData</c> Blueprint flag.
/// </summary>
public class AmbientEchoData : MonoBehaviour
{
    void Start()
    {
        var recorder = FindAnyObjectByType<EchoRecorder>();
        if (recorder != null)
        {
            // Set maxEchoes to 0 to prevent recording.
            var field = typeof(EchoRecorder).GetField("maxEchoes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
                field.SetValue(recorder, 0);
        }
        // TODO: spawn ambient Lyra ghost that walks the level.
    }
}
