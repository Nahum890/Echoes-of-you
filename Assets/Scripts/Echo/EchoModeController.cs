using UnityEngine;

/// <summary>
/// Singleton controller that configures the echo system based on the current LevelBlueprint.
/// Handles spawning of imposed, ambient, and inversion echoes at level start.
/// </summary>
public class EchoModeController : MonoBehaviour
{
    public static EchoModeController Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Reads echo configuration from a LevelBlueprint and applies it to the EchoRecorder.
    /// Spawns imposed/ambient echoes when the blueprint requires them.
    /// </summary>
    public void Configure(LevelBlueprint bp)
    {
        if (bp == null) return;

        var rec = FindAnyObjectByType<EchoRecorder>();
        if (!rec) return;

        rec.SetMode(bp.echoMode, bp.recordFuture, bp.degradationPerReplay, bp.lockEchoSlots, bp.lockedSlotIndices,
            bp.maxRecordSeconds, bp.maxEchoes);

        if (bp.echoMode == EchoPlaybackMode.Imposed || bp.echoMode == EchoPlaybackMode.Inversion)
        {
            if (bp.imposedEchoData)
                SpawnImposedEcho(bp.imposedEchoData, bp.echoMode == EchoPlaybackMode.Inversion);
        }
        else if (bp.echoMode == EchoPlaybackMode.Ambient)
        {
            if (bp.ambientEchoData)
                SpawnAmbientEcho(bp.ambientEchoData);
        }
    }

    void SpawnImposedEcho(EchoRecordingData data, bool isInversion)
    {
        var rec = FindAnyObjectByType<EchoRecorder>();
        if (!rec || !rec.echoPrefab) return;

        var obj = Instantiate(rec.echoPrefab);
        var playback = obj.GetComponent<EchoPlayback>();
        if (playback == null) playback = obj.AddComponent<EchoPlayback>();

        playback.BeginPlayback(data.frames, data.duration, data.audioClip, EchoPlaybackMode.Imposed, 0f);

        if (isInversion)
        {
            var recorder = FindAnyObjectByType<EchoRecorder>();
            if (recorder != null)
                recorder.EnableMirrorMode(data);
        }
    }

    void SpawnAmbientEcho(EchoRecordingData data)
    {
        var rec = FindAnyObjectByType<EchoRecorder>();
        if (!rec || !rec.echoPrefab) return;

        var obj = Instantiate(rec.echoPrefab);
        var playback = obj.GetComponent<EchoPlayback>();
        if (playback == null) playback = obj.AddComponent<EchoPlayback>();

        playback.BeginPlayback(data.frames, data.duration, data.audioClip, EchoPlaybackMode.Ambient, 0f);

        // Add SphereCollider trigger (2m radius) for TemporalBridge activation
        var trigger = obj.GetComponent<SphereCollider>();
        if (trigger == null) trigger = obj.AddComponent<SphereCollider>();
        trigger.radius = 2f;
        trigger.isTrigger = true;
    }
}
