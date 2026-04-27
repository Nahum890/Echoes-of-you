#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Editor tool: crea o reconfigura el Animator Controller del jugador
/// con todos los estados, transiciones y parámetros necesarios.
/// Menú: Echoes > Setup Player Animator
/// </summary>
public static class SetupPlayerAnimator
{
    const string ControllerPath = "Assets/Prefabs/PlayerAnimController.controller";
    const string AnimBasePath = "Assets/3D Models/Animaciones/Locomotion/";

    [MenuItem("Echoes/Setup Player Animator")]
    public static void Setup()
    {
        // Cargar o crear el controller
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            Debug.Log("SetupPlayerAnimator: creado nuevo AnimatorController");
        }

        // --- Parámetros ---
        ClearParameters(controller);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("VelocityX", AnimatorControllerParameterType.Float);
        controller.AddParameter("VelocityZ", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Falling", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("IsRecording", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsEchoPlayback", AnimatorControllerParameterType.Bool);

        // --- Layer base ---
        AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine sm = baseLayer.stateMachine;

        // Limpiar estados existentes
        foreach (var state in sm.states)
            sm.RemoveState(state.state);
        foreach (var child in sm.stateMachines)
            sm.RemoveStateMachine(child.stateMachine);

        // --- Cargar clips ---
        AnimationClip idleClip = LoadClip("idle.fbx");
        AnimationClip walkClip = LoadClip("walking.fbx");
        AnimationClip jumpClip = LoadClip("jump.fbx");
        AnimationClip runClip = LoadClip("running.fbx");

        // Usar idle como fallback para estados sin clip específico
        AnimationClip fallClip = idleClip;   // Fall usa idle (sin anim específica)
        AnimationClip recordClip = idleClip; // Record usa idle con modificación via script

        // --- Crear estados ---
        AnimatorState stateIdle = sm.AddState("Idle", new Vector3(0, 0, 0));
        stateIdle.motion = idleClip;

        AnimatorState stateWalk = sm.AddState("Walk", new Vector3(250, 0, 0));
        stateWalk.motion = walkClip;
        stateWalk.speedParameterActive = true;
        stateWalk.speedParameter = "Speed";
        // Normalizar velocidad: Speed 6 = velocidad normal de anim
        stateWalk.speed = 1f;

        AnimatorState stateJump = sm.AddState("Jump", new Vector3(125, -120, 0));
        stateJump.motion = jumpClip;

        AnimatorState stateFall = sm.AddState("Fall", new Vector3(250, -120, 0));
        stateFall.motion = fallClip;

        AnimatorState stateRecord = sm.AddState("Record", new Vector3(125, 120, 0));
        stateRecord.motion = recordClip;

        sm.defaultState = stateIdle;

        // --- Transiciones ---

        // Idle -> Walk (Speed > 0.1)
        AnimatorStateTransition idleToWalk = stateIdle.AddTransition(stateWalk);
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.15f;
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        // Walk -> Idle (Speed < 0.1)
        AnimatorStateTransition walkToIdle = stateWalk.AddTransition(stateIdle);
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.2f;
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        // Idle -> Jump (Jump trigger)
        AnimatorStateTransition idleToJump = stateIdle.AddTransition(stateJump);
        idleToJump.hasExitTime = false;
        idleToJump.duration = 0.05f;
        idleToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");

        // Walk -> Jump (Jump trigger)
        AnimatorStateTransition walkToJump = stateWalk.AddTransition(stateJump);
        walkToJump.hasExitTime = false;
        walkToJump.duration = 0.05f;
        walkToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");

        // Jump -> Fall (after exit time, not grounded)
        AnimatorStateTransition jumpToFall = stateJump.AddTransition(stateFall);
        jumpToFall.hasExitTime = true;
        jumpToFall.exitTime = 0.35f;
        jumpToFall.duration = 0.1f;
        jumpToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");

        // Jump -> Idle (grounded early, low speed)
        AnimatorStateTransition jumpToIdle = stateJump.AddTransition(stateIdle);
        jumpToIdle.hasExitTime = true;
        jumpToIdle.exitTime = 0.8f;
        jumpToIdle.duration = 0.15f;
        jumpToIdle.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        jumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        // Jump -> Walk (grounded early, moving)
        AnimatorStateTransition jumpToWalk = stateJump.AddTransition(stateWalk);
        jumpToWalk.hasExitTime = true;
        jumpToWalk.exitTime = 0.8f;
        jumpToWalk.duration = 0.15f;
        jumpToWalk.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        jumpToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        // Fall -> Idle (grounded, low speed)
        AnimatorStateTransition fallToIdle = stateFall.AddTransition(stateIdle);
        fallToIdle.hasExitTime = false;
        fallToIdle.duration = 0.15f;
        fallToIdle.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        fallToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        // Fall -> Walk (grounded, moving)
        AnimatorStateTransition fallToWalk = stateFall.AddTransition(stateWalk);
        fallToWalk.hasExitTime = false;
        fallToWalk.duration = 0.15f;
        fallToWalk.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        fallToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        // Any State -> Record (IsRecording)
        AnimatorStateTransition anyToRecord = sm.AddAnyStateTransition(stateRecord);
        anyToRecord.hasExitTime = false;
        anyToRecord.duration = 0.1f;
        anyToRecord.canTransitionToSelf = false;
        anyToRecord.AddCondition(AnimatorConditionMode.If, 0, "IsRecording");

        // Record -> Idle (stop recording)
        AnimatorStateTransition recordToIdle = stateRecord.AddTransition(stateIdle);
        recordToIdle.hasExitTime = false;
        recordToIdle.duration = 0.2f;
        recordToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRecording");

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"SetupPlayerAnimator: controller configurado con 5 estados y {CountTransitions(sm)} transiciones en '{ControllerPath}'");
    }

    static AnimationClip LoadClip(string filename)
    {
        string path = AnimBasePath + filename;
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        
        if (assets != null)
        {
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                    return clip;
            }
        }

        Debug.LogWarning($"SetupPlayerAnimator: no se encontró clip en '{path}'. Se usará null (Unity creará pose por defecto).");
        return null;
    }

    static void ClearParameters(AnimatorController controller)
    {
        // Limpiar todos los parámetros existentes
        while (controller.parameters.Length > 0)
            controller.RemoveParameter(0);
    }

    static int CountTransitions(AnimatorStateMachine sm)
    {
        int count = sm.anyStateTransitions.Length;
        foreach (var state in sm.states)
            count += state.state.transitions.Length;
        return count;
    }
}
#endif
