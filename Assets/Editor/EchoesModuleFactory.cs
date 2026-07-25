using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class EchoesModuleFactory
{
    // --- Asset resolution: búsqueda dinámica excluyendo el kit sci-fi ---

    /// <summary>
    /// Busca un asset por término de nombre dentro del proyecto y devuelve su
    /// ruta real, o null si no existe. Instantiate3DModel ya maneja el caso
    /// null con un fallback de cubo procedural — por eso es seguro usar esto
    /// en lugar de rutas hardcodeadas al kit sci-fi.
    /// </summary>
    private static string ResolveAssetPath(string searchTerm, string excludeTerm = null)
    {
        string[] guids = AssetDatabase.FindAssets("t:Model " + searchTerm);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (excludeTerm != null && path.Contains(excludeTerm)) continue;
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(fileName, searchTerm, System.StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }
        }
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (excludeTerm != null && path.Contains(excludeTerm)) continue;
            return path;
        }
        return null;
    }

    // Módulos escolares mapeados exactamente a las piezas del Kenney Furniture Kit del manifest real
    private static string SchoolFloorModule  { get { return ResolveAssetPath("floorFull", "SciFi"); } }
    private static string SchoolWallModule   { get { return ResolveAssetPath("wall", "SciFi"); } }
    private static string SchoolDoorModule   { get { return ResolveAssetPath("wallDoorway", "SciFi"); } }
    private static string SchoolColumnModule { get { return ResolveAssetPath("wallCorner", "SciFi"); } }
    private static string SchoolDeskModule   { get { return ResolveAssetPath("desk", "SciFi"); } }
    private static string SchoolLockerModule { get { return ResolveAssetPath("bookcaseClosed", "SciFi"); } }
    private static string SchoolShelfModule  { get { return ResolveAssetPath("bookcaseOpen", "SciFi"); } }
    private static string SchoolChairModule  { get { return ResolveAssetPath("chairDesk", "SciFi") ?? ResolveAssetPath("chair", "SciFi"); } }
    private static string SchoolStairsModule { get { return ResolveAssetPath("stairs", "SciFi"); } }
    private static string SchoolToiletModule { get { return ResolveAssetPath("toilet", "SciFi"); } }
    private static string SchoolSinkModule   { get { return ResolveAssetPath("bathroomSink", "SciFi"); } }
    private static string SchoolMirrorModule { get { return ResolveAssetPath("bathroomMirror", "SciFi"); } }
    private static string SchoolDeadTreeModule { get { return ResolveAssetPath("DeadTree_3", "SciFi") ?? ResolveAssetPath("DeadTree_1", "SciFi"); } }
    private static string SchoolFenceModule  { get { return ResolveAssetPath("FencePiece", "SciFi") ?? ResolveAssetPath("fence", "SciFi"); } }

    private const int GroundLayer = 6;

    public static GameObject BuildModule(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        GameObject obj = null;
        Transform parent = IsMechanical(placement.type) ? mechParent : envParent;

        switch (placement.type)
        {
            case ModuleType.StandardPlatform:
                obj = MakePlatform(placement.name, placement.position, placement.scale, parent, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);
                break;
            case ModuleType.BridgePlatform:
                obj = MakePlatform(placement.name, placement.position, placement.scale, parent, EchoesMaterialLibrary.BridgeMat, SchoolFloorModule);
                break;
            case ModuleType.RampPlatform:
                obj = MakePlatform(placement.name, placement.position, placement.scale, parent, EchoesMaterialLibrary.BridgeMat, SchoolFloorModule);
                break;
            case ModuleType.BarrierWall:
                obj = MakeBarrierWall(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.PressurePlate:
                obj = MakePressurePlate(placement.name, placement.position, parent);
                break;
            case ModuleType.Door:
                obj = MakeDoor(placement.name, placement.position, placement.scale, parent, placement.targetSignals);
                break;
            case ModuleType.MovingPlatform:
                obj = MakeMovingPlatform(placement.name, placement.position, placement.scale, parent, placement.targetSignals, placement.customData);
                break;
            case ModuleType.LevelExit:
                obj = MakeLevelExit(placement.name, placement.position, parent, placement.customData);
                break;
            case ModuleType.TutorialTrigger:
                obj = MakeTutorialTrigger(placement.name, placement.position, placement.scale, parent, placement.customData);
                break;
            case ModuleType.PointLight:
                obj = MakePointLight(placement.name, placement.position, parent, placement.customData);
                break;
            case ModuleType.AmbientParticles:
                obj = MakeAmbientParticles(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.DistantArchitecture:
                obj = MakeDistantArchitecture(placement.name, placement.position, parent, placement.customData);
                break;
            case ModuleType.LevelGoal:
                obj = MakeLevelGoal(placement.name, placement.position, parent, placement.customData, placement.targetSignals);
                break;
            case ModuleType.LevelRuntime:
                obj = MakeLevelRuntime(placement.name, parent, placement.customData);
                break;
            case ModuleType.PuzzleSignal:
                obj = MakePuzzleSignal(placement.name, parent, placement.customData);
                break;
            case ModuleType.PuzzleCondition:
                obj = MakePuzzleCondition(placement.name, parent, placement.customData);
                break;
            case ModuleType.HazardField:
                obj = MakeHazardField(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.ConflictTrap:
                obj = MakeConflictTrap(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.MomentumRelay:
                obj = MakeMomentumRelay(placement.name, placement.position, placement.scale, parent, placement.customData);
                break;
            case ModuleType.MotorPlatform:
                obj = MakeMotorPlatform(placement.name, placement.position, placement.scale, parent, placement.customData);
                break;

            // Phase 3 Architecture (21-30)
            case ModuleType.ObservationChamber:
                obj = MakeObservationChamber(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.TemporalBridge:
                obj = MakeTemporalBridge(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.PerspectiveAnchor:
                obj = MakePerspectiveAnchor(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.MemoryCorridor:
                obj = MakeMemoryCorridor(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.ParadoxArena:
                obj = MakeParadoxArena(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.ErosionVault:
                obj = MakeErosionVault(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.ResonanceChamber:
                obj = MakeResonanceChamber(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.LiminalThreshold:
                obj = MakeLiminalThreshold(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.ChronologicalSpire:
                obj = MakeChronologicalSpire(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.VoidGallery:
                obj = MakeVoidGallery(placement.name, placement.position, placement.scale, parent);
                break;

            // VOCABULARIO ESCOLAR (31-46)
            case ModuleType.SchoolHall:
                obj = MakeSchoolHall(placement, envParent, mechParent); break;
            case ModuleType.SchoolCorridor:
                obj = MakeSchoolCorridor(placement, envParent, mechParent); break;
            case ModuleType.SchoolClassroom:
                obj = MakeSchoolClassroom(placement, envParent, mechParent); break;
            case ModuleType.SchoolStairwell:
                obj = MakeSchoolStairwell(placement, envParent, mechParent); break;
            case ModuleType.SchoolBathroom:
                obj = MakeSchoolBathroom(placement, envParent, mechParent); break;
            case ModuleType.SchoolStaffRoom:
                obj = MakeSchoolStaffRoom(placement, envParent, mechParent); break;
            case ModuleType.SchoolLibrary:
                obj = MakeSchoolLibrary(placement, envParent, mechParent); break;
            case ModuleType.SchoolCourtyard:
                obj = MakeSchoolCourtyard(placement, envParent, mechParent); break;
            case ModuleType.SchoolGym:
                obj = MakeSchoolGym(placement, envParent, mechParent); break;
            case ModuleType.SchoolLab:
                obj = MakeSchoolLab(placement, envParent, mechParent); break;
            case ModuleType.SchoolMaintenanceCorridor:
                obj = MakeSchoolMaintenanceCorridor(placement, envParent, mechParent); break;
            case ModuleType.SchoolEmergencyCorridor:
                obj = MakeSchoolEmergencyCorridor(placement, envParent, mechParent); break;
            case ModuleType.SchoolLyraClassroom:
                obj = MakeSchoolLyraClassroom(placement, envParent, mechParent); break;
            case ModuleType.SchoolOffice:
                obj = MakeSchoolOffice(placement, envParent, mechParent); break;
            case ModuleType.SchoolLiminalClassroom:
                obj = MakeSchoolLiminalClassroom(placement, envParent, mechParent); break;
            case ModuleType.TransitionSpace:
                obj = MakeTransitionSpace(placement, envParent, mechParent); break;
        }

        if (obj != null)
        {
            obj.transform.localRotation = Quaternion.Euler(placement.rotation);
            SetupCollidersRecursive(obj);
        }

        return obj;
    }

    private static bool IsMechanical(ModuleType type)
    {
        switch (type)
        {
            case ModuleType.SchoolHall:
            case ModuleType.SchoolCorridor:
            case ModuleType.SchoolClassroom:
            case ModuleType.SchoolStairwell:
            case ModuleType.SchoolBathroom:
            case ModuleType.SchoolStaffRoom:
            case ModuleType.SchoolLibrary:
            case ModuleType.SchoolCourtyard:
            case ModuleType.SchoolGym:
            case ModuleType.SchoolLab:
            case ModuleType.SchoolMaintenanceCorridor:
            case ModuleType.SchoolEmergencyCorridor:
            case ModuleType.SchoolLyraClassroom:
            case ModuleType.SchoolOffice:
            case ModuleType.SchoolLiminalClassroom:
            case ModuleType.TransitionSpace:
                return false;

            default:
                return type == ModuleType.PressurePlate || 
                       type == ModuleType.Door || 
                       type == ModuleType.LevelExit || 
                       type == ModuleType.LevelGoal || 
                       type == ModuleType.LevelRuntime ||
                       type == ModuleType.TutorialTrigger ||
                       type == ModuleType.MovingPlatform ||
                       type == ModuleType.PuzzleSignal ||
                       type == ModuleType.PuzzleCondition ||
                       type == ModuleType.HazardField ||
                       type == ModuleType.ConflictTrap ||
                       type == ModuleType.MomentumRelay ||
                       type == ModuleType.MotorPlatform;
        }
    }

    // --- FACTORY METHOD IMPLEMENTATIONS ---

    private static GameObject MakePlatform(string name, Vector3 pos, Vector3 scale, Transform parent, Material mat, string modelPath)
    {
        GameObject obj = Instantiate3DModel(modelPath, name, pos, scale, Quaternion.identity, parent, mat);
        if (obj != null) obj.AddComponent<KenneyTiling>();
        return obj;
    }

    private static GameObject MakeMovingPlatform(string name, Vector3 pos, Vector3 scale, Transform parent, string[] targetSignals, string customData)
    {
        GameObject anchor = new GameObject(name + "_Anchor");
        if (parent != null) anchor.transform.SetParent(parent, false);
        anchor.transform.position = pos;

        Vector3 inactiveLocal = Vector3.zero;
        Vector3 activeLocal = new Vector3(0f, 6f, 0f);
        float speed = 6f;

        if (!string.IsNullOrEmpty(customData))
        {
            var parts = customData.Split('|');
            if (parts.Length > 0) inactiveLocal = ParseVector3(parts[0], Vector3.zero);
            if (parts.Length > 1) activeLocal = ParseVector3(parts[1], new Vector3(0f, 6f, 0f));
            if (parts.Length > 2) float.TryParse(parts[2], out speed);
        }

        GameObject bridge = Instantiate3DModel(SchoolFloorModule, name, inactiveLocal, scale, Quaternion.identity, anchor.transform, EchoesMaterialLibrary.BridgeMat);
        bridge.AddComponent<KenneyTiling>();

        TimedMovingPlatform platform = bridge.AddComponent<TimedMovingPlatform>();
        platform.inactiveLocal = inactiveLocal;
        platform.activeLocal = activeLocal;
        platform.travelSpeed = speed;

        return anchor;
    }

    private static Vector3 ParseVector3(string s, Vector3 defaultValue)
    {
        var parts = s.Split(',');
        if (parts.Length == 3)
        {
            float x, y, z;
            if (float.TryParse(parts[0], out x) && float.TryParse(parts[1], out y) && float.TryParse(parts[2], out z))
            {
                return new Vector3(x, y, z);
            }
        }
        return defaultValue;
    }

    private static GameObject MakeBarrierWall(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        Vector3 wallScale = new Vector3(scale.x, Mathf.Max(scale.y, EchoesWorldMetrics.MinBarrierHeight), scale.z);
        GameObject obj = Instantiate3DModel(SchoolWallModule, name, pos, wallScale, Quaternion.identity, parent, EchoesMaterialLibrary.DoorMat);
        if (obj != null) obj.AddComponent<KenneyTiling>();
        return obj;
    }

    private static GameObject MakePressurePlate(string name, Vector3 pos, Transform parent)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(2f, 0.12f, 2f);
        col.isTrigger = true;

        string plateModel = ResolveAssetPath("rugDoormat", "SciFi");
        GameObject visual = Instantiate3DModel(plateModel, "Visual", new Vector3(0f, 0.01f, 0f), new Vector3(1.6f, 1f, 1.2f), Quaternion.identity, root.transform, EchoesMaterialLibrary.PlateMat);

        EchoesLevelShell.SpawnPointLight(name + "_Glow", pos + new Vector3(0f, 1.2f, 0f), new Color(0.24f, 0.56f, 0.74f, 1f), 0.55f, 4f, root.transform);

        PressurePlate plate = root.AddComponent<PressurePlate>();
        if (name.Contains("Eco"))
            root.AddComponent<PressurePlateAlignment>();

        return root;
    }

    private static GameObject MakeDoor(string name, Vector3 pos, Vector3 scale, Transform parent, string[] targetSignals)
    {
        float originalHeight = scale.y;
        if (scale.y < EchoesWorldMetrics.MinDoorHeight)
        {
            scale.y = EchoesWorldMetrics.MinDoorHeight;
            pos.y += (EchoesWorldMetrics.MinDoorHeight - originalHeight) * 0.5f;
        }

        GameObject door = Instantiate3DModel(SchoolDoorModule, name, pos, scale, Quaternion.identity, parent, EchoesMaterialLibrary.DoorMat);
        if (door != null)
        {
            door.AddComponent<KenneyTiling>();
            DoorController controller = door.AddComponent<DoorController>();
            controller.latchOpen = false;
            
            // We will wire connections later after all modules are placed
        }
        return door;
    }

    private static GameObject MakeLevelExit(string name, Vector3 pos, Transform parent, string customData)
    {
        GameObject exitRoot = new GameObject("LevelExit_Area");
        exitRoot.transform.SetParent(parent, false);
        exitRoot.transform.position = pos;

        BoxCollider col = exitRoot.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(3.5f, 4f, 3.5f);
        col.center = new Vector3(0f, 0.5f, 0f);

        Rigidbody rb = exitRoot.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        GameObject goalFocus = new GameObject("GoalFocus");
        goalFocus.transform.SetParent(exitRoot.transform, false);
        goalFocus.transform.localPosition = new Vector3(0f, 1f, 0f);

        LevelExit exitComponent = exitRoot.AddComponent<LevelExit>();
        exitComponent.loadNextBuildIndex = false;
        exitComponent.nextSceneName = string.IsNullOrEmpty(customData) ? "MainMenu" : customData;

        // Meta del Juego: El pupitre de Lyra solitario
        GameObject lyraDesk = Instantiate3DModel(SchoolDeskModule, "LyraDesk", new Vector3(0f, 0.1f, 0f), new Vector3(1.3f, 1f, 0.8f), Quaternion.identity, exitRoot.transform, EchoesMaterialLibrary.MemoryMat);
        
        // Silla vacía
        Instantiate3DModel(SchoolChairModule, "LyraChair", new Vector3(0f, 0.1f, -0.7f), new Vector3(0.9f, 0.9f, 0.9f), Quaternion.Euler(0f, 180f, 0f), exitRoot.transform, EchoesMaterialLibrary.ArchMat);

        // Rayo de luz vertical cálida/dorada dura que corta la niebla (Cylinder translúcido)
        GameObject skyBeam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        skyBeam.name = "SkyBeam";
        skyBeam.transform.SetParent(exitRoot.transform, false);
        skyBeam.transform.localPosition = new Vector3(0f, 25f, 0f);
        skyBeam.transform.localScale = new Vector3(1.5f, 25f, 1.5f);
        Object.DestroyImmediate(skyBeam.GetComponent<Collider>());

        Material beamMat = new Material(Shader.Find(EchoesUrpMaterials.LitShaderName));
        beamMat.color = new Color(1.0f, 0.85f, 0.6f, 0.15f);
        SetupTransparentMaterial(beamMat);
        beamMat.EnableKeyword("_EMISSION");
        beamMat.SetColor("_EmissionColor", new Color(1.0f, 0.8f, 0.5f) * 2.0f);
        skyBeam.GetComponent<MeshRenderer>().sharedMaterial = beamMat;

        // Beacons luminosos cálidos del final del nivel
        EchoesLevelShell.SpawnPointLight("ExitBeacon", pos + new Vector3(0f, 5f, 0f), new Color(1.0f, 0.8f, 0.4f, 1f), 6f, 28f, exitRoot.transform, LightmapBakeType.Baked, LightShadows.Soft);
        EchoesLevelShell.SpawnPointLight("ExitGlow", pos + new Vector3(0f, 1.5f, 0f), new Color(1.0f, 0.75f, 0.35f, 1f), 4f, 14f, exitRoot.transform, LightmapBakeType.Baked, LightShadows.Soft);

        return exitRoot;
    }

    private static GameObject MakeTutorialTrigger(string name, Vector3 pos, Vector3 scale, Transform parent, string customData)
    {
        GameObject triggerObject = new GameObject(name);
        triggerObject.transform.SetParent(parent, false);
        triggerObject.transform.position = pos;

        BoxCollider col = triggerObject.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = scale;

        TutorialTrigger trigger = triggerObject.AddComponent<TutorialTrigger>();
        
        string title = name;
        string desc = customData;
        if (!string.IsNullOrEmpty(customData) && customData.Contains("|"))
        {
            var split = customData.Split('|');
            title = split[0];
            desc = split[1];
        }

        SetSerializedValue(trigger, "messageTitle", title);
        SetSerializedValue(trigger, "messageContent", desc);
        SetSerializedValue(trigger, "duration", 10f);

        return triggerObject;
    }

    private static GameObject MakePointLight(string name, Vector3 pos, Transform parent, string customData)
    {
        Color color = Color.white;
        float intensity = 5f;
        float range = 15f;

        if (!string.IsNullOrEmpty(customData))
        {
            var parts = customData.Split(',');
            if (parts.Length > 0) ColorUtility.TryParseHtmlString("#" + parts[0].Trim(), out color);
            if (parts.Length > 1) float.TryParse(parts[1], out intensity);
            if (parts.Length > 2) float.TryParse(parts[2], out range);
        }

        Light light = EchoesLevelShell.SpawnPointLight(name, pos, color, intensity, range, parent, LightmapBakeType.Baked, LightShadows.Soft);
        return light.gameObject;
    }

    private static GameObject MakeAmbientParticles(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject particleObject = new GameObject(name);
        particleObject.transform.SetParent(parent, false);
        particleObject.transform.position = pos;
        
        ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
        ParticleSystemRenderer rendererRef = particleObject.GetComponent<ParticleSystemRenderer>();
        rendererRef.sharedMaterial = EchoesMaterialLibrary.GoalMat;

        var main = particleSystem.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.maxParticles = 80;

        var emission = particleSystem.emission;
        emission.rateOverTime = 9f;

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = scale;

        return particleObject;
    }

    private static GameObject MakeDistantArchitecture(string name, Vector3 pos, Transform parent, string customData)
    {
        // Versión escolar: silueta baja de tejados y cerca perimetral en el
        // horizonte, NO monolitos. La escala humana es la regla — nada en el
        // fondo del nivel debe superar 6-8 unidades de altura salvo casos
        // narrativos explícitos (ej. Nivel 14, fragmentos flotantes).
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        MakeBackgroundRooftop("Rooftop_L", pos + new Vector3(-18f, 4f, 40f), new Vector3(8f, 3f, 6f), root.transform);
        MakeBackgroundRooftop("Rooftop_R", pos + new Vector3(18f, 4f, 42f), new Vector3(8f, 3f, 6f), root.transform);
        MakeBackgroundFence("Fence_Line", pos + new Vector3(0f, 1.2f, 50f), new Vector3(40f, 2.4f, 0.2f), root.transform);

        return root;
    }

    private static GameObject MakeBackgroundRooftop(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject block = MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.ArchMat, SchoolWallModule);
        MeshRenderer[] renderers = block.GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }
        return block;
    }

    private static GameObject MakeBackgroundFence(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject fence = MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.ArchMat, SchoolWallModule);
        return fence;
    }

    private static GameObject MakeLevelGoal(string name, Vector3 pos, Transform parent, string customData, string[] targetSignals)
    {
        GameObject goalObject = new GameObject(name);
        goalObject.transform.SetParent(parent, false);
        goalObject.transform.position = pos;

        LevelGoal goal = goalObject.AddComponent<LevelGoal>();
        
        string objective = "Activa los interruptores.";
        string ready = "Salida desbloqueada.";
        string complete = "Recuerdo restaurado.";

        if (!string.IsNullOrEmpty(customData) && customData.Contains("|"))
        {
            var split = customData.Split('|');
            objective = split[0];
            if (split.Length > 1) ready = split[1];
            if (split.Length > 2) complete = split[2];
        }

        SetSerializedValue(goal, "objectiveText", objective);
        SetSerializedValue(goal, "readyPrompt", ready);
        SetSerializedValue(goal, "completionToast", complete);
        SetSerializedValue(goal, "autoCollectChildTriggers", true);
        SetSerializedValue(goal, "requiredTriggerCount", targetSignals != null ? targetSignals.Length : 1);

        return goalObject;
    }

    private static GameObject MakeLevelRuntime(string name, Transform parent, string customData)
    {
        GameObject rtObj = new GameObject(name);
        rtObj.transform.SetParent(parent, false);
        LevelRuntimeController runtime = rtObj.AddComponent<LevelRuntimeController>();
        
        string objective = "Encuentra la salida.";
        string ready = "Listo.";
        string complete = "Completado.";

        if (!string.IsNullOrEmpty(customData) && customData.Contains("|"))
        {
            var split = customData.Split('|');
            objective = split[0];
            if (split.Length > 1) ready = split[1];
            if (split.Length > 2) complete = split[2];
        }

        SetSerializedValue(runtime, "objectiveText", objective);
        SetSerializedValue(runtime, "readyPrompt", ready);
        SetSerializedValue(runtime, "completionToast", complete);

        return rtObj;
    }


    public static int CurrentBuildingLevel = 0;

    private static Color GetChapterColor(out float intensityMultiplier, out bool addFlicker)
    {
        intensityMultiplier = 1f;
        addFlicker = false;
        
        int levelNum = CurrentBuildingLevel;
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (levelNum <= 0 && !string.IsNullOrEmpty(sceneName) && sceneName.StartsWith("Level_"))
        {
            int.TryParse(sceneName.Substring(6), out levelNum);
        }

        Color col = new Color(0.9f, 0.95f, 1.0f); // Default cold institution light
        if (levelNum <= 0) return col;

        if (levelNum >= 1 && levelNum <= 3) // Capítulo I: Persistencia
        {
            ColorUtility.TryParseHtmlString("#C9D4B0", out col); // Institutional yellow-green fluorescent
            intensityMultiplier = 0.85f;
            addFlicker = (levelNum == 1 || levelNum == 2);
        }
        else if (levelNum == 4 || levelNum == 5 || levelNum == 8) // Capítulo II: Coordinación
        {
            ColorUtility.TryParseHtmlString("#D8B262", out col); // Mustard yellow
            intensityMultiplier = 0.9f;
            addFlicker = (levelNum == 5);
        }
        else if (levelNum == 6 || levelNum == 7 || levelNum == 9) // Capítulo III: Confianza
        {
            ColorUtility.TryParseHtmlString("#A4C2E0", out col); // Cold grey-blue
            intensityMultiplier = 0.75f;
        }
        else if (levelNum == 10 || levelNum == 11) // Capítulo IV: Optimización
        {
            ColorUtility.TryParseHtmlString("#E8B262", out col); // Sunset amber
            intensityMultiplier = 1.1f;
        }
        else if (levelNum == 12 || levelNum == 13) // Capítulo V: Consecuencia
        {
            ColorUtility.TryParseHtmlString("#B23A3A", out col); // Warning red
            intensityMultiplier = 1.0f;
            addFlicker = (levelNum == 13);
        }
        else if (levelNum == 14 || levelNum == 15) // Capítulo VI: Aceptación
        {
            ColorUtility.TryParseHtmlString("#E8B262", out col); // Amber
            intensityMultiplier = 0.6f;
        }

        return col;
    }

    // --- UTILITIES ---

    public static void SetupCollidersRecursive(GameObject root)
    {
        if (root == null) return;
        var cols = root.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols)
        {
            if (c.CompareTag("Interactable") || c.GetComponent<LevelExit>() != null || c.GetComponent<PressurePlate>() != null || c.GetComponent<TutorialTrigger>() != null || c.GetComponent<LevelPacingMarker>() != null || c.GetComponent<DoorController>() != null || c.GetComponent<LevelGoal>() != null)
            {
                continue;
            }

            c.gameObject.layer = GroundLayer; // 6 (Ground)
            if (c is BoxCollider bc)
            {
                bc.isTrigger = false;
            }
            else if (c is MeshCollider mc)
            {
                mc.isTrigger = false;
            }
            else if (c is SphereCollider sc)
            {
                sc.isTrigger = false;
            }
            else if (c is CapsuleCollider cc)
            {
                cc.isTrigger = false;
            }
        }
    }

    /// <summary>
    /// Bounds del conjunto de meshes expresados en el espacio local de <paramref name="root"/>,
    /// independientes de la rotación y escala del contenedor (mesh -> mundo -> local de root).
    /// Devuelve una caja unitaria si el modelo no tiene meshes.
    /// </summary>
    public static Bounds ComputeLocalBounds(Transform root)
    {
        MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>(true);
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;
        bool any = false;

        foreach (MeshFilter mf in filters)
        {
            if (mf.sharedMesh == null) continue;
            Bounds mb = mf.sharedMesh.bounds;
            Vector3 c = mb.center;
            Vector3 e = mb.extents;
            for (int sx = -1; sx <= 1; sx += 2)
            for (int sy = -1; sy <= 1; sy += 2)
            for (int sz = -1; sz <= 1; sz += 2)
            {
                Vector3 corner = c + new Vector3(e.x * sx, e.y * sy, e.z * sz);
                Vector3 local = root.InverseTransformPoint(mf.transform.TransformPoint(corner));
                min = Vector3.Min(min, local);
                max = Vector3.Max(max, local);
                any = true;
            }
        }

        if (!any) return new Bounds(Vector3.zero, Vector3.one);

        Bounds b = new Bounds();
        b.SetMinMax(min, max);
        return b;
    }

    private static GameObject Instantiate3DModel(string modelPath, string name, Vector3 pos, Vector3 scale, Quaternion rot, Transform parent, Material mat = null)
    {
        GameObject container = new GameObject(name);
        if (parent != null) container.transform.SetParent(parent, false);
        container.transform.localPosition = pos;
        container.transform.localRotation = rot;
        container.transform.localScale = scale;
        
        container.layer = GroundLayer;
        container.isStatic = true;

        BoxCollider box = container.AddComponent<BoxCollider>();
        box.center = Vector3.zero;
        box.size = Vector3.one;

        LevelKitPiece kitPiece = container.AddComponent<LevelKitPiece>();
        string safePath = modelPath ?? "";
        bool isWalkable = (safePath.Contains("Platform") || safePath.Contains("Ramp") || name.Contains("Platform") || name.Contains("Floor") || name.Contains("Ramp") || name.Contains("Bridge") || name.Contains("Catwalk") || name.Contains("Ledge") || name.Contains("Tower") || name.Contains("Chamber") || name.Contains("Plat") || name.Contains("Floor") || name.Contains("Elevator"))
            && !name.Contains("Beam") && !name.Contains("Pillar") && !name.Contains("Wall") && !name.Contains("Barrier") && !name.Contains("Door") && !name.Contains("Frame") && !name.Contains("Gate");
        
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
            foreach (var col in childColliders) Object.DestroyImmediate(col);

            Material targetMat = mat != null ? mat : EchoesMaterialLibrary.FloorMat;
            ApplyMaterialOverride(visual, targetMat);

            // Normalizar el modelo a la caja unitaria del contenedor: así el
            // 'scale' que se pasa deja de ser un multiplicador del tamaño nativo
            // (arbitrario, distinto por pieza) y pasa a ser el footprint REAL en
            // mundo, que además coincide con el BoxCollider (center 0, size 1).
            // Esto evita que modelos con tamaño nativo grande se solapen con sus
            // vecinos. Se mide en el espacio local del propio visual para que la
            // rotación/escala del contenedor no distorsionen el cálculo.
            Bounds nativeBounds = ComputeLocalBounds(visual.transform);
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
            Material targetMat = mat != null ? mat : EchoesMaterialLibrary.FloorMat;
            fallbackCube.GetComponent<MeshRenderer>().sharedMaterial = targetMat;
        }

        return container;
    }

    private static GameObject MakeBrutalistBlock(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject block = MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.ArchMat, SchoolFloorModule);
        MeshRenderer[] renderers = block.GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }
        return block;
    }

    private static void SetupTransparentMaterial(Material mat)
    {
        mat.SetFloat("_Surface", 1f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    private static void ApplyMaterialOverride(GameObject obj, Material mat)
    {
        if (obj == null || mat == null) return;
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                Material[] mats = new Material[renderers[i].sharedMaterials.Length];
                for (int j = 0; j < mats.Length; j++)
                {
                    mats[j] = mat;
                }
                renderers[i].sharedMaterials = mats;
            }
        }
    }

    private static void SetSerializedValue(object component, string propertyName, object value)
    {
        if (component is UnityEngine.Object unityObj)
        {
            SerializedObject serializedObject = new SerializedObject(unityObj);
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
                case UnityEngine.Object objectValue:
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
        else
        {
            // Non-unity object reflection if needed, but for serialized properties it's always Unity object
            var field = component.GetType().GetField(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(component, value);
        }
    }

    private static GameObject MakePuzzleSignal(string name, Transform parent, string customData)
    {
        GameObject obj = new GameObject(name);
        if (parent != null) obj.transform.SetParent(parent, false);
        PuzzleSignal signal = obj.AddComponent<PuzzleSignal>();
        
        bool accumulateOnce = true;
        bool satisfiedOnStart = false;
        string displayName = name;

        if (!string.IsNullOrEmpty(customData))
        {
            var parts = customData.Split('|');
            if (parts.Length > 0) displayName = parts[0];
            if (parts.Length > 1) bool.TryParse(parts[1], out accumulateOnce);
            if (parts.Length > 2) bool.TryParse(parts[2], out satisfiedOnStart);
        }

        signal.Configure(displayName, accumulateOnce, satisfiedOnStart);
        return obj;
    }

    private static GameObject MakePuzzleCondition(string name, Transform parent, string customData)
    {
        GameObject obj = new GameObject(name);
        if (parent != null) obj.transform.SetParent(parent, false);
        PuzzleCondition condition = obj.AddComponent<PuzzleCondition>();
        
        PuzzleCondition.ConditionType type = PuzzleCondition.ConditionType.AllPlatesSimultaneous;
        string progressMsg = "";
        string successMsg = "";
        string failMsg = "";

        if (!string.IsNullOrEmpty(customData))
        {
            var parts = customData.Split('|');
            if (parts.Length > 0) System.Enum.TryParse(parts[0], out type);
            if (parts.Length > 1) progressMsg = parts[1];
            if (parts.Length > 2) successMsg = parts[2];
            if (parts.Length > 3) failMsg = parts[3];
        }

        condition.type = type;
        condition.progressMessage = progressMsg;
        condition.successMessage = successMsg;
        condition.failMessage = failMsg;

        return obj;
    }

    private static GameObject MakeHazardField(string name, Vector3 pos, Vector3 size, Transform parent)
    {
        GameObject root = new GameObject(name);
        if (parent != null) root.transform.SetParent(parent, false);
        root.transform.position = pos;

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

        Light lightRef = EchoesLevelShell.SpawnPointLight(name + "_Light", pos + Vector3.up * 1.5f, new Color(1f, 0.14f, 0.08f, 1f), 4f, Mathf.Max(size.x, size.z) + 6f, root.transform);
        EchoShieldField field = root.AddComponent<EchoShieldField>();
        
        return root;
    }

    private static GameObject MakeConflictTrap(string name, Vector3 pos, Vector3 size, Transform parent)
    {
        GameObject root = new GameObject(name);
        if (parent != null) root.transform.SetParent(parent, false);
        root.transform.position = pos;

        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = size;

        GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Cube);
        warning.name = "WarningRotor";
        warning.transform.SetParent(root.transform, false);
        warning.transform.localScale = new Vector3(size.x, 0.12f, 0.25f);
        Object.DestroyImmediate(warning.GetComponent<Collider>());
        warning.GetComponent<MeshRenderer>().sharedMaterial = EchoesMaterialLibrary.DoorMat;

        root.AddComponent<EchoConflictTrap>();
        return root;
    }

    private static GameObject MakeMomentumRelay(string name, Vector3 pos, Vector3 size, Transform parent, string customData)
    {
        GameObject zone = new GameObject(name);
        if (parent != null) zone.transform.SetParent(parent, false);
        zone.transform.position = pos;

        BoxCollider col = zone.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;

        float force = 14f;
        if (!string.IsNullOrEmpty(customData))
        {
            var parts = customData.Split('|');
            if (parts.Length > 0) float.TryParse(parts[0], out force);
        }

        EchoKineticZone kZone = zone.AddComponent<EchoKineticZone>();
        SetSerializedValue(kZone, "role", EchoKineticRole.MomentumRelay);
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
        visual.GetComponent<MeshRenderer>().sharedMaterial = EchoesMaterialLibrary.EchoMat;

        EchoesLevelShell.SpawnPointLight(name + "_Glow", pos + Vector3.up * 1.5f, new Color(0.16f, 0.85f, 1f), 2.5f, Mathf.Max(size.x, size.z) + 4f, zone.transform);

        return zone;
    }

    private static GameObject MakeMotorPlatform(string name, Vector3 pos, Vector3 scale, Transform parent, string customData)
    {
        GameObject anchor = new GameObject(name + "_Anchor");
        if (parent != null) anchor.transform.SetParent(parent, false);
        anchor.transform.position = pos;

        Vector3 localA = Vector3.zero;
        Vector3 localB = Vector3.zero;
        Vector3 rotationPerSecond = new Vector3(0f, 45f, 0f);
        float duration = 1f;
        float phase = 0f;

        if (!string.IsNullOrEmpty(customData))
        {
            var parts = customData.Split('|');
            if (parts.Length > 0) localA = ParseVector3(parts[0], Vector3.zero);
            if (parts.Length > 1) localB = ParseVector3(parts[1], Vector3.zero);
            if (parts.Length > 2) rotationPerSecond = ParseVector3(parts[2], new Vector3(0f, 45f, 0f));
            if (parts.Length > 3) float.TryParse(parts[3], out duration);
            if (parts.Length > 4) float.TryParse(parts[4], out phase);
        }

        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = name;
        platform.transform.SetParent(anchor.transform, false);
        platform.transform.localPosition = localA;
        platform.transform.localScale = scale;
        platform.layer = GroundLayer;
        platform.GetComponent<MeshRenderer>().sharedMaterial = EchoesMaterialLibrary.BridgeMat;
        platform.AddComponent<KenneyTiling>();

        DynamicTransformMotor motor = platform.AddComponent<DynamicTransformMotor>();
        var configMethod = motor.GetType().GetMethod("Configure");
        if (configMethod != null)
        {
            configMethod.Invoke(motor, new object[] { localA, localB, rotationPerSecond, duration, phase, localA != localB });
        }
        else
        {
            SetSerializedValue(motor, "localA", localA);
            SetSerializedValue(motor, "localB", localB);
            SetSerializedValue(motor, "rotationPerSecond", rotationPerSecond);
            SetSerializedValue(motor, "cycleDuration", duration);
            SetSerializedValue(motor, "phase", phase);
            SetSerializedValue(motor, "useTranslation", localA != localB);
        }

        return anchor;
    }

    // --- PHASE 3 & SCHOOL MODULE FACTORY IMPLEMENTATIONS ---

    private static GameObject MakeObservationChamber(string name, Vector3 pos, Vector3 scale, Transform parent) => MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);
    private static GameObject MakeTemporalBridge(string name, Vector3 pos, Vector3 scale, Transform parent) => MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.BridgeMat, SchoolFloorModule);
    private static GameObject MakePerspectiveAnchor(string name, Vector3 pos, Vector3 scale, Transform parent) => MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);
    private static GameObject MakeMemoryCorridor(string name, Vector3 pos, Vector3 scale, Transform parent) => MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.WallTealMat, SchoolWallModule);
    private static GameObject MakeParadoxArena(string name, Vector3 pos, Vector3 scale, Transform parent) => MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);
    private static GameObject MakeErosionVault(string name, Vector3 pos, Vector3 scale, Transform parent) => MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.WallTealMat, SchoolWallModule);
    private static GameObject MakeResonanceChamber(string name, Vector3 pos, Vector3 scale, Transform parent) => MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);
    private static GameObject MakeLiminalThreshold(string name, Vector3 pos, Vector3 scale, Transform parent) => MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);
    private static GameObject MakeChronologicalSpire(string name, Vector3 pos, Vector3 scale, Transform parent) => MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.WallTealMat, SchoolWallModule);
    private static GameObject MakeVoidGallery(string name, Vector3 pos, Vector3 scale, Transform parent) => MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

    // --- UTILITY PREFAB INSTANTIATION & HELPERS ---

    private static string GetMaterialTokenForPrefab(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName)) return "corridor-navy";
        if (prefabName.Contains("Floor")) return "corridor-navy";
        if (prefabName.Contains("Wall")) return "institutional-teal";
        if (prefabName.Contains("Desk")) return "memory-amber";
        if (prefabName.Contains("Chair")) return "faded-mustard";
        if (prefabName.Contains("Locker")) return "institutional-teal";
        if (prefabName.Contains("Shelf")) return "sage-green";
        if (prefabName.Contains("Column")) return "faded-mustard";
        if (prefabName.Contains("Door")) return "wrongness-red";
        if (prefabName.Contains("Bench")) return "faded-mustard";
        if (prefabName.Contains("Fence")) return "faded-mustard";
        if (prefabName.Contains("Tree")) return "faded-mustard";
        if (prefabName.Contains("Stairs")) return "faded-mustard";
        if (prefabName.StartsWith("Prop_Notebook") || prefabName.StartsWith("Prop_TeacherNotebook") || prefabName.StartsWith("Prop_BlankBook")) return "BookMat";
        if (prefabName.StartsWith("Prop_Backpack") || prefabName.StartsWith("Prop_CenterBackpack") || prefabName.StartsWith("Prop_LyraBackpack")) return "memory-amber";
        if (prefabName.StartsWith("Prop_DriedFlowers") || prefabName.StartsWith("Prop_LyraFlowers")) return "dusty-rose";
        return "corridor-navy";
    }

    private static void ValidateRoomGeometry(GameObject root)
    {
        if (root == null) return;

        // 1. Suelo a Y=0 exacto (no -0.1f)
        var floor = root.transform.Find("Floor") ?? root.transform.Find("CourtyardFloor") ?? root.transform.Find("LiminalFloor");
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(floor.transform.localPosition.x, 0f, floor.transform.localPosition.z);
        }

        // 2. Techo mínimo 2.8m de altura
        var ceiling = root.transform.Find("Ceiling");
        if (ceiling != null && ceiling.transform.position.y < 2.8f)
        {
            ceiling.transform.position = new Vector3(ceiling.transform.position.x, 3f, ceiling.transform.position.z);
        }

        // 3. Pasillos mínimo 3.5m ancho (jugable)
        var wallL = root.transform.Find("Wall_-1");
        var wallR = root.transform.Find("Wall_1");
        if (wallL != null && wallR != null)
        {
            float dist = Mathf.Abs(wallR.transform.localPosition.x - wallL.transform.localPosition.x);
            if (dist < 3.5f)
            {
                wallL.transform.localPosition = new Vector3(-1.75f, wallL.transform.localPosition.y, wallL.transform.localPosition.z);
                wallR.transform.localPosition = new Vector3(1.75f, wallR.transform.localPosition.y, wallR.transform.localPosition.z);
            }
        }

        // 4. Puertas 2.5m ancho x 2.5m alto mínimo
        var door = root.transform.Find("Door_Main") ?? root.transform.Find("Door");
        if (door != null)
        {
            Vector3 doorScale = door.transform.localScale;
            if (doorScale.x < 2.5f || doorScale.y < 2.5f)
            {
                door.transform.localScale = new Vector3(Mathf.Max(doorScale.x, 2.5f), Mathf.Max(doorScale.y, 2.5f), doorScale.z);
            }
            Vector3 doorPos = door.transform.localPosition;
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders)
            {
                if (col.gameObject == door.gameObject || col.transform.IsChildOf(door.transform)) continue;
                if (col.gameObject.name.Contains("Floor") || col.gameObject.name.Contains("Ceiling") || col.gameObject.name.Contains("Wall")) continue;
                
                Vector3 colPos = col.transform.localPosition;
                Vector3 diff = colPos - doorPos;
                diff.y = 0;
                if (diff.magnitude < 1.2f && diff.magnitude > 0.001f)
                {
                    col.transform.localPosition += diff.normalized * (1.2f - diff.magnitude + 0.3f);
                }
            }
        }

        // 4. Limpiar colliders trigger innecesarios en geometría
        var cols = root.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols)
        {
            c.gameObject.layer = GroundLayer; // 6 (Ground)
            if (!c.CompareTag("Interactable") && !c.CompareTag("Echo") && !c.CompareTag("Player") && c.GetComponent<LevelExit>() == null && c.GetComponent<PressurePlate>() == null && c.GetComponent<TutorialTrigger>() == null && c.GetComponent<LevelPacingMarker>() == null && c.GetComponent<DoorController>() == null && c.GetComponent<LevelGoal>() == null)
            {
                c.isTrigger = false;
            }
        }
    }

    private static GameObject InstantiatePrefab(string prefabName, string objName, Transform parent)
    {
        GameObject obj = null;
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/Architecture/{prefabName}") 
            ?? Resources.Load<GameObject>($"Prefabs/Props/Narrative/{prefabName}")
            ?? Resources.Load<GameObject>($"Prefabs/Lighting/{prefabName}")
            ?? Resources.Load<GameObject>(prefabName);

#if UNITY_EDITOR
        if (prefab == null)
        {
            string[] searchPaths = new string[] {
                $"Assets/Prefabs/Architecture/{prefabName}.prefab",
                $"Assets/Prefabs/Props/Narrative/{prefabName}.prefab",
                $"Assets/Prefabs/Lighting/{prefabName}.prefab",
                $"Assets/Prefabs/{prefabName}.prefab"
            };
            foreach (string path in searchPaths)
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) break;
            }
        }
#endif

        if (prefab != null)
        {
#if UNITY_EDITOR
            obj = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
#else
            obj = Object.Instantiate(prefab, parent);
#endif
            if (obj != null)
            {
                obj.name = objName;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
            }
        }

        if (obj == null)
        {
            obj = CreateProceduralPrefabFallback(prefabName, objName, parent);
        }

        if (obj != null)
        {
            var renderer = obj.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                string token = GetMaterialTokenForPrefab(prefabName);
                renderer.sharedMaterial = EchoesMaterialLibrary.GetMaterial(token);
            }
        }

        return obj;
    }

    private static GameObject CreateProceduralPrefabFallback(string prefabName, string objName, Transform parent)
    {
        GameObject go = new GameObject(objName);
        if (parent != null) go.transform.SetParent(parent, false);

        Material mat = EchoesMaterialLibrary.ArchMat;
        PrimitiveType prim = PrimitiveType.Cube;
        Vector3 localScale = Vector3.one;

        if (prefabName.StartsWith("Arch_Floor"))
        {
            mat = EchoesMaterialLibrary.FloorMat;
            localScale = new Vector3(1f, 0.3f, 1f);
        }
        else if (prefabName.StartsWith("Arch_Wall"))
        {
            mat = EchoesMaterialLibrary.WallTealMat;
            localScale = new Vector3(0.2f, 3f, 1f);
        }
        else if (prefabName.StartsWith("Arch_Desk"))
        {
            mat = EchoesMaterialLibrary.WallMustardMat;
            localScale = new Vector3(1.2f, 0.75f, 0.8f);
        }
        else if (prefabName.StartsWith("Arch_Chair"))
        {
            mat = EchoesMaterialLibrary.WallMustardMat;
            localScale = new Vector3(0.5f, 0.8f, 0.5f);
        }
        else if (prefabName.StartsWith("Arch_Locker"))
        {
            mat = EchoesMaterialLibrary.WallTealMat;
            localScale = new Vector3(0.8f, 1.8f, 0.5f);
        }
        else if (prefabName.StartsWith("Arch_Shelf"))
        {
            mat = EchoesMaterialLibrary.WallSageMat;
            localScale = new Vector3(0.6f, 1.8f, 1.2f);
        }
        else if (prefabName.StartsWith("Arch_Fence"))
        {
            mat = EchoesMaterialLibrary.WallTealMat;
            localScale = new Vector3(0.1f, 1.2f, 2f);
        }
        else if (prefabName.StartsWith("Arch_Tree"))
        {
            mat = EchoesMaterialLibrary.ArchMat;
            prim = PrimitiveType.Cylinder;
            localScale = new Vector3(0.4f, 3.5f, 0.4f);
        }
        else if (prefabName.StartsWith("Arch_Stairs"))
        {
            mat = EchoesMaterialLibrary.ArchMat;
            localScale = new Vector3(1.5f, 1.5f, 2f);
        }
        else if (prefabName.StartsWith("Prop_SoccerBall"))
        {
            mat = EchoesMaterialLibrary.ArchMat;
            prim = PrimitiveType.Sphere;
            localScale = Vector3.one * 0.3f;
        }
        else if (prefabName.StartsWith("Prop_Notebook") || prefabName.StartsWith("Prop_TeacherNotebook") || prefabName.StartsWith("Prop_BlankBook"))
        {
            mat = EchoesMaterialLibrary.GetMaterial("BookMat");
            localScale = new Vector3(0.2f, 0.05f, 0.3f);
        }
        else if (prefabName.StartsWith("Prop_PhotoFrame") || prefabName.StartsWith("Prop_StoppedClock"))
        {
            mat = EchoesMaterialLibrary.ArchMat;
            localScale = new Vector3(0.4f, 0.4f, 0.05f);
        }
        else if (prefabName.StartsWith("Prop_Backpack") || prefabName.StartsWith("Prop_CenterBackpack") || prefabName.StartsWith("Prop_LyraBackpack"))
        {
            mat = EchoesMaterialLibrary.GetMaterial("memory-amber");
            localScale = new Vector3(0.35f, 0.4f, 0.25f);
        }
        else if (prefabName.StartsWith("Prop_DriedFlowers") || prefabName.StartsWith("Prop_LyraFlowers"))
        {
            mat = EchoesMaterialLibrary.WallRoseMat;
            localScale = new Vector3(0.2f, 0.3f, 0.2f);
        }

        go.layer = GroundLayer;

        GameObject meshObj = GameObject.CreatePrimitive(prim);
        meshObj.name = "Visual";
        meshObj.layer = GroundLayer;
        meshObj.transform.SetParent(go.transform, false);
        meshObj.transform.localScale = localScale;
        meshObj.GetComponent<Renderer>().sharedMaterial = mat;

        return go;
    }

    private static GameObject CreateFluorescentLight(Transform parent, float z, bool isCentral)
    {
        var lightObj = new GameObject($"Fluorescent_{z:F1}");
        lightObj.transform.SetParent(parent, false);
        lightObj.transform.localPosition = new Vector3(0, 2.8f, z);
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = EchoesMaterialLibrary.TokenToColor("fluorescent-sick");
        light.intensity = 2.5f;
        light.range = 8f;
        light.shadows = LightShadows.Soft;

        var flicker = lightObj.AddComponent<LightFlicker>();
        flicker.baseIntensity = light.intensity;
        flicker.flickerSpeed = Random.Range(0.1f, 0.3f);
        flicker.intensityVariance = 0.4f;
        flicker.OnIntensityChange += (n) => EchoesAudioManager.PlayFluorescentHum(lightObj.transform.position, n);

        if (isCentral)
        {
            flicker.intensityVariance = 0.6f;
            flicker.flickerSpeed = 0.5f;
        }
        return lightObj;
    }

    private static void InjectCorridorNarrativeProps(Transform root, float length)
    {
        float propInterval = 6f;
        for (float z = -length * 0.45f; z <= length * 0.45f; z += propInterval)
        {
            if (Random.value < 0.2f)
            {
                var nb = InstantiatePrefab("Prop_Notebook", $"Notebook_{z:F1}", root);
                if (nb != null)
                {
                    nb.transform.localPosition = new Vector3(Random.Range(-1.5f, 1.5f), 0.1f, z + Random.Range(-1f, 1f));
                    nb.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                }
            }
            if (Random.value < 0.15f)
            {
                var pf = InstantiatePrefab("Prop_PhotoFrame", $"Photo_{z:F1}", root);
                if (pf != null)
                {
                    pf.transform.localPosition = new Vector3(1.8f, 1.5f, z);
                    pf.transform.localRotation = Quaternion.Euler(0, 90, 0);
                }
            }
            if (Mathf.Abs(z) < length * 0.05f)
            {
                var fl = InstantiatePrefab("Prop_FlickeringLight", $"FlickerLight_{z:F1}", root);
                if (fl != null)
                {
                    fl.transform.localPosition = new Vector3(0, 2.5f, z);
                }
            }
        }
    }

    private static void InjectClassroomNarrativeProps(Transform root, string customData, float depth)
    {
        if (Random.value < 0.25f)
        {
            var clock = InstantiatePrefab("Prop_StoppedClock", "Clock", root);
            if (clock != null)
            {
                clock.transform.localPosition = new Vector3(0, 2.5f, -2);
                clock.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }
        if (Random.value < 0.2f)
        {
            var nb = InstantiatePrefab("Prop_TeacherNotebook", "TeacherNotebook", root);
            if (nb != null)
            {
                nb.transform.localPosition = new Vector3(0.3f, 0.8f, depth * 0.4f);
            }
        }
        if (Random.value < 0.15f)
        {
            var cd = InstantiatePrefab("Prop_ChalkDrawing", "ChalkDrawing", root);
            if (cd != null)
            {
                cd.transform.localPosition = new Vector3(Random.Range(-1f, 1f), 1.5f, depth * 0.5f - 0.15f);
            }
        }
    }

    private static void PlaceDecalAt(Transform root, string decalName, Vector3 localPos)
    {
        var decalObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        decalObj.name = decalName;
        decalObj.transform.SetParent(root, false);
        decalObj.transform.localPosition = localPos;
        decalObj.transform.localRotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
        decalObj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        Object.DestroyImmediate(decalObj.GetComponent<Collider>());
        var renderer = decalObj.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = EchoesMaterialLibrary.GetMaterial(decalName);
    }

    private static void PlaceMoistureLines(Transform root, float length, float width, float height)
    {
        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * (width * 0.49f);
            for (float z = -length * 0.4f; z <= length * 0.4f; z += 5f)
            {
                var lineObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                lineObj.name = $"MoistureLine_{side}_{z:F1}";
                lineObj.transform.SetParent(root, false);
                lineObj.transform.localPosition = new Vector3(x, height * 0.5f, z);
                lineObj.transform.localRotation = Quaternion.Euler(0, side == 1 ? -90f : 90f, 0);
                lineObj.transform.localScale = new Vector3(0.4f, height * 0.8f, 1f);
                Object.DestroyImmediate(lineObj.GetComponent<Collider>());
                lineObj.GetComponent<MeshRenderer>().sharedMaterial = EchoesMaterialLibrary.GetMaterial("dec_moisture_line");
            }
        }
    }

    // --- SCHOOL MODULE CREATION METHODS ---

    private static GameObject MakeSchoolHall(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;
        float height = placement.scale.y;

        var floor = InstantiatePrefab("Arch_Floor", "Floor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            var wall = InstantiatePrefab("Arch_Wall", $"Wall_{side}", root.transform);
            if (wall != null)
            {
                wall.transform.localPosition = new Vector3(side * width * 0.5f, 0, 0);
                wall.transform.localScale = new Vector3(0.2f, height, depth);
                wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
                ApplyMaterialOverride(wall, EchoesMaterialLibrary.WallTealMat);
            }
        }

        var colL = InstantiatePrefab("Arch_Column", "Col_L", root.transform);
        if (colL != null) colL.transform.localPosition = new Vector3(-width * 0.3f, 0, 0);
        var colR = InstantiatePrefab("Arch_Column", "Col_R", root.transform);
        if (colR != null) colR.transform.localPosition = new Vector3(width * 0.3f, 0, 0);

        var bench0 = InstantiatePrefab("Arch_Bench", "Bench0", root.transform);
        if (bench0 != null) bench0.transform.localPosition = new Vector3(-width * 0.35f, 0, -depth * 0.25f);
        var bench1 = InstantiatePrefab("Arch_Bench", "Bench1", root.transform);
        if (bench1 != null) bench1.transform.localPosition = new Vector3(-width * 0.35f, 0, depth * 0.25f);

        var directory = InstantiatePrefab("Arch_Locker", "Directory", root.transform);
        if (directory != null)
        {
            directory.transform.localPosition = new Vector3(width * 0.35f, 0, 0);
            directory.transform.localRotation = Quaternion.Euler(0, -90, 0);
        }

        CreateFluorescentLight(root.transform, 0, true);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolCorridor(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float length = placement.scale.z;
        float width = Mathf.Max(placement.scale.x, 3.5f);
        float height = placement.scale.y;

        var floor = InstantiatePrefab("Arch_Floor", "Floor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, length);
        }

        float wallHeight = height;
        float convergence = 0.05f;
        for (int side = -1; side <= 1; side += 2)
        {
            var wall = InstantiatePrefab("Arch_Wall", $"Wall_{side}", root.transform);
            if (wall != null)
            {
                float xPos = side * (width * 0.5f + convergence * length * 0.5f);
                wall.transform.localPosition = new Vector3(xPos, 0, 0);
                wall.transform.localScale = new Vector3(0.2f * (1 + convergence * length * 0.1f), wallHeight, length);
                wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
            }
        }

        var ceiling = InstantiatePrefab("Arch_Floor", "Ceiling", root.transform);
        if (ceiling != null)
        {
            ceiling.transform.localPosition = new Vector3(0, height, 0);
            ceiling.transform.localScale = new Vector3(width, 0.3f, length);
            ceiling.transform.localRotation = Quaternion.Euler(180, 0, 0);
        }

        float lockerSpacing = 6f + Random.Range(-0.3f, 0.3f);
        for (float z = -length * 0.45f; z <= length * 0.45f; z += lockerSpacing)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                if (Random.value > 0.85f) continue;
                var locker = InstantiatePrefab("Arch_Locker", $"Locker_{side}_{z:F1}", root.transform);
                if (locker != null)
                {
                    float x = side * (width * 0.5f - 0.8f);
                    float y = 0.1f;
                    float rotY = side == 1 ? 90f : -90f;
                    float rotZ = Random.Range(-3f, 3f);
                    float scaleY = Random.value < 0.1f ? 0.95f : 1f;
                    bool open = Random.value < 0.15f;
                    locker.transform.localPosition = new Vector3(x, y, z);
                    locker.transform.localRotation = Quaternion.Euler(0, rotY, rotZ);
                    locker.transform.localScale = new Vector3(1, scaleY, 1);
                    if (open)
                    {
                        var door = locker.transform.Find("Door");
                        if (door != null) door.localRotation = Quaternion.Euler(0, 0, -90);
                    }
                    if (Random.value < 0.5f) PlaceDecalAt(root.transform, "dec_floor_drag", new Vector3(x, 0.01f, z));
                }
            }
        }

        float windowSpacing = 8f + Random.Range(-1f, 1f);
        for (float z = -length * 0.4f; z <= length * 0.4f; z += windowSpacing)
        {
            var window = InstantiatePrefab("Arch_WallWindow", $"Window_{z:F1}", root.transform);
            if (window != null)
            {
                window.transform.localPosition = new Vector3(width * 0.5f, 1.5f, z);
                window.transform.localRotation = Quaternion.Euler(0, 90, 0) * Quaternion.Euler(0, 0, Random.Range(-2f, 2f));
            }
        }

        float lightSpacing = 10f + Random.Range(-1f, 1f);
        for (float z = -length * 0.45f; z <= length * 0.45f; z += lightSpacing)
        {
            CreateFluorescentLight(root.transform, z, isCentral: Mathf.Abs(z) < length * 0.1f);
        }

        InjectCorridorNarrativeProps(root.transform, length);
        PlaceMoistureLines(root.transform, length, width, height);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolClassroom(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;
        float height = placement.scale.y;

        var floor = InstantiatePrefab("Arch_Floor", "Floor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            var wall = InstantiatePrefab("Arch_Wall", $"Wall_{side}", root.transform);
            if (wall != null)
            {
                wall.transform.localPosition = new Vector3(side * width * 0.5f, 0, 0);
                wall.transform.localScale = new Vector3(0.2f, height, depth);
                wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
                ApplyMaterialOverride(wall, EchoesMaterialLibrary.WallMustardMat);
            }
        }

        var backWall = InstantiatePrefab("Arch_Wall", "Wall_Back", root.transform);
        if (backWall != null)
        {
            backWall.transform.localPosition = new Vector3(0, 0, depth * 0.5f);
            backWall.transform.localScale = new Vector3(width, height, 0.2f);
            ApplyMaterialOverride(backWall, EchoesMaterialLibrary.WallMustardMat);
        }

        var ceiling = InstantiatePrefab("Arch_Floor", "Ceiling", root.transform);
        if (ceiling != null)
        {
            ceiling.transform.localPosition = new Vector3(0, height, 0);
            ceiling.transform.localScale = new Vector3(width, 0.3f, depth);
            ceiling.transform.localRotation = Quaternion.Euler(180, 0, 0);
        }

        float deskSpacingX = Mathf.Max(3f, width * 0.25f);
        float deskSpacingZ = Mathf.Max(3f, depth * 0.25f);
        int rows = Mathf.Min(4, Mathf.FloorToInt(depth / deskSpacingZ));
        if (rows < 1) rows = 1;
        int cols = Mathf.Min(5, Mathf.FloorToInt(width / deskSpacingX));
        if (cols < 1) cols = 1;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (col == 2) continue; // PASILLO CENTRAL LIBRE
                if (Random.value < 0.05f) continue;

                var desk = InstantiatePrefab("Arch_Desk", $"Desk_{row}_{col}", root.transform);
                float x = (col - (cols - 1) * 0.5f) * deskSpacingX + Random.Range(-0.15f, 0.15f);
                float z = (row - (rows - 1) * 0.5f) * deskSpacingZ + Random.Range(-0.15f, 0.15f);
                if (desk != null)
                {
                    desk.transform.localPosition = new Vector3(x, 0.1f, z);
                    desk.transform.localRotation = Quaternion.Euler(0, Random.Range(-5f, 5f), 0);

                    if ((row == 0 || row == rows - 1) && (col == 0 || col == cols - 1) && Random.value < 0.1f)
                    {
                        desk.transform.localRotation = Quaternion.Euler(Random.Range(-30f, 30f), Random.Range(0, 360), Random.Range(-30f, 30f));
                        desk.transform.localPosition += Vector3.up * 0.2f;
                    }
                }

                if (Random.value < 0.85f)
                {
                    var chair = InstantiatePrefab("Arch_Chair", $"Chair_{row}_{col}", root.transform);
                    if (chair != null)
                    {
                        chair.transform.localPosition = new Vector3(x, 0, z - 0.7f + Random.Range(-0.1f, 0.1f));
                        chair.transform.localRotation = Quaternion.Euler(0, 180f + Random.Range(-10f, 10f), 0);
                    }
                }
            }
        }

        var board = InstantiatePrefab("Arch_Wall", "Blackboard", root.transform);
        if (board != null)
        {
            board.transform.localPosition = new Vector3(0, 1.2f, depth * 0.5f - 0.1f);
            board.transform.localScale = new Vector3(width * 0.8f, 2f, 0.2f);
            var boardMat = EchoesMaterialLibrary.GetMaterial("ChalkboardMat");
            ApplyMaterialOverride(board, boardMat);
        }

        var teacherDesk = InstantiatePrefab("Arch_Desk", "TeacherDesk", root.transform);
        if (teacherDesk != null)
        {
            teacherDesk.transform.localPosition = new Vector3(0, 0.1f, depth * 0.4f);
            teacherDesk.transform.localScale = new Vector3(1.5f, 1f, 1f);
        }

        var mainDoor = InstantiatePrefab("Arch_Doorway", "Door_Main", root.transform);
        if (mainDoor != null)
        {
            mainDoor.transform.localPosition = new Vector3(0f, 0f, -depth * 0.5f);
            mainDoor.transform.localRotation = Quaternion.identity;
            var controller = mainDoor.GetComponent<DoorController>() ?? mainDoor.AddComponent<DoorController>();
            controller.latchOpen = false;
        }

        var winL = InstantiatePrefab("Arch_WallWindow", "Window_L", root.transform);
        if (winL != null)
        {
            winL.transform.localPosition = new Vector3(-width * 0.5f, 1.5f, 0);
            winL.transform.localScale = new Vector3(0.2f, 1.8f, depth * 0.4f);
            winL.transform.localRotation = Quaternion.Euler(0, -90, 0);
        }
        var winR = InstantiatePrefab("Arch_WallWindow", "Window_R", root.transform);
        if (winR != null)
        {
            winR.transform.localPosition = new Vector3(width * 0.5f, 1.5f, depth * 0.2f);
            winR.transform.localScale = new Vector3(0.2f, 1.2f, depth * 0.2f);
            winR.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }

        for (float z = -depth * 0.35f; z <= depth * 0.35f; z += 4f + Random.Range(-0.5f, 0.5f))
        {
            CreateFluorescentLight(root.transform, z, isCentral: Random.value < 0.3f);
        }

        InjectClassroomNarrativeProps(root.transform, placement.customData, depth);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolStairwell(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        int floors = 2;
        for (int f = 0; f < floors; f++)
        {
            float y = f * 3.5f;
            float inset = f * 0.5f;

            var floor = InstantiatePrefab("Arch_Floor", $"Floor_{f}", root.transform);
            if (floor != null)
            {
                floor.transform.localPosition = new Vector3(0, f == 0 ? 0f : y, 0);
                floor.transform.localScale = new Vector3(placement.scale.x - inset, 0.3f, placement.scale.z - inset);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                var wall = InstantiatePrefab("Arch_Wall", $"Wall_{f}_{side}", root.transform);
                if (wall != null)
                {
                    wall.transform.localPosition = new Vector3(side * (placement.scale.x * 0.5f - inset), y + 1.5f, 0);
                    wall.transform.localScale = new Vector3(0.2f, 3.5f, placement.scale.z - inset);
                    wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
                    ApplyMaterialOverride(wall, EchoesMaterialLibrary.WallTealMat);
                }
            }

            var stairs = InstantiatePrefab("Arch_Stairs", $"Stairs_{f}", root.transform);
            if (stairs != null)
            {
                stairs.transform.localPosition = new Vector3(0, y, -placement.scale.z * 0.25f + inset);
                stairs.transform.localScale = new Vector3((placement.scale.x - inset) * 0.5f, 3f, (placement.scale.z * 0.5f - inset));
            }
        }

        CreateFluorescentLight(root.transform, 0, false);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolBathroom(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;

        var floor = InstantiatePrefab("Arch_Floor", "Floor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            var wall = InstantiatePrefab("Arch_Wall", $"Wall_{side}", root.transform);
            if (wall != null)
            {
                wall.transform.localPosition = new Vector3(side * width * 0.5f, 0, 0);
                wall.transform.localScale = new Vector3(0.2f, placement.scale.y, depth);
                wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
                ApplyMaterialOverride(wall, EchoesMaterialLibrary.WallTealMat);
            }
        }

        var backWall = InstantiatePrefab("Arch_Wall", "Wall_Back", root.transform);
        if (backWall != null)
        {
            backWall.transform.localPosition = new Vector3(0, 0, depth * 0.5f);
            backWall.transform.localScale = new Vector3(width, placement.scale.y, 0.2f);
            ApplyMaterialOverride(backWall, EchoesMaterialLibrary.WallTealMat);
        }

        CreateFluorescentLight(root.transform, 0, true);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolStaffRoom(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;

        var floor = InstantiatePrefab("Arch_Floor", "Floor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            var wall = InstantiatePrefab("Arch_Wall", $"Wall_{side}", root.transform);
            if (wall != null)
            {
                wall.transform.localPosition = new Vector3(side * width * 0.5f, 0, 0);
                wall.transform.localScale = new Vector3(0.2f, placement.scale.y, depth);
                wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
                ApplyMaterialOverride(wall, EchoesMaterialLibrary.WallMustardMat);
            }
        }

        var staffDesk = InstantiatePrefab("Arch_Desk", "StaffDesk", root.transform);
        if (staffDesk != null)
        {
            staffDesk.transform.localPosition = Vector3.zero;
            staffDesk.transform.localScale = new Vector3(1.5f, 1f, 1.2f);
        }

        var cabinet = InstantiatePrefab("Arch_Locker", "Cabinet", root.transform);
        if (cabinet != null)
        {
            cabinet.transform.localPosition = new Vector3(width * 0.35f, 0, depth * 0.35f);
            cabinet.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }

        CreateFluorescentLight(root.transform, 0, false);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolLibrary(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;
        float height = placement.scale.y;

        var floor = InstantiatePrefab("Arch_Floor", "Floor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            var wall = InstantiatePrefab("Arch_Wall", $"Wall_{side}", root.transform);
            if (wall != null)
            {
                wall.transform.localPosition = new Vector3(side * width * 0.5f, 0, 0);
                wall.transform.localScale = new Vector3(0.2f, height, depth);
                wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
                ApplyMaterialOverride(wall, EchoesMaterialLibrary.WallSageMat);
            }
        }

        float shelfZ = -depth * 0.4f;
        while (shelfZ < depth * 0.4f)
        {
            float len = Random.Range(3f, 6f);
            float h = Random.Range(3f, 4f);
            float gap = Random.Range(1.5f, 2.5f);

            var shelfL = InstantiatePrefab("Arch_Shelf", $"Shelf_L_{shelfZ:F1}", root.transform);
            if (shelfL != null)
            {
                shelfL.transform.localPosition = new Vector3(-width * 0.35f, 0.1f, shelfZ);
                shelfL.transform.localScale = new Vector3(0.6f, h, len);
                shelfL.transform.localRotation = Quaternion.Euler(0, 90, 0);
            }

            var shelfR = InstantiatePrefab("Arch_Shelf", $"Shelf_R_{shelfZ:F1}", root.transform);
            if (shelfR != null)
            {
                shelfR.transform.localPosition = new Vector3(width * 0.35f, 0.1f, shelfZ + gap * 0.5f);
                shelfR.transform.localScale = new Vector3(0.6f, h, len);
                shelfR.transform.localRotation = Quaternion.Euler(0, -90, 0);
            }

            shelfZ += len + gap;
        }

        for (int i = 0; i < 4; i++)
        {
            var table = InstantiatePrefab("Arch_Desk", $"Table_{i}", root.transform);
            if (table != null)
            {
                table.transform.localPosition = new Vector3(Random.Range(-width * 0.2f, width * 0.2f), 0.1f, Random.Range(-depth * 0.3f, depth * 0.3f));
                table.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                table.transform.localScale = new Vector3(1.5f, 1f, 1.2f);

                for (int b = 0; b < 3; b++)
                {
                    var book = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    book.name = $"Book_{b}";
                    book.transform.SetParent(table.transform, false);
                    book.transform.localPosition = new Vector3(Random.Range(-0.5f, 0.5f), 0.6f + b * 0.15f, Random.Range(-0.4f, 0.4f));
                    book.transform.localScale = new Vector3(0.15f, 0.2f, 0.1f);
                    book.transform.localRotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
                    var mat = EchoesMaterialLibrary.GetMaterial("BookMat");
                    book.GetComponent<Renderer>().sharedMaterial = mat;
                    Object.DestroyImmediate(book.GetComponent<BoxCollider>());
                }
            }
        }

        for (float z = -depth * 0.35f; z <= depth * 0.35f; z += 5f + Random.Range(-0.8f, 0.8f))
        {
            var lightObj = new GameObject($"LibLight_{z:F1}");
            lightObj.transform.SetParent(root.transform, false);
            lightObj.transform.localPosition = new Vector3(0, height * 0.85f, z);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.92f, 0.85f, 0.65f);
            light.intensity = 3f;
            light.range = 10f;
        }

        if (Random.value < 0.3f) InstantiatePrefab("Prop_BlankBook", "BlankBook", root.transform);
        InstantiatePrefab("Prop_LibraryStamp", "LibraryStamp", root.transform);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolCourtyard(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;

        var floor = InstantiatePrefab("Arch_Floor", "CourtyardFloor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int i = 0; i < 15; i++)
        {
            var crack = new GameObject($"Crack_{i}");
            crack.transform.SetParent(root.transform, false);
            var lr = crack.AddComponent<LineRenderer>();
            lr.material = EchoesMaterialLibrary.GetMaterial("CrackMat");
            lr.startWidth = lr.endWidth = 0.02f;
            lr.positionCount = Random.Range(3, 8);
            Vector3 start = new Vector3(Random.Range(-width * 0.4f, width * 0.4f), 0.01f, Random.Range(-depth * 0.4f, depth * 0.4f));
            for (int p = 0; p < lr.positionCount; p++)
            {
                lr.SetPosition(p, start + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)));
            }
            var childCols = crack.GetComponentsInChildren<Collider>(true);
            foreach (var c in childCols) Object.DestroyImmediate(c);
        }

        float halfX = width * 0.5f;
        float halfZ = depth * 0.5f;
        for (float x = -halfX; x <= halfX; x += 3f)
        {
            var f1 = InstantiatePrefab("Arch_Fence", $"Fence_Front_{x:F1}", root.transform);
            if (f1 != null) f1.transform.localPosition = new Vector3(x, 0.1f, -halfZ);
            var f2 = InstantiatePrefab("Arch_Fence", $"Fence_Back_{x:F1}", root.transform);
            if (f2 != null) f2.transform.localPosition = new Vector3(x, 0.1f, halfZ);
        }
        for (float z = -halfZ; z <= halfZ; z += 3f)
        {
            var f3 = InstantiatePrefab("Arch_Fence", $"Fence_Left_{z:F1}", root.transform);
            if (f3 != null)
            {
                f3.transform.localPosition = new Vector3(-halfX, 0.1f, z);
                f3.transform.localRotation = Quaternion.Euler(0, 90, 0);
            }
            var f4 = InstantiatePrefab("Arch_Fence", $"Fence_Right_{z:F1}", root.transform);
            if (f4 != null)
            {
                f4.transform.localPosition = new Vector3(halfX, 0.1f, z);
                f4.transform.localRotation = Quaternion.Euler(0, 90, 0);
            }
        }

        var tree = InstantiatePrefab("Arch_Tree", "CenterTree", root.transform);
        if (tree != null)
        {
            tree.transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            tree.transform.localScale = Vector3.one * Random.Range(1.3f, 1.7f);
        }

        for (int i = 0; i < 6; i++)
        {
            var bench = InstantiatePrefab("Arch_Bench", $"Bench_{i}", root.transform);
            if (bench != null)
            {
                float angle = Random.Range(0, 360f);
                float dist = Random.Range(5f, Mathf.Min(halfX, halfZ) * 0.8f);
                bench.transform.localPosition = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * dist, 0.1f, Mathf.Sin(angle * Mathf.Deg2Rad) * dist);
                bench.transform.localRotation = Quaternion.Euler(0, angle + 180f + Random.Range(-15f, 15f), 0);
                if (Random.value < 0.2f) bench.transform.localScale = new Vector3(1, 0.8f, 0.5f);
            }
        }

        var cart = InstantiatePrefab("Prop_JanitorCart", "JanitorCart", root.transform);
        if (cart != null) cart.transform.localPosition = new Vector3(-8, 0.1f, -5);
        var graf = InstantiatePrefab("Prop_ChalkGraffiti", "ChalkGraffiti", root.transform);
        if (graf != null) graf.transform.localPosition = Vector3.up * 0.01f;
        var ball = InstantiatePrefab("Prop_SoccerBall", "SoccerBall", root.transform);
        if (ball != null) ball.transform.localPosition = new Vector3(8, 0.1f, 5);

        var dirLight = new GameObject("CourtyardSkyLight");
        dirLight.transform.SetParent(root.transform, false);
        dirLight.transform.localPosition = new Vector3(0, 10, 0);
        var l = dirLight.AddComponent<Light>();
        l.type = LightType.Directional;
        l.color = new Color(0.7f, 0.8f, 0.95f);
        l.intensity = 1.2f;

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolGym(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;

        var floor = InstantiatePrefab("Arch_Floor", "Floor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            var wall = InstantiatePrefab("Arch_Wall", $"Wall_{side}", root.transform);
            if (wall != null)
            {
                wall.transform.localPosition = new Vector3(side * width * 0.5f, 0, 0);
                wall.transform.localScale = new Vector3(0.5f, 5f, depth);
                wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
                ApplyMaterialOverride(wall, EchoesMaterialLibrary.WallTealMat);
            }
        }

        EchoesLevelShell.SpawnPointLight(placement.name + "_GymLight0", placement.position + new Vector3(-3f, 4.5f, 0f), EchoesMaterialLibrary.TokenToColor("fluorescent-sick"), 4f, 12f, root.transform);
        EchoesLevelShell.SpawnPointLight(placement.name + "_GymLight1", placement.position + new Vector3(3f, 4.5f, 0f), EchoesMaterialLibrary.TokenToColor("fluorescent-sick"), 4f, 12f, root.transform);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolLab(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;

        var floor = InstantiatePrefab("Arch_Floor", "Floor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            var wall = InstantiatePrefab("Arch_Wall", $"Wall_{side}", root.transform);
            if (wall != null)
            {
                wall.transform.localPosition = new Vector3(side * width * 0.5f, 0, 0);
                wall.transform.localScale = new Vector3(0.2f, placement.scale.y, depth);
                wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
                ApplyMaterialOverride(wall, EchoesMaterialLibrary.WallTealMat);
            }
        }

        var bench0 = InstantiatePrefab("Arch_Desk", "LabBench0", root.transform);
        if (bench0 != null)
        {
            bench0.transform.localPosition = new Vector3(-width * 0.25f, 0, 0);
            bench0.transform.localScale = new Vector3(1.2f, 1f, 1.5f);
        }
        var bench1 = InstantiatePrefab("Arch_Desk", "LabBench1", root.transform);
        if (bench1 != null)
        {
            bench1.transform.localPosition = new Vector3(width * 0.25f, 0, 0);
            bench1.transform.localScale = new Vector3(1.2f, 1f, 1.5f);
        }

        CreateFluorescentLight(root.transform, 0, false);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolMaintenanceCorridor(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;

        var floor = InstantiatePrefab("Arch_Floor", "Floor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            var wall = InstantiatePrefab("Arch_Wall", $"Wall_{side}", root.transform);
            if (wall != null)
            {
                wall.transform.localPosition = new Vector3(side * width * 0.5f, 0, 0);
                wall.transform.localScale = new Vector3(0.5f, 2.8f, depth);
                wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
                ApplyMaterialOverride(wall, EchoesMaterialLibrary.WallTealMat);
            }
        }

        CreateFluorescentLight(root.transform, 0, true);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolEmergencyCorridor(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;

        var floor = InstantiatePrefab("Arch_Floor", "Floor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            var wall = InstantiatePrefab("Arch_Wall", $"Wall_{side}", root.transform);
            if (wall != null)
            {
                wall.transform.localPosition = new Vector3(side * width * 0.5f, 0, 0);
                wall.transform.localScale = new Vector3(0.5f, 3f, depth);
                wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
                ApplyMaterialOverride(wall, EchoesMaterialLibrary.WallTealMat);
            }
        }

        EchoesLevelShell.SpawnPointLight(placement.name + "_EmergLight", placement.position + Vector3.up * 2.5f, EchoesMaterialLibrary.TokenToColor("wrongness-red"), 3f, 8f, root.transform);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolLyraClassroom(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;
        float height = placement.scale.y;

        var floor = InstantiatePrefab("Arch_Floor", "Floor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            var wall = InstantiatePrefab("Arch_Wall", $"Wall_{side}", root.transform);
            if (wall != null)
            {
                wall.transform.localPosition = new Vector3(side * width * 0.5f, 0, 0);
                wall.transform.localScale = new Vector3(0.2f, height, depth);
                wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
                ApplyMaterialOverride(wall, EchoesMaterialLibrary.WallRoseMat);
            }
        }

        int count = 20;
        float radius = Mathf.Max(3f, Mathf.Min(width, depth) * 0.35f);
        for (int i = 0; i < count; i++)
        {
            float angle = -90f + (i / (float)(count - 1)) * 180f;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(rad) * radius, 0.1f, Mathf.Sin(rad) * radius);
            Quaternion rot = Quaternion.Euler(0, angle + 90f, 0);

            var desk = InstantiatePrefab("Arch_Desk", $"LyraDesk_{i}", root.transform);
            if (desk != null)
            {
                desk.transform.localPosition = pos;
                desk.transform.localRotation = rot;
            }

            var chair = InstantiatePrefab("Arch_Chair", $"LyraChair_{i}", root.transform);
            if (chair != null)
            {
                chair.transform.localPosition = pos + new Vector3(0, 0, -0.7f);
                chair.transform.localRotation = rot * Quaternion.Euler(0, 180, 0);
            }

            if (i == count - 1 && desk != null)
            {
                ApplyMaterialOverride(desk, EchoesMaterialLibrary.GetMaterial("memory-amber"));
                var backpack = InstantiatePrefab("Prop_Backpack", "LyraBackpack", root.transform);
                if (backpack != null) backpack.transform.localPosition = pos + new Vector3(0, 0, -0.7f);
                var flowers = InstantiatePrefab("Prop_DriedFlowers", "LyraFlowers", root.transform);
                if (flowers != null) flowers.transform.localPosition = new Vector3(pos.x, 1f, pos.z + 0.5f);
            }
        }

        var board = InstantiatePrefab("Arch_Wall", "Blackboard", root.transform);
        if (board != null)
        {
            board.transform.localPosition = new Vector3(0, 1.2f, depth * 0.5f - 0.1f);
            board.transform.localScale = new Vector3(width * 0.8f, 2f, 0.2f);
            ApplyMaterialOverride(board, EchoesMaterialLibrary.GetMaterial("ChalkboardMat"));
            var sil = InstantiatePrefab("Prop_ChalkDrawing", "TwoSilhouettes", board.transform);
            if (sil != null) sil.transform.localPosition = Vector3.forward * -0.11f;
        }

        var window = InstantiatePrefab("Arch_WallWindow", "BigWindow", root.transform);
        if (window != null)
        {
            window.transform.localPosition = new Vector3(-width * 0.5f, 1.5f, 0);
            window.transform.localScale = new Vector3(0.2f, 2.2f, depth * 0.6f);
            window.transform.localRotation = Quaternion.Euler(0, -90, 0);
        }

        EchoesLevelShell.SpawnPointLight(placement.name + "_LyraLight", placement.position + Vector3.up * 2.5f, EchoesMaterialLibrary.TokenToColor("memory-amber"), 3f, 8f, root.transform);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolOffice(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;

        var floor = InstantiatePrefab("Arch_Floor", "Floor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            var wall = InstantiatePrefab("Arch_Wall", $"Wall_{side}", root.transform);
            if (wall != null)
            {
                wall.transform.localPosition = new Vector3(side * width * 0.5f, 0, 0);
                wall.transform.localScale = new Vector3(0.2f, placement.scale.y, depth);
                wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
                ApplyMaterialOverride(wall, EchoesMaterialLibrary.WallMustardMat);
            }
        }

        var desk = InstantiatePrefab("Arch_Desk", "OfficeDesk", root.transform);
        if (desk != null) desk.transform.localPosition = new Vector3(0, 0, depth * 0.25f);
        var chair = InstantiatePrefab("Arch_Chair", "OfficeChair", root.transform);
        if (chair != null) chair.transform.localPosition = new Vector3(0, 0, depth * 0.25f - 1f);

        CreateFluorescentLight(root.transform, 0, false);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeSchoolLiminalClassroom(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;

        var floor = InstantiatePrefab("Arch_Floor", "LiminalFloor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int i = 0; i < 3; i++)
        {
            var desk = InstantiatePrefab("Arch_Desk", $"FloatDesk_{i}", root.transform);
            if (desk != null)
            {
                desk.transform.localPosition = new Vector3(Random.Range(-2f, 2f), 0.4f, Random.Range(-2f, 2f));
                desk.transform.localRotation = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0, 360), Random.Range(-5f, 5f));
            }
        }

        var board = InstantiatePrefab("Arch_Wall", "BrokenBoard", root.transform);
        if (board != null)
        {
            board.transform.localPosition = new Vector3(1.5f, 1.2f, depth * 0.4f);
            board.transform.localScale = new Vector3(width * 0.6f, 2f, 0.2f);
            board.transform.localRotation = Quaternion.Euler(0, 15f, 5f);
            ApplyMaterialOverride(board, EchoesMaterialLibrary.GetMaterial("ChalkboardMat"));
        }

        var winVoid = InstantiatePrefab("Arch_Wall", "BlackWindow", root.transform);
        if (winVoid != null)
        {
            winVoid.transform.localPosition = new Vector3(-width * 0.5f, 1.5f, 0);
            winVoid.transform.localScale = new Vector3(0.2f, 2f, depth * 0.5f);
            winVoid.transform.localRotation = Quaternion.Euler(0, -90, 0);
            ApplyMaterialOverride(winVoid, EchoesMaterialLibrary.VoidBlackMat);
        }

        var overt = InstantiatePrefab("Prop_OverturnedDesk", "OverturnedDesk", root.transform);
        if (overt != null) overt.transform.localPosition = Vector3.zero;
        var pack = InstantiatePrefab("Prop_CenterBackpack", "CenterBackpack", root.transform);
        if (pack != null) pack.transform.localPosition = Vector3.up * 0.1f;

        Light limLight = EchoesLevelShell.SpawnPointLight(placement.name + "_WrongLight", placement.position + Vector3.up * 2.5f, EchoesMaterialLibrary.TokenToColor("wrongness-red"), 2.5f, 8f, root.transform);
        if (limLight != null)
        {
            var f = limLight.gameObject.AddComponent<LightFlicker>();
            f.baseIntensity = 2.5f;
            f.flickerSpeed = 0.5f;
        }

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }

    private static GameObject MakeTransitionSpace(ModulePlacement placement, Transform envParent, Transform mechParent)
    {
        var root = new GameObject(placement.name);
        root.transform.SetParent(envParent, false);
        root.transform.position = placement.position;
        root.transform.localScale = placement.scale;
        root.transform.localRotation = Quaternion.Euler(placement.rotation);

        float width = placement.scale.x;
        float depth = placement.scale.z;

        var floor = InstantiatePrefab("Arch_Floor", "Floor", root.transform);
        if (floor != null)
        {
            floor.transform.localPosition = new Vector3(0, 0f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            var wall = InstantiatePrefab("Arch_Wall", $"Wall_{side}", root.transform);
            if (wall != null)
            {
                wall.transform.localPosition = new Vector3(side * width * 0.5f, 0, 0);
                wall.transform.localScale = new Vector3(0.2f, placement.scale.y, depth);
                wall.transform.localRotation = Quaternion.Euler(0, side * 90, 0);
                ApplyMaterialOverride(wall, EchoesMaterialLibrary.WallTealMat);
            }
        }

        EchoesLevelShell.SpawnPointLight(placement.name + "_TransLight", placement.position + Vector3.up * 2.5f, new Color(0.55f, 0.6f, 0.7f), 2f, 8f, root.transform);

        ValidateRoomGeometry(root);
        SetupCollidersRecursive(root);
        return root;
    }
}
