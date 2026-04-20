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

    // Animator references
    Animator _anim;
    Transform _visualTransform;

    public bool IsPlaying => _playing;
    public float LoopDuration => _duration;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _cc.skinWidth = skinWidth;

        _anim = GetComponentInChildren<Animator>();
        if (_anim != null) _visualTransform = _anim.transform;
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

        _time += Time.deltaTime;
        
        // Loop o fin
        if (_time >= _duration)
        {
            _time = 0f; // Bucle
            // Si quieres que muera en vez de bucle, puedes destruir aquí o desactivarlo.
            // Para el diseño de niveles 1 a 6, suele ser mejor que mueran o se queden quietos,
            // pero mantengamos el bucle clásico del juego original (se repite).
        }

        RecordFrame.Evaluate(_frames, _time, out Vector3 nextP, out Quaternion nextR);

        // CharacterController override para empujar cajas/placas pero movido cinemáticamente
        _cc.enabled = false;
        transform.SetPositionAndRotation(nextP, nextR);
        _cc.enabled = true;
        _cc.Move(Vector3.down * 0.001f); // Pequeño empuje para registrar trigger/overlap
    }

    void Update()
    {
        if (!_playing || _frames.Count == 0) return;

        // Animator integration
        if (_anim != null)
        {
            // Evaluate current and next frame briefly to find velocity
            RecordFrame.Evaluate(_frames, _time, out Vector3 currP, out _);
            RecordFrame.Evaluate(_frames, _time + Time.deltaTime, out Vector3 nextP, out _);
            
            Vector3 vel = (nextP - currP) / Time.deltaTime;
            Vector3 flatVel = new Vector3(vel.x, 0, vel.z);
            _anim.SetFloat("Speed", flatVel.magnitude);

            // Set grounded true always for now since it's an echo playing back perfectly
            _anim.SetBool("Grounded", true);

            // Detect Jump (large Y difference)
            if (vel.y > 5f && flatVel.magnitude < 10f) 
            {
                // Unreliable, but works as a visual cue
            }

            if (flatVel.sqrMagnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatVel.normalized, Vector3.up);
                _visualTransform.rotation = Quaternion.Slerp(_visualTransform.rotation, targetRot, Time.deltaTime * 15f);
            }
        }
    }
}
