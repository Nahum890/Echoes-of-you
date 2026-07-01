using UnityEngine;

/// <summary>
/// PlayerController partial — Animation state, animator parameter sync, and movement feedback.
/// </summary>
public partial class PlayerController
{
    void UpdateAnimator()
    {
        if (_anim == null || _anim.runtimeAnimatorController == null)
            return;

        Vector3 flatVelocity = Vector3.ProjectOnPlane(_controller.velocity, _currentUp);
        bool isRecording = _echoRecorder != null && _echoRecorder.IsRecording && !_echoRecorder.IsProjectionRecording;
        CurrentAnimationState = ResolveAnimationState(flatVelocity.magnitude, isRecording);

        float speedParam = flatVelocity.magnitude * EchoesPresentationSettings.AnimationPlaybackSpeed;
        SetAnimatorFloatIfExists(AnimatorParamSpeed, speedParam);
        if (_anim != null)
            _anim.speed = EchoesPresentationSettings.AnimationPlaybackSpeed;
        SetAnimatorFloatIfExists(AnimatorParamVerticalSpeed, VerticalSpeed);
        SetAnimatorFloatIfExists(AnimatorParamTurn, Mathf.Clamp(_turnAmount / 180f, -1f, 1f));
        
        Vector3 localVelocity = transform.InverseTransformDirection(_controller.velocity);
        SetAnimatorFloatIfExists("VelocityX", localVelocity.x);
        SetAnimatorFloatIfExists("VelocityZ", localVelocity.z);
        
        SetAnimatorBoolIfExists(AnimatorParamIsGrounded, _grounded);
        SetAnimatorBoolIfExists(AnimatorParamIsRecording, isRecording);
        SetAnimatorBoolIfExists(AnimatorLegacyGrounded, _grounded);
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
                GameFeelController.Instance?.PlayMovementScrape(transform.position, movementUp, Mathf.InverseLerp(movementScrapeSpeed, moveSpeed * sprintMultiplier, speed));
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
