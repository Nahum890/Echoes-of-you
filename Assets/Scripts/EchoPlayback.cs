using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EchoPlayback : MonoBehaviour
{
    [SerializeField] float skinWidth = 0.08f;

    CharacterController _cc;
    List<RecordFrame> _frames = new List<RecordFrame>();
    float _duration;
    float _time;
    bool _playing;

    public bool IsPlaying => _playing;
    public float LoopDuration => _duration;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _cc.skinWidth = skinWidth;
    }

    /// <summary>Inicia el bucle de reproducción con una copia de los fotogramas.</summary>
    public void BeginPlayback(IReadOnlyList<RecordFrame> frames, float duration)
    {
        _frames.Clear();
        if (frames != null)
        {
            for (int i = 0; i < frames.Count; i++)
                _frames.Add(frames[i]);
        }

        _duration = Mathf.Max(0.05f, duration);
        _time = 0f;
        _playing = _frames.Count > 0;

        if (_playing)
        {
            RecordFrame.Evaluate(_frames, 0f, out Vector3 p, out Quaternion r);
            _cc.enabled = false;
            transform.SetPositionAndRotation(p, r);
            _cc.enabled = true;
        }
    }

    public void StopPlayback()
    {
        _playing = false;
    }

    void FixedUpdate()
    {
        if (!_playing || _frames.Count == 0)
            return;

        _time += Time.fixedDeltaTime;
        if (_time > _duration)
            _time -= _duration;

        RecordFrame.Evaluate(_frames, _time, out Vector3 targetPos, out Quaternion targetRot);

        Vector3 delta = targetPos - transform.position;
        _cc.Move(delta);
        transform.rotation = targetRot;
    }
}
