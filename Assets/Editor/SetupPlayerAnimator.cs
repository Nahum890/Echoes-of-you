#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class SetupPlayerAnimator
{
    const string ControllerPath = "Assets/Prefabs/PlayerAnimController.controller";
    const string AnimBasePath = "Assets/3D Models/Animaciones/Locomotion/";

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
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("JumpStart", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Respawn", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("State", AnimatorControllerParameterType.Int);

        AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine stateMachine = baseLayer.stateMachine;
        ClearStateMachine(stateMachine);

        AnimationClip idleClip = LoadClip("idle.fbx");
        AnimationClip walkClip = LoadClip("walking.fbx");
        AnimationClip runClip = LoadClip("running.fbx");
        AnimationClip jumpClip = LoadClip("jump.fbx");
        AnimationClip turnLeftClip = LoadClip("left turn 90.fbx");
        AnimationClip turnRightClip = LoadClip("right turn 90.fbx");
        AnimationClip fallClip = jumpClip != null ? jumpClip : idleClip;

        // Core clean states
        AnimatorState locomotion = stateMachine.AddState("Locomotion", new Vector3(0f, 0f, 0f));
        locomotion.motion = CreateLocomotionTree(controller, idleClip, walkClip, runClip);

        AnimatorState jump = stateMachine.AddState("Jump", new Vector3(260f, -80f, 0f));
        jump.motion = jumpClip;
        jump.speed = 1.1f;

        AnimatorState fall = stateMachine.AddState("Falling", new Vector3(260f, 80f, 0f));
        fall.motion = fallClip;

        AnimatorState death = stateMachine.AddState("Death", new Vector3(500f, 0f, 0f));
        death.motion = idleClip;

        stateMachine.defaultState = locomotion;

        // Transitions
        // Locomotion -> Jump (On JumpStart trigger or when losing ground) — 0.0f duration for instant jump start
        AddTransition(locomotion, jump, false, 0.0f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, "JumpStart");
        });
        AddTransition(locomotion, jump, false, 0.0f, t => {
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsGrounded);
        });

        // Jump -> Falling (When jump animation finishes or vertical speed goes negative)
        AddTransition(jump, fall, true, 0.20f, t => {
            t.exitTime = 0.85f;
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsGrounded);
        });
        AddTransition(jump, fall, false, 0.15f, t => {
            t.AddCondition(AnimatorConditionMode.Less, -0.1f, ParamVerticalSpeed);
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsGrounded);
        });

        // Jump -> Locomotion (If landing quickly before falling)
        AddTransition(jump, locomotion, false, 0.12f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, ParamIsGrounded);
        });

        // Falling -> Locomotion (Land smoothly back to walking/idle)
        AddTransition(fall, locomotion, false, 0.18f, t => {
            t.AddCondition(AnimatorConditionMode.If, 0f, ParamIsGrounded);
        });

        // Death / Respawn transitions
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
        Debug.Log("[Echoes] Cleaned up and optimized Player Animator Controller.");
    }

    static BlendTree CreateLocomotionTree(AnimatorController controller, AnimationClip idleClip, AnimationClip walkClip, AnimationClip runClip)
    {
        BlendTree blendTree = new BlendTree
        {
            name = "WalkRunBlend",
            blendType = BlendTreeType.Simple1D,
            blendParameter = ParamSpeed
        };

        // Walk is scaled to cover a wider range (1.2f to 6.5f) to prevent cutting in half
        blendTree.AddChild(idleClip, 0f);
        blendTree.AddChild(walkClip != null ? walkClip : idleClip, 1.8f);
        blendTree.AddChild(runClip != null ? runClip : walkClip, 6.8f);
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

    static AnimationClip LoadClip(string filename)
    {
        string path = AnimBasePath + filename;
        string clipHint = Path.GetFileNameWithoutExtension(filename).Replace(" ", "").ToLowerInvariant();
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        AnimationClip fallback = null;

        for (int i = 0; i < assets.Length; i++)
        {
            AnimationClip clip = assets[i] as AnimationClip;
            if (clip == null || clip.name.StartsWith("__preview__"))
                continue;

            string clipName = clip.name.Replace(" ", "").ToLowerInvariant();
            if (clipName.Contains(clipHint) || clipName.Contains("mixamo"))
                return clip;

            if (fallback == null)
                fallback = clip;
        }

        if (fallback == null)
            Debug.LogWarning("[Echoes] Clip no encontrado: " + path);

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
