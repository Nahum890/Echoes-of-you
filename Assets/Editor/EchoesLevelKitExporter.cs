using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Genera prefabs editables en Assets/Prefabs/LevelKit para paths, placas y tutoriales.
/// También regenera los prefabs PMK obsoletos de PlayableMegakit usando primitivas de URP.
/// </summary>
public static class EchoesLevelKitExporter
{
    const string KitRoot = "Assets/Prefabs/LevelKit";
    const string PmkRoot = "Assets/Prefabs/LevelKit/PlayableMegakit";

    [MenuItem("Echoes of You/Production/Export Level Kit Prefabs", false, 210)]
    public static void ExportLevelKitPrefabs()
    {
        if (!Directory.Exists(KitRoot))
            Directory.CreateDirectory(KitRoot);
        if (!Directory.Exists(PmkRoot))
            Directory.CreateDirectory(PmkRoot);

        // Exportar standard Level Kit
        SavePrefab(CreateEchoPathHint(), $"{KitRoot}/EchoPathHint.prefab");
        SavePrefab(CreatePressurePlate("PressurePlate_Eco", true), $"{KitRoot}/PressurePlate_Eco.prefab");
        SavePrefab(CreatePressurePlate("PressurePlate_Player", false), $"{KitRoot}/PressurePlate_Player.prefab");
        SavePrefab(CreateTutorialTrigger(), $"{KitRoot}/TutorialTrigger.prefab");

        // Exportar PlayableMegakit (PMK)
        ExportPlayableMegakitPrefabs();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Echoes] All Level Kit and PMK prefabs exported. Visual elements now use URP procedural primitives.");
    }

    static GameObject CreateEchoPathHint()
    {
        GameObject root = new GameObject("EchoPathHint");
        EchoPathHint hint = root.AddComponent<EchoPathHint>();
        if (hint != null)
        {
            hint.SetWaypoints(new[]
            {
                new Vector3(0f, 0.5f, 0f),
                new Vector3(2f, 0.5f, 2f),
                new Vector3(4f, 0.5f, 4f)
            });
        }
        else
        {
            Debug.LogWarning("[Echoes Exporter] Could not add EchoPathHint component during Editor prefab export. It will be wired at runtime.");
        }
        return root;
    }

    static GameObject CreatePressurePlate(string name, bool echoPlate)
    {
        GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        plate.name = name;
        plate.transform.localScale = new Vector3(2f, 0.12f, 2f);
        Object.DestroyImmediate(plate.GetComponent<Collider>());
        plate.AddComponent<PressurePlate>();
        PressurePlateAlignment align = plate.AddComponent<PressurePlateAlignment>();
        align.echoProjectionPlate = echoPlate;

        // Asignar material
        Renderer r = plate.GetComponent<Renderer>();
        if (r != null)
            r.sharedMaterial = EchoesMaterialLibrary.PlateMat;

        return plate;
    }

    static GameObject CreateTutorialTrigger()
    {
        GameObject root = new GameObject("TutorialTrigger");
        BoxCollider box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(8f, 4f, 8f);
        root.AddComponent<TutorialTrigger>();
        return root;
    }

    static void ExportPlayableMegakitPrefabs()
    {
        // 1. PMK_AccessBridge_4x3
        SavePrefab(CreatePmkSimpleCube("PMK_AccessBridge_4x3", "AccessBridge", new Vector3(4f, 0.5f, 3f), new Vector3(4f, 2.5f, 3f), EchoesMaterialLibrary.BridgeMat), $"{PmkRoot}/PMK_AccessBridge_4x3.prefab");

        // 2. PMK_BackdropColumn_Visual
        SavePrefab(CreatePmkCompositeCube("PMK_BackdropColumn_Visual", "Backdrop", new Vector3(0.55f, 10f, 0.55f), new Vector3(0.55f, 12f, 0.55f), false, false, false, EchoesMaterialLibrary.ArchMat), $"{PmkRoot}/PMK_BackdropColumn_Visual.prefab");

        // 3. PMK_DoorFrame_8x4
        SavePrefab(CreatePmkCompositeCube("PMK_DoorFrame_8x4", "DoorFrame", new Vector3(8f, 3.8f, 0.55f), new Vector3(8f, 5.8f, 0.55f), false, true, false, EchoesMaterialLibrary.DoorMat), $"{PmkRoot}/PMK_DoorFrame_8x4.prefab");

        // 4. PMK_EchoLane_4x48
        SavePrefab(CreatePmkSimpleCube("PMK_EchoLane_4x48", "EchoLane", new Vector3(4f, 0.55f, 48f), new Vector3(4f, 2.55f, 48f), EchoesMaterialLibrary.EchoMat), $"{PmkRoot}/PMK_EchoLane_4x48.prefab");

        // 5. PMK_LightPod_Visual
        SavePrefab(CreatePmkCompositeCube("PMK_LightPod_Visual", "LightPod", new Vector3(0.6f, 0.6f, 0.6f), new Vector3(0.6f, 2.6f, 0.6f), false, false, false, EchoesMaterialLibrary.GoalMat), $"{PmkRoot}/PMK_LightPod_Visual.prefab");

        // 6. PMK_LowParkourStep_3x3
        SavePrefab(CreatePmkSimpleCube("PMK_LowParkourStep_3x3", "ParkourStep", new Vector3(3f, 0.45f, 3f), new Vector3(3f, 2.45f, 3f), EchoesMaterialLibrary.BridgeMat), $"{PmkRoot}/PMK_LowParkourStep_3x3.prefab");

        // 7. PMK_MainDeck_14x10
        SavePrefab(CreatePmkSimpleCube("PMK_MainDeck_14x10", "WalkableDeck", new Vector3(14f, 0.6f, 10f), new Vector3(14f, 2.6f, 10f), EchoesMaterialLibrary.FloorMat), $"{PmkRoot}/PMK_MainDeck_14x10.prefab");

        // 8. PMK_PressurePlate_EchoOnly
        SavePrefab(CreatePmkPressurePlate(), $"{PmkRoot}/PMK_PressurePlate_EchoOnly.prefab");

        // 9. PMK_TimedBridge_6x7
        SavePrefab(CreatePmkSimpleCube("PMK_TimedBridge_6x7", "TimedBridge", new Vector3(6f, 0.55f, 7f), new Vector3(6f, 2.55f, 7f), EchoesMaterialLibrary.BridgeMat), $"{PmkRoot}/PMK_TimedBridge_6x7.prefab");

        // 10. PMK_WideDeck_18x12
        SavePrefab(CreatePmkSimpleCube("PMK_WideDeck_18x12", "WalkableDeck", new Vector3(18f, 0.6f, 12f), new Vector3(18f, 2.6f, 12f), EchoesMaterialLibrary.FloorMat), $"{PmkRoot}/PMK_WideDeck_18x12.prefab");
    }

    static GameObject CreatePmkSimpleCube(string name, string role, Vector3 footprint, Vector3 clearance, Material mat)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.localScale = footprint;
        
        // Ajustar material
        Renderer r = cube.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = mat;

        // Configurar LevelKitPiece
        LevelKitPiece piece = cube.AddComponent<LevelKitPiece>();
        piece.Configure(name, role, true, false, footprint, clearance);

        // Asegurar BoxCollider correcto
        BoxCollider box = cube.GetComponent<BoxCollider>();
        if (box != null)
        {
            box.size = Vector3.one;
            box.center = Vector3.zero;
        }

        return cube;
    }

    static GameObject CreatePmkCompositeCube(string name, string role, Vector3 footprint, Vector3 clearance, bool walkable, bool cameraOccluder, bool requiresCollider, Material mat)
    {
        GameObject root = new GameObject(name);
        
        // Colisionador y Lógica en la raíz
        BoxCollider box = root.AddComponent<BoxCollider>();
        box.size = footprint;
        box.center = Vector3.zero;
        box.isTrigger = !requiresCollider && !walkable;

        LevelKitPiece piece = root.AddComponent<LevelKitPiece>();
        piece.Configure(name, role, walkable, cameraOccluder, footprint, clearance);
        piece.requiresGameplayCollider = requiresCollider;

        // Visualización como hijo
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = name + "_Visual";
        visual.transform.SetParent(root.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = footprint;

        // Destruir colisionador duplicado en hijo visual
        Object.DestroyImmediate(visual.GetComponent<Collider>());

        // Ajustar material
        Renderer r = visual.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = mat;

        return root;
    }

    static GameObject CreatePmkPressurePlate()
    {
        string name = "PMK_PressurePlate_EchoOnly";
        GameObject root = new GameObject(name);

        Vector3 footprint = new Vector3(2f, 0.12f, 2f);
        Vector3 clearance = new Vector3(2f, 2.2f, 2f);

        // Colisionador en la raíz
        BoxCollider box = root.AddComponent<BoxCollider>();
        box.size = footprint;
        box.center = Vector3.zero;
        box.isTrigger = true;

        // Lógica de placa en la raíz
        PressurePlate plate = root.AddComponent<PressurePlate>();
        plate.acceptPlayer = false;
        plate.acceptEcho = true;
        plate.acceptEchoProjection = true;
        plate.inactiveColor = new Color(0.16f, 0.21f, 0.31f, 1f);
        plate.activeColor = new Color(0f, 0.9f, 1f, 1f);
        plate.emissionInactive = new Color(0f, 0.1f, 0.17f, 1f);
        plate.emissionActive = new Color(0f, 1.33f, 1.6f, 1f);
        plate.pulseSpeed = 2f;
        plate.createIndicatorLight = true;
        plate.lightIntensity = 0.85f;
        plate.lightRange = 4.5f;
        plate.autoReleaseTimer = 0f;

        PressurePlateAlignment align = root.AddComponent<PressurePlateAlignment>();
        align.echoProjectionPlate = true;
        align.surfaceOffset = 0.08f;
        align.echoTriggerHeight = 1.65f;
        
        LevelKitPiece piece = root.AddComponent<LevelKitPiece>();
        piece.Configure(name, "EchoOnlyPlate", false, false, footprint, clearance);
        piece.requiresGameplayCollider = false;

        // Visualización como hijo (Cilindro plano)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = name + "_Visual";
        visual.transform.SetParent(root.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = new Vector3(2f, 0.06f, 2f); // Delgada placa circular

        Object.DestroyImmediate(visual.GetComponent<Collider>());

        Renderer r = visual.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = EchoesMaterialLibrary.PlateMat;

        return root;
    }

    static void SavePrefab(GameObject source, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(source, path);
        Object.DestroyImmediate(source);
    }
}
