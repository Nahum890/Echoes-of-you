using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Editor tool: builds prefabs, materials and the 3 level scenes for "Echoes of You".
/// Menu: Echoes of You ▸ …
/// </summary>
public static class EchoesLevelBuilder
{
    // ─── paths ───────────────────────────────────────────────
    const string MAT_ROOT    = "Assets/Materials/Echoes";
    const string PREFAB_ROOT = "Assets/Prefabs";
    const string SCENE_ROOT  = "Assets/Scenes";
    const int    GROUND_LAYER = 6; // "Ground" layer index

    // ─── material cache ─────────────────────────────────────
    static Material _matFloor, _matPlate, _matDoor, _matExit, _matBridge, _matPlayer, _matEcho;

    // ================================================================
    //  MENU: 0 — Create everything at once
    // ================================================================
    [MenuItem("Echoes of You/Build All (Menu + 7 Levels)", false, 0)]
    static void BuildAll()
    {
        EnsureFolders();
        CreateMaterials();
        CreatePrefabs();
        BuildMainMenu();
        BuildLevel01();
        BuildLevel02();
        BuildLevel03();
        BuildLevel04();
        BuildLevel05();
        BuildLevel06();
        BuildLevel07();
        BuildLevel08();
        AddScenesToBuild();
        Debug.Log("[Echoes] ✔ Main menu, prefabs, materials, and 8 levels created successfully!");
    }

    // ================================================================
    //  MENU: individual items
    // ================================================================
    [MenuItem("Echoes of You/1. Create Materials", false, 20)]
    static void MenuMaterials() { EnsureFolders(); CreateMaterials(); }

    [MenuItem("Echoes of You/2. Create Prefabs", false, 21)]
    static void MenuPrefabs() { EnsureFolders(); CreateMaterials(); CreatePrefabs(); }

    [MenuItem("Echoes of You/3. Build Level 01", false, 40)]
    static void MenuL1() { EnsureFolders(); CreateMaterials(); BuildLevel01(); }

    [MenuItem("Echoes of You/4. Build Level 02", false, 41)]
    static void MenuL2() { EnsureFolders(); CreateMaterials(); BuildLevel02(); }

    [MenuItem("Echoes of You/5. Build Level 03", false, 42)]
    static void MenuL3() { EnsureFolders(); CreateMaterials(); BuildLevel03(); }

    [MenuItem("Echoes of You/6. Add Scenes to Build Settings", false, 60)]
    static void MenuBuild() { AddScenesToBuild(); }

    [MenuItem("Echoes of You/FIX — Recreate All Materials", false, 80)]
    static void MenuFixMaterials()
    {
        EnsureFolders();
        DeleteAllMaterials();
        CreateMaterials();
        Debug.Log("[Echoes] ✔ All materials recreated with Standard shader. Rebuild levels from the menu.");
    }

    [MenuItem("Echoes of You/FIX — Rebuild Everything From Scratch", false, 81)]
    static void MenuRebuildAll()
    {
        EnsureFolders();
        DeleteAllMaterials();
        // Delete old prefab so it gets recreated
        AssetDatabase.DeleteAsset(PREFAB_ROOT + "/EchoPrefab.prefab");
        CreateMaterials();
        CreatePrefabs();
        BuildMainMenu();
        BuildLevel01();
        BuildLevel02();
        BuildLevel03();
        BuildLevel04();
        BuildLevel05();
        BuildLevel06();
        BuildLevel07();
        BuildLevel08();
        AddScenesToBuild();
        Debug.Log("[Echoes] ✔ Full rebuild complete!");
    }

    // ================================================================
    //  FOLDERS
    // ================================================================
    static void EnsureFolders()
    {
        EnsureFolder(MAT_ROOT);
        EnsureFolder(PREFAB_ROOT);
        EnsureFolder(SCENE_ROOT);
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
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

    // ================================================================
    //  MATERIALS  (Built‑in Render Pipeline — Standard shader)
    // ================================================================
    static void DeleteAllMaterials()
    {
        string[] names = { "Mat_Floor", "Mat_Plate", "Mat_Door", "Mat_Exit", "Mat_Bridge", "Mat_Player", "Mat_Echo" };
        foreach (string n in names)
        {
            string p = MAT_ROOT + "/" + n + ".mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(p) != null)
                AssetDatabase.DeleteAsset(p);
        }
        AssetDatabase.Refresh();
    }

    static void CreateMaterials()
    {
        // Force Standard shader — project uses Built‑in Render Pipeline
        Shader standardShader = Shader.Find("Standard");
        if (standardShader == null)
        {
            Debug.LogError("[Echoes] Could not find Standard shader!");
            return;
        }

        _matFloor  = GetOrCreateMat("Mat_Floor",  standardShader, HexColor("1A1B26"), false);
        _matPlate  = GetOrCreateMat("Mat_Plate",  standardShader, HexColor("00E07A"), false);
        _matDoor   = GetOrCreateMat("Mat_Door",   standardShader, HexColor("E03050"), false);
        _matExit   = GetOrCreateMat("Mat_Exit",   standardShader, HexColor("FFD700"), true);
        _matBridge = GetOrCreateMat("Mat_Bridge", standardShader, HexColor("2A3B4C"), false);
        _matPlayer = GetOrCreateMat("Mat_Player", standardShader, HexColor("FFFFFF"), false);
        _matEcho   = GetOrCreateMat("Mat_Echo",   standardShader, HexColor("00FFFF", 0.5f), false, true);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Echoes] Materials created with Standard shader.");
    }

    static Material GetOrCreateMat(string name, Shader shader, Color color, bool emissive, bool transparent = false)
    {
        string path = MAT_ROOT + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null)
        {
            CacheMat(name, mat);
            return mat;
        }

        mat = new Material(shader);
        mat.name = name;
        mat.color = color; // Standard shader uses _Color

        if (emissive)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2f);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        if (transparent)
        {
            // Standard shader transparent mode
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        AssetDatabase.CreateAsset(mat, path);
        CacheMat(name, mat);
        return mat;
    }

    static void CacheMat(string name, Material mat)
    {
        switch (name)
        {
            case "Mat_Floor":  _matFloor  = mat; break;
            case "Mat_Plate":  _matPlate  = mat; break;
            case "Mat_Door":   _matDoor   = mat; break;
            case "Mat_Exit":   _matExit   = mat; break;
            case "Mat_Bridge": _matBridge = mat; break;
            case "Mat_Player": _matPlayer = mat; break;
            case "Mat_Echo":   _matEcho   = mat; break;
        }
    }

    // ================================================================
    //  PREFABS
    // ================================================================
    static void CreatePrefabs()
    {
        CreateEchoPrefab();
        Debug.Log("[Echoes] Prefabs created / verified.");
    }

    static GameObject GetEchoPrefab()
    {
        string path = PREFAB_ROOT + "/EchoPrefab.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null) return prefab;
        return CreateEchoPrefab();
    }

    static GameObject CreateEchoPrefab()
    {
        string path = PREFAB_ROOT + "/EchoPrefab.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        // Build in scene, then save as prefab
        GameObject root = new GameObject("EchoPrefab");
        root.tag = "Echo";

        var rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var cc = root.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.skinWidth = 0.08f;

        root.AddComponent<EchoPlayback>();

        // Visual child cube
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "EchoVisual";
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        visual.transform.localScale = new Vector3(0.6f, 1.8f, 0.6f);
        Object.DestroyImmediate(visual.GetComponent<BoxCollider>()); // Remove collider on visual
        if (_matEcho != null)
            visual.GetComponent<MeshRenderer>().sharedMaterial = _matEcho;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    // ================================================================
    //  LEVEL 01 — "El Primer Eco"
    // ================================================================
    static void BuildLevel01()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_01";

        // --- Environment ---
        var envParent = CreateEmpty("--- ENVIRONMENT ---", Vector3.zero);
        MakeFloor("Floor_Start",   new Vector3(0,0,0),     new Vector3(6,0.5f,6),   envParent.transform);
        MakeFloor("Bridge_1",      new Vector3(0,0,5),     new Vector3(2,0.5f,4),   envParent.transform);
        MakeFloor("Floor_Button",  new Vector3(0,0,9),     new Vector3(4,0.5f,4),   envParent.transform);
        MakeFloor("Bridge_2",      new Vector3(0,0,12.5f), new Vector3(2,0.5f,3),   envParent.transform);
        MakeFloor("Floor_Gate",    new Vector3(0,0,16),    new Vector3(4,0.5f,3),   envParent.transform);
        MakeFloor("Bridge_3",      new Vector3(0,0,19),    new Vector3(2,0.5f,3),   envParent.transform);
        MakeFloor("Floor_End",     new Vector3(0,0,23),    new Vector3(6,0.5f,6),   envParent.transform);

        // --- Mechanics ---
        var mechParent = CreateEmpty("--- MECHANICS ---", Vector3.zero);

        // Pressure plate
        var plate = MakePlate("PressurePlate_1", new Vector3(0, 0.35f, 9), new Vector3(1.5f, 0.1f, 1.5f), mechParent.transform);

        // Door
        var doorFrame = CreateEmpty("DoorFrame", new Vector3(0, 0.25f, 13.5f));
        doorFrame.transform.SetParent(mechParent.transform, true);
        var door = MakeCube("Door", Vector3.zero, new Vector3(3, 2.8f, 0.3f), doorFrame.transform, _matDoor);
        var dc = door.AddComponent<DoorController>();
        dc.plates = new PressurePlate[] { plate };
        dc.closedLocalPosition = Vector3.zero;
        dc.openLocalPosition = new Vector3(0, 2.8f, 0);
        dc.moveSpeed = 3f;

        // Level exit
        var exit = MakeCube("LevelExit", new Vector3(0, 0.5f, 25), new Vector3(2, 2, 0.3f), mechParent.transform, _matExit);
        exit.GetComponent<BoxCollider>().isTrigger = true;
        var exitComp = exit.AddComponent<LevelExit>();
        exitComp.loadNextBuildIndex = true;

        // --- Player, Camera, HUD, Light, Pause, Tutorial ---
        SpawnPlayer(new Vector3(0, 1.5f, 0));
        SpawnCamera();
        SpawnHUD();
        SpawnPauseAndTutorial();
        SpawnLight();

        // --- Tutorial Triggers (only Level 01) ---
        var tutParent = CreateEmpty("--- TUTORIAL ---", Vector3.zero);
        SpawnTutorialTrigger("Tut_Movement", new Vector3(0, 1, 0), new Vector3(4, 3, 4),
            "Usa WASD para moverte", "Espacio para saltar", 5f, tutParent.transform);
        SpawnTutorialTrigger("Tut_Button", new Vector3(0, 1, 7), new Vector3(3, 3, 2),
            "Pisa la placa verde", "Observa qué pasa con la puerta", 5f, tutParent.transform);
        SpawnTutorialTrigger("Tut_Record", new Vector3(0, 1, 9), new Vector3(3, 3, 3),
            "Mantén R para grabar tu eco", "Quédate sobre la placa mientras grabas", 6f, tutParent.transform);
        SpawnTutorialTrigger("Tut_Cross", new Vector3(0, 1, 16), new Vector3(3, 3, 2),
            "¡Tu eco mantiene la puerta abierta!", "Ahora cruza hacia la salida", 5f, tutParent.transform);
        
        SpawnParticles();

        SaveScene(scene, "Level_01");
        Debug.Log("[Echoes] Level 01 built.");
    }

    // ================================================================
    //  LEVEL 02 — "El Puente Fantasma"
    // ================================================================
    static void BuildLevel02()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_02";

        // --- Environment ---
        var envParent = CreateEmpty("--- ENVIRONMENT ---", Vector3.zero);
        MakeFloor("Floor_Start",        new Vector3(0,0,0),       new Vector3(8,0.5f,8),  envParent.transform);
        MakeFloor("Bridge_ToIsland",    new Vector3(-2.5f,0,6),   new Vector3(2,0.5f,4),  envParent.transform);
        MakeFloor("Floor_ButtonIsland", new Vector3(-5,0,12),     new Vector3(4,0.5f,4),  envParent.transform);
        MakeFloor("Floor_End",          new Vector3(0,0,20),      new Vector3(8,0.5f,8),  envParent.transform);

        // --- Mechanics ---
        var mechParent = CreateEmpty("--- MECHANICS ---", Vector3.zero);

        // Pressure plate
        var plate = MakePlate("PressurePlate_Bridge", new Vector3(-5, 0.35f, 12), new Vector3(1.5f, 0.1f, 1.5f), mechParent.transform);

        // Moving bridge
        var anchor = CreateEmpty("BridgeAnchor", new Vector3(0, 0, 8));
        anchor.transform.SetParent(mechParent.transform, true);
        var bridge = MakeCube("MovingBridge", new Vector3(0, -5, 0), new Vector3(3, 0.5f, 8), anchor.transform, _matBridge);
        bridge.layer = GROUND_LAYER;
        bridge.isStatic = false;
        var tmp = bridge.AddComponent<TimedMovingPlatform>();
        tmp.plate = plate;
        tmp.inactiveLocal = new Vector3(0, -5, 0);
        tmp.activeLocal = Vector3.zero;
        tmp.travelSpeed = 4f;

        // Level exit
        var exit = MakeCube("LevelExit", new Vector3(0, 0.5f, 23), new Vector3(2, 2, 0.3f), mechParent.transform, _matExit);
        var exitComp = exit.AddComponent<LevelExit>();
        exitComp.loadNextBuildIndex = true;
        exit.GetComponent<BoxCollider>().isTrigger = true;

        // --- Player, Camera, HUD, Light, Pause, Tutorial ---
        SpawnPlayer(new Vector3(0, 1.5f, 0));
        SpawnCamera();
        SpawnHUD();
        SpawnPauseAndTutorial();
        SpawnLight();
        SpawnParticles();

        SaveScene(scene, "Level_02");
        Debug.Log("[Echoes] Level 02 built.");
    }

    // ================================================================
    //  LEVEL 03 — "Sincronía"
    // ================================================================
    static void BuildLevel03()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_03";

        // --- Environment ---
        var envParent = CreateEmpty("--- ENVIRONMENT ---", Vector3.zero);
        MakeFloor("Floor_Central",  new Vector3(0,0,0),     new Vector3(6,0.5f,6),  envParent.transform);
        MakeFloor("Bridge_Left",    new Vector3(-4,0,4),    new Vector3(2,0.5f,5),  envParent.transform);
        MakeFloor("Bridge_Right",   new Vector3(4,0,4),     new Vector3(2,0.5f,5),  envParent.transform);
        MakeFloor("Floor_Left",     new Vector3(-8,0,6),    new Vector3(4,0.5f,4),  envParent.transform);
        MakeFloor("Floor_Right",    new Vector3(8,0,6),     new Vector3(4,0.5f,4),  envParent.transform);
        MakeFloor("Bridge_Gate",    new Vector3(0,0,10),    new Vector3(2,0.5f,4),  envParent.transform);
        MakeFloor("Floor_Gate",     new Vector3(0,0,14),    new Vector3(4,0.5f,4),  envParent.transform);
        MakeFloor("Bridge_End",     new Vector3(0,0,18),    new Vector3(2,0.5f,4),  envParent.transform);
        MakeFloor("Floor_End",      new Vector3(0,0,22),    new Vector3(6,0.5f,6),  envParent.transform);

        // --- Mechanics ---
        var mechParent = CreateEmpty("--- MECHANICS ---", Vector3.zero);

        // Pressure plates
        var plateL = MakePlate("PressurePlate_Left",  new Vector3(-8, 0.35f, 6), new Vector3(1.5f, 0.1f, 1.5f), mechParent.transform);
        var plateR = MakePlate("PressurePlate_Right", new Vector3( 8, 0.35f, 6), new Vector3(1.5f, 0.1f, 1.5f), mechParent.transform);

        // Door (AND: both plates)
        var doorFrame = CreateEmpty("DoorFrame", new Vector3(0, 0.25f, 12));
        doorFrame.transform.SetParent(mechParent.transform, true);
        var door = MakeCube("Door", Vector3.zero, new Vector3(3, 2.8f, 0.3f), doorFrame.transform, _matDoor);
        var dc = door.AddComponent<DoorController>();
        dc.plates = new PressurePlate[] { plateL, plateR };
        dc.closedLocalPosition = Vector3.zero;
        dc.openLocalPosition = new Vector3(0, 2.8f, 0);
        dc.moveSpeed = 3f;

        // Level exit
        var exit = MakeCube("LevelExit", new Vector3(0, 0.5f, 24), new Vector3(2, 2, 0.3f), mechParent.transform, _matExit);
        var exitComp = exit.AddComponent<LevelExit>();
        exitComp.loadNextBuildIndex = true;
        exit.GetComponent<BoxCollider>().isTrigger = true;

        // --- Player, Camera, HUD, Light, Pause, Tutorial ---
        SpawnPlayer(new Vector3(0, 1.5f, 0));
        SpawnCamera();
        SpawnHUD();
        SpawnPauseAndTutorial();
        SpawnLight();
        SpawnParticles();

        SaveScene(scene, "Level_03");
        Debug.Log("[Echoes] Level 03 built.");
    }

    // ================================================================
    //  LEVEL 04 — "Salto de Fe"
    // ================================================================
    static void BuildLevel04()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_04";

        var envParent = CreateEmpty("--- ENVIRONMENT ---", Vector3.zero);
        MakeFloor("Floor_Start", new Vector3(0, 0, 0), new Vector3(6, 0.5f, 6), envParent.transform);
        MakeFloor("Floor_End", new Vector3(0, 0, 18), new Vector3(6, 0.5f, 6), envParent.transform);

        var mechParent = CreateEmpty("--- MECHANICS ---", Vector3.zero);

        // Plate
        var plate = MakePlate("PressurePlate_1", new Vector3(0, 0.35f, 2), new Vector3(1.5f, 0.1f, 1.5f), mechParent.transform);

        // Moving Platform (inactive=Z:15, active=Z:6)
        var platFrame = CreateEmpty("PlatformFrame", Vector3.zero);
        platFrame.transform.SetParent(mechParent.transform, true);
        var plat = MakeCube("MovingPlatform", Vector3.zero, new Vector3(3, 0.5f, 3), platFrame.transform, _matBridge);
        plat.layer = GROUND_LAYER; // So player can jump on it
        var mp = plat.AddComponent<TimedMovingPlatform>();
        mp.plate = plate;
        mp.inactiveLocal = new Vector3(0, 0, 15);
        mp.activeLocal = new Vector3(0, 0, 6);
        mp.travelSpeed = 4f;

        // Level exit
        var exit = MakeCube("LevelExit", new Vector3(0, 0.5f, 20), new Vector3(2, 2, 0.3f), mechParent.transform, _matExit);
        var exitComp = exit.AddComponent<LevelExit>();
        exitComp.loadNextBuildIndex = true;
        exit.GetComponent<BoxCollider>().isTrigger = true;

        SpawnPlayer(new Vector3(0, 1.5f, 0));
        SpawnCamera();
        SpawnHUD();
        SpawnPauseAndTutorial();
        SpawnLight();
        SpawnParticles();

        SaveScene(scene, "Level_04");
        Debug.Log("[Echoes] Level 04 built.");
    }

    // ================================================================
    //  LEVEL 05 — "Doble Sacrificio"
    // ================================================================
    static void BuildLevel05()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_05";

        var envParent = CreateEmpty("--- ENVIRONMENT ---", Vector3.zero);
        MakeFloor("Floor_Start", new Vector3(0, 0, 0), new Vector3(6, 0.5f, 6), envParent.transform);
        MakeFloor("Bridge_1", new Vector3(0, 0, 4.5f), new Vector3(2, 0.5f, 3), envParent.transform);
        MakeFloor("Floor_Mid", new Vector3(0, 0, 9), new Vector3(6, 0.5f, 6), envParent.transform);
        MakeFloor("Bridge_2", new Vector3(0, 0, 13.5f), new Vector3(2, 0.5f, 3), envParent.transform);
        MakeFloor("Floor_End", new Vector3(0, 0, 18), new Vector3(6, 0.5f, 6), envParent.transform);

        var mechParent = CreateEmpty("--- MECHANICS ---", Vector3.zero);

        // Plate 1 -> Door 1
        var plate1 = MakePlate("PressurePlate_1", new Vector3(2, 0.35f, 0), new Vector3(1.5f, 0.1f, 1.5f), mechParent.transform);
        var doorFrame1 = CreateEmpty("DoorFrame_1", new Vector3(0, 0.25f, 6));
        doorFrame1.transform.SetParent(mechParent.transform, true);
        var door1 = MakeCube("Door_1", Vector3.zero, new Vector3(3, 2.8f, 0.3f), doorFrame1.transform, _matDoor);
        var dc1 = door1.AddComponent<DoorController>();
        dc1.plates = new PressurePlate[] { plate1 };
        dc1.closedLocalPosition = Vector3.zero;
        dc1.openLocalPosition = new Vector3(0, 8f, 0);
        dc1.moveSpeed = 4f;

        // Plate 2 -> Door 2
        var plate2 = MakePlate("PressurePlate_2", new Vector3(-2, 0.35f, 9), new Vector3(1.5f, 0.1f, 1.5f), mechParent.transform);
        var doorFrame2 = CreateEmpty("DoorFrame_2", new Vector3(0, 0.25f, 15));
        doorFrame2.transform.SetParent(mechParent.transform, true);
        var door2 = MakeCube("Door_2", Vector3.zero, new Vector3(3, 8f, 0.3f), doorFrame2.transform, _matDoor);
        var dc2 = door2.AddComponent<DoorController>();
        dc2.plates = new PressurePlate[] { plate2 };
        dc2.closedLocalPosition = Vector3.zero;
        dc2.openLocalPosition = new Vector3(0, 8f, 0);
        dc2.moveSpeed = 4f;

        var exit = MakeCube("LevelExit", new Vector3(0, 0.5f, 20), new Vector3(2, 2, 0.3f), mechParent.transform, _matExit);
        var exitComp = exit.AddComponent<LevelExit>();
        exitComp.loadNextBuildIndex = true;
        exit.GetComponent<BoxCollider>().isTrigger = true;

        SpawnPlayer(new Vector3(0, 1.5f, 0), 2); // Limit to 2 echoes
        SpawnCamera();
        SpawnHUD();
        SpawnPauseAndTutorial();
        SpawnLight();
        SpawnParticles();

        SaveScene(scene, "Level_05");
        Debug.Log("[Echoes] Level 05 built.");
    }

    // ================================================================
    //  LEVEL 06 — "El Laberinto Lógico"
    // ================================================================
    static void BuildLevel06()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_06";

        var envParent = CreateEmpty("--- ENVIRONMENT ---", Vector3.zero);
        MakeFloor("Floor_Start", new Vector3(0, 0, 0), new Vector3(8, 0.5f, 6), envParent.transform);
        MakeFloor("Floor_Mid1", new Vector3(-8, 0, 8), new Vector3(6, 0.5f, 6), envParent.transform);
        MakeFloor("Floor_Mid2", new Vector3(8, 0, 8), new Vector3(6, 0.5f, 6), envParent.transform);
        MakeFloor("Floor_End", new Vector3(0, 0, 16), new Vector3(8, 0.5f, 6), envParent.transform);
        MakeFloor("Bridge_L", new Vector3(-4, 0, 4), new Vector3(2, 0.5f, 2), envParent.transform);
        MakeFloor("Bridge_R", new Vector3(4, 0, 4), new Vector3(2, 0.5f, 2), envParent.transform);
        MakeFloor("Bridge_L2", new Vector3(-4, 0, 12), new Vector3(2, 0.5f, 2), envParent.transform);
        MakeFloor("Bridge_R2", new Vector3(4, 0, 12), new Vector3(2, 0.5f, 2), envParent.transform);

        var mechParent = CreateEmpty("--- MECHANICS ---", Vector3.zero);

        var p1 = MakePlate("Plate_L", new Vector3(-8, 0.35f, 8), new Vector3(1.5f, 0.1f, 1.5f), mechParent.transform);
        var p2 = MakePlate("Plate_R", new Vector3(8, 0.35f, 8), new Vector3(1.5f, 0.1f, 1.5f), mechParent.transform);
        var p3 = MakePlate("Plate_M", new Vector3(0, 0.35f, 0), new Vector3(1.5f, 0.1f, 1.5f), mechParent.transform);

        // Door 1 (Needs both sides pressed)
        var df1 = CreateEmpty("DoorFrame_1", new Vector3(0, 0.25f, 13));
        df1.transform.SetParent(mechParent.transform, true);
        var d1 = MakeCube("Door_1", Vector3.zero, new Vector3(3, 2.8f, 0.3f), df1.transform, _matDoor);
        var dc1 = d1.AddComponent<DoorController>();
        dc1.plates = new PressurePlate[] { p1, p2 };
        dc1.closedLocalPosition = Vector3.zero;
        dc1.openLocalPosition = new Vector3(0, 8f, 0);
        dc1.moveSpeed = 4f;

        // Door 2 (Needs start plate pressed)
        var df2 = CreateEmpty("DoorFrame_2", new Vector3(0, 0.25f, 14.5f));
        df2.transform.SetParent(mechParent.transform, true);
        var d2 = MakeCube("Door_2", Vector3.zero, new Vector3(3, 8f, 0.3f), df2.transform, _matDoor);
        var dc2 = d2.AddComponent<DoorController>();
        dc2.plates = new PressurePlate[] { p3 };
        dc2.closedLocalPosition = Vector3.zero;
        dc2.openLocalPosition = new Vector3(0, 8f, 0);
        dc2.moveSpeed = 4f;

        var exit = MakeCube("LevelExit", new Vector3(0, 0.5f, 18), new Vector3(2, 2, 0.3f), mechParent.transform, _matExit);
        var exitComp = exit.AddComponent<LevelExit>();
        exitComp.loadNextBuildIndex = true;
        exit.GetComponent<BoxCollider>().isTrigger = true;

        SpawnPlayer(new Vector3(0, 1.5f, -2), 3);
        SpawnCamera();
        SpawnHUD();
        SpawnPauseAndTutorial();
        SpawnLight();
        SpawnParticles();

        SaveScene(scene, "Level_06");
        Debug.Log("[Echoes] Level 06 built.");
    }

    // ================================================================
    //  LEVEL 07 — "Elevador Sincronizado"
    // ================================================================
    static void BuildLevel07()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_07";

        var envParent = CreateEmpty("--- ENVIRONMENT ---", Vector3.zero);
        MakeFloor("Floor_Start", new Vector3(0, 0, 0), new Vector3(6, 0.5f, 6), envParent.transform);
        MakeFloor("Floor_End", new Vector3(0, 0, 24), new Vector3(6, 0.5f, 6), envParent.transform);

        var mechParent = CreateEmpty("--- MECHANICS ---", Vector3.zero);

        var p1 = MakePlate("Plate_1", new Vector3(-2, 0.35f, 0), new Vector3(1f, 0.1f, 1f), mechParent.transform);
        var p2 = MakePlate("Plate_2", new Vector3(2, 0.35f, 0), new Vector3(1f, 0.1f, 1f), mechParent.transform);

        // Platform 1 (Moves far)
        var pf1 = CreateEmpty("PlatFrame_1", Vector3.zero);
        pf1.transform.SetParent(mechParent.transform, true);
        var plat1 = MakeCube("MovingPlatform_1", Vector3.zero, new Vector3(3, 0.5f, 3), pf1.transform, _matBridge);
        plat1.layer = GROUND_LAYER;
        var mp1 = plat1.AddComponent<TimedMovingPlatform>();
        mp1.plate = p1;
        mp1.inactiveLocal = new Vector3(0, 0, 5);
        mp1.activeLocal = new Vector3(0, 0, 12);
        mp1.travelSpeed = 5f;

        // Platform 2 (Moves from far to end)
        var pf2 = CreateEmpty("PlatFrame_2", Vector3.zero);
        pf2.transform.SetParent(mechParent.transform, true);
        var plat2 = MakeCube("MovingPlatform_2", Vector3.zero, new Vector3(3, 0.5f, 3), pf2.transform, _matBridge);
        plat2.layer = GROUND_LAYER;
        var mp2 = plat2.AddComponent<TimedMovingPlatform>();
        mp2.plate = p2;
        mp2.inactiveLocal = new Vector3(0, 0, 19);
        mp2.activeLocal = new Vector3(0, 0, 12);
        mp2.travelSpeed = 5f;

        var exit = MakeCube("LevelExit", new Vector3(0, 0.5f, 26), new Vector3(2, 2, 0.3f), mechParent.transform, _matExit);
        var exitComp = exit.AddComponent<LevelExit>();
        exitComp.loadNextBuildIndex = true;

        SpawnPlayer(new Vector3(0, 1.5f, -2), 3);
        SpawnCamera();
        SpawnHUD();
        SpawnPauseAndTutorial();
        SpawnLight();
        SpawnParticles();

        SaveScene(scene, "Level_07");
        Debug.Log("[Echoes] Level 07 built.");
    }

    // ================================================================
    //  LEVEL 08 — "La Carrera del Eco"
    // ================================================================
    static void BuildLevel08()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_08";

        var envParent = CreateEmpty("--- ENVIRONMENT ---", Vector3.zero);
        MakeFloor("Floor_Path", new Vector3(0, 0, 10), new Vector3(4, 0.5f, 22), envParent.transform);
        
        var mechParent = CreateEmpty("--- MECHANICS ---", Vector3.zero);

        // Sequence of 3 doors and 3 plates
        var p1 = MakePlate("Plate_1", new Vector3(0, 0.35f, 4), new Vector3(1.5f, 0.1f, 1.5f), mechParent.transform);
        var d1GO = CreateEmpty("DoorFrame_1", new Vector3(0, 0.25f, 8));
        d1GO.transform.SetParent(mechParent.transform, true);
        var d1C = MakeCube("Door_1", Vector3.zero, new Vector3(3, 8f, 0.3f), d1GO.transform, _matDoor);
        var dc1 = d1C.AddComponent<DoorController>();
        dc1.plates = new PressurePlate[] { p1 };
        dc1.closedLocalPosition = Vector3.zero;
        dc1.openLocalPosition = new Vector3(0, 8f, 0);
        dc1.moveSpeed = 6f;

        var p2 = MakePlate("Plate_2", new Vector3(0, 0.35f, 12), new Vector3(1.5f, 0.1f, 1.5f), mechParent.transform);
        var d2GO = CreateEmpty("DoorFrame_2", new Vector3(0, 0.25f, 16));
        d2GO.transform.SetParent(mechParent.transform, true);
        var d2C = MakeCube("Door_2", Vector3.zero, new Vector3(3, 8f, 0.3f), d2GO.transform, _matDoor);
        var dc2 = d2C.AddComponent<DoorController>();
        dc2.plates = new PressurePlate[] { p2 };
        dc2.closedLocalPosition = Vector3.zero;
        dc2.openLocalPosition = new Vector3(0, 8f, 0);
        dc2.moveSpeed = 6f;

        var p3 = MakePlate("Plate_3", new Vector3(0, 0.35f, 19), new Vector3(1.5f, 0.1f, 1.5f), mechParent.transform);
        var d3GO = CreateEmpty("DoorFrame_3", new Vector3(0, 0.25f, 22));
        d3GO.transform.SetParent(mechParent.transform, true);
        var d3C = MakeCube("Door_3", Vector3.zero, new Vector3(3, 8f, 0.3f), d3GO.transform, _matDoor);
        var dc3 = d3C.AddComponent<DoorController>();
        dc3.plates = new PressurePlate[] { p3 };
        dc3.closedLocalPosition = Vector3.zero;
        dc3.openLocalPosition = new Vector3(0, 8f, 0);
        dc3.moveSpeed = 6f;

        var exit = MakeCube("LevelExit", new Vector3(0, 0.5f, 26), new Vector3(3, 2, 0.3f), mechParent.transform, _matExit);
        var exitComp = exit.AddComponent<LevelExit>();
        exitComp.loadNextBuildIndex = false;
        exitComp.nextSceneName = "MainMenu";
        exit.GetComponent<BoxCollider>().isTrigger = true;

        SpawnPlayer(new Vector3(0, 1.5f, 0), 1, 15f); // 1 echo max, 15 seconds to run the whole track
        SpawnCamera();
        SpawnHUD();
        SpawnPauseAndTutorial();
        SpawnLight();
        SpawnParticles();

        SaveScene(scene, "Level_08");
        Debug.Log("[Echoes] Level 08 built.");
    }

    // ================================================================
    //  BUILD SETTINGS
    // ================================================================
    static void AddScenesToBuild()
    {
        string[] scenePaths = new[]
        {
            SCENE_ROOT + "/MainMenu.unity",
            SCENE_ROOT + "/Level_01.unity",
            SCENE_ROOT + "/Level_02.unity",
            SCENE_ROOT + "/Level_03.unity",
            SCENE_ROOT + "/Level_04.unity",
            SCENE_ROOT + "/Level_05.unity",
            SCENE_ROOT + "/Level_06.unity",
            SCENE_ROOT + "/Level_07.unity",
            SCENE_ROOT + "/Level_08.unity"
        };

        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();

        foreach (string sp in scenePaths)
        {
            if (!File.Exists(sp) && !File.Exists(Application.dataPath + "/../" + sp))
            {
                // Try the full path
                string fullPath = Path.GetFullPath(sp);
                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning($"[Echoes] Scene not found, skipping: {sp}");
                    continue;
                }
            }
            list.Add(new EditorBuildSettingsScene(sp, true));
        }

        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log($"[Echoes] Build Settings updated with {list.Count} scenes.");
    }

    // ================================================================
    //  HELPERS — SPAWNERS
    // ================================================================
    static void SpawnPlayer(Vector3 position, int maxEchoes = 3, float maxRecordSeconds = 10f)
    {
        var parent = CreateEmpty("--- PLAYER ---", Vector3.zero);

        GameObject player = new GameObject("Player");
        player.transform.SetParent(parent.transform, false);
        player.transform.position = position;
        player.tag = "Player";

        var rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0, 0.9f, 0);
        cc.skinWidth = 0.08f;

        player.AddComponent<PlayerController>();

        var recorder = player.AddComponent<EchoRecorder>();
        GameObject echoPrefab = GetEchoPrefab();
        SetPrivateField(recorder, "echoPrefab", echoPrefab);
        SetPrivateField(recorder, "maxEchoes", maxEchoes);
        SetPrivateField(recorder, "maxRecordSeconds", maxRecordSeconds);

        // Visual child
        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "PlayerVisual";
        visual.transform.SetParent(player.transform, false);
        visual.transform.localPosition = new Vector3(0, 0.9f, 0);
        visual.transform.localScale = new Vector3(0.6f, 1.8f, 0.6f);
        Object.DestroyImmediate(visual.GetComponent<BoxCollider>());
        if (_matPlayer != null)
            visual.GetComponent<MeshRenderer>().sharedMaterial = _matPlayer;

        // GroundCheck child
        var gc = new GameObject("GroundCheck");
        gc.transform.SetParent(player.transform, false);
        gc.transform.localPosition = new Vector3(0, 0.1f, 0);
    }

    static void SpawnCamera()
    {
        var parent = CreateEmpty("--- CAMERA ---", Vector3.zero);

        // Find or create camera
        Camera cam = Object.FindFirstObjectByType<Camera>();
        GameObject camGO;
        if (cam != null)
        {
            camGO = cam.gameObject;
        }
        else
        {
            camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
        }
        camGO.transform.SetParent(parent.transform, false);
        camGO.transform.position = new Vector3(0, 4, -6);

        // Estética: Sin Skybox por defecto, color oscuro
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = HexColor("080A12");
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = HexColor("151520");

        // ThirdPersonCamera
        var tpc = camGO.GetComponent<ThirdPersonCamera>();
        if (tpc == null) tpc = camGO.AddComponent<ThirdPersonCamera>();

        // Find the Player object to assign as target
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            tpc.target = player.transform;
    }

    static void SpawnHUD()
    {
        var parent = CreateEmpty("--- UI ---", Vector3.zero);
        var hudGO = new GameObject("GameHUD");
        hudGO.transform.SetParent(parent.transform, false);
        hudGO.AddComponent<GameHUD>();
    }

    static void SpawnPauseAndTutorial()
    {
        var uiParent = GameObject.Find("--- UI ---");
        Transform parent = uiParent != null ? uiParent.transform : null;

        var pauseGO = new GameObject("PauseMenu");
        if (parent != null) pauseGO.transform.SetParent(parent, false);
        pauseGO.AddComponent<PauseMenu>();

        var tutGO = new GameObject("TutorialHUD");
        if (parent != null) tutGO.transform.SetParent(parent, false);
        tutGO.AddComponent<TutorialHUD>();
    }

    static void SpawnTutorialTrigger(string name, Vector3 pos, Vector3 size,
        string msg, string hint, float duration, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;

        var col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;

        var trig = go.AddComponent<TutorialTrigger>();
        SetPrivateField(trig, "message", msg);
        SetPrivateField(trig, "hint", hint);
        SetPrivateField(trig, "displayDuration", duration);
        SetPrivateField(trig, "oneShot", true);
    }

    // ================================================================
    //  MAIN MENU SCENE
    // ================================================================
    static void BuildMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        // Camera
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();
        camGO.transform.position = new Vector3(0, 1, -10);

        // Menu Script
        var menuGO = new GameObject("MainMenu");
        menuGO.AddComponent<MainMenu>();

        // Aesthetic background (since skybox is removed)
        camGO.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        camGO.GetComponent<Camera>().backgroundColor = HexColor("080A12");
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = HexColor("151520");

        // Light & Particles
        SpawnLight();
        SpawnParticles();

        SaveScene(scene, "MainMenu");
        Debug.Log("[Echoes] MainMenu built.");
    }

    static void SpawnLight()
    {
        var lightGO = new GameObject("DirectionalLight");
        lightGO.transform.position = new Vector3(0, 10, 0);
        lightGO.transform.eulerAngles = new Vector3(50, -30, 0);
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        // Warm sunset look for aesthetics
        light.color = HexColor("FFD5A1");
        light.intensity = 1.3f; // Slightly lowered to reduce blowout
        light.shadows = LightShadows.Soft;
    }

    static void SpawnParticles()
    {
        var psGO = new GameObject("EnvironmentParticles");
        psGO.transform.position = new Vector3(0, 2, 8);
        var ps = psGO.AddComponent<ParticleSystem>();

        var main = ps.main;
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = _matEcho; // Usar material del eco para brillo

        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.maxParticles = 50;

        var em = ps.emission;
        em.rateOverTime = 8f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(20, 10, 25);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.y = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
        vel.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
        vel.z = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);

        var color = ps.colorOverLifetime;
        color.enabled = true;

        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) }
        );
        color.color = new ParticleSystem.MinMaxGradient(g);
    }

    // ================================================================
    //  HELPERS — GEOMETRY
    // ================================================================

    static GameObject MakeCube(string name, Vector3 localPos, Vector3 scale, Transform parent, Material mat, string fbxModelPath = null)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPos;
        cube.transform.localScale = scale;

        if (!string.IsNullOrEmpty(fbxModelPath))
        {
            var pfb = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(fbxModelPath);
            if (pfb != null)
            {
                cube.GetComponent<MeshRenderer>().enabled = false;
                var vis = UnityEditor.PrefabUtility.InstantiatePrefab(pfb) as GameObject;
                vis.transform.SetParent(cube.transform, false);
                vis.transform.localPosition = Vector3.zero;
                vis.transform.localScale = Vector3.one; // Inherit parent bounds perfectly
            }
            else
            {
                if (mat != null) cube.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }
        }
        else
        {
            if (mat != null) cube.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        return cube;
    }

    static void MakeFloor(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        var cube = MakeCube(name, pos, scale, parent, _matFloor, "Assets/3D Models/Models/FBX format/block-grass.fbx");
        cube.isStatic = true;
        cube.layer = GROUND_LAYER;
    }

    static PressurePlate MakePlate(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        var cube = MakeCube(name, pos, scale, parent, _matPlate, "Assets/3D Models/Models/FBX format/button-square.fbx");
        cube.layer = GROUND_LAYER;
        var pp = cube.AddComponent<PressurePlate>();
        return pp;
    }

    static GameObject CreateEmpty(string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        return go;
    }

    // ================================================================
    //  HELPERS — SCENE
    // ================================================================
    static void SaveScene(Scene scene, string sceneName)
    {
        string path = SCENE_ROOT + "/" + sceneName + ".unity";
        EditorSceneManager.SaveScene(scene, path);
    }

    // ================================================================
    //  HELPERS — REFLECTION (set [SerializeField] private fields)
    // ================================================================
    static void SetPrivateField<T>(Component comp, string fieldName, T value)
    {
        var so = new SerializedObject(comp);
        var prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogWarning($"[Echoes] Field '{fieldName}' not found on {comp.GetType().Name}");
            return;
        }

        switch (value)
        {
            case int i:
                prop.intValue = i;
                break;
            case float f:
                prop.floatValue = f;
                break;
            case bool b:
                prop.boolValue = b;
                break;
            case string s:
                prop.stringValue = s;
                break;
            case Vector3 v:
                prop.vector3Value = v;
                break;
            case GameObject go:
                prop.objectReferenceValue = go;
                break;
            case PressurePlate pp:
                prop.objectReferenceValue = pp;
                break;
            case PressurePlate[] ppArr:
                prop.arraySize = ppArr.Length;
                for (int idx = 0; idx < ppArr.Length; idx++)
                    prop.GetArrayElementAtIndex(idx).objectReferenceValue = ppArr[idx];
                break;
            default:
                prop.objectReferenceValue = value as Object;
                break;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ================================================================
    //  HELPERS — COLOR
    // ================================================================
    static Color HexColor(string hex, float alpha = 1f)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color c))
        {
            c.a = alpha;
            return c;
        }
        return new Color(1, 0, 1, alpha); // magenta fallback
    }
}
