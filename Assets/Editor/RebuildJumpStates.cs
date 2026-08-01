using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class RebuildJumpStates
{
    [MenuItem("Echoes of You/Tools/Rebuild Jump States (Clean 3-state)")]
    public static void Rebuild()
    {
        string controllerPath = "Assets/Prefabs/PlayerAnimController.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            Debug.LogError("[RebuildJump] Controller not found: " + controllerPath);
            return;
        }

        string jumpGuid = "d7c1007ef0fad194b8d1d28f6b4fecc4";    // jump_NoRoot
        var jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(jumpGuid));
        if (jumpClip == null)
        {
            Debug.LogError("[RebuildJump] Missing jump clip");
            return;
        }

        var sm = controller.layers[0].stateMachine;

        // Find required states
        AnimatorState locomotionState = null;
        AnimatorState fallingState = null;
        AnimatorState deathState = null;

        foreach (var child in sm.states)
        {
            if (child.state.name == "Locomotion") locomotionState = child.state;
            else if (child.state.name == "Falling") fallingState = child.state;
            else if (child.state.name == "Death") deathState = child.state;
        }

        if (locomotionState == null || fallingState == null)
        {
            Debug.LogError("[RebuildJump] Required states not found");
            return;
        }

        // === CLEAN UP ALL JUMP-RELATED STATES ===
        var statesToRemove = new System.Collections.Generic.List<AnimatorState>();
        foreach (var child in sm.states)
        {
            if (child.state.name.StartsWith("Jump"))
                statesToRemove.Add(child.state);
        }
        foreach (var s in statesToRemove)
        {
            // Remove transitions TO this state from other states
            foreach (var child in sm.states)
            {
                for (int i = child.state.transitions.Length - 1; i >= 0; i--)
                {
                    if (child.state.transitions[i].destinationState == s)
                        Object.DestroyImmediate(child.state.transitions[i], true);
                }
            }
            // Remove transitions FROM this state
            for (int i = s.transitions.Length - 1; i >= 0; i--)
                Object.DestroyImmediate(s.transitions[i], true);
            sm.RemoveState(s);
        }

        // === CREATE NEW CLEAN JUMP STATES ===
        var jumpIdle = sm.AddState("JumpIdle");
        jumpIdle.motion = jumpClip;
        jumpIdle.speed = 0.8f;

        var jumpWalk = sm.AddState("JumpWalk");
        jumpWalk.motion = jumpClip;
        jumpWalk.speed = 1.0f;

        var jumpRun = sm.AddState("JumpRun");
        jumpRun.motion = jumpClip;
        jumpRun.speed = 1.3f;

        // Clear Locomotion transitions (will rebuild)
        ClearTransitionsFromState(locomotionState);

        // Death transition from AnyState
        if (deathState != null)
        {
            var toDeath = sm.AddAnyStateTransition(deathState);
            toDeath.hasExitTime = false;
            toDeath.duration = 0.1f;
            toDeath.AddCondition(AnimatorConditionMode.If, 0, "Death");
        }

        // Locomotion -> JumpIdle (Speed < 0.15, JumpStart)
        var toJumpIdle = locomotionState.AddTransition(jumpIdle);
        toJumpIdle.hasExitTime = false;
        toJumpIdle.duration = 0f;
        toJumpIdle.AddCondition(AnimatorConditionMode.If, 0, "JumpStart");
        toJumpIdle.AddCondition(AnimatorConditionMode.Less, 0.15f, "Speed");

        // Locomotion -> JumpWalk (0.15 <= Speed < 0.7, JumpStart)
        var toJumpWalk = locomotionState.AddTransition(jumpWalk);
        toJumpWalk.hasExitTime = false;
        toJumpWalk.duration = 0f;
        toJumpWalk.AddCondition(AnimatorConditionMode.If, 0, "JumpStart");
        toJumpWalk.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");
        toJumpWalk.AddCondition(AnimatorConditionMode.Less, 0.7f, "Speed");

        // Locomotion -> JumpRun (Speed >= 0.7, JumpStart)
        var toJumpRun = locomotionState.AddTransition(jumpRun);
        toJumpRun.hasExitTime = false;
        toJumpRun.duration = 0f;
        toJumpRun.AddCondition(AnimatorConditionMode.If, 0, "JumpStart");
        toJumpRun.AddCondition(AnimatorConditionMode.Greater, 0.7f, "Speed");

        // Locomotion -> Falling (when leaving ground WITHOUT jumping)
        // Add LAST so it has lower priority than jump transitions
        var toFallingFromLoco = locomotionState.AddTransition(fallingState);
        toFallingFromLoco.hasExitTime = false;
        toFallingFromLoco.duration = 0.1f;
        toFallingFromLoco.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");
        toFallingFromLoco.AddCondition(AnimatorConditionMode.IfNot, 0, "JumpStart"); // Don't fall if jumping

        // Jump states -> Falling (VerticalSpeed < -0.1 && !IsGrounded)
        foreach (var js in new[] { jumpIdle, jumpWalk, jumpRun })
        {
            var toFalling = js.AddTransition(fallingState);
            toFalling.hasExitTime = false;
            toFalling.duration = 0.15f;
            toFalling.AddCondition(AnimatorConditionMode.Less, -0.1f, "VerticalSpeed");
            toFalling.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");

            var toLoco = js.AddTransition(locomotionState);
            toLoco.hasExitTime = false;
            toLoco.duration = 0.12f;
            toLoco.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
        }

        // Falling -> Locomotion (landing)
        ClearTransitionsFromState(fallingState);
        var fallingToLoco = fallingState.AddTransition(locomotionState);
        fallingToLoco.hasExitTime = false;
        fallingToLoco.duration = 0.18f;
        fallingToLoco.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");

        // Death transition
        if (deathState != null)
        {
            var toDeath = sm.AddAnyStateTransition(deathState);
            toDeath.hasExitTime = false;
            toDeath.duration = 0.1f;
            toDeath.AddCondition(AnimatorConditionMode.If, 0, "Death");
        }

        // Locomotion is default
        sm.defaultState = locomotionState;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("[RebuildJump] Clean 3 jump states (Idle/Walk/Run) with Speed-based transitions created.");
    }

    static void ClearTransitionsFromState(AnimatorState state)
    {
        if (state == null) return;
        for (int i = state.transitions.Length - 1; i >= 0; i--)
            Object.DestroyImmediate(state.transitions[i], true);
    }
}