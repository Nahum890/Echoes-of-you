using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Echoes.EnvironmentPass;

public static class SliceEnvironmentPassRunner
{
    public struct PassReport
    {
        public int levelNumber;
        public string levelName;
        public int totalProps;
        public int totalNarrativePieces;
        public int violationsCount;
        public int warningsCount;
        public List<string> errors;
        public List<string> warnings;
        public List<string> placedRooms;
        public bool success;
    }

    [MenuItem("Echoes of You/Environment Pass/Run N01 Environment Pass", false, 401)]
    public static void RunN01() => RunPassAndLog(1);

    [MenuItem("Echoes of You/Environment Pass/Run N02 Environment Pass", false, 402)]
    public static void RunN02() => RunPassAndLog(2);

    [MenuItem("Echoes of You/Environment Pass/Run N03 Environment Pass", false, 403)]
    public static void RunN03() => RunPassAndLog(3);

    public static PassReport RunPassAndLog(int levelNum)
    {
        var report = RunPass(levelNum);
        Debug.Log($"[SliceEnvPass] N{levelNum:00} Pass Result: Success={report.success}, Props={report.totalProps}, Narrative={report.totalNarrativePieces}, Violations={report.violationsCount}, Warnings={report.warningsCount}");
        foreach (var err in report.errors) Debug.LogError($"[SliceEnvPass] Error: {err}");
        foreach (var warn in report.warnings) Debug.LogWarning($"[SliceEnvPass] Warning: {warn}");
        return report;
    }

    public static PassReport RunPass(int levelNum)
    {
        var report = new PassReport
        {
            levelNumber = levelNum,
            levelName = $"Level_{levelNum:00}",
            errors = new List<string>(),
            warnings = new List<string>(),
            placedRooms = new List<string>()
        };

        EnvironmentPassDataLoader.InvalidateCache();
        var levelData = EnvironmentPassDataLoader.LoadLevel(levelNum);
        if (levelData == null)
        {
            report.errors.Add($"LevelData for level {levelNum} not found.");
            report.success = false;
            return report;
        }

        string scenePath = $"Assets/Scenes/Level_{levelNum:00}.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            report.errors.Add($"Scene at {scenePath} could not be opened.");
            report.success = false;
            return report;
        }

        // 1. Run EnvironmentPass placement engine
        var placeResult = EnvironmentPassPlacementEngine.PlaceLevel(levelData, dryRun: false);
        report.totalProps = placeResult.totalProps;
        foreach (var rRes in placeResult.roomResults)
        {
            int placed = rRes.propResults.Count(p => p.success);
            report.placedRooms.Add($"{rRes.roomId} ({placed} props)");
            foreach (var err in rRes.errors) report.errors.Add($"[{rRes.roomId}] {err}");
            foreach (var warn in rRes.warnings) report.warnings.Add($"[{rRes.roomId}] {warn}");
        }

        // 2. Run narrative decoration pass (§3.6)
        string narrativeSummary = EchoesPropDecorator.DecorateLevel(levelNum);
        Debug.Log($"[SliceEnvPass] Narrative decoration: {narrativeSummary}");
        GameObject narrativeRoot = GameObject.Find("--- NARRATIVE ---");
        if (narrativeRoot != null)
        {
            report.totalNarrativePieces = narrativeRoot.GetComponentsInChildren<Transform>().Length - 1;
        }

        // 3. Configure and tune EchoPathHint
        EnsureEchoPathHintConfigured(levelNum);

        // 4. Validate scene via EnvironmentPassValidator
        var valReport = EnvironmentPassValidator.ValidateActiveScene();
        report.violationsCount = valReport.totalViolations;
        report.warningsCount = valReport.totalWarnings;
        foreach (var rRep in valReport.roomReports)
        {
            foreach (var v in rRep.violations) report.errors.Add($"[Validation:{rRep.roomId}] {v}");
            foreach (var w in rRep.warnings) report.warnings.Add($"[Validation:{rRep.roomId}] {w}");
        }

        // 5. Custom Clearance & Puzzle Path Audit
        AuditPuzzleClearances(report);

        report.success = report.errors.Count == 0 && placeResult.success && valReport.passed;

        EditorSceneManager.SaveScene(scene);
        return report;
    }

    private static void EnsureEchoPathHintConfigured(int levelNum)
    {
        // Clean any residual static PointLight holder
        GameObject envRoot = GameObject.Find("--- ENVIRONMENT ---");
        if (envRoot != null)
        {
            Transform oldLights = envRoot.transform.Find("PathHintLights");
            if (oldLights != null) Object.DestroyImmediate(oldLights.gameObject);
        }

        // Find or create EchoPathHint under MECH
        GameObject mechRoot = GameObject.Find("--- MECHANICS ---");
        if (mechRoot == null) return;

        EchoPathHint hintComp = Object.FindAnyObjectByType<EchoPathHint>();
        if (hintComp == null)
        {
            GameObject go = new GameObject("EchoPathHint");
            go.transform.SetParent(mechRoot.transform, false);
            hintComp = go.AddComponent<EchoPathHint>();
        }

        // Load blueprint path hints if waypoints are empty
        string bpPath = $"Assets/Data/Levels/Level_{levelNum:00}_Blueprint.asset";
        LevelBlueprint bp = AssetDatabase.LoadAssetAtPath<LevelBlueprint>(bpPath);
        if (bp != null && bp.pathHints != null && bp.pathHints.Length >= 2)
        {
            hintComp.SetWaypoints(bp.pathHints);
            EditorUtility.SetDirty(hintComp);
        }
    }

    private static void AuditPuzzleClearances(PassReport report)
    {
        GameObject propsRoot = GameObject.Find("--- PROPS ---");
        if (propsRoot == null) return;

        var puzzlePositions = new List<(string name, Vector3 pos)>();
        foreach (var plate in Object.FindObjectsByType<PressurePlate>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            puzzlePositions.Add((plate.gameObject.name, plate.transform.position));
        foreach (var door in Object.FindObjectsByType<DoorController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            puzzlePositions.Add((door.gameObject.name, door.transform.position));
        foreach (var exit in Object.FindObjectsByType<LevelExit>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            puzzlePositions.Add((exit.gameObject.name, exit.transform.position));

        const float minClearance = 1.5f;

        foreach (Transform roomTr in propsRoot.transform)
        {
            foreach (Transform prop in roomTr)
            {
                Vector2 p2D = new Vector2(prop.position.x, prop.position.z);
                foreach (var (puzName, puzPos) in puzzlePositions)
                {
                    float dist = Vector2.Distance(p2D, new Vector2(puzPos.x, puzPos.z));
                    if (dist < minClearance)
                    {
                        report.errors.Add($"CRITICAL: Prop '{prop.name}' at {dist:F2}m from puzzle object '{puzName}' (min {minClearance}m required)");
                    }
                }
            }
        }
    }
}
