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
    private static string SchoolFloorModule  => ResolveAssetPath("floorFull", "SciFi");
    private static string SchoolWallModule   => ResolveAssetPath("wall", "SciFi");
    private static string SchoolDoorModule   => ResolveAssetPath("wallDoorway", "SciFi");
    private static string SchoolColumnModule => ResolveAssetPath("wallCorner", "SciFi");
    private static string SchoolDeskModule   => ResolveAssetPath("desk", "SciFi");
    private static string SchoolLockerModule => ResolveAssetPath("bookcaseClosed", "SciFi");
    private static string SchoolShelfModule  => ResolveAssetPath("bookcaseOpen", "SciFi");
    private static string SchoolChairModule  => ResolveAssetPath("chairDesk", "SciFi") ?? ResolveAssetPath("chair", "SciFi");
    private static string SchoolStairsModule => ResolveAssetPath("stairs", "SciFi");
    private static string SchoolToiletModule => ResolveAssetPath("toilet", "SciFi");
    private static string SchoolSinkModule   => ResolveAssetPath("bathroomSink", "SciFi");
    private static string SchoolMirrorModule => ResolveAssetPath("bathroomMirror", "SciFi");
    private static string SchoolDeadTreeModule => ResolveAssetPath("DeadTree_3", "SciFi") ?? ResolveAssetPath("DeadTree_1", "SciFi");
    private static string SchoolFenceModule  => ResolveAssetPath("FencePiece", "SciFi") ?? ResolveAssetPath("fence", "SciFi");

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
                obj = MakePressurePlate(placement.name, placement.position, parent, placement.customData);
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

            // New systems vocabulary (Phase 3)
            case ModuleType.ObservationChamber:
                obj = MakeObservationChamber(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.TemporalBridge:
                obj = MakeTemporalBridge(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.PerspectiveAnchor:
                obj = MakePerspectiveAnchor(placement.name, placement.position, placement.scale, parent, placement.customData);
                break;
            case ModuleType.MemoryCorridor:
                obj = MakeMemoryCorridor(placement.name, placement.position, placement.scale, parent, placement.customData);
                break;
            case ModuleType.ParadoxArena:
                obj = MakeParadoxArena(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.ErosionVault:
                obj = MakeErosionVault(placement.name, placement.position, placement.scale, parent, placement.customData);
                break;
            case ModuleType.ResonanceChamber:
                obj = MakeResonanceChamber(placement.name, placement.position, placement.scale, parent, placement.customData, placement.targetSignals);
                break;
            case ModuleType.LiminalThreshold:
                obj = MakeLiminalThreshold(placement.name, placement.position, placement.scale, parent, placement.customData);
                break;
            case ModuleType.ChronologicalSpire:
                obj = MakeChronologicalSpire(placement.name, placement.position, placement.scale, parent);
                break;
            case ModuleType.VoidGallery:
                obj = MakeVoidGallery(placement.name, placement.position, placement.scale, parent);
                break;

            // School Greybox Architecture (Phase 1)
            case ModuleType.SchoolHall:
                obj = MakeSchoolHall(placement.name, placement.position, placement.scale, placement.rotation, parent, placement.customData);
                break;
            case ModuleType.SchoolCorridor:
                obj = MakeSchoolCorridor(placement.name, placement.position, placement.scale, placement.rotation, parent, placement.customData);
                break;
            case ModuleType.SchoolClassroom:
                obj = MakeSchoolClassroom(placement.name, placement.position, placement.scale, placement.rotation, parent, placement.customData);
                break;
            case ModuleType.SchoolStairwell:
                obj = MakeSchoolStairwell(placement.name, placement.position, placement.scale, placement.rotation, parent, placement.customData);
                break;
            case ModuleType.SchoolLibrary:
                obj = MakeSchoolLibrary(placement.name, placement.position, placement.scale, placement.rotation, parent, placement.customData);
                break;
            case ModuleType.SchoolGym:
                obj = MakeSchoolGym(placement.name, placement.position, placement.scale, placement.rotation, parent, placement.customData);
                break;
            case ModuleType.SchoolLab:
                obj = MakeSchoolLab(placement.name, placement.position, placement.scale, placement.rotation, parent, placement.customData);
                break;
            case ModuleType.SchoolLyraClassroom:
                obj = MakeSchoolLyraClassroom(placement.name, placement.position, placement.scale, placement.rotation, parent, placement.customData);
                break;
            case ModuleType.SchoolLiminalClassroom:
                obj = MakeSchoolLiminalClassroom(placement.name, placement.position, placement.scale, placement.rotation, parent, placement.customData);
                break;
            case ModuleType.TransitionSpace:
                obj = MakeTransitionSpace(placement.name, placement.position, placement.scale, placement.rotation, parent, placement.customData);
                break;
            case ModuleType.SchoolEntrance:
                obj = MakeSchoolEntrance(placement.name, placement.position, placement.scale, placement.rotation, parent, placement.customData);
                break;
            case ModuleType.SchoolStaffRoom:
                obj = MakeSchoolStaffRoom(placement.name, placement.position, placement.scale, placement.rotation, parent, placement.customData);
                break;
            case ModuleType.SchoolCourtyard:
                obj = MakeSchoolCourtyard(placement.name, placement.position, placement.scale, placement.rotation, parent, placement.customData);
                break;
        }

        if (obj != null)
        {
            obj.transform.localRotation = Quaternion.Euler(placement.rotation);
        }

        return obj;
    }

    private static bool IsMechanical(ModuleType type)
    {
        return type == ModuleType.PressurePlate || 
               type == ModuleType.Door || 
               type == ModuleType.LevelExit || 
               type == ModuleType.LevelGoal || 
               type == ModuleType.LevelRuntime ||
               type == ModuleType.TutorialTrigger ||
               type == ModuleType.TemporalBridge ||
               type == ModuleType.ResonanceChamber ||
               type == ModuleType.MovingPlatform ||
               type == ModuleType.PuzzleSignal ||
               type == ModuleType.PuzzleCondition ||
               type == ModuleType.HazardField ||
               type == ModuleType.ConflictTrap ||
               type == ModuleType.MomentumRelay ||
               type == ModuleType.MotorPlatform;
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

    private static GameObject MakePressurePlate(string name, Vector3 pos, Transform parent, string customData = null)
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
        if (!string.IsNullOrEmpty(customData) && customData.Contains("EchoOnly"))
        {
            PressurePlateEchoOnly echoOnly = root.AddComponent<PressurePlateEchoOnly>();
            plate.acceptPlayer = false;
            plate.acceptEcho = true;
            plate.acceptEchoProjection = true;
        }
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
        beamMat.SetColor("_EmissionColor", new Color(1.0f, 0.8f, 0.5f) * 0.5f);
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

    // --- NEW SYSTEM FACTORY METHOD IMPLEMENTATIONS ---

    // --- NEW SYSTEM FACTORY METHOD IMPLEMENTATIONS ---

    private static GameObject MakeObservationChamber(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        // Suelo del Aula — slab fino para que muebles no queden enterrados
        Vector3 floorScale = new Vector3(scale.x, 0.3f, scale.z);
        MakePlatform("ClassroomFloor", new Vector3(0f, -0.15f, 0f), floorScale, root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

        // Paredes del aula con ventanas (dejan pasar la niebla/luz)
        float wX = scale.x * 0.5f;
        float wZ = scale.z * 0.5f;
        Instantiate3DModel(SchoolWallModule, "ClassroomWallBack", new Vector3(0f, 0f, -wZ), new Vector3(scale.x, 3.5f, 0.2f), Quaternion.identity, root.transform, EchoesMaterialLibrary.WallTealMat);
        Instantiate3DModel(SchoolWallModule, "ClassroomWallLeft", new Vector3(-wX, 0f, 0f), new Vector3(0.2f, 3.5f, scale.z), Quaternion.Euler(0f, 90f, 0f), root.transform, EchoesMaterialLibrary.WallTealMat);
        Instantiate3DModel(SchoolWallModule, "ClassroomWallRight", new Vector3(wX, 0f, 0f), new Vector3(0.2f, 3.5f, scale.z), Quaternion.Euler(0f, 90f, 0f), root.transform, EchoesMaterialLibrary.WallTealMat);

        // Pupitres y sillas escolares ordenados simétricamente
        float deskSpacingX = Mathf.Min(3f, scale.x * 0.25f);
        float deskSpacingZ = Mathf.Min(3f, scale.z * 0.25f);
        
        for (float xOffset = -scale.x * 0.25f; xOffset <= scale.x * 0.25f + 0.1f; xOffset += deskSpacingX)
        {
            for (float zOffset = -scale.z * 0.2f; zOffset <= scale.z * 0.3f + 0.1f; zOffset += deskSpacingZ)
            {
                Vector3 deskPos = new Vector3(xOffset, 0.1f, zOffset);
                string deskName = $"Desk_{xOffset:0.0}_{zOffset:0.0}";
                GameObject d = Instantiate3DModel(SchoolDeskModule, deskName, deskPos, new Vector3(1.2f, 1f, 0.8f), Quaternion.identity, root.transform, EchoesMaterialLibrary.MemoryMat);
                
                // Silla adyacente
                Vector3 chairPos = deskPos + new Vector3(0f, 0f, -0.7f);
                Instantiate3DModel(SchoolChairModule, deskName + "_Chair", chairPos, new Vector3(0.9f, 0.9f, 0.9f), Quaternion.Euler(0f, 180f, 0f), root.transform, EchoesMaterialLibrary.ArchMat);
            }
        }

        // Luces de techo fluorescentes espaciadas (más puntos lumínicos)
        float mult;
        bool flicker;
        Color capColor = GetChapterColor(out mult, out flicker);
        float zSpacing = scale.z * 0.25f;
        for (float zOffset = -scale.z * 0.25f; zOffset <= scale.z * 0.25f + 0.1f; zOffset += Mathf.Max(3f, zSpacing * 2f))
        {
            Vector3 lightPos = new Vector3(0f, 3.2f, zOffset);
            Light classroomLight = EchoesLevelShell.SpawnPointLight($"ClassroomLight_{zOffset:0.0}", lightPos, capColor, 6f * mult, 16f, root.transform, LightmapBakeType.Baked, LightShadows.Soft);
            if (flicker && Mathf.Abs(zOffset) < 0.1f)
            {
                var flickerComponent = classroomLight.gameObject.AddComponent<LightFlicker>();
                flickerComponent.baseIntensity = 6f * mult;
            }
        }

        return root;
    }

    private static GameObject MakeTemporalBridge(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        // Trigger del Eco
        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(scale.x, scale.y * 3f, scale.z);
        trigger.center = new Vector3(0f, scale.y, 0f);

        // Cuerpo físico del puente (un box collider plano invisible para caminar)
        GameObject colliderObj = new GameObject("BridgeCollider");
        colliderObj.transform.SetParent(root.transform, false);
        colliderObj.transform.localPosition = Vector3.zero;
        BoxCollider walkCol = colliderObj.AddComponent<BoxCollider>();
        walkCol.size = scale;
        walkCol.enabled = false; // Inicialmente fantasma
        colliderObj.layer = GroundLayer;

        // Representación visual: Serie de pupitres flotantes con el material del Eco
        GameObject visualContainer = new GameObject("VisualModel");
        visualContainer.transform.SetParent(root.transform, false);
        visualContainer.transform.localPosition = Vector3.zero;

        float length = Mathf.Max(scale.x, scale.z);
        bool alignX = scale.x > scale.z;
        float step = 2.2f; // Espaciado entre pupitres flotantes
        
        for (float offset = -length * 0.45f; offset <= length * 0.45f; offset += step)
        {
            Vector3 deskLocal = alignX ? new Vector3(offset, 0f, 0f) : new Vector3(0f, 0f, offset);
            GameObject d = Instantiate3DModel(SchoolDeskModule, $"FloatDesk_{offset:0.0}", deskLocal, new Vector3(1.2f, 1f, 0.8f), Quaternion.identity, visualContainer.transform, EchoesMaterialLibrary.EchoMat);
            
            // Silla flotante asociada
            Vector3 chairLocal = deskLocal + (alignX ? new Vector3(0f, 0f, -0.6f) : new Vector3(-0.6f, 0f, 0f));
            float chairRot = alignX ? 180f : 90f;
            Instantiate3DModel(SchoolChairModule, $"FloatChair_{offset:0.0}", chairLocal, new Vector3(0.9f, 0.9f, 0.9f), Quaternion.Euler(0f, chairRot, 0f), visualContainer.transform, EchoesMaterialLibrary.EchoMat);
        }

        TemporalBridge tb = root.AddComponent<TemporalBridge>();
        SetSerializedValue(tb, "bridgeCollider", walkCol);
        SetSerializedValue(tb, "visualMesh", visualContainer);

        return root;
    }

    private static GameObject MakePerspectiveAnchor(string name, Vector3 pos, Vector3 scale, Transform parent, string customData)
    {
        // El punto de anclaje de perspectiva es un pupitre de memoria interactivo
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        // Plataforma de base pequeña — slab fino
        MakePlatform("AnchorBase", new Vector3(0f, -0.15f, 0f), new Vector3(scale.x, 0.3f, scale.z), root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

        // El pupitre de memoria
        GameObject memoryDesk = Instantiate3DModel(SchoolDeskModule, "MemoryDesk", new Vector3(0f, 0.1f, 0f), new Vector3(1.4f, 1.1f, 0.9f), Quaternion.identity, root.transform, EchoesMaterialLibrary.MemoryMat);
        
        // Silla del pupitre
        Instantiate3DModel(SchoolChairModule, "MemoryChair", new Vector3(0f, 0.1f, -0.7f), new Vector3(0.95f, 0.95f, 0.95f), Quaternion.Euler(0f, 180f, 0f), root.transform, EchoesMaterialLibrary.ArchMat);

        // Cilindro indicador brillante sutil
        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        glow.name = "AnchorIndicator";
        glow.transform.SetParent(root.transform, false);
        glow.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        glow.transform.localScale = new Vector3(1.6f, 0.02f, 1.6f);
        Object.DestroyImmediate(glow.GetComponent<Collider>());
        glow.GetComponent<MeshRenderer>().sharedMaterial = EchoesMaterialLibrary.PlateMat;

        return root;
    }

    private static GameObject MakeMemoryCorridor(string name, Vector3 pos, Vector3 scale, Transform parent, string customData)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        // Suelo del pasillo — slab fino para que casilleros no queden enterrados
        Vector3 corridorFloorScale = new Vector3(scale.x, 0.3f, scale.z);
        MakePlatform("CorridorFloor", new Vector3(0f, -0.15f, 0f), corridorFloorScale, root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

        // Paredes laterales
        float halfWidth = scale.x * 0.5f;
        Vector3 wallScale = new Vector3(0.2f, 3.5f, scale.z);
        Instantiate3DModel(SchoolWallModule, "WallL", new Vector3(-halfWidth, 0f, 0f), wallScale, Quaternion.Euler(0f, 90f, 0f), root.transform, EchoesMaterialLibrary.WallMustardMat);
        Instantiate3DModel(SchoolWallModule, "WallR", new Vector3(halfWidth, 0f, 0f), wallScale, Quaternion.Euler(0f, 90f, 0f), root.transform, EchoesMaterialLibrary.WallMustardMat);

        // Colocar estantes/casilleros (bookcaseClosed) empotrados en las paredes a intervalos
        float step = 5f;
        for (float z = -scale.z * 0.4f; z <= scale.z * 0.4f; z += step)
        {
            // Casillero Izquierdo
            Instantiate3DModel(SchoolLockerModule, $"LockerL_{z:0.0}", new Vector3(-halfWidth + 0.4f, 0.1f, z), new Vector3(0.6f, 1.8f, 0.8f), Quaternion.Euler(0f, 90f, 0f), root.transform, EchoesMaterialLibrary.ArchMat);
            // Casillero Derecho
            Instantiate3DModel(SchoolLockerModule, $"LockerR_{z:0.0}", new Vector3(halfWidth - 0.4f, 0.1f, z + step * 0.5f), new Vector3(0.6f, 1.8f, 0.8f), Quaternion.Euler(0f, -90f, 0f), root.transform, EchoesMaterialLibrary.ArchMat);
        }

        // Luces de techo fluorescentes espaciadas en el pasillo
        float mult;
        bool flicker;
        Color capColor = GetChapterColor(out mult, out flicker);
        for (float z = -scale.z * 0.35f; z <= scale.z * 0.35f; z += 4f)
        {
            Light corridorLight = EchoesLevelShell.SpawnPointLight($"CorridorLight_{z:0.0}", new Vector3(0f, 3f, z), capColor, 5.5f * mult, 12f, root.transform, LightmapBakeType.Baked, LightShadows.Soft);
            if (flicker)
            {
                var flickerComponent = corridorLight.gameObject.AddComponent<LightFlicker>();
                flickerComponent.baseIntensity = 5.5f * mult;
            }
        }

        return root;
    }

    private static GameObject MakeParadoxArena(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        // Suelo gigante del patio — slab fino
        Vector3 patioFloorScale = new Vector3(scale.x, 0.3f, scale.z);
        MakePlatform("PatioFloor", new Vector3(0f, -0.15f, 0f), patioFloorScale, root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

        // Vallas perimetrales en los bordes para encerrar el patio (City Pack)
        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;
        
        // Valla delantera y trasera
        for (float x = -halfX; x <= halfX + 0.1f; x += 3f)
        {
            Instantiate3DModel(SchoolFenceModule, $"FenceFront_{x:0.0}", new Vector3(x, 0.1f, -halfZ), new Vector3(1.2f, 1.2f, 0.2f), Quaternion.identity, root.transform, EchoesMaterialLibrary.ArchMat);
            Instantiate3DModel(SchoolFenceModule, $"FenceBack_{x:0.0}", new Vector3(x, 0.1f, halfZ), new Vector3(1.2f, 1.2f, 0.2f), Quaternion.identity, root.transform, EchoesMaterialLibrary.ArchMat);
        }
        
        // Valla izquierda y derecha
        for (float z = -halfZ; z <= halfZ + 0.1f; z += 3f)
        {
            Instantiate3DModel(SchoolFenceModule, $"FenceLeft_{z:0.0}", new Vector3(-halfX, 0.1f, z), new Vector3(0.2f, 1.2f, 1.2f), Quaternion.Euler(0f, 90f, 0f), root.transform, EchoesMaterialLibrary.ArchMat);
            Instantiate3DModel(SchoolFenceModule, $"FenceRight_{z:0.0}", new Vector3(halfX, 0.1f, z), new Vector3(0.2f, 1.2f, 1.2f), Quaternion.Euler(0f, 90f, 0f), root.transform, EchoesMaterialLibrary.ArchMat);
        }

        // Un gran árbol seco en el centro (Stylized Nature MegaKit)
        Instantiate3DModel(SchoolDeadTreeModule, "CenterDeadTree", new Vector3(0f, 0f, 0f), new Vector3(1.5f, 1.5f, 1.5f), Quaternion.identity, root.transform, EchoesMaterialLibrary.ArchMat);

        return root;
    }

    private static GameObject MakeErosionVault(string name, Vector3 pos, Vector3 scale, Transform parent, string customData)
    {
        // Plataforma de baño escolar erosionable
        GameObject obj = MakePlatform(name, pos, scale, parent, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);
        
        // Add trigger collider for detection
        BoxCollider trigger = obj.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(scale.x, 3f, scale.z);
        trigger.center = new Vector3(0f, 1f, 0f);

        // Decoraciones de baño: Inodoro y Lavamanos en las esquinas
        float dX = scale.x * 0.35f;
        float dZ = scale.z * 0.35f;
        Instantiate3DModel(SchoolToiletModule, "BathToilet", new Vector3(-dX, 0.1f, dZ), new Vector3(0.8f, 0.8f, 0.8f), Quaternion.identity, obj.transform, EchoesMaterialLibrary.ArchMat);
        Instantiate3DModel(SchoolSinkModule, "BathSink", new Vector3(dX, 0.1f, dZ), new Vector3(0.8f, 0.8f, 0.8f), Quaternion.Euler(0f, 180f, 0f), obj.transform, EchoesMaterialLibrary.ArchMat);
        Instantiate3DModel(SchoolMirrorModule, "BathMirror", new Vector3(dX, 1.4f, dZ - 0.1f), new Vector3(0.6f, 0.8f, 0.05f), Quaternion.Euler(0f, 180f, 0f), obj.transform, EchoesMaterialLibrary.ArchMat);

        int durability = 3;
        if (!string.IsNullOrEmpty(customData)) int.TryParse(customData, out durability);

        ErosionSystem es = obj.AddComponent<ErosionSystem>();
        SetSerializedValue(es, "maxDurability", durability);

        return obj;
    }

    private static GameObject MakeResonanceChamber(string name, Vector3 pos, Vector3 scale, Transform parent, string customData, string[] targetSignals)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        // Suelo de la Oficina / Sala de Profesores — slab fino
        MakePlatform("ResonanceBase", new Vector3(0f, -0.15f, 0f), new Vector3(scale.x, 0.3f, scale.z), root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

        // Dos mesas/alfombras de resonancia temáticas
        GameObject pad1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pad1.name = "ResonancePad1";
        pad1.transform.SetParent(root.transform, false);
        pad1.transform.localPosition = new Vector3(-scale.x * 0.3f, 0.05f, 0f);
        pad1.transform.localScale = new Vector3(2.2f, 0.01f, 2.2f);
        Object.DestroyImmediate(pad1.GetComponent<Collider>());
        pad1.GetComponent<MeshRenderer>().sharedMaterial = EchoesMaterialLibrary.PlateMat;

        // Escritorio decorativo en el pad 1
        Instantiate3DModel(SchoolDeskModule, "OfficeDesk1", new Vector3(-scale.x * 0.3f, 0.1f, 0.5f), new Vector3(1.2f, 1f, 0.8f), Quaternion.identity, root.transform, EchoesMaterialLibrary.MemoryMat);
        
        BoxCollider trigger1 = pad1.AddComponent<BoxCollider>();
        trigger1.isTrigger = true;
        trigger1.size = new Vector3(2.5f, 4f, 2.5f);
        trigger1.center = new Vector3(0f, 2f, 0f);
        pad1.AddComponent<ResonanceZoneTrigger>();

        GameObject pad2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pad2.name = "ResonancePad2";
        pad2.transform.SetParent(root.transform, false);
        pad2.transform.localPosition = new Vector3(scale.x * 0.3f, 0.05f, 0f);
        pad2.transform.localScale = new Vector3(2.2f, 0.01f, 2.2f);
        Object.DestroyImmediate(pad2.GetComponent<Collider>());
        pad2.GetComponent<MeshRenderer>().sharedMaterial = EchoesMaterialLibrary.PlateMat;

        // Escritorio decorativo en el pad 2
        Instantiate3DModel(SchoolDeskModule, "OfficeDesk2", new Vector3(scale.x * 0.3f, 0.1f, 0.5f), new Vector3(1.2f, 1f, 0.8f), Quaternion.identity, root.transform, EchoesMaterialLibrary.MemoryMat);

        BoxCollider trigger2 = pad2.AddComponent<BoxCollider>();
        trigger2.isTrigger = true;
        trigger2.size = new Vector3(2.5f, 4f, 2.5f);
        trigger2.center = new Vector3(0f, 2f, 0f);
        pad2.AddComponent<ResonanceZoneTrigger>();

        ResonanceSystem rs = root.AddComponent<ResonanceSystem>();
        
        var zone1 = new ResonanceSystem.ResonanceZone
        {
            triggerCollider = trigger1,
            zoneRenderer = pad1.GetComponent<Renderer>()
        };
        var zone2 = new ResonanceSystem.ResonanceZone
        {
            triggerCollider = trigger2,
            zoneRenderer = pad2.GetComponent<Renderer>()
        };

        var zonesList = new List<ResonanceSystem.ResonanceZone> { zone1, zone2 };
        SetSerializedValue(rs, "zones", zonesList);
        SetSerializedValue(rs, "requiredActiveZones", 2);

        if (targetSignals != null && targetSignals.Length > 0)
        {
            GameObject sigObj = GameObject.Find(targetSignals[0]);
            if (sigObj != null)
            {
                PuzzleSignal signal = sigObj.GetComponent<PuzzleSignal>();
                if (signal != null) SetSerializedValue(rs, "targetSignal", signal);
            }
        }

        // Iluminación cálida de oficina
        float mult;
        bool flicker;
        Color capColor = GetChapterColor(out mult, out flicker);
        Light officeLightL = EchoesLevelShell.SpawnPointLight("OfficeLightL", new Vector3(-scale.x * 0.3f, 2.5f, 0f), capColor, 3f * mult, 8f, root.transform, LightmapBakeType.Baked, LightShadows.Soft);
        Light officeLightR = EchoesLevelShell.SpawnPointLight("OfficeLightR", new Vector3(scale.x * 0.3f, 2.5f, 0f), capColor, 3f * mult, 8f, root.transform, LightmapBakeType.Baked, LightShadows.Soft);
        if (flicker)
        {
            officeLightL.gameObject.AddComponent<LightFlicker>().baseIntensity = 3f * mult;
            officeLightR.gameObject.AddComponent<LightFlicker>().baseIntensity = 3f * mult;
        }

        return root;
    }

    private static GameObject MakeLiminalThreshold(string name, Vector3 pos, Vector3 scale, Transform parent, string customData)
    {
        // Esquina de pasillo en L con niebla
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        // Trigger de la zona liminal
        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = scale;

        // Geometría física de la esquina del pasillo — slab fino
        Vector3 cornerFloorScale = new Vector3(scale.x, 0.3f, scale.z);
        MakePlatform("CornerFloor", new Vector3(0f, -0.15f, 0f), cornerFloorScale, root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);
        
        // Paredes formando una L
        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;
        Instantiate3DModel(SchoolWallModule, "CornerWallBack", new Vector3(0f, 0f, halfZ), new Vector3(scale.x, 3.5f, 0.2f), Quaternion.identity, root.transform, EchoesMaterialLibrary.WallSageMat);
        Instantiate3DModel(SchoolWallModule, "CornerWallLeft", new Vector3(-halfX, 0f, 0f), new Vector3(0.2f, 3.5f, scale.z), Quaternion.Euler(0f, 90f, 0f), root.transform, EchoesMaterialLibrary.WallSageMat);
        Instantiate3DModel(SchoolColumnModule, "CornerCol", new Vector3(-halfX + 0.3f, 0f, halfZ - 0.3f), new Vector3(0.6f, 3.5f, 0.6f), Quaternion.identity, root.transform, EchoesMaterialLibrary.ArchMat);

        // Motes de niebla locales
        MakeAmbientParticles("FogMotes", Vector3.zero, scale, root.transform);

        return root;
    }

    private static GameObject MakeChronologicalSpire(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        // Tramo de Escaleras Escolares físicas
        MakePlatform("SpireBaseFloor", new Vector3(0f, 0f, 0f), new Vector3(scale.x, 0.5f, scale.z), root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);
        
        // Colocar escaleras de subida físicas
        Instantiate3DModel(SchoolStairsModule, "StairsUp1", new Vector3(0f, 0f, -scale.z * 0.25f), new Vector3(scale.x * 0.5f, 3f, scale.z * 0.5f), Quaternion.identity, root.transform, EchoesMaterialLibrary.ArchMat);
        
        // Descanso de escalera elevado
        MakePlatform("SpireMidLanding", new Vector3(0f, 3f, scale.z * 0.25f), new Vector3(scale.x, 0.5f, scale.z * 0.5f), root.transform, EchoesMaterialLibrary.BridgeMat, SchoolFloorModule);
        
        // Escaleras de subida 2
        Instantiate3DModel(SchoolStairsModule, "StairsUp2", new Vector3(0f, 3.5f, scale.z * 0.25f), new Vector3(scale.x * 0.5f, 3f, scale.z * 0.5f), Quaternion.Euler(0f, 180f, 0f), root.transform, EchoesMaterialLibrary.ArchMat);
        
        // Planta superior
        MakePlatform("SpireTopFloor", new Vector3(0f, 6.5f, -scale.z * 0.25f), new Vector3(scale.x, 0.5f, scale.z * 0.5f), root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

        // Luz de pared en el descanso
        float mult;
        bool flicker;
        Color capColor = GetChapterColor(out mult, out flicker);
        Light stairLight = EchoesLevelShell.SpawnPointLight("StaircaseLight", new Vector3(0f, 4.5f, scale.z * 0.25f), capColor, 3f * mult, 8f, root.transform, LightmapBakeType.Baked, LightShadows.Soft);
        if (flicker)
        {
            stairLight.gameObject.AddComponent<LightFlicker>().baseIntensity = 3f * mult;
        }

        return root;
    }

    private static GameObject MakeVoidGallery(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        // Biblioteca Escolar / Galería de Libros
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        // Suelo de la Galería — slab fino
        Vector3 galleryFloorScale = new Vector3(scale.x, 0.3f, scale.z);
        MakePlatform("GalleryFloor", new Vector3(0f, -0.15f, 0f), galleryFloorScale, root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

        // Instanciar filas de estanterías de libros (bookcaseOpen) llenas de libros (City Pack / Furniture Kit)
        float dX = scale.x * 0.35f;
        float dZ = scale.z * 0.35f;
        
        // Estanterías a los lados
        Instantiate3DModel(SchoolShelfModule, "LibraryShelf1", new Vector3(-dX, 0.1f, -dZ), new Vector3(0.6f, 2f, 1.8f), Quaternion.identity, root.transform, EchoesMaterialLibrary.WallMustardMat);
        Instantiate3DModel(SchoolShelfModule, "LibraryShelf2", new Vector3(-dX, 0.1f, 0f), new Vector3(0.6f, 2f, 1.8f), Quaternion.identity, root.transform, EchoesMaterialLibrary.WallMustardMat);
        Instantiate3DModel(SchoolShelfModule, "LibraryShelf3", new Vector3(-dX, 0.1f, dZ), new Vector3(0.6f, 2f, 1.8f), Quaternion.identity, root.transform, EchoesMaterialLibrary.WallMustardMat);

        Instantiate3DModel(SchoolShelfModule, "LibraryShelf4", new Vector3(dX, 0.1f, -dZ), new Vector3(0.6f, 2f, 1.8f), Quaternion.identity, root.transform, EchoesMaterialLibrary.WallMustardMat);
        Instantiate3DModel(SchoolShelfModule, "LibraryShelf5", new Vector3(dX, 0.1f, 0f), new Vector3(0.6f, 2f, 1.8f), Quaternion.identity, root.transform, EchoesMaterialLibrary.WallMustardMat);
        Instantiate3DModel(SchoolShelfModule, "LibraryShelf6", new Vector3(dX, 0.1f, dZ), new Vector3(0.6f, 2f, 1.8f), Quaternion.identity, root.transform, EchoesMaterialLibrary.WallMustardMat);

        // Luz cálida mortecina de biblioteca
        float mult;
        bool flicker;
        Color capColor = GetChapterColor(out mult, out flicker);
        Light libLight1 = EchoesLevelShell.SpawnPointLight("LibraryLight1", new Vector3(0f, 2.5f, -dZ * 0.5f), capColor, 3.5f * mult, 10f, root.transform, LightmapBakeType.Baked, LightShadows.Soft);
        Light libLight2 = EchoesLevelShell.SpawnPointLight("LibraryLight2", new Vector3(0f, 2.5f, dZ * 0.5f), capColor, 3.5f * mult, 10f, root.transform, LightmapBakeType.Baked, LightShadows.Soft);
        if (flicker)
        {
            libLight1.gameObject.AddComponent<LightFlicker>().baseIntensity = 3.5f * mult;
            libLight2.gameObject.AddComponent<LightFlicker>().baseIntensity = 3.5f * mult;
        }

        return root;
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
            ColorUtility.TryParseHtmlString("#FFBF00", out col); // Sunset amber (canon, CONS-MAT-001)
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
            ColorUtility.TryParseHtmlString("#FFBF00", out col); // Amber (canon, CONS-MAT-001)
            intensityMultiplier = 0.6f;
        }

        return col;
    }

    // --- UTILITIES ---

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

            if (mat != null) ApplyMaterialOverride(visual, mat);

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
            if (mat != null) fallbackCube.GetComponent<MeshRenderer>().sharedMaterial = mat;
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

    // =========================================================================
    // SCHOOL GREYBOX ARCHITECTURE FACTORY METHODS (ModuleType 31-46)
    // =========================================================================

    private const float WallThickness = 0.2f;

    private static GameObject MakeSchoolModuleBase(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, float height, string materialKey, bool openFront = true, bool openBack = true, bool openLeft = false, bool openRight = false)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;
        root.transform.rotation = Quaternion.Euler(rotation);

        GreyboxModule module = root.AddComponent<GreyboxModule>();
        module.moduleType = ModuleType.SchoolHall; // will be overridden by caller
        module.dimensions = new Vector3(scale.x, height, scale.z);
        module.clearance = 1.2f;

        // Floor slab
        MakePlatform("Floor", new Vector3(0f, -0.1f, 0f), new Vector3(scale.x, 0.2f, scale.z), root.transform, GetMaterial(materialKey), SchoolFloorModule);

        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;

        // Walls - only create if not open
        if (!openBack) 
            CreateBox("Wall_Back", root.transform, new Vector3(0f, height * 0.5f, -halfZ + WallThickness * 0.5f), new Vector3(scale.x, height, WallThickness), GetMaterial(materialKey));
        if (!openFront) 
            CreateBox("Wall_Front", root.transform, new Vector3(0f, height * 0.5f, halfZ - WallThickness * 0.5f), new Vector3(scale.x, height, WallThickness), GetMaterial(materialKey));
        if (!openLeft) 
            CreateBox("Wall_Left", root.transform, new Vector3(-halfX + WallThickness * 0.5f, height * 0.5f, 0f), new Vector3(WallThickness, height, scale.z), GetMaterial(materialKey));
        if (!openRight) 
            CreateBox("Wall_Right", root.transform, new Vector3(halfX - WallThickness * 0.5f, height * 0.5f, 0f), new Vector3(WallThickness, height, scale.z), GetMaterial(materialKey));

        return root;
    }

    private static Material GetMaterial(string key)
    {
        switch (key)
        {
            case "WallTeal": return EchoesMaterialLibrary.WallTealMat;
            case "WallMustard": return EchoesMaterialLibrary.WallMustardMat;
            case "WallRose": return EchoesMaterialLibrary.WallRoseMat;
            case "WallSage": return EchoesMaterialLibrary.WallSageMat;
            case "Arch": return EchoesMaterialLibrary.ArchMat;
            case "Floor": return EchoesMaterialLibrary.FloorMat;
            case "Door": return EchoesMaterialLibrary.DoorMat;
            default: return EchoesMaterialLibrary.WallTealMat;
        }
    }

    private static GameObject CreateBox(string name, Transform parent, Vector3 localPosition, Vector3 scale, Material mat)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localScale = scale;
        cube.GetComponent<MeshRenderer>().sharedMaterial = mat;
        cube.layer = GroundLayer;
        return cube;
    }

    private static GameObject MakeSchoolHall(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, string customData)
    {
        // SchoolHall: 5.0m height, large open space
        var module = MakeSchoolModuleBase(name, pos, new Vector2(scale.x, scale.z), rotation, parent, 5f, "WallTeal", true, true);
        module.GetComponent<GreyboxModule>().moduleType = ModuleType.SchoolHall;
        
        // Add columns for large halls
        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;
        Vector3[] columns = {
            new Vector3(-halfX * 0.5f, 0f, -halfZ * 0.5f),
            new Vector3(halfX * 0.5f, 0f, -halfZ * 0.5f),
            new Vector3(-halfX * 0.5f, 0f, halfZ * 0.5f),
            new Vector3(halfX * 0.5f, 0f, halfZ * 0.5f),
        };
        foreach (var colPos in columns)
        {
            CreateBox("Column", module.transform, colPos + new Vector3(0f, 2.5f, 0f), new Vector3(0.6f, 5f, 0.6f), EchoesMaterialLibrary.ArchMat);
        }
        
        // Ceiling lights
        AddCeilingLights(module.transform, scale, 5f);
        return module;
    }

    private static GameObject MakeSchoolCorridor(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, string customData)
    {
        // SchoolCorridor: 3.2m height, long and narrow
        var module = MakeSchoolModuleBase(name, pos, new Vector2(scale.x, scale.z), rotation, parent, 3.2f, "WallTeal", true, true);
        module.GetComponent<GreyboxModule>().moduleType = ModuleType.SchoolCorridor;
        
        // Add lockers along walls if long enough
        if (scale.z > 10f)
        {
            float halfX = scale.x * 0.5f;
            float step = 4f;
            for (float z = -scale.z * 0.4f; z <= scale.z * 0.4f; z += step)
            {
                // Left lockers
                Instantiate3DModel(SchoolLockerModule, $"Locker_L_{z:0.0}", new Vector3(-halfX + 0.5f, 0.1f, z), new Vector3(0.6f, 1.8f, 0.8f), Quaternion.Euler(0f, 90f, 0f), module.transform, EchoesMaterialLibrary.ArchMat);
                // Right lockers (offset)
                Instantiate3DModel(SchoolLockerModule, $"Locker_R_{z:0.0}", new Vector3(halfX - 0.5f, 0.1f, z + step * 0.5f), new Vector3(0.6f, 1.8f, 0.8f), Quaternion.Euler(0f, -90f, 0f), module.transform, EchoesMaterialLibrary.ArchMat);
            }
        }
        
        AddCeilingLights(module.transform, scale, 3.2f);
        return module;
    }

    private static GameObject MakeSchoolClassroom(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, string customData)
    {
        // SchoolClassroom: 3.8m height
        var module = MakeSchoolModuleBase(name, pos, new Vector2(scale.x, scale.z), rotation, parent, 3.8f, "WallMustard", true, true);
        module.GetComponent<GreyboxModule>().moduleType = ModuleType.SchoolClassroom;
        
        // Add desks in rows
        AddDesks(module.transform, scale);
        AddCeilingLights(module.transform, scale, 3.8f);
        return module;
    }

    private static GameObject MakeSchoolStairwell(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, string customData)
    {
        // SchoolStairwell: 7.6m height, vertical space
        var module = MakeSchoolModuleBase(name, pos, new Vector2(scale.x, scale.z), rotation, parent, 7.6f, "WallTeal", true, true);
        module.GetComponent<GreyboxModule>().moduleType = ModuleType.SchoolStairwell;
        
        // Add stair flights
        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;
        
        // Lower landing
        MakePlatform("Landing_Lower", new Vector3(0f, -0.1f, -halfZ * 0.5f), new Vector3(scale.x, 0.2f, scale.z * 0.5f), module.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);
        
        // Stairs up
        Instantiate3DModel(SchoolStairsModule, "Stairs_Up1", new Vector3(0f, 0f, -halfZ * 0.25f), new Vector3(scale.x * 0.8f, 3.8f, scale.z * 0.5f), Quaternion.identity, module.transform, EchoesMaterialLibrary.ArchMat);
        
        // Mid landing
        MakePlatform("Landing_Mid", new Vector3(0f, 3.8f, halfZ * 0.25f), new Vector3(scale.x, 0.2f, scale.z * 0.5f), module.transform, EchoesMaterialLibrary.BridgeMat, SchoolFloorModule);
        
        // Stairs up (reversed)
        Instantiate3DModel(SchoolStairsModule, "Stairs_Up2", new Vector3(0f, 3.8f, halfZ * 0.5f), new Vector3(scale.x * 0.8f, 3.8f, scale.z * 0.5f), Quaternion.Euler(0f, 180f, 0f), module.transform, EchoesMaterialLibrary.ArchMat);
        
        // Upper landing
        MakePlatform("Landing_Upper", new Vector3(0f, 7.6f, halfZ), new Vector3(scale.x, 0.2f, scale.z * 0.5f), module.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);
        
        AddCeilingLights(module.transform, scale, 7.6f);
        return module;
    }

    private static GameObject MakeSchoolLibrary(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, string customData)
    {
        // SchoolLibrary: 5.0m height, shelves
        var module = MakeSchoolModuleBase(name, pos, new Vector2(scale.x, scale.z), rotation, parent, 5f, "WallSage", true, true);
        module.GetComponent<GreyboxModule>().moduleType = ModuleType.SchoolLibrary;
        
        // Add bookshelves
        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;
        float shelfSpacing = 4f;
        
        for (float z = -halfZ + 2f; z <= halfZ - 2f; z += shelfSpacing)
        {
            // Left shelves
            Instantiate3DModel(SchoolShelfModule, $"Shelf_L_{z:0.0}", new Vector3(-halfX + 0.5f, 0.1f, z), new Vector3(0.6f, 2.5f, 2f), Quaternion.Euler(0f, 90f, 0f), module.transform, EchoesMaterialLibrary.WallSageMat);
            // Right shelves
            Instantiate3DModel(SchoolShelfModule, $"Shelf_R_{z:0.0}", new Vector3(halfX - 0.5f, 0.1f, z), new Vector3(0.6f, 2.5f, 2f), Quaternion.Euler(0f, -90f, 0f), module.transform, EchoesMaterialLibrary.WallSageMat);
        }
        
        // Reading tables in center
        for (float z = -halfZ * 0.3f; z <= halfZ * 0.3f; z += 3f)
        {
            Instantiate3DModel(SchoolDeskModule, $"Table_{z:0.0}", new Vector3(0f, 0.1f, z), new Vector3(2f, 1f, 1.5f), Quaternion.identity, module.transform, EchoesMaterialLibrary.MemoryMat);
        }
        
        AddCeilingLights(module.transform, scale, 5f, warm: true);
        return module;
    }

    private static GameObject MakeSchoolGym(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, string customData)
    {
        // SchoolGym: 6.0m height, open space
        var module = MakeSchoolModuleBase(name, pos, new Vector2(scale.x, scale.z), rotation, parent, 6f, "WallTeal", true, true);
        module.GetComponent<GreyboxModule>().moduleType = ModuleType.SchoolGym;
        
        // Basketball hoops on walls
        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;
        
        // Court lines would be texture, but we add visual markers
        CreateBox("Court_Center", module.transform, new Vector3(0f, 0.11f, 0f), new Vector3(6f, 0.02f, 4f), EchoesMaterialLibrary.ArchMat);
        
        AddCeilingLights(module.transform, scale, 6f, high: true);
        return module;
    }

    private static GameObject MakeSchoolLab(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, string customData)
    {
        // SchoolLab: 3.8m height, technical space
        var module = MakeSchoolModuleBase(name, pos, new Vector2(scale.x, scale.z), rotation, parent, 3.8f, "Arch", true, true);
        module.GetComponent<GreyboxModule>().moduleType = ModuleType.SchoolLab;
        
        // Lab benches along walls
        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;
        
        for (float z = -halfZ + 1.5f; z <= halfZ - 1.5f; z += 2.5f)
        {
            // Left bench
            MakePlatform($"Bench_L_{z:0.0}", new Vector3(-halfX + 1f, 0.9f, z), new Vector3(1.5f, 0.1f, 2f), module.transform, EchoesMaterialLibrary.ArchMat, SchoolFloorModule);
            // Right bench
            MakePlatform($"Bench_R_{z:0.0}", new Vector3(halfX - 2.5f, 0.9f, z), new Vector3(1.5f, 0.1f, 2f), module.transform, EchoesMaterialLibrary.ArchMat, SchoolFloorModule);
        }
        
        AddCeilingLights(module.transform, scale, 3.8f, cool: true);
        return module;
    }

    private static GameObject MakeSchoolLyraClassroom(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, string customData)
    {
        // SchoolLyraClassroom: 3.8m height, semicircle desk arrangement, WallRose
        var module = MakeSchoolModuleBase(name, pos, new Vector2(scale.x, scale.z), rotation, parent, 3.8f, "WallRose", true, true);
        module.GetComponent<GreyboxModule>().moduleType = ModuleType.SchoolLyraClassroom;
        
        // Desks in semicircle
        AddDesksSemicircle(module.transform, scale);
        
        // Special desk (Lyra's) in memory-amber
        Instantiate3DModel(SchoolDeskModule, "LyraDesk", new Vector3(0f, 0.1f, scale.z * 0.3f), new Vector3(1.2f, 1f, 0.8f), Quaternion.identity, module.transform, EchoesMaterialLibrary.MemoryMat);
        Instantiate3DModel(SchoolChairModule, "LyraChair", new Vector3(0f, 0.1f, scale.z * 0.3f - 0.7f), new Vector3(0.9f, 0.9f, 0.9f), Quaternion.Euler(0f, 180f, 0f), module.transform, EchoesMaterialLibrary.ArchMat);
        
        AddCeilingLights(module.transform, scale, 3.8f, warm: true);
        return module;
    }

    private static GameObject MakeSchoolLiminalClassroom(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, string customData)
    {
        // SchoolLiminalClassroom: 3.8m height, fragmented/broken geometry
        var module = MakeSchoolModuleBase(name, pos, new Vector2(scale.x, scale.z), rotation, parent, 3.8f, "WallRose", true, true);
        module.GetComponent<GreyboxModule>().moduleType = ModuleType.SchoolLiminalClassroom;
        
        // Floating desks (broken geometry)
        AddDesksFloating(module.transform, scale);
        
        // Broken wall section
        float halfX = scale.x * 0.5f;
        CreateBox("Wall_Broken", module.transform, new Vector3(halfX - 1f, 1.9f, 0f), new Vector3(2f, 3.8f, 0.2f), EchoesMaterialLibrary.ArchMat);
        
        AddCeilingLights(module.transform, scale, 3.8f, flicker: true);
        return module;
    }

    private static GameObject MakeTransitionSpace(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, string customData)
    {
        // TransitionSpace: 3.2m height, corridor-like connector
        var module = MakeSchoolModuleBase(name, pos, new Vector2(scale.x, scale.z), rotation, parent, 3.2f, "WallTeal", true, true);
        module.GetComponent<GreyboxModule>().moduleType = ModuleType.TransitionSpace;
        
        // Minimal - just a connector
        AddCeilingLights(module.transform, scale, 3.2f);
        return module;
    }

    private static GameObject MakeSchoolEntrance(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, string customData)
    {
        // SchoolEntrance: 5.0m height, porch with columns
        var module = MakeSchoolModuleBase(name, pos, new Vector2(scale.x, scale.z), rotation, parent, 5f, "WallTeal", true, false); // back wall closed
        module.GetComponent<GreyboxModule>().moduleType = ModuleType.SchoolEntrance;
        
        // Porch columns
        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;
        Vector3[] cols = {
            new Vector3(-halfX * 0.6f, 0f, halfZ),
            new Vector3(0f, 0f, halfZ),
            new Vector3(halfX * 0.6f, 0f, halfZ),
        };
        foreach (var cp in cols)
        {
            CreateBox("Column", module.transform, cp + new Vector3(0f, 2.5f, 0f), new Vector3(0.5f, 5f, 0.5f), EchoesMaterialLibrary.ArchMat);
        }
        
        // Door opening in back wall
        // (handled by openBack=false in base, but we add a door frame)
        CreateBox("Door_Header", module.transform, new Vector3(0f, 4.2f, -halfZ + WallThickness * 0.5f), new Vector3(3f, 0.8f, WallThickness), EchoesMaterialLibrary.DoorMat);
        
        AddCeilingLights(module.transform, scale, 5f);
        return module;
    }

    private static GameObject MakeSchoolStaffRoom(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, string customData)
    {
        // SchoolStaffRoom: 3.8m height, teacher desks
        var module = MakeSchoolModuleBase(name, pos, new Vector2(scale.x, scale.z), rotation, parent, 3.8f, "WallMustard", true, true);
        module.GetComponent<GreyboxModule>().moduleType = ModuleType.SchoolStaffRoom;
        
        // Individual teacher desks
        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;
        for (float x = -halfX + 2f; x <= halfX - 2f; x += 3f)
        {
            for (float z = -halfZ + 2f; z <= halfZ - 2f; z += 3f)
            {
                Instantiate3DModel(SchoolDeskModule, $"TeacherDesk_{x:0.0}_{z:0.0}", new Vector3(x, 0.1f, z), new Vector3(1.2f, 1f, 0.8f), Quaternion.identity, module.transform, EchoesMaterialLibrary.MemoryMat);
                Instantiate3DModel(SchoolChairModule, $"TeacherChair_{x:0.0}_{z:0.0}", new Vector3(x, 0.1f, z - 0.7f), new Vector3(0.9f, 0.9f, 0.9f), Quaternion.Euler(0f, 180f, 0f), module.transform, EchoesMaterialLibrary.ArchMat);
            }
        }
        
        AddCeilingLights(module.transform, scale, 3.8f, warm: true);
        return module;
    }

    private static GameObject MakeSchoolCourtyard(string name, Vector3 pos, Vector3 scale, Vector3 rotation, Transform parent, string customData)
    {
        // SchoolCourtyard: open space, no roof, fence perimeter
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;
        root.transform.rotation = Quaternion.Euler(rotation);

        GreyboxModule module = root.AddComponent<GreyboxModule>();
        module.moduleType = ModuleType.SchoolCourtyard;
        module.dimensions = new Vector3(scale.x, 0.2f, scale.z); // ground only
        module.clearance = 1.2f;

        // Ground slab
        MakePlatform("CourtyardFloor", new Vector3(0f, -0.1f, 0f), new Vector3(scale.x, 0.2f, scale.z), root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

        // Perimeter fence
        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;
        string fenceMat = "Arch";
        
        // Front/back fences
        for (float x = -halfX; x <= halfX; x += 3f)
        {
            Instantiate3DModel(SchoolFenceModule, $"Fence_Front_{x:0.0}", new Vector3(x, 0.1f, -halfZ), new Vector3(1.2f, 1.2f, 0.2f), Quaternion.identity, root.transform, EchoesMaterialLibrary.ArchMat);
            Instantiate3DModel(SchoolFenceModule, $"Fence_Back_{x:0.0}", new Vector3(x, 0.1f, halfZ), new Vector3(1.2f, 1.2f, 0.2f), Quaternion.identity, root.transform, EchoesMaterialLibrary.ArchMat);
        }
        // Left/right fences
        for (float z = -halfZ; z <= halfZ; z += 3f)
        {
            Instantiate3DModel(SchoolFenceModule, $"Fence_Left_{z:0.0}", new Vector3(-halfX, 0.1f, z), new Vector3(0.2f, 1.2f, 1.2f), Quaternion.Euler(0f, 90f, 0f), root.transform, EchoesMaterialLibrary.ArchMat);
            Instantiate3DModel(SchoolFenceModule, $"Fence_Right_{z:0.0}", new Vector3(halfX, 0.1f, z), new Vector3(0.2f, 1.2f, 1.2f), Quaternion.Euler(0f, 90f, 0f), root.transform, EchoesMaterialLibrary.ArchMat);
        }

        // Center dead tree
        Instantiate3DModel(SchoolDeadTreeModule, "CenterTree", Vector3.zero, new Vector3(1.5f, 1.5f, 1.5f), Quaternion.identity, root.transform, EchoesMaterialLibrary.ArchMat);

        // Bench perimeter
        for (float x = -halfX + 2f; x <= halfX - 2f; x += 4f)
        {
            Instantiate3DModel(SchoolDeskModule, $"Bench_Front_{x:0.0}", new Vector3(x, 0.1f, -halfZ + 1.5f), new Vector3(2f, 0.5f, 0.6f), Quaternion.identity, root.transform, EchoesMaterialLibrary.ArchMat);
            Instantiate3DModel(SchoolDeskModule, $"Bench_Back_{x:0.0}", new Vector3(x, 0.1f, halfZ - 1.5f), new Vector3(2f, 0.5f, 0.6f), Quaternion.identity, root.transform, EchoesMaterialLibrary.ArchMat);
        }

        return root;
    }

    // Helper methods
    private static void AddCeilingLights(Transform parent, Vector3 scale, float height, bool warm = false, bool cool = false, bool flicker = false, bool high = false)
    {
        float mult;
        bool addFlicker;
        Color capColor = GetChapterColor(out mult, out addFlicker);
        
        if (warm) ColorUtility.TryParseHtmlString("#FFBF00", out capColor);
        if (cool) ColorUtility.TryParseHtmlString("#A4C2E0", out capColor);
        if (flicker || addFlicker) flicker = true;

        float spacing = Mathf.Max(3f, Mathf.Min(scale.x, scale.z) * 0.3f);
        float zStart = -scale.z * 0.4f;
        float zEnd = scale.z * 0.4f;
        float lightHeight = height - 0.3f;

        for (float z = zStart; z <= zEnd + 0.1f; z += spacing)
        {
            for (float x = -scale.x * 0.3f; x <= scale.x * 0.3f + 0.1f; x += spacing)
            {
                Light l = EchoesLevelShell.SpawnPointLight($"Light_{x:0.0}_{z:0.0}", new Vector3(x, lightHeight, z), capColor, (high ? 6f : 4f) * mult, high ? 20f : 12f, parent, LightmapBakeType.Baked, LightShadows.Soft);
                if (flicker)
                {
                    var f = l.gameObject.AddComponent<LightFlicker>();
                    f.baseIntensity = (high ? 6f : 4f) * mult;
                }
            }
        }
    }

    private static void AddDesks(Transform parent, Vector3 scale)
    {
        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;
        float deskSpacingX = 2.5f;
        float deskSpacingZ = 2.8f;
        
        int row = 0;
        for (float z = -halfZ + 1.5f; z <= halfZ - 1.5f; z += deskSpacingZ)
        {
            for (float x = -halfX + 1.5f; x <= halfX - 1.5f; x += deskSpacingX)
            {
                string name = $"Desk_R{row}_X{x:0.0}";
                GameObject d = Instantiate3DModel(SchoolDeskModule, name, new Vector3(x, 0.1f, z), new Vector3(1.2f, 1f, 0.8f), Quaternion.identity, parent, EchoesMaterialLibrary.MemoryMat);
                Instantiate3DModel(SchoolChairModule, name + "_Chair", new Vector3(x, 0.1f, z - 0.7f), new Vector3(0.9f, 0.9f, 0.9f), Quaternion.Euler(0f, 180f, 0f), parent, EchoesMaterialLibrary.ArchMat);
            }
            row++;
        }
    }

    private static void AddDesksSemicircle(Transform parent, Vector3 scale)
    {
        float radius = Mathf.Min(scale.x, scale.z) * 0.35f;
        int count = 12;
        
        for (int i = 0; i < count; i++)
        {
            float angle = -Mathf.PI * 0.5f + (Mathf.PI * i / (count - 1));
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius + scale.z * 0.2f;
            
            GameObject d = Instantiate3DModel(SchoolDeskModule, $"Desk_Semi_{i}", new Vector3(x, 0.1f, z), new Vector3(1.2f, 1f, 0.8f), Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f), parent, EchoesMaterialLibrary.MemoryMat);
            Instantiate3DModel(SchoolChairModule, $"Chair_Semi_{i}", new Vector3(x + Mathf.Sin(angle) * 0.7f, 0.1f, z - Mathf.Cos(angle) * 0.7f), new Vector3(0.9f, 0.9f, 0.9f), Quaternion.Euler(0f, (-angle + Mathf.PI) * Mathf.Rad2Deg, 0f), parent, EchoesMaterialLibrary.ArchMat);
        }
    }

    private static void AddDesksFloating(Transform parent, Vector3 scale)
    {
        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;
        
        // 3 floating desks at slightly different heights
        Vector3[] positions = {
            new Vector3(-halfX * 0.3f, 0.3f, -halfZ * 0.2f),
            new Vector3(0f, 0.5f, 0f),
            new Vector3(halfX * 0.3f, 0.2f, halfZ * 0.2f),
        };
        
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject d = Instantiate3DModel(SchoolDeskModule, $"Desk_Float_{i}", positions[i], new Vector3(1.2f, 1f, 0.8f), Quaternion.Euler(0f, Random.Range(-15f, 15f), Random.Range(-5f, 5f)), parent, EchoesMaterialLibrary.MemoryMat);
            Instantiate3DModel(SchoolChairModule, $"Chair_Float_{i}", positions[i] + new Vector3(0f, 0f, -0.7f), new Vector3(0.9f, 0.9f, 0.9f), Quaternion.Euler(0f, 180f + Random.Range(-10f, 10f), Random.Range(-5f, 5f)), parent, EchoesMaterialLibrary.ArchMat);
        }
    }
}
