using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Unity.Cinemachine;
using System.Collections.Generic;

public static class EchoesLevelShell
{
    private const string GameHUDUxmlPath = "Assets/UI/GameHUDUI.uxml";
    private const string PauseMenuUxmlPath = "Assets/UI/PauseMenuUI.uxml";
    private const string EchoesThemeUssPath = "Assets/UI/EchoesTheme.uss";
    private const string EchoPrefabPath = "Assets/Prefabs/EchoPrefab.prefab";
    private const string AnimatorControllerPath = "Assets/Prefabs/PlayerAnimController.controller";

    public static Scene CreateNewScene(string name, out Transform envRoot, out Transform mechRoot, out Transform uiRoot)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = name;

        envRoot = CreateRoot("--- ENVIRONMENT ---");
        mechRoot = CreateRoot("--- MECHANICS ---");
        uiRoot = CreateRoot("--- UI ---");

        return scene;
    }

    public static void SetupAtmosphere(LevelBlueprint blueprint)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;

        // Usa los valores reales del blueprint. Sin esto, los 15 niveles
        // comparten la misma atmósfera sin importar qué se les configure.
        RenderSettings.fogColor = blueprint.fogColor;
        RenderSettings.fogDensity = Mathf.Clamp(blueprint.fogDensity, 0.012f, 0.04f);

        // Ambient plano, no Trilight — PS1 no suaviza el contraste entre
        // superficies con tres colores direccionales. Un solo tono de piso,
        // bajo y uniforme, deja que las luces puntuales hagan el contraste.
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = blueprint.ambientColor;
        RenderSettings.ambientIntensity = 0.25f; // Reducido para mayor contraste y claroscuro

        RenderSettings.skybox = null;
        RenderSettings.reflectionIntensity = 0f; // Sin reflejos de entorno. Nunca.

        GameObject atmosphereObj = new GameObject("AtmosphereController");
        AtmosphereController atmo = atmosphereObj.AddComponent<AtmosphereController>();
        SetSerializedValue(atmo, "enableGroundFog", true);
        SetSerializedValue(atmo, "maxDustMotes", 18); // menos partículas, más sucias — escuela, no espacio liminal abstracto
        SetSerializedValue(atmo, "spawnVolume", new Vector3(30f, 8f, 30f));
    }

    public static void SpawnLevelLightingSettings(Transform parent, LevelBlueprint blueprint)
    {
        GameObject root = new GameObject("LevelLighting");
        root.transform.SetParent(parent, false);
        LevelLightingSettings settings = root.AddComponent<LevelLightingSettings>();
        settings.fogColor = blueprint.fogColor;
        settings.fogDensity = blueprint.fogDensity;
        settings.disableRuntimeFillLights = false;
    }

    public static void SpawnDirectionalLight(LevelBlueprint blueprint)
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light lightRef = lightObject.AddComponent<Light>();
        lightRef.type = LightType.Directional;
        lightRef.color = blueprint.directionalLightColor;
        lightRef.intensity = blueprint.directionalLightIntensity;
        lightRef.shadows = LightShadows.Soft; // Sombras suaves
        lightRef.shadowStrength = 0.85f;
        lightRef.lightmapBakeType = LightmapBakeType.Mixed; // Modo mixto para personajes dinámicos y geometría estática
        lightObject.transform.rotation = Quaternion.Euler(blueprint.directionalLightRotation);
    }

    public static void SpawnAmbientLights(Transform parent, Vector3 center, float width, float depth)
    {
        // Se desactivan las luces redundantes dinámicas que lavaban el contraste de los niveles.
        // La iluminación ahora la definen las fuentes locales de los módulos y el ambiente tenue.
    }

    public static Light SpawnPointLight(string name, Vector3 position, Color color, float intensity, float range, Transform parent, LightmapBakeType bakeType = LightmapBakeType.Realtime, LightShadows shadows = LightShadows.None)
    {
        GameObject lightObject = new GameObject(name);
        if (parent != null)
            lightObject.transform.SetParent(parent, false);
        lightObject.transform.position = position;

        Light lightRef = lightObject.AddComponent<Light>();
        lightRef.type = LightType.Point;
        lightRef.color = color;
        lightRef.intensity = intensity;
        lightRef.range = range;
        lightRef.shadows = shadows;
        lightRef.lightmapBakeType = bakeType;
        return lightRef;
    }

    public static void SpawnUI(Transform parent)
    {
        // HUD
        GameObject hudObj = new GameObject("GameHUD");
        hudObj.transform.SetParent(parent, false);
        UIDocument hudDoc = hudObj.AddComponent<UIDocument>();
        hudDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GameHUDUxmlPath);
        hudDoc.panelSettings = GetOrCreatePanelSettings();
        hudDoc.sortingOrder = 0;
        hudObj.AddComponent<GameHUD>();

        // Pause Menu
        GameObject pauseObj = new GameObject("PauseMenu");
        pauseObj.transform.SetParent(parent, false);
        UIDocument pauseDoc = pauseObj.AddComponent<UIDocument>();
        pauseDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PauseMenuUxmlPath);
        pauseDoc.panelSettings = GetOrCreatePanelSettings();
        pauseDoc.sortingOrder = 10;
        pauseObj.AddComponent<PauseMenu>();

        // Event System
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.transform.SetParent(parent, false);
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    public static GameObject SpawnPlayer(Vector3 position, LevelBlueprint blueprint)
    {
        var playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
        if (playerPrefab == null)
        {
            playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab")
                        ?? AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Player.prefab");
        }

        Vector3 validPos = FindValidSpawnPosition(position);
        GameObject player = null;

        if (playerPrefab != null)
        {
            player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            player.transform.SetPositionAndRotation(validPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("[EchoesLevelShell] Player prefab no encontrado, generando jugador proceduralmente");
            player = BuildProceduralPlayer(validPos, blueprint);
        }

        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = true;

            var pc = player.GetComponent<PlayerController>();
            if (pc) pc.ForceUnlockAndReset();
        }

        return player;
    }

    public static Vector3 FindValidSpawnPosition(Vector3 desiredPos)
    {
        // 1. Raycast desde arriba para encontrar suelo real
        if (Physics.Raycast(desiredPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (!Physics.CheckSphere(hit.point + Vector3.up * 1f, 0.6f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return hit.point + Vector3.up * 1.5f; // Spawn 1.5m sobre suelo
            }
        }
        
        // 2. Buscar en espiral alrededor
        for (float r = 1f; r < 15f; r += 1f)
        {
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * 2f / 16f;
                Vector3 testPos = desiredPos + new Vector3(Mathf.Cos(i * Mathf.PI / 8f), 0, Mathf.Sin(i * Mathf.PI / 8f)) * r;
                if (Physics.Raycast(testPos + Vector3.up * 10f, Vector3.down, out RaycastHit spiralHit, 20f))
                {
                    if (!Physics.CheckSphere(spiralHit.point + Vector3.up * 1f, 0.6f))
                    {
                        return spiralHit.point + Vector3.up * 1.5f;
                    }
                }
            }
        }
        
        // 3. Fallback: LevelExit
        var exit = Object.FindAnyObjectByType<LevelExit>();
        if (exit != null) return exit.transform.position + Vector3.up * 2f;
        
        return Vector3.up * 10f;
    }

    private static GameObject BuildProceduralPlayer(Vector3 position, LevelBlueprint blueprint)
    {
        Transform playerRoot = CreateRoot("--- PLAYER ---");
        GameObject player = new GameObject("Player");
        player.transform.SetParent(playerRoot, false);
        player.transform.position = position;
        player.tag = "Player";

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2.2f;
        controller.radius = 0.36f;
        controller.center = new Vector3(0f, 1.1f, 0f);
        controller.skinWidth = 0.08f;

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        PlayerController playerController = player.AddComponent<PlayerController>();
        playerController.groundMask = (1 << 6);

        player.AddComponent<PlayerCharacterVisualSetup>();
        player.AddComponent<PlayerAdvancedLocomotion>();
        player.AddComponent<CharacterPush>();

        EchoRecorder recorder = player.AddComponent<EchoRecorder>();
        SetSerializedValue(recorder, "echoPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(EchoPrefabPath));
        SetSerializedValue(recorder, "maxEchoes", blueprint != null && blueprint.echoEnabled ? blueprint.maxEchoes : 0);
        SetSerializedValue(recorder, "maxRecordSeconds", blueprint != null ? blueprint.maxRecordSeconds : 10f);

        player.AddComponent<PlayerLocomotionAnimator>();
        player.AddComponent<PlayerAnimationRuntimeBootstrap>();

        CreateCharacterVisual(player.transform);

        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform, false);
        groundCheck.transform.localPosition = new Vector3(0f, -0.96f, 0f);

        GameObject focus = new GameObject("CameraFocus");
        focus.transform.SetParent(player.transform, false);
        focus.transform.localPosition = new Vector3(0f, 1.75f, 0.18f);

        SpawnPointLight("PlayerRimLight", position + new Vector3(1.2f, 2.4f, -1.8f), new Color(0.72f, 0.88f, 1f, 1f), 0.9f, 5f, player.transform);

        return player;
    }

    public static void SpawnGameplayCamera(Transform player, Vector3 offset, Transform goalFocus)
    {
        Transform cameraRoot = CreateRoot("--- CAMERA ---");

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(cameraRoot, false);
        cameraObject.transform.position = player.position + offset;
        Camera cameraRef = cameraObject.AddComponent<Camera>();
        cameraRef.clearFlags = CameraClearFlags.SolidColor;
        cameraRef.backgroundColor = new Color(0.02f, 0.03f, 0.05f, 1f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CameraShake>();
        cameraObject.AddComponent<AudioSource>();
        var gfc = cameraObject.AddComponent<GameFeelController>();

        var loop1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Efectos de sonido/144046__gchase__room_tone_ambience_medium_control_low_hum.wav");
        var loop2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Efectos de sonido/607238__szegvari__electric-dream-synth-drone-electric-cinematic.wav");
        var loop3 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Efectos de sonido/Ventilation.wav");
        var chime = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Efectos de sonido/freesound_community-clock-chime-88027.mp3");

        SetSerializedValue(gfc, "ambientLoopClip", loop1);
        SetSerializedValue(gfc, "industrialDroneClip", loop2);
        SetSerializedValue(gfc, "ventilationHumClip", loop3);
        SetSerializedValue(gfc, "clockChimeClip", chime);
        cameraObject.AddComponent<CinematicRecordingOverlay>();
        CinematicCameraDynamics cameraDynamics = cameraObject.AddComponent<CinematicCameraDynamics>();
        FixedPuzzleCameraController fixedCamera = cameraObject.AddComponent<FixedPuzzleCameraController>();
        cameraObject.AddComponent<EchoCameraTargetGroupManager>();
        cameraObject.AddComponent<EventCameraDirector>();

        CinemachineBrain brain = cameraObject.AddComponent<CinemachineBrain>();
        brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.35f);

        Transform playerFocus = player.Find("CameraFocus");

        GameObject targetGroupObject = new GameObject("GameplayCameraTargets");
        targetGroupObject.transform.SetParent(cameraRoot, false);
        CinemachineTargetGroup targetGroup = targetGroupObject.AddComponent<CinemachineTargetGroup>();
        targetGroup.Targets = new List<CinemachineTargetGroup.Target>
        {
            new CinemachineTargetGroup.Target
            {
                Object = playerFocus != null ? playerFocus : player,
                Weight = 1.35f,
                Radius = 0.6f
            },
            new CinemachineTargetGroup.Target
            {
                Object = goalFocus != null ? goalFocus : player,
                Weight = goalFocus != null ? 0.52f : 0f,
                Radius = 1.4f
            }
        };

        GameObject eventFocus = new GameObject("CameraEventFocus");
        eventFocus.transform.SetParent(targetGroupObject.transform, false);
        eventFocus.transform.position = goalFocus != null ? goalFocus.position : player.position;

        GameObject vcamObj = new GameObject("PlayerVCam");
        vcamObj.transform.SetParent(cameraRoot, false);
        CinemachineCamera vcam = vcamObj.AddComponent<CinemachineCamera>();
        vcam.Priority = new PrioritySettings { Value = 20 };
        vcam.Follow = player;
        vcam.LookAt = targetGroup.transform;
        var lens = vcam.Lens;
        lens.FieldOfView = 52f;
        vcam.Lens = lens;

        CinemachineFollow transposer = vcamObj.AddComponent<CinemachineFollow>();
        transposer.FollowOffset = offset;
        transposer.TrackerSettings = new Unity.Cinemachine.TargetTracking.TrackerSettings
        {
            BindingMode = Unity.Cinemachine.TargetTracking.BindingMode.WorldSpace,
            PositionDamping = new Vector3(0.55f, 0.65f, 0.5f)
        };

        CinemachineRotationComposer composer = vcamObj.AddComponent<CinemachineRotationComposer>();
        composer.TargetOffset = new Vector3(0f, 0.35f, 0f);
        composer.Damping = new Vector2(0.45f, 0.55f);
        composer.Composition = new ScreenComposerSettings
        {
            ScreenPosition = new Vector2(0.48f, 0.42f)
        };

        fixedCamera.virtualCamera = vcam;
        fixedCamera.targetGroup = targetGroup;
        fixedCamera.followTarget = player;
        fixedCamera.playerFocus = playerFocus != null ? playerFocus : player;
        fixedCamera.goalFocus = goalFocus;
        fixedCamera.eventFocus = eventFocus.transform;
        fixedCamera.baseFov = 52f;
        fixedCamera.playerWeight = 1.35f;
        fixedCamera.goalWeight = 0.52f;

        SetSerializedValue(cameraDynamics, "virtualCamera", vcam);
        SetSerializedValue(cameraDynamics, "followTarget", player);
        SetSerializedValue(cameraDynamics, "baseOffset", offset);
    }

    public static void SpawnExperienceSystems(
        Transform mech,
        Transform env,
        LevelExit exit,
        LevelBlueprint blueprint,
        float routeStartZ,
        float routeEndZ)
    {
        float span = Mathf.Max(4f, routeEndZ - routeStartZ);
        float step = span / 5f;

        Transform movement = CreateExperienceMarker("Section_Movement", new Vector3(0f, 1f, routeStartZ + step * 0.5f), env);
        Transform sync = CreateExperienceMarker("Section_Sync", new Vector3(0f, 1f, routeStartZ + step * 1.5f), env);
        Transform escalation = CreateExperienceMarker("Section_Escalation", new Vector3(0f, 1f, routeStartZ + step * 2.5f), env);
        Transform aha = CreateExperienceMarker("Section_Aha", new Vector3(0f, 1f, routeStartZ + step * 3.5f), env);
        Transform climax = CreateExperienceMarker("Section_Climax", new Vector3(0f, 1f, routeStartZ + step * 4.5f), env);

        Vector3 escapePos = exit != null
            ? exit.transform.position + new Vector3(0f, 0f, -4f)
            : new Vector3(0f, 1.25f, routeEndZ - 2f);
        Transform escapeEnd = CreateExperienceMarker("EscapeRouteEnd", escapePos, mech);

        GameObject blueprintObject = new GameObject("LevelExperienceBlueprint");
        blueprintObject.transform.SetParent(mech, false);
        LevelExperienceBlueprint levelExpBp = blueprintObject.AddComponent<LevelExperienceBlueprint>();
        SetSerializedValue(levelExpBp, "archetype", blueprint.archetype);
        SetSerializedValue(levelExpBp, "movementSection", movement);
        SetSerializedValue(levelExpBp, "syncSection", sync);
        SetSerializedValue(levelExpBp, "escalationSection", escalation);
        SetSerializedValue(levelExpBp, "ahaMoment", aha);
        SetSerializedValue(levelExpBp, "traversalClimax", climax);
        SetSerializedValue(levelExpBp, "requiresEscapeAfterPuzzle", true);
        
        bool isChase = blueprint.archetype == LevelArchetype.Chase;
        SetSerializedValue(levelExpBp, "escapeDurationSeconds", isChase ? 22f : 18f);

        ChaseHazardMotor chase = null;
        if (isChase)
        {
            GameObject hazardObject = new GameObject("ChaseHazard");
            hazardObject.transform.SetParent(mech, false);
            hazardObject.transform.position = new Vector3(0f, 1.25f, routeStartZ - 6f);
            chase = hazardObject.AddComponent<ChaseHazardMotor>();
            hazardObject.SetActive(false);
            SetSerializedValue(levelExpBp, "chaseHazard", chase);
        }

        GameObject escapeObject = new GameObject("LevelEscapeSequence");
        escapeObject.transform.SetParent(mech, false);
        LevelEscapeSequence escapeSequence = escapeObject.AddComponent<LevelEscapeSequence>();
        SetSerializedValue(escapeSequence, "escapeRouteEnd", escapeEnd);
        if (chase != null)
            SetSerializedValue(escapeSequence, "hazard", chase);

        CreatePacingTrigger("Pacing_Movement", new Vector3(0f, 1f, routeStartZ + step * 0.5f), LevelPacingMarker.Section.Movement, env);
        CreatePacingTrigger("Pacing_Sync", new Vector3(0f, 1f, routeStartZ + step * 1.5f), LevelPacingMarker.Section.Synchronization, env);
        CreatePacingTrigger("Pacing_Escalation", new Vector3(0f, 1f, routeStartZ + step * 2.5f), LevelPacingMarker.Section.Escalation, env);
        CreatePacingTrigger("Pacing_Aha", new Vector3(0f, 1f, routeStartZ + step * 3.5f), LevelPacingMarker.Section.AhaMoment, env);
        CreatePacingTrigger("Pacing_Climax", new Vector3(0f, 1f, routeStartZ + step * 4.5f), LevelPacingMarker.Section.TraversalClimax, env);
    }

    public static void SpawnLevelRuntime(Transform parent, LevelBlueprint blueprint)
    {
        GameObject rtObj = new GameObject("LevelRuntimeController");
        rtObj.transform.SetParent(parent, false);
        LevelRuntimeController runtime = rtObj.AddComponent<LevelRuntimeController>();
        SetSerializedValue(runtime, "objectiveText", blueprint.puzzleObjectiveText);
        SetSerializedValue(runtime, "readyPrompt", blueprint.puzzleActiveText);
        SetSerializedValue(runtime, "completionToast", blueprint.puzzleCompleteText);
    }

    private static void CreateCharacterVisual(Transform player)
    {
        GameObject visualRoot = new GameObject("PlayerVisual");
        visualRoot.transform.SetParent(player, false);
        CreateCapsuleVisual(visualRoot.transform, false);
    }

    private static GameObject CreateCapsuleVisual(Transform parent, bool useEchoMaterial)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3D Models/Animated Woman/Casual.fbx");
        GameObject visual;

        if (prefab != null)
        {
            GameObject scaler = new GameObject(useEchoMaterial ? "EchoScaler" : "PlayerScaler");
            scaler.transform.SetParent(parent, false);
            scaler.transform.localScale = Vector3.one * 1.0f;

            visual = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            visual.name = "Model";
            visual.transform.SetParent(scaler.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            Collider[] colliders = visual.GetComponentsInChildren<Collider>();
            foreach (var col in colliders) Object.DestroyImmediate(col);

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                Material[] mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = useEchoMaterial ? EchoesMaterialLibrary.EchoMat : EchoesMaterialLibrary.PlayerMat;
                r.sharedMaterials = mats;
            }

            Animator anim = visual.GetComponent<Animator>();
            if (anim == null) anim = visual.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(AnimatorControllerPath);
            Avatar avatar = LoadAvatarFromCharacterModel();
            if (avatar != null && avatar.isValid)
                anim.avatar = avatar;
        }
        else
        {
            visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = useEchoMaterial ? "EchoCapsule" : "PlayerCapsule";
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(0.8f, 1.05f, 0.8f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<MeshRenderer>().sharedMaterial = useEchoMaterial ? EchoesMaterialLibrary.EchoMat : EchoesMaterialLibrary.PlayerMat;
        }

        return visual;
    }

    private static Avatar LoadAvatarFromCharacterModel()
    {
        const string modelPath = "Assets/3D Models/Animated Woman/Casual.fbx";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is Avatar avatar && avatar.isValid)
                return avatar;
        }
        return null;
    }

    private static Transform CreateExperienceMarker(string name, Vector3 position, Transform parent)
    {
        GameObject marker = new GameObject(name);
        marker.transform.SetParent(parent, false);
        marker.transform.position = position;
        return marker.transform;
    }

    private static void CreatePacingTrigger(string name, Vector3 position, LevelPacingMarker.Section section, Transform parent)
    {
        GameObject triggerObject = new GameObject(name);
        triggerObject.transform.SetParent(parent, false);
        triggerObject.transform.position = position;
        BoxCollider colliderRef = triggerObject.AddComponent<BoxCollider>();
        colliderRef.isTrigger = true;
        colliderRef.size = new Vector3(14f, 8f, 10f);
        LevelPacingMarker marker = triggerObject.AddComponent<LevelPacingMarker>();
        SetSerializedValue(marker, "section", section);
    }

    private static Transform CreateRoot(string name)
    {
        GameObject root = new GameObject(name);
        return root.transform;
    }

    private static PanelSettings GetOrCreatePanelSettings()
    {
        string panelPath = "Assets/UI/EchoesPanelSettings.asset";
        PanelSettings existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelPath);
        if (existing != null) return existing;

        PanelSettings ps = ScriptableObject.CreateInstance<PanelSettings>();
        ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        ps.referenceResolution = new Vector2Int(1920, 1080);
        ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        ps.match = 0.5f;

        string tssPath = "Assets/UI/EchoesTheme.tss";
        ThemeStyleSheet theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(tssPath);
        if (theme != null)
            ps.themeStyleSheet = theme;

        AssetDatabase.CreateAsset(ps, panelPath);
        AssetDatabase.SaveAssets();
        return ps;
    }

    private static void SetSerializedValue(Component component, string propertyName, object value)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null) return;

        switch (value)
        {
            case int intValue:
                property.intValue = intValue;
                break;
            case float floatValue:
                property.floatValue = floatValue;
                break;
            case bool boolValue:
                property.boolValue = boolValue;
                break;
            case string stringValue:
                property.stringValue = stringValue;
                break;
            case Color colorValue:
                property.colorValue = colorValue;
                break;
            case Object objectValue:
                property.objectReferenceValue = objectValue;
                break;
            case Vector3 vectorValue:
                property.vector3Value = vectorValue;
                break;
            case System.Enum enumValue:
                property.enumValueIndex = System.Convert.ToInt32(enumValue);
                break;
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}
