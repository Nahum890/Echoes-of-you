using UnityEngine;

public class DebugPlayerState : MonoBehaviour
{
    PlayerController _pc;
    float _timer;

    void Awake()
    {
        _pc = GetComponent<PlayerController>();
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > 0.5f)
        {
            _timer = 0f;
            Debug.Log($"[DEBUG] Player pos={transform.position.y:F3} grounded={_pc.IsGrounded} vSpeed={_pc.VerticalSpeed:F3} pSpeed={_pc.PlanarSpeed:F3} groundProbeDist={_pc.groundProbeDistance:F3} stickForce={_pc.groundedStickForce:F3}");
        }
    }
}
