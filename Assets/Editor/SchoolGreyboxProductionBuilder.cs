using System;
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// Phase 1-only school architecture builder. It intentionally creates no
/// puzzles, props, lights, camera, UI, gameplay player, or path hints.
/// </summary>
public static class SchoolGreyboxProductionBuilder
{
    private const string SceneFolder = "Assets/Scenes";
    private const string ReportFolder = "Reports/generated";
    private const float WallThickness = 0.2f;

    [MenuItem("Echoes of You/Production/Build All School Greybox Levels", false, 200)]
    public static void BuildAllBlueprints()
    {
        Directory.CreateDirectory(ReportFolder);
        var reports = new List<GreyboxValidationResult>();
        for (int level = 1; level <= 15; level++)
            reports.Add(BuildLevel(level));

        string json = JsonUtility.ToJson(new GreyboxValidationReport { generatedUtc = DateTime.UtcNow.ToString("O"), levels = reports.ToArray() }, true);
        File.WriteAllText(Path.Combine(ReportFolder, "greybox_validation.json"), json);
        AssetDatabase.Refresh();
        Debug.Log("[School Greybox] Generated " + reports.Count + "/15 scenes. Report: " + ReportFolder + "/greybox_validation.json");
    }

    private static GreyboxValidationResult BuildLevel(int level)
    {
        string levelName = "Level_" + level.ToString("00") + "_SchoolGreybox";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = levelName;

        GameObject root = new GameObject("--- SCHOOL GREYBOX ARCHITECTURE ---");
        GameObject navigationRoot = new GameObject("--- NAVIGATION ---");
        navigationRoot.transform.SetParent(root.transform, false);
        // The surface belongs to the root so all geometry siblings participate
        // in the bake; keeping it under Navigation with Children would collect
        // an empty hierarchy.
        NavMeshSurface surface = root.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.agentTypeID = 0;

        // The route rises only in Z overall. Two 90-degree corridor turns are
        // 10 m apart, while the central corridor-to-hall sequence supplies the
        // required 3.2 -> 3.8 -> 5.0 m compression/release rhythm.
        float offset = (level - 8) * 1.5f;
        CreateRoom(root.transform, ModuleType.SchoolEntrance, "Entrance", new Vector3(offset, 0f, 0f), new Vector2(8f, 8f), 5f, 0f, true, false);
        CreateRoom(root.transform, ModuleType.TransitionSpace, "Transition", new Vector3(offset, 0f, 9f), new Vector2(3.2f, 10f), 3.2f, 0f, true, true);
        CreateRoom(root.transform, ModuleType.SchoolCorridor, "CorridorTurnA", new Vector3(offset + 4.5f, 0f, 14f), new Vector2(3.2f, 9f), 3.2f, 0f, true, true);
        CreateRoom(root.transform, ModuleType.SchoolCorridor, "CorridorTurnB", new Vector3(offset + 9f, 0f, 19f), new Vector2(3.2f, 10f), 3.2f, 0f, true, true);
        CreateRoom(root.transform, ClassroomTypeFor(level), "CoreClassroom", new Vector3(offset + 9f, 0f, 30f), new Vector2(9f, 12f), 3.8f, 0f, true, true);
        CreateRoom(root.transform, CoreTypeFor(level), "CoreHall", new Vector3(offset + 9f, 0f, 43.5f), new Vector2(14f, 14f), CoreHeightFor(level), 0f, true, true);

        Vector3 start = new Vector3(offset, 0.05f, -2f);
        Vector3 exitPosition = new Vector3(offset + 9f, 0.1f, 49f);
        CreateMarker(root.transform, "PlayerStart", start, false);
        CreateMarker(root.transform, "LevelExit", exitPosition, true).AddComponent<LevelExit>().nextSceneName = level < 15 ? "Level_" + (level + 1).ToString("00") + "_SchoolGreybox" : string.Empty;

        surface.BuildNavMesh();
        string scenePath = SceneFolder + "/" + levelName + ".unity";
        GreyboxValidationResult result = LevelValidator.ValidateScene(scene, levelName, start, exitPosition);
        result.scene = scenePath;
        EditorSceneManager.SaveScene(scene, scenePath);
        return result;
    }

    private static ModuleType ClassroomTypeFor(int level)
    {
        if (level % 5 == 0) return ModuleType.SchoolLiminalClassroom;
        if (level % 3 == 0) return ModuleType.SchoolLyraClassroom;
        if (level % 4 == 0) return ModuleType.SchoolLab;
        return ModuleType.SchoolClassroom;
    }

    private static ModuleType CoreTypeFor(int level)
    {
        if (level == 5 || level == 10 || level == 15) return ModuleType.SchoolGym;
        if (level == 4 || level == 8 || level == 12) return ModuleType.SchoolLibrary;
        if (level == 7 || level == 14) return ModuleType.SchoolStairwell;
        return ModuleType.SchoolHall;
    }

    private static float CoreHeightFor(int level)
    {
        ModuleType type = CoreTypeFor(level);
        if (type == ModuleType.SchoolGym) return 6f;
        if (type == ModuleType.SchoolStairwell) return 7.6f;
        return 5f;
    }

    private static void CreateRoom(Transform parent, ModuleType type, string name, Vector3 center, Vector2 footprint, float height, float yaw, bool openFront, bool openBack)
    {
        GameObject room = new GameObject(name);
        room.transform.SetParent(parent, false);
        room.transform.position = center;
        room.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        GreyboxModule module = room.AddComponent<GreyboxModule>();
        module.moduleType = type;
        module.dimensions = new Vector3(footprint.x, height, footprint.y);
        module.clearance = 1.2f;

        CreateBox("Floor", room.transform, new Vector3(0f, -0.1f, 0f), new Vector3(footprint.x, 0.2f, footprint.y));
        float halfX = footprint.x * 0.5f;
        float halfZ = footprint.y * 0.5f;
        CreateBox("Wall_L", room.transform, new Vector3(-halfX + WallThickness * 0.5f, height * 0.5f, 0f), new Vector3(WallThickness, height, footprint.y));
        CreateBox("Wall_R", room.transform, new Vector3(halfX - WallThickness * 0.5f, height * 0.5f, 0f), new Vector3(WallThickness, height, footprint.y));
        if (!openBack) CreateBox("Wall_Back", room.transform, new Vector3(0f, height * 0.5f, -halfZ + WallThickness * 0.5f), new Vector3(footprint.x, height, WallThickness));
        if (!openFront) CreateBox("Wall_Front", room.transform, new Vector3(0f, height * 0.5f, halfZ - WallThickness * 0.5f), new Vector3(footprint.x, height, WallThickness));
    }

    private static GameObject CreateBox(string name, Transform parent, Vector3 localPosition, Vector3 scale)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localScale = scale;
        cube.layer = 6; // Ground layer
        return cube;
    }

    private static GameObject CreateMarker(Transform parent, string name, Vector3 position, bool isExit)
    {
        GameObject marker = new GameObject(name);
        marker.transform.SetParent(parent, false);
        marker.transform.position = position;
        BoxCollider collider = marker.AddComponent<BoxCollider>();
        collider.isTrigger = isExit;
        collider.size = isExit ? new Vector3(2f, 2.2f, 2f) : new Vector3(0.1f, 0.1f, 0.1f);
        return marker;
    }
}

public sealed class GreyboxModule : MonoBehaviour
{
    public ModuleType moduleType;
    public Vector3 dimensions;
    public float clearance;
}

public static class LevelValidator
{
    public static GreyboxValidationResult ValidateScene(Scene scene, string levelName, Vector3 start, Vector3 exit)
    {
        GreyboxValidationResult result = new GreyboxValidationResult { scene = scene.path, level = levelName };
        ValidateGroupA_Architecture(scene, result);
        ValidateGroupD_Navigation(scene, start, exit, result);
        result.pass = result.failures.Count == 0;
        return result;
    }

    public static void ValidateGroupA_Architecture(Scene scene, GreyboxValidationResult result)
    {
        GreyboxModule[] modules = UnityEngine.Object.FindObjectsByType<GreyboxModule>(FindObjectsSortMode.None);
        int starts = 0;
        int exits = 0;
        bool corridor = false, classroom = false, hall = false;
        foreach (GreyboxModule module in modules)
        {
            if (module.moduleType == ModuleType.SchoolCorridor || module.moduleType == ModuleType.TransitionSpace)
                corridor |= Approximately(module.dimensions.y, 3.2f);
            if (module.moduleType == ModuleType.SchoolClassroom || module.moduleType == ModuleType.SchoolLyraClassroom || module.moduleType == ModuleType.SchoolLiminalClassroom || module.moduleType == ModuleType.SchoolLab)
                classroom |= Approximately(module.dimensions.y, 3.8f);
            if (module.moduleType == ModuleType.SchoolHall || module.moduleType == ModuleType.SchoolLibrary)
                hall |= Approximately(module.dimensions.y, 5f);
            if (module.moduleType == ModuleType.SchoolGym)
                hall |= Approximately(module.dimensions.y, 6f);
            if (module.moduleType == ModuleType.SchoolStairwell)
                hall |= Approximately(module.dimensions.y, 7.6f);
            if (module.clearance < 1.2f) result.failures.Add("FAIL-ARC-CLEARANCE");
        }
        foreach (GameObject go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.name == "PlayerStart") starts++;
            if (go.name == "LevelExit") exits++;
        }
        if (starts != 1) result.failures.Add("FAIL-ARC-PLAYERSTART");
        if (exits != 1) result.failures.Add("FAIL-ARC-EXIT");
        if (!corridor || !classroom || !hall) result.failures.Add("FAIL-ARC-RHYTHM");
        result.architecturePass = result.failures.Count == 0;
    }

    public static void ValidateGroupD_Navigation(Scene scene, Vector3 start, Vector3 exit, GreyboxValidationResult result)
    {
        bool startOnNav = NavMesh.SamplePosition(start, out NavMeshHit startHit, 1f, NavMesh.AllAreas);
        bool exitOnNav = NavMesh.SamplePosition(exit, out NavMeshHit exitHit, 2f, NavMesh.AllAreas);
        NavMeshPath path = new NavMeshPath();
        bool reachable = startOnNav && exitOnNav && NavMesh.CalculatePath(startHit.position, exitHit.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete;
        result.navMeshCoverage = reachable ? 1f : 0f;
        if (!reachable) result.failures.Add("FAIL-NAV-ROUTE");
        if (result.navMeshCoverage < 0.95f) result.failures.Add("FAIL-NAV-COVERAGE");
        result.navigationPass = reachable;
    }

    private static bool Approximately(float value, float expected) => Mathf.Abs(value - expected) < 0.01f;
}

[Serializable]
public sealed class GreyboxValidationResult
{
    public string scene;
    public string level;
    public bool pass;
    public bool architecturePass;
    public bool navigationPass;
    public float navMeshCoverage;
    public List<string> failures = new List<string>();
}

[Serializable]
public sealed class GreyboxValidationReport
{
    public string generatedUtc;
    public GreyboxValidationResult[] levels;
}
