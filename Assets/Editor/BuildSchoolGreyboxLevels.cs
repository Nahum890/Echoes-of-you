using System;
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement; // Recompiled

/// <summary>
/// Phase 1-only school architecture builder. It intentionally creates no
/// puzzles, props, lights, camera, UI, gameplay player, or path hints.
/// Only navigable school geometry with correct heights and NavMesh.
/// </summary>
public static class BuildSchoolGreyboxLevels
{
    private const string SceneFolder = "Assets/Scenes";
    private const string ReportFolder = "Reports/generated";
    private const float WallThickness = 0.2f;

    [MenuItem("Echoes of You/Production/Build All School Greybox Levels (NEW)", false, 199)]
    public static void BuildAllBlueprints()
    {
        Directory.CreateDirectory(ReportFolder);
        var reports = new List<GreyboxValidationResult>();

        for (int level = 1; level <= 15; level++)
        {
            reports.Add(BuildLevel(level));
        }

        string json = JsonUtility.ToJson(new GreyboxValidationReport { generatedUtc = DateTime.UtcNow.ToString("O"), levels = reports.ToArray() }, true);
        File.WriteAllText(Path.Combine(ReportFolder, "greybox_validation.json"), json);
        AssetDatabase.Refresh();
        Debug.Log("[School Greybox] Generated " + reports.Count + "/15 scenes. Report: " + ReportFolder + "/greybox_validation.json");
    }

    private static GreyboxValidationResult BuildLevel(int level)
    {
        string levelName = "Level_" + level.ToString("00");
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = levelName;

        GameObject root = new GameObject("--- SCHOOL GREYBOX ARCHITECTURE ---");
        NavMeshSurface surface = root.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.agentTypeID = 0;

        // Build level architecture based on BRIEF_ESPACIAL
        BuildLevelArchitecture(root.transform, level);

        Vector3 start = new Vector3(0f, 0.05f, -2f);
        Vector3 exitPosition = new Vector3(0f, 0.1f, GetExitZ(level) - 2f);
        CreateMarker(root.transform, "PlayerStart", start, false);
        CreateMarker(root.transform, "LevelExit", exitPosition, true).AddComponent<LevelExit>().nextSceneName = level < 15 ? "Level_" + (level + 1).ToString("00") : string.Empty;

        surface.BuildNavMesh();
        string scenePath = SceneFolder + "/" + levelName + ".unity";
        GreyboxValidationResult result = LevelValidator.ValidateScene(scene, levelName, start, exitPosition);
        result.scene = scenePath;
        EditorSceneManager.SaveScene(scene, scenePath);
        return result;
    }

    private static void BuildLevelArchitecture(Transform parent, int level)
    {
        switch (level)
        {
            case 1: BuildLevel1(parent); break;
            case 2: BuildLevel2(parent); break;
            case 3: BuildLevel3(parent); break;
            case 4: BuildLevel4(parent); break;
            case 5: BuildLevel5(parent); break;
            case 6: BuildLevel6(parent); break;
            case 7: BuildLevel7(parent); break;
            case 8: BuildLevel8(parent); break;
            case 9: BuildLevel9(parent); break;
            case 10: BuildLevel10(parent); break;
            case 11: BuildLevel11(parent); break;
            case 12: BuildLevel12(parent); break;
            case 13: BuildLevel13(parent); break;
            case 14: BuildLevel14(parent); break;
            case 15: BuildLevel15(parent); break;
        }
    }

    // ==================== LEVEL BUILDERS USING SEQUENTIAL AddRoom ====================

    // LEVEL 1: Desorientación - Porche → Pasillo A (20m) → Pasillo B (20m) → Umbral final
    private static void BuildLevel1(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.SchoolEntrance, "Entrance", ref z, new Vector2(8f, 8f), 5f, 0f, "", true, false);
        z = AddRoom(parent, ModuleType.SchoolCorridor, "Corridor_A", ref z, new Vector2(4f, 20f), 3.2f, 0f, "WallTeal");
        z = AddRoom(parent, ModuleType.SchoolCorridor, "Corridor_B", ref z, new Vector2(4f, 20f), 3.2f, 0f, "WallTeal");
        z = AddRoom(parent, ModuleType.TransitionSpace, "Threshold_Final", ref z, new Vector2(6f, 4f), 3.2f, 0f);
    }

    // LEVEL 2: Repetición - Corredor acceso → Aula 1 → Pasillo conexión → Aula 2 → Aula 3
    private static void BuildLevel2(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.TransitionSpace, "Access_Corridor", ref z, new Vector2(4f, 10f), 3.2f, 0f, "WallTeal");
        z = AddRoom(parent, ModuleType.SchoolClassroom, "Classroom_1", ref z, new Vector2(10f, 8f), 3.8f, 0f, "WallMustard");
        z = AddRoom(parent, ModuleType.TransitionSpace, "Connector_1", ref z, new Vector2(4f, 8f), 3.2f, 0f, "WallTeal");
        z = AddRoom(parent, ModuleType.SchoolClassroom, "Classroom_2", ref z, new Vector2(10f, 8f), 3.8f, 0f, "WallMustard");
        z = AddRoom(parent, ModuleType.SchoolClassroom, "Classroom_3", ref z, new Vector2(10f, 8f), 3.8f, 0f, "WallMustard");
    }

    // LEVEL 3: Indecisión - Hall bifurcación (8⌀) → Pasillo Aiden + Pasillo Lyra (espejo) → Confluencia
    private static void BuildLevel3(Transform parent)
    {
        // Hall bifurcación (SchoolHall 8×8, h=5) centered at Z=4
        CreateRoom(parent, ModuleType.SchoolHall, "Hall_Bifurcation", new Vector3(0f, 0f, 4f), new Vector2(8f, 8f), 5f, 0f, true, true);
        
        // Hall front at Z=8, corridors start there
        float corridorStartZ = 8f;
        
        // Pasillo Aiden (izquierda)
        CreateRoom(parent, ModuleType.SchoolCorridor, "Corridor_Aiden", new Vector3(-8f, 0f, corridorStartZ + 8f), new Vector2(4f, 16f), 3.2f, 0f, true, true, "WallTeal");
        
        // Pasillo Lyra (derecha)
        CreateRoom(parent, ModuleType.SchoolCorridor, "Corridor_Lyra", new Vector3(8f, 0f, corridorStartZ + 8f), new Vector2(4f, 16f), 3.2f, 0f, true, true, "WallRose");
        
        // Corridors end at Z=24, confluence at Z=24
        float confluenceZ = corridorStartZ + 16f;
        CreateRoom(parent, ModuleType.TransitionSpace, "Confluence", new Vector3(0f, 0f, confluenceZ + 4f), new Vector2(14f, 8f), 3.2f, 0f, true, true);
    }

    // LEVEL 4: Espera - Pre-observación (6×6) → Aula con desnivel (10×8) → Corredor escape
    private static void BuildLevel4(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.TransitionSpace, "PreObservation", ref z, new Vector2(6f, 6f), 3.2f, 0f, "WallTeal");
        z = AddRoom(parent, ModuleType.SchoolClassroom, "Classroom_Elevated", ref z, new Vector2(12f, 10f), 3.8f, 0f, "WallMustard");
        z = AddRoom(parent, ModuleType.TransitionSpace, "Escape_Corridor", ref z, new Vector2(3f, 8f), 3.2f, 0f, "WallMustard");
    }

    // LEVEL 5: Culpa - Corredor técnico → Observación → Laberinto (3 ramas) → Sala central
    private static void BuildLevel5(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.SchoolCorridor, "Tech_Corridor", ref z, new Vector2(3f, 8f), 3.2f, 0f, "Arch");
        z = AddRoom(parent, ModuleType.TransitionSpace, "Observation", ref z, new Vector2(6f, 4f), 3.2f, 0f);
        
        // Main maze corridor
        z = AddRoom(parent, ModuleType.SchoolCorridor, "Maze_Main", ref z, new Vector2(2.5f, 12f), 3.2f, 0f, "Arch");
        float mazeEndZ = z;
        
        // Left branch - starts from middle of maze (z - 6)
        CreateRoom(parent, ModuleType.SchoolCorridor, "Maze_Left", new Vector3(-5f, 0f, mazeEndZ - 6f), new Vector2(2.5f, 8f), 3.2f, 0f, true, true, "Arch");
        
        // Right branch
        CreateRoom(parent, ModuleType.SchoolCorridor, "Maze_Right", new Vector3(5f, 0f, mazeEndZ - 6f), new Vector2(2.5f, 8f), 3.2f, 0f, true, true, "Arch");
        
        // Central room at end of maze
        z = AddRoom(parent, ModuleType.SchoolHall, "Central_Room", ref z, new Vector2(6f, 6f), 5f, 0f);
    }

    // LEVEL 6: Negación - Entrada biblioteca → Pasillo estanterías → Abismo principal → Llegada
    private static void BuildLevel6(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.SchoolHall, "Library_Entrance", ref z, new Vector2(10f, 8f), 5f, 0f, "WallSage");
        z = AddRoom(parent, ModuleType.SchoolCorridor, "Shelves_Corridor", ref z, new Vector2(4f, 12f), 3.5f, 0f, "WallSage");
        
        // Abyss - two platforms with gap (TransitionSpace)
        float abyssStartZ = z;
        CreateRoom(parent, ModuleType.TransitionSpace, "Abyss_Left", new Vector3(-6f, 0f, abyssStartZ + 5f), new Vector2(4f, 10f), 3.2f, 0f, true, true);
        CreateRoom(parent, ModuleType.TransitionSpace, "Abyss_Right", new Vector3(6f, 0f, abyssStartZ + 5f), new Vector2(4f, 10f), 3.2f, 0f, true, true);
        
        // Spectral bridge connecting both sides at same Z
        CreateRoom(parent, ModuleType.TransitionSpace, "Spectral_Bridge", new Vector3(0f, 0f, abyssStartZ + 5f), new Vector2(8f, 10f), 3.2f, 0f, true, true);
        
        // Arrival hall
        z = abyssStartZ + 16f;
        z = AddRoom(parent, ModuleType.SchoolHall, "Arrival", ref z, new Vector2(10f, 8f), 5f, 0f, "WallSage");
    }

    // LEVEL 7: Evasión - Corredor emergencia → Patio trasero (3 lados) → Almacén
    private static void BuildLevel7(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.SchoolCorridor, "Emergency_Corridor", ref z, new Vector2(3f, 12f), 3.2f, 0f, "WallTeal");
        
        // Backyard - open on 3 sides, just floor + back wall
        CreateRoom(parent, ModuleType.SchoolHall, "Backyard", new Vector3(0f, 0f, z + 6f), new Vector2(12f, 12f), 3.2f, 0f, true, true);
        z += 12f;
        
        // Storage
        z = AddRoom(parent, ModuleType.SchoolClassroom, "Storage", ref z, new Vector2(8f, 6f), 3.8f, 0f, "WallMustard");
    }

    // LEVEL 8: Autosabotaje - Antesala → Sala profesores (14×10) → Fotocopiadora
    private static void BuildLevel8(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.TransitionSpace, "Antechamber", ref z, new Vector2(6f, 6f), 3.2f, 0f, "WallMustard");
        z = AddRoom(parent, ModuleType.SchoolStaffRoom, "Staff_Room", ref z, new Vector2(14f, 10f), 3.8f, 0f, "WallMustard");
        
        // Copier room - lateral to staff room
        CreateRoom(parent, ModuleType.SchoolClassroom, "Copier_Room", new Vector3(-8f, 0f, z - 5f), new Vector2(5f, 5f), 3.8f, 0f, true, true, "WallMustard");
    }

    // LEVEL 9: Control - Umbral interior → Patio exterior (30×30) → Galería perimetral → Umbral salida
    private static void BuildLevel9(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.TransitionSpace, "Interior_Threshold", ref z, new Vector2(5f, 3f), 3.2f, 0f);
        
        // Exterior courtyard - SchoolCourtyard (open, no roof)
        CreateRoom(parent, ModuleType.SchoolCourtyard, "Exterior_Courtyard", new Vector3(0f, 0f, z + 15f), new Vector2(30f, 30f), 0.2f, 0f, true, true);
        z += 30f;
        
        // Perimeter gallery (one representative corridor)
        CreateRoom(parent, ModuleType.SchoolCorridor, "Perimeter_Gallery", new Vector3(14f, 0f, z - 15f + 15f), new Vector2(3f, 30f), 3.2f, 0f, true, true, "WallTeal");
        
        // Exit threshold
        z = AddRoom(parent, ModuleType.TransitionSpace, "Exit_Threshold", ref z, new Vector2(5f, 3f), 3.2f, 0f);
    }

    // LEVEL 10: Recuerdos - Umbral Lyra → Aula Lyra (semicírculo) → Despacho → Pasillo salida
    private static void BuildLevel10(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.TransitionSpace, "Lyra_Threshold", ref z, new Vector2(4f, 2f), 3.2f, 0f);
        z = AddRoom(parent, ModuleType.SchoolLyraClassroom, "Lyra_Classroom", ref z, new Vector2(12f, 10f), 3.8f, 0f, "WallRose");
        
        // Projector office - lateral
        CreateRoom(parent, ModuleType.SchoolClassroom, "Projector_Office", new Vector3(8f, 0f, z - 5f), new Vector2(5f, 5f), 3.8f, 0f, true, true, "WallRose");
        
        // Exit corridor
        z = AddRoom(parent, ModuleType.SchoolCorridor, "Exit_Corridor", ref z, new Vector2(4f, 6f), 3.2f, 0f, "WallTeal");
    }

    // LEVEL 11: Conexión - Base escalera → Tramo 1 → Descansillo → Tramo 2 → Pasillo superior
    private static void BuildLevel11(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.SchoolStairwell, "Stair_Base", ref z, new Vector2(6f, 6f), 7.6f, 0f);
        
        // First flight (ramp)
        z = AddRoom(parent, ModuleType.SchoolStairwell, "Stair_Flight_1", ref z, new Vector2(3f, 8f), 3.8f, 0f);
        
        // Landing
        z = AddRoom(parent, ModuleType.TransitionSpace, "Landing", ref z, new Vector2(4f, 4f), 3.2f, 0f);
        
        // Second flight (ramp rotated 180)
        z = AddRoom(parent, ModuleType.SchoolStairwell, "Stair_Flight_2", ref z, new Vector2(3f, 8f), 3.8f, 180f);
        
        // Upper corridor (at Y=7.6)
        CreateRoom(parent, ModuleType.SchoolCorridor, "Upper_Corridor", new Vector3(0f, 7.6f, z + 4f), new Vector2(4f, 8f), 3.2f, 0f, true, true, "WallMustard");
    }

    // LEVEL 12: Conflicto - Acceso gimnasio → Gimnasio (20×16, h=6) → Almacén → Plataforma elevada
    private static void BuildLevel12(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.TransitionSpace, "Gym_Access", ref z, new Vector2(5f, 2f), 4f, 0f);
        z = AddRoom(parent, ModuleType.SchoolGym, "Gym_Main", ref z, new Vector2(20f, 16f), 6f, 0f);
        
        // Storage - lateral
        CreateRoom(parent, ModuleType.SchoolClassroom, "Storage", new Vector3(-10f, 0f, z - 8f), new Vector2(6f, 6f), 3.8f, 0f, true, true, "Arch");
        
        // Elevated platform
        z = AddRoom(parent, ModuleType.TransitionSpace, "Elevated_Platform", ref z, new Vector2(6f, 4f), 3.2f, 0f);
    }

    // LEVEL 13: Verdad - Umbral roto → Aula fragmentada (igual L10) → Conversación → Salida imposible
    private static void BuildLevel13(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.TransitionSpace, "Broken_Threshold", ref z, new Vector2(3f, 2f), 3.2f, 15f); // tilted
        z = AddRoom(parent, ModuleType.SchoolLiminalClassroom, "Fragmented_Classroom", ref z, new Vector2(12f, 10f), 3.8f, 0f, "WallRose");
        z = AddRoom(parent, ModuleType.TransitionSpace, "Conversation_Space", ref z, new Vector2(6f, 4f), 3.2f, 0f);
        z = AddRoom(parent, ModuleType.TransitionSpace, "Impossible_Exit", ref z, new Vector2(4f, 2f), 3.2f, 0f);
    }

    // LEVEL 14: Aceptación - Corredor void → Fragmento izq → Fragmento der → Confluencia
    private static void BuildLevel14(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.SchoolCorridor, "Void_Corridor", ref z, new Vector2(4f, 6f), 3.2f, 0f, "WallTeal");
        
        // Left fragment (floating)
        CreateRoom(parent, ModuleType.TransitionSpace, "Fragment_Left", new Vector3(-8f, 0f, z + 1.5f), new Vector2(8f, 3f), 0.2f, 0f, true, true);
        CreateBox("Locker", parent, new Vector3(-10f, 1f, z + 1.5f), new Vector3(0.4f, 1.8f, 0.8f), EchoesMaterialLibrary.ArchMat);
        
        // Right fragment (floating)
        CreateRoom(parent, ModuleType.TransitionSpace, "Fragment_Right", new Vector3(8f, 0f, z + 1.5f), new Vector2(8f, 3f), 0.2f, 0f, true, true);
        CreateBox("Locker", parent, new Vector3(10f, 1f, z + 1.5f), new Vector3(0.4f, 1.8f, 0.8f), EchoesMaterialLibrary.ArchMat);
        
        // Central confluence
        CreateRoom(parent, ModuleType.TransitionSpace, "Central_Confluence", new Vector3(0f, 0f, z + 5f), new Vector2(6f, 4f), 0.2f, 0f, true, true);
        CreateBox("Switch_Left", parent, new Vector3(-1f, 0.1f, z + 5f), new Vector3(1f, 0.2f, 1f), EchoesMaterialLibrary.PlateMat);
        CreateBox("Switch_Right", parent, new Vector3(1f, 0.1f, z + 5f), new Vector3(1f, 0.2f, 1f), EchoesMaterialLibrary.PlateMat);
    }

    // LEVEL 15: Integración - Pasillo N1 idéntico → 3 puzzles simples → Salida final
    private static void BuildLevel15(Transform parent)
    {
        float z = 0f;
        z = AddRoom(parent, ModuleType.SchoolCorridor, "Corridor_N1", ref z, new Vector2(4f, 20f), 3.2f, 0f, "WallTeal|flicker");
        
        // Puzzle 1: Wall niche
        CreateRoom(parent, ModuleType.TransitionSpace, "Puzzle_1", new Vector3(4f, 0f, z - 14f), new Vector2(3f, 3f), 3.2f, 0f, true, true);
        
        // Puzzle 2: Short stair + platform
        CreateRoom(parent, ModuleType.SchoolStairwell, "Puzzle_2_Stair", new Vector3(-4f, 0f, z - 8f), new Vector2(3f, 4f), 3.8f, 20f, true, true);
        
        // Puzzle 3: Small classroom
        CreateRoom(parent, ModuleType.SchoolClassroom, "Puzzle_3_Classroom", new Vector3(5f, 0f, z - 2f), new Vector2(5f, 4f), 3.8f, 0f, true, true);
        
        // Final exit
        z = AddRoom(parent, ModuleType.TransitionSpace, "Final_Exit", ref z, new Vector2(6f, 2f), 3.2f, 0f);
    }

    // ==================== HELPER METHODS ====================

    private static float AddRoom(Transform parent, ModuleType type, string name, ref float currentZ, Vector2 footprint, float height, float yaw, string customData = "", bool openFront = true, bool openBack = true)
    {
        // Room center is at currentZ + half depth
        float centerZ = currentZ + footprint.y * 0.5f;
        bool isFirstRoom = currentZ == 0f;
        
        bool roomOpenBack = !isFirstRoom && openBack;
        bool roomOpenFront = openFront;
        
        CreateRoom(parent, type, name, new Vector3(0f, 0f, centerZ), footprint, height, yaw, roomOpenFront, roomOpenBack, customData);
        
        return currentZ + footprint.y;
    }

    private static void CreateRoom(Transform parent, ModuleType type, string name, Vector3 center, Vector2 footprint, float height, float yaw, bool openFront, bool openBack, string customData = "")
    {
        GameObject room = new GameObject(name);
        room.transform.SetParent(parent, false);
        room.transform.position = center;
        room.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        GreyboxModule module = room.AddComponent<GreyboxModule>();
        module.moduleType = type;
        module.dimensions = new Vector3(footprint.x, height, footprint.y);
        module.clearance = 1.2f;

        // Floor slab
        CreateBox("Floor", room.transform, new Vector3(0f, -0.1f, 0f), new Vector3(footprint.x, 0.2f, footprint.y), EchoesMaterialLibrary.FloorMat);

        float halfX = footprint.x * 0.5f;
        float halfZ = footprint.y * 0.5f;
        Material wallMat = GetWallMaterialForCustomData(customData);

        if (!openBack) CreateBox("Wall_Back", room.transform, new Vector3(0f, height * 0.5f, -halfZ + WallThickness * 0.5f), new Vector3(footprint.x, height, WallThickness), wallMat);
        if (!openFront) CreateBox("Wall_Front", room.transform, new Vector3(0f, height * 0.5f, halfZ - WallThickness * 0.5f), new Vector3(footprint.x, height, WallThickness), wallMat);
        CreateBox("Wall_Left", room.transform, new Vector3(-halfX + WallThickness * 0.5f, height * 0.5f, 0f), new Vector3(WallThickness, height, footprint.y), wallMat);
        CreateBox("Wall_Right", room.transform, new Vector3(halfX - WallThickness * 0.5f, height * 0.5f, 0f), new Vector3(WallThickness, height, footprint.y), wallMat);
    }

    private static Material GetWallMaterialForCustomData(string customData)
    {
        if (customData.Contains("WallMustard")) return EchoesMaterialLibrary.WallMustardMat;
        if (customData.Contains("WallRose")) return EchoesMaterialLibrary.WallRoseMat;
        if (customData.Contains("WallSage")) return EchoesMaterialLibrary.WallSageMat;
        if (customData.Contains("Arch")) return EchoesMaterialLibrary.ArchMat;
        return EchoesMaterialLibrary.WallTealMat;
    }

    private static GameObject CreateBox(string name, Transform parent, Vector3 localPosition, Vector3 scale, Material mat)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localScale = scale;
        cube.GetComponent<MeshRenderer>().sharedMaterial = mat;
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

    private static int GetExitZ(int level)
    {
        switch (level)
        {
            case 1: return 52;
            case 2: return 48;
            case 3: return 26;
            case 4: return 14;
            case 5: return 26;
            case 6: return 28;
            case 7: return 24;
            case 8: return 14;
            case 9: return 30;
            case 10: return 20;
            case 11: return 28;
            case 12: return 18;
            case 13: return 18;
            case 14: return 10;
            case 15: return 26;
            default: return 46;
        }
    }
}