using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cinemachine;

public static partial class EchoesProductionBuilder
{
    const string SceneRoot = "Assets/Scenes";
    const string MaterialRoot = "Assets/Materials/Echoes";
    const string PrefabRoot = "Assets/Prefabs";
    const string EchoPrefabPath = "Assets/Prefabs/EchoPrefab.prefab";
    const string AnimatorControllerPath = "Assets/Prefabs/PlayerAnimController.controller";
    const string FenceStraightPath = "Assets/3D Models/Models/FBX format/fence-straight.fbx";
    const string PolesPath = "Assets/3D Models/Models/FBX format/poles.fbx";
    const string PipePath = "Assets/3D Models/Models/FBX format/pipe.fbx";
    const string SmokeDarkMaterialPath = "Assets/Materials/Echoes/Mat_LiminalFog.mat";
    const int GroundLayer = 6;

    // === Modular SciFi MegaKit model paths ===
    const string SciFiRoot = "Assets/3D Models/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX (Unity)";
    const string SciFiPlatformMetal    = SciFiRoot + "/Platforms/Platform_Metal.fbx";
    const string SciFiPlatformSimple   = SciFiRoot + "/Platforms/Platform_Simple.fbx";
    const string SciFiPlatformCenter   = SciFiRoot + "/Platforms/Platform_CenterPlate.fbx";
    const string SciFiPlatformRamp     = SciFiRoot + "/Platforms/Platform_Ramp_2.fbx";
    const string SciFiDoorDarkMetal    = SciFiRoot + "/Platforms/Door_DarkMetal.fbx";
    const string SciFiDoorFrame        = SciFiRoot + "/Platforms/Door_Frame_A.fbx";
    const string SciFiColumnSimple     = SciFiRoot + "/Columns/Column_Simple.fbx";
    const string SciFiColumnAstra      = SciFiRoot + "/Columns/Column_Astra.fbx";
    const string SciFiColumnLarge      = SciFiRoot + "/Columns/Column_Large_Straight.fbx";
    const string SciFiColumnRound      = SciFiRoot + "/Columns/Column_Round.fbx";
    const string SciFiWallAstra        = SciFiRoot + "/Walls/WallAstra_Straight.fbx";
    const string SciFiWallBand         = SciFiRoot + "/Walls/WallBand_Straight.fbx";
    const string SciFiWallWindow       = SciFiRoot + "/Walls/WallWindow_Straight.fbx";
    const string SciFiPropCrate        = SciFiRoot + "/Props/Prop_Crate3.fbx";
    const string SciFiPropRail         = SciFiRoot + "/Props/Prop_Rail_4.fbx";
    const string SciFiPropLight        = SciFiRoot + "/Props/Prop_Light_Small.fbx";
    const string SciFiPropVent         = SciFiRoot + "/Props/Prop_Vent_Small.fbx";

    // UI Toolkit asset paths
    const string MainMenuUxmlPath   = "Assets/UI/MainMenuUI.uxml";
    const string PauseMenuUxmlPath  = "Assets/UI/PauseMenuUI.uxml";
    const string GameOverUxmlPath   = "Assets/UI/GameOverUI.uxml";
    const string GameHUDUxmlPath    = "Assets/UI/GameHUDUI.uxml";
    const string EchoesThemeUssPath = "Assets/UI/EchoesTheme.uss";

    static Material _floorMat;
    static Material _plateMat;
    static Material _bridgeMat;
    static Material _doorMat;
    static Material _goalMat;
    static Material _playerMat;
    static Material _echoMat;
    static Material _archMat;
    static Material _memoryMat;
    static Material _fluorescentMat;
    static Material _wallRoseMat;
    static Material _wallTealMat;
    static Material _wallMustardMat;
    static Material _wallSageMat;
    static readonly List<string> _fallbackLog = new List<string>();

    [MenuItem("Echoes of You/Production/Rebuild Menu Hub and Levels", false, 200)]
    public static void RebuildAll()
    {
        EnsureFolders();
        EnsureMaterials();
        EnsureAnimatorController();
        EchoesAudioMixerBuilder.EnsureAudioMixer();
        EchoesLocomotionSettingsBuilder.EnsureLocomotionSettings();
        EnsureEchoPrefab();

        BuildMainMenu();
        BuildLevel01();
        BuildLevel02();
        BuildLevel03();
        BuildLevel04();
        BuildLevel05();
        BuildLevel06();
        BuildLevel07();
        BuildLevel08();
        BuildLevel09();
        BuildLevel10();
        BuildLevel11();
        BuildLevel12();
        BuildLevel13();
        BuildLevel14();
        BuildLevel15();
        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Echoes Production] Scenes rebuilt. Running validation...");
        LevelValidator.ValidateAllLevels();
    }

    [MenuItem("Echoes of You/Production/Rebuild Prototypes Only (A, B, C)", false, 201)]
    public static void RebuildPrototypes()
    {
        EnsureFolders();
        EnsureMaterials();
        EnsureAnimatorController();
        EchoesAudioMixerBuilder.EnsureAudioMixer();
        EchoesLocomotionSettingsBuilder.EnsureLocomotionSettings();
        EnsureEchoPrefab();

        BuildLevel07();
        BuildLevel10();
        BuildLevel14();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Echoes Production] Prototipos A, B y C reconstruidos.");
    }

    static void BuildMainMenu()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        SetupAtmosphere(HexColor("3A4858"), 0.008f, HexColor("B0B8C0"));
        SpawnDirectionalLight();

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera cameraRef = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.transform.position = new Vector3(0f, 3.6f, -12f);
        cameraObject.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
        cameraRef.clearFlags = CameraClearFlags.SolidColor;
        cameraRef.backgroundColor = new Color(0.04f, 0.06f, 0.08f, 1f);

        MakePlatform("MenuFloor", new Vector3(0f, 0f, 6f), new Vector3(20f, 0.5f, 20f), null, _floorMat);
        SpawnLiminalHorizon("MenuHorizon", Vector3.zero, 28f, 12f, null);
        MakePlatform("MenuMonolith", new Vector3(0f, 1.8f, 13f), new Vector3(3f, 3.6f, 3f), null, _goalMat);
        SpawnDistantArchitecture(Vector3.zero, 28f, 12f, null);
        SpawnPointLight("MenuGlow", new Vector3(0f, 5f, 13f), new Color(0.16f, 0.85f, 1f, 1f), 6f, 12f, null);
        SpawnAmbientParticles(new Vector3(0f, 2f, 8f), new Vector3(18f, 8f, 18f));

        // --- UI TOOLKIT MAIN MENU ---
        GameObject menuUIObj = new GameObject("MainMenuUI");
        UIDocument menuDoc = menuUIObj.AddComponent<UIDocument>();
        menuDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainMenuUxmlPath);
        menuDoc.panelSettings = GetOrCreatePanelSettings();
        menuUIObj.AddComponent<MainMenuController>();

        // Ensure EventSystem is present in MainMenu scene so UI Toolkit receives input
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.transform.SetParent(menuUIObj.transform, false);
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        GameObject tm = new GameObject("SceneTransitionManager");
        tm.AddComponent<SceneTransitionManager>();

        GameObject cinematic = new GameObject("MainMenuCinematicWorld");
        cinematic.AddComponent<MainMenuCinematicWorld>();

        SaveScene(scene, "MainMenu");
    }

    /// <summary>
    /// Generates the technical shell of a level: atmosphere, lighting, player,
    /// camera, UI, and exit. It does not generate puzzle geometry or layout.
    /// Playable architecture is designed by hand in the Unity editor.
    /// </summary>
    static void BuildLevelShell(
        string sceneName,
        string nextSceneName,
        Color fogColor,
        float fogDensity,
        Color ambientColor,
        Vector3 playerSpawn,
        Vector3 cameraOffset,
        float cameraFov,
        int maxEchos,
        float maxRecordDuration,
        string objectiveText,
        string introLine,
        string completionLine)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = sceneName;

        Transform env = CreateRoot("--- ENVIRONMENT ---");
        Transform mech = CreateRoot("--- MECHANICS ---");
        Transform ui = CreateRoot("--- UI ---");

        SetupAtmosphere(fogColor, fogDensity, ambientColor);
        SpawnDirectionalLight();
        SpawnLiminalHorizon("Horizon", new Vector3(0f, 0f, 12f), 24f, 36f, env);
        SpawnDistantArchitecture(new Vector3(0f, 0f, 12f), 24f, 36f, env);
        SpawnIntroDressing(env, Vector3.zero);

        MakePlatform("PLACEHOLDER_Floor", playerSpawn + new Vector3(0f, -0.5f, 0f),
            new Vector3(12f, 0.5f, 12f), env, _floorMat);

        GameObject player = SpawnPlayer(playerSpawn, true, maxEchos, maxRecordDuration);
        SpawnGameplayCameraCustom(player.transform, cameraOffset, cameraFov, mech);

        CreateLevelExit(playerSpawn + new Vector3(0f, 0f, 20f), mech, nextSceneName);

        SpawnGameplayHud(ui);
        SpawnPauseMenu(ui);
        SpawnGameOver(ui);
        SpawnLevelRuntime(mech, objectiveText, introLine, completionLine);
        SpawnAmbientLights(env, playerSpawn + new Vector3(0f, 4f, 8f), 20f, 24f);

        SaveScene(scene, sceneName);
    }

    /// <summary>
    /// Looks for a prefab by partial name, case-insensitive. Returns null so callers can use procedural fallback.
    /// The position is parent-local when a parent is provided.
    /// </summary>
    static GameObject TryInstantiateAssetByName(string searchTerm, Transform parent, Vector3 position)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return null;

        // Apply aliases to map logical terms to actual imported files in the project
        string resolvedSearchTerm = searchTerm;
        string lower = searchTerm.ToLowerInvariant();
        if (lower == "locker")
            resolvedSearchTerm = "bookcaseClosed";
        else if (lower == "bookshelf" || lower == "bookcase")
            resolvedSearchTerm = "bookcaseOpen";
        else if (lower == "school desk")
            resolvedSearchTerm = "desk";

        string[] guids = AssetDatabase.FindAssets("t:GameObject");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string fileName = Path.GetFileNameWithoutExtension(path);
            bool matchesName = fileName.IndexOf(resolvedSearchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0;
            bool matchesPath = path.IndexOf(resolvedSearchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (!matchesName && !matchesPath)
                continue;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
                return null;

            if (parent != null)
            {
                instance.transform.SetParent(parent, false);
                instance.transform.localPosition = position;
            }
            else
            {
                instance.transform.position = position;
            }
            return instance;
        }

        return null;
    }

    static void LogFallback(string moduleName)
    {
        _fallbackLog.Add(moduleName);
    }

    static void PrintFallbackReport(string levelName)
    {
        if (_fallbackLog.Count == 0)
        {
            Debug.Log($"[Echoes Production] {levelName}: todos los m├│dulos usaron asset real.");
            _fallbackLog.Clear();
            return;
        }

        Debug.LogWarning($"[Echoes Production] {levelName}: {_fallbackLog.Count} m├│dulos en fallback procedural: "
            + string.Join(", ", _fallbackLog));
        _fallbackLog.Clear();
    }

    static Transform SpawnCorridorModule(string name, Vector3 position, float length, bool withLockers, Transform parent)
    {
        Transform root = new GameObject(name).transform;
        root.SetParent(parent, false);
        root.position = position;

        GameObject asset = TryInstantiateAssetByName("corridor", root, Vector3.zero);
        if (asset == null)
        {
            float midZ = length * 0.5f;
            LogFallback(name + "_floor");
            MakePlatform(name + "_Floor", new Vector3(0f, 0f, midZ), new Vector3(4f, 0.3f, length), root, _floorMat);
            MakePlatform(name + "_CeilingShadow", new Vector3(0f, 3.2f, midZ), new Vector3(4f, 0.2f, length), root, _archMat);
            MakePlatform(name + "_WallL", new Vector3(-2f, 1.5f, midZ), new Vector3(0.2f, 3f, length), root, _archMat);
            MakePlatform(name + "_WallR", new Vector3(2f, 1.5f, midZ), new Vector3(0.2f, 3f, length), root, _archMat);
        }

        if (withLockers)
        {
            GameObject lockers = TryInstantiateAssetByName("locker", root, new Vector3(-1.9f, 1f, length * 0.5f));
            if (lockers == null)
            {
                LogFallback(name + "_lockers");
                MakePlatform(name + "_LockerRow", new Vector3(-1.85f, 1f, length * 0.5f), new Vector3(0.3f, 2f, length * 0.8f), root, _archMat);
            }
        }

        return root;
    }

    static Transform SpawnClassroomModule(
        string name,
        Vector3 position,
        Vector3 size,
        int deskRows,
        int deskCols,
        bool hasMemoryDesk,
        Transform parent)
    {
        Transform root = new GameObject(name).transform;
        root.SetParent(parent, false);
        root.position = position;

        MakePlatform(name + "_Floor", Vector3.zero, new Vector3(size.x, 0.3f, size.z), root, _floorMat);
        MakePlatform(name + "_WallBack", new Vector3(0f, size.y * 0.5f, size.z * 0.5f),
            new Vector3(size.x, size.y, 0.2f), root, _archMat);

        float spacingX = size.x / (deskCols + 1);
        float spacingZ = size.z / (deskRows + 1);

        for (int row = 0; row < deskRows; row++)
        {
            for (int col = 0; col < deskCols; col++)
            {
                Vector3 deskPos = new Vector3(
                    -size.x * 0.5f + spacingX * (col + 1),
                    0.4f,
                    -size.z * 0.5f + spacingZ * (row + 1));

                bool isMemoryDesk = hasMemoryDesk && row == deskRows - 1 && col == deskCols - 1;

                GameObject desk = TryInstantiateAssetByName("school desk", root, deskPos);
                if (desk == null)
                {
                    LogFallback(name + "_desk_" + row + "_" + col);
                    Material mat = isMemoryDesk ? _memoryMat : _archMat;
                    MakePlatform(name + "_Desk_" + row + "_" + col, deskPos, new Vector3(0.6f, 0.7f, 0.5f), root, mat);
                }
                else if (isMemoryDesk)
                {
                    SpawnMemoryGlow(root, root.TransformPoint(deskPos));
                }
            }
        }

        return root;
    }

    static Transform SpawnLibraryStackModule(string name, Vector3 position, float length, Transform parent)
    {
        Transform root = new GameObject(name).transform;
        root.SetParent(parent, false);
        root.position = position;

        MakePlatform(name + "_Floor", Vector3.zero, new Vector3(2.2f, 0.3f, length), root, _floorMat);

        GameObject shelfL = TryInstantiateAssetByName("bookshelf", root, new Vector3(-1.3f, 1.5f, 0f));
        GameObject shelfR = TryInstantiateAssetByName("bookshelf", root, new Vector3(1.3f, 1.5f, 0f));

        if (shelfL == null)
        {
            LogFallback(name + "_shelfL");
            MakePlatform(name + "_ShelfL", new Vector3(-1.3f, 1.5f, 0f), new Vector3(0.5f, 3f, length), root, _archMat);
        }
        if (shelfR == null)
        {
            LogFallback(name + "_shelfR");
            MakePlatform(name + "_ShelfR", new Vector3(1.3f, 1.5f, 0f), new Vector3(0.5f, 3f, length), root, _archMat);
        }

        return root;
    }

    static Transform SpawnCourtyardModule(string name, Vector3 position, float radius, Transform parent)
    {
        Transform root = new GameObject(name).transform;
        root.SetParent(parent, false);
        root.position = position;

        MakePlatform(name + "_Ground", Vector3.zero, new Vector3(radius * 2f, 0.3f, radius * 2f), root, _floorMat);
        return root;
    }

    static void SpawnCeilingFluorescent(Transform corridorRoot, float zPosition)
    {
        SpawnHardLight("Fluorescent", corridorRoot.position + new Vector3(0f, 2.9f, zPosition),
            HexColor("C9D4B0"), 2.2f, corridorRoot);
    }

    static void SpawnMemoryGlow(Transform parent, Vector3 position)
    {
        SpawnHardLight("MemoryGlow", position + Vector3.up * 1.2f,
            HexColor("E8B262"), 1.4f, parent);
    }




    static EchoKineticZone CreateMomentumRelay(string name, Vector3 position, Vector3 size, Transform target, float force, Transform parent)
    {
        GameObject zone = new GameObject(name);
        zone.transform.SetParent(parent, false);
        zone.transform.position = position;

        BoxCollider col = zone.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;

        EchoKineticZone kZone = zone.AddComponent<EchoKineticZone>();
        SetSerializedValue(kZone, "role", EchoKineticRole.MomentumRelay);
        SetSerializedValue(kZone, "momentumRelayTarget", target);
        SetSerializedValue(kZone, "momentumRelayForce", force);
        SetSerializedValue(kZone, "requireEcho", true);
        SetSerializedValue(kZone, "acceptPlayer", true);

        // Visual indicator (glowing zone)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(zone.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(size.x, 0.08f, size.z);
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        visual.GetComponent<MeshRenderer>().sharedMaterial = _echoMat;

        SpawnPointLight(name + "_Glow", position + Vector3.up * 1.5f, new Color(0.16f, 0.85f, 1f), 2.5f, Mathf.Max(size.x, size.z) + 4f, zone.transform);

        return kZone;
    }



    static GhostBridge CreateGhostBridge(string name, Vector3 position, Vector3 scale, Transform parent, PuzzleSignal signal)
    {
        GameObject bridge = Instantiate3DModel(SciFiPlatformSimple, name, position, scale, Quaternion.identity, parent, _bridgeMat);
        bridge.isStatic = false;
        GhostBridge ghost = bridge.AddComponent<GhostBridge>();
        Color activeCol = new Color(0f, 0.9f, 1f, 0.85f);
        Color inactiveCol = new Color(0f, 0.9f, 1f, 0.08f);
        ghost.Configure(signal, activeCol, inactiveCol);
        return ghost;
    }

    static GameObject CreatePushableBlock(string name, Vector3 position, Vector3 scale, Transform parent, Material mat)
    {
        GameObject block = Instantiate3DModel(SciFiPropCrate, name, position, scale, Quaternion.identity, parent, mat);
        block.isStatic = false;

        Rigidbody rb = block.GetComponent<Rigidbody>();
        if (rb == null)
            rb = block.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = 12f;
        rb.linearDamping = 1f;
        rb.angularDamping = 1f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        block.AddComponent<KineticPushableBlock>();
        return block;
    }

    static MemoryPlatform CreateMemoryPlatform(string name, Vector3 anchorPos, Vector3 scale, PressurePlate[] plates, Vector3[] localPositions, float speed, Transform parent, Material mat)
    {
        GameObject anchor = new GameObject(name + "_Anchor");
        anchor.transform.SetParent(parent, false);
        anchor.transform.position = anchorPos;

        GameObject platformObj = Instantiate3DModel(SciFiPlatformSimple, name, Vector3.zero, scale, Quaternion.identity, anchor.transform, mat);
        platformObj.isStatic = false;

        MemoryPlatform memPlat = platformObj.AddComponent<MemoryPlatform>();
        memPlat.plates = plates;
        memPlat.localPositions = localPositions;
        memPlat.travelSpeed = speed;

        return memPlat;
    }

    static PuzzleCondition CreateCondition(string name, PuzzleCondition.ConditionType type, PressurePlate[] plates, PuzzleSignal targetSignal, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        PuzzleCondition condition = obj.AddComponent<PuzzleCondition>();
        condition.type = type;
        condition.plates = plates;
        condition.targetSignal = targetSignal;
        return condition;
    }




    static GameObject SpawnPlayer(Vector3 position, bool enableRecorder, int maxEchoes, float maxRecordSeconds)
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

        // FIXED: Unity requiere que al menos un objeto tenga Rigidbody para que OnTriggerEnter se dispare
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        PlayerController playerController = player.AddComponent<PlayerController>();
        // Set groundMask strictly to the Ground layer (6) to avoid the SphereCast detecting the player's own CharacterController (on Default layer).
        playerController.groundMask = (1 << 6);
        playerController.jumpHeight = EchoesWorldMetrics.PlayerJumpHeight;

        player.AddComponent<PlayerCharacterVisualSetup>();
        player.AddComponent<PlayerAdvancedLocomotion>();
        player.AddComponent<CharacterPush>();

        if (enableRecorder)
        {
            EchoRecorder recorder = player.AddComponent<EchoRecorder>();
            SetSerializedValue(recorder, "echoPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(EchoPrefabPath));
            SetSerializedValue(recorder, "maxEchoes", maxEchoes);
            SetSerializedValue(recorder, "maxRecordSeconds", maxRecordSeconds);
        }

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

    static void CreateCharacterVisual(Transform player)
    {
        GameObject visualRoot = new GameObject("PlayerVisual");
        visualRoot.transform.SetParent(player, false);
        CreateCapsuleVisual(visualRoot.transform, false);
    }

    static void SpawnGameplayCamera(Transform player, Vector3? customOffset = null)
    {
        Transform cameraRoot = CreateRoot("--- CAMERA ---");

        Vector3 offset = customOffset.HasValue ? customOffset.Value : new Vector3(-5.5f, 3.2f, -9.5f);

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

        CinemachineBrain brain = cameraObject.AddComponent<CinemachineBrain>();
        brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        brain.m_DefaultBlend.m_Time = 0.35f;

        Transform playerFocus = player.Find("CameraFocus");
        Transform goalFocus = FindPrimaryGoalFocus();

        GameObject targetGroupObject = new GameObject("GameplayCameraTargets");
        targetGroupObject.transform.SetParent(cameraRoot, false);
        CinemachineTargetGroup targetGroup = targetGroupObject.AddComponent<CinemachineTargetGroup>();
        targetGroup.m_Targets = new[]
        {
            new CinemachineTargetGroup.Target
            {
                target = playerFocus != null ? playerFocus : player,
                weight = 1.35f,
                radius = 0.6f
            },
            new CinemachineTargetGroup.Target
            {
                target = goalFocus != null ? goalFocus : player,
                weight = goalFocus != null ? 0.52f : 0f,
                radius = 1.4f
            }
        };

        GameObject eventFocus = new GameObject("CameraEventFocus");
        eventFocus.transform.SetParent(targetGroupObject.transform, false);
        eventFocus.transform.position = goalFocus != null ? goalFocus.position : player.position;

        GameObject vcamObj = new GameObject("PlayerVCam");
        vcamObj.transform.SetParent(cameraRoot, false);
        CinemachineVirtualCamera vcam = vcamObj.AddComponent<CinemachineVirtualCamera>();
        vcam.Priority = 20;
        vcam.Follow = player;
        vcam.LookAt = targetGroup.transform;
        vcam.m_Lens.FieldOfView = 52f;

        CinemachineTransposer transposer = vcam.AddCinemachineComponent<CinemachineTransposer>();
        transposer.m_FollowOffset = offset;
        transposer.m_XDamping = 0.55f;
        transposer.m_YDamping = 0.65f;
        transposer.m_ZDamping = 0.5f;
        transposer.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;

        CinemachineComposer composer = vcam.AddCinemachineComponent<CinemachineComposer>();
        composer.m_TrackedObjectOffset = new Vector3(0f, 0.35f, 0f);
        composer.m_HorizontalDamping = 0.45f;
        composer.m_VerticalDamping = 0.55f;
        composer.m_DeadZoneWidth = 0.12f;
        composer.m_DeadZoneHeight = 0.1f;
        composer.m_SoftZoneWidth = 0.55f;
        composer.m_SoftZoneHeight = 0.48f;
        composer.m_ScreenX = 0.48f;
        composer.m_ScreenY = 0.42f;

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

    /// <summary>
    /// Version of SpawnGameplayCamera with level-specific FOV and offset.
    /// Uses Cinemachine with intentional composition settings.
    /// </summary>
    static void SpawnGameplayCameraCustom(
        Transform player,
        Vector3 offset,
        float fov,
        Transform parent = null)
    {
        Transform cameraRoot = CreateRoot("--- CAMERA ---");
        if (parent != null) cameraRoot.SetParent(parent, false);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(cameraRoot, false);
        cameraObject.transform.position = player.position + offset;

        Camera cameraRef = cameraObject.AddComponent<Camera>();
        cameraRef.clearFlags = CameraClearFlags.SolidColor;
        cameraRef.backgroundColor = new Color(0.02f, 0.03f, 0.05f, 1f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CameraShake>();

        var gfc = cameraObject.AddComponent<GameFeelController>();
        var loop1 = AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/Efectos de sonido/144046__gchase__room_tone_ambience_medium_control_low_hum.wav");
        var loop2 = AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/Efectos de sonido/607238__szegvari__electric-dream-synth-drone-electric-cinematic.wav");
        SetSerializedValue(gfc, "ambientLoopClip", loop1);
        SetSerializedValue(gfc, "industrialDroneClip", loop2);

        cameraObject.AddComponent<CinematicRecordingOverlay>();
        CinematicCameraDynamics cameraDynamics = cameraObject.AddComponent<CinematicCameraDynamics>();
        FixedPuzzleCameraController fixedCamera = cameraObject.AddComponent<FixedPuzzleCameraController>();

        CinemachineBrain brain = cameraObject.AddComponent<CinemachineBrain>();
        brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        brain.m_DefaultBlend.m_Time = 0.35f;

        Transform playerFocus = player.Find("CameraFocus");
        Transform goalFocus = FindPrimaryGoalFocus();

        GameObject targetGroupObject = new GameObject("GameplayCameraTargets");
        targetGroupObject.transform.SetParent(cameraRoot, false);
        CinemachineTargetGroup targetGroup = targetGroupObject.AddComponent<CinemachineTargetGroup>();
        targetGroup.m_Targets = new[]
        {
            new CinemachineTargetGroup.Target
            {
                target = playerFocus != null ? playerFocus : player,
                weight = 1.35f,
                radius = 0.6f
            },
            new CinemachineTargetGroup.Target
            {
                target = goalFocus != null ? goalFocus : player,
                weight = goalFocus != null ? 0.52f : 0f,
                radius = 1.4f
            }
        };

        GameObject eventFocus = new GameObject("CameraEventFocus");
        eventFocus.transform.SetParent(targetGroupObject.transform, false);
        eventFocus.transform.position = goalFocus != null ? goalFocus.position : player.position;

        GameObject vcamObj = new GameObject("PlayerVCam");
        vcamObj.transform.SetParent(cameraRoot, false);
        CinemachineVirtualCamera vcam = vcamObj.AddComponent<CinemachineVirtualCamera>();
        vcam.Priority = 20;
        vcam.Follow = player;
        vcam.LookAt = targetGroup.transform;
        vcam.m_Lens.FieldOfView = fov;

        CinemachineTransposer transposer = vcam.AddCinemachineComponent<CinemachineTransposer>();
        transposer.m_FollowOffset = offset;
        transposer.m_XDamping = 0.55f;
        transposer.m_YDamping = 0.65f;
        transposer.m_ZDamping = 0.5f;
        transposer.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;

        CinemachineComposer composer = vcam.AddCinemachineComponent<CinemachineComposer>();
        composer.m_TrackedObjectOffset = new Vector3(0f, 0.35f, 0f);
        composer.m_HorizontalDamping = 0.45f;
        composer.m_VerticalDamping = 0.55f;
        composer.m_DeadZoneWidth = 0.12f;
        composer.m_DeadZoneHeight = 0.1f;
        composer.m_SoftZoneWidth = 0.55f;
        composer.m_SoftZoneHeight = 0.48f;
        composer.m_ScreenX = 0.48f;
        composer.m_ScreenY = 0.42f;

        fixedCamera.virtualCamera = vcam;
        fixedCamera.targetGroup = targetGroup;
        fixedCamera.followTarget = player;
        fixedCamera.playerFocus = playerFocus != null ? playerFocus : player;
        fixedCamera.goalFocus = goalFocus;
        fixedCamera.eventFocus = eventFocus.transform;
        fixedCamera.baseFov = fov;
        fixedCamera.playerWeight = 1.35f;
        fixedCamera.goalWeight = 0.52f;

        SetSerializedValue(cameraDynamics, "virtualCamera", vcam);
        SetSerializedValue(cameraDynamics, "followTarget", player);
        SetSerializedValue(cameraDynamics, "baseOffset", offset);
    }

    static Transform FindPrimaryGoalFocus()
    {
        LevelExit exit = Object.FindAnyObjectByType<LevelExit>();
        if (exit != null)
        {
            Transform goalFocus = exit.transform.parent != null ? exit.transform.parent.Find("GoalFocus") : null;
            if (goalFocus != null) return goalFocus;
            return exit.transform;
        }

        GameObject area = GameObject.Find("LevelExit_Area");
        if (area != null) return area.transform;

        return null;
    }

    static void SpawnGameplayHud(Transform parent)
    {
        GameObject hud = new GameObject("GameHUD");
        hud.transform.SetParent(parent, false);
        UIDocument hudDoc = hud.AddComponent<UIDocument>();
        hudDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GameHUDUxmlPath);
        hudDoc.panelSettings = GetOrCreatePanelSettings();
        hudDoc.sortingOrder = 0; // Base layer
        hud.AddComponent<GameHUD>();
    }

    static void SpawnPauseMenu(Transform parent)
    {
        GameObject pause = new GameObject("PauseMenu");
        pause.transform.SetParent(parent, false);
        UIDocument pauseDoc = pause.AddComponent<UIDocument>();
        pauseDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PauseMenuUxmlPath);
        pauseDoc.panelSettings = GetOrCreatePanelSettings();
        pauseDoc.sortingOrder = 10; // Above HUD so settings sliders receive input
        pause.AddComponent<PauseMenu>();

        // Ensure EventSystem is present in gameplay scenes so UI Toolkit receives input
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.transform.SetParent(parent, false);
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    static void SpawnGameOver(Transform parent)
    {
        GameObject go = new GameObject("GameOverUI");
        go.transform.SetParent(parent, false);
        UIDocument goDoc = go.AddComponent<UIDocument>();
        goDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GameOverUxmlPath);
        goDoc.panelSettings = GetOrCreatePanelSettings();
        goDoc.sortingOrder = 20; // Above pause menu
        go.AddComponent<GameOverController>();
    }

    static PanelSettings GetOrCreatePanelSettings()
    {
        string panelPath = "Assets/UI/EchoesPanelSettings.asset";
        PanelSettings existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelPath);
        if (existing != null) return existing;

        PanelSettings ps = ScriptableObject.CreateInstance<PanelSettings>();
        ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        ps.referenceResolution = new Vector2Int(1920, 1080);
        ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        ps.match = 0.5f;
        ThemeStyleSheet theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(EchoesThemeUssPath);
        AssetDatabase.CreateAsset(ps, panelPath);
        AssetDatabase.SaveAssets();
        return ps;
    }

    static void SpawnAmbientLights(Transform parent, Vector3 center, float width, float depth)
    {
        Color warmDim = new Color(0.95f, 0.82f, 0.65f, 1f);
        Color coolDim = new Color(0.65f, 0.78f, 1f, 1f);
        float intensity = 5.5f;
        float range = 22f;
        float halfW = width * 0.4f;
        float halfD = depth * 0.4f;

        SpawnPointLight("Amb_FL", center + new Vector3(-halfW, 3f, -halfD), warmDim, intensity, range, parent);
        SpawnPointLight("Amb_FR", center + new Vector3(halfW, 3f, -halfD), coolDim, intensity, range, parent);
        SpawnPointLight("Amb_BL", center + new Vector3(-halfW, 3f, halfD), coolDim, intensity, range, parent);
        SpawnPointLight("Amb_BR", center + new Vector3(halfW, 3f, halfD), warmDim, intensity, range, parent);
        SpawnPointLight("Amb_Center", center + new Vector3(0f, 8f, 0f), new Color(0.9f, 0.94f, 1f, 1f), 7f, 28f, parent);
    }

    // Declara la intenci├│n de dise├▒o del puzzle para validaci├│n
    static void SpawnPuzzleIntent(Transform parent, int buttons, int actions, bool movement, bool timing, bool multiStep, float echoDistance, string note)
    {
        GameObject intentObj = new GameObject("PuzzleIntent");
        intentObj.transform.SetParent(parent, false);
        PuzzleIntent intent = intentObj.AddComponent<PuzzleIntent>();
        intent.buttonCount = buttons;
        intent.requiredActions = Mathf.Max(2, actions, buttons + 1);
        intent.requiresMovement = movement || buttons > 1;
        intent.requiresTiming = timing || actions >= 2;
        intent.isMultiStep = multiStep || buttons > 1;
        intent.minimumEchoDistance = echoDistance;
        SetSerializedValue(intent, "designNote", note);
    }

    static void SpawnLevelRuntime(Transform parent, string objective, string intro, string completion)
    {
        GameObject runtime = new GameObject("LevelRuntimeController");
        runtime.transform.SetParent(parent, false);
        LevelRuntimeController controller = runtime.AddComponent<LevelRuntimeController>();

        bool silent = string.IsNullOrEmpty(objective)
            && string.IsNullOrEmpty(intro)
            && string.IsNullOrEmpty(completion);

        if (!silent)
        {
            SetSerializedValue(controller, "objectiveText", objective);
            SetSerializedValue(controller, "introLine", intro);
            SetSerializedValue(controller, "completionLine", completion);
        }
        else
        {
            Debug.Log("[Echoes Production] LevelRuntimeController en modo silencioso para este nivel.");
        }
    }

    /// <summary>
    /// Blueprint de experiencia, secuencia de escape, marcadores de ritmo y chase opcional.
    /// </summary>
    static void SpawnExperienceSystems(
        Transform mech,
        Transform env,
        LevelExit exit,
        LevelArchetype archetype,
        float routeStartZ,
        float routeEndZ,
        bool enableChase = false)
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
        LevelExperienceBlueprint blueprint = blueprintObject.AddComponent<LevelExperienceBlueprint>();
        SetSerializedValue(blueprint, "archetype", archetype);
        SetSerializedValue(blueprint, "movementSection", movement);
        SetSerializedValue(blueprint, "syncSection", sync);
        SetSerializedValue(blueprint, "escalationSection", escalation);
        SetSerializedValue(blueprint, "ahaMoment", aha);
        SetSerializedValue(blueprint, "traversalClimax", climax);
        SetSerializedValue(blueprint, "requiresEscapeAfterPuzzle", true);
        SetSerializedValue(blueprint, "escapeDurationSeconds", enableChase ? 22f : 18f);

        ChaseHazardMotor chase = null;
        if (enableChase)
        {
            GameObject hazardObject = new GameObject("ChaseHazard");
            hazardObject.transform.SetParent(mech, false);
            hazardObject.transform.position = new Vector3(0f, 1.25f, routeStartZ - 6f);
            chase = hazardObject.AddComponent<ChaseHazardMotor>();
            hazardObject.SetActive(false);
            SetSerializedValue(blueprint, "chaseHazard", chase);
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

    static Transform CreateExperienceMarker(string name, Vector3 position, Transform parent)
    {
        GameObject marker = new GameObject(name);
        marker.transform.SetParent(parent, false);
        marker.transform.position = position;
        return marker.transform;
    }

    static void CreatePacingTrigger(string name, Vector3 position, LevelPacingMarker.Section section, Transform parent)
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

    static PuzzleSignal CreatePuzzleSignal(string name, string displayName, Transform parent)
    {
        GameObject signalObject = new GameObject(name);
        signalObject.transform.SetParent(parent, false);
        PuzzleSignal signal = signalObject.AddComponent<PuzzleSignal>();
        signal.Configure(displayName, true, false);
        return signal;
    }

    static LevelGoal CreateSignalGoal(Transform parent, string objectiveText, string readyPrompt, string completionToast, LevelExit exit, params PuzzleSignal[] signals)
    {
        GameObject goalObject = new GameObject("LevelGoal");
        goalObject.transform.SetParent(parent, false);
        goalObject.transform.position = exit != null ? exit.transform.position : parent.position;

        for (int i = 0; i < signals.Length; i++)
        {
            if (signals[i] != null)
                CreateGoalTrigger(goalObject.transform, signals[i], signals[i].DisplayName);
        }

        LevelGoal goal = goalObject.AddComponent<LevelGoal>();
        SetSerializedValue(goal, "objectiveText", objectiveText);
        SetSerializedValue(goal, "readyPrompt", readyPrompt);
        SetSerializedValue(goal, "completionToast", completionToast);
        SetSerializedValue(goal, "autoCollectChildTriggers", true);
        SetSerializedValue(goal, "requiredTriggerCount", Mathf.Max(1, signals.Length));
        return goal;
    }

    static GravityZone CreateGravityZone(string name, Vector3 position, Vector3 size, Vector3 gravityDirection, float gravityStrength, int priority, Transform parent)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;

        GravityZone zone = root.AddComponent<GravityZone>();
        SetSerializedValue(zone, "gravityDirection", gravityDirection);
        SetSerializedValue(zone, "gravityStrength", gravityStrength);
        SetSerializedValue(zone, "priority", priority);

        // Visual indicator (translucent violet box, URP transparent material)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = size;
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        visual.GetComponent<MeshRenderer>().sharedMaterial =
            EchoesMaterialLibrary.GetOrCreateTransparentMaterial("Mat_GravityZone", new Color(0.6f, 0.2f, 0.8f, 0.15f), false);

        return zone;
    }

    static EchoKineticBody CreateKineticBlock(string name, Vector3 anchorPosition, Vector3 blockScale, Vector3 triggerLocalPosition, Vector3 triggerSize, Vector3 activeLocal, Transform parent, PuzzleSignal signal, bool requireEcho = true, bool holdToMove = true, float speed = 3f)
    {
        GameObject anchor = new GameObject(name + "_Anchor");
        anchor.transform.SetParent(parent, false);
        anchor.transform.position = anchorPosition;

        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.SetParent(anchor.transform, false);
        block.transform.localScale = blockScale;
        block.layer = GroundLayer;
        block.GetComponent<MeshRenderer>().sharedMaterial = _bridgeMat;
        block.AddComponent<KenneyTiling>();

        GameObject trigger = new GameObject(name + "_EchoForce");
        trigger.transform.SetParent(anchor.transform, false);
        trigger.transform.localPosition = triggerLocalPosition;
        BoxCollider colliderRef = trigger.AddComponent<BoxCollider>();
        colliderRef.isTrigger = true;
        colliderRef.size = triggerSize;

        GameObject actionZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        actionZone.name = name + "_ActionZone_Visible";
        actionZone.transform.SetParent(trigger.transform, false);
        actionZone.transform.localPosition = Vector3.zero;
        actionZone.transform.localScale = new Vector3(triggerSize.x, 0.08f, triggerSize.z);
        Object.DestroyImmediate(actionZone.GetComponent<Collider>());
        actionZone.GetComponent<MeshRenderer>().sharedMaterial = _plateMat;

        SpawnPointLight(name + "_ActionZone_Glow", anchorPosition + triggerLocalPosition + Vector3.up * 1.4f, new Color(0.24f, 0.76f, 1f, 1f), 2f, Mathf.Max(triggerSize.x, triggerSize.z) + 3f, trigger.transform);

        EchoKineticBody body = trigger.AddComponent<EchoKineticBody>();
        body.Configure(block.transform, Vector3.zero, activeLocal, Vector3.zero, Vector3.zero, speed, !requireEcho, true, requireEcho, holdToMove, signal);
        return body;
    }

    static DynamicTransformMotor CreateMotorPlatform(string name, Vector3 anchorPosition, Vector3 scale, Vector3 localA, Vector3 localB, Vector3 rotationPerSecond, float cycleDuration, float phase, Transform parent, Material material = null)
    {
        GameObject anchor = new GameObject(name + "_Anchor");
        anchor.transform.SetParent(parent, false);
        anchor.transform.position = anchorPosition;

        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = name;
        platform.transform.SetParent(anchor.transform, false);
        platform.transform.localPosition = localA;
        platform.transform.localScale = scale;
        platform.layer = GroundLayer;
        platform.GetComponent<MeshRenderer>().sharedMaterial = material != null ? material : _bridgeMat;
        platform.AddComponent<KenneyTiling>();

        DynamicTransformMotor motor = platform.AddComponent<DynamicTransformMotor>();
        motor.Configure(localA, localB, rotationPerSecond, cycleDuration, phase, localA != localB);
        return motor;
    }

    static EchoShieldField CreateHazardField(string name, Vector3 position, Vector3 size, Transform parent, PuzzleSignal signal)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        Vector3 scaledSize = new Vector3(
            size.x * EchoesWorldMetrics.HazardThicknessScale,
            Mathf.Max(size.y, EchoesWorldMetrics.HazardMinHeight),
            size.z * EchoesWorldMetrics.HazardThicknessScale);

        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = scaledSize;

        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "EnergyCore";
        beam.transform.SetParent(root.transform, false);
        beam.transform.localScale = new Vector3(scaledSize.x, Mathf.Min(scaledSize.y, 3.2f), scaledSize.z);
        Object.DestroyImmediate(beam.GetComponent<Collider>());

        Material hazardMat = new Material(Shader.Find(EchoesUrpMaterials.LitShaderName));
        hazardMat.color = new Color(1f, 0.16f, 0.08f, 0.62f);
        hazardMat.EnableKeyword("_EMISSION");
        hazardMat.SetColor("_EmissionColor", new Color(1f, 0.12f, 0.04f) * 2.6f);
        beam.GetComponent<MeshRenderer>().sharedMaterial = hazardMat;

        Light lightRef = SpawnPointLight(name + "_Light", position + Vector3.up * 1.5f, new Color(1f, 0.14f, 0.08f, 1f), 4f, Mathf.Max(size.x, size.z) + 6f, root.transform);
        EchoShieldField field = root.AddComponent<EchoShieldField>();
        field.Configure(new[] { beam.GetComponent<Renderer>() }, lightRef, signal);
        return field;
    }

    static EchoConflictTrap CreateConflictTrap(string name, Vector3 position, Vector3 size, Transform parent, DoorController[] doorsToClose, DoorController[] doorsToOpen, PuzzleSignal signal)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = size;

        GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Cube);
        warning.name = "WarningRotor";
        warning.transform.SetParent(root.transform, false);
        warning.transform.localScale = new Vector3(size.x, 0.12f, 0.25f);
        Object.DestroyImmediate(warning.GetComponent<Collider>());
        warning.GetComponent<MeshRenderer>().sharedMaterial = _doorMat;

        EchoConflictTrap trap = root.AddComponent<EchoConflictTrap>();
        trap.Configure(doorsToClose, doorsToOpen, new[] { warning.transform }, signal);
        return trap;
    }

    static float GetPlatformSurfaceY(GameObject platform, Vector3 worldOffset)
    {
        if (platform == null)
            return worldOffset.y;

        Transform t = platform.transform;
        Vector3 world = t.position + new Vector3(worldOffset.x, 0f, worldOffset.z);
        return t.position.y + t.lossyScale.y * 0.5f + 0.08f;
    }

    static PressurePlate CreatePlateOnPlatform(string name, GameObject platform, Vector3 offsetOnPlatform, Transform parent, bool echoPlate)
    {
        Vector3 world = platform.transform.position + new Vector3(offsetOnPlatform.x, 0f, offsetOnPlatform.z);
        world.y = GetPlatformSurfaceY(platform, world);
        PressurePlate plate = CreatePlate(name, world, parent, echoPlate);
        return plate;
    }

    static PressurePlate CreatePlate(string name, Vector3 position, Transform parent, bool echoPlate = false)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        BoxCollider colliderRef = root.AddComponent<BoxCollider>();
        colliderRef.size = new Vector3(2f, 0.12f, 2f);
        colliderRef.isTrigger = true;

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3D Models/kenney_prototype-kit/Models/FBX format/button-floor-square.fbx");
        GameObject visual;
        if (prefab != null)
        {
            visual = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, -0.06f, 0f);
            visual.transform.localScale = new Vector3(2f, 1f, 2f);
            Collider[] cols = visual.GetComponentsInChildren<Collider>();
            foreach (var c in cols) Object.DestroyImmediate(c);
            ApplyMaterialOverride(visual, _plateMat);
        }
        else
        {
            visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(2f, 0.12f, 2f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<MeshRenderer>().sharedMaterial = _plateMat;
            visual.AddComponent<KenneyTiling>();
        }

        SpawnPointLight(name + "_Glow", position + new Vector3(0f, 1.2f, 0f),
            new Color(0.24f, 0.56f, 0.74f, 1f), 0.55f, 4f, root.transform);

        PressurePlate plate = root.AddComponent<PressurePlate>();
        if (echoPlate || name.Contains("Eco"))
            root.AddComponent<PressurePlateAlignment>();

        return plate;
    }

    static DoorController CreateDoor(string name, Vector3 position, Vector3 scale, Transform parent, PressurePlate[] plates)
    {
        // Enforce minimum door height to prevent jumping over
        float originalHeight = scale.y;
        if (scale.y < EchoesWorldMetrics.MinDoorHeight)
        {
            scale.y = EchoesWorldMetrics.MinDoorHeight;
            position.y += (EchoesWorldMetrics.MinDoorHeight - originalHeight) * 0.5f;
        }

        GameObject door = Instantiate3DModel(SciFiDoorDarkMetal, name, position, scale, Quaternion.identity, parent, _doorMat);
        door.AddComponent<KenneyTiling>();
        DoorController controller = door.AddComponent<DoorController>();
        controller.plates = plates;
        return controller;
    }

    static TimedMovingPlatform CreateBridge(string name, Vector3 anchorPosition, Vector3 inactiveLocal, Vector3 activeLocal, Vector3 scale, PressurePlate plate, Transform parent)
    {
        GameObject anchor = new GameObject(name + "_Anchor");
        anchor.transform.SetParent(parent, false);
        anchor.transform.position = anchorPosition;

        GameObject bridge = Instantiate3DModel(SciFiPlatformSimple, name, inactiveLocal, scale, Quaternion.identity, anchor.transform, _bridgeMat);
        bridge.AddComponent<KenneyTiling>();

        TimedMovingPlatform platform = bridge.AddComponent<TimedMovingPlatform>();
        platform.plate = plate;
        platform.inactiveLocal = inactiveLocal;
        platform.activeLocal = activeLocal;
        platform.travelSpeed = 6f;
        return platform;
    }

    static LevelExit CreateLevelExit(Vector3 position, Transform parent, string nextSceneName, string completionToast = "")
    {
        GameObject exitRoot = new GameObject("LevelExit_Area");
        exitRoot.transform.SetParent(parent, false);
        exitRoot.transform.position = position;

        // Bigger collider for reliable detection
        BoxCollider col = exitRoot.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(3.5f, 4f, 2.5f);
        col.center = new Vector3(0f, 0.5f, 0f);

        // Rigidbody for reliable trigger events
        Rigidbody rb = exitRoot.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        GameObject goalFocus = new GameObject("GoalFocus");
        goalFocus.transform.SetParent(exitRoot.transform, false);
        goalFocus.transform.localPosition = new Vector3(0f, 2f, 0f);

        LevelExit exitComponent = exitRoot.AddComponent<LevelExit>();
        exitComponent.loadNextBuildIndex = false;
        exitComponent.nextSceneName = nextSceneName;
        if (!string.IsNullOrEmpty(completionToast))
            SetSerializedValue(exitComponent, "completionToast", completionToast);

        // --- PORTAL PILLARS ---
        GameObject leftPillar = Instantiate3DModel(SciFiColumnSimple, "LeftPillar", new Vector3(-1.6f, 0.5f, 0f), new Vector3(0.4f, 4.5f, 0.4f), Quaternion.identity, exitRoot.transform, _bridgeMat);
        GameObject rightPillar = Instantiate3DModel(SciFiColumnSimple, "RightPillar", new Vector3(1.6f, 0.5f, 0f), new Vector3(0.4f, 4.5f, 0.4f), Quaternion.identity, exitRoot.transform, _bridgeMat);
        GameObject topBeam = Instantiate3DModel(SciFiPlatformSimple, "TopBeam", new Vector3(0f, 2.8f, 0f), new Vector3(3.6f, 0.4f, 0.4f), Quaternion.identity, exitRoot.transform, _goalMat);

        leftPillar.GetComponent<BoxCollider>().isTrigger = true;
        rightPillar.GetComponent<BoxCollider>().isTrigger = true;
        topBeam.GetComponent<BoxCollider>().isTrigger = true;

        // --- PORTAL SURFACE (glowing quad) ---
        GameObject portalSurface = GameObject.CreatePrimitive(PrimitiveType.Quad);
        portalSurface.name = "PortalSurface";
        portalSurface.transform.SetParent(exitRoot.transform, false);
        portalSurface.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        portalSurface.transform.localScale = new Vector3(2.8f, 4f, 1f);
        Object.DestroyImmediate(portalSurface.GetComponent<Collider>());

        Material portalMat = new Material(Shader.Find(EchoesUrpMaterials.LitShaderName));
        portalMat.color = new Color(0.25f, 0.45f, 0.9f, 0.12f);
        portalMat.SetFloat("_Surface", 1f);
        portalMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        portalMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        portalMat.SetInt("_ZWrite", 0);
        portalMat.DisableKeyword("_ALPHATEST_ON");
        portalMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        portalMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        portalMat.renderQueue = 3000;
        portalMat.EnableKeyword("_EMISSION");
        portalMat.SetColor("_EmissionColor", new Color(0.3f, 0.5f, 0.95f) * 2.2f);
        portalSurface.GetComponent<MeshRenderer>().sharedMaterial = portalMat;

        // --- SKY BEAM ---
        GameObject skyBeam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        skyBeam.name = "SkyBeam";
        skyBeam.transform.SetParent(exitRoot.transform, false);
        skyBeam.transform.localPosition = new Vector3(0f, 25f, 0f);
        skyBeam.transform.localScale = new Vector3(0.6f, 25f, 0.6f);
        Object.DestroyImmediate(skyBeam.GetComponent<Collider>());

        Material beamMat = new Material(Shader.Find(EchoesUrpMaterials.LitShaderName));
        beamMat.color = new Color(0.5f, 0.65f, 0.9f, 0.18f);
        beamMat.SetFloat("_Surface", 1f);
        beamMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        beamMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        beamMat.SetInt("_ZWrite", 0);
        beamMat.DisableKeyword("_ALPHATEST_ON");
        beamMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        beamMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        beamMat.renderQueue = 3000;
        beamMat.EnableKeyword("_EMISSION");
        beamMat.SetColor("_EmissionColor", new Color(0.4f, 0.55f, 0.85f) * 1.8f);
        skyBeam.GetComponent<MeshRenderer>().sharedMaterial = beamMat;

        // --- LIGHTS (portal glow) ---
        SpawnPointLight("ExitBeacon", position + new Vector3(0f, 5f, 0f),
            new Color(0.6f, 0.75f, 1f, 1f), 6f, 28f, exitRoot.transform);
        SpawnPointLight("ExitGlow", position + new Vector3(0f, 1.5f, 0f),
            new Color(0.4f, 0.6f, 0.95f, 1f), 4f, 14f, exitRoot.transform);
        SpawnPointLight("ExitRimL", position + new Vector3(-1.8f, 2f, 0f),
            new Color(0.3f, 0.5f, 1f, 1f), 2f, 6f, exitRoot.transform);
        SpawnPointLight("ExitRimR", position + new Vector3(1.8f, 2f, 0f),
            new Color(0.3f, 0.5f, 1f, 1f), 2f, 6f, exitRoot.transform);
        SpawnPointLight("ExitBase", position + new Vector3(0f, 0.3f, 0f),
            new Color(0.5f, 0.7f, 1f, 1f), 3f, 8f, exitRoot.transform);

        // --- PARTICLES (floating motes) ---
        GameObject motes = new GameObject("ExitParticles");
        motes.transform.SetParent(exitRoot.transform, false);
        motes.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        ParticleSystem ps = motes.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psr = motes.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = _goalMat;
        psr.renderMode = ParticleSystemRenderMode.Billboard;

        var psMain = ps.main;
        psMain.loop = true;
        psMain.playOnAwake = true;
        psMain.startLifetime = new ParticleSystem.MinMaxCurve(2f, 5f);
        psMain.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        psMain.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
        psMain.startColor = new ParticleSystem.MinMaxGradient(new Color(0.5f, 0.7f, 1f, 0.8f));
        psMain.maxParticles = 60;
        psMain.simulationSpace = ParticleSystemSimulationSpace.World;

        var psEmission = ps.emission;
        psEmission.rateOverTime = 15f;

        var psShape = ps.shape;
        psShape.shapeType = ParticleSystemShapeType.Box;
        psShape.scale = new Vector3(2.5f, 4f, 1.5f);

        var psVel = ps.velocityOverLifetime;
        psVel.enabled = true;
        psVel.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        psVel.y = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        psVel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var psCol = ps.colorOverLifetime;
        psCol.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.5f, 0.7f, 1f), 0f), new GradientColorKey(new Color(0.8f, 0.9f, 1f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.8f, 0.2f), new GradientAlphaKey(0.6f, 0.7f), new GradientAlphaKey(0f, 1f) });
        psCol.color = new ParticleSystem.MinMaxGradient(grad);

        return exitComponent;
    }

    static LevelGoal CreateLevelGoal(Transform parent, string objectiveText, string readyPrompt, string completionToast, LevelExit exit, params PressurePlate[] plates)
    {
        GameObject goalObject = new GameObject("LevelGoal");
        goalObject.transform.SetParent(parent, false);
        goalObject.transform.position = exit != null ? exit.transform.position : parent.position;

        for (int i = 0; i < plates.Length; i++)
            CreateGoalTrigger(goalObject.transform, plates[i], "Memoria " + (i + 1));

        LevelGoal goal = goalObject.AddComponent<LevelGoal>();
        SetSerializedValue(goal, "objectiveText", objectiveText);
        SetSerializedValue(goal, "readyPrompt", readyPrompt);
        SetSerializedValue(goal, "completionToast", completionToast);
        // CRITICAL: must be true so Awake() auto-discovers child GoalTriggers
        // SetSerializedValue cannot handle Object[] arrays, so manual array assignment fails
        SetSerializedValue(goal, "autoCollectChildTriggers", true);
        SetSerializedValue(goal, "requiredTriggerCount", plates.Length);
        // linkedExits fallback in LevelGoal.Awake uses FindObjectsOfType when null

        return goal;
    }

    static GoalTrigger CreateGoalTrigger(Transform parent, PressurePlate plate, string displayName)
    {
        GameObject triggerObject = new GameObject(displayName.Replace(" ", string.Empty) + "_Goal");
        triggerObject.transform.SetParent(parent, false);
        if (plate != null)
            triggerObject.transform.position = plate.transform.position + new Vector3(0f, 0.4f, 0f);

        GoalTrigger trigger = triggerObject.AddComponent<GoalTrigger>();
        SetSerializedValue(trigger, "displayName", displayName);
        SetSerializedValue(trigger, "pressurePlate", plate);
        SetSerializedValue(trigger, "usePlatePressedState", true);
        SetSerializedValue(trigger, "useDoorOpenState", false);
        SetSerializedValue(trigger, "accumulateOnce", true);
        return trigger;
    }

    static GoalTrigger CreateGoalTrigger(Transform parent, PuzzleSignal signal, string displayName)
    {
        GameObject triggerObject = new GameObject(displayName.Replace(" ", string.Empty) + "_Goal");
        triggerObject.transform.SetParent(parent, false);
        if (signal != null)
            triggerObject.transform.position = signal.transform.position;

        GoalTrigger trigger = triggerObject.AddComponent<GoalTrigger>();
        SetSerializedValue(trigger, "displayName", displayName);
        SetSerializedValue(trigger, "puzzleSignal", signal);
        SetSerializedValue(trigger, "usePlatePressedState", false);
        SetSerializedValue(trigger, "useDoorOpenState", false);
        SetSerializedValue(trigger, "usePuzzleSignalState", true);
        SetSerializedValue(trigger, "accumulateOnce", true);
        return trigger;
    }

    static void CreateTutorialTrigger(string name, Vector3 position, Vector3 size, string message, string hint, float duration, Transform parent)
    {
        GameObject triggerObject = new GameObject(name);
        triggerObject.transform.SetParent(parent, false);
        triggerObject.transform.position = position;

        BoxCollider colliderRef = triggerObject.AddComponent<BoxCollider>();
        colliderRef.isTrigger = true;
        colliderRef.size = size;

        TutorialTrigger trigger = triggerObject.AddComponent<TutorialTrigger>();
        SetSerializedValue(trigger, "message", message);
        SetSerializedValue(trigger, "hint", hint);
        SetSerializedValue(trigger, "displayDuration", duration);
        SetSerializedValue(trigger, "oneShot", true);
    }

    static void CreateHubPortal(Vector3 position, string sceneName, string displayName, string memoryLine, Transform mechanics, Transform visuals)
    {
        MakePlatform(displayName + "_Pedestal", position, new Vector3(3f, 0.5f, 3f), visuals, _bridgeMat);

        GameObject portalRoot = new GameObject(displayName + "_Portal");
        portalRoot.transform.SetParent(mechanics, false);
        portalRoot.transform.position = position + new Vector3(0f, 0.5f, 0f);
        portalRoot.transform.rotation = Quaternion.LookRotation((Vector3.zero - position).normalized, Vector3.up);

        GameObject left = Instantiate3DModel(SciFiColumnAstra, "LeftPillar", new Vector3(-1.1f, 2.2f, 0f), new Vector3(0.45f, 4.2f, 0.45f), Quaternion.identity, portalRoot.transform, _bridgeMat);
        GameObject right = Instantiate3DModel(SciFiColumnAstra, "RightPillar", new Vector3(1.1f, 2.2f, 0f), new Vector3(0.45f, 4.2f, 0.45f), Quaternion.identity, portalRoot.transform, _bridgeMat);
        GameObject top = Instantiate3DModel(SciFiPlatformSimple, "TopBeam", new Vector3(0f, 4.1f, 0f), new Vector3(2.8f, 0.35f, 0.45f), Quaternion.identity, portalRoot.transform, _bridgeMat);

        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glow.name = "PortalGlow";
        glow.transform.SetParent(portalRoot.transform, false);
        glow.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        glow.transform.localScale = new Vector3(2f, 3.2f, 0.15f);
        glow.GetComponent<MeshRenderer>().sharedMaterial = _goalMat;
        Object.DestroyImmediate(glow.GetComponent<BoxCollider>());

        BoxCollider trigger = portalRoot.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 2.2f, 0f);
        trigger.size = new Vector3(2.8f, 4.2f, 2.5f);

        Light light = SpawnPointLight(displayName + "_Light", position + new Vector3(0f, 3.2f, 0f), new Color(0.16f, 0.85f, 1f, 1f), 3.5f, 8f, portalRoot.transform);

        HubPortal portal = portalRoot.AddComponent<HubPortal>();
        SetSerializedValue(portal, "sceneName", sceneName);
        SetSerializedValue(portal, "displayName", displayName);
        SetSerializedValue(portal, "memoryLine", memoryLine);
        SetSerializedValue(portal, "portalLight", light);
    }

    static GameObject Instantiate3DModel(string modelPath, string name, Vector3 position, Vector3 scale, Quaternion rotation, Transform parent, Material materialOverride = null)
    {
        GameObject container = new GameObject(name);
        if (parent != null)
            container.transform.SetParent(parent, false);
        container.transform.localPosition = position;
        container.transform.localRotation = rotation;
        container.transform.localScale = scale;
        
        container.layer = GroundLayer;
        container.isStatic = true;

        BoxCollider box = container.AddComponent<BoxCollider>();
        box.center = Vector3.zero;
        box.size = Vector3.one;

        LevelKitPiece kitPiece = container.AddComponent<LevelKitPiece>();
        bool isWalkable = (modelPath.Contains("Platform") || modelPath.Contains("Ramp") || name.Contains("Platform") || name.Contains("Floor") || name.Contains("Ramp") || name.Contains("Bridge") || name.Contains("Catwalk") || name.Contains("Ledge") || name.Contains("Tower") || name.Contains("Chamber") || name.Contains("Plat") || name.Contains("Floor") || name.Contains("Elevator"))
            && !name.Contains("Beam") && !name.Contains("Pillar") && !name.Contains("Wall") && !name.Contains("Barrier") && !name.Contains("Door") && !name.Contains("Frame") && !name.Contains("Gate") && !name.Contains("Ceiling") && !name.Contains("Shadow");
        SetSerializedValue(kitPiece, "pieceId", name);
        SetSerializedValue(kitPiece, "role", isWalkable ? "WalkablePlatform" : "Prop");
        SetSerializedValue(kitPiece, "walkableSurface", isWalkable);
        SetSerializedValue(kitPiece, "cameraOccluder", isWalkable);
        SetSerializedValue(kitPiece, "requiresGameplayCollider", isWalkable);
        SetSerializedValue(kitPiece, "footprintSize", Vector3.one);
        SetSerializedValue(kitPiece, "clearanceSize", Vector3.one);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (prefab != null)
        {
            GameObject visual = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            visual.name = "VisualModel";
            visual.transform.SetParent(container.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            Collider[] childColliders = visual.GetComponentsInChildren<Collider>(true);
            foreach (var col in childColliders)
            {
                Object.DestroyImmediate(col);
            }

            if (materialOverride != null)
            {
                ApplyMaterialOverride(visual, materialOverride);
            }

            // Normalizar el modelo a la caja unitaria del contenedor para que el
            // 'scale' sea el footprint REAL en mundo (y coincida con el BoxCollider
            // size 1), evitando que modelos de tamaño nativo grande se solapen.
            // Ver EchoesModuleFactory.ComputeLocalBounds (robusto a rotación/escala).
            Bounds nativeBounds = EchoesModuleFactory.ComputeLocalBounds(visual.transform);
            Vector3 ns = nativeBounds.size;
            Vector3 fit = new Vector3(
                1f / Mathf.Max(1e-4f, ns.x),
                1f / Mathf.Max(1e-4f, ns.y),
                1f / Mathf.Max(1e-4f, ns.z));
            visual.transform.localScale = fit;
            visual.transform.localPosition = new Vector3(
                -nativeBounds.center.x * fit.x,
                -nativeBounds.center.y * fit.y,
                -nativeBounds.center.z * fit.z);
        }
        else
        {
            GameObject fallbackCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallbackCube.name = "FallbackVisual";
            fallbackCube.transform.SetParent(container.transform, false);
            fallbackCube.transform.localPosition = Vector3.zero;
            fallbackCube.transform.localRotation = Quaternion.identity;
            fallbackCube.transform.localScale = Vector3.one;
            Object.DestroyImmediate(fallbackCube.GetComponent<Collider>());
            if (materialOverride != null)
                fallbackCube.GetComponent<MeshRenderer>().sharedMaterial = materialOverride;
        }

        return container;
    }

    static void SpawnBarrierWall(string name, Vector3 position, Vector3 scale, Transform parent, Material material = null)
    {
        Vector3 wallScale = new Vector3(scale.x, Mathf.Max(scale.y, EchoesWorldMetrics.MinBarrierHeight), scale.z);
        Instantiate3DModel(SciFiWallBand, name, position, wallScale, Quaternion.identity, parent, material != null ? material : _doorMat);
    }

    static void SpawnKillZone(string name, Vector3 position, Vector3 size, Transform parent)
    {
        GameObject zone = new GameObject(name);
        zone.transform.SetParent(parent, false);
        zone.transform.position = position;

        BoxCollider trigger = zone.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        Vector3 killSize = new Vector3(size.x, Mathf.Max(size.y, EchoesWorldMetrics.MinBarrierHeight), size.z);
        trigger.size = killSize;

        zone.AddComponent<KillVolume>();

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(zone.transform, false);
        visual.transform.localScale = killSize;
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        Material hazardMat = new Material(Shader.Find(EchoesUrpMaterials.LitShaderName));
        hazardMat.color = new Color(1f, 0.12f, 0.06f, 0.5f);
        hazardMat.EnableKeyword("_EMISSION");
        hazardMat.SetColor("_EmissionColor", new Color(1.2f, 0.1f, 0.05f));
        visual.GetComponent<MeshRenderer>().sharedMaterial = hazardMat;
    }

    static GameObject MakePlatform(string name, Vector3 position, Vector3 scale, Transform parent, Material material)
    {
        string modelPath = SciFiPlatformMetal;
        if (name.Contains("Ramp") || name.Contains("Rampa"))
            modelPath = SciFiPlatformRamp;
        else if (name.Contains("Center") || name.Contains("Plate"))
            modelPath = SciFiPlatformCenter;
        else if (name.Contains("Simple"))
            modelPath = SciFiPlatformSimple;

        return Instantiate3DModel(modelPath, name, position, scale, Quaternion.identity, parent, material);
    }

    /// <summary>Open liminal frame: distant side pillars only. No ceiling, no closed box.</summary>
    static void SpawnLiminalHorizon(string prefix, Vector3 center, float width, float depth, Transform parent)
    {
        float pillarHeight = Mathf.Max(22f, width * 0.6f);
        float sideOffset = width * 0.5f + 28f;
        MakeVisualSilhouette(prefix + "_PillarL_Near", center + new Vector3(-sideOffset, pillarHeight * 0.5f, depth * 0.15f), new Vector3(0.35f, pillarHeight, 0.35f), parent);
        MakeVisualSilhouette(prefix + "_PillarR_Near", center + new Vector3(sideOffset, pillarHeight * 0.5f, depth * 0.15f), new Vector3(0.35f, pillarHeight, 0.35f), parent);
        MakeVisualSilhouette(prefix + "_PillarL_Far", center + new Vector3(-sideOffset * 1.15f, pillarHeight * 0.65f, depth * 0.55f), new Vector3(0.28f, pillarHeight * 1.2f, 0.28f), parent);
        MakeVisualSilhouette(prefix + "_PillarR_Far", center + new Vector3(sideOffset * 1.15f, pillarHeight * 0.65f, depth * 0.55f), new Vector3(0.28f, pillarHeight * 1.2f, 0.28f), parent);
    }

    /// <summary>Non-colliding distant brutalist skyline.</summary>
    static void SpawnDistantArchitecture(Vector3 center, float width, float depth, Transform parent)
    {
        Transform distantRoot = CreateRoot("--- DISTANT VISUALS ---");
        if (parent != null)
            distantRoot.SetParent(parent, false);

        float farZ = center.z + depth * 0.75f + 52f;
        for (int i = 0; i < 18; i++)
        {
            float t = i / 17f;
            float x = Mathf.Lerp(-width * 0.8f, width * 0.8f, t) + center.x;
            float z = farZ + Mathf.Sin(i * 2.1f) * 16f;
            float h = 30f + Mathf.PerlinNoise(i * 0.2f, 3f) * 55f;
            float w = 0.9f + Mathf.PerlinNoise(i, 1f) * 2.2f;
            MakeBrutalistBlock("SkyPillar_" + i, new Vector3(x, h * 0.5f, z), new Vector3(w, h, w), Quaternion.Euler(0f, i * 11f, 0f), distantRoot);
        }

        for (int i = 0; i < 5; i++)
        {
            Vector3 pos = center + new Vector3((i - 2) * width * 0.18f, 8f + i * 5f, farZ + i * 10f);
            MakeBrutalistBlock("SkyMass_" + i, pos, new Vector3(6f + i * 2f, 4f + i, 5f + i), Quaternion.Euler(i * 9f, i * 17f, i * 6f), distantRoot);
        }
    }

    static GameObject MakeVisualSilhouette(string name, Vector3 position, Vector3 scale, Transform parent)
    {
        Material targetMat = _archMat != null ? _archMat : _bridgeMat;
        GameObject silhouette = Instantiate3DModel(SciFiColumnLarge, name, position, scale, Quaternion.identity, parent, targetMat);
        
        Collider col = silhouette.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        
        MeshRenderer[] renderers = silhouette.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderers[i].receiveShadows = false;
        }
        return silhouette;
    }

    static void SetupAtmosphere(Color fogColor, float fogDensity, Color ambientColor)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = Mathf.Clamp(fogDensity, 0.002f, 0.04f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        RenderSettings.ambientIntensity = 0.85f;

        RenderSettings.skybox = null;
        RenderSettings.reflectionIntensity = 0f;

        GameObject atmosphere = new GameObject("AtmosphereController");
        AtmosphereController atmoController = atmosphere.AddComponent<AtmosphereController>();
        SetSerializedValue(atmoController, "enableGroundFog", true);
        SetSerializedValue(atmoController, "maxDustMotes", 18);
        SetSerializedValue(atmoController, "spawnVolume", new Vector3(30f, 8f, 30f));
    }

    static void SpawnLevelLightingSettings(Transform parent, Color fogColor, float fogDensity)
    {
        GameObject root = new GameObject("LevelLighting");
        root.transform.SetParent(parent, false);
        LevelLightingSettings settings = root.AddComponent<LevelLightingSettings>();
        settings.fogColor = fogColor;
        settings.fogDensity = Mathf.Clamp(fogDensity, 0.002f, 0.04f);
        settings.disableRuntimeFillLights = false;
    }

    static void SpawnDirectionalLight()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light lightRef = lightObject.AddComponent<Light>();
        lightRef.type = LightType.Directional;
        lightRef.color = new Color(0.6f, 0.62f, 0.58f, 1f);
        lightRef.intensity = 0.65f;
        lightRef.shadows = LightShadows.Hard;
        lightRef.shadowStrength = 1f;
        lightObject.transform.rotation = Quaternion.Euler(48f, -38f, 0f);
    }

    static Light SpawnPointLight(string name, Vector3 position, Color color, float intensity, float range, Transform parent)
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
        lightRef.shadows = LightShadows.None;
        return lightRef;
    }

    /// <summary>
    /// Aggressive short-range falloff for ceiling fluorescents and hard light pools.
    /// </summary>
    static Light SpawnHardLight(string name, Vector3 position, Color color, float intensity, Transform parent)
    {
        Light lightRef = SpawnPointLight(name, position, color, intensity, 4.5f, parent);
        lightRef.range = 4.5f;
        return lightRef;
    }

    static void SpawnAmbientParticles(Vector3 position, Vector3 boxScale)
    {
        GameObject particleObject = new GameObject("EnvironmentParticles");
        particleObject.transform.position = position;
        ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
        ParticleSystemRenderer rendererRef = particleObject.GetComponent<ParticleSystemRenderer>();
        rendererRef.sharedMaterial = _goalMat;

        var main = particleSystem.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.maxParticles = 80;

        var emission = particleSystem.emission;
        emission.rateOverTime = 9f;

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = boxScale;
    }

    static void SpawnSmokeVolume(string name, Vector3 position, Vector3 boxScale, Transform parent, float rateOverTime)
    {
        GameObject particleObject = new GameObject(name);
        if (parent != null)
            particleObject.transform.SetParent(parent, false);
        particleObject.transform.position = position;

        ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
        ParticleSystemRenderer rendererRef = particleObject.GetComponent<ParticleSystemRenderer>();
        rendererRef.sharedMaterial = LoadSmokeMaterial();
        rendererRef.renderMode = ParticleSystemRenderMode.Billboard;
        rendererRef.sortMode = ParticleSystemSortMode.Distance;

        // Subtle, gentle mist instead of heavy smoke
        float adjustedRate = rateOverTime * 0.4f; // Much less dense

        var main = particleSystem.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 16f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.06f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 1.8f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.55f, 0.6f, 0.72f, 0.04f),  // Cool blue-gray, very transparent
            new Color(0.65f, 0.68f, 0.78f, 0.07f));
        main.maxParticles = Mathf.RoundToInt(adjustedRate * 2.5f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particleSystem.emission;
        emission.rateOverTime = adjustedRate;

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = boxScale;

        var velocity = particleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.01f, 0.04f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);

        var noise = particleSystem.noise;
        noise.enabled = true;
        noise.strength = 0.08f;
        noise.frequency = 0.15f;

        var color = particleSystem.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.6f, 0.65f, 0.75f), 0f),
                new GradientColorKey(new Color(0.55f, 0.6f, 0.7f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.06f, 0.2f),
                new GradientAlphaKey(0.05f, 0.75f),
                new GradientAlphaKey(0f, 1f)
            });
        color.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    /// <summary>Decoraci├│n surrealista lejos del gameplay (anillo exterior + fondo en niebla).</summary>
    static void SpawnIntroDressing(Transform env, Vector3 puzzleAnchor)
    {
        SpawnSurrealBackdrop(env, puzzleAnchor, 12f, 20f);
    }

    static void SpawnSurrealBackdrop(Transform env, Vector3 puzzleAnchor, float playRadius, float playDepth)
    {
        Transform surrealRoot = CreateRoot("--- SURREAL ARCHITECTURE ---");
        surrealRoot.SetParent(env, false);

        float outerRing = playRadius + 32f;
        int pillarCount = 16;
        for (int i = 0; i < pillarCount; i++)
        {
            float angle = (i / (float)pillarCount) * Mathf.PI * 2f;
            float ring = outerRing + Mathf.PerlinNoise(i * 0.41f, 0.2f) * 10f;
            Vector3 basePos = puzzleAnchor + new Vector3(Mathf.Cos(angle) * ring, 0f, Mathf.Sin(angle) * ring * 0.55f);
            float height = 18f + Mathf.PerlinNoise(i * 0.17f, 1.9f) * 42f;
            float thickness = 0.3f + Mathf.PerlinNoise(i, 2.1f) * 0.45f;
            MakeBrutalistBlock("OuterPillar_" + i, basePos + Vector3.up * height * 0.5f, new Vector3(thickness, height, thickness), Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f), surrealRoot);
        }

        float fogZ = puzzleAnchor.z + playDepth + 42f;
        for (int i = 0; i < 5; i++)
        {
            float x = puzzleAnchor.x + (i - 2) * 14f;
            float y = 6f + i * 3.5f;
            float z = fogZ + i * 11f;
            MakeBrutalistBlock("FogSlab_" + i, new Vector3(x, y, z), new Vector3(5f + i, 0.2f, 4f), Quaternion.Euler(i * 6f, i * 11f, i * 4f), surrealRoot);
        }

        Vector3 farMonolith = puzzleAnchor + new Vector3(0f, 14f, fogZ + 28f);
        MakeBrutalistBlock("FarMonolith", farMonolith, new Vector3(4f, 32f, 3f), Quaternion.Euler(4f, 12f, 0f), surrealRoot);
        MakeBrutalistBlock("FarCantilever", farMonolith + new Vector3(12f, 8f, 6f), new Vector3(14f, 0.4f, 2.5f), Quaternion.Euler(0f, -28f, -38f), surrealRoot);

        for (int i = 0; i < 3; i++)
        {
            Vector3 archPos = puzzleAnchor + new Vector3((i - 1) * 22f, 12f + i * 5f, fogZ + 14f + i * 9f);
            MakeBrutalistBlock("FarArch_V_" + i, archPos, new Vector3(0.35f, 22f, 0.35f), Quaternion.identity, surrealRoot);
            MakeBrutalistBlock("FarArch_H_" + i, archPos + Vector3.up * 10f, new Vector3(9f + i * 2f, 0.3f, 0.45f), Quaternion.Euler(0f, 0f, 10f + i * 6f), surrealRoot);
        }
    }

    static GameObject MakeBrutalistBlock(string name, Vector3 position, Vector3 scale, Quaternion rotation, Transform parent)
    {
        GameObject block = MakeVisualSilhouette(name, position, scale, parent);
        block.transform.rotation = rotation;
        return block;
    }

    static void TrySpawnDecorPrefab(string name, string assetPath, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
            return;

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
            return;

        instance.name = name;
        if (parent != null)
            instance.transform.SetParent(parent, false);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale = scale;
    }

    static Material LoadSmokeMaterial()
    {
        Material smokeMaterial = AssetDatabase.LoadAssetAtPath<Material>(SmokeDarkMaterialPath);
        return smokeMaterial != null ? smokeMaterial : (_echoMat != null ? _echoMat : _goalMat);
    }

    static EchoDisintegrationZone SpawnDisintegrationZone(string name, Vector3 position, Vector3 scale, Transform emitter, Transform parent, Material mat)
    {
        GameObject obj = MakePlatform(name, position, scale, parent, mat);
        obj.isStatic = false;

        BoxCollider box = obj.GetComponent<BoxCollider>();
        if (box != null)
            box.isTrigger = true;

        EchoDisintegrationZone zone = obj.AddComponent<EchoDisintegrationZone>();
        zone.lightEmitter = emitter;
        zone.zoneCollider = box;
        zone.hazardRenderer = obj.GetComponentInChildren<Renderer>();
        zone.activeColor = mat.color;
        
        return zone;
    }

    static Transform CreateRoot(string name)
    {
        GameObject root = new GameObject(name);
        return root.transform;
    }

    static void EnsureFolders()
    {
        EnsureFolder(MaterialRoot);
        EnsureFolder(PrefabRoot);
        EnsureFolder(SceneRoot);
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    static void EnsureMaterials()
    {
        Shader standardShader = Shader.Find(EchoesUrpMaterials.LitShaderName);
        if (standardShader == null)
        {
            Debug.LogError("[Echoes Production] Could not find Standard shader!");
            return;
        }

        // Materiales flat URP (sin texturas 2K/4K) — estética minimalista de DESIGN.md.
        _archMat = GetOrCreateMaterial("Mat_Architecture", HexColor("B0B5BC"));
        SetupMaterialTextures(_archMat, 
            "Assets/Materials/Metal054B_2K-JPG/Metal054B_2K-JPG_Color.jpg", 
            "Assets/Materials/Metal054B_2K-JPG/Metal054B_2K-JPG_NormalGL.jpg", 
            "", 0.4f, 0.6f, 0.5f, new Vector2(4f, 4f));

        _floorMat = GetOrCreateMaterial("Mat_Floor", HexColor("DCDCDC"));
        SetupMaterialTextures(_floorMat, 
            "Assets/Materials/Concrete047A_2K-JPG (1)/Concrete047A_2K-JPG_Color.jpg", 
            "Assets/Materials/Concrete047A_2K-JPG (1)/Concrete047A_2K-JPG_NormalGL.jpg", 
            "Assets/Materials/Concrete047A_2K-JPG (1)/Concrete047A_2K-JPG_AmbientOcclusion.jpg", 
            0.8f, 0.1f, 0.4f, new Vector2(5f, 5f));

        _bridgeMat = GetOrCreateMaterial("Mat_Bridge", HexColor("E5E7EB"));
        SetupMaterialTextures(_bridgeMat, 
            "Assets/Materials/Concrete047A_2K-JPG (1)/Concrete047A_2K-JPG_Color.jpg", 
            "", "", 0.5f, 0.1f, 0.5f, new Vector2(3f, 3f));

        _plateMat = GetOrCreateEmissiveFlatMaterial("Mat_Plate",
            HexColor("3A4250"), HexColorEmissive("4FC3E8", 1.2f));

        _doorMat = GetOrCreateEmissiveFlatMaterial("Mat_Door",
            HexColor("4A2A2D"), HexColorEmissive("B23A3A", 0.6f));

        _goalMat = GetOrCreateEmissiveFlatMaterial("Mat_Exit",
            HexColor("E8B262"), HexColorEmissive("E8B262", 2.0f));

        _playerMat = GetOrCreateFlatMaterial("Mat_Player", HexColor("E0E0DB"));

        _echoMat = GetOrCreateTransparentMaterial("Mat_Echo",
            new Color(0.35f, 0.8f, 0.95f, 0.45f), true);

        _memoryMat = GetOrCreateEmissiveFlatMaterial("Mat_Memory",
            HexColor("9E6E3C"), HexColorEmissive("E8B262", 0.9f));

        _fluorescentMat = GetOrCreateEmissiveFlatMaterial("Mat_Fluorescent",
            HexColor("D2DBBC"), HexColorEmissive("D2DBBC", 3.0f));

        _wallRoseMat = GetOrCreateFlatMaterial("Mat_WallRose", HexColor("C87A7A"));
        SetupMaterialTextures(_wallRoseMat, 
            "Assets/Materials/Concrete047A_2K-JPG (1)/Concrete047A_2K-JPG_Color.jpg", 
            "", "", 0.2f, 0.05f, 0.3f, new Vector2(3f, 3f));

        _wallTealMat = GetOrCreateFlatMaterial("Mat_WallTeal", HexColor("4A7A82"));
        SetupMaterialTextures(_wallTealMat, 
            "Assets/Materials/Concrete047A_2K-JPG (1)/Concrete047A_2K-JPG_Color.jpg", 
            "", "", 0.2f, 0.05f, 0.3f, new Vector2(3f, 3f));

        _wallMustardMat = GetOrCreateFlatMaterial("Mat_WallMustard", HexColor("DDAA55"));
        SetupMaterialTextures(_wallMustardMat, 
            "Assets/Materials/Concrete047A_2K-JPG (1)/Concrete047A_2K-JPG_Color.jpg", 
            "", "", 0.2f, 0.05f, 0.3f, new Vector2(3f, 3f));

        _wallSageMat = GetOrCreateFlatMaterial("Mat_WallSage", HexColor("95A690"));
        SetupMaterialTextures(_wallSageMat, 
            "Assets/Materials/Concrete047A_2K-JPG (1)/Concrete047A_2K-JPG_Color.jpg", 
            "", "", 0.2f, 0.05f, 0.3f, new Vector2(3f, 3f));

        if (_goalMat != null)
            _goalMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        if (_echoMat != null)
            _echoMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        if (_memoryMat != null)
            _memoryMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        if (_fluorescentMat != null)
            _fluorescentMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
    }

    // Estética minimalista "void" (ver DESIGN.md): sin texturas PBR 2K/4K.
    // Mantiene solo la respuesta de superficie flat (metallic/smoothness) y
    // limpia cualquier mapa que un rebuild anterior hubiera dejado asignado.
    // Los parámetros de rutas/tiling/bumpScale se ignoran a propósito para no
    // tener que tocar todos los call sites.
    static void SetupMaterialTextures(
        Material mat,
        string albedoPath,
        string normalPath,
        string aoPath,
        float bumpScale,
        float metallic,
        float smoothness,
        Vector2? tiling)
    {
        if (mat == null) return;

        mat.SetTexture("_BaseMap", null);
        mat.SetTexture("_BumpMap", null);
        mat.SetTexture("_OcclusionMap", null);
        mat.DisableKeyword("_NORMALMAP");

        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
    }

    static Color HexColor(string hex, float alpha = 1f)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color c))
        {
            c.a = alpha;
            return c;
        }
        return new Color(1, 0, 1, alpha); // fallback
    }

    static Color HexColorEmissive(string hex, float intensity)
    {
        return HexColor(hex) * intensity;
    }

    static Material GetOrCreateMaterial(string name, Color color, bool emissive = false, Texture2D tex = null)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            material.color = color;
            if (tex != null) material.mainTexture = tex;
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 0.8f);
            }
            return material;
        }

        material = new Material(Shader.Find(EchoesUrpMaterials.LitShaderName));
        material.color = color;
        if (tex != null) material.mainTexture = tex;
        if (emissive)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.8f);
        }

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    /// <summary>
    /// Flat material without specular or environment reflections.
    /// </summary>
    static Material GetOrCreateFlatMaterial(string name, Color color)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find(EchoesUrpMaterials.LitShaderName));
            AssetDatabase.CreateAsset(material, path);
        }

        ResetMaterialToFlatBase(material, color);
        return material;
    }

    /// <summary>
    /// Flat emissive material for plates, doors, exits, and memory props.
    /// </summary>
    static Material GetOrCreateEmissiveFlatMaterial(string name, Color baseColor, Color emissionColor)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find(EchoesUrpMaterials.LitShaderName));
            AssetDatabase.CreateAsset(material, path);
        }

        ResetMaterialToFlatBase(material, baseColor);
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emissionColor);
        return material;
    }

    static void ResetMaterialToFlatBase(Material material, Color color)
    {
        if (material == null) return;

        material.color = color;
        material.mainTexture = null;
        material.SetTexture("_BumpMap", null);
        material.SetTexture("_OcclusionMap", null);
        material.DisableKeyword("_NORMALMAP");
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Smoothness", 0.05f);
    }

    static Material GetOrCreateTransparentMaterial(string name, Color color, bool emissive)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            material.color = color;
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.4f);
            }
            return material;
        }

        material = new Material(Shader.Find(EchoesUrpMaterials.LitShaderName));
        material.color = color;
        material.SetFloat("_Surface", 1f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        if (emissive)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.4f);
        }

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    static Material GetOrCreateEmissiveMaterial(string name, Color color, Color emissionColor, Texture2D tex = null)
    {
        string path = Path.Combine(MaterialRoot, name + ".mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            material.color = color;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emissionColor);
            if (tex != null) material.mainTexture = tex;
            return material;
        }

        material = new Material(Shader.Find(EchoesUrpMaterials.LitShaderName));
        material.color = color;
        if (tex != null) material.mainTexture = tex;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emissionColor);

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    static void EnsureAnimatorController()
    {
        SetupPlayerAnimator.Setup();
    }

    static void EnsureEchoPrefab()
    {
        GameObject root = new GameObject("EchoPrefab");
        root.tag = "Echo";

        Rigidbody body = root.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        CharacterController controller = root.AddComponent<CharacterController>();
        controller.height = 2.2f;
        controller.radius = 0.36f;
        controller.center = new Vector3(0f, 1.1f, 0f);
        controller.skinWidth = 0.08f;

        root.AddComponent<EchoPlayback>();
        root.AddComponent<EchoSpectralTrail>();
        root.AddComponent<EchoTemporalVisual>();
        root.AddComponent<PlayerLocomotionAnimator>();
        root.AddComponent<PlayerAnimationRuntimeBootstrap>();
        root.AddComponent<CharacterPush>();

        GameObject visualRoot = new GameObject("Visual");
        visualRoot.transform.SetParent(root.transform, false);
        CreateCapsuleVisual(visualRoot.transform, true);

        var echoVisualType = System.Type.GetType("EchoVisualStateController");
        if (echoVisualType != null)
            root.AddComponent(echoVisualType);

        PrefabUtility.SaveAsPrefabAsset(root, EchoPrefabPath);
        Object.DestroyImmediate(root);
    }

    static GameObject CreateCapsuleVisual(Transform parent, bool useEchoMaterial)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3D Models/lowpoly-character-freerigged-/source/LowPolyCharacterModel/FBX/LowPolyCharacter.fbx");
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

            SkinnedMeshRenderer[] renderers = visual.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var r in renderers)
            {
                // Si es Player, mantener materiales originales. Si es Eco, usar el material transparente violeta.
                if (useEchoMaterial)
                {
                    Material[] mats = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < mats.Length; i++)
                        mats[i] = _echoMat;
                    r.sharedMaterials = mats;
                }
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
            visual.GetComponent<MeshRenderer>().sharedMaterial = useEchoMaterial ? _echoMat : _playerMat;
        }

        return visual;
    }

    static void SetSerializedValue(Component component, string propertyName, object value)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            return;

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

    static void SaveScene(Scene scene, string sceneName)
    {
        EditorSceneManager.SaveScene(scene, $"{SceneRoot}/{sceneName}.unity");
    }

    static void UpdateBuildSettings()
    {
        string[] scenePaths =
        {
            $"{SceneRoot}/MainMenu.unity",
            $"{SceneRoot}/Level_01.unity",
            $"{SceneRoot}/Level_02.unity",
            $"{SceneRoot}/Level_03.unity",
            $"{SceneRoot}/Level_04.unity",
            $"{SceneRoot}/Level_05.unity",
            $"{SceneRoot}/Level_06.unity",
            $"{SceneRoot}/Level_07.unity",
            $"{SceneRoot}/Level_08.unity",
            $"{SceneRoot}/Level_09.unity",
            $"{SceneRoot}/Level_10.unity",
            $"{SceneRoot}/Level_11.unity",
            $"{SceneRoot}/Level_12.unity",
            $"{SceneRoot}/Level_13.unity",
            $"{SceneRoot}/Level_14.unity",
            $"{SceneRoot}/Level_15.unity"
        };

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        for (int i = 0; i < scenePaths.Length; i++)
            scenes.Add(new EditorBuildSettingsScene(scenePaths[i], true));

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static Avatar LoadAvatarFromCharacterModel()
    {
        const string modelPath = "Assets/3D Models/lowpoly-character-freerigged-/source/LowPolyCharacterModel/FBX/LowPolyCharacter.fbx";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is Avatar avatar && avatar.isValid)
                return avatar;
        }

        return null;
    }

    static AnimationClip LoadClipFromFBX(string path)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        if (assets != null)
        {
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip && !clip.name.Contains("__preview__"))
                    return clip;
            }
        }
        return null;
    }

    static void ApplyMaterialOverride(GameObject obj, Material mat)
    {
        if (obj == null || mat == null) return;
        MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].sharedMaterial = mat;
        }
    }
}
