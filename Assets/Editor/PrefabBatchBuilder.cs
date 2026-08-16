using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility that generates the 41 base prefabs (14 Architecture + 19 Narrative Props
/// + 1 Fluorescent Light + extras) using Kenney Furniture Kit models and EchoesMaterialLibrary.
/// Menu: Echoes of You ▸ Build Base Prefabs
/// </summary>
public static class PrefabBatchBuilder
{
    private const string KenneyRoot = "Assets/3D Models/kenney_furniture-kit/Models/FBX format";
    private const string ArchFolder = "Assets/Prefabs/Architecture";
    private const string NarrativeFolder = "Assets/Prefabs/Props/Narrative";
    private const string LightingFolder = "Assets/Prefabs/Lighting";

    // ──────────────────────────────────────────────
    //  ENTRY POINT
    // ──────────────────────────────────────────────

    [MenuItem("Echoes of You/Build Base Prefabs")]
    public static void BuildAllPrefabs()
    {
        EchoesMaterialLibrary.EnsureMaterials();
        EnsureFolder(ArchFolder);
        EnsureFolder(NarrativeFolder);
        EnsureFolder(LightingFolder);

        BuildArchitecturePrefabs();
        BuildNarrativePropPrefabs();
        BuildFluorescentPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[PrefabBatchBuilder] ✓ All base prefabs built successfully.");
    }

    // ──────────────────────────────────────────────
    //  ARCHITECTURE (14 prefabs)
    // ──────────────────────────────────────────────

    private static void BuildArchitecturePrefabs()
    {
        BuildArch("Arch_Floor",      "floorFull",      "FloorMat",      null);
        BuildArch("Arch_Wall",       "wall",           "WallTealMat",   null);
        BuildArch("Arch_WallWindow", "wallWindow",     "WallTealMat",   null);
        BuildArch("Arch_Doorway",    "wallDoorway",    "DoorMat",       SetupDoorway);
        BuildArch("Arch_Column",     "wallCorner",     "ArchMat",       null);
        BuildArch("Arch_Stairs",     "stairs",         "ArchMat",       null);
        BuildArch("Arch_Locker",     "bookcaseClosed", "ArchMat",       null);
        BuildArch("Arch_Shelf",      "bookcaseOpen",   "WallMustardMat",null);
        BuildArch("Arch_Desk",       "desk",           "MemoryMat",     null);
        BuildArchWithFallback("Arch_Chair", "chairDesk", "chair", "ArchMat");
        BuildArch("Arch_Bench",      "bench",          "ArchMat",       null);
        BuildArch("Arch_Trashcan",   "trashcan",       "ArchMat",       null);

        // Fence — no Kenney model, use primitive
        BuildArchPrimitive("Arch_Fence", PrimitiveType.Cube, new Vector3(0.1f, 1.2f, 2f), "ArchMat");

        // Tree — no Kenney model, composite primitive
        BuildArchTree();

        Debug.Log("[PrefabBatchBuilder]   Architecture: 14 prefabs done.");
    }

    private delegate void PostSetup(GameObject go);

    private static void BuildArch(string name, string kenneyModel, string matToken, PostSetup post)
    {
        GameObject go = LoadKenneyOrPrimitive(kenneyModel, name, PrimitiveType.Cube, Vector3.one);
        go.name = name;

        Material mat = ResolveMaterial(matToken);
        ApplyMaterial(go, mat);
        EnsureBoxCollider(go);

        var piece = go.AddComponent<ArchitecturePiece>();
        piece.materialToken = matToken;
        piece.useKenneyTiling = true;
        piece.pieceId = name;

        post?.Invoke(go);
        SavePrefab(go, ArchFolder, name);
    }

    private static void BuildArchWithFallback(string name, string primary, string fallback, string matToken)
    {
        GameObject go = LoadKenneyModel(primary);
        if (go == null)
            go = LoadKenneyModel(fallback);
        if (go == null)
            go = MakePrimitive(name, PrimitiveType.Cube, Vector3.one);

        go.name = name;
        ApplyMaterial(go, ResolveMaterial(matToken));
        EnsureBoxCollider(go);

        var piece = go.AddComponent<ArchitecturePiece>();
        piece.materialToken = matToken;
        piece.useKenneyTiling = true;
        piece.pieceId = name;

        SavePrefab(go, ArchFolder, name);
    }

    private static void BuildArchPrimitive(string name, PrimitiveType type, Vector3 scale, string matToken)
    {
        GameObject go = MakePrimitive(name, type, scale);
        ApplyMaterial(go, ResolveMaterial(matToken));
        // BoxCollider already on primitives

        var piece = go.AddComponent<ArchitecturePiece>();
        piece.materialToken = matToken;
        piece.useKenneyTiling = false;
        piece.pieceId = name;

        SavePrefab(go, ArchFolder, name);
    }

    private static void BuildArchTree()
    {
        GameObject root = new GameObject("Arch_Tree");

        // Trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(root.transform);
        trunk.transform.localPosition = Vector3.zero;
        trunk.transform.localScale = new Vector3(0.15f, 2f, 0.15f);

        // Canopy
        GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy.name = "Canopy";
        canopy.transform.SetParent(root.transform);
        canopy.transform.localPosition = new Vector3(0f, 2.8f, 0f);
        canopy.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);

        Material mat = ResolveMaterial("ArchMat");
        ApplyMaterial(root, mat);

        // Single collider on root
        var col = root.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 1.5f, 0f);
        col.size = new Vector3(1.6f, 4f, 1.6f);

        var piece = root.AddComponent<ArchitecturePiece>();
        piece.materialToken = "ArchMat";
        piece.useKenneyTiling = false;
        piece.pieceId = "Arch_Tree";

        SavePrefab(root, ArchFolder, "Arch_Tree");
    }

    private static void SetupDoorway(GameObject go)
    {
        var col = go.GetComponent<BoxCollider>();
        if (col != null)
            col.isTrigger = true;

        go.AddComponent<DoorController>();
    }

    // ──────────────────────────────────────────────
    //  NARRATIVE PROPS (19 prefabs)
    // ──────────────────────────────────────────────

    private static void BuildNarrativePropPrefabs()
    {
        // model-name (or null for primitive), primitive type, primitive scale, mat token, hint hex, hint intensity, prefab scale
        BuildProp("Prop_Coat",           "coatRackStanding", PrimitiveType.Cube,     Vector3.one,                    "MemoryMat", "FFBF00", 1.5f, 1.0f);
        BuildProp("Prop_Notebook",       "books",            PrimitiveType.Cube,     Vector3.one,                    "MemoryMat", "FFBF00", 1.0f, 0.5f);
        BuildProp("Prop_PhotoFrame",     null,               PrimitiveType.Cube,     new Vector3(0.4f, 0.3f, 0.05f), "MemoryMat", "FFBF00", 1.2f, 0.8f);
        BuildProp("Prop_StoppedClock",   null,               PrimitiveType.Cylinder, new Vector3(0.2f, 0.03f, 0.2f), "ArchMat",   "C9D4B0", 1.5f, 0.8f);
        BuildProp("Prop_TeacherNotebook","books",             PrimitiveType.Cube,     Vector3.one,                    "MemoryMat", "FFBF00", 1.0f, 0.5f);
        BuildProp("Prop_ChalkDrawing",   null,               PrimitiveType.Cube,     new Vector3(1.5f, 1.0f, 0.08f), "MemoryMat", "4A3438", 1.5f, 1.0f);
        BuildProp("Prop_Backpack",       "cardboardBoxClosed",PrimitiveType.Cube,    Vector3.one,                    "MemoryMat", "FFBF00", 1.5f, 0.7f);
        BuildProp("Prop_DriedFlowers",   "plantSmall1",      PrimitiveType.Sphere,   Vector3.one,                    "MemoryMat", "4A3438", 1.0f, 0.6f);
        BuildProp("Prop_BlankBook",      "books",            PrimitiveType.Cube,     Vector3.one,                    "MemoryMat", "FFBF00", 0.8f, 0.4f);
        BuildProp("Prop_LibraryStamp",   null,               PrimitiveType.Cube,     new Vector3(0.08f, 0.12f, 0.08f),"ArchMat",  "FFBF00", 0.8f, 0.3f);
        BuildProp("Prop_JanitorCart",    null,               PrimitiveType.Cube,     new Vector3(0.6f, 0.8f, 0.4f),  "ArchMat",   "B23A3A", 1.0f, 1.0f);
        BuildPropGraffiti();  // special: Quad
        BuildProp("Prop_SoccerBall",     null,               PrimitiveType.Sphere,   new Vector3(0.22f, 0.22f, 0.22f),"ArchMat",  "C9D4B0", 0.8f, 0.3f);
        BuildPropOverturnedDesk();  // special: rotated desk
        BuildProp("Prop_CenterBackpack", "cardboardBoxClosed",PrimitiveType.Cube,    Vector3.one,                    "MemoryMat", "FFBF00", 1.5f, 0.7f);
        BuildPropCoffeeCups();  // special: 2 cups
        BuildProp("Prop_AttendanceList", null,               PrimitiveType.Cube,     new Vector3(0.2f, 0.3f, 0.02f), "MemoryMat", "FFBF00", 1.2f, 0.5f);
        BuildProp("Prop_Stopwatch",      null,               PrimitiveType.Cylinder, new Vector3(0.06f, 0.01f, 0.06f),"ArchMat",  "B23A3A", 1.5f, 0.3f);
        BuildProp("Prop_RecordsBoard",   null,               PrimitiveType.Cube,     new Vector3(1.2f, 0.9f, 0.05f), "MemoryMat", "FFBF00", 1.0f, 1.2f);

        Debug.Log("[PrefabBatchBuilder]   Narrative Props: 19 prefabs done.");
    }

    private static void BuildProp(string name, string kenneyModel, PrimitiveType fallbackType,
                                   Vector3 fallbackScale, string matToken, string hintHex,
                                   float hintIntensity, float prefabScale)
    {
        GameObject go;
        if (!string.IsNullOrEmpty(kenneyModel))
        {
            go = LoadKenneyModel(kenneyModel);
            if (go == null)
                go = MakePrimitive(name, fallbackType, fallbackScale);
        }
        else
        {
            go = MakePrimitive(name, fallbackType, fallbackScale);
        }

        go.name = name;
        go.transform.localScale = Vector3.one * prefabScale;

        ApplyMaterial(go, ResolveMaterial(matToken));
        EnsureBoxCollider(go);
        SetupNarrativeProp(go, name, hintHex, hintIntensity);

        SavePrefab(go, NarrativeFolder, name);
    }

    private static void BuildPropGraffiti()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "Prop_ChalkGraffiti";
        go.transform.localScale = Vector3.one;

        ApplyMaterial(go, ResolveMaterial("MemoryMat"));
        // Quad has MeshCollider by default, add BoxCollider instead
        Object.DestroyImmediate(go.GetComponent<MeshCollider>());
        var col = go.AddComponent<BoxCollider>();
        col.size = new Vector3(1f, 1f, 0.01f);

        SetupNarrativeProp(go, "Prop_ChalkGraffiti", "FFBF00", 1.2f);
        SavePrefab(go, NarrativeFolder, "Prop_ChalkGraffiti");
    }

    private static void BuildPropOverturnedDesk()
    {
        GameObject go = LoadKenneyOrPrimitive("desk", "Prop_OverturnedDesk",
                                               PrimitiveType.Cube, new Vector3(1.2f, 0.8f, 0.6f));
        go.name = "Prop_OverturnedDesk";
        go.transform.localRotation = Quaternion.Euler(15f, 0f, -8f);

        ApplyMaterial(go, ResolveMaterial("MemoryMat"));
        EnsureBoxCollider(go);
        SetupNarrativeProp(go, "Prop_OverturnedDesk", "B23A3A", 1.5f);

        SavePrefab(go, NarrativeFolder, "Prop_OverturnedDesk");
    }

    private static void BuildPropCoffeeCups()
    {
        GameObject root = new GameObject("Prop_CoffeeCups");

        // Cup Full
        GameObject cupFull = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cupFull.name = "Cup_Full";
        cupFull.transform.SetParent(root.transform);
        cupFull.transform.localPosition = new Vector3(-0.06f, 0f, 0f);
        cupFull.transform.localScale = new Vector3(0.04f, 0.06f, 0.04f);

        // Cup Empty
        GameObject cupEmpty = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cupEmpty.name = "Cup_Empty";
        cupEmpty.transform.SetParent(root.transform);
        cupEmpty.transform.localPosition = new Vector3(0.06f, 0f, 0f);
        cupEmpty.transform.localScale = new Vector3(0.04f, 0.05f, 0.04f);

        root.transform.localScale = Vector3.one * 0.4f;

        ApplyMaterial(root, ResolveMaterial("ArchMat"));

        var col = root.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 0.03f, 0f);
        col.size = new Vector3(0.2f, 0.12f, 0.1f);

        SetupNarrativeProp(root, "Prop_CoffeeCups", "FFBF00", 1.0f);
        SavePrefab(root, NarrativeFolder, "Prop_CoffeeCups");
    }

    private static void SetupNarrativeProp(GameObject go, string propName, string hintHex, float intensity)
    {
        var prop = go.AddComponent<NarrativeProp>();
        prop.propName = propName;
        prop.chapterHintColor = HexColor(hintHex);
        prop.hintIntensity = intensity;
        prop.lightRange = 3f;
        prop.enableOnStart = true;
        prop.isLyraProp = false;
    }

    // ──────────────────────────────────────────────
    //  FLUORESCENT LIGHT
    // ──────────────────────────────────────────────

    private static void BuildFluorescentPrefab()
    {
        GameObject go = LoadKenneyModel("lampSquareCeiling");
        if (go == null)
            go = MakePrimitive("FluorescentLight", PrimitiveType.Cube, new Vector3(0.6f, 0.08f, 0.15f));

        go.name = "FluorescentLight";

        Material mat = EchoesMaterialLibrary.GetOrCreateEmissiveMaterial(
            "Mat_Fluorescent",
            new Color(0.79f, 0.83f, 0.69f),          // fluorescent-sick albedo
            new Color(0.79f, 0.83f, 0.69f) * 2f       // emission
        );
        ApplyMaterial(go, mat);

        // Point light
        var light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = HexColor("C9D4B0");
        light.intensity = 3f;
        light.range = 8f;
        light.shadows = LightShadows.Soft;

        // Flicker
        var flicker = go.AddComponent<LightFlicker>();
        flicker.baseIntensity = 3f;

        // Audio
        var audio = go.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.loop = true;
        audio.spatialBlend = 1f;

        EnsureBoxCollider(go);
        SavePrefab(go, LightingFolder, "FluorescentLight");

        Debug.Log("[PrefabBatchBuilder]   FluorescentLight prefab done.");
    }

    // ──────────────────────────────────────────────
    //  HELPERS
    // ──────────────────────────────────────────────

    private static GameObject LoadKenneyModel(string modelName)
    {
        string path = $"{KenneyRoot}/{modelName}.fbx";
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) return null;

        GameObject instance = (GameObject)Object.Instantiate(asset);
        instance.name = modelName;
        return instance;
    }

    private static GameObject LoadKenneyOrPrimitive(string modelName, string objName,
                                                     PrimitiveType fallback, Vector3 fallbackScale)
    {
        GameObject go = LoadKenneyModel(modelName);
        return go != null ? go : MakePrimitive(objName, fallback, fallbackScale);
    }

    private static GameObject MakePrimitive(string name, PrimitiveType type, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.localScale = scale;
        return go;
    }

    private static void ApplyMaterial(GameObject root, Material mat)
    {
        if (mat == null) return;
        foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            r.sharedMaterials = mats;
        }
    }

    private static void EnsureBoxCollider(GameObject go)
    {
        if (go.GetComponent<Collider>() == null)
            go.AddComponent<BoxCollider>();
    }

    private static Material ResolveMaterial(string token)
    {
        return token switch
        {
            "FloorMat"      => EchoesMaterialLibrary.FloorMat,
            "WallTealMat"   => EchoesMaterialLibrary.WallTealMat,
            "WallMustardMat"=> EchoesMaterialLibrary.WallMustardMat,
            "DoorMat"       => EchoesMaterialLibrary.DoorMat,
            "ArchMat"       => EchoesMaterialLibrary.ArchMat,
            "MemoryMat"     => EchoesMaterialLibrary.MemoryMat,
            "WallSageMat"   => EchoesMaterialLibrary.WallSageMat,
            "WallRoseMat"   => EchoesMaterialLibrary.WallRoseMat,
            _               => EchoesMaterialLibrary.ArchMat
        };
    }

    private static Color HexColor(string hex)
    {
        return EchoesMaterialLibrary.HexColor(hex);
    }

    private static void SavePrefab(GameObject go, string folder, string name)
    {
        string path = $"{folder}/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    private static void EnsureFolder(string assetPath)
    {
        EchoesMaterialLibrary.EnsureFolderExists(assetPath);
    }
}
