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
        controller.AddParameter("StartRun", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("StopRun", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("HardLanding", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("PushButton", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("RecordStart", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("RecordStop", AnimatorControllerParameterType.Trigger);
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
        AnimationClip recordClip = idleClip;

        AnimatorState idle = stateMachine.AddState("Idle", new Vector3(0f, 0f, 0f));
        idle.motion = idleClip;

        AnimatorState walkRun = stateMachine.AddState("WalkRun", new Vector3(260f, 0f, 0f));
        walkRun.motion = CreateLocomotionTree(controller, idleClip, walkClip, runClip);

        AnimatorState startRun = stateMachine.AddState("Start Run", new Vector3(250f, 130f, 0f));
        startRun.motion = runClip != null ? runClip : walkClip;
        startRun.speed = 1.25f;

        AnimatorState stopRun = stateMachine.AddState("Stop Run", new Vector3(390f, 130f, 0f));
        stopRun.motion = idleClip;

        AnimatorState jump = stateMachine.AddState("Jump Start", new Vector3(120f, -130f, 0f));
        jump.motion = jumpClip;

        AnimatorState fall = stateMachine.AddState("Falling", new Vector3(310f, -130f, 0f));
        fall.motion = fallClip;

        AnimatorState landingSoft = stateMachine.AddState("Landing Soft", new Vector3(500f, -120f, 0f));
        landingSoft.motion = idleClip;
        landingSoft.speed = 1.35f;

        AnimatorState landingHard = stateMachine.AddState("Landing Hard", new Vector3(650f, -120f, 0f));
        landingHard.motion = idleClip;
        landingHard.speed = 0.85f;

        AnimatorState turnLeft = stateMachine.AddState("Turn Left", new Vector3(-160f, 20f, 0f));
        turnLeft.motion = turnLeftClip != null ? turnLeftClip : idleClip;

        AnimatorState turnRight = stateMachine.AddState("Turn Right", new Vector3(-160f, -100f, 0f));
        turnRight.motion = turnRightClip != null ? turnRightClip : idleClip;

        AnimatorState pushButton = stateMachine.AddState("Push Button", new Vector3(120f, 260f, 0f));
        pushButton.motion = idleClip;

        AnimatorState record = stateMachine.AddState("Record Start", new Vector3(120f, 130f, 0f));
        record.motion = recordClip;

        AnimatorState recordStop = stateMachine.AddState("Record Stop", new Vector3(260f, 260f, 0f));
        recordStop.motion = idleClip;
        recordStop.speed = 1.2f;

        AnimatorState death = stateMachine.AddState("Death", new Vector3(500f, 260f, 0f));
        death.motion = idleClip;
        death.speed = 0.55f;

        AnimatorState respawn = stateMachine.AddState("Respawn", new Vector3(650f, 260f, 0f));
        respawn.motion = idleClip;
        respawn.speed = 0.8f;

        stateMachine.defaultState = idle;

        AddAnyTrigger(stateMachine, startRun, "StartRun", 0.04f);
        AddAnyTrigger(stateMachine, stopRun, "StopRun", 0.04f);
        AddAnyTrigger(stateMachine, landingHard, "HardLanding", 0.04f);
        AddAnyTrigger(stateMachine, pushButton, "PushButton", 0.04f);
        AddAnyTrigger(stateMachine, record, "RecordStart", 0.04f);
        AddAnyTrigger(stateMachine, recordStop, "RecordStop", 0.04f);
        AddAnyTrigger(stateMachine, death, "Death", 0.02f);
        AddAnyTrigger(stateMachine, respawn, "Respawn", 0.02f);

        AnimatorStateTransition anyToWalk = stateMachine.AddAnyStateTransition(walkRun);
        anyToWalk.hasExitTime = false;
        anyToWalk.duration = 0.1f;
        anyToWalk.canTransitionToSelf = true;
        anyToWalk.AddCondition(AnimatorConditionMode.Greater, 0.08f, ParamSpeed);
        anyToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsRecording);

        AddTransition(idle, walkRun, false, 0.12f, t =>
        {
            t.AddCondition(AnimatorConditionMode.Greater, 0.08f, ParamSpeed);
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsRecording);
        });

        AddTransition(walkRun, idle, false, 0.12f, t =>
        {
            t.AddCondition(AnimatorConditionMode.Less, 0.06f, ParamSpeed);
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsRecording);
        });

        AnimatorStateTransition anyToJump = stateMachine.AddAnyStateTransition(jump);
        anyToJump.hasExitTime = false;
        anyToJump.duration = 0.06f;
        anyToJump.canTransitionToSelf = false;
        anyToJump.AddCondition(AnimatorConditionMode.If, 0f, "JumpStart");
        anyToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsRecording);

        AddTransition(jump, fall, true, 0.08f, t =>
        {
            t.exitTime = 0.72f;
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsGrounded);
        });

        AddTransition(jump, landingSoft, false, 0.06f, t =>
        {
            t.AddCondition(AnimatorConditionMode.If, 0f, ParamIsGrounded);
            t.AddCondition(AnimatorConditionMode.Less, 0.1f, ParamSpeed);
        });

        AddTransition(jump, walkRun, false, 0.06f, t =>
        {
            t.AddCondition(AnimatorConditionMode.If, 0f, ParamIsGrounded);
            t.AddCondition(AnimatorConditionMode.Greater, 0.1f, ParamSpeed);
        });

        AddTransition(fall, landingSoft, false, 0.06f, t =>
        {
            t.AddCondition(AnimatorConditionMode.If, 0f, ParamIsGrounded);
            t.AddCondition(AnimatorConditionMode.Less, 0.1f, ParamSpeed);
        });

        AddTransition(fall, walkRun, false, 0.1f, t =>
        {
            t.AddCondition(AnimatorConditionMode.If, 0f, ParamIsGrounded);
            t.AddCondition(AnimatorConditionMode.Greater, 0.1f, ParamSpeed);
        });

        AddTransition(startRun, walkRun, true, 0.04f, t => t.exitTime = 0.45f);
        AddTransition(stopRun, idle, true, 0.05f, t => t.exitTime = 0.42f);
        AddTransition(landingSoft, idle, true, 0.05f, t => t.exitTime = 0.36f);
        AddTransition(landingHard, idle, true, 0.08f, t => t.exitTime = 0.58f);
        AddTransition(pushButton, idle, true, 0.06f, t => t.exitTime = 0.55f);
        AddTransition(recordStop, idle, true, 0.05f, t => t.exitTime = 0.35f);
        AddTransition(respawn, idle, true, 0.08f, t => t.exitTime = 0.75f);

        AddTransition(idle, turnLeft, false, 0.05f, t => t.AddCondition(AnimatorConditionMode.Less, -0.35f, ParamTurn));
        AddTransition(idle, turnRight, false, 0.05f, t => t.AddCondition(AnimatorConditionMode.Greater, 0.35f, ParamTurn));
        AddTransition(turnLeft, idle, true, 0.06f, t => t.exitTime = 0.65f);
        AddTransition(turnRight, idle, true, 0.06f, t => t.exitTime = 0.65f);

        AnimatorStateTransition anyToRecord = stateMachine.AddAnyStateTransition(record);
        anyToRecord.hasExitTime = false;
        anyToRecord.duration = 0.08f;
        anyToRecord.canTransitionToSelf = false;
        anyToRecord.AddCondition(AnimatorConditionMode.If, 0f, ParamIsRecording);

        AddTransition(record, jump, false, 0.08f, t =>
        {
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsRecording);
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsGrounded);
        });

        AddTransition(record, idle, false, 0.1f, t =>
        {
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsRecording);
            t.AddCondition(AnimatorConditionMode.If, 0f, ParamIsGrounded);
            t.AddCondition(AnimatorConditionMode.Less, 0.1f, ParamSpeed);
        });

        AddTransition(record, walkRun, false, 0.1f, t =>
        {
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsRecording);
            t.AddCondition(AnimatorConditionMode.If, 0f, ParamIsGrounded);
            t.AddCondition(AnimatorConditionMode.Greater, 0.1f, ParamSpeed);
        });

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Echoes] Player Animator configurado.");
    }

    static BlendTree CreateLocomotionTree(AnimatorController controller, AnimationClip idleClip, AnimationClip walkClip, AnimationClip runClip)
    {
        BlendTree blendTree = new BlendTree
        {
            name = "WalkRunBlend",
            blendType = BlendTreeType.Simple1D,
            blendParameter = ParamSpeed
        };

        blendTree.AddChild(idleClip, 0f);
        blendTree.AddChild(walkClip != null ? walkClip : idleClip, 0.1f);
        blendTree.AddChild(runClip != null ? runClip : walkClip, 5.5f);
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

    static void AddAnyTrigger(AnimatorStateMachine stateMachine, AnimatorState to, string trigger, float duration)
    {
        AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(to);
        transition.hasExitTime = false;
        transition.duration = duration;
        transition.canTransitionToSelf = false;
        transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
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
