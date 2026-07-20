using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Echoes.EnvironmentPass;

public class EnvironmentPassPlacer : EditorWindow
{
    [MenuItem("Tools/Environment Pass/1 - DryRun (console only)")]
    public static void RunDryRun()
    {
        Debug.Log("===========================================================");
        Debug.Log("[EnvPass] DRYRUN START -- no props will be placed");
        Debug.Log("===========================================================");

        var levels = Echoes.EnvironmentPass.EnvironmentPassDataLoader.LoadAllLevels();
        if (levels.Count == 0)
        {
            Debug.LogError("[EnvPass] No LevelDataSO assets found in Assets/ScriptableObjects/EnvironmentPass/");
            return;
        }

        int totalProps = 0, totalWarnings = 0, totalErrors = 0;
        foreach (var level in levels)
        {
            var result = Echoes.EnvironmentPass.EnvironmentPassPlacementEngine.PlaceLevel(level, dryRun: true);
            totalProps += result.totalProps;

            if (!result.success)
            {
                Debug.LogError($"[EnvPass]   {level.levelName} has errors:");
                foreach (var rr in result.roomResults)
                    foreach (var e in rr.errors)
                    {
                        Debug.LogError($"[EnvPass]     {rr.roomId}: {e}");
                        totalErrors++;
                    }
            }

            foreach (var rr in result.roomResults)
            {
                foreach (var w in rr.warnings)
                {
                    Debug.LogWarning($"[EnvPass]   {level.levelName}/{rr.roomId}: {w}");
                    totalWarnings++;
                }
            }
        }

        Debug.Log($"[EnvPass] DRYRUN COMPLETE: {totalProps} props, {totalWarnings} warnings, {totalErrors} errors");
        if (totalErrors == 0 && totalWarnings == 0)
            Debug.Log("[EnvPass] All clear. Run PlaceAll.");
        Debug.Log("===========================================================");
    }

    [MenuItem("Tools/Environment Pass/2 - PlaceAll (all scenes)")]
    public static void RunPlaceAll()
    {
        Debug.Log("===========================================================");
        Debug.Log("[EnvPass] PLACEALL START");
        Debug.Log("===========================================================");

        var levels = Echoes.EnvironmentPass.EnvironmentPassDataLoader.LoadAllLevels();
        if (levels.Count == 0)
        {
            Debug.LogError("[EnvPass] No LevelDataSO assets found.");
            return;
        }

        string originalScene = SceneManager.GetActiveScene().path;
        bool anyFailed = false;

        foreach (var level in levels)
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Environment Pass - PlaceAll",
                $"Process {level.levelName}?\n\nRooms: {level.rooms.Count(r => r != null)}\nProps: {EstimateProps(level)}\n\nScene will be saved automatically.",
                "Place", "Skip"
            );

            if (!proceed) { Debug.Log($"[EnvPass] Skipped: {level.levelName}"); continue; }

            try
            {
                var result = Echoes.EnvironmentPass.EnvironmentPassPlacementEngine.PlaceLevel(level, dryRun: false);
                if (!result.success)
                {
                    anyFailed = true;
                    foreach (var rr in result.roomResults)
                        foreach (var e in rr.errors)
                            Debug.LogError($"[EnvPass]   {rr.roomId}: {e}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnvPass] Exception in {level.levelName}: {e.Message}");
                anyFailed = true;
            }
        }

        if (!string.IsNullOrEmpty(originalScene))
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);

        Debug.Log(anyFailed
            ? "[EnvPass] PLACEALL finished WITH ERRORS - check console."
            : "[EnvPass] PLACEALL OK. Run VerifyOnly per scene.");
        Debug.Log("===========================================================");
    }

    [MenuItem("Tools/Environment Pass/3 - VerifyOnly (active scene)")]
    public static void RunVerifyOnly()
    {
        Debug.Log("===========================================================");
        Debug.Log("[EnvPass] VERIFY - active scene: " + SceneManager.GetActiveScene().name);
        Debug.Log("Checks: clearance vs puzzle objects, intra-prop overlap, floor proximity, color distribution");
        Debug.Log("LIMITATION: does NOT verify clipping against walls (manual visual check needed)");
        Debug.Log("===========================================================");

        Echoes.EnvironmentPass.EnvironmentPassValidator.ValidateActiveScene();

        Debug.Log("===========================================================");
    }

    [MenuItem("Tools/Environment Pass/4 - Single Level (DryRun + Place + Verify)")]
    public static void RunSingleLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        int levelNum = ExtractLevelNumber(sceneName);
        var level = Echoes.EnvironmentPass.EnvironmentPassDataLoader.LoadLevel(levelNum);

        if (level == null)
        {
            Debug.LogError($"[EnvPass] No LevelDataSO for {sceneName}");
            return;
        }

        var dryRun = Echoes.EnvironmentPass.EnvironmentPassPlacementEngine.PlaceLevel(level, dryRun: true);
        Debug.Log($"[EnvPass] DryRun: {dryRun.totalProps} props");

        int warnings = dryRun.roomResults.Sum(r => r.warnings.Count);
        int errors = dryRun.roomResults.Sum(r => r.errors.Count);

        if (errors > 0)
        {
            Debug.LogError("[EnvPass] DryRun has errors. Aborting.");
            return;
        }

        if (warnings > 0 && !EditorUtility.DisplayDialog("Warnings found",
            $"{warnings} warnings in DryRun. Continue anyway?", "Continue", "Cancel"))
            return;

        var result = Echoes.EnvironmentPass.EnvironmentPassPlacementEngine.PlaceLevel(level, dryRun: false);
        if (result.success)
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log($"[EnvPass] {sceneName} saved. Running verify...");
            RunVerifyOnly();
        }
    }

    [MenuItem("Tools/Environment Pass/5 - Generate Missing PropPlacementSOs")]
    public static void RunGenerateMissingPropPlacements()
    {
        Debug.Log("[EnvPass] Scanning and generating PropPlacementSO assets for all rooms...");

        PropScaleAndMaterialFixer.RunFixes();
        var scaleMap = PropScaleAndMaterialFixer.ComputePrefabScaleFactors();
        var extintorMat = AssetDatabase.LoadAssetAtPath<Material>(PropScaleAndMaterialFixer.ExtintorMaterialPath);

        var levels = Echoes.EnvironmentPass.EnvironmentPassDataLoader.LoadAllLevels();
        int created = 0;
        int deleted = 0;

        // System Random to make it repeatable and slightly varied
        var rand = new System.Random(42);

        foreach (var level in levels)
        {
            string folder = $"Assets/ScriptableObjects/EnvironmentPass/Level{level.levelNumber:D2}";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects/EnvironmentPass", $"Level{level.levelNumber:D2}");
            }

            foreach (var room in level.rooms)
            {
                if (room == null) continue;

                // 1. Delete existing PropPlacementSO files for this room to avoid orphaned files
                if (System.IO.Directory.Exists(folder))
                {
                    string[] existingFiles = System.IO.Directory.GetFiles(folder, $"PropPlacement_{room.roomId}_*.asset");
                    foreach (var file in existingFiles)
                    {
                        AssetDatabase.DeleteAsset(file.Replace("\\", "/"));
                        deleted++;
                    }
                }

                // Clean lists of nulls
                room.placements.Clear();
                room.decals.Clear();

                // 2. Gather required props
                var requiredList = room.GetRequiredPropsForType();
                var propsToCreate = new List<string>(requiredList);

                // 3. Count existing size distribution in props to create
                int smallCount = 0;
                int medCount = 0;
                int domCount = 0;

                foreach (var req in propsToCreate)
                {
                    var size = GetPropSize(req);
                    if (size == PropSize.Small) smallCount++;
                    else if (size == PropSize.Medium) medCount++;
                    else if (size == PropSize.Dominant) domCount++;
                }

                // Fillers by room type
                var smallFillers = GetSmallFillers(room.roomType);
                var medFillers = GetMediumFillers(room.roomType);
                var domFillers = GetDominantFillers(room.roomType);

                // Ensure minimum density: small >= 5, med >= 2, dom >= 1
                while (smallCount < 5 && smallFillers.Count > 0)
                {
                    string filler = smallFillers[rand.Next(smallFillers.Count)];
                    propsToCreate.Add(filler);
                    smallCount++;
                }
                while (medCount < 2 && medFillers.Count > 0)
                {
                    string filler = medFillers[rand.Next(medFillers.Count)];
                    propsToCreate.Add(filler);
                    medCount++;
                }
                while (domCount < 1 && domFillers.Count > 0)
                {
                    string filler = domFillers[rand.Next(domFillers.Count)];
                    propsToCreate.Add(filler);
                    domCount++;
                }

                // Append narrative props in key levels/rooms
                if ((level.levelNumber == 1 && room.roomId.Contains("Antesala")) ||
                    (level.levelNumber == 10 && room.roomId.Contains("AulaLyra")) ||
                    (level.levelNumber == 13 && room.roomId.Contains("PasilloLyra")) ||
                    (level.levelNumber == 15 && room.roomId.Contains("SalidaFinal")))
                {
                    if (!propsToCreate.Contains("MochilaLyra"))
                    {
                        propsToCreate.Add("MochilaLyra");
                    }
                }

                // 4. Create PropPlacementSO assets
                for (int i = 0; i < propsToCreate.Count; i++)
                {
                    string prefab = propsToCreate[i];
                    var newPP = ScriptableObject.CreateInstance<Echoes.EnvironmentPass.PropPlacementSO>();
                    newPP.prefabName = prefab;
                    newPP.size = GetPropSize(prefab);
                    newPP.requiredForRoomType = requiredList.Contains(prefab);
                    
                    // Slightly spread starting positions around center to avoid overlap failure
                    float angle = i * Mathf.PI * 2f / propsToCreate.Count;
                    float radius = 1.0f + (float)(rand.NextDouble() * 1.5f);
                    newPP.localPosition = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                    newPP.localRotationEuler = new Vector3(0, rand.Next(4) * 90f, 0);
                    newPP.scale = scaleMap.TryGetValue(prefab, out var s) ? s : Vector3.one;
                    newPP.minClearanceFromPuzzle = 1.5f;
                    if (prefab == "Extintor" && extintorMat != null)
                        newPP.materialOverride = extintorMat;

                    // Narrative Tags
                    if (prefab == "MochilaLyra")
                    {
                        newPP.narrativeTag = NarrativeTag.Lyra;
                    }
                    else if (level.levelNumber == 10 && room.roomId.Contains("AulaLyra") && prefab == "Pizarra")
                    {
                        newPP.narrativeTag = NarrativeTag.Environmental;
                    }

                    string assetPath = $"{folder}/PropPlacement_{room.roomId}_{prefab}_{i:D2}.asset";
                    AssetDatabase.CreateAsset(newPP, assetPath);
                    room.placements.Add(newPP);
                    created++;
                }

                // 5. Create Decals (2 per room)
                var decalPool = GetDecalPool(room.roomType);
                for (int i = 0; i < 2; i++)
                {
                    if (decalPool.Count == 0) break;
                    string decalPrefab = decalPool[rand.Next(decalPool.Count)];
                    var newDecal = ScriptableObject.CreateInstance<Echoes.EnvironmentPass.PropPlacementSO>();
                    newDecal.prefabName = decalPrefab;
                    newDecal.size = PropSize.Small;
                    newDecal.requiredForRoomType = false;

                    float angle = rand.Next(360) * Mathf.Deg2Rad;
                    float radius = (float)(rand.NextDouble() * 3.0f);
                    newDecal.localPosition = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                    newDecal.localRotationEuler = new Vector3(0, rand.Next(360), 0);
                    newDecal.scale = scaleMap.TryGetValue(decalPrefab, out var ds) ? ds : Vector3.one;

                    string assetPath = $"{folder}/PropPlacement_{room.roomId}_{decalPrefab}_{i:D2}_decal.asset";
                    AssetDatabase.CreateAsset(newDecal, assetPath);
                    room.decals.Add(newDecal);
                    created++;
                }

                EditorUtility.SetDirty(room);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[EnvPass] Cleanup: deleted {deleted} old stubs. Generation: created {created} new PropPlacementSOs.");
    }

    // Added helper to generate placements for a specific level (useful for incremental execution)
    public static void RunGenerateMissingPropPlacementsForLevel(int levelNumber)
    {
        Debug.Log($"[EnvPass] Generating PropPlacementSOs for level {levelNumber}");
        PropScaleAndMaterialFixer.RunFixes();
        var scaleMap = PropScaleAndMaterialFixer.ComputePrefabScaleFactors();
        var extintorMat = AssetDatabase.LoadAssetAtPath<Material>(PropScaleAndMaterialFixer.ExtintorMaterialPath);

        var level = Echoes.EnvironmentPass.EnvironmentPassDataLoader.LoadLevel(levelNumber);
        if (level == null) { Debug.LogError($"Level {levelNumber} not found"); return; }
        string folder = $"Assets/ScriptableObjects/EnvironmentPass/Level{level.levelNumber:D2}";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects/EnvironmentPass", $"Level{level.levelNumber:D2}");

        var rand = new System.Random(42);
        foreach (var room in level.rooms)
        {
            if (room == null) continue;
            string[] existingFiles = System.IO.Directory.GetFiles(folder, $"PropPlacement_{room.roomId}_*.asset");
            foreach (var f in existingFiles)
                AssetDatabase.DeleteAsset(f.Replace("\\", "/"));
            room.placements.Clear();
            room.decals.Clear();

            var required = room.GetRequiredPropsForType();
            int i = 0;
            foreach (var prefab in required)
            {
                var newPP = ScriptableObject.CreateInstance<Echoes.EnvironmentPass.PropPlacementSO>();
                newPP.prefabName = prefab;
                newPP.requiredForRoomType = true;
                newPP.size = GetPropSize(prefab);
                float angle = i * Mathf.PI * 2f / Mathf.Max(required.Count, 1);
                float radius = 1.0f + (float)(rand.NextDouble() * 1.5f);
                newPP.localPosition = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                newPP.localRotationEuler = new Vector3(0, rand.Next(4) * 90f, 0);
                newPP.scale = scaleMap.TryGetValue(prefab, out var s) ? s : Vector3.one;
                newPP.minClearanceFromPuzzle = 1.5f;
                if (prefab == "Extintor" && extintorMat != null)
                    newPP.materialOverride = extintorMat;
                string assetPath = $"{folder}/PropPlacement_{room.roomId}_{prefab}_{i:D2}.asset";
                AssetDatabase.CreateAsset(newPP, assetPath);
                room.placements.Add(newPP);
                i++;
            }
            EditorUtility.SetDirty(room);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[EnvPass] Generation for level {levelNumber} completed.");
    }

    // New menu items for easy per‑level generation / auto‑fix
    [MenuItem("Tools/Environment Pass/6 - Generate Missing PropPlacements for Current Level")]
    public static void RunGenerateMissingPropPlacementsForCurrentLevel()
    {
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int levelNum = ExtractLevelNumber(sceneName);
        if (levelNum <= 0)
        {
            Debug.LogError("[EnvPass] Could not determine level number from active scene name.");
            return;
        }
        RunGenerateMissingPropPlacementsForLevel(levelNum);
    }

    [MenuItem("Tools/Environment Pass/7 - AutoFix Current Level")]
    public static void RunAutoFixCurrentLevel()
    {
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int levelNum = ExtractLevelNumber(sceneName);
        if (levelNum <= 0)
        {
            Debug.LogError("[EnvPass] Could not determine level number from active scene name.");
            return;
        }
        AutoFixZoneAndPlace.RunAutoFixSingleLevel(levelNum);
    }

    [MenuItem("Tools/Environment Pass/8 - Regenerate Generic Room Pool")]
    public static void RegenerateGenericRoomPool()
    {
        Debug.Log("[EnvPass] Regenerating generic room pool...");

        PropScaleAndMaterialFixer.RunFixes();
        var scaleMap = PropScaleAndMaterialFixer.ComputePrefabScaleFactors();
        var extintorMat = AssetDatabase.LoadAssetAtPath<Material>(PropScaleAndMaterialFixer.ExtintorMaterialPath);

        string poolFolder = "Assets/ScriptableObjects/EnvironmentPass/Level_";
        if (!AssetDatabase.IsValidFolder(poolFolder))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects/EnvironmentPass", "Level_");

        string[] roomGuids = AssetDatabase.FindAssets("t:RoomDataSO", new[] { poolFolder });
        var rand = new System.Random(42);
        int created = 0;
        int deleted = 0;

        foreach (var guid in roomGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var template = AssetDatabase.LoadAssetAtPath<Echoes.EnvironmentPass.RoomDataSO>(path);
            if (template == null) continue;

            string folder = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");

            // Delete existing placements for this template
            string[] existingFiles = System.IO.Directory.GetFiles(folder, $"PropPlacement_{template.roomId}_*.asset");
            foreach (var f in existingFiles)
            {
                AssetDatabase.DeleteAsset(f.Replace("\\", "/"));
                deleted++;
            }
            template.placements.Clear();
            template.decals.Clear();

            var required = template.GetRequiredPropsForType();
            int i = 0;
            foreach (var prefab in required)
            {
                var newPP = ScriptableObject.CreateInstance<Echoes.EnvironmentPass.PropPlacementSO>();
                newPP.prefabName = prefab;
                newPP.requiredForRoomType = true;
                newPP.size = GetPropSize(prefab);
                float angle = i * Mathf.PI * 2f / Mathf.Max(required.Count, 1);
                float radius = 1.0f + (float)(rand.NextDouble() * 1.5f);
                newPP.localPosition = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                newPP.localRotationEuler = new Vector3(0, rand.Next(4) * 90f, 0);
                newPP.scale = scaleMap.TryGetValue(prefab, out var s) ? s : Vector3.one;
                newPP.minClearanceFromPuzzle = 1.5f;
                if (prefab == "Extintor" && extintorMat != null)
                    newPP.materialOverride = extintorMat;
                string assetPath = $"{folder}/PropPlacement_{template.roomId}_{prefab}_{i:D2}.asset";
                AssetDatabase.CreateAsset(newPP, assetPath);
                template.placements.Add(newPP);
                i++;
                created++;
            }

            // Create decals (2 per room)
            var decalPool = GetDecalPool(template.roomType);
            for (int d = 0; d < 2; d++)
            {
                if (decalPool.Count == 0) break;
                string decalPrefab = decalPool[rand.Next(decalPool.Count)];
                var newDecal = ScriptableObject.CreateInstance<Echoes.EnvironmentPass.PropPlacementSO>();
                newDecal.prefabName = decalPrefab;
                newDecal.size = PropSize.Small;
                newDecal.requiredForRoomType = false;
                float angle = rand.Next(360) * Mathf.Deg2Rad;
                float radius = (float)(rand.NextDouble() * 3.0f);
                newDecal.localPosition = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                newDecal.localRotationEuler = new Vector3(0, rand.Next(360), 0);
                newDecal.scale = scaleMap.TryGetValue(decalPrefab, out var ds) ? ds : Vector3.one;
                string assetPath = $"{folder}/PropPlacement_{template.roomId}_{decalPrefab}_{d:D2}_decal.asset";
                AssetDatabase.CreateAsset(newDecal, assetPath);
                template.decals.Add(newDecal);
                created++;
            }

            EditorUtility.SetDirty(template);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[EnvPass] Generic pool regenerated. Deleted {deleted} old, created {created} new placements.");
    }

    private static PropSize GetPropSize(string prefabName)
    {
        return prefabName switch
        {
            "Pizarra" or "MesaProfesor" or "PupitreDoble" or "Estanteria" or "Locker" or "Cartelera" or "EstanteriaCerrada" or "MesaRedonda" or "Escritorio" or "LockerPuertaAbierta"
                => PropSize.Dominant,
            "SillaEscolar" or "BancoMadera" or "CarritoConserje" or "SillaOficina" or "MesaKenney" or "Perchero" or "Radiador" or "VentanaMarco" or "AbrigoColgado"
                => PropSize.Medium,
            _ => PropSize.Small
        };
    }

    private static List<string> GetSmallFillers(RoomType type)
    {
        return type switch
        {
            RoomType.Classroom => new() { "Mochila", "Libros", "Basurero", "PapeleraKenney", "RelojPared", "Paraguas" },
            RoomType.Corridor => new() { "Mochila", "Basurero", "Extintor", "RelojPared", "Paraguas" },
            RoomType.Library => new() { "Libros", "Basurero", "PlantaMaceta", "RelojPared" },
            RoomType.Gym => new() { "Balon", "Cronometro", "Extintor", "Basurero", "Mochila" },
            RoomType.Patio => new() { "PlantaMaceta", "Basurero", "Balon", "Paraguas" },
            RoomType.Office => new() { "TazaCafe", "Libros", "Radio", "PlantaMaceta", "Basurero" },
            RoomType.Storage => new() { "CajaCartonCerrada", "CajaCartonAbierta", "Extintor", "Basurero" },
            RoomType.Hall => new() { "PlantaMaceta", "Basurero", "Mochila", "RelojPared" },
            _ => new() { "Libros", "Mochila", "Basurero", "TazaCafe" }
        };
    }

    private static List<string> GetMediumFillers(RoomType type)
    {
        return type switch
        {
            RoomType.Classroom => new() { "SillaEscolar" },
            RoomType.Corridor => new() { "BancoMadera", "Perchero", "Radiador" },
            RoomType.Library => new() { "MesaKenney", "SillaEscolar" },
            RoomType.Gym => new() { "BancoMadera", "Perchero" },
            RoomType.Patio => new() { "BancoMadera", "CarritoConserje" },
            RoomType.Office => new() { "SillaOficina", "MesaKenney" },
            RoomType.Storage => new() { "BancoMadera", "Perchero" },
            RoomType.Hall => new() { "BancoMadera", "Perchero", "Radiador" },
            _ => new() { "BancoMadera", "Perchero" }
        };
    }

    private static List<string> GetDominantFillers(RoomType type)
    {
        return type switch
        {
            RoomType.Classroom => new() { "PupitreDoble", "Estanteria" },
            RoomType.Corridor => new() { "Locker", "Cartelera", "LockerPuertaAbierta" },
            RoomType.Library => new() { "Estanteria", "EstanteriaCerrada" },
            RoomType.Gym => new() { "Locker", "EstanteriaCerrada" },
            RoomType.Patio => new() { "MesaRedonda", "EstanteriaCerrada" },
            RoomType.Office => new() { "Escritorio", "EstanteriaCerrada" },
            RoomType.Storage => new() { "EstanteriaCerrada", "Estanteria" },
            RoomType.Hall => new() { "Cartelera", "MesaRedonda" },
            _ => new() { "Locker", "Estanteria" }
        };
    }

    private static List<string> GetDecalPool(RoomType type)
    {
        return type switch
        {
            RoomType.Classroom => new() { "dec_tiza_borrada", "dec_nota_adhesiva", "dec_papel_suelo" },
            RoomType.Corridor => new() { "dec_arrastre", "dec_aviso_corcho", "dec_humedad", "dec_grieta" },
            RoomType.Library => new() { "dec_humedad", "dec_nota_adhesiva" },
            RoomType.Gym => new() { "dec_arrastre", "dec_grieta" },
            RoomType.Patio => new() { "dec_humedad", "dec_grieta" },
            RoomType.Office => new() { "dec_nota_adhesiva", "dec_foto_borrosa" },
            RoomType.Storage => new() { "dec_humedad", "dec_grieta", "dec_arrastre" },
            RoomType.Hall => new() { "dec_humedad", "dec_aviso_corcho" },
            _ => new() { "dec_grieta", "dec_humedad" }
        };
    }

    private static int EstimateProps(Echoes.EnvironmentPass.LevelDataSO level)
        => level.rooms.Where(r => r != null).Sum(r =>
            (r.placements?.Count(p => p != null) ?? 0) + (r.decals?.Count(d => d != null) ?? 0));

    private static int ExtractLevelNumber(string sceneName)
    {
        var parts = sceneName.Split('_');
        return parts.Length > 1 && int.TryParse(parts[1].Replace(".unity", ""), out int n) ? n : 1;
    }
}