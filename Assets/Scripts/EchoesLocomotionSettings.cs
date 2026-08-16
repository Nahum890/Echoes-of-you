using UnityEngine;

[CreateAssetMenu(fileName = "EchoesLocomotionSettings", menuName = "Echoes/Locomotion Settings")]
public class EchoesLocomotionSettings : ScriptableObject
{
    static EchoesLocomotionSettings _runtime;

    [Header("Visual")]
    public RuntimeAnimatorController animatorController;
    public Avatar humanoidAvatar;
    public GameObject characterModelPrefab;

    [Header("Sprint / Momentum (Legacy - not used in PS1 mode)")]
    public float sprintMomentumBuildRate = 0.35f;
    public float sprintMomentumDecayRate = 0.55f;
    public float sprintMomentumMaxBonus = 0.28f;
    public float minSpeedForMomentum = 4.5f;

    [Header("Slide (Legacy - not used in PS1 mode)")]
    public KeyCode slideKey = KeyCode.C;
    public float slideMinSpeed = 5.5f;
    public float slideDuration = 0.85f;
    public float slideSpeedMultiplier = 1.35f;
    public float slideFriction = 2.2f;

    [Header("Ledge Grab (Legacy - not used in PS1 mode)")]
    public float ledgeProbeForward = 0.55f;
    public float ledgeProbeUp = 1.35f;
    public float ledgeProbeDown = 1.6f;
    public float ledgeHangDuration = 1.4f;
    public float ledgeClimbImpulse = 7.5f;

    [Header("Wall Jump / Run (Legacy - not used in PS1 mode)")]
    public float wallProbeDistance = 0.72f;
    public float wallJumpImpulse = 8.5f;
    public float wallJumpAwayForce = 6f;
    public float wallRunDuration = 0.55f;
    public float wallRunGravityMultiplier = 0.35f;
    public int wallRunMaxCount = 2;

    [Header("Air Dash (Legacy - not used in PS1 mode)")]
    public KeyCode airDashKey = KeyCode.LeftAlt;
    public float airDashImpulse = 11f;
    public float airDashCooldown = 1.1f;
    public int airDashUnlockAtLevelIndex = 6;

    [Header("Aterrizaje PS1")]
    [Tooltip("0.0 = FRENO TOTAL al aterrizar (PS1 tank feel). >0 = retiene algo de momentum.")]
    public float landingVelocityRetention = 0.0f;
    public float landingAnimationLock = 0.012f;

    // Backward compatibility properties (not used in PS1 mode)
    public float moveSpeed => 4.5f; // default PS1 maxSpeed

    public static EchoesLocomotionSettings Instance
    {
        get
        {
            if (_runtime != null)
                return _runtime;

            _runtime = Resources.Load<EchoesLocomotionSettings>("EchoesLocomotionSettings");
            if (_runtime == null)
                _runtime = CreateInstance<EchoesLocomotionSettings>();

            return _runtime;
        }
    }
}