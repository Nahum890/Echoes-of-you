using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.35f;
    public float rotationSmooth = 12f;

    [Header("Salto / Gravedad")]
    public float jumpHeight = 1.25f;
    public float gravity = -24f;

    [Header("Detección de suelo")]
    public Transform groundCheck;
    public float groundDistance = 0.28f;
    public LayerMask groundMask = ~0;

    CharacterController _controller;
    Vector3 _velocity;
    bool _grounded;
    Transform _cam;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (groundCheck == null)
        {
            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform, false);
            gc.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            groundCheck = gc.transform;
        }

        if (Camera.main != null)
            _cam = Camera.main.transform;
    }

    void Update()
    {
        _grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore);

        if (_grounded && _velocity.y < 0f)
            _velocity.y = -2f;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v).sqrMagnitude > 1f ? new Vector3(h, 0f, v).normalized : new Vector3(h, 0f, v);

        Vector3 moveDir = transform.forward * input.z + transform.right * input.x;
        if (_cam != null)
        {
            Vector3 f = Vector3.ProjectOnPlane(_cam.forward, Vector3.up).normalized;
            Vector3 r = Vector3.ProjectOnPlane(_cam.right, Vector3.up).normalized;
            moveDir = f * input.z + r * input.x;
        }

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);
        _controller.Move(moveDir * speed * Time.deltaTime);

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmooth * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump") && _grounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity.y * Time.deltaTime * Vector3.up);
    }

    public void Teleport(Vector3 worldPosition, Quaternion worldRotation)
    {
        _controller.enabled = false;
        transform.SetPositionAndRotation(worldPosition, worldRotation);
        _velocity = Vector3.zero;
        _controller.enabled = true;
    }
}
