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
        EchoesLevelShell.SpawnPointLight("ExitBeacon", pos + new Vector3(0f, 5f, 0f), new Color(1.0f, 0.8f, 0.4f, 1f), 6f, 28f, exitRoot.transform);
        EchoesLevelShell.SpawnPointLight("ExitGlow", pos + new Vector3(0f, 1.5f, 0f), new Color(1.0f, 0.75f, 0.35f, 1f), 4f, 14f, exitRoot.transform);

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

        Light light = EchoesLevelShell.SpawnPointLight(name, pos, color, intensity, range, parent);
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

        // Suelo del Aula
        MakePlatform("ClassroomFloor", Vector3.zero, scale, root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

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

        // Luz de techo parpadeante fluorescente
        EchoesLevelShell.SpawnPointLight("ClassroomLight", new Vector3(0f, 3f, 0f), new Color(0.85f, 0.95f, 1f, 1f), 5f, 15f, root.transform);

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

        // Plataforma de base pequeña
        MakePlatform("AnchorBase", Vector3.zero, scale, root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

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

        // Suelo del pasillo
        MakePlatform("CorridorFloor", Vector3.zero, scale, root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

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
        for (float z = -scale.z * 0.35f; z <= scale.z * 0.35f; z += 6f)
        {
            EchoesLevelShell.SpawnPointLight($"CorridorLight_{z:0.0}", new Vector3(0f, 3f, z), new Color(0.8f, 0.9f, 0.95f, 1f), 4f, 10f, root.transform);
        }

        return root;
    }

    private static GameObject MakeParadoxArena(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        // Suelo gigante del patio
        MakePlatform("PatioFloor", Vector3.zero, scale, root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

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

        // Suelo de la Oficina / Sala de Profesores
        MakePlatform("ResonanceBase", Vector3.zero, scale, root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

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
        EchoesLevelShell.SpawnPointLight("OfficeLightL", new Vector3(-scale.x * 0.3f, 2.5f, 0f), new Color(1.0f, 0.85f, 0.6f, 1f), 3f, 8f, root.transform);
        EchoesLevelShell.SpawnPointLight("OfficeLightR", new Vector3(scale.x * 0.3f, 2.5f, 0f), new Color(1.0f, 0.85f, 0.6f, 1f), 3f, 8f, root.transform);

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

        // Geometría física de la esquina del pasillo
        MakePlatform("CornerFloor", Vector3.zero, scale, root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);
        
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
        EchoesLevelShell.SpawnPointLight("StaircaseLight", new Vector3(0f, 4.5f, scale.z * 0.25f), new Color(0.9f, 0.9f, 0.85f, 1f), 3f, 8f, root.transform);

        return root;
    }

    private static GameObject MakeVoidGallery(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        // Biblioteca Escolar / Galería de Libros
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        // Suelo de la Galería
        MakePlatform("GalleryFloor", Vector3.zero, scale, root.transform, EchoesMaterialLibrary.FloorMat, SchoolFloorModule);

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
        EchoesLevelShell.SpawnPointLight("LibraryLight1", new Vector3(0f, 2.5f, -dZ * 0.5f), new Color(1.0f, 0.8f, 0.5f, 1f), 3.5f, 10f, root.transform);
        EchoesLevelShell.SpawnPointLight("LibraryLight2", new Vector3(0f, 2.5f, dZ * 0.5f), new Color(1.0f, 0.8f, 0.5f, 1f), 3.5f, 10f, root.transform);

        return root;
    }

    // --- UTILITIES ---

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
            
            MeshRenderer[] renderers = visual.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                Vector3 localCenter = container.transform.InverseTransformPoint(bounds.center);
                visual.transform.localPosition = -localCenter;
            }
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
        MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].sharedMaterial = mat;
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
}
