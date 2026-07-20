using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Echoes.EnvironmentPass;

public static class AutoFixZoneAndPlace
{
    private const string BASE_FOLDER = "Assets/ScriptableObjects/EnvironmentPass";

    [MenuItem("Tools/EnvPass/AutoFix Zones and Place All")]
    public static void RunAutoFix()
    {
            var levels = EnvironmentPassDataLoader.LoadAllLevels();
        if (levels.Count == 0) { Debug.LogError("[AutoFix] No LevelDataSO found."); return; }

        string originalScene = SceneManager.GetActiveScene().path;
        int totalPlaced = 0;

        foreach (var level in levels.OrderBy(l => l.levelNumber))
        {
            try
            {
                totalPlaced += ProcessLevel(level);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AutoFix] Exception in {level.levelName}: {e.Message}");
            }
        }

        if (!string.IsNullOrEmpty(originalScene))
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[AutoFix] Completed. Total props placed: {totalPlaced}");
    }

    // Added helper to process a single level by its number (useful for incremental execution)
    public static int RunAutoFixSingleLevel(int levelNumber)
    {
        var levels = EnvironmentPassDataLoader.LoadAllLevels();
        var level = levels.FirstOrDefault(l => l.levelNumber == levelNumber);
        if (level == null)
        {
            Debug.LogError($"[AutoFix] Level number {levelNumber} not found.");
            return 0;
        }
        // Preserve original scene to restore later
        string originalScene = SceneManager.GetActiveScene().path;
        try
        {
            int placed = ProcessLevel(level);
            // Restore original scene after processing
            if (!string.IsNullOrEmpty(originalScene))
                EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AutoFix] Completed single level {level.levelName}. Props placed: {placed}");
            return placed;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AutoFix] Exception processing level {level.levelName}: {e.Message}");
            return 0;
        }
    }
    

    private static int ProcessLevel(LevelDataSO level)
    {
        Debug.Log($"[AutoFix] === {level.levelName} ===");

        EditorSceneManager.OpenScene(level.scenePath, OpenSceneMode.Single);
        GameObject envRoot = GameObject.Find("--- ENVIRONMENT ---");
        if (envRoot == null)
        {
            Debug.LogError($"[AutoFix] --- ENVIRONMENT --- not found in {level.levelName}");
            return 0;
        }

        var zoneNames = new List<string>();
        for (int i = 0; i < envRoot.transform.childCount; i++)
        {
            var name = envRoot.transform.GetChild(i).name;
            if (name.StartsWith("Zona") || name.StartsWith("zona"))
                zoneNames.Add(name);
        }

        if (zoneNames.Count == 0)
        {
            Debug.LogError($"[AutoFix] No zones found in {level.levelName}");
            return 0;
        }

        Debug.Log($"[AutoFix] Zones in scene: {string.Join(", ", zoneNames)}");

        string levelFolder = $"{BASE_FOLDER}/Level{level.levelNumber:D2}";
        EnsureFolder(levelFolder);

        var newRooms = new List<RoomDataSO>();
        var genericRoomPool = LoadGenericRoomPool();

        foreach (var zoneName in zoneNames)
        {
            RoomDataSO bestMatch = FindBestGenericRoom(genericRoomPool, zoneName, level.levelNumber);
            RoomDataSO newRoom = CreateLevelRoomData(levelFolder, zoneName, bestMatch);
            newRooms.Add(newRoom);
        }

        level.rooms = newRooms;
        EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssets();

        var result = EnvironmentPassPlacementEngine.PlaceLevel(level, dryRun: false);
        int placed = result.totalProps;

        if (result.success)
            Debug.Log($"[AutoFix]   OK: {placed} props");
        else
            Debug.LogWarning($"[AutoFix]   Placed {placed} props with warnings");

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        return placed;
    }

    private static List<RoomDataSO> LoadGenericRoomPool()
    {
        var pool = new List<RoomDataSO>();
        string[] guids = AssetDatabase.FindAssets("t:RoomDataSO", new[] { $"{BASE_FOLDER}/Level_" });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var rd = AssetDatabase.LoadAssetAtPath<RoomDataSO>(path);
            if (rd != null) pool.Add(rd);
        }
        return pool;
    }

    private static RoomDataSO FindBestGenericRoom(List<RoomDataSO> pool, string zoneName, int levelNumber)
    {
        if (pool.Count == 0) return null;

        RoomType guessedType = GuessRoomType(zoneName);
        var matches = pool.Where(r => r.roomType == guessedType).ToList();
        if (matches.Count > 0)
        {
            if (matches.Count == 1) return matches[0];
            int idx = (levelNumber * 7 + zoneName.GetHashCode()) % matches.Count;
            if (idx < 0) idx = -idx;
            return matches[idx % matches.Count];
        }

        return pool[levelNumber % pool.Count];
    }

    private static RoomDataSO CreateLevelRoomData(string levelFolder, string zoneName, RoomDataSO template)
    {
        string assetPath = $"{levelFolder}/RoomData_{zoneName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<RoomDataSO>(assetPath);
        if (existing != null)
        {
            existing.roomId = zoneName;
            if (template != null)
            {
                existing.roomType = template.roomType;
                existing.placements = DeepCopyPlacements(template.placements, levelFolder, zoneName);
                existing.decals = DeepCopyPlacements(template.decals, levelFolder, zoneName, isDecal: true);
            }
            EditorUtility.SetDirty(existing);
            return existing;
        }

        var rd = ScriptableObject.CreateInstance<RoomDataSO>();
        rd.roomId = zoneName;
        if (template != null)
        {
            rd.roomType = template.roomType;
            rd.placements = DeepCopyPlacements(template.placements, levelFolder, zoneName);
            rd.decals = DeepCopyPlacements(template.decals, levelFolder, zoneName, isDecal: true);
        }
        else
        {
            rd.roomType = GuessRoomType(zoneName);
            rd.placements = new List<PropPlacementSO>();
            rd.decals = new List<PropPlacementSO>();
        }

        AssetDatabase.CreateAsset(rd, assetPath);
        return rd;
    }

    private static List<PropPlacementSO> DeepCopyPlacements(List<PropPlacementSO> source, string levelFolder,
        string zoneName, bool isDecal = false)
    {
        var list = new List<PropPlacementSO>();
        if (source == null) return list;

        for (int i = 0; i < source.Count; i++)
        {
            var src = source[i];
            if (src == null) continue;

            var copy = ScriptableObject.CreateInstance<PropPlacementSO>();
            copy.prefabName = src.prefabName;
            copy.narrativeTag = src.narrativeTag;
            copy.localPosition = src.localPosition;
            copy.localRotationEuler = src.localRotationEuler;
            copy.scale = src.scale;
            copy.size = src.size;
            copy.materialOverride = src.materialOverride;
            copy.requiredForRoomType = src.requiredForRoomType;
            copy.minClearanceFromPuzzle = src.minClearanceFromPuzzle;

            string suffix = isDecal ? "_decal" : "";
            string assetPath = $"{levelFolder}/PropPlacement_{zoneName}_{src.prefabName}_{i:D2}{suffix}.asset";

            AssetDatabase.CreateAsset(copy, assetPath);
            list.Add(copy);
        }

        return list;
    }

    private static RoomType GuessRoomType(string zoneName)
    {
        string lower = zoneName.ToLower();
        if (lower.Contains("aula")) return RoomType.Classroom;
        if (lower.Contains("gimnasio")) return RoomType.Gym;
        if (lower.Contains("patio")) return RoomType.Patio;
        if (lower.Contains("biblioteca")) return RoomType.Library;
        if (lower.Contains("oficina") || lower.Contains("despacho") || lower.Contains("profesor")) return RoomType.Office;
        if (lower.Contains("almacen") || lower.Contains("mantenimiento") || lower.Contains("fotocop")) return RoomType.Storage;
        if (lower.Contains("hall") || lower.Contains("umbral") || lower.Contains("porche") || lower.Contains("entrada")) return RoomType.Hall;
        if (lower.Contains("pasillo") || lower.Contains("corredor") || lower.Contains("acceso")) return RoomType.Corridor;
        return RoomType.Corridor;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string name = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}