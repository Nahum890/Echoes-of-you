using UnityEngine;

public static class EchoesAnimatorParams
{
    public static void SetBoolIfExists(Animator anim, string name, bool value)
    {
        if (anim == null || anim.runtimeAnimatorController == null || !HasParameter(anim, name, AnimatorControllerParameterType.Bool))
            return;

        anim.SetBool(name, value);
    }

    public static void SetFloatIfExists(Animator anim, string name, float value)
    {
        if (anim == null || anim.runtimeAnimatorController == null || !HasParameter(anim, name, AnimatorControllerParameterType.Float))
            return;

        anim.SetFloat(name, value);
    }

    public static void SetGrounded(Animator anim, bool grounded)
    {
        SetBoolIfExists(anim, "IsGrounded", grounded);
        SetBoolIfExists(anim, "Grounded", grounded);
    }

    public static void SetFalling(Animator anim, bool falling)
    {
        SetBoolIfExists(anim, "Falling", falling);
        SetBoolIfExists(anim, "IsFalling", falling);
    }

    public static void SetLocomotion(Animator anim, float speed, Vector3 localVelocity, bool grounded, bool falling = false)
    {
        if (anim == null || anim.runtimeAnimatorController == null)
            return;

        SetFloatIfExists(anim, "Speed", speed);
        SetFloatIfExists(anim, "VelocityX", localVelocity.x);
        SetFloatIfExists(anim, "VelocityZ", localVelocity.z);
        SetGrounded(anim, grounded);
        SetFalling(anim, falling);
    }

    static bool HasParameter(Animator anim, string parameterName, AnimatorControllerParameterType parameterType)
    {
        AnimatorControllerParameter[] parameters = anim.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == parameterType && parameters[i].name == parameterName)
                return true;
        }

        return false;
    }
}
