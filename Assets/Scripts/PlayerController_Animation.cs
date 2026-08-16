using UnityEngine;

/// <summary>
/// PlayerController partial — Animation state, animator parameter sync, and movement feedback.
/// </summary>
public partial class PlayerController
{
    // Grounded truth used ONLY for animator bools. The authoritative ground
    // state consumed by movement/gravity/landing logic is `_grounded`, which
    // is owned exclusively by Update() (SphereCast-based). Keeping the old
    // `cc.isGrounded`+0.3s grace smoothing here, isolated in this field,
    // preserves animation flicker-avoidance without leaking into landing
    // detection — which was the micro-stutter root cause: a phantom
    // `_grounded: false->true` produced a fake landing and zeroed both
    // `_planarVelocity` *and* `desiredVelocity` for 12-28ms.
    bool _animGrounded;

    void UpdateAnimator()
    {
        if (_anim == null || _anim.runtimeAnimatorController == null)
            return;

        // Animator-only ground state. We deliberately do NOT mutate the
        // authoritative `_grounded` from here; movement must read the
        // probe-based value maintained by Update().
        if (_controller != null)
        {
            if (!_animGrounded && _controller.isGrounded)
            {
                _animGrounded = true;
                _notGroundedTimer = 0f;
            }
            else if (_animGrounded && !_controller.isGrounded)
            {
                _notGroundedTimer += Time.deltaTime;
                if (_notGroundedTimer > 0.3f)
                {
                    _animGrounded = false;
                    _notGroundedTimer = 0f;
                }
            }
            else
            {
                _notGroundedTimer = 0f;
            }
        }
        else
        {
            // No CharacterController available — fall back to probe truth.
            _animGrounded = _grounded;
        }

        Vector3 flatVelocity = Vector3.ProjectOnPlane(_controller.velocity, _currentUp);
        bool isRecording = _echoRecorder != null && _echoRecorder.IsRecording;
        CurrentAnimationState = ResolveAnimationState(flatVelocity.magnitude, isRecording);

        float currentMaxSpeed = maxSpeed * sprintMultiplier;
        float speedParam = Mathf.Clamp01(flatVelocity.magnitude / currentMaxSpeed);
        SetAnimatorFloatIfExists(AnimatorParamSpeed, speedParam);
        if (_anim != null)
            _anim.speed = EchoesPresentationSettings.AnimationPlaybackSpeed;
        SetAnimatorFloatIfExists(AnimatorParamVerticalSpeed, VerticalSpeed);
        SetAnimatorFloatIfExists(AnimatorParamTurn, Mathf.Clamp(_turnAmount / 180f, -1f, 1f));
        
        Vector3 localVelocity = transform.InverseTransformDirection(_controller.velocity);
        SetAnimatorFloatIfExists("VelocityX", localVelocity.x);
        SetAnimatorFloatIfExists("VelocityZ", localVelocity.z);
        
        SetAnimatorBoolIfExists(AnimatorParamIsGrounded, _animGrounded);
        SetAnimatorBoolIfExists(AnimatorParamIsRecording, isRecording);
        SetAnimatorBoolIfExists(AnimatorLegacyGrounded, _animGrounded);
        SetAnimatorBoolIfExists(AnimatorLegacyFalling, _isFalling);

        if (HasAnimatorParameter("State", AnimatorControllerParameterType.Int))
            _anim.SetInteger("State", (int)CurrentAnimationState);
    }

    void UpdateMovementFeedback(Vector3 movementUp, float deltaTime)
    {
        Vector3 flatVelocity = Vector3.ProjectOnPlane(_controller.velocity, movementUp);
        float speed = flatVelocity.magnitude;
        bool moving = _grounded && speed > 0.35f;

        if (moving)
        {
            _distanceSinceFootstep += speed * deltaTime;
            if (_distanceSinceFootstep >= footstepDistance)
            {
                _distanceSinceFootstep = 0f;
                GameFeelController.Instance?.PlayFootstep(transform.position, movementUp, speed);
            }

            if (speed > movementScrapeSpeed)
                GameFeelController.Instance?.PlayMovementScrape(transform.position, movementUp, Mathf.InverseLerp(movementScrapeSpeed, this.maxSpeed * sprintMultiplier, speed));
        }
        else
        {
            _distanceSinceFootstep = Mathf.Min(_distanceSinceFootstep, footstepDistance * 0.65f);
        }

        if (!_wasMoving && moving)
            TriggerAnimatorIfExists(AnimatorParamStartRun);
        else if (_wasMoving && !moving && _lastPlanarSpeed > 1.2f)
            TriggerAnimatorIfExists(AnimatorParamStopRun);

        _wasMoving = moving;
        _lastPlanarSpeed = speed;
    }

    PlayerAnimationState ResolveAnimationState(float speed, bool isRecording)
    {
        if (_isDead)
            return PlayerAnimationState.Death;
        if (isRecording)
            return PlayerAnimationState.Recording;
        if (_landingLockTimer > 0f)
            return PlayerAnimationState.Landing;
        if (_isFalling)
            return PlayerAnimationState.Falling;
        if (!_grounded || _jumpedThisFrame)
            return PlayerAnimationState.Jump;
        if (speed > 0.15f)
            return PlayerAnimationState.Run;
        return PlayerAnimationState.Idle;
    }

    void TriggerAnimatorIfExists(string parameterName)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Trigger))
            _anim.SetTrigger(parameterName);
    }

    bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (_anim == null)
            return false;

        AnimatorControllerParameter[] parameters = _anim.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == parameterType && parameters[i].name == parameterName)
                return true;
        }

        return false;
    }

    void SetAnimatorBoolIfExists(string parameterName, bool value)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Bool))
            _anim.SetBool(parameterName, value);
    }

    void SetAnimatorFloatIfExists(string parameterName, float value)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Float))
            _anim.SetFloat(parameterName, value);
    }
}
