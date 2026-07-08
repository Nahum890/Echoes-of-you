using UnityEngine;

/// <summary>
/// Allows CharacterController actors (like Player and Echoes) to push rigidbodies (kinetic blocks).
/// </summary>
public class CharacterPush : MonoBehaviour
{
    [SerializeField] float pushPower = 2.0f;
    [SerializeField] float weightLimit = 50f;

    Animator _anim;
    bool _wasPushingThisFrame;

    void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // Ensure we hit a valid, non-kinematic Rigidbody
        if (body == null || body.isKinematic)
            return;

        // Do not push objects below us (jumping/falling on top of them)
        if (hit.moveDirection.y < -0.3f)
            return;

        // Do not push objects that exceed the weight limit
        if (body.mass > weightLimit)
            return;

        // Calculate push direction (horizontal only)
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // Apply velocity based on push power and mass (heavier objects move slower)
        body.linearVelocity = pushDir * (pushPower / body.mass);

        _wasPushingThisFrame = true;
    }

    void LateUpdate()
    {
        if (_anim == null)
            _anim = GetComponentInChildren<Animator>();

        if (_anim != null && _anim.runtimeAnimatorController != null)
        {
            // Verify if there is actual input or horizontal velocity
            CharacterController cc = GetComponent<CharacterController>();
            bool isMoving = cc != null && cc.velocity.sqrMagnitude > 0.05f;
            
            bool isPushing = _wasPushingThisFrame && isMoving;
            
            // Check if parameter exists in animator before setting it to avoid warnings
            if (HasParameter(_anim, "IsPushing"))
            {
                _anim.SetBool("IsPushing", isPushing);
            }
        }
        _wasPushingThisFrame = false;
    }

    bool HasParameter(Animator anim, string paramName)
    {
        AnimatorControllerParameter[] parameters = anim.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].name == paramName)
                return true;
        }
        return false;
    }
}
