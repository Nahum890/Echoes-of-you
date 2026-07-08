#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class SetupPlayerAnimator
{
    const string ControllerPath = "Assets/Prefabs/PlayerAnimController.controller";
    const string AnimBasePath = "Assets/3D Models/Animaciones/Universal Animation Library[Standard]/Universal Animation Library[Standard]/Unity/UAL1_Standard.fbx";

    const string ParamSpeed = "Speed";
    const string ParamIsGrounded = "IsGrounded";
    const string ParamIsRecording = "IsRecording";
    const string ParamVerticalSpeed = "VerticalSpeed";
    const string ParamTurn = "Turn";

    [MenuItem("Echoes of You/FIX - Setup Player Animator", false, 82)]
    [MenuItem("Echoes/Setup Player Animator")]
    public static void Setup()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        ClearParameters(controller);
        controller.AddParameter(ParamSpeed, AnimatorControllerParameterType.Float);
        controller.AddParameter("VelocityX", AnimatorControllerParameterType.Float);
        controller.AddParameter("VelocityZ", AnimatorControllerParameterType.Float);
        controller.AddParameter(ParamVerticalSpeed, AnimatorControllerParameterType.Float);
        controller.AddParameter(ParamTurn, AnimatorControllerParameterType.Float);
        controller.AddParameter(ParamIsGrounded, AnimatorControllerParameterType.Bool);
        controller.AddParameter(ParamIsRecording, AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsEchoPlayback", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Falling", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsPushing", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("JumpStart", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Respawn", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Slide", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("LedgeGrab", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("LedgeClimb", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("WallJump", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("AirDash", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Interact", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("State", AnimatorControllerParameterType.Int);

        AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine stateMachine = baseLayer.stateMachine;
        ClearStateMachine(stateMachine);

        // Load UAL clips
        AnimationClip idleClip = LoadClip("Armature|Idle_Loop");
        AnimationClip walkClip = LoadClip("Armature|Walk_Loop");
        AnimationClip runClip = LoadClip("Armature|Jog_Fwd_Loop");
        AnimationClip sprintClip = LoadClip("Armature|Sprint_Loop");
        AnimationClip jumpClip = LoadClip("Armature|Jump_Start");
        AnimationClip fallClip = LoadClip("Armature|Jump_Loop");
        AnimationClip landClip = LoadClip("Armature|Jump_Land");
        AnimationClip deathClip = LoadClip("Armature|Death01");
        AnimationClip pushClip = LoadClip("Armature|Push_Loop");
        AnimationClip interactClip = LoadClip("Armature|Interact");
        AnimationClip slideClip = LoadClip("Armature|Roll");
        AnimationClip ledgeGrabClip = LoadClip("Armature|Sitting_Idle_Loop");
        AnimationClip ledgeClimbClip = LoadClip("Armature|Sitting_Exit");
        AnimationClip recIdleClip = LoadClip("Armature|Idle_Torch_Loop");
        AnimationClip recWalkClip = LoadClip("Armature|Walk_Formal_Loop");

        // Core states
        AnimatorState locomotion = stateMachine.AddState("Locomotion", new Vector3(0f, 0f, 0f));
        locomotion.motion = CreateLocomotionTree(controller, "WalkRunBlend", idleClip, walkClip, runClip, sprintClip);
        locomotion.iKOnFeet = true;

        AnimatorState recLocomotion = stateMachine.AddState("RecordingLocomotion", new Vector3(0f, -100f, 0f));
        recLocomotion.motion = CreateLocomotionTree(controller, "RecordingBlend", recIdleClip, recWalkClip, runClip, sprintClip);
        recLocomotion.iKOnFeet = true;

        AnimatorState jump = stateMachine.AddState("Jump", new Vector3(260f, -120f, 0f));
        jump.motion = jumpClip;
        jump.speed = 1.1f;
        jump.iKOnFeet = false;

        AnimatorState fall = stateMachine.AddState("Falling", new Vector3(260f, 120f, 0f));
        fall.motion = fallClip;
        fall.iKOnFeet = false;

        AnimatorState land = stateMachine.AddState("Landing", new Vector3(500f, 120f, 0f));
        land.motion = landClip;
        land.iKOnFeet = true;

        AnimatorState death = stateMachine.AddState("Death", new Vector3(700f, 0f, 0f));
        death.motion = deathClip;
        death.iKOnFeet = false;

        AnimatorState slide = stateMachine.AddState("Slide", new Vector3(0f, 160f, 0f));
        slide.motion = slideClip;
        slide.iKOnFeet = false;

        AnimatorState ledgeGrab = stateMachine.AddState("LedgeGrab", new Vector3(-260f, -80f, 0f));
        ledgeGrab.motion = ledgeGrabClip;
        ledgeGrab.iKOnFeet = false;

        AnimatorState ledgeClimb = stateMachine.AddState("LedgeClimb", new Vector3(-260f, 80f, 0f));
        ledgeClimb.motion = ledgeClimbClip;
        ledgeClimb.iKOnFeet = false;

        AnimatorState wallJump = stateMachine.AddState("WallJump", new Vector3(-450f, -80f, 0f));
        wallJump.motion = jumpClip;
        wallJump.iKOnFeet = false;

        AnimatorState airDash = stateMachine.AddState("AirDash", new Vector3(-450f, 80f, 0f));
        airDash.motion = slideClip;
        airDash.iKOnFeet = false;

        AnimatorState push = stateMachine.AddState("Push", new Vector3(0f, -200f, 0f));
        push.motion = pushClip;
        push.iKOnFeet = true;

        AnimatorState interact = stateMachine.AddState("Interact", new Vector3(500f, -120f, 0f));
        interact.motion = interactClip;
        interact.iKOnFeet = true;

        stateMachine.defaultState = locomotion;

        // Transitions

        // Locomotion <-> RecordingLocomotion
        AddTransition(locomotion, recLocomotion, false, 0.25f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, ParamIsRecording);
        });
        AddTransition(recLocomotion, locomotion, false, 0.25f, t => {
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsRecording);
        });

        // Locomotion -> Jump / Falling
        AddTransition(locomotion, jump, false, 0.05f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, "JumpStart");
        });
        AddTransition(locomotion, jump, false, 0.05f, t => {
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsGrounded);
        });
        
        // RecordingLocomotion -> Jump / Falling
        AddTransition(recLocomotion, jump, false, 0.05f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, "JumpStart");
        });
        AddTransition(recLocomotion, jump, false, 0.05f, t => {
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsGrounded);
        });

        // Jump -> Falling
        AddTransition(jump, fall, true, 0.20f, t => {
            t.exitTime = 0.85f;
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsGrounded);
        });
        AddTransition(jump, fall, false, 0.15f, t => {
            t.AddCondition(AnimatorConditionMode.Less, -0.1f, ParamVerticalSpeed);
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsGrounded);
        });

        // Jump -> Landing
        AddTransition(jump, land, false, 0.10f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, ParamIsGrounded);
        });

        // Falling -> Landing
        AddTransition(fall, land, false, 0.10f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, ParamIsGrounded);
        });

        // Landing -> Locomotion
        AddTransition(land, locomotion, true, 0.15f, t => {
            t.exitTime = 0.95f;
        });

        // Locomotion <-> Push
        AddTransition(locomotion, push, false, 0.15f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, "IsPushing");
        });
        AddTransition(push, locomotion, false, 0.15f, t => {
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsPushing");
        });
        AddTransition(push, jump, false, 0.05f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, "JumpStart");
        });

        // Locomotion -> Interact
        AddTransition(locomotion, interact, false, 0.15f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, "Interact");
        });
        AddTransition(interact, locomotion, true, 0.15f, t => {
            t.exitTime = 0.95f;
        });

        // Locomotion <-> Slide
        AddTransition(locomotion, slide, false, 0.10f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, "Slide");
        });
        AddTransition(slide, locomotion, true, 0.20f, t => {
            t.exitTime = 0.95f;
        });

        // Parkour state transitions
        // Any state -> LedgeGrab
        AddAnyStateTransitionWithTrigger(stateMachine, ledgeGrab, "LedgeGrab", 0.10f);
        
        // LedgeGrab -> LedgeClimb
        AddTransition(ledgeGrab, ledgeClimb, false, 0.10f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, "LedgeClimb");
        });
        // LedgeClimb -> Locomotion
        AddTransition(ledgeClimb, locomotion, true, 0.20f, t => {
            t.exitTime = 0.95f;
        });
        // LedgeGrab -> Falling (in case we drop down)
        AddTransition(ledgeGrab, fall, false, 0.15f, t => {
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsGrounded);
        });

        // Any state -> WallJump
        AddAnyStateTransitionWithTrigger(stateMachine, wallJump, "WallJump", 0.08f);
        // WallJump -> Falling
        AddTransition(wallJump, fall, true, 0.15f, t => {
            t.exitTime = 0.85f;
        });

        // Any state -> AirDash
        AddAnyStateTransitionWithTrigger(stateMachine, airDash, "AirDash", 0.08f);
        // AirDash -> Falling
        AddTransition(airDash, fall, true, 0.15f, t => {
            t.exitTime = 0.95f;
        });

        // Death / Respawn transitions (Any State to Death)
        AnimatorStateTransition anyToDeath = stateMachine.AddAnyStateTransition(death);
        anyToDeath.hasExitTime = false;
        anyToDeath.duration = 0.1f;
        anyToDeath.canTransitionToSelf = false;
        anyToDeath.AddCondition(AnimatorConditionMode.If, 0f, "Death");

        AddTransition(death, locomotion, false, 0.2f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, "Respawn");
        });

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Echoes] Cleaned up and optimized Player Animator Controller with UAL animations.");
    }

    static BlendTree CreateLocomotionTree(AnimatorController controller, string treeName, AnimationClip idleClip, AnimationClip walkClip, AnimationClip runClip, AnimationClip sprintClip)
    {
        BlendTree blendTree = new BlendTree
        {
            name = treeName,
            blendType = BlendTreeType.Simple1D,
            blendParameter = ParamSpeed
        };

        blendTree.AddChild(idleClip, 0f);
        blendTree.AddChild(walkClip != null ? walkClip : idleClip, 2.5f);
        blendTree.AddChild(runClip != null ? runClip : walkClip, 8.0f);
        blendTree.AddChild(sprintClip != null ? sprintClip : runClip, 12.8f);
        AssetDatabase.AddObjectToAsset(blendTree, controller);
        return blendTree;
    }

    static void AddTransition(AnimatorState from, AnimatorState to, bool hasExitTime, float duration, System.Action<AnimatorStateTransition> configure)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = hasExitTime;
        transition.duration = duration;
        transition.canTransitionToSelf = false;
        configure?.Invoke(transition);
    }

    static void AddAnyStateTransitionWithTrigger(AnimatorStateMachine stateMachine, AnimatorState to, string triggerName, float duration)
    {
        AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(to);
        transition.hasExitTime = false;
        transition.duration = duration;
        transition.canTransitionToSelf = false;
        transition.AddCondition(AnimatorConditionMode.If, 0f, triggerName);
    }

    static AnimationClip LoadClip(string clipName)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AnimBasePath);
        AnimationClip fallback = null;

        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is AnimationClip clip)
            {
                if (clip.name.StartsWith("__preview__"))
                    continue;

                if (clip.name == clipName || clip.name.EndsWith(clipName))
                    return clip;

                if (fallback == null)
                    fallback = clip;
            }
        }

        if (fallback == null)
            Debug.LogWarning("[Echoes] Clip no encontrado en UAL: " + clipName);

        return fallback;
    }

    static void ClearParameters(AnimatorController controller)
    {
        while (controller.parameters.Length > 0)
            controller.RemoveParameter(0);
    }

    static void ClearStateMachine(AnimatorStateMachine stateMachine)
    {
        ChildAnimatorState[] states = stateMachine.states;
        for (int i = states.Length - 1; i >= 0; i--)
            stateMachine.RemoveState(states[i].state);

        ChildAnimatorStateMachine[] subStateMachines = stateMachine.stateMachines;
        for (int i = subStateMachines.Length - 1; i >= 0; i--)
            stateMachine.RemoveStateMachine(subStateMachines[i].stateMachine);

        AnimatorStateTransition[] anyStateTransitions = stateMachine.anyStateTransitions;
        for (int i = anyStateTransitions.Length - 1; i >= 0; i--)
            stateMachine.RemoveAnyStateTransition(anyStateTransitions[i]);
    }
}
#endif
