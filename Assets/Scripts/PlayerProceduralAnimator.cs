using UnityEngine;

[DisallowMultipleComponent]
public class PlayerProceduralAnimator : MonoBehaviour
{
    [Header("Rig")]
    [SerializeField] Transform visualRoot;
    [SerializeField] Transform modelRoot;

    [Header("Body Weight")]
    [SerializeField] float leanAmount = 6f;
    [SerializeField] float runBobHeight = 0.045f;
    [SerializeField] float idleBreathHeight = 0.018f;
    [SerializeField] float squashAmount = 0.12f;
    [SerializeField] float turnLeanAmount = 7f;
    [SerializeField] float smoothing = 12f;

    [Header("Anticipation")]
    [SerializeField] float jumpAnticipationSeconds = 0.12f;
    [SerializeField] float landingRecoverySeconds = 0.18f;
    [SerializeField] float recordPulseScale = 0.035f;

    PlayerController _player;
    Vector3 _baseLocalPosition;
    Quaternion _baseLocalRotation;
    Vector3 _baseLocalScale;
    float _jumpAnticipation;
    float _landingRecovery;
    bool _wasGrounded;
    bool _wasRecording;
    Vector3 _smoothPosition;
    Quaternion _smoothRotation;
    Vector3 _smoothScale;
    float _fatigue;

    void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
        ResolveRig();

        _baseLocalPosition = visualRoot.localPosition;
        _baseLocalRotation = visualRoot.localRotation;
        _baseLocalScale = visualRoot.localScale;
        _smoothPosition = _baseLocalPosition;
        _smoothRotation = _baseLocalRotation;
        _smoothScale = _baseLocalScale;
        _wasGrounded = _player == null || _player.IsGrounded;
    }

    void LateUpdate()
    {
        if (_player == null || visualRoot == null)
            return;

        bool grounded = _player.IsGrounded;
        bool recording = _player.CurrentAnimationState == PlayerController.PlayerAnimationState.Recording
            && !(_player.GetComponent<EchoRecorder>()?.IsProjectionRecording ?? false);
        float dt = Time.deltaTime;
        float speed01 = Mathf.Clamp01(_player.PlanarSpeed / Mathf.Max(0.01f, _player.moveSpeed * _player.sprintMultiplier));
        float proceduralWeight = 1f - Mathf.SmoothStep(0.15f, 1.2f, _player.PlanarSpeed);

        // Procedural fatigue builds up during sprint and decays standing still
        if (speed01 > 0.72f)
            _fatigue = Mathf.MoveTowards(_fatigue, 1.0f, dt * 0.45f);
        else
            _fatigue = Mathf.MoveTowards(_fatigue, 0.0f, dt * 0.12f);

        if (_wasGrounded && !grounded)
            _jumpAnticipation = jumpAnticipationSeconds;
        if (!_wasGrounded && grounded)
            _landingRecovery = landingRecoverySeconds * (_player.LastLandingWasHard ? 1.45f : 1f);
        if (!_wasRecording && recording)
            _jumpAnticipation = Mathf.Max(_jumpAnticipation, 0.08f);

        _jumpAnticipation = Mathf.MoveTowards(_jumpAnticipation, 0f, Time.deltaTime);
        _landingRecovery = Mathf.MoveTowards(_landingRecovery, 0f, Time.deltaTime);

        float locomotionPhase = Time.time * Mathf.Lerp(3.2f, 8.2f, speed01);
        float bob = grounded
            ? Mathf.Sin(locomotionPhase) * runBobHeight * speed01 * proceduralWeight
            : Mathf.Clamp(_player.VerticalSpeed * 0.012f, -0.08f, 0.08f);

        // Exhausted breathing: breath speed and amplitude scales up dramatically when fatigued
        float breathSpeed = Mathf.Lerp(1.8f, 4.4f, _fatigue);
        float breathHeight = Mathf.Lerp(idleBreathHeight, idleBreathHeight * 2.8f, _fatigue);
        float breath = Mathf.Sin(Time.time * breathSpeed) * breathHeight * (1f - speed01) * proceduralWeight;
        float anticipation = _jumpAnticipation / Mathf.Max(0.001f, jumpAnticipationSeconds);
        float landing = _landingRecovery / Mathf.Max(0.001f, landingRecoverySeconds);
        float recordPulse = recording ? Mathf.Sin(Time.unscaledTime * 9f) * recordPulseScale : 0f;

        Vector3 targetPosition = _baseLocalPosition + Vector3.up * (bob + breath - anticipation * 0.055f - landing * 0.035f);
        Vector3 targetScale = _baseLocalScale;
        float scalePulse = (anticipation * squashAmount + landing * squashAmount * 0.75f + recordPulse) * proceduralWeight;
        targetScale.x += scalePulse;
        targetScale.z += scalePulse;
        targetScale.y -= anticipation * squashAmount * 0.8f + landing * squashAmount * 0.45f - recordPulse * 0.45f;

        float forwardLean = grounded ? speed01 * leanAmount : Mathf.Clamp(-_player.VerticalSpeed * 0.32f, -10f, 10f);
        float turnLean = Mathf.Clamp(_player.TurnAmount / 180f, -1f, 1f) * turnLeanAmount;
        if (_player.CurrentAnimationState == PlayerController.PlayerAnimationState.Death)
        {
            forwardLean = -18f;
            turnLean = 10f;
            targetPosition += Vector3.down * 0.18f;
        }

        Quaternion targetRotation = _baseLocalRotation * Quaternion.Euler(forwardLean, 0f, -turnLean);

        float blend = 1f - Mathf.Exp(-Mathf.Max(0.01f, smoothing) * dt);
        _smoothPosition = Vector3.Lerp(_smoothPosition, targetPosition, blend);
        _smoothRotation = Quaternion.Slerp(_smoothRotation, targetRotation, blend);
        _smoothScale = Vector3.Lerp(_smoothScale, targetScale, blend);

        visualRoot.localPosition = _smoothPosition;
        visualRoot.localRotation = _smoothRotation;
        visualRoot.localScale = _smoothScale;

        _wasGrounded = grounded;
        _wasRecording = recording;
    }

    void ResolveRig()
    {
        if (visualRoot == null)
        {
            Transform found = transform.Find("PlayerVisual");
            if (found == null)
                found = transform.Find("Visual");
            visualRoot = found != null ? found : transform;
        }

        if (modelRoot == null && visualRoot.childCount > 0)
            modelRoot = visualRoot.GetChild(0);
    }
}
