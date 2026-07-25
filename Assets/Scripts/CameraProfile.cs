using UnityEngine;

public enum CameraProfileType
{
    Learning,
    Discovery,
    Puzzle,
    Transition,
    Emotional,
    Suspense,
    Memory,
    Replay,
    Acceptance,
    LeapOfFaith,
    Inversion
}

[CreateAssetMenu(fileName = "NewCameraProfile", menuName = "Echoes of You/Camera Profile", order = 3)]
public class CameraProfile : ScriptableObject
{
    public CameraProfileType profileType;
    public int priority = 10;
    public float fieldOfView = 45f;
    public float FOV { get => fieldOfView; set => fieldOfView = value; }

    public float blendDuration = 1.5f;
    public float blendTime { get => blendDuration; set => blendDuration = value; }
    public AnimationCurve blendCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Body / Transposer
    public Vector3 followOffset = new Vector3(0f, 3f, -6f);
    public Vector3 offset { get => followOffset; set => followOffset = value; }
    public float xDamping = 1f;
    public float yDamping = 1f;
    public float zDamping = 1f;
    public float pitch = 15f;
    public float yaw = 0f;
    public float roll = 0f;

    // Aim / Composer
    public float aimLag = 0.5f;
    public float lag { get => aimLag; set => aimLag = value; }
    public Vector2 deadZone = new Vector2(0.1f, 0.1f);
    public Vector2 softZone = new Vector2(0.8f, 0.8f);

    // Lens
    public float nearClip = 0.1f;
    public float farClip = 500f;
    public float dutch = 0f;

    // Noise
    public Object noiseProfile;
    public float noiseGain = 0.5f;
}
