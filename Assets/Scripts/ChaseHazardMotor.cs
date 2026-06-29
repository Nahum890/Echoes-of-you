using UnityEngine;

/// <summary>
/// Amenaza en persecución (nivel tipo CHASE). Se activa tras resolver el puzzle.
/// </summary>
public class ChaseHazardMotor : MonoBehaviour
{
    [SerializeField] float chaseSpeed = 7.5f;
    [SerializeField] float acceleration = 12f;
    [SerializeField] float killRadius = 1.4f;
    [SerializeField] bool activeOnStart;

    Transform _player;
    float _currentSpeed;
    bool _active;

    public bool IsActive => _active;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        gameObject.SetActive(activeOnStart);
        _active = activeOnStart;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        _active = true;
    }

    void Update()
    {
        if (!_active || _player == null)
            return;

        Vector3 toPlayer = _player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.01f)
            return;

        _currentSpeed = Mathf.MoveTowards(_currentSpeed, chaseSpeed, acceleration * Time.deltaTime);
        transform.position += toPlayer.normalized * (_currentSpeed * Time.deltaTime);

        if (toPlayer.magnitude <= killRadius)
            LevelRuntimeController.Instance?.HandlePlayerDeath(transform.position, 0.6f);
    }
}
