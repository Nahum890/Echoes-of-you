using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Muestra de posición y rotación en un instante del tiempo durante la grabación.
/// Incluye interpolación Cubic Hermite Spline para reproducción suave a 60/144 FPS.
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

    /// <summary>
    /// Interpolación Cubic Hermite Spline (Catmull-Rom) para posición y SLERP para rotación.
    /// Requiere al menos 4 frames para spline completo; fallback a lineal en bordes.
    /// </summary>
    public static void Evaluate(IReadOnlyList<RecordFrame> frames, float t, out Vector3 pos, out Quaternion rot)
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

        int count = frames.Count;
        float lastTime = frames[count - 1].time;
        if (t >= lastTime)
        {
            pos = frames[count - 1].position;
            rot = frames[count - 1].rotation;
            return;
        }

        // Buscar el segmento [i-1, i] que contiene t
        int i = 1;
        while (i < count && frames[i].time < t)
            i++;

        // i es el índice del frame posterior, i-1 el anterior
        // Para Catmull-Rom necesitamos p0, p1, p2, p3 donde p1=frames[i-1], p2=frames[i]
        RecordFrame p1 = frames[i - 1];
        RecordFrame p2 = frames[i];

        // Índices para tangentes (con clamping en bordes)
        int i0 = Mathf.Max(0, i - 2);
        int i3 = Mathf.Min(count - 1, i + 1);

        RecordFrame p0 = frames[i0];
        RecordFrame p3 = frames[i3];

        float span = Mathf.Max(0.00001f, p2.time - p1.time);
        float u = Mathf.Clamp01((t - p1.time) / span);

        // Tangentes tipo Catmull-Rom (0.5 * (p2 - p0), 0.5 * (p3 - p1))
        Vector3 tangent1 = (p2.position - p0.position) * 0.5f;
        Vector3 tangent2 = (p3.position - p1.position) * 0.5f;

        // Cubic Hermite basis functions
        float u2 = u * u;
        float u3 = u2 * u;
        float h1 = 2f * u3 - 3f * u2 + 1f;
        float h2 = -2f * u3 + 3f * u2;
        float h3 = u3 - 2f * u2 + u;
        float h4 = u3 - u2;

        pos = p1.position * h1 + p2.position * h2 + tangent1 * h3 + tangent2 * h4;

        // Rotación: SLERP entre p1 y p2 (Cubic Hermite en quaterniones es complejo, SLERP es suficiente y estable)
        rot = Quaternion.SlerpUnclamped(p1.rotation, p2.rotation, u);
    }

    /// <summary>
    /// Versión simplificada para compatibilidad - usa interpolación lineal.
    /// </summary>
    public static void EvaluateLinear(IReadOnlyList<RecordFrame> frames, float t, out Vector3 pos, out Quaternion rot)
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
