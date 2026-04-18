using System;
using UnityEngine;

/// <summary>
/// Muestra de posición y rotación en un instante del tiempo durante la grabación.
/// </summary>
[Serializable]
public struct RecordFrame
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;

    public RecordFrame(float time, Vector3 position, Quaternion rotation)
    {
        this.time = time;
        this.position = position;
        this.rotation = rotation;
    }

    public static void Evaluate(System.Collections.Generic.IReadOnlyList<RecordFrame> frames, float t, out Vector3 pos, out Quaternion rot)
    {
        if (frames == null || frames.Count == 0)
        {
            pos = Vector3.zero;
            rot = Quaternion.identity;
            return;
        }

        if (t <= frames[0].time)
        {
            pos = frames[0].position;
            rot = frames[0].rotation;
            return;
        }

        for (int i = 1; i < frames.Count; i++)
        {
            if (frames[i].time >= t)
            {
                RecordFrame a = frames[i - 1];
                RecordFrame b = frames[i];
                float span = Mathf.Max(0.00001f, b.time - a.time);
                float u = Mathf.Clamp01((t - a.time) / span);
                pos = Vector3.LerpUnclamped(a.position, b.position, u);
                rot = Quaternion.SlerpUnclamped(a.rotation, b.rotation, u);
                return;
            }
        }

        RecordFrame last = frames[frames.Count - 1];
        pos = last.position;
        rot = last.rotation;
    }
}
