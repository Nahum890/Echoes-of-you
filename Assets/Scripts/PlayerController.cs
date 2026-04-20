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
    public LayerMask groundMask = 1 << 6; // Layer 6 es "Ground"

    CharacterController _controller;
    Vector3 _velocity;
    bool _grounded;
    Transform _cam;
    bool _isDead = false;

    // Animator for 3D model
    Animator _anim;
    Transform _visualTransform;

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

        _anim = GetComponentInChildren<Animator>();
        if (_anim != null) _visualTransform = _anim.transform;
    }

    void Update()
    {
        if (_isDead) return;

        if (transform.position.y < -15f)
        {
            Die();
            return;
        }

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
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (_anim != null) _anim.SetTrigger("Jump");
        }

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity.y * Time.deltaTime * Vector3.up);

        // Update Animator
        if (_anim != null)
        {
            Vector3 flatVel = new Vector3(_controller.velocity.x, 0, _controller.velocity.z);
            _anim.SetFloat("Speed", flatVel.magnitude);
            _anim.SetBool("Grounded", _grounded);

            // Rotate Visual towards movement
            if (flatVel.sqrMagnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatVel.normalized, Vector3.up);
                _visualTransform.rotation = Quaternion.Slerp(_visualTransform.rotation, targetRot, Time.deltaTime * 15f);
            }
        }
    }

    public void Teleport(Vector3 worldPosition, Quaternion worldRotation)
    {
        _controller.enabled = false;
        transform.SetPositionAndRotation(worldPosition, worldRotation);
        _velocity = Vector3.zero;
        _controller.enabled = true;
    }

    void Die()
    {
        _isDead = true;
        _controller.enabled = false;
        Invoke(nameof(RestartScene), 2f);
    }

    void RestartScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void OnGUI()
    {
        if (_isDead)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 50;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.red;
            style.fontStyle = FontStyle.Bold;

            // Draw shadow
            Rect r = new Rect(0, 0, Screen.width, Screen.height);
            var shadowStyle = new GUIStyle(style);
            shadowStyle.normal.textColor = Color.black;
            
            Rect shadowRect = new Rect(2, 2, Screen.width, Screen.height);
            GUI.Label(shadowRect, "HAS CAÍDO AL VACÍO", shadowStyle);
            GUI.Label(r, "HAS CAÍDO AL VACÍO", style);

            // Add Hint
            style.fontSize = 20;
            style.normal.textColor = Color.white;
            shadowStyle.fontSize = 20;
            shadowStyle.normal.textColor = Color.black;

            Rect hintRect = new Rect(0, Screen.height * 0.5f + 40, Screen.width, 50);
            Rect hintShadowRect = new Rect(2, Screen.height * 0.5f + 42, Screen.width, 50);
            GUI.Label(hintShadowRect, "CONSEJO: MANTÉN 'R' PARA GRABAR UN ECO Y SALTÁ MÁS LEJOS", shadowStyle);
            GUI.Label(hintRect, "CONSEJO: MANTÉN 'R' PARA GRABAR UN ECO Y SALTÁ MÁS LEJOS", style);
        }
    }
}
